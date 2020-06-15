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

namespace BooleanNetworkSupportTool
{
    /// <summary>
    /// Interaction logic for FunctionsDialog.xaml
    /// </summary>
    public partial class FunctionsDialog : Window
    {
        public FunctionsDialog()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.txtInputVariable.KeyDown += new KeyEventHandler(CheckKeys);
            txtInputVariable.Focus();
        }
        public string Result
        {
            get { return txtInputVariable.Text; }
            set { txtInputVariable.Text = value; }
        }

        private void CheckKeys(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OKButton_Click(sender, e);
            }
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
