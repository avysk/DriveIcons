﻿open RegistryLib

let localMachine = new WindowsRegistryKey(Microsoft.Win32.Registry.LocalMachine)
let driveIcons = new DriveIcons(localMachine)
driveIcons['A'] |> printfn "%s"
