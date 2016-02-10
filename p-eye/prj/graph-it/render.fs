namespace GraphIt.Render

open System
open System.IO
open GraphIt.Lib
open HdrHistogram.NET

module Histogram =

  let sprintPercentileDistribution (hdr:Histogram) = 
    let wr = new StringWriter()
    hdr.outputPercentileDistribution(wr)
    wr.ToString()

module Chart = // (templateDir : string) = 

  let _data_div         = GraphIt.RenderInfo.dataDiv // File.ReadAllText templateDir + "/chart-html/data-div"
  let _draw_initial_fun = GraphIt.RenderInfo.drawInitialFun // File.ReadAllText templateDir + "./chart-html/draw-initial-fun"
  let _chart            = GraphIt.RenderInfo.chart //File.ReadAllText templateDir + "./chart-html/chart"

  let create (name:string) (data:(string * Histogram) list) =

    let ids       = List.mapi(fun i _->sprintf "data%i" i) data
    
    let vars      = List.map(fun x -> sprintf "%sStr" x) ids
    
    let selectors = 
      (vars,ids)  
      ||> List.mapi2(fun i var id -> sprintf "var %s = document.querySelector(\"div#%s\").innerHTML.trim();\n" var id)
    
    let squote s  = sprintf "'%s'" s
    
    let toList (s:string list) =
      s 
      |> List.mapi(fun i s->(if i > 0 then ", " else "") + s) 
      |> fun l -> String.Join("",l)
    
    let toLines (s:string list) = 
      String.Join("\n",s)
    
    let wrapInSqParens x = 
      sprintf "[%s]" x
    
    let data_divs = 
      (ids,data 
            |> List.map(fun (_,histogram)->histogram |> Histogram.sprintPercentileDistribution) )
      ||> List.map2(fun id data-> 
            _data_div
            |> String.replace "$id" id
            |> String.replace "$data" data)
    
    let histos =
      vars 
      |> toList 
      |> wrapInSqParens
    
    let names = 
      data 
      |> List.map(fun (nme,_)-> squote nme) 
      |> toList
      |> wrapInSqParens
    
    let drawInitial = 
      _draw_initial_fun 
      |> String.replace "$selectors" (toLines selectors)
      |> String.replace "$histos" histos
      |> String.replace "$names" names
    
    let chart =
      _chart 
      |> String.replace "Latency" name
      |> String.replace "$draw-initial" drawInitial
      |> String.replace "$data-divs" (toLines data_divs)
    
    chart

