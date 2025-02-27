﻿using Reclaimer.Blam.Common;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo3
{
    public abstract class ObjectTag
    {
        [Offset(0)]
        public short ObjectType { get; set; }

        [Offset(48)]
        public StringId DefaultVariant { get; set; }

        [Offset(52)]
        public TagReference Model { get; set; }

        [Offset(68)]
        public TagReference CrateObject { get; set; }

        [Offset(84)]
        public TagReference CollisionDamage { get; set; }

        public RenderModelTag ReadRenderModel() => Model.Tag?.ReadMetadata<ModelTag>().RenderModel.Tag?.ReadMetadata<RenderModelTag>();
    }
}
