using System;
using Avalonia;

namespace SciTwi.UI.Controls.Plotting;

public readonly record struct ScaleGridDims(
    double WidthInSteps,
    double HeightInSteps,
    double CoarseStepX,
    Point PixelStep,
    Point PixelBase,
    int Subdivision
);


public delegate void CanvasTransformStatus(Matrix transform);

public class CanvasTransform
{
    private readonly double minScaleLimit = 1e-3;
    private readonly double maxScaleLimit = 1e4;
    private double scale;
    private Point center = new(0.0, 0.0);

    public CanvasTransform(double scale0)
    {
        this.scale = scale0;
    }

    public Matrix Matrix { get; private set; }

    public event CanvasTransformStatus? MatrixChanged;


    private Matrix GetMatrix(Rect bounds) =>
        new(scale, 0.0, 0.0, -scale, (0.5 * bounds.Width - center.X * scale), (0.5 * bounds.Height + center.Y * scale));

    public void TryUpdateMatrix(Rect bounds)
    {
        var newMatrix = this.GetMatrix(bounds);
        if(newMatrix != this.Matrix)
        {
            this.Matrix = newMatrix;
            this.MatrixChanged?.Invoke(this.Matrix);
        }
    }

    public void Drag(Point delta, Rect bounds)
    {
        this.center -= new Point(delta.X, -delta.Y) * (1.0 / scale);
        this.TryUpdateMatrix(bounds);
    }

    public void ZoomAt(Point point, double zoom, Rect bounds)
    {
        var nextScale = Math.Clamp(this.scale * zoom, minScaleLimit, maxScaleLimit);
        var offset = point - bounds.Center;
        this.center += new Point(offset.X, -offset.Y) * (1.0 / scale - 1.0 / nextScale);
        this.scale = nextScale;
        this.TryUpdateMatrix(bounds);
    }


    private static int GetSubdivision(double frac)
    {
        if(frac > Math.Log10(5.0))
            return 0;
        if(frac > Math.Log10(2.0))
            return 1;
        return 2;
    }

    public static ScaleGridDims GridStep(Matrix m, double targetPixelSpacing, Rect pixelBounds)
    {
        var logX = Math.Log10(Math.Abs(targetPixelSpacing / m.M11));
        var stepX = Math.Pow(10.0, Math.Floor(logX));
        var factorX = stepX * m.M11;
        var factorY = stepX * m.M22;
        int subdivision = GetSubdivision(logX - Math.Floor(logX));

        return new ScaleGridDims(
            Math.Ceiling(10.0 * pixelBounds.Width / targetPixelSpacing),
            Math.Ceiling(10.0 * pixelBounds.Height / targetPixelSpacing),
            stepX,
            new Point(factorX, factorY),
            new Point(m.M31 + factorX * Math.Floor(-m.M31 / factorX), m.M32 + factorY * Math.Ceiling(-m.M32 / factorY)),
            subdivision
        );
    }
}
