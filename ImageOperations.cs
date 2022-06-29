using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using static System.Math;
using System.Linq;
///Algorithms Project

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }

    public struct vertexInfo
    {
        
        public Color Distination;
        public double distance;
        public Color Source;
    }
    public struct distinct
    {
        public Color distinct_color;
        public bool is_visited;
    }

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }
        public static HashSet<distinct> distinct_colors(string path, RGBPixel[,] image)
        {
            int Height = GetHeight(image);
            int Width = GetWidth(image);
            HashSet<distinct> d = new HashSet<distinct>();
            distinct dd;
            Bitmap b = new Bitmap(path);
            for (int i = 1; i <= (Height + Width - 1); i++)
            {
                int start = Max(0, i - Height);
                int min = Min(i, Min((Width - start), Height));
                for (int j = 0; j < min; j++)
                {

                    dd.distinct_color = b.GetPixel(start + j, Min(Height, i) - j - 1);
                    dd.is_visited = false;
                    d.Add(dd);
                }
            }

            return d;
        }
        //------------------ To Make mst value is global ---------------------
        public static double MST_Value;
        // To display mst value in the form 
        //----------------------------------------------------------------------
        public static List<vertexInfo> MST_Graph(HashSet<distinct> distinct_colors)
        {
            List<vertexInfo> cluster_list = new List<vertexInfo>();
            double distance = 0, mst_value = 0;
            int r, g, b;
            vertexInfo v;
            distinct[] arr = distinct_colors.ToArray();
            vertexInfo[] distance_arr = new vertexInfo[distinct_colors.Count];
            for (int i = 0; i < arr.Length; i++)
            {
                distance_arr[i].distance = double.MaxValue;
            }
            distance_arr[0].distance = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                int min_indx = -5;
                double min_value = double.MaxValue;
                for (int x = 0; x < arr.Length; x++)
                {
                    if (!arr[x].is_visited && distance_arr[x].distance < min_value)
                    {
                        min_value = distance_arr[x].distance;
                        min_indx = x;
                    }
                }
                arr[min_indx].is_visited = true;
                for (int j = 0; j < arr.Length; j++)
                {
                    if (!arr[j].is_visited)
                    {
                        r = arr[min_indx].distinct_color.R - arr[j].distinct_color.R;
                        b = arr[min_indx].distinct_color.G - arr[j].distinct_color.G;
                        g = arr[min_indx].distinct_color.B - arr[j].distinct_color.B;

                        distance = Math.Sqrt((r * r) + (g * g) + (b * b));
                        if (distance < distance_arr[j].distance)
                        {
                            distance_arr[j].distance = distance;
                            distance_arr[j].Source = arr[min_indx].distinct_color;
                            distance_arr[j].Distination = arr[j].distinct_color;
                        }
                    }
                }
            }

            for (int i = 1; i < distance_arr.Length; i++)
            {
                mst_value += distance_arr[i].distance;
                v.Source = distance_arr[i].Source;
                v.Distination = distance_arr[i].Distination;
                v.distance = distance_arr[i].distance;
                cluster_list.Add(v);
            }

            MST_Value = mst_value;
            return cluster_list;
        }

        public static List<HashSet<Color>> MSTCluster(List<vertexInfo> mst_edges, int k)
        {
            Dictionary<Color, List<Color>> adjacents = new Dictionary<Color, List<Color>>();
            HashSet<Color> steppedOver = new HashSet<Color>();
            List<HashSet<Color>> clusters = new List<HashSet<Color>>();
            // removing k-1 edges by calculating max distance: 
            for (int i = 0; i < k - 1; i++)
            {
                double maxDis = -999;
                int maxIdx = 0;
                for (int j = 0; j < mst_edges.Count - 1; j++)
                {
                    if (mst_edges[j].distance > maxDis)
                    {
                        maxDis = mst_edges[j].distance;
                        maxIdx = j;
                    }
                }
                vertexInfo edge;
                edge.distance = 0;
                edge.Source = mst_edges[maxIdx].Source;
                edge.Distination = mst_edges[maxIdx].Distination;
                mst_edges[maxIdx] = edge;
            }
            // setting neighbourhood:
            foreach (var edge in mst_edges)
            {
                if (edge.distance == 0)
                {
                    if (!adjacents.ContainsKey(edge.Source))
                    {
                        adjacents.Add(edge.Source, new List<Color>());
                    }
                    if (!adjacents.ContainsKey(edge.Distination))
                    {
                        adjacents.Add(edge.Distination, new List<Color>());
                    }
                }
                else if (edge.distance != 0)
                {
                    if (adjacents.ContainsKey(edge.Source))
                    {
                        adjacents[edge.Source].Add(edge.Distination);
                    }
                    else
                    {
                        adjacents.Add(edge.Source, new List<Color>());
                        adjacents[edge.Source].Add(edge.Distination);
                    }
                    if (adjacents.ContainsKey(edge.Distination))
                    {
                        adjacents[edge.Distination].Add(edge.Source);
                    }
                    else
                    {
                        adjacents.Add(edge.Distination, new List<Color>());
                        adjacents[edge.Distination].Add(edge.Source);
                    }
                }
            }
            //determine clusters:
            foreach (var pixel in adjacents.Keys)
            {
                if (!steppedOver.Contains(pixel))
                {
                    HashSet<Color> c = new HashSet<Color>();
                    getCluster(pixel, steppedOver, adjacents, c);
                    clusters.Add(c);
                }
            }
            return clusters;
        }
        public static HashSet<Color> getCluster(Color color, HashSet<Color> Stepped, Dictionary<Color, List<Color>> adj, HashSet<Color> h)
        {
            Stepped.Add(color);
            h.Add(color);
            foreach (var a in adj[color])
            {
                if (!Stepped.Contains(a))
                    getCluster(a, Stepped, adj, h);
            }
            return h;
        }
        public static Dictionary<Color, Color> representative(List<HashSet<Color>> clusters)
        {
            Dictionary<Color, Color> reps = new Dictionary<Color, Color>();
            int red, green, blue;
            Color c;
            foreach (var list in clusters)
            {
                red = 0;
                green = 0;
                blue = 0;
                foreach (var Color in list)
                {
                    red += Color.R;
                    green += Color.G;
                    blue += Color.B;
                }
                c = Color.FromArgb(red / list.Count, green / list.Count, blue / list.Count);
                foreach (var Color in list)
                {
                    reps.Add(Color, c);
                }
            }
            return reps;
        }
        public static RGBPixel[,] replace_disColors(string path, RGBPixel[,] image, Dictionary<Color, Color> rep)

        {
            int Height = GetHeight(image);
            int Width = GetWidth(image);
            Color s;
            Bitmap b = new Bitmap(path);
            for (int i = 1; i <= (Height + Width - 1); i++)
            {
                int start = Max(0, i - Height);
                int min = Min(i, Min((Width - start), Height));
                for (int j = 0; j < min; j++)
                {
                    s = b.GetPixel(start + j, Min(Height, i) - j - 1);
                    s = rep[s];
                    image[Min(Height, i) - j - 1,start + j].red = s.R;
                    image[Min(Height, i) - j - 1, start + j].green = s.G;
                    image[Min(Height, i) - j - 1, start + j].blue = s.B;
                }
            }
            
            return image;
        }
    }
}

