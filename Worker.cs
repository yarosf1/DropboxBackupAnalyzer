using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DropboxBackupAnalyzer
{
    internal class Worker
    {
        private readonly string _rootFolder;
        private readonly string _backupFolder;

        private readonly List<GenericFileInfo> _storageFiles = new List<GenericFileInfo>();
        private readonly List<GenericFileInfo> _backupFiles = new List<GenericFileInfo>();

        private int _totalMissingFiles;

        internal Worker(string rootFolder, string backupFolder)
        {
            _rootFolder = rootFolder;
            _backupFolder = backupFolder;
        }

        internal void Run()
        {
            Log("Collecting backup files...");
            CollectBackupFiles();

            Log("Collecting storage files...");
            CollectStorageFiles();

            Log("Analyzing...");
            Analyze();

            Log("Done. Press [Enter] to exit");
            Console.ReadLine();
        }

        private void CollectBackupFiles()
        {
            string[] files = Directory.GetFiles(_backupFolder);
            foreach (string path in files)
            {
                // Visa_Norway_2008_12 (deleted c5c703c15f13620fc90e1a52d5d0f6a0).docx
                string filename = Path.GetFileName(path);

                _backupFiles.Add(new GenericFileInfo
                {
                    OriginalName = filename,
                    FormattedName = Regex.Replace(filename, @" \(deleted [a-f0-9]{32}\)", string.Empty, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
                    FullPath = path,
                    FileSize = new FileInfo(path).Length
                });
            }
        }

        private void CollectStorageFiles()
        {
            CollectFilesInDirectory(_rootFolder);
        }

        private void CollectFilesInDirectory(string parent)
        {
            string[] files = Directory.GetFiles(parent);
            foreach (string path in files)
            {
                _storageFiles.Add(new GenericFileInfo
                {
                    OriginalName = Path.GetFileName(path),
                    FullPath = path,
                    FileSize = new FileInfo(path).Length
                });
            }

            string[] folders = Directory.GetDirectories(parent);
            foreach (string folderPath in folders)
            {
                CollectFilesInDirectory(folderPath);
            }
        }

        private void Analyze()
        {
            foreach (GenericFileInfo backupFile in _backupFiles)
            {
                if (!_storageFiles.Any(storageFile =>
                        storageFile.OriginalName.Equals(backupFile.FormattedName, StringComparison.InvariantCultureIgnoreCase)
                        && storageFile.FileSize == backupFile.FileSize
                    ))
                {
                    _totalMissingFiles++;
                    Log(backupFile.OriginalName);
                }
            }

            Log(string.Empty);
            Log($"Total files: {_totalMissingFiles}");
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
