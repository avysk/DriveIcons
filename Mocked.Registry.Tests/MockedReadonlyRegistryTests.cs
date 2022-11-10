using Fi.Pentode.MockedRegistry;
using Fi.Pentode.Registry.Lib;

namespace Fi.Pentode.Mocked.Registry.Test
{
    public class MockedReadonlyRegistryTests
    {
        private readonly MockedRegistryKey _emptyLocalMachine;

        public MockedReadonlyRegistryTests()
        {
            const string localMachine = "localMachine";
            var emptyKeys = new Dictionary<string, string[]>()
            {
                { localMachine, Array.Empty<string>() }
            };
            Dictionary<string, object> emptyValues = new();
            _emptyLocalMachine = new MockedRegistryKey(
                path: localMachine,
                keys: emptyKeys,
                values: emptyValues
            );
        }

        [Fact]
        public void MockedRegistry_FreshRegistry_MustBeEmpty()
        {
            Assert.Empty(_emptyLocalMachine.SubKeyNames);
            Assert.Empty(_emptyLocalMachine.ValueNames);
        }

        [Fact]
        public void MockedRegisry_ReadonlyKey_ThrowsOnSubkeyCreation()
        {
            RegistryException exception = Assert.Throws<RegistryException>(
                () => _emptyLocalMachine.CreateSubKey("foo")
            );
            Assert.IsType<RegistryException>(exception);
            Assert.Equal(
                "Cannot create subkeys in a readonly key.",
                exception.Message
            );
        }
    }
}
