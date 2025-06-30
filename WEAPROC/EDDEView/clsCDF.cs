using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Research.Science.Data;
using Microsoft.Research.Science.Data.Imperative;

namespace EDDEView
{
    public class ClsCDF
    {
        public ClsCDF(string ncfile)
        {
            using (Microsoft.Research.Science.Data.DataSet ds =
                    Microsoft.Research.Science.Data.DataSet.Open("msds:nc?file=" + ncfile))
            {
                Debug.WriteLine(ds);
                Debug.WriteLine(ds.Metadata["comment"]);

                foreach (var svar in ds)
                {
                    Debug.WriteLine(svar);
                }

                double[] lat = ds.GetData<double[]>("y");
                double[] lon = ds.GetData<double[]>("x");
                Debug.WriteLine($"latitude: len={lat.Length}, min={lat.Min()}, max={lat.Max()}");
                Debug.WriteLine($"longitude: len={lon.Length}, min={lon.Min()}, max={lon.Max()}");
                float[,] y = ds.GetData<float[,]>("lat");
                float[,] x = ds.GetData<float[,]>("lon");
                float[,,] clt = ds.GetData<float[,,]>("CLDF");

                //for (int i = 0; i < lat.Length; i++)
                //    for (int j = 0; j < lon.Length; j++)
                //    {
                //Debug.WriteLine(i.ToString() + ", " + j.ToString() + ", " +
                //    y[i, j].ToString("F5"));
                //    }
                Debug.WriteLine(clt[0, 0, 17].ToString("F4"));
            }
        }
    }
}
