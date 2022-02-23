using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Dds
{
    [Flags]
    internal enum TextureFlags
    {
        DdsSurfaceFlagsTexture = DdsCaps.Texture,
        DdsSurfaceFlagsCubemap = DdsCaps.Texture | DdsCaps.Complex,
        DdsSurfaceFlagsMipmap = DdsCaps.Texture | DdsCaps.Complex | DdsCaps.Mipmap
    }

    [Flags]
    public enum DecompressOptions
    {
        /// <summary>
        /// The default option. If no other flags are specified, 32bpp BGRA will be used.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Outputs pixel data in 24bpp BGR format. Does not output an alpha channel regardless of any other flags specified.
        /// </summary>
        Bgr24 = 1,

        /// <summary>
        /// Replaces all blue channel data with zeros.
        /// </summary>
        RemoveBlueChannel = 2,

        /// <summary>
        /// Replaces all green channel data with zeros.
        /// </summary>
        RemoveGreenChannel = 4,

        /// <summary>
        /// Replaces all red channel data with zeros.
        /// </summary>
        RemoveRedChannel = 8,

        /// <summary>
        /// Replaces all alpha channel data with full opacity.
        /// </summary>
        RemoveAlphaChannel = 16,

        /// <summary>
        /// Replicates the blue channel data over the green and red channels. The alpha channel will be fully opaque.
        /// </summary>
        BlueChannelOnly = RemoveGreenChannel | RemoveRedChannel | RemoveAlphaChannel,

        /// <summary>
        /// Replicates the green channel data over the blue and red channels. The alpha channel will be fully opaque.
        /// </summary>
        GreenChannelOnly = RemoveBlueChannel | RemoveRedChannel | RemoveAlphaChannel,

        /// <summary>
        /// Replicates the red channel data over the blue and green and channels. The alpha channel will be fully opaque.
        /// </summary>
        RedChannelOnly = RemoveBlueChannel | RemoveGreenChannel | RemoveAlphaChannel,

        /// <summary>
        /// Replicates the alpha channel data over the blue, green and red channels. The alpha channel will be fully opaque.
        /// </summary>
        AlphaChannelOnly = RemoveBlueChannel | RemoveGreenChannel | RemoveRedChannel,

        /// <summary>
        /// Produces a solid black image with opaque alpha.
        /// </summary>
        RemoveAllChannels = RemoveBlueChannel | RemoveGreenChannel | RemoveRedChannel | RemoveAlphaChannel
    }

    public enum CubemapFace
    {
        None,
        Top,
        Left,
        Front,
        Right,
        Back,
        Bottom
    }
}
