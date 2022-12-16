﻿using Microsoft.Win32;

namespace Fi.Pentode.Registry.Lib;

/// <summary>
/// The wrapper class, presenting Windows registry keys via <see
/// cref="IRegistryKey"/> interface.
/// </summary>
public sealed class WindowsRegistryKey : IRegistryKey
{
    private readonly RegistryKey _registryKey;

    private bool _disposedValue;

    /// <summary>
    /// Create wrapper for the given Windows registry key.
    /// </summary>
    /// <remarks>
    /// The corresponding Windows registry key will be readonly.
    /// </remarks>
    /// <param name="key">
    /// The Windows registry key to create wrapper for.
    /// </param>
    public WindowsRegistryKey(RegistryKey key) => _registryKey = key;

    /// <inheritdoc/>
    public IRegistryKey CreateSubKey(string subKey) =>
        new WindowsRegistryKey(_registryKey.CreateSubKey(subKey));

    /// <inheritdoc/>
    public void DeleteSubKeyTree(string subKey) =>
        _registryKey.DeleteSubKeyTree(subKey, throwOnMissingSubKey: false);

    /// <inheritdoc/>
    public IEnumerable<string> SubKeyNames => _registryKey.GetSubKeyNames();

    /// <inheritdoc/>
    public object? GetValue(string valueName, object? defaultValue) =>
        _registryKey.GetValue(valueName, defaultValue);

    /// <inheritdoc/>
    public RegistryValueKind GetValueKind(string valueName) =>
        _registryKey.GetValueKind(valueName);

    /// <inheritdoc/>
    public IEnumerable<string> ValueNames => _registryKey.GetValueNames();

    /// <inheritdoc/>
    public IRegistryKey OpenSubKey(string subKey) =>
        new WindowsRegistryKey(_registryKey.OpenSubKey(subKey)!);

    /// <inheritdoc/>
    public IRegistryKey OpenSubKeyAsWritable(string subKey)
    {
        return new WindowsRegistryKey(
            _registryKey.OpenSubKey(subKey, writable: true)!
        );
    }

    /// <inheritdoc/>
    public void SetValue(string valueName, object value) =>
        _registryKey.SetValue(valueName, value);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposedValue)
        {
            _registryKey.Dispose();
            _disposedValue = true;
        }
    }
}
