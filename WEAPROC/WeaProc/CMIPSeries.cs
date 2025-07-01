using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCEIData
{
    public class CMIP6Series
    {
        private int _begYr, _endYr;
        private string _scenario, _pathway, _variant;
        private List<string> _lstClivar, _lstGrid;
        private BoundingBox _gridBox;

        public CMIP6Series(int bYr, int eYr, string _scen, string _path,
                List<string> _lstvar, List<string> _lstgrd, BoundingBox _bbox)
        {
            _begYr = bYr;
            _endYr = eYr;
            _scenario = _scen;
            _pathway = _path;
            _lstClivar = _lstvar;
            _lstGrid = _lstgrd;
            _gridBox = _bbox;
        }

        public int BeginYear
        {
            get { return _begYr; }
            set { _begYr = value; }
        }
        public int EndYear
        {
            get { return _endYr; }
            set { _endYr = value; }
        }
        public string Scenario
        {
            get { return _scenario; }
            set { _scenario = value; }
        }
        public string Pathway
        {
            get { return _pathway; }
            set { _pathway = value; }
        }
        public string Variant
        {
            get { return _variant; }
            set { _variant = value; }
        }
        public List<string> ClimateVar
        {
            get { return _lstClivar; }
            set { _lstClivar = value; }
        }

        public List<string> ClimateGrid
        {
            get { return _lstGrid; }
            set { _lstGrid = value; }
        }
        public BoundingBox GridBox
        {
            get { return _gridBox; }
            set { _gridBox = value; }
        }
    }
}
