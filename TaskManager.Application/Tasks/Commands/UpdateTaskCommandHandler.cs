namespace TaskManager.Application.Tasks.Commands;

using Microsoft.Extensions.Logging;
using TaskManager.Application.Common.Interfaces;
using TaskManager.Contracts.Tasks;
using TaskManager.Contracts.Users;
using TaskManager.Domain.Interfaces;

public class UpdateTaskCommandHandler : ICommandHandler<UpdateTaskCommand, TaskWithUsersResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateTaskCommandHandler> _logger;

    public UpdateTaskCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ILogger<UpdateTaskCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TaskWithUsersResponse> Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Updating task {TaskId} by user {UserId}",
            request.TaskId, _currentUserService.UserId);

        if (string.IsNullOrEmpty(_currentUserService.UserId))
        {
            _logger.LogWarning("Update task failed: User not authenticated");
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId, ct);
        if (task == null)
        {
            _logger.LogWarning("Update task failed: Task {TaskId} not found", request.TaskId);
            throw new KeyNotFoundException("Task not found");
        }

        var isOwner = task.UserId == _currentUserService.UserId;
        var isAssigned = task.AssignedUserIds.Contains(_currentUserService.UserId);
        var isAdmin = _currentUserService.IsAdmin;

        _logger.LogDebug("Update task permissions - Owner: {IsOwner}, Assigned: {IsAssigned}, Admin: {IsAdmin}",
            isOwner, isAssigned, isAdmin);

        if (!isAdmin && !isOwner && !isAssigned)
        {
            _logger.LogWarning("Update task failed: User {UserId} has no access to task {TaskId}",
                _currentUserService.UserId, request.TaskId);
            throw new UnauthorizedAccessException("Access denied");
        }

        if (isAdmin || isOwner)
        {
            if (!string.IsNullOrEmpty(request.Title)) task.Title = request.Title;
            if (request.Description != null) task.Description = request.Description;
            if (!string.IsNullOrEmpty(request.Status)) task.Status = request.Status;
            if (!string.IsNullOrEmpty(request.Priority)) task.Priority = request.Priority;
            if (request.DueDate.HasValue) task.DueDate = request.DueDate;
            if (request.Tags != null) task.Tags = request.Tags;
            if (request.AssignedUserIds != null) task.AssignedUserIds = request.AssignedUserIds;

            _logger.LogInformation("Task {TaskId} updated by {UserType} {UserId}",
                request.TaskId, isAdmin ? "admin" : "owner", _currentUserService.UserId);
        }
        else if (isAssigned && !string.IsNullOrEmpty(request.Status))
        {
            task.Status = request.Status;
            _logger.LogInformation("Task {TaskId} status updated to '{Status}' by assigned user {UserId}",
                request.TaskId, task.Status, _currentUserService.UserId);
        }

        await _unitOfWork.Tasks.UpdateAsync(request.TaskId, task, ct);

        _logger.LogInformation("Task {TaskId} updated successfully", request.TaskId);

        var assignedUsers = new List<UserDto>();
        foreach (var userId in task.AssignedUserIds)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, ct);
            if (user != null)
                assignedUsers.Add(new UserDto(user.Id, user.Name));
        }

        var attachments = task.Attachments.Select(a => new AttachmentResponse(
            a.Id, a.FileName, a.FileSize, a.ContentType, a.GridFsId, a.UploadedAt
        )).ToList();

        return new TaskWithUsersResponse(
            task.Id, task.Title, task.Description, task.Status, task.Priority,
            task.DueDate, task.Tags, task.UserId, assignedUsers, attachments, task.CreatedAt
        );
    }
}