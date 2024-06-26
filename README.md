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
            var transformer = new JTran.Transformer(transform);
            var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

            return transformer.Transform(source, context);
        }
    }

You can input and output directly from/to streams

    public class JTranSample2
    {
        public void Transform(string transform, Stream input, Stream output)
        {
            var transformer = new JTran.Transformer(transform);
            var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

            transformer.Transform(input, output, context);
        }
    }

You can input directly from a POCO or a list of POCOS

    public class JTranSample3
    {
        public void TransformSchool(string transform, School input, Stream output)
        {
            var transformer = new JTran.Transformer(transform);

            transformer.Transform(input, output);
        }

        public void TransformEmployees(string transform, List<Employee> input, Stream output)
        {
            var transformer = new JTran.Transformer(transform);

            transformer.Transform(input, output);
        }
    }

You can output to multiple json documents, e.g. files by using the IStreamFactory. Note that your JTran must output to an array, e.g. use "[]" in a #foreach. Each object in that array is output as a separate document.

    public class JTranSample4
    {
        public void TransformToFiles(string transform, Stream data)
        {
            var transformer = new JTran.Transformer(transform);

            // FileStreamFactory is provided by the JTran library but you can implement your own class. 
            //   See the MongoDBTests project for an example.
            var output = new FileStreamFactory((index)=> $"c:\\documents\jtran\file_{index}.json"); // Pass in a lambda to name each file

            transformer.Transform(data, output);
        }
    }

Return a POCO

    public class JTranSample5
    {
        public List<Student> Transform(string transform, Stream input)
        {
            var transformer = new JTran.Transformer(transform);
            using var output = new MemoryStream();

            transformer.Transform(input, output);

            return output.ToObject<List<Student>>();
        }
    }

#### TransformerContext

The TransformerContext provides a way of extending a default transform

    public class JTranSample
    {
        public string Transform(string transform, string source)
        {
            var transformer = new JTran.Transformer(transform);
            var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

            return transformer.Transform(source, context);
        }
    }

<br />
<small>IDictionary<string, object>?</small> <b>Arguments</b><br /><br />
This is a dictionary to pass in a set of arguments. This dictionary is called at runtime and since it's an interface you could implement a custom dictionary that returns values from a secret store or configuration store, e.g. KeyVault or Microsoft.Extensions.IConfiguration<br /><br />

<small>IDictionary<string, IDocumentRepository></small> <b>DocumentRepositories</b><br /><br />
This is a dictionary of document repositories. These are how calls to the document() function are resolved. The repo name as the first argument in that function is the key in the dictionary you provide here.

<small>bool</small> <b>AllowDeferredLoading</b><br /><br />
When the input source document is a json array (starts with "[") by default the transform will not load the json until it starts to get processed, e.g. thru a #foreach loop and will only load one item at at time. This is to allow super large json documents that would otherwise cause memory issues. However in certain cases this may cause performance issues. For instance if you jtran code is accessing items out of order, e.g. "#(@[42])". Setting this value to false will cause the entire json source to be parsed and loaded at once.

<small>IReadOnlyDictionary<string, object></small> <b>OutputArguments</b><br /><br />
This dictionary is a readonly value. It where any output variables set in the transform, e.g. "#outputvariable(Name, 'Fred')". Once the transform is complete this dictionary will be filled with output variables

<small>Action<string, object>?</small> <b>OnOutputArgument</b><br /><br />
Allows a lambda expression to passed in that will be called immediately as soon as an output variable is set in the transform.

<br>

<small>Note: The transformer would benefit from caching so it would be better to inject the transformer object as a singleton in your dependency injection code.</small>
    
<br>

<strong>[Language Reference](docs/reference.md)</strong>
