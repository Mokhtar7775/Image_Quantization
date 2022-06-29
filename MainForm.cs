using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {

        public MainForm()
        {
            InitializeComponent();

        }
        RGBPixel[,] ImageMatrix,  quantize_image;
        HashSet<distinct> set = new HashSet<distinct>();
        Queue<Color> color_queue = new Queue<Color>();
        List<vertexInfo> mst;
        string OpenedFilePath;
        TimeSpan total ;
        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                
                //Open the browsed image and display it
                OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
                txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
                //---------------------Distinct Colors return----------------------
                set = ImageOperations.distinct_colors(OpenedFilePath, ImageMatrix);
                //-----------------------------------------------------------------
                DistinctColor.Text = set.Count.ToString();
                DateTime start = DateTime.UtcNow;
                //------------Mst_graph return----------------------- 
                mst = ImageOperations.MST_Graph(set);
                //---------------------------------------------------
                DateTime end = DateTime.UtcNow;
                TimeSpan timeDiff = end - start;
                total = timeDiff;
                TimeOfGettingGrarh.Text = timeDiff.TotalSeconds.ToString();
                txtMST.Text = ImageOperations.MST_Value.ToString();

            }
            
        }
        private void btnQuantization_Click(object sender, EventArgs e)
        {

            if (set.Count == 0)
            {
                MessageBox.Show(" Please open an Image first ", "ERROR");
                return;
            }
            else if ((int)Clusters.Value > set.Count)
            {
                MessageBox.Show(" Number of clusters must be less than or equal distinct colors \n" +
                    "       0 < num of clusters <= num of distinct colors", "ERROR");
                return;
            }
            DateTime start_Quantize = DateTime.UtcNow;
            //----------------------Mst Clusters return--------------------------
            List<HashSet<Color>> mst_clusters = ImageOperations.MSTCluster(mst, (int)Clusters.Value);
            //--------------------------------------------------------------------

            //-----------------------representative return------------------------------------------
            Dictionary<Color, Color> representative = ImageOperations.representative(mst_clusters);
            //--------------------------------------------------------------------------------------
            DateTime End_Quantize = DateTime.UtcNow;
            TimeSpan QuantizationTime = End_Quantize - start_Quantize;
            //-------------------------------quantize return------------------------------------------
            quantize_image = ImageOperations.replace_disColors(OpenedFilePath, ImageMatrix, representative);
            //------------------------------------------------------------------------------------------
            QuantizeTime.Text = QuantizationTime.TotalSeconds.ToString();
            ImageOperations.DisplayImage(quantize_image, pictureBox2);
            total += QuantizationTime;
            Totaltime.Text = total.TotalSeconds.ToString();

        }

        private void Clusters_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void txtWidth_TextChanged(object sender, EventArgs e)
        {

        }

        private void DistinctColor_TextChanged(object sender, EventArgs e)
        {

        }

        private void TimeOfCalculation_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtMST_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void QuantizeTime_TextChanged(object sender, EventArgs e)
        {

        }

        private void Totaltime_TextChanged(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}