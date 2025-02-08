using System;
using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.Media;

namespace SciTwi.UI.Controls.Plotting;

public abstract class OverlayBase : StyledElement
{
    public static readonly DirectProperty<OverlayBase, bool> IsVisibleProperty =
        AvaloniaProperty.RegisterDirect<OverlayBase, bool>
            (nameof(IsVisible), o => o.IsVisible, (o, v) => o.IsVisible = v);

    public static readonly DirectProperty<OverlayBase, Matrix> TransformProperty =
        AvaloniaProperty.RegisterDirect<OverlayBase, Matrix>
            (nameof(Transform), o => o.Transform, (o, v) => o.Transform = v);


    private bool isVisible = true;
    private Matrix transform = Matrix.Identity;


    public bool IsVisible
    {
        get => this.isVisible;
        set
        {
            if (this.SetAndRaise(IsVisibleProperty, ref this.isVisible, value))
                this.NotifyReRender(true);
        }
    }

    public Matrix Transform
    {
        get => this.transform;
        set
        {
            if (this.SetAndRaise(TransformProperty, ref this.transform, value))
                this.NotifyReRender();
        }
    }

    internal abstract void RenderPass(DrawingContext context, Rect bounds, Matrix parentTransform);

    internal void NotifyReRender(bool ignoreCurrentVisibility = false)
    {
        for (ILogical? node = ignoreCurrentVisibility ? this.GetLogicalParent() : this; node is not null; node = node.GetLogicalParent())
            switch (node)
            {
                case PlotOverlayHost owner:
                    owner.NotifyRenderGraphChanged();
                    return;

                case OverlayBase layer:
                    if (!layer.IsVisible)
                        return;
                    break;
            }
    }
}

public abstract class OverlayBasePresenting : OverlayBase
{
    public static readonly DirectProperty<OverlayBasePresenting, IBrush?> FillProperty =
        AvaloniaProperty.RegisterDirect<OverlayBasePresenting, IBrush?>
            (nameof(Fill), o => o.Fill, (o, v) => o.Fill = v);

    public static readonly DirectProperty<OverlayBasePresenting, IPen?> StrokeProperty =
        AvaloniaProperty.RegisterDirect<OverlayBasePresenting, IPen?>
            (nameof(Stroke), o => o.Stroke, (o, v) => o.Stroke = v);


    private IBrush? fill;
    private IPen? stroke;


    public IBrush? Fill
    {
        get => this.fill;
        set
        {
            if (this.SetAndRaise(FillProperty, ref this.fill, value))
                this.NotifyReRender();
        }
    }

    public IPen? Stroke
    {
        get => this.stroke;
        set
        {
            if (this.SetAndRaise(StrokeProperty, ref this.stroke, value))
                this.NotifyReRender();
        }
    }


    internal sealed override void RenderPass(DrawingContext context, Rect bounds, Matrix parentTransform)
    {
        if (this.IsVisible)
            this.RenderContent(context, bounds, this.Transform * parentTransform);
    }

    internal abstract void RenderContent(DrawingContext context, Rect bounds, Matrix transform);
}

public abstract class OverlayBaseSeries : OverlayBasePresenting
{
    public static readonly DirectProperty<OverlayBaseSeries, Point[]?> PointsProperty =
        AvaloniaProperty.RegisterDirect<OverlayBaseSeries, Point[]?>
            (nameof(Points), o => o.Points, (o, v) => o.Points = v);


    private Point[]? points;

    public Point[]? Points
    {
        get => this.points;
        set
        {
            if (this.SetAndRaise(PointsProperty, ref this.points, value))
                this.NotifyReRender();
        }
    }
}
