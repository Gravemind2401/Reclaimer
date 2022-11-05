using Reclaimer.IO.Dynamic;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.IO
{
    public partial class EndianReader : BinaryReader
    {
        #region ReadObject Overloads

        /// <inheritdoc cref="ReadObject{T}(double)"/>
        public T ReadObject<T>() => (T)ReadObject(null, typeof(T), null);

        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <inheritdoc cref="ReadObject(Type, double)"/>
        public T ReadObject<T>(double version) => (T)ReadObject(null, typeof(T), version);

        /// <inheritdoc cref="ReadObject(Type, double)"/>
        public object ReadObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return ReadObject(null, type, null);
        }

        /// <summary>
        /// Reads a complex object from the current stream using reflection.
        /// </summary>
        /// <remarks>
        /// The type being read must have a public parameterless constructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </remarks>
        /// <param name="type">The type of object to read.</param>
        /// <returns>A new instance of the specified type whose values have been populated from the current stream.</returns>
        /// <inheritdoc cref="ReadObject(object, double)"/>
        public object ReadObject(Type type, double version)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return ReadObject(null, type, version);
        }

        /// <inheritdoc cref="ReadObject{T}(T, double)"/>
        public T ReadObject<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return (T)ReadObject(instance, instance.GetType(), null);
        }

        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <inheritdoc cref="ReadObject(object, double)"/>
        public T ReadObject<T>(T instance, double version)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return (T)ReadObject(instance, instance.GetType(), version);
        }

        /// <inheritdoc cref="ReadObject(object, double)"/>
        public object ReadObject(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return ReadObject(instance, instance.GetType(), null);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// </summary>
        /// <remarks>
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </remarks>
        /// <returns>The same object that was supplied as the <paramref name="instance"/> parameter.</returns>
        /// <param name="instance">The object to populate.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// <para>
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// </para>
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(object instance, double version)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return ReadObject(instance, instance.GetType(), version);
        }

        #endregion

        /// <summary>
        /// This function is called by all public ReadObject overloads.
        /// </summary>
        /// <param name="instance">The object to populate. This value will be null if no instance was provided.</param>
        /// <param name="type">The type of object to read.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// This value will be null if no version was provided.
        /// </param>
        protected virtual object ReadObject(object instance, Type type, double? version)
        {
            //cannot detect string type automatically (fixed/prefixed/terminated)
            if (type.Equals(typeof(string)))
                throw Exceptions.NotValidForStringTypes();

            if (instance == null)
                instance = CreateInstance(type, version);

            return TypeConfiguration.Populate(instance, type, this, version);
        }

        protected virtual object CreateInstance(Type type, double? version) => Activator.CreateInstance(type);
    }
}
