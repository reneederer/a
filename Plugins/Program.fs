open System
open System.Reflection
open System.Linq
open FSharp.Reflection
open System.ServiceProcess

[<EntryPoint>]
let main argv =
    let pluginLibPath =
        @"C:\Users\rene\source\repos\Plugins\PluginLib\bin\Debug\net5.0\PluginLib.dll"
    let pluginLibNs =
        (Assembly.LoadFile pluginLibPath).Modules
        |> Seq.item 0
    let say = pluginLibNs.GetTypes().[0]
    let l = say.GetMethods()
    let s = l.[0].Invoke(null, [|"test"|])
    printfn $"""{s}"""
    let pluginLibPath =
        @"C:\Users\rene\source\repos\Plugins\PluginLib1\bin\Debug\net5.0\PluginLib.dll"
    let pluginLibNs =
        (Assembly.LoadFile pluginLibPath).Modules
        |> Seq.item 0
    let say = pluginLibNs.GetTypes().[0]
    let l = say.GetMethods()
    let s = l.[0].Invoke(null, [|"test"|])
    printfn $"""{s}"""
    use serviceController = new ServiceController("AtherosSvc")
    //serviceController.Stop()
    //printfn $"{serviceController.Status}"
    printfn $"{serviceController.Status}"
    0