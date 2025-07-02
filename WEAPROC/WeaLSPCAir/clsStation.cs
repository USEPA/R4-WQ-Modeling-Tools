using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaLSPCAir
{
    public class clsStation
    {
        public string Station { get; set; }
        public string Scenario { get; set; }
        public string Constituent { get; set; }
        public int DSN { get; set; }
        public DateTime BegDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
