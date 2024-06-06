namespace SciTwi.UI.Rendering

open System
open Avalonia


[<RequireQualifiedAccess>]
type LayeredGeometry =
    | Line    of A : float * B : float * C : float
    | Chain   of Points : Point[]
    | Ellipse of Matrix : Matrix
