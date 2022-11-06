namespace RegistryLib;

public class RegistryException : Exception
{
    public RegistryException() { }

    public RegistryException(string message) : base(message) { }

    public RegistryException(string message, Exception inner)
        : base(message, inner) { }
}

public class DriveIcons
{
    private readonly IRegistryKey _localMachine;

    private IRegistryKey _driveIconsKey
    {
        get
        {
            const string keyString =
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\DriveIcons";
            using IRegistryKey? driveKey = _localMachine.OpenSubKey(keyString);
            if (driveKey == null)
            {
                throw new RegistryException(
                    $"No key {keyString} is found in registry"
                );
            }

            return driveKey;
        }
    }

    private IRegistryKey _driveIconsKeyWritable
    {
        get
        {
            const string keyString =
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\DriveIcons";
            using IRegistryKey? driveKey = _localMachine.OpenSubKey(
                keyString,
                true
            );
            if (driveKey == null)
            {
                throw new RegistryException(
                    $"No key {keyString} is found in registry"
                );
            }

            return driveKey;
        }
    }

    private static void _verifyDiskIsValid(char disk)
    {
        if ((disk < 'A') || (disk > 'Z'))
        {
            throw new IndexOutOfRangeException($"Disk {disk} is not in range");
        }
    }

    private string? _iconPath(char disk)
    {
        string letter = disk.ToString();
        string error;

        using IRegistryKey driveIconsKey = _driveIconsKey;

        if (!driveIconsKey.GetSubKeyNames().Contains(letter))
        {
            return null;
        }

        using IRegistryKey? driveIconKey = driveIconsKey.OpenSubKey(letter);
        if (driveIconKey == null)
        {
            error = $"Key for disk {letter} both does and does not exist.";
            throw new RegistryException(error);
        }

        const string defaultIcon = "DefaultIcon";

        if (!driveIconKey.GetSubKeyNames().Contains(defaultIcon))
        {
            error =
                $"Key for disk {letter} does exist but does not have "
                + $"{defaultIcon} subkey.";
            throw new RegistryException(error);
        }

        using IRegistryKey? finalKey = driveIconKey.OpenSubKey(defaultIcon);
        if (finalKey == null)
        {
            error =
                $"Default icon subkey for disk {letter} both does "
                + "and does not exist.";
            throw new RegistryException(error);
        }

        const string dfault = "(Default)";

        object? value = finalKey.GetValue(dfault, null);
        if (value == null)
        {
            return null;
        }

        Microsoft.Win32.RegistryValueKind kind = finalKey.GetValueKind(dfault);
        if (kind != Microsoft.Win32.RegistryValueKind.String)
        {
            error =
                $"There is a value {dfault} for disk {disk} "
                + "but it is not a string.";
            throw new RegistryException(error);
        }

        return (string)value;
    }

    private void _writeIconPath(char disk, string newPath)
    {
        string letter = disk.ToString();
        using IRegistryKey driveIconsKey = _driveIconsKeyWritable;
        using IRegistryKey driveIconKey = driveIconsKey.CreateSubKey(letter);
        const string defaultIcon = "DefaultIcon";
        using IRegistryKey finalKey = driveIconKey.CreateSubKey(defaultIcon);
        const string dfault = "(Default)";
        finalKey.SetValue(dfault, newPath);
    }

    private void _deleteIconPath(char disk)
    {
        string letter = disk.ToString();
        string error;

        using IRegistryKey driveIconsKey = _driveIconsKeyWritable;

        if (!driveIconsKey.GetSubKeyNames().Contains(letter))
        {
            return;
        }

        using IRegistryKey driveIconKey = driveIconsKey.OpenSubKey(letter)!;

        const string defaultIcon = "DefaultIcon";

        if (driveIconKey.GetSubKeyNames().Length > 1)
        {
            error = $"Key for disk {letter} looks very strange.";
            throw new RegistryException(error);
        }

        using IRegistryKey finalKey = driveIconKey.OpenSubKey(defaultIcon)!;
        if (finalKey.GetSubKeyNames().Length > 0)
        {
            error = $"Default icon key for disk {letter} looks very strange.";
            throw new RegistryException(error);
        }

        const string dfault = "(Default)";

        object? value = finalKey.GetValue(dfault, null);
        if (value == null)
        {
            return null;
        }

        Microsoft.Win32.RegistryValueKind kind = finalKey.GetValueKind(dfault);
        if (kind != Microsoft.Win32.RegistryValueKind.String)
        {
            error =
                $"There is a value {dfault} for disk {disk} "
                + "but it is not a string.";
            throw new RegistryException(error);
        }
    }

    public DriveIcons(IRegistryKey registry)
    {
        _localMachine = registry;
    }

    public string? this[char disk]
    {
        get
        {
            _verifyDiskIsValid(disk);
            return _iconPath(disk);
        }
        set
        {
            _verifyDiskIsValid(disk);
            dynamic? _ = _iconPath(disk);

            if (value != null)
            {
                _writeIconPath(disk, value);
            }
            else
            {
                _deleteIconPath(disk);
            }
        }
    }
}
