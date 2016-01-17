using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using System.Collections;
using OxyPlot;
using HelixToolkit;
using HelixToolkit.Wpf;
using OxyPlot.Wpf;
using System.IO;

namespace DiffusionOfSlowingNeutrons
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ModellingSession session;

        public MainWindow()
        {
            InitializeComponent();
            System.Uri pdf = new System.Uri(String.Format("file:///{0}/diffusion.pdf", Directory.GetCurrentDirectory()));
            webHelp.Navigate(pdf);

            axisEnergy1.LabelFormatter = EnergyFormatter;
            axisEnergy2.LabelFormatter = EnergyFormatter;
        }

        public void DrawNeutronWay(Result points) //отрисовка судьбы нейтрона
        {
            viewport.Children.Clear(); //очищаем
            if (points.Count == 0) //если нет точек, то выходим
                return;

            viewport.Children.Add(new DefaultLights()); //добавляем свет

            double maxEnergy = points[0].Energy; //запоминаем начальную энергию
            float size = Math.Min(0.1f, (float)points.AverageL / 5.0f); //размер шариков
            LinesVisual3D lines = new LinesVisual3D(); //соединительные линии
            lines.Thickness = 0.5; //толщина линий

            ArrowVisual3D xAxis = new ArrowVisual3D(); //координатные оси
            ArrowVisual3D yAxis = new ArrowVisual3D();
            ArrowVisual3D zAxis = new ArrowVisual3D();

            xAxis.Diameter = size / 2; //толщина осей - половина от размера шаров
            yAxis.Diameter = size / 2;
            zAxis.Diameter = size / 2;

            xAxis.Fill = new SolidColorBrush(Color.FromRgb(0x96, 0x4B, 0x4B)); //цвета осей
            yAxis.Fill = new SolidColorBrush(Color.FromRgb(0x4B, 0x96, 0x4B));
            zAxis.Fill = new SolidColorBrush(Color.FromRgb(0x4B, 0x4B, 0x96));

            float maxX = 1, maxY = 1, maxZ = 1; //в этих переменных будем хранить максимальные значения координат

            foreach (var point in points) //для каждой точки
            {
                SphereVisual3D item = new SphereVisual3D(); //создаем сферу
                item.Center = new Point3D(point.Position.X,
                                            point.Position.Y,
                                            point.Position.Z); //в данной точке

                item.Radius = size; //задаем размер

                //задаем цвет, чем меньше энергия, тем краснее
                byte red = (byte)(255 * (1 - point.Energy / maxEnergy));
                //чем ближе к начальной энергии - тем зеленее
                byte green = (byte)(255 * point.Energy / maxEnergy);
                item.Material  = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(red, green, 0)));
                viewport.Children.Add(item);
                
                maxX = Math.Max(maxX, (float)item.Center.X); //пересчитываем максимальные значения координат
                maxY = Math.Max(maxY, (float)item.Center.Y);
                maxZ = Math.Max(maxZ, (float)item.Center.Z);
            }

            xAxis.Point2 = new Point3D(maxX, 0, 0); //проводим оси от 0 до максимальных значений
            yAxis.Point2 = new Point3D(0, maxY, 0);
            zAxis.Point2 = new Point3D(0, 0, maxZ);

            viewport.Children.Add(xAxis); //добавляем оси
            viewport.Children.Add(yAxis);
            viewport.Children.Add(zAxis);

            for (int j = 1; j < points.Count; j++) //создаем соединительные линии
            {
                Point3D begin = new Point3D(points[j - 1].Position.X,
                                            points[j - 1].Position.Y,
                                            points[j - 1].Position.Z);
                Point3D end = new Point3D(points[j].Position.X,
                                            points[j].Position.Y,
                                            points[j].Position.Z);
                lines.Points.Add(begin);
                lines.Points.Add(end);
            }
            viewport.Children.Add(lines); //добавляем их
        }

        private void Button_Click(object sender, RoutedEventArgs e) //кнопка Начать моделирование
        {
            txtStart.Visibility = System.Windows.Visibility.Collapsed; //скрываем приветствие

            StartModelWindow win = new StartModelWindow(); //вызываем окно начальных параметров
            win.ShowDialog();

            Vector3D startPoint = win.Position; //получаем введенные значения
            double energy = win.Energy * 1e+6;
            int count = win.Count;

            EnvironmentPreset env;
            env = win.Env;

            session = new ModellingSession(env, energy, startPoint); //создаем новую сессию
            this.DataContext = session;

            //Пишем параметры среды для пользователя
            lblModelParams.Content = String.Format("Параметры среды:\nВещество: {0}\nМакросечение: {1:0.###} см^-1\nКоординаты источника:\n{{{2}, {3}, {4}}}\nЭнергия источника: {5} МэВ",
                    env.name, env.Sigma, startPoint.X, startPoint.Y, startPoint.Z, energy / 1e+6);

            //рассмотрим несколько судеб нейтронов
            int neutronToShow = -1;
            for (int i = 0; i < count; i++)
                neutronToShow = NextNeutron();

            if (neutronToShow != -1)
                ShowNeutron(neutronToShow); //покажем последний нейтрон
            plotAverageTau.InvalidatePlot(); //перерисуем графики
            plotAverageTauM.InvalidatePlot();
        }

        private int NextNeutron() //рассмотреть еще один нейтрон
        {
            if (session != null)
            {
                int neutronsCount = session.ModelNextNeutron();
                lstNeutrons.Items.Clear();
                for (int i = 1; i <= neutronsCount; i++)
                    lstNeutrons.Items.Add(i);
                return neutronsCount - 1;
            }
            return -1;
        }

        private void ShowNeutron(int i) //показать судьбу нейтрона
        {
            if (i == -1)
                return;
            Result neutron = session[i];
            DrawNeutronWay(neutron);
            lblStats.Content = String.Format("ls = {0}, τ = {1}", neutron.AverageL, neutron.GetR2ForE(Model.Et) / 6);
        }

        private static string EnergyFormatter(double d)
        {
            return String.Format("{0:0.###e-0}", d);
        }

        private void lstNeutrons_SelectionChanged(object sender, SelectionChangedEventArgs e) //выбрали нейтрон из списка
        {
            if (e.AddedItems.Count > 0)
            {
                int neutron = (int)e.AddedItems[0] - 1;
                ShowNeutron(neutron); //показать судьбу
            }
        }

        private void btnNextNeutron_Click(object sender, RoutedEventArgs e)
        {
            int neutronToShow = NextNeutron();
            ShowNeutron(neutronToShow);
            plotAverageTau.InvalidatePlot();
            plotAverageTauM.InvalidatePlot();
        }

        private void btnNextStep_Click(object sender, RoutedEventArgs e) //рассмотреть еще n нейтронов
        {
            int cnt = 0;
            string neutronsCountText = txtNeutronsToModel.Text; //сколько нейтронов рассмотреть
            bool status = int.TryParse(neutronsCountText, out cnt);
            int lastCnt = 0;
            if (status)
            {
                for (int i = 0; i < cnt; i++)
                {
                    lastCnt = NextNeutron();
                }
                ShowNeutron(lastCnt);
                plotAverageTau.InvalidatePlot();
                plotAverageTauM.InvalidatePlot();
            }
        }

        private void btnOther2_Click(object sender, RoutedEventArgs e) //рассмотреть две другие судьбы для графика E от r^2
        {
            plotEr.Series.Clear(); //очищаем график

            if (session != null && session.Count >= 2) //если есть хотя бы две судьбы
            {
                LineSeries Er2 = new LineSeries();
                Er2.ItemsSource = session.Er2ForRandomNeutron; //берем одну случайную судьбу
                plotEr.Series.Add(Er2); //добавляем на график

                Er2 = new LineSeries();
                Er2.ItemsSource = session.Er2ForRandomNeutron; //берем еще одну судьбу
                plotEr.Series.Add(Er2);

                plotEr.InvalidatePlot(); //перерисовываем график
            }
        }
    }
}
