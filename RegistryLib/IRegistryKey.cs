namespace RegistryLib;

public interface IRegistryKey : IDisposable
{
    public IRegistryKey CreateSubKey(string key);

    public IRegistryKey? OpenSubKey(string key);

    public IRegistryKey? OpenSubKeyAsWritable(string name);

    public void DeleteSubKeyTree(string subkey);

    public string[] SubKeyNames { get; }

    public object? GetValue(string? name, object? defaultValue);

    public Microsoft.Win32.RegistryValueKind GetValueKind(string? name);

    public void SetValue(string? name, object value);

    public string[] ValueNames { get; }
}
