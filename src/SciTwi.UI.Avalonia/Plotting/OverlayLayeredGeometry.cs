using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;
using SciTwi.UI.Rendering;

namespace SciTwi.UI.Controls.Plotting
{
    public sealed class OverlayLayeredGeometry : OverlayBasePresenting
    {
        public static readonly DirectProperty<OverlayLayeredGeometry, IEnumerable<LayeredGeometry>> GeometryProperty =
            AvaloniaProperty.RegisterDirect<OverlayLayeredGeometry, IEnumerable<LayeredGeometry>>
                (nameof(Geometry), o => o.Geometry, (o, v) => o.Geometry = v);


        private IEnumerable<LayeredGeometry> geometry;

        [Content]
        public IEnumerable<LayeredGeometry> Geometry
        {
            get => this.geometry;
            set
            {
                if (this.SetAndRaise(GeometryProperty, ref this.geometry, value))
                    this.NotifyReRender();
            }
        }

        internal override void RenderContent(DrawingContext context, Rect bounds, Matrix matrix)
        {
            var geometry = this.Geometry;
            if (geometry is null)
                return;

            foreach (var g in geometry)
            {
                if (g is null)
                    continue;

                Rendering.Plotting.Geometry.render(context, bounds, matrix, this.Fill, this.Stroke, g);
            }
        }
    }
}
