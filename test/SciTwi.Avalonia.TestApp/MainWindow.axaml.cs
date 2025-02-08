using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using SciTwi.UI.Controls.Plotting;

namespace SciTwi.Avalonia
{
    public class MainWindowViewModel : ReactiveObject
    {
        public MainWindowViewModel()
        {
            var list = new List<Point>();
            for(int i = 0; i < 2000; i++)
                list.Add(new Point(i, 100.0 * Math.Sin(0.02 * i)));
            this.Points = list.ToArray();

            this.LayeredGeometries = [
                new LayeredGeometryLine(1, 1, 1),
                new LayeredGeometryEllipse(new Matrix(100, 0, 0, 100, 250, 250))
            ];
        }

        public Point[] Points { get; }

        public LayeredGeometry[] LayeredGeometries { get;}
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
