namespace TaskManager.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Tasks.Commands;
using TaskManager.Application.Tasks.Queries;
using TaskManager.Contracts.Tasks;

[ApiController]
[Route("api/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TaskWithUsersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TaskWithUsersResponse>>> GetTasks(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? tag,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = new GetTasksQuery(status, priority, tag, search);
        var response = await _mediator.Send(query, ct);
        return Ok(response);
    }

    [HttpGet("{taskId}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetTask(string taskId, CancellationToken ct)
    {
        var query = new GetTaskByIdQuery(taskId);
        var response = await _mediator.Send(query, ct);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskWithUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskWithUsersResponse>> CreateTask(
        [FromBody] CreateTaskRequest request,
        CancellationToken ct)
    {
        var command = new CreateTaskCommand(
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate,
            request.Tags,
            request.AssignedUserIds
        );

        var response = await _mediator.Send(command, ct);
        return Ok(response);
    }

    [HttpPut("{taskId}")]
    [ProducesResponseType(typeof(TaskWithUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskWithUsersResponse>> UpdateTask(
        string taskId,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct)
    {
        var command = new UpdateTaskCommand(
            taskId,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate,
            request.Tags,
            request.AssignedUserIds
        );

        var response = await _mediator.Send(command, ct);
        return Ok(response);
    }

    [HttpDelete("{taskId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteTask(string taskId, CancellationToken ct)
    {
        var command = new DeleteTaskCommand(taskId);
        await _mediator.Send(command, ct);
        return Ok(new { message = "Task deleted successfully" });
    }

    [HttpPost("{taskId}/attachments")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100MB
    [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttachmentResponse>> UploadAttachment(
        string taskId,
        IFormFile file,
        CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { detail = "No file uploaded" });

        using var stream = file.OpenReadStream();
        var command = new UploadAttachmentCommand(
            taskId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length
        );

        var response = await _mediator.Send(command, ct);
        return Ok(response);
    }

    [HttpGet("{taskId}/attachments/{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadAttachment(
        string taskId,
        string attachmentId,
        CancellationToken ct)
    {
        var query = new DownloadAttachmentQuery(taskId, attachmentId);
        var (stream, fileName, contentType) = await _mediator.Send(query, ct);
        return File(stream, contentType, fileName);
    }

    [HttpDelete("{taskId}/attachments/{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAttachment(
        string taskId,
        string attachmentId,
        CancellationToken ct)
    {
        var command = new DeleteAttachmentCommand(taskId, attachmentId);
        await _mediator.Send(command, ct);
        return Ok(new { message = "Attachment deleted successfully" });
    }
}