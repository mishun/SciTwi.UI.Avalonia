using System;
using System.Collections.Specialized;
using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Metadata;

namespace SciTwi.UI.Controls.Plotting
{
    public class OverlayGroup : OverlayBase
    {
        public OverlayGroup()
        {
            this.Layers.CollectionChanged += LayersChanged;
        }

        [Content]
        public AvaloniaList<OverlayBase> Layers { get; } =
            new AvaloniaList<OverlayBase>();

        private void LayersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var newItems = e.NewItems.OfType<OverlayBase>().ToList();
                        LogicalChildren.InsertRange(e.NewStartingIndex, newItems);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    LogicalChildren.MoveRange(e.OldStartingIndex, e.OldItems.Count, e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        var deletedItems = e.OldItems.OfType<OverlayBase>().ToList();
                        LogicalChildren.RemoveAll(deletedItems);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.OldItems.Count; ++i)
                    {
                        var index = i + e.OldStartingIndex;
                        var item = (OverlayBase)e.NewItems[i];
                        LogicalChildren[index] = item;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    throw new NotSupportedException();
            }
            this.NotifyReRender();
        }

        internal override void RenderPass(DrawingContext context, Rect bound, Matrix parentTransform)
        {
            if (!this.IsVisible || this.Layers is null || this.Layers.Count < 1)
                return;

            var transform = this.Transform * parentTransform;
            foreach (var layer in this.Layers)
                layer.RenderPass(context, bound, transform);
        }
    }
}
