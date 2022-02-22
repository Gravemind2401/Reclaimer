using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace System.Drawing.Dds
{
    public class DdsOutputArgs
    {
        /// <summary>
        /// Gets or sets the options to use when decompressing the image.
        /// </summary>
        public DecompressOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the layout of the cubemap. Has no effect if the DDS cubemap flags are not set.
        /// </summary>
        public CubemapLayout Layout { get; set; }

        internal bool Bgr24 => Options.HasFlag(DecompressOptions.Bgr24);
        internal PixelFormat Format => Bgr24 ? PixelFormats.Bgr24 : PixelFormats.Bgra32;
        internal int Bpp => Bgr24 ? 3 : 4;
        internal bool UseChannelMask => (Options & DecompressOptions.RemoveAllChannels) != 0; //at least one 'remove channel' flag is set
        internal bool ValidCubeLayout => Layout?.IsValid ?? false;

        /// <summary>
        /// Creates a new instance of <see cref="DdsOutputArgs"/> with the specified parameters.
        /// </summary>
        /// <param name="options">Options to use when decompressing the image.</param>
        /// <param name="layout">The layout of the cubemap. Has no effect if the DDS cubemap flags are not set.</param>
        /// <param name="slice">The slice index for 3d textures, or the array index for array textures.</param>
        /// <param name="mip">The mipmap index.</param>
        public DdsOutputArgs(DecompressOptions options = DecompressOptions.Default, CubemapLayout layout = null)
        {
            Options = options;
            Layout = layout ?? CubemapLayout.NonCubemap;
        }
    }
}
