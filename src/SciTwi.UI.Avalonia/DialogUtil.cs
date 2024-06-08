using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace SciTwi.UI;


public static class DialogUtil
{
    static DialogUtil()
    {
        OpenFileInteractionProperty.Changed.Subscribe(OpenFileInteractionChanged);
        SaveFileInteractionProperty.Changed.Subscribe(SaveFileInteractionChanged);
        ClipboardTextInteractionProperty.Changed.Subscribe(ClipboardTextInteraction);
    }


    public static readonly AttachedProperty< Interaction<FilePickerOpenOptions, IStorageFile?> > OpenFileInteractionProperty =
        AvaloniaProperty.RegisterAttached<Visual, Interaction<FilePickerOpenOptions, IStorageFile?>>("OpenFileInteraction", typeof(DialogUtil));

    public static Interaction<FilePickerOpenOptions, IStorageFile?> GetOpenFileInteraction(Visual element) =>
        element.GetValue(OpenFileInteractionProperty);

    public static void SetOpenFileInteraction(Visual element, Interaction<FilePickerOpenOptions, IStorageFile?> value) =>
        element.SetValue(OpenFileInteractionProperty, value);

    private static readonly ConditionalWeakTable<Visual, SerialDisposable> openFilePickerHandlerCache = [];
    private static void OpenFileInteractionChanged(AvaloniaPropertyChangedEventArgs< Interaction<FilePickerOpenOptions, IStorageFile?> > args)
    {
        if (args.Sender is Visual owner)
        {
            var interaction = args.NewValue.GetValueOrDefault();
            var handler = openFilePickerHandlerCache.GetOrCreateValue(owner);
            handler.Disposable =
                interaction?.RegisterHandler(async ctx => {
                    IStorageFile? storageFile = null;
                    try
                    {
                        if (TopLevel.GetTopLevel(owner) is TopLevel topLevel && topLevel.StorageProvider.CanOpen)
                            storageFile = await topLevel.StorageProvider.OpenFilePickerAsync(ctx.Input);
                    }
                    finally
                    {
                        ctx.SetOutput(storageFile);
                    }
                });
        }
    }


    public static readonly AttachedProperty< Interaction<FilePickerSaveOptions, IStorageFile?> > SaveFileInteractionProperty =
        AvaloniaProperty.RegisterAttached<Visual, Interaction<FilePickerSaveOptions, IStorageFile?>>("SaveFileInteraction", typeof(DialogUtil));

    public static Interaction<FilePickerSaveOptions, IStorageFile?> GetSaveFileInteraction(Visual element) =>
        element.GetValue(SaveFileInteractionProperty);

    public static void SetSaveFileInteraction(Visual element, Interaction<FilePickerSaveOptions, IStorageFile?> value) =>
        element.SetValue(SaveFileInteractionProperty, value);

    private static readonly ConditionalWeakTable<Visual, SerialDisposable> saveFilePickerHandlerCache = [];
    private static void SaveFileInteractionChanged(AvaloniaPropertyChangedEventArgs< Interaction<FilePickerSaveOptions, IStorageFile?> > args)
    {
        if (args.Sender is Visual owner)
        {
            var interaction = args.NewValue.GetValueOrDefault();
            var handler = saveFilePickerHandlerCache.GetOrCreateValue(owner);
            handler.Disposable =
                interaction?.RegisterHandler(async ctx => {
                    IStorageFile? storageFile = null;
                    try
                    {
                        if (TopLevel.GetTopLevel(owner) is TopLevel topLevel && topLevel.StorageProvider.CanSave)
                            storageFile = await topLevel.StorageProvider.SaveFilePickerAsync(ctx.Input);
                    }
                    finally
                    {
                        ctx.SetOutput(storageFile);
                    }
                });
        }
    }


    public static readonly AttachedProperty< Interaction<string, Unit> > ClipboardTextInteractionProperty =
        AvaloniaProperty.RegisterAttached<Visual, Interaction<string, Unit>>("ClipboardTextInteraction", typeof(DialogUtil));

    public static Interaction<string, Unit> GetClipboardTextInteraction(Visual element) =>
        element.GetValue(ClipboardTextInteractionProperty);

    public static void SetClipboardTextInteraction(Visual element, Interaction<string, Unit> value) =>
        element.SetValue(ClipboardTextInteractionProperty, value);

    private static readonly ConditionalWeakTable<Visual, SerialDisposable> clipboardTextHandlerCache = [];
    private static void ClipboardTextInteraction(AvaloniaPropertyChangedEventArgs< Interaction<string, Unit> > args)
    {
        if (args.Sender is Visual owner)
        {
            var interaction = args.NewValue.GetValueOrDefault();
            var handler = clipboardTextHandlerCache.GetOrCreateValue(owner);
            handler.Disposable =
                interaction?.RegisterHandler(async ctx => {
                    if (TopLevel.GetTopLevel(owner) is TopLevel topLevel)
                        await topLevel.Clipboard.SetTextAsync(ctx.Input);
                    ctx.SetOutput(Unit.Default);
                });
        }
    }
}
