using System.IO;
using System.Text;

namespace JTran.Extensions
{
    /****************************************************************************/
    /****************************************************************************/
    internal static class StreamExtensions
    {
        /****************************************************************************/
        /// <summary>
        /// Reads in from string and converts into a string
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="encoder">Text encoder. Will default to UTFEncoding</param>
        /// <returns>The resulting string</returns>
        internal static string ReadString(this Stream stream, Encoding encoder = null)
        {
            encoder = encoder ?? UTF8Encoding.UTF8;

            if(stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            try
            { 
                using(var mem = new MemoryStream())
                { 
                    stream.CopyTo(mem);

                    return encoder.GetString(mem.ToArray());
                }
            }
            finally
            { 
                if(stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);
            }
        }    
    }
}
