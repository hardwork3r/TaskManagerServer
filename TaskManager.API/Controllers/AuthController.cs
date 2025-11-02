namespace TaskManager.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Authentication.Commands;
using TaskManager.Application.Authentication.Queries;
using TaskManager.Contracts.Authentication;
using TaskManager.Contracts.Users;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TokenResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.Name,
            request.Role
        );

        var response = await _mediator.Send(command, ct);
        return Ok(response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var response = await _mediator.Send(command, ct);
        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken ct)
    {
        var query = new GetCurrentUserQuery();
        var response = await _mediator.Send(query, ct);
        return Ok(response);
    }
}
