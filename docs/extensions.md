# JTran
   JTran is a .Net Standard Library for doing JSON to JSON transformations.

  <br>

## Extension Functions
   Extension functions are functions written in code (.Net) that add to the built-in functions available in transform expressions.

### Setup

You can provide extensions in two ways by passing in an IEnumerable to the JTran.Transformer constructor:

To provide your extension functions as an instantiated object where the functions are non-static pass the object with the functions:

     public class JTranSample
     {
         public string Transform(string transform, string source)
         {
             var transformer = new JTran.Transformer(transform1, new object[] { new MyExtensionFunctions() } );
             var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

            return transformer.Transform(source, context);
        }
     }


If your extension functions are in a static class and/or the functions themselves are static you can pass the type of class:

    public class JTranSample
    {
        public string Transform(string transform, string source)
        {
            var transformer = new JTran.Transformer(transform1, new object[] { typeof(MyStaticExtensionFunctions) } );
            var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

            return transformer.Transform(source, context);
        }
    }

Since you can pass in an IEnumerable you can pass in more than object and you can mix and match the method:

    public class JTranSample
    {
        public string Transform(string transform, string source)
        {
            var transformer = new JTran.Transformer(transform1, new object[] {  new MyExtensionFunctions(), typeof(MyStaticExtensionFunctions) } );
            var context     = new TransformContext { Arguments = new Dictionary<string, object>() };

            return transformer.Transform(source, context);
        }
    }

### Parameters

JSON really has only 5 types:

1. String
2. Number
3. Boolean
4. Object
5. Array

JTran will attempt to convert any value passed as a parameter to your function if the types do not match. If JTran cannot convert the value (e.g. converting "bob" to an int) then you will get runtime exceptions.

1. Numbers in JTran are internally represented by either a Double or a long (Int64). JTran will attempt to convert from one of those types to the type of the parameter. e.g. from a long to an int or from a decimal to a double. JTran uses the .Net function Convert.ChangeType to do this. If that function fails then its exception will be propagated up.
2. JTran uses the same function to convert to bool.
3. Objects in JSON are simply properties that have child properties. Internally your source document is converted into an ExpandoObject. If your function takes a class as a parameter JTran will use json serialization/deserialization to convert the ExpandoObject into your class.

