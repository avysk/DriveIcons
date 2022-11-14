using Fi.Pentode.MockedRegistry;

using Ninject;
using System.Collections;

namespace Fi.Pentode.Registry.Lib.Test;

public class DriveIconsReadonlyTest
{
    private const string _localMachine = "LocalMachine";
    public readonly DriveIcons _driveIcons;

    public class WrongDiskTestData : IEnumerable<object[]>
    {
        private readonly DriveIcons _driveIcons;

        public WrongDiskTestData()
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

        public IEnumerator<object[]> GetEnumerator()
        {
            foreach (
                var disk in new char[] { (char)('A' - 1), (char)('Z' + 1) }
            )
            {
                yield return new object[]
                {
                    disk,
                    () =>
                    {
                        _ = _driveIcons[disk];
                        return;
                    }
                };
                yield return new object[]
                {
                    disk,
                    () =>
                    {
                        _driveIcons[disk] = "foo";
                        return;
                    }
                };
                yield return new object[]
                {
                    disk,
                    () =>
                    {
                        _driveIcons[disk] = null;
                        return;
                    }
                };
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

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
    [ClassData(typeof(WrongDiskTestData))]
    public void DriveIcons_DiskCharIsNotInRange_AccessorsThrow(
        char disk,
        Action action
    )
    {
        var exception = Assert.Throws<RegistryException>(action);
        Assert.IsType<RegistryException>(exception);
        Assert.Equal($"Disk {disk} is not in range.", exception.Message);
    }
}
