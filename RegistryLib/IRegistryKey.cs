namespace RegistryLib;

public interface IRegistryKey : IDisposable
{
    public IRegistryKey CreateSubKey(string key);

    public IRegistryKey? OpenSubKey(string key);

    public IRegistryKey? OpenSubKey(string name, bool writable);

    public void DeleteSubKeyTree(string subkey, bool throwOnMissingSubKey);

    public string[] GetSubKeyNames();

    public object? GetValue(string? name, object? defaultValue);

    public Microsoft.Win32.RegistryValueKind GetValueKind(string? name);

    public void SetValue(string? name, object value);

    public string[] GetValueNames();
}
