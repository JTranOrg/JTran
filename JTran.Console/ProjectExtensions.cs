using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("JTran.Console.UnitTests")]

namespace JTran.Console
{
    internal static class ProjectExtensions
    {   
        internal static void AddArguments(this JTran.Project.Project project, string parmStr)
        {
            var arguments = new Dictionary<string, object>();

            var sb        = new StringBuilder();
            var inQuotes  = false;
            var inDoubleQuotes  = false;
            var start     = true;
            var inName    = false;
            var inValue   = false;
            var paramName = "";

            foreach(var ch in parmStr)
            {
                if(start)
                { 
                    if(ch == ' ')
                        continue;

                    if(ch != '-')
                        throw new ArgumentException("Parameter name must start with hypen (-)");

                    start = false;
                    inName = true;
                }
                else if(inName)
                {
                    if(ch == ' ')
                    { 
                        paramName = sb.ToString();
                        inName = false;
                        inValue = true;
                        sb.Clear();
                    }
                    else
                        sb.Append(ch);
                }
                else if(inValue)
                {
                    if(inValue && !inQuotes && ch == '\'') 
                    {
                        inQuotes = true;
                    }
                    else if(inValue && !inDoubleQuotes && ch == '"') 
                    {
                        inDoubleQuotes = true;
                    }
                    else if(inValue && inQuotes && ch == '\'') 
                    {
                        arguments[paramName] = sb.ToString();

                        start = true;
                        inValue = false;
                        sb.Clear();
                    }
                    else if(inValue && inDoubleQuotes && ch == '"') 
                    {
                        arguments[paramName] = sb.ToString();

                        start = true;
                        inValue = false;
                        sb.Clear();
                    }
                    else if(ch == ' ' && !inQuotes && !inDoubleQuotes) 
                    {
                        arguments[paramName] = sb.ToString();

                        start = true;
                        inValue = false;
                        sb.Clear();
                    }
                    else 
                        sb.Append(ch);
                }
            }

            // Last param
            if(inValue && sb.Length > 0)
                arguments[paramName] = sb.ToString();

            if(arguments.Count > 0)
                project.ArgumentProviders.Insert(0, arguments);

            return;
        }
    }
}
