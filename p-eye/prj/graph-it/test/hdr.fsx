open System
open System.IO
#I "../packages"
#r "HdrHistogram.NET/HdrHistogram.NET.dll"
open HdrHistogram.NET
#I "../bin/debug"
#r "GraphIt.dll"
open GraphIt
open GraphIt.Lib
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

let hdr = new HdrHistogram.NET.Histogram(int64 1,TimeSpan.TicksPerSecond * 10L,1)
[|1L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;2L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;3L;4L|] |> Array.map(fun x->x*1000L)|> Array.iter (fun x->hdr.recordValueWithCount(x,1L))

let rendered = __SOURCE_DIRECTORY__ + "/../chart-html/chart-rendered.html"

[("hdr",hdr)] 
|> GraphIt.Render.Chart.configure "Latency"
|> File.toFile rendered

Process.start1 rendered

// manual load
// File.toFile "./chart-html/chart-rendered-data.hgrm" (hdr.sprintPercentileDistribution())
// Process.start1 "http://hdrhistogram.github.io/HdrHistogram/plotFiles.html"
