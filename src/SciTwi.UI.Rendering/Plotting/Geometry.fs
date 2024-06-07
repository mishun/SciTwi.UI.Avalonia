module SciTwi.UI.Rendering.Plotting.Geometry

open System
open Avalonia
open Avalonia.Media
open SciTwi.UI.Rendering


let render (context : DrawingContext, bounds : Rect, t : Matrix, brush : IBrush, pen : IPen, geometry : LayeredGeometry) =
    match geometry with
    | LayeredGeometry.Line (a, b, c) ->
        let struct (p0, p1) =
            let dx = a / sqrt (a * a + b * b)
            let dy = b / sqrt (a * a + b * b)
            let dc = -c / sqrt (a * a + b * b)
            struct (Point(dc * dx - dy, dc * dy + dx).Transform t, Point(dc * dx + dy, dc * dy - dx).Transform t)
        if abs (p0.X - p1.X) > abs (p0.Y - p1.Y) then
            let put x = p0 + (p1 - p0) * ((x - p0.X) / (p1.X - p0.X))
            context.DrawLine (pen, put bounds.X, put bounds.Right)
        else
            let put y = p0 + (p1 - p0) * ((y - p0.Y) / (p1.Y - p0.Y))
            context.DrawLine (pen, put bounds.Y, put bounds.Bottom)

    | LayeredGeometry.Ellipse ellipse ->
        use __ = context.PushTransform (ellipse * t)
        let pen = Pen (brush, 2.0)
        context.DrawRectangle (pen, Rect (-1.0, -1.0, 2.0, 2.0), cornerRadius = 1.0f)

    | LayeredGeometry.Chain points ->
        if Array.length points > 1 then
            for i in 0 .. points.Length - 2 do
                context.DrawLine (pen, points.[i].Transform t, points.[i + 1].Transform t)
