using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SS_OpenCV
{
    public partial class HorizontalProjection : Form
    {
        public HorizontalProjection(int [] array)
        {
            InitializeComponent();
            DataPointCollection black_list = chart1.Series[0].Points;
            for (int i = 0; i < array.Length ; i++)
            {
                black_list.AddXY(i, array[i]);
            }

            chart1.Series[0].Color = Color.Black;
            chart1.ChartAreas[0].AxisX.Maximum = array.Length-1;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Title = "Coluna";
            chart1.ChartAreas[0].AxisY.Title = "Numero Pixeis";

            chart1.ResumeLayout();
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
