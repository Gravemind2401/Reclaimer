using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.Utilities
{
    /// <summary>
    /// An <seealso cref="EndianReader"/> capable of basic dependency injection.
    /// </summary>
    public class DependencyReader : EndianReader
    {
        private readonly Dictionary<Type, Func<object>> registeredTypes;
        private readonly Dictionary<Type, object> registeredInstances;
        private readonly Dictionary<Type, ConstructorInfo> ctorLookup;

        public DependencyReader(Stream input, ByteOrder byteOrder)
            : this(input, byteOrder, false)
        {
        }

        public DependencyReader(Stream input, ByteOrder byteOrder, bool leaveOpen)
            : base(input, byteOrder, leaveOpen)
        {
            registeredTypes = new Dictionary<Type, Func<object>>();
            registeredInstances = new Dictionary<Type, object>();
            ctorLookup = new Dictionary<Type, ConstructorInfo>();
            DynamicReadEnabled = true;
        }

        protected DependencyReader(DependencyReader parent, long virtualOrigin)
            : base(parent, virtualOrigin)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            registeredTypes = parent.registeredTypes;
            registeredInstances = parent.registeredInstances;
            ctorLookup = parent.ctorLookup;
            DynamicReadEnabled = true;
        }

        public void RegisterType<T>(Func<T> constructor)
        {
            if (registeredTypes.ContainsKey(typeof(T)) || registeredInstances.ContainsKey(typeof(T)))
                throw new ArgumentException(Utils.CurrentCulture($"{typeof(T).Name} has already been registered."));

            registeredTypes.Add(typeof(T), () => constructor());
        }

        public void RegisterInstance<T>(T instance)
        {
            if (registeredTypes.ContainsKey(typeof(T)) || registeredInstances.ContainsKey(typeof(T)))
                throw new ArgumentException(Utils.CurrentCulture($"{typeof(T).Name} has already been registered."));

            registeredInstances.Add(typeof(T), instance);
        }

        public override EndianReader CreateVirtualReader()
        {
            return CreateVirtualReader(BaseStream.Position);
        }

        public override EndianReader CreateVirtualReader(long origin)
        {
            return new DependencyReader(this, origin);
        }

        protected override object CreateInstance(Type type, double? version)
        {
            if (registeredTypes.ContainsKey(type))
                return registeredTypes[type]();

            var constructor = FindConstructor(type);
            if (constructor == null)
                return base.CreateInstance(type, version);
            else return Construct(type, constructor);
        }

        private object Construct(Type type, ConstructorInfo constructor)
        {
            var info = constructor.GetParameters();
            var args = new List<object>();

            foreach (var p in info)
            {
                if (registeredTypes.ContainsKey(p.ParameterType))
                    args.Add(registeredTypes[p.ParameterType]());
                else if (registeredInstances.ContainsKey(p.ParameterType))
                    args.Add(registeredInstances[p.ParameterType]);
                else if (CanCastTo(p.ParameterType))
                    args.Add(this);
                else
                {
                    var ctor2 = FindConstructor(type);
                    if (ctor2 == null)
                        throw new InvalidOperationException();
                    args.Add(Construct(p.ParameterType, ctor2));
                }
            }

            return constructor.Invoke(args.ToArray());
        }

        private bool CanCastTo(Type type)
        {
            return typeof(DependencyReader).IsSubclassOf(type) || typeof(DependencyReader) == type;
        }

        private bool CanConstruct(Type type)
        {
            return CanCastTo(type) || registeredTypes.ContainsKey(type) || registeredInstances.ContainsKey(type) || FindConstructor(type) != null;
        }

        private ConstructorInfo FindConstructor(Type type)
        {
            if (ctorLookup.ContainsKey(type))
                return ctorLookup[type];

            foreach (var constructor in type.GetConstructors().OrderByDescending(c => c.GetParameters().Length))
            {
                var info = constructor.GetParameters();
                if (info.Any() && info.All(i => CanConstruct(i.ParameterType)))
                {
                    ctorLookup.Add(type, constructor);
                    return constructor;
                }
            }

            ctorLookup.Add(type, null);
            return null;
        }
    }
}
