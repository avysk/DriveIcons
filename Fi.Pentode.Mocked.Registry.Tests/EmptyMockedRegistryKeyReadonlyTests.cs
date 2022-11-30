using Fi.Pentode.Registry.Lib;

namespace Fi.Pentode.Mocked.Registry.Test
{
    public class EmptyMockedReadonlyRegistryTests
    {
        private readonly MockedRegistryKey _emptyLocalMachine;

        public EmptyMockedReadonlyRegistryTests()
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

            // keys and values dictionaries are static
            Assert.Single(MockedRegistryKey.GetKeys());
            Assert.Empty(MockedRegistryKey.GetValues());
        }

        [Fact]
        public void MockedRegisry_ReadonlyKey_ThrowsOnSubkeyCreation()
        {
            RegistryException exception = Assert.Throws<RegistryException>(
                () => _emptyLocalMachine.CreateSubKey("foo")
            );
            Assert.IsType<RegistryException>(exception);
            Assert.Equal(
                "Cannot create subKeys in a readonly key.",
                exception.Message
            );
        }
    }
}
