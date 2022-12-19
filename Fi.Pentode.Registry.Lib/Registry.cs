using JetBrains.Annotations;

namespace Fi.Pentode.Registry.Lib;

/// <summary>
/// Exception to throw if something is wrong with registry.
/// </summary>
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
public sealed class RegistryException : Exception
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
{
    /// <inheritdoc/>
    public RegistryException() { }

    /// <inheritdoc/>
    public RegistryException(string message) : base(message) { }

    /// <inheritdoc/>
    public RegistryException(string message, Exception inner)
        : base(message, inner) { }
}

/// <summary>
/// The class to set disks' icons.
/// </summary>
[PublicAPI]
public sealed class DriveIcons
{
    private readonly IRegistryKey _rootKey;

    /// <summary>
    /// Minimal supported disk.
    /// </summary>
    public const char MinDisk = 'A';

    /// <summary>
    /// Maximal supported disk.
    /// </summary>
    public const char MaxDisk = 'Z';

    private IRegistryKey DriveIconsKey()
    {
        List<string> subKeys =
            new()
            {
                "SOFTWARE",
                "Microsoft",
                "Windows",
                "CurrentVersion",
                "Explorer",
                "DriveIcons"
            };
        var lastKey = _rootKey;
        foreach (var subKey in subKeys)
        {
            var nextKey = lastKey.OpenSubKey(subKey);
            if (!ReferenceEquals(lastKey, _rootKey))
            {
                lastKey.Dispose();
            }

            lastKey =
                nextKey
                ?? throw new RegistryException(
                    $"Failed to get subKey {subKey}."
                );
        }

        return lastKey;
    }

    private IRegistryKey DriveIconsKeyWritable()
    {
        List<string> subKeys =
            new()
            {
                "SOFTWARE",
                "Microsoft",
                "Windows",
                "CurrentVersion",
                "Explorer",
                "DriveIcons"
            };
        var lastKey = _rootKey;
        foreach (var subKey in subKeys)
        {
            if (!ReferenceEquals(lastKey, _rootKey))
            {
                lastKey!.Dispose();
            }

            var nextKey = lastKey.OpenSubKeyAsWritable(subKey);
            lastKey = nextKey;
            if (nextKey == null)
            {
                throw new RegistryException($"Failed to get subKey {subKey}.");
            }
        }

        return lastKey!;
    }

    private string? IconPath([CheckDisk] char disk)
    {
        var letter = disk.ToString();
        var subKeys = DriveIconsKey().SubKeyNames.ToArray();

        if (!subKeys.Contains(letter))
        {
            return null;
        }

        using var driveIconKey = DriveIconsKey().OpenSubKey(letter);
        if (driveIconKey == null)
        {
            throw new RegistryException(
                $"Key for disk {letter} both does and does not exist."
            );
        }

        const string defaultIcon = "DefaultIcon";

        if (!driveIconKey.SubKeyNames.Contains(defaultIcon))
        {
            throw new RegistryException(
                $"Key for disk {letter} does exist but does not have "
                    + $"{defaultIcon} subKey."
            );
        }

        using var finalKey = driveIconKey.OpenSubKey(defaultIcon);
        if (finalKey == null)
        {
            throw new RegistryException(
                $"Default icon subKey for disk {letter} both does "
                    + "and does not exist."
            );
        }

        var value = finalKey.GetValue(string.Empty, null);
        if (value == null)
        {
            return null;
        }

        var kind = finalKey.GetValueKind(string.Empty);
        if (kind != Microsoft.Win32.RegistryValueKind.String)
        {
            throw new RegistryException(
                $"There is a default icon value for disk {disk} "
                    + "but it is not a string."
            );
        }

        return (string)value;
    }

    private void WriteIconPath([CheckDisk] char disk, string newPath)
    {
        var letter = disk.ToString();
        using var driveIconsKey = DriveIconsKeyWritable();
        using var driveIconKey = driveIconsKey.CreateSubKey(letter);
        const string defaultIcon = "DefaultIcon";
        using var finalKey = driveIconKey.CreateSubKey(defaultIcon);
        finalKey.SetValue(string.Empty, newPath);
    }

    private void DeleteIconPath([CheckDisk] char disk)
    {
        var letter = disk.ToString();

        if (!DriveIconsKey().SubKeyNames.Contains(letter))
        {
            // If key for drive does not exist, we do nothing.
            // Otherwise, we will check that it is well-formed and delete it.
            return;
        }

        using var driveIconKey = DriveIconsKey().OpenSubKey(letter)!;
        if (driveIconKey == null)
        {
            throw new RegistryException(
                $"Key for disk {letter} both does and does not exist."
            );
        }

        const string defaultIcon = "DefaultIcon";
        if (!driveIconKey.SubKeyNames.Contains(defaultIcon))
        {
            throw new RegistryException(
                $"Key for disk {letter} does exist but does not have "
                    + $"{defaultIcon} subKey."
            );
        }

        using var finalKey = driveIconKey.OpenSubKey(defaultIcon);
        if (finalKey == null)
        {
            throw new RegistryException(
                $"Default icon subKey for disk {letter} both does "
                    + "and does not exist."
            );
        }

        if (finalKey.SubKeyNames.Any())
        {
            throw new RegistryException(
                $"Default icon subKey for disk {letter} "
                    + "looks very strange."
            );
        }

        var names = finalKey.ValueNames;
        var enumerable = names as string[] ?? names.ToArray();
        var length = enumerable.Length;
        if ((length != 1) || (!enumerable.Contains(string.Empty)))
        {
            throw new RegistryException(
                $"Default icon subKey for disk {letter} "
                    + "looks very strange."
            );
        }

        var kind = finalKey.GetValueKind(string.Empty);
        if (kind != Microsoft.Win32.RegistryValueKind.String)
        {
            throw new RegistryException(
                $"Default icon subKey for disk {letter} "
                    + "looks very strange."
            );
        }

        this.DriveIconsKey().DeleteSubKeyTree(letter);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DriveIcons"/> class given
    /// the root registry key.
    /// </summary>
    /// <remarks>
    /// The root key is never disposed.
    /// </remarks>
    /// <param name="registry">
    /// The registry key representing the root key.
    /// </param>
    public DriveIcons(IRegistryKey registry)
    {
        this._rootKey = registry;
    }

    /// <summary>
    /// Get the path to custom drive icon for the given disk.
    /// </summary>
    /// <remarks>
    /// The path is read from the registry. If it is not set,
    /// <strong>null</strong> is returned.
    /// </remarks>
    /// <param name="disk">
    /// The character, representing the disk.
    /// </param>
    /// <returns>
    /// The path (inside quotes) to the custom disk icon or
    /// <strong>null</strong>.
    /// </returns>
    /// <exception cref="RegistryException">
    /// Thrown, if the disk specified is not between 'A' and 'Z'.
    /// </exception>
    public string? this[char disk]
    {
        get => this.IconPath(disk);
        set
        {
            if (value != null)
            {
                this.WriteIconPath(disk, value);
            }
            else
            {
                this.DeleteIconPath(disk);
            }
        }
    }
}
