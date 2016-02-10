#I "bin/debug"
#r "graphit.dll"
open Lib

type NPopulation = 
  | Full
  | Partial

let rnd = System.Random()

let c = 100
let population = NPopulation.Full

/// values
let V = Array.init c (fun _->float <| rnd.Next())

let Vmin = Array.min V
let Vmax = Array.max V
let mean = Array.average V
let n = population |> function | NPopulation.Full -> c | NPopulation.Partial -> c - 1
let variance = (Array.fold(fun acc x-> acc + ((x - mean) ** 2.0) ) 0.0 V) / (float n)
let stdDeviation = sqrt variance

