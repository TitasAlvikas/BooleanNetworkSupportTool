﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace BooleanNetworkSupportTool
{
    /// <summary>
    /// Interaction logic for UserManual.xaml
    /// </summary>
    public partial class UserManual : Window
    {
        public UserManual()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var path = Path.Combine(Environment.CurrentDirectory, "UserManual.txt");

            lblContent.Content = File.ReadAllText(path);
        }
    }
}
