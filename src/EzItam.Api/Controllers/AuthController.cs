using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace EzItam.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly IConfiguration _config;

    public AuthController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, IConfiguration config)
    {
        _users = users;
        _signIn = signIn;
        _config = config;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _users.FindByNameAsync(request.UserName ?? string.Empty);
        if (user is null) return Unauthorized(new ApiError { Message = "아이디 또는 비밀번호가 올바르지 않습니다." });

        var result = await _signIn.CheckPasswordSignInAsync(user, request.Password ?? string.Empty, lockoutOnFailure: true);
        if (result.IsLockedOut) return Unauthorized(new ApiError { Message = "로그인 실패 횟수 초과로 계정이 잠겼습니다. 10분 후 다시 시도하세요." });
        if (!result.Succeeded) return Unauthorized(new ApiError { Message = "아이디 또는 비밀번호가 올바르지 않습니다." });

        var roles = await _users.GetRolesAsync(user);
        var (token, expires) = IssueToken(user.UserName!, roles);
        return Ok(new TokenResponse { AccessToken = token, ExpiresAtUtc = expires, UserName = user.UserName!, Roles = roles.ToArray() });
    }

    [HttpPost("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _users.GetUserAsync(User);
        if (user is null) return Unauthorized(new ApiError { Message = "사용자를 찾을 수 없습니다." });
        var result = await _users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(new ApiError { Message = string.Join(" ", result.Errors.Select(e => e.Description)) });
        return NoContent();
    }

    private (string Token, DateTime Expires) IssueToken(string userName, IEnumerable<string> roles)
    {
        var jwt = _config.GetSection("Jwt");
        var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key 설정이 없습니다.");
        var minutes = jwt.GetValue("ExpiresMinutes", 480);
        var expires = DateTime.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim> { new(ClaimTypes.Name, userName), new(JwtRegisteredClaimNames.Sub, userName) };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt["Issuer"], jwt["Audience"], claims, expires: expires, signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
