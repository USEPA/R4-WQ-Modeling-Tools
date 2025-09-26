using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREQANAL
{
    class RegData
    {
        public String SiteNo { get; set; }
        public double Qcfs { get; set; }
        public double Area { get; set; }
        public double Rainfall { get; set; }
        public double MeanFlow { get; set; }
        public double Forest { get; set; }
        public double Urban { get; set; }
        //public double Wetland { get; set; }
        public double Others { get; set; }
        //public double Water { get; set; }
    }
}
