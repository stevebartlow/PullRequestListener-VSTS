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

            HashAlgorithm hashAlgorithm = HMACSHA512.Create();
            byte[] sourceHash = hashAlgorithm.ComputeHash(stream);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (StreamReader streamReader = new StreamReader(stream))
                using (StreamWriter streamWriter = new StreamWriter(memoryStream, encoding))
                {
                    while (!streamReader.EndOfStream)
                    {
                        streamWriter.Write(streamReader.Read());
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                byte[] compareHash = hashAlgorithm.ComputeHash(memoryStream);

                return Array.Equals(sourceHash, compareHash);
            }
        }
    }
}
