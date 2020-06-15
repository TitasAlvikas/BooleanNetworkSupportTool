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
    /// Interaction logic for ClusteringDialog.xaml
    /// </summary>
    public partial class ClusteringDialog : Window
    {
        public List<string> variables;

        private List<CheckBox> checkboxes = new List<CheckBox>();

        public List<int> results = new List<int>();
        
        public ClusteringDialog(List<string> variables)
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            Loaded += Window_Loaded;
            this.variables = variables;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StackPanel innerStack = new StackPanel { Orientation = Orientation.Vertical };
            
            foreach (string var in variables)
            {
                CheckBox checkBox = new CheckBox();
                checkBox.Name = "checkbox" + variables.IndexOf(var).ToString();
                checkBox.Content = var;
                innerStack.Children.Add(checkBox);
                checkboxes.Add(checkBox);

            }

            Grid.SetColumn(innerStack, 0 /*Set the column of your stackPanel, default is 0*/);
            Grid.SetRow(innerStack, 0 /*Set the row of your stackPanel, default is 0*/);
            Grid.SetColumnSpan(innerStack, 1 /*Set the columnSpan of your stackPanel, default is 1*/);
            Grid.SetRowSpan(innerStack,  2/*Set the rowSpan of your stackPanel, default is 1*/);
            
            innerStack.UpdateLayout();
            mainGrid.Children.Add(innerStack);
            innerStack.UpdateLayout();
            mainGrid.UpdateLayout();
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (CheckBox checkbox in checkboxes)
            {
                if ((bool) checkbox.IsChecked)
                {
                        int val = Convert.ToInt32(checkbox.Name.Split('x')[1]);
                        results.Add(val);
                }
            }

            DialogResult = true;
        }
    }
}
