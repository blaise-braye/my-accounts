using System.IO.Abstractions;
using System.Threading.Tasks;

namespace MyAccounts.Business.Managers
{
    public interface IWorkingCopy
    {
        string Root { get; }

        string SettingsPath { get; }

        IFileSystem Fs { get; }

        Task<bool> CreateFolderIfDoesNotExistsYet(string workingCopyRoot);

        string GetAbsolutePath(params object[] relativePathChain);
    }
}