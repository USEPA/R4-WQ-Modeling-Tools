using System;
using System.Windows.Forms;

namespace NCEIData
{
    class clsStats
    {
        static double sum, ncount;        //daily
        static double msum, mncount;      //monthly
        static double asum, ancount;      //annual

        private string MISS = "9999";

        public clsStats(string _MISS)
        {
            this.MISS = _MISS;
            InitDaily();
            InitMonthly();
            InitAnnual();
        }

        public void InitDaily()
        {
            sum = 0;
            ncount = 0;
        }
        public void InitMonthly()
        {
            msum = 0;
            mncount = 0;
        }
        public void InitAnnual()
        {
            asum = 0;
            ancount = 0;
        }
        public void DailySum(string dat)
        {
            if (!dat.Contains(MISS))
            {
                sum += Convert.ToDouble(dat);
                ncount += 1.0;
            }
        }
        public void MonthlySum(string dat)
        {
            if (!dat.Contains(MISS))
            {
                msum += Convert.ToDouble(dat);
                mncount += 1.0;
            }
        }
        public void AnnualSum(string dat)
        {
            if (!dat.Contains(MISS))
            {
                asum += Convert.ToDouble(dat);
                ancount += 1.0;
            }
        }
        public double DailyAverage()
        {
            double avg;
            if (ncount > 0.0)
                sum /= ncount;
            else
                sum = 9999;
            avg = sum;
            return avg;
        }
        public double MonthlyAverage()
        {
            double avg;
            if (mncount > 0.0)
                msum /= mncount;
            else
                msum = 9999;
            avg = msum;
            return avg;
        }
        public double AnnualAverage()
        {
            double avg;
            if (ancount > 0.0)
                asum /= ancount;
            else
                asum = 9999;
            avg = asum;
            return avg;
        }
        private void ShowError(string msg, Exception ex)
        {
            msg += "\r\n\r\n" + ex.Message + "\r\n\r\n" + ex.StackTrace;
            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

    }
}
