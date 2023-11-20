using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace FillingTriangles
{
    public class Edge
    {
        public int Ymax { get; set; }

        public double Cur { get; set; }

        public double M { get; set; }

        public Edge(int y, double cur, double m)
        {
            Ymax = y;
            Cur = cur;
            M = m;
        }
    }

    public static class PolygonFiller
    {
        public static Vector3D[,] NsForTriangles = new Vector3D[1,1]; 

        private static double CalcM(Vector3D Start, Vector3D End)
        {
            if (Start.X == End.X)
                return 0;

            return (End.X - Start.X) / (End.Y - Start.Y);
        }

        /// <summary>
        /// calculates n over k
        /// </summary>
        /// <param name="i"></param>
        /// <param name="n"></param>
        private static int NewtonBinom(int n, int k)
        {
            if (k > n - k)
                k = n - k;
            int liczGrupy = 1;
            for (int i = 0; i < k; i++)
            {
                liczGrupy = liczGrupy * (n - i);
                liczGrupy = liczGrupy / (i + 1);
            }
            return liczGrupy;
        }

        private static double CalculateB(int i, int n, double value)
        {
            return NewtonBinom(n, i) * Math.Pow(value, i) * Math.Pow(1 - value, n - i);
        }

        private static Vector3D TransformByMatrix(Vector3D T,  Vector3D B, Vector3D NSurf, Vector3D NText)
        {
            double x = NText.X * T.X + NText.Y * B.X + NText.Z * NSurf.X;
            double y = NText.X * T.Y + NText.Y * B.Y + NText.Z * NSurf.Y;
            double z = NText.X * T.Z + NText.Y * B.Z + NText.Z * NSurf.Z;
            return new Vector3D(x, y, z);
        }

        private static Vector3D CalcuateN(Vector3D[] BezierPoints, Vector3D Puv, 
            int width, int height)
        {
            Vector3D PUuv = new Vector3D(1,0,0);
            Vector3D PVuv = new Vector3D(0,1,0);

            for(int i=0; i<width - 1; i++)
            {
                for(int j=0; j<height; j++)
                {
                    PUuv.Z += (BezierPoints[(i+1)*width + j].Z - 
                        BezierPoints[i * width + j].Z)*
                        CalculateB(i, width-2, Puv.X) * 
                        CalculateB(j, height-1, Puv.Y);
                }
            }
            
            PUuv.Z *= width-1;

            for (int i=0; i<width; i++)
            {
                for(int j=0; j<height - 1; j++)
                {
                    PVuv.Z += (BezierPoints[i * width + (j+1)].Z - 
                        BezierPoints[i * width + j].Z) *
                        CalculateB(i, width-1, Puv.X) * 
                        CalculateB(j, height - 2, Puv.Y);
                }
            }

            PVuv.Z *= height-1;

            return Vector3D.CrossProduct(PUuv, PVuv);
        }


        private static Vector3D CalculateNTexture(Vector3D NText, Vector3D NSurf)
        {
            Vector3D B = new Vector3D(0, 1, 0);
            
            if(NSurf.X != 0 || NSurf.Y != 0 || NSurf.Y != 0)
            {
                Vector3D V = new Vector3D(0, 0, 1);
                B = Vector3D.CrossProduct(NSurf, V);
                B.Normalize();
            }

            Vector3D T = Vector3D.CrossProduct(B, NSurf);
            T.Normalize();
            return TransformByMatrix(T, B, NSurf, NText);
        }

        /// <summary>
        /// Colors pixels inside polygon and bounders in random color
        /// </summary>
        /// <param name="array">sorted array (by Y) of Vertexs </param>
        /// <param name="DBmp">bitmap for coloring</param>
        public static void ColorPolygon(Vertex[] array, Vector3D[] Narray, DirectBitmap DBmp)
        {
            //Vector3D v1 = new Vector3D( (array[0].Start.X - array[1].Start.X)/DBmp.Width, 
            //    (array[0].Start.Y - array[1].Start.Y) / DBmp.Height, 
            //    array[0].Start.Z - array[1].Start.Z);

            Vector3D[] vector3Ds = new Vector3D[array.Length];
            vector3Ds[0] = array[0].Start;
            vector3Ds[1] = array[1].Start;
            vector3Ds[2] = array[2].Start;

            //v1.Normalize();

            //Vector3D v2 = new Vector3D((array[0].Start.X - array[2].Start.X)/DBmp.Width, 
            //    (array[0].Start.Y - array[2].Start.Y)/DBmp.Height,
            //    array[0].Start.Z - array[2].Start.Z);

            //v2.Normalize();

            //Vector3D v3 = Vector3D.CrossProduct(v1, v2);
            //v3.Normalize();
            //double Z = Math.Abs(v3.Z);

            //var p = Color.FromArgb((int)(Z*255), (int)(Z * 255), (int)(Z * 255));

            int ymin = (int)array[0].Start.Y - 1;
            int ymax = (int)array[array.Length - 1].Start.Y;
            int next = 0;

            List<Edge> AET = new List<Edge>(array.Length);

            do
            {
                AET.Sort((x1, x2) => x1.Cur.CompareTo(x2.Cur));
                while (ymin < (int)array[next].Start.Y)
                {
                    for (int i = 0; i < AET.Count; i += 2)
                    {
                        for (int j = (int)AET[i].Cur; j < (int)AET[i + 1].Cur; j++)
                        {
                            
                            var p = GetPixelColor(j, ymin, vector3Ds, Narray,
                                MainWindow.Instance.MWHelper.ObjectColor,
                                MainWindow.Instance.MWHelper.LightColor
                                );
                            ;

                            DBmp.SetPixel(j, ymin, p);
                        }
                        // increase on m
                        AET[i].Cur += AET[i].M;
                        AET[i + 1].Cur += AET[i + 1].M;
                    }

                    ymin++;
                }

                while (next < array.Length && (int)array[next].Start.Y == ymin)
                {
                    if (array[next].Next.Start.Y >= ymin)
                    {
                        AET.Add(new Edge((int)array[next].Next.Start.Y, (int)array[next].Start.X,
                            CalcM(array[next].Next.Start, array[next].Start)
                            ));
                    }

                    if (array[next].Prev.Start.Y >= ymin)
                    {
                        AET.Add(new Edge((int)array[next].Prev.Start.Y, (int)array[next].Start.X,
                            CalcM(array[next].Prev.Start, array[next].Start)));
                    }

                    next++;
                }

                AET.RemoveAll(t => t.Ymax <= ymin);

            } while (AET.Count > 0);

        }

        /// <summary>
        /// Calculates Z vectors for each triangle vertex
        /// </summary>
        /// <param name="BezierPlane"></param>
        /// <param name="TriangleVertexes"></param>
        public static void CalculateVertexZ(Vector3D[] BezierPlane, Vector3D[,] TriangleVertexes, int Vwidth, int VHeight)
        {
            for(int Vi=0; Vi<VHeight; Vi++)
            {
                for(int Vj=0; Vj<Vwidth; Vj++)
                {
                    double sum = 0;
                    double X = TriangleVertexes[Vi, Vj].X / MainWindow.Instance.MWHelper.ImageWidth;
                    double Y = TriangleVertexes[Vi, Vj].Y / MainWindow.Instance.MWHelper.ImageHeight;

                    for (int i=0; i<4; i++)
                    {
                        for(int j=0; j<4; j++)
                        {
                            sum += BezierPlane[i*4 + j].Z * CalculateB(i, 3, X) * CalculateB(j, 3, Y);
                        }
                    }
                    TriangleVertexes[Vi,Vj].Z = sum;
                }
            }

            NsForTriangles = new Vector3D[VHeight, Vwidth];

            for (int Vi = 0; Vi < VHeight; Vi++)
            {
                for (int Vj = 0; Vj < Vwidth; Vj++)
                {
                    double X = TriangleVertexes[Vi, Vj].X / MainWindow.Instance.MWHelper.ImageWidth;
                    double Y = TriangleVertexes[Vi, Vj].Y / MainWindow.Instance.MWHelper.ImageHeight;
                    double Z = TriangleVertexes[Vi, Vj].Z;
                    NsForTriangles[Vi, Vj] = CalcuateN(BezierPlane, new Vector3D(Y, X, Z), 4, 4);
                    NsForTriangles[Vi, Vj].Normalize();
                }
            }
        }
        

        /// <summary>
        /// Calcultes color for pixel
        /// </summary>
        /// <param name="x">x-cordinate of pixel</param>
        /// <param name="y">y-cordinate of pixel</param>
        /// <param name="Triangle">Triangle for interpolation</param>
        /// <param name="L">Versor to light from x,y point</param>
        /// <param name="IO">Color of object </param>
        /// <param name="IL">Color of light</param>
        /// <param name="Ks"></param>
        /// <param name="Kd"></param>
        /// <param name="m"></param>
        /// <returns>Color for pixel</returns>
        public static Color GetPixelColor(int x, int y, 
            Vector3D[] Triangle, Vector3D[] TriangleNArr, Color IO, Color IL, 
            double Ks = 0, double Kd = 0, int m = 0)
        {
            Ks = MainWindow.Instance.MWHelper.KS;
            Kd = MainWindow.Instance.MWHelper.KD;
            m = MainWindow.Instance.MWHelper.M;
            IO = MainWindow.Instance.MWHelper.ObjectColor;
            IL = MainWindow.Instance.MWHelper.LightColor;

            if(MainWindow.Instance.MWHelper.UseTexture)
                IO = MainWindow.Instance.MWHelper.Texture.GetPixel(x,y);

            Vector3D io = new Vector3D(IO.R / 255.0, IO.G / 255.0, IO.B / 255.0);

            Vector3D il = new Vector3D(IL.R / 255.0, IL.G / 255.0, IL.B / 255.0);

            Vector3D V = new Vector3D(0, 0, 1);

            double mian = (Triangle[1].Y - Triangle[2].Y) * (Triangle[0].X - Triangle[2].X) + 
                (Triangle[2].X - Triangle[1].X) * (Triangle[0].Y - Triangle[2].Y);

            double W1 = ((Triangle[1].Y - Triangle[2].Y) * (x - Triangle[2].X) + (Triangle[2].X - Triangle[1].X) * (y - Triangle[2].Y)) / mian;

            double W2 = ((Triangle[2].Y - Triangle[0].Y) * (x - Triangle[2].X) + (Triangle[0].X - Triangle[2].X) * (y - Triangle[2].Y)) / mian;

            double W3 = 1 - W1 - W2;

            double PointZ = W1 * Triangle[0].Z + W2 * Triangle[1].Z + W3*Triangle[2].Z;

            Vector3D N = new Vector3D(W1 * TriangleNArr[0].X + W2 * TriangleNArr[1].X + W3 * TriangleNArr[2].X,
                                      W1 * TriangleNArr[0].Y + W2 * TriangleNArr[1].Y + W3 * TriangleNArr[2].Y,
                                      W1 * TriangleNArr[0].Z + W2 * TriangleNArr[1].Z + W3 * TriangleNArr[2].Z);

            if (N.Length != 0)
                N.Normalize();

            if (MainWindow.Instance.MWHelper.UseNVTexture)
            {
                Color p = MainWindow.Instance.MWHelper.NormalVectorsTexture.GetPixel(x, y);
                double Nx = (p.R / 255.0f) * 2 - 1;
                double Ny = (p.G / 255.0f) * 2 - 1;
                double Nz = (p.B / 255.0f);
                Vector3D NText = new Vector3D(Nx, Ny, Nz);
                Vector3D NSurf = CalculateNTexture(N, NText);
                NSurf.Normalize();
                N = NSurf;
            }

            Vector3D L = new Vector3D( (MainWindow.Instance.MWHelper.LightPosition.X - (double)x) / MainWindow.Instance.MWHelper.ImageWidth,
                (MainWindow.Instance.MWHelper.LightPosition.Y - (double)y) / MainWindow.Instance.MWHelper.ImageHeight,
                MainWindow.Instance.MWHelper.LightPosition.Z - PointZ);
            
            if(L.Length != 0)
                L.Normalize();


            double cosNL = Vector3D.DotProduct(N,L);

            Vector3D R = 2*Vector3D.DotProduct(N, L)*N - L;

            if (cosNL <= 0)
                cosNL = 0;
            
            double cosVR = Vector3D.DotProduct(V,R);
            if(cosVR <= 0)
                cosVR = 0;

            double temp = cosVR;
            for(int i=0; i<m; i++)
            {
                cosVR *= temp;
            }

            double Red = Kd * il.X * io.X * cosNL + Ks * il.X * io.X * cosVR;

            double Green = Kd * il.Y * io.Y * cosNL + Ks * il.Y * io.Y * cosVR;

            double Blue = Kd * il.Z * io.Z * cosNL + Ks * il.Z * io.Z * cosVR;

            if(Red > 1)
                Red = 1;

            if(Green > 1)
                Green = 1;

            if(Blue > 1)
                Blue = 1;

            return Color.FromArgb(1, (int)(Red * 255), (int)(Green * 255), (int)(Blue * 255));
        }


    }
}
