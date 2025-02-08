using System;
using System.Globalization;
using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;

namespace SciTwi.UI.Controls.Plotting;

public enum TextAnchor
{
    BottomLeft, BottomCenter, BottomRight,
    CenterLeft, CenterCenter, CenterRight,
    TopLeft, TopCenter, TopRight
}

public sealed class OverlayText : OverlayBasePresenting
{
    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<OverlayText, FontFamily>(nameof(FontFamily), defaultValue: FontFamily.Default);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<OverlayText, double>(nameof(FontSize), defaultValue: 12);

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        AvaloniaProperty.Register<OverlayText, FontStyle>(nameof(FontStyle), defaultValue: FontStyle.Normal);

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        AvaloniaProperty.Register<OverlayText, FontWeight>(nameof(FontWeight), defaultValue: FontWeight.Normal);

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<OverlayText, string>(nameof(Text));

    public static readonly StyledProperty<Point> ReferenceProperty =
        AvaloniaProperty.Register<OverlayText, Point>(nameof(Reference), defaultValue: new Point(0.0, 0.0));

    public static readonly StyledProperty<TextAnchor> AnchorProperty =
        AvaloniaProperty.Register<OverlayText, TextAnchor>(nameof(Anchor), defaultValue: TextAnchor.TopLeft);


    static OverlayText()
    {
        //Observable.Merge<AvaloniaPropertyChangedEventArgs>(FontStyleProperty.Changed, FontWeightProperty.Changed, FontFamilyProperty.Changed)
        //    .AddClassHandler<OverlayText>((x, _) => x.InvalidateTextLayout());
    }


    public FontFamily FontFamily
    {
        get => this.GetValue(FontFamilyProperty);
        set => this.SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => this.GetValue(FontSizeProperty);
        set => this.SetValue(FontSizeProperty, value);
    }

    public FontStyle FontStyle
    {
        get => this.GetValue(FontStyleProperty);
        set => this.SetValue(FontStyleProperty, value);
    }

    public FontWeight FontWeight
    {
        get => this.GetValue(FontWeightProperty);
        set => this.SetValue(FontWeightProperty, value);
    }

    [Content]
    public string Text
    {
        get => this.GetValue(TextProperty);
        set => this.SetValue(TextProperty, value);
    }

    public Point Reference
    {
        get => this.GetValue(ReferenceProperty);
        set => this.SetValue(ReferenceProperty, value);
    }

    public TextAnchor Anchor
    {
        get => this.GetValue(AnchorProperty);
        set => this.SetValue(AnchorProperty, value);
    }


    internal override void RenderContent(DrawingContext context, Rect bounds, Matrix matrix)
    {
        var text = this.Text;
        if (string.IsNullOrEmpty(text))
            return;

        var typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight);
        var ft = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, this.FontSize, this.Fill);

        var reference = this.Reference.Transform(matrix);
        var (dx, dy) = AnchorOffset(this.Anchor, ft);
        context.DrawText(ft, new Point(reference.X + dx, reference.Y + dy));
    }

    private static (double, double) AnchorOffset(TextAnchor anchor, FormattedText ft)
    {
        return anchor switch
        {
            TextAnchor.BottomLeft => (0.0, -ft.Height),
            TextAnchor.BottomCenter => (-0.5 * ft.Width, -ft.Height),
            TextAnchor.BottomRight => (-ft.Width, -ft.Height),
            TextAnchor.CenterLeft => (0.0, -0.5 * ft.Height),
            TextAnchor.CenterCenter => (-0.5 * ft.Width, -0.5 * ft.Height),
            TextAnchor.CenterRight => (-ft.Width, -0.5 * ft.Height),
            TextAnchor.TopLeft => (0.0, 0.0),
            TextAnchor.TopCenter => (-0.5 * ft.Width, 0.0),
            TextAnchor.TopRight => (-ft.Width, 0.0),
            _ => (0.0, 0.0),
        };
    }
}
