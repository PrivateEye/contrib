[<AutoOpen>] 
module GraphIt.PrivateEye2Extensions

open System
open System.IO
open PrivateEye.Bridge
open PrivateEye.Bridge.profilersession

type MethodAllocationInfo with
  static member method_ (x:MethodAllocationInfo) = x.Method
  static member allocationCount (x:MethodAllocationInfo) = x.AllocationCount
  static member allocationSize (x:MethodAllocationInfo) = x.AllocationSize

type MethodInformation with
  static member allocatedCount (x:MethodInformation) = x.AllocatedCount   
  static member allocationsMadeCount (x:MethodInformation) = x.AllocationsMadeCount   
  static member allocationsMadeSize (x:MethodInformation) = x.AllocationsMadeSize   
  static member calledCount (x:MethodInformation) = x.CalledCount   
  static member calledFromCount (x:MethodInformation) = x.CalledFromCount   
  static member callsToCount (x:MethodInformation) = x.CallsToCount   
  static member effectiveAllocationsMadeCount (x:MethodInformation) = x.EffectiveAllocationsMadeCount   
  static member effectiveAllocationsMadeSize (x:MethodInformation) = x.EffectiveAllocationsMadeSize   
  static member exceptionsCaught (x:MethodInformation) = x.ExceptionsCaught   
  static member exceptionsThrown (x:MethodInformation) = x.ExceptionsThrown   
  static member getAllocated (x:MethodInformation) = x.GetAllocated   
  static member getCalledFrom (x:MethodInformation) = x.GetCalledFrom   
  static member getCallsTo (x:MethodInformation) = x.GetCallsTo   
  static member getHashCode (x:MethodInformation) = x.GetHashCode   
  static member name (x:MethodInformation) = x.Name   
  static member tailCalls (x:MethodInformation) = x.TailCalls   
  static member timeUnder (x:MethodInformation) = x.TimeUnder   
  static member totalTime (x:MethodInformation) = x.TotalTime   
  static member totalTime (x:MethodInformation) = x.TotalTime   

//type PrivateEye.Bridge.profilersession.ExceptionThrownEventArgs with
//type InvalidTypeNameException with
//type InvalidTypeNameException with

type MethodCallInfo with
  static member method (x:MethodCallInfo) = x.Method
  static member count (x:MethodCallInfo) = x.Count   

type MethodCalledEventArgs with
  static member method (x:MethodCalledEventArgs) = x.Method
  static member time (x:MethodCalledEventArgs) = x.Time
  static member timeIn (x:MethodCalledEventArgs) = x.TimeIn
//type MethodEnteredEventArgs with
//type MethodEnteredEventArgs with
//type MethodLeftEventArgs with
//type MethodName with
//type ObjectAllocatedEventArgs with
//type ProfilerSession with
//type RawReadModel  with
//type StackEntry with
//type TypeAllocationInfo with
type TypeInformation with
  static member allocatedCount (x:TypeInformation) = x.AllocatedCount
  static member allocatedSize (x:TypeInformation) = x.AllocatedSize
  static member name (x:TypeInformation) = x.Name
  static member thrownCount (x:TypeInformation) = x.ThrownCount
//type PrivateEye.Bridge.profilersession.TypeInformation with
//  static member (x:TypeInformation) = x.

type TypeName with
  static member namespace_ (x:TypeName) = x.Namespace
  static member type_ (x:TypeName) = x.Type
    
//// aliases
//type MAI = MethodAllocationInfo
//type MI  = MethodInformation

module Filter = 
  let called (xs:MethodInformation seq) =
    xs
    |> Seq.filter (fun x-> x.CalledCount > 0UL)