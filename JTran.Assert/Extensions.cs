using System.Reflection;
using System.Text;

namespace JTran.Assertions
{
    /****************************************************************************/
    /****************************************************************************/
    public static class Extensions
    {
        /****************************************************************************/
        public static string Assert(this TransformerBuilder builder, string data, TransformerContext? context = null)
        {
            try
            {
                return builder
                         .Build<string>()
                         .Transform(data, context);
            }
            catch(TargetInvocationException ex2) 
            {
                if(ex2.InnerException?.GetType()?.Name == "AssertFailedException")
                {
                    throw ex2.InnerException;
                }

                throw;
            }
        }

        /****************************************************************************/
        public static void Assert(this TransformerBuilder builder, object data, Stream output, TransformerContext? context = null)
        {
            try
            {
                builder
                    .Build<string>()
                    .Transform(data, output, context);
            }
            catch(TargetInvocationException ex2) 
            {
                if(ex2.InnerException?.GetType()?.Name == "AssertFailedException")
                {
                    throw ex2.InnerException;
                }

                throw;
            }
        }

        /****************************************************************************/
        private static string LoadInclude(string name)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"JTran.Assertions.{name}.jtran");
                       
            if(stream == null)
                throw new FileNotFoundException(name);

            return stream!.ReadString();
        }

        /****************************************************************************/
        /// <summary>
        /// Reads in from string and converts into a string
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="encoder">Text encoder. Will default to UTFEncoding</param>
        /// <returns>The resulting string</returns>
        internal static string ReadString(this Stream stream, Encoding? encoder = null)
        {
            encoder = encoder ?? UTF8Encoding.UTF8;

            if(stream is MemoryStream memStream)
            { 
                var array  = memStream.ToArray();
                var arrLen = array.Length;
                var str    = encoder.GetString(memStream.ToArray());
                var atrLen = str.Length;

                return str;
            }

            if(stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            try
            { 
                using(var mem = new MemoryStream())
                { 
                    stream.CopyTo(mem);

                    var str = encoder.GetString(mem.ToArray());
                        str = str.Trim();

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
