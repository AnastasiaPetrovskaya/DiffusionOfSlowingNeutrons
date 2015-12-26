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
        List<Result> neutrons; //список судеб нейтронов
        Model model; //экспериментальная модель
        List<DataPoint> averageL, averageR2, averageTau, averageTauM; //данные для графиков
        Random rnd;

        public ModellingSession(EnvironmentPreset env, double energy, Vector3D position)
        {
            this.rnd = new Random();
            this.model = new Model(env, energy, position);
            this.neutrons = new List<Result>();
            this.averageL = new List<DataPoint>();
            this.averageR2 = new List<DataPoint>();
            this.averageTau = new List<DataPoint>();
            this.averageTauM = new List<DataPoint>();
        }

        public int ModelNextNeutron() //промоделировать судьбу одного нейтрона
        {
            Result res = model.mainCalculations();
            neutrons.Add(res); //добавляем судьбу в список

            //добавляем новое значение средней длины свободного пробега до столкновения
            DataPoint avgL = new DataPoint(neutrons.Count, 0);
            for (int i = 0; i < neutrons.Count; i++)
            {
                avgL.Y += neutrons[i].AverageL;
            }
            avgL.Y /= neutrons.Count;
            averageL.Add(avgL);

            //пересчитываем r^2 от E и возраст нейтрона от E
            this.averageR2.Clear();
            this.averageTau.Clear();
            for(double E = model.Energy; E >= 0.025; E -= model.Energy / 100.0) //идем от начальной энергии до 0.025 эВ за 100 шагов
            {
                DataPoint r = new DataPoint(E, 0);
                for (int i = 0; i < neutrons.Count; i++)
                {
                    r.Y += neutrons[i].GetR2ForE(E); //получаем r^2 для данной энергии
                }
                r.Y /= neutrons.Count;
                this.averageR2.Add(r);
                this.averageTau.Add(new DataPoint(r.X, r.Y / 6.0)); // tau = 1/6 * r^2, возраст нейтрона
            }

            this.averageTauM.Add(new DataPoint(neutrons.Count, averageTau.Last().Y)); //добавляем новый средний возраст нейтрона

            return neutrons.Count(); // возвращаем количество судеб
        }

        public int Count //количество судеб
        {
            get
            {
                return neutrons.Count;
            }
        }

        public Result this[int i] //i-ая судьба
        {
            get
            {
                return neutrons[i];
            }
        }

        public List<DataPoint> AverageL //средняя длина свободного пробега до столкновения
        {
            get
            {
                return averageL;
            }
        }

        public List<DataPoint> AverageR2 //средняя величина r^2 от E
        {
            get
            {
                return averageR2;
            }
        }

        public List<DataPoint> AverageTau //средний возраст нейтрона от E
        {
            get
            {
                return averageTau;
            }
        }

        public List<DataPoint> AverageTauM //средний возраст нейтрона от количества судеб
        {
            get
            {
                return averageTauM;
            }
        }

        public List<DataPoint> Er2ForRandomNeutron //зависимость E от r^2 для случайной судьбы
        {
            get
            {
                List<DataPoint> result = new List<DataPoint>();
                Result neutron = neutrons[rnd.Next(neutrons.Count)]; //берем случайную судьбу из списка
                for (int i = 0; i < neutron.Count; i++)
                {
                    result.Add(new DataPoint((neutron[i].Position - neutron[0].Position).LengthSquared, neutron[i].Energy));
                }
                result.Sort((point1, point2) => point1.X.CompareTo(point2.X)); //сортруем точки по r^2
                return result;
            }
        }
    }
}
