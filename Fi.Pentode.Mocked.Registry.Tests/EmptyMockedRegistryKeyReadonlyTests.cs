namespace Fi.Pentode.Mocked.Registry.Tests;

using Fi.Pentode.Registry.Lib;

public class EmptyMockedReadonlyRegistryTests
{
    private readonly MockedRegistryKey emptyLocalMachine;

    public EmptyMockedReadonlyRegistryTests()
    {
        const string localMachine = "localMachine";
        var emptyKeys = new Dictionary<string, string[]>()
        {
            { localMachine, Array.Empty<string>() }
        };
        Dictionary<string, object> emptyValues = new();
        this.emptyLocalMachine = new MockedRegistryKey(
            path: localMachine,
            keys: emptyKeys,
            values: emptyValues
        );
    }

    [Fact]
    public void MockedRegistry_FreshRegistry_MustBeEmpty()
    {
        Assert.Empty(this.emptyLocalMachine.SubKeyNames);
        Assert.Empty(this.emptyLocalMachine.ValueNames);

        // keys and values dictionaries are static
        Assert.Single(MockedRegistryKey.GetKeys());
        Assert.Empty(MockedRegistryKey.GetValues());
    }

    [Fact]
    public void MockedRegistry_ReadonlyKey_ThrowsOnSubkeyCreation()
    {
        RegistryException exception = Assert.Throws<RegistryException>(
            () => this.emptyLocalMachine.CreateSubKey("foo")
        );
        Assert.IsType<RegistryException>(exception);
        Assert.Equal(
            "Cannot create subKeys in a readonly key.",
            exception.Message
        );
    }
}
