using System;
using System.Runtime.CompilerServices;
using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI;
using Avalonia;
using Avalonia.Controls;

namespace SciTwi.UI
{
    public static class WindowUtil
    {
        static WindowUtil()
        {
            CloseInteractionProperty.Changed.Subscribe(CloseInteractionChanged);
            HideWindowOnCloseProperty.Changed.Subscribe(HideWindowOnCloseChanged);
        }


        public static readonly AttachedProperty< Interaction<Unit, Unit> > CloseInteractionProperty =
            AvaloniaProperty.RegisterAttached<Window, Interaction<Unit, Unit>>("CloseInteraction", typeof(WindowUtil));

        public static Interaction<Unit, Unit> GetCloseInteraction(Window element) =>
            element.GetValue(CloseInteractionProperty);

        public static void SetCloseInteraction(Window element, Interaction<Unit, Unit> value) =>
            element.SetValue(CloseInteractionProperty, value);

        private static readonly ConditionalWeakTable<Window, SerialDisposable> closeInteractionHandlerCache = new();
        private static void CloseInteractionChanged(AvaloniaPropertyChangedEventArgs<Interaction<Unit, Unit>> args)
        {
            if (args.Sender is Window window)
            {
                var interaction = args.NewValue.GetValueOrDefault();
                var handler = closeInteractionHandlerCache.GetOrCreateValue(window);
                handler.Disposable =
                    interaction?.RegisterHandler(ctx => {
                        try
                        {
                            window.Close();
                        }
                        finally
                        {
                            ctx.SetOutput(Unit.Default);
                        }
                    });
            }
        }


        public static readonly AttachedProperty<bool> HideWindowOnCloseProperty =
            AvaloniaProperty.RegisterAttached<Window, bool>("HideWindowOnClose", typeof(WindowUtil), false);

        public static bool GetHideWindowOnClose(Window element) =>
            element.GetValue(HideWindowOnCloseProperty);

        public static void SetHideWindowOnClose(Window element, bool value) =>
            element.SetValue(HideWindowOnCloseProperty, value);

        private static void HideWindowOnCloseHandler(object sender, System.ComponentModel.CancelEventArgs args)
        {
            if (sender is Window window)
            {
                window.Hide();
                args.Cancel = true;
            }
        }

        private static void HideWindowOnCloseChanged(AvaloniaPropertyChangedEventArgs<bool> args)
        {
            if (args.Sender is Window window)
            {
                if (args.NewValue.GetValueOrDefault())
                    window.Closing += HideWindowOnCloseHandler;
                else
                    window.Closing -= HideWindowOnCloseHandler;
            }
        }
    }
}
