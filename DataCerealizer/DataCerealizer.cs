using System.Reflection;
using System.Text;

namespace DataCerealizer
{
    public static class DataCerealizer<T>
    {
        private static readonly byte 
            US = 0x1f,
            SOH = 0x1,
            STX = 0x2,
            ETX = 0x3,
            EM = 0x19;

        public static T Deserialize(byte[] data)
        {
            throw new NotImplementedException();

            /// Structure:
            /// 0x01(SOH) Header [0x1f(US) Metadata if it exists] 0x02(STX) 
            /// Prop1 0x1f(US) Prop2 0x1f(US) PropN 0x19(EM)
            /// Value1 0x1f(US) Value2 0x1f(US) ValueN 0x03(ETX)
        }

        public static object DeserializeRaw(byte[] data)
        {
            throw new NotImplementedException();
        }

        public static byte[] Serialize(T data) => Serialize(data, "");
        public static byte[] Serialize(T data, string metadata) => GenCerealHead(metadata)
            .Concat(_Serialize(data))
            .Concat(B2BA(ETX))
            .ToArray();
        public static byte[] Serialize(T[] data) => Serialize(data, "");
        public static byte[] Serialize(T[] data, string metadata) => GenCerealHead(metadata)
            .Concat(_Serialize(data, metadata))
            .Concat(B2BA(ETX))
            .ToArray();

        private static byte[] _Serialize(T data)
        {
            byte[] cerealBowl = new byte[0];

            foreach (var prop in GetProps())
                cerealBowl = cerealBowl.Concat(GB(prop.GetValue(data)?.ToString()!))
                    .Concat(B2BA(US))
                    .ToArray();

            return cerealBowl.SkipLast(1).ToArray();
        }

        private static byte[] _Serialize(T[] data, string metadata)
        {
            var cereal = Array.Empty<byte>();
            foreach(var e in data)
                cereal = cereal.Concat(_Serialize(e)).ToArray();
            return cereal;
        }

        private static byte[] GenCerealHead(string metadata)
        {
            byte[] head = B2BA(SOH)
                .Concat(GB($"Cerealized Data v0.1"))
                .Concat(metadata != "" ? B2BA(US).Concat(GB(metadata).Concat(B2BA(STX))) : B2BA(STX))
                .ToArray();
            foreach (var prop in GetProps())
                head = head.Concat(GB($"{prop.Name}"))
                    .Concat(B2BA(US))
                    .ToArray();
            return head.SkipLast(1).Concat(B2BA(EM)).ToArray();
        }
        
        private static PropertyInfo[] GetProps() => typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        private static string GS(byte[] i) => Encoding.ASCII.GetString(i);
        private static byte[] GB(string i) => Encoding.ASCII.GetBytes(i);
        private static byte[] B2BA(byte a) => new byte[] { a };
    }
}