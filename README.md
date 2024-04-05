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

<small>Note: The transformer would benefit from caching so it would better to inject a singleton in your dependency injection code.</small>
    
<br>

<strong>[Language Reference](docs/reference.md)</strong>
