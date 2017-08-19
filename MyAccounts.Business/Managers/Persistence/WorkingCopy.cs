using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace MyAccounts.Business.Managers.Persistence
{
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

        public string GetAbsolutePath(params object[] relativePathChain)
        {
            var rootedPath = relativePathChain.Aggregate(
                Root,
                (rootedPathStep, relativePath) =>
                {
                    var rawRelativePath = relativePath.ToString().Replace('\\', '/');
                    if (!rawRelativePath.StartsWith("./"))
                    {
                        rawRelativePath = @"./" + relativePath;
                    }

                    return Path.Combine(rootedPathStep, rawRelativePath);
                });
            var absoluteRootedPath = Path.GetFullPath(rootedPath);
            return absoluteRootedPath;
        }
    }
}