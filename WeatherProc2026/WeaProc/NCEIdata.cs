using System;

namespace NCEIData
{
    public class clsISDdata
    {
        public string Date { get; set; }
        public string ATEM { get; set; }
        public string WIND { get; set; }
        public string CLOU { get; set; }
        public string DEWP { get; set; }
        public string PREC { get; set; }
    }

    public class GridPoint
    {
        private double _x, _y;
        public double xlon
        {
            get => _x;
            set {_x = value;}
        }
        public double ylat
        {
            get => _y;
            set {_y = value;}
        }
    }
    
    public class MetGages
    {
        public string Station { get; set; }
        public string Station_ID { get; set; }
        public DateTime BEG_DATE { get; set; }
        public DateTime END_DATE { get; set; }
        public string LATITUDE { get; set; }
        public string LONGITUDE { get; set; }
        public string ELEVATION { get; set; }
        public string STATE { get; set; }
        public string TYPE { get; set; }
        public string TZONE { get; set; }
    }
}