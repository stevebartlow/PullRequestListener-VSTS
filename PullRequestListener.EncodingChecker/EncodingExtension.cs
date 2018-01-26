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
        public static bool isOfEncoding(this Encoding encoding, Stream stream)
        {
            if (!stream.CanSeek)
            {
                throw new Exception("Seekable stream is required");
            }
            if (!stream.CanRead)
            {
                throw new Exception("Readable stream is required");
            }

            byte[] originalBytes;
            byte[] comparisonBytes;

            using (MemoryStream originalStream = new MemoryStream())
            {
                stream.CopyTo(originalStream);
                originalBytes = originalStream.ToArray();
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
            }
            return false;
        }
    }
}
