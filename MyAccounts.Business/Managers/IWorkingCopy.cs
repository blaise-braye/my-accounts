using System.Threading.Tasks;

namespace MyAccounts.Business.Managers
{
    public interface IWorkingCopy
    {
        string Root { get; }

        string SettingsPath { get; }

        IO.IFileSystem Fs { get; }

        Task<bool> CreateFolderIfDoesNotExistsYet(string workingCopyRoot);

        string GetAbsolutePath(params object[] relativePathChain);
    }
}