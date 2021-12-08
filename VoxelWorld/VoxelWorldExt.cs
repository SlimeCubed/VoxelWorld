using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelWorld
{
    internal static class VoxelWorldExt
    {
        public static int Read7BitEncodedInt(this BinaryReader self)
        {
            int count = 0;
            int shift = 0;
            byte b;
            do
            {
                if (shift == 5 * 7)  // 5 bytes max per Int32, shift += 7
                    throw new FormatException("Bad 7 bit integer format!");

                b = self.ReadByte();
                count |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);

            return count;
        }
    }
}
