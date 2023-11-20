using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace FillingTriangles
{
    public class MainWindowHelper : NotifyBase
    {
        #region Private properties

        private ImageSource _image;

        private MapVertexs _Vertexs;

        private int _WindowWidth = 1024;

        private int _WindowHeight = 720;

        private int _ImageWidth = 0;

        private int _ImageHeight = 0;

        private double _KD = 0.5;

        private double _KS = 0.5;

        private int _M = 1;

        private System.Drawing.Color _ObjectColor = System.Drawing.Color.White;

        private System.Drawing.Color _LightColor = System.Drawing.Color.White;

        private bool _IsAnimating = false;

        private bool _UseTexture = false;

        private bool _UseNVTexture = false;

        private double _IntervalInMs = 50/3;

        private Vector3D LightPositionBase;

        private DispatcherTimer Timer;

        private double _AngleTick = 2*Math.PI/64;

        private double _CurrentAngle = 0.0;

        private DirectBitmap _Texture;

        private DirectBitmap _NormalVectorsTexture;


        #endregion

        #region Public properties

        public DirectBitmap Texture
        {
            get => _Texture; 
            set => _Texture = value;
        }
         
        public DirectBitmap NormalVectorsTexture
        {
            get => _NormalVectorsTexture;
            set => _NormalVectorsTexture = value;
        }

        public int ImageWidth => _ImageWidth;
        public int ImageHeight => _ImageHeight;

        public ImageSource BitmapImage
        {
            get => _image;
            set
            {
                if (_image == value)
                    return;

                _image = value;
                OnPropertyChanged(nameof(BitmapImage));
            }
        }

        public MapVertexs Vertexs
        {
            get => _Vertexs;
            set
            {
                if (_Vertexs == value)
                    return;

                _Vertexs = value;
                OnPropertyChanged(nameof(Vertexs));
            }
        }

        public DirectBitmap DBmp { get; private set; }

        public BezierPointHelper BPH { get; private set; } = new BezierPointHelper();

        public Vector3D LightPosition { get; private set; } = new Vector3D(405, 340, 1);

        #region Binded Properties
        public double KD 
        {
            get => _KD; 
            set
            {
                if(_KD == value) 
                    return;

                _KD = value;
                Vertexs.DrawMap();
            }
        }

        public double KS
        {
            get => _KS;
            set
            {
                if (_KS == value)
                    return;

                _KS = value;
                Vertexs.DrawMap();
            }
        }

        public int M
        {
            get => _M;
            set
            {
                if (_M == value)
                    return;

                _M = value;
                Vertexs.DrawMap();
            }
        }

        public System.Drawing.Color LightColor
        {
            get => _LightColor;
            set
            {
                if (_LightColor == value)
                    return;

                _LightColor = value;
                Vertexs.DrawMap();
            }
        }

        public System.Drawing.Color ObjectColor
        {
            get => _ObjectColor;
            set
            {
                if (_ObjectColor == value)
                    return;

                _ObjectColor = value;
                Vertexs.DrawMap();
            }
        }

        public double LightZ
        {
            get => LightPosition.Z;
            set
            {
                if (LightPosition.Z == value)
                    return;

                LightPosition = new Vector3D(LightPosition.X, LightPosition.Y, value);
                Vertexs.DrawMap();
            }

        }

        public int WindowWidth
        {
            get => _WindowWidth;
            set
            {
                if (_WindowWidth == value)
                    return;

                _WindowWidth = value;
            }
        }

        public int WindowHeight
        {
            get => _WindowHeight;
            set
            {
                if (_WindowHeight == value)
                    return;

                _WindowHeight = value;
            }
        }

        public bool IsAnimating
        {
            get => _IsAnimating;
            set
            {
                if (_IsAnimating == value)
                    return;

                _IsAnimating = value;
                if (_IsAnimating)
                    Timer.Start();
                else
                    Timer.Stop();
            }
        }

        public bool UseTexture
        {
            get => _UseTexture;
            set
            {
                if (_UseTexture == value || Texture == null)
                    return;

                _UseTexture = value;
                OnPropertyChanged(nameof(UseTexture));
            }
        }

        public bool UseNVTexture
        {
            get => _UseNVTexture;
            set
            {
                if (_UseNVTexture == value || NormalVectorsTexture == null)
                    return;

                _UseNVTexture = value;
                OnPropertyChanged(nameof(UseNVTexture));
            }
        }
        #endregion

        #endregion

        public MainWindowHelper()
        {
            Timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(_IntervalInMs),
            };
            Timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_CurrentAngle >= 2 * Math.PI)
                _CurrentAngle = 0.0;

            _CurrentAngle += _AngleTick;
            LightPosition = new Vector3D(LightPositionBase.X + Math.Sin(_CurrentAngle) * 300,
               LightPositionBase.Y + Math.Cos(_CurrentAngle) * 300,
                LightPosition.Z);

            Vertexs.DrawMap();
        }

        public void CreateNewBitMap(int pixelWidth, int pixelHeight, Canvas CVN)
        {
            BPH.AdjustBezierPoints(CVN, pixelWidth, pixelHeight);

            _ImageWidth = pixelWidth;
            _ImageHeight = pixelHeight;
            DBmp?.Dispose();
            DBmp = new DirectBitmap(pixelWidth + 1, pixelHeight + 1);
            Vertexs = new MapVertexs(DBmp, Vertexs == null ? 4 : Vertexs.VerticesWidth,
                Vertexs == null ? 4 : Vertexs.VerticesHeight, pixelWidth, pixelHeight, Vertexs == null ? null : Vertexs.Vertices);
            Vertexs.SetBitMap(DBmp);

            LightPositionBase = new Vector3D(pixelWidth / 2, pixelHeight / 2, 0);

            UpdateBitmap();
        }

        public void UpdateBitmap()
        {
            ((MainWindowHelper)MainWindow.Instance.DataContext).BitmapImage =
                BitMapConverter.ToImageSource(DBmp.Bitmap, ImageFormat.Bmp);

            
        }

    }
}
