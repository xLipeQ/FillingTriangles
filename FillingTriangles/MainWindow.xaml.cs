using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FillingTriangles
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowHelper MWHelper { get; set; } = new MainWindowHelper();

        public int FuncWidth = 220;

        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = MWHelper;
            Instance = this;
            MWHelper.CreateNewBitMap(1440 - FuncWidth, 720, BezierCVN);

            Loaded += (x, y) => Keyboard.Focus(BezierCVN);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateLayout();
            
            if(ImagePlace.ActualHeight == 0)
                MWHelper.CreateNewBitMap(1440 - FuncWidth, 720, BezierCVN);
            else
                MWHelper.CreateNewBitMap((int)ImagePlace.ActualWidth, (int)ImagePlace.ActualHeight, BezierCVN);
        }

        private void TextureBtn_Click(object sender, RoutedEventArgs e)
        {
            GetImage();
        }

        private void VMapbtn_Click(object sender, RoutedEventArgs e)
        {
            GetImage(true);
        }

        private void GetImage(bool NVTexture = false)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.tif";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                    return;

                System.Drawing.Image image = System.Drawing.Image.FromFile(dialog.FileName);

                if(NVTexture)
                {
                    MWHelper.NormalVectorsTexture = new DirectBitmap(ResizeImage(image, MWHelper.ImageWidth, MWHelper.ImageHeight));
                    MWHelper.UseNVTexture = true;
                }
                else
                {
                    MWHelper.Texture = new DirectBitmap(ResizeImage(image, MWHelper.ImageWidth, MWHelper.ImageHeight));
                    MWHelper.UseTexture = true;
                }
                MWHelper.Vertexs.DrawMap();
            }
        }

        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            TextBox Tb = sender as TextBox;
            string vl = Tb.Text.Replace(".", ",");
            if (double.TryParse(vl, out double val) && val >= -1 && val <= 1)
            {
                MWHelper.BPH.SetZSelected(val);
                Keyboard.ClearFocus();
                MWHelper.CreateNewBitMap(MWHelper.ImageWidth, MWHelper.ImageHeight, BezierCVN);
            }

            else
                MessageBox.Show("Pass proper value from [-1;1]");

        }

    }
}
