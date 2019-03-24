using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.IO
{
    /// <summary>
    /// An <seealso cref="EndianReader"/> capable of basic dependency injection.
    /// </summary>
    public class DependencyReader : EndianReader
    {
        private readonly Dictionary<Type, Func<object>> registeredTypes;

        public DependencyReader(Stream input, ByteOrder byteOrder)
            : base(input, byteOrder)
        {
            registeredTypes = new Dictionary<Type, Func<object>>();
        }

        protected DependencyReader(DependencyReader parent, long virtualOrigin)
            : base(parent, virtualOrigin)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            registeredTypes = parent.registeredTypes;
        }

        public void RegisterType<T>(Func<T> constructor)
        {
            if (registeredTypes.ContainsKey(typeof(T)))
                throw new ArgumentException(Utils.CurrentCulture($"{typeof(T).Name} has already been registered."));

            registeredTypes.Add(typeof(T), () => constructor());
        }

        protected override object ReadObject(object instance, Type type, double? version)
        {
            if (instance == null && CanConstruct(type))
                instance = Construct(type);

            return base.ReadObject(instance, type, version);
        }

        public override EndianReader CreateVirtualReader()
        {
            return CreateVirtualReader(BaseStream.Position);
        }

        public override EndianReader CreateVirtualReader(long origin)
        {
            return new DependencyReader(this, origin);
        }

        protected override void ReadProperty(object instance, PropertyInfo prop, Type storeType, double? version)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            if (CanConstruct(prop.PropertyType))
            {
                var value = Construct(prop.PropertyType);
                base.ReadObject(value, storeType, version);
                prop.SetValue(instance, value);
            }
            else base.ReadProperty(instance, prop, storeType, version);
        }

        private bool CanConstruct(Type type)
        {
            return registeredTypes.ContainsKey(type) || FindConstructor(type) != null;
        }

        private object Construct(Type type)
        {
            if (!CanConstruct(type))
                throw new InvalidOperationException();

            if (registeredTypes.ContainsKey(type))
                return registeredTypes[type]();

            var constructor = FindConstructor(type);
            var info = constructor.GetParameters();
            var args = new List<object>();

            foreach (var p in info)
            {
                if (registeredTypes.ContainsKey(p.ParameterType))
                    args.Add(registeredTypes[p.ParameterType]());
                else if (p.ParameterType == typeof(DependencyReader))
                    args.Add(this);
                else if (CanConstruct(p.ParameterType))
                    args.Add(Construct(p.ParameterType));
                else throw new InvalidOperationException();
            }

            return constructor.Invoke(args.ToArray());
        }

        private ConstructorInfo FindConstructor(Type type)
        {
            foreach (var constructor in type.GetConstructors())
            {
                var info = constructor.GetParameters();
                if (info.Any() && info.All(i => CanConstruct(i.ParameterType) || i.ParameterType == typeof(DependencyReader)))
                    return constructor;
            }

            return null;
        }
    }
}
