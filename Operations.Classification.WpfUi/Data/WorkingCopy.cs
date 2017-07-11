using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Operations.Classification.WpfUi.Data
{
    public interface IWorkingCopy
    {
        string Root { get; }

        string SettingsPath { get; }

        string GetAccountAggregatedOperationsPath(string accountName, string extension);

        string GetAccountDirectory(string accountName);

        string GetAccountOperationsDirectory(string accountName);

        Task<bool> MakeFolderOrSkip(string workingCopyRoot);
    }

    public class WorkingCopy : IWorkingCopy
    {
        public WorkingCopy(string workingFolder)
        {
            Root = workingFolder;
            SettingsPath = GetAbsolutePath("Classifications.json");
        }

        public string Root { get; }

        public string SettingsPath { get; }

        public string GetAccountDirectory(string accountName)
        {
            return GetAbsolutePath(accountName);
        }

        public string GetAccountOperationsDirectory(string accountName)
        {
            return GetAbsolutePath(accountName, "operations");
        }

        public string GetAccountAggregatedOperationsPath(string accountName, string extension)
        {
            if (!extension.StartsWith("."))
            {
                extension = $".{extension}";
            }

            return GetAbsolutePath(accountName, $"operations{extension}");
        }

        public async Task<bool> MakeFolderOrSkip(string folder)
        {
            Directory.CreateDirectory(folder);
            while (!Directory.Exists(folder))
            {
                await Task.Yield();
            }

            return true;
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