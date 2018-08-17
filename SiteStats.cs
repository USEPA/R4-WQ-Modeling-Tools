using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREQANAL
{
    class SiteStats
    {
        public String SiteNo { get; set; }
        public double DlyMean { get; set; }
        public double DlyStDev { get; set; }
        public double DlyMedian { get; set; }
        public double DlyGeoMean { get; set; }
        public double DlyHarMean { get; set; }
        public double AnnMean { get; set; }
        public double AnnStDev { get; set; }
        public double AnnMedian { get; set; }
        public double AnnGeoMean { get; set; }
        public double AnnHarMean { get; set; }
    }
}
