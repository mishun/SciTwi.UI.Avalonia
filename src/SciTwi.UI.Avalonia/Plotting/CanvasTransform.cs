using System;
using Avalonia;
using Avalonia.Media;
using SciTwi.UI.Rendering;


namespace SciTwi.UI.Controls.Plotting;


// internal record struct ScaleGridDims(
//     double WidthInSteps,
//     double HeightInSteps,
//     double CoarseStepX,
//     Point PixelStep,
//     Point PixelBase,
//     int Subdivision
// );

public class CanvasTransform : ITransform, IMutableTransform
{
    private readonly double minScale = 1e-3;
    private readonly double maxScale = 1e4;
    private double scale;
    private Point center = new Point(0.0, 0.0);

    public CanvasTransform(double scale0)
    {
        this.scale = scale0;
    }

    public Matrix Matrix { get; private set; }

    Matrix ITransform.Value => this.Matrix;

    public event EventHandler? Changed;


    private Matrix GetMatrix(Rect bounds) =>
        new(scale, 0.0, 0.0, -scale, (0.5 * bounds.Width - center.X * scale), (0.5 * bounds.Height + center.Y * scale));

    public void UpdateMatrix(Rect bounds)
    {
        var newMatrix = this.GetMatrix(bounds);
        if (newMatrix != this.Matrix)
        {
            this.Matrix = newMatrix;
            this.Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Drag(Point delta, Rect bounds)
    {
        this.center -= new Point (delta.X, -delta.Y) * (1.0 / scale);
        this.UpdateMatrix(bounds);
    }

    public void ZoomAt(Point point, double zoom, Rect bounds)
    {
        var nextScale = Math.Clamp(this.scale * zoom, minScale, maxScale);
        var offset = point - bounds.Center;
        this.center += new Point (offset.X, -offset.Y) * (1.0 / scale - 1.0 / nextScale);
        this.scale = nextScale;
        this.UpdateMatrix(bounds);
    }

    public ScaleGridDims GridStep (double targetPixelSpacing, Rect bounds)
    {
        var log = Math.Log10(targetPixelSpacing / scale);
        var stepX = Math.Pow(10.0, Math.Floor(log));
        var m = this.GetMatrix(bounds);
        var factorX = stepX * m.M11;
        var factorY = stepX * m.M22;

        var frac = log - Math.Floor(log);
        int subdivision = 2;
        if(frac > Math.Log10(5.0))
            subdivision = 0;
        else if(frac > Math.Log10(2.0))
            subdivision = 1;

        return new ScaleGridDims(
            Math.Ceiling(10.0 * bounds.Width / targetPixelSpacing),
            Math.Ceiling(10.0 * bounds.Height / targetPixelSpacing),
            stepX,
            new Point(factorX, factorY),
            new Point(m.M31 + factorX * Math.Floor(-m.M31 / factorX), m.M32 + factorY * Math.Ceiling(-m.M32 / factorY)),
            subdivision
        );
    }
}
