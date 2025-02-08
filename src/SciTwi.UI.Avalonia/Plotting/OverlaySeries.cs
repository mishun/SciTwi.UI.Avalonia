using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;


namespace SciTwi.UI.Controls.Plotting;

internal class SeriesDrawOp : IDisposable, IEquatable<ICustomDrawOperation>, ICustomDrawOperation
{
    private readonly Rect bounds;
    private readonly Matrix transform;
    private readonly IImmutableSolidColorBrush brush;
    private readonly Point[] points;

    public SeriesDrawOp(Rect bounds, Matrix transform, IImmutableSolidColorBrush brush, Point[] points)
    {
        this.bounds = bounds;
        this.transform = transform;
        this.brush = brush;
        this.points = points;
    }

    void IDisposable.Dispose()
    {
    }

    bool IEquatable<ICustomDrawOperation>.Equals(ICustomDrawOperation? that) =>
        object.ReferenceEquals(this, that);

    Rect ICustomDrawOperation.Bounds => this.bounds;

    bool ICustomDrawOperation.HitTest(Point p) => false;

    void ICustomDrawOperation.Render(ImmediateDrawingContext context)
    {
        this.RenderGeneric(context);
//     match context.TryGetFeature<ISkiaSharpApiLeaseFeature>() with
//     | null ->
//         renderGeneric(context, bounds, transform, brush, points)
//     | leaseFeature ->
//         use lease = leaseFeature.Lease()
//         renderWithSkia(lease.SkCanvas, bounds, transform, brush.Color, points)
    }

    private void RenderGeneric(ImmediateDrawingContext ctx)
    {
        var marker = new Rect(-2.5, -2.5, 5.0, 5.0);
        foreach(var point in this.points)
        {
            var p = point.Transform(this.transform);
            using var pushed = ctx.PushPreTransform(Matrix.CreateTranslation(p.X, p.Y));
            ctx.DrawRectangle(this.brush, null, marker);
        }
    }

    private void RenderSkia(SKCanvas canvas)
    {
        canvas.Save();
        canvas.ClipRect(bounds.ToSKRect());

        var marker = new SKRect(-2.5f, -2.5f, 2.5f, 2.5f);
        using var paint = new SKPaint { Color = this.brush.Color.ToSKColor() };
        foreach(var point in this.points)
        {
            var p = point.Transform(transform);
            var save = canvas.TotalMatrix;
            canvas.Translate((float)p.X, (float)p.Y);
            canvas.DrawRect(marker, paint);
            canvas.SetMatrix(save);
        }

        canvas.Restore();
    }
}

public class OverlaySeries : OverlayBaseSeries
{
    internal override void RenderContent(DrawingContext context, Rect bounds, Matrix transform)
    {
        var points = this.Points;
        if (points is null || points.Length < 1)
            return;

        IImmutableSolidColorBrush brush =
            this.Fill switch {
                IImmutableSolidColorBrush b => b,
                ISolidColorBrush b => (IImmutableSolidColorBrush)b.ToImmutable(),
                _ => Brushes.Black
            };

        context.Custom(new SeriesDrawOp(bounds, transform, brush, points));
    }
}
