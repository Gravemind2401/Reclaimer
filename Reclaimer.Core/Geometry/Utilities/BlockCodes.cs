using System.Runtime.CompilerServices;

namespace Reclaimer.Geometry.Utilities
{
    /// <summary>
    /// A four-character-code used as the header of a data block to indicate what kind of data the block contains.
    /// </summary>
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    internal class BlockCode
    {
        private readonly string charCode;
        private readonly string name;

        public BlockCode(string charCode, [CallerMemberName] string name = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(charCode);
            ArgumentException.ThrowIfNullOrEmpty(name);

            if (charCode.Length > 4)
                throw new ArgumentException("Value cannot be longer than four characters.", nameof(charCode));

            this.charCode = charCode.PadRight(4);
            this.name = name;
        }

        public ReadOnlySpan<char> Value => charCode;

        private string GetDebuggerDisplay() => $"[{charCode}] {name}";
    }

    internal static class SceneCodes
    {
        public static readonly BlockCode FileHeader = new BlockCode("RMF!");
        public static readonly BlockCode List = new BlockCode("list");
        public static readonly BlockCode StringList = new BlockCode("STRS");
        public static readonly BlockCode SceneGroup = new BlockCode("NODE");
        public static readonly BlockCode Placement = new BlockCode("PLAC");
        public static readonly BlockCode ModelReference = new BlockCode("MOD*");

        public static readonly BlockCode Model = new BlockCode("MODL");
        public static readonly BlockCode Material = new BlockCode("MATL");
        public static readonly BlockCode TextureMapping = new BlockCode("TMAP");
        public static readonly BlockCode Texture = new BlockCode("BITM");
        public static readonly BlockCode Tint = new BlockCode("TINT");
        
        public static readonly BlockCode Region = new BlockCode("REGN");
        public static readonly BlockCode Permutation = new BlockCode("PERM");
        public static readonly BlockCode Marker = new BlockCode("MARK");
        public static readonly BlockCode MarkerInstance = new BlockCode("MKIN");
        public static readonly BlockCode Bone = new BlockCode("BONE");
        public static readonly BlockCode Mesh = new BlockCode("MESH");
        public static readonly BlockCode MeshSegment = new BlockCode("MSEG");
        
        public static readonly BlockCode VectorDescriptor = new BlockCode("VECD");
        public static readonly BlockCode VertexBuffer = new BlockCode("VBUF");
        public static readonly BlockCode IndexBuffer = new BlockCode("IBUF");
        public static readonly BlockCode Data = new BlockCode("DATA");
    }

    internal static class VertexChannelCodes
    {
        public static readonly BlockCode Position = new BlockCode("POSN");
        public static readonly BlockCode TextureCoordinate = new BlockCode("TEXC");
        public static readonly BlockCode Normal = new BlockCode("NORM");
        public static readonly BlockCode Tangent = new BlockCode("TANG");
        public static readonly BlockCode Binormal = new BlockCode("BNRM");
        public static readonly BlockCode BlendIndex = new BlockCode("BLID");
        public static readonly BlockCode BlendWeight = new BlockCode("BLWT");
        public static readonly BlockCode Color = new BlockCode("COLR");
    }
}