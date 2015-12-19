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
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace DiffusionOfSlowingNeutrons
{
    /// <summary>
    /// Interaction logic for StartModelWindow.xaml
    /// </summary>
    public partial class StartModelWindow : Window
    {
        List<EnvironmentPreset> environments;
        Vector3D position;
        double energy;
        int count;
        Element[] env;

        public StartModelWindow()
        {
            InitializeComponent();

            environments = new List<EnvironmentPreset>(); //добавляем параметры для различных сред
            environments.Add(new EnvironmentPreset("H2O", new Element[]{new Element(1, 1), new Element(16, 1)}));
            environments.Add(new EnvironmentPreset("D2O", new Element[]{new Element(2, 1), new Element(16, 1)}));
            environments.Add(new EnvironmentPreset("Be", new Element[]{new Element(9, 1)}));
            environments.Add(new EnvironmentPreset("BeO", new Element[]{new Element(9, 1), new Element(16, 1)}));
            environments.Add(new EnvironmentPreset("C", new Element[]{new Element(12, 1)}));

            lstEnvironment.ItemsSource = environments;
            lstEnvironment.SelectedItem = environments.Last();

            this.position = new Vector3D(0, 0, 0); //координаты источника
            this.energy = 1; //начальная энергия (МэВ)
            this.count = 10; //число судеб для рассмотрения

            //среда - углерод
            this.env = ((EnvironmentPreset)lstEnvironment.SelectedItem).env;

            this.DataContext = this;
        }

        public Vector3D Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public double Energy
        {
            get
            {
                return energy;
            }
            set
            {
                energy = value;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }

        public Element[] Env
        {
            get
            {
                return env;
            }
        }

        List<EnvironmentPreset> Environments
        {
            get
            {
                return environments;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lstEnvironment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnvironmentPreset preset = (EnvironmentPreset)e.AddedItems[0];
            env = preset.env;
        }
    }
}
