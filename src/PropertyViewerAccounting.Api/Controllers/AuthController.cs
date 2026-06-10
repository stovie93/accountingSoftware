using PropertyViewerAccounting.Api.Services;
using PropertyViewerAccounting.Core.DTOs;
using PropertyViewerAccounting.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace PropertyViewerAccounting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(AppDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserEmail == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.UserPasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }

        user.UserLastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = _jwtService.GetTokenExpiration();

        var userDto = new UserDto(
            user.UserId,
            user.UserEmail,
            user.UserFirstName,
            user.UserLastName,
            user.UserRole.ToString()
        );

        return Ok(new LoginResponse(token, refreshToken, expiresAt, userDto));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser(UserContext userContext)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == userContext.UserId);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserDto(
            user.UserId,
            user.UserEmail,
            user.UserFirstName,
            user.UserLastName,
            user.UserRole.ToString()
        ));
    }
}
