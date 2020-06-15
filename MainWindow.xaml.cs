using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Algorithms;
using QuickGraph.Graphviz.Dot;
using QuickGraph.Serialization;
using QuickGraph.Algorithms.Search;
using QuickGraph.Algorithms.Observers;
using DotBuilder;
using DotBuilder.Statements;
using DotBuilder.Attributes;
using Color = DotBuilder.Attributes.Color;
using Shape = DotBuilder.Attributes.Shape;
using Label = DotBuilder.Attributes.Label;
using System.Drawing;
using Image = System.Drawing.Image;
using Font = DotBuilder.Attributes.Font;

namespace BooleanNetworkSupportTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AdjacencyGraph<string, Edge<string>> g = new AdjacencyGraph<string, Edge<string>>();

        GraphBase graphToDraw = Graph.Directed("agraph");

        GraphBase wiringDiagram = Graph.Directed("adiagram");

        Dictionary<string, string> booleanFunctions = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.txtFunction.KeyDown += new KeyEventHandler(CheckKeys);
            Loaded += ReloadFunctions;
        }

        public static class FileDotEngine
        {
            public static Bitmap Run(string dot)
            {
                string executable = @".\external\dot.exe";
                string output = @".\Graphs\temporary";
                File.WriteAllText(output, dot);

                System.Diagnostics.Process process = new System.Diagnostics.Process();

                // Stop the process from opening a new window
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;

                // Setup executable and parameters
                process.StartInfo.FileName = executable;
                process.StartInfo.Arguments = string.Format(@"{0} -Tjpg -O", output);

                // Go
                process.Start();
                // and wait dot.exe to complete and exit
                process.WaitForExit();
                Bitmap bitmap = null; ;
                using (Stream bmpStream = System.IO.File.Open(output + ".jpg", System.IO.FileMode.Open))
                {
                    Image image = Image.FromStream(bmpStream);
                    bitmap = new Bitmap(image);
                    bmpStream.Close();
                }
                File.Delete(output);
                File.Delete(output + ".jpg");
                return bitmap;
            }
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

        private void ReloadFunctions(object sender, RoutedEventArgs e)
        {
            cmbSelectVariable.Items.Clear();

            foreach (string variable in booleanFunctions.Keys)
            {
                cmbSelectVariable.Items.Add(variable);
            }
        }

        private void ComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (cmbSelectVariable.SelectedItem is null)
            {
                return;
            }

            txtFunction.Text = booleanFunctions[cmbSelectVariable.SelectedItem.ToString()];

            txtFunction.Focus();
        }

        private void CheckKeys(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                saveFunction(sender, e);
            }
        }

        public void createSynchronousGraph()
        {
            int totalStates = Convert.ToInt32(Math.Pow(2.0, Convert.ToDouble(cmbSelectVariable.Items.Count)));

            if (cmbSelectVariable.Items.Count == 0)
            {
                graphToDraw = null;
                return;
            }
            foreach (var val in booleanFunctions.Values)
            {
                if (val == "")
                {
                    MessageBox.Show("You have entered an empty Boolean function for one or more variables!");
                    graphToDraw = null;
                    return;
                }
            }

            graphToDraw = Graph.Directed("agraph");

            graphToDraw.WithGraphAttributesOf(
                    RankDir.TB,
                    Font.Name("Arial"),
                    Font.Size(55))
                .WithNodeAttributesOf(
                    Shape.Ellipse,
                    Color.Black);

            List<List<byte>> inputs = new List<List<byte>>();
            
            for (int i = 0; i < totalStates; i++)
            {
                string binary = "00000000000" + Convert.ToString(i, 2);
                string final = binary.Substring(binary.Count() - cmbSelectVariable.Items.Count);
                List<byte> newInput = new List<byte>();
                for (int j = 0; j < cmbSelectVariable.Items.Count; j++)
                {
                    newInput.Add(Convert.ToByte(final[j].ToString()));
                }

                inputs.Add(newInput);
            }

            foreach (var state in inputs)   //add all possible states to state graph
            {
                string stateX = "";
                foreach (byte val in state)
                {
                    stateX += val;
                }

                graphToDraw.Containing(DotBuilder.Statements.Node.Name(stateX));
            }

            List<Dictionary<List<byte>, byte>> allTruthtables = new List<Dictionary<List<byte>, byte>>();

            for (int k = 0; k < cmbSelectVariable.Items.Count; k++)
            {
                Dictionary<List<byte>, byte> truthTableX = new Dictionary<List<byte>, byte>();
                foreach (List<byte> input in inputs)
                {
                    truthTableX.Add(input, evaluateBoolean(input, k));
                }

                allTruthtables.Add(truthTableX);
            }

            foreach (var pair in allTruthtables.ElementAt(0))
            {
                string inputX = "";
                string outputX = "";
                for (int l = 0; l < allTruthtables.Count; l++)
                {
                    inputX += pair.Key.ElementAt(l);
                    outputX += allTruthtables.ElementAt(l)[pair.Key];
                }

                graphToDraw.Containing(Edge.Between(inputX, outputX).WithAttributesOf(Color.Black));
            }
        }

        public void createAsynchronousGraph()
        {
            int totalStates = Convert.ToInt32(Math.Pow(2.0, Convert.ToDouble(cmbSelectVariable.Items.Count)));

            if (cmbSelectVariable.Items.Count == 0)
            {
                graphToDraw = null;
                return;
            }

            graphToDraw = Graph.Directed("agraph");

            graphToDraw.WithGraphAttributesOf(
                    RankDir.TB,
                    Font.Name("Arial"),
                    Font.Size(55))
                .WithNodeAttributesOf(
                    Shape.Ellipse,
                    Color.Black);

            List<List<byte>> inputs = new List<List<byte>>();

            for (int i = 0; i < totalStates; i++)
            {
                string binary = "000000000000000000000" + Convert.ToString(i, 2);
                string final = binary.Substring(binary.Count() - cmbSelectVariable.Items.Count);
                List<byte> newInput = new List<byte>();
                for (int j = 0; j < cmbSelectVariable.Items.Count; j++)
                {
                    newInput.Add(Convert.ToByte(final[j].ToString()));
                }

                inputs.Add(newInput);
            }

            foreach (var state in inputs)   //add all possible states to state graph
            {
                string stateX = "";
                foreach (byte val in state)
                {
                    stateX += val;
                }
                graphToDraw.Containing(DotBuilder.Statements.Node.Name(stateX));
            }

            List<Dictionary<List<byte>, byte>> allTruthtables = new List<Dictionary<List<byte>, byte>>();

            foreach (var state in inputs)
            {
                for (int i = 0; i < state.Count; i++)
                {
                    byte a = state.ElementAt(i);
                    a = Convert.ToByte((Convert.ToInt32(a) + 1) % 2);

                    if (evaluateBoolean(state, i) == a)
                    {
                        string oldState = "";
                        string newState = "";

                        for (int k = 0; k < state.Count; k++)
                        {
                            oldState += state.ElementAt(k);
                            if (k == i)
                            {
                                newState += a;
                            }
                            else
                            {
                                newState += state.ElementAt(k);
                            }
                        }

                        graphToDraw.Containing(Edge.Between(oldState, newState).WithAttributesOf(Color.Black));
                    }
                }
            }
        }

        private void drawGraph(object sender, RoutedEventArgs e)
        {
            if (graphToDraw is null)
            {
                return;
            }

            // to get the raw dot output
            var dot = graphToDraw.Render();

            //// to render to a file stream
            
            //var graphviz = new GraphViz(@"C:\Program Files (x86)\Graphviz2.38\bin", OutputFormat.Png);

            //System.GC.Collect();
            //System.GC.WaitForPendingFinalizers();
            
            //var path = System.IO.Path.Combine(Environment.CurrentDirectory, "Graphs", "temporary.png");
            //File.Delete(path);
            
            //using (var stream = new FileStream(path, FileMode.Create))
            //{
            //    graphviz.RenderGraph(graphToDraw, stream);
            //    stream.Close();
            //}

            Bitmap bm = FileDotEngine.Run(dot);

            StateGraph stateGraph = new StateGraph(bm, booleanFunctions);
            stateGraph.Show();
        }

        private void drawSynchronousGraph(object sender, RoutedEventArgs e)
        {
            createSynchronousGraph();

            drawGraph(sender, e);
        }

        private void drawAsynchronousGraph(object sender, RoutedEventArgs e)
        {
            createAsynchronousGraph();

            drawGraph(sender, e);
        }

        private void graphClusteringDialog(object sender, RoutedEventArgs e)
        {
            ClusteringDialog dialog = new ClusteringDialog(booleanFunctions.Keys.ToList());
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                drawGraphClustering(sender, e, dialog.results, dialog.variables);
            }
        }

        private void drawGraphClustering(object sender, RoutedEventArgs e, List<int> indexes, List<string> variables)
        {
            int totalStates = Convert.ToInt32(Math.Pow(2.0, Convert.ToDouble(cmbSelectVariable.Items.Count)));

            if (cmbSelectVariable.Items.Count == 0)
            {
                return;
            }

            graphToDraw = Graph.Directed("agraph");

            graphToDraw.WithGraphAttributesOf(
                    RankDir.TB,
                    Font.Name("Arial"),
                    Font.Size(55))
                .WithNodeAttributesOf(
                    Shape.Ellipse,
                    Color.Black);

            string attributes = "";

            int last = variables.LastIndexOf(variables.Last());

            while (indexes.Contains(last))
            {
                last--;
            }

            for (int i = 0; i < variables.Count; i++)
            {
                if (!indexes.Contains(i))
                {
                    attributes += variables.ElementAt(i);
                    
                    if (i != last)
                    {
                        attributes += ", ";
                    }
                }
            }

            graphToDraw.WithGraphAttributesOf(
                Label.Set("Graph showing attributes: " + attributes),
                Font.Size(16));

            g = new AdjacencyGraph<string, Edge<string>>();


            List<List<byte>> inputs = new List<List<byte>>();

            for (int i = 0; i < totalStates; i++)
            {
                string binary = "00000000000" + Convert.ToString(i, 2);
                string final = binary.Substring(binary.Count() - cmbSelectVariable.Items.Count);
                List<byte> newInput = new List<byte>();
                for (int j = 0; j < cmbSelectVariable.Items.Count; j++)
                {
                    newInput.Add(Convert.ToByte(final[j].ToString()));
                }

                inputs.Add(newInput);
            }

            List<Dictionary<List<byte>, byte>> allTruthtables = new List<Dictionary<List<byte>, byte>>();

            for (int k = 0; k < cmbSelectVariable.Items.Count; k++)
            {
                Dictionary<List<byte>, byte> truthTableX = new Dictionary<List<byte>, byte>();
                foreach (List<byte> input in inputs)
                {
                    truthTableX.Add(input, evaluateBoolean(input, k));
                }

                allTruthtables.Add(truthTableX);
            }

            foreach (var pair in allTruthtables.ElementAt(0))
            {
                string inputX = "";
                string outputX = "";
                
                for (int l = 0; l < allTruthtables.Count; l++)
                {
                    if (!indexes.Contains(l))
                    {
                        inputX += pair.Key.ElementAt(l);
                        outputX += allTruthtables.ElementAt(l)[pair.Key];
                    }
                }

                if (!g.ContainsEdge(inputX, outputX))
                {
                    graphToDraw.Containing(Edge.Between(inputX, outputX).WithAttributesOf(Color.Black));
                }

                //*****Code to convert graph for use with QuickGraph library

                string v1 = inputX;
                string v2 = outputX;
                var edge = new Edge<string>(v1, v2);

                g.AddVerticesAndEdge(edge);
            }

            if (graphToDraw is null)
            {
                return;
            }

            // to get the raw dot output
            var dot = graphToDraw.Render();

            //// to render to a file stream

            //var graphviz = new GraphViz(@"C:\Program Files (x86)\Graphviz2.38\bin", OutputFormat.Png);

            //System.GC.Collect();
            //System.GC.WaitForPendingFinalizers();

            //var path = System.IO.Path.Combine(Environment.CurrentDirectory, "Graphs", "temporary.png");
            //File.Delete(path);

            //using (var stream = new FileStream(path, FileMode.Create))
            //{
            //    graphviz.RenderGraph(graphToDraw, stream);
            //    stream.Close();
            //}

            Bitmap bm = FileDotEngine.Run(dot);

            StateGraph stateGraph = new StateGraph(bm, booleanFunctions);
            stateGraph.Show();
        }

        private void showAttractors(object sender, RoutedEventArgs e)
        {
            createSynchronousGraph();
            //g = new AdjacencyGraph<string, Edge<string>>();
            //Edge<string> e1 = new Edge<string>("1", "2");g.AddVerticesAndEdge(e1);
            //Edge<string> e2 = new Edge<string>("2", "3");g.AddVerticesAndEdge(e2);
            //Edge<string> e3 = new Edge<string>("3", "4");g.AddVerticesAndEdge(e3);
            //Edge<string> e4 = new Edge<string>("4", "5");g.AddVerticesAndEdge(e4);
            //Edge<string> e5 = new Edge<string>("5", "1");g.AddVerticesAndEdge(e5);
            //Edge<string> e6 = new Edge<string>("23", "24");g.AddVerticesAndEdge(e6);
            //Edge<string> e7 = new Edge<string>("24", "25");g.AddVerticesAndEdge(e7);
            //Edge<string> e8 = new Edge<string>("25", "26");g.AddVerticesAndEdge(e8);
            //Edge<string> e9 = new Edge<string>("26", "27");g.AddVerticesAndEdge(e9);
            //Edge<string> e10 = new Edge<string>("27", "23");g.AddVerticesAndEdge(e10);
            //Edge<string> e11 = new Edge<string>("26", "45");g.AddVerticesAndEdge(e11);
            //Edge<string> e12 = new Edge<string>("45", "88");g.AddVerticesAndEdge(e12);
            //Edge<string> e13 = new Edge<string>("88", "23");g.AddVerticesAndEdge(e13);

            CycleDetector findcycles = new CycleDetector();
            findcycles.DFS(g);      //change g
            foreach (List<string> l in findcycles.cycles)
            {
                txtOutput.AppendText("\n" + string.Join("->", l.GetRange(0, l.Count()).ToArray()));
            }
        }

        private void clearGraph(object sender, RoutedEventArgs e)
        {
            g = new AdjacencyGraph<string, Edge<string>>();
            cmbSelectVariable.Items.Clear();
            booleanFunctions.Clear();
        }

        private void importGraph(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "GRAPHML files | *.xml;*.graphml";    //file types, that will be allowed to upload
            dialog.Multiselect = false;             //deny user from uploading more than one file at a time
            bool? result = dialog.ShowDialog();

            if (result == true)     //if user clicked OK
            {
                String path = dialog.FileName;      //get name of file
                using (var xreader = XmlReader.Create(path))
                    g.DeserializeFromGraphML(xreader, id => (id), (source, target, id) => new Edge<string>(source, target));
            }

            string booleanRules = "";
            using (var xreader = XmlReader.Create(dialog.FileName))
            {
                while (xreader.Read())
                {
                    if (xreader.IsStartElement())
                    {
                        if (xreader.Name == "meta")
                        {
                            if (xreader.Read())
                            {
                                booleanRules = xreader.Value.Trim();
                                txtOutput.AppendText("\nLoaded in Boolean network\n" + booleanRules);
                            }
                        }
                    }
                }
            }

            string[] rules = booleanRules.Split('\n');

            booleanFunctions = new Dictionary<string, string>();
            cmbSelectVariable.Items.Clear();

            for (int i = 0; i < rules.Length; i++)
            {
                string var = rules[i].Split('=')[0].Trim();
                string rule = rules[i].Split('=')[1].Trim();
                cmbSelectVariable.Items.Add(var);
                booleanFunctions.Add(var, rule);
            }

            cmbSelectVariable.SelectedIndex = 0;
            saveFunction(sender, e);
        }

        private void exportGraph(object sender, EventArgs e)
        {
            g = new AdjacencyGraph<string, Edge<string>>();

            string result = "";

            foreach (string key in booleanFunctions.Keys)
            {
                string value;
                booleanFunctions.TryGetValue(key, out value);
                result += key + " = " + value + "\n";

                foreach (string k in booleanFunctions.Keys)
                {
                    if (value.Contains(k))
                    {
                        g.AddVerticesAndEdge(new Edge<string>(k, key));
                    }
                }
            }

            if (g.VertexCount == 0)
            {
                MessageBox.Show("Please create a Boolean network first!");
                return;
            }
            
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "booleannetwork"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML file (*.xml) | *.xml"; // Filter files by extension

            // Process save file dialog box results
            if (dlg.ShowDialog() == true)
            {
                using (var xwriter = XmlWriter.Create(dlg.FileName))
                {
                    g.SerializeToGraphML<string, Edge<string>, AdjacencyGraph<string, Edge<string>>>(xwriter);
                    xwriter.Close();
                }

                string fullFile = File.ReadAllText(dlg.FileName);

                string[] splitFile = fullFile.Split(new string[] { "<graph id" }, StringSplitOptions.None);

                File.Delete(dlg.FileName);

                using (var stream = File.AppendText(dlg.FileName))
                {
                    stream.Write(splitFile[0]);
                    stream.Write(@"<meta id=""rules"">
" + result + "</meta>");
                    stream.Write("<graph id");
                    stream.Write(splitFile[1]);
                }

                MessageBox.Show("Boolean network exported successfully.");
            }
        }

        private void addVariable(object sender, RoutedEventArgs e)
        {
            var newDialog = new FunctionsDialog();
            newDialog.ShowDialog();
            if (newDialog.DialogResult == true)
            {
                booleanFunctions.Add(newDialog.Result, "");
            }

            ReloadFunctions(sender, e);


            wiringDiagram.WithGraphAttributesOf(
                    RankDir.TB,
                    Font.Name("Arial"),
                    Font.Size(55))
                .WithNodeAttributesOf(
                    Shape.Ellipse,
                    Color.Black);

            wiringDiagram.Containing(DotBuilder.Statements.Node.Name(newDialog.Result));
            

            // to get the raw dot output
            var dot = wiringDiagram.Render();

            // to render to a file stream

            var graphviz = new GraphViz(@"C:\Program Files (x86)\Graphviz2.38\bin", OutputFormat.Png);

            
            GraphViewer.Source = null;
            
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            File.Delete(System.IO.Path.Combine(Environment.CurrentDirectory, "Graphs", "wiringdiagram.png"));
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "Graphs", "wiringdiagram.png");

            using (var stream = new FileStream(path, FileMode.Create))
            {
                graphviz.RenderGraph(wiringDiagram, stream);
                
                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                GraphViewer.Source = bitmap;
                
                stream.Close();
            }
        }

        private void saveFunction(object sender, RoutedEventArgs e)
        {
            //*****VALIDATE USER INPUT
            if (cmbSelectVariable.SelectedItem != null)
            {
                booleanFunctions[cmbSelectVariable.SelectedItem.ToString()] = txtFunction.Text;
            }
            else
            {
                MessageBox.Show("Please select a Boolean variable first!");
                return;
            }

            wiringDiagram = Graph.Directed("adiagram");

            wiringDiagram.WithGraphAttributesOf(
                    RankDir.TB,
                    Font.Name("Arial"),
                    Font.Size(55))
                .WithNodeAttributesOf(
                    Shape.Ellipse,
                    Color.Black);

            foreach (string key in booleanFunctions.Keys)
            {
                wiringDiagram.Containing(DotBuilder.Statements.Node.Name(key)); //make sure all variables are displayed

                string value;
                booleanFunctions.TryGetValue(key, out value);
                foreach (string k in booleanFunctions.Keys)
                {
                    if (value.Contains(k))
                    {
                        wiringDiagram.Containing(Edge.Between(k, key).WithAttributesOf(Color.Black));
                    }
                }
            }


            // to get the raw dot output
            var dot = wiringDiagram.Render();

            // to render to a file stream

            var graphviz = new GraphViz(@"C:\Program Files (x86)\Graphviz2.38\bin", OutputFormat.Png);

            GraphViewer.Source = null;

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            File.Delete(System.IO.Path.Combine(Environment.CurrentDirectory, "Graphs", "wiringdiagram.png"));
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "Graphs", "wiringdiagram.png");

            using (var stream = new FileStream(path, FileMode.Create))
            {
                graphviz.RenderGraph(wiringDiagram, stream);

                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                GraphViewer.Source = bitmap;

                stream.Close();
            }
        }

        private void removeVariable(object sender, RoutedEventArgs e)
        {
            if (cmbSelectVariable.SelectedItem != null)
            {
                booleanFunctions.Remove(cmbSelectVariable.SelectedItem.ToString());
                cmbSelectVariable.Items.Remove(cmbSelectVariable.SelectedItem);
            }
            else
            {
                MessageBox.Show("Please select a Boolean variable first!");
                return;
            }

            wiringDiagram = Graph.Directed("adiagram");

            wiringDiagram.WithGraphAttributesOf(
                    RankDir.TB,
                    Font.Name("Arial"),
                    Font.Size(55))
                .WithNodeAttributesOf(
                    Shape.Ellipse,
                    Color.Black);

            foreach (string key in booleanFunctions.Keys)
            {
                wiringDiagram.Containing(DotBuilder.Statements.Node.Name(key)); //make sure all variables are displayed

                string value;
                booleanFunctions.TryGetValue(key, out value);
                foreach (string k in booleanFunctions.Keys)
                {
                    if (value.Contains(k))
                    {
                        wiringDiagram.Containing(Edge.Between(k, key).WithAttributesOf(Color.Black));
                    }
                }
            }

            // to get the raw dot output
            var dot = wiringDiagram.Render();

            // to render to a file stream

            var graphviz = new GraphViz(@"C:\Program Files (x86)\Graphviz2.38\bin", OutputFormat.Png);


            GraphViewer.Source = null;

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            File.Delete(System.IO.Path.Combine(Environment.CurrentDirectory, "Graphs", "wiringdiagram.png"));
            var path = System.IO.Path.Combine(Environment.CurrentDirectory, "Graphs", "wiringdiagram.png");

            using (var stream = new FileStream(path, FileMode.Create))
            {
                graphviz.RenderGraph(wiringDiagram, stream);

                var bitmap = new BitmapImage();

                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                GraphViewer.Source = bitmap;

                stream.Close();
            }
        }

        public byte evaluateBoolean(List<byte> input, int index)
        {
            byte result;
            var pair = booleanFunctions.ElementAt(index);
            Dictionary<string, byte> booleanValues = new Dictionary<string, byte>();

            foreach (string variable in cmbSelectVariable.Items)
            {
                booleanValues.Add(variable, input.ElementAt(cmbSelectVariable.Items.IndexOf(variable)));
            }

            var tokenizer = new Tokenizer(pair.Value);
            Parser parser = new Parser(tokenizer.Tokenize());
            Node expr = parser.Parse();

            result = Convert.ToByte(evalBool(expr, booleanValues));

            return result;
        }

        public bool evalBool(Node node, Dictionary<string, byte> booleanValues)
        {
            if (node.ToString() == "!")
            {
                return !evalBool(node.children.First(), booleanValues);
            }
            if (node.ToString() == "&" || node.ToString() == "|")
            {
                bool left = evalBool(node.children.ElementAt(0), booleanValues);
                bool right = evalBool(node.children.ElementAt(1), booleanValues);

                if (node.ToString() == "&")
                {
                    return left && right;
                }
                else
                {
                    return left || right;
                }
            }
            else
            {
                return Convert.ToBoolean(booleanValues[node.ToString()]);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Open_UserManual(object sender, RoutedEventArgs e)
        {
            UserManual um = new UserManual();
            um.Show();
        }
    }
}
