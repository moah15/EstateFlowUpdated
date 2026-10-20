using EstateFlow.Api.Services.Auth;
using EstateFlow.Core.Dtos;
using EstateFlow.Core.Models;
using EstateFlow.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EstateFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TokenService _tokenService;
    private readonly EstateFlowDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TokenService tokenService,
        EstateFlowDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _context = context;
    }

    // TEMPORARY: open for initial team testing. Before real deployment,
    // add [Authorize(Roles = "FirmAdministrator")] here per Section 8.2.
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!await _roleManager.RoleExistsAsync(request.Role))
            return BadRequest(new { message = $"Role '{request.Role}' does not exist." });

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        // Internal staff (Estate Controller / Firm Administrator) get a linked
        // EstateController record — this is what Estate.ControllerId points to,
        // and what "assigned estates" and audit logging are tied to.
        if (request.Role is "EstateController" or "FirmAdministrator")
        {
            var controllerRecord = new EstateController
            {
                StaffName = request.FullName,
                Role = request.Role,
                Email = request.Email
            };
            _context.Controllers.Add(controllerRecord);
            await _context.SaveChangesAsync();
            user.ControllerId = controllerRecord.ControllerId;
        }

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, request.Role);

        if (request.Role == "FirmAdministrator")
            await _userManager.SetTwoFactorEnabledAsync(user, false);

        return Ok(new { message = "User registered.", userId = user.Id });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid email or password." });

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Unauthorized(new { message = "Invalid email or password." });

        if (await _userManager.GetTwoFactorEnabledAsync(user))
            return Ok(new LoginResponse { RequiresTwoFactor = true });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        return Ok(new LoginResponse
        {
            RequiresTwoFactor = false,
            Token = token,
            FullName = user.FullName,
            Role = roles.FirstOrDefault()
        });
    }

    [HttpPost("login/verify-2fa")]
    public async Task<ActionResult<LoginResponse>> VerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized(new { message = "Invalid request." });

        var codeValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);

        if (!codeValid)
            return Unauthorized(new { message = "Invalid authentication code." });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        return Ok(new LoginResponse
        {
            RequiresTwoFactor = false,
            Token = token,
            FullName = user.FullName,
            Role = roles.FirstOrDefault()
        });
    }

    [Authorize]
    [HttpPost("2fa/setup")]
    public async Task<ActionResult<TwoFactorSetupResponse>> SetupTwoFactor()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

        var email = user.Email ?? user.UserName;
        var authenticatorUri =
            $"otpauth://totp/EstateFlow:{Uri.EscapeDataString(email!)}?secret={unformattedKey}&issuer={Uri.EscapeDataString("EstateFlow")}&digits=6";

        return Ok(new TwoFactorSetupResponse
        {
            SharedKey = unformattedKey!,
            AuthenticatorUri = authenticatorUri
        });
    }

    [Authorize]
    [HttpPost("2fa/enable")]
    public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var codeValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);

        if (!codeValid)
            return BadRequest(new { message = "Invalid authentication code." });

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        return Ok(new { message = "Two-factor authentication enabled." });
    }
}