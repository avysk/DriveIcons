namespace Fi.Pentode.Registry.Lib.Test;

public class RegistryExceptionTests
{
    [Fact]
    public void RegistryException_BasicConstructorIsCalleed_ItWorks()
    {
        var exception = new RegistryException();
        Assert.IsType<RegistryException>(exception);
    }

    [Fact]
    public void RegistryException_ConstuctorWithMessage_SetsMessage()
    {
        var exception = new RegistryException("foobar");
        Assert.IsType<RegistryException>(exception);
        Assert.Equal("foobar", exception.Message);
    }

    [Fact]
    public void RegistryException_ConstructorWithMessageAndCause_SetsBoth()
    {
        var inner = new Exception();
        var exception = new RegistryException("foobar", inner);
        Assert.IsType<RegistryException>(exception);
        Assert.Equal("foobar", exception.Message);
        Assert.True(ReferenceEquals(exception.InnerException, inner));
    }
}
