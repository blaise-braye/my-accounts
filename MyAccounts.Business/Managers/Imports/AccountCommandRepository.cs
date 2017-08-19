using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace MyAccounts.Business.Managers.Imports
{
    public class AccountCommandRepository : IAccountCommandRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccountCommandRepository));

        private readonly IWorkingCopy _workingCopy;

        public AccountCommandRepository(IWorkingCopy workingCopy)
        {
            _workingCopy = workingCopy;
        }

        private IFileSystem Fs => _workingCopy.Fs;

        public async Task<bool> Add(ImportCommand importCommand, Stream attachment)
        {
            var accountId = importCommand.AccountId;
            await EnsureRefLogStructureIsClean(accountId);

            var importCommands = await GetAll(accountId);

            if (importCommands.Any(i => i.Id == importCommand.Id))
            {
                return false;
            }

            importCommands.Add(importCommand);
            await PersistReflog(accountId, importCommands);

            string attachmentsPath = GetCommandAttachmentFilePath(importCommand);

            using (var sTarget = Fs.File.Create(attachmentsPath))
            {
                await attachment.CopyToAsync(sTarget);
            }

            return true;
        }

        public async Task<bool> Replace(ImportCommand importCommand)
        {
            var accountId = importCommand.AccountId;
            await EnsureRefLogStructureIsClean(accountId);

            var importCommands = await GetAll(accountId);

            var position = importCommands.FindIndex(i => i.Id == importCommand.Id);

            if (position < 0)
            {
                return false;
            }

            importCommands.RemoveAt(position);
            importCommands.Insert(position, importCommand);

            await PersistReflog(accountId, importCommands);

            return true;
        }

        public async Task<bool> Delete(Guid accountId, Guid commandId)
        {
            var importCommands = await GetAll(accountId);
            var position = importCommands.FindIndex(i => i.Id == commandId);

            if (position >= 0)
            {
                importCommands.RemoveAt(position);
                await PersistReflog(accountId, importCommands);

                string attachmentsPath = GetCommandAttachmentFilePath(accountId, commandId);
                if (Fs.File.Exists(attachmentsPath))
                {
                    Fs.File.Delete(attachmentsPath);
                }

                return true;
            }

            return false;
        }

        public async Task<List<ImportCommand>> GetAll(Guid accountId)
        {
            await EnsureRefLogStructureIsClean(accountId);
            string reflogFile = GetCommandsReflogFilePath(accountId);
            List<ImportCommand> commands;
            Stream stream = null;
            try
            {
                stream = Fs.File.OpenRead(reflogFile);
                using (var sr = new StreamReader(stream))
                {
                    stream = null;
                    var jsonCommands = await sr.ReadToEndAsync();
                    commands = JsonConvert.DeserializeObject<List<ImportCommand>>(jsonCommands);
                }
            }
            catch (Exception exn)
            {
                _logger.Error($"failed to deserialize reflog commands from file {reflogFile}", exn);
                commands = new List<ImportCommand>();
                stream?.Dispose();
            }

            return commands;
        }

        public async Task<Stream> OpenAttachment(ImportCommand importCommand)
        {
            string attachmentsPath = GetCommandAttachmentFilePath(importCommand);
            await EnsureRefLogStructureIsClean(importCommand.AccountId);
            return Fs.File.OpenRead(attachmentsPath);
        }

        public async Task<bool> AddExecutionImpact(Guid accountId, ImportExecutionImpact executionImpact)
        {
            await EnsureRefLogStructureIsClean(accountId);

            var importCommands = await GetExecutionImpacts(accountId, executionImpact.CommandId);

            if (importCommands.Any(i => i.Id == executionImpact.Id))
            {
                return false;
            }

            importCommands.Add(executionImpact);
            await PersistCommandExec(accountId, executionImpact.CommandId, importCommands);

            return true;
        }

        public async Task<List<ImportExecutionImpact>> GetExecutionImpacts(Guid accountId, Guid commandId)
        {
            await EnsureRefLogStructureIsClean(accountId, commandId);
            string execFilePath = GetImportCommandExecutionsFilePath(accountId, commandId);
            var jsonCommands = Fs.File.ReadAllText(execFilePath);
            var result = JsonConvert.DeserializeObject<List<ImportExecutionImpact>>(jsonCommands);
            return result;
        }

        private async Task PersistReflog(Guid accountId, List<ImportCommand> importCommands)
        {
            string reflogFile = GetCommandsReflogFilePath(accountId);
            Stream stream = null;
            try
            {
                stream = Fs.File.Create(reflogFile);
                using (var sw = new StreamWriter(stream))
                {
                    stream = null;
                    var jsonImportCommands = JsonConvert.SerializeObject(importCommands, Formatting.Indented);
                    await sw.WriteAsync(jsonImportCommands);
                }
            }
            catch (Exception exn)
            {
                _logger.Error($"Failed to write to reflog {reflogFile}", exn);
                stream?.Dispose();
            }
        }

        private async Task PersistCommandExec(Guid accountId, Guid commandId, List<ImportExecutionImpact> importExecutionImpacts)
        {
            string executionsFilePath = GetImportCommandExecutionsFilePath(accountId, commandId);
            Stream stream = null;
            try
            {
                stream = Fs.File.Create(executionsFilePath);
                using (var sw = new StreamWriter(stream))
                {
                    stream = null;
                    var jsonImportCommands = JsonConvert.SerializeObject(importExecutionImpacts, Formatting.Indented);
                    await sw.WriteAsync(jsonImportCommands);
                }
            }
            catch (Exception exn)
            {
                _logger.Error($"Failed to write command execution {executionsFilePath}", exn);
                stream?.Dispose();
            }
        }

        private async Task EnsureRefLogStructureIsClean(Guid accountId, Guid commandId)
        {
            await EnsureRefLogStructureIsClean(accountId);

            var execDirectory = GetCommandExecutionDirectory(accountId);
            await _workingCopy.CreateFolderIfDoesNotExistsYet(execDirectory);

            string execFilePath = GetImportCommandExecutionsFilePath(accountId, commandId);
            if (!Fs.File.Exists(execFilePath))
            {
                await PersistCommandExec(accountId, commandId, new List<ImportExecutionImpact>());
            }
        }

        private async Task EnsureRefLogStructureIsClean(Guid accountId)
        {
            var reflogDirectory = GetCommandsDirectory(accountId);
            await _workingCopy.CreateFolderIfDoesNotExistsYet(reflogDirectory);
            var refLogAttachmentDirectory = GetAccountRefLogAttachmentsDirectory(accountId);
            await _workingCopy.CreateFolderIfDoesNotExistsYet(refLogAttachmentDirectory);

            string reflogFile = GetCommandsReflogFilePath(accountId);
            if (!Fs.File.Exists(reflogFile))
            {
                await PersistReflog(accountId, new List<ImportCommand>());
            }
        }

        private string GetCommandsDirectory(Guid accountId)
        {
            return _workingCopy.GetAbsolutePath(accountId, "commands_queue");
        }

        private string GetAccountRefLogAttachmentsDirectory(Guid accountId)
        {
            return Path.Combine(GetCommandsDirectory(accountId), "reflog/attachments");
        }

        private string GetCommandAttachmentFilePath(ImportCommand command)
        {
            return GetCommandAttachmentFilePath(command.AccountId, command.Id);
        }

        private string GetCommandAttachmentFilePath(Guid accountId, Guid commandId)
        {
            return Path.Combine(GetAccountRefLogAttachmentsDirectory(accountId), commandId.ToString());
        }

        private string GetCommandsReflogFilePath(Guid accountId)
        {
            return Path.Combine(GetCommandsDirectory(accountId), "reflog.json");
        }

        private string GetCommandExecutionDirectory(Guid accountId)
        {
            return _workingCopy.GetAbsolutePath(accountId, "commands_executions");
        }

        private string GetImportCommandExecutionsFilePath(Guid accountId, Guid commandId)
        {
            return Path.Combine(GetCommandExecutionDirectory(accountId), commandId + ".json");
        }
    }
}