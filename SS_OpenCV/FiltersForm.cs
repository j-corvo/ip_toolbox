using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;


namespace SS_OpenCV
{
    public partial class FiltersForm : Form
    {
        //declarar estrutura do filtro de convuloção
        private float[,] matrix = new float[3, 3];
        private float matrixWeight = 0;
        private float offset = 0;
        public FiltersForm()
        {
            InitializeComponent();
        }

        public float[,] getMatrix()
        {
            return matrix;
        }

        public float getMatrixWeight()
        {
            return matrixWeight;
        }

        public float getOffset()
        {
            return offset;
        }

        private void FiltersForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // verificar os vários filtros possíveis
            int selectedIndex = comboBox1.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    // colocar info do filtro de medias uniforme
                    textBox1.Text = "1";
                    textBox2.Text = "1";
                    textBox3.Text = "1";
                    textBox4.Text = "1";
                    textBox5.Text = "1";
                    textBox6.Text = "1";
                    textBox12.Text = "1";
                    textBox11.Text = "1";
                    textBox10.Text = "1";
                    //Weight
                    textBox7.Text = "9";
                    //Offset
                    
                    break;
                case 1:
                    // Gaussiano
                    textBox1.Text = "1";
                    textBox2.Text = "2";
                    textBox3.Text = "1";
                    textBox4.Text = "2";
                    textBox5.Text = "4";
                    textBox6.Text = "2";
                    textBox12.Text = "1";
                    textBox11.Text = "2";
                    textBox10.Text = "1";
                    //Weight
                    textBox7.Text = "16";
                    //Offset
                    break;
                case 2:
                    //laplacian hard
                    textBox1.Text = "1";
                    textBox2.Text = "-2";
                    textBox3.Text = "1";
                    textBox4.Text = "-2";
                    textBox5.Text = "4";
                    textBox6.Text = "-2";
                    textBox12.Text = "1";
                    textBox11.Text = "-2";
                    textBox10.Text = "1";
                    //Weight
                    textBox7.Text = "0.1";
                    //Offset
                    break;
                case 3:
                    //verticals lines
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    textBox3.Text = "0";
                    textBox4.Text = "-1";
                    textBox5.Text = "2";
                    textBox6.Text = "-1";
                    textBox12.Text = "0";
                    textBox11.Text = "0";
                    textBox10.Text = "0";
                    //Weight
                    textBox7.Text = "0.1";
                    //Offset
                    break;
                case 4:
                    //horizontal lines
                    textBox1.Text = "0";
                    textBox2.Text = "-1";
                    textBox3.Text = "0";
                    textBox4.Text = "0";
                    textBox5.Text = "2";
                    textBox6.Text = "0";
                    textBox12.Text = "0";
                    textBox11.Text = "-1";
                    textBox10.Text = "0";
                    //Weight
                    textBox7.Text = "0.1";
                    //Offset
                    break;
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            matrix[0,0] = float.Parse(textBox1.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            matrix[0, 1] = float.Parse(textBox2.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            matrix[0, 2] = float.Parse(textBox3.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            matrix[1, 0] = float.Parse(textBox4.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            matrix[1, 1] = float.Parse(textBox5.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            matrix[1, 2] = float.Parse(textBox6.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            matrix[2, 0] = float.Parse(textBox12.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            matrix[2, 1] = float.Parse(textBox11.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            matrix[2, 2] = float.Parse(textBox10.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            matrixWeight = float.Parse(textBox7.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            offset = float.Parse(textBox8.Text, System.Globalization.CultureInfo.InvariantCulture);
        }

        private void button1_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
