namespace SciTwi.UI.Rendering

open System
open Avalonia
open Avalonia.Media


type PanState () =
    let panning = Event<bool> ()
    let mutable mouseTrack = ValueNone

    member __.Panning =
        panning.Publish :> IObservable<_>

    member __.Start (origin : Point) =
        match mouseTrack with
        | ValueNone ->
            mouseTrack <- ValueSome origin
            panning.Trigger true
        | _ -> ()

    member __.TryMove (curr : Point) =
        match mouseTrack with
        | ValueSome prev ->
            let delta = curr - prev
            mouseTrack <- ValueSome curr
            Nullable delta
        | _ -> Nullable ()

    member __.Stop () =
        match mouseTrack with
        | ValueSome _ ->
            mouseTrack <- ValueNone
            panning.Trigger false
        | _ -> ()
