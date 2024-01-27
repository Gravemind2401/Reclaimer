using Reclaimer.Geometry.Vectors;

namespace Reclaimer.Geometry.Utilities
{
    internal class VectorDescriptor
    {
        #region Inner Types

        public enum VectorDataType : byte
        {
            Real,
            Integer,
            Packed
        }

        public enum DescriptorFlags : byte
        {
            None = 0,
            Normalized = 1 << 0,
            SignExtended = 1 << 1,
            SignShifted = 1 << 2,
        }

        public readonly record struct VectorDimension(DescriptorFlags Flags, byte BitCount);

        #endregion

        private static readonly Dictionary<Type, VectorDescriptor> descriptorCache = new();

        public VectorDataType DataType { get; }
        public byte Size { get; } // size in bytes (in total if packed, else per dimension)
        public VectorDimension[] Configuration { get; }

        private VectorDescriptor(VectorDataType dataType, byte size, byte count, DescriptorFlags flags = default, params byte[] packBits)
        {
            //currently only providing one set of flags, but replicating it for each vector dimension
            //this is for future proofing in case of a packed vector where each dimension could have a different packing strategy

            if (dataType == VectorDataType.Packed && packBits?.Length != count)
                throw new ArgumentException($"{nameof(packBits)} array length does not match {nameof(count)}");
            else if (dataType != VectorDataType.Packed && packBits?.Length > 0)
                throw new ArgumentException($"{nameof(packBits)} array should not be provided for data type of {dataType}");

            DataType = dataType;
            Size = size;

            Configuration = packBits?.Length > 0
                ? packBits.Select(i => new VectorDimension(flags, i)).ToArray()
                : Enumerable.Repeat(new VectorDimension(flags, (byte)(size * 8)), count).ToArray();
        }

        public static VectorDescriptor FromType(Type vectorType)
        {
            if (vectorType == null)
                return null;

            if (descriptorCache.TryGetValue(vectorType, out var descriptor))
                return descriptor;

            const DescriptorFlags SNorm = DescriptorFlags.Normalized | DescriptorFlags.SignExtended;
            const DescriptorFlags UNorm = DescriptorFlags.Normalized;

            return descriptorCache[vectorType] = vectorType switch
            {
                //float32
                _ when vectorType == typeof(RealVector2) => new VectorDescriptor(VectorDataType.Real, 4, 2),
                _ when vectorType == typeof(RealVector3) => new VectorDescriptor(VectorDataType.Real, 4, 3),
                _ when vectorType == typeof(RealVector4) => new VectorDescriptor(VectorDataType.Real, 4, 4),

                //TODO: read float16 in python?
                //_ when vectorType == typeof(HalfVector2) => new VectorDescriptor(VectorDataType.Real, 2, 2),
                //_ when vectorType == typeof(HalfVector3) => new VectorDescriptor(VectorDataType.Real, 2, 3),
                //_ when vectorType == typeof(HalfVector4) => new VectorDescriptor(VectorDataType.Real, 2, 4),

                //int16
                _ when vectorType == typeof(Int16N2) => new VectorDescriptor(VectorDataType.Integer, 2, 2, SNorm),
                _ when vectorType == typeof(Int16N4) => new VectorDescriptor(VectorDataType.Integer, 2, 4, SNorm),
                _ when vectorType == typeof(UInt16N2) => new VectorDescriptor(VectorDataType.Integer, 2, 2, UNorm),
                _ when vectorType == typeof(UInt16N4) => new VectorDescriptor(VectorDataType.Integer, 2, 4, UNorm),
                _ when vectorType == typeof(UShort2) => new VectorDescriptor(VectorDataType.Integer, 2, 2),

                //int8
                _ when vectorType == typeof(ByteN2) => new VectorDescriptor(VectorDataType.Integer, 1, 2, SNorm),
                _ when vectorType == typeof(ByteN4) => new VectorDescriptor(VectorDataType.Integer, 1, 4, SNorm),
                _ when vectorType == typeof(UByteN2) => new VectorDescriptor(VectorDataType.Integer, 1, 2, UNorm),
                _ when vectorType == typeof(UByteN4) => new VectorDescriptor(VectorDataType.Integer, 1, 4, UNorm),
                _ when vectorType == typeof(UByte4) => new VectorDescriptor(VectorDataType.Integer, 1, 4),

                //packed signed
                _ when vectorType == typeof(DecN4) => new VectorDescriptor(VectorDataType.Packed, 4, 4, SNorm, 10, 10, 10, 2),
                _ when vectorType == typeof(DHenN3) => new VectorDescriptor(VectorDataType.Packed, 4, 3, SNorm, 10, 11, 11),
                _ when vectorType == typeof(HenDN3) => new VectorDescriptor(VectorDataType.Packed, 4, 3, SNorm, 11, 11, 10),

                //packed unsigned
                _ when vectorType == typeof(UDecN4) => new VectorDescriptor(VectorDataType.Packed, 4, 4, UNorm, 10, 10, 10, 2),
                _ when vectorType == typeof(UDHenN3) => new VectorDescriptor(VectorDataType.Packed, 4, 3, UNorm, 10, 11, 11),
                _ when vectorType == typeof(UHenDN3) => new VectorDescriptor(VectorDataType.Packed, 4, 3, UNorm, 11, 11, 10),

                _ => null
            };
        }
    }
}