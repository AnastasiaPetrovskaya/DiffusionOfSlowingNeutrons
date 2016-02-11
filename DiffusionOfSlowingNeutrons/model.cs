using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace DiffusionOfSlowingNeutrons
{

    public struct ResultPoint
    {
        public Vector3D Position;
        public double Energy;
    }

    class Model
    {
        EnvironmentPreset data; //массовые числа ядер и соответствующие макроконстанты
        double energy; //энергия нейтронов источника
        Vector3D position; //координаты источника нейтронов
        Random rand;
        public const double Et = 0.025;

        //конструктор
        public Model(EnvironmentPreset env, double eng, Vector3D pos)
        {
            data = env;
            energy = eng;
            position = pos;
            rand = new Random();
        }

        //свойства
        public EnvironmentPreset Data 
        {
            get { return data; }
            set { data = value; }
        }
        public double Energy
        {
            get { return energy;}
            set { energy = value; }
        }
        public Vector3D Position
        {
            get { return position; }
            set { position = value; }
        }

        private double WayLength()
        {
            //длина свободного пробега нейтрона до столкновения
            double gamma = rand.NextDouble();
            double sumSigma = data.SigmaTr;
            double gammaLn = Math.Log(gamma);
            double res;
            res = -(gammaLn / sumSigma);
            return res;            
        }

        private Vector3D Omega()
        {
            //направляющие косинусы движения нейтронов от изотропного источника
            double gamma = rand.NextDouble();
            Vector3D res = new Vector3D();
            res.Z = 1.0 - 2.0 * gamma;
            res.X = Math.Sqrt(1.0 - res.Z * res.Z) * Math.Cos(gamma * 2.0 * Math.PI);
            res.Y = Math.Sqrt(1.0 - res.Z * res.Z) * Math.Sin(2.0 * Math.PI * gamma);
            return res;
        }

        private Vector3D ContactPoint(Vector3D omegaCos, double wayLenght, Vector3D currentPos)
        {
            //расчет точки, где нейтрон столкнулся с ядром
            Vector3D res = new Vector3D();
            res.X = currentPos.X + omegaCos.X * wayLenght;
            res.Y = currentPos.Y + omegaCos.Y * wayLenght;
            res.Z = currentPos.Z + omegaCos.Z * wayLenght;
            return res;
        }

        private int ChooseElement()
        {
            // определяется с ядром какого сорта столкнулся нейтрон
            double gamma = rand.NextDouble();
            double sumSigma = data.Sigma;
            if (gamma < data.SigmaElement(0) / sumSigma)
            {
                return 0;
            }
            return 1;
        }

        private double Final(int element, double curEnergy, Vector3D omega)
        {
            double eps = Math.Pow((data.env[element].MassNumber - 1.0), 2) / Math.Pow((data.env[element].MassNumber + 1.0), 2);
            double resEnergy;
            resEnergy = curEnergy * ((1.0 + eps) + (1.0 - eps) * omega.Z) / 2;
            return resEnergy;
        }

        public Result mainCalculations()
        {
            Result res = new Result();
            double resEnergy = energy;
            Vector3D curPosition = position;
            ResultPoint r;
            r.Position = position;
            r.Energy = energy;
            res.Add(r);
            do
            {
                double ls = WayLength();
                Vector3D omega = Omega();
                Vector3D contactPoint = ContactPoint(omega, ls, curPosition);
                curPosition = contactPoint;
                int elementNumber;
                if (data.env.Length == 2)
                {
                    elementNumber = ChooseElement();
                }
                else
                {
                    elementNumber = 0;
                }
                resEnergy = Final(elementNumber, resEnergy, omega);
                r.Energy = resEnergy;
                r.Position = contactPoint;
                res.Add(r);
            } while (resEnergy >= Et);
            return res;
        }
    }
}
