namespace Fi.Pentode.Mocked.Registry;

using Fi.Pentode.Registry.Lib;
using Microsoft.Win32;

/// <summary>
/// In-memory "registry" for testing.
/// </summary>
public sealed class MockedRegistryKey : IRegistryKey
{
    private static Dictionary<string, string[]> keys = new();

    private static Dictionary<string, object> values = new();

    private readonly string path;

    private readonly bool writable;

    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockedRegistryKey"/> class.
    /// In-memory "registry" for testing.
    /// </summary>
    /// <param name="path">Path of selected key.</param>
    /// <param name="keys">The dictionary, mapping registry key names to arrays
    /// of subKey and value names.</param>
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
        this.path = path;
        MockedRegistryKey.keys = keys;
        MockedRegistryKey.values = values;
        this.writable = writable;
    }

    /// <summary>
    /// Get the dictionary of defined "registry keys".
    /// </summary>
    /// <returns>
    /// Dictionary keys are "full names of registry keys" without trailing \,
    /// values are arrays of full names of subKeys and full names of "registry
    /// values".
    /// </returns>
    public static Dictionary<string, string[]> GetKeys()
    {
        return keys;
    }

    /// <summary>
    /// Get dictionary of defined "registry values".
    /// </summary>
    /// <returns>
    /// Dictionary keys are "full names of registry values", dictionary values
    /// are their values.
    /// </returns>
    public static Dictionary<string, object> GetValues()
    {
        return values;
    }

    /// <summary>
    /// Gets values in this key.
    /// </summary>
    /// <value>
    /// The names of the values in the key.
    /// </value>
    public IEnumerable<string> ValueNames
    {
        get
        {
            return keys[this.path].Where(
                static value => values.ContainsKey(value)
            );
        }
    }

    /// <summary>
    /// Gets the names of subKeys of this key.
    /// </summary>
    /// <value>
    /// The names of the subKeys in the key.
    /// </value>
    public IEnumerable<string> SubKeyNames
    {
        get
        {
            return keys[this.path].Where(static key => keys.ContainsKey(key));
        }
    }

    /// <summary>
    /// Get the subKey of this key.
    /// </summary>
    /// <remarks>
    /// If the subKey does not exist, the empty subKey is created first.
    /// The returned subKey is writable.
    /// </remarks>
    /// <param name="subKey">
    /// The short name of the requested subKey.
    /// </param>
    /// <returns>
    /// <see cref="MockedRegistryKey"/> corresponding to the writable
    /// subKey of the key.
    /// </returns>
    /// <exception cref="RegistryException">
    /// Thrown if the key is not writable.
    /// </exception>
    public IRegistryKey CreateSubKey(string subKey)
    {
        if (!this.writable)
        {
            throw new RegistryException(
                "Cannot create subKeys in a readonly key."
            );
        }

        string newPath = $"{this.path}\\{subKey}";

        if (!keys.ContainsKey(newPath))
        {
            // create new empty key
            keys.Add(newPath, Array.Empty<string>());
        }

        return new MockedRegistryKey(newPath, keys, values, true);
    }

    /// <summary>
    /// Deletes key tree for the given subKey.
    /// </summary>
    /// <remarks>
    /// Associated values are not deleted.
    /// </remarks>
    /// <param name="subKey">
    /// Starting subKey for tree to delete.
    /// </param>
    /// <exception cref="RegistryException">
    /// Thrown at attempt to delete subtree in a readonly key.
    /// </exception>
    public void DeleteSubKeyTree(string subKey)
    {
        if (!this.writable)
        {
            throw new RegistryException(
                "Cannot delete subtree in a readonly key."
            );
        }

        string treeRoot = $"{this.path}\\{subKey}";

        var badKeys = (
            from key in keys.Keys.Where(
                k => k.StartsWith(treeRoot, StringComparison.Ordinal)
            )
            let wasInDictionary = keys.Remove(key)
            where !wasInDictionary
            select key
        ).ToArray();

        if (badKeys.Any())
        {
            var badKey = badKeys[0];
            throw new RegistryException(
                $"Key {badKey} was supposed be in "
                    + "the registry but it was not."
            );
        }
    }

    /// <summary>
    /// Get value from "registry".
    /// </summary>
    /// <param name="valueName">
    /// Short name of the value in the current key.
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
        if (!values.ContainsKey(newPath))
        {
            return defaultValue;
        }

        return values[newPath];
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
        object? value = this.GetValue(valueName, null);
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
    /// Open the subKey of the key.
    /// </summary>
    /// <remarks>
    /// Opens the subKey as readonly.
    /// </remarks>
    /// <param name="subKey">
    /// The name of the subKey to open.
    /// </param>
    /// <returns>
    /// <see cref="MockedRegistryKey"/> corresponding to the requested subKey.
    /// </returns>
    /// <exception cref="RegistryException">
    /// Thrown if subKey does not exist.
    /// </exception>
    public IRegistryKey OpenSubKey(string subKey)
    {
        string newPath = $"{this.path}\\{subKey}";
        if (!keys.ContainsKey(newPath))
        {
            throw new RegistryException("Cannot open non-existent subKey.");
        }

        return new MockedRegistryKey(newPath, keys, values);
    }

    /// <summary>
    /// Open the subKey of the key.
    /// </summary>
    /// <remarks>
    /// Opens the subKey as writable.
    /// </remarks>
    /// <param name="subKey">
    /// The name of the subKey to open.
    /// </param>
    /// <returns>
    /// <see cref="MockedRegistryKey"/> corresponding to the requested subKey.
    /// </returns>
    /// <exception cref="RegistryException">
    /// Thrown if subKey does not exist.
    /// </exception>
    public IRegistryKey OpenSubKeyAsWritable(string subKey)
    {
        string newPath = $"{this.path}\\{subKey}";
        if (!keys.ContainsKey(newPath))
        {
            throw new RegistryException("Cannot open non-existent subKey.");
        }

        return new MockedRegistryKey(newPath, keys, values, true);
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
        string valuePath = $"{this.path}\\{valueName}";
        values[valuePath] = value;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!this.disposedValue)
        {
            this.disposedValue = true;
        }
    }
}
