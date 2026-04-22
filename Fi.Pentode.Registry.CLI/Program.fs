open Fi.Pentode.Registry.Lib

let localMachine = new WindowsRegistryKey(Microsoft.Win32.Registry.LocalMachine)
let driveIcons = DriveIcons(localMachine)
driveIcons['A'] |> printfn "%s"
