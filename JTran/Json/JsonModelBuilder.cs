
using System.Runtime.CompilerServices;
using System.Collections.Generic;

[assembly: InternalsVisibleTo("JTran.UnitTests")]
[assembly: InternalsVisibleTo("JTran.PerformanceTests")]

namespace JTran.Json
{   
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Create an object model (JsonObject or jtran)
    /// </summary>
    internal interface IJsonModelBuilder
    {
        object AddObject(string name, object parent, object? previous, long lineNumber);
        object AddArray(string name, object parent, long lineNumber);
        object AddText(string name, string val, object parent, object? previous, long lineNumber);
        object AddBoolean(string name, bool val, object parent, object? previous, long lineNumber);
        object AddNumber(string name, double val, object parent, object? previous, long lineNumber);
        object AddNull(string name, object parent, object? previous, long lineNumber);

        object AddObject(object? parent, long lineNumber);
        object AddArray(object? parent, long lineNumber);
        object AddText(string val, object parent, long lineNumber);
        object AddBoolean(bool val, object parent, long lineNumber);
        object AddNumber(double val, object parent, long lineNumber);
        object AddNull(object parent, long lineNumber);
    }
    
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Create a JTran data object model (an JsonObject)
    /// </summary>
    internal class JsonModelBuilder : IJsonModelBuilder
    {
        #region Properties

        /****************************************************************************/
        public object AddObject(string name, object? parent, object? _, long lineNumber)
        {
            var newObj = new JsonObject();

            if(parent != null)
                if(parent is JsonObject ex)
                    ex.TryAdd(name, newObj);

            return newObj;
        }

        /****************************************************************************/
        public object AddArray(string name, object parent, long lineNumber)
        {
            var newArr = new List<object>();

            if(parent is JsonObject ex)
                ex.TryAdd(name, newArr);

            return newArr;
        }

        /****************************************************************************/
        public object AddText(string name, string val, object parent, object? previous, long lineNumber)      
        { 
            if(parent is JsonObject ex)
                ex.TryAdd(name, val);

            return val; 
        }

        /****************************************************************************/
        public object AddBoolean(string name, bool val, object parent, object? previous, long lineNumber)     
        { 
            if(parent is JsonObject ex)
                ex.TryAdd(name, val);

            return val; 
        }
        
        /****************************************************************************/
        public object AddNumber(string name, double val, object parent, object? previous, long lineNumber)    
        { 
            if(parent is JsonObject ex)
                ex.TryAdd(name, val);

            return val; 
        }

        /****************************************************************************/
        public object AddNull(string name, object parent, object? previous, long lineNumber)                
        { 
            if(parent is JsonObject ex)
                ex.TryAdd(name, null);

            return (object)null; 
        }

        #endregion

        #region Array Items

        /****************************************************************************/
        public object AddObject(object? parent, long lineNumber)
        {
            var newObj = new JsonObject();

            if(parent is IList<object> list)
                list.Add(newObj);

            return newObj;
        }

        /****************************************************************************/
        public object AddArray(object? parent, long lineNumber)
        {
            var newArr = new List<object>();

            if(parent is IList<object> list)
                list.Add(newArr);

            return newArr;
        }

        /****************************************************************************/
        public object AddText(string val, object parent, long lineNumber)      
        { 
            if(parent is IList<object> list)
                list.Add(val);

            return val; 
        }

        /****************************************************************************/
        public object AddBoolean(bool val, object parent, long lineNumber)     
        { 
            if(parent is IList<object> list)
                list.Add(val);

            return val; 
        }
        
        /****************************************************************************/
        public object AddNumber(double val, object parent, long lineNumber)    
        { 
            if(parent is IList<object> list)
                list.Add(val);

            return val; 
        }

        /****************************************************************************/
        public object AddNull(object parent, long lineNumber)                
        { 
            if(parent is IList<object> list)
                list.Add(null);

            return (object)null; 
        }

        #endregion
    }
}
