namespace SciTwi.UI.Rendering

open System
open Avalonia
open Avalonia.Media


[<Struct>]
type ScaleGridDims = {
    WidthInSteps : float
    HeightInSteps : float
    CoarseStepX : float
    PixelStep : Point
    PixelBase : Point
    Subdivision : int
}


type CanvasTransform (scale0) =
    inherit MatrixTransform ()

    let invalidate = Event<_, _> ()
    let minScale = 1e-3
    let maxScale = 1e4
    let mutable scale = scale0
    let mutable center = Point (0.0, 0.0)


    interface IAffectsRender with
        [<CLIEvent>]
        member __.Invalidated = invalidate.Publish


    member __.GetMatrix (bounds : Rect) =
        Matrix (
            scale, 0.0, 0.0, -scale,
            (0.5 * bounds.Width - center.X * scale), (0.5 * bounds.Height + center.Y * scale)
        )

    member this.UpdateMatrix (bounds : Rect) =
        let newMatrix = this.GetMatrix bounds
        if newMatrix <> base.Matrix then
            base.Matrix <- newMatrix
            invalidate.Trigger (this, EventArgs.Empty)

    member t.Drag (delta : Point, bounds : Rect) =
        center <- center - Point (delta.X, -delta.Y) * (1.0 / scale)
        t.UpdateMatrix bounds

    member t.ZoomAt (point : Point, zoom : float, bounds : Rect) =
        let nextScale = scale * zoom |> min maxScale |> max minScale
        let offset = point - bounds.Center
        center <- center + Point (offset.X, -offset.Y) * (1.0 / scale - 1.0 / nextScale)
        scale <- nextScale
        t.UpdateMatrix bounds


    member this.GridStep (targetPixelSpacing, bounds : Rect) =
        let log = log10 (targetPixelSpacing / scale)
        let stepX = Math.Pow (10.0, floor log)
        let m = this.GetMatrix bounds
        let factorX = stepX * m.M11
        let factorY = stepX * m.M22
        {   WidthInSteps = ceil (10.0 * bounds.Width / targetPixelSpacing)
            HeightInSteps = ceil (10.0 * bounds.Height / targetPixelSpacing)
            CoarseStepX = stepX
            PixelStep = Point (factorX, factorY)
            PixelBase = Point (m.M31 + factorX * floor (-m.M31 / factorX), m.M32 + factorY * ceil (-m.M32 / factorY))
            Subdivision =
                match log - floor log with
                | l when l > log10 5.0 -> 0
                | l when l > log10 2.0 -> 1
                | _                    -> 2
        }
