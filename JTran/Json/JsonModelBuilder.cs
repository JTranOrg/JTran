using System;
using System.Dynamic;
using System.IO;

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Json
{   
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse a text file and convert into the JTran data object model
    /// </summary>
    internal interface IJsonModelBuilder
    {
        object AddObject(string name, object parent);
        object AddArray(string name, object parent);
        object AddText(string name, string val, object parent);
        object AddBoolean(string name, bool val, object parent);
        object AddNumber(string name, double val, object parent);
        object AddNull(string name, object parent);

        object AddObject(object? parent);
        object AddArray(object? parent);
        object AddText(string val, object parent);
        object AddBoolean(bool val, object parent);
        object AddNumber(double val, object parent);
        object AddNull(object parent);
    }
    
    /****************************************************************************/
    /****************************************************************************/
    /// <summary>
    /// Parse a text file and convert into the JTran data object model (an ExpandoObject)
    /// </summary>
    internal class JsonModelBuilder : IJsonModelBuilder
    {
        #region Properties

        /****************************************************************************/
        public object AddObject(string name, object? parent)
        {
            var newObj = new ExpandoObject();

            if(parent != null)
                if(parent is ExpandoObject ex)
                    ex.TryAdd(name, newObj);

            return newObj;
        }

        /****************************************************************************/
        public object AddArray(string name, object parent)
        {
            var newArr = new List<object>();

            if(parent is ExpandoObject ex)
                ex.TryAdd(name, newArr);

            return newArr;
        }

        /****************************************************************************/
        public object AddText(string name, string val, object parent)      
        { 
            if(parent is ExpandoObject ex)
                ex.TryAdd(name, val);

            return val; 
        }

        /****************************************************************************/
        public object AddBoolean(string name, bool val, object parent)     
        { 
            if(parent is ExpandoObject ex)
                ex.TryAdd(name, val);

            return val; 
        }
        
        /****************************************************************************/
        public object AddNumber(string name, double val, object parent)    
        { 
            if(parent is ExpandoObject ex)
                ex.TryAdd(name, val);

            return val; 
        }

        /****************************************************************************/
        public object AddNull(string name, object parent)                
        { 
            if(parent is ExpandoObject ex)
                ex.TryAdd(name, null);

            return (object)null; 
        }

        #endregion

        #region Array Items

        /****************************************************************************/
        public object AddObject(object parent)
        {
            var newObj = new ExpandoObject();

            if(parent is IList<object> list)
                list.Add(newObj);

            return newObj;
        }

        /****************************************************************************/
        public object AddArray(object parent)
        {
            var newArr = new List<object>();

            if(parent is IList<object> list)
                list.Add(newArr);

            return newArr;
        }

        /****************************************************************************/
        public object AddText(string val, object parent)      
        { 
            if(parent is IList<object> list)
                list.Add(val);

            return val; 
        }

        /****************************************************************************/
        public object AddBoolean(bool val, object parent)     
        { 
            if(parent is IList<object> list)
                list.Add(val);

            return val; 
        }
        
        /****************************************************************************/
        public object AddNumber(double val, object parent)    
        { 
            if(parent is IList<object> list)
                list.Add(val);

            return val; 
        }

        /****************************************************************************/
        public object AddNull(object parent)                
        { 
            if(parent is IList<object> list)
                list.Add(null);

            return (object)null; 
        }

        #endregion
    }
}
