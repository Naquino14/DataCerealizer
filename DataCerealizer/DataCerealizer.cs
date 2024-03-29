﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace DataCerealizer
{
    public static class DataCerealizer<T>
    {
        private static readonly byte
            US = 0x1f,
            SOH = 0x1,
            STX = 0x2,
            ETX = 0x3,
            EM = 0x19,
            SI = 0x15,
            VT = 0x0b;

        private static readonly BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic;

        public static T Deserialize(byte[] data) => Deserialize(data, out _);
        public static object DeserializeRaw(byte[] data) => Deserialize(data, out _)!;
        public static T Deserialize(byte[] data, out string? metadata)
        {
            object? result = Activator.CreateInstance(typeof(T));

            /// Structure:
            /// 0x01(SOH) Header [0x1f(US) Metadata if it exists] 0x02(STX) 
            /// Prop1 0x1f(US) Prop2 0x1f(US) PropN 0x19(EM)
            /// Field1 0x1f(US) Field2 0x1f(US) Value3[] 0x02(STX) { ValElem1 0x1f(US) ValElem2 0x1f(US) ValueElemN 0x1f(US) } 0x2x(STX) Value4 0x1f(US) ValueN 0x03(EM)
            /// PropValue1 0x1f(US) PropValue2 0x1f(US) PropValueN 0x02(EM)
            /// FieldValue1 0x1f(US) FieldValue2 0x1f(US) FieldValueN 0x03(ETX)

            var sdata = GS(data);
            var sections  = sdata.Split((char)STX)[1].Split((char)EM);

            metadata = new Regex($"(?<={(char)US})(.*?)(?={(char)STX})").Match(sdata).Value;

            string[]? propSecs, fieldSecs, propValues, fieldValues; 

            propSecs = sections[0].Split((char)US);
            fieldSecs = sections[1].Split((char)US);
            propValues = new Regex($"(?<={(char)SI})(.*?)(?={(char)SI})|(?<={(char)SI})(.*?)(?={(char)US})|(?<={(char)SI}|{(char)US}).*")
                .Matches(sections[2]).
                Cast<Match>().
                Select(m => m.Value).
                Where(s => !string.IsNullOrEmpty(s)).
                ToArray();
            fieldValues = new Regex($"(?<={(char)SI})(.*?)(?={(char)SI})|(?<={(char)SI})(.*?)(?={(char)US})|(?<={(char)SI}|{(char)US})(.*?)(?={(char)US}|{(char)ETX})|.*?(?={(char)ETX})")
                .Matches(sections[3])
                .Cast<Match>()
                .Select(m => m.Value)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            if (propSecs.Length != propValues.Length)
                throw new Exception(); // todo: deserializing errors
            else if (propSecs.Length != 0)
                for (int i = 0; i < propSecs.Length; i++)
                {
                    
                    var prop = GetProp(propSecs[i]) ?? throw new Exception();
                    if (prop is not null && prop.CanWrite)
                        prop.SetValue(result, Convert.ChangeType(prop.PropertyType.Name == "String[]" ? propValues[i].Split((char)VT) : propValues[i], prop.PropertyType));
                }

            if (fieldSecs.Length != fieldValues.Length)
                throw new Exception(); // todo: deserializing errors
            else if (fieldSecs.Length != 0)
                for (int i = 0; i < fieldSecs.Length; i++)
                {
                    var field = GetField(fieldSecs[i] ?? throw new Exception());
                    if (field is not null)
                        field.SetValue(result, Convert.ChangeType(field.FieldType.Name == "String[]" ? fieldValues[i].Split((char)VT) : fieldValues[i], field.FieldType));
                }

            return (T)result! ?? throw new Exception(); // todo: deserializing errors
        }

        public static byte[] Serialize(T data) => Serialize(data, "");
        public static byte[] Serialize(T data, string metadata) => GenCerealHead(metadata)
            .Concat(_Serialize(data))
            .Concat(B2BA(ETX))
            .ToArray();
        public static byte[] Serialize(T[] data) => Serialize(data, "");
        public static byte[] Serialize(T[] data, string metadata) => GenCerealHead(metadata)
            .Concat(_Serialize(data))
            .Concat(B2BA(ETX))
            .ToArray();

        private static byte[] _Serialize(T data)
        {
            byte[] cerealBowl = Array.Empty<byte>();

            foreach (var prop in GetProps())
                if (prop.PropertyType.IsArray)
                {
                    cerealBowl = cerealBowl.Concat(B2BA(SI)).ToArray();
                    foreach (var e in (Array)prop.GetValue(data)!)
                        cerealBowl = cerealBowl.Concat(GB(e.ToString() ?? throw new NullReferenceException()))
                                .Concat(B2BA(VT))
                                .ToArray();
                    cerealBowl = cerealBowl.SkipLast(1).Concat(B2BA(SI)).ToArray();
                }
                else
                    cerealBowl = cerealBowl.Concat(GB(prop.GetValue(data)?.ToString()!))
                    .Concat(B2BA(US))
                    .ToArray();
            cerealBowl = cerealBowl.SkipLast(1).Concat(B2BA(EM)).ToArray();
            foreach (var field in GetFields())
                if (field.GetType().IsArray)
                {
                    cerealBowl = cerealBowl.Concat(B2BA(SI)).ToArray();
                    foreach (var e in (Array)field.GetValue(data)!)
                        cerealBowl = cerealBowl.Concat(GB(e.ToString() ?? throw new NullReferenceException()))
                                .Concat(B2BA(VT))
                                .ToArray();
                    cerealBowl = cerealBowl.SkipLast(1).Concat(B2BA(SI)).ToArray();
                }
                else
                    cerealBowl = cerealBowl.Concat(GB(field.GetValue(data)?.ToString()!))
                    .Concat(B2BA(US))
                    .ToArray();

            return cerealBowl.SkipLast(1).ToArray();
        }

        private static byte[] _Serialize(T[] data)
        {
            var cereal = Array.Empty<byte>();
            foreach(var e in data)
                cereal = cereal.Concat(_Serialize(e)).ToArray();
            return cereal;
        }

        private static byte[] GenCerealHead(string metadata)
        {
            byte[] head = B2BA(SOH)
                .Concat(GB($"Cerealized Data v0.2"))
                .Concat(metadata != "" ? B2BA(US).Concat(GB(metadata).Concat(B2BA(STX))) : B2BA(STX))
                .ToArray();
            foreach (var prop in GetProps())
                head = head.Concat(GB($"{prop.Name}"))
                    .Concat(B2BA(US))
                    .ToArray();
            head = head.SkipLast(1).Concat(B2BA(EM)).ToArray();
            foreach (var field in GetFields())
                head = head.Concat(GB($"{field.Name}"))
                    .Concat(B2BA(US))
                    .ToArray();
            return head.SkipLast(1).Concat(B2BA(EM)).ToArray();
        }
        
        private static PropertyInfo[] GetProps() => typeof(T).GetProperties(bindingAttr);
        private static PropertyInfo? GetProp(string key) => typeof(T).GetProperty(key, bindingAttr);
        private static FieldInfo[] GetFields() => typeof(T).GetFields(bindingAttr).Where(f => f.GetCustomAttribute<CompilerGeneratedAttribute>() == null).ToArray();
        private static FieldInfo? GetField(string key) => typeof(T).GetField(key, bindingAttr);
        private static string GS(byte[] i) => Encoding.ASCII.GetString(i);
        private static byte[] GB(string i) => Encoding.ASCII.GetBytes(i);
        private static byte[] B2BA(byte a) => new byte[] { a };
    }
}