using Reclaimer.Blam.Utilities;

namespace Reclaimer.Blam.Common.Gen5
{
    public static class ModuleFactory
    {
        public static IModule ReadModuleFile(string fileName)
        {
            ArgumentNullException.ThrowIfNull(fileName);
            Exceptions.ThrowIfFileNotFound(fileName);

            var args = ModuleArgs.FromFile(fileName);

            return args.Version switch
            {
                ModuleType.Halo5Server or ModuleType.Halo5Forge => new Halo5.Module(fileName),
                ModuleType.HaloInfinite => new HaloInfinite.Module(fileName),
                _ => throw Exceptions.UnknownModuleFile(fileName)
            };
        }
    }
}
