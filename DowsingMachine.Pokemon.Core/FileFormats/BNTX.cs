using DirectXTexNet;
using Syroot.NintenTools.NSW.Bntx;
using Syroot.NintenTools.NSW.Bntx.GFX;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Toolbox.Library;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

public class BNTX : IDisposable
{
    public string Name { get; set; }
    public Dictionary<string, Bitmap> Textures { get; set; }

    public BNTX(string path)
    {
        BntxFile bf = null;
        try
        {
            bf = new BntxFile(path);
        }
        catch (Exception ex)
        {
            return;
        }
        Load(bf);
    }

    public BNTX(byte[] data)
    {
        using var ms = new MemoryStream(data);
        BntxFile bf = null;
        try
        {
            bf = new BntxFile(ms);
        }
        catch (Exception ex)
        {
            return;
        }
        Load(bf);
    }

    public BNTX(Stream stream)
    {
        BntxFile bf = null;
        try
        {
            bf = new BntxFile(stream);
        }
        catch (Exception ex)
        {
            return;
        }
        Load(bf);
    }


    public void Dispose()
    {
        if (Textures != null)
        {
            foreach (var bmp in Textures.Values)
            {
                bmp.Dispose();
            }
            Textures.Clear();
        }
    }

    public void Output(string path, OutputMode mode)
    {
        if (Textures == null || Textures.Count == 0) return;

        switch (mode)
        {
            case OutputMode.CreateFolderWhenMultiple when Textures.Count == 1 && Textures.First().Key == Name:
                Directory.CreateDirectory(path);
                Textures.First().Value.Save(Path.Combine(path, Textures.First().Key + ".png"));
                break;
            case OutputMode.IgnoreTextureNameWhenSingle when Textures.Count == 1:
                Directory.CreateDirectory(path);
                Textures.First().Value.Save(Path.Combine(path, Name + ".png"));
                break;
            case OutputMode.CreateFolder or OutputMode.CreateFolderWhenMultiple or OutputMode.IgnoreTextureNameWhenSingle:
                var pngFolder = Path.Combine(path, Path.GetFileNameWithoutExtension(Name));
                Directory.CreateDirectory(pngFolder);
                foreach (var (name, bmp) in Textures)
                {
                    bmp.Save(Path.Combine(pngFolder, name + ".png"));
                }
                break;
            case OutputMode.IgnoreFilename:
                Directory.CreateDirectory(path);
                foreach (var (name, bmp) in Textures)
                {
                    bmp.Save(Path.Combine(path, name + ".png"));
                }
                break;
        }
    }

    public enum OutputMode
    {
        CreateFolder,
        CreateFolderWhenMultiple,
        IgnoreFilename,
        IgnoreTextureNameWhenSingle,
    }

    private void Load(BntxFile bf)
    {
        Name = bf.Name;
        Textures = new();
        for (var i = 0; i < bf.Textures.Count; i++)
        {
            var tex = bf.Textures[i];
            //if(tex.Format == SurfaceFormat.D32_FLOAT_S8X24_UINT) { continue; }
            var data = GetImageData(tex);


            switch (tex.Format)
            {
                case SurfaceFormat.BC1_UNORM:
                case SurfaceFormat.BC1_SRGB:
                case SurfaceFormat.BC2_SRGB:
                case SurfaceFormat.BC2_UNORM:
                case SurfaceFormat.BC3_UNORM:
                case SurfaceFormat.BC3_SRGB:
                case SurfaceFormat.BC4_UNORM:
                case SurfaceFormat.BC4_SNORM:
                case SurfaceFormat.BC5_UNORM:
                case SurfaceFormat.BC5_SNORM:
                case SurfaceFormat.BC6_UFLOAT:
                case SurfaceFormat.BC6_FLOAT:
                case SurfaceFormat.BC7_UNORM:
                case SurfaceFormat.BC7_SRGB:
                    data = DecompressBlock(data, (int)tex.Width, (int)tex.Height, tex.Format.ToString());
                    break;
                case var fmt when fmt.ToString().Contains("ASTC"):
                    throw new NotImplementedException();
                case SurfaceFormat.R8_G8_B8_A8_UNORM:
                    break;
                default:
                    data = Convert(data, (int)tex.Width, (int)tex.Height, tex.Format.ToString(), DXGI_FORMAT.R8G8B8A8_UNORM);
                    break;
            }
            var data3 = GetImageData(tex, data);
            var bmp = GetBitmap(data3, (int)tex.Width, (int)tex.Height);
            Textures.Add(tex.Name, bmp);
        }
    }

    public static Bitmap GetBitmap(byte[] Buffer, int width, int height, PixelFormat pixelFormat = PixelFormat.Format32bppArgb)
    {
        Rectangle Rect = new Rectangle(0, 0, width, height);
        Bitmap Img = new Bitmap(width, height, pixelFormat);
        BitmapData ImgData = Img.LockBits(Rect, ImageLockMode.WriteOnly, Img.PixelFormat);
        Marshal.Copy(Buffer, 0, ImgData.Scan0, Buffer.Length);
        Img.UnlockBits(ImgData);
        return Img;
    }

    private static byte[] GetImageData(Texture tex, byte[] buffer)
    {
        var dest = new byte[buffer.Length];
        var channels = new[]
        {
            tex.ChannelBlue,
            tex.ChannelGreen,
            tex.ChannelRed,
            tex.ChannelAlpha,
        };

        for (var i = 0; i < buffer.Length; i += 4)
        {
            for (var j = 0; j <= 3; j++)
            {
                dest[i + j] = channels[j] switch
                {
                    ChannelType.Zero => 0,
                    ChannelType.One => 255,
                    ChannelType.Red => buffer[i + 0],
                    ChannelType.Green => buffer[i + 1],
                    ChannelType.Blue => buffer[i + 2],
                    ChannelType.Alpha => buffer[i + 3],
                };
            }
        }

        var x = new Dictionary<uint, (int, int)>
        {
            { 0x1a, (4, 4) }
        };

        return dest;


    }

    // https://github.com/aboood40091/BNTX-Injector/blob/master/globals.py

    public static Dictionary<uint, (uint, uint)> BlockDims = new()
    {
        { 0x1a, (4, 4) },
        { 0x1b, (4, 4) },
        { 0x1c, (4, 4) },
        { 0x1d, (4, 4) },
        { 0x1e, (4, 4) },
        { 0x1f, (4, 4) },
        { 0x20, (4, 4) },
        { 0x2d, (4, 4) },
        { 0x2e, (5, 4) },
        { 0x2f, (5, 5) },
        { 0x30, (6, 5) },
        { 0x31, (6, 6) },
        { 0x32, (8, 5) },
        { 0x33, (8, 6) },
        { 0x34, (8, 8) },
        { 0x35, (10, 5) },
        { 0x36, (10, 6) },
        { 0x37, (10, 8) },
        { 0x38, (10, 10) },
        { 0x39, (12, 10) },
        { 0x3a, (12, 12) },
    };

    public static Dictionary<uint, uint> BytesPerPixels = new()
    {
        { 0x01, 0x01 },
        { 0x02, 0x01 },
        { 0x03, 0x02 },
        { 0x05, 0x02 },
        { 0x07, 0x02 },
        { 0x09, 0x02 },
        { 0x0b, 0x04 },
        { 0x0e, 0x04 },
        { 0x1a, 0x08 },
        { 0x1b, 0x10 },
        { 0x1c, 0x10 },
        { 0x1d, 0x08 },
        { 0x1e, 0x10 },
        { 0x1f, 0x10 },
        { 0x20, 0x10 },
        { 0x2d, 0x10 },
        { 0x2e, 0x10 },
        { 0x2f, 0x10 },
        { 0x30, 0x10 },
        { 0x31, 0x10 },
        { 0x32, 0x10 },
        { 0x33, 0x10 },
        { 0x34, 0x10 },
        { 0x35, 0x10 },
        { 0x36, 0x10 },
        { 0x37, 0x10 },
        { 0x38, 0x10 },
        { 0x39, 0x10 },
        { 0x3a, 0x10 },

        { 0x0a, 0x02 },
        { 0x15, 0x08 },
    };

    private static byte[] GetImageData(Texture tex)
    {
        int ArrayLevel = 0; int MipLevel = 0; int DepthLevel = 0;

        int target = 1;

        var type = (uint)tex.Format >> 8;
        var bpp = BytesPerPixels[type];
        var (blkWidth, blkHeight) = BlockDims.ContainsKey(type) ? BlockDims[type] : (1, 1);

        uint blkDepth = 1;

        int linesPerBlockHeight = (1 << (int)tex.BlockHeightLog2) * 8;

        uint numDepth = 1;
        //if (Depth > 1) numDepth = Depth;

        for (int depthLevel = 0; depthLevel < numDepth; depthLevel++)
        {
            for (int arrayLevel = 0; arrayLevel < tex.TextureData.Count; arrayLevel++)
            {
                int blockHeightShift = 0;

                for (int mipLevel = 0; mipLevel < tex.TextureData[arrayLevel].Count; mipLevel++)
                {
                    uint width = (uint)Math.Max(1, tex.Width >> mipLevel);
                    uint height = (uint)Math.Max(1, tex.Height >> mipLevel);
                    uint depth = (uint)Math.Max(1, tex.Depth >> mipLevel);

                    uint size = TegraX1Swizzle.DIV_ROUND_UP(width, blkWidth) * TegraX1Swizzle.DIV_ROUND_UP(height, blkHeight) * bpp;

                    if (TegraX1Swizzle.pow2_round_up(TegraX1Swizzle.DIV_ROUND_UP(height, blkWidth)) < linesPerBlockHeight)
                        blockHeightShift += 1;

                    byte[] result = TegraX1Swizzle.deswizzle(width, height, depth, blkWidth, blkHeight, blkDepth, target, bpp, (uint)tex.TileMode, (int)Math.Max(0, tex.BlockHeightLog2 - blockHeightShift), tex.TextureData[arrayLevel][mipLevel]);
                    //Create a copy and use that to remove uneeded data
                    byte[] result_ = new byte[size];
                    Array.Copy(result, 0, result_, 0, size);

                    result = null;

                    if (ArrayLevel == arrayLevel && MipLevel == mipLevel && DepthLevel == depthLevel)
                        return result_;
                }
            }
        }

        return new byte[0];
    }

    private static DXGI_FORMAT ToDxgiFormat(string format)
    {
        format = format.Replace("_SRGB", "_UNORM_SRGB");
        format = Regex.Replace(format, @"([A-Z]\d+)_([A-Z]\d+)_([A-Z]\d+)_([A-Z]\d+)_", "$1$2$3$4_");
        format = Regex.Replace(format, @"([A-Z]\d+)_([A-Z]\d+)_([A-Z]\d+)_", "$1$2$3_");
        format = Regex.Replace(format, @"([A-Z]\d+)_([A-Z]\d+)_", "$1$2_");
        format = format switch
        {
            "BC6_UFLOAT" => "BC6H_UF16",
            "BC6_FLOAT" => "BC6H_SF16",
            _ => format
        };
        return Enum.Parse<DXGI_FORMAT>(format);
    }

    public static unsafe byte[] DecompressBlock(byte[] data, int width, int height, string format)
    {
        DXGI_FORMAT dxgiFormat = ToDxgiFormat(format);

        TexHelper.Instance.ComputePitch(dxgiFormat, width, height, out long inputRowPitch, out long inputSlicePitch, CP_FLAGS.NONE);

        DXGI_FORMAT FormatDecompressed;
        if (dxgiFormat.ToString().Contains("SRGB"))
            FormatDecompressed = DXGI_FORMAT.R8G8B8A8_UNORM_SRGB;
        else
            FormatDecompressed = DXGI_FORMAT.R8G8B8A8_UNORM;

        byte* buf;
        buf = (byte*)Marshal.AllocHGlobal((int)inputSlicePitch);
        Marshal.Copy(data, 0, (IntPtr)buf, (int)inputSlicePitch);

        var inputImage = new DirectXTexNet.Image(width, height, dxgiFormat, inputRowPitch, inputSlicePitch, (IntPtr)buf, null);
        var texMetadata = new TexMetadata(width, height, 1, 1, 1, 0, 0, dxgiFormat, TEX_DIMENSION.TEXTURE2D);
        using var scratchImage = TexHelper.Instance.InitializeTemporary(new[] { inputImage }, texMetadata, null);

        using var decomp = scratchImage.Decompress(0, FormatDecompressed);
        byte[] result = new byte[4 * width * height];
        Marshal.Copy(decomp.GetImage(0).Pixels, result, 0, result.Length);

        return result;
    }

    public static unsafe byte[] Convert(byte[] data, int width, int height, string format, DXGI_FORMAT outputFormat)
    {
        DXGI_FORMAT inputFormat = ToDxgiFormat(format);

        TexHelper.Instance.ComputePitch(inputFormat, width, height, out long inputRowPitch, out long inputSlicePitch, CP_FLAGS.NONE);

        if (data.Length != inputSlicePitch)
        {
            return null;
        }

        byte* buf;
        buf = (byte*)Marshal.AllocHGlobal((int)inputSlicePitch);
        Marshal.Copy(data, 0, (IntPtr)buf, (int)inputSlicePitch);

        var inputImage = new DirectXTexNet.Image(
            width, height, inputFormat, inputRowPitch,
            inputSlicePitch, (IntPtr)buf, null);

        var texMetadata = new TexMetadata(width, height, 1, 1, 1, 0, 0, inputFormat, TEX_DIMENSION.TEXTURE2D);

        using var scratchImage = TexHelper.Instance.InitializeTemporary(new[] { inputImage }, texMetadata, null);

        var convFlags = TEX_FILTER_FLAGS.DEFAULT;

        if (inputFormat == DXGI_FORMAT.B8G8R8A8_UNORM_SRGB ||
         inputFormat == DXGI_FORMAT.B8G8R8X8_UNORM_SRGB ||
         inputFormat == DXGI_FORMAT.R8G8B8A8_UNORM_SRGB)
        {
            convFlags |= TEX_FILTER_FLAGS.SRGB;
        }

        using var decomp = scratchImage.Convert(0, outputFormat, convFlags, 0.5f);
        TexHelper.Instance.ComputePitch(outputFormat, width, height, out long outRowPitch, out long outSlicePitch, CP_FLAGS.NONE);

        byte[] result = new byte[outSlicePitch];
        Marshal.Copy(decomp.GetImage(0).Pixels, result, 0, result.Length);

        return result;
    }

}
