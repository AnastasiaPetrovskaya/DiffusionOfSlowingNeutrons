using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffusionOfSlowingNeutrons
{
    public class Result : List<ResultPoint>
    {
        double averageL;
        double sumL;

        public new void Add(ResultPoint point)
        {
            base.Add(point);
            RefreshStats();
        }

        private void RefreshStats()
        {
            averageL = 0.0f;
            sumL = 0.0f;
            int cnt = this.Count();

            for (int i = 1; i < cnt; i++)
            {
                sumL += (this[i].Position - this[i-1].Position).Length;
            }
            averageL = sumL / (cnt - 1);
        }

        public double AverageL
        {
            get
            {
                return averageL;
            }
       } 

        public double SumL
        {
            get
            {
                return sumL;
            }
        }
    }
}
