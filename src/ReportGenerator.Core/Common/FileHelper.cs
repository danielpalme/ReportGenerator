using System;
using System.IO;
using System.Text;

namespace Palmmedia.ReportGenerator.Core.Common
{
    /// <summary>
    /// Helper class to detect encoding of files.
    /// </summary>
    public class FileHelper
    {
        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM) and if not found try parsing into diferent encodings.
        /// Defaults to UTF8 when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(string filename)
        {
            var encodingByBOM = GetEncodingByBOM(filename);
            if (encodingByBOM != null)
            {
                return encodingByBOM;
            }

            // BOM not found, so try to parse characters into several encodings
            var encodingByParsingUTF8 = GetEncodingByParsing(filename, Encoding.UTF8);
            if (encodingByParsingUTF8 != null)
            {
                return encodingByParsingUTF8;
            }

            var encodingByParsingLatin1 = GetEncodingByParsing(filename, Encoding.GetEncoding("iso-8859-1"));
            if (encodingByParsingLatin1 != null)
            {
                return encodingByParsingLatin1;
            }

            var encodingByParsingUTF7 = GetEncodingByParsing(filename, Encoding.UTF7);
            if (encodingByParsingUTF7 != null)
            {
                return encodingByParsingUTF7;
            }

            return Encoding.UTF8;
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        private static Encoding GetEncodingByBOM(string filename)
        {
            // Read the BOM
            var byteOrderMark = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(byteOrderMark, 0, 4);
            }

            // Analyze the BOM
            if (byteOrderMark[0] == 0x2b && byteOrderMark[1] == 0x2f && byteOrderMark[2] == 0x76)
            {
                return Encoding.UTF7;
            }

            if (byteOrderMark[0] == 0xef && byteOrderMark[1] == 0xbb && byteOrderMark[2] == 0xbf)
            {
                return Encoding.UTF8;
            }

            if (byteOrderMark[0] == 0xff && byteOrderMark[1] == 0xfe)
            {
                // UTF-16LE
                return Encoding.Unicode;
            }

            if (byteOrderMark[0] == 0xfe && byteOrderMark[1] == 0xff)
            {
                // UTF-16BE
                return Encoding.BigEndianUnicode;
            }

            if (byteOrderMark[0] == 0 && byteOrderMark[1] == 0 && byteOrderMark[2] == 0xfe && byteOrderMark[3] == 0xff)
            {
                return Encoding.UTF32;
            }

            // no BOM found
            return null;
        }

        private static Encoding GetEncodingByParsing(string filename, Encoding encoding)
        {
            var encodingVerifier = Encoding.GetEncoding(encoding.BodyName, new EncoderExceptionFallback(), new DecoderExceptionFallback());

            try
            {
                using (var textReader = new StreamReader(filename, encodingVerifier, detectEncodingFromByteOrderMarks: true))
                {
                    while (!textReader.EndOfStream)
                    {
                        textReader.ReadLine();   // in order to increment the stream position
                    }

                    // all text parsed ok
                    return textReader.CurrentEncoding;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
