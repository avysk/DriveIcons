using Fi.Pentode.Registry.Lib;

using Microsoft.Win32;

namespace Fi.Pentode.MockedRegistry;

/// <summary>
/// In-memory "registry" for testing.
/// </summary>
public sealed class MockedRegistryKey : IRegistryKey
{
    private bool _disposedValue;

    private readonly string _path;

    private readonly bool _writable;

    private static Dictionary<string, string[]> _keys = new();

    /// <summary>
    /// Get the dictionary of defined "registry keys".
    /// </summary>
    /// <returns>
    /// Dictionary keys are "full names of registry keys" without trailing \,
    /// values are arrays of full names of subkeys and full names of "registry
    /// values".
    /// </returns>
    public static Dictionary<string, string[]> GetKeys() => _keys;

    private static Dictionary<string, object> _values = new();

    /// <summary>
    /// Get dictionary of defined "registry values".
    /// </summary>
    /// <returns>
    /// Dictionary keys are "full names of registry values", dictionary values
    /// are their values.
    /// </returns>
    public static Dictionary<string, object> GetValues() => _values;

    /// <summary>
    /// In-memory "registry" for testing.
    /// </summary>
    /// <param name="path">Path of selected key.</param>
    /// <param name="keys">The dictionary, mapping cefistry key names to arrays
    /// of subkey and value names.</param>
    /// <param name="values">The dictionary, mapping rvalue names to their
    /// values.</param>
    /// <param name="writable">Specifies, if the selected key is supposed
    /// to be writable.</param>
    public MockedRegistryKey(
        string path,
        Dictionary<string, string[]> keys,
        Dictionary<string, object> values,
        bool writable = false
    )
    {
        _path = path;
        _keys = keys;
        _values = values;
        _writable = writable;
    }

    /// <summary>
    /// Allows to get values of this key.
    /// </summary>
    /// <value>
    /// The names of the values in the key.
    /// </value>
    public IEnumerable<string> ValueNames
    {
        get { return _keys[_path].Where(value => _values.ContainsKey(value)); }
    }

    /// <summary>
    /// Allows to get subkeys of this key.
    /// </summary>
    /// <value>
    /// The names of the subkeys in the key.
    /// </value>
    public IEnumerable<string> SubKeyNames
    {
        get { return _keys[_path].Where(key => _keys.ContainsKey(key)); }
    }

    /// <summary>
    /// Get the subkey of this key.
    /// </summary>
    /// <remarks>
    /// If the subkey does not exist, the empty subkey is created first.
    /// The returned subkey is writable.
    /// </remarks>
    /// <param name="subkey">
    /// The short name of the requested subkey.
    /// </param>
    /// <returns>
    /// <see cref="MockedRegistryKey"/> corresponding to the writable
    /// subkey of the key.
    /// </returns>
    /// <exception cref="RegistryException">
    /// Thrown if the key is not writable.
    /// </exception>
    public IRegistryKey CreateSubKey(string subkey)
    {
        if (!_writable)
        {
            throw new RegistryException(
                "Cannot create subkeys in a readonly key."
            );
        }

        string newPath = $"{_path}\\{subkey}";

        if (!_keys.ContainsKey(newPath))
        {
            // create new empty key
            _keys.Add(newPath, Array.Empty<string>());
        }

        return new MockedRegistryKey(
            path: newPath,
            keys: _keys,
            values: _values,
            writable: true
        );
    }

    /// <summary>
    /// Deletes key tree for the given subkey.
    /// </summary>
    /// <remarks>
    /// Associated values are not deleted.
    /// </remarks>
    /// <param name="subkey">
    /// Starting subkey for tree to delete.
    /// </param>
    /// <exception cref="RegistryException">
    /// Thrown at attempt to delete subtree in a readonly key.
    /// </exception>
    public void DeleteSubKeyTree(string subkey)
    {
        if (!_writable)
        {
            throw new RegistryException(
                "Cannot delete subtree in a readonly key."
            );
        }

        string treeRoot = $"{_path}\\{subkey}";
        string[] keysToRemove = Array.Empty<string>();
        foreach (var key in _keys.Keys)
        {
            if (key.StartsWith(treeRoot))
            {
                _ = keysToRemove.Append(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            bool wasInDictionary = _keys.Remove(key);
            if (!wasInDictionary)
            {
                throw new RegistryException(
                    $"Key {key} was supposed be in "
                        + "the registry but it was not."
                );
            }
        }
    }

    /// <summary>
    /// Get value from "registry".
    /// </summary>
    /// <param name="valueName">
    /// Name of the value in the current key.
    /// </param>
    /// <param name="defaultValue">
    /// Default return value.
    /// </param>
    /// <returns>
    /// Object representing value with the given name, or default value if
    /// "registry value" with the given name does not exist in the current key.
    /// </returns>
    public object? GetValue(string valueName, object? defaultValue)
    {
        const string newPath = "{_path}\\{name}";
        if (!_values.ContainsKey(newPath))
        {
            return defaultValue;
        }

        return _values[newPath];
    }

    /// <summary>
    /// Get type of value in "registry".
    /// </summary>
    /// <param name="valueName">
    /// Name of the value in the current key.
    /// </param>
    /// <returns>
    /// <see cref="RegistryValueKind.String"/> for string value,
    /// <see cref="RegistryValueKind.None"/> for null value,
    /// <see cref="RegistryValueKind.Unknown"/> otherwise.
    /// </returns>
    public RegistryValueKind GetValueKind(string valueName)
    {
        object? value = GetValue(valueName, null);
        if (value is string)
        {
            return RegistryValueKind.String;
        }
        else if (value == null)
        {
            return RegistryValueKind.None;
        }
        else
        {
            return RegistryValueKind.Unknown;
        }
    }

    /// <summary>
    /// Open the subkey of the key.
    /// </summary>
    /// <remarks>
    /// Opens the subkey as readonly.
    /// </remarks>
    /// <param name="subkey">
    /// The name of the subkey to open.
    /// </param>
    /// <returns>
    /// <see cref="MockedRegistryKey"/> corresponding to the requested subkey.
    /// </returns>
    /// <exception cref="RegistryException">
    /// Thrown if subkey does not exist.
    /// </exception>
    public IRegistryKey OpenSubKey(string subkey)
    {
        string newPath = $"{_path}\\{subkey}";
        if (!_keys.ContainsKey(newPath))
        {
            throw new RegistryException("Cannot open non-existent subkey.");
        }

        return new MockedRegistryKey(
            path: newPath,
            keys: _keys,
            values: _values
        );
    }

    /// <summary>
    /// Open the subkey of the key.
    /// </summary>
    /// <remarks>
    /// Opens the subkey as writable.
    /// </remarks>
    /// <param name="subkey">
    /// The name of the subkey to open.
    /// </param>
    /// <returns>
    /// <see cref="MockedRegistryKey"/> corresponding to the requested subkey.
    /// </returns>
    /// <exception cref="RegistryException">
    /// Thrown if subkey does not exist.
    /// </exception>
    public IRegistryKey OpenSubKeyAsWritable(string subkey)
    {
        string newPath = $"{_path}\\{subkey}";
        if (!_keys.ContainsKey(newPath))
        {
            throw new RegistryException("Cannot open non-existent subkey.");
        }

        return new MockedRegistryKey(
            path: newPath,
            keys: _keys,
            values: _values,
            writable: true
        );
    }

    /// <summary>
    /// Set the registry value in the given key.
    /// </summary>
    /// <param name="valueName">
    /// The short name of the registry value.
    /// </param>
    /// <param name="value">
    /// The value to set.
    /// </param>
    public void SetValue(string valueName, object value)
    {
        string valuePath = $"{_path}\\{valueName}";
        _values[valuePath] = value;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
        }

        GC.SuppressFinalize(this);
    }
}
