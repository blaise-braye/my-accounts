using System;
using System.Collections.Generic;
using System.IO;

namespace Operations.Classification.Tests.AutoFixtures
{
    public class FileSystemAdapter : MyAccounts.Business.IO.IFileSystem
    {
        private readonly System.IO.Abstractions.IFileSystem _fileSystem;

        public FileSystemAdapter(System.IO.Abstractions.IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public bool DirectoryExists(string path)
        {
            return _fileSystem.Directory.Exists(path);
        }

        public void DirectoryCreate(string folder)
        {
            _fileSystem.Directory.CreateDirectory(folder);
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            _fileSystem.Directory.Delete(path);
        }

        public IEnumerable<string> DirectoryGetFiles(string path, string searchPattern)
        {
            return _fileSystem.Directory.GetFiles(path, searchPattern);
        }

        public bool IsDirectoy(string path)
        {
            return (_fileSystem.File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public bool FileExists(string path)
        {
            return _fileSystem.File.Exists(path);
        }

        public Stream FileOpenRead(string path)
        {
            return _fileSystem.File.OpenRead(path);
        }

        public Stream FileCreate(string path)
        {
            return _fileSystem.File.Create(path);
        }

        public void FileDelete(string path)
        {
            _fileSystem.File.Delete(path);
        }

        public DateTime FileGetCreationTime(string path)
        {
            return _fileSystem.File.GetCreationTime(path);
        }
    }
}
