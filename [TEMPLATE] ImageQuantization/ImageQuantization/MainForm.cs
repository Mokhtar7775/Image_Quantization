using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        //-----------Show Console----------------------
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        //---------------------------------------------

        public MainForm()
        {
            InitializeComponent();

        }
        RGBPixel[,] ImageMatrix,  quantize_image;
        HashSet<distinct> set = new HashSet<distinct>();
        Queue<Color> color_queue = new Queue<Color>();
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                //---------------------Distinct Colors return----------------------
                set = ImageOperations.distinct_colors(OpenedFilePath, ImageMatrix);
                //-----------------------------------------------------------------
                Console.WriteLine("will fill  ");
                Console.WriteLine("distinct colors:" + set.Count);
                DateTime start = DateTime.UtcNow;
                //------------Mst_graph return----------------------- 
                List<vertexInfo>mst = ImageOperations.MST_Graph(set);
                //---------------------------------------------------
                DateTime end = DateTime.UtcNow;
                TimeSpan timeDiff = end - start;
                Console.WriteLine("distinct Time in seconds:"+timeDiff.TotalSeconds);

                //------------------Determine the number of k-------------------
                Console.Write("Enter The Number of k : ");
                int k = int.Parse(Console.ReadLine());
                //----------------------------------------------------------------

                //----------------------Mst Clusters return--------------------------
                DateTime startt = DateTime.UtcNow;
                List<HashSet<Color>> mst_clusters = ImageOperations.MSTCluster(mst, k);
                DateTime endd = DateTime.UtcNow;
                TimeSpan timeDifff = endd - startt;
                Console.WriteLine("MST Time in seconds:" + timeDifff.TotalSeconds);
                //--------------------------------------------------------------------

                //-----------------------representative return------------------------------------------
                Dictionary<Color, Color> representative = ImageOperations.representative(mst_clusters);
                //--------------------------------------------------------------------------------------

                //-------------------------------quantize return------------------------------------------
                quantize_image = ImageOperations.replace_disColors(OpenedFilePath, ImageMatrix, representative);
               //------------------------------------------------------------------------------------------

            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }
        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(txtGaussSigma.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            ImageOperations.DisplayImage(quantize_image, pictureBox2);
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            AllocConsole();

        }
    }
}