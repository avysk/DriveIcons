using Microsoft.Win32;

namespace RegistryLib;

public class WindowsRegistryKey : IRegistryKey
{
    private readonly Microsoft.Win32.RegistryKey _registryKey;

    private bool _disposedValue;

    public WindowsRegistryKey(Microsoft.Win32.RegistryKey key) =>
        _registryKey = key;

    public IRegistryKey CreateSubKey(string key) =>
        new WindowsRegistryKey(_registryKey.CreateSubKey(key));

    public void DeleteSubKeyTree(string subkey) =>
        _registryKey.DeleteSubKeyTree(subkey, throwOnMissingSubKey: false);

    public IEnumerable<string> SubKeyNames => _registryKey.GetSubKeyNames();

    public object? GetValue(string? name, object? defaultValue) =>
        _registryKey.GetValue(name, defaultValue);

    public RegistryValueKind GetValueKind(string? name) =>
        _registryKey.GetValueKind(name);

    public IEnumerable<string> ValueNames => _registryKey.GetValueNames();

    public IRegistryKey? OpenSubKey(string key) =>
        new WindowsRegistryKey(_registryKey.OpenSubKey(key)!);

    public IRegistryKey? OpenSubKeyAsWritable(string key) =>
        new WindowsRegistryKey(_registryKey.OpenSubKey(key, writable: true)!);

    public void SetValue(string? name, object value) =>
        _registryKey.SetValue(name, value);

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
