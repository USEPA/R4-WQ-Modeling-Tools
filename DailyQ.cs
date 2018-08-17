using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FREQANAL
{
    class DailyQ
    {
        public double Day { get; set; }
        public DateTime Date { get; set; }
        public double Year { get; set; }
        public double Qdly { get; set; }
        public double Qnday { get; set; }
        public double DecYear { get; set; }
    }

    class CurDailyQ
    {
        public int nDayQ { get; set; }
        public double Year { get; set; }
        public double Qnday { get; set; }
    }
}
