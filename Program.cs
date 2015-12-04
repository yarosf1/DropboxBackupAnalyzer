using System.Configuration;

namespace DropboxBackupAnalyzer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var worker = new Worker(ConfigurationManager.AppSettings["DropboxRootFolder"], ConfigurationManager.AppSettings["BackupFolder"]);
            worker.Run();
        }
    }
}
