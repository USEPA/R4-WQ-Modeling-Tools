using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREQANAL
{
    class Frequency
    {
        public String SiteNo { get; set; }
        public double SqMi { get; set; }
        public double Precip { get; set; }
        public double Slope { get; set; }
        public double Mean { get; set; }
        public double StDev { get; set; }
        public double Skew { get; set; }
        public double P0 { get; set; }
        public double PTr { get; set; }
        public double P10 { get; set; }
        public double P20 { get; set; }
        public double P25 { get; set; }
        public double P50 { get; set; }
        public double P90 { get; set; }
        public double P99 { get; set; }
        public double LCL90 { get; set; }
        public double UCL90 { get; set; }
        
        public Int16 NumYrs { get; set; }
    }
}
