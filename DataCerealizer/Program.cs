using c = System.Console;
using System.Text;
using DataCerealizer;

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
    }
}