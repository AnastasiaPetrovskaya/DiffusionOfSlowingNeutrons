using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffusionOfSlowingNeutrons
{
    public class Result : List<ResultPoint> //судьба нейтрона - список точек
    {
        double averageL; //средняя длина свободного пробега до столкновения

        public new void Add(ResultPoint point) //добавление новой точки
        {
            base.Add(point);
            RefreshStats();
        }

        private void RefreshStats() //обновление среднего значения
        {
            averageL = 0.0f;
            double sumL = 0.0f;
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

        public double GetR2ForE(double energy) //получение r^2 для заданной энергии
        {
            double r = 0;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Energy >= energy)
                    r = (this[i].Position - this[0].Position).LengthSquared;
                else
                    break;
            }
            return r;
        }
    }
}
