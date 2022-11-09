using Fi.Pentode.MockedRegistry;

using Ninject;

namespace Fi.Pentode.Registry.Lib.Test;

public class UnitTest1
{
    private readonly IRegistryKey _registry;

    public UnitTest1()
    {
        IKernel kernel = new StandardKernel();
        kernel.Bind<IRegistryKey>().To<MockedRegistryKey>();
        _registry = kernel.Get<IRegistryKey>();
    }

    [Fact]
    public void Test1()
    {
        Assert.Equal(1, 1 + 0);
    }
}
