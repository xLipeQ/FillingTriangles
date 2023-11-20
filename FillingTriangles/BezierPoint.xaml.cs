using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FillingTriangles
{
    /// <summary>
    /// Logika interakcji dla klasy BezierPoint.xaml
    /// </summary>
    public partial class BezierPoint : UserControl
    {
        public int AssignedVectorIndex { get; set; }

        public BezierPoint()
        {
            InitializeComponent();
        }
    }
}
