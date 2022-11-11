using PBT.DowsingMachine.Projects;
using System.Diagnostics;

namespace PBT.DowsingMachine.Pokemon.Core.FileFormats;

// Compressed Nitro Archives
// https://github.com/Plombo/vcromclaim/blob/master/lzh8.py
public class CARC : IObjectArchive<byte[]>
{
    public byte[] Data { get; set; }

    public CARC()
    {
    }

    public void Open(string path)
    {
        using var fs = File.OpenRead(path);
        Data = Decompress(fs, (int)fs.Length);
    }

    public void Open(byte[] data)
    {
        using var ms = new MemoryStream(data);
        Data = Decompress(ms, data.Length);
    }

    #region "Decompress"
    private long input_offset = 0;
    private int bit_pool = 0;
    private int bits_left = 0;

    private ushort Get_next_bits(Stream instream, int bit_count)
    {
        var offset_p = input_offset;
        var bit_pool_p = bit_pool;
        var bits_left_p = bits_left;
        ushort out_bits = 0;
        var num_bits_produced = 0;

        while (num_bits_produced < bit_count)
        {
            if (bits_left_p == 0)
            {
                instream.Seek(offset_p, SeekOrigin.Begin);
                bit_pool_p = instream.ReadByte();
                bits_left_p = 8;
                offset_p += 1;
            }

            int bits_this_round;
            if (bits_left_p > bit_count - num_bits_produced)
            {
                bits_this_round = bit_count - num_bits_produced;
            }
            else
            {
                bits_this_round = bits_left_p;
            }

            out_bits <<= bits_this_round;
            out_bits |= (ushort)(bit_pool_p >> bits_left_p - bits_this_round & (1 << bits_this_round) - 1);
            bits_left_p -= bits_this_round;
            num_bits_produced += bits_this_round;
        }
        input_offset = offset_p;
        bit_pool = bit_pool_p;
        bits_left = bits_left_p;
        return out_bits;
    }

    private byte[] Decompress(Stream instream, long inLength)
    {
        var type = instream.ReadByte();
        if (type != 0x40) throw new InvalidDataException("not LZH8");

        int uncompressed_length = instream.ReadByte() | instream.ReadByte() << 8 | instream.ReadByte() << 16;
        if (uncompressed_length == 0)
        {
            uncompressed_length = instream.ReadByte() | instream.ReadByte() << 8 | instream.ReadByte() << 16 | instream.ReadByte() << 24;
        }

        var outputBuffer = new byte[uncompressed_length];

        var LENBITS = 9;
        var DISPBITS = 5;
        var LENCNT = 1 << LENBITS;
        var DISPCNT = 1 << DISPBITS;

        // allocate backreference length decode table
        var length1 = instream.ReadByte() | instream.ReadByte() << 8;
        var length_table_bytes = (length1 + 1) * 4;
        var length_decode_table_size = LENCNT * 2;
        var length_decode_table = new ushort[length_decode_table_size * 2];

        input_offset = instream.Position;

        // read backreference length decode table
        var start_input_offset = input_offset - 2;
        var i = 1;
        bits_left = 0;
        while (input_offset - start_input_offset < length_table_bytes)
        {
            if (i >= length_decode_table_size) break;
            length_decode_table[i] = Get_next_bits(instream, LENBITS);
            i += 1;
        }
        input_offset = start_input_offset + length_table_bytes;
        bits_left = 0;


        // allocate backreference displacement length decode table
        instream.Seek(input_offset, SeekOrigin.Begin);
        var displen_table_bytes = (instream.ReadByte() + 1) * 4;
        input_offset += 1;
        var displen_decode_table = new ushort[DISPCNT * 2];

        // read backreference displacement length decode table
        start_input_offset = input_offset - 1;
        i = 1;
        bits_left = 0;
        while (input_offset - start_input_offset < displen_table_bytes)
        {
            if (i >= length_decode_table_size) break;
            displen_decode_table[i] = Get_next_bits(instream, DISPBITS);
            i += 1;
        }

        input_offset = start_input_offset + displen_table_bytes;
        bits_left = 0;
        var bytes_decoded = 0;

        // main decode loop
        while (bytes_decoded < uncompressed_length)
        {
            var length_table_offset = 1;
            // get next backreference length or literal byte
            while (true)
            {
                var next_length_child = Get_next_bits(instream, 1);
                var length_node_payload = length_decode_table[length_table_offset] & 0x7F;
                var next_length_table_offset = length_table_offset / 2 * 2 + (length_node_payload + 1) * 2 + (next_length_child == 0 ? 0 : next_length_child);
                var next_length_child_isleaf = length_decode_table[length_table_offset] & 0x100 >> next_length_child;
                if (next_length_child_isleaf != 0)
                {
                    var length = length_decode_table[next_length_table_offset];

                    if (0x100 > length)
                    {
                        outputBuffer[bytes_decoded] = (byte)length;
                        bytes_decoded += 1;
                    }
                    else
                    {
                        length = (ushort)((length & 0xFF) + 3);
                        var displen_table_offset = 1;
                        while (true)
                        {
                            var next_displen_child = Get_next_bits(instream, 1);
                            var displen_node_payload = displen_decode_table[displen_table_offset] & 0x7;
                            var next_displen_table_offset = displen_table_offset / 2 * 2 + (displen_node_payload + 1) * 2 + (next_displen_child == 0 ? 0 : next_displen_child);
                            var next_displen_child_isleaf = displen_decode_table[displen_table_offset] & 0x10 >> next_displen_child;
                            if (next_displen_child_isleaf != 0)
                            {
                                var displen = displen_decode_table[next_displen_table_offset];
                                var displacement = 0;
                                if (displen != 0)
                                {
                                    displacement = 1;
                                    for (i = displen - 1; i > 0; i--)
                                    {
                                        displacement *= 2;
                                        var next_bit = Get_next_bits(instream, 1);
                                        displacement |= next_bit;
                                    }
                                }

                                for (i = 0; i < length; i++)
                                {
                                    outputBuffer[bytes_decoded] = outputBuffer[bytes_decoded - displacement - 1];
                                    bytes_decoded += 1;
                                    if (bytes_decoded >= uncompressed_length) break;
                                }

                                break;
                            }
                            else
                            {
                                Debug.Assert(next_displen_table_offset != displen_table_offset);
                                displen_table_offset = next_displen_table_offset;
                            }
                        }
                    }
                    break;
                }
                else
                {
                    Debug.Assert(next_length_table_offset != length_table_offset);
                    length_table_offset = next_length_table_offset;
                }
            }
        }

        return outputBuffer;
    }


    #endregion
}

