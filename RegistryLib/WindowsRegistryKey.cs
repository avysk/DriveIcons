using Microsoft.Win32;

namespace RegistryLib;

public class WindowsRegistryKey : IRegistryKey
{
    private readonly Microsoft.Win32.RegistryKey _registryKey;

    private bool _disposedValue;

    public WindowsRegistryKey(Microsoft.Win32.RegistryKey key)
    {
        _registryKey = key;
    }

    public IRegistryKey CreateSubKey(string key)
    {
        return new WindowsRegistryKey(_registryKey.CreateSubKey(key));
    }

    public void DeleteSubKeyTree(string subkey, bool throwOnMissingSubKey)
    {
        _registryKey.DeleteSubKeyTree(subkey, throwOnMissingSubKey);
    }

    public string[] GetSubKeyNames()
    {
        return _registryKey.GetSubKeyNames();
    }

    public object? GetValue(string? name, object? defaultValue)
    {
        return _registryKey.GetValue(name, defaultValue);
    }

    public RegistryValueKind GetValueKind(string? name)
    {
        return _registryKey.GetValueKind(name);
    }

    public string[] GetValueNames()
    {
        return _registryKey.GetValueNames();
    }

    public IRegistryKey? OpenSubKey(string key)
    {
        return new WindowsRegistryKey(_registryKey.OpenSubKey(key)!);
    }

    public IRegistryKey? OpenSubKey(string name, bool writable)
    {
        return new WindowsRegistryKey(_registryKey.OpenSubKey(name, writable)!);
    }

    public void SetValue(string? name, object value)
    {
        _registryKey.SetValue(name, value);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue && disposing)
        {
            _registryKey.Dispose();
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
