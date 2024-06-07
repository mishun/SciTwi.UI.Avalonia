using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

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
        }

        public Point[] Points { get; }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
