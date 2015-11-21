﻿using System;
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
        private WPFChart3D.Chart3D neutronWay;       // data for 3d chart
        public int m_nChartModelIndex = -1;         // model index in the Viewport3d
        public int m_nSurfaceChartGridNo = 100;     // surface chart grid no. in each axis
        public int m_nScatterPlotDataNo = 5000;     // total data number of the scatter plot

        // ***************************** selection rect ***************************
        ViewportRect m_selectRect = new ViewportRect();
        public int m_nRectModelIndex = -1;
        ModellingSession session;

        public MainWindow()
        {
            InitializeComponent();

            m_selectRect.SetRect(new Point(-0.5, -0.5), new Point(-0.5, -0.5));
            WPFChart3D.Model3D model3d = new WPFChart3D.Model3D();
            ArrayList meshs = m_selectRect.GetMeshes();
            m_nRectModelIndex = model3d.UpdateModel(meshs, null, m_nRectModelIndex, this.vpNeutrons);
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

        public void DrawNeutronWay(List<Result> points)
        {
            if (points.Count == 0)
                return;

            float dataRange = 1.0f;
            neutronWay = new ScatterChart3D();
            neutronWay.SetDataNo(points.Count());
            double maxEnergy = points[0].Energy;
            int i = 0;
            foreach (var point in points)
            {
                Console.WriteLine("Position: {0}, {1}, {2}; Energy: {3}", point.Position.X, point.Position.Y, point.Position.Z, point.Energy);

                ScatterPlotItem item = new ScatterPlotItem();
                item.x = (float)point.Position.X;
                item.y = (float)point.Position.Y;
                item.z = (float)point.Position.Z;

                item.w = 0.2f; //(float)point.Energy;
                item.h = 0.2f; //(float)point.Energy;

                dataRange = Math.Max(dataRange, Math.Abs(item.x));
                dataRange = Math.Max(dataRange, Math.Abs(item.y));
                dataRange = Math.Max(dataRange, Math.Abs(item.z));

                item.shape = (int)ScatterChart3D.SHAPE.ELLIPSE;
                byte red = (byte)(255 * (1 - point.Energy / maxEnergy));
                byte green = (byte)(255 * point.Energy / maxEnergy);
                item.color = Color.FromRgb(red, green, 0);
                ((ScatterChart3D)neutronWay).SetVertex(i, item);
                i++;
            }

            neutronWay.GetDataRange();
            neutronWay.SetAxes();

            ArrayList meshs = ((ScatterChart3D)neutronWay).GetMeshes();
            WPFChart3D.Model3D model3d = new WPFChart3D.Model3D();
            m_nChartModelIndex = model3d.UpdateModel(meshs, null, m_nChartModelIndex, this.vpNeutrons);

            float viewRange = (float)dataRange;
            m_transformMatrix.CalculateProjectionMatrix(0, viewRange, 0, viewRange, 0, viewRange, 0.5);
            TransformChart();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment[] env = new Environment[1];
            env[0].MassNumber = 12;
            env[0].Sigma = 1;

            session = new ModellingSession(env, 3, new Vector3D(1, 2, 3));

            int neutronToShow = NextNeutron();
            ShowNeutron(neutronToShow);
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
            List<Result> neutron = session[i];
            DrawNeutronWay(neutron);
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
                neutronWay.Select(m_selectRect, m_transformMatrix, vpNeutrons);

                // 3. update selection display
                neutronWay.HighlightSelection(meshGeometry, Color.FromRgb(200, 200, 200));
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
            //else if (e.RightButton == MouseButtonState.Pressed)          // select rect
            //{
            //    m_selectRect.OnMouseMove(pt, vpNeutrons, m_nRectModelIndex);
            //}
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

        public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs args)
        {
            m_transformMatrix.OnKeyDown(args);
            TransformChart();
        }

        private void vpNeutrons_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point pt = e.GetPosition(vpNeutrons);
            if (e.ChangedButton == MouseButton.Left)         // rotate or drag 3d model
            {
                m_transformMatrix.OnLBtnDown(pt);
            }
            //else if (e.ChangedButton == MouseButton.Right)   // select rect
            //{
            //    m_selectRect.OnMouseDown(pt, vpNeutrons, m_nRectModelIndex);
            //}
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
            }
        }
    }
}
