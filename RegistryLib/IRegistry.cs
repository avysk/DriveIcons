using System;

namespace RegistryLib;

public interface IRegistryKey : IDisposable
{
    public string[] GetSubKeyNames();
}

public interface IRegistry
{
    public IRegistryKey OpenSubKey(string key);
}
