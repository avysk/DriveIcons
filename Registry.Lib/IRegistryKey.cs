namespace Fi.Pentode.Registry.Lib;

/// <summary>
/// The interface for working with registry.
/// </summary>
/// <remarks>
/// Do not use Windows registry directly - use wrapper class <see
/// cref="WindowsRegistryKey"/> implementing this interface. This allows
/// dependency injection and substitution of in-memory registry in tests.
/// Implementing classes should implement <see cref="IDisposable"/> as well.
/// </remarks>
public interface IRegistryKey : IDisposable
{
    /// <summary>
    /// Open for writing the subkey of the registry key.
    /// </summary>
    /// <remarks>
    /// If the subkey does not exist, an empty subkey is created first.
    /// </remarks>
    /// <param name="subkey">
    /// The name of the subkey,
    /// </param>
    /// <returns>
    /// The subkey opened.
    /// </returns>
    public IRegistryKey CreateSubKey(string subkey);

    /// <summary>
    /// Open for reading the subkey of the registry key.
    /// </summary>
    /// <remarks>
    /// The subkey must exist. The subkey is opened as readonly,
    /// </remarks>
    /// <param name="subkey">
    /// The name of the subkey,
    /// </param>
    /// <returns>
    /// The subkey opened.
    /// </returns>
    public IRegistryKey? OpenSubKey(string subkey);

    /// <summary>
    /// Open for writing the subkey of the registry key.
    /// </summary>
    /// <remarks>
    /// The subkey must exist.
    /// </remarks>
    /// <param name="subkey">
    /// The name of the subkey,
    /// </param>
    /// <returns>
    /// The subkey opened.
    /// </returns>
    public IRegistryKey? OpenSubKeyAsWritable(string subkey);

    /// <summary>
    /// Delete a subtree starting from the given subkey.
    /// </summary>
    /// <param name="subkey">
    /// A short name of the subkey at the root of the tree to delete.
    /// </param>
    public void DeleteSubKeyTree(string subkey);

    /// <summary>
    /// The prorperty represents the names of registry subkeys in the key.
    /// </summary>
    public IEnumerable<string> SubKeyNames { get; }

    /// <summary>
    /// Get the value in the registry key.
    /// </summary>
    /// <remarks>
    /// If the value does not exist, the given default value is returned.
    /// </remarks>
    /// <param name="valueName">
    /// The short name of the value in the key.
    /// </param>
    /// <param name="defaultValue">
    /// Default to return if the value with the given name daes not exist.
    /// </param>
    /// <returns>
    /// The value with the given name or default if it does not exist.
    /// </returns>
    public object? GetValue(string valueName, object? defaultValue);

    /// <summary>
    /// The kind (<see cref="Microsoft.Win32.RegistryValueKind"/>) of the
    /// value in the key.
    /// </summary>
    /// <param name="valueName">
    /// The short name of the value.
    /// </param>
    /// <returns>
    /// The kind of the registry value.
    /// </returns>
    public Microsoft.Win32.RegistryValueKind GetValueKind(string valueName);

    /// <summary>
    /// Set the value in the registry key,
    /// </summary>
    /// <param name="valueName">
    /// The name of the value.
    /// </param>
    /// <param name="value">
    /// The value.
    /// </param>
    public void SetValue(string valueName, object value);

    /// <summary>
    /// The prorperty represents the names of registry values in the key,
    /// </summary>
    public IEnumerable<string> ValueNames { get; }
}
