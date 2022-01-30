using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using Palmmedia.ReportGenerator.Core.Common;
using Palmmedia.ReportGenerator.Core.Properties;

namespace Palmmedia.ReportGenerator.Core.Parser.FileReading
{
    /// <summary>
    /// File reader for reading files from local disk.
    /// </summary>
    internal class AltCoverEmbeddedFileReader : IFileReader
    {
        /// <summary>
        /// Line endings to split lines on Windows and Unix.
        /// </summary>
        private static readonly string[] LineEndings = new string[]
        {
            "\r\n",
            "\n"
        };

        /// <summary>
        /// The Base64 and deflate compressed file.
        /// </summary>
        private readonly string base64DeflateCompressedFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="AltCoverEmbeddedFileReader" /> class.
        /// </summary>
        /// <param name="base64DeflateCompressedFile">The Base64 and deflate compressed file.</param>
        public AltCoverEmbeddedFileReader(string base64DeflateCompressedFile)
        {
            this.base64DeflateCompressedFile = base64DeflateCompressedFile ?? throw new ArgumentNullException(nameof(base64DeflateCompressedFile));
        }

        /// <summary>
        /// Loads the file with the given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="error">Error message if file reading failed, otherwise <code>null</code>.</param>
        /// <returns><code>null</code> if an error occurs, otherwise the lines of the file.</returns>
        public string[] LoadFile(string path, out string error)
        {
            try
            {
                byte[] base64Decoded = Convert.FromBase64String(this.base64DeflateCompressedFile);
                byte[] decompressed = this.Decompress(base64Decoded);
                string content = Encoding.UTF8.GetString(decompressed);

                string[] lines = content.Split(LineEndings, StringSplitOptions.None);

                error = null;
                return lines;
            }
            catch (Exception ex)
            {
                error = string.Format(CultureInfo.InvariantCulture, Resources.ErrorDuringReadingFile, path, ex.GetExceptionMessageForDisplay());
                return null;
            }
        }

        private byte[] Decompress(byte[] data)
        {
            byte[] decompressedArray = null;
            using (MemoryStream decompressedStream = new MemoryStream())
            {
                using (MemoryStream compressStream = new MemoryStream(data))
                {
                    using (DeflateStream deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(decompressedStream);
                    }
                }

                decompressedArray = decompressedStream.ToArray();
            }

            return decompressedArray;
        }
    }
}
