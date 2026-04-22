namespace Fi.Pentode.Registry.Lib.Tests;

public class RegistryExceptionTests
{
    [Fact]
    public void RegistryExceptionBasicConstructorIsCalledItWorks()
    {
        var exception = new RegistryException();
        Assert.IsType<RegistryException>(exception);
    }

    [Fact]
    public void RegistryExceptionConstructorWithMessageSetsMessage()
    {
        var exception = new RegistryException("foobar");
        Assert.IsType<RegistryException>(exception);
        Assert.Equal("foobar", exception.Message);
    }

    [Fact]
    public void RegistryExceptionConstructorWithMessageAndCauseSetsBoth()
    {
        var inner = new Exception();
        var exception = new RegistryException("foobar", inner);
        Assert.IsType<RegistryException>(exception);
        Assert.Equal("foobar", exception.Message);
        Assert.True(ReferenceEquals(exception.InnerException, inner));
    }
}
