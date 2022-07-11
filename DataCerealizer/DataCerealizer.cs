using System.Reflection;

namespace DataCerealizer
{
    public static class DataCerealizer<T>
    {
        private static readonly byte US = 0x1f,
            CR = 0x0d,
            LF = 0x0a,
            SOH = 0x1,
            STX = 0x2,
            ETX = 0x3;

        public static T Deserialize(Stream dataStream)
        {
            throw new NotImplementedException();
        }
        public static T Deserialize(byte[] data)
        {
            throw new NotImplementedException();
        }

        public static void Serialize(Stream dataStream, T data)
        {
            throw new NotImplementedException();
        }

        public static byte[] Serialize(T data) => GenCerealHead().Concat(_Serialize(data)).ToArray();
        public static byte[] Serialize(T[] data) => GenCerealHead().Concat(_Serialize(data)).ToArray();

        public static byte[] _Serialize(T data)
        {
            byte[] cerealBowl = new byte[0];

            foreach (var prop in GetProps())
                cerealBowl = cerealBowl.Concat(GB(prop.GetValue(data)?.ToString()!)).Concat(new byte[] { US }).SkipLast(1).ToArray();

            return cerealBowl;
        }

        private static byte[] _Serialize(T[] data)
        {
            var cereal = Array.Empty<byte>();
            foreach(var e in data)
                cereal = cereal.Concat(_Serialize(e)).ToArray();
            return cereal;
        }

        public static byte[] GenCerealHead()
        {
            byte[] head = GB($"{SOH}Cerealized Data v0.1{STX}");
            foreach (var prop in GetProps())
                head = head.Concat(GB($"{prop.Name}")).Concat(new byte[] { US }).ToArray();
            return head.SkipLast(1).ToArray();
        }

        private static PropertyInfo[] GetProps() => typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        private static string GS(byte[] i) => System.Text.Encoding.ASCII.GetString(i);
        private static byte[] GB(string i) => System.Text.Encoding.ASCII.GetBytes(i);
    }
}