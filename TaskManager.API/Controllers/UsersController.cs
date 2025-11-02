namespace TaskManager.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Users.Commands;
using TaskManager.Application.Users.Queries;
using TaskManager.Contracts.Users;

[ApiController]
[Route("api/admin/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<UserResponse>>> GetAll(CancellationToken ct)
    {
        var query = new GetAllUsersQuery();
        var response = await _mediator.Send(query, ct);
        return Ok(response);
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Update(
    string userId,
    [FromBody] UpdateUserRequest request,
    CancellationToken ct)
    {
        var command = new UpdateUserCommand(
            userId,
            request.Name,
            request.Email,
            request.Role
        );

        var response = await _mediator.Send(command, ct);
        return Ok(response);
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string userId, CancellationToken ct)
    {
        var command = new DeleteUserCommand(userId);
        await _mediator.Send(command, ct);
        return Ok(new { message = "User deleted successfully" });
    }
}