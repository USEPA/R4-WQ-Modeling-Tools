using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ZedGraph;


namespace NCEIData
{
    class clsGraph
    {
        private PointPairList varSeries = new PointPairList();
        private PointPairList fillSeries = new PointPairList();
        private double[] missX, missY;
        private GraphPane seriesPane = new GraphPane();
        private ZedGraphControl SeriesPlot;
        private NCEImessage NCEImsg = new NCEImessage();
        private string MISS = "9999";

        public clsGraph(ZedGraphControl _zgv)
        {
            SeriesPlot = _zgv;
        }

        public bool GenerateGraph(bool withMiss, string site, string svar,
            List<DateTime> dt, List<string> series, List<DateTime> dtmiss, List<string> missseries)
        {
            try
            {
                SeriesPlot.Invalidate();

                // get a reference to the GraphPane
                GraphPane seriesPane = SeriesPlot.GraphPane;
                seriesPane.Legend.IsVisible = false;
                string YAxisTitle = string.Empty;
                switch (svar)
                {
                    case "ATEM":
                        YAxisTitle = "Air Temperature, degree F";
                        break;
                    case "TEMP":
                        YAxisTitle = "Daily Air Temperature, degree F";
                        break;
                    case "TMAX":
                        YAxisTitle = "Maximum Temperature, degree F";
                        break;
                    case "TMIN":
                        YAxisTitle = "Minimum Temperature, degree F";
                        break;
                    case "WIND":
                        YAxisTitle = "Wind Speed, mi/hr";
                        break;
                    case "HUMI":
                        YAxisTitle = "Relative Humidity, %";
                        break;
                    case "DEWP":
                        YAxisTitle = "Dewpoint Temperature, degree F";
                        break;
                    case "CLOU":
                        YAxisTitle = "Cloud Cover, tenths";
                        break;
                    case "PREC":
                        YAxisTitle = "Hourly Rainfall, in";
                        break;
                    case "HPCP":
                        YAxisTitle = "Hourly Rainfall, in";
                        break;
                    case "PRCP":
                        YAxisTitle = "Daily Rainfall, in";
                        break;
                    case "SOLR":
                        YAxisTitle = "Solar Radiation, langley";
                        break;
                    case "LRAD":
                        YAxisTitle = "Longwave Radiation, langley";
                        break;
                    case "ATMP":
                        YAxisTitle = "Sea Level Pressure, mmHG";
                        break;
                    case "WNDD":
                        YAxisTitle = "Wind Direction, deg from North";
                        break;
                }

                // Set the Titles
                seriesPane.Title.Text = "";
                seriesPane.XAxis.Title.Text = "Date";
                seriesPane.YAxis.Title.Text = YAxisTitle;
                seriesPane.XAxis.Type = AxisType.Date;
                seriesPane.XAxis.Scale.FontSpec.Size = 8;
                seriesPane.YAxis.Scale.FontSpec.Size = 8;

                // Series arrays
                if (varSeries.Count > 0)
                    varSeries.Clear();

                //missing series
                if (fillSeries.Count > 0)
                    fillSeries.Clear();

                //for bar curve
                double min = 9999, max = -99;
                for (int i = 0; i < series.Count(); i++)
                {
                    if (!series[i].Contains(MISS))
                    {
                        double val = Convert.ToDouble(series[i]);
                        varSeries.Add(dt.ElementAt(i).ToOADate(), val);
                        if (val < min) min = val;
                        if (val > max) max = val;
                    }
                }
                double missmin = 9999, missmax = -99;
                if (withMiss)
                {
                    missX = new double[missseries.Count()];
                    missY = new double[missseries.Count()];
                    int j = 0;
                    for (int i = 0; i < missseries.Count(); i++)
                    {
                        if (!missseries[i].Contains(MISS))
                        {
                            missY[j] = Convert.ToDouble(missseries[i]);
                            missX[j] = dtmiss.ElementAt(i).ToOADate();
                            double val = Convert.ToDouble(missseries[i]);
                            fillSeries.Add(dtmiss.ElementAt(i).ToOADate(), val);
                            if (val < missmin) missmin = val;
                            if (val > missmax) missmax = val;
                            j++;
                        }
                    }

                }

                LineItem curve = seriesPane.AddCurve("", varSeries, Color.Blue, ZedGraph.SymbolType.Circle);
                curve.Line.IsVisible = true;
                curve.Symbol.IsVisible = false;
                curve.Symbol.Fill = new Fill(Color.Blue);
                curve.Symbol.Border.Color = Color.Blue;
                curve.Symbol.Size = 1;

                if (withMiss)
                {
                    LineItem misscurve = seriesPane.AddCurve("", fillSeries, Color.Red, ZedGraph.SymbolType.Circle);
                    misscurve.Line.IsVisible = false;
                    misscurve.Symbol.IsVisible = true;
                    misscurve.Symbol.Fill = new Fill(Color.Red);
                    misscurve.Symbol.Border.Color = Color.Red;
                    misscurve.Symbol.Size = 4;
                    //BarItem misscurve = seriesPane.AddBar("", missX, missY, Color.Red);
                }

                // Tell ZedGraph to refigure the
                // axes since the data have changed
                SeriesPlot.AxisChange();
                SeriesPlot.Refresh();
                return true;
            }
            catch (Exception ex)
            {
                NCEImsg.ShowError("Error generating graph!", ex);
                return false;
            }
        }
    }
}
