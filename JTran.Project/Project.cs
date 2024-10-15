
using System.Collections.Generic;

namespace JTran.Project
{
    /****************************************************************************/
    /****************************************************************************/
    public class Project
    {
        public string                             Name              { get; set; } = "";
        public string                             TransformPath     { get; set; } = "";
        public string                             SourcePath        { get; set; } = "";
        public string                             DestinationPath   { get; set; } = "";
        public bool                               SplitOutput       { get; set; } = false;
        public Dictionary<string, string>         IncludePaths      { get; set; } = new();
        public Dictionary<string, string>         DocumentPaths     { get; set; } = new();
        public List<string>                       ExtensionPaths    { get; set; } = [];
        public Dictionary<string, object>?        Arguments         { get; set; }
        public List<IReadOnlyDictionary<string, object>>  ArgumentProviders { get; set; } = [];
    }    
}
