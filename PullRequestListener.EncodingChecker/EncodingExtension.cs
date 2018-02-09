using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PullRequestListener.EncodingChecker
{
    public static class EncodingExtension
    {
        public static bool IsOfEncoding(this Encoding encoding, Stream stream, bool? ByteOrderMarkAllowed = null)
        {
            if (!stream.CanRead)
            {
                throw new Exception("Readable stream is required");
            }

            byte[] originalBytes;
            byte[] comparisonBytes;
            byte[] headerBytes = new byte[4];

            using (MemoryStream originalStream = new MemoryStream())
            {
                for (int i = 0; i < 4; i++)
                {
                    headerBytes[i] = Convert.ToByte(stream.ReadByte());
                }

                if (ByteOrderMarkAllowed.HasValue)
                {
                    if (ByteOrderMarkAllowed.Value && encoding == Encoding.ASCII)
                    {
                        throw new Exception("ACSII does not have a Byte Order Mark (BOM)");
                    }
                    if (HasByteOrderMark(headerBytes) == !ByteOrderMarkAllowed)
                    {
                        return false;
                    }
                }
                stream.CopyTo(originalStream);
                originalBytes = headerBytes.Concat(originalStream.ToArray()).ToArray();
            }

            foreach (EncodingInfo comparisonEncodingInfo in Encoding.GetEncodings())
            {
                Encoding comparisonEncoding = comparisonEncodingInfo.GetEncoding();

                comparisonBytes = Encoding.Convert(encoding, comparisonEncoding, originalBytes);
                if (Enumerable.SequenceEqual(originalBytes, comparisonBytes)
                    && encoding == comparisonEncoding)
                {
                    return true;
                }
            }
            return false;
        }
        private static bool HasByteOrderMark(byte[] bytes)
        {
            if (bytes[0] == 239 && bytes[1] == 187 && bytes[2] == 191) //UTF8 BOM - EE BB FF
            {
                return true;
            }
            else if (bytes[0] == 254 && bytes[1] == 255) //UTF-16(BE) - FE FF
            {
                return true;
            }
            else if (bytes[0] == 255 && bytes[1] == 254) //UTF-16(LE) - FF FE
            {
                return true;
            }
            else if (bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 254 && bytes[3] == 255) //UTF-32(BE) - 00 00 FE FF
            {
                return true;
            }
            else if (bytes[0] == 255 && bytes[1] == 254 && bytes[2] == 0 && bytes[3] == 0) //UTF-32(LE) - FF FE 00 00
            {
                return true;
            }
            else if (bytes[0] == 43 && bytes[1] == 47 && bytes[2] == 118) //UTF-7 - 2B 2F 76
            {
                return true;
            }
            else if (bytes[0] == 247 && bytes[1] == 100 && bytes[2] == 76) //UTF-1 - F7 64 4C
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
