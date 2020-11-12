# JTran
   JTran is a .Net Standard Library for doing JSON to JSON transformations.

   JTran is heavily influenced by XSLT but whereas XSLT does XML to XML transformations, JTran does JSON to JSON transformations.

### Getting started

#### Installing via NuGet

    Install-Package JTran


A transform is a JSON file that contains JTran processing instructions. To transform a source JSON document you provide the source JSON and the transform:


    public class JTranSample
    {
        public string Transform(string transform, string source)
        {
            var transformer = new JTran.Transformer(transform1);
            var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

            return transformer.Transform(source, context);
        }
    }

Note: The transformer would benefit from caching so creating the transformer each time would be inefficient.
    
<br>

<strong>[Language Reference](docs/reference.md)</strong>
