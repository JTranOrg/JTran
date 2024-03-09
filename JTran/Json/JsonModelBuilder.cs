
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using JTran.Common;
using System.Xml.Linq;

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
        object AddObject(ICharacterSpan name, object parent, object? previous, long lineNumber);
        object AddArray(ICharacterSpan name, object parent, long lineNumber);
        object AddText(ICharacterSpan name, ICharacterSpan val, object parent, object? previous, long lineNumber);
        object AddBoolean(ICharacterSpan name, bool val, object parent, object? previous, long lineNumber);
        object AddNumber(ICharacterSpan name, decimal val, object parent, object? previous, long lineNumber);
        object AddNull(ICharacterSpan name, object parent, object? previous, long lineNumber);
               
        object AddObject(object? parent, long lineNumber);
        object AddArray(object? parent, long lineNumber);
        object AddText(ICharacterSpan val, object parent, long lineNumber);
        object AddBoolean(bool val, object parent, long lineNumber);
        object AddNumber(decimal val, object parent, long lineNumber);
        object AddNull(object parent, long lineNumber);
    }
    
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Create a JTran data object model (a JsonObject)
    /// </summary>
    internal class JsonModelBuilder : IJsonModelBuilder
    {
        #region Properties

        /****************************************************************************/
        public object AddObject(ICharacterSpan name, object? parent, object? _, long lineNumber)
        {
            var newObj = new JsonObject(parent, name);

            if(parent != null)
            { 
                if(parent is JsonObject pobj)
                    pobj.TryAdd(name, newObj);
            }

            return newObj;
        }

        /****************************************************************************/
        public object AddArray(ICharacterSpan name, object parent, long lineNumber)
        {
            var newArr = new JsonArray(parent);

            if(parent is JsonObject ex)
                ex.TryAdd(name, newArr);

            return newArr;
        }

        /****************************************************************************/
        public object AddText(ICharacterSpan name, ICharacterSpan val, object parent, object? previous, long lineNumber)      
        { 
            if(parent is JsonObject ex)
                ex.TryAdd(name, val);

            return val; 
        }

        /****************************************************************************/
        public object AddBoolean(ICharacterSpan name, bool val, object parent, object? previous, long lineNumber)     
        { 
            if(parent is JsonObject ex)
                ex.TryAdd(name, val);

            return val; 
        }
        
        /****************************************************************************/
        public object AddNumber(ICharacterSpan name, decimal val, object parent, object? previous, long lineNumber)    
        { 
            if(parent is JsonObject ex)
                ex.TryAdd(name, val);

            return val; 
        }

        /****************************************************************************/
        public object AddNull(ICharacterSpan name, object parent, object? previous, long lineNumber)                
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
            var newObj = new JsonObject(parent);

            if(parent is IList<object> list)
                list.Add(newObj);

            return newObj;
        }

        /****************************************************************************/
        public object AddArray(object? parent, long lineNumber)
        {
            var newArr = new JsonArray(parent);

            if(parent is IList<object> list)
                list.Add(newArr);

            return newArr;
        }

        /****************************************************************************/
        public object AddText(ICharacterSpan val, object parent, long lineNumber)      
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
        public object AddNumber(decimal val, object parent, long lineNumber)    
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
