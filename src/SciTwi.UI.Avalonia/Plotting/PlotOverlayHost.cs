using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;


namespace SciTwi.UI.Controls.Plotting;


internal delegate void PanningStatus(bool isPanning);

internal class PanState
{
    private Point? mouseTrack = null;

    public event PanningStatus? Panning;

    public void Start(Point origin)
    {
        if(this.mouseTrack is null)
        {
            this.mouseTrack = origin;
            this.Panning?.Invoke(true);
        }
    }

    public void Stop()
    {
        if(this.mouseTrack is Point)
        {
            this.mouseTrack = null;
            this.Panning?.Invoke(false);
        }
    }

    public Point? TryMove(Point curr)
    {
        if(this.mouseTrack is Point prev)
        {
            var delta = curr - prev;
            this.mouseTrack = curr;
            return delta;
        }
        else
            return null;
    }
}


public class PlotOverlayHost : Control
{
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<PlotOverlayHost>();

    public IBrush? Background
    {
        get => base.GetValue(BackgroundProperty);
        set => base.SetValue(BackgroundProperty, value);
    }


    public static readonly StyledProperty<IBrush?> ScaleRulerBrushProperty =
        AvaloniaProperty.Register<PlotOverlayHost, IBrush?>(nameof(ScaleRulerBrush));

    public IBrush? ScaleRulerBrush
    {
        get => base.GetValue(ScaleRulerBrushProperty);
        set => base.SetValue(ScaleRulerBrushProperty, value);
    }


    public static readonly StyledProperty<IPen?> ScaleGridFinePenProperty =
        AvaloniaProperty.Register<PlotOverlayHost, IPen?>(nameof(ScaleGridFinePen));

    public IPen? ScaleGridFinePen
    {
        get => base.GetValue(ScaleGridFinePenProperty);
        set => base.SetValue(ScaleGridFinePenProperty, value);
    }


    public static readonly StyledProperty<IPen?> ScaleGridCoarsePenProperty =
        AvaloniaProperty.Register<PlotOverlayHost, IPen?>(nameof(ScaleGridCoarsePen));

    public IPen? ScaleGridCoarsePen
    {
        get => base.GetValue(ScaleGridCoarsePenProperty);
        set => base.SetValue(ScaleGridCoarsePenProperty, value);
    }


    public static readonly StyledProperty<IPen?> ScaleGridZeroPenProperty =
        AvaloniaProperty.Register<PlotOverlayHost, IPen?>(nameof(ScaleGridZeroPen));

    public IPen? ScaleGridZeroPen
    {
        get => base.GetValue(ScaleGridZeroPenProperty);
        set => base.SetValue(ScaleGridZeroPenProperty, value);
    }


    public static readonly StyledProperty<IPen?> MouseRulerStrokeProperty =
        AvaloniaProperty.Register<PlotOverlayHost, IPen?>(nameof(MouseRulerStroke));

    public IPen? MouseRulerStroke
    {
        get => base.GetValue(MouseRulerStrokeProperty);
        set => base.SetValue(MouseRulerStrokeProperty, value);
    }


    public static readonly StyledProperty<OverlayBase> RootProperty =
        AvaloniaProperty.Register<PlotOverlayHost, OverlayBase>(nameof(Root));

    [Content]
    public OverlayBase Root
    {
        get => base.GetValue(RootProperty);
        set => base.SetValue(RootProperty, value);
    }


    public static readonly StyledProperty<double> TargetGridSpacingProperty =
        AvaloniaProperty.Register<PlotOverlayHost, double>(nameof(TargetGridSpacing), 400.0);

    public double TargetGridSpacing
    {
        get => base.GetValue(TargetGridSpacingProperty);
        set => base.SetValue(TargetGridSpacingProperty, value);
    }


    public static readonly StyledProperty<bool> ZoomEnabledProperty =
        AvaloniaProperty.Register<PlotOverlayHost, bool>(nameof(ZoomEnabled), true);

    public bool ZoomEnabled
    {
        get => base.GetValue(ZoomEnabledProperty);
        set => base.SetValue(ZoomEnabledProperty, value);
    }


    private static readonly Typeface typeface = new(FontFamily.Default); // TODO: something better?
    private readonly Cursor panningCursor = new(StandardCursorType.SizeAll);
    private readonly PanState panState = new();
    private (Point, Point)? mouseTrackRuler = null;
    private readonly CanvasTransform canvasTransform = new(1.0);


    static PlotOverlayHost()
    {
        AffectsRender<PlotOverlayHost>(BackgroundProperty, RootProperty, TargetGridSpacingProperty);

        RootProperty.Changed.AddClassHandler<PlotOverlayHost>((host, args) => {
            if(args.OldValue is ILogical prev)
                host?.LogicalChildren.Remove(prev);
            if(args.NewValue is ILogical next)
                host?.LogicalChildren.Add(next);
        });

        Visual.BoundsProperty.Changed.AddClassHandler<PlotOverlayHost>((host, args) => {
            if(args.NewValue is Rect bounds)
                host?.CanvasTransform.TryUpdateMatrix(bounds);
        });
    }

    public PlotOverlayHost()
    {
        base.ClipToBounds = true;
        this.Background = Brushes.Transparent;
        this.MouseRulerStroke = new Pen(Brushes.Black, 2, DashStyle.Dash);
        this.ScaleRulerBrush = new SolidColorBrush(Colors.Black);
        this.ScaleGridFinePen =  new Pen(new SolidColorBrush(Color.FromRgb(230, 230, 230)), 1.0);
        this.ScaleGridCoarsePen = new Pen(new SolidColorBrush(Color.FromRgb(210, 210, 210)), 1.0);
        this.ScaleGridZeroPen = new Pen(new SolidColorBrush(Colors.DarkSlateGray), 1.0);

        this.CanvasTransform.MatrixChanged += this.OnCanvasTransformMatrixChanged;
        this.panState.Panning += this.OnPanningStateChanged;
    }


    private void OnCanvasTransformMatrixChanged(Matrix matrix)
    {
        base.InvalidateVisual();
    }

    private void OnPanningStateChanged(bool panning)
    {
        this.Cursor = panning ? this.panningCursor : null;
    }


    public CanvasTransform CanvasTransform => this.canvasTransform;


    public override void Render(DrawingContext context)
    {
        var bounds = new Rect(0.0, 0.0, this.Bounds.Width, this.Bounds.Height);
        if(this.Background is IBrush background)
            context.FillRectangle(background, bounds);

        {
            var matrix = this.CanvasTransform.Matrix;
            var dims = CanvasTransform.GridStep(matrix, Math.Max(10.0, this.TargetGridSpacing), bounds);
            Rendering.ScaleGrid.renderScaleGrid(context, ref dims, matrix, this.ScaleGridFinePen, this.ScaleGridCoarsePen, this.ScaleGridZeroPen);
            this.Root?.RenderPass(context, bounds, matrix);

            {
                if(this.ScaleRulerBrush is IBrush rulerBrush)
                    Rendering.ScaleGrid.renderScaleRuler(context, ref dims, typeface, rulerBrush);
            }

            if(this.mouseTrackRuler is (var a, var b))
            {
                if(this.MouseRulerStroke is IPen pen)
                    context.DrawLine(pen, a.Transform(matrix), b.Transform(matrix));

                if(this.ScaleRulerBrush is IBrush rulerBrush)
                {
                    var diff = b - a;
                    var text = $"d = {double.Hypot(diff.X, diff.Y):N2}\nΔx = {diff.X:N2}\nΔy = {diff.Y:N2}";
                    var ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12.0, rulerBrush);
                    context.DrawText(ft, new Point(10, 35));
                }
            }
        }

        base.Render(context);
    }

    internal void NotifyRenderGraphChanged()
    {
        //Dispatcher.UIThread.Post(this.InvalidateVisual, DispatcherPriority.Background);
        this.InvalidateVisual();
    }


    void StartRuler(Point initial)
    {
        if(this.mouseTrackRuler is null)
        {
            var p = initial.Transform(this.CanvasTransform.Matrix.Invert());
            this.mouseTrackRuler = (p, p);
            this.InvalidateVisual();
        }
    }

    void TryMoveRuler(Point pos)
    {
        switch(this.mouseTrackRuler)
        {
            case (var begin, _):
                var end = pos.Transform(this.CanvasTransform.Matrix.Invert());
                this.mouseTrackRuler = (begin, end);
                this.InvalidateVisual();
                break;
        }
    }

    void StopRuler()
    {
        if(this.mouseTrackRuler.HasValue)
        {
            this.mouseTrackRuler = null;
            this.InvalidateVisual();
        }
    }


    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        this.Focus();
        var p = e.GetCurrentPoint(this);
        switch(p.Properties.PointerUpdateKind)
        {
            case PointerUpdateKind.LeftButtonPressed:
                this.panState.Start(p.Position);
                e.Handled = true;
                break;

            case PointerUpdateKind.RightButtonPressed:
                this.StartRuler(p.Position);
                e.Handled = true;
                break;
        }
        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        switch(e.InitialPressMouseButton)
        {
            case MouseButton.Left:
                this.panState.Stop();
                break;

            case MouseButton.Right:
                this.StopRuler();
                break;
        }
        base.OnPointerReleased(e);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if(object.ReferenceEquals(e.Pointer.Captured, this))
        {
            var p = e.GetCurrentPoint(this);
            switch(p.Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    this.panState.Start(p.Position);
                    break;
                case PointerUpdateKind.LeftButtonReleased:
                    this.panState.Stop();
                    break;
                case PointerUpdateKind.RightButtonPressed:
                    this.StartRuler(p.Position);
                    break;
                case PointerUpdateKind.RightButtonReleased:
                    this.StopRuler();
                    break;

                case PointerUpdateKind.Other:
                    this.TryMoveRuler(p.Position);
                    if (this.panState.TryMove(p.Position) is Point delta)
                        this.CanvasTransform.Drag(delta, this.Bounds);
                    break;
            }
        }

        base.OnPointerMoved(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        this.panState.Stop();
        this.StopRuler();
        base.OnPointerCaptureLost(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        if(this.ZoomEnabled)
        {
            var zoom = Math.Pow(1.2, 0.5 * e.Delta.Y);
            this.CanvasTransform.ZoomAt(e.GetPosition(this), zoom, this.Bounds);
            e.Handled = true;
        }
    }


    private bool TryConsumeKey(Key key)
    {
        var bounds = this.Bounds;
        switch(key)
        {
            case Key.PageUp: this.CanvasTransform.Drag(new Point(0.0, 0.8 * bounds.Height), bounds); break;
            case Key.Up: this.CanvasTransform.Drag(new Point(0.0, 0.2 * bounds.Height), bounds); break;
            case Key.PageDown: this.CanvasTransform.Drag(new Point(0.0, -0.8 * bounds.Height), bounds); break;
            case Key.Down: this.CanvasTransform.Drag(new Point(0.0, -0.2 * bounds.Height), bounds); break;
            case Key.Left: this.CanvasTransform.Drag(new Point(0.2 * bounds.Width, 0.0), bounds); break;
            case Key.Right: this.CanvasTransform.Drag(new Point(-0.2 * bounds.Width, 0.0), bounds); break;
            case Key.OemMinus: this.CanvasTransform.ZoomAt(bounds.Center, 0.8, bounds); break;
            case Key.OemPlus: this.CanvasTransform.ZoomAt(bounds.Center, 1.25, bounds); break;

            default:
                return false;
        }

        return true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        e.Handled = TryConsumeKey(e.Key);
        base.OnKeyDown(e);
    }
}
