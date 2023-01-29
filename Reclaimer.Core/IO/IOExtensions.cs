using System.IO;
using System.Numerics;

namespace Reclaimer.IO
{
    public static class IOExtensions
    {
        /// <summary>
        /// This is the same as <see cref="Stream.Read(byte[], int, int)"/> except it is guaranteed not read less than the specified
        /// number of bytes unless the end of stream has been reached.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <remarks>
        /// <see cref="Stream"/> implementations are not required to read all bytes in one operation. This method is a wrapper
        /// that continues reading until either all bytes have been read or the end of stream has been reached.
        /// </remarks>
        /// <inheritdoc cref="Stream.Read(byte[], int, int)"/>
        public static int ReadAll(this Stream stream, byte[] buffer, int offset, int count)
        {
            var totalBytes = 0;

            int bytesRead;
            do
            {
                bytesRead = stream.Read(buffer, offset, count);
                totalBytes += bytesRead;
                offset += bytesRead;
                count -= bytesRead;
            }
            while (bytesRead > 0 && count > 0);

            return totalBytes;
        }

        /// <summary>
        /// This is the same as <see cref="Stream.Read(Span{byte})"/> except it is guaranteed not read less than the specified
        /// number of bytes unless the end of stream has been reached.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <remarks>
        /// <see cref="Stream"/> implementations are not required to read all bytes in one operation. This method is a wrapper
        /// that continues reading until either all bytes have been read or the end of stream has been reached.
        /// </remarks>
        /// <inheritdoc cref="Stream.Read(Span{byte})"/>
        public static int ReadAll(this Stream stream, Span<byte> buffer)
        {
            var totalBytes = 0;

            int bytesRead;
            while (buffer.Length > 0 && (bytesRead = stream.Read(buffer)) > 0)
            {
                totalBytes += bytesRead;
                buffer = buffer[bytesRead..];
            }

            return totalBytes;
        }

        /// <summary>
        /// Reads two consecutive <see cref="float"/> values from the current stream that form the X and Y components of a <see cref="Vector2"/>.
        /// </summary>
        /// <param name="reader">The binary reader to use.</param>
        /// <returns/>
        /// <inheritdoc cref="BinaryReader.ReadSingle"/>
        public static Vector2 ReadVector2(this BinaryReader reader) => new Vector2(reader.ReadSingle(), reader.ReadSingle());

        /// <summary>
        /// Reads three consecutive <see cref="float"/> values from the current stream that form the X, Y and Z components of a <see cref="Vector3"/>.
        /// </summary>
        /// <inheritdoc cref="ReadVector2(BinaryReader)"/>
        public static Vector3 ReadVector3(this BinaryReader reader) => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        /// <summary>
        /// Reads four consecutive <see cref="float"/> values from the current stream that form the X, Y, Z and W components of a <see cref="Vector4"/>.
        /// </summary>
        /// <inheritdoc cref="ReadVector2(BinaryReader)"/>
        public static Vector4 ReadVector4(this BinaryReader reader) => new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        /// <summary>
        /// Reads four consecutive <see cref="float"/> values from the current stream that form the X, Y, Z and W components of a <see cref="Quaternion"/>.
        /// </summary>
        /// <inheritdoc cref="ReadVector2(BinaryReader)"/>
        public static Quaternion ReadQuaternion(this BinaryReader reader) => new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

        /// <summary>
        /// Reads nine consecutive <see cref="float"/> values from the current stream that form the first three columns of a <see cref="Matrix4x4"/>.
        /// </summary>
        /// <remarks>
        /// The components of the <see cref="Matrix4x4"/> are read in order from M11 to M33 except for M14 and M24. The translation component (row 4) is set to [0, 0, 0, 1].
        /// </remarks>
        /// <inheritdoc cref="ReadVector2(BinaryReader)"/>
        public static Matrix4x4 ReadMatrix3x3(this BinaryReader reader)
        {
            return new Matrix4x4(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                0, 0, 0, 1
            );
        }

        /// <summary>
        /// Reads twelve consecutive <see cref="float"/> values from the current stream that form the first three columns of a <see cref="Matrix4x4"/>.
        /// </summary>
        /// <remarks>
        /// The components of the <see cref="Matrix4x4"/> are read in order from M11 to M43 except for M14, M24 and M34 which are set to 0. M44 is set to 1.
        /// </remarks>
        /// <inheritdoc cref="ReadVector2(BinaryReader)"/>
        public static Matrix4x4 ReadMatrix3x4(this BinaryReader reader)
        {
            return new Matrix4x4(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0,
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1
            );
        }

        /// <summary>
        /// Reads sixteen consecutive <see cref="float"/> values from the current stream that form the components of a <see cref="Matrix4x4"/>.
        /// </summary>
        /// <remarks>
        /// The components of the <see cref="Matrix4x4"/> are read in order from M11 to M44.
        /// </remarks>
        /// <inheritdoc cref="ReadVector2(BinaryReader)"/>
        public static Matrix4x4 ReadMatrix4x4(this BinaryReader reader)
        {
            return new Matrix4x4(
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(),
                reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()
            );
        }

        /// <summary>
        /// Writes the X and Y components of a <see cref="Vector2"/> to the current stream as two consecutive <see cref="float"/> values.
        /// </summary>
        /// <param name="writer">The binary writer to use.</param>
        /// <param name="value">The two-dimensional vector value to write.</param>
        /// <inheritdoc cref="BinaryWriter.Write(float)"/>
        public static void Write(this BinaryWriter writer, Vector2 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        /// <summary>
        /// Writes the X, Y and Z components of a <see cref="Vector3"/> to the current stream as three consecutive <see cref="float"/> values.
        /// </summary>
        /// <param name="value">The three-dimensional vector value to write.</param>
        /// <inheritdoc cref="Write(BinaryWriter, Vector2)"/>
        public static void Write(this BinaryWriter writer, Vector3 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        /// <summary>
        /// Writes the X, Y, Z and W components of a <see cref="Vector4"/> to the current stream as four consecutive <see cref="float"/> values.
        /// </summary>
        /// <param name="value">The four-dimensional vector value to write.</param>
        /// <inheritdoc cref="Write(BinaryWriter, Vector2)"/>
        public static void Write(this BinaryWriter writer, Vector4 value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        /// <summary>
        /// Writes the X, Y, Z and W components of a <see cref="Quaternion"/> to the current stream as four consecutive <see cref="float"/> values.
        /// </summary>
        /// <param name="value">The quaternion value to write.</param>
        /// <inheritdoc cref="Write(BinaryWriter, Vector2)"/>
        public static void Write(this BinaryWriter writer, Quaternion value)
        {
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        /// <summary>
        /// Writes the first three columns of a <see cref="Matrix4x4"/> to the current stream as nine consecutive <see cref="float"/> values.
        /// </summary>
        /// <remarks>
        /// The components of the <see cref="Matrix4x4"/> are written in order from M11 to M33 except for M14 and M24. The translation component (row 4) is omitted.
        /// </remarks>
        /// <param name="value">The matrix value to write.</param>
        /// <inheritdoc cref="Write(BinaryWriter, Vector2)"/>
        public static void WriteMatrix3x3(this BinaryWriter writer, Matrix4x4 value)
        {
            writer.Write(value.M11);
            writer.Write(value.M12);
            writer.Write(value.M13);
            writer.Write(value.M21);
            writer.Write(value.M22);
            writer.Write(value.M23);
            writer.Write(value.M31);
            writer.Write(value.M32);
            writer.Write(value.M33);
        }

        /// <summary>
        /// Writes the first three columns of a <see cref="Matrix4x4"/> to the current stream as twelve consecutive <see cref="float"/> values.
        /// </summary>
        /// <remarks>
        /// The components of the <see cref="Matrix4x4"/> are written in order from M11 to M43 except for M14, M24 and M34.
        /// </remarks>
        /// <param name="value">The matrix value to write.</param>
        /// <inheritdoc cref="Write(BinaryWriter, Vector2)"/>
        public static void WriteMatrix3x4(this BinaryWriter writer, Matrix4x4 value)
        {
            writer.Write(value.M11);
            writer.Write(value.M12);
            writer.Write(value.M13);
            writer.Write(value.M21);
            writer.Write(value.M22);
            writer.Write(value.M23);
            writer.Write(value.M31);
            writer.Write(value.M32);
            writer.Write(value.M33);
            writer.Write(value.M41);
            writer.Write(value.M42);
            writer.Write(value.M43);
        }

        /// <summary>
        /// Writes the components of a <see cref="Matrix4x4"/> to the current stream as sixteen consecutive <see cref="float"/> values.
        /// </summary>
        /// <remarks>
        /// The components of the <see cref="Matrix4x4"/> are written in order from M11 to M44.
        /// </remarks>
        /// <param name="value">The matrix value to write.</param>
        /// <inheritdoc cref="Write(BinaryWriter, Vector2)"/>
        public static void WriteMatrix4x4(this BinaryWriter writer, Matrix4x4 value)
        {
            writer.Write(value.M11);
            writer.Write(value.M12);
            writer.Write(value.M13);
            writer.Write(value.M14);
            writer.Write(value.M21);
            writer.Write(value.M22);
            writer.Write(value.M23);
            writer.Write(value.M24);
            writer.Write(value.M31);
            writer.Write(value.M32);
            writer.Write(value.M33);
            writer.Write(value.M34);
            writer.Write(value.M41);
            writer.Write(value.M42);
            writer.Write(value.M43);
            writer.Write(value.M44);
        }
    }
}
