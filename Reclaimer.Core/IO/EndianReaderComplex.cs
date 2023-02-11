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

        /// <summary>
        /// Reads a complex object from the current stream using reflection.
        /// The type being read must have a public parameterless conustructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public T ReadObject<T>() => (T)ReadObject(null, typeof(T), null);

        /// <summary>
        /// Reads a complex object from the current stream using reflection.
        /// The type being read must have a public parameterless conustructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public T ReadObject<T>(double version) => (T)ReadObject(null, typeof(T), version);

        /// <summary>
        /// Reads a complex object from the current stream using reflection.
        /// The type being read must have a public parameterless conustructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <param name="type">The type of object to read.</param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return ReadObject(null, type, null);
        }

        /// <summary>
        /// Reads a complex object from the current stream using reflection.
        /// The type being read must have a public parameterless conustructor.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <param name="type">The type of object to read.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(Type type, double version)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return ReadObject(null, type, version);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// The object returned is the same instance that was supplied as the parameter.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="instance">The object to populate.</param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public T ReadObject<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return (T)ReadObject(instance, instance.GetType(), null);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <typeparam name="T">The type of object to read.</typeparam>
        /// <param name="instance">The object to populate.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// The object returned is the same instance that was supplied as the parameter.
        /// </param>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public T ReadObject<T>(T instance, double version)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return (T)ReadObject(instance, instance.GetType(), version);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// The object returned is the same instance that was supplied as the parameter.
        /// </summary>
        /// <exception cref="AmbiguousMatchException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidCastException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="MissingMethodException" />
        public object ReadObject(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            return ReadObject(instance, instance.GetType(), null);
        }

        /// <summary>
        /// Populates the properties of a complex object from the current stream using reflection.
        /// Each property to be read must have public get/set methods and
        /// must have at least the <seealso cref="OffsetAttribute"/> attribute applied.
        /// </summary>
        /// <param name="instance">The object to populate.</param>
        /// <param name="version">
        /// The version that was used to store the object.
        /// This determines which properties will be read, how they will be
        /// read and at what location in the stream to read them from.
        /// The object returned is the same instance that was supplied as the parameter.
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

            //take note of origin before creating instance in case a derived class moves the stream.
            //this is important for attributes like FixedSizeAttribute to ensure the final position is correct.
            var origin = Position;
            instance ??= CreateInstance(type, version);

            return TypeConfiguration.Populate(instance, type, this, origin, version);
        }

        protected virtual object CreateInstance(Type type, double? version)
        {
            return Activator.CreateInstance(type);
        }
    }
}
