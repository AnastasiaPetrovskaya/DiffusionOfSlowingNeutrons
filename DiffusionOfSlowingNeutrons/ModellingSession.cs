using OxyPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace DiffusionOfSlowingNeutrons
{
    class ModellingSession
    {
        List<Result> neutrons;
        Model model;
        List<DataPoint> averageL;

        public ModellingSession(Environment[] env, double energy, Vector3D position)
        {
            this.model = new Model(env, energy, position);
            this.neutrons = new List<Result>();
            this.averageL = new List<DataPoint>();
        }

        //return neutrons count for list box
        public int ModelNextNeutron()
        {
            Result res = model.mainCalculations();
            neutrons.Add(res);
            DataPoint avgL = new DataPoint(neutrons.Count, 0);
            for (int i = 0; i < neutrons.Count; i++)
            {
                avgL.Y += neutrons[i].AverageL;
            }
            avgL.Y /= neutrons.Count;
            averageL.Add(avgL);
            return neutrons.Count();
        }

        public Result this[int i]
        {
            get
            {
                return neutrons[i];
            }
        }

        public List<DataPoint> AverageL
        {
            get
            {
                return averageL;
            }
        }
    }
}
