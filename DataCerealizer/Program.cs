using c = System.Console;
using System.Text;
using DataCerealizer;

internal class Program
{
    public static void Main(string[] args)
    {
        var cereal = new Cereal
        {
            AmtCereal = 10,
            Brand = "Corn Flakes"
        };

        var guh = DataCerealizer<Cereal>.Serialize(cereal);

        foreach (var e in guh)
            c.Write($"{e:X}");
        c.WriteLine();
        string ee = Encoding.ASCII.GetString(guh);
        c.Write(ee);
        ;
    }
}