using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JTran.Assertions.UnitTests
{
    [TestClass]
    public class AssertTests
    {
        [TestMethod]
        public void Assert_fails()
        {
            JTran.Assert(_transformAssert, _data1);
        }

        [TestMethod]
        public void Assert_fails2()
        {
            JTran.Assert(_transformAssert2, _data1);
        }

        #region Samples
        
        private static class JTran
        {
            internal static void Assert(string transformSource, string data)
            {
                try
                { 
                    TransformerBuilder                               
                        .FromString(transformSource)
                        .Build<string>()
                        .Transform(data);
                }
                catch (AssertFailedException ex) 
                {
                    throw new Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException(ex.Message, ex);
                }
            }
        }

        private static readonly string _transformAssert =
        @"{
            '#variable(model)':     'Corvette',
            '#variable(message)':   'Model should be Corvette',

            '#assert(Model == $model)': 'Model should be Corvette'            
        }";

        private static readonly string _transformAssert2 =
        @"{
            '#variable(model)':     'Corvette',
            '#variable(message)':   'Invalid year: ',

            '#assert(Year >= 1970 and Year < 1980)':   '#($message + Year)'    
        }";
        
        private static readonly string _data1 =
        @"{
            Make:   'Chevy',
            Model:  'Silverado',
            Year:   1964,
            Color:  'Blue'
        }"; 

        #endregion
    }
}