using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp_FileAnalyzer.Interfaces
{
    internal interface IFileReader
    {
        string ReadFile(string path);
    }
}
