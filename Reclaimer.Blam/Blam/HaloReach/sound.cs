using Reclaimer.Audio;
using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Blam.HaloReach
{
    public class sound : ISoundContainer
    {
        private readonly ICacheFile cache;
        private readonly IIndexItem item;

        public sound(ICacheFile cache, IIndexItem item)
        {
            this.cache = cache;
            this.item = item;
        }

        [Offset(0)]
        public short Flags { get; set; }

        [Offset(2)]
        public byte SoundClass { get; set; }

        [Offset(3)]
        public SampleRate SampleRate { get; set; }

        [Offset(4)]
        public byte Encoding { get; set; }

        [Offset(5)]
        public byte CodecIndex { get; set; }

        [Offset(6)]
        public short PlaybackIndex { get; set; }

        [Offset(28)]
        public ResourceIdentifier ResourceIdentifier { get; set; }

        #region ISoundContainer

        string ISoundContainer.Name => item.FullPath;

        string ISoundContainer.Class => item.ClassName;

        public GameSound ReadData()
        {
            var resourceGestalt = cache.TagIndex.GetGlobalTag("ugh!").ReadMetadata<sound_cache_file_gestalt>();
            var playback = resourceGestalt.Playbacks[PlaybackIndex];
            var codec = resourceGestalt.Codecs[CodecIndex];
            var sourceData = ResourceIdentifier.ReadSoundData();

            var result = new GameSound
            {
                Name = item.FileName,
                FormatHeader = new XmaHeader(codec.SampleRateInt, codec.ChannelCounts),
                DefaultExtension = "xma"
            };

            for (int i = 0; i < playback.PermutationCount; i++)
            {
                var perm = resourceGestalt.SoundPermutations[playback.FirstPermutationIndex + i];
                var name = resourceGestalt.SoundNames[perm.NameIndex].Name;

                byte[] permData;

                if (playback.PermutationCount == 1)
                {
                    //skip the array copy
                    permData = sourceData;
                }
                else
                {
                    var blocks = Enumerable.Range(perm.BlockIndex, perm.BlockCount)
                        .Select(x => resourceGestalt.DataBlocks[x])
                        .ToList();

                    permData = new byte[blocks.Sum(b => b.Size)];
                    var offset = 0;
                    foreach (var block in blocks)
                    {
                        Array.Copy(sourceData, block.FileOffset, permData, offset, block.Size);
                        offset += block.Size;
                    }
                }

                result.Permutations.Add(new GameSoundPermutation
                {
                    Name = name,
                    SoundData = permData
                });
            }

            return result;
        }

        #endregion
    }
}
