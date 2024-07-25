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
