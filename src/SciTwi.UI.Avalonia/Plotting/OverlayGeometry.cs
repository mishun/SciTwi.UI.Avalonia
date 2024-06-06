using System;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Metadata;

namespace SciTwi.UI.Controls.Plotting
{
    public sealed class OverlayGeometry : OverlayBasePresenting
    {
        public static readonly StyledProperty<Geometry> GeometryProperty =
            AvaloniaProperty.Register<OverlayGeometry, Geometry>(nameof(Geometry));

        static OverlayGeometry()
        {
            GeometryProperty.Changed.AddClassHandler<OverlayGeometry>((g, _) => g.NotifyReRender());
        }

        [Content]
        public Geometry Geometry
        {
            get => GetValue(GeometryProperty);
            set => SetValue(GeometryProperty, value);
        }

        internal override void RenderContent(DrawingContext context, Rect bounds, Matrix matrix)
        {
            var geometry = this.Geometry;
            if (geometry is null)
                return;

            var save = geometry.Transform;
            geometry.Transform = (save is null) ? new MatrixTransform(matrix) : new MatrixTransform(save.Value * matrix);
            context.DrawGeometry(this.Fill, this.Stroke, geometry);
            geometry.Transform = save;
        }
    }

    public sealed class OverlayGeometryGroup : OverlayBasePresenting
    {
        [Content]
        public AvaloniaList<Geometry> Geometry { get; } = new AvaloniaList<Geometry>();

        internal override void RenderContent(DrawingContext context, Rect bounds, Matrix matrix)
        {
            if (this.Geometry is null || this.Geometry.Count < 1)
                return;

            var transform = new MatrixTransform(matrix);
            var stroke = this.Stroke;
            var fill = this.Fill;
            foreach (var geometry in this.Geometry)
            {
                if (geometry is null)
                    continue;

                var save = geometry.Transform;
                geometry.Transform = (save is null) ? transform : new MatrixTransform(save.Value * matrix);
                context.DrawGeometry(fill, stroke, geometry);
                geometry.Transform = save;
            }
        }
    }
}
