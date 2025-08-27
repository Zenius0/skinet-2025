using System;
using System.Security.Claims;
using API.DTOs;
using API.Extensions;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(SignInManager<AppUser> signInManager, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        var user = new AppUser
        {
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            Email = registerDto.Email,
            UserName = registerDto.Email
        };

        var result = await signInManager.UserManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem();
        }

        // Eğer admin email'i ise admin rolü ver (isteğe bağlı)
        if (registerDto.Email == "admin@test.com")
        {
            await signInManager.UserManager.AddToRoleAsync(user, "Admin");
        }

        var roles = await signInManager.UserManager.GetRolesAsync(user);
        var token = await tokenService.CreateToken(user);

        // HttpOnly Cookie set et (en güvenli yöntem) - Register
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Development için false, production'da true olmalı
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("AuthToken", token, cookieOptions);

        return Ok(new UserDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Token = token, // Client-side kullanım için (opsiyonel)
            Roles = roles
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return Unauthorized();

        var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

        if (!result.Succeeded) return Unauthorized();

        var roles = await signInManager.UserManager.GetRolesAsync(user);
        var token = await tokenService.CreateToken(user);

        // HttpOnly Cookie set et (en güvenli yöntem) - Login
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // Development için false, production'da true olmalı
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("AuthToken", token, cookieOptions);

        return Ok(new UserDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Token = token, // Client-side kullanım için (opsiyonel)
            Roles = roles
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        // HttpOnly cookie'yi temizle
        Response.Cookies.Delete("AuthToken");

        return NoContent();
    }

    [HttpGet("user-info")]
    public async Task<ActionResult> GetUserInfo()
    {
        if (User.Identity?.IsAuthenticated == false) return NoContent();

        var user = await signInManager.UserManager.GetUserByEmailWithAddress(User);

        return Ok(new
        {
            user.FirstName,
            user.LastName,
            user.Email,
            Address = user.Address?.ToDto(),
            Roles = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    [HttpGet("auth-status")]
    public ActionResult GetAuthState()
    {
        return Ok(new { IsAuthenticated = User.Identity?.IsAuthenticated ?? false });
    }

    [Authorize]
    [HttpPost("address")]
    public async Task<ActionResult<Address>> CreateOrUpdateAddress(AddressDto addressDto)
    {
        var user = await signInManager.UserManager.GetUserByEmailWithAddress(User);

        if (user.Address == null)
        {
            user.Address = addressDto.ToEntity();
        }
        else
        {
            user.Address.UpdateFromDto(addressDto);
        }

        var result = await signInManager.UserManager.UpdateAsync(user);

        if (!result.Succeeded) return BadRequest("Problem updating user address");

        return Ok(user.Address.ToDto());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("assign-role")]
    public async Task<ActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(assignRoleDto.Email);
        if (user == null) return NotFound("User not found");

        var result = await signInManager.UserManager.AddToRoleAsync(user, assignRoleDto.Role);
        if (!result.Succeeded) return BadRequest("Failed to assign role");

        return Ok($"Role {assignRoleDto.Role} assigned to {assignRoleDto.Email}");
    }
}
