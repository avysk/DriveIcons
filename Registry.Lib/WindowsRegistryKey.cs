using Microsoft.Win32;

namespace Fi.Pentode.Registry.Lib;

public class WindowsRegistryKey : IRegistryKey
{
    private readonly Microsoft.Win32.RegistryKey _registryKey;

    private bool _disposedValue;

    public WindowsRegistryKey(Microsoft.Win32.RegistryKey key) =>
        _registryKey = key;

    public IRegistryKey CreateSubKey(string subkey) =>
        new WindowsRegistryKey(_registryKey.CreateSubKey(subkey));

    public void DeleteSubKeyTree(string subkey) =>
        _registryKey.DeleteSubKeyTree(subkey, throwOnMissingSubKey: false);

    public IEnumerable<string> SubKeyNames => _registryKey.GetSubKeyNames();

    public object? GetValue(string valueName, object? defaultValue) =>
        _registryKey.GetValue(valueName, defaultValue);

    public RegistryValueKind GetValueKind(string valueName) =>
        _registryKey.GetValueKind(valueName);

    public IEnumerable<string> ValueNames => _registryKey.GetValueNames();

    public IRegistryKey? OpenSubKey(string subkey) =>
        new WindowsRegistryKey(_registryKey.OpenSubKey(subkey)!);

    public IRegistryKey? OpenSubKeyAsWritable(string subkey)
    {
        return new WindowsRegistryKey(
            _registryKey.OpenSubKey(subkey, writable: true)!
        );
    }

    public void SetValue(string valueName, object value) =>
        _registryKey.SetValue(valueName, value);

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
