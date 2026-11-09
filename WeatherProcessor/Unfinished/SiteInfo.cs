using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREQANAL
{
    class SiteInfo
    {
        public String SiteNo { get; set; }
        public Double BegDate { get; set; }
        public double EndDate { get; set; }
        public double DArea { get; set; }
        public Int32 RecCount { get; set; }
        public double Precip { get; set; }
        public double Slope { get; set; }
        public double MeanQ { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public string Comid { get; set; }
        public double Water { get; set; }
        public double Urban { get; set; }
        public double Forest { get; set; }
        public double Wetlands { get; set; }
        public double Others { get; set; }
    }
}
