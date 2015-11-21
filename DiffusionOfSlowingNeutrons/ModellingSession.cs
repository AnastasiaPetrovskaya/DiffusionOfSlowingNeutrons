using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DiffusionOfSlowingNeutrons
{
    class ModellingSession
    {
        List<List<Result>> neutrons;
        Model model;

        public ModellingSession(Environment[] env, double energy, Vector3D position)
        {
            this.model = new Model(env, energy, position);
            this.neutrons = new List<List<Result>>();
        }

        //return neutrons count for list box
        public int ModelNextNeutron()
        {
            List<Result> res = model.mainCalculations();
            neutrons.Add(res);
            return neutrons.Count();
        }

        public List<Result> this[int i]
        {
            get
            {
                return neutrons[i];
            }
        }
    }
}
