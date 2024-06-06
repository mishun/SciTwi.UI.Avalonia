using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Reactive.Disposables;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace SciTwi.UI
{
    public static class DialogUtil
    {
        static DialogUtil()
        {
            SaveFileInteractionProperty.Changed.Subscribe(SaveFileInteractionChanged);
        }


        public struct SaveParams
        {
            public string FileName;
            public string Extension;
            public string Hint;
        }

        public static AttachedProperty< Interaction<SaveParams, string> > SaveFileInteractionProperty =
            AvaloniaProperty.RegisterAttached<Visual, Interaction<SaveParams, string>>("SaveFileInteraction", typeof(DialogUtil));

        public static Interaction<SaveParams, string> GetSaveFileInteraction(Visual element) =>
            element.GetValue(SaveFileInteractionProperty);

        public static void SetSaveFileInteraction(Visual element, Interaction<SaveParams, string> value) =>
            element.SetValue(SaveFileInteractionProperty, value);

        private static readonly ConditionalWeakTable<Window, SaveFileDialog> saveFileDialogCache = new();
        private static readonly ConditionalWeakTable<IVisual, SerialDisposable> saveFileDialogHandlerCache = new();
        private static void SaveFileInteractionChanged(AvaloniaPropertyChangedEventArgs< Interaction<SaveParams, string> > args)
        {
            if (args.Sender is IVisual visual)
            {
                var interaction = args.NewValue.GetValueOrDefault();
                var handler = saveFileDialogHandlerCache.GetOrCreateValue(visual);
                handler.Disposable =
                    interaction?.RegisterHandler(async ctx => {
                        string path = null;
                        try
                        {
                            if (visual.VisualRoot is Window window)
                            {
                                var dialog = saveFileDialogCache.GetOrCreateValue(window);
                                dialog.DefaultExtension = ctx.Input.Extension;
                                dialog.InitialFileName = ctx.Input.FileName;
                                dialog.Filters = new List<FileDialogFilter> {
                                    new FileDialogFilter { Name = ctx.Input.Hint, Extensions = new List<string> { ctx.Input.Extension } }
                                };
                                path = await dialog.ShowAsync(window);
                            }
                        }
                        finally
                        {
                            ctx.SetOutput(path);
                        }
                    });
            }
        }
    }
}
