using Reclaimer.IO;
using System.IO;
using System.Reflection;

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
        }

        protected DependencyReader(DependencyReader parent, long virtualOrigin)
            : base(parent, virtualOrigin)
        {
            ArgumentNullException.ThrowIfNull(parent);

            registeredTypes = parent.registeredTypes;
            registeredInstances = parent.registeredInstances;
            ctorLookup = parent.ctorLookup;
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

        public override EndianReader CreateVirtualReader() => CreateVirtualReader(BaseStream.Position);

        public override EndianReader CreateVirtualReader(long origin) => new DependencyReader(this, origin);

        protected override T CreateInstance<T>(double? version)
        {
            if (registeredTypes.TryGetValue(typeof(T), out var factory))
                return (T)factory.Invoke();

            var constructor = FindConstructor(typeof(T));
            return constructor == null
                ? base.CreateInstance<T>(version)
                : (T)Construct(typeof(T), constructor);
        }

        private object Construct(Type type, ConstructorInfo constructor)
        {
            var info = constructor.GetParameters();
            var args = new List<object>();

            foreach (var p in info)
            {
                if (registeredTypes.TryGetValue(p.ParameterType, out var factory))
                    args.Add(factory());
                else if (registeredInstances.TryGetValue(p.ParameterType, out var instance))
                    args.Add(instance);
                else if (CanCastTo(p.ParameterType))
                    args.Add(this);
                else
                {
                    var ctor2 = FindConstructor(type) ?? throw new InvalidOperationException();
                    args.Add(Construct(p.ParameterType, ctor2));
                }
            }

            return constructor.Invoke(args.ToArray());
        }

        private static bool CanCastTo(Type type)
        {
            return typeof(DependencyReader).IsSubclassOf(type) || typeof(DependencyReader) == type;
        }

        private bool CanConstruct(Type type)
        {
            return CanCastTo(type) || registeredTypes.ContainsKey(type) || registeredInstances.ContainsKey(type) || FindConstructor(type) != null;
        }

        private ConstructorInfo FindConstructor(Type type)
        {
            if (ctorLookup.TryGetValue(type, out var ctor))
                return ctor;

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
