using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CSharp_FileAnalyzer.Services
{
    public class Logger
    {
        public void LogInfo(string message)
        {
            string logText =
                "[" + DateTime.Now + "] INFO: " +
                message + "\n";

            File.AppendAllText(
                "uygulama_log.txt",
                logText
            );
        }

        public void LogError(string message)
        {
            Console.WriteLine("\nHata: " + message);

            string logText =
                "[" + DateTime.Now + "] ERROR: " +
                message + "\n";

            File.AppendAllText(
                "uygulama_log.txt",
                logText
            );
        }
    }
}
