namespace Fi.Pentode.Registry.Lib;

using Microsoft.Win32;

/// <summary>
/// The wrapper class, presenting Windows registry keys via <see
/// cref="IRegistryKey"/> interface.
/// </summary>
public sealed class WindowsRegistryKey : IRegistryKey
{
    private readonly RegistryKey registryKey;

    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowsRegistryKey"/> clas:.
    /// Create wrapper for the given Windows registry key.
    /// </summary>
    /// <remarks>
    /// The corresponding Windows registry key will be readonly.
    /// </remarks>
    /// <param name="key">
    /// The Windows registry key to create wrapper for.
    /// </param>
    public WindowsRegistryKey(RegistryKey key) => registryKey = key;

    /// <inheritdoc/>
    public IRegistryKey CreateSubKey(string subKey) =>
        new WindowsRegistryKey(registryKey.CreateSubKey(subKey));

    /// <inheritdoc/>
    public void DeleteSubKeyTree(string subKey) =>
        registryKey.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);

    /// <inheritdoc/>
    public IEnumerable<string> SubKeyNames => registryKey.GetSubKeyNames();

    /// <inheritdoc/>
    public object? GetValue(string valueName, object? defaultValue) =>
        registryKey.GetValue(valueName, defaultValue);

    /// <inheritdoc/>
    public RegistryValueKind GetValueKind(string valueName) =>
        registryKey.GetValueKind(valueName);

    /// <inheritdoc/>
    public IEnumerable<string> ValueNames => registryKey.GetValueNames();

    /// <inheritdoc/>
    public IRegistryKey OpenSubKey(string subKey) =>
        new WindowsRegistryKey(registryKey.OpenSubKey(subKey)!);

    /// <inheritdoc/>
    public IRegistryKey OpenSubKeyAsWritable(string subKey)
    {
        return new WindowsRegistryKey(
            registryKey.OpenSubKey(subKey, writable: true)!
        );
    }

    /// <inheritdoc/>
    public void SetValue(string valueName, object value) =>
        registryKey.SetValue(valueName, value);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!disposedValue)
        {
            registryKey.Dispose();
            disposedValue = true;
        }
    }
}
