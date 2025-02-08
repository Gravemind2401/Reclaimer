using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo1
{
    public class ShaderEnvironmentTag
    {
        [Offset(40)]
        public ShaderEnvironmentFlags Flags { get; set; }

        [Offset(42)]
        public ShaderEnvironmentType Type { get; set; }

        [Offset(108)]
        public ShaderEnvironmentDetailFlags DetailFlags { get; set; }

        [Offset(136)]
        public TagReference BaseMap { get; set; }

        [Offset(176)]
        public ShaderDetailFunction DetailFunction { get; set; }

        [Offset(180)]
        public float PrimaryDetailMapScale { get; set; }

        [Offset(184)]
        public TagReference PrimaryDetailMap { get; set; }

        [Offset(200)]
        public float SecondaryDetailMapScale { get; set; }

        [Offset(204)]
        public TagReference SecondaryDetailMap { get; set; }

        [Offset(244)]
        public ShaderDetailFunction MicroDetailFunction { get; set; }

        [Offset(248)]
        public float MicroDetailMapScale { get; set; }

        [Offset(252)]
        public TagReference MicroDetailMap { get; set; }

        [Offset(292)]
        public float BumpMapScale { get; set; }

        [Offset(296)]
        public TagReference BumpMap { get; set; }

        [Offset(564)]
        public TagReference ReflectionCubeMap { get; set; }
    }

    [Flags]
    public enum ShaderEnvironmentFlags : ushort
    {
        None = 0,
        AlphaTested = 1 << 0,
        BumpMapIsSpecularMask = 1 << 1,
        TrueAtmosphericFog = 1 << 2
    }

    public enum ShaderEnvironmentType : ushort
    {
        Normal = 0,
        Blended = 1,
        BlendedBaseSpecular = 2
    }

    [Flags]
    public enum ShaderEnvironmentDetailFlags : ushort
    {
        None = 0,
        RescaleDetailMaps = 1 << 0,
        RescaleBumpMap = 1 << 1
    }
}
