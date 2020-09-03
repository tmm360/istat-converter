namespace Istat
{
    class IstatCsvOutput
    {
        public string Date { get; set; }
        public string RegionName { get; set; }
        public string ProvinceName { get; set; }
        public string CityName { get; set; }
        public string AgeClass { get; set; }
        public int FDeaths { get; set; }
        public int MDeaths { get; set; }
        public int TDeaths => FDeaths + MDeaths;
    }
}
