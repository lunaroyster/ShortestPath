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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShortestPath
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Node
    {
        string data;
        //public bool isSelected;
        public Point location;
        public List<Connection> Exits = new List<Connection>();
        public List<Connection> Entries = new List<Connection>();
        public Ellipse LinkedEllipse;
        //public double DistanceToExit(Node Exit)
        //{
        //    if(Exits.Contains(Exit))
        //    {
        //        double dx = Math.Abs(Exit.location.X - location.X);
        //        double dy = Math.Abs(Exit.location.Y - location.Y);
        //        return Math.Sqrt(Math.Pow(dx,2) + Math.Pow(dy, 2));
        //    }
        //    else
        //    {
        //        return double.PositiveInfinity;
        //    }
        //}
    }
    public class Connection
    {
        public Node A;
        public Node B;
        public Line LinkedLine;
        public Decimal Length
        {
            get
            {
                return (decimal)Math.Sqrt(Math.Pow(LinkedLine.X2 - LinkedLine.X1, 2) + Math.Pow(LinkedLine.Y2 - LinkedLine.Y1, 2));
            }
        }
        public void draw()
        {
            Ellipse lA = A.LinkedEllipse;
            Ellipse lB = B.LinkedEllipse;
            LinkedLine.X1 = Canvas.GetLeft(lA) + 16;
            LinkedLine.Y1 = Canvas.GetTop(lA) + 16;
            LinkedLine.X2 = Canvas.GetLeft(lB) + 16;
            LinkedLine.Y2 = Canvas.GetTop(lB) + 16;
        }
        public void initializeLine()
        {
            Console.WriteLine("Initializing Line");
            LinkedLine.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            LinkedLine.Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            LinkedLine.Height = double.NaN;
            LinkedLine.Width = double.NaN;
        }
    }

    public partial class MainWindow : Window
    {
        public class NodeData
        {
            public bool evaluated = false;
            public Node LinkedNode;
            public List<Connection> ShortestRoute = new List<Connection>();
            public decimal ShortestRouteLength = decimal.MaxValue;
            public NodeData(Node node)
            {
                LinkedNode = node;
            }
            public NodeData(Node node, decimal shortestRouteLength)
            {
                LinkedNode = node;
                ShortestRouteLength = shortestRouteLength;
            }
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        Ellipse selectedEllipse = null;
        Ellipse FirstEllipse = null;
        Ellipse HomeEllipse = null;
        Ellipse EndEllipse = null;
        private void Ellipse_MouseDown(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            selectedEllipse = ellipse;
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                HomeEllipse = ellipse;
                HomeEllipse.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                EndEllipse = ellipse;
                EndEllipse.Fill = new SolidColorBrush(Color.FromRgb(0,255,0));
            }
        }
        private void Ellipse_MouseUp(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            selectedEllipse = null;
        }

        private void Ellipse_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            if (FirstEllipse==null)
            {
                FirstEllipse = ellipse;
            }
            else
            {
                CreateRoute(FirstEllipse, ellipse);
                FirstEllipse = null;
            }
        }

        public void CreateRoute(Ellipse A, Ellipse B)
        {
            Node nA = A.Tag as Node;
            Node nB = B.Tag as Node;
            Connection con = new Connection();
            Line LinkedLine = new Line();
            con.A = nA;
            con.B = nB;
            nA.Exits.Add(con);
            nB.Entries.Add(con);
            con.LinkedLine = LinkedLine;
            Canv.Children.Add(LinkedLine);
            con.initializeLine();
            con.draw();
        }

        private Ellipse CreateEllipse(Point loc)
        {
            Console.WriteLine("Adding ellipse");
            Ellipse elle = new Ellipse();
            Canv.Children.Add(elle);
            elle.Height = 32;
            elle.Width = 32;
            elle.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            elle.MouseUp += Ellipse_MouseUp;
            elle.MouseDown += Ellipse_MouseDown;
            elle.MouseRightButtonUp += Ellipse_MouseRightButtonUp;
            Node linkedNode = new Node();
            linkedNode.location = loc;
            elle.Tag = linkedNode;
            linkedNode.LinkedEllipse = elle;
            Canvas.SetLeft(elle, loc.X - 24);
            Canvas.SetTop(elle, loc.Y - 24);
            return elle;
        }

        private void Canv_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && selectedEllipse!=null)
            {
                Canvas.SetLeft(selectedEllipse, e.GetPosition(null).X - 24);
                Canvas.SetTop(selectedEllipse, e.GetPosition(null).Y - 24);
                Node selectedNode = selectedEllipse.Tag as Node;
                selectedNode.Entries.ForEach(delegate (Connection c)
                 {
                     c.draw();
                 });
                selectedNode.Exits.ForEach(delegate (Connection c)
                {
                    c.draw();
                });
                drawRoute();
            }
        }

        private void Canv_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.LeftCtrl)
            {
                Point loc = Mouse.GetPosition(null);
                Ellipse elle = CreateEllipse(loc);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Canv.Focus();
        }

        public void drawRoute()
        {
            foreach (Line l in Canv.Children.OfType<Line>())
            {
                l.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                l.Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                l.StrokeThickness = 1;
            }
            List<Connection> ConnectionList = ShortestConnectionList((Node)HomeEllipse.Tag, (Node)EndEllipse.Tag);
            ConnectionList.ForEach(delegate (Connection cn)
            {
                cn.LinkedLine.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 0));
                cn.LinkedLine.Stroke = new SolidColorBrush(Color.FromRgb(128, 128, 0));
                cn.LinkedLine.StrokeThickness = 5;
            });
        }

        public List<Connection> ShortestConnectionList(Node HomeNode, Node EndNode)
        {
            List<NodeData> NodeDataList = new List<NodeData>();
            NodeData HomeNodeData = new NodeData(HomeNode, 0);
            NodeDataList.Add(HomeNodeData);
            while (NodeDataList.FindIndex(nodedata => nodedata.evaluated == false) > -1)
            {
                decimal shortestRouteLength = decimal.MaxValue;
                NodeData BaseNodeData = null;
                foreach (NodeData nd in NodeDataList)
                {
                    if (nd.ShortestRouteLength < shortestRouteLength && !nd.evaluated)
                    {
                        BaseNodeData = nd;
                        shortestRouteLength = nd.ShortestRouteLength;
                    }
                }
                foreach (Connection c in BaseNodeData.LinkedNode.Exits)
                {
                    Node connectedNode = c.B;
                    decimal currentRouteLength = BaseNodeData.ShortestRouteLength + c.Length;
                    List<Connection> currentRoute = new List<Connection>(BaseNodeData.ShortestRoute);
                    currentRoute.Add(c);
                    List<NodeData> NDList = NodeDataList.FindAll(nd => nd.LinkedNode == connectedNode);
                    if (NDList.Count == 1)
                    {
                        if(currentRouteLength < NDList[0].ShortestRouteLength)
                        {
                            NDList[0].ShortestRouteLength = currentRouteLength;
                            NDList[0].ShortestRoute = currentRoute;
                        }
                    }
                    if (NDList.Count == 0)
                    {
                        NodeData ND = new NodeData(connectedNode);
                        ND.ShortestRouteLength = currentRouteLength;
                        ND.ShortestRoute = currentRoute;
                        NodeDataList.Add(ND);
                    }
                }
                BaseNodeData.evaluated = true;
            }
            NodeData EndNodeData = NodeDataList.Find(nd => nd.LinkedNode == EndNode);
            return EndNodeData.ShortestRoute;
    }

    }
}
