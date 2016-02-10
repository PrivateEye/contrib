#I "bin/debug"
#I "lib"
#I "packages"
#r "HdrHistogram.NET/HdrHistogram.NET.dll"
#r "PrivateEye.Bridge.dll"
#r "GraphIt.dll"
open System
open System.IO
open GraphIt
open GraphIt.Lib
open HdrHistogram.NET
open PrivateEye.Bridge
open PrivateEye.Bridge.profilersession

addFsiPrinters fsi

Profiler.StartListening()

// 1. start a console (cmd.exe, etc)
//
// from ..
//
// 2. launch/anycpu ../basic-app/bin/Debug/basicApp.exe

mostCalledMethods()

let rendered = __SOURCE_DIRECTORY__ + "/chart-html/chart-rendered.html"
  
let calledHdr = new HdrHistogram.NET.Histogram(int64 1,TimeSpan.TicksPerSecond * 10L,1)

Profiler.Data.Methods
|> Filter.called
|> Seq.iter(fun x->calledHdr.recordValueWithCount(int64 <| x.GetHashCode(),int64 <| x.CalledCount))

[("called",calledHdr)] 
|> (fun l->Render.Chart.create "Called" l)
|> File.toFile rendered

Process.start1 rendered