using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Configuration;
using System.Data;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SS_OpenCV
{
    class ImageClass
    {

        private static char[,] first_6digit_group_code = new char[10, 6] {  {'L','L','L','L','L','L'} ,
                                                                            {'L','L','G','L','G','G'} ,
                                                                            {'L','L','G','G','L','G'} ,
                                                                            {'L','L','G','G','G','L'} ,
                                                                            {'L','G','L','L','G','G'} ,
                                                                            {'L','G','G','L','L','G'} ,
                                                                            {'L','G','G','G','L','L'} ,
                                                                            {'L','G','L','G','L','G'} ,
                                                                            {'L','G','L','G','G','L'} ,
                                                                            {'L','G','G','L','G','L'} 
                                                                         };
        
        private static int [,] digit_coding_R = new int[10, 7]       { 
                                                                      { 1,1,1,0,0,1,0 },
                                                                      { 1,1,0,0,1,1,0 },
                                                                      { 1,1,0,1,1,0,0 },
                                                                      { 1,0,0,0,0,1,0 },
                                                                      { 1,0,1,1,1,0,0 },
                                                                      { 1,0,0,1,1,1,0 },
                                                                      { 1,0,1,0,0,0,0 },
                                                                      { 1,0,0,0,1,0,0 },
                                                                      { 1,0,0,1,0,0,0 },
                                                                      { 1,1,1,0,1,0,0 } 
                                                                    };
        /// <summary>
        /// RightShift
        /// </summary> faz um shift para a direita com inserção de um '1' na esquerda nos codigos R
        /// <param name="matrix"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private static int[] RightShift(int[] array)
        {
            int[] ret_row = new int[array.Length];
            for (int k = 0; k < array.Length-1; k++)
            {
                ret_row[k+1] = array[k];
            }
            ret_row[0] = 1;
            return ret_row;
        }
        // GetRow permite obter uma linha inteira de uma 
        private static int[] GetRow(int [,] matrix, int row)
        {
            int[] ret_row = new int[matrix.GetLength(1)];
            for (int k = 0; k < matrix.GetLength(1); k++)
            {
                ret_row[k] = matrix[row, k];
            }
            return ret_row;
        }

        // GetString permite obter uma linha inteira de uma 
        private static char[] GetString(char[,] matrix, int row)
        {
            char[] ret_row = new char[matrix.GetLength(1)];
            for (int k = 0; k < matrix.GetLength(1); k++)
            {
                ret_row[k] = matrix[row, k];
            }
            return ret_row;
        }
        // ArrayEqual
        private static int ArrayEqual(int[] arr1, int[] arr2)
        {
            //bool result = false;
            int result = -1;
            if (arr1.Length == arr2.Length) // se tiverem as mesmas dimensoes
            {
                result = 0;
                for(int n = 0; n < arr1.Length; n++)
                {
                    result = arr2[n] == arr1[n] ? result + 1 : result ; // incrementar resultado caso as casas contenham a mesma informação
                    //if (!result) // se for detetado um falso, sair
                    //{
                    //    break;
                    //}
                }
            }
            return result;
        }
        // ArrayEqual
        private static bool StringEqual(char[] arr1, char[] arr2)
        {
            bool result = false;
            if (arr1.Length == arr2.Length) // se tiverem as mesmas dimensoes
            {
                result = true;
                for (int n = 0; n < arr1.Length; n++)
                {
                    result = arr2[n] == arr1[n];
                    if (!result) // se for detetado um falso, sair
                    {
                        break;
                    }
                }
            }
            return result;
        }
        //O codigo L do digito pode ser obtido invertendo os bits do codigo R
        private static int[] LcodeFromRcode(int [] digit_coding_R)
        {
            int[] digit_coding_L = new int[digit_coding_R.Length];
            for (int k = 0; k < digit_coding_R.Length; k++)
            {
                digit_coding_L[k] = digit_coding_R[k] == 1 ? 0 : 1;
            }
            return digit_coding_L;
        }
        //O codigo G do digito pode ser obtido invertendo a ordem dos bits do codigo R
        private static int[] GcodeFromRcode(int[] digit_coding_R)
        {
            int len = digit_coding_R.Length;
            int[] digit_coding_G = new int[len];
            for (int k = 0; k < len; k++)
            {
                digit_coding_G[len-(k+1)] = digit_coding_R[k];
            }
            return digit_coding_G;
        }


        // ACRESCENTADO NO DIA 19/09/2020, 20:00 H
        // POR : DIOGO DIAS
        // ULTIMA ALTERAÇÃO : 22/09/2020, 12:00 H

        /// <summary>
        /// Image Negative using direct access to memmory
        /// faster method to get the negative equivalent of an image
        /// </summary>
        /// <param name="img">Image</param>
        public static void Negative(Image<Bgr, byte> img)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte blue, green, red;

                //comprimento da imagem (em bytes)
                int width = img.Width;
                //largura da imagem (em bytes)
                int height = img.Height;
                //numero de canais de byte maps para cada pixel - se for 3. entao a imagem necessita do controlo dos fotodiodos Red, Blue, Green para cada pixel
                int nChan = m.nChannels; // number of channels - 3
                //padding - numero de pixeis "dummy" acrescentados para que a imagem seja mapeada por um numero de bytes
                // que seja multiplo de 4 ou 8 ! - otimização de tempo de execução no processamento da imagem
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)

                //DUVIDA : O QUE É O WIDTHSTEP ? 
                int x, y;
                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais rapido
                            red = dataPtr[2]; // R
                            green = dataPtr[1]; // G
                            blue = dataPtr[0]; // B

                            //Para meter a imagem em negativo é necessario inverter a sua cor, pelo que 
                            //subtraimos a intensidade de cada pigmento base (RGB) ao valor de intensidade maximo - 255 (1 byte de codificação)
                            //Nota: valor de intensidade minimo : 0 
                            dataPtr[2] = (byte)(255 - red);
                            dataPtr[1] = (byte)(255 - green);
                            dataPtr[0] = (byte)(255 - blue);

                            dataPtr += nChan; // avançar o apontador para o proximo pixel

                        }

                        dataPtr += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }

        }

        /// <summary>
        /// Convert to gray
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void ConvertToGray(Image<Bgr, byte> img)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte blue, green, red, gray;

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                int x, y;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrive 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            // convert to gray
                            gray = (byte)Math.Round(((int)blue + green + red) / 3.0);

                            // store in the image
                            dataPtr[0] = gray;
                            dataPtr[1] = gray;
                            dataPtr[2] = gray;

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        /// <summary>
        /// Isolate Colour - isolates a colour channel of the image
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="color_index"> clour_index of the colour to isolate</param>
        public static void IsolateColour(Image<Bgr, byte> img, byte color_index)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte chosen_color_brightness;

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                int x, y, k;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrive the colour component to isolate
                            chosen_color_brightness = dataPtr[color_index];

                            //store the brightness values in the pixel
                            k = 0;
                            while (k < nChan)
                            {
                                if (k != color_index)
                                {
                                    //set the brightness of the chosen color to isolate
                                    // in every other colour channels of the same pixel
                                    dataPtr[k] = chosen_color_brightness;

                                    // dataPtr[k] = 0;

                                    //this method produces a gray scale version of an image
                                    // in which a colour was isolated, making filtering actions 
                                    // less time consuming and less complex!

                                }
                                k++;
                            }

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        /// <summary>
        /// RedChannel - isolates the red channel of the image
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void RedChannel(Image<Bgr, byte> img)
        {
            IsolateColour(img, 2);
        }

        /// <summary>
        /// GreenChannel - isolates the green channel of the image
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void GreenChannel(Image<Bgr, byte> img)
        {
            IsolateColour(img, 1);
        }

        /// <summary>
        /// BlueChannel - isolates the blue channel of the image
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>

        public static void BlueChannel(Image<Bgr, byte> img)
        {
            IsolateColour(img, 0);
        }

        /// <summary>
        /// BrightContrast - adjusts the level of brightness and contrast of the image
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="color_index"> clour_index of the colour to isolate</param>
        public static void BrightContrast(Image<Bgr, byte> img, int bright, double contrast)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image

                //comprimento da imagem (em bytes)
                int width = img.Width;
                //largura da imagem (em bytes)
                int height = img.Height;
                //numero de canais de byte maps para cada pixel - se for 3. entao a imagem necessita do controlo dos fotodiodos Red, Blue, Green para cada pixel
                int nChan = m.nChannels; // number of channels - 3
                //padding - numero de pixeis "dummy" acrescentados para que a imagem seja mapeada por um numero de bytes
                // que seja multiplo de 4 ou 8 ! - otimização de tempo de execução no processamento da imagem
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                //int k;
                int x, y;
                double aux_colour;

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 

                            //change the pixel
                            aux_colour = dataPtr[0];
                            aux_colour = contrast * aux_colour + bright;
                            /*if (aux_colour < 0)
                                aux_colour = 0;*/
                            aux_colour = aux_colour < 0 ? 0 : aux_colour;
                            /*if (aux_colour > 255)
                                aux_colour = 255;*/
                            aux_colour = aux_colour > 255 ? 255 : aux_colour;

                            dataPtr[0] = (byte)Math.Round(aux_colour);

                            aux_colour = dataPtr[1];
                            aux_colour = contrast * aux_colour + bright;
                            /*if (aux_colour < 0)
                                aux_colour = 0;*/
                            aux_colour = aux_colour < 0 ? 0 : aux_colour;
                            /*if (aux_colour > 255)
                                aux_colour = 255;*/
                            aux_colour = aux_colour > 255 ? 255 : aux_colour;

                            dataPtr[1] = (byte)Math.Round(aux_colour);

                            aux_colour = dataPtr[2];
                            aux_colour = contrast * aux_colour + bright;
                            /*if (aux_colour < 0)
                                aux_colour = 0;*/
                            aux_colour = aux_colour < 0 ? 0 : aux_colour;
                            /*if (aux_colour > 255)
                                aux_colour = 255;*/
                            aux_colour = aux_colour > 255 ? 255 : aux_colour;

                            dataPtr[2] = (byte)Math.Round(aux_colour);


                            dataPtr += nChan; // avançar o apontador para o proximo pixel

                        }

                        dataPtr += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }
        }

        ////////////////////////// GEOMETRIC OPERATIONS ////////////////////////////////
        ///
        /// <summary>
        /// Translaction - applies an horizontal and vertical translaction to the image according to input
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="color_index"> clour_index of the colour to isolate</param>
        public static void Translation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, int dx, int dy)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_origem = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte* dataPtr_destino = (byte*)m_cpy.imageData.ToPointer();
                //comprimento da imagem (em bytes)

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y, x_origem, y_origem;
                byte* aux_ptr;

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 
                            //POSIÇÃO DE X destino EM FUNÇÃO DO X origem - ENDEREÇAMENTO ABSOLUTO
                            //NOTA : x = x_destino/ y = y_destino
                            y_origem = y - dy;
                            x_origem = x - dx;
                            //se a posição estiver sobre o indice a sguir aos pixeis de borda, nao os devemos processar, pois estamos fora da imagem! (>=)
                            if (y_origem >= height || x_origem >= width || y_origem < 0 || x_origem < 0)
                            {
                                dataPtr_origem[0] = 0;
                                dataPtr_origem[1] = 0;
                                dataPtr_origem[2] = 0;
                            }
                            else
                            {
                                aux_ptr = (dataPtr_destino + (y_origem) * (widthstep) + (x_origem) * nChan);
                                //blue
                                dataPtr_origem[0] = aux_ptr[0];
                                //green
                                dataPtr_origem[1] = aux_ptr[1];
                                //red
                                dataPtr_origem[2] = aux_ptr[2];
                            }

                            dataPtr_origem += nChan; // avançar o apontador para o proximo pixel na imagem de origem, para saber que info tenho de copiar dessa posição para a imagem destino, segundo a translacao
                            //x_origem = x_destino - tx 
                        }

                        dataPtr_origem += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }
        }
        public static byte[] Bilinear(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, double x, double y)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_origem = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte* dataPtr_destino = (byte*)m_cpy.imageData.ToPointer();

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width

                double delta_x, delta_y;
                byte bx1, bx2, x1, x2;
                byte* aux_ptr;
                byte[] intensities = new byte[3];

                delta_x = x - (int)(x);
                delta_y = y - (int)(y);

                for (int i = 0; i < 3; i++)
                {
                    aux_ptr = dataPtr_destino + (int)y * widthstep + (int)x * nChan;
                    x1 = aux_ptr[i];
                    aux_ptr = dataPtr_destino + (int)y * widthstep + ((int)x + 1) * nChan;
                    x2 = aux_ptr[i];

                    bx1 = (byte)(x1 + (x2 - x1) * delta_x);

                    aux_ptr = dataPtr_destino + ((int)y + 1) * widthstep + (int)x * nChan;
                    x1 = aux_ptr[i];
                    aux_ptr = dataPtr_destino + ((int)y + 1) * widthstep + ((int)x + 1) * nChan;
                    x2 = aux_ptr[i];

                    bx2 = (byte)(x1 + (x2 - x1) * delta_y);

                    //interpolação em y 
                    intensities[i] = (byte)(bx1 + (bx2 - bx1) * delta_y);
                }

                if (y >= height || x >= width || y < 0 || x < 0)
                {
                    intensities[0] = 0;
                    intensities[1] = 0;
                    intensities[2] = 0;
                }

                return intensities;
            }
        }
        /// <summary>
        /// Rotation - applies an inverse rotation to the image according to input angle
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="color_index"> clour_index of the colour to isolate</param>
        public static void Rotation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float angle)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_origem = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte* dataPtr_destino = (byte*)m_cpy.imageData.ToPointer();
                //comprimento da imagem (em bytes)

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                int x_origem, y_origem;
                double sen_angle = Math.Sin(angle);
                double cos_angle = Math.Cos(angle);
                double x_centro = (width / 2.0);
                double y_centro = (height / 2.0);

                byte* aux_ptr;

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 
                            //POSIÇÃO DE X destino EM FUNÇÃO DO X origem - ENDEREÇAMENTO ABSOLUTO
                            //NOTA: y = y_destino
                            //NOTA: x = x_destino
                            y_origem = (int)Math.Round(y_centro - (x - x_centro) * sen_angle - (y_centro - y) * cos_angle);
                            x_origem = (int)Math.Round(x_centro + (x - x_centro) * cos_angle - (y_centro - y) * sen_angle);
                            //se a posição estiver sobre o indice a seguir aos pixeis de borda, nao os devemos processar, pois estamos fora da imagem! (>=)
                            if (y_origem >= height || x_origem >= width || y_origem < 0 || x_origem < 0)
                            {
                                dataPtr_origem[0] = 0;
                                dataPtr_origem[1] = 0;
                                dataPtr_origem[2] = 0;
                            }
                            else
                            {
                                aux_ptr = dataPtr_destino + y_origem * widthstep + x_origem * nChan;
                                //blue
                                dataPtr_origem[0] = aux_ptr[0];
                                //green
                                dataPtr_origem[1] = aux_ptr[1];
                                //red
                                dataPtr_origem[2] = aux_ptr[2];
                            }

                            dataPtr_origem += nChan; // avançar o apontador para o proximo pixel na imagem de origem, para saber que info tenho de copiar dessa posição para a imagem destino, segundo a translacao
                            //x_origem = x_destino - tx 
                        }

                        dataPtr_origem += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }
        }

        /// <summary>
        /// Rotation - applies an inverse rotation to the image according to input angle
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="color_index"> clour_index of the colour to isolate</param>
        public static void Rotation_Bilinear(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float angle)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_destino = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer();

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                double x_origem, y_origem;
                double sen_angle = Math.Sin(angle);
                double cos_angle = Math.Cos(angle);
                double x_centro = (width / 2.0);
                double y_centro = (height / 2.0);
                byte[] result = new byte[3];
                byte* aux_ptr;
                double delta_x, delta_y;
                double bx1r,bx1b,bx1g, bx2r,bx2b,bx2g, x1r,x1b,x1g, x2r,x2b,x2g;
                bx1r = bx1b = bx1g = bx2r = bx2b = bx2g = x1r = x1b = x1g = x2r = x2b = x2g = 0.0;
                byte[] intensities = new byte[3];


                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 
                            //POSIÇÃO DE X destino EM FUNÇÃO DO X origem - ENDEREÇAMENTO ABSOLUTO
                            //NOTA: y = y_destino
                            //NOTA: x = x_destino
                            y_origem = y_centro - ((double)x - x_centro) * sen_angle - (y_centro - (double)y) * cos_angle;
                            x_origem = x_centro + ((double)x - x_centro) * cos_angle - (y_centro - (double)y) * sen_angle;

                            //result = Bilinear(img, imgCopy, x_origem, y_origem);
                            delta_x = x_origem - Math.Floor(x_origem);
                            delta_y = y_origem - Math.Floor(y_origem);

                            if (Math.Ceiling(y_origem) >= height || Math.Ceiling(x_origem) >= width || Math.Floor(y_origem) < 0 || Math.Floor(x_origem) < 0)
                            {
                                dataPtr_destino[0] = 255;
                                dataPtr_destino[1] = 255;
                                dataPtr_destino[2] = 255;
                            }

                            else
                            {
                                // percorrer os canales de cor

                                aux_ptr = dataPtr_origem + (int)Math.Floor(y_origem) * widthstep + (int)Math.Floor(x_origem) * nChan;
                                x1b = (double)aux_ptr[0];
                                x1g = (double)aux_ptr[1];
                                x1r = (double)aux_ptr[2];
                                if (Math.Ceiling(x_origem) < width) // se o pixel seguinte da origem estiver dentro da imagem
                                {
                                    aux_ptr = dataPtr_origem + (int)Math.Floor(y_origem) * widthstep + (int)Math.Ceiling(x_origem) * nChan;
                                    x2b = (double)aux_ptr[0];
                                    x2g = (double)aux_ptr[1];
                                    x2r = (double)aux_ptr[2];

                                }
                                else // se o pixel seguinte nao estiver dentro da imagem
                                {
                                    x2b = x1b;
                                    x2g = x1g;
                                    x2r = x1r; // os pixeis de interpolação sao iguais
                                }
                                bx1b = x1b + (x2b - x1b) * delta_x;
                                bx1g = x1g + (x2g - x1g) * delta_x;
                                bx1r = x1r + (x2r - x1r) * delta_x;

                                // interpolação em Bx2
                                if ((int)Math.Ceiling(y_origem) < height && (int)Math.Ceiling(x_origem) < width)
                                {
                                    aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + (int)Math.Floor(x_origem) * nChan;
                                    x1b = (double)aux_ptr[0];
                                    x1g = (double)aux_ptr[1];
                                    x1r = (double)aux_ptr[2];
                                    aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                    x2b = (double)aux_ptr[0];
                                    x2g = (double)aux_ptr[1];
                                    x2r = (double)aux_ptr[2];
                                }
                                else
                                {
                                    if ((int)Math.Ceiling(y_origem) > height - 1) // se o y_origem + 1 estiver fora dos limites da imagem
                                    {
                                        // os pixeis sao duplicados, e por isso y(k-1) = y(k)
                                        aux_ptr = dataPtr_origem + ((int)Math.Floor(y_origem)) * widthstep + (int)Math.Floor(x_origem) * nChan;
                                        x1b = (double)aux_ptr[0];
                                        x1g = (double)aux_ptr[1];
                                        x1r = (double)aux_ptr[2];
                                        // se o pixel estiver em colunas fora da imagem
                                        if ((int)Math.Ceiling(x_origem) > width - 1)
                                        {
                                            // a instruçao : aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                            // e substituida por : aux_ptr = dataPtr_origem + ((int)Math.Floor(y_origem)) * widthstep + ((int)x_origem) * nChan;
                                            //x2 = (double)aux_ptr[i];
                                            x2b = x1b;
                                            x2g = x1g;
                                            x2r = x1r; // os pixeis de interpolação sao iguais
                                        }
                                        else // o x origem + 1 esta dentro dos limites da imagem
                                        {
                                            aux_ptr = dataPtr_origem + ((int)Math.Floor(y_origem)) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                            x2b = (double)aux_ptr[0];
                                            x2g = (double)aux_ptr[1];
                                            x2r = (double)aux_ptr[2];
                                        }

                                    }
                                    else // o y_origem + 1 esta dentro dos limites da imagem - entao o x_origem esta fora dos limites da imagem
                                    {
                                        aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + (int)Math.Floor(x_origem) * nChan;
                                        x1b = (double)aux_ptr[0];
                                        x1g = (double)aux_ptr[1];
                                        x1r = (double)aux_ptr[2];
                                        x2b = x1b;
                                        x2g = x1g;
                                        x2r = x1r; // os pixeis de interpolação sao iguais

                                    }

                                }
                                bx2b = x1b + (x2b - x1b) * delta_x;
                                bx2g = x1g + (x2g - x1g) * delta_x;
                                bx2r = x1r + (x2r - x1r) * delta_x;

                                //interpolação em y 
                                //intensities[i] = (byte)(bx1 + (bx2 - bx1) * delta_y);


                                //blue
                                dataPtr_destino[0] = (byte)(bx1b + (bx2b - bx1b) * delta_y);
                                //green
                                dataPtr_destino[1] = (byte)(bx1g + (bx2g - bx1g) * delta_y);
                                //red
                                dataPtr_destino[2] = (byte)(bx1r + (bx2r - bx1r) * delta_y);

                            }

                            dataPtr_destino += nChan; // avançar o apontador para o proximo pixel na imagem de origem, para saber que info tenho de copiar dessa posição para a imagem destino, segundo a translacao
                            //x_origem = x_destino - tx 
                        }

                        dataPtr_destino += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }
        }

        /// <summary>
        /// Rotation_Bilinear_xy_point
        /// </summary> Roda em torno de um ponto
        /// <param name="img"></param>
        /// <param name="imgCopy"></param>
        /// <param name="angle"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        public static void Rotation_Bilinear_xy_point(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float angle, int centerX, int centerY)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_destino = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer();

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                double x_origem, y_origem;
                double sen_angle = Math.Sin(angle);
                double cos_angle = Math.Cos(angle);
                //double x_centro = (width / 2.0);
                double x_centro = (double)centerX;
                double y_centro = (double)centerY;
                byte[] result = new byte[3];
                byte* aux_ptr;
                double delta_x, delta_y;
                double bx1r, bx1b, bx1g, bx2r, bx2b, bx2g, x1r, x1b, x1g, x2r, x2b, x2g;
                bx1r = bx1b = bx1g = bx2r = bx2b = bx2g = x1r = x1b = x1g = x2r = x2b = x2g = 0.0;
                byte[] intensities = new byte[3];


                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 
                            //POSIÇÃO DE X destino EM FUNÇÃO DO X origem - ENDEREÇAMENTO ABSOLUTO
                            //NOTA: y = y_destino
                            //NOTA: x = x_destino
                            y_origem = y_centro - ((double)x - x_centro) * sen_angle - (y_centro - (double)y) * cos_angle;
                            x_origem = x_centro + ((double)x - x_centro) * cos_angle - (y_centro - (double)y) * sen_angle;

                            //result = Bilinear(img, imgCopy, x_origem, y_origem);
                            delta_x = x_origem - Math.Floor(x_origem);
                            delta_y = y_origem - Math.Floor(y_origem);

                            if (Math.Ceiling(y_origem) >= height || Math.Ceiling(x_origem) >= width || Math.Floor(y_origem) < 0 || Math.Floor(x_origem) < 0)
                            {
                                dataPtr_destino[0] = 255;
                                dataPtr_destino[1] = 255;
                                dataPtr_destino[2] = 255;
                            }

                            else
                            {
                                // percorrer os canales de cor

                                aux_ptr = dataPtr_origem + (int)Math.Floor(y_origem) * widthstep + (int)Math.Floor(x_origem) * nChan;
                                x1b = (double)aux_ptr[0];
                                x1g = (double)aux_ptr[1];
                                x1r = (double)aux_ptr[2];
                                if (Math.Ceiling(x_origem) < width) // se o pixel seguinte da origem estiver dentro da imagem
                                {
                                    aux_ptr = dataPtr_origem + (int)Math.Floor(y_origem) * widthstep + (int)Math.Ceiling(x_origem) * nChan;
                                    x2b = (double)aux_ptr[0];
                                    x2g = (double)aux_ptr[1];
                                    x2r = (double)aux_ptr[2];

                                }
                                else // se o pixel seguinte nao estiver dentro da imagem
                                {
                                    x2b = x1b;
                                    x2g = x1g;
                                    x2r = x1r; // os pixeis de interpolação sao iguais
                                }
                                bx1b = x1b + (x2b - x1b) * delta_x;
                                bx1g = x1g + (x2g - x1g) * delta_x;
                                bx1r = x1r + (x2r - x1r) * delta_x;

                                // interpolação em Bx2
                                if ((int)Math.Ceiling(y_origem) < height && (int)Math.Ceiling(x_origem) < width)
                                {
                                    aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + (int)Math.Floor(x_origem) * nChan;
                                    x1b = (double)aux_ptr[0];
                                    x1g = (double)aux_ptr[1];
                                    x1r = (double)aux_ptr[2];
                                    aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                    x2b = (double)aux_ptr[0];
                                    x2g = (double)aux_ptr[1];
                                    x2r = (double)aux_ptr[2];
                                }
                                else
                                {
                                    if ((int)Math.Ceiling(y_origem) > height - 1) // se o y_origem + 1 estiver fora dos limites da imagem
                                    {
                                        // os pixeis sao duplicados, e por isso y(k-1) = y(k)
                                        aux_ptr = dataPtr_origem + ((int)Math.Floor(y_origem)) * widthstep + (int)Math.Floor(x_origem) * nChan;
                                        x1b = (double)aux_ptr[0];
                                        x1g = (double)aux_ptr[1];
                                        x1r = (double)aux_ptr[2];
                                        // se o pixel estiver em colunas fora da imagem
                                        if ((int)Math.Ceiling(x_origem) > width - 1)
                                        {
                                            // a instruçao : aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                            // e substituida por : aux_ptr = dataPtr_origem + ((int)Math.Floor(y_origem)) * widthstep + ((int)x_origem) * nChan;
                                            //x2 = (double)aux_ptr[i];
                                            x2b = x1b;
                                            x2g = x1g;
                                            x2r = x1r; // os pixeis de interpolação sao iguais
                                        }
                                        else // o x origem + 1 esta dentro dos limites da imagem
                                        {
                                            aux_ptr = dataPtr_origem + ((int)Math.Floor(y_origem)) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                            x2b = (double)aux_ptr[0];
                                            x2g = (double)aux_ptr[1];
                                            x2r = (double)aux_ptr[2];
                                        }

                                    }
                                    else // o y_origem + 1 esta dentro dos limites da imagem - entao o x_origem esta fora dos limites da imagem
                                    {
                                        aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + (int)Math.Floor(x_origem) * nChan;
                                        x1b = (double)aux_ptr[0];
                                        x1g = (double)aux_ptr[1];
                                        x1r = (double)aux_ptr[2];
                                        x2b = x1b;
                                        x2g = x1g;
                                        x2r = x1r; // os pixeis de interpolação sao iguais

                                    }

                                }
                                bx2b = x1b + (x2b - x1b) * delta_x;
                                bx2g = x1g + (x2g - x1g) * delta_x;
                                bx2r = x1r + (x2r - x1r) * delta_x;

                                //interpolação em y 
                                //intensities[i] = (byte)(bx1 + (bx2 - bx1) * delta_y);


                                //blue
                                dataPtr_destino[0] = (byte)(bx1b + (bx2b - bx1b) * delta_y);
                                //green
                                dataPtr_destino[1] = (byte)(bx1g + (bx2g - bx1g) * delta_y);
                                //red
                                dataPtr_destino[2] = (byte)(bx1r + (bx2r - bx1r) * delta_y);

                            }

                            dataPtr_destino += nChan; // avançar o apontador para o proximo pixel na imagem de origem, para saber que info tenho de copiar dessa posição para a imagem destino, segundo a translacao
                            //x_origem = x_destino - tx 
                        }

                        dataPtr_destino += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }
        }

        /// <summary>
        /// Rotation - applies an inverse rotation to the image according to input angle
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="color_index"> clour_index of the colour to isolate</param>
        public static void Scale(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float scaleFactor)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_origem = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte* dataPtr_destino = (byte*)m_cpy.imageData.ToPointer();
                //comprimento da imagem (em bytes)

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                int x_origem, y_origem;

                byte* aux_ptr;

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 
                            //POSIÇÃO DE X destino EM FUNÇÃO DO X origem - ENDEREÇAMENTO ABSOLUTO
                            //NOTA: y = y_destino
                            //NOTA: x = x_destino
                            y_origem = (int)Math.Round(y / scaleFactor);
                            x_origem = (int)Math.Round(x / scaleFactor);
                            //se a posição estiver sobre o indice a sguir aos pixeis de borda, nao os devemos processar, pois estamos fora da imagem! (>=)
                            if (y_origem >= height || x_origem >= width || y_origem < 0 || x_origem < 0)
                            {
                                dataPtr_origem[0] = 0;
                                dataPtr_origem[1] = 0;
                                dataPtr_origem[2] = 0;
                            }
                            else
                            {
                                aux_ptr = dataPtr_destino + y_origem * widthstep + x_origem * nChan;
                                //blue
                                dataPtr_origem[0] = aux_ptr[0];
                                //green
                                dataPtr_origem[1] = aux_ptr[1];
                                //red
                                dataPtr_origem[2] = aux_ptr[2];
                            }

                            dataPtr_origem += nChan; // avançar o apontador para o proximo pixel na imagem de origem, para saber que info tenho de copiar dessa posição para a imagem destino, segundo a translacao
                            //x_origem = x_destino - tx 
                        }

                        dataPtr_origem += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }
        }

        /// <summary>
        /// Rotation - applies an inverse rotation to the image according to input angle
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="color_index"> clour_index of the colour to isolate</param>
        public static void Scale_point_xy(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float scaleFactor, int centerX, int centerY)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_destino = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer();
                //comprimento da imagem (em bytes)

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep;
                int x, y;
                int x_origem, y_origem;
                double x_center_img = width / 2;
                double y_center_img = height / 2;
                byte* aux_ptr;

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 
                            //POSIÇÃO DE X destino EM FUNÇÃO DO X origem - ENDEREÇAMENTO ABSOLUTO
                            //NOTA: y = y_destino
                            //NOTA: x = x_destino
                            y_origem = (int)Math.Round((y - y_center_img) / scaleFactor + centerY);
                            x_origem = (int)Math.Round((x - x_center_img) / scaleFactor + centerX);
                            //se a posição estiver sobre o indice a sguir aos pixeis de borda, nao os devemos processar, pois estamos fora da imagem! (>=)
                            if (y_origem >= height || x_origem >= width || y_origem < 0 || x_origem < 0)
                            {
                                dataPtr_destino[0] = 255;
                                dataPtr_destino[1] = 255;
                                dataPtr_destino[2] = 255;
                            }
                            else
                            {
                                aux_ptr = dataPtr_origem + y_origem * widthstep + x_origem * nChan;
                                //blue
                                dataPtr_destino[0] = aux_ptr[0];
                                //green
                                dataPtr_destino[1] = aux_ptr[1];
                                //red
                                dataPtr_destino[2] = aux_ptr[2];
                            }

                            dataPtr_destino += nChan; // avançar o apontador para o proximo pixel na imagem de origem, para saber que info tenho de copiar dessa posição para a imagem destino, segundo a translacao
                            //x_origem = x_destino - tx 
                        }

                        dataPtr_destino += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }
        }

        /// <summary>
        /// Rotation - applies zoom to the image according to input scale factor
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img"> image </param>
        /// <param name="imgCopy"> copy of the image </param>
        /// <param name="scaleFactor"> scale factor applied </param>
        public static void Scale_Bilinear(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float scaleFactor)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_destino = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer();
                //comprimento da imagem (em bytes)

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                double x_origem, y_origem;
                double delta_x, delta_y;
                double bx1, bx2, x1, x2;
                byte[] intensities = new byte[3];

                byte* aux_ptr;

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 
                            //POSIÇÃO DE X destino EM FUNÇÃO DO X origem - ENDEREÇAMENTO ABSOLUTO
                            //NOTA: y = y_destino
                            //NOTA: x = x_destino
                            y_origem = y / scaleFactor;
                            x_origem = x / scaleFactor;

                            delta_x = x_origem - (int)(x_origem);
                            delta_y = y_origem - (int)(y_origem);

                            if (y_origem >= height || x_origem >= width || y_origem < 0 || x_origem < 0)
                            {
                                dataPtr_destino[0] = 255;
                                dataPtr_destino[1] = 255;
                                dataPtr_destino[2] = 255;
                            }

                            else
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    aux_ptr = dataPtr_origem + (int)Math.Floor(y_origem) * widthstep + (int)Math.Floor(x_origem)* nChan;
                                    x1 = aux_ptr[i];
                                    aux_ptr = dataPtr_origem + (int)Math.Floor(y_origem) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                    x2 = aux_ptr[i];

                                    bx1 = x1 + (x2 - x1) * delta_x;

                                    aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + (int)Math.Floor(x_origem)* nChan;
                                    x1 = aux_ptr[i];
                                    aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                    x2 = aux_ptr[i];

                                    bx2 = x1 + (x2 - x1) * delta_y;

                                    //interpolação em y 
                                    intensities[i] = (byte)(bx1 + (bx2 - bx1) * delta_y);
                                }

                                //blue
                                dataPtr_destino[0] = intensities[0];
                                //green
                                dataPtr_destino[1] = intensities[1];
                                //red
                                dataPtr_destino[2] = intensities[2];

                            }

                            dataPtr_destino += nChan; // avançar o apontador para o proximo pixel na imagem de origem, para saber que info tenho de copiar dessa posição para a imagem destino, segundo a translacao
                            //x_origem = x_destino - tx 
                        }

                        dataPtr_destino += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }
        }

        /// <summary>
        /// Rotation - applies an inverse rotation to the image according to input angle
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img"> image </param>
        /// <param name="imgCopy"> copy of the image </param>
        /// <param name="scaleFactor"> sacle factor applied </param>
        /// <param name="centerX"> X coordinate of the image where the mouse click was performed </param>
        /// <param name="centerY"> Y coordinate of the image where the mouse click was performed </param>
        public static void Scale_point_xy_Bilinear(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float scaleFactor, int centerX, int centerY)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_destino = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer();
                //comprimento da imagem (em bytes)

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep;
                int x, y;
                double x_origem, y_origem;
                double x_center_img = width / 2;
                double y_center_img = height / 2;
                byte* aux_ptr;
                double delta_x, delta_y;
                double bx1, bx2, x1, x2;
                byte[] intensities = new byte[3];

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 
                            //POSIÇÃO DE X destino EM FUNÇÃO DO X origem - ENDEREÇAMENTO ABSOLUTO
                            //NOTA: y = y_destino
                            //NOTA: x = x_destino
                            y_origem = (y - y_center_img) / scaleFactor + centerY;
                            x_origem = (x - x_center_img) / scaleFactor + centerX;

                            delta_x = x_origem - (int)(x_origem);
                            delta_y = y_origem - (int)(y_origem);

                            if (y_origem >= height || x_origem >= width || y_origem < 0 || x_origem < 0)
                            {
                                dataPtr_destino[0] = 255;
                                dataPtr_destino[1] = 255;
                                dataPtr_destino[2] = 255;
                            }

                            else
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    aux_ptr = dataPtr_origem + (int)Math.Floor(y_origem) * widthstep + (int)Math.Floor(x_origem)* nChan;
                                    x1 = aux_ptr[i];
                                    aux_ptr = dataPtr_origem + (int)Math.Floor(y_origem) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                    x2 = aux_ptr[i];

                                    bx1 = x1 + (x2 - x1) * delta_x;

                                    aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + (int)Math.Floor(x_origem)* nChan;
                                    x1 = aux_ptr[i];
                                    aux_ptr = dataPtr_origem + ((int)Math.Ceiling(y_origem)) * widthstep + ((int)Math.Ceiling(x_origem)) * nChan;
                                    x2 = aux_ptr[i];

                                    bx2 = x1 + (x2 - x1) * delta_y;

                                    //interpolação em y 
                                    intensities[i] = (byte)(bx1 + (bx2 - bx1) * delta_y);
                                }

                                //blue
                                dataPtr_destino[0] = intensities[0];
                                //green
                                dataPtr_destino[1] = intensities[1];
                                //red
                                dataPtr_destino[2] = intensities[2];

                            }

                            dataPtr_destino += nChan; // avançar o apontador para o proximo pixel na imagem de origem, para saber que info tenho de copiar dessa posição para a imagem destino, segundo a translacao
                            //x_origem = x_destino - tx 
                        }

                        dataPtr_destino += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }

                }

            }
        }

        /// <summary>
        /// Rotation - applies an inverse rotation to the image according to input angle
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="color_index"> clour_index of the colour to isolate</param>
        public static void Mean(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image

                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer();

                //comprimento da imagem (em bytes)

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y, j, k;
                byte* aux_ptr;
                int blue_sum, green_sum, red_sum;
                double area_reg = 9.0;

                if (nChan == 3)
                {

                    // avançar logo uma linha 
                    //avançar logo uma coluna
                    //dataPtr = dataPtr + widthstep + nChan;

                    //processar core
                    for (y = 1; y < (height - 1); y++) // a cada linha de pixeis
                    {
                        for (x = 1; x < (width - 1); x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais 
                            // processar pixeis do core - regiao 3x3

                            blue_sum = 0;
                            green_sum = 0;
                            red_sum = 0;

                            for (k = -1; k < 2; k++) // j e k sao os indices na imagem de origem dos pixeis
                            {
                                for (j = -1; j < 2; j++)
                                {

                                    aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                                    //blue
                                    blue_sum += (int)aux_ptr[0];
                                    //green
                                    green_sum += (int)aux_ptr[1];
                                    //red
                                    red_sum += (int)aux_ptr[2];

                                }
                            }
                            // usar o endereçamento absoluto para nao alterar o valor do apontador da imagem a retornar
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            //blue
                            aux_ptr[0] = (byte)Math.Round((double)blue_sum / area_reg);
                            //green
                            aux_ptr[1] = (byte)Math.Round((double)green_sum / area_reg);
                            //red
                            aux_ptr[2] = (byte)Math.Round((double)red_sum / area_reg);


                            //dataPtr += nChan; // avançar o apontador para o proximo pixel na imagem de origem, para saber que info tenho de copiar dessa posição para a imagem destino, segundo a translacao
                            //x_origem = x_destino - tx 
                        }

                        //dataPtr += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }


                    //processar bordas
                    //processamento à esquerda - na primeira coluna inteira, sem cantos
                    x = 0;
                    for (y = 1; y < height - 1; y++) // a cada linha de pixeis
                    {

                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;

                        for (k = -1; k < 2; k++)
                        {
                            for (j = 0; j < 2; j++)// ir aos pixeis da coluna da borda, e da coluna da direita, apenas
                            {
                                aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                                //blue
                                blue_sum += (int)aux_ptr[0];
                                //green
                                green_sum += (int)aux_ptr[1];
                                //red
                                red_sum += (int)aux_ptr[2];
                            }
                        }
                        //duplicar valores correspondentes a coluna fora da imagem
                        j = 0; // ficar na coluna da borda
                        for (k = -1; k < 2; k++) // percorrer as linhas de cima para baixo
                        {
                            aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                            //blue
                            blue_sum += (int)aux_ptr[0];
                            //green
                            green_sum += (int)aux_ptr[1];
                            //red
                            red_sum += (int)aux_ptr[2];
                        }

                        // usar o endereçamento absoluto para nao alterar o valor do apontador da imagem a retornar
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        //blue
                        aux_ptr[0] = (byte)Math.Round((double)blue_sum / area_reg);
                        //green
                        aux_ptr[1] = (byte)Math.Round((double)green_sum / area_reg);
                        //red
                        aux_ptr[2] = (byte)Math.Round((double)red_sum / area_reg);

                    }

                    //processar borda da coluna mais a direita
                    x = width - 1;
                    for (y = 1; y < height - 1; y++) // a cada linha de pixeis
                    {
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;

                        for (k = -1; k < 2; k++)
                        {
                            for (j = -1; j < 1; j++) // ir aos pixeis da coluna da borda, e da coluna da esquerda, apenas
                            {
                                aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                                //blue
                                blue_sum += (int)aux_ptr[0];
                                //green
                                green_sum += (int)aux_ptr[1];
                                //red
                                red_sum += (int)aux_ptr[2];
                            }
                        }
                        //duplicar valores correspondentes a coluna fora da imagem
                        j = 0; // ficar na coluna da borda
                        for (k = -1; k < 2; k++) // percorrer as linhas de cima para baixo
                        {
                            aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                            //blue
                            blue_sum += (int)aux_ptr[0];
                            //green
                            green_sum += (int)aux_ptr[1];
                            //red
                            red_sum += (int)aux_ptr[2];
                        }
                        // usar o endereçamento absoluto para nao alterar o valor do apontador da imagem a retornar
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        //blue
                        aux_ptr[0] = (byte)Math.Round((double)blue_sum / area_reg);
                        //green
                        aux_ptr[1] = (byte)Math.Round((double)green_sum / area_reg);
                        //red
                        aux_ptr[2] = (byte)Math.Round((double)red_sum / area_reg);
                    }

                    //processar borda da linha de cima
                    y = 0;
                    for (x = 1; x < width - 1; x++) // a cada coluna de pixeis, sem contar com o do canto
                    {
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;

                        for (k = 0; k < 2; k++) // ir aos pixeis da linha da borda, e da linha abaixo, apenas
                        {
                            for (j = -1; j < 2; j++)
                            {
                                aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                                //blue
                                blue_sum += (int)aux_ptr[0];
                                //green
                                green_sum += (int)aux_ptr[1];
                                //red
                                red_sum += (int)aux_ptr[2];
                            }
                        }
                        //duplicar valores correspondentes a linha fora da imagem, na parte de cima
                        k = 0; // ficar na linha da borda
                        for (j = -1; j < 2; j++) // percorrer as colunas da esquerda para a direita
                        {
                            aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                            //blue
                            blue_sum += (int)aux_ptr[0];
                            //green
                            green_sum += (int)aux_ptr[1];
                            //red
                            red_sum += (int)aux_ptr[2];
                        }
                        // usar o endereçamento absoluto para nao alterar o valor do apontador da imagem a retornar
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        //blue
                        aux_ptr[0] = (byte)Math.Round((double)blue_sum / area_reg);
                        //green
                        aux_ptr[1] = (byte)Math.Round((double)green_sum / area_reg);
                        //red
                        aux_ptr[2] = (byte)Math.Round((double)red_sum / area_reg);

                    }

                    //processar borda da linha de baixo
                    y = height - 1;
                    for (x = 1; x < width - 1; x++) // a cada coluna de pixeis, sem contar com os cantos
                    {
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;

                        for (k = -1; k < 1; k++) // ir aos pixeis da linha da borda, e da linha acima, apenas
                        {
                            for (j = -1; j < 2; j++)
                            {
                                aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                                //blue
                                blue_sum += (int)aux_ptr[0];
                                //green
                                green_sum += (int)aux_ptr[1];
                                //red
                                red_sum += (int)aux_ptr[2];
                            }
                        }
                        //duplicar valores correspondentes a linha fora da imagem, na parte de cima
                        k = 0; // ficar na linha da borda
                        for (j = -1; j < 2; j++) // percorrer as colunas da esquerda para a direita
                        {
                            aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                            //blue
                            blue_sum += (int)aux_ptr[0];
                            //green
                            green_sum += (int)aux_ptr[1];
                            //red
                            red_sum += (int)aux_ptr[2];
                        }
                        // usar o endereçamento absoluto para nao alterar o valor do apontador da imagem a retornar
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        //blue
                        aux_ptr[0] = (byte)Math.Round((double)blue_sum / area_reg);
                        //green
                        aux_ptr[1] = (byte)Math.Round((double)green_sum / area_reg);
                        //red
                        aux_ptr[2] = (byte)Math.Round((double)red_sum / area_reg);

                    }

                    //reset aos acumuladores
                    blue_sum = 0;
                    green_sum = 0;
                    red_sum = 0;
                    //canto superior esquerdo
                    x = 0;
                    y = 0;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    //acumular os tres pixeis iguais ao do canto - solucao A (duplicaçao) do filtro de medias
                    blue_sum = 3 * aux_ptr[0];
                    green_sum = 3 * aux_ptr[1];
                    red_sum = 3 * aux_ptr[2];

                    //acumular os 3 pixeis dentro da imagem encostados ao pixel de canto, e o propiro pixel de canto
                    for (k = 0; k < 2; k++)
                    {
                        for (j = 0; j < 2; j++)
                        {
                            aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }
                    }
                    // duplicar o pixel encostado a borda 
                    // diretamente abaixo do pixel de canto
                    aux_ptr = dataPtr_origem + (y + 1) * widthstep + x * nChan;
                    blue_sum += aux_ptr[0];
                    green_sum += aux_ptr[1];
                    red_sum += aux_ptr[2];
                    // diretamente a direita do pixel de canto
                    aux_ptr = dataPtr_origem + y * widthstep + (x + 1) * nChan;
                    blue_sum += aux_ptr[0];
                    green_sum += aux_ptr[1];
                    red_sum += aux_ptr[2];

                    //atualizar intensidades RGB do pixel
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    // blue
                    aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                    //green
                    aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                    //red
                    aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);


                    //reset aos acumuladores
                    blue_sum = 0;
                    green_sum = 0;
                    red_sum = 0;
                    //canto superior direito
                    x = width - 1;
                    y = 0;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    //acumular os tres pixeis iguais ao do canto - solucao A (duplicaçao) do filtro de medias
                    blue_sum = 3 * aux_ptr[0];
                    green_sum = 3 * aux_ptr[1];
                    red_sum = 3 * aux_ptr[2];
                    //acumular 4 os pixeis 
                    for (k = 0; k < 2; k++) // que estao abaixo do canto superior
                    {
                        for (j = -1; j < 1; j++) // que estao atras do pixel de canto direito , e no proprio canto
                        {
                            aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }
                    }
                    // duplicar o pixel encostado a borda 
                    // diretamente abaixo do pixel de canto
                    aux_ptr = dataPtr_origem + (y + 1) * widthstep + x * nChan;
                    blue_sum += aux_ptr[0];
                    green_sum += aux_ptr[1];
                    red_sum += aux_ptr[2];
                    // e diretamente a esquerda do pixel canto
                    aux_ptr = dataPtr_origem + y * widthstep + (x - 1) * nChan;
                    blue_sum += aux_ptr[0];
                    green_sum += aux_ptr[1];
                    red_sum += aux_ptr[2];
                    //atualizar intensidades RGB do pixel
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    // blue
                    aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                    //green
                    aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                    //red
                    aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);

                    //reset aos acumuladores
                    blue_sum = 0;
                    green_sum = 0;
                    red_sum = 0;
                    //canto inferior esquerdo
                    x = 0;
                    y = height - 1;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    // acumular os 3 pixeis encostados ao pixel do canto, que sao copias do pixel de canto - solucao A (duplicacao) do filtro de medias
                    blue_sum = 3 * aux_ptr[0];
                    green_sum = 3 * aux_ptr[1];
                    red_sum = 3 * aux_ptr[2];
                    //acumular os pixeis de canto
                    for (k = -1; k < 1; k++) // juntamente com o pixel na linha acima do canto inferior
                    {
                        for (j = 0; j < 2; j++) // juntamnete com o pixel na coluna a direita do canto esquerdo
                        {
                            aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }
                    }
                    // duplicar o pixel encostado a borda 
                    // diretamente acima do pixel de canto
                    aux_ptr = dataPtr_origem + (y - 1) * widthstep + x * nChan;
                    blue_sum += aux_ptr[0];
                    green_sum += aux_ptr[1];
                    red_sum += aux_ptr[2];
                    // diretamente a direita do canto
                    aux_ptr = dataPtr_origem + y * widthstep + (x + 1) * nChan;
                    blue_sum += aux_ptr[0];
                    green_sum += aux_ptr[1];
                    red_sum += aux_ptr[2];
                    //atualizar intensidades RGB do pixel
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    // blue
                    aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                    //green
                    aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                    //red
                    aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);

                    //reset aos acumuladores
                    blue_sum = 0;
                    green_sum = 0;
                    red_sum = 0;
                    //canto inferior direito
                    x = width - 1;
                    y = height - 1;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    //repetir 3 vezes o valor do pixel do canto, para contabilizar os que estao fora da imagem
                    blue_sum = 3 * aux_ptr[0];
                    green_sum = 3 * aux_ptr[1];
                    red_sum = 3 * aux_ptr[2];
                    //acumular o pixel de canto
                    for (k = -1; k < 1; k++) // juntamente com o pixel na linha acima do canto inferior
                    {
                        for (j = -1; j < 1; j++) //juntamente com o pixel na coluna a esquerda do canto direito
                        {
                            aux_ptr = dataPtr_origem + (y + k) * widthstep + (x + j) * nChan;
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }
                    }
                    //duplicar valor do pixel da linha acima do pixel de canto
                    aux_ptr = dataPtr_origem + (y - 1) * widthstep + x * nChan;
                    blue_sum += aux_ptr[0];
                    green_sum += aux_ptr[1];
                    red_sum += aux_ptr[2];
                    //duplicar valor do pixel da coluna a esquerda do pixel de canto
                    aux_ptr = dataPtr_origem + y * widthstep + (x - 1) * nChan;
                    blue_sum += aux_ptr[0];
                    green_sum += aux_ptr[1];
                    red_sum += aux_ptr[2];
                    //atualizar intensidades RGB do pixel
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    // blue
                    aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                    //green
                    aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                    //red
                    aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);


                }


            }
        }

        /// <summary>
        /// Mean - applies an means filter using C solution for the interior of the image
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void Mean_solutionC(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, int size)
        {
            unsafe
            {
                // imagem integral - estrutura igual a da imagem origem 


                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer(); // Pointer to the original image

                //comprimento da imagem (em bytes)
                int width = img.Width;
                int height = img.Height;
                long[,,] img_int = new long[height, width, 3];
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y, i, j, k;
                byte* aux_ptr;  // apontador auxiliar para a imagem final
                long* aux_ptr_int;  // apontador auxiliar para a imagem integral
                long blue_sum, green_sum, red_sum;
                double area_reg = (double)(size * size);
                int prev_avg_blue = 0; // media colocada no pixel anterior - para implementar algoritmo de soluçao B
                int prev_avg_green = 0;
                int prev_avg_red = 0;
                int overlap_size = (int)Math.Round(size / 2.0) - 1; // numero de pixeis duplidcados para fora da imagem a considerar
                                                                    // (em todas as direçoes)
                int y_pix = 0;
                int x_pix = 0;
                if (nChan == 3 && (size > 1) && (size % 2 != 0)) // se o size for par, nulo ou de tamanho 1, nao vale a pena correr o algoritmo
                {

                    //processar primeiro pixie - USANDO SOLUCAO A - canto superior esquerdo
                    //reset aos acumuladores
                    blue_sum = 0;
                    green_sum = 0;
                    red_sum = 0;
                    //canto superior esquerdo
                    x = 0;
                    y = 0;
                    aux_ptr = (byte*)(dataPtr_origem + y * widthstep + x * nChan);
                    //acumular os pixeis iguais ao do canto - solucao A (duplicaçao) do filtro de medias
                    blue_sum = (overlap_size + overlap_size * overlap_size + overlap_size) * aux_ptr[0];
                    green_sum = (overlap_size + overlap_size * overlap_size + overlap_size) * aux_ptr[1];
                    red_sum = (overlap_size + overlap_size * overlap_size + overlap_size) * aux_ptr[2];

                    //acumular os *overlap_size* pixeis dentro da imagem encostados ao pixel de canto, e o propiro pixel de canto
                    // FAZER COM PADDING
                    for (k = 0; k < overlap_size; k++) // EM Y
                    {
                        for (j = 0; j < overlap_size; j++) // EM X
                        {
                            aux_ptr = (byte*)(dataPtr_origem + (y + k) * widthstep + (x + j) * nChan);
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }
                    }
                    // duplicar os pixeis encostados as bordas

                    // diretamente abaixo do pixel de canto
                    for (j = 0; j < overlap_size; j++)
                    {
                        aux_ptr = (byte*)(dataPtr_origem + (y + j) * widthstep + x * nChan);
                        blue_sum += aux_ptr[0];
                        green_sum += aux_ptr[1];
                        red_sum += aux_ptr[2];
                    }

                    // diretamente a direita do pixel de canto
                    for (i = 0; i < overlap_size; i++)
                    {
                        aux_ptr = (byte*)(dataPtr_origem + y * widthstep + (x + i) * nChan);
                        blue_sum += aux_ptr[0];
                        green_sum += aux_ptr[1];
                        red_sum += aux_ptr[2];
                    }

                    //atualizar intensidades RGB do pixel
                    aux_ptr = (dataPtr + y * widthstep + x * nChan);
                    // blue
                    aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                    //green
                    aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                    //red
                    aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);

                    // declarar medias anteriores - para calcular solução B
                    prev_avg_blue = aux_ptr[0];
                    prev_avg_green = aux_ptr[1];
                    prev_avg_red = aux_ptr[2];

                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ///
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // PROCESSAR PRIMEIRA COLUNA - USANDO SOLUCAO B
                    //processamento à esquerda - na primeira coluna inteira, sem cantos
                    x = 0;

                    for (y = 1; y < overlap_size + 1; y++)
                    {
                        // linha que entra
                        // copiar duas vezes o valor do canto superior esquerdo
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // diferenca entre o pixel em questao e o pixel do canto
                        aux_ptr = (byte*)(dataPtr_origem + (0) * widthstep + x * nChan);
                        //subtrair os over_lap+1 pixeis iguais ao do canto que estao fora da area do filtro 
                        blue_sum -= (overlap_size + 1) * aux_ptr[0];
                        green_sum -= (overlap_size + 1) * aux_ptr[1];
                        red_sum -= (overlap_size + 1) * aux_ptr[2];

                        // subtrair prixel copiado da primeira linha, nas overlap_size colunas ao lado direito
                        for (i = 0; i < overlap_size; i++)
                        {
                            aux_ptr = (byte*)(dataPtr_origem + (0) * widthstep + (x + (i + 1)) * nChan);
                            blue_sum -= aux_ptr[0];
                            green_sum -= aux_ptr[1];
                            red_sum -= aux_ptr[2];
                        }

                        //somar valores da linha a seguir à segunda - "linha que entra"
                        //duplicar o valor do pixel encostado a borda, da segunda linha
                        aux_ptr = (byte*)(dataPtr_origem + (y + overlap_size) * widthstep + x * nChan);
                        blue_sum += (overlap_size + 1) * aux_ptr[0];
                        green_sum += (overlap_size + 1) * aux_ptr[1];
                        red_sum += (overlap_size + 1) * aux_ptr[2];

                        for (i = 0; i < overlap_size; i++)
                        {
                            //somar o pixel da 2a coluna, pertencente a linha que entra
                            aux_ptr = (byte*)(dataPtr_origem + (y + overlap_size) * widthstep + (x + (i + 1)) * nChan);
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }

                        // nova media                 //media que vem do pixel anterior + linhas que entram - linhas que saem
                        prev_avg_blue = (byte)Math.Round(prev_avg_blue + (double)blue_sum / area_reg);
                        prev_avg_green = (byte)Math.Round(prev_avg_green + (double)green_sum / area_reg);
                        prev_avg_red = (byte)Math.Round(prev_avg_red + (double)red_sum / area_reg);

                        aux_ptr = (dataPtr + y * widthstep + x * nChan);
                        //meter o valor de media no pixel da iamgem destino
                        //blue 
                        aux_ptr[0] = (byte)prev_avg_blue;
                        //green
                        aux_ptr[1] = (byte)prev_avg_green;
                        //red
                        aux_ptr[2] = (byte)prev_avg_red;

                    }


                    //apartir de y = overlap_size ultima linha nao processada
                    for (y = overlap_size + 1; y < height - overlap_size; y++) // a cada linha de pixeis, ate a penultima linha
                    {
                        // limpar acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;

                        //subtrair os pixeis da linha que entra
                        aux_ptr = (byte*)(dataPtr_origem + (y - overlap_size - 1) * widthstep + x * nChan);
                        blue_sum -= (overlap_size + 1) * aux_ptr[0];
                        green_sum -= (overlap_size + 1) * aux_ptr[1];
                        red_sum -= (overlap_size + 1) * aux_ptr[2];

                        // subtrair prixel copiado da primeira linha, nas overlap_size colunas ao lado direito
                        for (i = 0; i < overlap_size; i++)
                        {
                            aux_ptr = (byte*)(dataPtr_origem + (y - overlap_size - 1) * widthstep + (x + (i + 1)) * nChan);
                            blue_sum -= aux_ptr[0];
                            green_sum -= aux_ptr[1];
                            red_sum -= aux_ptr[2];
                        }

                        //somar valores da linha a seguir - "linha que entra"
                        //duplicar o valor do pixel encostado a borda, da segunda linha
                        aux_ptr = (byte*)(dataPtr_origem + (y + overlap_size) * widthstep + x * nChan);
                        blue_sum += (overlap_size + 1) * aux_ptr[0];
                        green_sum += (overlap_size + 1) * aux_ptr[1];
                        red_sum += (overlap_size + 1) * aux_ptr[2];

                        for (i = 0; i < overlap_size; i++)
                        {
                            //somar o pixel das colunas dentro da iamgem, pertencente a linha que entra
                            aux_ptr = (byte*)(dataPtr_origem + (y + overlap_size) * widthstep + (x + (i + 1)) * nChan);
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }

                        // nova media                 //media que vem do pixel anterior + linhas que entram - linhas que saem
                        prev_avg_blue = (byte)Math.Round(prev_avg_blue + (double)blue_sum / area_reg);
                        prev_avg_green = (byte)Math.Round(prev_avg_green + (double)green_sum / area_reg);
                        prev_avg_red = (byte)Math.Round(prev_avg_red + (double)red_sum / area_reg);

                        aux_ptr = (dataPtr + y * widthstep + x * nChan);
                        //meter o valor de media no pixel da iamgem destino
                        //blue 
                        aux_ptr[0] = (byte)prev_avg_blue;
                        //green
                        aux_ptr[1] = (byte)prev_avg_green;
                        //red
                        aux_ptr[2] = (byte)prev_avg_red;

                    }

                    //processar ultimas (overlap_size) linhas que sao caso especial
                    // linha que entra
                    // copiar duas vezes o valor do canto superior esquerdo
                    for (y = height - overlap_size; y < height; y++)
                    {
                        // limpar acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // diferenca entre o pixel em questao e o pixel do canto
                        aux_ptr = (byte*)(dataPtr_origem + (y - overlap_size - 1) * widthstep + x * nChan);
                        //subtrair os over_lap+1 pixeis iguais ao do canto que estao fora da area do filtro 
                        blue_sum -= (overlap_size + 1) * aux_ptr[0];
                        green_sum -= (overlap_size + 1) * aux_ptr[1];
                        red_sum -= (overlap_size + 1) * aux_ptr[2];

                        // subtrair prixel copiado da primeira linha, nas overlap_size colunas ao lado direito
                        for (i = 0; i < overlap_size; i++)
                        {
                            aux_ptr = (byte*)(dataPtr_origem + (y - overlap_size - 1) * widthstep + (x + (i + 1)) * nChan);
                            blue_sum -= aux_ptr[0];
                            green_sum -= aux_ptr[1];
                            red_sum -= aux_ptr[2];
                        }

                        //somar valores da linha a seguir à segunda - "linha que entra"
                        //duplicar o valor do pixel encostado a borda, da segunda linha
                        aux_ptr = (byte*)(dataPtr_origem + (height - 1) * widthstep + x * nChan);
                        blue_sum += (overlap_size + 1) * aux_ptr[0];
                        green_sum += (overlap_size + 1) * aux_ptr[1];
                        red_sum += (overlap_size + 1) * aux_ptr[2];

                        for (i = 0; i < overlap_size; i++)
                        {
                            //somar o pixel da 2a coluna, pertencente a linha que entra
                            aux_ptr = (byte*)(dataPtr_origem + (height - 1) * widthstep + (x + (i + 1)) * nChan);
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }

                        // nova media                 //media que vem do pixel anterior + linhas que entram - linhas que saem
                        prev_avg_blue = (byte)Math.Round(prev_avg_blue + (double)blue_sum / area_reg);
                        prev_avg_green = (byte)Math.Round(prev_avg_green + (double)green_sum / area_reg);
                        prev_avg_red = (byte)Math.Round(prev_avg_red + (double)red_sum / area_reg);

                        aux_ptr = (dataPtr + y * widthstep + x * nChan);
                        //meter o valor de media no pixel da iamgem destino
                        //blue 
                        aux_ptr[0] = (byte)prev_avg_blue;
                        //green
                        aux_ptr[1] = (byte)prev_avg_green;
                        //red
                        aux_ptr[2] = (byte)prev_avg_red;

                    }


                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                    ///
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // PROCESSAR PRIMEIRA LINHA - USANDO SOLUCAO B
                    //processamento na primeira linha inteira, sem cantos
                    y = 0;

                    for (x = 1; x < overlap_size + 1; x++)
                    {
                        // linha que entra
                        // copiar duas vezes o valor do canto superior esquerdo
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // diferenca entre o pixel em questao e o pixel do canto
                        aux_ptr = (byte*)(dataPtr_origem + y * widthstep + 0 * nChan);
                        //subtrair os over_lap+1 pixeis iguais ao do canto que estao fora da area do filtro 
                        blue_sum -= (overlap_size + 1) * aux_ptr[0];
                        green_sum -= (overlap_size + 1) * aux_ptr[1];
                        red_sum -= (overlap_size + 1) * aux_ptr[2];

                        // subtrair os prixeis copiados da primeira coluna, nas overlap_size linhas em baixo do canto
                        for (j = 0; j < overlap_size; j++)
                        {
                            aux_ptr = (byte*)(dataPtr_origem + (y + (j + 1)) * widthstep + 0 * nChan);
                            blue_sum -= aux_ptr[0];
                            green_sum -= aux_ptr[1];
                            red_sum -= aux_ptr[2];
                        }

                        //somar valores da coluna a seguir - "linha que entra"
                        //duplicar o valor do pixel encostado a borda, da coluna que entra
                        aux_ptr = (byte*)(dataPtr_origem + y * widthstep + (x + overlap_size) * nChan);
                        blue_sum += (overlap_size + 1) * aux_ptr[0];
                        green_sum += (overlap_size + 1) * aux_ptr[1];
                        red_sum += (overlap_size + 1) * aux_ptr[2];

                        for (j = 0; j < overlap_size; j++)
                        {
                            //somar o pixel das overlap_size linhas, pertencente a linha que entra
                            aux_ptr = (byte*)(dataPtr_origem + (y + (j + 1)) * widthstep + (x + overlap_size) * nChan);
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }

                        // nova media                 //media que vem do pixel anterior + linhas que entram - linhas que saem
                        prev_avg_blue = (byte)Math.Round(prev_avg_blue + (double)blue_sum / area_reg);
                        prev_avg_green = (byte)Math.Round(prev_avg_green + (double)green_sum / area_reg);
                        prev_avg_red = (byte)Math.Round(prev_avg_red + (double)red_sum / area_reg);

                        aux_ptr = (dataPtr + y * widthstep + x * nChan);
                        //meter o valor de media no pixel da iamgem destino
                        //blue 
                        aux_ptr[0] = (byte)prev_avg_blue;
                        //green
                        aux_ptr[1] = (byte)prev_avg_green;
                        //red
                        aux_ptr[2] = (byte)prev_avg_red;

                    }


                    //apartir de x = overlap_size ultima linha nao processada
                    for (x = overlap_size + 1; x < width - overlap_size; x++)
                    {
                        // linha que entra
                        // copiar duas vezes o valor do canto superior esquerdo
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // diferenca entre o pixel em questao e o pixel do canto
                        aux_ptr = (byte*)(dataPtr_origem + y * widthstep + (x - overlap_size - 1) * nChan);
                        //subtrair os over_lap+1 pixeis iguais ao do canto que estao fora da area do filtro 
                        blue_sum -= (overlap_size + 1) * aux_ptr[0];
                        green_sum -= (overlap_size + 1) * aux_ptr[1];
                        red_sum -= (overlap_size + 1) * aux_ptr[2];

                        // subtrair os prixeis copiados da primeira coluna, nas overlap_size linhas em baixo do canto
                        for (j = 0; j < overlap_size; j++)
                        {
                            aux_ptr = (byte*)(dataPtr_origem + (y + (j + 1)) * widthstep + (x - overlap_size - 1) * nChan);
                            blue_sum -= aux_ptr[0];
                            green_sum -= aux_ptr[1];
                            red_sum -= aux_ptr[2];
                        }

                        //somar valores da coluna a seguir - "linha que entra"
                        //duplicar o valor do pixel encostado a borda, da coluna que entra
                        aux_ptr = (byte*)(dataPtr_origem + y * widthstep + (x + overlap_size) * nChan);
                        blue_sum += (overlap_size + 1) * aux_ptr[0];
                        green_sum += (overlap_size + 1) * aux_ptr[1];
                        red_sum += (overlap_size + 1) * aux_ptr[2];

                        for (j = 0; j < overlap_size; j++)
                        {
                            //somar o pixel das overlap_size linhas, pertencente a linha que entra
                            aux_ptr = (byte*)(dataPtr_origem + (y + (j + 1)) * widthstep + (x + overlap_size) * nChan);
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }

                        // nova media                 //media que vem do pixel anterior + linhas que entram - linhas que saem
                        prev_avg_blue = (byte)Math.Round(prev_avg_blue + (double)blue_sum / area_reg);
                        prev_avg_green = (byte)Math.Round(prev_avg_green + (double)green_sum / area_reg);
                        prev_avg_red = (byte)Math.Round(prev_avg_red + (double)red_sum / area_reg);

                        aux_ptr = (dataPtr + y * widthstep + x * nChan);
                        //meter o valor de media no pixel da iamgem destino
                        //blue 
                        aux_ptr[0] = (byte)prev_avg_blue;
                        //green
                        aux_ptr[1] = (byte)prev_avg_green;
                        //red
                        aux_ptr[2] = (byte)prev_avg_red;

                    }

                    //apartir de x = overlap_size ultima linha nao processada
                    for (x = width - overlap_size; x < width; x++)
                    {
                        // linha que entra
                        // copiar duas vezes o valor do canto superior esquerdo
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // diferenca entre o pixel em questao e o pixel do canto
                        aux_ptr = (byte*)(dataPtr_origem + y * widthstep + (x - overlap_size - 1) * nChan);
                        //subtrair os over_lap+1 pixeis iguais ao do canto que estao fora da area do filtro 
                        blue_sum -= (overlap_size + 1) * aux_ptr[0];
                        green_sum -= (overlap_size + 1) * aux_ptr[1];
                        red_sum -= (overlap_size + 1) * aux_ptr[2];

                        // subtrair os prixeis copiados da primeira coluna, nas overlap_size linhas em baixo do canto
                        for (j = 0; j < overlap_size; j++)
                        {
                            aux_ptr = (byte*)(dataPtr_origem + (y + (j + 1)) * widthstep + (x - overlap_size - 1) * nChan);
                            blue_sum -= aux_ptr[0];
                            green_sum -= aux_ptr[1];
                            red_sum -= aux_ptr[2];
                        }

                        //somar valores da coluna a seguir - "linha que entra"
                        //duplicar o valor do pixel encostado a borda, da coluna que entra
                        aux_ptr = (byte*)(dataPtr_origem + y * widthstep + (width - 1) * nChan);
                        blue_sum += (overlap_size + 1) * aux_ptr[0];
                        green_sum += (overlap_size + 1) * aux_ptr[1];
                        red_sum += (overlap_size + 1) * aux_ptr[2];

                        for (j = 0; j < overlap_size; j++)
                        {
                            //somar o pixel das overlap_size linhas, pertencente a linha que entra
                            aux_ptr = (byte*)(dataPtr_origem + (y + (j + 1)) * widthstep + (width - 1) * nChan);
                            blue_sum += aux_ptr[0];
                            green_sum += aux_ptr[1];
                            red_sum += aux_ptr[2];
                        }

                        // nova media                 //media que vem do pixel anterior + linhas que entram - linhas que saem
                        prev_avg_blue = (byte)Math.Round(prev_avg_blue + (double)blue_sum / area_reg);
                        prev_avg_green = (byte)Math.Round(prev_avg_green + (double)green_sum / area_reg);
                        prev_avg_red = (byte)Math.Round(prev_avg_red + (double)red_sum / area_reg);

                        aux_ptr = (dataPtr + y * widthstep + x * nChan);
                        //meter o valor de media no pixel da iamgem destino
                        //blue 
                        aux_ptr[0] = (byte)prev_avg_blue;
                        //green
                        aux_ptr[1] = (byte)prev_avg_green;
                        //red
                        aux_ptr[2] = (byte)prev_avg_red;

                    }

                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                    /// implementação da soluão C
                    /// primeiro passo da soluçao
                    /// 
                    // o canto superior esquerdo da imagem integral é igual ao da imagem original
                    // construir imagem integral para a primeira linha 
                    y = 0;
                    x = 0;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    img_int[y, x, 0] = aux_ptr[0];
                    img_int[y, x, 1] = aux_ptr[1];
                    img_int[y, x, 2] = aux_ptr[2];

                    // calcular imagem integral para a primeira linha
                    y = 0;
                    for (x = 1; x < width; x++)
                    {
                        // reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // acumular o valor da imagem integral no pixel anterior

                        blue_sum += img_int[y, (x - 1), 0];
                        green_sum += img_int[y, (x - 1), 1];
                        red_sum += img_int[y, (x - 1), 2];
                        // alterar o valor da imagem integral no pixel atual
                        aux_ptr = dataPtr_origem + y * widthstep + x * nChan; // apontador para a imagem original
                        img_int[y, x, 0] = aux_ptr[0] + blue_sum;
                        img_int[y, x, 1] = aux_ptr[1] + green_sum;
                        img_int[y, x, 2] = aux_ptr[2] + red_sum;
                    }
                    // construir imagem integral para a primeira coluna
                    x = 0;
                    // o canto superior esquerdo da imagem integral é igual ao da imagem original
                    for (y = 1; y < height; y++)
                    {
                        // reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // acumular o valor da imagem integral no pixel anterior
                        //aux_ptr_int = dataPtr_int + (y - 1) * widthstep_int + x * nChan;
                        blue_sum += img_int[(y - 1), x, 0];
                        green_sum += img_int[(y - 1), x, 1];
                        red_sum += img_int[(y - 1), x, 2];
                        // alterar o valor da imagem integral no pixel atual
                        //aux_ptr_int = dataPtr_int + y * widthstep_int + x * nChan;
                        aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                        img_int[y, x, 0] = aux_ptr[0] + blue_sum;
                        img_int[y, x, 1] = aux_ptr[1] + green_sum;
                        img_int[y, x, 2] = aux_ptr[2] + red_sum;
                    }
                    // construir imagem integral nos restantes pixeis
                    //ja calculamos a linha e coluna iniciais da imagem integral
                    //1º passo - calular a imagem integral de toda a imagem
                    for (y = 1; y < height; y++)
                    {
                        for (x = 1; x < width; x++)
                        {
                            // reset aos acumuladores
                            blue_sum = 0;
                            green_sum = 0;
                            red_sum = 0;
                            // acumular o valor da imagem integral no pixel da linha anterior > I (x, y-1)
                            //aux_ptr_int = dataPtr_int + (y - 1) * widthstep_int + x * nChan;
                            blue_sum += img_int[(y - 1), x, 0];
                            green_sum += img_int[(y - 1), x, 1];
                            red_sum += img_int[(y - 1), x, 2];
                            // acumular o valor da imagem integral no pixel da coluna anterior > I(x-1,y)
                            //aux_ptr_int = dataPtr_int + y * widthstep_int + (x - 1) * nChan;
                            blue_sum += img_int[y, (x - 1), 0];
                            green_sum += img_int[y, (x - 1), 1];
                            red_sum += img_int[y, (x - 1), 2];
                            // descontar o valor da imagem integral no pixel da coluna e linha anterior > I(x-1,y-1)
                            //aux_ptr_int = dataPtr_int + (y - 1) * widthstep_int + (x - 1) * nChan;
                            blue_sum -= img_int[(y - 1), (x - 1), 0];
                            green_sum -= img_int[(y - 1), (x - 1), 1];
                            red_sum -= img_int[(y - 1), (x - 1), 2];
                            // alterar o valor da imagem integral no pixel atual > f(x,y) + I (x, y-1) + I(x-1,y) - I(x-1,y-1)
                            //aux_ptr_int = dataPtr_int + y * widthstep_int + x * nChan;
                            aux_ptr = (dataPtr_origem + y * widthstep + x * nChan);
                            img_int[y, x, 0] = aux_ptr[0] + blue_sum;
                            img_int[y, x, 1] = aux_ptr[1] + green_sum;
                            img_int[y, x, 2] = aux_ptr[2] + red_sum;
                        }
                    }

                    // 2º passo aplicar o filtro de médias enquanto o filtro tiver pixeis das suas colunas fora da imagem (do lado esquerdo)
                    k = 1;
                    y = 1;
                    for (x = 1; x < overlap_size + 1; x++)
                    {

                        // reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                        //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan);
                        blue_sum += img_int[(y + overlap_size), (x + overlap_size), 0];
                        green_sum += img_int[(y + overlap_size), (x + overlap_size), 1];
                        red_sum += img_int[(y + overlap_size), (x + overlap_size), 2];
                        //acumular pixeis que estao fora da imagem e sao duplos do pixel do canto da imagem
                        aux_ptr = dataPtr_origem + (0) * widthstep + (0) * nChan;
                        blue_sum += ((overlap_size) ^ 2 - k) * aux_ptr[0];
                        green_sum += ((overlap_size) ^ 2 - k) * aux_ptr[1];
                        red_sum += ((overlap_size) ^ 2 - k) * aux_ptr[2];
                        //acumular pixeis que estao nas colunas fora da imagem e sao duplos dos pixeis da borda
                        for (j = 0; j < overlap_size + 1; j++)
                        {
                            aux_ptr = dataPtr_origem + (y + j) * widthstep + (0) * nChan;
                            blue_sum += ((overlap_size) - k) * aux_ptr[0];
                            green_sum += ((overlap_size) - k) * aux_ptr[1];
                            red_sum += ((overlap_size) - k) * aux_ptr[2];
                        }
                        //acumular pixeis que estao nas linhas fora da imagem e sao duplos dos pixeis da borda
                        for (j = 0; j < overlap_size + k; j++)
                        {
                            aux_ptr = dataPtr_origem + (0) * widthstep + (x + j) * nChan;
                            blue_sum += ((overlap_size) - 1) * aux_ptr[0];
                            green_sum += ((overlap_size) - 1) * aux_ptr[1];
                            red_sum += ((overlap_size) - 1) * aux_ptr[2];
                        }

                        // atualizar valor da imagem destino
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                        aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                        aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);

                        k++; // incrementar o K =  (numero de colunas da area do filtro que estao dentro da imagem  + 1 )
                    }
                    // repetir passo 2 enquanto o filtro tiver pixeis das suas linhas fora da imagem (do lado esquerdo)
                    k = 1;
                    x = 1;
                    for (y = 2; y < overlap_size + 1; y++)
                    {
                        // reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                        //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan);
                        blue_sum += img_int[(y + overlap_size), (x + overlap_size), 0];
                        green_sum += img_int[(y + overlap_size), (x + overlap_size), 1];
                        red_sum += img_int[(y + overlap_size), (x + overlap_size), 2];
                        //acumular pixeis que estao fora da imagem e sao duplos do pixel do canto da imagem
                        aux_ptr = dataPtr_origem + (0) * widthstep + (0) * nChan;
                        blue_sum += ((overlap_size) ^ 2 - k) * aux_ptr[0];
                        green_sum += ((overlap_size) ^ 2 - k) * aux_ptr[1];
                        red_sum += ((overlap_size) ^ 2 - k) * aux_ptr[2];
                        //acumular pixeis que estao nas colunas fora da imagem e sao duplos dos pixeis da borda
                        for (j = 0; j < overlap_size + k; j++)
                        {
                            aux_ptr = dataPtr_origem + (y + j) * widthstep + (0) * nChan;
                            blue_sum += ((overlap_size) - 1) * aux_ptr[0];
                            green_sum += ((overlap_size) - 1) * aux_ptr[1];
                            red_sum += ((overlap_size) - 1) * aux_ptr[2];
                        }
                        //acumular pixeis que estao nas linhas fora da imagem e sao duplos dos pixeis da borda
                        for (j = 0; j < overlap_size + 1; j++)
                        {
                            aux_ptr = dataPtr_origem + (0) * widthstep + (x + j) * nChan;
                            blue_sum += ((overlap_size) - k) * aux_ptr[0];
                            green_sum += ((overlap_size) - k) * aux_ptr[1];
                            red_sum += ((overlap_size) - k) * aux_ptr[2];
                        }

                        // atualizar valor da imagem destino
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                        aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                        aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);
                        k++;
                    }
                    // 3º passo - calcular o filtro de médias para as linhas do core, considerando que o filtro apenas tem as suas 
                    // primeiras linhas fora da imagem, e todas as colunas estao dentro, na borda de cima da imagem
                    k = 1;
                    for (y = 1; y < overlap_size; y++)
                    {
                        for (x = overlap_size + 1; x < width - overlap_size; x++)
                        {
                            // reset aos acumuladores 
                            blue_sum = 0;
                            green_sum = 0;
                            red_sum = 0;
                            // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                            //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan);
                            blue_sum += img_int[(y + overlap_size), (x + overlap_size), 0];
                            green_sum += img_int[(y + overlap_size), (x + overlap_size), 1];
                            red_sum += img_int[(y + overlap_size), (x + overlap_size), 2];
                            // descontar o valor da imagem integral no pixel da coluna anterior ao canto inferior direito da area do filtro
                            //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x - overlap_size-1) * nChan);
                            blue_sum -= img_int[(y + overlap_size), (x - overlap_size - 1), 0];
                            green_sum -= img_int[(y + overlap_size), (x - overlap_size - 1), 1];
                            red_sum -= img_int[(y + overlap_size), (x - overlap_size - 1), 2];
                            //acumular pixeis que estao nas linhas fora da imagem e sao duplos dos pixeis da borda
                            for (j = -overlap_size; j < overlap_size + 1; j++)
                            {
                                aux_ptr = dataPtr_origem + (0) * widthstep + (x + j) * nChan;
                                blue_sum += ((overlap_size) - k) * aux_ptr[0];
                                green_sum += ((overlap_size) - k) * aux_ptr[1];
                                red_sum += ((overlap_size) - k) * aux_ptr[2];
                            }
                            // atualizar valor da imagem destino
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                            aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                            aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);

                        }

                        k++;
                    }

                    // repetir 3º passo  considerando que o filtro apenas tem as suas
                    // primeiras colunas fora da imagem, e todas as linhas estao dentro, na borda esquerda da imagem
                    k = 1;
                    for (x = 1; x < overlap_size; x++)
                    {
                        for (y = overlap_size + 1; y < height - overlap_size; y++)
                        {
                            // reset aos acumuladores 
                            blue_sum = 0;
                            green_sum = 0;
                            red_sum = 0;
                            // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                            //aux_ptr_int = ( dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan );
                            blue_sum += img_int[(y + overlap_size), (x + overlap_size), 0];
                            green_sum += img_int[(y + overlap_size), (x + overlap_size), 1];
                            red_sum += img_int[(y + overlap_size), (x + overlap_size), 2];
                            // descontar o valor da imagem integral no pixel da coluna anterior ao canto inferior direito da area do filtro
                            //aux_ptr_int = (dataPtr_int + (y - overlap_size - 1) * widthstep_int + (x  + overlap_size) * nChan);
                            blue_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 0];
                            green_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 1];
                            red_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 2];
                            //acumular pixeis que estao nas linhas fora da imagem e sao duplos dos pixeis da borda
                            for (j = -overlap_size; j < overlap_size + 1; j++)
                            {
                                aux_ptr = dataPtr_origem + (y + j) * widthstep + (0) * nChan;
                                blue_sum += ((overlap_size) - k) * aux_ptr[0];
                                green_sum += ((overlap_size) - k) * aux_ptr[1];
                                red_sum += ((overlap_size) - k) * aux_ptr[2];
                            }
                            // atualizar valor da imagem destino
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                            aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                            aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);

                        }

                        k++;
                    }

                    // 4 º passo -aplicação do filtro de medias ao core da imagem
                    for (y = overlap_size + 1; y < height - overlap_size; y++)
                    {
                        for (x = overlap_size + 1; x < width - overlap_size; x++)
                        {
                            //reset aos acumuladores 
                            blue_sum = 0;
                            green_sum = 0;
                            red_sum = 0;
                            // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                            //aux_ptr_int = ( dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan);
                            blue_sum += img_int[(y + overlap_size), (x + overlap_size), 0];
                            green_sum += img_int[(y + overlap_size), (x + overlap_size), 1];
                            red_sum += img_int[(y + overlap_size), (x + overlap_size), 2];
                            // descontar o valor da imagem integral no pixel da coluna anterior ao canto inferior direito da area do filtro
                            //aux_ptr_int = ( dataPtr_int + (y - overlap_size - 1) * widthstep_int + (x + overlap_size) * nChan);
                            blue_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 0];
                            green_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 1];
                            red_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 2];
                            // descontar o valor da imagem integral no pixel da linha anterior ao canto superior direito da area do filtro
                            //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x - overlap_size-1) * nChan);
                            blue_sum -= img_int[(y + overlap_size), (x - overlap_size - 1), 0];
                            green_sum -= img_int[(y + overlap_size), (x - overlap_size - 1), 1];
                            red_sum -= img_int[(y + overlap_size), (x - overlap_size - 1), 2];
                            // acumular o valor da imagem integral no pixel da linha e coluna anterior ao canto superior esquerdo da area do filtro
                            //aux_ptr_int = (dataPtr_int + ( y - overlap_size - 1 ) * widthstep_int + (x - overlap_size - 1) * nChan);
                            blue_sum += img_int[(y - overlap_size - 1), (x - overlap_size - 1), 0];
                            green_sum += img_int[(y - overlap_size - 1), (x - overlap_size - 1), 1];
                            red_sum += img_int[(y - overlap_size - 1), (x - overlap_size - 1), 2];

                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                            aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                            aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);
                        }
                    }
                    // 5º passo - repetir o 2o passo ao aplicar o filtro de médias enquanto o filtro tiver pixeis das suas primeiras colunas e ultimas linhas 
                    // fora da imagem (do lado esquerdo, em baixo)

                    // tratar das colunas fora da imagem
                    k = 1;
                    y = height - overlap_size;
                    for (x = 1; x < overlap_size + 1; x++)
                    {

                        // reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                        //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan);
                        blue_sum += img_int[(y + overlap_size-1), (x + overlap_size), 0];
                        green_sum += img_int[(y + overlap_size-1), (x + overlap_size), 1];
                        red_sum += img_int[(y + overlap_size-1), (x + overlap_size), 2];
                        // subtrair valor da imagem integral do ppixel da linha e coluna anteriores ao canto da area do filtro
                        blue_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 0];
                        green_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 1];
                        red_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 2];
                        //acumular pixeis que estao fora da imagem e sao duplos do pixel do canto da imagem
                        aux_ptr = dataPtr_origem + (height-1) * widthstep + (0) * nChan;
                        blue_sum += ((overlap_size) ^ 2 - k) * aux_ptr[0];
                        green_sum += ((overlap_size) ^ 2 - k) * aux_ptr[1];
                        red_sum += ((overlap_size) ^ 2 - k) * aux_ptr[2];
                        //acumular pixeis que estao nas colunas fora da imagem e sao duplos dos pixeis da borda
                        for (j = -overlap_size; j < 1; j++)
                        {
                            aux_ptr = dataPtr_origem + (y + j) * widthstep + (0) * nChan;
                            blue_sum += ((overlap_size) - k) * aux_ptr[0];
                            green_sum += ((overlap_size) - k) * aux_ptr[1];
                            red_sum += ((overlap_size) - k) * aux_ptr[2];
                        }
                        //acumular pixeis que estao nas linhas fora da imagem e sao duplos dos pixeis da borda
                        for (j = 0; j < overlap_size + k; j++)
                        {
                            aux_ptr = dataPtr_origem + (height-1) * widthstep + (x + j) * nChan;
                            blue_sum += ((overlap_size) - 1) * aux_ptr[0];
                            green_sum += ((overlap_size) - 1) * aux_ptr[1];
                            red_sum += ((overlap_size) - 1) * aux_ptr[2];
                        }

                        // atualizar valor da imagem destino
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                        aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                        aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);

                        k++; // incrementar o K =  (numero de colunas da area do filtro que estao dentro da imagem  + 1 )
                    }

                    // tratar restantes colunas da imagem, na borda inferior, assumindo que o filtro tem todas as suas colunas dentro da imagem
                    // mas as linhas finais do elemento estruturante do filtro estao fora da imagem
                    k = 1;
                    for(y = height - overlap_size; y< height; y++)
                    {
                        for (x = overlap_size + 1 ; x < width-overlap_size; x++)
                        {

                            // reset aos acumuladores
                            blue_sum = 0;
                            green_sum = 0;
                            red_sum = 0;
                            // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                            //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan);
                            blue_sum += img_int[(y + overlap_size - k), (x + overlap_size), 0];
                            green_sum += img_int[(y + overlap_size - k), (x + overlap_size), 1];
                            red_sum += img_int[(y + overlap_size - k), (x + overlap_size), 2];
                            // subtrair valor da imagem integral do ppixel da linha e coluna anteriores ao canto superior direito da area do filtro
                            blue_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 0];
                            green_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 1];
                            red_sum -= img_int[(y - overlap_size - 1), (x + overlap_size), 2];
                            // subtrair valor da imagem integral do ppixel da linha e coluna anteriores ao canto inferioresquerdoda area do filtro
                            blue_sum -= img_int[(y + overlap_size - k), (x - overlap_size-1), 0];
                            green_sum -= img_int[(y + overlap_size - k), (x - overlap_size-1), 1];
                            red_sum -= img_int[(y + overlap_size - k), (x - overlap_size-1), 2];
                            // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                            //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan);
                            blue_sum += img_int[(y - overlap_size - 1), (x - overlap_size-1), 0];
                            green_sum += img_int[(y - overlap_size - 1), (x - overlap_size-1), 1];
                            red_sum += img_int[(y - overlap_size - 1), (x - overlap_size-1), 2];
                           
                            //acumular pixeis que estao nas linhas fora da imagem e sao duplos dos pixeis da borda
                            for (j = -overlap_size; j < overlap_size; j++)
                            {
                                aux_ptr = dataPtr_origem + (height - 1) * widthstep + (x + j) * nChan;
                                blue_sum += ((overlap_size) + k) * aux_ptr[0];
                                green_sum += ((overlap_size) + k) * aux_ptr[1];
                                red_sum += ((overlap_size) + k) * aux_ptr[2];
                            }

                            // atualizar valor da imagem destino
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                            aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                            aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);

                            
                        }
                        k++; // incrementar o K =  (numero de colunas da area do filtro que estao dentro da imagem  + 1 )
                    }

                    //tratar das restantes linhas entre x = 1 e x = overlap_size + 1 , em que o filtro tem linhas e colunas fora da imagem



                    // tratar da borda direita da imagem, sem contar com os cantos
                    // tratar restantes colunas da imagem, na borda inferior, assumindo que o filtro tem todas as suas colunas dentro da imagem
                    // mas as linhas finais do elemento estruturante do filtro estao fora da imagem
                    k = 1;
                    for (x = width - overlap_size; x < width; x++)
                    {
                        for (y = overlap_size + 1; y < height - overlap_size; y++)
                        {

                            // reset aos acumuladores
                            blue_sum = 0;
                            green_sum = 0;
                            red_sum = 0;
                            // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                            //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan);
                            blue_sum += img_int[(y + overlap_size ), (x + overlap_size - k), 0];
                            green_sum += img_int[(y + overlap_size ), (x + overlap_size - k), 1];
                            red_sum += img_int[(y + overlap_size ), (x + overlap_size - k), 2];
                            // subtrair valor da imagem integral do ppixel da linha e coluna anteriores ao canto superior direito da area do filtro
                            blue_sum -= img_int[(y - overlap_size - 1), (x + overlap_size - k), 0];
                            green_sum -= img_int[(y - overlap_size - 1), (x + overlap_size - k), 1];
                            red_sum -= img_int[(y - overlap_size - 1), (x + overlap_size - k), 2];
                            // subtrair valor da imagem integral do ppixel da linha e coluna anteriores ao canto inferioresquerdoda area do filtro
                            blue_sum -= img_int[(y + overlap_size ), (x - overlap_size - 1), 0];
                            green_sum -= img_int[(y + overlap_size), (x - overlap_size - 1), 1];
                            red_sum -= img_int[(y + overlap_size), (x - overlap_size - 1), 2];
                            // acumular o valor da imagem integral no pixel da linha e coluna seguinte do canto do filtro >  I (x+overlap, y+overlap)
                            //aux_ptr_int = (dataPtr_int + (y + overlap_size) * widthstep_int + (x + overlap_size) * nChan);
                            blue_sum += img_int[(y - overlap_size - 1), (x - overlap_size - 1), 0];
                            green_sum += img_int[(y - overlap_size - 1), (x - overlap_size - 1), 1];
                            red_sum += img_int[(y - overlap_size - 1), (x - overlap_size - 1), 2];

                            //acumular pixeis que estao nas linhas fora da imagem e sao duplos dos pixeis da borda
                            for (j = -overlap_size; j < overlap_size; j++)
                            {
                                aux_ptr = dataPtr_origem + (y + j) * widthstep + (width-1) * nChan;
                                blue_sum += ((overlap_size) + k) * aux_ptr[0];
                                green_sum += ((overlap_size) + k) * aux_ptr[1];
                                red_sum += ((overlap_size) + k) * aux_ptr[2];
                            }

                            // atualizar valor da imagem destino
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = (byte)Math.Round(blue_sum / area_reg);
                            aux_ptr[1] = (byte)Math.Round(green_sum / area_reg);
                            aux_ptr[2] = (byte)Math.Round(red_sum / area_reg);


                        }
                        k++; // incrementar o K =  (numero de colunas da area do filtro que estao dentro da imagem  + 1 )
                    }




                }


            }
        }

        /// <summary>
        /// Mean - applies an means filter using C solution for the interior of the image
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void NonUniform(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, float[,] matrix, float matrixWeight)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;
                //MIplImage m_pad = img_pad.MIplImage; // imagem para padding

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer(); // Pointer to the original image


                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y, j, k;
                byte* aux_ptr;
                float blue_sum, green_sum, red_sum;
                int kernel_height = 3;
                int kernel_width = 3;
                float coef = 0;
                if (nChan == 3)
                {
                    // verificar se o determinante e nulo
                    // se for, o filtro e separavel,
                    // se nao, o filtro nao e separavel

                    //PARA JA TAMOS A TRATAR OS FILTROS NAO UNIFORMES TODOS COMO NAO SEPARAVEIS


                    //tratar bordas - padding por duplicação de pixeis

                    //tratar do canto superior esquerdo - (x,y) = (0,0)
                    //reset aos acumuladores
                    blue_sum = 0;
                    green_sum = 0;
                    red_sum = 0;
                    //canto superior esquerdo
                    x = 0;
                    y = 0;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    //convolucionar os tres pixeis iguais ao do canto, com os respectivos pixeis do filtro
                    // e o canto tambem
                    for (j = 0; j < 2; j++)
                    {
                        for (k = 0; k < 2; k++)
                        {
                            blue_sum += matrix[j, k] * aux_ptr[0];
                            green_sum += matrix[j, k] * aux_ptr[1];
                            red_sum += matrix[j, k] * aux_ptr[2];
                        }
                    }


                    for (j = 0; j < 2; j++)
                    {
                        for (k = 0; k < 2; k++)
                        {
                            aux_ptr = dataPtr_origem + (y + j) * widthstep + (x + k) * nChan;
                            coef = matrix[j + 1, k + 1];
                            blue_sum += aux_ptr[0] * coef;
                            green_sum += aux_ptr[1] * coef;
                            red_sum += aux_ptr[2] * coef;

                        }
                    }
                    //subtrair o pixel do canto que ja foi adicionado antes
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    coef = matrix[1, 1];
                    blue_sum -= aux_ptr[0] * coef;
                    green_sum -= aux_ptr[1] * coef;
                    red_sum -= aux_ptr[2] * coef;

                    // duplicar o pixel encostado a borda 
                    // diretamente abaixo do pixel de canto
                    aux_ptr = dataPtr_origem + (y + 1) * widthstep + x * nChan;
                    coef = matrix[2, 0];
                    blue_sum += aux_ptr[0] * coef;
                    green_sum += aux_ptr[1] * coef;
                    red_sum += aux_ptr[2] * coef;

                    // diretamente a direita do pixel de canto
                    aux_ptr = dataPtr_origem + y * widthstep + (x + 1) * nChan;
                    coef = matrix[0, 2];
                    blue_sum += aux_ptr[0] * coef;
                    green_sum += aux_ptr[1] * coef;
                    red_sum += aux_ptr[2] * coef;
                    //atualizar intensidades RGB do pixel
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    aux_ptr[0] = blue_sum / matrixWeight > 255 ? (byte)255 : (blue_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(blue_sum / matrixWeight));
                    aux_ptr[1] = green_sum / matrixWeight > 255 ? (byte)255 : (green_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(green_sum / matrixWeight));
                    aux_ptr[2] = red_sum / matrixWeight > 255 ? (byte)255 : (red_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(red_sum / matrixWeight));

                    //tratar da linha de cima - y = 0;
                    y = 0;
                    for (x = 1; x < width - 1; x++)
                    {
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        //convolucionar os pixeis duplicados
                        for (k = -1; k < 2; k++)
                        {
                            aux_ptr = dataPtr_origem + y * widthstep + (x + k) * nChan;
                            coef = matrix[0, k + 1];
                            blue_sum += coef * aux_ptr[0];
                            green_sum += coef * aux_ptr[1];
                            red_sum += coef * aux_ptr[2];
                        }
                        // convolucionar os pixeis dentro da imagem
                        for (j = 0; j < kernel_height - 1; j++)
                        {
                            for (k = -1; k < kernel_width - 1; k++)
                            {
                                aux_ptr = dataPtr_origem + (y + j) * widthstep + (x + k) * nChan;
                                coef = matrix[j + 1, k + 1];
                                blue_sum += aux_ptr[0] * coef; // j+1 e k+1 para obrigar os indices a começar em 0,0
                                green_sum += aux_ptr[1] * coef;
                                red_sum += aux_ptr[2] * coef;
                            }
                        }
                        //atualizar intensidades RGB do pixel
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum / matrixWeight > 255 ? (byte)255 : (blue_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(blue_sum / matrixWeight));
                        aux_ptr[1] = green_sum / matrixWeight > 255 ? (byte)255 : (green_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(green_sum / matrixWeight));
                        aux_ptr[2] = red_sum / matrixWeight > 255 ? (byte)255 : (red_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(red_sum / matrixWeight));
                    }

                    // tratar do canto superior direito
                    //tratar do canto superior esquerdo - (x,y) = (width-1,0)
                    //reset aos acumuladores
                    blue_sum = 0;
                    green_sum = 0;
                    red_sum = 0;
                    //canto superior direito
                    x = width - 1;
                    y = 0;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    //convolucionar os tres pixeis iguais ao do canto, com os respectivos pixeis do filtro
                    // e o canto tambem
                    for (j = -1; j < kernel_height - 2; j++)
                    {
                        for (k = 0; k < kernel_width - 1; k++)
                        {
                            coef = matrix[j + 1, k + 1];
                            blue_sum += coef * aux_ptr[0];
                            green_sum += coef * aux_ptr[1];
                            red_sum += coef * aux_ptr[2];
                        }
                    }

                    //acumular os 3 pixeis dentro da imagem encostados ao pixel de canto, e o propiro pixel de 
                    //no caso de ter de implementar filtros maiores
                    for (j = 0; j < kernel_height - 1; j++)
                    {
                        for (k = -1; k < kernel_width - 2; k++)
                        {
                            aux_ptr = dataPtr_origem + (y + j) * widthstep + (x + k) * nChan;
                            coef = matrix[j + 1, k + 1];
                            blue_sum += aux_ptr[0] * coef;
                            green_sum += aux_ptr[1] * coef;
                            red_sum += aux_ptr[2] * coef;

                        }
                    }
                    //subtrair o pixel do canto que ja foi adicionado antes
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    coef = matrix[1, 1];
                    blue_sum -= aux_ptr[0] * coef;
                    green_sum -= aux_ptr[1] * coef;
                    red_sum -= aux_ptr[2] * coef;

                    // duplicar o pixel encostado a borda 
                    // diretamente abaixo do pixel de canto
                    aux_ptr = dataPtr_origem + (y + 1) * widthstep + x * nChan;
                    coef = matrix[2, 2];
                    blue_sum += aux_ptr[0] * coef;
                    green_sum += aux_ptr[1] * coef;
                    red_sum += aux_ptr[2] * coef;
                    //duplicar o pixel
                    // diretamente a esquerda do pixel de canto
                    aux_ptr = dataPtr_origem + y * widthstep + (x - 1) * nChan;
                    coef = matrix[0, 0];
                    blue_sum += aux_ptr[0] * coef;
                    green_sum += aux_ptr[1] * coef;
                    red_sum += aux_ptr[2] * coef;
                    //atualizar intensidades RGB do pixel
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    aux_ptr[0] = blue_sum / matrixWeight > 255 ? (byte)255 : (blue_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(blue_sum / matrixWeight));
                    aux_ptr[1] = green_sum / matrixWeight > 255 ? (byte)255 : (green_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(green_sum / matrixWeight));
                    aux_ptr[2] = red_sum / matrixWeight > 255 ? (byte)255 : (red_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(red_sum / matrixWeight));

                    //tratar da coluna da esquerda - x = 0;
                    x = 0;
                    for (y = 1; y < height - 1; y++)
                    {
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        //convolucionar os pixeis duplicados
                        for (j = -1; j < kernel_height - 1; j++)
                        {
                            aux_ptr = dataPtr_origem + (y + j) * widthstep + x * nChan;
                            coef = matrix[j + 1, 0];
                            blue_sum += coef * aux_ptr[0];
                            green_sum += coef * aux_ptr[1];
                            red_sum += coef * aux_ptr[2];
                        }
                        // convolucionar os pixeis dentro da imagem
                        for (j = -1; j < kernel_height - 1; j++)
                        {
                            for (k = 0; k < kernel_width - 1; k++)
                            {
                                aux_ptr = dataPtr_origem + (y + j) * widthstep + (x + k) * nChan;
                                coef = matrix[j + 1, k + 1];
                                blue_sum += aux_ptr[0] * coef; // j+1 e k+1 para obrigar os indices a começar em 0,0
                                green_sum += aux_ptr[1] * coef;
                                red_sum += aux_ptr[2] * coef;
                            }
                        }
                        //atualizar intensidades RGB do pixel
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum / matrixWeight > 255 ? (byte)255 : (blue_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(blue_sum / matrixWeight));
                        aux_ptr[1] = green_sum / matrixWeight > 255 ? (byte)255 : (green_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(green_sum / matrixWeight));
                        aux_ptr[2] = red_sum / matrixWeight > 255 ? (byte)255 : (red_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(red_sum / matrixWeight));
                    }

                    //tratar do pixel do canto inferior esquerdo > (x,y) = (0, height-1)

                    //reset aos acumuladores
                    blue_sum = 0;
                    green_sum = 0;
                    red_sum = 0;
                    //canto inferior esquerdo
                    x = 0;
                    y = height - 1;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    //convolucionar os tres pixeis iguais ao do canto, com os respectivos pixeis do filtro
                    // e o canto tambem
                    for (j = 0; j < kernel_height - 1; j++)
                    {
                        for (k = -1; k < kernel_width - 2; k++)
                        {
                            coef = matrix[j + 1, k + 1];
                            blue_sum += coef * aux_ptr[0];
                            green_sum += coef * aux_ptr[1];
                            red_sum += coef * aux_ptr[2];
                        }
                    }

                    //acumular os 3 pixeis dentro da imagem encostados ao pixel de canto, e o propiro pixel de 
                    //no caso de ter de implementar filtros maiores
                    for (j = -1; j < kernel_height - 2; j++)
                    {
                        for (k = 0; k < kernel_width - 1; k++)
                        {
                            aux_ptr = dataPtr_origem + (y + j) * widthstep + (x + k) * nChan;
                            coef = matrix[j + 1, k + 1];
                            blue_sum += aux_ptr[0] * coef;
                            green_sum += aux_ptr[1] * coef;
                            red_sum += aux_ptr[2] * coef;

                        }
                    }
                    //subtrair o pixel do canto que ja foi adicionado antes
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    coef = matrix[1, 1];
                    blue_sum -= aux_ptr[0] * coef;
                    green_sum -= aux_ptr[1] * coef;
                    red_sum -= aux_ptr[2] * coef;

                    // duplicar o pixel encostado a borda 
                    // diretamente acima do pixel de canto
                    aux_ptr = dataPtr_origem + (y - 1) * widthstep + x * nChan;
                    coef = matrix[0, 0];
                    blue_sum += aux_ptr[0] * coef;
                    green_sum += aux_ptr[1] * coef;
                    red_sum += aux_ptr[2] * coef;

                    //duplicar o pixel
                    // diretamente a direita do pixel de canto
                    aux_ptr = dataPtr_origem + y * widthstep + (x + 1) * nChan;
                    coef = matrix[2, 2];
                    blue_sum += aux_ptr[0] * coef;
                    green_sum += aux_ptr[1] * coef;
                    red_sum += aux_ptr[2] * coef;

                    //atualizar intensidades RGB do pixel
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    aux_ptr[0] = blue_sum / matrixWeight > 255 ? (byte)255 : (blue_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(blue_sum / matrixWeight));
                    aux_ptr[1] = green_sum / matrixWeight > 255 ? (byte)255 : (green_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(green_sum / matrixWeight));
                    aux_ptr[2] = red_sum / matrixWeight > 255 ? (byte)255 : (red_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(red_sum / matrixWeight));


                    //tratar da ultima linha - y = height-1
                    y = height - 1;
                    for (x = 1; x < width - 1; x++)
                    {
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        //convolucionar os pixeis duplicados
                        for (k = -1; k < kernel_width - 1; k++)
                        {
                            aux_ptr = dataPtr_origem + y * widthstep + (x + k) * nChan;
                            coef = matrix[2, k + 1];
                            blue_sum += coef * aux_ptr[0];
                            green_sum += coef * aux_ptr[1];
                            red_sum += coef * aux_ptr[2];
                        }
                        // convolucionar os pixeis dentro da imagem
                        for (j = -1; j < kernel_height - 2; j++)
                        {
                            for (k = -1; k < kernel_width - 1; k++)
                            {
                                aux_ptr = dataPtr_origem + (y + j) * widthstep + (x + k) * nChan;
                                coef = matrix[j + 1, k + 1];
                                blue_sum += aux_ptr[0] * coef; // j+1 e k+1 para obrigar os indices a começar em 0,0
                                green_sum += aux_ptr[1] * coef;
                                red_sum += aux_ptr[2] * coef;
                            }
                        }
                        //atualizar intensidades RGB do pixel
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum / matrixWeight > 255 ? (byte)255 : (blue_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(blue_sum / matrixWeight));
                        aux_ptr[1] = green_sum / matrixWeight > 255 ? (byte)255 : (green_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(green_sum / matrixWeight));
                        aux_ptr[2] = red_sum / matrixWeight > 255 ? (byte)255 : (red_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(red_sum / matrixWeight));
                    }

                    // canto inferior direito
                    //tratar do pixel do canto inferior direito > (x,y) = (width-1, height-1)

                    //reset aos acumuladores
                    blue_sum = 0;
                    green_sum = 0;
                    red_sum = 0;
                    //canto inferior esquerdo
                    x = width - 1;
                    y = height - 1;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    //convolucionar os tres pixeis iguais ao do canto, com os respectivos pixeis do filtro
                    // e o canto tambem
                    for (j = 0; j < kernel_height - 1; j++)
                    {
                        for (k = 0; k < kernel_width - 1; k++)
                        {
                            coef = matrix[j + 1, k + 1];
                            blue_sum += coef * aux_ptr[0];
                            green_sum += coef * aux_ptr[1];
                            red_sum += coef * aux_ptr[2];
                        }
                    }

                    //acumular os 3 pixeis dentro da imagem encostados ao pixel de canto, e o propiro pixel de 
                    //no caso de ter de implementar filtros maiores
                    for (j = -1; j < kernel_height - 2; j++)
                    {
                        for (k = -1; k < kernel_width - 2; k++)
                        {
                            aux_ptr = dataPtr_origem + (y + j) * widthstep + (x + k) * nChan;
                            coef = matrix[j + 1, k + 1];
                            blue_sum += aux_ptr[0] * coef;
                            green_sum += aux_ptr[1] * coef;
                            red_sum += aux_ptr[2] * coef;

                        }
                    }
                    //subtrair o pixel do canto que ja foi adicionado antes
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    coef = matrix[1, 1];
                    blue_sum -= aux_ptr[0] * coef;
                    green_sum -= aux_ptr[1] * coef;
                    red_sum -= aux_ptr[2] * coef;

                    // duplicar o pixel encostado a borda 
                    // diretamente acima do pixel de canto
                    aux_ptr = dataPtr_origem + (y - 1) * widthstep + x * nChan;
                    coef = matrix[0, 2];
                    blue_sum += aux_ptr[0] * coef;
                    green_sum += aux_ptr[1] * coef;
                    red_sum += aux_ptr[2] * coef;

                    //duplicar o pixel
                    // diretamente a esquerda do pixel de canto
                    aux_ptr = dataPtr_origem + y * widthstep + (x - 1) * nChan;
                    coef = matrix[2, 0];
                    blue_sum += aux_ptr[0] * coef;
                    green_sum += aux_ptr[1] * coef;
                    red_sum += aux_ptr[2] * coef;

                    //atualizar intensidades RGB do pixel
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    aux_ptr[0] = blue_sum / matrixWeight > 255 ? (byte)255 : (blue_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(blue_sum / matrixWeight));
                    aux_ptr[1] = green_sum / matrixWeight > 255 ? (byte)255 : (green_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(green_sum / matrixWeight));
                    aux_ptr[2] = red_sum / matrixWeight > 255 ? (byte)255 : (red_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(red_sum / matrixWeight));

                    //tratar da coluna da direita - x = width-1;
                    x = width - 1;
                    for (y = 1; y < height - 1; y++)
                    {
                        //reset aos acumuladores
                        blue_sum = 0;
                        green_sum = 0;
                        red_sum = 0;
                        //convolucionar os pixeis duplicados
                        for (j = -1; j < kernel_height - 1; j++)
                        {
                            aux_ptr = dataPtr_origem + (y + j) * widthstep + x * nChan;
                            coef = matrix[j + 1, 2];
                            blue_sum += coef * aux_ptr[0];
                            green_sum += coef * aux_ptr[1];
                            red_sum += coef * aux_ptr[2];
                        }
                        // convolucionar os pixeis dentro da imagem
                        for (j = -1; j < kernel_height - 1; j++)
                        {
                            for (k = -1; k < kernel_width - 2; k++)
                            {
                                aux_ptr = dataPtr_origem + (y + j) * widthstep + (x + k) * nChan;
                                coef = matrix[j + 1, k + 1];
                                blue_sum += aux_ptr[0] * coef; // j+1 e k+1 para obrigar os indices a começar em 0,0
                                green_sum += aux_ptr[1] * coef;
                                red_sum += aux_ptr[2] * coef;
                            }
                        }
                        //atualizar intensidades RGB do pixel
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum / matrixWeight > 255 ? (byte)255 : (blue_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(blue_sum / matrixWeight));
                        aux_ptr[1] = green_sum / matrixWeight > 255 ? (byte)255 : (green_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(green_sum / matrixWeight));
                        aux_ptr[2] = red_sum / matrixWeight > 255 ? (byte)255 : (red_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(red_sum / matrixWeight));
                    }

                    //core
                    for (y = 1; y < height - 1; y++) // para cada linha da img
                    {
                        for (x = 1; x < width - 1; x++) // para cada coluna da img
                        {
                            //limpar acumuladores
                            blue_sum = 0;
                            green_sum = 0;
                            red_sum = 0;
                            //convolucionar a imagem com o filtro centrado em (x,y)
                            for (j = -1; j < kernel_height - 1; j++) // para cada linha do kernel
                            {
                                for (k = -1; k < kernel_width - 1; k++) // para cada coluna do kernel
                                {
                                    // aceder ao pixel na posição(x, y) da img orginal
                                    //colocamos j e k a variar de j = -1 até j = 1, e k= -1 até k = 1 (no caso de filtros 3*3) para aceder
                                    //diretamente as posições que queremos da imagem original atraves dos indices do filtro de convoluçao
                                    aux_ptr = dataPtr_origem + (y + j) * widthstep + (x + k) * nChan;
                                    coef = matrix[j + 1, k + 1];
                                    blue_sum += aux_ptr[0] * coef; // j+1 e k+1 para obrigar os indices a começar em 0,0
                                    green_sum += aux_ptr[1] * coef;
                                    red_sum += aux_ptr[2] * coef;

                                }
                            }
                            //atualizar valor da imagem destino - saturando os se necessario
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = blue_sum / matrixWeight > 255 ? (byte)255 : (blue_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(blue_sum / matrixWeight));
                            aux_ptr[1] = green_sum / matrixWeight > 255 ? (byte)255 : (green_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(green_sum / matrixWeight));
                            aux_ptr[2] = red_sum / matrixWeight > 255 ? (byte)255 : (red_sum / matrixWeight < 0 ? (byte)0 : (byte)Math.Round(red_sum / matrixWeight));
                        }
                    }
                }
            }

        }
        /// <summary>
        /// Sobel - applies an contour detection / high contrast/ filter for images with low contrast - higher degradation of the image
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void Sobel(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;
                //MIplImage m_pad = img_pad.MIplImage; // imagem para padding

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer(); // Pointer to the original image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte* aux_ptr;
                int blue_sum, green_sum, red_sum;
                int blue_sx, green_sx, red_sx, blue_sy, green_sy, red_sy;
                sbyte coef = 0;
                sbyte[,] sx_m = { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } }; // sobel vertical - verificar contornos nas colunas
                sbyte[,] sy_m = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } }; // sobel horizontal - verificar contornos nas linhas
                blue_sum = 0;
                green_sum = 0;
                red_sum = 0;
                if (nChan == 3)
                {
                    // canto superior esquerdo
                    x = 0;
                    y = 0;
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    //convolucionar os tres pixeis iguais ao do canto, com os respectivos pixeis do filtro
                    // e o canto tambem
                    //realizar calculos relativos a Sx
                    //a
                    coef = sx_m[0, 0];
                    blue_sx = coef * aux_ptr[0];
                    green_sx = coef * aux_ptr[1];
                    red_sx = coef * aux_ptr[2];

                    // + 2d
                    coef = sx_m[1, 0];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // + g
                    aux_ptr = (dataPtr_origem + (y + 1) * widthstep + x * nChan);
                    coef = sx_m[2, 0];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - c
                    aux_ptr = (dataPtr_origem + y * widthstep + (x + 1) * nChan);
                    coef = sx_m[0, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - 2f
                    coef = sx_m[1, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - i
                    aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan);
                    coef = sx_m[2, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    //realizar calculos relativos a Sy
                    aux_ptr = (dataPtr_origem + y * widthstep + x * nChan);
                    // - a
                    coef = sy_m[0, 0];
                    blue_sy = coef * aux_ptr[0];
                    green_sy = coef * aux_ptr[1];
                    red_sy = coef * aux_ptr[2];

                    // - 2b
                    coef = sy_m[0, 1];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // - c
                    aux_ptr = (dataPtr_origem + y * widthstep + (x + 1) * nChan);
                    coef = sy_m[0, 2];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // +g
                    aux_ptr = (dataPtr_origem + (y + 1) * widthstep + x * nChan);
                    coef = sy_m[2, 0];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // + 2h
                    coef = sy_m[2, 1];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // + i
                    aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan);
                    coef = sy_m[2, 2];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    //calcular soma dos módulos
                    blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                    green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                    red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);

                    //atualizar valor da imagem destino - saturando os se necessario
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                    aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                    aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;



                    // canto superior direito
                    x = width - 1;
                    y = 0;
                    aux_ptr = dataPtr_origem + y * widthstep + (x - 1) * nChan;
                    //convolucionar os tres pixeis iguais ao do canto, com os respectivos pixeis do filtro
                    // e o canto tambem
                    //realizar calculos relativos a Sx

                    //a
                    coef = sx_m[0, 0];
                    blue_sx = coef * aux_ptr[0];
                    green_sx = coef * aux_ptr[1];
                    red_sx = coef * aux_ptr[2];

                    // + 2d
                    coef = sx_m[1, 0];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // + g
                    aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x - 1) * nChan);
                    coef = sx_m[2, 0];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - c
                    aux_ptr = (dataPtr_origem + y * widthstep + x * nChan);
                    coef = sx_m[0, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - 2f
                    coef = sx_m[1, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - i
                    aux_ptr = (dataPtr_origem + (y + 1) * widthstep + x * nChan);
                    coef = sx_m[2, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    //realizar calculos relativos a Sy

                    // - a
                    aux_ptr = (dataPtr_origem + y * widthstep + (x - 1) * nChan);
                    coef = sy_m[0, 0];
                    blue_sy = coef * aux_ptr[0];
                    green_sy = coef * aux_ptr[1];
                    red_sy = coef * aux_ptr[2];

                    // - 2b
                    aux_ptr = (dataPtr_origem + y * widthstep + x * nChan);
                    coef = sy_m[0, 1];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // - c
                    coef = sy_m[0, 2];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // +g
                    aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x - 1) * nChan);
                    coef = sy_m[2, 0];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // + 2h
                    aux_ptr = (dataPtr_origem + (y + 1) * widthstep + x * nChan);
                    coef = sy_m[2, 1];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // + i
                    coef = sy_m[2, 2];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    //calcular soma dos módulos
                    blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                    green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                    red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);

                    //atualizar valor da imagem destino - saturando os se necessario
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                    aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                    aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;

                    // canto inferior esquerdo
                    x = 0;
                    y = height - 1;
                    aux_ptr = dataPtr_origem + (y - 1) * widthstep + x * nChan;
                    //convolucionar os tres pixeis iguais ao do canto, com os respectivos pixeis do filtro
                    //e o canto tambem
                    //realizar calculos relativos a Sx

                    //a
                    coef = sx_m[0, 0];
                    blue_sx = coef * aux_ptr[0];
                    green_sx = coef * aux_ptr[1];
                    red_sx = coef * aux_ptr[2];

                    // + 2d
                    aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                    coef = sx_m[1, 0];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // + g
                    coef = sx_m[2, 0];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - c
                    aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x + 1) * nChan);
                    coef = sx_m[0, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - 2f
                    aux_ptr = (dataPtr_origem + y * widthstep + (x + 1) * nChan);
                    coef = sx_m[1, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - i
                    coef = sx_m[2, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    //realizar calculos relativos a Sy

                    // - a
                    aux_ptr = (dataPtr_origem + (y - 1) * widthstep + x * nChan);
                    coef = sy_m[0, 0];
                    blue_sy = coef * aux_ptr[0];
                    green_sy = coef * aux_ptr[1];
                    red_sy = coef * aux_ptr[2];

                    // - 2b
                    coef = sy_m[0, 1];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // - c
                    aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x + 1) * nChan);
                    coef = sy_m[0, 2];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // +g
                    aux_ptr = (dataPtr_origem + y * widthstep + x * nChan);
                    coef = sy_m[2, 0];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // + 2h
                    coef = sy_m[2, 1];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // + i
                    aux_ptr = (dataPtr_origem + y * widthstep + (x + 1) * nChan);
                    coef = sy_m[2, 2];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    //calcular soma dos módulos
                    blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                    green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                    red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);

                    //atualizar valor da imagem destino - saturando os se necessario
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                    aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                    aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;

                    // canto inferior direito
                    x = width - 1;
                    y = height - 1;
                    aux_ptr = dataPtr_origem + (y - 1) * widthstep + (x - 1) * nChan;
                    //convolucionar os tres pixeis iguais ao do canto, com os respectivos pixeis do filtro
                    // e o canto tambem
                    //realizar calculos relativos a Sx

                    //a
                    coef = sx_m[0, 0];
                    blue_sx = coef * aux_ptr[0];
                    green_sx = coef * aux_ptr[1];
                    red_sx = coef * aux_ptr[2];

                    // + 2d
                    aux_ptr = dataPtr_origem + y * widthstep + (x - 1) * nChan;
                    coef = sx_m[1, 0];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // + g
                    coef = sx_m[2, 0];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - c
                    aux_ptr = (dataPtr_origem + (y - 1) * widthstep + x * nChan);
                    coef = sx_m[0, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - 2f
                    aux_ptr = (dataPtr_origem + y * widthstep + x * nChan);
                    coef = sx_m[1, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    // - i
                    coef = sx_m[2, 2];
                    blue_sx += coef * aux_ptr[0];
                    green_sx += coef * aux_ptr[1];
                    red_sx += coef * aux_ptr[2];

                    //realizar calculos relativos a Sy

                    // - a
                    aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x - 1) * nChan);
                    coef = sy_m[0, 0];
                    blue_sy = coef * aux_ptr[0];
                    green_sy = coef * aux_ptr[1];
                    red_sy = coef * aux_ptr[2];

                    // - 2b
                    aux_ptr = (dataPtr_origem + (y - 1) * widthstep + x * nChan);
                    coef = sy_m[0, 1];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // - c
                    coef = sy_m[0, 2];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // +g
                    aux_ptr = (dataPtr_origem + y * widthstep + (x - 1) * nChan);
                    coef = sy_m[2, 0];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // + 2h
                    aux_ptr = (dataPtr_origem + y * widthstep + x * nChan);
                    coef = sy_m[2, 1];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    // + i
                    coef = sy_m[2, 2];
                    blue_sy += coef * aux_ptr[0];
                    green_sy += coef * aux_ptr[1];
                    red_sy += coef * aux_ptr[2];

                    //calcular soma dos módulos
                    blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                    green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                    red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);

                    //atualizar valor da imagem destino - saturando os se necessario
                    aux_ptr = dataPtr + y * widthstep + x * nChan;
                    aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                    aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                    aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;

                    // borda da coluna da esquerda
                    x = 0;
                    y = 1;
                    for (y = 1; y < height - 1; y++)
                    {
                        //realizar calculos relativos a Sx
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x) * nChan);
                        //a
                        coef = sx_m[0, 0];
                        blue_sx = coef * aux_ptr[0];
                        green_sx = coef * aux_ptr[1];
                        red_sx = coef * aux_ptr[2];

                        // + 2d
                        aux_ptr = (dataPtr_origem + y * widthstep + (x) * nChan);
                        coef = sx_m[1, 0];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        // + g
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x) * nChan);
                        coef = sx_m[2, 0];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        // - c
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x + 1) * nChan);
                        coef = sx_m[0, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];
                        // - 2f
                        aux_ptr = (dataPtr_origem + y * widthstep + (x + 1) * nChan);
                        coef = sx_m[1, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];
                        // - i
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan);
                        coef = sx_m[2, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        //realizar calculos relativos a Sy
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x) * nChan);
                        // - a
                        coef = sy_m[0, 0];
                        blue_sy = coef * aux_ptr[0];
                        green_sy = coef * aux_ptr[1];
                        red_sy = coef * aux_ptr[2];

                        // - 2b
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + x * nChan);
                        coef = sy_m[0, 1];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        // - c
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x + 1) * nChan);
                        coef = sy_m[0, 2];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        // +g
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x) * nChan);
                        coef = sy_m[2, 0];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];
                        // + 2h
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + x * nChan);
                        coef = sy_m[2, 1];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];
                        // + i
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan);
                        coef = sy_m[2, 2];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        //calcular soma dos módulos
                        blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                        green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                        red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);

                        //atualizar valor da imagem destino - saturando os se necessario
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                        aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                        aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                    }

                    // borda da ultima coluna - coluna da direita
                    x = width - 1;
                    //y = 1;
                    for (y = 1; y < height - 1; y++)
                    {
                        //realizar calculos relativos a Sx
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x - 1) * nChan);
                        //a
                        coef = sx_m[0, 0];
                        blue_sx = coef * aux_ptr[0];
                        green_sx = coef * aux_ptr[1];
                        red_sx = coef * aux_ptr[2];

                        // + 2d
                        aux_ptr = (dataPtr_origem + y * widthstep + (x - 1) * nChan);
                        coef = sx_m[1, 0];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        // + g
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x - 1) * nChan);
                        coef = sx_m[2, 0];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        // - c
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x) * nChan);
                        coef = sx_m[0, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];
                        // - 2f
                        aux_ptr = (dataPtr_origem + y * widthstep + (x) * nChan);
                        coef = sx_m[1, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];
                        // - i
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x) * nChan);
                        coef = sx_m[2, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        //realizar calculos relativos a Sy
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x - 1) * nChan);
                        // - a
                        coef = sy_m[0, 0];
                        blue_sy = coef * aux_ptr[0];
                        green_sy = coef * aux_ptr[1];
                        red_sy = coef * aux_ptr[2];

                        // - 2b
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + x * nChan);
                        coef = sy_m[0, 1];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        // - c
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x) * nChan);
                        coef = sy_m[0, 2];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        // +g
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x - 1) * nChan);
                        coef = sy_m[2, 0];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];
                        // + 2h
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + x * nChan);
                        coef = sy_m[2, 1];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];
                        // + i
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x) * nChan);
                        coef = sy_m[2, 2];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        //calcular soma dos módulos
                        blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                        green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                        red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);

                        //atualizar valor da imagem destino - saturando os se necessario
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                        aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                        aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                    }

                    // borda da primeira linha
                    y = 0;
                    //x = 1 ;
                    for (x = 1; x < width - 1; x++)
                    {
                        //realizar calculos relativos a Sx
                        aux_ptr = (dataPtr_origem + (y) * widthstep + (x - 1) * nChan);
                        //a
                        coef = sx_m[0, 0];
                        blue_sx = coef * aux_ptr[0];
                        green_sx = coef * aux_ptr[1];
                        red_sx = coef * aux_ptr[2];

                        // + 2d
                        aux_ptr = (dataPtr_origem + y * widthstep + (x - 1) * nChan);
                        coef = sx_m[1, 0];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        // + g
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x - 1) * nChan);
                        coef = sx_m[2, 0];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        // - c
                        aux_ptr = (dataPtr_origem + (y) * widthstep + (x + 1) * nChan);
                        coef = sx_m[0, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];
                        // - 2f
                        aux_ptr = (dataPtr_origem + y * widthstep + (x + 1) * nChan);
                        coef = sx_m[1, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];
                        // - i
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan);
                        coef = sx_m[2, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        //realizar calculos relativos a Sy
                        aux_ptr = (dataPtr_origem + (y) * widthstep + (x - 1) * nChan);
                        // - a
                        coef = sy_m[0, 0];
                        blue_sy = coef * aux_ptr[0];
                        green_sy = coef * aux_ptr[1];
                        red_sy = coef * aux_ptr[2];

                        // - 2b
                        aux_ptr = (dataPtr_origem + (y) * widthstep + x * nChan);
                        coef = sy_m[0, 1];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        // - c
                        aux_ptr = (dataPtr_origem + (y) * widthstep + (x + 1) * nChan);
                        coef = sy_m[0, 2];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        // +g
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x - 1) * nChan);
                        coef = sy_m[2, 0];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];
                        // + 2h
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + x * nChan);
                        coef = sy_m[2, 1];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];
                        // + i
                        aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan);
                        coef = sy_m[2, 2];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        //calcular soma dos módulos
                        blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                        green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                        red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);

                        //atualizar valor da imagem destino - saturando os se necessario
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                        aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                        aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                    }

                    //borda da ultima linha
                    y = height - 1;

                    for (x = 1; x < width - 1; x++)
                    {
                        //realizar calculos relativos a Sx
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x - 1) * nChan);
                        //a
                        coef = sx_m[0, 0];
                        blue_sx = coef * aux_ptr[0];
                        green_sx = coef * aux_ptr[1];
                        red_sx = coef * aux_ptr[2];

                        // + 2d
                        aux_ptr = (dataPtr_origem + y * widthstep + (x - 1) * nChan);
                        coef = sx_m[1, 0];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        // + g
                        aux_ptr = (dataPtr_origem + (y) * widthstep + (x - 1) * nChan);
                        coef = sx_m[2, 0];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        // - c
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x + 1) * nChan);
                        coef = sx_m[0, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];
                        // - 2f
                        aux_ptr = (dataPtr_origem + y * widthstep + (x + 1) * nChan);
                        coef = sx_m[1, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];
                        // - i
                        aux_ptr = (dataPtr_origem + (y) * widthstep + (x + 1) * nChan);
                        coef = sx_m[2, 2];
                        blue_sx += coef * aux_ptr[0];
                        green_sx += coef * aux_ptr[1];
                        red_sx += coef * aux_ptr[2];

                        //realizar calculos relativos a Sy
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x - 1) * nChan);
                        // - a
                        coef = sy_m[0, 0];
                        blue_sy = coef * aux_ptr[0];
                        green_sy = coef * aux_ptr[1];
                        red_sy = coef * aux_ptr[2];

                        // - 2b
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + x * nChan);
                        coef = sy_m[0, 1];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        // - c
                        aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x + 1) * nChan);
                        coef = sy_m[0, 2];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        // +g
                        aux_ptr = (dataPtr_origem + (y) * widthstep + (x - 1) * nChan);
                        coef = sy_m[2, 0];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];
                        // + 2h
                        aux_ptr = (dataPtr_origem + (y) * widthstep + x * nChan);
                        coef = sy_m[2, 1];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];
                        // + i
                        aux_ptr = (dataPtr_origem + (y) * widthstep + (x + 1) * nChan);
                        coef = sy_m[2, 2];
                        blue_sy += coef * aux_ptr[0];
                        green_sy += coef * aux_ptr[1];
                        red_sy += coef * aux_ptr[2];

                        //calcular soma dos módulos
                        blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                        green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                        red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);

                        //atualizar valor da imagem destino - saturando os se necessario
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                        aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                        aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                    }

                    //core
                    for (y = 1; y < height - 1; y++) // para cada linha da img
                    {
                        for (x = 1; x < width - 1; x++) // para cada coluna da img
                        {

                            //realizar calculos relativos a Sx
                            aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x - 1) * nChan);
                            //a
                            coef = sx_m[0, 0];
                            blue_sx = coef * aux_ptr[0];
                            green_sx = coef * aux_ptr[1];
                            red_sx = coef * aux_ptr[2];

                            // + 2d
                            aux_ptr = (dataPtr_origem + y * widthstep + (x - 1) * nChan);
                            coef = sx_m[1, 0];
                            blue_sx += coef * aux_ptr[0];
                            green_sx += coef * aux_ptr[1];
                            red_sx += coef * aux_ptr[2];

                            // + g
                            aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x - 1) * nChan);
                            coef = sx_m[2, 0];
                            blue_sx += coef * aux_ptr[0];
                            green_sx += coef * aux_ptr[1];
                            red_sx += coef * aux_ptr[2];

                            // - c
                            aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x + 1) * nChan);
                            coef = sx_m[0, 2];
                            blue_sx += coef * aux_ptr[0];
                            green_sx += coef * aux_ptr[1];
                            red_sx += coef * aux_ptr[2];
                            // - 2f
                            aux_ptr = (dataPtr_origem + y * widthstep + (x + 1) * nChan);
                            coef = sx_m[1, 2];
                            blue_sx += coef * aux_ptr[0];
                            green_sx += coef * aux_ptr[1];
                            red_sx += coef * aux_ptr[2];
                            // - i
                            aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan);
                            coef = sx_m[2, 2];
                            blue_sx += coef * aux_ptr[0];
                            green_sx += coef * aux_ptr[1];
                            red_sx += coef * aux_ptr[2];

                            //realizar calculos relativos a Sy
                            aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x - 1) * nChan);
                            // - a
                            coef = sy_m[0, 0];
                            blue_sy = coef * aux_ptr[0];
                            green_sy = coef * aux_ptr[1];
                            red_sy = coef * aux_ptr[2];

                            // - 2b
                            aux_ptr = (dataPtr_origem + (y - 1) * widthstep + x * nChan);
                            coef = sy_m[0, 1];
                            blue_sy += coef * aux_ptr[0];
                            green_sy += coef * aux_ptr[1];
                            red_sy += coef * aux_ptr[2];

                            // - c
                            aux_ptr = (dataPtr_origem + (y - 1) * widthstep + (x + 1) * nChan);
                            coef = sy_m[0, 2];
                            blue_sy += coef * aux_ptr[0];
                            green_sy += coef * aux_ptr[1];
                            red_sy += coef * aux_ptr[2];

                            // +g
                            aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x - 1) * nChan);
                            coef = sy_m[2, 0];
                            blue_sy += coef * aux_ptr[0];
                            green_sy += coef * aux_ptr[1];
                            red_sy += coef * aux_ptr[2];
                            // + 2h
                            aux_ptr = (dataPtr_origem + (y + 1) * widthstep + x * nChan);
                            coef = sy_m[2, 1];
                            blue_sy += coef * aux_ptr[0];
                            green_sy += coef * aux_ptr[1];
                            red_sy += coef * aux_ptr[2];
                            // + i
                            aux_ptr = (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan);
                            coef = sy_m[2, 2];
                            blue_sy += coef * aux_ptr[0];
                            green_sy += coef * aux_ptr[1];
                            red_sy += coef * aux_ptr[2];

                            //calcular soma dos módulos
                            blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                            green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                            red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);

                            //atualizar valor da imagem destino - saturando os se necessario
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                            aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                            aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Diferentiation - applies an contour detection /low contrast/ filter using a Diferentiation filter for high contrast images
        /// the advantage against sobel relies on the lower size of the used kernel
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void Diferentiation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;
                //MIplImage m_pad = img_pad.MIplImage; // imagem para padding

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer(); // Pointer to the original image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte* aux_ptr;
                int blue_sum, green_sum, red_sum;
                int blue_sx, green_sx, red_sx, blue_sy, green_sy, red_sy;
                blue_sum = 0;
                green_sum = 0;
                red_sum = 0;
                if (nChan == 3)
                {
                    // ultima coluna
                    //y = 0;
                    x = width - 1;
                    for (y = 0; y < height - 1; y++)
                    {
                        // aceder ao pixel na posição(x, y) da img orginal
                        //colocamos j e k a variar de j = -1 até j = 1, e k= -1 até k = 1 (no caso de filtros 3*3) para aceder
                        //diretamente as posições que queremos da imagem original atraves dos indices do filtro de convoluçao

                        aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                        //em Sx
                        // Sx e sempre nulo na ultima coluna
                        //em Sy
                        blue_sy = aux_ptr[0] - (dataPtr_origem + (y + 1) * widthstep + x * nChan)[0];
                        green_sy = aux_ptr[1] - (dataPtr_origem + (y + 1) * widthstep + x * nChan)[1];
                        red_sy = aux_ptr[2] - (dataPtr_origem + (y + 1) * widthstep + x * nChan)[2];
                        //calcular modulo 
                        blue_sum = Math.Abs(blue_sy);
                        green_sum = Math.Abs(green_sy);
                        red_sum = Math.Abs(red_sy);
                        //atualizar valor da imagem destino - saturando os se necessario
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                        aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                        aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                    }

                    // ultima linha
                    //y = 0;
                    y = height - 1;
                    for (x = 0; x < width - 1; x++)
                    {
                        // aceder ao pixel na posição(x, y) da img orginal
                        //colocamos j e k a variar de j = -1 até j = 1, e k= -1 até k = 1 (no caso de filtros 3*3) para aceder
                        //diretamente as posições que queremos da imagem original atraves dos indices do filtro de convoluçao

                        aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                        //em Sx
                        blue_sx = aux_ptr[0] - (dataPtr_origem + y * widthstep + (x + 1) * nChan)[0];
                        green_sx = aux_ptr[1] - (dataPtr_origem + y * widthstep + (x + 1) * nChan)[1];
                        red_sx = aux_ptr[2] - (dataPtr_origem + y * widthstep + (x + 1) * nChan)[2];
                        //em Sy
                        // sy e sewmpre nulo na ultima linha
                        //calcular modulo 
                        blue_sum = Math.Abs(blue_sx);
                        green_sum = Math.Abs(green_sx);
                        red_sum = Math.Abs(red_sx);
                        //atualizar valor da imagem destino - saturando os se necessario
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                        aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                        aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                    }

                    // canto inferior direito
                    y = height - 1;
                    x = width - 1;
                    aux_ptr = dataPtr + y * widthstep + x * nChan; // na diferenciacao, o canto inferiror direito e sempre preto
                    aux_ptr[0] = 0;
                    aux_ptr[1] = 0;
                    aux_ptr[2] = 0;

                    //core -diferenciação usual
                    for (y = 0; y < height - 1; y++) // para cada linha da img
                    {
                        for (x = 0; x < width - 1; x++) // para cada coluna da img
                        {

                            // aceder ao pixel na posição(x, y) da img orginal
                            //colocamos j e k a variar de j = -1 até j = 1, e k= -1 até k = 1 (no caso de filtros 3*3) para aceder
                            //diretamente as posições que queremos da imagem original atraves dos indices do filtro de convoluçao

                            aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                            //em Sx
                            blue_sx = aux_ptr[0] - (dataPtr_origem + y * widthstep + (x + 1) * nChan)[0];
                            green_sx = aux_ptr[1] - (dataPtr_origem + y * widthstep + (x + 1) * nChan)[1];
                            red_sx = aux_ptr[2] - (dataPtr_origem + y * widthstep + (x + 1) * nChan)[2];
                            //em Sy
                            blue_sy = aux_ptr[0] - (dataPtr_origem + (y + 1) * widthstep + x * nChan)[0];
                            green_sy = aux_ptr[1] - (dataPtr_origem + (y + 1) * widthstep + x * nChan)[1];
                            red_sy = aux_ptr[2] - (dataPtr_origem + (y + 1) * widthstep + x * nChan)[2];
                            //calcular modulo 
                            blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                            green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                            red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);
                            //atualizar valor da imagem destino - saturando os se necessario
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                            aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                            aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Roberts - applies an contour detection filter using a Roberts Diferentiation filter for high contrast images
        /// the advantage against sobel and Diferentiation relies on the lower size of the used kernel (vs. Sobel) and higher
        /// preservation level of  contrast of the contour borders detected by the filter (vs.Diferentiation)
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        /// Diferenciação com solução de roberts - i(x,y) = |f(x,y)-f(x+1,y+1)| + |f(x,y+1)-f(x+1,y)|
        public static void Roberts(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;
                //MIplImage m_pad = img_pad.MIplImage; // imagem para padding

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer(); // Pointer to the original image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte* aux_ptr;
                int blue_sum, green_sum, red_sum;
                int blue_sx, green_sx, red_sx, blue_sy, green_sy, red_sy;
                blue_sum = 0;
                green_sum = 0;
                red_sum = 0;

                if (nChan == 3)
                {
                    //tratar da ultima coluna com ocaso especial
                    x = width - 1;
                    for (y = 0; y < height - 1; y++)
                    {
                        // aceder ao pixel na posição(x, y) da img orginal
                        //colocamos j e k a variar de j = -1 até j = 1, e k= -1 até k = 1 (no caso de filtros 3*3) para aceder
                        //diretamente as posições que queremos da imagem original atraves dos indices do filtro de convoluçao

                        aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                        //em Sx
                        blue_sx = aux_ptr[0] - (dataPtr_origem + (y + 1) * widthstep + (x) * nChan)[0];
                        green_sx = aux_ptr[1] - (dataPtr_origem + (y + 1) * widthstep + (x) * nChan)[1];
                        red_sx = aux_ptr[2] - (dataPtr_origem + (y + 1) * widthstep + (x) * nChan)[2];

                        //calcular modulo - 
                        blue_sum = 2 * Math.Abs(blue_sx);
                        green_sum = 2 * Math.Abs(green_sx);
                        red_sum = 2 * Math.Abs(red_sx);
                        //atualizar valor da imagem destino - saturando os se necessario
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                        aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                        aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                    }


                    //tratar da ultima linha com ocaso especial
                    y = height - 1;
                    for (x = 0; x < width - 1; x++)
                    {
                        // aceder ao pixel na posição(x, y) da img orginal
                        //colocamos j e k a variar de j = -1 até j = 1, e k= -1 até k = 1 (no caso de filtros 3*3) para aceder
                        //diretamente as posições que queremos da imagem original atraves dos indices do filtro de convoluçao

                        aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                        //em Sx
                        blue_sx = aux_ptr[0] - (dataPtr_origem + (y) * widthstep + (x + 1) * nChan)[0];
                        green_sx = aux_ptr[1] - (dataPtr_origem + (y) * widthstep + (x + 1) * nChan)[1];
                        red_sx = aux_ptr[2] - (dataPtr_origem + (y) * widthstep + (x + 1) * nChan)[2];

                        //calcular modulo 
                        blue_sum = 2 * Math.Abs(blue_sx);
                        green_sum = 2 * Math.Abs(green_sx);
                        red_sum = 2 * Math.Abs(red_sx);
                        //atualizar valor da imagem destino - saturando os se necessario
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                        aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                        aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                    }

                    // canto inferior direito
                    y = height - 1;
                    x = width - 1;
                    aux_ptr = dataPtr + y * widthstep + x * nChan; // no roberts, o canto inferior direito e sempre preto
                    aux_ptr[0] = 0;
                    aux_ptr[1] = 0;
                    aux_ptr[2] = 0;

                    //core -diferenciação usual
                    for (y = 0; y < height - 1; y++) // para cada linha da img
                    {
                        for (x = 0; x < width - 1; x++) // para cada coluna da img
                        {

                            // aceder ao pixel na posição(x, y) da img orginal
                            //colocamos j e k a variar de j = -1 até j = 1, e k= -1 até k = 1 (no caso de filtros 3*3) para aceder
                            //diretamente as posições que queremos da imagem original atraves dos indices do filtro de convoluçao

                            aux_ptr = dataPtr_origem + y * widthstep + x * nChan;
                            //em Sx
                            blue_sx = aux_ptr[0] - (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan)[0];
                            green_sx = aux_ptr[1] - (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan)[1];
                            red_sx = aux_ptr[2] - (dataPtr_origem + (y + 1) * widthstep + (x + 1) * nChan)[2];

                            //em Sy
                            aux_ptr = dataPtr_origem + (y + 1) * widthstep + x * nChan;
                            blue_sy = aux_ptr[0] - (dataPtr_origem + y * widthstep + (x + 1) * nChan)[0];
                            green_sy = aux_ptr[1] - (dataPtr_origem + y * widthstep + (x + 1) * nChan)[1];
                            red_sy = aux_ptr[2] - (dataPtr_origem + y * widthstep + (x + 1) * nChan)[2];
                            //calcular modulo 
                            blue_sum = Math.Abs(blue_sy) + Math.Abs(blue_sx);
                            green_sum = Math.Abs(green_sy) + Math.Abs(green_sx);
                            red_sum = Math.Abs(red_sy) + Math.Abs(red_sx);
                            //atualizar valor da imagem destino - saturando os se necessario
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = blue_sum > 255 ? (byte)255 : (byte)blue_sum;
                            aux_ptr[1] = green_sum > 255 ? (byte)255 : (byte)green_sum;
                            aux_ptr[2] = red_sum > 255 ? (byte)255 : (byte)red_sum;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Median - 
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void Median(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;
                //MIplImage m_pad = img_pad.MIplImage; // imagem para padding

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer(); // Pointer to the original image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y, j, k;
                byte* aux_ptr;
                int blue_sum, green_sum, red_sum;
                if (nChan == 3)
                {

                    // CALCULAR MEDIANA EM 3 DIMENSOES USANDO FILTROS 3 X 3 NOS 3 CANAIS DE COR

                }
                imgCopy.SmoothMedian(3).CopyTo(img);
            }
        }

        /// <summary>
        /// Histogram_Gray - 
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static int[] Histogram_Gray(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                //MIplImage m_pad = img_pad.MIplImage; // imagem para padding

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte red, green, blue, gray;
                red = 0;
                blue = 0;
                green = 0;
                gray = 0;
                int[] hist_arr = new int[256];

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais rapido
                            red = dataPtr[0]; // R
                            green = dataPtr[1]; // G
                            blue = dataPtr[2]; // B

                            // convert to gray
                            gray = (byte)Math.Round(((int)blue + green + red) / 3.0);
                            hist_arr[gray]++; // para cada intensidade de cinzentos, incrementar o numero de pixeis que verificam essa intensidade

                            dataPtr += nChan; // avançar o apontador para o proximo pixel

                        }

                        dataPtr += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }
                }
                return hist_arr;
            }
        }

        /// <summary>
        /// Histogram_RGB - 
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static int[,] Histogram_RGB(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte red, green, blue;
                red = 0;
                blue = 0;
                green = 0;
                int[,] hist_arr = new int[3, 256];

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais rapido
                            blue = dataPtr[0]; // b
                            green = dataPtr[1]; // g
                            red = dataPtr[2]; // r

                            hist_arr[2, red]++; // para cada intensidade de cinzentos, incrementar o numero de pixeis que verificam essa intensidade
                            hist_arr[1, green]++;
                            hist_arr[0, blue]++;


                            dataPtr += nChan; // avançar o apontador para o proximo pixel

                        }

                        dataPtr += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }
                }
                return hist_arr;
            }
        }
        /// <summary>
        /// Histogram_All - 
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static int[,] Histogram_All(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                int[,] hist_arr = new int[4, 256];
                int[,] aux = Histogram_RGB(img);
                int[] aux2 = Histogram_Gray(img);
                int j, k;
                //COPIAR ARRAYS PARA O ARRAY DE SAIDA
                for (k = 0; k < 256; k++)
                {
                    hist_arr[0, k] = aux2[k];
                }

                for (j = 1; j < 4; j++)
                {
                    for (k = 0; k < 256; k++)
                    {
                        hist_arr[j, k] = aux[j - 1, k];
                    }
                }

                return hist_arr;
            }
        }
        /// <summary>
        /// Histogram_All - 
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void Equalization(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                // passar do espaço RGB para YCbCr
                Image<Ycc, byte> imgYcc = img.Convert<Ycc, byte>();

                //apontador para img ycc
                MIplImage m_ycc = imgYcc.MIplImage;
                byte* dataPtr_ycc = (byte*)m_ycc.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y, j, k;
                byte* aux_ptr;
                byte red, green, blue;
                long num_pixels = width * height;

                byte intensidade = 0;

                int n_intensidades = 256;

                int[] hist_arr_y = new int[256];
                byte[] mapa_intensidades_y = new byte[256];
                int[] ac_y = new int[256];

                // fazer histograma no canal Y
                for (y = 0; y < height; y++) // a cada linha de pixeis
                {
                    for (x = 0; x < width; x++) // a cada coluna de pixeis
                    {
                        // acesso direto a memoria - mais rapido
                        intensidade = (dataPtr_ycc + y * widthstep + x * nChan)[0]; // Y

                        hist_arr_y[intensidade]++; // para cada intensidade de cinzentos, incrementar o numero de pixeis que verificam essa intensidade
                    }
                }

                // fazer equalizacao no canal Y

                //criar a funcao de acumulacao de numero de vezes que cada intensidade e observada
                ac_y[0] = hist_arr_y[0];
                int val_min = 0;
                for (k = 1; k < n_intensidades; k++)
                {
                    ac_y[k] = ac_y[k - 1] + hist_arr_y[k];
                    if (ac_y[k] != 0 && val_min == 0)
                    {
                        val_min = ac_y[k]; // a primeira vez que e diferente de zero, apanhamos o minimo de Sky
                    }
                }

                // fazer a equalizacao do histograma
                for (k = 0; k < n_intensidades; k++)
                {
                    mapa_intensidades_y[k] = (byte)Math.Round((double)((n_intensidades - 1) * ((double)ac_y[k] - val_min) / (num_pixels - val_min)));
                }

                // atualizar valores de intensidade no canal Y da imagem Ycc
                for (y = 0; y < height; y++) // a cada linha de pixeis
                {
                    for (x = 0; x < width; x++) // a cada coluna de pixeis
                    {
                        // acesso direto a memoria - mais rapido
                        aux_ptr = (dataPtr_ycc + y * widthstep + x * nChan);

                        aux_ptr[0] = mapa_intensidades_y[aux_ptr[0]];
                    }
                }

                img.ConvertFrom<Ycc, byte>(imgYcc);
            }
        }
        public static void ConvertToBW(Emgu.CV.Image<Bgr, byte> img, int threshold)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y, pIntensity;
                byte red, green, blue;
                red = 0;
                blue = 0;
                green = 0;
                pIntensity = 0;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrive 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            // convert to gray
                            pIntensity = (int)Math.Round(((int)blue + green + red) / 3.0);

                            if (pIntensity > threshold)
                                pIntensity = 255;
                            else
                                pIntensity = 0;

                            // store in the image
                            dataPtr[0] = (byte)pIntensity;
                            dataPtr[1] = (byte)pIntensity;
                            dataPtr[2] = (byte)pIntensity;

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }

        public static void ConvertToBW_Otsu(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int i, j, x, y, threshold, intensity;
                byte red, green, blue;
                byte* aux_ptr;
                red = green = blue = 0;
                threshold = intensity = 0;
                int[] intensity_array = new int[256];
                int[] pIntensity = new int[256];
                double prob1, prob2, variance, max_variance, mean1, mean2;
                prob1 = prob2 = variance = mean1 = mean2 = max_variance = 0.0;


                if (nChan == 3) // image in RGB
                {

                    for (y = 0; y < height; y++) // a cada linha de pixeis
                    {
                        for (x = 0; x < width; x++) // a cada coluna de pixeis
                        {
                            // acesso direto a memoria - mais rapido
                            aux_ptr = dataPtr + x * nChan + y * widthstep;

                            blue = aux_ptr[0];
                            green = aux_ptr[1];
                            red = aux_ptr[2];

                            // convert to gray
                            intensity = (int)Math.Round(((int)blue + green + red) / 3.0);

                            intensity_array[intensity]++; // para cada intensidade de cinzentos, incrementar o numero de pixeis que verificam essa intensidade   

                        }

                    }
                }


                //acumular intensidades
                pIntensity[0] = intensity_array[0];
                for (i = 1; i < 256; i++)
                {
                    pIntensity[i] = intensity_array[i] + pIntensity[i - 1];
                }

                //254 pela formula t+1
                for (i = 0; i < 255; i++)
                {
                    prob1 = pIntensity[i]; //esquerda do threshold 
                    prob2 = pIntensity[255] - prob1; //direita do threshold 


                    //i é como se fosse threshold temporário 
                    for (j = 0; j <= i; j++)
                    {
                        mean1 += j * intensity_array[j];

                    }

                    //i+1 ver formula
                    for (j = (i + 1); j < 256; j++)
                    {
                        mean2 += j * intensity_array[j];

                    }

                    mean1 = mean1 / prob1;
                    mean2 = mean2 / prob2;

                    variance = prob1 * prob2 * Math.Pow(mean1 - mean2, 2.0);

                    if (variance > max_variance)
                    {
                        threshold = i;
                        max_variance = variance;
                    }

                    //reset nas variáveis:
                    mean1 = 0.0;
                    mean2 = 0.0;
                    prob1 = 0.0;
                    prob2 = 0.0;

                }

                ConvertToBW(img, threshold);

            }
        }

        public static int[] HorizontalProjection(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte* aux_ptr;
                int[] intensity_array = new int[width];

                for (y = 0; y < height; y++) // a cada coluna da iamgem
                {
                    for (x = 0; x < width; x++) // a cada linha da imagem
                    {
                        // acesso direto a memoria - mais rapido
                        aux_ptr = dataPtr + x * nChan + y * widthstep;
                        if (aux_ptr[0] == 0) // se detetarmos um preto
                            intensity_array[x]++;

                    }
                }
                return intensity_array;
            }
        }

        public static int[] VerticalProjection(Emgu.CV.Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte* aux_ptr;
                int[] intensity_array = new int[height];

                for (y = 0; y < height; y++) // a cada linha de pixeis
                {
                    for (x = 0; x < width; x++) // a cada coluna de pixeis
                    {
                        // acesso direto a memoria - mais rapido
                        aux_ptr = dataPtr + x * nChan + y * widthstep;
                        if (aux_ptr[0] == 0)
                            intensity_array[y]++;

                    }

                }
                return intensity_array;
            }
        }
        public static void Dilation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, int[,] mask)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_destino = (byte*)m.imageData.ToPointer();
                byte* aux_ptr;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int mask_size = mask.GetLength(0);
                int x, y;

                //This is the offset of center pixel from border of the kernel
                int kernelOffset = (int)((mask_size - 1) / 2.0); 
                //int calcOffset = 0;
                //int byteOffset = 0;
                bool dilation_done = false;
                if(nChan == 3)
                {

                    // bordas da imagem
                    // tratar do canto sperior esquerdo da imagem, considerando que o kernel está com colunas e linhas fora da imagem
                    // enquanto o kernel estiver com as suas colunas fora da imagem fora da imagem
                    // x = 0 -> x = kernelOffset ; y = 0 -> y = kernelOffset
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for(int k = 0; k < kernelOffset; k++)
                        {
                            dilation_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (xkernel + k > 0 && ykernel + j > 0)
                                    x = xkernel + k;
                                    y = ykernel + j;

                                    if (xkernel + k < 0)
                                        x = 0;
                                    if (ykernel + j < 0)
                                        y = 0;

                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 && aux_ptr[0] == 0)
                                    {
                                        // entao aplica-se a dilatação
                                        aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                        aux_ptr[0] = 0;
                                        aux_ptr[1] = 0;
                                        aux_ptr[2] = 0;
                                        dilation_done = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (dilation_done) { break; } // sair do for }
                                }
                                if (dilation_done) { break; } // sair do for }
                            }
                        }
                        
                    }

                    // tratar do canto inferior esquerdo
                    // x = 0 -> x = kernelOffset ; y = height-kernelOffset -> y = height-1
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            dilation_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (xkernel + k > 0 && ykernel + j < 0)
                                    x = xkernel + k;
                                    y = height-1 + ykernel + j;

                                    if (xkernel + k < 0)
                                        x = 0;
                                    if (ykernel + j > 0)
                                        y = height-1;

                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 && aux_ptr[0] == 0)
                                    {
                                        // entao aplica-se a dilatação
                                        aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                        aux_ptr[0] = 0;
                                        aux_ptr[1] = 0;
                                        aux_ptr[2] = 0;
                                        dilation_done = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (dilation_done) { break; } // sair do for }
                                }
                                if (dilation_done) { break; } // sair do for }
                            }
                        }

                    }

                    // tratar do canto superior direito
                    // x = width-kernelOffset -> x = width ; y = 0 -> y = kernelOffset
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            dilation_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (xkernel + k < 0 && ykernel + j > 0)
                                    x = width-1 + xkernel + k;
                                    y = ykernel + j;

                                    if (xkernel + k > 0)
                                        x = width-1;
                                    if (ykernel + j < 0)
                                        y = 0;

                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 && aux_ptr[0] == 0)
                                    {
                                        // entao aplica-se a dilatação
                                        aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                        aux_ptr[0] = 0;
                                        aux_ptr[1] = 0;
                                        aux_ptr[2] = 0;
                                        dilation_done = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (dilation_done) { break; } // sair do for }
                                }
                                if (dilation_done) { break; } // sair do for }
                            }
                        }

                    }

                    // tratar do canto inferior direito
                    // x = width-kernelOffset -> x = width ; y = height - kernelOffset -> y = height - 1
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            dilation_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (xkernel + k < 0 && ykernel + j < 0)
                                    x = width-1 + xkernel + k;
                                    y = height-1 + ykernel + j;

                                    if (xkernel + k > 0)
                                        x = width-1;
                                    if (ykernel + j > 0)
                                        y = height - 1;

                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 && aux_ptr[0] == 0)
                                    {
                                        // entao aplica-se a dilatação
                                        aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                        aux_ptr[0] = 0;
                                        aux_ptr[1] = 0;
                                        aux_ptr[2] = 0;
                                        dilation_done = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (dilation_done) { break; } // sair do for }
                                }
                                if (dilation_done) { break; } // sair do for }
                            }
                        }

                    }

                    // tratar da linha de borda superior
                    // x = kernelOffset -> x = width-kernelOffset ; y = 0 -> y = kernelOffser-1
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (x = kernelOffset; x < width - kernelOffset; x++)
                        {
                            dilation_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (  ykernel + j >= 0 )
                                    y = ykernel + j;

                                    //verificar se as linhas que estamos a ir buscar do kernel estao situadas fora da imagem
                                    if(ykernel + j < 0)
                                    {
                                        // caso estejam, tenho de ir buscar o valor das linhas duplicando a linha de borda
                                        y = 0;
                                    }

                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x+xkernel) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 && aux_ptr[0] == 0)
                                    {
                                        // entao aplica-se a dilatação
                                        aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                        aux_ptr[0] = 0;
                                        aux_ptr[1] = 0;
                                        aux_ptr[2] = 0;
                                        dilation_done = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (dilation_done) { break; } // sair do for }
                                }
                                if (dilation_done) { break; } // sair do for }
                            }
                        }

                    }
                    // tratar da linha de borda inferior
                    // x = kernelOffset -> x = width-kernelOffset ; y = 0 -> y = kernelOffser-1
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (x = kernelOffset; x < width - kernelOffset; x++)
                        {
                            dilation_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (  ykernel + j >= 0 )
                                    y = height-1 + ykernel + j;

                                    //verificar se as linhas que estamos a ir buscar do kernel estao situadas fora da imagem
                                    if (ykernel + j > 0)
                                    {
                                        // caso estejam, tenho de ir buscar o valor das linhas duplicando a linha de borda
                                        y = height-1;
                                    }

                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x+xkernel) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 && aux_ptr[0] == 0)
                                    {
                                        // entao aplica-se a dilatação
                                        aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                        aux_ptr[0] = 0;
                                        aux_ptr[1] = 0;
                                        aux_ptr[2] = 0;
                                        dilation_done = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (dilation_done) { break; } // sair do for }
                                }
                                if (dilation_done) { break; } // sair do for }
                            }
                        }

                    }
                    // tratar da linha de borda esquerda
                    // x = kernelOffset -> x = width-kernelOffset ; y = 0 -> y = kernelOffser-1
                    for (y= kernelOffset; y < height-kernelOffset; y++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            dilation_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (  ykernel + j >= 0 )
                                    x = xkernel + k;

                                    //verificar se as colunas que estamos a ir buscar do kernel estao situadas fora da imagem ou na borda
                                    if (xkernel + k < 0)
                                    {
                                        // caso estejam, tenho de ir buscar o valor das colunas duplicando a coluna de borda
                                        x = 0;
                                    }

                                    aux_ptr = dataPtr_origem + (y+ykernel) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 && aux_ptr[0] == 0)
                                    {
                                        // entao aplica-se a dilatação
                                        aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                        aux_ptr[0] = 0;
                                        aux_ptr[1] = 0;
                                        aux_ptr[2] = 0;
                                        dilation_done = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (dilation_done) { break; } // sair do for }
                                }
                                if (dilation_done) { break; } // sair do for }
                            }
                        }

                    }

                    // tratar da linha de borda direita
                    // x = width - kernelOffset -> x = width-1 ; y = kernelOffset -> y = height - kernelOffset
                    for (y = kernelOffset; y < height - kernelOffset; y++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            dilation_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (  ykernel + j >= 0 )
                                    x = width-1 + xkernel + k;

                                    //verificar se as colunas que estamos a ir buscar do kernel estao situadas fora da imagem ou na borda
                                    if (xkernel + k > 0)
                                    {
                                        // caso estejam, tenho de ir buscar o valor das colunas duplicando a coluna de borda
                                        x = width-1;
                                    }

                                    aux_ptr = dataPtr_origem + (y+ykernel) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 && aux_ptr[0] == 0)
                                    {
                                        // entao aplica-se a dilatação
                                        aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                        aux_ptr[0] = 0;
                                        aux_ptr[1] = 0;
                                        aux_ptr[2] = 0;
                                        dilation_done = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (dilation_done) { break; } // sair do for }
                                }
                                if (dilation_done) { break; } // sair do for }
                            }
                        }

                    }
                    // core da imagem
                    for (y = kernelOffset; y < height - kernelOffset; y++)
                    {
                        for (x = kernelOffset; x < width - kernelOffset; x++)
                        {
                            dilation_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    aux_ptr = dataPtr_origem + (y + ykernel) * widthstep + (x + xkernel) * nChan;
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 && aux_ptr[0] == 0)
                                    {
                                        // entao aplica-se a dilatação
                                        aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                        aux_ptr[0] = 0;
                                        aux_ptr[1] = 0;
                                        aux_ptr[2] = 0;
                                        dilation_done = true;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (dilation_done) { break; } // sair do for }
                                }
                                if (dilation_done) { break; } // sair do for }
                            }

                            //dataPtr_destino += nChan; // avançar o apontador para o proximo pixel
                        }

                        // dataPtr_destino += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }
                    //return img;

                }
            }
                
        }
        public static void Erosion(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, int[,] mask)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_destino = (byte*)m.imageData.ToPointer();
                byte* aux_ptr;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int mask_size = mask.GetLength(0); // assume-se que os kernels sao sempre quadrados
                int x, y, x2, y2;

//This is the offset of center pixel from border of the kernel
                int kernelOffset = (int)((mask_size - 1) / 2.0);
                //int calcOffset = 0;
                //int byteOffset = 0;
                bool erosion_done = false; // flag para saber se a erosao ja foi efectuada no pixel em questao
                bool out_of_img = false;
                int area = 0; //area do elemento estruturante
                int correspondance = 0; // numero de pixeis do objeto que incluem os pixeis da mascara
                if (nChan == 3)
                {
                    x = 0;
                    y = 0;
                    // calcular area do elemento estruturante
                    for (int k = 0; k < mask_size; k++)
                    {
                        for (int j = 0; j < mask_size; j++)
                        {
                            if (mask[k, j] == 1)
                                area += 1;
                        }
                    }

                    // bordas da imagem
                    // tratar do canto sperior esquerdo da imagem, considerando que o kernel está com colunas e linhas fora da imagem
                    // enquanto o kernel estiver com as suas colunas fora da imagem fora da imagem
                    // x = 0 -> x = kernelOffset ; y = 0 -> y = kernelOffset
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            correspondance = 0;
                            erosion_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (xkernel + k > 0 && ykernel + j > 0)
                                    x = xkernel + k;
                                    y = ykernel + j;
                                    out_of_img = false;
                                    if (xkernel + k < 0)
                                    {
                                        x = 0;
                                        out_of_img = true;
                                    }
                                    if (ykernel + j < 0)
                                    {
                                        y = 0;
                                        out_of_img = true;
                                    }
                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                   
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 )
                                    {
                                        if (aux_ptr[0] == 0) // se a regiao e o kernel coincidirem
                                        {
                                            correspondance++; // aumentar a correspondencia entre mascara e regiao
                                        }
                                        else// caso contrario efectua-se uma erosao do pixel central
                                        {
                                            aux_ptr = dataPtr_destino + (j) * widthstep + (k) * nChan;
                                            aux_ptr[0] = 255;
                                            aux_ptr[1] = 255;
                                            aux_ptr[2] = 255;
                                            erosion_done = true;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (erosion_done)
                                        break; // quebrar o for
                                }
                                if (erosion_done) { break; } // sair do for }
                            }
                            
                            if (correspondance == area) // se a correspondencia da regiao a mascara for completa
                            {
                                // colocar o pixel central a preto na imagem resultante
                                aux_ptr = dataPtr_destino + (j) * widthstep + (k) * nChan;
                                aux_ptr[0] = 0;
                                aux_ptr[1] = 0;
                                aux_ptr[2] = 0;

                            }
                        }

                    }

                    // tratar do canto inferior esquerdo
                    // x = 0 -> x = kernelOffset ; y = height-kernelOffset -> y = height-1
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            correspondance = 0;
                            erosion_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (xkernel + k > 0 && ykernel + j < 0)
                                    x = xkernel + k;
                                    y = height-1 + ykernel + j;
                                    out_of_img = false;
                                    if (xkernel + k < 0)
                                    {
                                        x = 0;
                                        out_of_img = true;
                                    }
                                    if (ykernel + j > 0)
                                    {
                                        y = height - 1;
                                        out_of_img = true;
                                    }

                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1)
                                    {
                                        if (aux_ptr[0] == 0 )
                                        {
                                            correspondance++; // aumentar a correspondencia entre mascara e regiao
                                        }
                                        else// caso contrario efectua-se uma erosao do pixel central
                                        {
                                            aux_ptr = dataPtr_destino + (height - kernelOffset + j) * widthstep + (k) * nChan;
                                            aux_ptr[0] = 255;
                                            aux_ptr[1] = 255;
                                            aux_ptr[2] = 255;
                                            erosion_done = true;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (erosion_done)
                                        break; // quebrar o for
                                }
                                if (erosion_done) { break; } // sair do for }
                            }
                            
                            if (correspondance == area) // se a correspondencia da regiao a mascara for completa
                            {
                                // colocar o pixel central a preto na imagem resultante
                                aux_ptr = dataPtr_destino + (height - kernelOffset + j) * widthstep + (k) * nChan;
                                aux_ptr[0] = 0;
                                aux_ptr[1] = 0;
                                aux_ptr[2] = 0;

                            }
                        }

                    }

                    // tratar do canto superior direito
                    // x = width-kernelOffset -> x = width ; y = 0 -> y = kernelOffset
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            correspondance = 0;
                            erosion_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (xkernel + k < 0 && ykernel + j > 0)
                                    x = width-1 + xkernel + k;
                                    y = ykernel + j;
                                    out_of_img = false;
                                    if (xkernel + k > 0)
                                    {
                                        x = width - 1;
                                        out_of_img = true;
                                    }


                                    if (ykernel + j < 0)
                                    {
                                        y = 0;
                                        out_of_img = true;
                                    }
                                        

                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                   
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1)
                                    {
                                        if (aux_ptr[0] == 0 )
                                        {
                                            correspondance++; // aumentar a correspondencia entre mascara e regiao
                                        }
                                        else// caso contrario efectua-se uma erosao do pixel central
                                        {
                                            aux_ptr = dataPtr_destino + (j) * widthstep + (width - kernelOffset + k) * nChan;
                                            aux_ptr[0] = 255;
                                            aux_ptr[1] = 255;
                                            aux_ptr[2] = 255;
                                            erosion_done = true;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (erosion_done)
                                        break; // quebrar o for
                                }
                                if (erosion_done) { break; } // sair do for }
                            }
                           
                            if (correspondance == area) // se a correspondencia da regiao a mascara for completa
                            {
                                // colocar o pixel central a preto na imagem resultante
                                aux_ptr = dataPtr_destino + (j) * widthstep + (width-kernelOffset+k) * nChan;
                                aux_ptr[0] = 0;
                                aux_ptr[1] = 0;
                                aux_ptr[2] = 0;

                            }
                        }

                    }

                    // tratar do canto inferior direito
                    // x = width-kernelOffset -> x = width ; y = height - kernelOffset -> y = height - 1
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            correspondance = 0;
                            erosion_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (xkernel + k < 0 && ykernel + j < 0)
                                    x = width -1+ xkernel + k;
                                    y = height- 1 + ykernel + j;
                                    out_of_img = false;
                                    if (xkernel + k > 0)
                                    {
                                        x = width - 1;
                                        out_of_img = true;
                                    }
                                   
                                    if (ykernel + j > 0)
                                    {
                                        y = height - 1;
                                        out_of_img = true;
                                    }
                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1)
                                    {
                                        if (aux_ptr[0] == 0)
                                        {
                                            correspondance++; // aumentar a correspondencia entre mascara e regiao
                                        }
                                        else// caso contrario efectua-se uma erosao do pixel central
                                        {
                                            aux_ptr = dataPtr_destino + (height - kernelOffset + j) * widthstep + (width - kernelOffset + k) * nChan;
                                            aux_ptr[0] = 255;
                                            aux_ptr[1] = 255;
                                            aux_ptr[2] = 255;
                                            erosion_done = true;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (erosion_done)
                                        break; // quebrar o for
                                }
                                if (erosion_done) { break; } // sair do for }
                            }
                            
                            if (correspondance == area) // se a correspondencia da regiao a mascara for completa
                            {
                                // colocar o pixel central a preto na imagem resultante
                                aux_ptr = dataPtr_destino + (height-kernelOffset+j) * widthstep + (width-kernelOffset+k) * nChan;
                                aux_ptr[0] = 0;
                                aux_ptr[1] = 0;
                                aux_ptr[2] = 0;

                            }
                        }

                    }

                    // tratar da linha de borda superior
                    // x = kernelOffset -> x = width-kernelOffset ; y = 0 -> y = kernelOffser-1
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (x = kernelOffset; x < width - kernelOffset; x++)
                        {
                            correspondance = 0;
                            erosion_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (  ykernel + j >= 0 )
                                    y = ykernel + j;
                                    out_of_img = false;
                                    //verificar se as linhas que estamos a ir buscar do kernel estao situadas fora da imagem
                                    if (ykernel + j < 0)
                                    {
                                        // caso estejam, tenho de ir buscar o valor das linhas duplicando a linha de borda
                                        y = 0;
                                        out_of_img = true;
                                    }
                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x+xkernel) * nChan;
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                   
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1)
                                    {
                                        if (aux_ptr[0] == 0 )
                                        {
                                            correspondance++; // aumentar a correspondencia entre mascara e regiao
                                        }
                                        else// caso contrario efectua-se uma erosao do pixel central
                                        {
                                            aux_ptr = dataPtr_destino + (j) * widthstep + (x) * nChan;
                                            aux_ptr[0] = 255;
                                            aux_ptr[1] = 255;
                                            aux_ptr[2] = 255;
                                            erosion_done = true;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (erosion_done)
                                        break; // quebrar o for
                                }
                                if (erosion_done) { break; } // sair do for }
                            }
                            
                            if (correspondance == area) // se a correspondencia da regiao a mascara for completa
                            {
                                // colocar o pixel central a preto na imagem resultante
                                aux_ptr = dataPtr_destino + (j) * widthstep + (x) * nChan;
                                aux_ptr[0] = 0;
                                aux_ptr[1] = 0;
                                aux_ptr[2] = 0;

                            }
                        }

                    }
                    // tratar da linha de borda inferior
                    // x = kernelOffset -> x = width-kernelOffset ; y = 0 -> y = kernelOffser-1
                    for (int j = 0; j < kernelOffset; j++)
                    {
                        for (x = kernelOffset; x < width - kernelOffset; x++)
                        {
                            correspondance = 0;
                            erosion_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (  ykernel + j >= 0 )
                                    y = height-1 + ykernel + j;
                                    out_of_img = false;
                                    //verificar se as linhas que estamos a ir buscar do kernel estao situadas fora da imagem
                                    if (ykernel + j > 0)
                                    {
                                        // caso estejam, tenho de ir buscar o valor das linhas duplicando a linha de borda
                                        y = height - 1;
                                        out_of_img = true;
                                    }

                                    aux_ptr = dataPtr_origem + (y) * widthstep + (x+xkernel) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                   
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 )
                                    {
                                        if (aux_ptr[0] == 0 )
                                        {
                                            correspondance++; // aumentar a correspondencia entre mascara e regiao
                                        }
                                        else// caso contrario efectua-se uma erosao do pixel central
                                        {
                                            aux_ptr = dataPtr_destino + (height - kernelOffset + j) * widthstep + (x) * nChan;
                                            aux_ptr[0] = 255;
                                            aux_ptr[1] = 255;
                                            aux_ptr[2] = 255;
                                            erosion_done = true;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (erosion_done)
                                        break; // quebrar o for
                                }
                                if (erosion_done) { break; } // sair do for }
                            }
                            
                            if (correspondance == area) // se a correspondencia da regiao a mascara for completa
                            {
                                // colocar o pixel central a preto na imagem resultante
                                aux_ptr = dataPtr_destino + (height - kernelOffset + j) * widthstep + (x) * nChan;
                                aux_ptr[0] = 0;
                                aux_ptr[1] = 0;
                                aux_ptr[2] = 0;
                            }
                        }

                    }
                    // tratar da linha de borda esquerda
                    // x = kernelOffset -> x = width-kernelOffset ; y = 0 -> y = kernelOffser-1
                    for (y = kernelOffset; y < height - kernelOffset; y++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            correspondance = 0;
                            erosion_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (  ykernel + j >= 0 )
                                    x = xkernel + k;
                                    out_of_img = false;
                                    //verificar se as colunas que estamos a ir buscar do kernel estao situadas fora da imagem ou na borda
                                    if (xkernel + k < 0)
                                    {
                                        // caso estejam, tenho de ir buscar o valor das colunas duplicando a coluna de borda
                                        x = 0;
                                        out_of_img = true;
                                    }

                                    aux_ptr = dataPtr_origem + (y+ykernel) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                    
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 )
                                    {
                                        if (aux_ptr[0] == 0 )
                                        {
                                            correspondance++; // aumentar a correspondencia entre mascara e regiao
                                        }
                                        else// caso contrario efectua-se uma erosao do pixel central
                                        {
                                            aux_ptr = dataPtr_destino + (y) * widthstep + (k) * nChan;
                                            aux_ptr[0] = 255;
                                            aux_ptr[1] = 255;
                                            aux_ptr[2] = 255;
                                            erosion_done = true;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (erosion_done)
                                        break; // quebrar o for
                                }
                                if (erosion_done) { break; } // sair do for }
                            }
                            
                            if (correspondance == area) // se a correspondencia da regiao a mascara for completa
                            {
                                // colocar o pixel central a preto na imagem resultante
                                aux_ptr = dataPtr_destino + (y) * widthstep + (k) * nChan;
                                aux_ptr[0] = 0;
                                aux_ptr[1] = 0;
                                aux_ptr[2] = 0;

                            }
                        }

                    }

                    // tratar da linha de borda direita
                    // x = width - kernelOffset -> x = width-1 ; y = kernelOffset -> y = height - kernelOffset
                    for (y = kernelOffset; y < height - kernelOffset; y++)
                    {
                        for (int k = 0; k < kernelOffset; k++)
                        {
                            correspondance = 0;
                            erosion_done = false; // flag para saber se a dilataºão ja foi efectuada em relação a este pixel
                            // percorrer o kernel nas ordenadas
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                // percorrer o kernel nas abcissas
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    // declaração inicial, assumindo que ja estamos a observar o ernel num sitio em que ja estamos dentro da imagem
                                    //if (  ykernel + j >= 0 )
                                    x = width-1 + xkernel + k;
                                    out_of_img = false;
                                    //verificar se as colunas que estamos a ir buscar do kernel estao situadas fora da imagem ou na borda
                                    if (xkernel + k > 0)
                                    {
                                        // caso estejam, tenho de ir buscar o valor das colunas duplicando a coluna de borda
                                        x = width - 1;
                                        out_of_img = true;
                                    }

                                    aux_ptr = dataPtr_origem + (y+ykernel) * widthstep + (x) * nChan; // ir buscar o pixel do canto
                                    //observação de 
                                    // se um unico dos pixeis da mascara apanhar um pixel pertencente ao objeto
                                   
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1 )
                                    {
                                        if (aux_ptr[0] == 0 )
                                        {
                                            correspondance++; // aumentar a correspondencia entre mascara e regiao
                                        }
                                        else// caso contrario efectua-se uma erosao do pixel central
                                        {
                                            aux_ptr = dataPtr_destino + (y) * widthstep + (width - kernelOffset + k) * nChan;
                                            aux_ptr[0] = 255;
                                            aux_ptr[1] = 255;
                                            aux_ptr[2] = 255;
                                            erosion_done = true;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (erosion_done)
                                        break; // quebrar o for
                                }
                                if (erosion_done) { break; } // sair do for }
                            }
                            
                            if (correspondance == area) // se a correspondencia da regiao a mascara for completa
                            {
                                // colocar o pixel central a preto na imagem resultante
                                aux_ptr = dataPtr_destino + (y) * widthstep + (width - 1 - kernelOffset + k) * nChan;
                                aux_ptr[0] = 0;
                                aux_ptr[1] = 0;
                                aux_ptr[2] = 0;

                            }
                        }

                    }

                    // tratar do core da imagem na erosao
                    for (y = kernelOffset; y < height - kernelOffset; y++)
                    {
                        for (x = kernelOffset; x < width - kernelOffset; x++)
                        {
                            erosion_done = false;
                            correspondance = 0;
                            for (int ykernel = -kernelOffset; ykernel <= kernelOffset; ykernel++)
                            {
                                for (int xkernel = -kernelOffset; xkernel <= kernelOffset; xkernel++)
                                {
                                    aux_ptr = dataPtr_origem + (y + ykernel) * widthstep + (x + xkernel) * nChan;
                                    // se o pixel da mascara estiver sobreposto a um pixel do objeto,
                                    if (mask[ykernel + kernelOffset, xkernel + kernelOffset] == 1)
                                    {
                                        if (aux_ptr[0] == 0)
                                        {
                                            correspondance++; // aumentar a correspondencia entre mascara e regiao
                                        }
                                        else// caso contrario efectua-se uma erosao do pixel central
                                        {
                                            aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                            aux_ptr[0] = 255;
                                            aux_ptr[1] = 255;
                                            aux_ptr[2] = 255;
                                            erosion_done = true;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (erosion_done)
                                        break; // quebrar o for
                                }
                                if (erosion_done)
                                    break; // quebrar o for
                            }

                            if (correspondance == area) // se a correspondencia da regiao a mascara for completa
                            {
                                // colocar o pixel central a preto na imagem resultante
                                aux_ptr = dataPtr_destino + (y) * widthstep + (x) * nChan;
                                aux_ptr[0] = 0;
                                aux_ptr[1] = 0;
                                aux_ptr[2] = 0;

                            }

                            //dataPtr_destino += nChan; // avançar o apontador para o proximo pixel
                        }

                        // dataPtr_destino += padding; // contornar os pixeis de padding, para nao lhes tocar sequer - nao sao representados na imagem
                    }
                    //return img;
                }

            }
        }

        public static double angleFinder(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy.MIplImage;

                byte* dataPtr_origem = (byte*)m_cpy.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_destino = (byte*)m.imageData.ToPointer();
                byte* aux_ptr;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                long Sx = 0;
                long Sy = 0;
                long Sxx = 0;
                long Syy = 0;
                long Sxy = 0;
                long area = 0;
                long Mxx, Myy, Mxy;
                double angle_momentum = 0;
                int y_init = 0;
                int x_init = 0;
                int y_fin = 0;
                int x_fin_esq = 0;
                int x_fin_dir = 0;
                int x_fin = 0;
                bool found_init = false;
                bool found_max = false;
                int[] vert_projection = new int[width];
                int threshold = 4;
                if (nChan == 3)
                {
                    // acumular as ordenadas de cada pixel - Sy
                    //acumular as abcissas de cada pixel da imagem - Sx
                    // se Sx > Sy - > imagem vai ser rodada na horizontal
                    // se Sy > Sx -> imagem esta num angulo de 90 graus -> rodamos 90, e depois e que apliamos o algoritmo desta função
                    // calcular eixo de momento
                    //ConvertToBW_Otsu(img);
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan;
                            if (aux_ptr[0] == 0) // se o pixel tiver a preto - pertencer ao objecto
                            {
                                area++;
                                Sx += x;
                                Sy += y;
                                Sxx += x * x;
                                Syy += y * y;
                                Sxy += x * y;
                            }
                        }
                    }
                    Mxx = Sxx - (Sx * Sx) / area;
                    Myy = Syy - (Sy * Sy) / area;
                    Mxy = Sxy - (Sx * Sy) / area;
                    angle_momentum = Math.Atan((double)(Mxx - Myy + Math.Sqrt(Math.Pow(Mxx - Myy, 2) + 4 * Math.Pow(Mxy, 2))) / (2 * Mxy));

                    if (Mxy < 0)
                    {
                        angle_momentum += (Math.PI / 2);
                    }
                    else
                    {
                        angle_momentum -= (Math.PI / 2);
                    }
                    //if (angle_momentum < 0.015 && angle_momentum >-0.015)
                    //    angle_momentum = 0.0;//Rotation_Bilinear(img, imgCopy, (float)angle_momentum);


                   
                }
                return angle_momentum;
            }

        }

        public static int[] Centroid(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtr_origem = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* aux_ptr;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                int[] centroid = new int[2];
                int Cx, Cy;
                long Sx, Sy;
                long area;
                if (nChan == 3)
                {
                    //ConvertToBW_Otsu(img);
                    Sx = 0;
                    Sy = 0;
                    area = 0;
                    Cx = 0;
                    Cy = 0;
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            aux_ptr = dataPtr_origem + (y) * widthstep + (x) * nChan;
                            if (aux_ptr[0] == 0) // se o pixel tiver a preto - pertencer ao objecto
                            {
                                area++;
                                Sx += x;
                                Sy += y;
                            }
                        }
                    }
                    Cx = (int)(Sx / area);
                    Cy = (int)(Sy / area);
                    centroid[0] = Cx;
                    centroid[1] = Cy;

                }
                return centroid;
            }

        }

        // filtro para suavizar a serie de dados do histograma
        public static int[] MeanFilter(int[] histogram, int order)
        {
            unsafe
            {
                int[] filt_data_series = new int[histogram.Length];
                double[] coefs = new double[order * 2];
                //definir coeficientes do filtro
                for (int i = 0; i < order; i++)
                {
                    coefs[i] = (i + 1 - 0.1) / order;
                }
                for (int i = order; i < order * 2; i++)
                {
                    coefs[i] = (order - (i - order) - 0.1) / order;
                }
                // aplicar o filtro as primeiras amostras
                double val_sum = 0;
                // tratar os valores do fim do histograma
                for (int k = 0; k < order; k++)
                {
                    val_sum = 0;
                    for (int i = -k; i < order; i++)
                    {
                        val_sum += histogram[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = (int)val_sum;
                }
                //tratar os valores do meio do histograma
                for (int k = order; k < histogram.Length - order; k++)
                {
                    val_sum = 0;
                    for (int i = -order; i < order; i++)
                    {
                        val_sum += histogram[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = (int)val_sum;
                }
                // tratar os valores do fim do histograma
                for (int k = histogram.Length - order; k < histogram.Length; k++)
                {
                    val_sum = 0;
                    for (int i = -order; i < (histogram.Length - k); i++)
                    {
                        val_sum += histogram[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = (int)val_sum;
                }
                return filt_data_series;
            }

        }

        // filtro para detetar transições elevadas no histograma da imagem (aka bordas do CB) normalizado aos limites maximos da imagem
        public static double[] NormMeanFilter(int[] histogram, int order, int n_max)
        {
            unsafe
            {

                double[] norm_data_series = new double[histogram.Length];
                double[] filt_data_series = new double[histogram.Length];
                for (int k = 0; k < histogram.Length; k++)
                {
                    norm_data_series[k] = (double)(histogram[k] / n_max);
                }

                double[] coefs = new double[order * 2];
                //definir coeficientes do filtro
                for (int i = 0; i < order; i++)
                {
                    coefs[i] = (i + 1 - 0.1) / order;
                }
                for (int i = order; i < order * 2; i++)
                {
                    coefs[i] = (order - (i - order) - 0.1) / order;
                }
                // aplicar o filtro as primeiras amostras
                double val_sum = 0;
                // tratar os valores do fim do histograma
                for (int k = 0; k < order; k++)
                {
                    val_sum = 0;
                    for (int i = -k; i < order; i++)
                    {
                        val_sum += norm_data_series[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = val_sum;
                }
                //tratar os valores do meio do histograma
                for (int k = order; k < histogram.Length - order; k++)
                {
                    val_sum = 0;
                    for (int i = -order; i < order; i++)
                    {
                        val_sum += norm_data_series[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = val_sum;
                }
                // tratar os valores do fim do histograma
                for (int k = histogram.Length - order; k < histogram.Length; k++)
                {
                    val_sum = 0;
                    for (int i = -order; i < (histogram.Length - k); i++)
                    {
                        val_sum += norm_data_series[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = val_sum;
                }
                return filt_data_series;
            }

        }
        // filtro para detetar transições elevadas no histograma da imagem (aka bordas do CB)
        public static int[] MatchFilter(int[] histogram, int order, double amp)
        {
            unsafe
            {
                int[] filt_data_series = new int[histogram.Length];
                double[] coefs = new double[order * 2];
                //definir coeficientes do filtro
                for (int i = 0; i < order; i++)
                {
                    coefs[i] = -amp;
                }
                for (int i = order; i < order * 2; i++)
                {
                    coefs[i] = amp;
                }
                // aplicar o filtro as primeiras amostras
                double val_sum = 0;
                // tratar os valores do fim do histograma
                for (int k = 0; k < order; k++)
                {
                    val_sum = 0;
                    for (int i = -k; i < order; i++)
                    {
                        val_sum += histogram[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = (int)val_sum;
                }
                //tratar os valores do meio do histograma
                for (int k = order; k < histogram.Length - order; k++)
                {
                    val_sum = 0;
                    for (int i = -order; i < order; i++)
                    {
                        val_sum += histogram[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = (int)val_sum;
                }
                // tratar os valores do fim do histograma
                for (int k = histogram.Length - order; k < histogram.Length; k++)
                {
                    val_sum = 0;
                    for (int i = -order; i < (histogram.Length - k); i++)
                    {
                        val_sum += histogram[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = (int)val_sum;
                }
                return filt_data_series;
            }

        }

        // filtro para detetar transições elevadas no histograma da imagem (aka bordas do CB) normalizado aos limites maximos da imagem
        public static double[] NormMatchFilter(int[] histogram, int order, double amp, int n_max)
        {
            unsafe
            {
                double[] norm_data_series = new double[histogram.Length];
                double[] filt_data_series = new double[histogram.Length];
                for (int k = 0; k < histogram.Length; k++)
                {
                    norm_data_series[k] = (int)(histogram[k] / n_max);
                }
                double[] coefs = new double[order * 2];
                //definir coeficientes do filtro
                for (int i = 0; i < order; i++)
                {
                    coefs[i] = -amp;
                }
                for (int i = order; i < order * 2; i++)
                {
                    coefs[i] = amp;
                }
                // aplicar o filtro as primeiras amostras
                double val_sum = 0;
                // tratar os valores do fim do histograma
                for (int k = 0; k < order; k++)
                {
                    val_sum = 0;
                    for (int i = -k; i < order; i++)
                    {
                        val_sum += norm_data_series[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = val_sum;
                }
                //tratar os valores do meio do histograma
                for (int k = order; k < histogram.Length - order; k++)
                {
                    val_sum = 0;
                    for (int i = -order; i < order; i++)
                    {
                        val_sum += norm_data_series[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = val_sum;
                }
                // tratar os valores do fim do histograma
                for (int k = histogram.Length - order; k < histogram.Length; k++)
                {
                    val_sum = 0;
                    for (int i = -order; i < (histogram.Length - k); i++)
                    {
                        val_sum += norm_data_series[k + i] * coefs[order + i];
                    }
                    filt_data_series[k] = val_sum;
                }
                return filt_data_series;
            }

        }

        //Average - calcula o valor médio de uma serie de dados
        private static double Average(double[] projection)
        {
            double sum = 0;
            for (int k = 0; k < projection.Length; k++)
            {
                sum += projection[k];
            }
            return sum / projection.Length;
        }
        //Variance - calcula a variancia de uma serie de dados discretos
        private static double Variance(double[] projection)
        {
            double squareSum = 0;
            double avg = Average(projection);
            for (int k = 0; k < projection.Length; k++)
            {
                squareSum += Math.Pow((projection[k] - avg), 2);
            }
            return squareSum / (projection.Length - 1);
        }
        //StandardDeviation - calcula o desvio pardrao da serie de dados
        private static double StandardDeviation(double[] projection)
        {
            return Math.Sqrt(Variance(projection));
        }
        //LogVect - calcula o logaritmo de cada elemento do vector de dados
        private static int[] LogVect(int[] projection)
        {
            int[] log_v = new int[projection.Length];
            for (int k = 0; k < projection.Length; k++)
            {
                log_v[k] = (int)Math.Log10((double)projection[k]);
            }
            return log_v;
        }

        //WhiteTopHat - transformada que devolve a diferença entre uma imagem binarizada e filtrada por Roberts e a sua abertura segundo um elemento estruturante (mascara)
        public static void WhiteTopHat(Image<Bgr, byte> img, int[,] mask, int iter)
        {
            unsafe
            {   //realizar abertura na imagem copia, para a subtrair a imagem original
                Image<Bgr, byte> imgCopy1 = img.Copy();
                Image<Bgr, byte> imgCopy2 = img.Copy();
                Open(imgCopy1, imgCopy2, mask, iter);

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy1.MIplImage;
                byte* dataPtr_origem = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_transformada = (byte*)m_cpy.imageData.ToPointer();
                byte* aux_ptr;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int mask_size = mask.GetLength(0);
                int x, y;
                int aux = 0;
                if (nChan == 3)
                {

                    // de seguida realizar a subtração entre imagens

                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            // aux_ptr = dataPtr_origem + y * widthstep + x * nChan;

                            //subtrair a imagem alterada, a imagem original
                            //dataPtr_origem[0] = dataPtr_origem[0] - dataPtr_transformada[0] == 0 ? (byte)255 : (dataPtr_origem[0] - dataPtr_transformada[0] < 0 ? dataPtr_origem[0] : dataPtr_transformada[0]);
                            dataPtr_origem[0] = dataPtr_origem[0] - dataPtr_transformada[0] == 0 ? (byte)255 : dataPtr_origem[0];
                            dataPtr_origem[1] = dataPtr_origem[0];
                            dataPtr_origem[2] = dataPtr_origem[0];

                            dataPtr_transformada += nChan; // avançar o apontador para o proximo pixel
                            dataPtr_origem += nChan;
                        }
                        dataPtr_transformada += padding; // contornar os pixeis de padding, para nao lhes tocar s
                        dataPtr_origem += padding;
                    }

                }

            }

        }

        //WhiteTopHat - transformada que devolve a diferença entre uma imagem binarizada e filtrada por Roberts e a sua abertura segundo um elemento estruturante (mascara)
        public static void WhiteHat(Image<Bgr, byte> img, int[,] mask)
        {
            unsafe
            {   //realizar abertura na imagem copia, para a subtrair a imagem original
                Image<Bgr, byte> imgCopy1 = img.Copy();
                Erosion(imgCopy1, img, mask);
                Image<Bgr, byte> imgCopy2 = imgCopy1.Copy(); // criar copia da imagem erodida
                Dilation(imgCopy1, imgCopy2, mask); // dilatar imagem erodida

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy1.MIplImage;
                byte* dataPtr_origem = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_transformada = (byte*)m_cpy.imageData.ToPointer();
                byte* aux_ptr;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int mask_size = mask.GetLength(0);
                int x, y;
                int aux = 0;
                if (nChan == 3)
                {

                    // de seguida realizar a subtração entre imagens

                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            // aux_ptr = dataPtr_origem + y * widthstep + x * nChan;

                            //subtrair a imagem alterada, a imagem original
                            aux = dataPtr_origem[0] - dataPtr_transformada[0];
                            dataPtr_origem[0] = (byte)(aux < 0 ? dataPtr_transformada[0] : aux);

                            aux = dataPtr_origem[1] - dataPtr_transformada[1];
                            dataPtr_origem[1] = (byte)(aux < 0 ? dataPtr_transformada[1] : aux);

                            aux = dataPtr_origem[2] - dataPtr_transformada[2];
                            dataPtr_origem[2] = (byte)(aux < 0 ? dataPtr_transformada[2] : aux);


                            dataPtr_transformada += nChan; // avançar o apontador para o proximo pixel
                            dataPtr_origem += nChan;
                        }
                        dataPtr_transformada += padding; // contornar os pixeis de padding, para nao lhes tocar s
                        dataPtr_origem += padding;
                    }

                }

            }

        }
        public static void BlackHat(Image<Bgr, byte> img, int[,] mask)
        {
            unsafe
            {   //realizar um fecho na imagem copia
                Image<Bgr, byte> imgCopy1 = img.Copy();
                Dilation(imgCopy1, img, mask); // dilatar imagem erodidaq
                Image<Bgr, byte> imgCopy2 = imgCopy1.Copy(); // criar copia da imagem erodida
                Erosion(imgCopy1, imgCopy2, mask);

                MIplImage m = img.MIplImage;
                MIplImage m_cpy = imgCopy1.MIplImage;
                byte* dataPtr_origem = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* dataPtr_transformada = (byte*)m_cpy.imageData.ToPointer();
                byte* aux_ptr;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int mask_size = mask.GetLength(0);
                int x, y;
                int aux = 0;
                if (nChan == 3)
                {

                    // de seguida realizar a subtração entre imagens

                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            // aux_ptr = dataPtr_origem + y * widthstep + x * nChan;

                            //subtrair a imagem original, a imagem alterada
                            aux = dataPtr_transformada[0] - dataPtr_origem[0];
                            dataPtr_origem[0] = (byte)(aux < 0 ? dataPtr_origem[0] : aux);

                            aux = dataPtr_transformada[1] - dataPtr_origem[1];
                            dataPtr_origem[1] = (byte)(aux < 0 ? dataPtr_origem[1] : aux);

                            aux = dataPtr_transformada[2] - dataPtr_origem[2];
                            dataPtr_origem[2] = (byte)(aux < 0 ? dataPtr_origem[2] : aux);


                            dataPtr_transformada += nChan; // avançar o apontador para o proximo pixel
                            dataPtr_origem += nChan;
                        }
                        dataPtr_transformada += padding; // contornar os pixeis de padding, para nao lhes tocar s
                        dataPtr_origem += padding;
                    }

                }

            }

        }
        //Open - cria uma abertura na imagem com iterações iter
        public static void Open(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, int[,] mask, int iter)
        {

            for (int k = 0; k < iter; k++)
            {
                Erosion(img, imgCopy, mask);
                imgCopy = img.Copy();
            }
            for(int k = 0; k < iter; k++)
            {
                Dilation(img, imgCopy, mask);
                imgCopy = img.Copy();
            }

        }
        //Open - cria uma abertura na imagem com iterações iter
        public static void Close(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, int[,] mask, int iter)
        {

            for (int k = 0; k < iter; k++)
            {
                Dilation(img, imgCopy, mask);
                imgCopy = img.Copy();
            }
            for (int k = 0; k < iter; k++)
            {
                Erosion(img, imgCopy, mask);
                imgCopy = img.Copy();
            }

        }

        /// <summary>
        /// ImageToByteArray
        /// </summary> Devolve um vetor bidimensional de bytes com a informação da imagem
        /// <param name="img"></param>
        /// <returns></returns> - Matriz de bytes bidimensional com a informação da imagem binária
        public static byte[,] ImageToByteArray(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y, i;
                byte[,] imgByte = new byte[height,width];

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            imgByte[y,x] = (dataPtr + y * widthstep + x * nChan)[0];
                        }
                    }
                }
                return imgByte;
            }
        }
        /// <summary>
        /// ImageToIntArray 
        /// </summary> Cria um array de inteiros apartir de uma imagem binarizada
        /// <param name="img"></param>
        /// <returns></returns>
        public static int[,] ImageToIntArray(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y, i;
                int[,] imgArray = new int[height, width];

                if (nChan == 3)
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            imgArray[y, x] = (dataPtr + y * widthstep + x * nChan)[0];
                        }
                    }
                }
                return imgArray;
            }
        }
        /// <summary>
        /// minIntList
        /// </summary> Permite obter o valor inteiro minimo numa lista
        /// <param name="list"></param> Lista de inteiros
        /// <returns></returns> Int - min - minimo
        private static int minIntList(List<int> list)
        {
            int min;
            min = 0;

            foreach (int num in list)
            {
                if (num <= min)
                {
                    min = num;
                }
            }
            return min;
        }

       
        /// <summary>
        /// Segmentation_four
        /// </summary>
        /// <param name="img"></param>
        /// <param name="imgCopy"></param>
        /// <returns></returns>
        public static int[,] Segmentation_four(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width

                int x, y, ticker;
                int[,] imgArray = new int[height, width];
                //List<int> equivalence_tuple = new List<int>();
                List<List<int>> table = new List<List<int>>();
                List<List<int>> finalTable = new List<List<int>>();
                List<Image> segmentationList = new List<Image>();
                
                if (nChan == 3)
                {
                    ticker = 1;
                    bool assigned = false; // flag para verificar se o píxel já foi observado ou não

                    //converter imagem para array de inteiros
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            imgArray[y, x] = (dataPtr + y * widthstep + x * nChan)[0];
                        }
                    }
                    
                    //1º passo
                    //Atribuiução de uma etiqueta a todos os pixeis não nulos
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            if (imgArray[y, x] == 0) // se o pixel pertencer a um objeto na imagem, atribuir etiqueta
                            {
                                imgArray[y, x] = ticker;
                                ticker++;
                            }
                            else // o pixel é branco, e temos de meter a zero
                            {
                                imgArray[y, x] = 0;
                            }
                        }
                    }

                    // tratar da primeira linha : nao ha equivalencias porque nao ha conflitos entre o pixel de cima!
                    y = 0;
                    for (x = 1; x < width; x++)
                    {
                        if (imgArray[y, x] != 0)
                        {
                            // pixel da esquerda 
                            if (imgArray[y, x - 1] != 0) // se nao for nulo, entao o pixel da esquerda vai ser inferior ao pixel central, e por isso tem de haver atribuição
                            {
                                // e entao temos de declarar o pixel central igual ao da esquerda
                                imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                            }
                        }
                    }

                    // tratar da primeira linha da imagem
                    x = 0;
                    for (y = 0; y < height; y++)
                    {
                        if (imgArray[y, x] != 0)
                        {
                            // pixel de cima
                            if (imgArray[y - 1, x] != 0) // se nao for nulo, entao o pixel de cima vai ser inferior ao pixel central, e por isso tem de haver atribuição
                            {
                                // e entao temos de declarar o pixel central igual ao da esquerda
                                imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                            }
                        }
                    }

                    // tratar do core 
                    //2º passo
                    //Varrimento para atribuir a etiqueta de menor valor na vizinhança de cada pixel e registo de conflito na tabela de equivalências
                    for (y = 1; y < height; y++)
                    {
                        for (x = 1; x < width; x++)
                        {
                            assigned = false; // começa como não observado
                            // se o pixel obdervado for nao nulo
                            if (imgArray[y, x] != 0)
                            {
                                // pixel acima pixel de cima 
                                if (imgArray[y - 1, x] != 0) // se nao for nulo, entao o pixel de cima é inferior ao central de certeza
                                {
                                    imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo 
                                    assigned = true; // declarar atribuição

                                }
                                // pixel acima pixel da esquerda 
                                if (imgArray[y, x - 1] != 0) // se nao for nulo, entao o pixel da esquerda tera uma label que de certeza sera inferior a da direita
                                {
                                    if (assigned) // se houve atribuição, entao o pixel de cima tinha label
                                    {
                                        // nesse caso, temos de declarar equivalencias entre o pixel de cima e o da esquerda
                                        imgArray[y, x-1] = imgArray[y-1,x]; // atribuir valor de mínimo

                                    }
                                    
                                    else// no entanto, caso nao tenham havido atribuições, o pixel da esquerda é inferior ao do centro
                                    {
                                        // e entao temos de declarar o pixel central igual ao da esquerda
                                        imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                    }


                                }

                            }
                        }
                    }

                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            dataPtr[0] = (byte)(imgArray[y, x]);
                            dataPtr[1] = 0;
                            dataPtr[2] = (byte)(imgArray[y, x]);

                            dataPtr += nChan;
                        }
                        dataPtr += padding;
                    }
                }
                return imgArray;
            }
        }

        /// <summary>
        /// Segmentation_four
        /// </summary>
        /// <param name="img"></param>
        /// <param name="imgCopy"></param>
        /// <returns></returns>
        public static int[,] Segmentation_eight(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width

                int x, y, ticker;
                int[,] imgArray = new int[height, width];
                

                if (nChan == 3)
                {
                    ticker = 1;
                    bool top_assigned = false; // flag para verificar se o píxel já foi observado ou não
                    bool diag_left_assigned = false;
                    bool diag_right_assigned = false;
                    
                    //converter imagem para array de inteiros
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            imgArray[y, x] = (dataPtr + y * widthstep + x * nChan)[0];
                        }
                    }
                    
                    //1º passo
                    //Atribuiução de uma etiqueta a todos os pixeis não nulos
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            if (imgArray[y, x] == 0)// se o pixel pertencer a um objeto na imagem, atribuir etiqueta
                            {
                                imgArray[y, x] = ticker;
                                ticker++;
                            }
                            else // o pixel é branco, e temos de meter a zero
                            {
                                imgArray[y, x] = 0;
                            }
                        }
                    }
                    
                    // primeira passagem - top -> bottom / left to right
                    // tratar da primeira linha : nao ha equivalencias porque nao ha conflitos entre o pixel de cima!
                    y = 0;
                    for (x = 0; x < width; x++)
                    {
                        if (imgArray[y, x] != 0)
                        {
                            // pixel da esquerda 
                            if (imgArray[y, x - 1] != 0) // se nao for nulo, entao o pixel da esquerda vai ser inferior ao pixel central, e por isso tem de haver atribuição
                            {
                                // e entao temos de declarar o pixel central igual ao da esquerda
                                imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                            }
                        }
                    }

                    // tratar da primeira linha da imagem
                    x = 0;
                    for (y = 0; y < height; y++)
                    {
                        if (imgArray[y, x] != 0)
                        {
                            // pixel de cima
                            if (imgArray[y - 1, x] != 0) // se nao for nulo, entao o pixel de cima vai ser inferior ao pixel central, e por isso tem de haver atribuição
                            {
                                // e entao temos de declarar o pixel central igual ao da esquerda
                                imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo

                            }
                        }
                    }
                    
                    // tratar do core 
                    //2º passo
                    //Varrimento para atribuir a etiqueta de menor valor na vizinhança de cada pixel e registo de conflito na tabela de equivalências
                    for (y = 1; y < height-1; y++)
                    {
                        for (x = 1; x < width-1; x++)
                        {
                            diag_left_assigned = false;
                            top_assigned = false; // começa como não observado
                            diag_right_assigned = false;
                            // se o pixel obdervado for nao nulo
                            if (imgArray[y, x] != 0)
                            {
                                // pixel na diagonal da esquerda
                                if (imgArray[y - 1, x-1] != 0) // se nao for nulo, entao o pixel na diagonal esquerda de cima é inferior ao central de certeza
                                {
                                    imgArray[y, x] = imgArray[y - 1, x-1]; // atribuir valor de mínimo 
                                    diag_left_assigned = true; // declarar atribuição
                                }

                                // pixel acima pixel de cima 
                                if (imgArray[y - 1, x] != 0) // se nao for nulo, entao o pixel de cima é inferior ao central de certeza
                                {
                                    if (diag_left_assigned)
                                    {
                                        imgArray[y-1, x] = imgArray[y - 1, x-1]; // atribuir valor de mínimo 
                                    }
                                    else
                                    {
                                        imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo 
                                    }
                                    top_assigned = true; // declarar atribuição

                                }
                                // pixel na diagonal a direita
                                if (imgArray[y - 1, x+1] != 0) // se nao for nulo, entao o pixel de cima é inferior ao central de certeza
                                {
                                    if (diag_left_assigned)
                                    {
                                        imgArray[y - 1, x+1] = imgArray[y - 1, x - 1]; // atribuir valor de mínimo 
                                    }
                                    else
                                    {
                                        if (top_assigned) // se o de cima tiver sido atribuido, atribuir ao da diagonal a direita
                                        {
                                            imgArray[y-1, x+1] = imgArray[y - 1, x]; // atribuir valor de mínimo 
                                        }
                                        else
                                        {
                                            imgArray[y, x] = imgArray[y - 1, x+1]; // atribuir valor de mínimo 
                                        }
                                        
                                    }
                                    diag_right_assigned = true; // declarar atribuição

                                }
                                // pixel acima pixel da esquerda 
                                if (imgArray[y, x - 1] != 0) // se nao for nulo, entao o pixel da esquerda tera uma label que de certeza sera inferior a da direita
                                {
                                    if (diag_left_assigned)
                                    {
                                        imgArray[y, x - 1] = imgArray[y -1, x - 1]; // atribuir valor de mínimo 
                                    }
                                    else
                                    {
                                        if (top_assigned) // se o de cima tiver sido atribuido, atribuir ao da diagonal a direita
                                        {
                                            imgArray[y, x - 1] = imgArray[y - 1, x]; // atribuir valor de mínimo 
                                        }
                                        else
                                        {
                                            if(diag_right_assigned) // se o pixel da diagonal a direita foi assigned, entao atribuir
                                            {
                                                imgArray[y, x - 1] = imgArray[y - 1, x + 1]; // atribuir valor de mínimo 
                                            }
                                            else
                                            {
                                                imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo 
                                            }
                                        }

                                    }

                                }
                              

                            }
                        }
                    }

                    

                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            dataPtr[0] = (byte)(imgArray[y, x]);
                            dataPtr[1] = 0;
                            dataPtr[2] = (byte)(imgArray[y, x]);

                            dataPtr += nChan;
                        }
                        dataPtr += padding;
                    }
                }
                return imgArray;
            }
        }
        /// <summary>
        /// IterativeSegmentation
        /// </summary>
        /// <param name="img"></param>
        /// <param name="imgCopy"></param>
        /// <returns></returns>
        public static int[,] IterativeSegmentation(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer();
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width

                int x, y, ticker;
                int[,] imgArray = new int[height, width];
                bool control = false;
                bool assigned = true;
                int count;

                ticker = 1;
                x = y = 0;
                count = 0;


                if (nChan == 3)
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            imgArray[y, x] = (dataPtr + y * widthstep + x * nChan)[0];
                        }
                    }

                    //1º passo
                    //Atribuiução de uma etiqueta a todos os pixeis não nulos
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            if (imgArray[y, x] == 0)// se o pixel pertencer a um objeto na imagem, atribuir etiqueta
                            {
                                imgArray[y, x] = ticker;
                                ticker++;
                            }
                            else // o pixel é branco, e temos de meter a zero
                            {
                                imgArray[y, x] = 0;
                            }
                        }
                    }


                    // Começar o algoritmo iterativo

                    // Passagem top/down left/right

                    while (assigned)
                    {

                        // tratar da primeira linha : nao ha equivalencias porque nao ha conflitos entre o pixel de cima!
                        y = 0;
                        for (x = 0; x < width; x++)
                        {
                            if (imgArray[y, x] != 0)
                            {
                                // Se for o primeiro pixel
                                if (x == 0)
                                {

                                    if (imgArray[y, x + 1] != 0) // pixel da direita
                                    {
                                        if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x + 1] != 0) // pixel inferior direito
                                    {
                                        if (imgArray[y + 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x] != 0) // pixel inferior central
                                    {
                                        if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }

                                // Se for o último pixel
                                else if (x == width - 1)
                                {
                                    if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                    {
                                        if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x - 1] != 0) // pixel inferior esquerdo
                                    {
                                        if (imgArray[y + 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x] != 0) // pixel inferior central
                                    {
                                        if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }

                                else
                                {
                                    if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                    {
                                        if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }

                                    else if (imgArray[y + 1, x - 1] != 0) // pixel inferior esquerdo
                                    {
                                        if (imgArray[y + 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x] != 0) // pixel inferior central
                                    {
                                        if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x + 1] != 0) // pixel inferior direito
                                    {
                                        if (imgArray[y + 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y, x + 1] != 0) // pixel da direita
                                    {
                                        if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }
                            }
                        }


                        //Tratar do core
                        for (y = 1; y < height - 1; y++)
                        {

                            for (x = 0; x < width; x++)
                            {


                                if (imgArray[y, x] != 0)
                                {
                                    // Se for o primeiro pixel
                                    if (x == 0)
                                    {

                                        if (imgArray[y - 1, x] != 0) // pixel superior central
                                        {
                                            if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y - 1, x + 1] != 0) // pixel superior direito
                                        {
                                            if (imgArray[y - 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y - 1, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y, x + 1] != 0) // pixel da direita
                                        {
                                            if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x + 1] != 0) // pixel inferior direito
                                        {
                                            if (imgArray[y + 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y + 1, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x] != 0) // pixel inferior central
                                        {
                                            if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                    }

                                    // Se for o último pixel
                                    else if (x == width - 1)
                                    {
                                        if (imgArray[y - 1, x] != 0) //pixel superior central
                                        {
                                            if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y - 1, x - 1] != 0) // pixel superior esquerdo
                                        {
                                            if (imgArray[y - 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y - 1, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                        {
                                            if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x - 1] != 0) // pixel inferior esquerdo
                                        {
                                            if (imgArray[y + 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y + 1, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x] != 0) // pixel inferior central
                                        {
                                            if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                    }

                                    else // pixeis restantes
                                    {
                                        if (imgArray[y, x - 1] != 0) //  pixel da esquerda
                                        {
                                            if (imgArray[y, x - 1] < imgArray[y, x]) // se o pixel superior esquerdo for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual superior esquerdo
                                                imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y - 1, x - 1] != 0) //  pixel superior esquerdo
                                        {
                                            if (imgArray[y - 1, x - 1] < imgArray[y, x]) //se o pixel superior central for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel superior central
                                                imgArray[y, x] = imgArray[y - 1, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y - 1, x] != 0) // pixel superior central
                                        {
                                            if (imgArray[y - 1, x] < imgArray[y, x]) //se o pixel superior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel superior direito
                                                imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y - 1, x + 1] != 0) // pixel superior direito
                                        {
                                            if (imgArray[y - 1, x + 1] < imgArray[y, x]) //se o pixel superior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior esquerdo
                                                imgArray[y, x] = imgArray[y - 1, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y, x + 1] != 0) // pixel da direita
                                        {
                                            if (imgArray[y, x + 1] < imgArray[y, x]) //se o pixel superior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior esquerdo
                                                imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x + 1] != 0) // pixel inferior direito
                                        {
                                            if (imgArray[y + 1, x + 1] < imgArray[y, x]) //se o pixel inferior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior direito
                                                imgArray[y, x] = imgArray[y + 1, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }

                                        else if (imgArray[y + 1, x] != 0) // pixel inferior central
                                        {
                                            if (imgArray[y + 1, x] < imgArray[y, x]) //se o pixel inferior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior direito
                                                imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }

                                        else if (imgArray[y + 1, x - 1] != 0) // pixel inferior esquerdo
                                        {
                                            if (imgArray[y + 1, x - 1] < imgArray[y, x]) //se o pixel inferior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior direito
                                                imgArray[y, x] = imgArray[y + 1, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Tratar ultima linha
                        y = height - 1;

                        for (x = 0; x < width; x++)
                        {
                            if (imgArray[y, x] != 0)
                            {
                                // Se for o primeiro pixel
                                if (x == 0)
                                {

                                    if (imgArray[y - 1, x] != 0) // pixel superior central
                                    {
                                        if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel superior central for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao pixel superior central
                                            imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x + 1] != 0) // pixel superior direito
                                    {
                                        if (imgArray[y - 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel superior direito for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao pixel superior direito
                                            imgArray[y, x] = imgArray[y - 1, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y, x + 1] != 0) //pixel da direita
                                    {
                                        if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da direita for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da direita
                                            imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }

                                // Se for o último pixel
                                else if (x == width - 1)
                                {
                                    if (imgArray[y - 1, x] != 0) // se nao for nulo, entao o pixel superior central for inferior ao pixel central
                                    {
                                        if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel superior central for inferior ao pixel central
                                        {
                                            imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x - 1] != 0) // pixel superior esquerdo
                                    {
                                        if (imgArray[y - 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y - 1, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                    {
                                        if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }

                                else // Nos pixeis restantes
                                {

                                    if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                    {
                                        if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x - 1] != 0) // pixel superior esquerdo
                                    {
                                        if (imgArray[y - 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel superior direito for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel superior igual ao da esquerda
                                            imgArray[y, x] = imgArray[y - 1, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x] != 0) // pixel superior central
                                    {
                                        if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x + 1] != 0) // pixel superior direito
                                    {
                                        if (imgArray[y - 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y - 1, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y, x + 1] != 0) // pixel direita
                                    {
                                        if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }
                            }
                        }

                        // Se não houve propagação
                        if (!control)
                        {
                            assigned = false;
                            break;
                        }
                        else
                        {
                            control = false;
                            count++;
                        }


                        // Passagem bottom/up right/left

                        //Tratar ultima linha
                        y = height - 1;

                        for (x = width - 1; x >= 0; x--)
                        {
                            if (imgArray[y, x] != 0)
                            {
                                // Se for o primeiro pixel
                                if (x == 0)
                                {

                                    if (imgArray[y, x + 1] != 0) // pixel da direita
                                    {
                                        if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel superior central for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao pixel superior central
                                            imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x + 1] != 0) // pixel superior direito
                                    {
                                        if (imgArray[y - 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel superior direito for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao pixel superior direito
                                            imgArray[y, x] = imgArray[y - 1, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x] != 0) //pixel da direita
                                    {
                                        if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da direita for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da direita
                                            imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }

                                // Se for o último pixel
                                else if (x == width - 1)
                                {
                                    if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                    {
                                        if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel superior central for inferior ao pixel central
                                        {
                                            imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x - 1] != 0) // pixel superior esquerdo
                                    {
                                        if (imgArray[y - 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y - 1, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x] != 0) // pixel superior central
                                    {
                                        if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }

                                else // Nos pixeis restantes
                                {

                                    if (imgArray[y, x + 1] != 0) // pixel da direita
                                    {
                                        if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x + 1] != 0) // pixel superior direito
                                    {
                                        if (imgArray[y - 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel superior direito for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel superior igual ao da esquerda
                                            imgArray[y, x] = imgArray[y - 1, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x] != 0) // pixel superior central
                                    {
                                        if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y - 1, x - 1] != 0) // pixel superior esquerdo
                                    {
                                        if (imgArray[y - 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y - 1, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y, x - 1] != 0) // pixel esquerda
                                    {
                                        if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }
                            }
                        }

                        //Tratar do core
                        for (y = height - 2; y >= 0; y--)
                        {

                            for (x = width - 1; x >= 0; x--)
                            {


                                if (imgArray[y, x] != 0)
                                {
                                    // Se for o primeiro pixel
                                    if (x == 0)
                                    {

                                        if (imgArray[y + 1, x] != 0) // pixel inferior central
                                        {
                                            if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x + 1] != 0) // pixel inferior direito
                                        {
                                            if (imgArray[y + 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y + 1, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y, x + 1] != 0) // pixel da direita
                                        {
                                            if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y - 1, x + 1] != 0) // pixel superior direito
                                        {
                                            if (imgArray[y - 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y - 1, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y - 1, x] != 0) // pixel superior central
                                        {
                                            if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                    }

                                    // Se for o último pixel
                                    else if (x == width - 1)
                                    {
                                        if (imgArray[y + 1, x] != 0) //pixel inferior central
                                        {
                                            if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x - 1] != 0) // pixel inferior esquerdo
                                        {
                                            if (imgArray[y + 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y + 1, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                        {
                                            if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y - 1, x - 1] != 0) // pixel superior esquerdo
                                        {
                                            if (imgArray[y - 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y - 1, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }

                                        else if (imgArray[y - 1, x] != 0) // pixel superior central
                                        {
                                            if (imgArray[y - 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao da esquerda
                                                imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                    }

                                    else // pixeis restantes
                                    {
                                        if (imgArray[y, x + 1] != 0) //  pixel da direita
                                        {
                                            if (imgArray[y, x + 1] < imgArray[y, x]) // se o pixel superior esquerdo for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual superior esquerdo
                                                imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x + 1] != 0) //  pixel inferior direito
                                        {
                                            if (imgArray[y + 1, x + 1] < imgArray[y, x]) //se o pixel superior central for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel superior central
                                                imgArray[y, x] = imgArray[y + 1, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x] != 0) // pixel inferior central
                                        {
                                            if (imgArray[y + 1, x] < imgArray[y, x]) //se o pixel superior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel superior direito
                                                imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y + 1, x - 1] != 0) // pixel inferior esquerdo
                                        {
                                            if (imgArray[y + 1, x - 1] < imgArray[y, x]) //se o pixel superior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior esquerdo
                                                imgArray[y, x] = imgArray[y + 1, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                        {
                                            if (imgArray[y, x - 1] < imgArray[y, x]) //se o pixel superior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior esquerdo
                                                imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                        else if (imgArray[y - 1, x - 1] != 0) // pixel superior esquerdo
                                        {
                                            if (imgArray[y - 1, x - 1] < imgArray[y, x]) //se o pixel inferior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior direito
                                                imgArray[y, x] = imgArray[y + 1, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }

                                        else if (imgArray[y - 1, x] != 0) // pixel superior central
                                        {
                                            if (imgArray[y - 1, x] < imgArray[y, x]) //se o pixel inferior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior direito
                                                imgArray[y, x] = imgArray[y - 1, x]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }

                                        else if (imgArray[y - 1, x + 1] != 0) // pixel inferior esquerdo
                                        {
                                            if (imgArray[y - 1, x + 1] < imgArray[y, x]) //se o pixel inferior direito for inferior ao pixel central
                                            {
                                                // e entao temos de declarar o pixel central igual ao pixel inferior direito
                                                imgArray[y, x] = imgArray[y - 1, x + 1]; // atribuir valor de mínimo
                                                control = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // tratar da primeira linha : nao ha equivalencias porque nao ha conflitos entre o pixel de cima!
                        y = 0;
                        for (x = width - 1; x >= 0; x--)
                        {
                            if (imgArray[y, x] != 0)
                            {
                                // Se for o primeiro pixel
                                if (x == 0)
                                {

                                    if (imgArray[y + 1, x] != 0) // pixel inferior central
                                    {
                                        if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x + 1] != 0) // pixel inferior direito
                                    {
                                        if (imgArray[y + 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y, x + 1] != 0) // pixel da direita
                                    {
                                        if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }

                                // Se for o último pixel
                                else if (x == width - 1)
                                {
                                    if (imgArray[y + 1, x] != 0) // pixel inferior central
                                    {
                                        if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x - 1] != 0) // pixel inferior esquerdo
                                    {
                                        if (imgArray[y + 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                    {
                                        if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }

                                else
                                {
                                    if (imgArray[y, x + 1] != 0) // pixel da direita
                                    {
                                        if (imgArray[y, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }

                                    else if (imgArray[y + 1, x + 1] != 0) // pixel inferior direito
                                    {
                                        if (imgArray[y + 1, x + 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x + 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x] != 0) // pixel inferior central
                                    {
                                        if (imgArray[y + 1, x] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y + 1, x - 1] != 0) // pixel inferior esquerdo
                                    {
                                        if (imgArray[y + 1, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y + 1, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                    else if (imgArray[y, x - 1] != 0) // pixel da esquerda
                                    {
                                        if (imgArray[y, x - 1] < imgArray[y, x]) // se nao for nulo, entao o pixel da esquerda for inferior ao pixel central
                                        {
                                            // e entao temos de declarar o pixel central igual ao da esquerda
                                            imgArray[y, x] = imgArray[y, x - 1]; // atribuir valor de mínimo
                                            control = true;
                                        }
                                    }
                                }
                            }
                        }

                        // Se não houve propagação
                        if (!control)
                            assigned = false;
                        else
                        {
                            control = false;
                            count++;
                        }

                    }

                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            dataPtr[0] = (byte)(imgArray[y, x]);
                            dataPtr[1] = 0;
                            dataPtr[2] = (byte)(imgArray[y, x]);

                            dataPtr += nChan;
                        }
                        dataPtr += padding;
                    }
                }
                return imgArray;
            }

        }

        /// <summary>
        /// ProjectionSegmentation 
        /// </summary> Coloca todos os píxeis da imagem fora do rectangulo definido pelos limites X1 - X2 / Y1 -> Y2 a branco
        /// <param name="img"></param>
        /// <param name="imgCopy"></param>
        /// <param name="X1"></param>
        /// <param name="X2"></param>
        /// <param name="Y1"></param>
        /// <param name="Y2"></param>
        public static void ProjectionSegmentation(Image<Bgr, byte> img, int X1, int X2, int Y1, int Y2)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* aux_ptr;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                if(nChan == 3)
                {
                    for (y = 0; y < Y1; y ++ ){
                        for (x = 0; x < width; x++)
                        {
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = 255;
                            aux_ptr[1] = 255;
                            aux_ptr[2] = 255;   
                        }
                    }

                    for (y = Y2+1; y < height; y++)
                    {
                        for (x = 0 ; x < width; x++)
                        {
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = 255;
                            aux_ptr[1] = 255;
                            aux_ptr[2] = 255;
                        }
                    }
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < X1; x++)
                        {
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = 255;
                            aux_ptr[1] = 255;
                            aux_ptr[2] = 255;
                        }
                    }

                    for (y =0; y < height; y++)
                    {
                        for (x = X2+1; x < width; x++)
                        {
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = 255;
                            aux_ptr[1] = 255;
                            aux_ptr[2] = 255;
                        }
                    }

                }
            }
        }



        // IdentifyCB : procura os limites (verticais ou horizontais) do codigo de barras numa imagem com ruido
        public static void IdentifyCB(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy)
        {
            unsafe
            {
                // guardar uma cópia original da imagem
                Image<Bgr, byte> imgCopy2 = img.Copy();

                int[] vert_proj = new int[img.Height];
                int[] vert_proj_meanfilt = new int[img.Height];
                int[] vert_proj_matchfilt = new int[img.Height];
                int[] horz_proj = new int[img.Width];
                int[] horz_proj_meanfilt = new int[img.Width];
                int[] horz_proj_matchfilt = new int[img.Width];
                //binarizar a imagem
                ConvertToBW_Otsu(img);
                //aplicar filtro de sobel para encontrar as zonas de alto contraste da imagem
                imgCopy = img.Copy();
                Roberts(img, imgCopy);
                // depois do roberts aplicamos uma transformada Black Hat ao negativo da imagem
                Negative(img);
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
                BlackHat(img, maskBlackHat);

                //// aplicar uma dilatação 
                imgCopy = img.Copy();
                int[,] maskDilation = new int[3, 3];

                for (i = 0; i < maskDilation.GetLength(0); i++)
                {
                    for (j = 0; j < maskDilation.GetLength(1); j++)
                    {
                        maskDilation[i, j] = 1;
                    }
                }
                Dilation(img, imgCopy, maskDilation);
                //// aplicar filtro de mediana para eliminar ruido impulsivo resultante de possiveis letras 

                ////aplicar uma abertura com 10 iterações
                //// criar mascara em cruz
                int[,] maskOpen = new int[11, 11];
                for (i = 0; i < maskOpen.GetLength(0); i++)
                {
                    for (j = 0; j < maskOpen.GetLength(1); j++)
                    {
                        maskOpen[i, j] = 0;
                    }
                }
                for (i = 0; i < maskOpen.GetLength(0); i++)
                {
                    maskOpen[i, (int)(maskOpen.GetLength(1) / 2)] = 1;
                }
                for (j = 0; j < maskOpen.GetLength(1); j++)
                {
                    maskOpen[(int)(maskOpen.GetLength(0) / 2), j] = 1;
                }
                imgCopy = img.Copy();       
                Open(img, imgCopy, maskOpen, 1);

                ////aplicar uma nova abertura com 3 iterações
                imgCopy = img.Copy();
                Open(img, imgCopy, maskOpen, 2);

                //// de seguida fazemos duas dilatações e metemos a imagem em negativo (objeto preto sobre fundo branco)
                imgCopy = img.Copy();
                Dilation(img, imgCopy, maskDilation);
                //imgCopy = img.Copy();
                //Dilation(img, imgCopy, maskDilation);
                Negative(img);
                // fechar os restantes buracos que possam afetar o calculo do centroide e da rotação
                //imgCopy = img.Copy();
                //Close(img, imgCopy, maskOpen, 7);

                // fazer uma erosao com kernel enorme para eliminar ruido remanescente
                int[,] maskErosion = new int[15, 15];
                for (i = 0; i < maskErosion.GetLength(0); i++)
                {
                    for (j = 0; j < maskErosion.GetLength(1); j++)
                    {
                        maskErosion[i, j] = 1;
                    }
                }
                imgCopy = img.Copy();
                Close(img, imgCopy, maskErosion, 6);
                
                // fazer segmentação

                // detetar os limites do código de barras 
                //meter a imagem em tons de negativo para poder detetar as zonas de transição de contraste atraves do histograma
                //Negative(img);
                //binarizar de novo para garantir que todos os pixeis estao a preto ou branco
                //ConvertToBW_Otsu(img);
                // aplicar projeção vertical
                vert_proj = VerticalProjection(img);
                // aplicar projeção horizontal
                horz_proj = HorizontalProjection(img);
                //aplicar filtro  de media de ordem 20 na serie de dados de projeção vertical para suavizar o possivel ruido nos histogramas
                vert_proj_meanfilt = MeanFilter(vert_proj, 20);
                // aplicar math filter para determinar os limites do bloco resultante das 
                vert_proj_matchfilt = MatchFilter(vert_proj_meanfilt, 20, 0.3);
                //aplicar filtro  de media de ordem 20 na serie de dados de projeção  horizontal para suavizar o possivel ruido nos histogramas
                horz_proj_meanfilt = MeanFilter(horz_proj, 20);
                //aplicar match filter para detetar os limites do codigo de barras
                horz_proj_matchfilt = MatchFilter(horz_proj_meanfilt, 20, 0.3);
                // atraves das caracteristicas resultantes do match e do mean filter, podemos determinar os limites do codigo de barras
                
                //detetar o primeiro maximo do match filter na vertical 
                

                // detetar o centroid do objeto, e com base no centroid, e nas dimensoes do objeto

                // rodar imagem para o angulo correto
                imgCopy = img.Copy();
                //angleFinder(img, imgCopy);
            }
        }
        /// <summary>
        /// averageInt 
        /// </summary> calcula o valor médio de uma serie de dados 
        /// <param name="projection"></param>
        /// <returns></returns>
        private static double averageInt(int[] projection)
        {
            double sum = 0;
            for (int k = 0; k < projection.Length; k++)
            {
                sum += projection[k];
            }
            return sum / projection.Length;
        }

        /// <summary>
        /// probabilityBC
        /// </summary> calcula a probabilidade de haver um codigo de barras atraves das projeçoes de transições verticais e horizontais
        /// <param name="countTransitionVertical"></param>
        /// <param name="countTransitionHorizontal"></param>
        /// <returns></returns>
        public static double probabiltyBC(int[] countTransitionVertical, int[] countTransitionHorizontal)
        {
            unsafe
            {

                double probabilty = 0.0;

                int x, y, i;
                double avgVertical, avgHorizontal;
                avgVertical = avgHorizontal = 0.0;

                //procurar o valor máximo de transições na projecção vertical
                for (i = 0; i < countTransitionVertical.Length; i++)
                {
                    avgVertical = averageInt(countTransitionVertical);
                }

                //procurar o valor máximo de transições na projecção horizontal 
                for (i = 0; i < countTransitionHorizontal.Length; i++)
                {
                    avgHorizontal = averageInt(countTransitionHorizontal);

                }

                probabilty = Math.Abs(avgHorizontal - avgVertical) / Math.Max(avgHorizontal, avgVertical);

                return probabilty;
            }
        }

        /// <summary>
        /// horizontalTransitionProjection 
        /// </summary> calcula a projeção do numero de transições de branco para preto e vice versa ao longo das linhas
        /// <param name="img"></param>
        /// <returns></returns>
        public static int[] horizontalTransitionProjection(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte* aux_ptr;
                byte* aux_ptr_prev;
                int[] countTransition = new int[width];

                for (y = 0; y < height; y++) // a cada coluna da imagem
                {
                    for (x = 1; x < width; x++) // a cada linha da imagem
                    {

                        aux_ptr_prev = dataPtr + (x) * nChan + (y - 1) * widthstep;
                        aux_ptr = dataPtr + x * nChan + y * widthstep;

                        if (aux_ptr[0] == 0 && aux_ptr_prev[0] == 255 || aux_ptr[0] == 255 && aux_ptr_prev[0] == 0) //detectar uma transição entre pixeis pretos e brancos e vice versa 
                            countTransition[x]++;

                    }
                }
                return countTransition;
            }
        }

        /// <summary>
        /// verticalTransitionProjection
        /// </summary> calcula a projeção do numero de transições de branco para preto e vice versa ao longo das colunas
        /// <param name="img"></param>
        /// <returns></returns>
        public static int[] verticalTransitionProjection(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte* aux_ptr;
                byte* aux_ptr_prev;
                int[] countTransition = new int[height];

                for (y = 0; y < height; y++) // a cada coluna da iamgem
                {
                    for (x = 1; x < width; x++) // a cada linha da imagem
                    {

                        aux_ptr_prev = dataPtr + (x - 1) * nChan + (y) * widthstep;
                        aux_ptr = dataPtr + x * nChan + y * widthstep;

                        if (aux_ptr[0] == 0 && aux_ptr_prev[0] == 255 || aux_ptr[0] == 255 && aux_ptr_prev[0] == 0) //detectar uma transição entre pixeis pretos e brancos e vice versa 
                            countTransition[y]++;

                    }
                }
                return countTransition;
            }
        }

        /// <summary>
        /// HorizontalStripImg
        /// </summary> Permite ir buscar uma faixa de bits horizontal do codigo de barras
        /// <param name="img"></param>
        /// <param name="centroidY"></param> ponto central do bloco em Y
        /// <param name="offsetY"></param> permite dizer o offset do ponto do centroideem Y
        public static void HorizontalStripImg(Image<Bgr, byte> img, int centroidY, int offsetY) 
        {
            unsafe
            {
                MIplImage m = img.MIplImage;

                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                byte* aux_ptr;
                int threshold = 1;
                if(nChan == 3)
                {
                    for (y = 0; y < (centroidY + offsetY)- threshold; y++) // a cada coluna da iamgem
                    {
                        for (x = 0; x < width; x++) // a cada linha da imagem
                        {
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = 255;
                            aux_ptr[1] = 255;
                            aux_ptr[2] = 255;
                        }
                    }
                    for (y = (centroidY + offsetY) + threshold; y < height; y++) // a cada coluna da iamgem
                    {
                        for (x = 0; x < width; x++) // a cada linha da imagem
                        {
                            aux_ptr = dataPtr + y * widthstep + x * nChan;
                            aux_ptr[0] = 255;
                            aux_ptr[1] = 255;
                            aux_ptr[2] = 255;
                        }
                    }
                }
                
            }
        }

        

        /// <summary>
        ///
        /// </summary>
        /// <param name="img"></param>
        /// <param name="imgCopy"></param>
        /// <param name="centroid"></param>
        /// <param name="verticalProjection"></param>
        public static int[] IsolateNumbersBC(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, int y_centroid, int x_centroid)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the final image
                byte* aux_ptr;
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding
                int widthstep = m.widthStep; // padding + width
                int x, y;
                int beginCol, endCol;
                int begin;
                double threshold, thresholdFactor;
                int minWidth, colWidth;
                bool bcFlag = false;

                int[] horizontalProj = HorizontalProjection(img);
                int[] verticalProj = VerticalProjection(img);

                beginCol = 0;
                begin = 0;
                threshold = 0;
                minWidth = 0;
                thresholdFactor = 0.7;

                threshold = thresholdFactor * verticalProj.Max();

                for (int i = y_centroid; i < verticalProj.Length; i++)
                {

                    if (verticalProj[i] < threshold)
                    {
                        begin = i;
                        break;
                    }
                }

                // limpar a imagem até as linhas de início dos números
                for (y = 0; y < begin; y++)
                {
                    for (x = 0; x < width; x++)
                    {
                        aux_ptr = dataPtr + y * widthstep + x * nChan;
                        aux_ptr[0] = 255;
                        aux_ptr[1] = 255;
                        aux_ptr[2] = 255;
                    }
                }

                // Retirar o lixo da projeção horizontal verificando os índices de cada objeto da projeção horizontal
                for (int i = 0; i < horizontalProj.Length; i++)
                {
                    if (horizontalProj[i] >= 0)
                    {
                        beginCol = i;
                        bcFlag = true;
                    }

                    if (horizontalProj[i] < 3)
                    {
                        endCol = i;
                        colWidth = endCol - beginCol;

                        if (colWidth == minWidth)
                        {
                            for (int j = beginCol; j < endCol + 1; j++)
                            {
                                horizontalProj[i] = 0;
                            }
                        }
                    }
                }

                // A partir da largura de cada projeção sabemos a localização dos números

                // Saber qual as linhas restantes da projeção resultante da segmentação(lixo)

                horizontalProj = HorizontalProjection(img);

                for (int i = 0; i < horizontalProj.Length; i++)
                {
                    if (horizontalProj[i] > 3)
                    {
                        beginCol = i;
                        bcFlag = true;
                    }

                    if (horizontalProj[i] < 3)
                    {
                        endCol = i;
                        colWidth = endCol - beginCol;

                        if (minWidth == 0) { minWidth = colWidth; }

                        else if (minWidth > colWidth) { minWidth = colWidth; }


                        bcFlag = false;
                    }
                }

                
                // para cada numero, calcular centroide width e height do numero, resize à imagem do numero da base de dados, inserir o numero numa imagem na posição do centroide, comparar imagems


                return horizontalProj;
            }
        }

        /// <summary>
        /// DigitCompare
        /// </summary>compara uma regiao de uma imagem contendo um numero com as várias imagens de números eistentes, calculando o seu grau de semelhança
        /// <param name="imgList"></param>
        /// <param name="img"></param>
        /// <returns></returns>
        private static int DigitCompare(List<Image<Bgr,byte>> imgList , Image<Bgr,byte> img)
        {
            return 0;
        }

        // BC_DigitExtractor - permite realçar, extrair e identificar os digitos que existem no código de barras, convertendo-os para um
        // array de inteiros que permite confirmar os digitos identificados 
        private static int[] BC_DigitExtractor()
        {
            return new int[2] { 0,0} ;
        }
        private static int BC_VerifyingDigitExtractor()
        {
            return 0;
        }
        //BC_DigitDecoder - permite descodificar o código de barras segundo o protocolo EAN-13, sabendo o primeiro digito
        private static int[] BC_DigitDecoder(int[] black_list)
        {
            int[,] barcode_bit = new int[12, 7]; // tirando o primeiro digito
            int[] barcode_digit = new int[13]; // tirando o primeiro digito
            int[] bit_sequence1 = new int[96]; // vetor de sequencia de bits, a contar com os separadores de grupos de digitos
            int[] bit_sequence2 = new int[84]; // nao contando com os separadores de grupos de digitos
            

            double factor = 0.3; // 60 % da gama de valores do cogio de barras
            //descodificaçao da projeçao horizontal de pixeis
            //detetar miinimo e maximo do black_list
            //valor de threshold apartir do 
            int blkListMin = black_list.Min();
            int blkListMax = black_list.Max();
            int threshold = (int)Math.Round(factor * (blkListMax - blkListMin)) + blkListMin ; // somar o valor minimo do numero de pixeis a preto
                                                                                              // para contornar eventuais offsets na projecao horizontal
                                                                                              //encontrar o numero de colunas correspondente a uma barra preta - 1 bit
            int numColBar = 0;
            int col_init = 0;
            int col_end = 0;
            int verifying_digit = -1;
            // percorrer  a projecao horizontal ate encontrara primeira barrinha 
            //que indica o inicio do codigo de barras
            bool bcFlag = false;
            for (int k = 0; k < black_list.Length; k++)
            {
                if ((black_list[k] >= threshold) && !bcFlag)// se detetarmos a primeira barrinha, 
                {
                    bcFlag = true; // detetar o inicio do CB
                    col_init = k;
                    break;
                }
            }

            bcFlag = false;
            for (int k = black_list.Length - 1; k > -1; k--)
            {
                if ((black_list[k] >= threshold) && !bcFlag)// se detetarmos a primeira barrinha, 
                {
                    bcFlag = true; // detetar o inicio do CB
                    col_end = k;
                    break;
                }
            }

            numColBar = (int)Math.Round((double)(col_end - col_init) / bit_sequence1.Length);
            int offset = (int)((numColBar-1)/ 2);
            bcFlag = false;
            //tendo o numero de colunas de uma barrinha de um bit, temos de separar o codigo em varias barras
            int bit_index = 0;
            for (int k = col_init-numColBar; (bit_index < bit_sequence1.Length); k++)
            {
                if ((k-col_init+numColBar) % numColBar == 0) // se o indice for multiplo do numero de colunas correspondente a cada barra
                {
                    if (black_list[k+offset] >= threshold) // verificar se temos uma barra preta
                    {
                        bit_sequence1[bit_index] = 1;
                    }
                    else
                    {
                        bit_sequence1[bit_index] = 0; // ou uma barra branca
                    }

                    bit_index++;
                }
            }
            // retirar os bits associados aos separadores de grupos de digitos
            // primeiros 5 bits - bits que indicam o inicio do codigo de barras
            // bits nas posições 46, 47, 48 ,49, 50 - sao os separadores intermedios
            // ultimos 4 bits sao os ultimos separadores -  95, 94, 93, 92
            int j = 0;
            for (int k = 4; k < 46; k ++ )
            {
                bit_sequence2[j] = bit_sequence1[k];
                j++;
            }
            for (int k = 51; k <93 ; k++)
            {
                bit_sequence2[j] = bit_sequence1[k];
                j++;
            }
            //separar o codigo de barras em grupos de 7 bits
            j = 0;
            int n = 0;
            for (int k = 0; j < barcode_bit.GetLength(0); k++)
            {
                
                if (k % barcode_bit.GetLength(1) == 0 && k != 0)
                {
                    j++;
                    k = 0;
                }
                if (j < 12)
                    barcode_bit[j, k] = bit_sequence2[n];
                n++;
            }
            //descobrir o codigo dos primeiros 6 digitos para descobrir o digito verificador
            char[] first_six_digit_code = new char[6];
            for(j = 0; j < first_six_digit_code.Length; j++)
            {
                for(int k = 0; k < digit_coding_R.GetLength(0); k++)
                {

                    if (ArrayEqual(GetRow(barcode_bit, j), LcodeFromRcode(GetRow(digit_coding_R, k)))==digit_coding_R.GetLength(1))
                    {
                        first_six_digit_code[j] = 'L';
                        break; // sair do for
                    }
                    // se não for L, entao será G
                    if (ArrayEqual(GetRow(barcode_bit, j), GcodeFromRcode(GetRow(digit_coding_R, k)))== digit_coding_R.GetLength(1))
                    {
                        first_six_digit_code[j] = 'G';
                        break; // sair do for
                    }
                }
            }
            // se seguida, tendo o código de letras dos seis primeiros dígitos do CB, consigo ir buscar o dígito verificador
            for(j = 0; j < first_6digit_group_code.GetLength(0); j++)
            {
                if (StringEqual(GetString(first_6digit_group_code, j), first_six_digit_code))
                {
                    verifying_digit = j;
                    break;
                }
                    
            }
            barcode_digit[0] = verifying_digit;
            //transformar os primeiros seis digitos do codigo de barras em bits para digito
            unsafe
            {
                int prob = 0;
                for (int k = 1; k < first_six_digit_code.Length+1; k++)
                {
                    prob = 0;
                    switch (first_six_digit_code[k-1])
                    {
                        case 'L': //caso seja um digito de codigo L, comparar o digito em questao com os digitos de codigo L
                            {
                                for (j = 0; j < digit_coding_R.GetLength(0); j++)
                                {
                                    if ( ArrayEqual(GetRow(barcode_bit,k-1), LcodeFromRcode( GetRow( digit_coding_R,j ) ) ) == digit_coding_R.GetLength(1))
                                    {
                                        barcode_digit[k] = j;
                                        break; // sair do for
                                    }
                                    // caso nenhum tenha sido encontrado, procurar por probabilidade maxima
                                    // fazemos um right shift, e vamos procurar o codigo de maior probabilidade
                                    if (ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j)) > prob)
                                    {
                                        prob = ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j));
                                        barcode_digit[k] = j;
                                    }
                                }
                            }
                            break;
                        case 'G'://caso seja um digito de codigo G, comparar o digito em questao com os digitos de codigo G
                            {
                                for (j = 0; j < digit_coding_R.GetLength(0); j++)
                                {
                                    if (ArrayEqual(GetRow(barcode_bit, k-1), GcodeFromRcode(GetRow(digit_coding_R, j))) == digit_coding_R.GetLength(1))
                                    {
                                        barcode_digit[k] = j;
                                        break; // sair do for
                                    }
                                    // caso nenhum tenha sido encontrado, procurar por probabilidade maxima
                                    // fazemos um right shift, e vamos procurar o codigo de maior probabilidade
                                    if (ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j)) > prob)
                                    {
                                        prob = ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j));
                                        barcode_digit[k] = j;
                                    }
                                }
                                
                            }
                            break;
                    }
                }
                // transformar os seis digitos restantes

                for(int k = first_six_digit_code.Length+1; k < barcode_digit.Length; k++)
                {
                    prob = 0;
                    for (j = 0; j < digit_coding_R.GetLength(0); j++)
                    {
                        if ( ArrayEqual(GetRow(barcode_bit, k-1), GetRow(digit_coding_R, j) ) == digit_coding_R.GetLength(1))
                        {
                            barcode_digit[k] = j;
                            break; // sair do for
                        }
                        //caso nenhum tenha sido encontrado, procurar por probabilidade maxima
                        //if (ArrayEqual(GetRow(barcode_bit, k - 1), GetRow(digit_coding_R, j)) > prob)
                        //{
                        //    prob = ArrayEqual(GetRow(barcode_bit, k - 1), GetRow(digit_coding_R, j));
                        //    barcode_digit[k] = j;
                        //}
                        //caso nenhum tenha sido encontrado, procurar por probabilidade maxima pela segunda vez
                        // fazemos um right shift, e vamos procurar o codigo de maior probabilidade
                        if (ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j)) > prob)
                        {
                            prob = ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j));
                            barcode_digit[k] = j;
                        }
                    }

                }

            }

            //return string.Join("",barcode_digit);
            return barcode_digit;
        }

        //BC_DigitDecoderv2 - permite descodificar o código de barras segundo o protocolo EAN-13, apartir de uma segmentação das barras do codigo de barras
        private static int[] BC_DigitDecoderv2(int[] colour_list) // recebe uma lista que é a conversao de uma linha da imagem segmentada para array de inteiros
        {
            int[,] barcode_bit = new int[12, 7]; // tirando o primeiro digito
            int[] barcode_digit = new int[13]; // tirando o primeiro digito
            int[] bit_sequence1 = new int[96]; // vetor de sequencia de bits, a contar com os separadores de grupos de digitos
            int[] bit_sequence2 = new int[84]; // nao contando com os separadores de grupos de digitos

            int numColBar = 0;
            int col_init = 0;
            int col_end = 0;
            int verifying_digit = -1;
            
            // percorrer  a projecao horizontal ate encontrara primeira barrinha 
            //que indica o inicio do codigo de barras
            bool bcFlag = false;
            for (int k = 0; k < colour_list.Length; k++)
            {
                if ((colour_list[k] > 0) && !bcFlag)// se detetarmos a primeira barrinha, 
                {
                    bcFlag = true; // detetar o inicio do CB
                    col_init = k;
                    break;
                }
            }

            bcFlag = false;
            for (int k = colour_list.Length - 1; k > -1; k--)
            {
                if ((colour_list[k] > 0) && !bcFlag)// se detetarmos a ultima barrinha, 
                {
                    bcFlag = true; // detetar o inicio do CB
                    col_end = k;
                    break;
                }
            }
            
            int numBitsZero = 0;
            int numBitsOne = 0;
            numColBar = (int)Math.Round((double)(col_end - col_init) / bit_sequence1.Length);
            int offset = (int)((numColBar - 1) / 2);
            bcFlag = false;
            int bit_index = 0;
            bool currentZero = true;
            bool currentOne = false;
            int u = col_init - numColBar;
            
            while (bit_index < bit_sequence1.Length )
            {
                while (u < colour_list.Length && colour_list[u]==0 )
                {
                    if (currentOne) // se o anterior era um 1
                    {
                        currentOne = false; currentZero = true;
                        // temos de verificar quantos pixeis da barraa 1 detetamos
                        if ((int)Math.Abs(numColBar - numBitsOne) < ((numColBar/2) + 1) && bit_index < bit_sequence1.Length) // se  numero de pixeis a um detetados for equivalente ao de uma barra de um bit (com tolerancia de 2 pixeis)
                        {
                            // entao sabemos que detetamos uma barra de 1 e temos de reiniciar a contagem
                            bit_sequence1[bit_index] = 1;
                            
                            bit_index++;
                        }
                        numBitsOne = 0;
                    }
                    numBitsZero++;
                    // no caso de nao ter havido transição
                    if (numBitsZero % numColBar == 0 && bit_index < bit_sequence1.Length) // detetamos um um adicional
                    {
                        bit_sequence1[bit_index] = 0;
                        bit_index++;
                        numBitsZero = 0;
                    }
                    u++;
                }
                
                while(u < colour_list.Length && colour_list[u]>0)
                {
                    if (currentZero) // se o anterior era um zero
                    {
                        currentOne = true; currentZero = false;
                        // temos de verificar quantos pixeis da barraa zero detetamos
                        if ((int)Math.Abs(numColBar - numBitsZero) < ((numColBar / 2) + 1) && bit_index<bit_sequence1.Length) // se  numero de pixeis a zero detetados for equivalente ao de uma barra de um bit (com tolerancia de 3 pixeis)
                        {
                            // entao sabemos que detetamos uma barra de zero e temos de reiniciar a contagem
                            bit_sequence1[bit_index] = 0;
                            
                            bit_index++;
                        }
                        numBitsZero = 0;
                    }
                    numBitsOne++;
                    if (numBitsOne % numColBar == 0 && bit_index < bit_sequence1.Length) // detetamos um um adicional
                    {
                        bit_sequence1[bit_index] = 1;
                        bit_index++;
                        numBitsOne = 0;
                    }
                    u++;
                }

            }
            
            // retirar os bits associados aos separadores de grupos de digitos
            // primeiros 5 bits - bits que indicam o inicio do codigo de barras
            // bits nas posições 46, 47, 48 ,49, 50 - sao os separadores intermedios
            // ultimos 4 bits sao os ultimos separadores -  95, 94, 93, 92
            int j = 0;
            for (int k = 4; k < 46; k++)
            {
                bit_sequence2[j] = bit_sequence1[k];
                j++;
            }
            for (int k = 51; k < 93; k++)
            {
                bit_sequence2[j] = bit_sequence1[k];
                j++;
            }
            
            //separar o codigo de barras em grupos de 7 bits
            j = 0;
            int n = 0;
            for (int k = 0; j < barcode_bit.GetLength(0); k++)
            {

                if (k % barcode_bit.GetLength(1) == 0 && k != 0)
                {
                    j++;
                    k = 0;
                }
                if (j < 12)
                    barcode_bit[j, k] = bit_sequence2[n];
                n++;
            }
            
            //descobrir o codigo dos primeiros 6 digitos para descobrir o digito verificador
            char[] first_six_digit_code = new char[6];
            for (j = 0; j < first_six_digit_code.Length; j++)
            {
                for (int k = 0; k < digit_coding_R.GetLength(0); k++)
                {

                    if (ArrayEqual(GetRow(barcode_bit, j), LcodeFromRcode(GetRow(digit_coding_R, k))) == digit_coding_R.GetLength(1))
                    {
                        first_six_digit_code[j] = 'L';
                        break; // sair do for
                    }
                    // se não for L, entao será G
                    if (ArrayEqual(GetRow(barcode_bit, j), GcodeFromRcode(GetRow(digit_coding_R, k))) == digit_coding_R.GetLength(1))
                    {
                        first_six_digit_code[j] = 'G';
                        break; // sair do for
                    }
                }
            }
            // se seguida, tendo o código de letras dos seis primeiros dígitos do CB, consigo ir buscar o dígito verificador
            for (j = 0; j < first_6digit_group_code.GetLength(0); j++)
            {
                if (StringEqual(GetString(first_6digit_group_code, j), first_six_digit_code))
                {
                    verifying_digit = j;
                    break;
                }

            }
            barcode_digit[0] = verifying_digit;
            //transformar os primeiros seis digitos do codigo de barras em bits para digito
            unsafe
            {
                int prob = 0;
                for (int k = 1; k < first_six_digit_code.Length + 1; k++)
                {
                    prob = 0;
                    switch (first_six_digit_code[k - 1])
                    {
                        case 'L': //caso seja um digito de codigo L, comparar o digito em questao com os digitos de codigo L
                            {
                                for (j = 0; j < digit_coding_R.GetLength(0); j++)
                                {
                                    if (ArrayEqual(GetRow(barcode_bit, k - 1), LcodeFromRcode(GetRow(digit_coding_R, j))) == digit_coding_R.GetLength(1))
                                    {
                                        barcode_digit[k] = j;
                                        break; // sair do for
                                    }
                                    // caso nenhum tenha sido encontrado, procurar por probabilidade maxima
                                    // fazemos um right shift, e vamos procurar o codigo de maior probabilidade
                                    if (ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j)) > prob)
                                    {
                                        prob = ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j));
                                        barcode_digit[k] = j;
                                    }
                                }
                            }
                            break;
                        case 'G'://caso seja um digito de codigo G, comparar o digito em questao com os digitos de codigo G
                            {
                                for (j = 0; j < digit_coding_R.GetLength(0); j++)
                                {
                                    if (ArrayEqual(GetRow(barcode_bit, k - 1), GcodeFromRcode(GetRow(digit_coding_R, j))) == digit_coding_R.GetLength(1))
                                    {
                                        barcode_digit[k] = j;
                                        break; // sair do for
                                    }
                                    // caso nenhum tenha sido encontrado, procurar por probabilidade maxima
                                    // fazemos um right shift, e vamos procurar o codigo de maior probabilidade
                                    if (ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j)) > prob)
                                    {
                                        prob = ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j));
                                        barcode_digit[k] = j;
                                    }
                                }

                            }
                            break;
                    }
                }
                // transformar os seis digitos restantes

                for (int k = first_six_digit_code.Length + 1; k < barcode_digit.Length; k++)
                {
                    prob = 0;
                    for (j = 0; j < digit_coding_R.GetLength(0); j++)
                    {
                        if (ArrayEqual(GetRow(barcode_bit, k - 1), GetRow(digit_coding_R, j)) == digit_coding_R.GetLength(1))
                        {
                            barcode_digit[k] = j;
                            break; // sair do for
                        }
                        
                        //caso nenhum tenha sido encontrado, procurar por probabilidade maxima
                        //caso nenhum tenha sido encontrado, procurar por probabilidade maxima pela segunda vez
                        // fazemos um right shift, e vamos procurar o codigo de maior probabilidade
                        if (ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j)) > prob)
                        {
                            prob = ArrayEqual(RightShift(GetRow(barcode_bit, k - 1)), GetRow(digit_coding_R, j));
                            barcode_digit[k] = j;
                        }
                    }
                }
            }

            return barcode_digit;
        }


        /// <summary>
        /// BC_size
        /// </summary> Calcula os parametros geometricos do codigo de barras
        /// <param name="matchFilterHorizontal"></param>
        /// <param name="matchFilterVertical"></param>
        /// <returns></returns>
        public static int[] BC_size(int[] matchFilterHorizontal, int[] matchFilterVertical)
        {
            int i, rectWidth, rectHeight;
            int maxHorizontal, minHorizontal, maxVertical, minVertical;
            int[] size = new int[2];

            maxHorizontal = minHorizontal = maxVertical = minVertical = rectHeight = rectWidth = 0;

            //obter o valor máximo e minimo do match filter horizontal, portanto vou ter as coordenadas do Y

            //maxHorizontal = matchFilterHorizontal.ToList().IndexOf(matchFilterHorizontal.Min());
            maxHorizontal = matchFilterHorizontal.Max();
            minHorizontal = matchFilterHorizontal.Min();
            //minHorizontal = matchFilterHorizontal.ToList().IndexOf(matchFilterHorizontal.Max());
            //obter o valor máximo e minimo do match filter vertical, portanto vou ter as coordenadas do X
            maxVertical = matchFilterVertical.Max();
            minVertical = matchFilterVertical.Min();
            //maxVertical = matchFilterVertical.ToList().IndexOf(matchFilterVertical.Min());
            //minVertical = matchFilterVertical.ToList().IndexOf(matchFilterVertical.Max());
            rectHeight = maxHorizontal - minHorizontal;
            //rectWidth = maxHorizontal - minHorizontal;
            rectWidth = maxVertical - minVertical;
            //rectHeight = maxVertical - minVertical;
            size[0] = rectWidth;
            size[1] = rectHeight;

            return size;
        }

       

        /// <summary>
        ///         Barcode reader - SS final project
        /// </summary>
        /// <param name="img"> Original image </param>
        /// <param name="type"> image type </param>
        /// <param name="bc_centroid1"> output the centroid of the first barcode </param>
        /// <param name="bc_size1"> output the size of the first barcode </param>
        /// <param name="bc_image1"> output a string containing the first barcode read from the bars </param>
        /// <param name="bc_number1"> output a string containing the first barcode read from the numbers in the bottom </param>
        /// <param name="bc_centroid2"> output the centroid of the second barcode </param>
        /// <param name="bc_size2"> output the size of the second barcode </param>
        /// <param name="bc_image2"> output a string containing the second barcode read from the bars. It returns null, if it does not exist.</param>
        /// <param name="bc_number2" output a string containing the second barcode read from the numbers in the bottom. It returns null, if it does not exist.</param>
        /// <returns> 
        ///         image with barcodes detected 
        /// </returns>
        public static Image<Bgr, byte> BarCodeReader(Image<Bgr, byte> img,
                                                int type,
                                                out Point bc_centroid1,
                                                out Size bc_size1,
                                                out string bc_image1,
                                                out string bc_number1,
                                                out Point bc_centroid2,
                                                out Size bc_size2,
                                                out string bc_image2,
                                                out string bc_number2)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                Image<Bgr, byte> imgCopy = img.Copy();
                Image<Bgr, byte> imgCopy2 = img.Copy();
                Image<Bgr, byte> imgCopy3 = img.Copy();

                int[] centroid = new int[2];
                int[] size = new int[2];
                bc_centroid1 = default;
                bc_size1 = default;
                bc_image1 = null;
                bc_number1 = null;
                bc_centroid2 = default;
                bc_size2 = default;
                bc_image2 = null;
                bc_number2 = null;
                string aux_bc_number = default;
                int[] aux_bc_number1 = default;
                int[] aux_bc_number2 = default;

                double angle_momentum = default;

                int[,] maskClose1 = new int[11, 11];
                int i, j;
                int[,] maskCloseAux = new int[3, 3];
                
                for(i=0; i< maskCloseAux.GetLength(0); i++)
                {
                    for (j = 0; j < maskCloseAux.GetLength(1); j++)
                    {
                        maskCloseAux[i, j] = 1;
                    }

                }

                for (i = 0; i < maskClose1.GetLength(0); i++)
                {
                    for (j = 0; j < maskClose1.GetLength(1); j++)
                    {
                        maskClose1[i, j] = 1;
                    }
                }
                int[,] maskErosion1 = new int[7, 7];
                for (i = 0; i < maskErosion1.GetLength(0); i++)
                {
                    for (j = 0; j < maskErosion1.GetLength(1); j++)
                    {
                        maskErosion1[i, j] = 1;
                    }
                }
                int[,] maskVLine = new int[3, 3];
                for (i = 0; i < maskVLine.GetLength(1); i++)
                {
                    maskVLine[i, maskVLine.GetLength(0) - 1] = 1;
                }
                
                int[,] maskClose2 = new int[15, 15];

                for (i = 0; i < maskClose2.GetLength(0); i++)
                {
                    for (j = 0; j < maskClose2.GetLength(1); j++)
                    {
                        maskClose2[i, j] = 1;
                    }
                }
                int[,] maskErosion2 = new int[9, 9];
                for (i = 0; i < maskErosion2.GetLength(0); i++)
                {
                    for (j = 0; j < maskErosion2.GetLength(1); j++)
                    {
                        maskErosion2[i, j] = 1;
                    }
                }
                
                //mascara de blackhat
                int[,] maskBlackHat = new int[9,9];

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
                int[,] maskBigVLine = new int[5, 5];
                for (i = 0; i < maskBigVLine.GetLength(0); i++)
                {
                    maskBigVLine[i, maskBigVLine.GetLength(1) - 1] = 1;
                    maskBigVLine[i, maskBigVLine.GetLength(1) - 2] = 1;
                }
                
                int[,] maskBigHLine = new int[15, 15];
                for (i = 0; i < maskBigHLine.GetLength(1); i++)
                {
                    maskBigHLine[maskBigHLine.GetLength(0) - 1, i] = 1;
                    maskBigHLine[maskBigHLine.GetLength(0) - 2, i] = 1;
                }
                int[,] maskClose3 = new int[11, 11];

                for (i = 0; i < maskClose3.GetLength(0); i++)
                {
                    for (j = 0; j < maskClose3.GetLength(1); j++)
                    {
                        maskClose3[i, j] = 1;
                    }
                }
                switch (type)
                {
                    case 1:
                        {
                            // operações morfológicas
                            ConvertToBW_Otsu(imgCopy2);
                            imgCopy = imgCopy2.Copy();

                            Roberts(imgCopy2, imgCopy);
                            Negative(imgCopy2);
                            imgCopy = imgCopy2.Copy();

                            
                            //fecho
                            Dilation(imgCopy2, imgCopy, maskClose1);
                            imgCopy = imgCopy2.Copy();
                            Dilation(imgCopy2, imgCopy, maskClose1);
                            imgCopy = imgCopy2.Copy();
                            Erosion(imgCopy2, imgCopy, maskClose1);
                            imgCopy = imgCopy2.Copy();
                            Erosion(imgCopy2, imgCopy, maskClose1);

                            
                            //erosao
                            imgCopy = imgCopy2.Copy();
                            Erosion(imgCopy2, imgCopy, maskErosion1);
                         
                            //Projeções
                            int[] vert_proj = VerticalProjection(imgCopy2);
                            int[] horz_proj = HorizontalProjection(imgCopy2);
                            
                            //Cálculo das dimensões do CB
                            size = BC_size(horz_proj, vert_proj);

                            //Cálculo do centro do CB
                            centroid = Centroid(imgCopy2);

                            // converter a imagem original de novo para binario apos destacar apenas uma faixa horizontal de pixeis em torno do centroide
                            imgCopy2 = img.Copy();

                            imgCopy = imgCopy2.Copy();
                            ConvertToBW_Otsu(imgCopy2);

                            // aplicar erosao controlada das linhas verticais para limpar o ruído remanescente das imagens rodadas
                            int iter = 5;
                            for (i = 0; i < iter; i++)
                            {
                                imgCopy = imgCopy2.Copy();
                                Erosion(imgCopy2, imgCopy, maskVLine);
                            }


                            // definir rectangulo de segmentação
                            int X1 = (centroid[0] - (int)Math.Round(size[0] / 2.0) - 30);
                            int X2 = (centroid[0] + (int)Math.Round(size[0] / 2.0) + 30);
                            int Y1 = centroid[1] - (int)Math.Round(size[1] / 2.0) - 30;
                            int Y2 = centroid[1] + (int)Math.Round(size[1] / 2.0) + 30;
                            ProjectionSegmentation(imgCopy2, X1, X2, Y1, Y2);
                            imgCopy = imgCopy2.Copy();
                            
                            //segmentação por algoritmo iterativo
                            int[,] imgArray = Segmentation_eight(imgCopy2, imgCopy);
                         
                            // fazer descodificação do código de barras através das barras
                            int[] colour_list = new int[img.Width];
                            for (int x = 0; x < img.Width; x++)
                            {
                                colour_list[x] = imgArray[centroid[1], x];
                            }

                            aux_bc_number1 = BC_DigitDecoderv2(colour_list);
                            
                            for (int x = 0; x < img.Width; x++)
                            {
                                colour_list[x] = imgArray[centroid[1] - 10, x]; // ver a imagem com um offset de 10 pixeis
                            }
                            aux_bc_number2 = BC_DigitDecoderv2(colour_list);

                            for (int k = 0; k < aux_bc_number1.Length; k++)
                            {
                                aux_bc_number1[k] = (int)Math.Max(aux_bc_number2[k], aux_bc_number1[k]);
                            }
                            aux_bc_number = string.Join("", aux_bc_number1);
                           
                            break;
                        }

                    case 2:
                        {
                            ConvertToBW_Otsu(imgCopy2);
                            imgCopy = imgCopy2.Copy();
                            Roberts(imgCopy2, imgCopy);
                            Negative(imgCopy2);
                            //int[,] maskBlackHat = new int[7, 7];
                            

                            imgCopy = imgCopy2.Copy();
                            Close(imgCopy2, imgCopy, maskClose2, 2);

                            
                            //erosao
                            imgCopy = imgCopy2.Copy();
                            Dilation(imgCopy2, imgCopy, maskErosion2);
                            imgCopy = imgCopy2.Copy();
                            Erosion(imgCopy2, imgCopy, maskErosion2);


                            //detetar o angulo para o qual temos de rodar a imagem
                            imgCopy = imgCopy2.Copy();
                            angle_momentum = angleFinder(imgCopy2, imgCopy);
                            double angle_correction = 0.025;
                            //if(angle_momentum != 0.0)
                            angle_momentum = angle_momentum > 0.0 ? angle_momentum + angle_correction : angle_momentum < 0.0 ? angle_momentum - angle_correction : angle_momentum;

                            //Rotation_Bilinear_xy_point(imgCopy2, imgCopy, (float)angle_momentum, centroid[0], centroid[1]);
                            Rotation_Bilinear(imgCopy2, imgCopy, (float)angle_momentum);
                            // obter o centroide e as dimensoes apos rodar o bloco
                            int[] vert_proj = VerticalProjection(imgCopy2);
              

                            int[] horz_proj = HorizontalProjection(imgCopy2);
                            

                            size = BC_size(horz_proj, vert_proj);
                            centroid = Centroid(imgCopy2);


                           
                            

                            // aplicar rotação à imagem principal
                            
                            imgCopy = img.Copy();
                            //Rotation_Bilinear_xy_point(img, imgCopy, (float)angle_momentum, centroid[0], centroid[1]);
                            Rotation_Bilinear(img, imgCopy, (float)angle_momentum);
                            imgCopy2 = img.Copy();
                            imgCopy = img.Copy();
                            //Rotation_Bilinear(imgCopy2, imgCopy, (float)angle_momentum);
                            // converter a imagem original de novo para binario apos destacar apenas uma faixa horizontal de pixeis em torno do centroide
                            ConvertToBW_Otsu(imgCopy2);
                            // aplicar erosao controlada das linhas verticais para limpar o ruído remanescente das imagens rodadas
                            
                            int iter = 8;
                            for (i = 0; i<iter; i++ )
                            {
                                imgCopy = imgCopy2.Copy();
                                Erosion(imgCopy2, imgCopy, maskVLine);
                            }

                          

                            // definir rectangulo de segmentação
                            int X1 = (centroid[0] - (int)Math.Round(size[0] / 2.0) - 30);
                            int X2 = (centroid[0] + (int)Math.Round(size[0] / 2.0) + 30);
                            int Y1 = centroid[1] - (int)Math.Round(size[1] / 2.0) - 30;
                            int Y2 = centroid[1] + (int)Math.Round(size[1] / 2.0) + 30;
                            ProjectionSegmentation(imgCopy2, X1, X2, Y1, Y2);
                            imgCopy = imgCopy2.Copy();
                            //segmentação por algoritmo iterativo
                        
                            int[,] imgArray = Segmentation_four(imgCopy2, imgCopy);
                         
                            // fazer descodificação do código de barras através das barras
                            int[] colour_list = new int[img.Width];
                            for (int x = 0; x < img.Width; x++)
                            {
                                colour_list[x] = imgArray[centroid[1], x];
                            }
                            aux_bc_number1 = BC_DigitDecoderv2(colour_list);
                            for (int x = 0; x < img.Width; x++)
                            {
                                colour_list[x] = imgArray[centroid[1] - 5, x]; // ver a imagem com um offset de 10 pixeis
                            }
                            aux_bc_number2 = BC_DigitDecoderv2(colour_list);

                            for (int k = 0; k < aux_bc_number1.Length; k++)
                            {
                                //aux_bc_number1[k] = (int)Math.Round(((double)aux_bc_number1[k] + aux_bc_number2[k])/2);
                                aux_bc_number1[k] = (int)Math.Max(aux_bc_number2[k], aux_bc_number1[k]);
                            }
                            aux_bc_number = string.Join("", aux_bc_number1);

                            break;
                        }

                    case 3:
                        {
                            //// fazer resize a iamgem para acelecar o algoritmo
                            Image<Bgr, byte> resized_img = img.Resize((int)img.Width/2, (int)img.Height/2, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);//this is image with resize
                            imgCopy2 = resized_img.Copy();
                            imgCopy3 = imgCopy2.Copy();
                            //converter para binaria

                            ConvertToBW_Otsu(imgCopy2);

                            WhiteTopHat(imgCopy2, maskBlackHat, 2);
                            
                            imgCopy = imgCopy2.Copy();
                            Dilation(imgCopy2, imgCopy, maskCloseAux);
                            
                            //aplicar transformada blackhat
                            imgCopy = imgCopy2.Copy();
                            BlackHat(imgCopy2, maskBlackHat);
                            //passar a negativo e destacar linhas verticais
                            Negative(imgCopy2);
                            imgCopy = imgCopy2.Copy();
                            Erosion(imgCopy2, imgCopy, maskBigVLine);
                            // colar horizontalmente as linhas verticais

                            Negative(imgCopy2);
                            imgCopy = imgCopy2.Copy();
                            Erosion(imgCopy2, imgCopy, maskBigHLine);

                            Negative(imgCopy2);
                            // fechar o objeto para criar o bloco maciço

                            imgCopy = imgCopy2.Copy();
                            Close(imgCopy2, imgCopy, maskClose3, 8);

                            ////detetar o angulo para o qual temos de rodar a imagem
                            imgCopy = imgCopy2.Copy();
                            angle_momentum = angleFinder(imgCopy2, imgCopy);

                            ////if(angle_momentum != 0.0)


                            //Rotation_Bilinear_xy_point(imgCopy2, imgCopy, (float)angle_momentum, centroid[0], centroid[1]);
                            Rotation_Bilinear(imgCopy2, imgCopy, (float)angle_momentum);
                            // obter o centroide e as dimensoes apos rodar o bloco
                            int[] vert_proj = VerticalProjection(imgCopy2);
                            //int[] filt_vert_proj = MeanFilter(vert_proj, 20);
                            //filt_vert_proj = MatchFilter(filt_vert_proj, 20, 0.5);

                            int[] horz_proj = HorizontalProjection(imgCopy2);
                            //int[] filt_horz_proj = MeanFilter(horz_proj, 20);
                            //filt_horz_proj = MatchFilter(filt_horz_proj, 20, 0.5);

                            size = BC_size(horz_proj, vert_proj);
                            size[0] = 2 * size[0];
                            size[1] = 2 * size[1];
                            centroid = Centroid(imgCopy2);
                            centroid[0] = 2 * centroid[0];
                            centroid[1] = 2 * centroid[1];



                            // aplicar rotação à imagem principal

                            imgCopy = img.Copy();
                            //Rotation_Bilinear_xy_point(img, imgCopy, (float)angle_momentum, centroid[0], centroid[1]);
                            double angle_correction = 0.04;
                            angle_momentum = angle_momentum > 0.02 ? angle_momentum - angle_correction : angle_momentum < 0.02 ? angle_momentum + angle_correction : 0.0;
                            Rotation_Bilinear(img, imgCopy, (float)angle_momentum);
                            imgCopy2 = img.Copy();
                            imgCopy = imgCopy2.Copy();
                           
                            // converter a imagem original de novo para binario apos destacar apenas uma faixa horizontal de pixeis em torno do centroide
                            ConvertToBW_Otsu(imgCopy2);
                            // aplicar erosao controlada das linhas verticais para limpar o ruído remanescente das imagens rodadas

                            int iter = 8;
                            for (i = 0; i < iter; i++)
                            {
                                imgCopy = imgCopy2.Copy();
                                Erosion(imgCopy2, imgCopy, maskVLine);
                            }

                            //segmentar a imagem 
                            // definir rectangulo de segmentação
                            int X1 = (centroid[0] - (int)Math.Round(size[0] / 2.0) - 30);
                            int X2 = (centroid[0] + (int)Math.Round(size[0] / 2.0) + 30);
                            int Y1 = centroid[1] - (int)Math.Round(size[1] / 2.0) - 30;
                            int Y2 = centroid[1] + (int)Math.Round(size[1] / 2.0) + 30;
                            ProjectionSegmentation(imgCopy2, X1, X2, Y1, Y2);
                            imgCopy = imgCopy2.Copy();
                            
                            //segmentação por algoritmo iterativo
                            int[,] imgArray = Segmentation_four(imgCopy2, imgCopy);
                           
                            // fazer descodificação do código de barras através das barras
                            int[] colour_list = new int[imgCopy2.Width];
                            for (int x = 0; x < imgCopy2.Width; x++)
                            {
                                colour_list[x] = imgArray[centroid[1], x];
                            }
                            aux_bc_number1 = BC_DigitDecoderv2(colour_list);
                            for (int x = 0; x < imgCopy2.Width; x++)
                            {
                                colour_list[x] = imgArray[centroid[1] - 5, x]; // ver a imagem com um offset de 10 pixeis
                            }
                            aux_bc_number2 = BC_DigitDecoderv2(colour_list);

                            for (int k = 0; k < aux_bc_number1.Length; k++)
                            {
                                //aux_bc_number1[k] = (int)Math.Round(((double)aux_bc_number1[k] + aux_bc_number2[k])/2);
                                aux_bc_number1[k] = (int)Math.Max(aux_bc_number2[k], aux_bc_number1[k]);
                            }
                            aux_bc_number = string.Join("", aux_bc_number1);
                            img = imgCopy2.Copy();
                            break;
                        }
                   case 4: 
                        {
                            //// fazer resize a iamgem para acelecar o algoritmo
                            Image<Bgr, byte> resized_img = img.Resize((int)img.Width / 2, (int)img.Height / 2, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);//this is image with resize
                            imgCopy2 = resized_img.Copy();
                            imgCopy3 = imgCopy2.Copy();
                            //converter para binaria

                            ConvertToBW_Otsu(imgCopy2);

                            WhiteTopHat(imgCopy2, maskBlackHat, 2);

                            imgCopy = imgCopy2.Copy();
                            Dilation(imgCopy2, imgCopy, maskCloseAux);

                            //aplicar transformada blackhat
                            imgCopy = imgCopy2.Copy();
                            BlackHat(imgCopy2, maskBlackHat);
                            //passar a negativo e destacar linhas verticais
                            Negative(imgCopy2);
                            imgCopy = imgCopy2.Copy();
                            Erosion(imgCopy2, imgCopy, maskBigVLine);
                            // colar horizontalmente as linhas verticais

                            Negative(imgCopy2);
                            imgCopy = imgCopy2.Copy();
                            Erosion(imgCopy2, imgCopy, maskBigHLine);

                            Negative(imgCopy2);
                            // fechar o objeto para criar o bloco maciço

                            imgCopy = imgCopy2.Copy();
                            Close(imgCopy2, imgCopy, maskClose3, 8);

                            ////detetar o angulo para o qual temos de rodar a imagem
                            imgCopy = imgCopy2.Copy();
                            angle_momentum = angleFinder(imgCopy2, imgCopy);

                            ////if(angle_momentum != 0.0)


                            //Rotation_Bilinear_xy_point(imgCopy2, imgCopy, (float)angle_momentum, centroid[0], centroid[1]);
                            Rotation_Bilinear(imgCopy2, imgCopy, (float)angle_momentum);
                            // obter o centroide e as dimensoes apos rodar o bloco
                            int[] vert_proj = VerticalProjection(imgCopy2);
                            //int[] filt_vert_proj = MeanFilter(vert_proj, 20);
                            //filt_vert_proj = MatchFilter(filt_vert_proj, 20, 0.5);

                            int[] horz_proj = HorizontalProjection(imgCopy2);
                            //int[] filt_horz_proj = MeanFilter(horz_proj, 20);
                            //filt_horz_proj = MatchFilter(filt_horz_proj, 20, 0.5);

                            size = BC_size(horz_proj, vert_proj);
                            size[0] = 2 * size[0];
                            size[1] = 2 * size[1];
                            centroid = Centroid(imgCopy2);
                            centroid[0] = 2 * centroid[0];
                            centroid[1] = 2 * centroid[1];



                            // aplicar rotação à imagem principal

                            imgCopy = img.Copy();
                            //Rotation_Bilinear_xy_point(img, imgCopy, (float)angle_momentum, centroid[0], centroid[1]);
                            double angle_correction = 0.04;
                            angle_momentum = angle_momentum > 0.02 ? angle_momentum - angle_correction : angle_momentum < 0.02 ? angle_momentum + angle_correction : 0.0;
                            Rotation_Bilinear(img, imgCopy, (float)angle_momentum);
                            imgCopy2 = img.Copy();
                            imgCopy = imgCopy2.Copy();
                            //Rotation_Bilinear(imgCopy2, imgCopy, (float)angle_momentum);
                            // converter a imagem original de novo para binario apos destacar apenas uma faixa horizontal de pixeis em torno do centroide
                            ConvertToBW_Otsu(imgCopy2);
                            // aplicar erosao controlada das linhas verticais para limpar o ruído remanescente das imagens rodadas

                            int iter = 8;
                            for (i = 0; i < iter; i++)
                            {
                                imgCopy = imgCopy2.Copy();
                                Erosion(imgCopy2, imgCopy, maskVLine);
                            }
                            //segmentar a imagem 
                            // definir rectangulo de segmentação
                            int X1 = (centroid[0] - (int)Math.Round(size[0] / 2.0) - 30);
                            int X2 = (centroid[0] + (int)Math.Round(size[0] / 2.0) + 30);
                            int Y1 = centroid[1] - (int)Math.Round(size[1] / 2.0) - 30;
                            int Y2 = centroid[1] + (int)Math.Round(size[1] / 2.0) + 30;
                            ProjectionSegmentation(imgCopy2, X1, X2, Y1, Y2);
                            imgCopy = imgCopy2.Copy();
                            //segmentação por algoritmo iterativo
                            //int[,] imgArray = IterativeSegmentation(img, imgCopy);
                            int[,] imgArray = Segmentation_four(imgCopy2, imgCopy);
                            //imgCopy2 = img.Copy();
                            //imgCopy = imgCopy2.Copy();
                            //// imgCopy = imgCopy2.Copy();
                            //HorizontalStripImg(imgCopy2, centroid[1], 0);
                            // fazer descodificação do código de barras através das barras
                            int[] colour_list = new int[imgCopy2.Width];
                            for (int x = 0; x < imgCopy2.Width; x++)
                            {
                                colour_list[x] = imgArray[centroid[1], x];
                            }
                            aux_bc_number1 = BC_DigitDecoderv2(colour_list);
                            for (int x = 0; x < imgCopy2.Width; x++)
                            {
                                colour_list[x] = imgArray[centroid[1] - 5, x]; // ver a imagem com um offset de 10 pixeis
                            }
                            aux_bc_number2 = BC_DigitDecoderv2(colour_list);

                            for (int k = 0; k < aux_bc_number1.Length; k++)
                            {
                                //aux_bc_number1[k] = (int)Math.Round(((double)aux_bc_number1[k] + aux_bc_number2[k])/2);
                                aux_bc_number1[k] = (int)Math.Max(aux_bc_number2[k], aux_bc_number1[k]);
                            }
                            aux_bc_number = string.Join("", aux_bc_number1);
                            img = imgCopy2.Copy();
                            break;
                        }
                   case 5:
                        {


                            break;
                        }
                }

                // first barcode
                //bc_image1 = "5601212323434";
                bc_image1 = aux_bc_number;
                //bc_number1 = "9780201379624";
                bc_centroid1 = new Point(centroid[0], centroid[1]);
                bc_size1 = new Size(size[0], size[1]);

                //second barcode
                bc_image2 = null;
                bc_number2 = null;
                bc_centroid2 = Point.Empty;
                bc_size2 = Size.Empty;

                // draw the rectangle over the destination image
                img.Draw(new Rectangle(bc_centroid1.X - bc_size1.Width / 2,
                                       bc_centroid1.Y - bc_size1.Height / 2,
                                       bc_size1.Width,
                                       bc_size1.Height),
                                       new Bgr(0, 255, 0), 3);

                return img;
            }

        }
    }
}