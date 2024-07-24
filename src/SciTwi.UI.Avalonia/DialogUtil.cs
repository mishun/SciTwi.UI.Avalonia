using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Input.Platform;

namespace SciTwi.UI;


public static class DialogUtil
{
    private static readonly ConditionalWeakTable<Visual, ConcurrentDictionary<string, SerialDisposable>> disposablesCache = [];

    private static void HandleInteractionPropertyWith<Arg, Res>(AttachedProperty< Interaction<Arg, Res?> > property, Func<Visual, Arg, Task<Res?>> handler)
    {
        property.Changed.Subscribe(args => {
            if (args.Sender is Visual owner)
            {
                var handlerDisposable = disposablesCache.GetOrCreateValue(owner).GetOrAdd(property.Name, _ => new SerialDisposable());
                handlerDisposable.Disposable =
                    args.NewValue.GetValueOrDefault()?.RegisterHandler(async ctx => {
                        Res? result = default;
                        try
                        {
                            result = await handler(owner, ctx.Input);
                        }
                        catch(Exception)
                        {
                        }
                        finally
                        {
                            ctx.SetOutput(result);
                        }
                    });
            }
        });
    }


    static DialogUtil()
    {
        HandleInteractionPropertyWith(OpenFileInteractionProperty, HandleOpenFileInteraction);
        HandleInteractionPropertyWith(SaveFileInteractionProperty, HandleSaveFileInteraction);
        HandleInteractionPropertyWith(ClipboardSetTextInteractionProperty, HandleSetClipboardTextInteraction);
        HandleInteractionPropertyWith(LaunchStorageItemInteractionProperty, HandleLaunchStorageItemInteraction);
        HandleInteractionPropertyWith(LaunchFileInfoInteractionProperty, HandleLaunchFileInfoInteraction);
    }


    public static readonly AttachedProperty< Interaction<FilePickerOpenOptions, IReadOnlyList<IStorageFile>?> > OpenFileInteractionProperty =
        AvaloniaProperty.RegisterAttached<Visual, Interaction<FilePickerOpenOptions, IReadOnlyList<IStorageFile>?>>("OpenFileInteraction", typeof(DialogUtil));

    public static Interaction<FilePickerOpenOptions, IReadOnlyList<IStorageFile>?> GetOpenFileInteraction(Visual element) =>
        element.GetValue(OpenFileInteractionProperty);

    public static void SetOpenFileInteraction(Visual element, Interaction<FilePickerOpenOptions, IReadOnlyList<IStorageFile>?> value) =>
        element.SetValue(OpenFileInteractionProperty, value);

    private static async Task<IReadOnlyList<IStorageFile>?> HandleOpenFileInteraction(Visual owner, FilePickerOpenOptions options)
    {
        if (TopLevel.GetTopLevel(owner) is TopLevel topLevel && topLevel.StorageProvider.CanOpen)
            return await topLevel.StorageProvider.OpenFilePickerAsync(options);
        return null;
    }


    public static readonly AttachedProperty< Interaction<FilePickerSaveOptions, IStorageFile?> > SaveFileInteractionProperty =
        AvaloniaProperty.RegisterAttached<Visual, Interaction<FilePickerSaveOptions, IStorageFile?>>("SaveFileInteraction", typeof(DialogUtil));

    public static Interaction<FilePickerSaveOptions, IStorageFile?> GetSaveFileInteraction(Visual element) =>
        element.GetValue(SaveFileInteractionProperty);

    public static void SetSaveFileInteraction(Visual element, Interaction<FilePickerSaveOptions, IStorageFile?> value) =>
        element.SetValue(SaveFileInteractionProperty, value);

    private static async Task<IStorageFile?> HandleSaveFileInteraction(Visual owner, FilePickerSaveOptions options)
    {
        if (TopLevel.GetTopLevel(owner) is TopLevel topLevel && topLevel.StorageProvider.CanSave)
            return await topLevel.StorageProvider.SaveFilePickerAsync(options);
        return null;
    }


    public static readonly AttachedProperty< Interaction<string, Unit> > ClipboardSetTextInteractionProperty =
        AvaloniaProperty.RegisterAttached<Visual, Interaction<string, Unit>>("ClipboardSetTextInteraction", typeof(DialogUtil));

    public static Interaction<string, Unit> GetClipboardSetTextInteraction(Visual element) =>
        element.GetValue(ClipboardSetTextInteractionProperty);

    public static void SetClipboardSetTextInteraction(Visual element, Interaction<string, Unit> value) =>
        element.SetValue(ClipboardSetTextInteractionProperty, value);

    private static async Task<Unit> HandleSetClipboardTextInteraction(Visual owner, string text)
    {
        if (TopLevel.GetTopLevel(owner) is TopLevel topLevel && topLevel.Clipboard is IClipboard clipboard)
            await clipboard.SetTextAsync(text);
        return Unit.Default;
    }


    public static readonly AttachedProperty< Interaction<IStorageItem, bool> > LaunchStorageItemInteractionProperty =
        AvaloniaProperty.RegisterAttached<Visual, Interaction<IStorageItem, bool>>("LaunchStorageItemInteraction", typeof(DialogUtil));

    private static async Task<bool> HandleLaunchStorageItemInteraction(Visual owner, IStorageItem storageItem)
    {
        if (TopLevel.GetTopLevel(owner) is TopLevel topLevel)
            return await topLevel.Launcher.LaunchFileAsync(storageItem);
        return false;
    }

    public static readonly AttachedProperty< Interaction<FileInfo, bool> > LaunchFileInfoInteractionProperty =
        AvaloniaProperty.RegisterAttached<Visual, Interaction<FileInfo, bool>>("LaunchFileInfoInteraction", typeof(DialogUtil));

    private static async Task<bool> HandleLaunchFileInfoInteraction(Visual owner, FileInfo fileInfo)
    {
        if (TopLevel.GetTopLevel(owner) is TopLevel topLevel)
            return await topLevel.Launcher.LaunchFileInfoAsync(fileInfo);
        return false;
    }
}
