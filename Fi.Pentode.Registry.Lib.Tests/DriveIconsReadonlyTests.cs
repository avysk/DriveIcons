using Fi.Pentode.Mocked.Registry;
using Ninject;

namespace Fi.Pentode.Registry.Lib.Test;

public class DriveIconsReadonlyTests
{
    private const string _localMachine = "LocalMachine";
    private static readonly DriveIcons _driveIcons;

    private static readonly List<char> _wrongDisks =
        new() { (char)('A' - 1), (char)('Z' + 1) };

    private static readonly List<Action<char>> _accessors =
        new()
        {
            (char disk) =>
            {
                // read icon path for the disk
                _ = _driveIcons![disk];
                return;
            },
            (char disk) =>
            {
                // write icon path for the disk
                _driveIcons![disk] = "foo";
                return;
            },
            (char disk) =>
            {
                // delete icon path for the disk
                _driveIcons![disk] = null;
                return;
            }
        };

    public readonly static IEnumerable<object[]> _wrongDiskTestData =
        from disk in _wrongDisks
        from action in _accessors
        select new object[] { disk, () => action(disk) };
    public readonly static IEnumerable<object[]> _accessorTestData =
        from action in _accessors
        select new object[] { () => action('T') };

    static DriveIconsReadonlyTests()
    {
        IKernel kernel = new StandardKernel();
        kernel
            .Bind<IRegistryKey>()
            .To<MockedRegistryKey>()
            .WithConstructorArgument("path", "localMachine")
            .WithConstructorArgument(
                "keys",
                new Dictionary<string, string[]>
                {
                    { _localMachine, Array.Empty<string>() }
                }
            )
            .WithConstructorArgument(
                "values",
                new Dictionary<string, object>()
            );

        // Now we have a static instance of DriveIcons, using MockedRegistry.
        // The tests here are readonly so it can be static.
        _driveIcons = kernel.Get<DriveIcons>();
    }

    [Theory]
    [MemberData(nameof(_wrongDiskTestData))]
    public void DriveIcons_DiskCharIsNotInRange_AccessorsThrow(
        char disk,
        Action action
    )
    {
        var exception = Assert.Throws<RegistryException>(action);
        Assert.IsType<RegistryException>(exception);
        Assert.Equal($"Disk {disk} is not in range.", exception.Message);
    }

    [Theory]
    [MemberData(nameof(_accessorTestData))]
    public void DriveIcons_NoRegistryKeyForTheDiskChar_AccessorsThrow(
        Action action
    )
    {
        var exception = Assert.Throws<RegistryException>(action);
        Assert.IsType<RegistryException>(exception);
        Assert.Equal("Cannot open non-existent subKey.", exception.Message);
    }
}
