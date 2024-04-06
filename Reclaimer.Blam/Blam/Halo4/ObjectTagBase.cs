using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo4
{
    public abstract class ObjectTagBase
    {
        //TODO: the changeover for the MccHalo4 values below was actually U7? (but U7 is grouped under the U6 enum)

        [Offset(0)]
        public short ObjectType { get; set; }

        [Offset(108, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(132, MinVersion = (int)CacheType.Halo4Retail, MaxVersion = (int)CacheType.MccHalo4U6)]
        [Offset(96, MinVersion = (int)CacheType.MccHalo4U6, MaxVersion = (int)CacheType.MccHalo2X)]
        [Offset(132, MinVersion = (int)CacheType.MccHalo2X, MaxVersion = (int)CacheType.MccHalo2XU10)]
        [Offset(96, MinVersion = (int)CacheType.MccHalo2XU10)]
        public StringId DefaultVariant { get; set; }

        [Offset(112, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(136, MinVersion = (int)CacheType.Halo4Retail, MaxVersion = (int)CacheType.MccHalo4U6)]
        [Offset(100, MinVersion = (int)CacheType.MccHalo4U6, MaxVersion = (int)CacheType.MccHalo2X)]
        [Offset(136, MinVersion = (int)CacheType.MccHalo2X, MaxVersion = (int)CacheType.MccHalo2XU10)]
        [Offset(100, MinVersion = (int)CacheType.MccHalo2XU10)]
        public TagReference Model { get; set; }

        [Offset(128, MaxVersion = (int)CacheType.Halo4Retail)]
        [Offset(152, MinVersion = (int)CacheType.Halo4Retail, MaxVersion = (int)CacheType.MccHalo4U6)]
        [Offset(116, MinVersion = (int)CacheType.MccHalo4U6, MaxVersion = (int)CacheType.MccHalo2X)]
        [Offset(152, MinVersion = (int)CacheType.MccHalo2X, MaxVersion = (int)CacheType.MccHalo2XU10)]
        [Offset(116, MinVersion = (int)CacheType.MccHalo2XU10)]
        public TagReference CrateObject { get; set; }

        public render_model ReadRenderModel() => Model.Tag?.ReadMetadata<model>().RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
