using System;
using Avalonia;
using Avalonia.Media;
using SciTwi.UI.Rendering;

namespace SciTwi.UI.Controls.Plotting
{
    public class OverlaySeries : OverlayBaseSeries
    {
        internal override void RenderContent(DrawingContext context, Rect bounds, Matrix transform)
        {
            var points = this.Points;
            if (points is null || points.Length < 1)
                return;

            if (this.Fill is SolidColorBrush brush)
                context.Custom(new Rendering.Plotting.Series.SkiaSeriesRenderOp(bounds, transform, brush.Color, this.Points));
        }
    }
}
