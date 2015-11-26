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
        public StartModelWindow()
        {
            InitializeComponent();
            this.position = new Vector3D(1, 2, 3);
            this.energy = 3;
            this.count = 10;
            this.twoComponents = false;
            this.env = new Environment[2];
            this.env[0].MassNumber = 12;
            this.env[0].Sigma = 1;
            this.DataContext = this;
        }

        Vector3D position;
        double energy;
        int count;
        bool twoComponents;
        Environment[] env;

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

        public Environment[] Env
        {
            get
            {
                return env;
            }
        }

        public bool TwoComponents
        {
            get
            {
                return twoComponents;
            }
            set
            {
                twoComponents = value;
            }
        }

        public double MassNumber1
        {
            get
            {
                return env[0].MassNumber;
            }
            set
            {
                env[0].MassNumber = value;
            }
        }

        public double Sigma1
        {
            get
            {
                return env[0].Sigma;
            }
            set
            {
                env[0].Sigma = value;
            }
        }

        public double MassNumber2
        {
            get
            {
                return env[1].MassNumber;
            }
            set
            {
                env[1].MassNumber = value;
            }
        }

        public double Sigma2
        {
            get
            {
                return env[1].Sigma;
            }
            set
            {
                env[1].Sigma = value;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
