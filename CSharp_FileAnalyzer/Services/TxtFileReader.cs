using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using CSharp_FileAnalyzer.Interfaces;

namespace CSharp_FileAnalyzer.Services
{
    public class TxtFileReader : IFileReader
    {
        public string ReadFile(string path)
        {
            return File.ReadAllText(path);
        }
    }
}