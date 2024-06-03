using System.Security.Claims;
using ChatForge.DTOs;
using ChatForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace ChatForge.ApiControllers;


[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    //User control specific endpoints
    
    //POST api/user/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationDto registration)
    {
        var result = await _userService.CreateUser(registration.Username, registration.Password);
        if (!result.Successful)
        {
            return BadRequest(result.Message);
        }

        return Ok(result.Message);
    }
    
    //POST api/user/authenticate
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] AuthenticationDto authentication)
    {
        var result = await _userService.Authenticate(authentication.Username, authentication.Password);
        if (!result.Successful)
        {
            return Unauthorized(result.Message);
        }

        return Ok(result.Data);
    }
    
    //DELETE api/user/deleteuser
    [HttpDelete("deleteuser")]
    [Authorize]
    public async Task<IActionResult> Delete()
    {
        
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || User.HasClaim("RefreshKey", "RefreshKey"))
        {
            return Unauthorized("Invalid Token");
        }
        int userId = int.Parse(userIdClaim);
        var result = await _userService.DeleteUser(userId, Request.Headers.Authorization!);
        if (result.Successful)
        {
            return Ok(result.Message);
        }

        return BadRequest(result.Message);
    }
    
    //PATCH api/user/updatepassword
    [HttpPatch("updatepassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto pwdChange)
    {
        
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || User.HasClaim("RefreshKey", "RefreshKey"))
        {
            return Unauthorized("Invalid Token");
        }
        int userId = int.Parse(userIdClaim);
        var result = await _userService.ChangePassword(userId, pwdChange.Password, Request.Headers.Authorization!);
        if (result.Successful)
        {
            
            return Ok(result.Message);
        }

        return BadRequest(result.Message);
    }
    
    //POST api/user/refresh
    [HttpPost("refresh")]
    [Authorize]
    public async Task<IActionResult> Refresh()
    {
        var token = new ServiceResult();
        if(User.HasClaim("RefreshKey", "RefreshKey"))
        {
            token = await _userService.RefreshToken(int.Parse(User.Identity?.Name));
            if (token.Successful)
            {
                return Ok(token.Data);
            }
            else
            {
                return BadRequest(token.Message);
            }
        }

        return Unauthorized("Invalid Token");
    }
    
    //POST api/user/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var response = _userService.Logout(int.Parse(User.Identity.Name), Request.Headers.Authorization).Result;
        if (response.Successful)
        {
            return Ok();
        }
        
        return BadRequest(response.Message);
    }
    
    //GET api/user
    [HttpGet]
    public IActionResult TestConnection()
    {
        return Ok("Server is online!");
    }
}