# JTran
   A JSON to JSON transformer with simliar functionality to XSLT.

### Getting started

A transform is a JSON file that contains JTran processing instructions. To transform a source JSON document you provide the source JSON and the transoform:

<code>

public class JTranSample
{
    public string Transform(string transform, string source)
    {
        var transformer = new JTran.Transformer(_transform1);
        var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

        return transformer.Transform(_data1, context);
    }
}

</code>


