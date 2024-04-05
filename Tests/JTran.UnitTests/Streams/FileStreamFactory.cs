using Microsoft.VisualStudio.TestTools.UnitTesting;

using JTran.Streams;
using System.Text;

namespace JTran.UnitTests
{
    [TestClass]
    [TestCategory("FileStreamFactory")]
    public class FileStreamFactoryTests
    {
        [TestMethod]
        public async Task FileStreamFactory_success()
        {
            await using var factory = new FileStreamFactory((index)=> $"c:\\Documents\\Testing\\JTran\\FileStreamFactory\\test1_{index}.json");
                
            for(var i = 1; i <= 100; i++) 
            { 
                var stream = factory.BeginStream(i);
                var bytes = UTF8Encoding.Default.GetBytes($"This is file {i}");

                await stream.WriteAsync(bytes, 0, bytes.Length);

                #if DEBUG
                factory.EndStream(stream, i);
                #else
                stream.Dispose();
                #endif
            }            
        }
    }
}
