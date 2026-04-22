using Metalama.Framework.Aspects;

namespace Fi.Pentode.Registry.Lib;

internal sealed class CheckDiskAttribute : ContractAspect
{
    public override void Validate(dynamic? value)
    {
        var disk = (char)value!;
        const char minDisk = DriveIcons.MinDisk;
        const char maxDisk = DriveIcons.MaxDisk;
        if (disk is < minDisk or > maxDisk)
        {
            throw new RegistryException($"Disk {disk} is not in range.");
        }
    }
}
