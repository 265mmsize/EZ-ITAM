using Microsoft.AspNetCore.Identity;

namespace EzItam.Api.Controllers;

[Authorize(Policy = "AdminOnly")]
public class UsersController : ApiControllerBase
{
    private readonly UserManager<IdentityUser> _users;
    public UsersController(UserManager<IdentityUser> users) => _users = users;

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> List()
    {
        var list = new List<UserDto>();
        foreach (var u in await _users.Users.OrderBy(x => x.UserName).ToListAsync())
            list.Add(await ToDto(u));
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] UserFormDto form)
    {
        if (string.IsNullOrWhiteSpace(form.Password)) throw new DomainException("비밀번호를 입력하세요.");
        var user = new IdentityUser { UserName = form.UserName.Trim(), Email = form.Email, EmailConfirmed = true };
        var result = await _users.CreateAsync(user, form.Password);
        if (!result.Succeeded) throw new DomainException(string.Join(" ", result.Errors.Select(e => e.Description)));
        await _users.AddToRoleAsync(user, form.Role);
        return Ok(await ToDto(user));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> Update(string id, [FromBody] UserFormDto form)
    {
        var user = await _users.FindByIdAsync(id) ?? throw new DomainException("사용자를 찾을 수 없습니다.");
        user.Email = form.Email;
        if (!string.Equals(user.UserName, form.UserName, StringComparison.Ordinal))
            await _users.SetUserNameAsync(user, form.UserName.Trim());
        await _users.UpdateAsync(user);

        var roles = await _users.GetRolesAsync(user);
        if (!roles.Contains(form.Role))
        {
            await _users.RemoveFromRolesAsync(user, roles);
            await _users.AddToRoleAsync(user, form.Role);
        }
        if (!string.IsNullOrWhiteSpace(form.Password))
        {
            var token = await _users.GeneratePasswordResetTokenAsync(user);
            var reset = await _users.ResetPasswordAsync(user, token, form.Password);
            if (!reset.Succeeded) throw new DomainException(string.Join(" ", reset.Errors.Select(e => e.Description)));
        }
        return Ok(await ToDto(user));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return NotFoundError("사용자");
        if (string.Equals(user.UserName, CurrentUser, StringComparison.OrdinalIgnoreCase))
            throw new DomainException("현재 로그인한 계정은 삭제할 수 없습니다.");
        await _users.DeleteAsync(user);
        return NoContent();
    }

    [HttpPost("{id}/unlock")]
    public async Task<IActionResult> Unlock(string id)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return NotFoundError("사용자");
        await _users.SetLockoutEndDateAsync(user, null);
        await _users.ResetAccessFailedCountAsync(user);
        return NoContent();
    }

    private async Task<UserDto> ToDto(IdentityUser u) => new()
    {
        Id = u.Id, UserName = u.UserName ?? string.Empty, Email = u.Email,
        Roles = await _users.GetRolesAsync(u), IsLockedOut = await _users.IsLockedOutAsync(u)
    };
}
