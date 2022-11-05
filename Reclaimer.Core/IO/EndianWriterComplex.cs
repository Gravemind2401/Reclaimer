using Reclaimer.IO.Dynamic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO
{
    public partial class EndianWriter : BinaryWriter
    {
        #region WriteObject Overloads

        /// <inheritdoc cref="WriteObject{T}(T, double)"/>
        public void WriteObject<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteObject(value, null);
        }

        /// <typeparam name="T">The type of object to write.</typeparam>
        /// <inheritdoc cref="WriteObject(object, double)"/>
        public void WriteObject<T>(T value, double version)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteObject(value, (double?)version);
        }

        /// <inheritdoc cref="WriteObject(object, double)"/>
        public void WriteObject(object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteObject(value, null);
        }

        /// <summary>
        /// Writes a complex object to the current stream using reflection.
        /// </summary>
        /// <remarks>
        /// The type being written must have a public parameterless constructor.
        /// Each property to be written must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </remarks>
        /// <param name="value">The object to write.</param>
        /// <param name="version">
        /// The version that should be used to store the object.
        /// <para>
        /// This determines which properties will be written, how they will be
        /// written and at what location in the stream to write them to.
        /// </para>
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public void WriteObject(object value, double version)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            WriteObject(value, (double?)version);
        }

        #endregion

        /// <summary>
        /// This function is called by all public WriteObject overloads.
        /// </summary>
        /// <param name="value">The object to write.</param>
        /// <param name="version">
        /// The version that should be used to store the object.
        /// This determines which properties will be written, how they will be
        /// written and at what location in the stream to write them to.
        /// </param>
        protected virtual void WriteObject(object value, double? version)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            //cannot detect string type automatically (fixed/prefixed/terminated)
            var type = value.GetType();
            if (type.Equals(typeof(string)))
                throw Exceptions.NotValidForStringTypes();

            TypeConfiguration.Write(value, this, version);
        }
    }
}
