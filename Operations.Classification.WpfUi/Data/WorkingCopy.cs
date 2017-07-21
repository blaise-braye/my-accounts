using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Operations.Classification.WpfUi.Data
{
    public interface IWorkingCopy
    {
        string Root { get; }

        string SettingsPath { get; }

        IFileSystem Fs { get; }

        string GetAccountOperationsDirectory(string accountName);

        Task<bool> CreateFolderIfDoesNotExistsYet(string workingCopyRoot);
    }

    public class WorkingCopy : IWorkingCopy
    {
        public WorkingCopy(IFileSystem fileSystem, string workingFolder)
        {
            Fs = fileSystem;
            Root = workingFolder;
            SettingsPath = GetAbsolutePath("Classifications.json");
        }

        public IFileSystem Fs { get; }

        public string Root { get; }

        public string SettingsPath { get; }

        public async Task<bool> CreateFolderIfDoesNotExistsYet(string folder)
        {
            if (!Fs.Directory.Exists(folder))
            {
                Fs.Directory.CreateDirectory(folder);
                while (!Fs.Directory.Exists(folder))
                {
                    await Task.Yield();
                }
            }

            return true;
        }

        public string GetAccountOperationsDirectory(string accountName)
        {
            return GetAbsolutePath(accountName, "operations");
        }

        private string GetAbsolutePath(params string[] relativePathChain)
        {
            var rootedPath = relativePathChain.Aggregate(
                Root,
                (rootedPathStep, relativePath) =>
                {
                    if (!relativePath.StartsWith("./"))
                    {
                        relativePath = "./" + relativePath;
                    }

                    return Path.Combine(rootedPathStep, relativePath);
                });
            var absoluteRootedPath = Path.GetFullPath(rootedPath);
            return absoluteRootedPath;
        }
    }
}