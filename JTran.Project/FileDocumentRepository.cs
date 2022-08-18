using System;
using System.Collections.Generic;
using System.IO;

namespace JTran.Project
{
    /****************************************************************************/
    /****************************************************************************/
    public class FileDocumentRepository : IDocumentRepository
    {
        private readonly string _path;

        /****************************************************************************/
        public FileDocumentRepository(string path)
        {
            _path = path;
        }

        /****************************************************************************/
        public string GetDocument(string name)
        {
            var fullPath = Path.Combine(_path, name + ".json");

            return File.ReadAllText(fullPath);
        }
    }
}
