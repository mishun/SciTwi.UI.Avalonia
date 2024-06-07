using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Metadata;
using ReactiveUI;

namespace SciTwi.UI.Controls.Plotting
{
    public class OverlayDictionary<K, V> : OverlayBase
    {
        public class ElementContext : ReactiveObject
        {
            private V? value;
            private object? parent;

            private K Key { get; }

            public V? Value
            {
                get => this.value;
                set => this.RaiseAndSetIfChanged(ref this.value, value);
            }

            public object? Parent
            {
                get => this.parent;
                set => this.RaiseAndSetIfChanged(ref this.parent, value);
            }

            public ElementContext(K key)
            {
                this.Key = key;
            }
        }

        protected struct Cell
        {
            public K Key { get; set; }
            public ElementContext Context { get; set; }
            public OverlayBase? Overlay { get; set; }
        }


        public static readonly DirectProperty<OverlayDictionary<K, V>, IDictionary<K, V>?> DictionaryProperty =
            AvaloniaProperty.RegisterDirect<OverlayDictionary<K, V>, IDictionary<K, V>?>
                (nameof(Dictionary), o => o.Dictionary, (o, v) => o.Dictionary = v);


        public OverlayDictionary()
        {
            this.OverlayTemplates.WeakSubscribe(this.TemplatesChanged);
        }


        protected Dictionary<K, Cell> cells = [];
        private IDictionary<K, V>? dictionary;
        private IDisposable? subscription;


        protected void UpdateLayers<L, T>(Action<L> def, Action<L, T> update, IEnumerable<(K, T)> values)
        {
            if (values is null || this.cells is null)
                return;

            var ups = new Dictionary<K, T>();
            foreach (var (descr, value) in values)
                ups.TryAdd(descr, value);

            foreach (var kv in this.cells)
                if (kv.Value.Overlay is L layer)
                {
                    if (ups.TryGetValue(kv.Key, out var value))
                        update(layer, value);
                    else
                        def(layer);
                }

            this.NotifyReRender();
        }


        [Content]
        public OverlayTemplates OverlayTemplates { get; } = new OverlayTemplates();


        public IDictionary<K, V>? Dictionary
        {
            get => this.dictionary;
            set
            {
                if (this.SetAndRaise(DictionaryProperty, ref this.dictionary, value))
                {
                    this.subscription?.Dispose();
                    this.subscription = null;
                    if (value is INotifyCollectionChanged incc)
                        this.subscription = incc.WeakSubscribe(DictionaryChanged);
                    this.RegenerateLayers(value);
                }
            }
        }


        internal sealed override void RenderPass(DrawingContext context, Rect bounds, Matrix parentTransform)
        {
            var cells = this.cells;
            if (cells is null || !this.IsVisible)
                return;

            var transform = this.Transform * parentTransform;
            foreach (var kv in cells)
            {
                var overlay = kv.Value.Overlay;
                if (overlay is null)
                    continue;
                overlay.RenderPass(context, bounds, transform);
            }
        }



        protected virtual ElementContext MakeElementContext(K key, V value)
        {
            return new ElementContext(key) { Value = value, Parent = this.DataContext };
        }


        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (this.cells is not null)
                foreach (var kv in this.cells)
                    kv.Value.Context.Parent = this.DataContext;
        }

        private void TemplatesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.RegenerateLayers(this.dictionary);
            this.NotifyReRender();
        }

        private void DictionaryChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (KeyValuePair<K, V> kv in e.NewItems)
                    {
                        var context = this.MakeElementContext(kv.Key, kv.Value);
                        var overlay = kv.Value is null ? null : this.OverlayTemplates.Build(kv.Value);
                        this.cells.Add(kv.Key, new Cell { Key = kv.Key, Context = context, Overlay = overlay });
                        if (!(overlay is null))
                        {
                            overlay.DataContext = context;
                            this.LogicalChildren.Add(overlay);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (KeyValuePair<K, V> kv in e.OldItems)
                    {
                        var overlay = this.cells?[kv.Key].Overlay;
                        if (overlay is not null)
                            this.LogicalChildren.Remove(overlay);
                        if (this.cells is not null)
                            this.cells.Remove(kv.Key);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    for (var i = 0; i < e.OldItems.Count; ++i)
                    {
                        var src = (KeyValuePair<K, V>)e.OldItems[i];
                        var dst = (KeyValuePair<K, V>)e.NewItems[i];
                        this.cells[src.Key].Context.Value = dst.Value;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    this.RegenerateLayers(this.dictionary);
                    break;
            }
            this.NotifyReRender();
        }

        private void RegenerateLayers(IDictionary<K, V>? data)
        {
            var old = this.cells;
            this.cells = new Dictionary<K, Cell>();
            this.LogicalChildren.Clear();

            if (data is null || this.OverlayTemplates is null)
                return;

            foreach (var kv in data)
            {
                var cell = default(Cell);
                old?.TryGetValue(kv.Key, out cell);

                cell.Key = kv.Key;
                if (cell.Context is null)
                    cell.Context = this.MakeElementContext(kv.Key, kv.Value);
                else
                    cell.Context.Value = kv.Value;

                if (cell.Overlay is null && kv.Value is not null)
                    cell.Overlay = this.OverlayTemplates.Build(kv.Value);

                this.cells.Add(kv.Key, cell);
                if (cell.Overlay is not null)
                {
                    cell.Overlay.DataContext = cell.Context;
                    this.LogicalChildren.Add(cell.Overlay);
                }
            }
        }
    }
}
