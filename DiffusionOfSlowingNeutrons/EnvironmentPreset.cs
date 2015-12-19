using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffusionOfSlowingNeutrons
{
    struct EnvironmentPreset
    {
        public Element[] env;
        public string name;

        public EnvironmentPreset(string name, Element[] env)
        {
            this.env = env;
            this.name = name;
        }

        override public string ToString()
        {
            return name;
        }
    }
}
