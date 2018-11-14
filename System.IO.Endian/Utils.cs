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
            var matches = (from attr in Attribute.GetCustomAttributes(member, typeof(T))
                           let o = (T)attr
                           where

                           //exclude when read version is specified and is out of bounds (the expression in brackets will always be false if version is null)
                           !(version < o.MinVersion || version >= o.MaxVersion)

                           //exclude when read version is not specified but at least one of the bounds is
                           && !(!version.HasValue && (o.HasMinVersion || o.HasMaxVersion))

                           select o).ToList();

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

        internal static bool CheckPropertyForRead(PropertyInfo property, double? version)
        {
            if (!Attribute.IsDefined(property, typeof(OffsetAttribute)))
                return false; //ignore properties with no offset assigned

            if (property.GetSetMethod() == null)
                return false; //no public setter for this property

            if (Attribute.IsDefined(property, typeof(VersionSpecificAttribute)))
            {
                if (!version.HasValue)
                    return false; //ignore versioned properties if no read version is specified

                var versionBounds = (VersionSpecificAttribute)Attribute.GetCustomAttribute(property, typeof(VersionSpecificAttribute));
                if (version < versionBounds.MinVersion || version >= versionBounds.MaxVersion)
                    return false; //property not valid for this version
            }

            return true;
        }
    }
}
