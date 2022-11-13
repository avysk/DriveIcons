using Fi.Pentode.MockedRegistry;

using Ninject;

namespace Fi.Pentode.Registry.Lib.Test;

public class DriveIconsReadonlyTest
{
    private const string _localMachine = "LocalMachine";
    private readonly DriveIcons _driveIcons;

    private class TestDataGenerator :

    private const char _smallDisk = (char)('A' - 1);
    private const char _largeDisk = (char)('Z' + 1);

    public DriveIconsReadonlyTest()
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
        _driveIcons = kernel.Get<DriveIcons>();
    }

    [Theory]
    [InlineData(
        _smallDisk,
        (Action<char>)((disk) =>
        {
            {
                _ = _driveIcons[disk];
                return;
            }
        })
    )]
    [InlineData(
        _largeDisk,
        (disk) =>
        {
            (char disk) => _ = _driveIcons[disk];
        }
    )]
    public void DriveIcons_DiskCharIsNotInRange_AccessorsThrow(
        char disk,
        Func<char, Action> useDisk
    )
    {
        var code = useDisk(disk);
        var exception = Assert.Throws<RegistryException>(code);
        Assert.IsType<RegistryException>(exception);
        Assert.Equal($"Disk {disk} is not in range.", exception.Message);
    }

    [Fact]
    public void DriveIcons_DiskCharIsTooSmall_WritingPathThrows()
    {
        var exception = Assert.Throws<RegistryException>(
            () => _driveIcons[_smallDisk] = "foo"
        );
        ;
        Assert.IsType<RegistryException>(exception);
        Assert.Equal($"Disk {_smallDisk} is not in range.", exception.Message);
    }

    [Fact]
    public void DriveIcons_DiskCharIsTooSmall_DeletingPathThrows()
    {
        var exception = Assert.Throws<RegistryException>(
            () => _driveIcons[_smallDisk] = null
        );
        ;
        Assert.IsType<RegistryException>(exception);
        Assert.Equal($"Disk {_smallDisk} is not in range.", exception.Message);
    }

    [Fact]
    public void DriveIcons_DiskCharIsTooLarge_ReadingPathThrows()
    {
        var exception = Assert.Throws<RegistryException>(
            () => _driveIcons[_largeDisk]
        );
        Assert.IsType<RegistryException>(exception);
        Assert.Equal($"Disk {_largeDisk} is not in range.", exception.Message);
    }

    [Fact]
    public void DriveIcons_DiskCharIsTooLarge_WritingPathThrows()
    {
        var exception = Assert.Throws<RegistryException>(
            () => _driveIcons[_largeDisk] = "foo"
        );
        ;
        Assert.IsType<RegistryException>(exception);
        Assert.Equal($"Disk {_largeDisk} is not in range.", exception.Message);
    }

    [Fact]
    public void DriveIcons_DiskCharIsTooLarge_DeletingPathThrows()
    {
        var exception = Assert.Throws<RegistryException>(
            () => _driveIcons[_largeDisk] = null
        );
        ;
        Assert.IsType<RegistryException>(exception);
        Assert.Equal($"Disk {_largeDisk} is not in range.", exception.Message);
    }
}
