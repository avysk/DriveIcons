open Fi.Pentode.Registry.Lib

let localMachine = new WindowsRegistryKey(Microsoft.Win32.Registry.LocalMachine) in
let driveIcons = DriveIcons(localMachine) in
driveIcons['A'] |> printfn "%s"
