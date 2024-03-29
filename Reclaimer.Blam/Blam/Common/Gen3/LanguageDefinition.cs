﻿using Reclaimer.IO;

namespace Reclaimer.Blam.Common.Gen3
{
    public class LanguageDefinition
    {
        [Offset(0)]
        public int StringCount { get; set; }

        [Offset(4)]
        public int StringsSize { get; set; }

        [Offset(8)]
        public int IndicesOffset { get; set; }

        [Offset(12)]
        public int StringsOffset { get; set; }
    }
}
