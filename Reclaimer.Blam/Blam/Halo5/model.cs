﻿using Reclaimer.Blam.Common.Gen5;
using Reclaimer.IO;

namespace Reclaimer.Blam.Halo5
{
    public class model
    {
        [Offset(16)]
        public TagReferenceGen5 RenderModel { get; set; }

        [Offset(48)]
        public TagReferenceGen5 CollisionModel { get; set; }

        [Offset(80)]
        public TagReferenceGen5 Animation { get; set; }

        [Offset(112)]
        public TagReferenceGen5 PhysicsModel { get; set; }

        public render_model ReadRenderModel() => RenderModel.Tag?.ReadMetadata<render_model>();
    }
}
