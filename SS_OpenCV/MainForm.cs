using System;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SS_OpenCV
{ 
    public partial class MainForm : Form
    {
        Image<Bgr, Byte> img = null; // working image
        Image<Bgr, Byte> imgUndo = null; // undo backup image - UNDO
        string title_bak = "";

        public MainForm()
        {
            InitializeComponent();
            title_bak = Text;
        }

        /// <summary>
        /// Opens a new image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                img = new Image<Bgr, byte>(openFileDialog1.FileName);
                Text = title_bak + " [" +
                        openFileDialog1.FileName.Substring(openFileDialog1.FileName.LastIndexOf("\\") + 1) +
                        "]";
                imgUndo = img.Copy();
                ImageViewer.Image = img.Bitmap;
                ImageViewer.Refresh();
            }
        }

        /// <summary>
        /// Saves an image with a new name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageViewer.Image.Save(saveFileDialog1.FileName);
            }
        }

        /// <summary>
        /// Closes the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// restore last undo copy of the working image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (imgUndo == null) // verify if the image is already opened
                return; 
            Cursor = Cursors.WaitCursor;
            img = imgUndo.Copy();

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        /// <summary>
        /// Change visualization mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // zoom
            if (autoZoomToolStripMenuItem.Checked)
            {
                ImageViewer.SizeMode = PictureBoxSizeMode.Zoom;
                ImageViewer.Dock = DockStyle.Fill;
            }
            else // with scroll bars
            {
                ImageViewer.Dock = DockStyle.None;
                ImageViewer.SizeMode = PictureBoxSizeMode.AutoSize;
            }
        }

        /// <summary>
        /// Show authors form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AuthorsForm form = new AuthorsForm();
            form.ShowDialog();
        }

        /// <summary>
        /// Calculate the image negative
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void negativeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            DateTime t_init = DateTime.Now; 
            ImageClass.Negative(img); // nosso procedimento
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        /// <summary>
        /// Call automated image processing check
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void evalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EvalForm eval = new EvalForm();
            eval.ShowDialog();
        }

        /// <summary>
        /// Call image convertion to gray scale
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void grayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            ImageClass.ConvertToGray(img);

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        private void greenToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            DateTime t_init = DateTime.Now;
            ImageClass.GreenChannel(img); // nosso procedimento
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        private void blueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            DateTime t_init = DateTime.Now;
            ImageClass.BlueChannel(img); // nosso procedimento
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        private void redToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();

            DateTime t_init = DateTime.Now;
            ImageClass.RedChannel(img); // nosso procedimento
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        private void isolateColourToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void contrastAndBrightnessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 
            //copy Undo Image
            imgUndo = img.Copy();

            InputBox form = new InputBox("brilho?"); 
            form.ShowDialog();
            int brightness = Convert.ToInt32(form.ValueTextBox.Text);
            if (brightness > 255 || brightness < 0 )
                return;

            form = new InputBox("contraste?");
            form.ShowDialog();
            double contrast = Convert.ToDouble(form.ValueTextBox.Text);
            if (contrast > 3.0 || contrast < 0.0)
                return;

            DateTime t_init = DateTime.Now;
            ImageClass.BrightContrast( img, brightness, contrast ); // nosso procedimento
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 

        }

        private void translationToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();

            InputBox form = new InputBox("X translation?");
            form.ShowDialog();
            int tx = (int)Math.Round(Convert.ToDouble(form.ValueTextBox.Text));

            form = new InputBox("Y translation?");
            form.ShowDialog();
            int ty = (int)Math.Round(Convert.ToDouble(form.ValueTextBox.Text));
           

            DateTime t_init = DateTime.Now;
            ImageClass.Translation(img, imgCopy, tx, ty); // nosso procedimento
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        private void rotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();

            InputBox form = new InputBox("Angle?");
            form.ShowDialog();
            double angle = Math.Round(Convert.ToDouble(form.ValueTextBox.Text));
            angle = (angle * Math.PI) / 180;
            DateTime t_init = DateTime.Now;
            ImageClass.Rotation(img, imgCopy, (float)angle); // nosso procedimento
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        // mouse vars
        int mouseX, mouseY;
        bool mouseFlag = false;
        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();

            InputBox form = new InputBox("Zoom?");
            form.ShowDialog();
            double zoom = Convert.ToDouble(form.ValueTextBox.Text);
            if(zoom > 100.0 || zoom <= 0.0)
                return;

            // ativar sniffing do mouse click - vamos esperar por um click no rate
            mouseFlag = true;
            while (mouseFlag)
                Application.DoEvents(); // esperar pelo evento do click do rato


            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Scale_point_xy(img, imgCopy, (float)zoom, mouseX, mouseY); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }


        private void meanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();


            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Mean(img, imgCopy); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void nonUniformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            
            FiltersForm ff = new FiltersForm();
            if (ff.ShowDialog() != DialogResult.OK) return;

            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.NonUniform(img, imgCopy, ff.getMatrix(), ff.getMatrixWeight() ); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 
           
            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

        }

        private void sobelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();

            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Sobel(img, imgCopy); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void differentiationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();

            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Diferentiation(img, imgCopy); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void robertsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();

            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Roberts(img, imgCopy); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 
            HistogramForm hist = new HistogramForm(ImageClass.Histogram_All(img));
            hist.ShowDialog();
        }

        private void histogramEqualizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Ycc, byte> imgCopy = img.Convert<Ycc,byte>();

            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Equalization(img); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = imgCopy.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void verticalProjectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 
            VerticalProjection hp = new VerticalProjection(ImageClass.VerticalProjection(img));
            hp.ShowDialog();
        }

        private void horizontalProjectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 
            int[] black_list = ImageClass.HorizontalProjection(img);
            HorizontalProjection hp = new HorizontalProjection(black_list);
            hp.ShowDialog();
            
        }

        private void otsuBinarizationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
           

            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.ConvertToBW_Otsu(img); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            

            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void rotationBilinearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();

            InputBox form = new InputBox("Angle?");
            form.ShowDialog();
            double angle = Math.Round(Convert.ToDouble(form.ValueTextBox.Text));
            angle = (angle * Math.PI) / 180;
            DateTime t_init = DateTime.Now;
            ImageClass.Rotation_Bilinear(img, imgCopy, (float)angle); // nosso procedimento
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        private void scaleBilinearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();

            InputBox form = new InputBox("Zoom?");
            form.ShowDialog();
            double zoom = Convert.ToDouble(form.ValueTextBox.Text);
            if (zoom > 100.0 || zoom <= 0.0)
                return;

            // ativar sniffing do mouse click - vamos esperar por um click no rate
            mouseFlag = true;
            while (mouseFlag)
                Application.DoEvents(); // esperar pelo evento do click do rato


            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Scale_point_xy_Bilinear(img, imgCopy, (float)zoom, mouseX, mouseY); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen

            Cursor = Cursors.Default; // normal cursor 
        }

        private void dilationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            // criar mascara em cruz invertida 
            int[,] mask = new int[7, 7];
            int i, j;
            for (i = 0; i < mask.GetLength(0); i++)
            {
                for (j = 0; j < mask.GetLength(1); j++)
                {
                    mask[i, j] = 1;
                }
            }
            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Dilation(img, imgCopy, mask); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void erosionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            // criar mascara em cruz
            int[,] mask = new int[3, 3] { {0,1,0},
                                          {0,1,0},
                                          {0,1,0} };
            
            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Erosion(img, imgCopy, mask); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void angleFinderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.angleFinder(img, imgCopy); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void decoderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 
            int[] centroid = new int[2];
            int Cx, Cy;
            float zoom;
            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            DateTime t_init = DateTime.Now;
            ImageClass.angleFinder(img, imgCopy);
            centroid = ImageClass.Centroid(img);
            Cx = centroid[0];
            Cy = centroid[1];
            zoom = (float)1.6;
            ImageClass.Scale_point_xy_Bilinear(img,imgCopy, zoom, Cx, Cy);
            //ImageClass.BCDecoder(ImageClass.HorizontalProjection(img)); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void verticalMeanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            int[] vert_proj = ImageClass.VerticalProjection(img); 
            int[] filt_vert_proj = ImageClass.MeanFilter(vert_proj, 20);
            VerticalProjection hp = new VerticalProjection(vert_proj);
            hp.ShowDialog();
            VerticalProjection hp2 = new VerticalProjection(filt_vert_proj);
            hp2.ShowDialog();
            //TimeSpan t_final = DateTime.Now - t_init;
            //MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void horizontalMeanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            int[] horz_proj = ImageClass.HorizontalProjection(img);
            int[] filt_horz_proj = ImageClass.MeanFilter(horz_proj, 20);
            HorizontalProjection hp = new HorizontalProjection(horz_proj);
            hp.ShowDialog();
            HorizontalProjection hp2 = new HorizontalProjection(filt_horz_proj);
            hp2.ShowDialog();
            //TimeSpan t_final = DateTime.Now - t_init;
            //MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void verticalMatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            int[] vert_proj = ImageClass.VerticalProjection(img);
            int[] filt_vert_proj = ImageClass.MeanFilter(vert_proj, 20);
            filt_vert_proj = ImageClass.MatchFilter(filt_vert_proj, 20, 0.5);
            VerticalProjection hp = new VerticalProjection(vert_proj);
            hp.ShowDialog();
            VerticalProjection hp2 = new VerticalProjection(filt_vert_proj);
            hp2.ShowDialog();
            //TimeSpan t_final = DateTime.Now - t_init;
            //MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void horizontalMatchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            int[] horz_proj = ImageClass.HorizontalProjection(img);
            int[] filt_horz_proj = ImageClass.MeanFilter(horz_proj, 20);
            filt_horz_proj = ImageClass.MatchFilter(filt_horz_proj, 20, 0.1);
            filt_horz_proj = ImageClass.MeanFilter(filt_horz_proj, 20);
            HorizontalProjection hp = new HorizontalProjection(horz_proj);
            hp.ShowDialog();
            HorizontalProjection hp2 = new HorizontalProjection(filt_horz_proj);
            hp2.ShowDialog();

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void identifyCBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            ImageClass.IdentifyCB(img,imgCopy);
            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void whiteTopHatTransformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            // criar mascara em cruz
            int[,] mask = new int[5, 5];
            int i, j;
            for (i = 0; i < mask.GetLength(0); i++)
            {
                for (j = 0; j < mask.GetLength(1); j++)
                {
                    mask[i, j] = 0;
                }
            }
            for (i = 0; i < mask.GetLength(0); i++)
            {
                mask[i, (int)(mask.GetLength(1) / 2)] = 1;
            }
            for (j = 0; j < mask.GetLength(1); j++)
            {
                mask[(int)(mask.GetLength(0) / 2), j] = 1;
            }
            //for (int i = 0; i < mask.GetLength(0); i++)
            //{
            //    for (int j = 0; j < mask.GetLength(1); j++)
            //    {
            //        if()
            //        mask[i, j] = 0;
            //    }
            //}
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            ImageClass.WhiteHat(img, mask);
            
            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            imgUndo = img.Copy();
            // criar mascara em cruz
            int[,] mask = new int[11, 11];
            int i, j;
            for (i = 0; i < mask.GetLength(0); i++)
            {
                for (j = 0; j < mask.GetLength(1); j++)
                {
                        mask[i, j] = 0;
                }
            }
            for (i = 0; i < mask.GetLength(0); i++)
            {
                mask[i, (int)(mask.GetLength(1) / 2)] = 1;
            }
            for (j = 0; j < mask.GetLength(1); j++)
            {
                mask[(int)(mask.GetLength(0) / 2), j] = 1;
            }
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            ImageClass.Open(img, imgCopy, mask, 10);
            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            imgUndo = img.Copy();
            // criar mascara em cruz
            int[,] mask = new int[11, 11];
            int i, j;
            for (i = 0; i < mask.GetLength(0); i++)
            {
                for (j = 0; j < mask.GetLength(1); j++)
                {
                    mask[i, j] = 1;
                }
            }
            for (i = 0; i < mask.GetLength(0); i++)
            {
                mask[i, (int)(mask.GetLength(1) / 2)] = 1;
            }
            for (j = 0; j < mask.GetLength(1); j++)
            {
                mask[(int)(mask.GetLength(0) / 2), j] = 1;
            }
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            ImageClass.Open(img, imgCopy, mask, 20);
            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            imgUndo = img.Copy();
            // criar mascara em cruz
            int[,] mask = new int[11, 11];
            int i, j;
            for (i = 0; i < mask.GetLength(0); i++)
            {
                for (j = 0; j < mask.GetLength(1); j++)
                {
                    mask[i, j] = 0;
                }
            }
            for (i = 0; i < mask.GetLength(0); i++)
            {
                mask[i, (int)(mask.GetLength(1) / 2)] = 1;
            }
            for (j = 0; j < mask.GetLength(1); j++)
            {
                mask[(int)(mask.GetLength(0) / 2), j] = 1;
            }
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            ImageClass.Open(img, imgCopy, mask, 5);
            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void blackHatTransformToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imgUndo = img.Copy();
            // criar mascara em cruz
            int[,] maskBlackHat = new int[11, 11];
            int i, j;
            for (i = 0; i < maskBlackHat.GetLength(0); i++)
            {
                for (j = 0; j < maskBlackHat.GetLength(1); j++)
                {
                    maskBlackHat[i, j] = 0;
                }
            }
            for (i = 0; i < maskBlackHat.GetLength(0); i++)
            {
                maskBlackHat[i, (int)(maskBlackHat.GetLength(1) / 2)] = 1;
            }
            for (j = 0; j < maskBlackHat.GetLength(1); j++)
            {
                maskBlackHat[(int)(maskBlackHat.GetLength(0) / 2), j] = 1;
            }
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateTime.Now;
            ImageClass.BlackHat(img, maskBlackHat);
            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void standardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imgUndo = img.Copy();
            int[,] mask = new int [30, 30];
            for(int i = 0; i < 30; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    mask[i, j] = 1;
                }
            }
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateT,ime.Now;
            ImageClass.Open(img, imgCopy, mask, 1);
            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            imgUndo = img.Copy();
            int[,] mask = new int[15, 15];
            for (int i = 0; i < mask.GetLength(0); i++)
            {
                for (int j = 0; j < mask.GetLength(1); j++)
                {
                    mask[i, j] = 1;
                }
            }
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateT,ime.Now;
            ImageClass.Close(img, imgCopy, mask, 1);
            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
        }


        private void segmentationV8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imgUndo = img.Copy();
            //criar copia local da imagem
            int[,] imgArray = new int[img.Height, img.Width];
            Image<Bgr, byte> imgCopy = img.Copy();
            //DateTime t_init = DateT,ime.Now;
            imgArray = ImageClass.IterativeSegmentation(img, imgCopy);
            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
        }

        private void vertLinePreservationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 

            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            // criar mascara em cruz
            int[,] mask = new int[3, 3];
            for (int i = 0; i < mask.GetLength(0); i++)
            {
                mask[i, mask.GetLength(1) - 1] = 1;
                mask[i, mask.GetLength(1) - 2] = 1;
            }

            DateTime t_init = DateTime.Now;
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Erosion(img, imgCopy, mask); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void horzLinePreservationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 
            DateTime t_init = DateTime.Now;
            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            // criar mascara em cruz
            int[,] mask = new int[45, 45];
            for (int i = 0; i < mask.GetLength(1); i++)
            {
                mask[mask.GetLength(0) - 1, i] = 1;
                mask[mask.GetLength(0) - 2, i] = 1;
            }

            
            //ImageClass.Scale(img, imgCopy, zoom); // nosso procedimento sem click do rato
            ImageClass.Erosion(img, imgCopy, mask); // procedimento com click de rato
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void imageG3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 
            
            DateTime t_init = DateTime.Now;
            Image<Bgr, byte> imgCopy = img.Copy();
            
            ImageClass.ConvertToBW_Otsu(img);
            imgCopy = img.Copy();

            int[,] maskBlackHat = new int[15, 15];

            for (int i = 0; i < maskBlackHat.GetLength(0); i++)
            {
                for (int j = 0; j < maskBlackHat.GetLength(1); j++)
                {
                    maskBlackHat[i, j] = 0;
                }
            }
            for (int i = 0; i < maskBlackHat.GetLength(0); i++)
            {
                maskBlackHat[i, (int)(maskBlackHat.GetLength(1) / 2)] = 1;
            }
            for (int j = 0; j < maskBlackHat.GetLength(1); j++)
            {
                maskBlackHat[(int)(maskBlackHat.GetLength(0) / 2), j] = 1;
            }

            //aplicar transformada blackhat
            ImageClass.BlackHat(img, maskBlackHat);

            //passar a negativo e destacar linhas verticais
            ImageClass.Negative(img);

            int[,] mask = new int[45, 45];
            for (int i = 0; i < mask.GetLength(0); i++)
            {
                mask[i, mask.GetLength(1) - 1] = 1;
                mask[i, mask.GetLength(1) - 2] = 1;
            }

            imgCopy = img.Copy();
            ImageClass.Erosion(img, imgCopy, mask);

            // colar horizontalmente as linhas verticais
            int[,] mask2 = new int[35, 35];
            for (int i = 0; i < mask2.GetLength(1); i++)
            {
                mask2[mask2.GetLength(0) - 1, i] = 1;
                mask2[mask2.GetLength(0) - 2, i] = 1;
            }

            //ImageClass.Negative(img);
            imgCopy = img.Copy();
            //ImageClass.Erosion(img, imgCopy, mask2);
            //ImageClass.Negative(img);

            // fechar o objeto para criar o bloco maciço
            int[,] mask3 = new int[15, 15];

            for (int i = 0; i < mask3.GetLength(0); i++)
            {
                for (int j = 0; j < mask3.GetLength(1); j++)
                {
                    mask3[i, j] = 1;
                }
            }

            imgCopy = img.Copy();
            //ImageClass.Close(img, imgCopy, mask3, 4);


            //detetar o angulo para o qual temos de rodar a imagem
            imgCopy = img.Copy();
            double angle_momentum = ImageClass.angleFinder(img, imgCopy);
            double angle_correction = 0.02;
            if (angle_momentum < 0.03 && angle_momentum > -0.03)
                angle_momentum = 0.0;
            //if (angle_momentum != 0.0)
            //    angle_momentum = angle_momentum > 0.0 ? angle_momentum + angle_correction : angle_momentum - angle_correction;


            ImageClass.Rotation_Bilinear(img, imgCopy, (float)angle_momentum);
            // obter o centroide e as dimensoes apos rodar o bloco
            int[] vert_proj = ImageClass.VerticalProjection(img);
            int[] filt_vert_proj = ImageClass.MeanFilter(vert_proj, 20);
            filt_vert_proj = ImageClass.MatchFilter(filt_vert_proj, 20, 0.5);

            int[] horz_proj = ImageClass.HorizontalProjection(img);
            int[] filt_horz_proj = ImageClass.MeanFilter(horz_proj, 20);
            filt_horz_proj = ImageClass.MatchFilter(filt_horz_proj, 20, 0.5);

            int [] size = ImageClass.BC_size(horz_proj, vert_proj);
            int [] centroid = ImageClass.Centroid(img);

            // aplicar rotação à imagem principal

            imgCopy = img.Copy();
            //Rotation_Bilinear_xy_point(img, imgCopy, (float)angle_momentum, centroid[0], centroid[1]);
            //ImageClass.Rotation_Bilinear(img, imgCopy, (float)angle_momentum);
            
            imgCopy = img.Copy();

            // converter a imagem original de novo para binario apos destacar apenas uma faixa horizontal de pixeis em torno do centroide
            ImageClass.ConvertToBW_Otsu(img);
            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void isolateNumbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (img == null) // verify if the image is already opened
                return;
            Cursor = Cursors.WaitCursor; // clock cursor 
            DateTime t_init = DateTime.Now;
            //copy Undo Image
            imgUndo = img.Copy();
            //criar copia local da imagem
            Image<Bgr, byte> imgCopy = img.Copy();
            int[] horizontalProj;
            // operações morfológicas
            ImageClass.ConvertToBW_Otsu(img);
         

            int[] vert_proj = ImageClass.VerticalProjection(img);
            int[] filt_vert_proj = ImageClass.MeanFilter(vert_proj, 20);
            filt_vert_proj = ImageClass.MatchFilter(filt_vert_proj, 20, 0.5);

            int[] horz_proj = ImageClass.HorizontalProjection(img);
            int[] filt_horz_proj = ImageClass.MeanFilter(horz_proj, 20);
            filt_horz_proj = ImageClass.MatchFilter(filt_horz_proj, 20, 0.5);

            int[] centroid = ImageClass.Centroid(img);

            //ImageClass.IsolateNumbersBC(img, imgCopy, centroid[1], vert_proj);
            //horizontalProj = ImageClass.IsolateNumbersBC(img, imgCopy, centroid[1], vert_proj);
            //HorizontalProjection hp = new HorizontalProjection(horizontalProj);
            //hp.ShowDialog();

            TimeSpan t_final = DateTime.Now - t_init;
            MessageBox.Show(t_final.ToString()); // display time consumption of the algorithm

            Cursor = Cursors.Default; // normal cursor 

            ImageViewer.Image = img.Bitmap;
            ImageViewer.Refresh(); // refresh image on the screen
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void imageG4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void ImageViewer_MouseClick_1(object sender, MouseEventArgs e)
        {
            if (mouseFlag)
            {
                mouseX = e.X;
                mouseY = e.Y;
                mouseFlag = false; // unlock while loop in Zoom Tool Strip ^^
            }
        }

    }




}