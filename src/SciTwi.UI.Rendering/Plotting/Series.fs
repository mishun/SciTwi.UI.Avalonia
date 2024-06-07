module SciTwi.UI.Rendering.Plotting.Series

open System
open Avalonia
open Avalonia.Media
open Avalonia.Platform
open Avalonia.Rendering.SceneGraph
open Avalonia.Skia
open SkiaSharp


let render (canvas : SKCanvas, bounds : Rect, transform : Matrix, color : Color, points : Point[]) =
    canvas.Save() |> ignore
    canvas.ClipRect (bounds.ToSKRect ())

    do
        let marker = SKRect (-2.5f, -2.5f, 2.5f, 2.5f)
        use paint = new SKPaint (Color = color.ToSKColor ())
        for p in points do
            let p' = p.Transform transform
            let save = canvas.TotalMatrix
            canvas.Translate (single p'.X, single p'.Y)
            canvas.DrawRect (marker, paint)
            canvas.SetMatrix save

    canvas.Restore()


type SkiaSeriesRenderOp (bounds : Rect, transform : Matrix, color : Color, points : Point[]) =
    interface IDisposable with
        member __.Dispose () = ()

    interface IEquatable<ICustomDrawOperation> with
        member this.Equals that =
            obj.ReferenceEquals(this, that)

    interface ICustomDrawOperation with
        member __.Bounds = bounds

        member __.HitTest _ = false

        member __.Render context =
            match context.TryGetFeature<ISkiaSharpApiLeaseFeature>() with
            | null -> ()
            | leaseFeature ->
                use lease = leaseFeature.Lease()
                render(lease.SkCanvas, bounds, transform, color, points)
