using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace Operations.Classification.WorkingCopyStorage
{
    public interface IAccountCommandQueue
    {
        Task<List<ImportCommand>> GetAll(Guid accountId);

        Task<bool> Enqueue(ImportCommand importCommand, Stream attachment);

        Task<bool> Replace(ImportCommand importCommand);

        Task<Stream> OpenAttachment(ImportCommand importCommand);
    }

    public class AccountCommandQueue : IAccountCommandQueue
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccountCommandQueue));

        private readonly IWorkingCopy _workingCopy;

        public AccountCommandQueue(IWorkingCopy workingCopy)
        {
            _workingCopy = workingCopy;
        }

        private IFileSystem Fs => _workingCopy.Fs;

        public async Task<bool> Enqueue(ImportCommand importCommand, Stream attachment)
        {
            var accountId = importCommand.AccountId;
            await EnsureRefLogStructureIsClean(accountId);

            var importCommands = await GetAll(accountId);

            if (importCommands.Any(i => i.Id == importCommand.Id))
            {
                return false;
            }

            string reflogFile = GetCommandsReflogFilePath(accountId);
            importCommands.Add(importCommand);

            Stream stream = null;
            try
            {
                stream = Fs.File.OpenWrite(reflogFile);
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

            string attachmentsPath = GetCommandAttachmentFilePath(importCommand);

            using (var sTarget = Fs.File.OpenWrite(attachmentsPath))
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

            string reflogFile = GetCommandsReflogFilePath(accountId);
            importCommands.Add(importCommand);
            var jsonImportCommands = JsonConvert.SerializeObject(importCommands, Formatting.Indented);
            Fs.File.WriteAllText(reflogFile, jsonImportCommands);

            return true;
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

        private async Task EnsureRefLogStructureIsClean(Guid accountId)
        {
            var reflogDirectory = GetCommandsDirectory(accountId);
            await _workingCopy.CreateFolderIfDoesNotExistsYet(reflogDirectory);
            var refLogAttachmentDirectory = GetAccountRefLogAttachmentsDirectory(accountId);
            await _workingCopy.CreateFolderIfDoesNotExistsYet(refLogAttachmentDirectory);

            string reflogFile = GetCommandsReflogFilePath(accountId);
            if (!Fs.File.Exists(reflogFile))
            {
                var commands = new List<ImportCommand>();
                var jsonCommands = JsonConvert.SerializeObject(commands);
                Fs.File.WriteAllText(reflogFile, jsonCommands);
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

        private string GetCommandAttachmentFilePath(ImportCommand importCommand)
        {
            return Path.Combine(GetAccountRefLogAttachmentsDirectory(importCommand.AccountId), importCommand.Id.ToString());
        }

        private string GetCommandsReflogFilePath(Guid accountId)
        {
            return Path.Combine(GetCommandsDirectory(accountId), "reflog.json");
        }
    }
}