using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffusionOfSlowingNeutrons
{
    public struct Element
    {
        public double MassNumber;
        public double Sigma;

        //mass - атомный номер
        //sigma - микроскопическое сечение рассеивания, * 10^-24
        public Element(double mass, double sigma)
        {
            this.MassNumber = mass;
            this.Sigma = sigma * 1e-24;
        }
    }

    public struct EnvironmentPreset
    {
        const double Na = 6.023e+23; //число Авогадро

        public Element[] env; //элементы
        public int[] amount; //количество каждого элемента в молекуле
        public string name; //название вещества
        public float density; //плотность

        //макроскопическое сечение для среды
        public double Sigma
        {
            get
            {
                double sigma = 0;
                for(int i = 0; i < env.Length; i++)
                {
                    sigma += SigmaElement(i);
                }
                return sigma;
            }
        }

        //молярная масса вещества, г/моль
        public double MolarMass
        {
            get
            {
                double mass = 0;
                for (int i = 0; i < env.Length; i++)
                    mass += env[i].MassNumber * amount[i];
                return mass;
            }
        }

        //макроскопическое сечение для одного сорта атомов
        public double SigmaElement(int element)
        {
            double N = Math.Round(density * Na / MolarMass);
            return N * env[element].Sigma * amount[element];
        }

        public EnvironmentPreset(string name, float density, Element[] env, int[] amount)
        {
            this.env = env;
            this.amount = amount;
            this.name = name;
            this.density = density;
        }

        override public string ToString()
        {
            return name;
        }
    }
}
