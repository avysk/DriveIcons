namespace Fi.Pentode.Registry.Lib;

public interface IRegistryKey : IDisposable
{
    public IRegistryKey CreateSubKey(string key);

    public IRegistryKey? OpenSubKey(string key);

    public IRegistryKey? OpenSubKeyAsWritable(string key);

    public void DeleteSubKeyTree(string subkey);

    public IEnumerable<string> SubKeyNames { get; }

    public object? GetValue(string? name, object? defaultValue);

    public Microsoft.Win32.RegistryValueKind GetValueKind(string? name);

    public void SetValue(string? name, object value);

    public IEnumerable<string> ValueNames { get; }
}
