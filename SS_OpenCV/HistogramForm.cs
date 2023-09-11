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
   
    public partial class HistogramForm : Form
    {
        
        public   HistogramForm(int [,] array)
        {
            InitializeComponent();
            DataPointCollection red_list = chart1.Series[2].Points;
            DataPointCollection green_list = chart1.Series[1].Points;
            DataPointCollection blue_list = chart1.Series[0].Points;
            DataPointCollection gray_list = chart2.Series[0].Points;
            for (int i = 0; i < 256; i++)
            {
                red_list.AddXY(i, array[3, i]);
                green_list.AddXY(i, array[2, i]);
                blue_list.AddXY(i, array[1, i]);
                gray_list.AddXY(i, array[0, i]);
            }

            chart1.Series[2].Color = Color.Red;
            chart1.Series[1].Color = Color.Green;
            chart1.Series[0].Color = Color.Blue;
            chart2.Series[0].Color = Color.Gray;
            chart1.ChartAreas[0].AxisX.Maximum = 255;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Title = "Intensidade";
            chart1.ChartAreas[0].AxisY.Title = "Numero Pixeis";
            chart2.ChartAreas[0].AxisX.Maximum = 255;
            chart2.ChartAreas[0].AxisX.Minimum = 0;
            chart2.ChartAreas[0].AxisX.Title = "Intensidade";
            chart2.ChartAreas[0].AxisY.Title = "Numero Pixeis";


            chart2.ResumeLayout();
            chart1.ResumeLayout();

        }

        public void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }
    }
}
