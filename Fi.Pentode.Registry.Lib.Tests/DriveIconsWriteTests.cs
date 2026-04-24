namespace Fi.Pentode.Registry.Lib.Tests;

using Microsoft.Win32;

public sealed class DriveIconsWriteTests
{
    [Fact]
    public void DriveIconsSetterWritesValueUsingWritableTraversal()
    {
        var rootKey = TrackingRegistryKey.CreateWithDriveIconsPath();
        var driveIcons = new DriveIcons(rootKey);

        driveIcons['T'] = @"C:\Icons\drive.ico";

        Assert.Equal(
            @"C:\Icons\drive.ico",
            rootKey.GetValue(
                "SOFTWARE",
                "Microsoft",
                "Windows",
                "CurrentVersion",
                "Explorer",
                "DriveIcons",
                "T",
                "DefaultIcon",
                string.Empty
            )
        );
    }

    [Fact]
    public void DriveIconsSetterNullDeletesDriveIconTreeUsingWritableTraversal()
    {
        var rootKey = TrackingRegistryKey.CreateWithDriveIconsPath();
        var driveIcons = new DriveIcons(rootKey);
        driveIcons['T'] = @"C:\Icons\drive.ico";

        driveIcons['T'] = null;

        Assert.False(
            rootKey.ContainsSubKey(
                "SOFTWARE",
                "Microsoft",
                "Windows",
                "CurrentVersion",
                "Explorer",
                "DriveIcons",
                "T"
            )
        );
    }

    private sealed class TrackingRegistryKey : IRegistryKey
    {
        private readonly Node _node;
        private readonly bool _writable;
        private bool _disposed;

        private TrackingRegistryKey(Node node, bool writable)
        {
            _node = node;
            _writable = writable;
        }

        public IEnumerable<string> SubKeyNames
        {
            get
            {
                ThrowIfDisposed();
                return _node.SubKeys.Keys.ToArray();
            }
        }

        public IEnumerable<string> ValueNames
        {
            get
            {
                ThrowIfDisposed();
                return _node.Values.Keys.ToArray();
            }
        }

        public static TrackingRegistryKey CreateWithDriveIconsPath()
        {
            var root = new Node();
            Node current = root;
            foreach (string subKey in new[]
            {
                "SOFTWARE",
                "Microsoft",
                "Windows",
                "CurrentVersion",
                "Explorer",
                "DriveIcons"
            })
            {
                var next = new Node();
                current.SubKeys[subKey] = next;
                current = next;
            }

            return new TrackingRegistryKey(root, writable: true);
        }

        public IRegistryKey CreateSubKey(string subKey)
        {
            ThrowIfDisposed();
            EnsureWritable();

            if (!_node.SubKeys.TryGetValue(subKey, out Node? child))
            {
                child = new Node();
                _node.SubKeys[subKey] = child;
            }

            return new TrackingRegistryKey(child, writable: true);
        }

        public void DeleteSubKeyTree(string subKey)
        {
            ThrowIfDisposed();
            EnsureWritable();
            _node.SubKeys.Remove(subKey);
        }

        public object? GetValue(string valueName, object? defaultValue)
        {
            ThrowIfDisposed();
            return _node.Values.TryGetValue(valueName, out object? value) ? value : defaultValue;
        }

        public RegistryValueKind GetValueKind(string valueName)
        {
            ThrowIfDisposed();
            return _node.Values[valueName] is string
                ? RegistryValueKind.String
                : RegistryValueKind.Unknown;
        }

        public IRegistryKey? OpenSubKey(string subKey)
        {
            ThrowIfDisposed();
            return _node.SubKeys.TryGetValue(subKey, out Node? child)
                ? new TrackingRegistryKey(child, writable: false)
                : null;
        }

        public IRegistryKey? OpenSubKeyAsWritable(string subKey)
        {
            ThrowIfDisposed();
            return _node.SubKeys.TryGetValue(subKey, out Node? child)
                ? new TrackingRegistryKey(child, writable: true)
                : null;
        }

        public void SetValue(string valueName, object value)
        {
            ThrowIfDisposed();
            EnsureWritable();
            _node.Values[valueName] = value;
        }

        public bool ContainsSubKey(params string[] path)
        {
            Node? current = Traverse(path);
            return current != null;
        }

        public object? GetValue(params string[] pathAndValueName)
        {
            string[] path = pathAndValueName[..^1];
            string valueName = pathAndValueName[^1];
            Node? node = Traverse(path);
            return node?.Values.TryGetValue(valueName, out object? value) == true ? value : null;
        }

        public void Dispose() => _disposed = true;

        private Node? Traverse(IEnumerable<string> path)
        {
            Node current = _node;
            foreach (string part in path)
            {
                if (!current.SubKeys.TryGetValue(part, out Node? next))
                {
                    return null;
                }

                current = next;
            }

            return current;
        }

        private void EnsureWritable()
        {
            if (!_writable)
            {
                throw new InvalidOperationException("Key is not writable.");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(TrackingRegistryKey));
            }
        }

        private sealed class Node
        {
            public Dictionary<string, Node> SubKeys { get; } = new();

            public Dictionary<string, object> Values { get; } = new();
        }
    }
}
