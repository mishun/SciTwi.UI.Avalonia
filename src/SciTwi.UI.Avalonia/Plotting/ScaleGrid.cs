using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using SciTwi.UI.Controls.Plotting;

namespace SciTwi.UI.Rendering.Plotting;

static class ScaleGrid
{
    public record GridPens(Pen CoarsePen, Pen FinePen, Pen ZeroPen);
    public record RulerPens(Pen RulerPen, Brush RulerBrush);

    public static GridPens makeDefaultGridPens() =>
        new GridPens(
            new Pen(new SolidColorBrush(Color.FromRgb(210, 210, 210)), 1.0),
            new Pen(new SolidColorBrush(Color.FromRgb(230, 230, 230)), 1.0),
            new Pen(new SolidColorBrush(Colors.DarkSlateGray), 1.0)
        );

    public static RulerPens makeDefaultRulerPens()
    {
        var rulerBrush = new SolidColorBrush(Colors.Black);
        return new RulerPens(new Pen(rulerBrush, 1.0), rulerBrush);
    }


    private static readonly double[][] fineCheckpoints = [
        [0.5],
        [0.2, 0.4, 0.6, 0.8],
        [0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9]
    ];

    public static void renderScaleGrid(DrawingContext context, ref readonly ScaleGridDims dim, Matrix m, GridPens pens)
    {
        var gridT = new Matrix(dim.PixelStep.X, 0.0, 0.0, -dim.PixelStep.Y, dim.PixelBase.X, dim.PixelBase.Y);
        var subdivision = fineCheckpoints[dim.Subdivision];

        void hatch(double width, Func<double, (Point, Point)> place)
        {
            for(double position = 0.0; position <= width; position += 1.0)
            {
                {
                    var (p0, p1) = place(position);
                    context.DrawLine(pens.CoarsePen, p0.Transform(gridT), p1.Transform(gridT));
                }
                foreach(var dx in subdivision)
                {
                    var (p0, p1) = place(position + dx);
                    context.DrawLine(pens.FinePen, p0.Transform(gridT), p1.Transform(gridT));
                }
            }
        }

        var height = dim.HeightInSteps;
        hatch(dim.WidthInSteps, x => (new Point(x, 0.0), new Point(x, height)));
        var width = dim.WidthInSteps;
        hatch(dim.HeightInSteps, y => (new Point(0.0, y), new Point(width, y)));

        context.DrawLine(pens.ZeroPen, new Point(m.M31, dim.PixelBase.Y), new Point(m.M31, dim.PixelBase.Y - dim.PixelStep.Y * dim.HeightInSteps));
        context.DrawLine(pens.ZeroPen, new Point(dim.PixelBase.X, m.M32), new Point(dim.PixelBase.X + dim.PixelStep.X * dim.WidthInSteps, m.M32));
    }


    private static Rect[][] generateRulers()
    {
        var res = new List<Rect[]>();
        var cur = new List<Rect>();
        foreach(var n in new[] { 2, 5, 10 })
        {
            cur.Clear();
            for(int i = 0; i < n; i++)
            {
                cur.Add(new Rect((double)i / (double)n, ((i % 2 == 0) ? 0.0 : 0.5), 1.0 / (double)n, 0.5));
            }
            res.Add(cur.ToArray());
        }
        return res.ToArray();
    }

    private static readonly Rect[][] rulers = generateRulers();

    public static void renderScaleRuler(DrawingContext context, ref readonly ScaleGridDims dim, RulerPens pens, Typeface typeface)
    {
        var rulerT = new Matrix(dim.PixelStep.X, 0.0, 0.0, 6.0, 10.0, 10.0);

        foreach(var r in rulers[dim.Subdivision])
        {
            context.FillRectangle(pens.RulerBrush, r.TransformToAABB(rulerT));
        }
        context.DrawRectangle(pens.RulerPen, new Rect(0.0, 0.0, 1.0, 1.0).TransformToAABB(rulerT));

        void placeLabel(string text, Point p)
        {
            var ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12.0, pens.RulerBrush); // (Text = text, Typeface = typeface, FontSize = 12.0)
            context.DrawText(ft, p.Transform(rulerT) - new Point(0.5 * ft.Width, 0.0));
        }

        placeLabel("0", new Point(0.0, 1.0));
        placeLabel(dim.CoarseStepX.ToString(), new Point(1.0, 1.0));
    }
}
