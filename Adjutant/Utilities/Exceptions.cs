using Adjutant.Saber3D.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    internal static class Exceptions
    {
        internal static ArgumentException ParamMustBeNonZero(string paramName)
        {
            return new ArgumentException(Utils.CurrentCulture($"{paramName} cannot be zero."), paramName);
        }

        internal static InvalidOperationException CoordSysNotConvertable()
        {
            return new InvalidOperationException(Utils.CurrentCulture($"No conversion exists between the given coordinate systems."));
        }

        internal static FileNotFoundException FileNotFound(string fileName)
        {
            return new FileNotFoundException(Utils.CurrentCulture($"The file does not exist."), fileName);
        }

        internal static ArgumentException NotAValidMapFile(string fileName)
        {
            return new ArgumentException(Utils.CurrentCulture($"The file '{Utils.GetFileName(fileName)}' cannot be opened. It is not a valid map file or it may be compressed."));
        }

        internal static ArgumentException UnknownMapFile(string fileName)
        {
            return new ArgumentException(Utils.CurrentCulture($"The file '{Utils.GetFileName(fileName)}' cannot be opened. It looks like a valid map file, but may not be a supported version."));
        }

        internal static NotSupportedException BitmapFormatNotSupported(string formatName)
        {
            return new NotSupportedException($"The BitmapFormat '{formatName}' is not supported.");
        }

        internal static ArgumentException NotASaberTextureItem(IPakItem item)
        {
            return new ArgumentException($"'{item.Name}' is not a texture file.");
        }

        internal static NotSupportedException AmbiguousScenarioReference()
        {
            return new NotSupportedException("Could not determine primary scenario tag.");
        }
    }
}
