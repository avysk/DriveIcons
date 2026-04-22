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
    /// Open for writing the subKey of the registry key.
    /// </summary>
    /// <remarks>
    /// If the subKey does not exist, an empty subKey is created first.
    /// </remarks>
    /// <param name="subKey">
    /// The name of the subKey.
    /// </param>
    /// <returns>
    /// The subKey opened.
    /// </returns>
    public IRegistryKey CreateSubKey(string subKey);

    /// <summary>
    /// Open for reading the subKey of the registry key.
    /// </summary>
    /// <remarks>
    /// The subKey must exist. The subKey is opened as readonly,
    /// </remarks>
    /// <param name="subKey">
    /// The name of the subKey,
    /// </param>
    /// <returns>
    /// The subKey opened.
    /// </returns>
    public IRegistryKey? OpenSubKey(string subKey);

    /// <summary>
    /// Open for writing the subKey of the registry key.
    /// </summary>
    /// <remarks>
    /// The subKey must exist.
    /// </remarks>
    /// <param name="subKey">
    /// The name of the subKey,
    /// </param>
    /// <returns>
    /// The subKey opened.
    /// </returns>
    public IRegistryKey? OpenSubKeyAsWritable(string subKey);

    /// <summary>
    /// Delete a subtree starting from the given subKey.
    /// </summary>
    /// <param name="subKey">
    /// A short name of the subKey at the root of the tree to delete.
    /// </param>
    public void DeleteSubKeyTree(string subKey);

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
