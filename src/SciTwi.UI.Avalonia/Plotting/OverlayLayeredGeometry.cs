using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;

// namespace System.Runtime.CompilerServices
// {
//     internal static class IsExternalInit {}
// }

namespace SciTwi.UI.Controls.Plotting;

public abstract record LayeredGeometry;
public record LayeredGeometryLine(double A, double B, double C) : LayeredGeometry;
public record LayeredGeometryEllipse(Matrix Matrix) : LayeredGeometry;


public sealed class OverlayLayeredGeometry : OverlayBasePresenting
{
    public static readonly DirectProperty<OverlayLayeredGeometry, IEnumerable<LayeredGeometry>?> GeometryProperty =
        AvaloniaProperty.RegisterDirect<OverlayLayeredGeometry, IEnumerable<LayeredGeometry>?>
            (nameof(Geometry), o => o.Geometry, (o, v) => o.Geometry = v);


    private IEnumerable<LayeredGeometry>? geometry;

    [Content]
    public IEnumerable<LayeredGeometry>? Geometry
    {
        get => this.geometry;
        set
        {
            if(this.SetAndRaise(GeometryProperty, ref this.geometry, value))
                this.NotifyReRender();
        }
    }

    internal override void RenderContent(DrawingContext context, Rect bounds, Matrix matrix)
    {
        var geometry = this.Geometry;
        if(geometry is null)
            return;

        foreach (var g in geometry)
        {
            RenderGeometry(context, bounds, matrix, this.Fill, this.Stroke, g);
        }
    }

    private static void RenderGeometry(DrawingContext context, Rect bounds, Matrix transform, IBrush? fill, IPen? stroke, LayeredGeometry geometry)
    {
        switch(geometry)
        {
            case LayeredGeometryLine line:
                if(stroke is not null)
                {
                    var dx = line.A / Math.Sqrt(line.A * line.A + line.B * line.B);
                    var dy = line.B / Math.Sqrt(line.A * line.A + line.B * line.B);
                    var dc = -line.C / Math.Sqrt(line.A * line.A + line.B * line.B);

                    var p0 = new Point(dc * dx - dy, dc * dy + dx).Transform(transform);
                    var p1 = new Point(dc * dx + dy, dc * dy - dx).Transform(transform);
                    var delta = p1 - p0;

                    if(Math.Abs(delta.X) > Math.Abs(delta.Y))
                    {
                        var l0 = p0 + delta * ((bounds.X - p0.X) / delta.X);
                        var l1 = p0 + delta * ((bounds.Right - p0.X) / delta.X);
                        context.DrawLine(stroke, l0, l1);
                    }
                    else
                    {
                        var l0 = p0 + delta * ((bounds.Y - p0.Y) / delta.Y);
                        var l1 = p0 + delta * ((bounds.Bottom - p0.Y) / delta.Y);
                        context.DrawLine(stroke, l0, l1);
                    }
                }
                break;

            case LayeredGeometryEllipse ellipse:
                {
                    using var tmp = context.PushTransform(ellipse.Matrix * transform);
                    var pen = new Pen(fill, 2.0);
                    context.DrawRectangle(pen, new Rect (-1.0, -1.0, 2.0, 2.0), cornerRadius: 1.0f);
                }
                break;

            default:
                break;
        }
    }
}
