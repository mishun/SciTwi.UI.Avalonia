using System;
using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;

namespace SciTwi.UI.Controls.Plotting;

public class OverlayGroup : OverlayBase
{
    public OverlayGroup()
    {
        this.Layers.CollectionChanged += LayersChanged;
    }

    [Content]
    public AvaloniaList<OverlayBase> Layers { get; } =
        new AvaloniaList<OverlayBase>();

    private void LayersChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                {
                    if(e.NewItems is IList newItems)
                        LogicalChildren.InsertRange(e.NewStartingIndex, newItems.OfType<OverlayBase>().ToList());
                }
                break;

            case NotifyCollectionChangedAction.Move:
                {
                    if(e.OldItems is IList old)
                        LogicalChildren.MoveRange(e.OldStartingIndex, old.Count, e.NewStartingIndex);
                }
                break;

            case NotifyCollectionChangedAction.Remove:
                {
                    if(e.OldItems is IList old)
                    {
                        var deletedItems = old.OfType<OverlayBase>().ToList();
                        LogicalChildren.RemoveAll(deletedItems);
                    }
                }
                break;

            case NotifyCollectionChangedAction.Replace:
                {
                    if(e.OldItems is IList oldItems && e.NewItems is IList newItems && oldItems.Count == newItems.Count)
                    {
                        for(int i = 0; i < newItems.Count; i++)
                            if(e.NewItems[i] is ILogical element)
                                LogicalChildren[i + e.OldStartingIndex] = element;
                    }
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
