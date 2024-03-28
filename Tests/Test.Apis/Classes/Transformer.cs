using System.Reflection;
using System.Runtime.CompilerServices;

using JTran;
using MondoCore.Common;

namespace Test.Apis.Classes
{
    public interface ITransformer<T>
    {
        string Transform(string data);
    }    
    
    public class Transformer<T> : ITransformer<T>
    {
        private readonly JTran.Transformer _transformer;

        public Transformer(string transformName)
        {
            _transformer = new JTran.Transformer(LoadTransform(transformName));
        }

        public string Transform(string data)
        {
            return _transformer.Transform(data);
        }

        private string LoadTransform(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream($"Test.Apis.Transforms.{name}.jtran");

            return stream.ReadString();
        }
    }
}
