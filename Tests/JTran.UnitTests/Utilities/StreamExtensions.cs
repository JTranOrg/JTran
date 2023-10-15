
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JTran.UnitTests
{
    /****************************************************************************/
    /****************************************************************************/
    public static class StreamExtensions
    {
        /****************************************************************************/
        /// <summary>
        /// Reads in from string and converts into a string
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="encoder">Text encoder. Will default to UTFEncoding</param>
        /// <returns>The resulting string</returns>
        public static async Task<string> ReadStringAsync(this Stream stream, Encoding encoder = null)
        {
            encoder = encoder ?? UTF8Encoding.UTF8;

            if(stream is MemoryStream memStream)
            { 
                var array = memStream.ToArray();
                var arrLen = array.Length;
                var str = encoder.GetString(memStream.ToArray()).SubstringBefore("\0");
                var atrLen = str.Length;

                return str;
            }

            if(stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            try
            { 
                using(var mem = new MemoryStream())
                { 
                    await stream.CopyToAsync(mem).ConfigureAwait(false);

                    var str = encoder.GetString(mem.ToArray());
                        str = str[..str.IndexOf("\0")];

                        return str;
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
