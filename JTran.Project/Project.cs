
using System.Collections.Generic;

namespace JTran.Project
{
    /****************************************************************************/
    /****************************************************************************/
    public class Project
    {
        public string                     Name            { get; set; } = "";
        public string                     TransformPath   { get; set; } = "";
        public string                     SourcePath      { get; set; } = "";
        public string                     DestinationPath { get; set; } = "";
        public Dictionary<string, string> IncludePaths    { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> DocumentPaths   { get; set; } = new Dictionary<string, string>();
        public List<string>               ExtensionPaths  { get; set; } = new List<string>();
    }    
}
