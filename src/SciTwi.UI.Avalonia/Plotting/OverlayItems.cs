using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Metadata;

namespace SciTwi.UI.Controls.Plotting
{
    public sealed class OverlayItems : OverlayBase
    {
        public static readonly DirectProperty<OverlayItems, IEnumerable> ItemsProperty =
            AvaloniaProperty.RegisterDirect<OverlayItems, IEnumerable>
                (nameof(Items), o => o.Items, (o, v) => o.Items = v);


        public OverlayItems()
        {
            this.OverlayTemplates.WeakSubscribe(this.TemplatesChanged);
        }


        public OverlayTemplates OverlayTemplates { get; } = new OverlayTemplates();


        private IEnumerable items;
        private IDisposable itemsSubscription;
        private readonly List<OverlayBase> layers = new();

        [Content]
        public IEnumerable Items
        {
            get => this.items;
            set
            {
                if (SetAndRaise(ItemsProperty, ref this.items, value))
                {
                    this.itemsSubscription?.Dispose();
                    this.itemsSubscription = null;
                    if (value is INotifyCollectionChanged incc)
                        this.itemsSubscription = incc.WeakSubscribe(ItemsCollectionChanged);
                    this.RegenerateLayers(value);
                    this.NotifyReRender();
                }
            }
        }


        internal override void RenderPass(DrawingContext context, Rect bounds, Matrix parentTransform)
        {
            var template = this.OverlayTemplates;
            if (template is null)
                return;

            foreach (var layer in this.layers)
                if (layer is OverlayBase)
                    layer.RenderPass(context, bounds, parentTransform);
        }


        private void TemplatesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RegenerateLayers(items);
            this.NotifyReRender();
        }

        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RegenerateLayers(this.items);
            this.NotifyReRender();
        }

        private void RegenerateLayers(IEnumerable items)
        {
            this.layers.Clear();

            if (items is not null)
            {
                foreach (var item in items)
                    if (item is OverlayBase layer)
                        this.layers.Add(layer);
                    else
                    {
                        var built = this.OverlayTemplates.Build(item);
                        if (built is not null)
                            built.DataContext = item;
                        this.layers.Add(built);
                    }
            }

            this.LogicalChildren.Clear();
            foreach (var layer in this.layers)
                if (layer is not null)
                    this.LogicalChildren.Add(layer);
        }
    }
}
