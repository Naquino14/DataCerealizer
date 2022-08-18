using c = System.Console;
using System.Text;
using DataCerealizer;
using System.Reflection;

internal class Program
{
    public static void Main(string[] args)
    {
        string path = @"C:\Users\naqui\Desktop", filename = "test", fullpath = $@"{path}/{filename}.cereal";

        var cereal = new Cereal
        {
            AmtCereal = 10,
            Brand = "Corn Flakes"
        };

        if (File.Exists(fullpath))
            File.Delete(fullpath);
        
        var digestedCereal = DataCerealizer<Cereal>.Serialize(cereal, "This file contains info on cereal.");
        var huh = Encoding.ASCII.GetString(digestedCereal);
        c.Write(huh);

        File.WriteAllBytes(fullpath, digestedCereal);

        c.WriteLine("\nSaved! Deserializing...");

        Cereal deCerealized = DataCerealizer<Cereal>.Deserialize(File.ReadAllBytes(fullpath));

        c.WriteLine($"Deserealized cereal: ({deCerealized.GetType()}): {PrintProps(deCerealized)[..^1]} }}");
    }

    private static string PrintProps<T>(T obj) 
    {
        string res = "";

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic))
            res += $"{prop.Name} ({prop.PropertyType}): {prop.GetValue(obj, null)}, ";

        return res;
    }
}