using Reclaimer.Geometry.Vectors;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Reclaimer.Geometry.Utilities
{
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
        
        public static readonly BlockCode VertexBuffer = new BlockCode("VBUF");
        public static readonly BlockCode IndexBuffer = new BlockCode("IBUF");
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

    internal static class VectorTypeCodes
    {
        public static readonly BlockCode Float2 = new BlockCode("VEC2");
        public static readonly BlockCode Float3 = new BlockCode("VEC3");
        public static readonly BlockCode Float4 = new BlockCode("VEC4");

        public static readonly BlockCode DecN4 = new BlockCode("SDC4");
        public static readonly BlockCode DHenN3 = new BlockCode("SDH3");
        public static readonly BlockCode HenDN3 = new BlockCode("SHD3");
        public static readonly BlockCode UDecN4 = new BlockCode("UDC4");
        public static readonly BlockCode UDHenN3 = new BlockCode("UDH3");
        public static readonly BlockCode UHenDN3 = new BlockCode("UHD3");

        public static readonly BlockCode Int16N2 = new BlockCode("S162");
        public static readonly BlockCode Int16N4 = new BlockCode("S164");
        public static readonly BlockCode UInt16N2 = new BlockCode("U162");
        public static readonly BlockCode UInt16N4 = new BlockCode("U164");

        public static BlockCode FromType(Type vectorType)
        {
            return vectorType switch
            {
                _ when vectorType == typeof(RealVector2) => Float2,
                _ when vectorType == typeof(RealVector3) => Float3,
                _ when vectorType == typeof(RealVector4) => Float4,
                _ when vectorType == typeof(DecN4) => DecN4,
                _ when vectorType == typeof(DHenN3) => DHenN3,
                _ when vectorType == typeof(HenDN3) => HenDN3,
                _ when vectorType == typeof(UDecN4) => UDecN4,
                _ when vectorType == typeof(UDHenN3) => UDHenN3,
                _ when vectorType == typeof(UHenDN3) => UHenDN3,
                _ when vectorType == typeof(Int16N2) => Int16N2,
                _ when vectorType == typeof(Int16N4) => Int16N4,
                _ when vectorType == typeof(UInt16N2) => UInt16N2,
                _ when vectorType == typeof(UInt16N4) => UInt16N4,
                _ => null
            };
        }
    }
}