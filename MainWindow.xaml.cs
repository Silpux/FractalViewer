using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FractalViewer.Classes;

namespace FractalViewer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Point lastMousePosition;
        private bool isDragging = false;
        private double scale = 1.0;

        private double imagePosX = 0;
        private double imagePosY = 0;

        private int imageWidth = 0;
        private int imageHeight = 0;

        private double xMin = -2.45;
        private double yMin = -1.3368;
        private double xMax = 1.4;
        private double yMax = 1.3368;

        private int newImageMaxSide = 1000;

        public MainWindow() {
            InitializeComponent();

            BitmapImage coordinateImageBitmap = new BitmapImage(new Uri("../../../Images/DefaultFractal.jpg", UriKind.Relative));

            CoordinateImage.Source = coordinateImageBitmap;
            imageWidth = coordinateImageBitmap.PixelWidth;
            imageHeight = coordinateImageBitmap.PixelHeight;
        }
        private void CoordinateCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            isDragging = true;
            lastMousePosition = e.GetPosition(CoordinateCanvas);
            CoordinateCanvas.CaptureMouse();
        }
        private void CoordinateCanvas_MouseMove(object sender, MouseEventArgs e) {
            if (isDragging) {

                Point currentMousePosition = e.GetPosition(CoordinateCanvas);
                double deltaX = currentMousePosition.X - lastMousePosition.X;
                double deltaY = currentMousePosition.Y - lastMousePosition.Y;

                imagePosX = Canvas.GetLeft(CoordinateImage) + deltaX;
                imagePosY = Canvas.GetTop(CoordinateImage) + deltaY;

                Canvas.SetLeft(CoordinateImage, imagePosX);
                Canvas.SetTop(CoordinateImage, imagePosY);

                lastMousePosition = currentMousePosition;
            }
        }

        private void CoordinateImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

        }
        private void CoordinateCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

            Point p = e.GetPosition(CoordinateCanvas);

        }

        private BitmapImage AddRectangle(BitmapImage image, Point p) {

            WriteableBitmap writableBitmap = new WriteableBitmap(image);
            var width = 10;
            var height = 10;

            var rect = new Int32Rect((int)(p.X / scale + ((image.PixelWidth) - ((image.PixelWidth) / scale)) / 2 - imagePosX * (1 / scale)), (int)(p.Y / scale + ((image.PixelHeight) - ((image.PixelHeight) / scale)) / 2 - imagePosY * (1 / scale)), width, height);

            Color color = Colors.Red;

            int colorValue = color.A << 24 | color.R << 16 | color.G << 8 | color.B;

            var buffer = Enumerable.Repeat(colorValue, width * height).ToArray();

            writableBitmap.WritePixels(rect, buffer, width * 4, 0);
            return ConvertWriteableBitmapToBitmapImage(writableBitmap);
        }


        private void OnStartFractalsClick(object sender, RoutedEventArgs e) {

            xMin = -2.45;
            xMax = 1.4;

            yMin = -1.3368;
            yMax = 1.3368;

            CoordinateImage.Source = DrawFractal((int)IterationNumberSlider.Value);

        }

        private void OnDrawFractalsClick(object sender, RoutedEventArgs e) {

            Point pMin = GetPointOnCoordinateImage(new Point(0, 0));
            Point pMax = GetPointOnCoordinateImage(new Point(CoordinateCanvas.ActualWidth, CoordinateCanvas.ActualHeight));

            double xMinPercent = pMin.X / imageWidth;
            double xMaxPercent = pMax.X / imageWidth;
            double yMinPercent = pMin.Y / imageHeight;
            double yMaxPercent = pMax.Y / imageHeight;

            double xMinCopy = xMin;
            double xMaxCopy = xMax;

            double yMinCopy = yMin;
            double yMaxCopy = yMax;

            xMin = xMinCopy + (xMaxCopy - xMinCopy) * xMinPercent;
            xMax = xMinCopy + (xMaxCopy - xMinCopy) * xMaxPercent;

            yMin = yMinCopy + (yMaxCopy - yMinCopy) * yMinPercent;
            yMax = yMinCopy + (yMaxCopy - yMinCopy) * yMaxPercent;

            CoordinateImage.Source = DrawFractal((int)IterationNumberSlider.Value);

        }

        private Point GetPointOnCoordinateImage(Point p) {
            return new Point((p.X / scale + ((imageWidth) - ((imageWidth) / scale)) / 2 - imagePosX * (1 / scale)), (p.Y / scale + ((imageHeight) - ((imageHeight) / scale)) / 2 - imagePosY * (1 / scale)));
        }

        private BitmapImage DrawFractal(int maxIterations) {

            double widthD = Math.Abs(xMax - xMin);
            double heightD = Math.Abs(yMax - yMin);

            double edgeValue = edgeValueSlider.Value;

            int width = (int)(widthD > heightD ? newImageMaxSide : newImageMaxSide * (widthD / heightD));
            int height = (int)(heightD >= widthD ? newImageMaxSide : newImageMaxSide * (heightD / widthD));

            WriteableBitmap writableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);

            imageWidth = width;
            imageHeight = height;

            double dx = (xMax - xMin) / (width - 1);
            double dy = (yMax - yMin) / (height - 1);

            for (int i = 0; i < width; i++) {

                for (int j = 0; j < height; j++) {

                    double x = xMin + i * dx;
                    double y = yMin + j * dy;

                    Complex z = new Complex(x, y);
                    Complex c = z;

                    int iterations = 0;

                    while (z.Magnitude < edgeValue && iterations < maxIterations) {
                        z = z * z + c;
                        iterations++;
                    }

                    Color fractalColor = GetColor(iterations, maxIterations);

                    SetPixelColor(writableBitmap, i, j, fractalColor);

                }
            }

            scale = CoordinateCanvas.ActualWidth / imageWidth;
            CoordinateImage.RenderTransform = new ScaleTransform(scale, scale);

            imagePosX = imageWidth / 2 * (scale - 1);
            imagePosY = imageHeight / 2 * (scale - 1);


            Canvas.SetLeft(CoordinateImage, imagePosX);
            Canvas.SetTop(CoordinateImage, imagePosY);


            return ConvertWriteableBitmapToBitmapImage(writableBitmap);
        }
        public static Color GetColor(int iteration, int maxIterations) {
            if (iteration == maxIterations) {
                return Colors.Black;
            }

            double t = (double)iteration / maxIterations;

            //byte r = (byte)(9 * (1 - t) * t * t * t * 255);
            //byte g = (byte)(15 * (1 - t) * (1 - t) * t * t * 255);
            //byte b = (byte)(9 * (1 - t) * (1 - t) * (1 - t) * t * 255);

            var colors = new[]
            {
                
                Colors.DarkRed,
                Colors.Orange,

                Color.FromRgb(0xd2, 0xab, 0x99),
                Color.FromRgb(0xbd, 0xbe, 0xa9),
                Color.FromRgb(0x8d, 0xb3, 0x8b),
                Color.FromRgb(0x56, 0x87, 0x6d),
                Color.FromRgb(0x04, 0x72, 0x4d),
                Color.FromRgb(0xb8, 0xd8, 0xba),
                Color.FromRgb(0xd9, 0xdb, 0xbc),
                Color.FromRgb(0xfc, 0xdd, 0xbc),
                Color.FromRgb(0xef, 0x95, 0x9d),
                Color.FromRgb(0x69, 0x58, 0x5f),


                Color.FromRgb(0x34, 0x30, 0x90),
                Color.FromRgb(0x5f, 0x59, 0xf7),
                Color.FromRgb(0x65, 0x92, 0xfd),
                Color.FromRgb(0x44, 0xc2, 0xfd),
                Color.FromRgb(0x8c, 0x61, 0xff),

            };

            double changeFactor = 0.9;

            int power = 0;

            double currentFactor = changeFactor;
            double factorReverseVal = 1 - currentFactor;

            while (factorReverseVal < t) {
                currentFactor *= changeFactor;
                factorReverseVal = 1 - currentFactor;
                power++;
            }

            double prevColorFactor = 1 - currentFactor / changeFactor;
            double nextColorFactor = 1 - currentFactor;

            double localFactor = (t - prevColorFactor) / (nextColorFactor - prevColorFactor);

            Color c1 = power == 0 ? Colors.Black : colors[power % colors.Length];
            Color c2 = colors[(power + 1) % colors.Length];

            byte r = (byte)(c1.R + localFactor * (c2.R - c1.R));
            byte g = (byte)(c1.G + localFactor * (c2.G - c1.G));
            byte b = (byte)(c1.B + localFactor * (c2.B - c1.B));

            return Color.FromRgb(r, g, b);
        }
        private Color InterpolateColors(Color startColor, Color endColor, double factor) {
            byte r = InterpolateColorComponent(startColor.R, endColor.R, factor);
            byte g = InterpolateColorComponent(startColor.G, endColor.G, factor);
            byte b = InterpolateColorComponent(startColor.B, endColor.B, factor);

            return Color.FromRgb(r, g, b);
        }

        private byte InterpolateColorComponent(byte start, byte end, double factor) {
            return (byte)(start + (end - start) * factor);
        }

        private void SetPixelColor(WriteableBitmap writableBitmap, int x, int y, Color color) {
            byte[] colorBytes = { color.B, color.G, color.R, color.A };
            int stride = (writableBitmap.PixelWidth * 4);
            int index = y * stride + x * 4;
            writableBitmap.WritePixels(new Int32Rect(x, y, 1, 1), colorBytes, 4, 0);
        }

        public BitmapImage ConvertWriteableBitmapToBitmapImage(WriteableBitmap wbm) {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream()) {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }


        private void OnSaveFractalImageClick(object sender, RoutedEventArgs e) {

            if (CoordinateImage.Source is BitmapSource bitmapSource) {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "JPEG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png|All Files (*.*)|*.*";
                saveDialog.Title = "Save Image";

                if (saveDialog.ShowDialog() == true) {
                    string filePath = saveDialog.FileName;
                    string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();

                    BitmapEncoder encoder;
                    if (fileExtension == ".jpg" || fileExtension == ".jpeg") {
                        encoder = new JpegBitmapEncoder();
                    }
                    else if (fileExtension == ".png") {
                        encoder = new PngBitmapEncoder();
                    }
                    else {
                        encoder = new PngBitmapEncoder();
                    }

                    encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                    using (FileStream stream = new FileStream(filePath, FileMode.Create)) {
                        encoder.Save(stream);
                    }

                    MessageBox.Show("Image saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
            else {
                MessageBox.Show("No image to save.");
            }


        }

        private void CoordinateCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            isDragging = false;
            CoordinateCanvas.ReleaseMouseCapture();
        }

        private void CoordinateCanvas_MouseWheel(object sender, MouseWheelEventArgs e) {
            Point p = e.GetPosition(CoordinateCanvas);
            Point pc = p;

            p = new Point((p.X / scale + ((imageWidth) - ((imageWidth) / scale)) / 2 - imagePosX * (1 / scale)), (p.Y / scale + ((imageHeight) - ((imageHeight) / scale)) / 2 - imagePosY * (1 / scale)));

            double deltaScale = e.Delta > 0 ? 1.1 : 0.9;
            scale *= deltaScale;

            scale = Math.Max(0.1, Math.Min(100, scale));

            imagePosX = pc.X - (p.X * scale - (imageWidth * scale - imageWidth) / 2);
            imagePosY = pc.Y - (p.Y * scale - (imageHeight * scale - imageHeight) / 2);
            CoordinateImage.RenderTransform = new ScaleTransform(scale, scale);
            Canvas.SetLeft(CoordinateImage, imagePosX);
            Canvas.SetTop(CoordinateImage, imagePosY);
        }
    }
}