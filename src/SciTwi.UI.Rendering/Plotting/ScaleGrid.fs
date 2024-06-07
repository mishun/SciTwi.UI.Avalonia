module SciTwi.UI.Rendering.Plotting.ScaleGrid

open System
open System.Globalization
open Avalonia
open Avalonia.Media
open SciTwi.UI.Rendering


let private fineCheckpoints =
    [|  [| 0.5 |]
        [| 0.2 ; 0.4 ; 0.6 ; 0.8 |]
        [| 0.1 ; 0.2 ; 0.3 ; 0.4 ; 0.5 ; 0.6 ; 0.7 ; 0.8 ; 0.9 |]
    |]


type GridPens = {
    CoarsePen : Pen
    FinePen : Pen
    ZeroPen : Pen
}


let makeDefaultGridPens () = {
    CoarsePen = Pen (SolidColorBrush (Color.FromRgb (210uy, 210uy, 210uy)), 1.0)
    FinePen = Pen (SolidColorBrush (Color.FromRgb (230uy, 230uy, 230uy)), 1.0)
    ZeroPen = Pen (SolidColorBrush (Colors.DarkSlateGray), 1.0)
}


let renderScaleGrid (context : DrawingContext, dim : inref<ScaleGridDims>, m : Matrix, pens : GridPens) =
    let gridT = Matrix (dim.PixelStep.X, 0.0, 0.0, -dim.PixelStep.Y, dim.PixelBase.X, dim.PixelBase.Y)
    let subdivision = fineCheckpoints.[dim.Subdivision]
    let hatch (width, place : float -> struct (Point * Point)) =
        for position in 0.0 .. width do
            do
                let struct (p0, p1) = place position
                context.DrawLine (pens.CoarsePen, p0.Transform gridT, p1.Transform gridT)
            for dx in subdivision do
                let struct (p0, p1) = place (position + dx)
                context.DrawLine (pens.FinePen, p0.Transform gridT, p1.Transform gridT)

    let height = dim.HeightInSteps
    hatch (dim.WidthInSteps, fun x -> struct (Point (x, 0.0), Point (x, height)))
    let width = dim.WidthInSteps
    hatch (dim.HeightInSteps, fun y -> struct (Point (0.0, y), Point (width, y)))

    context.DrawLine (pens.ZeroPen, Point (m.M31, dim.PixelBase.Y), Point (m.M31, dim.PixelBase.Y - dim.PixelStep.Y * dim.HeightInSteps))
    context.DrawLine (pens.ZeroPen, Point (dim.PixelBase.X, m.M32), Point (dim.PixelBase.X + dim.PixelStep.X * dim.WidthInSteps, m.M32))


let private rulers =
    [|  for n in [| 2 ; 5 ; 10 |] ->
            [|  for i in 0 .. n - 1 ->
                    Rect (float i / float n, (if i % 2 = 0 then 0.0 else 0.5), 1.0 / float n, 0.5)
            |]
    |]


type RulerPens = {
    RulerPen : Pen
    RulerBrush : Brush
}


let makeDefaultRulerPens () =
    let rulerBrush = SolidColorBrush (Colors.Black)
    {   RulerPen = Pen (rulerBrush, 1.0)
        RulerBrush = rulerBrush
    }


let renderScaleRuler (context : DrawingContext, dim : inref<ScaleGridDims>, pens : RulerPens, typeface) =
    let rulerT = Matrix (dim.PixelStep.X, 0.0, 0.0, 6.0, 10.0, 10.0)

    for r in rulers.[dim.Subdivision] do
        context.FillRectangle (pens.RulerBrush, r.TransformToAABB rulerT)
    context.DrawRectangle (pens.RulerPen, Rect(0.0, 0.0, 1.0, 1.0).TransformToAABB rulerT)

    let placeLabel text (p : Point) =
        let ft = FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, 12.0, pens.RulerBrush) // (Text = text, Typeface = typeface, FontSize = 12.0)
        context.DrawText(ft, p.Transform rulerT - Point (0.5 * ft.Width, 0.0))

    Point (0.0, 1.0) |> placeLabel "0"
    Point (1.0, 1.0) |> placeLabel (dim.CoarseStepX.ToString ())
