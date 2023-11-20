using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace FillingTriangles
{
    public class MapVertexs
    {
        /// <summary>
        /// 
        /// </summary>
        public Vector3D[,] Vertices;

        private DirectBitmap DBmp;

        #region Private Properties

        private int _VerticesWidth;

        private int _CanvasWidth;

        private int _VerticesHeight;

        private int _CanvasHeight;

        #endregion

        #region Public Properties
        public int VerticesWidth
        {
            get => _VerticesWidth;
            set
            {
                if (_VerticesWidth == value)
                    return;

                _VerticesWidth = value;
                CreateVertexs(null);
                PolygonFiller.CalculateVertexZ(MainWindow.Instance.MWHelper.BPH.Bezier,
                    Vertices, _VerticesWidth, _VerticesHeight);
                DrawMap();
            }
        }

        public int VerticesHeight
        {
            get => _VerticesHeight;
            set 
            {
                if(_VerticesHeight == value) 
                    return;

                _VerticesHeight = value;
                CreateVertexs(null);
                PolygonFiller.CalculateVertexZ(MainWindow.Instance.MWHelper.BPH.Bezier,
                    Vertices, _VerticesWidth, _VerticesHeight);
                DrawMap();
            } 
        }

        public int CanvasWidth
        {
            get => _CanvasWidth;
            set => _CanvasWidth = value;
        }

        public int CanvasHeight
        {
            get => _CanvasHeight; 
            set => _CanvasHeight = value;
        }

        #endregion

        public MapVertexs(DirectBitmap Dbmp, int Vwidth = 1, int Vheight = 1, int width = 10, int height = 10, Vector3D[,] vector3Ds = null)
        {
            DBmp = Dbmp;
            _VerticesWidth = Vwidth;
            _VerticesHeight = Vheight;

            ChangeResolution(width, height);            
            CreateVertexs(vector3Ds);
            PolygonFiller.CalculateVertexZ(MainWindow.Instance.MWHelper.BPH.Bezier,
                    Vertices, _VerticesWidth, _VerticesHeight);
            DrawMap();

        }

        public void ChangeResolution(int Width, int Height)
        {
            _CanvasWidth = Width;
            _CanvasHeight = Height;
        }

        private void CreateVertexs(Vector3D[,] vector3Ds)
        {
            Vertices = new Vector3D[VerticesHeight, VerticesWidth];

            double deltaX = (double)CanvasWidth / (VerticesWidth - 1);
            double deltaY = (double)CanvasHeight / (VerticesHeight - 1);

            double PrevY = 0;
            double PrevX;
            for (int i=0; i < VerticesHeight; i++)
            {
                PrevX = 0;
                for(int j=0; j < VerticesWidth ; j++)
                {
                    Vertices[i,j] = new Vector3D(j*deltaX, i*deltaY, vector3Ds == null ? 1 : vector3Ds[i, j].Z);
                    PrevX += deltaX;
                }
                PrevY += deltaY;
            }

        }

        public void DrawMap(bool thisThread = true)
        {
            List<(Vertex[], Vector3D[])> TriangleList = new List<(Vertex[], Vector3D[])>(VerticesHeight * VerticesWidth * 2);  

            for(int i=0; i < VerticesHeight - 1; i++)
            {
                for(int j=0; j < VerticesWidth - 1; j++)
                {
                    Vertex[] arrtop = new Vertex[3];
                    arrtop[0] = new Vertex(Vertices[i,j], null, null);
                    arrtop[1] = new Vertex(Vertices[i, j + 1], arrtop[0], null);
                    arrtop[2] = new Vertex(Vertices[i+1, j], arrtop[1], arrtop[0]);

                    arrtop[0].Prev = arrtop[1].Next = arrtop[2];
                    arrtop[0].Next = arrtop[1];

                    Vector3D[] tmp = new Vector3D[3];

                    tmp[0] = PolygonFiller.NsForTriangles[i, j];
                    tmp[1] = PolygonFiller.NsForTriangles[i, j+1];
                    tmp[2] = PolygonFiller.NsForTriangles[i+1, j];

                    //PolygonFiller.ColorPolygon(arrtop, DBmp);
                    TriangleList.Add( (arrtop, tmp));

                    Vertex[] arrdown = new Vertex[3];

                    arrdown[0] = new Vertex(Vertices[i, j+1], null, null);
                    arrdown[1] = new Vertex(Vertices[i + 1, j], arrdown[0], null);
                    arrdown[2] = new Vertex(Vertices[i+1, j+1], arrdown[1], arrdown[0]);

                    arrdown[0].Prev = arrdown[1].Next = arrdown[2];
                    arrdown[0].Next= arrdown[1];


                    tmp = new Vector3D[3];

                    tmp[0] = PolygonFiller.NsForTriangles[i, j + 1];
                    tmp[1] = PolygonFiller.NsForTriangles[i + 1, j];
                    tmp[2] = PolygonFiller.NsForTriangles[i + 1, j + 1];

                    //PolygonFiller.ColorPolygon(arrdown, DBmp);
                    TriangleList.Add( (arrdown, tmp));

                }
            }

            Parallel.ForEach(TriangleList, triangle => PolygonFiller.ColorPolygon(triangle.Item1, triangle.Item2, DBmp));

            if(thisThread)
                ((MainWindowHelper)MainWindow.Instance.DataContext).UpdateBitmap();

            var list = new List<Vertex[]>(VerticesHeight*VerticesWidth*2);
            foreach(var triangle in TriangleList)
            {
                list.Add(triangle.Item1);
            }
            DrawPolygons(list);
            
        }

        public void DrawPolygons(List<Vertex[]> Triangles)
        {
            if ((bool)!MainWindow.Instance.ShowGrid.IsChecked)
                return;

            MainWindow.Instance.ShapeCVN.Children.Clear();

            foreach(var Triangle in Triangles) 
            {
                var p = new Polygon();
                p.Points = new System.Windows.Media.PointCollection()
                {
                    new System.Windows.Point(Triangle[0].Start.X,Triangle[0].Start.Y),
                    new System.Windows.Point(Triangle[1].Start.X,Triangle[1].Start.Y),
                    new System.Windows.Point(Triangle[2].Start.X,Triangle[2].Start.Y),

                };
                p.Stroke = System.Windows.Media.Brushes.LightGray;
                MainWindow.Instance.ShapeCVN.Children.Add(p);
            }

        }

        public void SetBitMap(DirectBitmap Bmp)
        {
            DBmp = Bmp;
        }

    }
}
