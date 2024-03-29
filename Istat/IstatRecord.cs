﻿using System;

namespace Istat
{
    class IstatRecord
    {
        public int AgeClass { get; set; }
        public string CityName { get; set; }
        public DateTime Date { get; set; }
        public int FDeaths { get; set; }
        public int MDeaths { get; set; }
        public string ProvinceName { get; set; }
        public string RegionName { get; set; }
    }
}
