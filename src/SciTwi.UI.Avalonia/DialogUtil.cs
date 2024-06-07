using System;
using System.Runtime.CompilerServices;
using System.Reactive.Disposables;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
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

        public static readonly AttachedProperty< Interaction<SaveParams, IStorageFile?> > SaveFileInteractionProperty =
            AvaloniaProperty.RegisterAttached<Visual, Interaction<SaveParams, IStorageFile?>>("SaveFileInteraction", typeof(DialogUtil));

        public static Interaction<SaveParams, IStorageFile?> GetSaveFileInteraction(Visual element) =>
            element.GetValue(SaveFileInteractionProperty);

        public static void SetSaveFileInteraction(Visual element, Interaction<SaveParams, IStorageFile?> value) =>
            element.SetValue(SaveFileInteractionProperty, value);


        private static readonly ConditionalWeakTable<Visual, SerialDisposable> saveFilePickerHandlerCache = [];
        private static void SaveFileInteractionChanged(AvaloniaPropertyChangedEventArgs< Interaction<SaveParams, IStorageFile?> > args)
        {
            if (args.Sender is Visual visual)
            {
                var interaction = args.NewValue.GetValueOrDefault();
                var handler = saveFilePickerHandlerCache.GetOrCreateValue(visual);
                handler.Disposable = interaction?.RegisterHandler(ctx => HandleSaveFileInteraction(visual, ctx));
            }
        }

        private static async void HandleSaveFileInteraction(Visual owner, InteractionContext<SaveParams, IStorageFile?> ctx)
        {
            IStorageFile? storageFile = null;
            try
            {
                if(owner.GetVisualRoot() is TopLevel topLevel && topLevel.StorageProvider.CanSave)
                {
                    var options = new FilePickerSaveOptions() {
                        SuggestedFileName = ctx.Input.FileName,
                        DefaultExtension = ctx.Input.Extension,
                        FileTypeChoices = [ new FilePickerFileType(ctx.Input.Hint) { Patterns = [ $"*.{ctx.Input.Extension}" ] } ],
                        // SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
                    };
                    storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(options);
                }
            }
            finally
            {
                ctx.SetOutput(storageFile);
            }
        }
    }
}
