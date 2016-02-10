
module Test

open System
open System.IO
open PrivateEye.Bridge
open PrivateEye.Bridge.profilersession
open GraphIt
open GraphIt.Lib
open HdrHistogram.NET

module Filter = 
  let called (xs:MethodInformation seq) =
    xs
    |> Seq.filter (fun x-> x.CalledCount > 0UL)

module Test = 
  let rendered = __SOURCE_DIRECTORY__ + "/chart-html/chart-rendered.html"
  
  let hdr = new HdrHistogram.NET.Histogram(int64 1,TimeSpan.TicksPerSecond * 10L,1)

  Profiler.Data.Methods
  |> Filter.called
  |> Seq.iter(fun x->hdr.recordValueWithCount(int64 <| x.GetHashCode(),int64 <| x.CalledCount))

  let chart = new Render.Chart(__SOURCE_DIRECTORY__)

  [("hdr",hdr)] 
  |> (fun l->Render.Chart.create("Latency",l))
  |> File.toFile rendered

  Process.start1 rendered
