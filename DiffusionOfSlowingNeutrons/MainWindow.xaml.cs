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
        }

        public void DrawNeutronWay(Result points)
        {
            viewport.Children.Clear();
            if (points.Count == 0)
                return;

            viewport.Children.Add(new DefaultLights());

            double maxEnergy = points[0].Energy;
            float size = Math.Min(0.1f, (float)points.AverageL / 5.0f);
            int i = 0;
            LinesVisual3D lines = new LinesVisual3D();
            lines.Thickness = 0.5;

            ArrowVisual3D xAxis = new ArrowVisual3D();
            ArrowVisual3D yAxis = new ArrowVisual3D();
            ArrowVisual3D zAxis = new ArrowVisual3D();

            xAxis.Diameter = size / 2;
            yAxis.Diameter = size / 2;
            zAxis.Diameter = size / 2;

            xAxis.Fill = new SolidColorBrush(Color.FromRgb(0x96, 0x4B, 0x4B));
            yAxis.Fill = new SolidColorBrush(Color.FromRgb(0x4B, 0x96, 0x4B));
            zAxis.Fill = new SolidColorBrush(Color.FromRgb(0x4B, 0x4B, 0x96));

            float maxX = 1, maxY = 1, maxZ = 1;

            foreach (var point in points)
            {
                SphereVisual3D item = new SphereVisual3D();
                item.Center = new Point3D(point.Position.X,
                                            point.Position.Y,
                                            point.Position.Z);

                item.Radius = size;

                byte red = (byte)(255 * (1 - point.Energy / maxEnergy));
                byte green = (byte)(255 * point.Energy / maxEnergy);
                item.Material  = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(red, green, 0)));
                viewport.Children.Add(item);
                i++;

                
                maxX = Math.Max(maxX, (float)item.Center.X);
                maxY = Math.Max(maxY, (float)item.Center.Y);
                maxZ = Math.Max(maxZ, (float)item.Center.Z);
            }

            xAxis.Point2 = new Point3D(maxX, 0, 0);
            yAxis.Point2 = new Point3D(0, maxY, 0);
            zAxis.Point2 = new Point3D(0, 0, maxZ);

            viewport.Children.Add(xAxis);
            viewport.Children.Add(yAxis);
            viewport.Children.Add(zAxis);

            for (int j = 1; j < points.Count; j++)
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
            viewport.Children.Add(lines);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartModelWindow win = new StartModelWindow();
            win.ShowDialog();

            Vector3D startPoint = win.Position;
            double energy = win.Energy;
            int count = win.Count;

            //Environment[] env = new Environment[1];
            //env[0].MassNumber = 12;
            //env[0].Sigma = 1;

            Environment[] env;
            if (win.TwoComponents)
            {
                env = win.Env;
            }
            else
            {
                env = new Environment[1];
                env[0] = win.Env[0];
            }

            session = new ModellingSession(env, energy, startPoint);
            this.DataContext = session;

            if (env.Count() == 1)
                lblModelParams.Content = String.Format("Параметры среды:\nМассовое число: {0}\nМакросечение: {1}\nКоординаты источника:\n{{{2}, {3}, {4}}}\nЭнергия источника: {5}",
                    env[0].MassNumber, env[0].Sigma, startPoint.X, startPoint.Y, startPoint.Z, energy);
            else if (env.Count() == 2)
                lblModelParams.Content = String.Format("Параметры среды:\nЭлемент 1:\nМассовое число: {0}\nМакросечение: {1}\nЭлемент 2:\nМассовое число: {2}\nМакросечение: {3}\nКоординаты источника:\n{{{4}, {5}, {6}}}\nЭнергия источника: {7}",
                    env[0].MassNumber, env[0].Sigma, env[1].MassNumber, env[1].Sigma, startPoint.X, startPoint.Y, startPoint.Z, energy);

            int neutronToShow = -1;
            for (int i = 0; i < count; i++)
                neutronToShow = NextNeutron();

            if (neutronToShow != -1)
                ShowNeutron(neutronToShow);
            plotAverageL.InvalidatePlot();
        }

        private int NextNeutron()
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

        private void ShowNeutron(int i)
        {
            if (i == -1)
                return;
            Result neutron = session[i];
            DrawNeutronWay(neutron);
            lblStats.Content = String.Format("<l> = {0}, L = {1}, t = {2}", neutron.AverageL, neutron.SumL, neutron.Count - 1);
        }

        private void lstNeutrons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                int neutron = (int)e.AddedItems[0] - 1;
                ShowNeutron(neutron);
            }
        }

        private void btnNextNeutron_Click(object sender, RoutedEventArgs e)
        {
            int neutronToShow = NextNeutron();
            ShowNeutron(neutronToShow);
            plotAverageL.InvalidatePlot();
        }

        private void btnNextStep_Click(object sender, RoutedEventArgs e)
        {
            int cnt = 0;
            string neutronsCountText = txtNeutronsToModel.Text;
            bool status = int.TryParse(neutronsCountText, out cnt);
            int lastCnt = 0;
            if (status)
            {
                for (int i = 0; i < cnt; i++)
                {
                    lastCnt = NextNeutron();
                }
                ShowNeutron(lastCnt);
                plotAverageL.InvalidatePlot();
            }
        }
    }
}
