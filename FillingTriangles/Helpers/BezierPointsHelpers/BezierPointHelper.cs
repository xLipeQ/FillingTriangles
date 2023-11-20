using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace FillingTriangles
{
    public class BezierPointHelper : NotifyBase
    {
        private Canvas _Cnv;

        private int _BezierSelected = -1;

        public Vector3D[] Bezier { get; set; } = new Vector3D[16]
        {
            new Vector3D(0, 0, 1.0),
            new Vector3D(1/3.0, 0, 1.0),
            new Vector3D(2/3.0, 0, 1.0),
            new Vector3D(1, 0, 1.0),

            new Vector3D(0, 1/3.0, 1.0),
            new Vector3D(1/3.0, 1/3.0, 1.0),
            new Vector3D(2/3.0, 1/3.0, 1.0),
            new Vector3D(1, 1/3.0, 1.0),

            new Vector3D(0, 2/3.0, 1.0),
            new Vector3D(1/3.0, 2/3.0, 1.0),
            new Vector3D(2/3.0, 2/3.0, 1.0),
            new Vector3D(1, 2/3.0, 1.0),

            new Vector3D(0, 1.0, 1.0),
            new Vector3D(1/3.0, 1, 1.0),
            new Vector3D(2/3.0, 1, 1.0),
            new Vector3D(1, 1, 1.0),
        };

        public double X
        {
            get => _BezierSelected < 0 ? -1 : Bezier[_BezierSelected].X;
            set
            {
                Bezier[_BezierSelected].X = value;
                OnPropertyChanged(nameof(X));
            }
        }
        public double Y
        {
            get => _BezierSelected < 0 ? -1 : Bezier[_BezierSelected].Y;
            set
            {
                Bezier[_BezierSelected].Y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
        public double Z
        {
            get => _BezierSelected < 0 ? -1 : Bezier[_BezierSelected].Z;
            set
            {
                Bezier[_BezierSelected].Z = value;
                OnPropertyChanged(nameof(Z));
            }
        }

        public void AdjustBezierPoints(Canvas CVN, double width, double height)
        {
            _Cnv = CVN;
            CVN.Children.Clear();
            int i = 0; 
            foreach(var Vector in Bezier)
            {
                DrawPoint(width * Vector.X, height * Vector.Y, i);
                i++;
            }
        }

        public void UnSelectBezier()
        {
            _BezierSelected = -1;
        }

        public void IncreaseVectorZ(double delta = 0.05)
        {
            if (Z >= 1 || _BezierSelected < 0)
                return;

            Z += delta;
        }

        public void DecreaseVectorZ(double delta = 0.05)
        {
            if (Z <= -1 || _BezierSelected < 0) 
                return;

            Z -= delta;
        }

        public void SetZSelected(double value)
        {
            if (_BezierSelected < 0)
                return;

            if(value >= -1 && value <= 1)   
                Z = value;
        }

        private void DrawPoint(double X, double Y, int index)
        {
            BezierPoint BP = new BezierPoint();
            Canvas.SetLeft(BP, X);
            Canvas.SetTop(BP, Y);
            BP.AssignedVectorIndex = index;
            BP.MouseDown += BP_MouseDown;
            _Cnv.Children.Add(BP);
        }

        private void BP_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BezierPoint BP = (BezierPoint)sender;
            _BezierSelected = BP.AssignedVectorIndex;
            X = Bezier[_BezierSelected].X;
            Y = Bezier[_BezierSelected].Y;
            Z = Bezier[_BezierSelected].Z;
        }
    }
}
