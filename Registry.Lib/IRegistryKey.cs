namespace Fi.Pentode.Registry.Lib;

/// <summary>
/// The interface for working with registry.
/// </summary>
/// <remarks>
/// Do not use Windows registry directly - use wrapper class <see
/// cref="WindowsRegistryKey"/> implementing this interface. This allows
/// dependency injection and substitution of in-memory registry in tests.
/// Implementing classes should implement <see cref="IDisposable"/> as well.
/// </remarks>
public interface IRegistryKey : IDisposable
{
    public IRegistryKey CreateSubKey(string subkey);

    public IRegistryKey? OpenSubKey(string subkey);

    public IRegistryKey? OpenSubKeyAsWritable(string subkey);

    public void DeleteSubKeyTree(string subkey);

    public IEnumerable<string> SubKeyNames { get; }

    public object? GetValue(string valueName, object? defaultValue);

    public Microsoft.Win32.RegistryValueKind GetValueKind(string valueName);

    public void SetValue(string valueName, object value);

    public IEnumerable<string> ValueNames { get; }
}
