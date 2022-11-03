namespace RegistryLib;

[Serializable]
public class RegistryException : Exception
{
    public RegistryException() { }

    public RegistryException(string message) : base(message) { }

    public RegistryException(string message, Exception inner) : base(message, inner) { }
}

public class DriveIcons
{
    private readonly IRegistry _registry;

    public DriveIcons(IRegistry registry)
    {
        _registry = registry;
    }

    public bool IconDefinedFor(string letter)
    {
        string error;

        const string keyString = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\DriveIcons";

        {
            using IRegistryKey driveKey = _registry.OpenSubKey(keyString);
            if (driveKey == null)
            {
                error = $"No key {keyString} is found in registry";
                throw new RegistryException(error);
            }

            if (!driveKey.GetSubKeyNames().Contains(letter))
            {
                return false;
            }

            string newKeyString = $"{keyString}\\{letter}";

            using IRegistryKey newKey = _registry.OpenSubKey(newKeyString);
            if (newKey == null)
            {
                error = $"Key {newKeyString} both does and does not exist.";
                throw new RegistryException(error);
            }

            return true;
        }
    }
}
