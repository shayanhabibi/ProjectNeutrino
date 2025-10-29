module ElectronApi.Json.Parser.Tests

open Expecto
    
[<EntryPoint>]
let main argv =
    #if !(ELECTRON_OS_MAC || ELECTRON_OS_WIN)
    let openAsHidden: bool = true
    5
    #endif
    Tests.runTestsInAssemblyWithCLIArgs [] argv
    
