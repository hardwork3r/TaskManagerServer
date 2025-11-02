namespace TaskManager.Application.Tasks.Commands;

using TaskManager.Application.Common.Interfaces;

public record DeleteAttachmentCommand(string TaskId, string AttachmentId) : ICommand<bool>;