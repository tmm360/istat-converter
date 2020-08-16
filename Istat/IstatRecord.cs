using System;
using System.Collections.Generic;
using System.Text;

namespace Istat
{
    class IstatRecord
    {
        public int AgeClass { get; set; }
        public string CityName { get; set; }
        public DateTime Date { get; set; }
        public int Deaths { get; set; }
        public string ProvinceName { get; set; }
        public string RegionName { get; set; }
    }
}
