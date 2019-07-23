using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    internal static class Utils
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo[]> propInfoCache = new ConcurrentDictionary<string, PropertyInfo[]>();
        private static readonly ConcurrentDictionary<string, Attribute> attrVerCache = new ConcurrentDictionary<string, Attribute>();
        private static readonly ConcurrentDictionary<string, bool> propValidationCache = new ConcurrentDictionary<string, bool>();

        internal static string CurrentCulture(FormattableString formattable)
        {
            if (formattable == null)
                throw new ArgumentNullException(nameof(formattable));

            return formattable.ToString(CultureInfo.CurrentCulture);
        }

        internal static PropertyInfo[] GetProperties(Type type, double? version)
        {
            var key = CurrentCulture($"{type.FullName}:{version}");
            if (propInfoCache.ContainsKey(key))
                return propInfoCache[key];

            var propInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => Utils.CheckPropertyForReadWrite(p, version))
                .OrderBy(p => Utils.GetAttributeForVersion<OffsetAttribute>(p, version).Offset)
                .ToArray();

            propInfoCache.TryAdd(key, propInfo);
            return propInfo;
        }

        internal static T GetAttributeForVersion<T>(MemberInfo member, double? version) where T : Attribute, IVersionAttribute
        {
            var typeInfo = member as TypeInfo;
            var ctorInfo = member as ConstructorInfo;

            string infoKey;
            if (typeInfo != null)
                infoKey = CurrentCulture($"{typeof(T).Name}|{typeInfo.FullName}:{version}");
            else if (ctorInfo != null)
                infoKey = CurrentCulture($"{typeof(T).Name}|{ctorInfo}:{version}");
            else
                infoKey = CurrentCulture($"{typeof(T).Name}|{member.DeclaringType.FullName}.{member.Name}:{version}");

            if (attrVerCache.ContainsKey(infoKey))
                return (T)attrVerCache[infoKey];

            var matches = Utils.GetCustomAttributes<T>(member).Where(o =>
            {
                var minVersion = o.HasMinVersion ? o.MinVersion : (double?)null;
                var maxVersion = o.HasMaxVersion ? o.MaxVersion : (double?)null;

                //exclude when read version is specified and is out of bounds (this expression will always be false if version is null)
                if ((version != minVersion && (version < minVersion || version >= maxVersion)))
                    return false;

                //exclude when read version is not specified but at least one of the bounds is
                if (!version.HasValue && (minVersion.HasValue || maxVersion.HasValue))
                    return false;

                return true;
            }).ToList();

            if (matches.Count > 1)
            {
                //if there is a versioned match and an unversioned match we should use the versioned match
                if (matches.Count == 2)
                {
                    matches = matches.Where(o => o.HasMinVersion || o.HasMaxVersion).ToList();
                    if (matches.Count == 1)
                    {
                        attrVerCache.TryAdd(infoKey, matches.Single());
                        return matches.Single();
                    }
                    //else both or neither are versioned: fall through to the error below
                }

                throw Exceptions.AttributeVersionOverlap(member.Name, typeof(T).Name, version);
            }

            attrVerCache.TryAdd(infoKey, matches.FirstOrDefault());
            return matches.FirstOrDefault();
        }

        internal static bool CheckPropertyForReadWrite(PropertyInfo property, double? version)
        {
            var key = CurrentCulture($"{property.DeclaringType.FullName}.{property.Name}:{version}");
            if (propValidationCache.ContainsKey(key)) return true;

            if (!Attribute.IsDefined(property, typeof(OffsetAttribute)))
                return false; //ignore properties with no offset assigned

            if (Attribute.IsDefined(property, typeof(VersionSpecificAttribute))
                || Attribute.IsDefined(property, typeof(MinVersionAttribute))
                || Attribute.IsDefined(property, typeof(MaxVersionAttribute)))
            {
                if (!version.HasValue)
                    return false; //ignore versioned properties if no read version is specified

                var single = Utils.GetCustomAttribute<VersionSpecificAttribute>(property);
                var min = Utils.GetCustomAttribute<MinVersionAttribute>(property);
                var max = Utils.GetCustomAttribute<MaxVersionAttribute>(property);

                //must satisfy any and all version restrictions that are applied

                if (version != (single?.Version ?? version))
                    return false;

                if (version < min?.MinVersion)
                    return false;

                if (version >= max?.MaxVersion && version != min?.MinVersion)
                    return false;
            }

            if (Utils.GetAttributeForVersion<OffsetAttribute>(property, version) == null)
                throw Exceptions.NoOffsetForVersion(property.Name, version);

            propValidationCache.TryAdd(key, true);
            return true;
        }

        internal static T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(member, typeof(T));
        }

        internal static IEnumerable<T> GetCustomAttributes<T>(MemberInfo member) where T : Attribute
        {
            return Attribute.GetCustomAttributes(member, typeof(T)).OfType<T>();
        }

        internal static bool TryConvert(ref object value, Type fromType, Type toType)
        {
            var converter = TypeDescriptor.GetConverter(fromType);
            if (converter.CanConvertTo(toType))
            {
                value = converter.ConvertTo(value, toType);
                return true;
            }

            converter = TypeDescriptor.GetConverter(toType);
            if (converter.CanConvertFrom(fromType))
            {
                value = converter.ConvertFrom(value);
                return true;
            }

            return false;
        }
    }
}
