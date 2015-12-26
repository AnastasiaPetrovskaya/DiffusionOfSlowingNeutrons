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
        EnvironmentPreset env;

        public StartModelWindow()
        {
            InitializeComponent();

            environments = new List<EnvironmentPreset>(); //добавляем параметры для различных сред
            environments.Add(new EnvironmentPreset("H2O", 1, new Element[]{new Element(1, 20.4), new Element(16, 3.76)}, new int[]{2, 1}));
            environments.Add(new EnvironmentPreset("D2O", 1.11f, new Element[] { new Element(2, 3.39), new Element(16, 3.76) }, new int[] {2, 1}));
            environments.Add(new EnvironmentPreset("Be", 1.848f, new Element[]{new Element(9, 6.14)}, new int[]{1}));
            environments.Add(new EnvironmentPreset("BeO", 3.02f, new Element[] { new Element(9, 6.14), new Element(16, 3.76) }, new int[] {1, 1}));
            environments.Add(new EnvironmentPreset("C", 2.25f, new Element[] { new Element(12, 4.75) }, new int[] {1}));

            lstEnvironment.ItemsSource = environments;
            lstEnvironment.SelectedItem = environments.Last();

            this.position = new Vector3D(0, 0, 0); //координаты источника
            this.energy = 1; //начальная энергия (МэВ)
            this.count = 10; //число судеб для рассмотрения

            //по умолчанию среда - углерод
            this.env = ((EnvironmentPreset)lstEnvironment.SelectedItem);

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

        public EnvironmentPreset Env
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
            env = preset;
        }
    }
}
