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
using WPFChart;
using WPFChart3D;
using System.Collections;

namespace DiffusionOfSlowingNeutrons
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // transform class object for rotate the 3d model
        public WPFChart3D.TransformMatrix m_transformMatrix = new WPFChart3D.TransformMatrix();

        // ***************************** 3d chart ***************************
        private WPFChart3D.Chart3D m_3dChart;       // data for 3d chart
        public int m_nChartModelIndex = -1;         // model index in the Viewport3d
        public int m_nSurfaceChartGridNo = 100;     // surface chart grid no. in each axis
        public int m_nScatterPlotDataNo = 5000;     // total data number of the scatter plot

        // ***************************** selection rect ***************************
        ViewportRect m_selectRect = new ViewportRect();
        public int m_nRectModelIndex = -1;

        public MainWindow()
        {
            InitializeComponent();

            m_selectRect.SetRect(new Point(-0.5, -0.5), new Point(-0.5, -0.5));
            WPFChart3D.Model3D model3d = new WPFChart3D.Model3D();
            ArrayList meshs = m_selectRect.GetMeshes();
            m_nRectModelIndex = model3d.UpdateModel(meshs, null, m_nRectModelIndex, this.vpNeutrons);
        }

        public void TestScatterPlot(int nDotNo)
        {
            // 1. set scatter chart data no.
            m_3dChart = new ScatterChart3D();
            m_3dChart.SetDataNo(nDotNo);

            // 2. set property of each dot (size, position, shape, color)
            Random randomObject = new Random();
            int nDataRange = 200;
            for (int i = 0; i < nDotNo; i++)
            {
                ScatterPlotItem plotItem = new ScatterPlotItem();

                plotItem.w = 4;
                plotItem.h = 6;

                plotItem.x = (float)randomObject.Next(nDataRange);
                plotItem.y = (float)randomObject.Next(nDataRange);
                plotItem.z = (float)randomObject.Next(nDataRange);

                plotItem.shape = randomObject.Next(4);

                Byte nR = (Byte)randomObject.Next(256);
                Byte nG = (Byte)randomObject.Next(256);
                Byte nB = (Byte)randomObject.Next(256);

                plotItem.color = Color.FromRgb(nR, nG, nB);
                ((ScatterChart3D)m_3dChart).SetVertex(i, plotItem);
            }

            // 3. set the axes
            m_3dChart.GetDataRange();
            m_3dChart.SetAxes();

            // 4. get Mesh3D array from the scatter plot
            ArrayList meshs = ((ScatterChart3D)m_3dChart).GetMeshes();

            // 6. display scatter plot in Viewport3D
            WPFChart3D.Model3D model3d = new WPFChart3D.Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, this.vpNeutrons);

            // 7. set projection matrix
            float viewRange = (float)nDataRange;
            m_transformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        private void TransformChart()
        {
            if (m_nChartModelIndex == -1) return;
            ModelVisual3D visual3d = (ModelVisual3D)(this.vpNeutrons.Children[m_nChartModelIndex]);
            if (visual3d.Content == null) return;
            Transform3DGroup group1 = visual3d.Content.Transform as Transform3DGroup;
            group1.Children.Clear();
            group1.Children.Add(new MatrixTransform3D(m_transformMatrix.m_totalMatrix));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int nDataNo = 50;
            TestScatterPlot(nDataNo);
        }

        private void vpNeutrons_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(vpNeutrons);
            if (e.ChangedButton == MouseButton.Left)
            {
                m_transformMatrix.OnLBtnUp();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                if (m_nChartModelIndex == -1) return;
                // 1. get the mesh structure related to the selection rect
                MeshGeometry3D meshGeometry = WPFChart3D.Model3D.GetGeometry(vpNeutrons, m_nChartModelIndex);
                if (meshGeometry == null) return;

                // 2. set selection in 3d chart
                m_3dChart.Select(m_selectRect, m_transformMatrix, vpNeutrons);

                // 3. update selection display
                m_3dChart.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));
            }
        }

        private void vpNeutrons_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = e.GetPosition(vpNeutrons);

            if (e.LeftButton == MouseButtonState.Pressed)                // rotate or drag 3d model
            {
                m_transformMatrix.OnMouseMove(pt, vpNeutrons);

                TransformChart();
            }
            else if (e.RightButton == MouseButtonState.Pressed)          // select rect
            {
                m_selectRect.OnMouseMove(pt, vpNeutrons, m_nRectModelIndex);
            }
            else
            {
                /*
                String s1;
                Point pt2 = m_transformMatrix.VertexToScreenPt(new Point3D(0.5, 0.5, 0.3), vpNeutrons);
                s1 = string.Format("Screen:({0:d},{1:d}), Predicated: ({2:d}, H:{3:d})", 
                    (int)pt.X, (int)pt.Y, (int)pt2.X, (int)pt2.Y);
                this.statusPane.Text = s1;
                */
            }
        }

        private void vpNeutrons_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(vpNeutrons);
            if (e.ChangedButton == MouseButton.Left)         // rotate or drag 3d model
            {
                m_transformMatrix.OnLBtnDown(pt);
            }
            else if (e.ChangedButton == MouseButton.Right)   // select rect
            {
                m_selectRect.OnMouseDown(pt, vpNeutrons, m_nRectModelIndex);
            }
        }
    }
}
