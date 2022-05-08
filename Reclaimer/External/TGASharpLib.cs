//https://github.com/ALEXGREENALEX/TGASharpLib/blob/master/TGASharpLib/TGASharpLib.cs
/*                         MIT License
                 Copyright (c) 2017 TGASharpLib

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TGASharpLib
{
    #region Enums
    /// <summary>
    /// <para>The first 128 Color Map Type codes are reserved for use by Truevision,
    /// while the second set of 128 Color Map Type codes(128 to 255) may be used for
    /// developer applications.</para>
    /// True-Color images do not normally make use of the color map field, but some current
    /// applications store palette information or developer-defined information in this field.
    /// It is best to check Field 3, Image Type, to make sure you have a file which can use the
    /// data stored in the Color Map Field.
    /// Otherwise ignore the information. When saving or creating files for True-Color
    /// images do not use this field and set it to Zero to ensure compatibility. Please refer
    /// to the Developer Area specification for methods of storing developer defined information.
    /// </summary>
    public enum TgaColorMapType : byte
    {
        NoColorMap = 0,
        ColorMap = 1,
        Truevision_2,
        Truevision_3,
        Truevision_4,
        Truevision_5,
        Truevision_6,
        Truevision_7,
        Truevision_8,
        Truevision_9,
        Truevision_10,
        Truevision_11,
        Truevision_12,
        Truevision_13,
        Truevision_14,
        Truevision_15,
        Truevision_16,
        Truevision_17,
        Truevision_18,
        Truevision_19,
        Truevision_20,
        Truevision_21,
        Truevision_22,
        Truevision_23,
        Truevision_24,
        Truevision_25,
        Truevision_26,
        Truevision_27,
        Truevision_28,
        Truevision_29,
        Truevision_30,
        Truevision_31,
        Truevision_32,
        Truevision_33,
        Truevision_34,
        Truevision_35,
        Truevision_36,
        Truevision_37,
        Truevision_38,
        Truevision_39,
        Truevision_40,
        Truevision_41,
        Truevision_42,
        Truevision_43,
        Truevision_44,
        Truevision_45,
        Truevision_46,
        Truevision_47,
        Truevision_48,
        Truevision_49,
        Truevision_50,
        Truevision_51,
        Truevision_52,
        Truevision_53,
        Truevision_54,
        Truevision_55,
        Truevision_56,
        Truevision_57,
        Truevision_58,
        Truevision_59,
        Truevision_60,
        Truevision_61,
        Truevision_62,
        Truevision_63,
        Truevision_64,
        Truevision_65,
        Truevision_66,
        Truevision_67,
        Truevision_68,
        Truevision_69,
        Truevision_70,
        Truevision_71,
        Truevision_72,
        Truevision_73,
        Truevision_74,
        Truevision_75,
        Truevision_76,
        Truevision_77,
        Truevision_78,
        Truevision_79,
        Truevision_80,
        Truevision_81,
        Truevision_82,
        Truevision_83,
        Truevision_84,
        Truevision_85,
        Truevision_86,
        Truevision_87,
        Truevision_88,
        Truevision_89,
        Truevision_90,
        Truevision_91,
        Truevision_92,
        Truevision_93,
        Truevision_94,
        Truevision_95,
        Truevision_96,
        Truevision_97,
        Truevision_98,
        Truevision_99,
        Truevision_100,
        Truevision_101,
        Truevision_102,
        Truevision_103,
        Truevision_104,
        Truevision_105,
        Truevision_106,
        Truevision_107,
        Truevision_108,
        Truevision_109,
        Truevision_110,
        Truevision_111,
        Truevision_112,
        Truevision_113,
        Truevision_114,
        Truevision_115,
        Truevision_116,
        Truevision_117,
        Truevision_118,
        Truevision_119,
        Truevision_120,
        Truevision_121,
        Truevision_122,
        Truevision_123,
        Truevision_124,
        Truevision_125,
        Truevision_126,
        Truevision_127,
        Other_128,
        Other_129,
        Other_130,
        Other_131,
        Other_132,
        Other_133,
        Other_134,
        Other_135,
        Other_136,
        Other_137,
        Other_138,
        Other_139,
        Other_140,
        Other_141,
        Other_142,
        Other_143,
        Other_144,
        Other_145,
        Other_146,
        Other_147,
        Other_148,
        Other_149,
        Other_150,
        Other_151,
        Other_152,
        Other_153,
        Other_154,
        Other_155,
        Other_156,
        Other_157,
        Other_158,
        Other_159,
        Other_160,
        Other_161,
        Other_162,
        Other_163,
        Other_164,
        Other_165,
        Other_166,
        Other_167,
        Other_168,
        Other_169,
        Other_170,
        Other_171,
        Other_172,
        Other_173,
        Other_174,
        Other_175,
        Other_176,
        Other_177,
        Other_178,
        Other_179,
        Other_180,
        Other_181,
        Other_182,
        Other_183,
        Other_184,
        Other_185,
        Other_186,
        Other_187,
        Other_188,
        Other_189,
        Other_190,
        Other_191,
        Other_192,
        Other_193,
        Other_194,
        Other_195,
        Other_196,
        Other_197,
        Other_198,
        Other_199,
        Other_200,
        Other_201,
        Other_202,
        Other_203,
        Other_204,
        Other_205,
        Other_206,
        Other_207,
        Other_208,
        Other_209,
        Other_210,
        Other_211,
        Other_212,
        Other_213,
        Other_214,
        Other_215,
        Other_216,
        Other_217,
        Other_218,
        Other_219,
        Other_220,
        Other_221,
        Other_222,
        Other_223,
        Other_224,
        Other_225,
        Other_226,
        Other_227,
        Other_228,
        Other_229,
        Other_230,
        Other_231,
        Other_232,
        Other_233,
        Other_234,
        Other_235,
        Other_236,
        Other_237,
        Other_238,
        Other_239,
        Other_240,
        Other_241,
        Other_242,
        Other_243,
        Other_244,
        Other_245,
        Other_246,
        Other_247,
        Other_248,
        Other_249,
        Other_250,
        Other_251,
        Other_252,
        Other_253,
        Other_254,
        Other_255
    }

    /// <summary>
    /// Establishes the number of bits per entry. Typically 15, 16, 24 or 32-bit values are used.
    /// <para>When working with VDA or VDA/D cards it is preferred that you select 16 bits(5 bits
    /// per primary with 1 bit to select interrupt control) and set the 16th bit to 0 so that the
    /// interrupt bit is disabled. Even if this field is set to 15 bits(5 bits per primary) you
    /// must still parse the color map data 16 bits at a time and ignore the 16th bit.</para>
    /// <para>When working with a TARGA M8 card you would select 24 bits (8 bits per primary)
    /// since the color map is defined as 256 entries of 24 bit color values.</para>
    /// When working with a TrueVista card(ATVista or NuVista) you would select 24-bit(8 bits per
    /// primary) or 32-bit(8 bits per primary including Alpha channel) depending on your
    /// application’s use of look-up tables. It is suggested that when working with 16-bit and
    /// 32-bit color images, you store them as True-Color images and do not use the color map 
    /// field to store look-up tables. Please refer to the TGA Extensions for fields better suited
    /// to storing look-up table information.
    /// </summary>
    public enum TgaColorMapEntrySize : byte
    {
        Other = 0,
        X1R5G5B5 = 15,
        A1R5G5B5 = 16,
        R8G8B8 = 24,
        A8R8G8B8 = 32
    }

    /// <summary>
    /// Truevision has currently defined seven image types:
    /// <para>0 - No Image Data Included;</para>
    /// <para>1 - Uncompressed, Color-mapped Image;</para>
    /// <para>2 - Uncompressed, True-color Image;</para>
    /// <para>3 - Uncompressed, Black-and-white Image;</para>
    /// <para>9 - Run-length encoded, Color-mapped Image;</para>
    /// <para>10 - Run-length encoded, True-color Image;</para>
    /// <para>11 - Run-length encoded, Black-and-white Image.</para>
    /// Image Data Type codes 0 to 127 are reserved for use by Truevision for general applications.
    /// Image Data Type codes 128 to 255 may be used for developer applications.
    /// </summary>
    public enum TgaImageType : byte
    {
        NoImageData = 0,
        Uncompressed_ColorMapped = 1,
        Uncompressed_TrueColor,
        Uncompressed_BlackWhite,
        _Truevision_4,
        _Truevision_5,
        _Truevision_6,
        _Truevision_7,
        _Truevision_8,
        RLE_ColorMapped = 9,
        RLE_TrueColor,
        RLE_BlackWhite,
        _Truevision_12,
        _Truevision_13,
        _Truevision_14,
        _Truevision_15,
        _Truevision_16,
        _Truevision_17,
        _Truevision_18,
        _Truevision_19,
        _Truevision_20,
        _Truevision_21,
        _Truevision_22,
        _Truevision_23,
        _Truevision_24,
        _Truevision_25,
        _Truevision_26,
        _Truevision_27,
        _Truevision_28,
        _Truevision_29,
        _Truevision_30,
        _Truevision_31,
        _Truevision_32,
        _Truevision_33,
        _Truevision_34,
        _Truevision_35,
        _Truevision_36,
        _Truevision_37,
        _Truevision_38,
        _Truevision_39,
        _Truevision_40,
        _Truevision_41,
        _Truevision_42,
        _Truevision_43,
        _Truevision_44,
        _Truevision_45,
        _Truevision_46,
        _Truevision_47,
        _Truevision_48,
        _Truevision_49,
        _Truevision_50,
        _Truevision_51,
        _Truevision_52,
        _Truevision_53,
        _Truevision_54,
        _Truevision_55,
        _Truevision_56,
        _Truevision_57,
        _Truevision_58,
        _Truevision_59,
        _Truevision_60,
        _Truevision_61,
        _Truevision_62,
        _Truevision_63,
        _Truevision_64,
        _Truevision_65,
        _Truevision_66,
        _Truevision_67,
        _Truevision_68,
        _Truevision_69,
        _Truevision_70,
        _Truevision_71,
        _Truevision_72,
        _Truevision_73,
        _Truevision_74,
        _Truevision_75,
        _Truevision_76,
        _Truevision_77,
        _Truevision_78,
        _Truevision_79,
        _Truevision_80,
        _Truevision_81,
        _Truevision_82,
        _Truevision_83,
        _Truevision_84,
        _Truevision_85,
        _Truevision_86,
        _Truevision_87,
        _Truevision_88,
        _Truevision_89,
        _Truevision_90,
        _Truevision_91,
        _Truevision_92,
        _Truevision_93,
        _Truevision_94,
        _Truevision_95,
        _Truevision_96,
        _Truevision_97,
        _Truevision_98,
        _Truevision_99,
        _Truevision_100,
        _Truevision_101,
        _Truevision_102,
        _Truevision_103,
        _Truevision_104,
        _Truevision_105,
        _Truevision_106,
        _Truevision_107,
        _Truevision_108,
        _Truevision_109,
        _Truevision_110,
        _Truevision_111,
        _Truevision_112,
        _Truevision_113,
        _Truevision_114,
        _Truevision_115,
        _Truevision_116,
        _Truevision_117,
        _Truevision_118,
        _Truevision_119,
        _Truevision_120,
        _Truevision_121,
        _Truevision_122,
        _Truevision_123,
        _Truevision_124,
        _Truevision_125,
        _Truevision_126,
        _Truevision_127,
        _Other_128,
        _Other_129,
        _Other_130,
        _Other_131,
        _Other_132,
        _Other_133,
        _Other_134,
        _Other_135,
        _Other_136,
        _Other_137,
        _Other_138,
        _Other_139,
        _Other_140,
        _Other_141,
        _Other_142,
        _Other_143,
        _Other_144,
        _Other_145,
        _Other_146,
        _Other_147,
        _Other_148,
        _Other_149,
        _Other_150,
        _Other_151,
        _Other_152,
        _Other_153,
        _Other_154,
        _Other_155,
        _Other_156,
        _Other_157,
        _Other_158,
        _Other_159,
        _Other_160,
        _Other_161,
        _Other_162,
        _Other_163,
        _Other_164,
        _Other_165,
        _Other_166,
        _Other_167,
        _Other_168,
        _Other_169,
        _Other_170,
        _Other_171,
        _Other_172,
        _Other_173,
        _Other_174,
        _Other_175,
        _Other_176,
        _Other_177,
        _Other_178,
        _Other_179,
        _Other_180,
        _Other_181,
        _Other_182,
        _Other_183,
        _Other_184,
        _Other_185,
        _Other_186,
        _Other_187,
        _Other_188,
        _Other_189,
        _Other_190,
        _Other_191,
        _Other_192,
        _Other_193,
        _Other_194,
        _Other_195,
        _Other_196,
        _Other_197,
        _Other_198,
        _Other_199,
        _Other_200,
        _Other_201,
        _Other_202,
        _Other_203,
        _Other_204,
        _Other_205,
        _Other_206,
        _Other_207,
        _Other_208,
        _Other_209,
        _Other_210,
        _Other_211,
        _Other_212,
        _Other_213,
        _Other_214,
        _Other_215,
        _Other_216,
        _Other_217,
        _Other_218,
        _Other_219,
        _Other_220,
        _Other_221,
        _Other_222,
        _Other_223,
        _Other_224,
        _Other_225,
        _Other_226,
        _Other_227,
        _Other_228,
        _Other_229,
        _Other_230,
        _Other_231,
        _Other_232,
        _Other_233,
        _Other_234,
        _Other_235,
        _Other_236,
        _Other_237,
        _Other_238,
        _Other_239,
        _Other_240,
        _Other_241,
        _Other_242,
        _Other_243,
        _Other_244,
        _Other_245,
        _Other_246,
        _Other_247,
        _Other_248,
        _Other_249,
        _Other_250,
        _Other_251,
        _Other_252,
        _Other_253,
        _Other_254,
        _Other_255
    }

    /// <summary>
    /// Number of bits per pixel. This number includes the Attribute or Alpha channel bits.
    /// Common values are 8, 16, 24 and 32 but other pixel depths could be used.
    /// </summary>
    public enum TgaPixelDepth : byte
    {
        Other = 0,
        Bpp8 = 8,
        Bpp16 = 16,
        Bpp24 = 24,
        Bpp32 = 32
    }

    /// <summary>
    /// Used to indicate the order in which pixel data is transferred from the file to the screen.
    /// (Bit 4 (bit 0 in enum) is for left-to-right ordering and bit 5 (bit 1 in enum) is for
    /// topto-bottom ordering as shown below.)
    /// </summary>
    public enum TgaImgOrigin : byte
    {
        BottomLeft = 0,
        BottomRight,
        TopLeft,
        TopRight
    }

    /// <summary>
    /// Contains a value which specifies the type of Alpha channel
    /// data contained in the file. Value Meaning:
    /// <para>0: no Alpha data included (bits 3-0 of field 5.6 should also be set to zero)</para>
    /// <para>1: undefined data in the Alpha field, can be ignored</para>
    /// <para>2: undefined data in the Alpha field, but should be retained</para>
    /// <para>3: useful Alpha channel data is present</para>
    /// <para>4: pre-multiplied Alpha(see description below)</para>
    /// <para>5 -127: RESERVED</para>
    /// <para>128-255: Un-assigned</para>
    /// <para>Pre-multiplied Alpha Example: Suppose the Alpha channel data is being used to specify the
    /// opacity of each pixel(for use when the image is overlayed on another image), where 0 indicates
    /// that the pixel is completely transparent and a value of 1 indicates that the pixel is
    /// completely opaque(assume all component values have been normalized).</para>
    /// <para>A quadruple(a, r, g, b) of( 0.5, 1, 0, 0) would indicate that the pixel is pure red with a
    /// transparency of one-half. For numerous reasons(including image compositing) is is better to
    /// pre-multiply the individual color components with the value in the Alpha channel.</para>
    /// A pre-multiplication of the above would produce a quadruple(0.5, 0.5, 0, 0).
    /// A value of 3 in the Attributes Type Field(field 23) would indicate that the color components
    /// of the pixel have already been scaled by the value in the Alpha channel.
    /// </summary>
    public enum TgaAttrType : byte
    {
        NoAlpha = 0,
        UndefinedAlphaCanBeIgnored,
        UndefinedAlphaButShouldBeRetained,
        UsefulAlpha,
        PreMultipliedAlpha,
        _Reserved_5,
        _Reserved_6,
        _Reserved_7,
        _Reserved_8,
        _Reserved_9,
        _Reserved_10,
        _Reserved_11,
        _Reserved_12,
        _Reserved_13,
        _Reserved_14,
        _Reserved_15,
        _Reserved_16,
        _Reserved_17,
        _Reserved_18,
        _Reserved_19,
        _Reserved_20,
        _Reserved_21,
        _Reserved_22,
        _Reserved_23,
        _Reserved_24,
        _Reserved_25,
        _Reserved_26,
        _Reserved_27,
        _Reserved_28,
        _Reserved_29,
        _Reserved_30,
        _Reserved_31,
        _Reserved_32,
        _Reserved_33,
        _Reserved_34,
        _Reserved_35,
        _Reserved_36,
        _Reserved_37,
        _Reserved_38,
        _Reserved_39,
        _Reserved_40,
        _Reserved_41,
        _Reserved_42,
        _Reserved_43,
        _Reserved_44,
        _Reserved_45,
        _Reserved_46,
        _Reserved_47,
        _Reserved_48,
        _Reserved_49,
        _Reserved_50,
        _Reserved_51,
        _Reserved_52,
        _Reserved_53,
        _Reserved_54,
        _Reserved_55,
        _Reserved_56,
        _Reserved_57,
        _Reserved_58,
        _Reserved_59,
        _Reserved_60,
        _Reserved_61,
        _Reserved_62,
        _Reserved_63,
        _Reserved_64,
        _Reserved_65,
        _Reserved_66,
        _Reserved_67,
        _Reserved_68,
        _Reserved_69,
        _Reserved_70,
        _Reserved_71,
        _Reserved_72,
        _Reserved_73,
        _Reserved_74,
        _Reserved_75,
        _Reserved_76,
        _Reserved_77,
        _Reserved_78,
        _Reserved_79,
        _Reserved_80,
        _Reserved_81,
        _Reserved_82,
        _Reserved_83,
        _Reserved_84,
        _Reserved_85,
        _Reserved_86,
        _Reserved_87,
        _Reserved_88,
        _Reserved_89,
        _Reserved_90,
        _Reserved_91,
        _Reserved_92,
        _Reserved_93,
        _Reserved_94,
        _Reserved_95,
        _Reserved_96,
        _Reserved_97,
        _Reserved_98,
        _Reserved_99,
        _Reserved_100,
        _Reserved_101,
        _Reserved_102,
        _Reserved_103,
        _Reserved_104,
        _Reserved_105,
        _Reserved_106,
        _Reserved_107,
        _Reserved_108,
        _Reserved_109,
        _Reserved_110,
        _Reserved_111,
        _Reserved_112,
        _Reserved_113,
        _Reserved_114,
        _Reserved_115,
        _Reserved_116,
        _Reserved_117,
        _Reserved_118,
        _Reserved_119,
        _Reserved_120,
        _Reserved_121,
        _Reserved_122,
        _Reserved_123,
        _Reserved_124,
        _Reserved_125,
        _Reserved_126,
        _Reserved_127,
        _UnAssigned_128,
        _UnAssigned_129,
        _UnAssigned_130,
        _UnAssigned_131,
        _UnAssigned_132,
        _UnAssigned_133,
        _UnAssigned_134,
        _UnAssigned_135,
        _UnAssigned_136,
        _UnAssigned_137,
        _UnAssigned_138,
        _UnAssigned_139,
        _UnAssigned_140,
        _UnAssigned_141,
        _UnAssigned_142,
        _UnAssigned_143,
        _UnAssigned_144,
        _UnAssigned_145,
        _UnAssigned_146,
        _UnAssigned_147,
        _UnAssigned_148,
        _UnAssigned_149,
        _UnAssigned_150,
        _UnAssigned_151,
        _UnAssigned_152,
        _UnAssigned_153,
        _UnAssigned_154,
        _UnAssigned_155,
        _UnAssigned_156,
        _UnAssigned_157,
        _UnAssigned_158,
        _UnAssigned_159,
        _UnAssigned_160,
        _UnAssigned_161,
        _UnAssigned_162,
        _UnAssigned_163,
        _UnAssigned_164,
        _UnAssigned_165,
        _UnAssigned_166,
        _UnAssigned_167,
        _UnAssigned_168,
        _UnAssigned_169,
        _UnAssigned_170,
        _UnAssigned_171,
        _UnAssigned_172,
        _UnAssigned_173,
        _UnAssigned_174,
        _UnAssigned_175,
        _UnAssigned_176,
        _UnAssigned_177,
        _UnAssigned_178,
        _UnAssigned_179,
        _UnAssigned_180,
        _UnAssigned_181,
        _UnAssigned_182,
        _UnAssigned_183,
        _UnAssigned_184,
        _UnAssigned_185,
        _UnAssigned_186,
        _UnAssigned_187,
        _UnAssigned_188,
        _UnAssigned_189,
        _UnAssigned_190,
        _UnAssigned_191,
        _UnAssigned_192,
        _UnAssigned_193,
        _UnAssigned_194,
        _UnAssigned_195,
        _UnAssigned_196,
        _UnAssigned_197,
        _UnAssigned_198,
        _UnAssigned_199,
        _UnAssigned_200,
        _UnAssigned_201,
        _UnAssigned_202,
        _UnAssigned_203,
        _UnAssigned_204,
        _UnAssigned_205,
        _UnAssigned_206,
        _UnAssigned_207,
        _UnAssigned_208,
        _UnAssigned_209,
        _UnAssigned_210,
        _UnAssigned_211,
        _UnAssigned_212,
        _UnAssigned_213,
        _UnAssigned_214,
        _UnAssigned_215,
        _UnAssigned_216,
        _UnAssigned_217,
        _UnAssigned_218,
        _UnAssigned_219,
        _UnAssigned_220,
        _UnAssigned_221,
        _UnAssigned_222,
        _UnAssigned_223,
        _UnAssigned_224,
        _UnAssigned_225,
        _UnAssigned_226,
        _UnAssigned_227,
        _UnAssigned_228,
        _UnAssigned_229,
        _UnAssigned_230,
        _UnAssigned_231,
        _UnAssigned_232,
        _UnAssigned_233,
        _UnAssigned_234,
        _UnAssigned_235,
        _UnAssigned_236,
        _UnAssigned_237,
        _UnAssigned_238,
        _UnAssigned_239,
        _UnAssigned_240,
        _UnAssigned_241,
        _UnAssigned_242,
        _UnAssigned_243,
        _UnAssigned_244,
        _UnAssigned_245,
        _UnAssigned_246,
        _UnAssigned_247,
        _UnAssigned_248,
        _UnAssigned_249,
        _UnAssigned_250,
        _UnAssigned_251,
        _UnAssigned_252,
        _UnAssigned_253,
        _UnAssigned_254,
        _UnAssigned_255
    }
    #endregion

    #region Classes
    public class TgaColorKey : ICloneable
    {
        public TgaColorKey()
        { }

        /// <summary>
        /// Make <see cref="TgaColorKey"/> from ARGB bytes.
        /// </summary>
        /// <param name="a">Alpha value.</param>
        /// <param name="r">Red value.</param>
        /// <param name="g">Green value.</param>
        /// <param name="b">Blue value.</param>
        public TgaColorKey(byte a, byte r, byte g, byte b)
        {
            A = a;
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Make <see cref="TgaColorKey"/> from ARGB bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes(byte[4]).</param>
        public TgaColorKey(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be equal " + Size + "!");

            var color = Color.FromArgb(BitConverter.ToInt32(bytes, 0));
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        /// <summary>
        /// Make <see cref="TgaColorKey"/> from <see cref="int"/>.
        /// </summary>
        /// <param name="argb">32bit ARGB integer color value.</param>
        public TgaColorKey(int argb)
        {
            var colorARGB = Color.FromArgb(argb);
            A = colorARGB.A;
            R = colorARGB.R;
            G = colorARGB.G;
            B = colorARGB.B;
        }

        /// <summary>
        /// Make <see cref="TgaColorKey"/> from <see cref="Color"/>.
        /// </summary>
        /// <param name="color">GDI+ <see cref="Color"/> value.</param>
        public TgaColorKey(Color color)
        {
            A = color.A;
            R = color.R;
            G = color.G;
            B = color.B;
        }

        /// <summary>
        /// Gets or sets alpha color value.
        /// </summary>
        public byte A { get; set; }

        /// <summary>
        /// Gets or sets red color value.
        /// </summary>
        public byte R { get; set; }

        /// <summary>
        /// Gets or sets green color value.
        /// </summary>
        public byte G { get; set; }

        /// <summary>
        /// Gets or sets blue color value.
        /// </summary>
        public byte B { get; set; }

        /// <summary>
        /// Gets TGA Field size in bytes.
        /// </summary>
        public const int Size = 4;

        /// <summary>
        /// Make full independed copy of <see cref="TgaColorKey"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaColorKey"/></returns>
        public TgaColorKey Clone() => new TgaColorKey(A, R, G, B);

        /// <summary>
        /// Make full independed copy of <see cref="TgaColorKey"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaColorKey"/></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaColorKey value && TgaColorKey.Equals(this, value);

        public bool Equals(TgaColorKey item) => A == item.A && R == item.R && G == item.G && B == item.B;

        public static bool operator ==(TgaColorKey item1, TgaColorKey item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaColorKey item1, TgaColorKey item2) => !(item1 == item2);

        public override int GetHashCode() => ToInt().GetHashCode();

        /// <summary>
        /// Gets <see cref="TgaColorKey"/> like string.
        /// </summary>
        /// <returns>String in ARGB format.</returns>
        public override string ToString()
        {
            return string.Format("{0}={1}, {2}={3}, {4}={5}, {6}={7}",
                nameof(A), A, nameof(R), R, nameof(G), G, nameof(B), B);
        }

        /// <summary>
        /// Convert <see cref="TgaColorKey"/> to byte array.
        /// </summary>
        /// <returns>Byte array with length = 4.</returns>
        public byte[] ToBytes() => BitConverter.GetBytes(ToInt());

        /// <summary>
        /// Gets <see cref="TgaColorKey"/> like GDI+ <see cref="Color"/>.
        /// </summary>
        /// <returns><see cref="Color"/> value of <see cref="TgaColorKey"/>.</returns>
        public Color ToColor() => Color.FromArgb(A, R, G, B);

        /// <summary>
        /// Gets <see cref="TgaColorKey"/> like ARGB <see cref="int"/>.
        /// </summary>
        /// <returns>ARGB <see cref="int"/> value of <see cref="TgaColorKey"/>.</returns>
        public int ToInt() => ToColor().ToArgb();
    }

    /// <summary>
    /// This field (5 bytes) and its sub-fields describe the color map (if any) used for the image.
    /// If the Color Map Type field is set to zero, indicating that no color map exists, then
    /// these 5 bytes should be set to zero. These bytes always must be written to the file.
    /// </summary>
    public class TgaColorMapSpec : ICloneable
    {
        /// <summary>
        /// Make new <see cref="TgaColorMapSpec"/>.
        /// </summary>
        public TgaColorMapSpec()
        { }

        /// <summary>
        /// Make <see cref="TgaColorMapSpec"/> from bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes(byte[5]).</param>
        public TgaColorMapSpec(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be equal " + Size + "!");

            FirstEntryIndex = BitConverter.ToUInt16(bytes, 0);
            ColorMapLength = BitConverter.ToUInt16(bytes, 2);
            ColorMapEntrySize = (TgaColorMapEntrySize)bytes[4];
        }

        /// <summary>
        /// Field 4.1 (2 bytes):
        /// Index of the first color map entry. Index refers to the starting entry in loading
        /// the color map.
        /// <para>Example: If you would have 1024 entries in the entire color map but you only
        /// need to store 72 of those entries, this field allows you to start in the middle of
        /// the color-map (e.g., position 342).</para>
        /// </summary>
        public ushort FirstEntryIndex { get; set; }

        /// <summary>
        /// Field 4.2 (2 bytes):
        /// Total number of color map entries included.
        /// </summary>
        public ushort ColorMapLength { get; set; }

        /// <summary>
        /// Field 4.3 (1 byte):
        /// Establishes the number of bits per entry. Typically 15, 16, 24 or 32-bit values are used.
        /// <para>When working with VDA or VDA/D cards it is preferred that you select 16 bits(5 bits
        /// per primary with 1 bit to select interrupt control) and set the 16th bit to 0 so that the
        /// interrupt bit is disabled. Even if this field is set to 15 bits(5 bits per primary) you
        /// must still parse the color map data 16 bits at a time and ignore the 16th bit.</para>
        /// <para>When working with a TARGA M8 card you would select 24 bits (8 bits per primary)
        /// since the color map is defined as 256 entries of 24 bit color values.</para>
        /// When working with a TrueVista card(ATVista or NuVista) you would select 24-bit(8 bits per
        /// primary) or 32-bit(8 bits per primary including Alpha channel) depending on your
        /// application’s use of look-up tables. It is suggested that when working with 16-bit and
        /// 32-bit color images, you store them as True-Color images and do not use the color map 
        /// field to store look-up tables. Please refer to the TGA Extensions for fields better suited
        /// to storing look-up table information.
        /// </summary>
        public TgaColorMapEntrySize ColorMapEntrySize { get; set; } = TgaColorMapEntrySize.Other;

        /// <summary>
        /// Gets TGA Field size in bytes.
        /// </summary>
        public const int Size = 5;

        /// <summary>
        /// Make full independed copy of <see cref="TgaColorMapSpec"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaColorMapSpec"/></returns>
        public TgaColorMapSpec Clone() => new TgaColorMapSpec(ToBytes());

        /// <summary>
        /// Make full independed copy of <see cref="TgaColorMapSpec"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaColorMapSpec"/></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaColorMapSpec value && TgaColorMapSpec.Equals(this, value);

        public bool Equals(TgaColorMapSpec item)
        {
            return FirstEntryIndex == item.FirstEntryIndex &&
                ColorMapLength == item.ColorMapLength &&
                ColorMapEntrySize == item.ColorMapEntrySize;
        }

        public static bool operator ==(TgaColorMapSpec item1, TgaColorMapSpec item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaColorMapSpec item1, TgaColorMapSpec item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                return (FirstEntryIndex << 16 | ColorMapLength).GetHashCode() ^ ColorMapEntrySize.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format("{0}={1}, {2}={3}, {4}={5}", nameof(FirstEntryIndex), FirstEntryIndex,
                nameof(ColorMapLength), ColorMapLength, nameof(ColorMapEntrySize), ColorMapEntrySize);
        }

        /// <summary>
        /// Convert ColorMapSpec to byte array.
        /// </summary>
        /// <returns>Byte array with length = 5.</returns>
        public byte[] ToBytes() => BitConverterExt.ToBytes(FirstEntryIndex, ColorMapLength, (byte)ColorMapEntrySize);
    }

    public class TgaComment : ICloneable
    {
        private const int StrNLen = 80; //80 ASCII chars + 1 '\0' = 81 per SrtN!

        public TgaComment()
        { }

        public TgaComment(string str, char blankSpaceChar = '\0')
        {
            OriginalString = str ?? throw new ArgumentNullException(nameof(str) + " = null!");
            BlankSpaceChar = blankSpaceChar;
        }

        public TgaComment(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be equal " + Size + "!");

            var s = Encoding.ASCII.GetString(bytes, 0, StrNLen);
            s += Encoding.ASCII.GetString(bytes, 81, StrNLen);
            s += Encoding.ASCII.GetString(bytes, 162, StrNLen);
            s += Encoding.ASCII.GetString(bytes, 243, StrNLen);

            switch (s[s.Length - 1])
            {
                case '\0':
                case ' ':
                    BlankSpaceChar = s[s.Length - 1];
                    OriginalString = s.TrimEnd(new char[] { s[s.Length - 1] });
                    break;
                default:
                    OriginalString = s;
                    break;
            }
        }

        /// <summary>
        /// Gets TGA Field size in bytes.
        /// </summary>
        public const int Size = 81 * 4;

        public string OriginalString { get; set; } = string.Empty;

        public char BlankSpaceChar { get; set; } = TgaString.DefaultBlankSpaceChar;

        /// <summary>
        /// Make full independed copy of <see cref="TgaComment"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaComment"/></returns>
        public TgaComment Clone() => new TgaComment(OriginalString, BlankSpaceChar);

        /// <summary>
        /// Make full independed copy of <see cref="TgaComment"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaComment"/></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaComment value && TgaComment.Equals(this, value);

        public bool Equals(TgaComment item) => OriginalString == item.OriginalString && BlankSpaceChar == item.BlankSpaceChar;

        public static bool operator ==(TgaComment item1, TgaComment item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaComment item1, TgaComment item2) => !(item1 == item2);

        public override int GetHashCode() => OriginalString.GetHashCode() ^ BlankSpaceChar.GetHashCode();

        /// <summary>
        /// Get ASCII-Like string with string-terminators, example: "Line1 \0\0 Line2 \0\0\0".
        /// </summary>
        /// <returns>String with replaced string-terminators to "\0".</returns>
        public override string ToString() => Encoding.ASCII.GetString(ToBytes()).Replace("\0", @"\0");

        /// <summary>
        /// Get ASCII-Like string to first string-terminator, example:
        /// "Some string \0 Some Data \0" - > "Some string".
        /// </summary>
        /// <returns>String to first string-terminator.</returns>
        public string GetString()
        {
            var str = Encoding.ASCII.GetString(ToBytes());
            for (var i = 1; i < 4; i++)
                str = str.Insert((StrNLen + 1) * i + i - 1, "\n");
            return str.Replace("\0", string.Empty).TrimEnd(new char[] { '\n' });
        }

        /// <summary>
        /// Convert <see cref="TgaComment"/> to byte array.
        /// </summary>
        /// <returns>Byte array, every byte is ASCII symbol.</returns>
        public byte[] ToBytes() => ToBytes(OriginalString, BlankSpaceChar);

        /// <summary>
        /// Convert <see cref="TgaComment"/> to byte array.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <param name="blankSpaceChar">Char for filling blank space in string.</param>
        /// <returns>Byte array, every byte is ASCII symbol.</returns>
        public static byte[] ToBytes(string str, char blankSpaceChar = '\0')
        {
            var c = new char[81 * 4];

            for (var i = 0; i < c.Length; i++)
            {
                if ((i + 82) % 81 == 0)
                    c[i] = TgaString.DefaultEndingChar;
                else
                {
                    var index = i - i / 81;
                    c[i] = index < str.Length ? str[index] : blankSpaceChar;
                }
            }
            return Encoding.ASCII.GetBytes(c);
        }
    }

    public class TgaDateTime : ICloneable
    {
        /// <summary>
        /// Make empty <see cref="TgaDateTime"/>.
        /// </summary>
        public TgaDateTime()
        { }

        /// <summary>
        /// Make <see cref="TgaDateTime"/> from <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateAndTime">Some <see cref="DateTime"/> variable.</param>
        public TgaDateTime(DateTime dateAndTime)
        {
            Month = (ushort)dateAndTime.Month;
            Day = (ushort)dateAndTime.Day;
            Year = (ushort)dateAndTime.Year;
            Hour = (ushort)dateAndTime.Hour;
            Minute = (ushort)dateAndTime.Minute;
            Second = (ushort)dateAndTime.Second;
        }

        /// <summary>
        /// Make <see cref="TgaDateTime"/> from ushort values.
        /// </summary>
        /// <param name="month">Month (1 - 12).</param>
        /// <param name="day">Day (1 - 31).</param>
        /// <param name="year">Year (4 digit, ie. 1989).</param>
        /// <param name="hour">Hour (0 - 23).</param>
        /// <param name="minute">Minute (0 - 59).</param>
        /// <param name="second">Second (0 - 59).</param>
        public TgaDateTime(ushort month, ushort day, ushort year, ushort hour, ushort minute, ushort second)
        {
            Month = month;
            Day = day;
            Year = year;
            Hour = hour;
            Minute = minute;
            Second = second;
        }

        /// <summary>
        /// Make <see cref="TgaDateTime"/> from bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes(byte[12]).</param>
        public TgaDateTime(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            else if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes) + " must be equal " + Size + "!");

            Month = BitConverter.ToUInt16(bytes, 0);
            Day = BitConverter.ToUInt16(bytes, 2);
            Year = BitConverter.ToUInt16(bytes, 4);
            Hour = BitConverter.ToUInt16(bytes, 6);
            Minute = BitConverter.ToUInt16(bytes, 8);
            Second = BitConverter.ToUInt16(bytes, 10);
        }

        /// <summary>
        /// Gets or Sets month (1 - 12).
        /// </summary>
        public ushort Month { get; set; }

        /// <summary>
        /// Gets or Sets day (1 - 31).
        /// </summary>
        public ushort Day { get; set; }

        /// <summary>
        /// Gets or Sets year (4 digit, ie. 1989).
        /// </summary>
        public ushort Year { get; set; }

        /// <summary>
        /// Gets or Sets hour (0 - 23).
        /// </summary>
        public ushort Hour { get; set; }

        /// <summary>
        /// Gets or Sets minute (0 - 59).
        /// </summary>
        public ushort Minute { get; set; }

        /// <summary>
        /// Gets or Sets second (0 - 59).
        /// </summary>
        public ushort Second { get; set; }

        /// <summary>
        /// Gets TGA Field size in bytes.
        /// </summary>
        public const int Size = 12;

        /// <summary>
        /// Make full independed copy of <see cref="TgaDateTime"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaDateTime"/></returns>
        public TgaDateTime Clone() => new TgaDateTime(Month, Day, Year, Hour, Minute, Second);

        /// <summary>
        /// Make full independed copy of <see cref="TgaDateTime"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaDateTime"/></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaDateTime value && TgaDateTime.Equals(this, value);

        public bool Equals(TgaDateTime item)
        {
            return 
                Month == item.Month &&
                Day == item.Day &&
                Year == item.Year &&
                Hour == item.Hour &&
                Minute == item.Minute &&
                Second == item.Second;
        }

        public static bool operator ==(TgaDateTime item1, TgaDateTime item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaDateTime item1, TgaDateTime item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + (Month << 16 | Hour).GetHashCode();
                hash = hash * 23 + (Day << 16 | Minute).GetHashCode();
                hash = hash * 23 + (Year << 16 | Second).GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Gets <see cref="TgaDateTime"/> like string.
        /// </summary>
        /// <returns>String in "1990.01.23 1:02:03" format.</returns>
        public override string ToString() => string.Format("{0:D4}.{1:D2}.{2:D2} {3}:{4:D2}:{5:D2}", Year, Month, Day, Hour, Minute, Second);

        /// <summary>
        /// Convert <see cref="TgaDateTime"/> to byte array.
        /// </summary>
        /// <returns>Byte array with length = 12.</returns>
        public byte[] ToBytes() => BitConverterExt.ToBytes(Month, Day, Year, Hour, Minute, Second);

        /// <summary>
        /// Gets <see cref="TgaDateTime"/> like <see cref="DateTime"/>.
        /// </summary>
        /// <returns><see cref="DateTime"/> value of <see cref="TgaDateTime"/>.</returns>
        public DateTime ToDateTime() => new DateTime(Year, Month, Day, Hour, Minute, Second);
    }

    public class TgaDevEntry : ICloneable
    {
        /// <summary>
        /// Make empty <see cref="TgaDevEntry"/>.
        /// </summary>
        public TgaDevEntry()
        { }

        /// <summary>
        /// Make <see cref="TgaDevEntry"/> from other <see cref="TgaDevEntry"/>.
        /// </summary>
        /// <param name="entry">Some <see cref="TgaDevEntry"/> variable.</param>
        public TgaDevEntry(TgaDevEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException();

            Tag = entry.Tag;
            Offset = entry.Offset;
            Data = BitConverterExt.ToBytes(entry.Data);
        }

        /// <summary>
        /// Make <see cref="TgaDevEntry"/> from <see cref="Tag"/>, <see cref="Offset"/> and <see cref="FieldSize"/>.
        /// </summary>
        /// <param name="tag">TAG ID (0 - 65535). See <see cref="Tag"/>.</param>
        /// <param name="offset">TAG file offset in bytes. See <see cref="Offset"/>.</param>
        /// <param name="data">This is DevEntry Field Data. See <see cref="Data"/>.</param>
        public TgaDevEntry(ushort tag, uint offset, byte[] data = null)
        {
            Tag = tag;
            Offset = offset;
            Data = data;
        }

        /// <summary>
        /// Make <see cref="TgaDevEntry"/> from bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes(byte[6] or bigger, if <see cref="Data"/> exist).</param>
        public TgaDevEntry(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            else if (bytes.Length < 6)
                throw new ArgumentOutOfRangeException(nameof(bytes) + " must be >= 6!");

            Tag = BitConverter.ToUInt16(bytes, 0);
            Offset = BitConverter.ToUInt32(bytes, 2);

            if (bytes.Length > 6)
                Data = BitConverterExt.GetElements(bytes, 6, bytes.Length - 6);
        }

        /// <summary>
        /// Each TAG is a value in the range of 0 to 65535. Values from 0 - 32767 are available for developer use,
        /// while values from 32768 - 65535 are reserved for Truevision.
        /// </summary>
        public ushort Tag { get; set; }

        /// <summary>
        /// This OFFSET is a number of bytes from the beginning of the file to the start of the field
        /// referenced by the tag.
        /// </summary>
        public uint Offset { get; set; }

        /// <summary>
        /// Field DATA.
        /// Although the size and format of the actual Developer Area fields are totally up to the developer,
        /// please define your formats to address future considerations you might have concerning your fields.
        /// This means that if you anticipate changing a field, build flexibility into the format to make these
        /// changes easy on other developers.Major changes to an existing TAG’s definition should never happen.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// The FIELD SIZE is a number of bytes in the field. Same like: <see cref="Data.Length"/>,
        /// if <see cref="Data"/> is null, return -1.
        /// </summary>
        public int FieldSize => Data?.Length ?? -1;

        /// <summary>
        /// Gets TGA <see cref="TgaDevEntry"/> size in bytes (Always constant and equal 10!).
        /// It is not <see cref="FieldSize"/>! It is just size of entry sizeof(ushort + uint + uint).
        /// </summary>
        public const int Size = 10;

        /// <summary>
        /// Make full independed copy of <see cref="TgaDevEntry"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaDevEntry"/></returns>
        public TgaDevEntry Clone() => new TgaDevEntry(this);

        /// <summary>
        /// Make full independed copy of <see cref="TgaDevEntry"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaDevEntry"/></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaDevEntry value && TgaDevEntry.Equals(this, value);

        public bool Equals(TgaDevEntry item)
        {
            return Tag == item.Tag &&
                Offset == item.Offset &&
                BitConverterExt.IsArraysEqual(Data, item.Data);
        }

        public static bool operator ==(TgaDevEntry item1, TgaDevEntry item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaDevEntry item1, TgaDevEntry item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Tag.GetHashCode();
                hash = hash * 23 + Offset.GetHashCode();

                if (Data != null)
                {
                    for (var i = 0; i < Data.Length; i++)
                        hash = hash * 23 + Data[i].GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Gets <see cref="TgaDevEntry"/> like string.
        /// </summary>
        /// <returns>String in "Tag={0}, Offset={1}, FieldSize={2}" format.</returns>
        public override string ToString()
        {
            return string.Format("{0}={1}, {1}={2}, {3}={4}", nameof(Tag), Tag,
                nameof(Offset), Offset, nameof(FieldSize), FieldSize);
        }

        /// <summary>
        /// Convert <see cref="TgaDevEntry"/> to byte array. (Not include <see cref="Data"/>!).
        /// </summary>
        /// <returns>Byte array with length = 10.</returns>
        public byte[] ToBytes() => BitConverterExt.ToBytes(Tag, Offset, Data == null ? 0 : Data.Length);
    } //Not full ToBytes()

    public class TgaFraction : ICloneable
    {
        /// <summary>
        /// Make <see cref="TgaFraction"/> from <see cref="Numerator"/> and <see cref="Denominator"/>.
        /// </summary>
        /// <param name="numerator">Numerator value.</param>
        /// <param name="denominator">Denominator value.</param>
        public TgaFraction(ushort numerator = 0, ushort denominator = 0)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        /// <summary>
        /// Make <see cref="TgaFraction"/> from bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes(byte[4]).</param>
        public TgaFraction(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be equal " + Size + "!");

            Numerator = BitConverter.ToUInt16(bytes, 0);
            Denominator = BitConverter.ToUInt16(bytes, 2);
        }

        /// <summary>
        /// Gets or sets numerator value.
        /// </summary>
        public ushort Numerator { get; set; }

        /// <summary>
        /// Gets or sets denominator value.
        /// </summary>
        public ushort Denominator { get; set; }

        /// <summary>
        /// Get aspect ratio = <see cref="Numerator"/> / <see cref="Denominator"/>.
        /// </summary>
        public float AspectRatio
        {
            get
            {
                if (Numerator == Denominator)
                    return 1f;

                return Numerator / (float)Denominator;
            }
        }

        /// <summary>
        /// Gets Empty <see cref="TgaFraction"/>, all values are 0.
        /// </summary>
        public static readonly TgaFraction Empty = new TgaFraction();

        /// <summary>
        /// Gets One <see cref="TgaFraction"/>, all values are 1 (ones, 1 / 1 = 1).
        /// </summary>
        public static readonly TgaFraction One = new TgaFraction(1, 1);

        /// <summary>
        /// Gets TGA Field size in bytes.
        /// </summary>
        public const int Size = 4;

        /// <summary>
        /// Make full independed copy of <see cref="TgaFraction"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaFraction"/></returns>
        public TgaFraction Clone() => new TgaFraction(Numerator, Denominator);

        /// <summary>
        /// Make full independed copy of <see cref="TgaFraction"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaFraction"/></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaFraction value && TgaFraction.Equals(this, value);

        public bool Equals(TgaFraction item) => Numerator == item.Numerator && Denominator == item.Denominator;

        public static bool operator ==(TgaFraction item1, TgaFraction item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaFraction item1, TgaFraction item2) => !(item1 == item2);

        public override int GetHashCode() => (Numerator << 16 | Denominator).GetHashCode();

        /// <summary>
        /// Gets <see cref="TgaFraction"/> like string.
        /// </summary>
        /// <returns>String in "Numerator=1, Denominator=2" format.</returns>
        public override string ToString()
        {
            return string.Format("{0}={1}, {2}={3}", nameof(Numerator), Numerator,
                nameof(Denominator), Denominator);
        }

        /// <summary>
        /// Convert <see cref="TgaFraction"/> to byte array.
        /// </summary>
        /// <returns>Byte array with length = 4.</returns>
        public byte[] ToBytes() => BitConverterExt.ToBytes(Numerator, Denominator);
    }

    /// <summary>
    /// Contains image origin bits and alpha channel bits(or number of overlay bits)
    /// </summary>
    public class TgaImageDescriptor : ICloneable
    {
        /// <summary>
        /// Make empty <see cref="TgaImageDescriptor"/>.
        /// </summary>
        public TgaImageDescriptor()
        { }

        /// <summary>
        /// Make <see cref="TgaImageDescriptor"/> from bytes.
        /// </summary>
        /// <param name="b">ImageDescriptor byte with reserved 7-6 bits, bits 5-4 used for
        /// <see cref="ImageOrigin"/>, 3-0 used as alpha channel bits or number of overlay bits.</param>
        public TgaImageDescriptor(byte b)
        {
            ImageOrigin = (TgaImgOrigin)((b & 0x30) >> 4);
            AlphaChannelBits = (byte)(b & 0x0F);
        }

        /// <summary>
        /// Gets or Sets Image Origin bits (select from enum only, don'n use 5-4 bits!).
        /// </summary>
        public TgaImgOrigin ImageOrigin { get; set; }

        /// <summary>
        /// Gets or Sets alpha channel bits or number of overlay bits.
        /// </summary>
        public byte AlphaChannelBits { get; set; }

        /// <summary>
        /// Gets TGA Field size in bytes.
        /// </summary>
        public const int Size = 1;

        /// <summary>
        /// Make full copy of <see cref="TgaImageDescriptor"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaImageDescriptor"/>.</returns>
        public TgaImageDescriptor Clone() => new TgaImageDescriptor(ToByte());

        /// <summary>
        /// Make full copy of <see cref="TgaImageDescriptor"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaImageDescriptor"/>.</returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaImageDescriptor value && TgaImageDescriptor.Equals(this, value);

        public bool Equals(TgaImageDescriptor item) => ImageOrigin == item.ImageOrigin && AlphaChannelBits == item.AlphaChannelBits;

        public static bool operator ==(TgaImageDescriptor item1, TgaImageDescriptor item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaImageDescriptor item1, TgaImageDescriptor item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)ImageOrigin << 4 | AlphaChannelBits).GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.Format("{0}={1}, {2}={3}, ImageDescriptor_AsByte={4}", nameof(ImageOrigin),
                ImageOrigin, nameof(AlphaChannelBits), AlphaChannelBits, ToByte());
        }

        /// <summary>
        /// Gets ImageDescriptor byte.
        /// </summary>
        /// <returns>ImageDescriptor byte with reserved 7-6 bits, bits 5-4 used for imageOrigin,
        /// 3-0 used as alpha channel bits or number of overlay bits.</returns>
        public byte ToByte() => (byte)(((int)ImageOrigin << 4) | AlphaChannelBits);
    }

    /// <summary>
    /// Image Specification - Field 5 (10 bytes):
    /// <para>This field and its sub-fields describe the image screen location, size and pixel depth.
    /// These information is always written to the file.</para>
    /// </summary>
    public class TgaImageSpec : ICloneable
    {
        public TgaImageSpec()
        { }

        /// <summary>
        /// Make ImageSpec from values.
        /// </summary>
        /// <param name="xOrigin">These specify the absolute horizontal coordinate for the lower
        /// left corner of the image as it is positioned on a display device having an origin at
        /// the lower left of the screen(e.g., the TARGA series).</param>
        /// <param name="yOrigin">These specify the absolute vertical coordinate for the lower
        /// left corner of the image as it is positioned on a display device having an origin at
        /// the lower left of the screen(e.g., the TARGA series).</param>
        /// <param name="imageWidth">This field specifies the width of the image in pixels.</param>
        /// <param name="imageHeight">This field specifies the height of the image in pixels.</param>
        /// <param name="pixelDepth">This field indicates the number of bits per pixel. This number
        /// includes the Attribute or Alpha channel bits. Common values are 8, 16, 24 and 32 but
        /// other pixel depths could be used.</param>
        /// <param name="imageDescriptor">Contains image origin bits and alpha channel bits
        /// (or number of overlay bits).</param>
        public TgaImageSpec(ushort xOrigin, ushort yOrigin, ushort imageWidth, ushort imageHeight,
            TgaPixelDepth pixelDepth, TgaImageDescriptor imageDescriptor)
        {
            X_Origin = xOrigin;
            Y_Origin = yOrigin;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            PixelDepth = pixelDepth;
            ImageDescriptor = imageDescriptor;
        }

        /// <summary>
        /// Make ImageSpec from bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes(byte[10]).</param>
        public TgaImageSpec(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be equal " + Size + "!");

            X_Origin = BitConverter.ToUInt16(bytes, 0);
            Y_Origin = BitConverter.ToUInt16(bytes, 2);
            ImageWidth = BitConverter.ToUInt16(bytes, 4);
            ImageHeight = BitConverter.ToUInt16(bytes, 6);
            PixelDepth = (TgaPixelDepth)bytes[8];
            ImageDescriptor = new TgaImageDescriptor(bytes[9]);
        }

        /// <summary>
        /// These specify the absolute horizontal coordinate for the lower left corner of the image
        /// as it is positioned on a display device having an origin at the lower left of the
        /// screen(e.g., the TARGA series).
        /// </summary>
        public ushort X_Origin { get; set; }

        /// <summary>
        /// These specify the absolute vertical coordinate for the lower left corner of the image
        /// as it is positioned on a display device having an origin at the lower left of the
        /// screen(e.g., the TARGA series).
        /// </summary>
        public ushort Y_Origin { get; set; }

        /// <summary>
        /// This field specifies the width of the image in pixels.
        /// </summary>
        public ushort ImageWidth { get; set; }

        /// <summary>
        /// This field specifies the height of the image in pixels.
        /// </summary>
        public ushort ImageHeight { get; set; }

        /// <summary>
        /// This field indicates the number of bits per pixel. This number includes the Attribute or
        /// Alpha channel bits. Common values are 8, 16, 24 and 32 but other pixel depths could be used.
        /// </summary>
        public TgaPixelDepth PixelDepth { get; set; } = TgaPixelDepth.Other;

        /// <summary>
        /// Contains image origin bits and alpha channel bits(or number of overlay bits).
        /// </summary>
        public TgaImageDescriptor ImageDescriptor { get; set; } = new TgaImageDescriptor();

        /// <summary>
        /// Gets TGA Field size in bytes.
        /// </summary>
        public const int Size = 10;

        /// <summary>
        /// Make full copy of <see cref="TgaImageDescriptor"/>.
        /// </summary>
        /// <returns></returns>
        public TgaImageSpec Clone() => new TgaImageSpec(ToBytes());

        /// <summary>
        /// Make full copy of <see cref="TgaImageDescriptor"/>.
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaImageSpec value && TgaImageSpec.Equals(this, value);

        public bool Equals(TgaImageSpec item)
        {
            return 
                X_Origin == item.X_Origin &&
                Y_Origin == item.Y_Origin &&
                ImageWidth == item.ImageWidth &&
                ImageHeight == item.ImageHeight &&
                PixelDepth == item.PixelDepth &&
                ImageDescriptor == item.ImageDescriptor;
        }

        public static bool operator ==(TgaImageSpec item1, TgaImageSpec item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaImageSpec item1, TgaImageSpec item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + X_Origin.GetHashCode();
                hash = hash * 23 + Y_Origin.GetHashCode();
                hash = hash * 23 + ImageWidth.GetHashCode();
                hash = hash * 23 + ImageHeight.GetHashCode();
                hash = hash * 23 + PixelDepth.GetHashCode();

                if (ImageDescriptor != null)
                    hash = hash * 23 + ImageDescriptor.GetHashCode();

                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}={1}, {2}={3}, {4}={5}, {6}={7}, {8}={9}, {10}={11}",
                nameof(X_Origin), X_Origin,
                nameof(Y_Origin), Y_Origin,
                nameof(ImageWidth), ImageWidth,
                nameof(ImageHeight), ImageHeight,
                nameof(PixelDepth), PixelDepth,
                nameof(ImageDescriptor), ImageDescriptor);
        }

        /// <summary>
        /// Convert <see cref="TgaImageSpec"/> to byte array.
        /// </summary>
        /// <returns>Byte array with length = 10.</returns>
        public byte[] ToBytes()
        {
            return BitConverterExt.ToBytes(X_Origin, Y_Origin, ImageWidth, ImageHeight,
                (byte)PixelDepth, ImageDescriptor == null ? byte.MinValue : ImageDescriptor.ToByte());
        }
    }

    /// <summary>
    /// Postage Stamp Image (MaxSize 64x64, uncompressed, PixelDepth like in full image).
    /// </summary>
    public class TgaPostageStampImage : ICloneable
    {
        public TgaPostageStampImage()
        { }

        /// <summary>
        /// Make <see cref="TgaPostageStampImage"/> from bytes array.
        /// </summary>
        /// <param name="bytes">Bytes array, first 2 bytes are <see cref="Width"/> and <see cref="Height"/>,
        /// next bytes - image data.</param>
        public TgaPostageStampImage(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length < 2)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be >= " + 2 + "!");

            Width = bytes[0];
            Height = bytes[1];

            if (bytes.Length > 2)
                Data = BitConverterExt.GetElements(bytes, 2, bytes.Length - 2);
        }

        /// <summary>
        /// Make <see cref="TgaPostageStampImage"/> from bytes and size.
        /// </summary>
        /// <param name="width">Image Width.</param>
        /// <param name="height">Image Height.</param>
        /// <param name="bytes">Postage Stamp Image Data.</param>
        public TgaPostageStampImage(byte width, byte height, byte[] bytes)
        {
            Width = width;
            Height = height;
            Data = bytes ?? throw new ArgumentNullException(nameof(bytes) + " = null!");
        }

        /// <summary>
        /// Postage Stamp Image Data
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Postage Stamp Image Width (maximum = 64).
        /// </summary>
        public byte Width { get; set; }

        /// <summary>
        /// Postage Stamp Image Height (maximum = 64).
        /// </summary>
        public byte Height { get; set; }

        /// <summary>
        /// Make full copy of <see cref="TgaPostageStampImage"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaPostageStampImage"/>.</returns>
        public TgaPostageStampImage Clone() => new TgaPostageStampImage(Width, Height, BitConverterExt.ToBytes(Data));

        /// <summary>
        /// Make full copy of <see cref="TgaPostageStampImage"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaPostageStampImage"/>.</returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaPostageStampImage value && TgaPostageStampImage.Equals(this, value);

        public bool Equals(TgaPostageStampImage item) => Width == item.Width && Height == item.Height && BitConverterExt.IsArraysEqual(Data, item.Data);

        public static bool operator ==(TgaPostageStampImage item1, TgaPostageStampImage item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaPostageStampImage item1, TgaPostageStampImage item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 27;
                hash = (13 * hash) + Width.GetHashCode();
                hash = (13 * hash) + Height.GetHashCode();
                if (Data != null)
                {
                    for (var i = 0; i < Data.Length; i++)
                        hash = (13 * hash) + Data[i].GetHashCode();
                }

                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}={1}, {2}={3}, DataLength={4}",
                nameof(Width), Width, nameof(Height), Height, Data == null ? -1 : Data.Length);
        }

        /// <summary>
        /// Convert <see cref="TgaPostageStampImage"/> to byte array.
        /// </summary>
        /// <returns>Byte array.</returns>
        public byte[] ToBytes() => BitConverterExt.ToBytes(Width, Height, Data);
    }

    public class TgaSoftVersion : ICloneable
    {
        private ushort versionNumber = 0;

        /// <summary>
        /// Gets Empty <see cref="TgaSoftVersion"/>, <see cref="VersionLetter"/> = ' ' (space).
        /// </summary>
        public TgaSoftVersion()
        { }

        /// <summary>
        /// Make <see cref="TgaSoftVersion"/> from string.
        /// </summary>
        /// <param name="str">Input string, example: "123d".</param>
        public TgaSoftVersion(string str)
        {
            if (str == null)
                throw new ArgumentNullException();
            if (str.Length < 3 || str.Length > 4)
                throw new ArgumentOutOfRangeException(nameof(str.Length) + " must be equal 3 or 4!");

            var res = ushort.TryParse(str.Substring(0, 3), out versionNumber);
            if (res && str.Length == 4)
                VersionLetter = str[3];
        }

        /// <summary>
        /// Make <see cref="TgaSoftVersion"/> from bytes.
        /// </summary>
        /// <param name="bytes">Bytes array (byte[3]).</param>
        public TgaSoftVersion(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be equal " + Size + "!");

            versionNumber = BitConverter.ToUInt16(bytes, 0);
            VersionLetter = Encoding.ASCII.GetString(bytes, 2, 1)[0];
        }

        public TgaSoftVersion(ushort versionNumber, char versionLetter = ' ')
        {
            this.versionNumber = versionNumber;
            VersionLetter = versionLetter;
        }

        public ushort VersionNumber
        {
            get => versionNumber;
            set => versionNumber = value;
        }

        public char VersionLetter { get; set; } = ' ';

        /// <summary>
        /// Gets TGA Field size in bytes.
        /// </summary>
        public const int Size = 3;

        /// <summary>
        /// Make full copy of <see cref="TgaSoftVersion"/>.
        /// </summary>
        /// <returns></returns>
        public TgaSoftVersion Clone() => new TgaSoftVersion(versionNumber, VersionLetter);

        /// <summary>
        /// Make full copy of <see cref="TgaSoftVersion"/>.
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaSoftVersion value && TgaSoftVersion.Equals(this, value);

        public bool Equals(TgaSoftVersion item) => versionNumber == item.versionNumber && VersionLetter == item.VersionLetter;

        public static bool operator ==(TgaSoftVersion item1, TgaSoftVersion item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaSoftVersion item1, TgaSoftVersion item2) => !(item1 == item2);

        public override int GetHashCode() => versionNumber.GetHashCode() ^ VersionLetter.GetHashCode();

        public override string ToString() => (versionNumber.ToString("000") + VersionLetter).TrimEnd(new char[] { ' ', '\0' });

        /// <summary>
        /// Convert <see cref="TgaSoftVersion"/> to byte array.
        /// </summary>
        /// <returns>Byte array, <see cref="VersionNumber"/> (2 bytes) and
        /// <see cref="VersionLetter"/> (ASCII symbol).</returns>
        public byte[] ToBytes() => ToBytes(versionNumber, VersionLetter);

        /// <summary>
        /// Convert <see cref="TgaSoftVersion"/> to byte array.
        /// </summary>
        /// <param name="VersionNumber">Set 123 for 1.23 version.</param>
        /// <param name="VersionLetter">Version letter, example: for 'a' - "1.23a".</param>
        /// <returns>Byte array, <see cref="VersionNumber"/> (2 bytes) and <see cref="VersionLetter"/> (ASCII symbol).</returns>
        public static byte[] ToBytes(ushort VersionNumber, char VersionLetter = ' ') => BitConverterExt.ToBytes(VersionNumber, Encoding.ASCII.GetBytes(VersionLetter.ToString()));
    }

    /// <summary>
    /// Use it for working with ASCII strings in TGA files.
    /// </summary>
    public class TgaString : ICloneable
    {
        public const string XFileSignatuteConst = "TRUEVISION-XFILE";
        public const string DotSymbolConst = ".";

        public TgaString(bool useEnding = false)
        {
            UseEndingChar = useEnding;
        }

        public TgaString(byte[] bytes, bool useEnding = false)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");

            Length = bytes.Length;
            UseEndingChar = useEnding;
            var s = Encoding.ASCII.GetString(bytes, 0, bytes.Length - (UseEndingChar ? 1 : 0));

            if (s.Length > 0)
            {
                switch (s[s.Length - 1])
                {
                    case '\0':
                    case ' ':
                        BlankSpaceChar = s[s.Length - 1];
                        OriginalString = s.TrimEnd(new char[] { s[s.Length - 1] });
                        break;
                    default:
                        OriginalString = s;
                        break;
                }
            }
        }

        public TgaString(int length, bool useEnding = false)
        {
            Length = length;
            UseEndingChar = useEnding;
        }

        public TgaString(string str, int length, bool useEnding = false, char blankSpaceChar = '\0')
        {
            OriginalString = str ?? throw new ArgumentNullException(nameof(str) + " = null!");
            Length = length;
            BlankSpaceChar = blankSpaceChar;
            UseEndingChar = useEnding;
        }

        public string OriginalString { get; set; } = string.Empty;

        public int Length { get; set; }

        public char BlankSpaceChar { get; set; } = DefaultBlankSpaceChar;

        public bool UseEndingChar { get; set; }

        /// <summary>
        /// Gets ending char, default '\0'.
        /// </summary>
        public static readonly char DefaultEndingChar = '\0';

        /// <summary>
        /// Gets blank space char, value = '\0'.
        /// </summary>
        public static readonly char DefaultBlankSpaceChar = '\0';

        /// <summary>
        /// Gets Empty <see cref="TgaString"/>.
        /// </summary>
        public static readonly TgaString Empty = new TgaString();

        /// <summary>
        /// Gets <see cref="TgaString"/> with <see cref="DefaultEndingChar"/> = '\0' and <see cref="UseEndingChar"/> = true.
        /// </summary>
        public static readonly TgaString ZeroTerminator = new TgaString(true);

        /// <summary>
        /// Gets "." <see cref="TgaString"/> with dot (period) symbol.
        /// </summary>
        public static readonly TgaString DotSymbol = new TgaString(DotSymbolConst, DotSymbolConst.Length);

        /// <summary>
        /// Gets "TRUEVISION-XFILE" <see cref="TgaString"/> (TGA File Format Version 2.0 signatute).
        /// </summary>
        public static readonly TgaString XFileSignatute = new TgaString(XFileSignatuteConst, XFileSignatuteConst.Length);

        /// <summary>
        /// Make full independed copy of <see cref="TgaString"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaString"/></returns>
        public TgaString Clone() => new TgaString(OriginalString, Length, UseEndingChar, BlankSpaceChar);

        /// <summary>
        /// Make full independed copy of <see cref="TgaString"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaString"/></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaString value && TgaString.Equals(this, value);

        public bool Equals(TgaString item)
        {
            return 
                OriginalString == item.OriginalString &&
                Length == item.Length &&
                BlankSpaceChar == item.BlankSpaceChar &&
                UseEndingChar == item.UseEndingChar;
        }

        public static bool operator ==(TgaString item1, TgaString item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaString item1, TgaString item2) => !(item1 == item2);

        public static TgaString operator +(TgaString item1, TgaString item2)
        {
            if (ReferenceEquals(item1, null) || ReferenceEquals(item2, null))
                throw new ArgumentNullException();

            return new TgaString(BitConverterExt.ToBytes(item1.ToBytes(), item2.ToBytes()));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + OriginalString.GetHashCode();
                hash = hash * 23 + Length.GetHashCode();
                hash = hash * 23 + BlankSpaceChar.GetHashCode();
                hash = hash * 23 + UseEndingChar.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Get ASCII-Like string with string-terminators, example: "Some string\0\0\0\0\0".
        /// </summary>
        /// <returns>String with replaced string-terminators to "\0".</returns>
        public override string ToString() => Encoding.ASCII.GetString(ToBytes()).Replace("\0", @"\0");

        /// <summary>
        /// Get ASCII-Like string to first string-terminator, example:
        /// "Some string \0 Some Data \0" - > "Some string".
        /// </summary>
        /// <returns>String to first string-terminator.</returns>
        public string GetString()
        {
            var str = Encoding.ASCII.GetString(ToBytes());
            var endIndex = str.IndexOf('\0');
            if (endIndex != -1)
                str = str.Substring(0, endIndex);
            return str;
        }

        /// <summary>
        /// Convert <see cref="TgaString"/> to byte array.
        /// </summary>
        /// <returns>Byte array, every byte is ASCII symbol.</returns>
        public byte[] ToBytes() => ToBytes(OriginalString, Length, UseEndingChar, BlankSpaceChar);

        /// <summary>
        /// Convert <see cref="TgaString"/> to byte array.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <param name="length">Length of output ASCII string with Ending char (if used).</param>
        /// <param name="useEnding">Add <see cref="EndingChr"/> to string or not?</param>
        /// <param name="blankSpaceChar">Char for filling blank space in string. If this char is '-' (only for example!),
        /// for string "ABC" with <see cref="Length"/> = 7, with <see cref="UseEnding"/> = true,
        /// <see cref="DefaultEndingChar"/> is '\0', result string is "ABC---\0".</param>
        /// <returns>Byte array, every byte is ASCII symbol.</returns>
        public static byte[] ToBytes(string str, int length, bool useEnding = true, char blankSpaceChar = '\0')
        {
            var c = new char[Math.Max(length, useEnding ? 1 : 0)];

            for (var i = 0; i < c.Length; i++)
                c[i] = i < str.Length ? str[i] : blankSpaceChar;

            if (useEnding)
                c[c.Length - 1] = DefaultEndingChar;

            return Encoding.ASCII.GetBytes(c);
        }
    }

    public class TgaTime : ICloneable
    {
        /// <summary>
        /// Make empty <see cref="TgaTime"/>.
        /// </summary>
        public TgaTime()
        { }

        /// <summary>
        /// Make <see cref="TgaTime"/> from <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="time">Some <see cref="TimeSpan"/> variable.</param>
        public TgaTime(TimeSpan time)
        {
            Hours = (ushort)time.TotalHours;
            Minutes = (ushort)time.Minutes;
            Seconds = (ushort)time.Seconds;
        }

        /// <summary>
        /// Make <see cref="TgaTime"/> from ushort values.
        /// </summary>
        /// <param name="hours">Hour (0 - 65535).</param>
        /// <param name="minutes">Minute (0 - 59).</param>
        /// <param name="seconds">Second (0 - 59).</param>
        public TgaTime(ushort hours, ushort minutes, ushort seconds)
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
        }

        /// <summary>
        /// Make <see cref="TgaTime"/> from bytes.
        /// </summary>
        /// <param name="bytes">Array of bytes(byte[6]).</param>
        public TgaTime(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            else if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes) + " must be equal " + Size + "!");

            Hours = BitConverter.ToUInt16(bytes, 0);
            Minutes = BitConverter.ToUInt16(bytes, 2);
            Seconds = BitConverter.ToUInt16(bytes, 4);
        }

        /// <summary>
        /// Gets or Sets hour (0 - 65535).
        /// </summary>
        public ushort Hours { get; set; }

        /// <summary>
        /// Gets or Sets minute (0 - 59).
        /// </summary>
        public ushort Minutes { get; set; }

        /// <summary>
        /// Gets or Sets second (0 - 59).
        /// </summary>
        public ushort Seconds { get; set; }

        /// <summary>
        /// Gets TGA Field size in bytes.
        /// </summary>
        public const int Size = 6;

        /// <summary>
        /// Make full independed copy of <see cref="TgaTime"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaTime"/></returns>
        public TgaTime Clone() => new TgaTime(Hours, Minutes, Seconds);

        /// <summary>
        /// Make full independed copy of <see cref="TgaTime"/>.
        /// </summary>
        /// <returns>Copy of <see cref="TgaTime"/></returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaTime value && TgaTime.Equals(this, value);

        public bool Equals(TgaTime item) => Hours == item.Hours && Minutes == item.Minutes && Seconds == item.Seconds;

        public static bool operator ==(TgaTime item1, TgaTime item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaTime item1, TgaTime item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + Hours.GetHashCode();
                hash = hash * 23 + (Minutes << 16 | Seconds).GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Gets <see cref="TgaTime"/> like string.
        /// </summary>
        /// <returns>String in "H:M:S" format.</returns>
        public override string ToString() => string.Format("{0}:{1}:{2}", Hours, Minutes, Seconds);

        /// <summary>
        /// Convert <see cref="TgaTime"/> to byte array.
        /// </summary>
        /// <returns>Byte array with length = 6.</returns>
        public byte[] ToBytes() => BitConverterExt.ToBytes(Hours, Minutes, Seconds);

        /// <summary>
        /// Gets <see cref="TgaTime"/> like <see cref="TimeSpan"/>.
        /// </summary>
        /// <returns><see cref="TimeSpan"/> value of <see cref="TgaTime"/>.</returns>
        public TimeSpan ToTimeSpan() => new TimeSpan(Hours, Minutes, Seconds);
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// File Header Area (18 bytes)
    /// </summary>
    public class TgaHeader : ICloneable
    {
        /// <summary>
        /// Make empty <see cref="TgaHeader"/>.
        /// </summary>
        public TgaHeader()
        { }

        /// <summary>
        /// Make <see cref="TgaHeader"/> from bytes.
        /// </summary>
        /// <param name="bytes">Bytes array (byte[18]).</param>
        public TgaHeader(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be equal " + Size + "!");

            IDLength = bytes[0];
            ColorMapType = (TgaColorMapType)bytes[1];
            ImageType = (TgaImageType)bytes[2];
            ColorMapSpec = new TgaColorMapSpec(BitConverterExt.GetElements(bytes, 3, TgaColorMapSpec.Size));
            ImageSpec = new TgaImageSpec(BitConverterExt.GetElements(bytes, 8, TgaImageSpec.Size));
        }

        /// <summary>
        /// ID Length - Field 1 (1 byte):
        /// This field identifies the number of bytes contained in the <see cref="ImageID"/> Field.
        /// The maximum number of characters is 255. A value of zero indicates that no Image ID
        /// field is included with the image.
        /// </summary>
        public byte IDLength { get; set; }

        /// <summary>
        /// Color Map Type - Field 2 (1 byte):
        /// This field indicates the type of color map (if any) included with the image.
        /// There are currently 2 defined values for this field:
        /// <para>0 - indicates that no color-map data is included with this image;</para>
        /// <para>1 - indicates that a color-map is included with this image.</para>
        /// </summary>
        public TgaColorMapType ColorMapType { get; set; } = TgaColorMapType.NoColorMap;

        /// <summary>
        /// Image Type - Field 3 (1 byte):
        /// <para>The TGA File Format can be used to store Pseudo-Color, True-Color and Direct-Color images
        /// of various pixel depths.</para>
        /// </summary>
        public TgaImageType ImageType { get; set; } = TgaImageType.NoImageData;

        /// <summary>
        /// Color Map Specification - Field 4 (5 bytes):
        /// <para>This field and its sub-fields describe the color map (if any) used for the image.
        /// If the Color Map Type field is set to zero, indicating that no color map exists, then
        /// these 5 bytes should be set to zero. These bytes always must be written to the file.</para>
        /// </summary>
        public TgaColorMapSpec ColorMapSpec { get; set; } = new TgaColorMapSpec();

        /// <summary>
        /// Image Specification - Field 5 (10 bytes):
        /// <para>This field and its sub-fields describe the image screen location, size and pixel depth.
        /// These information is always written to the file.</para>
        /// </summary>
        public TgaImageSpec ImageSpec { get; set; } = new TgaImageSpec();

        /// <summary>
        /// Gets TGA Header Section size in bytes.
        /// </summary>
        public const int Size = 18;

        /// <summary>
        /// Make full copy of <see cref="TgaHeader"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaHeader"/>.</returns>
        public TgaHeader Clone() => new TgaHeader(ToBytes());

        /// <summary>
        /// Make full copy of <see cref="TgaHeader"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaHeader"/>.</returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaHeader value && TgaHeader.Equals(this, value);

        public bool Equals(TgaHeader item)
        {
            return IDLength == item.IDLength &&
                ColorMapType == item.ColorMapType &&
                ImageType == item.ImageType &&
                ColorMapSpec == item.ColorMapSpec &&
                ImageSpec == item.ImageSpec;
        }

        public static bool operator ==(TgaHeader item1, TgaHeader item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaHeader item1, TgaHeader item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + (IDLength << 24 | (byte)ColorMapType << 8 | (byte)ImageType).GetHashCode();

                if (ColorMapSpec != null)
                    hash = hash * 23 + ColorMapSpec.GetHashCode();

                if (ImageSpec != null)
                    hash = hash * 23 + ImageSpec.GetHashCode();

                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}={1}, {2}={3}, {4}={5}, {6}={7}, {8}={9}",
                nameof(IDLength), IDLength,
                nameof(ColorMapType), ColorMapType,
                nameof(ImageType), ImageType,
                nameof(ColorMapSpec), ColorMapSpec,
                nameof(ImageSpec), ImageSpec);
        }

        /// <summary>
        /// Convert <see cref="TgaHeader"/> to byte array.
        /// </summary>
        /// <returns>Byte array with size equal <see cref="Size"/>.</returns>
        public byte[] ToBytes()
        {
            return BitConverterExt.ToBytes(IDLength, (byte)ColorMapType, (byte)ImageType,
                ColorMapSpec == null ? new byte[TgaColorMapSpec.Size] : ColorMapSpec.ToBytes(),
                ImageSpec == null ? new byte[TgaImageSpec.Size] : ImageSpec.ToBytes());
        }
    }

    /// <summary>
    /// Image Or ColorMap Area
    /// </summary>
    public class TgaImgOrColMap : ICloneable
    {
        /// <summary>
        /// Make empty <see cref="TgaImgOrColMap"/>.
        /// </summary>
        public TgaImgOrColMap()
        { }

        /// <summary>
        /// Make <see cref="TgaImgOrColMap"/> from arrays.
        /// </summary>
        /// <param name="imageID">This optional field contains identifying information about the image.
        /// The maximum length for this field is 255 bytes. Refer to <see cref="TgaHeader.IDLength"/>
        /// for the length of this field. If field 1 is set to Zero indicating that no Image ID exists
        /// then these bytes are not written to the file.</param>
        /// <param name="colorMapData">Color Map Data, see <see cref="ColorMapData"/> description.</param>
        /// <param name="imageData">Image Data, see <see cref="ImageData"/> description.</param>
        public TgaImgOrColMap(TgaString imageID, byte[] colorMapData, byte[] imageData)
        {
            ImageID = imageID;
            ColorMapData = colorMapData;
            ImageData = imageData;
        }

        /// <summary>
        /// Image ID - Field 6 (variable):
        /// <para>This optional field contains identifying information about the image. The maximum length
        /// for this field is 255 bytes. Refer to <see cref="TgaHeader.IDLength"/> for the length of this
        /// field. If field 1 is set to Zero indicating that no Image ID exists then these bytes are not
        /// written to the file. Can have text inside (ASCII).</para>
        /// </summary>
        public TgaString ImageID { get; set; }

        /// <summary>
        /// Color Map Data - Field 7 (variable):
        /// <para>If the Color Map Type(field 2) field is set to zero indicating that no Color-Map
        /// exists then this field will not be present (i.e., no bytes written to the file).</para>
        /// <para>This variable-length field contains the actual color map information (LUT data).
        /// Field 4.3 specifies the width in bits of each color map entry while Field 4.2 specifies
        /// the number of color map entries in this field. These two fields together are used to
        /// determine the number of bytes contained in field 7.</para>
        /// <para>Each color map entry is stored using an integral number of bytes.The RGB specification
        /// for each color map entry is stored in successive bit-fields in the multi-byte entries.
        /// Each color bit-field is assumed to be MIN(Field4.3/3, 8) bits in length. If Field 4.3
        /// contains 24, then each color specification is 8 bits in length; if Field 4.3 contains 32,
        /// then each color specification is also 8 bits (32/3 gives 10, but 8 is smaller).
        /// Unused bit(s) in the multi-byte entries are assumed to specify attribute bits. The
        /// attribute bit field is often called the Alpha Channel, Overlay Bit(s) or Interrupt Bit(s).</para>
        /// For the TARGA M-8, ATVista and NuVista, the number of bits in a color map specification is
        /// 24 (or 32). The red, green, and blue components are each represented by one byte.
        /// </summary>
        public byte[] ColorMapData { get; set; }

        /// <summary>
        /// Image Data - Field 8 (variable):
        /// <para>This field contains (Width)x(Height) pixels. Each pixel specifies image data in one
        /// of the following formats:</para>
        /// <para>a single color-map index for Pseudo-Color;
        /// Attribute, Red, Green and Blue ordered data for True-Color;
        /// and independent color-map indices for Direct-Color.</para>
        /// <para>The values for Width and Height are specified in Fields 5.3 and 5.4 respectively.
        /// The number of attribute and color-definition bits for each pixel are defined in Fields 5.6
        /// and 5.5, respectively.Each pixel is stored as an integral number of bytes.</para>
        /// </summary>
        public byte[] ImageData { get; set; }

        /// <summary>
        /// Make full copy of <see cref="TgaImgOrColMap"/>.
        /// </summary>
        /// <returns>Full independed copy of <see cref="TgaImgOrColMap"/>.</returns>
        public TgaImgOrColMap Clone()
        {
            return new TgaImgOrColMap(
                ImageID?.Clone(),
                ColorMapData == null ? null : (byte[])ColorMapData.Clone(),
                ImageData == null ? null : (byte[])ImageData.Clone());
        }

        /// <summary>
        /// Make full copy of <see cref="TgaImgOrColMap"/>.
        /// </summary>
        /// <returns>Full independed copy of <see cref="TgaImgOrColMap"/>.</returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaImgOrColMap value && TgaImgOrColMap.Equals(this, value);

        public bool Equals(TgaImgOrColMap item)
        {
            return ImageID == item.ImageID &&
                BitConverterExt.IsArraysEqual(ColorMapData, item.ColorMapData) &&
                BitConverterExt.IsArraysEqual(ImageData, item.ImageData);
        }

        public static bool operator ==(TgaImgOrColMap item1, TgaImgOrColMap item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaImgOrColMap item1, TgaImgOrColMap item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 27;

                if (ImageID != null)
                    hash = (13 * hash) + ImageID.GetHashCode();
                if (ColorMapData != null)
                {
                    for (var i = 0; i < ColorMapData.Length; i++)
                        hash = (13 * hash) + ColorMapData[i].GetHashCode();
                }

                if (ImageData != null)
                {
                    for (var i = 0; i < ImageData.Length; i++)
                        hash = (13 * hash) + ImageData[i].GetHashCode();
                }

                return hash;
            }
        }
    } //No ToBytes()

    /// <summary>
    /// Developer Area
    /// </summary> //?
    public class TgaDevArea : ICloneable
    {
        public TgaDevArea()
        { }

        public TgaDevArea(List<TgaDevEntry> entries)
        {
            Entries = entries ?? throw new ArgumentNullException(nameof(entries) + " = null!");
        }

        /// <summary>
        /// Developer Data - Field 9 (variable):
        /// </summary>
        public List<TgaDevEntry> Entries { get; set; } = new List<TgaDevEntry>();

        public int Count => Entries.Count;

        public TgaDevEntry this[int index]
        {
            get => Entries[index];
            set => Entries[index] = value;
        }

        /// <summary>
        /// Make full copy of <see cref="TgaDevArea"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaDevArea"/>.</returns>
        public TgaDevArea Clone()
        {
            if (Entries == null)
                return new TgaDevArea(null);

            var list = new List<TgaDevEntry>();
            for (var i = 0; i < Entries.Count; i++)
                list.Add(Entries[i].Clone());

            return new TgaDevArea(list);
        }

        /// <summary>
        /// Make full copy of <see cref="TgaDevArea"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaDevArea"/>.</returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaDevArea value && TgaDevArea.Equals(this, value);

        public bool Equals(TgaDevArea item) => BitConverterExt.IsListsEqual(Entries, item.Entries);

        public static bool operator ==(TgaDevArea item1, TgaDevArea item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaDevArea item1, TgaDevArea item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 27;
                if (Entries != null)
                {
                    for (var i = 0; i < Entries.Count; i++)
                        hash = (13 * hash) + Entries[i].GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Convert <see cref="TgaDevArea"/> (without Fields Data, only Directory!) to byte array.
        /// </summary>
        /// <returns>Byte array, Len = (NUMBER_OF_TAGS_IN_THE_DIRECTORY * 10) + 2 bytes in size.
        /// The "+ 2" includes the 2 bytes for the number of tags in the directory.</returns>
        public byte[] ToBytes()
        {
            if (Entries == null)
                throw new Exception(nameof(Entries) + " = null!");

            var numberOfEntries = (ushort)Math.Min(ushort.MaxValue, Entries.Count);
            var devDir = new List<byte>(BitConverter.GetBytes(numberOfEntries));

            for (var i = 0; i < Entries.Count; i++)
            {
                devDir.AddRange(BitConverter.GetBytes(Entries[i].Tag));
                devDir.AddRange(BitConverter.GetBytes(Entries[i].Offset));
                devDir.AddRange(BitConverter.GetBytes(Entries[i].FieldSize));
            }

            return devDir.ToArray();
        }
    } //Not full ToBytes()

    /// <summary>
    /// Extension Area
    /// </summary>
    public class TgaExtArea : ICloneable
    {
        public const int MinSize = 495; //bytes

        public TgaExtArea()
        { }

        /// <summary>
        /// Make <see cref="TgaExtArea"/> from bytes. Warning: <see cref="ScanLineTable"/>,
        /// <see cref="PostageStampImage"/>, <see cref="ColorCorrectionTable"/> not included,
        /// because thea are can be not in the Extension Area of TGA file!
        /// </summary>
        /// <param name="bytes">Bytes of <see cref="TgaExtArea"/>.</param>
        /// <param name="slt">Scan Line Table.</param>
        /// <param name="postImg">Postage Stamp Image.</param>
        /// <param name="cct">Color Correction Table.</param>
        public TgaExtArea(byte[] bytes, uint[] slt = null, TgaPostageStampImage postImg = null, ushort[] cct = null)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length < MinSize)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be >= " + MinSize + "!");

            ExtensionSize = BitConverter.ToUInt16(bytes, 0);
            AuthorName = new TgaString(BitConverterExt.GetElements(bytes, 2, 41), true);
            AuthorComments = new TgaComment(BitConverterExt.GetElements(bytes, 43, TgaComment.Size));
            DateTimeStamp = new TgaDateTime(BitConverterExt.GetElements(bytes, 367, TgaDateTime.Size));
            JobNameOrID = new TgaString(BitConverterExt.GetElements(bytes, 379, 41), true);
            JobTime = new TgaTime(BitConverterExt.GetElements(bytes, 420, TgaTime.Size));
            SoftwareID = new TgaString(BitConverterExt.GetElements(bytes, 426, 41), true);
            SoftVersion = new TgaSoftVersion(BitConverterExt.GetElements(bytes, 467, TgaSoftVersion.Size));
            KeyColor = new TgaColorKey(BitConverterExt.GetElements(bytes, 470, TgaColorKey.Size));
            PixelAspectRatio = new TgaFraction(BitConverterExt.GetElements(bytes, 474, TgaFraction.Size));
            GammaValue = new TgaFraction(BitConverterExt.GetElements(bytes, 478, TgaFraction.Size));
            ColorCorrectionTableOffset = BitConverter.ToUInt32(bytes, 482);
            PostageStampOffset = BitConverter.ToUInt32(bytes, 486);
            ScanLineOffset = BitConverter.ToUInt32(bytes, 490);
            AttributesType = (TgaAttrType)bytes[494];

            if (ExtensionSize > MinSize)
                OtherDataInExtensionArea = BitConverterExt.GetElements(bytes, 495, bytes.Length - MinSize);

            ScanLineTable = slt;
            PostageStampImage = postImg;
            ColorCorrectionTable = cct;
        }

        #region Properties
        /// <summary>
        /// Extension Size - Field 10 (2 Bytes):
        /// This field is a SHORT field which specifies the number of BYTES in the fixedlength portion of
        /// the Extension Area. For Version 2.0 of the TGA File Format, this number should be set to 495.
        /// If the number found in this field is not 495, then the file will be assumed to be of a
        /// version other than 2.0. If it ever becomes necessary to alter this number, the change
        /// will be controlled by Truevision, and will be accompanied by a revision to the TGA File
        /// Format with an accompanying change in the version number.
        /// </summary>
        public ushort ExtensionSize { get; set; } = MinSize;

        /// <summary>
        /// Author Name - Field 11 (41 Bytes):
        /// Bytes 2-42 - This field is an ASCII field of 41 bytes where the last byte must be a null
        /// (binary zero). This gives a total of 40 ASCII characters for the name. If the field is used,
        /// it should contain the name of the person who created the image (author). If the field is not
        /// used, you may fill it with nulls or a series of blanks(spaces) terminated by a null.
        /// The 41st byte must always be a null.
        /// </summary>
        public TgaString AuthorName { get; set; } = new TgaString(41, true);

        /// <summary>
        /// Author Comments - Field 12 (324 Bytes):
        /// Bytes 43-366 - This is an ASCII field consisting of 324 bytes which are organized as four lines
        /// of 80 characters, each followed by a null terminator.This field is provided, in addition to the
        /// original IMAGE ID field(in the original TGA format), because it was determined that a few
        /// developers had used the IMAGE ID field for their own purposes.This field gives the developer
        /// four lines of 80 characters each, to use as an Author Comment area. Each line is fixed to 81
        /// bytes which makes access to the four lines easy.Each line must be terminated by a null.
        /// If you do not use all 80 available characters in the line, place the null after the last
        /// character and blank or null fill the rest of the line. The 81st byte of each of the four
        /// lines must be null.
        /// </summary>
        public TgaComment AuthorComments { get; set; } = new TgaComment();

        /// <summary>
        /// Date/Time Stamp - Field 13 (12 Bytes):
        /// Bytes 367-378 - This field contains a series of 6 SHORT values which define the integer
        /// value for the date and time that the image was saved. This data is formatted as follows:
        /// <para>SHORT 0: Month(1 - 12)</para>
        /// <para>SHORT 1: Day(1 - 31)</para>
        /// <para>SHORT 2: Year(4 digit, ie. 1989)</para>
        /// <para>SHORT 3: Hour(0 - 23)</para>
        /// <para>SHORT 4: Minute(0 - 59)</para>
        /// <para>SHORT 5: Second(0 - 59)</para>
        /// Even though operating systems typically time- and date-stamp files, this feature is
        /// provided because the operating system may change the time and date stamp if the file is
        /// copied. By using this area, you are guaranteed an unmodified region for date and time
        /// recording. If the fields are not used, you should fill them with binary zeros (0).
        /// </summary>
        public TgaDateTime DateTimeStamp { get; set; } = new TgaDateTime();

        /// <summary>
        /// Job Name/ID - Field 14 (41 Bytes):
        /// Bytes 379-419 - This field is an ASCII field of 41 bytes where the last byte must be 
        /// a binary zero. This gives a total of 40 ASCII characters for the job name or the ID.
        /// If the field is used, it should contain a name or id tag which refers to the job with
        /// which the image was associated.This allows production companies (and others) to tie
        /// images with jobs by using this field as a job name (i.e., CITY BANK) or job id number
        /// (i.e., CITY023). If the field is not used, you may fill it with a null terminated series
        /// of blanks (spaces) or nulls. In any case, the 41st byte must be a null.
        /// </summary>
        public TgaString JobNameOrID { get; set; } = new TgaString(41, true);

        /// <summary>
        /// Job Time - Field 15 (6 Bytes):
        /// Bytes 420-425 - This field contains a series of 3 SHORT values which define the integer
        /// value for the job elapsed time when the image was saved.This data is formatted as follows:
        /// <para>SHORT 0: Hours(0 - 65535)</para>
        /// <para>SHORT 1: Minutes(0 - 59)</para>
        /// <para>SHORT 2: Seconds(0 - 59)</para>
        /// The purpose of this field is to allow production houses (and others) to keep a running total
        /// of the amount of time invested in a particular image. This may be useful for billing, costing,
        /// and time estimating. If the fields are not used, you should fill them with binary zeros (0).
        /// </summary>
        public TgaTime JobTime { get; set; } = new TgaTime();

        /// <summary>
        /// Software ID - Field 16 (41 Bytes):
        /// Bytes 426-466 - This field is an ASCII field of 41 bytes where the last byte must be
        /// a binary zero (null). This gives a total of 40 ASCII characters for the Software ID.
        /// The purpose of this field is to allow software to determine and record with what program
        /// a particular image was created.If the field is not used, you may fill it with a
        /// null terminated series of blanks (spaces) or nulls. The 41st byte must always be a null.
        /// </summary>
        public TgaString SoftwareID { get; set; } = new TgaString(41, true);

        /// <summary>
        /// Software Version - Field 17 (3 Bytes):
        /// Bytes 467-469 - This field consists of two sub-fields, a SHORT and an ASCII BYTE.
        /// The purpose of this field is to define the version of software defined by the
        /// "Software ID" field above. The SHORT contains the version number as a binary
        /// integer times 100.
        /// <para>Therefore, software version 4.17 would be the integer value 417.This allows for
        /// two decimal positions of sub-version.The ASCII BYTE supports developers who also
        /// tag a release letter to the end. For example, if the version number is 1.17b, then
        /// the SHORT would contain 117. and the ASCII BYTE would contain "b".
        /// The organization is as follows:</para>
        /// <para>SHORT (Bytes 0 - 1): Version Number * 100</para>
        /// <para>BYTE(Byte 2): Version Letter</para>
        /// If you do not use this field, set the SHORT to binary zero, and the BYTE to a space(" ")
        /// </summary>
        public TgaSoftVersion SoftVersion { get; set; } = new TgaSoftVersion();

        /// <summary>
        /// Key Color - Field 18 (4 Bytes):
        /// Bytes 470-473 - This field contains a long value which is the key color in effect at
        /// the time the image is saved. The format is in A:R:G:B where ‘A’ (most significant byte)
        /// is the alpha channel key color(if you don’t have an alpha channel in your application,
        /// keep this byte zero [0]).
        /// <para>The Key Color can be thought of as the ‘background color’ or ‘transparent color’.
        /// This is the color of the ‘non image’ area of the screen, and the same color that the
        /// screen would be cleared to if erased in the application. If you don’t use this field,
        /// set it to all zeros (0). Setting the field to all zeros is the same as selecting a key
        /// color of black.</para>
        /// A good example of a key color is the ‘transparent color’ used in TIPS™ for WINDOW loading/saving.
        /// </summary>
        public TgaColorKey KeyColor { get; set; } = new TgaColorKey();

        /// <summary>
        /// Pixel Aspect Ratio - Field 19 (4 Bytes):
        /// Bytes 474-477 - This field contains two SHORT sub-fields, which when taken together
        /// specify a pixel size ratio.The format is as follows:
        /// <para>SHORT 0: Pixel Ratio Numerator(pixel width)</para>
        /// <para>SHORT 1: Pixel Ratio Denominator(pixel height)</para>
        /// These sub-fields may be used to determine the aspect ratio of a pixel. This is useful when
        /// it is important to preserve the proper aspect ratio of the saved image. If the two values
        /// are set to the same non-zero value, then the image is composed of square pixels. A zero
        /// in the second sub-field (denominator) indicates that no pixel aspect ratio is specified.
        /// </summary>
        public TgaFraction PixelAspectRatio { get; set; } = TgaFraction.Empty;

        /// <summary>
        /// Gamma Value - Field 20 (4 Bytes):
        /// Bytes 478-481 - This field contains two SHORT sub-fields, which when taken together in a ratio,
        /// provide a fractional gamma value.The format is as follows:
        /// <para>SHORT 0: Gamma Numerator</para>
        /// <para>SHORT 1: Gamma Denominator</para>
        /// The resulting value should be in the range of 0.0 to 10.0, with only one decimal place of
        /// precision necessary. An uncorrected image (an image with no gamma) should have the value 1.0 as
        /// the result.This may be accomplished by placing thesame, non-zero values in both positions
        /// (i.e., 1/1). If you decide to totally ignore this field, please set the denominator (the second
        /// SHORT) to the value zero. This will indicate that the Gamma Value field is not being used.
        /// </summary>
        public TgaFraction GammaValue { get; set; } = TgaFraction.Empty;

        /// <summary>
        /// Color Correction Offset - Field 21 (4 Bytes):
        /// Bytes 482-485 - This field is a 4-byte field containing a single offset value. This is an offset
        /// from the beginning of the file to the start of the Color Correction table. This table may be
        /// written anywhere between the end of the Image Data field (field 8) and the start of the TGA
        /// File Footer. If the image has no Color Correction Table or if the Gamma Value setting is
        /// sufficient, set this value to zero and do not write a Correction Table anywhere.
        /// </summary>
        public uint ColorCorrectionTableOffset { get; set; }

        /// <summary>
        /// Postage Stamp Offset - Field 22 (4 Bytes):
        /// Bytes 486-489 - This field is a 4-byte field containing a single offset value. This is an offset
        /// from the beginning of the file to the start of the Postage Stamp Image. The Postage Stamp Image
        /// must be written after Field 25 (Scan Line Table) but before the start of the TGA File Footer.
        /// If no postage stamp is stored, set this field to the value zero (0).
        /// </summary>
        public uint PostageStampOffset { get; set; }

        /// <summary>
        /// Scan Line Offset - Field 23 (4 Bytes):
        /// Bytes 490-493 - This field is a 4-byte field containing a single offset value. This is an
        /// offset from the beginning of the file to the start of the Scan Line Table.
        /// </summary>
        public uint ScanLineOffset { get; set; }

        /// <summary>
        /// Attributes Type - Field 24 (1 Byte):
        /// Byte 494 - This single byte field contains a value which specifies the type of Alpha channel
        /// data contained in the file. Value Meaning:
        /// <para>0: no Alpha data included (bits 3-0 of field 5.6 should also be set to zero)</para>
        /// <para>1: undefined data in the Alpha field, can be ignored</para>
        /// <para>2: undefined data in the Alpha field, but should be retained</para>
        /// <para>3: useful Alpha channel data is present</para>
        /// <para>4: pre-multiplied Alpha(see description below)</para>
        /// <para>5 -127: RESERVED</para>
        /// <para>128-255: Un-assigned</para>
        /// <para>Pre-multiplied Alpha Example: Suppose the Alpha channel data is being used to specify the
        /// opacity of each pixel(for use when the image is overlayed on another image), where 0 indicates
        /// that the pixel is completely transparent and a value of 1 indicates that the pixel is
        /// completely opaque(assume all component values have been normalized).</para>
        /// <para>A quadruple(a, r, g, b) of( 0.5, 1, 0, 0) would indicate that the pixel is pure red with a
        /// transparency of one-half. For numerous reasons(including image compositing) is is better to
        /// pre-multiply the individual color components with the value in the Alpha channel.</para>
        /// A pre-multiplication of the above would produce a quadruple(0.5, 0.5, 0, 0).
        /// A value of 3 in the Attributes Type Field(field 23) would indicate that the color components
        /// of the pixel have already been scaled by the value in the Alpha channel.
        /// </summary>
        public TgaAttrType AttributesType { get; set; } = TgaAttrType.NoAlpha;

        /// <summary>
        /// Scan Line Table - Field 25 (Variable):
        /// This information is provided, at the developers’ request, for two purposes:
        /// <para>1) To make random access of compressed images easy.</para>
        /// <para>2) To allow "giant picture" access in smaller "chunks".</para>
        /// This table should contain a series of 4-byte offsets.Each offset you write should point to the
        /// start of the next scan line, in the order that the image was saved (i.e., top down or bottom up).
        /// The offset should be from the start of the file.Therefore, you will have a four byte value for
        /// each scan line in your image. This means that if your image is 768 pixels tall, you will have 768,
        /// 4-byte offset pointers (for a total of 3072 bytes). This size is not extreme, and thus this table
        /// can be built and maintained in memory, and then written out at the proper time.
        /// </summary>
        public uint[] ScanLineTable { get; set; }

        /// <summary>
        /// Postage Stamp Image - Field 26 (Variable):
        /// The Postage Stamp area is a smaller representation of the original image. This is useful for
        /// "browsing" a collection of image files. If your application can deal with a postage stamp image,
        /// it is recommended that you create one using sub-sampling techniques to create the best
        /// representation possible. The postage stamp image must be stored in the same format as the normal
        /// image specified in the file, but without any compression. The first byte of the postage stamp
        /// image specifies the X size of the stamp in pixels, the second byte of the stamp image specifies the
        /// Y size of the stamp in pixels. Truevision does not recommend stamps larger than 64x64 pixels, and
        /// suggests that any stamps stored larger be clipped. Obviously, the storage of the postage stamp
        /// relies heavily on the storage of the image. The two images are stored using the same format under
        /// the assumption that if you can read the image, then you can read the postage stamp. If the original
        /// image is color mapped, DO NOT average the postage stamp, as you will create new colors not in your map.
        /// </summary>
        public TgaPostageStampImage PostageStampImage { get; set; }

        /// <summary>
        /// Color Correction Table - Field 27 (2K Bytes):
        /// The Color Correction Table is a block of 256 x 4 SHORT values, where each set of
        /// four contiguous values are the desired A:R:G:B correction for that entry. This
        /// allows the user to store a correction table for image remapping or LUT driving.
        /// Since each color in the block is a SHORT, the maximum value for a color gun (i.e.,
        /// A, R, G, B) is 65535, and the minimum value is zero.Therefore, BLACK maps to
        /// 0, 0, 0, 0 and WHITE maps to 65535, 65535, 65535, 65535.
        /// </summary>
        public ushort[] ColorCorrectionTable { get; set; }

        /// <summary>
        /// Other Data In Extension Area (if <see cref="ExtensionSize"/> > 495).
        /// </summary>
        public byte[] OtherDataInExtensionArea { get; set; }
        #endregion

        /// <summary>
        /// Make full copy of <see cref="TgaExtArea"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaExtArea"/>.</returns>
        public TgaExtArea Clone()
        {
            var newExtArea = new TgaExtArea
            {
                ExtensionSize = ExtensionSize,
                AuthorName = AuthorName.Clone(),
                AuthorComments = AuthorComments.Clone(),
                DateTimeStamp = DateTimeStamp.Clone(),
                JobNameOrID = JobNameOrID.Clone(),
                JobTime = JobTime.Clone(),
                SoftwareID = SoftwareID.Clone(),
                SoftVersion = SoftVersion.Clone(),
                KeyColor = KeyColor.Clone(),
                PixelAspectRatio = PixelAspectRatio.Clone(),
                GammaValue = GammaValue.Clone(),
                ColorCorrectionTableOffset = ColorCorrectionTableOffset,
                PostageStampOffset = PostageStampOffset,
                ScanLineOffset = ScanLineOffset,
                AttributesType = AttributesType
            };

            if (ScanLineTable != null)
                newExtArea.ScanLineTable = (uint[])ScanLineTable.Clone();
            if (PostageStampImage != null)
                newExtArea.PostageStampImage = new TgaPostageStampImage(PostageStampImage.ToBytes());
            if (ColorCorrectionTable != null)
                newExtArea.ColorCorrectionTable = (ushort[])ColorCorrectionTable.Clone();

            if (OtherDataInExtensionArea != null)
                newExtArea.OtherDataInExtensionArea = (byte[])OtherDataInExtensionArea.Clone();

            return newExtArea;
        }

        /// <summary>
        /// Make full copy of <see cref="TgaExtArea"/>.
        /// </summary>
        /// <returns>Full independent copy of <see cref="TgaExtArea"/>.</returns>
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj) => obj is TgaExtArea value && TgaExtArea.Equals(this, value);

        public bool Equals(TgaExtArea item)
        {
            return ExtensionSize == item.ExtensionSize &&
                AuthorName == item.AuthorName &&
                AuthorComments == item.AuthorComments &&
                DateTimeStamp == item.DateTimeStamp &&
                JobNameOrID == item.JobNameOrID &&
                JobTime == item.JobTime &&
                SoftwareID == item.SoftwareID &&
                SoftVersion == item.SoftVersion &&
                KeyColor == item.KeyColor &&
                PixelAspectRatio == item.PixelAspectRatio &&
                GammaValue == item.GammaValue &&
                ColorCorrectionTableOffset == item.ColorCorrectionTableOffset &&
                PostageStampOffset == item.PostageStampOffset &&
                ScanLineOffset == item.ScanLineOffset &&
                AttributesType == item.AttributesType &&

                BitConverterExt.IsArraysEqual(ScanLineTable, item.ScanLineTable) &&
                PostageStampImage == item.PostageStampImage &&
                BitConverterExt.IsArraysEqual(ColorCorrectionTable, item.ColorCorrectionTable) &&

                BitConverterExt.IsArraysEqual(OtherDataInExtensionArea, item.OtherDataInExtensionArea);
        }

        public static bool operator ==(TgaExtArea item1, TgaExtArea item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaExtArea item1, TgaExtArea item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 27;
                hash = (13 * hash) + ExtensionSize.GetHashCode();
                hash = (13 * hash) + AuthorName.GetHashCode();
                hash = (13 * hash) + AuthorComments.GetHashCode();
                hash = (13 * hash) + DateTimeStamp.GetHashCode();
                hash = (13 * hash) + JobNameOrID.GetHashCode();
                hash = (13 * hash) + JobTime.GetHashCode();
                hash = (13 * hash) + SoftwareID.GetHashCode();
                hash = (13 * hash) + SoftVersion.GetHashCode();
                hash = (13 * hash) + KeyColor.GetHashCode();
                hash = (13 * hash) + PixelAspectRatio.GetHashCode();
                hash = (13 * hash) + GammaValue.GetHashCode();
                hash = (13 * hash) + ColorCorrectionTableOffset.GetHashCode();
                hash = (13 * hash) + PostageStampOffset.GetHashCode();
                hash = (13 * hash) + ScanLineOffset.GetHashCode();
                hash = (13 * hash) + AttributesType.GetHashCode();

                if (ScanLineTable != null)
                {
                    for (var i = 0; i < ScanLineTable.Length; i++)
                        hash = (13 * hash) + ScanLineTable[i].GetHashCode();
                }

                if (PostageStampImage != null)
                    hash = (13 * hash) + PostageStampImage.GetHashCode();

                if (ColorCorrectionTable != null)
                {
                    for (var i = 0; i < ColorCorrectionTable.Length; i++)
                        hash = (13 * hash) + ColorCorrectionTable[i].GetHashCode();
                }

                if (OtherDataInExtensionArea != null)
                {
                    for (var i = 0; i < OtherDataInExtensionArea.Length; i++)
                        hash = (13 * hash) + OtherDataInExtensionArea[i].GetHashCode();
                }

                return hash;
            }
        }

        /// <summary>
        /// Convert <see cref="TgaExtArea"/> to byte array. Warning: <see cref="ScanLineTable"/>,
        /// <see cref="PostageStampImage"/>, <see cref="ColorCorrectionTable"/> not included,
        /// because thea are can be not in the Extension Area of TGA file!
        /// </summary>
        /// <returns>Byte array.</returns>
        public byte[] ToBytes()
        {
            #region Exceptions check
            if (AuthorName == null)
                AuthorName = new TgaString(41, true);

            if (AuthorComments == null)
                AuthorComments = new TgaComment();

            if (DateTimeStamp == null)
                DateTimeStamp = new TgaDateTime(DateTime.UtcNow);

            if (JobNameOrID == null)
                JobNameOrID = new TgaString(41, true);

            if (JobTime == null)
                JobTime = new TgaTime();

            if (SoftwareID == null)
                SoftwareID = new TgaString(41, true);

            if (SoftVersion == null)
                SoftVersion = new TgaSoftVersion();

            if (KeyColor == null)
                KeyColor = new TgaColorKey();

            if (PixelAspectRatio == null)
                PixelAspectRatio = new TgaFraction();

            if (GammaValue == null)
                GammaValue = new TgaFraction();
            #endregion

            return BitConverterExt.ToBytes(
                ExtensionSize,
                AuthorName.ToBytes(),
                AuthorComments.ToBytes(),
                DateTimeStamp.ToBytes(),
                JobNameOrID.ToBytes(),
                JobTime.ToBytes(),
                SoftwareID.ToBytes(),
                SoftVersion.ToBytes(),
                KeyColor.ToBytes(),
                PixelAspectRatio.ToBytes(),
                GammaValue.ToBytes(),
                ColorCorrectionTableOffset,
                PostageStampOffset,
                ScanLineOffset,
                (byte)AttributesType,
                OtherDataInExtensionArea);
        }
    } //Not full ToBytes()

    /// <summary>
    /// File Footer Area
    /// </summary>
    public class TgaFooter : ICloneable
    {
        /// <summary>
        /// Make NewXFile format TGA Footer with <see cref="ExtensionAreaOffset"/> = 0 and
        /// <see cref="DeveloperDirectoryOffset"/> = 0.
        /// </summary>
        public TgaFooter()
        { }

        /// <summary>
        /// Make <see cref="TgaFooter"/> from values.
        /// </summary>
        /// <param name="extOff">Extension Area Offset, offset from the beginning of the file.</param>
        /// <param name="devDirOff">Developer Directory Offset, offset from the beginning of the file.</param>
        /// <param name="sign">New TGA format signature.</param>
        /// <param name="reservChr">Reserved Character - ASCII character "." (period).</param>
        /// <param name="termin">Binary Zero Terminator, a binary zero which acts as a final terminator.</param>
        public TgaFooter(uint extOff, uint devDirOff, TgaString sign, TgaString reservChr, TgaString termin)
        {
            ExtensionAreaOffset = extOff;
            DeveloperDirectoryOffset = devDirOff;
            Signature = sign;
            ReservedCharacter = reservChr;
            BinaryZeroStringTerminator = termin;
        }

        /// <summary>
        /// Make <see cref="TgaFooter"/> from bytes (if signature is right).
        /// </summary>
        /// <param name="bytes">Bytes array (byte[26]).</param>
        public TgaFooter(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes) + " = null!");
            if (bytes.Length != Size)
                throw new ArgumentOutOfRangeException(nameof(bytes.Length) + " must be equal " + Size + "!");

            ExtensionAreaOffset = BitConverter.ToUInt32(bytes, 0);
            DeveloperDirectoryOffset = BitConverter.ToUInt32(bytes, 4);
            Signature = new TgaString(BitConverterExt.GetElements(bytes, 8, TgaString.XFileSignatuteConst.Length));
            ReservedCharacter = new TgaString(new byte[] { bytes[24] });
            BinaryZeroStringTerminator = new TgaString(new byte[] { bytes[25] });
        }

        /// <summary>
        /// Byte 0-3 - Extension Area Offset - Field 28
        /// The first four bytes (bytes 0-3, the first LONG) of the TGA File Footer contain an
        /// offset from the beginning of the file to the start of the Extension Area. Simply
        /// SEEK to this location to position to the start of the Extension Area. If the
        /// Extension Area Offset is zero, no Extension Area exists in the file.
        /// </summary>
        public uint ExtensionAreaOffset { get; set; }

        /// <summary>
        /// Byte 4-7 - Developer Directory Offset - Field 29
        /// The next four bytes(bytes 4-7, the second LONG) contain an offset from the
        /// beginning of the file to the start of the Developer Directory. If the Developer
        /// Directory Offset is zero, then the Developer Area does not exist.
        /// </summary>
        public uint DeveloperDirectoryOffset { get; set; }

        /// <summary>
        /// Byte 8-23 - Signature - Field 30
        /// This string is exactly 16 bytes long and is formatted exactly as shown below
        /// capital letters), with a hyphen between "TRUEVISION" and "XFILE." If the
        /// signature is detected, the file is assumed to be of the New TGA format and MAY,
        /// therefore, contain the Developer Area and/or the Extension Area fields.If the
        /// signature is not found, then the file is assumed to be in the Original TGA format.
        /// </summary>
        public TgaString Signature { get; set; } = TgaString.XFileSignatute;

        /// <summary>
        /// Byte 24 - Reserved Character - Field 31
        /// Byte 24 is an ASCII character "." (period). This character MUST BE a period or
        /// the file is not considered a proper TGA file.
        /// </summary>
        public TgaString ReservedCharacter { get; set; } = TgaString.DotSymbol;

        /// <summary>
        /// Byte 25 - Binary Zero String Terminator - Field 32
        /// Byte 25 is a binary zero which acts as a final terminator and allows the entire TGA
        /// File Footer to be read and utilized as a "C" string.
        /// </summary>
        public TgaString BinaryZeroStringTerminator { get; set; } = TgaString.ZeroTerminator;

        /// <summary>
        /// Make full copy of <see cref="TgaFooter"/>.
        /// </summary>
        /// <returns></returns>
        public TgaFooter Clone()
        {
            return new TgaFooter(ExtensionAreaOffset, DeveloperDirectoryOffset, Signature.Clone(),
                ReservedCharacter.Clone(), BinaryZeroStringTerminator.Clone());
        }

        /// <summary>
        /// Make full copy of <see cref="TgaFooter"/>.
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Gets TGA Footer Section size in bytes.
        /// </summary>
        public const int Size = 26;

        public override bool Equals(object obj) => obj is TgaFooter value && TgaFooter.Equals(this, value);

        public bool Equals(TgaFooter item)
        {
            return ExtensionAreaOffset == item.ExtensionAreaOffset &&
                DeveloperDirectoryOffset == item.DeveloperDirectoryOffset &&
                Signature == item.Signature &&
                ReservedCharacter == item.ReservedCharacter &&
                BinaryZeroStringTerminator == item.BinaryZeroStringTerminator;
        }

        public static bool operator ==(TgaFooter item1, TgaFooter item2)
        {
            if (ReferenceEquals(item1, null))
                return ReferenceEquals(item2, null);

            if (ReferenceEquals(item2, null))
                return ReferenceEquals(item1, null);

            return item1.Equals(item2);
        }

        public static bool operator !=(TgaFooter item1, TgaFooter item2) => !(item1 == item2);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = hash * 23 + ExtensionAreaOffset.GetHashCode();
                hash = hash * 23 + DeveloperDirectoryOffset.GetHashCode();

                if (Signature != null)
                    hash = hash * 23 + Signature.GetHashCode();

                if (ReservedCharacter != null)
                    hash = hash * 23 + ReservedCharacter.GetHashCode();

                if (BinaryZeroStringTerminator != null)
                    hash = hash * 23 + BinaryZeroStringTerminator.GetHashCode();

                return hash;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}={1}, {2}={3}, FullSignature={4}",
                nameof(ExtensionAreaOffset), ExtensionAreaOffset, nameof(DeveloperDirectoryOffset), DeveloperDirectoryOffset,
                (Signature + ReservedCharacter + BinaryZeroStringTerminator).ToString());
        }

        /// <summary>
        /// Convert <see cref="TgaFooter"/> to byte array.
        /// </summary>
        /// <returns>Byte array with size equal <see cref="Size"/>.</returns>
        public byte[] ToBytes()
        {
            return BitConverterExt.ToBytes(ExtensionAreaOffset, DeveloperDirectoryOffset,
                Signature.ToBytes(), ReservedCharacter.ToBytes(), BinaryZeroStringTerminator.ToBytes());
        }

        /// <summary>
        /// Is footer is real footer of TGA File Format Version 2.0?
        /// Checking by <see cref="TgaString.XFileSignatute"/>.
        /// </summary>
        public bool IsFooterCorrect => Signature == TgaString.XFileSignatute;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Simplify ByteConversion operations, like concatination of byte arrays, comparing and other.
    /// </summary>
    public static class BitConverterExt
    {
        /// <summary>
        /// Combine byte, byte[], (u)short, (u)int, (u)long values to byte[] array.
        /// </summary>
        /// <param name="obj">Array of byte, byte[], (u)short, (u)int, (u)long values.</param>
        /// <returns>Array of bytes, null when some object is null.</returns>
        public static byte[] ToBytes(params object[] obj)
        {
            if (obj == null)
                return null;

            var bytesList = new List<byte>();

            for (var i = 0; i < obj.Length; i++)
            {
                if (obj[i] == null)
                    continue;
                else if (obj[i] is byte)
                    bytesList.Add((byte)obj[i]);
                else if (obj[i] is byte[])
                    bytesList.AddRange((byte[])obj[i]);
                else if (obj[i] is short)
                    bytesList.AddRange(BitConverter.GetBytes((short)obj[i]));
                else if (obj[i] is ushort)
                    bytesList.AddRange(BitConverter.GetBytes((ushort)obj[i]));
                else if (obj[i] is int)
                    bytesList.AddRange(BitConverter.GetBytes((int)obj[i]));
                else if (obj[i] is uint)
                    bytesList.AddRange(BitConverter.GetBytes((uint)obj[i]));
                else if (obj[i] is long)
                    bytesList.AddRange(BitConverter.GetBytes((long)obj[i]));
                else if (obj[i] is ulong)
                    bytesList.AddRange(BitConverter.GetBytes((ulong)obj[i]));
            }
            return bytesList.ToArray();
        }

        /// <summary>
        /// Copies a range of elements from an Array starting at the specified source index.
        /// The length and the index are specified as 32-bit integers.
        /// </summary>
        /// <param name="srcArray">The <see cref="Array"/> that contains the data to copy.</param>
        /// <param name="offset">A 32-bit integer that represents the index in the
        /// <see cref="SrcArray"/> at which copying begins.</param>
        /// <param name="count">A 32-bit integer that represents the number of elements to copy.</param>
        /// <returns></returns>
        public static T[] GetElements<T>(T[] srcArray, int offset, int count)
        {
            if (srcArray == null)
                throw new ArgumentNullException(nameof(srcArray) + " is null!");

            if (offset >= srcArray.Length || offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset) + " has wrong value!");

            if (count <= 0 || offset + count > srcArray.Length)
                throw new ArgumentOutOfRangeException(nameof(count) + " has wrong value!");

            var buff = new T[count];
            Array.Copy(srcArray, offset, buff, 0, buff.Length);
            return buff;
        }

        /// <summary>
        /// Compare N-dimensional Arrays.
        /// </summary>
        /// <typeparam name="T">Arrays Type.</typeparam>
        /// <param name="item1">First Array.</param>
        /// <param name="item2">Second Array.</param>
        /// <returns>True, if Arrays are equal.</returns>
        public static bool IsArraysEqual<T>(T[] item1, T[] item2)
        {
            if (ReferenceEquals(item1, item2))
                return true;

            if (item1 == null || item2 == null)
                return false;

            if (item1.Length != item2.Length)
                return false;

            var comparer = EqualityComparer<T>.Default;
            for (var i = 0; i < item1.Length; i++)
            {
                if (!comparer.Equals(item1[i], item2[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Compare Lists.
        /// </summary>
        /// <typeparam name="T">List Type.</typeparam>
        /// <param name="item1">First List.</param>
        /// <param name="item2">Second List.</param>
        /// <returns>True, if Lists are equal.</returns>
        public static bool IsListsEqual<T>(List<T> item1, List<T> item2)
        {
            if (ReferenceEquals(item1, item2))
                return true;

            if (item1 == null || item2 == null)
                return false;

            if (item1.Count != item2.Count)
                return false;

            for (var i = 0; i < item1.Count; i++)
            {
                if (!item1[i].Equals(item2[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Compare elements in one Array with different offsets.
        /// </summary>
        /// <typeparam name="T">Array type.</typeparam>
        /// <param name="arr">Some Array.</param>
        /// <param name="offset1">First offset.</param>
        /// <param name="offset2">Second offset.</param>
        /// <param name="count">Elements count which must be compared.</param>
        /// <returns></returns>
        public static bool IsElementsEqual<T>(T[] arr, int offset1, int offset2, int count)
        {
            if (arr == null)
                throw new ArgumentNullException(nameof(arr) + " is null!");

            if (offset1 >= arr.Length || offset1 < 0)
                throw new ArgumentOutOfRangeException(nameof(offset1) + " has wrong value!");

            if (offset2 >= arr.Length || offset2 < 0)
                throw new ArgumentOutOfRangeException(nameof(offset2) + " has wrong value!");

            if (count <= 0 || offset1 + count > arr.Length || offset2 + count > arr.Length)
                throw new ArgumentOutOfRangeException(nameof(count) + " has wrong value!");

            if (offset1 == offset2)
                return true;

            for (var i = 0; i < count; i++)
            {
                if (!arr[offset1 + i].Equals(arr[offset2 + i]))
                    return false;
            }

            return true;
        }
    }
    #endregion

    public class TGA : ICloneable
    {
        public TgaHeader Header = new TgaHeader();
        public TgaImgOrColMap ImageOrColorMapArea = new TgaImgOrColMap();
        public TgaDevArea DevArea = null;
        public TgaExtArea ExtArea = null;
        public TgaFooter Footer = null;

        #region TGA Creation, Loading, Saving (all are public, have reference to private metods).
        /// <summary>
        /// Create new empty <see cref="TGA"/> istance.
        /// </summary>
        public TGA()
        { }

        /// <summary>
        /// Create <see cref="TGA"/> instance with some params. If it must have ColorMap,
        /// check all ColorMap fields and settings after.
        /// </summary>
        /// <param name="width">Image Width.</param>
        /// <param name="height">Image Height.</param>
        /// <param name="pixDepth">Image Pixel Depth (bits / pixel), set ColorMap bpp after, if needed!</param>
        /// <param name="imgType">Image Type (is RLE compressed, ColorMapped or GrayScaled).</param>
        /// <param name="attrBits">Set numder of Attrbute bits (Alpha channel bits), default: 0, 1, 8.</param>
        /// <param name="newFormat">Use new 2.0 TGA XFile format?</param>
        public TGA(ushort width, ushort height, TgaPixelDepth pixDepth = TgaPixelDepth.Bpp24,
            TgaImageType imgType = TgaImageType.Uncompressed_TrueColor, byte attrBits = 0, bool newFormat = true)
        {
            if (width <= 0 || height <= 0 || pixDepth == TgaPixelDepth.Other)
            {
                width = height = 0;
                pixDepth = TgaPixelDepth.Other;
                imgType = TgaImageType.NoImageData;
                attrBits = 0;
            }
            else
            {
                var bytesPerPixel = (int)Math.Ceiling((double)pixDepth / 8.0);
                ImageOrColorMapArea.ImageData = new byte[width * height * bytesPerPixel];

                if (imgType == TgaImageType.Uncompressed_ColorMapped || imgType == TgaImageType.RLE_ColorMapped)
                {
                    Header.ColorMapType = TgaColorMapType.ColorMap;
                    Header.ColorMapSpec.FirstEntryIndex = 0;
                    Header.ColorMapSpec.ColorMapEntrySize = (TgaColorMapEntrySize)Math.Ceiling((double)pixDepth / 8);
                }
            }

            Header.ImageType = imgType;
            Header.ImageSpec.ImageWidth = width;
            Header.ImageSpec.ImageHeight = height;
            Header.ImageSpec.PixelDepth = pixDepth;
            Header.ImageSpec.ImageDescriptor.AlphaChannelBits = attrBits;

            if (newFormat)
            {
                Footer = new TgaFooter();
                ExtArea = new TgaExtArea
                {
                    DateTimeStamp = new TgaDateTime(DateTime.UtcNow),
                    AttributesType = attrBits > 0 ? TgaAttrType.UsefulAlpha : TgaAttrType.NoAlpha
                };
            }
        }

        /// <summary>
        /// Make <see cref="TGA"/> from some <see cref="TGA"/> instance.
        /// Equal to <see cref="TGA.Clone()"/> function.
        /// </summary>
        /// <param name="tga">Original <see cref="TGA"/> instance.</param>
        public TGA(TGA tga)
        {
            Header = tga.Header.Clone();
            ImageOrColorMapArea = tga.ImageOrColorMapArea.Clone();
            DevArea = tga.DevArea.Clone();
            ExtArea = tga.ExtArea.Clone();
            Footer = tga.Footer.Clone();
        }

        /// <summary>
        /// Load <see cref="TGA"/> from file.
        /// </summary>
        /// <param name="filename">Full path to TGA file.</param>
        /// <returns>Loaded <see cref="TGA"/> file.</returns>
        public TGA(string filename)
        {
            LoadFunc(filename);
        }

        /// <summary>
        /// Make <see cref="TGA"/> from bytes array.
        /// </summary>
        /// <param name="bytes">Bytes array (same like TGA File).</param>
        public TGA(byte[] bytes)
        {
            LoadFunc(bytes);
        }

        /// <summary>
        /// Make <see cref="TGA"/> from <see cref="Stream"/>.
        /// For file opening better use <see cref="FromFile(string)"/>.
        /// </summary>
        /// <param name="stream">Some stream. You can use a lot of Stream types, but Stream must support:
        /// <see cref="Stream.CanSeek"/> and <see cref="Stream.CanRead"/>.</param>
        public TGA(Stream stream)
        {
            LoadFunc(stream);
        }

        /// <summary>
        /// Make <see cref="TGA"/> from <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="bmp">Input Bitmap, supported a lot of bitmaps types: 8/15/16/24/32 Bpp's.</param>
        /// <param name="useRLE">Use RLE Compression?</param>
        /// <param name="newFormat">Use new 2.0 TGA XFile format?</param>
        /// <param name="colorMap2BytesEntry">Is Color Map Entry size equal 15 or 16 Bpp, else - 24 or 32.</param>
        public TGA(Bitmap bmp, bool useRLE = false, bool newFormat = true, bool colorMap2BytesEntry = false)
        {
            LoadFunc(bmp, useRLE, newFormat, colorMap2BytesEntry);
        }

        /// <summary>
        /// Load <see cref="TGA"/> from file.
        /// </summary>
        /// <param name="filename">Full path to TGA file.</param>
        /// <returns>Loaded <see cref="TGA"/> file.</returns>
        public static TGA FromFile(string filename) => new TGA(filename);

        /// <summary>
        /// Make <see cref="TGA"/> from bytes array.
        /// </summary>
        /// <param name="bytes">Bytes array (same like TGA File).</param>
        public static TGA FromBytes(byte[] bytes) => new TGA(bytes);

        /// <summary>
        /// Make <see cref="TGA"/> from <see cref="Stream"/>.
        /// For file opening better use <see cref="FromFile(string)"/>.
        /// </summary>
        /// <param name="stream">Some stream. You can use a lot of Stream types, but Stream must support:
        /// <see cref="Stream.CanSeek"/> and <see cref="Stream.CanRead"/>.</param>
        public static TGA FromStream(Stream stream) => new TGA(stream);

        /// <summary>
        /// Make <see cref="TGA"/> from <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="bmp">Input Bitmap, supported a lot of bitmaps types: 8/15/16/24/32 Bpp's.</param>
        /// <param name="useRLE">Use RLE Compression?</param>
        /// <param name="newFormat">Use new 2.0 TGA XFile format?</param>
        /// <param name="colorMap2BytesEntry">Is Color Map Entry size equal 15 or 16 Bpp, else - 24 or 32.</param>
        public static TGA FromBitmap(Bitmap bmp, bool useRLE = false,
            bool newFormat = true, bool colorMap2BytesEntry = false) => new TGA(bmp, useRLE, newFormat, colorMap2BytesEntry);

        /// <summary>
        /// Save <see cref="TGA"/> to file.
        /// </summary>
        /// <param name="filename">Full path to file.</param>
        /// <returns>Return "true", if all done or "false", if failed.</returns>
        public bool Save(string filename)
        {
            try
            {
                var result = false;
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var ms = new MemoryStream())
                    {
                        result = SaveFunc(ms);
                        ms.WriteTo(fs);
                        fs.Flush();
                    }
                }
                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Save <see cref="TGA"/> to <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">Some stream, it must support: <see cref="Stream.CanWrite"/>.</param>
        /// <returns>Return "true", if all done or "false", if failed.</returns>
        public bool Save(Stream stream) => SaveFunc(stream);
        #endregion

        /// <summary>
        /// Gets or Sets Image Width (see <see cref="Header.ImageSpec.ImageWidth"/>).
        /// </summary>
        public ushort Width
        {
            get => Header.ImageSpec.ImageWidth;
            set => Header.ImageSpec.ImageWidth = value;
        }

        /// <summary>
        /// Gets or Sets Image Height (see <see cref="Header.ImageSpec.ImageHeight"/>).
        /// </summary>
        public ushort Height
        {
            get => Header.ImageSpec.ImageHeight;
            set => Header.ImageSpec.ImageHeight = value;
        }

        /// <summary>
        /// Gets or Sets <see cref="TGA"/> image Size.
        /// </summary>
        public Size Size
        {
            get => new Size(Header.ImageSpec.ImageWidth, Header.ImageSpec.ImageHeight);
            set
            {
                Header.ImageSpec.ImageWidth = (ushort)value.Width;
                Header.ImageSpec.ImageHeight = (ushort)value.Height;
            }
        }

        /// <summary>
        /// Make full independed copy of <see cref="TGA"/>.
        /// </summary>
        /// <returns>Full independed copy of <see cref="TGA"/>.</returns>
        public TGA Clone() => new TGA(this);

        object ICloneable.Clone() => Clone();

        /// <summary>
        /// Flip <see cref="TGA"/> directions, for more info see <see cref="TgaImgOrigin"/>.
        /// </summary>
        /// <param name="horizontal">Flip horizontal.</param>
        /// <param name="vertical">Flip vertical.</param>
        public void Flip(bool horizontal = false, bool vertical = false)
        {
            var newOrigin = (int)Header.ImageSpec.ImageDescriptor.ImageOrigin;
            newOrigin ^= (vertical ? 0x20 : 0) | (horizontal ? 0x10 : 0);
            Header.ImageSpec.ImageDescriptor.ImageOrigin = (TgaImgOrigin)newOrigin;
        }

        /// <summary>
        /// Get information from TGA image.
        /// </summary>
        /// <returns>MultiLine string with info fields (one per line).</returns>
        public string GetInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Header:");
            sb.AppendLine("\tID Length = " + Header.IDLength);
            sb.AppendLine("\tImage Type = " + Header.ImageType);
            sb.AppendLine("\tHeader -> ImageSpec:");
            sb.AppendLine("\t\tImage Width = " + Header.ImageSpec.ImageWidth);
            sb.AppendLine("\t\tImage Height = " + Header.ImageSpec.ImageHeight);
            sb.AppendLine("\t\tPixel Depth = " + Header.ImageSpec.PixelDepth);
            sb.AppendLine("\t\tImage Descriptor (AsByte) = " + Header.ImageSpec.ImageDescriptor.ToByte());
            sb.AppendLine("\t\tImage Descriptor -> AttributeBits = " + Header.ImageSpec.ImageDescriptor.AlphaChannelBits);
            sb.AppendLine("\t\tImage Descriptor -> ImageOrigin = " + Header.ImageSpec.ImageDescriptor.ImageOrigin);
            sb.AppendLine("\t\tX_Origin = " + Header.ImageSpec.X_Origin);
            sb.AppendLine("\t\tY_Origin = " + Header.ImageSpec.Y_Origin);
            sb.AppendLine("\tColorMap Type = " + Header.ColorMapType);
            sb.AppendLine("\tHeader -> ColorMapSpec:");
            sb.AppendLine("\t\tColorMap Entry Size = " + Header.ColorMapSpec.ColorMapEntrySize);
            sb.AppendLine("\t\tColorMap Length = " + Header.ColorMapSpec.ColorMapLength);
            sb.AppendLine("\t\tFirstEntry Index = " + Header.ColorMapSpec.FirstEntryIndex);

            sb.AppendLine("\nImage / Color Map Area:");
            if (Header.IDLength > 0 && ImageOrColorMapArea.ImageID != null)
                sb.AppendLine("\tImage ID = \"" + ImageOrColorMapArea.ImageID.GetString() + "\"");
            else
                sb.AppendLine("\tImage ID = null");

            if (ImageOrColorMapArea.ImageData != null)
                sb.AppendLine("\tImage Data Length = " + ImageOrColorMapArea.ImageData.Length);
            else
                sb.AppendLine("\tImage Data = null");

            if (ImageOrColorMapArea.ColorMapData != null)
                sb.AppendLine("\tColorMap Data Length = " + ImageOrColorMapArea.ColorMapData.Length);
            else
                sb.AppendLine("\tColorMap Data = null");

            sb.AppendLine("\nDevelopers Area:");
            if (DevArea != null)
                sb.AppendLine("\tCount = " + DevArea.Count);
            else
                sb.AppendLine("\tDevArea = null");

            sb.AppendLine("\nExtension Area:");
            if (ExtArea != null)
            {
                sb.AppendLine("\tExtension Size = " + ExtArea.ExtensionSize);
                sb.AppendLine("\tAuthor Name = \"" + ExtArea.AuthorName.GetString() + "\"");
                sb.AppendLine("\tAuthor Comments = \"" + ExtArea.AuthorComments.GetString() + "\"");
                sb.AppendLine("\tDate / Time Stamp = " + ExtArea.DateTimeStamp);
                sb.AppendLine("\tJob Name / ID = \"" + ExtArea.JobNameOrID.GetString() + "\"");
                sb.AppendLine("\tJob Time = " + ExtArea.JobTime);
                sb.AppendLine("\tSoftware ID = \"" + ExtArea.SoftwareID.GetString() + "\"");
                sb.AppendLine("\tSoftware Version = \"" + ExtArea.SoftVersion + "\"");
                sb.AppendLine("\tKey Color = " + ExtArea.KeyColor);
                sb.AppendLine("\tPixel Aspect Ratio = " + ExtArea.PixelAspectRatio);
                sb.AppendLine("\tGamma Value = " + ExtArea.GammaValue);
                sb.AppendLine("\tColor Correction Table Offset = " + ExtArea.ColorCorrectionTableOffset);
                sb.AppendLine("\tPostage Stamp Offset = " + ExtArea.PostageStampOffset);
                sb.AppendLine("\tScan Line Offset = " + ExtArea.ScanLineOffset);
                sb.AppendLine("\tAttributes Type = " + ExtArea.AttributesType);

                if (ExtArea.ScanLineTable != null)
                    sb.AppendLine("\tScan Line Table = " + ExtArea.ScanLineTable.Length);
                else
                    sb.AppendLine("\tScan Line Table = null");

                if (ExtArea.PostageStampImage != null)
                    sb.AppendLine("\tPostage Stamp Image: " + ExtArea.PostageStampImage.ToString());
                else
                    sb.AppendLine("\tPostage Stamp Image = null");

                sb.AppendLine("\tColor Correction Table = " + (ExtArea.ColorCorrectionTable != null));
            }
            else
            {
                sb.AppendLine("\tExtArea = null");
            }

            sb.AppendLine("\nFooter:");
            if (Footer != null)
            {
                sb.AppendLine("\tExtension Area Offset = " + Footer.ExtensionAreaOffset);
                sb.AppendLine("\tDeveloper Directory Offset = " + Footer.DeveloperDirectoryOffset);
                sb.AppendLine("\tSignature (Full) = \"" + Footer.Signature.ToString() +
                    Footer.ReservedCharacter.ToString() + Footer.BinaryZeroStringTerminator.ToString() + "\"");
            }
            else
            {
                sb.AppendLine("\tFooter = null");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Check and update all fields with data length and offsets.
        /// </summary>
        /// <returns>Return "true", if all OK or "false", if checking failed.</returns>
        public bool CheckAndUpdateOffsets(out string errorStr)
        {
            errorStr = string.Empty;

            if (Header == null)
            {
                errorStr = "Header = null";
                return false;
            }

            if (ImageOrColorMapArea == null)
            {
                errorStr = "ImageOrColorMapArea = null";
                return false;
            }

            uint Offset = TgaHeader.Size; // Virtual Offset

            #region Header
            if (ImageOrColorMapArea.ImageID != null)
            {
                var strMaxLen = 255;
                if (ImageOrColorMapArea.ImageID.UseEndingChar)
                    strMaxLen--;

                Header.IDLength = (byte)Math.Min(ImageOrColorMapArea.ImageID.OriginalString.Length, strMaxLen);
                ImageOrColorMapArea.ImageID.Length = Header.IDLength;
                Offset += Header.IDLength;
            }
            else
            {
                Header.IDLength = 0;
            }
            #endregion

            #region ColorMap
            if (Header.ColorMapType != TgaColorMapType.NoColorMap)
            {
                if (Header.ColorMapSpec == null)
                {
                    errorStr = "Header.ColorMapSpec = null";
                    return false;
                }

                if (Header.ColorMapSpec.ColorMapLength == 0)
                {
                    errorStr = "Header.ColorMapSpec.ColorMapLength = 0";
                    return false;
                }

                if (ImageOrColorMapArea.ColorMapData == null)
                {
                    errorStr = "ImageOrColorMapArea.ColorMapData = null";
                    return false;
                }

                var cmBytesPerPixel = (int)Math.Ceiling((double)Header.ColorMapSpec.ColorMapEntrySize / 8.0);
                var lenBytes = Header.ColorMapSpec.ColorMapLength * cmBytesPerPixel;

                if (lenBytes != ImageOrColorMapArea.ColorMapData.Length)
                {
                    errorStr = "ImageOrColorMapArea.ColorMapData.Length has wrong size!";
                    return false;
                }

                Offset += (uint)ImageOrColorMapArea.ColorMapData.Length;
            }
            #endregion

            #region Image Data
            var bytesPerPixel = 0;
            if (Header.ImageType != TgaImageType.NoImageData)
            {
                if (Header.ImageSpec == null)
                {
                    errorStr = "Header.ImageSpec = null";
                    return false;
                }

                if (Header.ImageSpec.ImageWidth == 0 || Header.ImageSpec.ImageHeight == 0)
                {
                    errorStr = "Header.ImageSpec.ImageWidth = 0 or Header.ImageSpec.ImageHeight = 0";
                    return false;
                }

                if (ImageOrColorMapArea.ImageData == null)
                {
                    errorStr = "ImageOrColorMapArea.ImageData = null";
                    return false;
                }

                bytesPerPixel = (int)Math.Ceiling((double)Header.ImageSpec.PixelDepth / 8.0);
                if (Width * Height * bytesPerPixel != ImageOrColorMapArea.ImageData.Length)
                {
                    errorStr = "ImageOrColorMapArea.ImageData.Length has wrong size!";
                    return false;
                }

                if (Header.ImageType >= TgaImageType.RLE_ColorMapped &&
                    Header.ImageType <= TgaImageType.RLE_BlackWhite)
                {
                    var rle = RLE_Encode(ImageOrColorMapArea.ImageData, Width, Height);
                    if (rle == null)
                    {
                        errorStr = "RLE Compressing error! Check Image Data size.";
                        return false;
                    }

                    Offset += (uint)rle.Length;
                    rle = null;
                }
                else
                {
                    Offset += (uint)ImageOrColorMapArea.ImageData.Length;
                }
            }
            #endregion

            #region Footer, DevArea, ExtArea
            if (Footer != null)
            {
                #region DevArea
                if (DevArea != null)
                {
                    var devAreaCount = DevArea.Count;
                    for (var i = 0; i < devAreaCount; i++)
                    {
                        if (DevArea[i] == null || DevArea[i].FieldSize <= 0) //Del Empty Entries
                        {
                            DevArea.Entries.RemoveAt(i);
                            devAreaCount--;
                            i--;
                        }
                    }

                    if (DevArea.Count <= 0)
                        Footer.DeveloperDirectoryOffset = 0;

                    if (DevArea.Count > 2)
                    {
                        DevArea.Entries.Sort((a, b) => { return a.Tag.CompareTo(b.Tag); });
                        for (var i = 0; i < DevArea.Count - 1; i++)
                        {
                            if (DevArea[i].Tag == DevArea[i + 1].Tag)
                            {
                                errorStr = "DevArea Enties has same Tags!";
                                return false;
                            }
                        }
                    }

                    for (var i = 0; i < DevArea.Count; i++)
                    {
                        DevArea[i].Offset = Offset;
                        Offset += (uint)DevArea[i].FieldSize;
                    }

                    Footer.DeveloperDirectoryOffset = Offset;
                    Offset += (uint)(DevArea.Count * 10 + 2);
                }
                else
                {
                    Footer.DeveloperDirectoryOffset = 0;
                }
                #endregion

                #region ExtArea
                if (ExtArea != null)
                {
                    ExtArea.ExtensionSize = TgaExtArea.MinSize;
                    if (ExtArea.OtherDataInExtensionArea != null)
                        ExtArea.ExtensionSize += (ushort)ExtArea.OtherDataInExtensionArea.Length;

                    ExtArea.DateTimeStamp = new TgaDateTime(DateTime.UtcNow);

                    Footer.ExtensionAreaOffset = Offset;
                    Offset += ExtArea.ExtensionSize;

                    #region ScanLineTable
                    if (ExtArea.ScanLineTable == null)
                    {
                        ExtArea.ScanLineOffset = 0;
                    }
                    else
                    {
                        if (ExtArea.ScanLineTable.Length != Height)
                        {
                            errorStr = "ExtArea.ScanLineTable.Length != Height";
                            return false;
                        }

                        ExtArea.ScanLineOffset = Offset;
                        Offset += (uint)(ExtArea.ScanLineTable.Length * 4);
                    }
                    #endregion

                    #region PostageStampImage
                    if (ExtArea.PostageStampImage == null)
                    {
                        ExtArea.PostageStampOffset = 0;
                    }
                    else
                    {
                        if (ExtArea.PostageStampImage.Width == 0 || ExtArea.PostageStampImage.Height == 0)
                        {
                            errorStr = "ExtArea.PostageStampImage Width or Height is equal 0!";
                            return false;
                        }

                        if (ExtArea.PostageStampImage.Data == null)
                        {
                            errorStr = "ExtArea.PostageStampImage.Data == null";
                            return false;
                        }

                        var pImgSB = ExtArea.PostageStampImage.Width * ExtArea.PostageStampImage.Height * bytesPerPixel;
                        if (Header.ImageType != TgaImageType.NoImageData &&
                            ExtArea.PostageStampImage.Data.Length != pImgSB)
                        {
                            errorStr = "ExtArea.PostageStampImage.Data.Length is wrong!";
                            return false;
                        }

                        ExtArea.PostageStampOffset = Offset;
                        Offset += (uint)ExtArea.PostageStampImage.Data.Length;
                    }
                    #endregion

                    #region ColorCorrectionTable
                    if (ExtArea.ColorCorrectionTable == null)
                        ExtArea.ColorCorrectionTableOffset = 0;
                    else
                    {
                        if (ExtArea.ColorCorrectionTable.Length != 1024)
                        {
                            errorStr = "ExtArea.ColorCorrectionTable.Length != 256 * 4";
                            return false;
                        }

                        ExtArea.ColorCorrectionTableOffset = Offset;
                        Offset += (uint)(ExtArea.ColorCorrectionTable.Length * 2);
                    }
                    #endregion
                }
                else
                    Footer.ExtensionAreaOffset = 0;
                #endregion

                #region Footer
                if (Footer.ToBytes().Length != TgaFooter.Size)
                {
                    errorStr = "Footer.Length is wrong!";
                    return false;
                }

                Offset += TgaFooter.Size;
                #endregion
            }
            #endregion

            return true;
        }

        #region Convert
        /// <summary>
        /// Convert <see cref="TGA"/> to <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="forceUseAlpha">Force use alpha channel.</param>
        /// <returns>Bitmap or null, on error.</returns>
        public Bitmap ToBitmap(bool forceUseAlpha = false) => ToBitmapFunc(forceUseAlpha, false);

        /// <summary>
        /// Convert <see cref="TGA"/> to bytes array.
        /// </summary>
        /// <returns>Bytes array, (equal to saved file, but in memory) or null (on error).</returns>
        public byte[] ToBytes()
        {
            try
            {
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    Save(ms);
                    bytes = ms.ToArray();
                    ms.Flush();
                }
                return bytes;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert TGA Image to new XFile format (v2.0).
        /// </summary>
        public void ToNewFormat()
        {
            if (Footer == null)
                Footer = new TgaFooter();

            if (ExtArea == null)
            {
                ExtArea = new TgaExtArea
                {
                    DateTimeStamp = new TgaDateTime(DateTime.UtcNow)
                };

                if (Header.ImageSpec.ImageDescriptor.AlphaChannelBits > 0)
                    ExtArea.AttributesType = TgaAttrType.UsefulAlpha;
                else
                    ExtArea.AttributesType = TgaAttrType.NoAlpha;
            }
        }
        #endregion

        #region Private functions
        private bool LoadFunc(string filename)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("File: \"" + filename + "\" not found!");

            try
            {
                using (var fs = new FileStream(filename, FileMode.Open))
                    return LoadFunc(fs);
            }
            catch
            {
                return false;
            }
        }

        private bool LoadFunc(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException();

            try
            {
                using (var fs = new MemoryStream(bytes, false))
                    return LoadFunc(fs);
            }
            catch
            {
                return false;
            }
        }

        private bool LoadFunc(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException();
            if (!(stream.CanRead && stream.CanSeek))
                throw new FileLoadException("Stream reading or seeking is not avaiable!");

            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var br = new BinaryReader(stream);

                Header = new TgaHeader(br.ReadBytes(TgaHeader.Size));

                if (Header.IDLength > 0)
                    ImageOrColorMapArea.ImageID = new TgaString(br.ReadBytes(Header.IDLength));

                if (Header.ColorMapSpec.ColorMapLength > 0)
                {
                    var cmBytesPerPixel = (int)Math.Ceiling((double)Header.ColorMapSpec.ColorMapEntrySize / 8.0);
                    var lenBytes = Header.ColorMapSpec.ColorMapLength * cmBytesPerPixel;
                    ImageOrColorMapArea.ColorMapData = br.ReadBytes(lenBytes);
                }

                #region Read Image Data
                var bytesPerPixel = (int)Math.Ceiling((double)Header.ImageSpec.PixelDepth / 8.0);
                if (Header.ImageType != TgaImageType.NoImageData)
                {
                    var imageDataSize = Width * Height * bytesPerPixel;
                    switch (Header.ImageType)
                    {
                        case TgaImageType.RLE_ColorMapped:
                        case TgaImageType.RLE_TrueColor:
                        case TgaImageType.RLE_BlackWhite:

                            var dataOffset = 0;
                            byte packetInfo;
                            int packetCount;
                            byte[] rleBytes, rlePart;
                            ImageOrColorMapArea.ImageData = new byte[imageDataSize];

                            do
                            {
                                packetInfo = br.ReadByte(); //1 type bit and 7 count bits. Len = Count + 1.
                                packetCount = (packetInfo & 127) + 1;

                                if (packetInfo >= 128) // bit7 = 1, RLE
                                {
                                    rleBytes = new byte[packetCount * bytesPerPixel];
                                    rlePart = br.ReadBytes(bytesPerPixel);
                                    for (var i = 0; i < rleBytes.Length; i++)
                                        rleBytes[i] = rlePart[i % bytesPerPixel];
                                }
                                else // RAW format
                                    rleBytes = br.ReadBytes(packetCount * bytesPerPixel);

                                Buffer.BlockCopy(rleBytes, 0, ImageOrColorMapArea.ImageData, dataOffset, rleBytes.Length);
                                dataOffset += rleBytes.Length;
                            }

                            while (dataOffset < imageDataSize);
                            rleBytes = null;
                            break;

                        case TgaImageType.Uncompressed_ColorMapped:
                        case TgaImageType.Uncompressed_TrueColor:
                        case TgaImageType.Uncompressed_BlackWhite:
                            ImageOrColorMapArea.ImageData = br.ReadBytes(imageDataSize);
                            break;
                    }
                }
                #endregion

                #region Try parse Footer
                stream.Seek(-TgaFooter.Size, SeekOrigin.End);
                var footerOffset = (uint)stream.Position;
                var mbFooter = new TgaFooter(br.ReadBytes(TgaFooter.Size));
                if (mbFooter.IsFooterCorrect)
                {
                    Footer = mbFooter;
                    var devDirOffset = Footer.DeveloperDirectoryOffset;
                    var extAreaOffset = Footer.ExtensionAreaOffset;

                    #region If Dev Area exist, read it.
                    if (devDirOffset != 0)
                    {
                        stream.Seek(devDirOffset, SeekOrigin.Begin);
                        DevArea = new TgaDevArea();
                        uint numberOfTags = br.ReadUInt16();

                        var tags = new ushort[numberOfTags];
                        var tagOffsets = new uint[numberOfTags];
                        var tagSizes = new uint[numberOfTags];

                        for (var i = 0; i < numberOfTags; i++)
                        {
                            tags[i] = br.ReadUInt16();
                            tagOffsets[i] = br.ReadUInt32();
                            tagSizes[i] = br.ReadUInt32();
                        }

                        for (var i = 0; i < numberOfTags; i++)
                        {
                            stream.Seek(tagOffsets[i], SeekOrigin.Begin);
                            var ent = new TgaDevEntry(tags[i], tagOffsets[i], br.ReadBytes((int)tagSizes[i]));
                            DevArea.Entries.Add(ent);
                        }

                        tags = null;
                        tagOffsets = null;
                        tagSizes = null;
                    }
                    #endregion

                    #region If Ext Area exist, read it.
                    if (extAreaOffset != 0)
                    {
                        stream.Seek(extAreaOffset, SeekOrigin.Begin);
                        var extAreaSize = Math.Max((ushort)TgaExtArea.MinSize, br.ReadUInt16());
                        stream.Seek(extAreaOffset, SeekOrigin.Begin);
                        ExtArea = new TgaExtArea(br.ReadBytes(extAreaSize));

                        if (ExtArea.ScanLineOffset > 0)
                        {
                            stream.Seek(ExtArea.ScanLineOffset, SeekOrigin.Begin);
                            ExtArea.ScanLineTable = new uint[Height];
                            for (var i = 0; i < ExtArea.ScanLineTable.Length; i++)
                                ExtArea.ScanLineTable[i] = br.ReadUInt32();
                        }

                        if (ExtArea.PostageStampOffset > 0)
                        {
                            stream.Seek(ExtArea.PostageStampOffset, SeekOrigin.Begin);
                            var w = br.ReadByte();
                            var h = br.ReadByte();
                            var imgDataSize = w * h * bytesPerPixel;
                            if (imgDataSize > 0)
                                ExtArea.PostageStampImage = new TgaPostageStampImage(w, h, br.ReadBytes(imgDataSize));
                        }

                        if (ExtArea.ColorCorrectionTableOffset > 0)
                        {
                            stream.Seek(ExtArea.ColorCorrectionTableOffset, SeekOrigin.Begin);
                            ExtArea.ColorCorrectionTable = new ushort[256 * 4];
                            for (var i = 0; i < ExtArea.ColorCorrectionTable.Length; i++)
                                ExtArea.ColorCorrectionTable[i] = br.ReadUInt16();
                        }
                    }
                    #endregion
                }
                #endregion

                br.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool LoadFunc(Bitmap bmp, bool UseRLE = false, bool NewFormat = true, bool ColorMap2BytesEntry = false)
        {
            if (bmp == null)
                throw new ArgumentNullException();

            try
            {
                Header.ImageSpec.ImageWidth = (ushort)bmp.Width;
                Header.ImageSpec.ImageHeight = (ushort)bmp.Height;
                Header.ImageSpec.ImageDescriptor.ImageOrigin = TgaImgOrigin.TopLeft;

                switch (bmp.PixelFormat)
                {
                    case PixelFormat.Indexed:
                    case PixelFormat.Gdi:
                    case PixelFormat.Alpha:
                    case PixelFormat.Undefined:
                    case PixelFormat.PAlpha:
                    case PixelFormat.Extended:
                    case PixelFormat.Max:
                    case PixelFormat.Canonical:
                    case PixelFormat.Format16bppRgb565:
                    default:
                        throw new FormatException(nameof(PixelFormat) + " is not supported!");

                    case PixelFormat.Format1bppIndexed:
                    case PixelFormat.Format4bppIndexed:
                    case PixelFormat.Format8bppIndexed:
                    case PixelFormat.Format16bppGrayScale:
                    case PixelFormat.Format16bppRgb555:
                    case PixelFormat.Format16bppArgb1555:
                    case PixelFormat.Format24bppRgb:
                    case PixelFormat.Format32bppRgb:
                    case PixelFormat.Format32bppArgb:
                    case PixelFormat.Format32bppPArgb:
                    case PixelFormat.Format48bppRgb:
                    case PixelFormat.Format64bppArgb:
                    case PixelFormat.Format64bppPArgb:

                        var bpp = Math.Max(8, Image.GetPixelFormatSize(bmp.PixelFormat));
                        var bytesPP = bpp / 8;

                        if (bmp.PixelFormat == PixelFormat.Format16bppRgb555)
                            bpp = 15;

                        var isAlpha = Image.IsAlphaPixelFormat(bmp.PixelFormat);
                        var isPreAlpha = isAlpha && bmp.PixelFormat.ToString().EndsWith("PArgb");
                        var isColorMapped = bmp.PixelFormat.ToString().EndsWith("Indexed");

                        Header.ImageSpec.PixelDepth = (TgaPixelDepth)(bytesPP * 8);

                        if (isAlpha)
                        {
                            Header.ImageSpec.ImageDescriptor.AlphaChannelBits = (byte)(bytesPP * 2);

                            if (bmp.PixelFormat == PixelFormat.Format16bppArgb1555)
                                Header.ImageSpec.ImageDescriptor.AlphaChannelBits = 1;
                        }

                        #region ColorMap
                        var isGrayImage = bmp.PixelFormat == PixelFormat.Format16bppGrayScale | isColorMapped;

                        if (isColorMapped && bmp.Palette != null)
                        {
                            var colors = bmp.Palette.Entries;

                            #region Analyze ColorMapType
                            var alphaSum = 0;
                            var colorMapUseAlpha = false;

                            for (var i = 0; i < colors.Length; i++)
                            {
                                isGrayImage &= colors[i].R == colors[i].G && colors[i].G == colors[i].B;
                                colorMapUseAlpha |= colors[i].A < 248;
                                alphaSum |= colors[i].A;
                            }
                            colorMapUseAlpha &= alphaSum > 0;

                            var cMapBpp = (ColorMap2BytesEntry ? 15 : 24) + (colorMapUseAlpha ? (ColorMap2BytesEntry ? 1 : 8) : 0);
                            var cMBytesPP = (int)Math.Ceiling(cMapBpp / 8.0);
                            #endregion

                            Header.ColorMapSpec.ColorMapLength = Math.Min((ushort)colors.Length, ushort.MaxValue);
                            Header.ColorMapSpec.ColorMapEntrySize = (TgaColorMapEntrySize)cMapBpp;
                            ImageOrColorMapArea.ColorMapData = new byte[Header.ColorMapSpec.ColorMapLength * cMBytesPP];

                            var cMapEntry = new byte[cMBytesPP];

                            const float to5Bit = 32f / 256f; // Scale value from 8 to 5 bits.
                            for (var i = 0; i < colors.Length; i++)
                            {
                                switch (Header.ColorMapSpec.ColorMapEntrySize)
                                {
                                    case TgaColorMapEntrySize.A1R5G5B5:
                                    case TgaColorMapEntrySize.X1R5G5B5:
                                        var r = (int)(colors[i].R * to5Bit);
                                        var g = (int)(colors[i].G * to5Bit) << 5;
                                        var b = (int)(colors[i].B * to5Bit) << 10;
                                        var a = 0;

                                        if (Header.ColorMapSpec.ColorMapEntrySize == TgaColorMapEntrySize.A1R5G5B5)
                                            a = (colors[i].A & 0x80) << 15;

                                        cMapEntry = BitConverter.GetBytes(a | r | g | b);
                                        break;

                                    case TgaColorMapEntrySize.R8G8B8:
                                        cMapEntry[0] = colors[i].B;
                                        cMapEntry[1] = colors[i].G;
                                        cMapEntry[2] = colors[i].R;
                                        break;

                                    case TgaColorMapEntrySize.A8R8G8B8:
                                        cMapEntry[0] = colors[i].B;
                                        cMapEntry[1] = colors[i].G;
                                        cMapEntry[2] = colors[i].R;
                                        cMapEntry[3] = colors[i].A;
                                        break;

                                    case TgaColorMapEntrySize.Other:
                                    default:
                                        break;
                                }

                                Buffer.BlockCopy(cMapEntry, 0, ImageOrColorMapArea.ColorMapData, i * cMBytesPP, cMBytesPP);
                            }
                        }
                        #endregion

                        #region ImageType
                        if (UseRLE)
                        {
                            if (isGrayImage)
                                Header.ImageType = TgaImageType.RLE_BlackWhite;
                            else if (isColorMapped)
                                Header.ImageType = TgaImageType.RLE_ColorMapped;
                            else
                                Header.ImageType = TgaImageType.RLE_TrueColor;
                        }
                        else
                        {
                            if (isGrayImage)
                                Header.ImageType = TgaImageType.Uncompressed_BlackWhite;
                            else if (isColorMapped)
                                Header.ImageType = TgaImageType.Uncompressed_ColorMapped;
                            else
                                Header.ImageType = TgaImageType.Uncompressed_TrueColor;
                        }

                        Header.ColorMapType = isColorMapped ? TgaColorMapType.ColorMap : TgaColorMapType.NoColorMap;
                        #endregion

                        #region NewFormat
                        if (NewFormat)
                        {
                            Footer = new TgaFooter();
                            ExtArea = new TgaExtArea
                            {
                                DateTimeStamp = new TgaDateTime(DateTime.UtcNow)
                            };

                            if (isAlpha)
                            {
                                ExtArea.AttributesType = TgaAttrType.UsefulAlpha;

                                if (isPreAlpha)
                                    ExtArea.AttributesType = TgaAttrType.PreMultipliedAlpha;
                            }
                            else
                            {
                                ExtArea.AttributesType = TgaAttrType.NoAlpha;

                                if (Header.ImageSpec.ImageDescriptor.AlphaChannelBits > 0)
                                    ExtArea.AttributesType = TgaAttrType.UndefinedAlphaButShouldBeRetained;
                            }
                        }
                        #endregion

                        #region Bitmap width is aligned by 32 bits = 4 bytes! Delete it.
                        var strideBytes = bmp.Width * bytesPP;
                        var paddingBytes = (int)Math.Ceiling(strideBytes / 4.0) * 4 - strideBytes;

                        var imageData = new byte[(strideBytes + paddingBytes) * bmp.Height];

                        var re = new Rectangle(0, 0, bmp.Width, bmp.Height);
                        var bmpData = bmp.LockBits(re, ImageLockMode.ReadOnly, bmp.PixelFormat);
                        Marshal.Copy(bmpData.Scan0, imageData, 0, imageData.Length);
                        bmp.UnlockBits(bmpData);
                        bmpData = null;

                        if (paddingBytes > 0) //Need delete bytes align
                        {
                            ImageOrColorMapArea.ImageData = new byte[strideBytes * bmp.Height];
                            for (var i = 0; i < bmp.Height; i++)
                            {
                                Buffer.BlockCopy(imageData, i * (strideBytes + paddingBytes),
                                    ImageOrColorMapArea.ImageData, i * strideBytes, strideBytes);
                            }
                        }
                        else
                            ImageOrColorMapArea.ImageData = imageData;

                        imageData = null;

                        // Not official supported, but works (tested on 16bpp GrayScale test images)!
                        if (bmp.PixelFormat == PixelFormat.Format16bppGrayScale)
                        {
                            for (long i = 0; i < ImageOrColorMapArea.ImageData.Length; i++)
                                ImageOrColorMapArea.ImageData[i] ^= byte.MaxValue;
                        }
                        #endregion

                        break;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool SaveFunc(Stream stream)
        {
            try
            {
                if (stream == null)
                    throw new ArgumentNullException();
                if (!(stream.CanWrite && stream.CanSeek))
                    throw new FileLoadException("Stream writing or seeking is not avaiable!");

                if (!CheckAndUpdateOffsets(out _))
                    return false;

                var bw = new BinaryWriter(stream);
                bw.Write(Header.ToBytes());

                if (ImageOrColorMapArea.ImageID != null)
                    bw.Write(ImageOrColorMapArea.ImageID.ToBytes());

                if (Header.ColorMapType != TgaColorMapType.NoColorMap)
                    bw.Write(ImageOrColorMapArea.ColorMapData);

                #region ImageData
                if (Header.ImageType != TgaImageType.NoImageData)
                {
                    if (Header.ImageType >= TgaImageType.RLE_ColorMapped &&
                        Header.ImageType <= TgaImageType.RLE_BlackWhite)
                    {
                        bw.Write(RLE_Encode(ImageOrColorMapArea.ImageData, Width, Height));
                    }
                    else
                        bw.Write(ImageOrColorMapArea.ImageData);
                }
                #endregion

                #region Footer
                if (Footer != null)
                {
                    #region DevArea
                    if (DevArea != null)
                    {
                        for (var i = 0; i < DevArea.Count; i++)
                            bw.Write(DevArea[i].Data);

                        bw.Write((ushort)DevArea.Count);

                        for (var i = 0; i < DevArea.Count; i++)
                        {
                            bw.Write(DevArea[i].Tag);
                            bw.Write(DevArea[i].Offset);
                            bw.Write(DevArea[i].FieldSize);
                        }
                    }
                    #endregion

                    #region ExtArea
                    if (ExtArea != null)
                    {
                        bw.Write(ExtArea.ToBytes());

                        if (ExtArea.ScanLineTable != null)
                        {
                            for (var i = 0; i < ExtArea.ScanLineTable.Length; i++)
                                bw.Write(ExtArea.ScanLineTable[i]);
                        }

                        if (ExtArea.PostageStampImage != null)
                            bw.Write(ExtArea.PostageStampImage.ToBytes());

                        if (ExtArea.ColorCorrectionTable != null)
                        {
                            for (var i = 0; i < ExtArea.ColorCorrectionTable.Length; i++)
                                bw.Write(ExtArea.ColorCorrectionTable[i]);
                        }
                    }
                    #endregion

                    bw.Write(Footer.ToBytes());
                }
                #endregion

                bw.Flush();
                stream.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encode image with RLE compression (used RLE per line)!
        /// </summary>
        /// <param name="imageData">Image data, bytes array with size = Width * Height * BytesPerPixel.</param>
        /// <param name="width">Image Width, must be > 0.</param>
        /// <param name="height">Image Height, must be > 0.</param>
        /// <returns>Bytes array with RLE compressed image data.</returns>
        private byte[] RLE_Encode(byte[] imageData, int width, int height)
        {
            if (imageData == null)
                throw new ArgumentNullException(nameof(imageData) + "in null!");

            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException(nameof(width) + " and " + nameof(height) + " must be > 0!");

            var bpp = imageData.Length / width / height; // Bytes per pixel
            var scanLineSize = width * bpp;

            if (scanLineSize * height != imageData.Length)
                throw new ArgumentOutOfRangeException("ImageData has wrong Length!");

            try
            {
                var count = 0;
                var pos = 0;
                var isRLE = false;
                var encoded = new List<byte>();
                var rowData = new byte[scanLineSize];

                for (var y = 0; y < height; y++)
                {
                    pos = 0;
                    Buffer.BlockCopy(imageData, y * scanLineSize, rowData, 0, scanLineSize);

                    while (pos < scanLineSize)
                    {
                        if (pos >= scanLineSize - bpp)
                        {
                            encoded.Add(0);
                            encoded.AddRange(BitConverterExt.GetElements(rowData, pos, bpp));
                            pos += bpp;
                            break;
                        }

                        count = 0; //1
                        isRLE = BitConverterExt.IsElementsEqual(rowData, pos, pos + bpp, bpp);

                        for (var i = pos + bpp; i < Math.Min(pos + 128 * bpp, scanLineSize) - bpp; i += bpp)
                        {
                            if (isRLE ^ BitConverterExt.IsElementsEqual(rowData, isRLE ? pos : i, i + bpp, bpp))
                            {
                                //Count--;
                                break;
                            }
                            else
                                count++;
                        }

                        var countBpp = (count + 1) * bpp;
                        encoded.Add((byte)(isRLE ? count | 128 : count));
                        encoded.AddRange(BitConverterExt.GetElements(rowData, pos, isRLE ? bpp : countBpp));
                        pos += countBpp;
                    }
                }

                return encoded.ToArray();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert <see cref="TGA"/> to <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="forceUseAlpha">Force use alpha channel.</param>
        /// <param name="postageStampImage">Get Postage Stamp Image (Thumb) or get main image?</param>
        /// <returns>Bitmap or null, on error.</returns>
        private Bitmap ToBitmapFunc(bool forceUseAlpha = false, bool postageStampImage = false)
        {
            try
            {
                #region UseAlpha?
                var useAlpha = true;
                if (ExtArea != null)
                {
                    switch (ExtArea.AttributesType)
                    {
                        case TgaAttrType.NoAlpha:
                        case TgaAttrType.UndefinedAlphaCanBeIgnored:
                        case TgaAttrType.UndefinedAlphaButShouldBeRetained:
                            useAlpha = false;
                            break;
                        case TgaAttrType.UsefulAlpha:
                        case TgaAttrType.PreMultipliedAlpha:
                        default:
                            break;
                    }
                }
                useAlpha = (Header.ImageSpec.ImageDescriptor.AlphaChannelBits > 0 && useAlpha) | forceUseAlpha;
                #endregion

                #region IsGrayImage
                var isGrayImage = Header.ImageType == TgaImageType.RLE_BlackWhite ||
                    Header.ImageType == TgaImageType.Uncompressed_BlackWhite;
                #endregion

                #region Get PixelFormat
                var pixFormat = PixelFormat.Format24bppRgb;

                switch (Header.ImageSpec.PixelDepth)
                {
                    case TgaPixelDepth.Bpp8:
                        pixFormat = PixelFormat.Format8bppIndexed;
                        break;

                    case TgaPixelDepth.Bpp16:
                        if (isGrayImage)
                            pixFormat = PixelFormat.Format16bppGrayScale;
                        else
                            pixFormat = useAlpha ? PixelFormat.Format16bppArgb1555 : PixelFormat.Format16bppRgb555;
                        break;

                    case TgaPixelDepth.Bpp24:
                        pixFormat = PixelFormat.Format24bppRgb;
                        break;

                    case TgaPixelDepth.Bpp32:
                        if (useAlpha)
                        {
                            var f = Footer;
                            if (ExtArea?.AttributesType == TgaAttrType.PreMultipliedAlpha)
                                pixFormat = PixelFormat.Format32bppPArgb;
                            else
                                pixFormat = PixelFormat.Format32bppArgb;
                        }
                        else
                            pixFormat = PixelFormat.Format32bppRgb;

                        break;

                    default:
                        pixFormat = PixelFormat.Undefined;
                        break;
                }
                #endregion

                var bmpWidth = postageStampImage ? ExtArea.PostageStampImage.Width : Width;
                var bmpHeight = postageStampImage ? ExtArea.PostageStampImage.Height : Height;
                var bmp = new Bitmap(bmpWidth, bmpHeight, pixFormat);

                #region ColorMap and GrayPalette
                if (Header.ColorMapType == TgaColorMapType.ColorMap &&
                   (Header.ImageType == TgaImageType.RLE_ColorMapped ||
                    Header.ImageType == TgaImageType.Uncompressed_ColorMapped))
                {

                    var colorMap = bmp.Palette;
                    var cMapColors = colorMap.Entries;

                    switch (Header.ColorMapSpec.ColorMapEntrySize)
                    {
                        case TgaColorMapEntrySize.X1R5G5B5:
                        case TgaColorMapEntrySize.A1R5G5B5:
                            const float to8Bit = 255f / 31f; // Scale value from 5 to 8 bits.
                            for (var i = 0; i < Math.Min(cMapColors.Length, Header.ColorMapSpec.ColorMapLength); i++)
                            {
                                var a1r5g5b5 = BitConverter.ToUInt16(ImageOrColorMapArea.ColorMapData, i * 2);
                                var a = (useAlpha ? (a1r5g5b5 & 0x8000) >> 15 : 1) * 255; // (0 or 1) * 255
                                var r = (int)(((a1r5g5b5 & 0x7C00) >> 10) * to8Bit);
                                var g = (int)(((a1r5g5b5 & 0x3E0) >> 5) * to8Bit);
                                var b = (int)((a1r5g5b5 & 0x1F) * to8Bit);
                                cMapColors[i] = Color.FromArgb(a, r, g, b);
                            }
                            break;

                        case TgaColorMapEntrySize.R8G8B8:
                            for (var i = 0; i < Math.Min(cMapColors.Length, Header.ColorMapSpec.ColorMapLength); i++)
                            {
                                var index = i * 3; //RGB = 3 bytes
                                int r = ImageOrColorMapArea.ColorMapData[index + 2];
                                int g = ImageOrColorMapArea.ColorMapData[index + 1];
                                int b = ImageOrColorMapArea.ColorMapData[index];
                                cMapColors[i] = Color.FromArgb(r, g, b);
                            }
                            break;

                        case TgaColorMapEntrySize.A8R8G8B8:
                            for (var i = 0; i < Math.Min(cMapColors.Length, Header.ColorMapSpec.ColorMapLength); i++)
                            {
                                var argb = BitConverter.ToInt32(ImageOrColorMapArea.ColorMapData, i * 4);
                                cMapColors[i] = Color.FromArgb(useAlpha ? argb | (0xFF << 24) : argb);
                            }
                            break;

                        default:
                            colorMap = null;
                            break;
                    }

                    if (colorMap != null)
                        bmp.Palette = colorMap;
                }

                if (pixFormat == PixelFormat.Format8bppIndexed && isGrayImage)
                {
                    var grayPalette = bmp.Palette;
                    var grayColors = grayPalette.Entries;
                    for (var i = 0; i < grayColors.Length; i++)
                        grayColors[i] = Color.FromArgb(i, i, i);
                    bmp.Palette = grayPalette;
                }
                #endregion

                #region Bitmap width must by aligned (align value = 32 bits = 4 bytes)!
                byte[] imageData;
                var bytesPerPixel = (int)Math.Ceiling((double)Header.ImageSpec.PixelDepth / 8.0);
                var strideBytes = bmp.Width * bytesPerPixel;
                var paddingBytes = (int)Math.Ceiling(strideBytes / 4.0) * 4 - strideBytes;

                if (paddingBytes > 0) //Need bytes align
                {
                    imageData = new byte[(strideBytes + paddingBytes) * bmp.Height];
                    for (var i = 0; i < bmp.Height; i++)
                    {
                        Buffer.BlockCopy(postageStampImage ? ExtArea.PostageStampImage.Data :
                            ImageOrColorMapArea.ImageData, i * strideBytes, imageData,
                            i * (strideBytes + paddingBytes), strideBytes);
                    }
                }
                else
                {
                    imageData = BitConverterExt.ToBytes(postageStampImage ? ExtArea.PostageStampImage.Data :
                        ImageOrColorMapArea.ImageData);
                }

                // Not official supported, but works (tested on 2 test images)!
                if (pixFormat == PixelFormat.Format16bppGrayScale)
                {
                    for (long i = 0; i < imageData.Length; i++)
                        imageData[i] ^= byte.MaxValue;
                }
                #endregion

                var re = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bmpData = bmp.LockBits(re, ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(imageData, 0, bmpData.Scan0, imageData.Length);
                bmp.UnlockBits(bmpData);
                imageData = null;
                bmpData = null;

                if (ExtArea != null && ExtArea.KeyColor.ToInt() != 0)
                    bmp.MakeTransparent(ExtArea.KeyColor.ToColor());

                #region Flip Image
                switch (Header.ImageSpec.ImageDescriptor.ImageOrigin)
                {
                    case TgaImgOrigin.BottomLeft:
                        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
                        break;
                    case TgaImgOrigin.BottomRight:
                        bmp.RotateFlip(RotateFlipType.RotateNoneFlipXY);
                        break;
                    case TgaImgOrigin.TopLeft:
                    default:
                        break;
                    case TgaImgOrigin.TopRight:
                        bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                }
                #endregion

                return bmp;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Explicit
        public static explicit operator Bitmap(TGA tga) => tga.ToBitmap();
        public static explicit operator TGA(Bitmap bmp) => FromBitmap(bmp);
        #endregion

        #region PostageStamp Image
        /// <summary>
        /// Convert <see cref="TgaPostageStampImage"/> to <see cref="Bitmap"/>.
        /// </summary>
        /// <param name="forceUseAlpha">Force use alpha channel.</param>
        /// <returns>Bitmap or null.</returns>
        public Bitmap GetPostageStampImage(bool forceUseAlpha = false)
        {
            if (ExtArea == null || ExtArea.PostageStampImage == null || ExtArea.PostageStampImage.Data == null ||
                ExtArea.PostageStampImage.Width <= 0 || ExtArea.PostageStampImage.Height <= 0)
            {
                return null;
            }

            return ToBitmapFunc(forceUseAlpha, true);
        }

        /// <summary>
        /// Update Postage Stamp Image or set it.
        /// </summary>
        public void UpdatePostageStampImage()
        {
            if (Header.ImageType == TgaImageType.NoImageData)
            {
                if (ExtArea != null)
                    ExtArea.PostageStampImage = null;
                return;
            }

            ToNewFormat();
            if (ExtArea.PostageStampImage == null)
                ExtArea.PostageStampImage = new TgaPostageStampImage();

            int psWidth = Header.ImageSpec.ImageWidth;
            int psHeight = Header.ImageSpec.ImageHeight;

            if (Width > 64 || Height > 64)
            {
                var aspectRatio = Width / (float)Height;
                psWidth = (byte)(64f * (aspectRatio < 1f ? aspectRatio : 1f));
                psHeight = (byte)(64f / (aspectRatio > 1f ? aspectRatio : 1f));
            }

            psWidth = Math.Max(psWidth, 4);
            psHeight = Math.Max(psHeight, 4);

            ExtArea.PostageStampImage.Width = (byte)psWidth;
            ExtArea.PostageStampImage.Height = (byte)psHeight;

            var bytesPerPixel = (int)Math.Ceiling((double)Header.ImageSpec.PixelDepth / 8.0);
            ExtArea.PostageStampImage.Data = new byte[psWidth * psHeight * bytesPerPixel];

            var widthCoef = Width / (float)psWidth;
            var heightCoef = Height / (float)psHeight;

            for (var y = 0; y < psHeight; y++)
            {
                var yOffset1 = (int)(y * heightCoef) * Width * bytesPerPixel;
                var yOffset2 = y * psWidth * bytesPerPixel;

                for (var x = 0; x < psWidth; x++)
                {
                    Buffer.BlockCopy(ImageOrColorMapArea.ImageData, yOffset1 + (int)(x * widthCoef) * bytesPerPixel,
                        ExtArea.PostageStampImage.Data, yOffset2 + x * bytesPerPixel, bytesPerPixel);
                }
            }
        }

        public void DeletePostageStampImage()
        {
            if (ExtArea != null)
                ExtArea.PostageStampImage = null;
        }
        #endregion
    }
}