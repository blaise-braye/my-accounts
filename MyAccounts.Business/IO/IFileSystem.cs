using System;
using System.Collections.Generic;
using System.IO;

namespace MyAccounts.Business.IO
{
    public interface IFileSystem
    {
        bool DirectoryExists(string path);

        void DirectoryCreate(string folder);

        void DirectoryDelete(string path, bool recursive);

        IEnumerable<string> DirectoryGetFiles(string path, string searchPattern);

        bool IsDirectoy(string path);

        bool FileExists(string path);

        Stream FileOpenRead(string path);

        Stream FileCreate(string path);

        void FileDelete(string path);

        DateTime FileGetCreationTime(string path);
    }

    public class FileSystem : IFileSystem
    {
        public bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public void DirectoryCreate(string folder)
        {
            Directory.CreateDirectory(folder);
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }

        public IEnumerable<string> DirectoryGetFiles(string path, string searchPattern)
        {
            return Directory.GetFiles(path, searchPattern);
        }

        public bool IsDirectoy(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;
        }

        public Stream FileOpenRead(string path)
        {
            return File.OpenRead(path);
        }

        public Stream FileCreate(string path)
        {
            return File.Create(path);
        }

        public void FileDelete(string path)
        {
            File.Delete(path);
        }

        public bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public DateTime FileGetCreationTime(string path)
        {
            return File.GetCreationTime(path);
        }
    }
}
