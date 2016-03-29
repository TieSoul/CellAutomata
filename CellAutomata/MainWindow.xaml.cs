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
using Xceed.Wpf.Toolkit;

namespace CellAutomata
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Automaton currentAutomaton;
        double[] viewportMin = new double[] { 0, 0 };
        double[] viewportMax = new double[] { 10, 20 };
        int gridSize = 50;
        int currDrawColor;
        Color[] colors;
        void setAutomaton(Automaton automaton)
        {
            currentAutomaton = automaton;
            colors = new Color[currentAutomaton.colors];
            for (var i = 0; i < colors.Length; i++)
            {
                byte c = (byte)(255 - (int)(i * 255.0 / (currentAutomaton.colors - 1)));
                colors[i] = Color.FromRgb(c, c, c);
            }
            currDrawColor = 0;
            colorUpDown.Value = 0;
            colorUpDown.Maximum = currentAutomaton.colors - 1;
        }
        void adjustViewport()
        {
            viewportMax[0] = viewportMin[0] + canvas.ActualWidth / gridSize;
            viewportMax[1] = viewportMin[1] + canvas.ActualHeight / gridSize;
        }
        void drawGrid()
        {
            List<UIElement> toRemove = new List<UIElement>();
            foreach (UIElement e in canvas.Children)
            {
                if (e is Line) toRemove.Add(e);
            }
            foreach (var e in toRemove)
            {
                canvas.Children.Remove(e);
            }
            for (var x = Math.Ceiling(viewportMin[0]); x <= viewportMax[0]; x++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.X1 = line.X2 = (x - viewportMin[0]) * canvas.ActualWidth / (viewportMax[0] - viewportMin[0]);
                line.Y1 = 0;
                line.Y2 = canvas.ActualHeight;
                canvas.Children.Add(line);
            }
            for (var y = Math.Ceiling(viewportMin[1]); y <= viewportMax[1]; y++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.Y1 = line.Y2 = (y - viewportMin[1]) * canvas.ActualHeight / (viewportMax[1] - viewportMin[1]);
                line.X1 = 0;
                line.X2 = canvas.ActualWidth;
                canvas.Children.Add(line);
            }
        }
        void drawAutomatonData(bool all)
        {
            if (all) canvas.Children.Clear();
            int bg = 0;
            if (currentAutomaton.dimensions == 2)
            {
                bg = currentAutomaton.backgroundColor;
                Rectangle rect = new Rectangle();
                rect.Height = canvas.ActualHeight;
                rect.Width = canvas.ActualWidth;
                rect.Fill = new SolidColorBrush(colors[bg]);
                rect.Fill.Freeze();
                Canvas.SetLeft(rect, 0);
                Canvas.SetTop(rect, 0);
                canvas.Children.Add(rect);
            }
            for (int y = (int)viewportMin[1]; y < viewportMax[1]; y++)
            {
                if (currentAutomaton.dimensions == 1 && !all) y = currentAutomaton.currentStep;
                if (currentAutomaton.dimensions == 1)
                {
                    bg = (y < currentAutomaton.backgroundHistory.ToArray().Length && y >= 0) ?
                        currentAutomaton.backgroundHistory[y] :
                        currentAutomaton.backgroundColor;
                    Rectangle rect = new Rectangle();
                    rect.Height = gridSize;
                    rect.Width = canvas.ActualWidth;
                    rect.Fill = new SolidColorBrush(colors[bg]);
                    rect.Fill.Freeze();
                    Canvas.SetLeft(rect, 0);
                    var top = (y - viewportMin[1]) * canvas.ActualHeight / (viewportMax[1] - viewportMin[1]);
                    if (top < 0) top = 0;
                    Canvas.SetTop(rect, top);
                    canvas.Children.Add(rect);
                }
                for (int x = (int)viewportMin[0]; x < viewportMax[0]; x++)
                {
                    
                    int color;
                    if (currentAutomaton.dimensions == 1)
                    {
                        color = currentAutomaton.getFromHistory(y, x);
                    }
                    else
                    {
                        color = currentAutomaton[x, y];
                    }
                    if (color != bg)
                    {
                        Rectangle rect = new Rectangle();
                        rect.Width = rect.Height = gridSize;
                        rect.Fill = new SolidColorBrush(colors[color]);
                        rect.Fill.Freeze();
                        Canvas.SetLeft(rect, (x - viewportMin[0]) * canvas.ActualWidth / (viewportMax[0] - viewportMin[0]));
                        var top = (y - viewportMin[1]) * canvas.ActualHeight / (viewportMax[1] - viewportMin[1]);
                        if (top < 0) top = 0;
                        Canvas.SetTop(rect, top);
                        canvas.Children.Add(rect);
                    }
                }
                if (currentAutomaton.dimensions == 1 && !all) break;
            }
            if (gridSize > 5) drawGrid();
        }
        public MainWindow()
        {
            InitializeComponent();
            setAutomaton(new OuterTotalisticAutomaton(2, 6152));
        }

        private void canvas_Loaded(object sender, RoutedEventArgs e)
        {
            adjustViewport();
            if (viewportMax[0] != viewportMin[0] && viewportMax[1] != viewportMin[1])
            {
                drawAutomatonData(true);
            }
        }

        private void canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvas.Children.Clear();
            adjustViewport();
            if (viewportMax[0] != viewportMin[0] && viewportMax[1] != viewportMin[1])
            {
                drawAutomatonData(true);
            }
        }

        private void btn_step_Click(object sender, RoutedEventArgs e)
        {
            currentAutomaton.step();
            drawAutomatonData(false);
        }

        private void canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            gridSize += e.Delta/100;
            if (gridSize < 1) gridSize = 1;
            viewportMin[0] = Math.Floor(viewportMin[0]);
            viewportMin[1] = Math.Floor(viewportMin[1]);
            adjustViewport();
            drawAutomatonData(true);
        }

        private void canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(canvas);
            var x = (int)(viewportMin[0] + pos.X / gridSize);
            var y = (int)(viewportMin[1] + pos.Y / gridSize);
            if (currentAutomaton.dimensions == 1)
            {
                if (y == currentAutomaton.currentStep) currentAutomaton[x] = currDrawColor;
                else if (y < currentAutomaton.backgroundHistory.ToArray().Length && y >= 0)
                {
                    currentAutomaton.cells = currentAutomaton.cellHistory[y];
                    currentAutomaton.backgroundColor = currentAutomaton.backgroundHistory[y];
                    currentAutomaton.cellHistory.RemoveRange(y, currentAutomaton.cellHistory.ToArray().Length - y);
                    currentAutomaton.backgroundHistory.RemoveRange(y, currentAutomaton.backgroundHistory.ToArray().Length - y);
                    currentAutomaton.currentStep = y;
                    currentAutomaton[x] = currDrawColor;
                }
            } else
            {
                currentAutomaton[x, y] = currDrawColor;
            }
            drawAutomatonData(true);
        }

        private void btn_clear_Click(object sender, RoutedEventArgs e)
        {
            currentAutomaton.backgroundHistory = new List<int>();
            currentAutomaton.cellHistory = new List<Dictionary<int[], int>>();
            currentAutomaton.currentStep = 0;
            currentAutomaton.cells = new Dictionary<int[], int>(Automaton.COMPARER);
            currentAutomaton.backgroundColor = 0;
            drawAutomatonData(true);
        }

        private Point lastPoint;

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(canvas);
                var x = (int)(viewportMin[0] + pos.X / gridSize);
                var y = (int)(viewportMin[1] + pos.Y / gridSize);
                if (currentAutomaton.dimensions == 1)
                {
                    if (y == currentAutomaton.currentStep) currentAutomaton[x] = currDrawColor;
                    else if (y < currentAutomaton.backgroundHistory.ToArray().Length && y >= 0)
                    {
                        currentAutomaton.cells = currentAutomaton.cellHistory[y];
                        currentAutomaton.backgroundColor = currentAutomaton.backgroundHistory[y];
                        currentAutomaton.cellHistory.RemoveRange(y, currentAutomaton.cellHistory.ToArray().Length - y);
                        currentAutomaton.backgroundHistory.RemoveRange(y, currentAutomaton.backgroundHistory.ToArray().Length - y);
                        currentAutomaton.currentStep = y;
                        currentAutomaton[x] = currDrawColor;
                    }
                }
                else
                {
                    currentAutomaton[x, y] = currDrawColor;
                }
                drawAutomatonData(true);
            }
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                var d = e.GetPosition(canvas) - lastPoint;
                viewportMin[0] -= d.X / gridSize;
                viewportMin[1] -= d.Y / gridSize;
                adjustViewport();
                lastPoint = e.GetPosition(canvas);
                drawAutomatonData(true);
            }
        }

        private void canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            lastPoint = e.GetPosition(canvas);
        }

        private void colorUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            currDrawColor = (int)e.NewValue;
        }
    }
}
