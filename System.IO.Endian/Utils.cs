using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Endian
{
    internal static class Utils
    {
        internal static T GetAttributeForVersion<T>(MemberInfo member, double? version) where T : Attribute, IVersionAttribute
        {
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
                        return matches.Single();
                    //else both or neither are versioned: fall through to the error below
                }

                throw Exceptions.AttributeVersionOverlap(member.Name, typeof(T).Name, version);
            }

            return matches.FirstOrDefault();
        }

        internal static bool CheckPropertyForReadWrite(PropertyInfo property, double? version)
        {
            if (!Attribute.IsDefined(property, typeof(OffsetAttribute)))
                return false; //ignore properties with no offset assigned

            if (Attribute.IsDefined(property, typeof(VersionSpecificAttribute)))
            {
                if (!version.HasValue)
                    return false; //ignore versioned properties if no read version is specified

                var bounds = Utils.GetCustomAttribute<VersionSpecificAttribute>(property);
                var minVersion = bounds.HasMinVersion ? bounds.MinVersion : (double?)null;
                var maxVersion = bounds.HasMaxVersion ? bounds.MaxVersion : (double?)null;

                if (version != minVersion && (version < minVersion || version >= maxVersion))
                    return false; //property not valid for this version
            }

            if (Utils.GetAttributeForVersion<OffsetAttribute>(property, version) == null)
                throw Exceptions.NoOffsetForVersion(property.Name, version);

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
    }
}
