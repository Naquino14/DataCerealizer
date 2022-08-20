namespace DataCerealizer
{
    public class Cereal
    {
        public int AmtCereal { get; set; }
        public string? Brand { get; set; }
        public int AmountOfBoxes;
    }

    public class LuckyCharms : Cereal
    {
        public string[]? MarshmallowTypes { get; set; }
    }
}