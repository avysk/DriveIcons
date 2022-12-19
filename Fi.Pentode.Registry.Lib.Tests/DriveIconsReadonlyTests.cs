namespace Fi.Pentode.Registry.Lib.Tests;

using Fi.Pentode.Mocked.Registry;
using Ninject;

public sealed class DriveIconsReadonlyTests
{
    private const string LOCALMACHINE = "LocalMachine";

    private static readonly DriveIcons driveIcons;

    private static readonly List<char> wrongDisks =
        new() { (char)('A' - 1), (char)('Z' + 1) };

    private static readonly List<Action<char>> accessors =
        new()
        {
            disk =>
            {
                // read icon path for the disk
                _ = driveIcons![disk];
            },
            static disk =>
            {
                // write icon path for the disk
                driveIcons![disk] = "foo";
            },
            static disk =>
            {
                // delete icon path for the disk
                driveIcons![disk] = null;
            }
        };

    public static readonly IEnumerable<object[]> WrongDiskTestData =
        from disk in wrongDisks
        from action in accessors
        select new object[] { disk, () => action(disk) };

    public static readonly IEnumerable<object[]> AccessorTestData =
        from action in accessors
        select new object[] { () => action('T') };

#pragma warning disable S3963 // "static" fields should be initialized inline
    static DriveIconsReadonlyTests()
#pragma warning restore S4963 // "static" fields should be initialized inline
    {
        IKernel kernel = new StandardKernel();
        kernel
            .Bind<IRegistryKey>()
            .To<MockedRegistryKey>()
            .WithConstructorArgument("path", "localMachine")
            .WithConstructorArgument(
                "keys",
                new Dictionary<string, string[]>()
                {
                    { LOCALMACHINE, Array.Empty<string>() }
                }
            )
            .WithConstructorArgument(
                "values",
                new Dictionary<string, object>()
            );

        // Now we have a static instance of DriveIcons, using MockedRegistry.
        // The tests here are readonly so it can be static.
        driveIcons = kernel.Get<DriveIcons>();
    }

    [Theory]
    [MemberData(nameof(WrongDiskTestData))]
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
    [MemberData(nameof(AccessorTestData))]
    public void DriveIcons_NoRegistryKeyForTheDiskChar_AccessorsThrow(
        Action action
    )
    {
        var exception = Assert.Throws<RegistryException>(action);
        Assert.IsType<RegistryException>(exception);
        Assert.Equal("Cannot open non-existent subKey.", exception.Message);
    }
}
