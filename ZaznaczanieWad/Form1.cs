using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Stitching;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZaznaczanieWad
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

        private int maxW;
        private int maxH;

        private double resizeW = 64.0;
        private double resizeH = 64.0;

        double[,] ball = new double[,]
{
                {  -1e6,      -1e6,         0,    0.6248,    1.2497,    1.8745,    1.2497,    0.6248,         0,      -1e6,      -1e6},
                {  -1e6,    0.6248,    1.2497,    1.8745,    2.4994,    2.4994,    2.4994,    1.8745,    1.2497,    0.6248,      -1e6},
                {     0,    1.2497,    1.8745,    2.4994,    3.1242,    3.1242,    3.1242,    2.4994,    1.8745,    1.2497,         0},
                {0.6248,    1.8745,    2.4994,    3.1242,    3.7491,    3.7491,    3.7491,    3.1242,    2.4994,    1.8745,    0.6248},
                {1.2497,    2.4994,    3.1242,    3.7491,    4.3739,    4.3739,    4.3739,    3.7491,    3.1242,    2.4994,    1.2497},
                {1.8745,    2.4994,    3.1242,    3.7491,    4.3739,    4.9987,    4.3739,    3.7491,    3.1242,    2.4994,    1.8745},
                {1.2497,    2.4994,    3.1242,    3.7491,    4.3739,    4.3739,    4.3739,    3.7491,    3.1242,    2.4994,    1.2497},
                {0.6248,    1.8745,    2.4994,    3.1242,    3.7491,    3.7491,    3.7491,    3.1242,    2.4994,    1.8745,    0.6248},
                {     0,    1.2497,    1.8745,    2.4994,    3.1242,    3.1242,    3.1242,    2.4994,    1.8745,    1.2497,         0},
                {  -1e6,    0.6248,    1.2497,    1.8745,    2.4994,    2.4994,    2.4994,    1.8745,    1.2497,    0.6248,      -1e6},
                {  -1e6,      -1e6,         0,    0.6248,    1.2497,    1.8745,    1.2497,    0.6248,         0,      -1e6,      -1e6},
};

        static byte[] To1DArray(byte[,] input)
        {
            // Step 1: get total size of 2D array, and allocate 1D array.
            int size = input.Length;
            byte[] result = new byte[size];

            // Step 2: copy 2D array elements into a 1D array.
            int write = 0;
            for (int i = 0; i <= input.GetUpperBound(0); i++)
            {
                for (int z = 0; z <= input.GetUpperBound(1); z++)
                {
                    result[write++] = input[i, z];
                }
            }
            // Step 3: return the new array.
            return result;
        }

        Mat MojaErozja(Mat wejscie, double[,] element)
        {
            Mat wynikowa = new Mat(wejscie.Height, wejscie.Width, wejscie.Depth, wejscie.NumberOfChannels);

            byte[,] dane = (byte[,])wejscie.GetData();
            byte[,] dane2 = (byte[,])wynikowa.GetData();

            int rozmiarY = element.GetLength(0);
            int rozmiarX = element.GetLength(1);

            for (int y = 0; y < wejscie.Height; y++)
                for (int x = 0; x < wejscie.Width; x++)
                {
                    double? min = null;
                    for (int dy = 0; dy < rozmiarY; dy++)
                        for (int dx = 0; dx < rozmiarX; dx++)
                        {
                            int ady = y - rozmiarY / 2 + dy;
                            int adx = x - rozmiarX / 2 + dx;
                            if (adx >= 0 && ady >= 0 && adx < wejscie.Width && ady < wejscie.Height)
                            {
                                double nowy = dane[ady, adx] - element[dy, dx];
                                if (min == null || nowy < min) min = nowy;
                            }
                        }
                    dane2[y, x] = (byte)min;
                }
            wynikowa.SetTo(To1DArray(dane2));
            return wynikowa;
        }

        Mat MojaDylatacja(Mat wejscie, double[,] element)
        {
            Mat wynikowa = new Mat(wejscie.Height, wejscie.Width, wejscie.Depth, wejscie.NumberOfChannels);

            byte[,] dane = (byte[,])wejscie.GetData();
            byte[,] dane2 = (byte[,])wynikowa.GetData();

            int rozmiarY = element.GetLength(0);
            int rozmiarX = element.GetLength(1);

            for (int y = 0; y < wejscie.Height; y++)
                for (int x = 0; x < wejscie.Width; x++)
                {
                    double? max = null;
                    for (int dy = 0; dy < rozmiarY; dy++)
                        for (int dx = 0; dx < rozmiarX; dx++)
                        {
                            int ady = y - rozmiarY / 2 + dy;
                            int adx = x - rozmiarX / 2 + dx;
                            if (adx >= 0 && ady >= 0 && adx < wejscie.Width && ady < wejscie.Height)
                            {
                                double nowy = dane[ady, adx] + element[dy, dx];
                                if (max == null || nowy > max) max = nowy;
                            }
                        }
                    dane2[y, x] = (byte)max;
                }
            wynikowa.SetTo(To1DArray(dane2));
            return wynikowa;
        }

        List<Rectangle> mergeRectangles(List<Rectangle> rects)
        {
            List<Rectangle> temp = rects;
            bool changed = false;
            for (int i = 0; i < temp.Count && !changed; i++)
                for (int j = i + 1; j < temp.Count && !changed; j++)
                {
                    Rectangle r1 = temp[i];
                    Rectangle r2 = temp[j];
                    Point LT1 = new Point(
                        r1.X < 2 ? 0 : r1.X - 2,
                        r1.Y < 2 ? 0 : r1.Y - 2
                        );
                    Point LB1 = new Point(
                        r1.X < 2 ? 0 : r1.X - 2,
                        r1.Y > maxH - 2 ? maxH : r1.Y + r1.Height + 2
                        );
                    Point RT1 = new Point(
                        r1.X > maxW - 2 ? maxW : r1.X + r1.Width + 2,
                        r1.Y < 2 ? 0 : r1.Y - 2
                        );
                    Point RB1 = new Point(
                        r1.X > maxW - 2 ? maxW : r1.X + r1.Width + 2,
                        r1.Y > maxH - 2 ? maxH : r1.Y + r1.Height + 2
                        );

                    Point LT2 = new Point(
                        r2.X < 2 ? 0 : r2.X - 2,
                        r2.Y < 2 ? 0 : r2.Y - 2
                        );
                    Point LB2 = new Point(
                        r2.X < 2 ? 0 : r2.X - 2,
                        r2.Y > maxH - 2 ? maxH : r2.Y + r2.Height + 2
                        );
                    Point RT2 = new Point(
                        r2.X > maxW - 2 ? maxW : r2.X + r2.Width + 2,
                        r2.Y < 2 ? 0 : r2.Y - 2
                        );
                    Point RB2 = new Point(
                        r2.X > maxW - 2 ? maxW : r2.X + r2.Width + 2,
                        r2.Y > maxH - 2 ? maxH : r2.Y + r2.Height + 2
                        );
                    if (r1.Contains(LT2) || r1.Contains(LB2) || r1.Contains(RT2) || r1.Contains(RB2) ||
                        r2.Contains(LT1) || r2.Contains(LB1) || r2.Contains(RT1) || r2.Contains(RB1))
                    {
                        int XL = Math.Min(r1.X, r2.X);
                        int YL = Math.Min(r1.Y, r2.Y);
                        int XR = Math.Max(r1.X + r1.Width, r2.X + r2.Width);
                        int YR = Math.Max(r1.Y + r1.Height, r2.Y + r2.Height);
                        Rectangle rect = new Rectangle(XL, YL, XR - XL, YR - YL);
                        temp.RemoveAt(j);
                        temp.RemoveAt(i);
                        temp.Add(rect);
                        changed = true;
                    }
                }
            if (changed)
                temp = mergeRectangles(temp);

            return temp;
        }

        private void btn_process_Click(object sender, EventArgs e)
        {
            string path = @"C:\Users\mkafk\OneDrive\Pulpit\testOpenCV2.jpg";
            Mat imageBGR = new Mat(path);
            Mat imageResize = new Mat();
            CvInvoke.Resize(imageBGR, imageResize, default, 1.0 / resizeW, 1.0 / resizeH);
            Mat imageGray = new Mat();
            CvInvoke.CvtColor(imageResize, imageGray, ColorConversion.Bgr2Gray);

            Mat imageClahe = new Mat();
            CvInvoke.CLAHE(imageGray, 0.02, new Size(8, 8), imageClahe);



            Mat imageDilate = MojaDylatacja(imageClahe, ball);
            Mat imageErode = MojaErozja(imageClahe, ball);

            Mat imageGradient = imageDilate - imageErode;

            double max = 0, min = 0;
            Point minP = new Point();
            Point maxP = new Point();
            CvInvoke.MinMaxLoc(imageGradient, ref min, ref max, ref minP, ref maxP);


            double thrashold = max * 0.95;
            Mat imageProgowana = new Mat();
            CvInvoke.Threshold(imageGradient, imageProgowana, thrashold, 255, ThresholdType.Binary);



            var contours = new VectorOfVectorOfPoint();
            Mat hierarchy = new Mat();
            CvInvoke.FindContours(imageProgowana, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            List<Rectangle> rectangles = new List<Rectangle>();
            for (int i = 0; i < contours.Size; i++)
                rectangles.Add(CvInvoke.BoundingRectangle(contours[i]));


            maxH = imageProgowana.Height;
            maxW = imageProgowana.Width;

            List<Rectangle> mergedRectangles = mergeRectangles(rectangles);

            //VectorOfRect rects = new VectorOfRect(rectangles.ToArray());
            VectorOfRect rects = new VectorOfRect(mergedRectangles.ToArray());

            for (int i = 0; i < rects.Size; i++)
                CvInvoke.Rectangle(imageResize, rects[i], new MCvScalar(0, 255, 0), 1);

            imageBox2.Image = imageProgowana;
            imageBox1.Image = imageResize;
        }

        private void btn_stitch_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = true;

                VectorOfMat mats = new VectorOfMat();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in dialog.FileNames)
                    {
                        mats.Push(new Mat(file));
                    }

                    //Brisk detector = new Brisk();
                    //WarperCreator warper = new SphericalWarper();
                    //WarperCreator warper = new PlaneWarper();

                    Cursor = Cursors.WaitCursor;
                    Stitcher stitcher = new Stitcher();
                    //stitcher.SetFeaturesFinder(detector);
                    //stitcher.SetWarper(warper);

                    Mat m12 = new Mat();
                    var status = stitcher.Stitch(mats, m12);

                    if (status == Stitcher.Status.Ok)
                    {
                        imageBox1.Image = m12;
                    }
                    else
                    {
                        MessageBox.Show("CHUJ");
                    }
                    Cursor = Cursors.Default;
                }

            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
            }
        }
    }
}
