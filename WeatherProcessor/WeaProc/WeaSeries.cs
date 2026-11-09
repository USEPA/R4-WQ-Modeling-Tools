using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCEIData
{
    public class WeaSeries
    {
        //tseries
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictWea;
        //missing
        private SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>> dictMiss;

        public WeaSeries()
        {
            dictWea = new
                       SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>>();
            dictMiss = new
                    SortedDictionary<string, Dictionary<string, SortedDictionary<DateTime, string>>>();
        }

        public void AddSeries(string site, string svar, SortedDictionary<DateTime, string>tseries)
        {
            Dictionary<string, SortedDictionary<DateTime, string>> varSeries;
            if(!dictWea.TryGetValue(site, out varSeries))
            {
                varSeries= new Dictionary<string, SortedDictionary<DateTime, string>>();
                varSeries.Add(svar, tseries);
                dictWea.Add(site, varSeries);
            }
            else
            {
                if (!varSeries.ContainsKey(svar))
                    varSeries.Add(svar, tseries);
            }
        }

        public void AddMissSeries(string site, string svar, SortedDictionary<DateTime, string> tseries)
        {
            Dictionary<string, SortedDictionary<DateTime, string>> varSeries;
            if (!dictMiss.TryGetValue(site, out varSeries))
            {
                varSeries = new Dictionary<string, SortedDictionary<DateTime, string>>();
                varSeries.Add(svar, tseries);
                dictMiss.Add(site, varSeries);
            }
            else
            {
                if (!varSeries.ContainsKey(svar))
                    varSeries.Add(svar, tseries);
            }
        }

        public SortedDictionary<DateTime, string>GetSeries(string site, string svar)
        {
            SortedDictionary<DateTime, string> tseries;
            Dictionary<string, SortedDictionary<DateTime, string>> varSeries;

            if (dictWea.TryGetValue(site, out varSeries))
                varSeries.TryGetValue(svar, out tseries);
            else
                return null;
            return tseries;
        }

        public SortedDictionary<DateTime, string> GetMissSeries(string site, string svar)
        {
            SortedDictionary<DateTime, string> tseries;
            Dictionary<string, SortedDictionary<DateTime, string>> varSeries;

            if (dictMiss.TryGetValue(site, out varSeries))
                varSeries.TryGetValue(svar, out tseries);
            else
                return null;
            return tseries;
        }

        public int NumSites()
        {
            return dictWea.Keys.Count;
        }

        public Dictionary<string, SortedDictionary<DateTime, string>>GetMissingSeriesForSite(string site)
        {
            Dictionary<string, SortedDictionary<DateTime, string>> siteMissingSeries = new
                Dictionary<string, SortedDictionary<DateTime, string>>();

            if (!dictMiss.TryGetValue(site, out siteMissingSeries))
                return null;
            return siteMissingSeries;
        }

        public void ReplaceMissingSeriesForSite(string site, 
                Dictionary<string, SortedDictionary<DateTime, string>> varMiss)
        {
            dictMiss.Remove(site);
            dictMiss.Add(site, varMiss);
        }
        public void ReplaceSeriesForSite(string site,
                Dictionary<string, SortedDictionary<DateTime, string>> dictSeries)
        {
            dictWea.Remove(site);
            dictWea.Add(site, dictSeries);
        }
    }
}
