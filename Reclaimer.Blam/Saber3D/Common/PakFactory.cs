using Reclaimer.Blam.Utilities;
using Reclaimer.Saber3D.Halo1X;
using System.IO;

namespace Reclaimer.Saber3D.Common
{
    public static class PakFactory
    {
        public static IPakFile ReadPakFile(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            Exceptions.ThrowIfFileNotFound(fileName);

            return Path.GetExtension(fileName).Equals(".ipak", StringComparison.OrdinalIgnoreCase)
                ? new InplacePakFile(fileName)
                : new PakFile(fileName);
        }
    }
}
