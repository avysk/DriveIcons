using Metalama.Framework.Aspects;

namespace Fi.Pentode.Registry.Lib;

internal class CheckDiskAttribute : ContractAspect
{
    public override void Validate(dynamic? value)
    {
        char disk = (char)value!;
        char minDisk = DriveIcons.MinDisk;
        char maxDisk = DriveIcons.MaxDisk;
        if ((disk < minDisk) || (disk > maxDisk))
        {
            throw new RegistryException($"Disk {disk} is not in range.");
        }
    }
}
