// a scriptable package manager

open System
open System.IO
 
Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
 
if not (File.Exists "paket.exe") then
    let url = "http://fsprojects.github.io/Paket/stable"
    use wc = new Net.WebClient() 
    let tmp = Path.GetTempFileName() 
    let stable = wc.DownloadString(url) 
    wc.DownloadFile(stable, tmp)
    File.Move(tmp,Path.GetFileName stable)

