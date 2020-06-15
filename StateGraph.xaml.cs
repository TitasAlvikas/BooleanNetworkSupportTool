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
using System.Windows.Shapes;
using System.IO;
using Path = System.IO.Path;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Net;
using System.Drawing.Imaging;

namespace BooleanNetworkSupportTool
{
    /// <summary>
    /// Interaction logic for StateGraph.xaml
    /// </summary>
    public partial class StateGraph : Window
    {
        Bitmap bm = null;

        Dictionary<string, string> booleanFunctions = new Dictionary<string, string>();

        public StateGraph(Bitmap bm, Dictionary<string, string> booleanFunctions)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.bm = bm;
            this.booleanFunctions = booleanFunctions;
            Loaded += Window_Loaded;
        }

        private void UIElement_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var matrix = GraphViewer.RenderTransform.Value;

            if (e.Delta > 0)
            {
                matrix.ScaleAt(1.5, 1.5, e.GetPosition(this).X, e.GetPosition(this).Y);
            }
            else
            {
                matrix.ScaleAt(1.0 / 1.5, 1.0 / 1.5, e.GetPosition(this).X, e.GetPosition(this).Y);
            }

            GraphViewer.RenderTransform = new MatrixTransform(matrix);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            GraphViewer.Source = null;
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GraphViewer.Source = ImageSourceFromBitmap(bm);

            foreach (string variable in booleanFunctions.Keys)
            {
                string value = "";
                booleanFunctions.TryGetValue(variable, out value);
                txtRules.AppendText(variable + " = " + value + "\n");
            }
        }

        private void saveGraph(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "stategraph"; // Default file name
            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "PNG file (*.png) | *.png"; // Filter files by extension

            // Process save file dialog box results
            if (dlg.ShowDialog() == true)
            {
                bm.Save(dlg.FileName, ImageFormat.Png);
            }
        }
    }
}
