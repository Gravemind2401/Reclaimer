using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.Utilities
{
    public class EndianWriterEx : EndianWriter
    {
        private readonly Dictionary<Type, Action<object, double?>> registeredTypes;

        public EndianWriterEx(Stream input, ByteOrder byteOrder)
            : this(input, byteOrder, false)
        { }

        public EndianWriterEx(Stream input, ByteOrder byteOrder, bool leaveOpen)
            : base(input, byteOrder, leaveOpen)
        {
            registeredTypes = new Dictionary<Type, Action<object, double?>>();
        }

        protected EndianWriterEx(EndianWriterEx parent, long virtualOrigin)
            : base(parent, virtualOrigin)
        {
            ArgumentNullException.ThrowIfNull(parent);
            registeredTypes = parent.registeredTypes;
        }

        public void RegisterType<T>(Action<T, double?> writer)
        {
            if (registeredTypes.ContainsKey(typeof(T)))
                throw new ArgumentException(Utils.CurrentCulture($"{typeof(T).Name} has already been registered."));

            registeredTypes.Add(typeof(T), (obj, ver) => writer((T)obj, ver));
        }

        public override EndianWriter CreateVirtualWriter() => CreateVirtualWriter(BaseStream.Position);

        public override EndianWriter CreateVirtualWriter(long origin) => new EndianWriterEx(this, origin);

        protected override void WriteObjectGeneric<T>(T value, double? version)
        {
            if (value is IWriteable writeable)
                writeable.Write(this, version);
            else if (registeredTypes.TryGetValue(typeof(T), out var writeFunc))
                writeFunc(value, version);
            else
                base.WriteObjectGeneric(value, version);
        }
    }
}
