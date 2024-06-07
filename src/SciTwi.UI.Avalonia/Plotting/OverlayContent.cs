using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Metadata;

namespace SciTwi.UI.Controls.Plotting
{
    public sealed class OverlayContent : OverlayBase
    {
        public static readonly DirectProperty<OverlayContent, object?> ContentProperty =
            AvaloniaProperty.RegisterDirect<OverlayContent, object?>
                (nameof(Content), o => o.Content, (o, v) => o.Content = v);

        private OverlayTemplates? _overlayTemplates;
        public OverlayTemplates OverlayTemplates => _overlayTemplates ??= new OverlayTemplates();


        private object? content;

        [Content]
        public object? Content
        {
            get => this.content;
            set
            {
                if (value == this.content)
                    return;

                if (value is null)
                {
                    this.content = null;
                    this.LogicChild = null;
                }
                else
                {
                    if (this.content is null || !this.content.GetType().IsInstanceOfType(value))
                        this.LogicChild = this.OverlayTemplates.Build(value);
                    this.content = value;
                    if (this.LogicChild is not null)
                        this.LogicChild.DataContext = value;
                }
            }
        }


        private OverlayBase? _logicChild;
        internal OverlayBase? LogicChild
        {
            get => this._logicChild;
            set
            {
                if (value == this._logicChild)
                    return;

                this._logicChild = value;
                this.LogicalChildren.Clear();
                if (value is not null)
                    this.LogicalChildren.Add(value);
            }
        }

        internal override void RenderPass(DrawingContext context, Rect bound, Matrix parentTransform)
        {
            if (!this.IsVisible || this.LogicChild is null)
                return;

            var transform = this.Transform * parentTransform;
            this.LogicChild.RenderPass(context, bound, transform);
        }
    }
}
