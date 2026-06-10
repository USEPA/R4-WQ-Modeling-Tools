using System;

namespace NCEIData
{
    public class clsStation
    {
        public int DSN { get; set; }
        public string Station { get; set; }
        public string StationName { get; set; }
        public string Scenario { get; set; }
        public string Constituent { get; set; }
        public DateTime BegDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Elevation { get; set; }
        public double LatPrj { get; set; }
        public double LonPrj { get; set; }
        public string HUC { get; set; }
        public string State { get; set; }
        public string Description { get; set; }
        public string STAID { get; set; }
        public string TimeStep { get; set; }
        public string Pathway { get; set; }
        public string ScenPath { get; set; }
    }
    public class CPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}