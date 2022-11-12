using PBT.DowsingMachine.Projects;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

// https://github.com/Dirbaio/NSMB-Editor/blob/bef1e962e412b810eb0657386fc89749485ba7df/NSMBe4/ROM.cs
public class Overlay : IObjectArchive<byte[]>
{
    public byte[] Data { get; set; }

    public Overlay()
    {
    }

    public Overlay(string path)
    {
        Open(path);
    }

    public Overlay(byte[] data)
    {
        Open(data);
    }

    public void Open(string path)
    {
        var data = File.ReadAllBytes(path);
        Open(data);
    }

    public void Open(byte[] data)
    {
        Data = DecompressOverlay(data);
    }

    public void Open(Stream stream)
    {
        throw new NotSupportedException();
    }

    private static byte[] DecompressOverlay(byte[] sourcedata)
    {
        uint DataVar1, DataVar2;
        //uint last 8-5 bytes
        DataVar1 = (uint)(sourcedata[^8] | (sourcedata[^7] << 8) | (sourcedata[^6] << 16) | (sourcedata[^5] << 24));
        //uint last 4 bytes
        DataVar2 = (uint)(sourcedata[^4] | (sourcedata[^3] << 8) | (sourcedata[^2] << 16) | (sourcedata[^1] << 24));

        if(DataVar2 == 0)
        {
            return sourcedata;
        }

        byte[] memory = new byte[sourcedata.Length + DataVar2];
        sourcedata.CopyTo(memory, 0);

        uint r0, r1, r2, r3, r5, r6, r7, r12;
        bool N, V;
        r0 = (uint)sourcedata.Length;

        if (r0 == 0)
        {
            return null;
        }
        r1 = DataVar1;
        r2 = DataVar2;
        r2 = r0 + r2; //length + datavar2 -> decompressed length
        r3 = r0 - (r1 >> 0x18); //delete the latest 3 bits??
        r1 &= 0xFFFFFF; //save the latest 3 bits
        r1 = r0 - r1;
    a958:
        if (r3 <= r1)
        { //if r1 is 0 they will be equal
            goto a9B8; //return the memory buffer
        }
        r3 -= 1;
        r5 = memory[r3];
        r6 = 8;
    a968:
        SubS(out r6, r6, 1, out N, out V);
        if (N != V)
        {
            goto a958;
        }
        if ((r5 & 0x80) != 0)
        {
            goto a984;
        }
        r3 -= 1;
        r0 = memory[r3];
        r2 -= 1;
        memory[r2] = (byte)r0;
        goto a9AC;
    a984:
        r3 -= 1;
        r12 = memory[r3];
        r3 -= 1;
        r7 = memory[r3];
        r7 |= (r12 << 8);
        r7 &= 0xFFF;
        r7 += 2;
        r12 += 0x20;
    a99C:
        r0 = memory[r2 + r7];
        r2 -= 1;
        memory[r2] = (byte)r0;
        SubS(out r12, r12, 0x10, out N, out V);
        if (N == V)
        {
            goto a99C;
        }
    a9AC:
        r5 <<= 1;
        if (r3 > r1)
        {
            goto a968;
        }
    a9B8:
        return memory;
    }

    private static void SubS(out uint dest, uint v1, uint v2, out bool N, out bool V)
    {
        dest = v1 - v2;
        N = (dest & 2147483648) != 0;
        V = ((((v1 & 2147483648) != 0) && ((v2 & 2147483648) == 0) && ((dest & 2147483648) == 0)) || ((v1 & 2147483648) == 0) && ((v2 & 2147483648) != 0) && ((dest & 2147483648) != 0));
    }

    public static byte[] LZ77_Decompress(byte[] source)
    {
        // This code converted from Elitemap 
        int DataLen;
        DataLen = source[1] | (source[2] << 8) | (source[3] << 16);
        byte[] dest = new byte[DataLen];
        int i, j, xin, xout;
        xin = 4;
        xout = 0;
        int length, offset, windowOffset, data;
        byte d;
        while (DataLen > 0)
        {
            d = source[xin++];
            if (d != 0)
            {
                for (i = 0; i < 8; i++)
                {
                    if ((d & 0x80) != 0)
                    {
                        data = ((source[xin] << 8) | source[xin + 1]);
                        xin += 2;
                        length = (data >> 12) + 3;
                        offset = data & 0xFFF;
                        windowOffset = xout - offset - 1;
                        for (j = 0; j < length; j++)
                        {
                            dest[xout++] = dest[windowOffset++];
                            DataLen--;
                            if (DataLen == 0)
                            {
                                return dest;
                            }
                        }
                    }
                    else
                    {
                        dest[xout++] = source[xin++];
                        DataLen--;
                        if (DataLen == 0)
                        {
                            return dest;
                        }
                    }
                    d <<= 1;
                }
            }
            else
            {
                for (i = 0; i < 8; i++)
                {
                    dest[xout++] = source[xin++];
                    DataLen--;
                    if (DataLen == 0)
                    {
                        return dest;
                    }
                }
            }
        }
        return dest;
    }

}
