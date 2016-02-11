[<AutoOpen>] 
module GraphIt.PrivateEye2

open System
open System.IO
open PrivateEye.Bridge
open PrivateEye.Bridge.profilersession

type ByteSizeFormatProvider() =

    let formatSpecifier = "SZ"
    let kiloByte = 1024m
    let megaByte = kiloByte * 1024m
    let gigaByte = megaByte * 1024m

    let rec getPrecisionFormat precision =
        if String.IsNullOrEmpty(precision) then getPrecisionFormat "2"
        else match precision with
             | "0" -> "N0"
             | "1" -> "N1"
             | "2" -> "N2"
             | "3" -> "N3"
             | "4" -> "N4"
             | _ -> "N" + precision

    let defaultFormat format formatProvider (arg : obj) =
        match arg with
        | :? IFormattable as formattable -> formattable.ToString(format, formatProvider) 
        | _ -> arg.ToString()

    interface IFormatProvider with
        member this.GetFormat(formatType) = 
            if formatType = typeof<ICustomFormatter> then this :> obj else null

    interface ICustomFormatter with
        member this.Format(format, arg, formatProvider) =
            if (format = null 
                || not (format.StartsWith(formatSpecifier, StringComparison.Ordinal)) 
                || arg :? string) then
                arg |> defaultFormat format formatProvider
            else
                let size = try  Some(Convert.ToDecimal(arg, formatProvider))
                           with | :? InvalidCastException -> None
                match size with
                | None -> arg |> defaultFormat format formatProvider
                | Some(size) ->
                    let (size, suffix, ignorePrecision) = 
                        if      (size > gigaByte) then (size / gigaByte, "GB", false)
                        else if (size > megaByte) then (size / megaByte, "MB", false)
                        else if (size > kiloByte) then (size / kiloByte, "KB", false)
                        else if (size = 1m      ) then (size, " byte", true)
                        else                           (size, " bytes", true)
                    let precision = if ignorePrecision then "0" else format.Substring(formatSpecifier.Length)
                    size.ToString(getPrecisionFormat precision, formatProvider) + suffix

let line = new string('-', 80)
let longline = new string('-', 105)

let left str length = 
    if String.length str > length then str.Substring(0, length)
    else str 

let tinfoHeader() = 
    sprintf "%s\n|%-42s|%10s|%15s|%8s|\n%s\n" 
        line 
        "Name" 
        "Allocs" 
        "Alloc Size" 
        "Thrown" 
        line

let tinfoFooter() = line 
let tinfoRow (info : TypeInformation) = 
    let nameShort = left info.Name 42
    sprintf "|%-42s|%10u|%15s|%8u|\n" 
        nameShort 
        info.AllocatedCount 
        (String.Format(new ByteSizeFormatProvider(), "{0:SZ}", info.AllocatedSize))
        info.ThrownCount

let printTypeSeq (seq : seq<TypeInformation>) =
    let x = seq |> Seq.map(tinfoRow) |> String.concat ""
    tinfoHeader() + x + tinfoFooter()

let minfoHeader() = 
    sprintf "\n%s\n|%-33s|%8s|%11s|%8s|%11s|%8s|%11s|%6s|\n%s\n" 
        longline 
        "Name" 
        "Sampled" 
        "Total Time" 
        "Allocs" 
        "A. Size"
        "AAlloc"
        "AA. Size"
        "Thrown" 
        longline 

let printMethodName (info : MethodInformation) =
    if info.Name.Contains("ctor") then
        let sections = info.Name.Split('.')
        sections |> 
        Seq.skip (sections.Length - 2) |> 
        String.concat ""
    else
        info.Name.Split(':') |> Seq.last

let minfoFooter() = longline
let minfoRow (info : MethodInformation) =
    let ms = decimal info.TotalTime / 1000000M
    sprintf "|%-33s|%8d|%11M|%8d|%11s|%8d|%11s|%6d|\n" 
        (left (printMethodName info) 33)
        info.CalledCount 
        (System.Math.Round(ms, 4))
        info.AllocationsMadeCount 
        (String.Format(new ByteSizeFormatProvider(), "{0:SZ}", info.AllocationsMadeSize))
        info.EffectiveAllocationsMadeCount
        (String.Format(new ByteSizeFormatProvider(), "{0:SZ}", info.EffectiveAllocationsMadeSize))
        info.ExceptionsThrown

let printMethodSeq (seq : seq<MethodInformation>) =
    let txt = seq |> Seq.map(minfoRow) |> String.concat ""
    minfoHeader() + txt + minfoFooter()

let calledHeader() = line + "\n"
let calledFooter() = line + "\n"

let allocatedHeader() = line + "\n"
let allocatedFooter() = line + "\n"

let calledRow (call : MethodCallInfo) =
    sprintf "|%17d|%60s|\n"
        call.Count
        call.Method.Name

let allocatedRow (info : MethodAllocationInfo) =
    sprintf "|%17d|%60s|\n"
        info.AllocationCount
        info.Method.Name

let calledInfo (seq : seq<MethodCallInfo>) =
    let rows = seq |> Seq.map(calledRow) |> String.concat ""
    calledHeader() + rows + calledFooter()

let allocatedInfo (seq : seq<MethodAllocationInfo>) =
    let rows = seq |> Seq.map(allocatedRow) |> String.concat ""
    allocatedHeader() + rows + allocatedFooter()

let getMethodStr(x : MethodInformation) =
    let sw = new StringWriter()
    fprintfn sw "Name: %s" x.Name
    fprintfn sw "Samples  : %10d\t\tTail   : %15d" x.CalledCount x.TailCalls
    fprintfn sw "Allocs  : %10d\t\tSize   : %15s" x.AllocationsMadeCount (String.Format(new ByteSizeFormatProvider(), "{0:SZ}", x.AllocationsMadeSize))
    fprintfn sw "A.Allocs: %10d\t\tA.Size : %15s" x.EffectiveAllocationsMadeCount (String.Format(new ByteSizeFormatProvider(), "{0:SZ}", x.EffectiveAllocationsMadeSize))
    fprintfn sw "Time T  : %10d\t\tUnder  : %15d\n" x.TotalTime x.TimeUnder
    sw.ToString()

let getTypeStr(x : TypeInformation) =
    let sw = new StringWriter()
    fprintfn sw "Name: %s" x.Name
    fprintfn sw "Allocs: %10d\t\tSize : %15s" x.AllocatedCount (String.Format(new ByteSizeFormatProvider(), "{0:SZ}", x.AllocatedSize))
    fprintfn sw "Thrown: %10d\t\t\n" x.ThrownCount
    sw.ToString()

let printMethodInfo (x : MethodInformation) =
    let callsTo = "Calls To:\n" + calledInfo(x.GetCallsTo())
    let callsFrom = "Called From:\n" + calledInfo(x.GetCalledFrom())
    let info = getMethodStr(x) 
    info + callsTo + callsFrom

let printTypeInfo (x : TypeInformation) =
    let allocatedBy = "Allocated By:\n" + allocatedInfo(x.GetAllocatedBy())
    let actualAllocatedBy = "Actual Allocations By\n" + allocatedInfo(x.GetEffectiveAllocatedBy())
    let info = getTypeStr(x) 
    info + allocatedBy + actualAllocatedBy


type peyeStats = { 
    totalMessages: uint64;
    calls: uint64;
    exceptions : uint64;
    allocations : uint64;
    typeDefinitions : uint64;
    methodDefinitions : uint64;
    methodDefinitionMisses : uint64;
    typeDefinitionMisses : uint64;
    methodEnterM : uint64;
    methodLeaveM : uint64;
    allocationsM : uint64;
} 

let printStats(x : peyeStats) = 
    let sw = new StringWriter()
    fprintfn sw "Profiler statistics"
    fprintfn sw "    Messages    : %d" x.totalMessages 
    fprintfn sw "    Calls       : %d" x.calls
    fprintfn sw "    Exceptions  : %d" x.exceptions
    fprintfn sw "    Allocations : %d" x.allocations
    fprintfn sw "    Type Defs   : %d" x.typeDefinitions
    fprintfn sw "    Method Defs : %d" x.methodDefinitions
    fprintfn sw "    EnterM      : %d" x.methodEnterM
    fprintfn sw "    LeaveM      : %d" x.methodLeaveM
    fprintfn sw "    AllocM      : %d" x.allocationsM
    fprintfn sw "    MD misses   : %d" x.methodDefinitionMisses
    fprintfn sw "    TD misses   : %d" x.typeDefinitionMisses
    sw.ToString()

let addFsiPrinters(fsi:Microsoft.FSharp.Compiler.Interactive.InteractiveSession) =
    fsi.AddPrinter (fun (x:seq<MethodInformation>) -> printMethodSeq x)
    fsi.AddPrinter (fun (x:seq<TypeInformation>) -> printTypeSeq x)
    fsi.AddPrinter (fun (x:MethodInformation) -> printMethodInfo x)
    fsi.AddPrinter (fun (x:TypeInformation) -> printTypeInfo x)
    fsi.AddPrinter (fun (x:peyeStats) -> printStats x)

let inverse x = UInt64.MaxValue - x


//method helpers

let mostTimeMethods () =  
    Profiler.Data.Methods 
    |> Seq.filter (fun x-> x.CalledCount > 0UL)
    |> Seq.sortBy (fun x-> inverse x.TotalTime) 


let mostTimeUnderMethods () =  
    Profiler.Data.Methods 
    |> Seq.filter (fun x-> x.CalledCount > 0UL)
    |> Seq.sortBy (fun x-> inverse x.TimeUnder) 

let highestAfferent () =
    Profiler.Data.Methods
    |> Seq.filter (fun x-> x.CalledCount > 0UL)
    |> Seq.sortBy (fun x-> inverse x.CalledFromCount)

let highestEfferent () =
    Profiler.Data.Methods
    |> Seq.filter (fun x-> x.CalledCount > 0UL)
    |> Seq.sortBy (fun x-> inverse x.CallsToCount)

let mostCalledMethods () =  
    Profiler.Data.Methods 
    |> Seq.filter  (fun x-> x.CalledCount > 0UL) 
    |> Seq.sortBy (fun x-> inverse x.CalledCount) 

let mostActualAllocatingMethods () =  
    Profiler.Data.Methods 
    |> Seq.filter  (fun x-> x.EffectiveAllocationsMadeCount > 0UL) 
    |> Seq.sortBy (fun x-> inverse x.EffectiveAllocationsMadeSize) 


let mostAllocatingMethods () =  
    Profiler.Data.Methods 
    |> Seq.filter  (fun x-> x.AllocationsMadeCount > 0UL) 
    |> Seq.sortBy (fun x-> inverse x.AllocationsMadeSize) 


let methodsThrowingExceptions () =
    Profiler.Data.Methods
    |> Seq.filter (fun x->x.ExceptionsThrown > 0UL)
    |> Seq.sortBy (fun x-> inverse x.ExceptionsThrown)

//type helpers

let mostThrownTypes () =
    Profiler.Data.Types
    |> Seq.filter (fun x->x.ThrownCount > 0UL)
    |> Seq.sortBy (fun x->inverse x.ThrownCount)

let mostAllocatedTypes () =
    Profiler.Data.Types
    |> Seq.filter (fun x->x.AllocatedCount > 0UL)
    |> Seq.sortBy (fun x->inverse x.AllocatedSize)

let onAllocation () =
    Profiler.Data.ObjectAllocated :> System.IObservable<_>


let onMethodEnter () =
    Profiler.Data.MethodEntered :> System.IObservable<_>


let onMethodLeave () =
    Profiler.Data.MethodLeft :> System.IObservable<_>


let onMethodCalled () =
    Profiler.Data.MethodCalled :> System.IObservable<_>

let onExceptionThrown () =
    Profiler.Data.ExceptionThrown :> System.IObservable<_>


let profSummary() = 
    let x = mostThrownTypes() |> printTypeSeq
    printf "%s" x

let resetProfiler() =
    Profiler.ResetProfilerSession()

let resetSession() = 
    Profiler.ResetData()

//stats
let profilerStats() =
    {
        totalMessages = Profiler.Data.TotalMessages;
        calls = Profiler.Data.MethodsCalled;
        exceptions = Profiler.Data.TotalExceptionCount;
        allocations = Profiler.Data.TotalAllocations;
        typeDefinitions = Profiler.Data.TypeDefinitions;
        methodDefinitions = Profiler.Data.MethodDefinitions;
        methodEnterM = Profiler.Data.MethodEnterM;
        methodLeaveM = Profiler.Data.MethodLeaveM;
        allocationsM = Profiler.Data.AllocationsM;
        methodDefinitionMisses = Profiler.Data.MethodDefinitionMisses;
        typeDefinitionMisses = Profiler.Data.TypeDefinitionMisses;
    }

//commands
let enableGCTracking() = 
    Profiler.EnableGCTracking()

let disableGCTracking() = 
    Profiler.DisableGCTracking()

let enableAllocationTracking() = 
    Profiler.EnableAllocationTracking()

let disableAllocationTracking() = 
    Profiler.DisableAllocationTracking()

let enableMethodTracking() = 
    Profiler.EnableMethodTracking()

let disableMethodTracking() = 
    Profiler.DisableMethodTracking()

let enableExceptionTracking() = 
    Profiler.EnableExceptionTracking()

let disableExceptionTracking() = 
    Profiler.DisableExceptionTracking()

//self profiling

let profile x = 
    printf "starting self profile\n"
    Profiler.SelfStartProfiling()
    printf "done.\n"
    printf "calling code\n"
    let ret = x()
    printf "done calling code\n"
    printf "calling stop self\n"
    Profiler.SelfStopProfiling 5
    printf "stop self done\n"
    ret

let printLogo() = printf "Private Eye\n%s\n" Profiler.Logo

printLogo()

//example
//connect to a remote profiler start remote profiler first
//Profiler.ConnectTo(new IPEndPoint (new IPAddress(...), 4444)

//Start listening after this you would start the profiler which connects
//Profiler.StartListening()

//now you are connected and can run something like this:
//
//Profiler.Data.Methods 
//      |> Seq.filter  (fun x-> x.AllocationsMadeCount > 0UL) 
//      |> Seq.sortBy (fun x-> x.CalledCount) 
//      |> Seq.take 50 
//      |> Seq.iter dump_minfo

