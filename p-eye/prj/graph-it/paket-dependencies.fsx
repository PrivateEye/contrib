open System
open System.IO
#load "paket-get.fsx"
#r "paket.exe"         
open Paket

let dependencies = Dependencies.Locate(__SOURCE_DIRECTORY__)

System.Diagnostics.Process.Start("paket.exe", "init")

dependencies.Add("HdrHistogram.NET")
dependencies.Install(true,true)

