using System;
using System.Collections.Generic;
using System.IO;

namespace JTran.Project
{
    /****************************************************************************/
    /****************************************************************************/
    public class FileDocumentRepository : IDocumentRepository2
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

        /****************************************************************************/
        public Stream GetDocumentStream(string name)
        {
            var fullPath = Path.Combine(_path, name + ".json");

            return File.OpenRead(fullPath);
        }
    }
}
