using RegistryLib;

using System.Diagnostics;

namespace MockedRegistry;

internal class MockedRegistryKey : IRegistryKey
{
    private bool _disposedValue;

    private readonly string _path;

    private readonly bool _writable;

    /// <summary>
    /// This field contains in-memory "registry" for testing. Dictionary keys
    /// are "registry keys" without trailing \, values are either other keys or
    /// full names of "registry values". The value of
    /// MockedRegistryKey.
    /// </summary>
    public static Dictionary<string, string[]> Keys = new();

    /// <summary>
    /// This field contains in-memory "registry" for testing. Dictionary keys
    /// are "names of registry values", dictionary values are their values.
    /// </summary>
    public static Dictionary<string, object> Values = new();

    public MockedRegistryKey(
        string path,
        Dictionary<string, string[]> keys,
        Dictionary<string, object> values,
        bool writable = false
    )
    {
        _path = path;
        Keys = keys;
        Values = values;
        _writable = writable;
    }

    public IEnumerable<string> ValueNames
    {
        get { return Keys[_path].Where(value => Values.ContainsKey(value)); }
    }

    public IEnumerable<string> SubKeyNames
    {
        get { return Keys[_path].Where(key => Keys.ContainsKey(key)); }
    }

    public IRegistryKey CreateSubKey(string key)
    {
        Debug.Assert(_writable);
        string newPath = $"{_path}\\{key}";
        // create new empty key
        Keys.Add(newPath, Array.Empty<string>());
        return new MockedRegistryKey(
            path: newPath,
            keys: Keys,
            values: Values,
            writable: true
        );
    }

    /// <summary>
    /// Deletes key tree for the given subkey. Notice that associated values
    /// are not deleted.
    /// </summary>
    /// <param name="subkey">Starting subkey for tree to delete.</param>
    public void DeleteSubKeyTree(string subkey)
    {
        Debug.Assert(_writable);
        string treeRoot = $"{_path}\\{subkey}";
        string[] keysToRemove = Array.Empty<string>();
        foreach (var key in Keys.Keys)
        {
            if (key.StartsWith(treeRoot))
            {
                _ = keysToRemove.Append(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            bool wasInDictionary = Keys.Remove(key);
            Debug.Assert(wasInDictionary);
        }
    }

    public object? GetValue(string? name, object? defaultValue)
    {
        Debug.Assert(name != null);
        const string newPath = "{_path}\\{name}";
        Debug.Assert(Keys[_path].Contains(newPath));
        return Values[newPath];
    }

    public Microsoft.Win32.RegistryValueKind GetValueKind(string? name)
    {
        object? value = GetValue(name, null);
        if (value is string)
        {
            return Microsoft.Win32.RegistryValueKind.String;
        }
        else if (value == null)
        {
            return Microsoft.Win32.RegistryValueKind.None;
        }
        else
        {
            return Microsoft.Win32.RegistryValueKind.Unknown;
        }
    }

    public IRegistryKey? OpenSubKey(string key)
    {
        string newPath = $"{_path}\\{key}";
        Debug.Assert(Keys.ContainsKey(newPath));
        return new MockedRegistryKey(path: newPath, keys: Keys, values: Values);
    }

    public IRegistryKey? OpenSubKeyAsWritable(string key)
    {
        string newPath = $"{_path}\\{key}";
        Debug.Assert(Keys.ContainsKey(newPath));
        return new MockedRegistryKey(
            path: newPath,
            keys: Keys,
            values: Values,
            writable: true
        );
    }

    public void SetValue(string? name, object value)
    {
        string valuePath = $"{_path}\\{name}";
        Values[valuePath] = value;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
