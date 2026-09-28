using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharp_FileAnalyzer.Interfaces;

namespace CSharp_FileAnalyzer.Services
{
    public class DocxFileReader : IFileReader
    {
        public string ReadFile(string path)
        {
            StringBuilder content = new StringBuilder();

            using (var wordFile = Xceed.Words.NET.DocX.Load(path))
            {
                foreach (var paragraph in wordFile.Paragraphs)
                {
                    content.AppendLine(paragraph.Text);
                }
            }

            return content.ToString();
        }
    }
}
