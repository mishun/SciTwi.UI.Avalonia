using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Media;
using SciTwi.UI.Rendering;


namespace SciTwi.UI.Controls.Plotting;


internal static class ScaleGrid
{
    public static void RenderScaleRuler(DrawingContext context, ScaleGridDims dim, SciTwi.UI.Rendering.Plotting.ScaleGrid.RulerPens pens, Typeface typeface)
    {
        var rulerT = new Matrix(dim.PixelStep.X, 0.0, 0.0, 6.0, 10.0, 10.0);
    }

//     for r in rulers.[dim.Subdivision] do
//         context.FillRectangle (pens.RulerBrush, r.TransformToAABB rulerT)
//     context.DrawRectangle (pens.RulerPen, Rect(0.0, 0.0, 1.0, 1.0).TransformToAABB rulerT)

//     let placeLabel text (p : Point) =
//         let ft = FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12.0, pens.RulerBrush) // (Text = text, Typeface = typeface, FontSize = 12.0)
//         context.DrawText(ft, p.Transform rulerT - Point (0.5 * ft.Width, 0.0))

//     Point (0.0, 1.0) |> placeLabel "0"
//     Point (1.0, 1.0) |> placeLabel (dim.CoarseStepX.ToString ())

}