using c = System.Console;
using System.Text;
using DataCerealizer;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Drawing;

internal class Program
{
    public static void Main()
    {
        string path = @"C:\Users\naqui\Desktop", filename = "test", fullpath = $@"{path}/{filename}.cereal";

        var cereal = new Cereal
        {
            AmtCereal = 10,
            Brand = "Corn Flakes",
            AmountOfBoxes = 2
        };

        var luckyCharms = new LuckyCharms
        {
            AmtCereal = 5,
            AmountOfBoxes = 1,
            Brand = "Lucky Charms",
            MarshmallowTypes = new string[] { "Chocolate", "Vanilla" }
        };

        if (File.Exists(fullpath))
            File.Delete(fullpath);

        //var digestedCereal = DataCerealizer<Cereal>.Serialize(cereal, "This file contains info on cereal.");
        //var huh = Encoding.ASCII.GetString(digestedCereal);
        //c.WriteLine(huh);

        //File.WriteAllBytes(fullpath, digestedCereal);

        //c.WriteLine("Saved! Deserializing...");

        //Cereal deCerealized = DataCerealizer<Cereal>.Deserialize(File.ReadAllBytes(fullpath));

        //c.WriteLine($"Deserealized cereal: ({deCerealized.GetType()}): {{ {PrintPropsAndFields(deCerealized)} }}");

        var digestedLuckyCharms = DataCerealizer<LuckyCharms>.Serialize(luckyCharms, "This file contains lucky charms");
        var guh = Encoding.ASCII.GetString(digestedLuckyCharms);
        c.WriteLine(guh);

        File.WriteAllBytes(fullpath, digestedLuckyCharms);

        c.WriteLine("Saved! Deserializing...");

        LuckyCharms deserializedLuckyCharms = DataCerealizer<LuckyCharms>.Deserialize(File.ReadAllBytes(fullpath));
        // print the digested lucky charms
            c.WriteLine($"Deserialized lucky charms: ({deserializedLuckyCharms}): {{ {PrintPropsAndFields(deserializedLuckyCharms)} }}");
        ;
    }

    private static string PrintPropsAndFields<T>(T obj) 
    {
        string res = "";

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic))
            res += $"{prop.Name} ({prop.PropertyType}): {prop.GetValue(obj, null)}, ";

        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
            .Where(f => f.GetCustomAttribute<CompilerGeneratedAttribute>() == null))
            res += $"{field.Name} ({field.FieldType}): {field.GetValue(obj)}, ";
        
        return res[..^2];
    }
}