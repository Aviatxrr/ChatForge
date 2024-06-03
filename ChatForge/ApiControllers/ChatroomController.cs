using System.Runtime.CompilerServices;
using System.Security.Claims;
using ChatForge.DTOs;
using ChatForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.JsonWebTokens;
namespace ChatForge.ApiControllers;


[Route("api/[controller]")]
[ApiController]
public class ChatroomController : ControllerBase
{

    private readonly IChatroomService _chatroomService;
    private readonly TokenService _tokenService;
    private readonly ForgeHub _hub;
    
    public ChatroomController(IChatroomService chatroomService, TokenService tokenService, ForgeHub hub)
    {
        _chatroomService = chatroomService;
        _tokenService = tokenService;
        _hub = hub;
    }
    
    //POST api/chatroom/createroom
    [Authorize]
    [HttpPost("createroom")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateChatroomDto dto)
    {
        if (_tokenService.CheckForbid(Request.Headers.Authorization)
            || User.HasClaim("RefreshKey", "RefreshKey"))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _chatroomService.CreateRoom(dto.RoomName, int.Parse(User.Identity.Name), dto.IsPrivate);
            return Ok(result.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    //POST api/chatroom/deleteroom
    [Authorize]
    [HttpDelete("deleteroom")]
    public async Task<IActionResult> DeleteRoom([FromBody] DeleteRoomDto dto)
    {
        if (_tokenService.CheckForbid(Request.Headers.Authorization)
            || User.HasClaim("RefreshKey", "RefreshKey"))
        {
                return Unauthorized();
        }

        try
        {
            var result = await _chatroomService.DeleteRoom(dto.RoomId, int.Parse(User.Identity.Name));
            return Ok(result.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
    
    //POST api/chatroom/joinroom
    [Authorize]
    [HttpPost("joinroom")]
    public async Task<IActionResult> JoinRoom([FromBody] JoinRoomDto dto)
    {
        if (_tokenService.CheckForbid(Request.Headers.Authorization)
            || User.HasClaim("RefreshKey", "RefreshKey"))
        {
            return Unauthorized();
        }

        try
        { 
            var result = await _chatroomService.JoinRoom(dto.RoomId, int.Parse(User.Identity.Name)); 
            return Ok(result.Message);
        }
        catch (Exception ex)
        { 
            return BadRequest(ex.Message);
        }
        
    }

    //POST api/chatroom/leaveroom
    [Authorize]
    [HttpPost("leaveroom")]
    public async Task<IActionResult> LeaveRoom([FromBody] LeaveRoomDto dto)
    {
        if (_tokenService.CheckForbid(Request.Headers.Authorization)
            || User.HasClaim("RefreshKey", "RefreshKey"))
        {
            return Unauthorized();
        }
        try
        {
            var result = await _chatroomService.LeaveRoom(dto.RoomId, int.Parse(User.Identity.Name)); 
            return Ok(result.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        
    }
    
    //POST api/chatroom/getmessages
    [Authorize]
    [HttpPost("getmessages")]
    public async Task<IActionResult> GetMessages([FromBody] GetMessagesDto dto)
    {
        Console.WriteLine("Got req");
        
        if (_tokenService.CheckForbid(Request.Headers.Authorization)
            || User.HasClaim("RefreshKey", "RefreshKey"))
        {
            return Unauthorized();
        }
        try
        {
            var result = await _chatroomService.GetMessages(dto.RoomId, int.Parse(User.Identity.Name), dto.BeginIndex);
            if(result.Successful)
            {
                return Ok(result.Data);
            }

            return Unauthorized("User is not in this room");
        }
        catch (Exception ex)
        {
            
            return BadRequest("Room does not exist");
        }
        
    }
    
    //POST api/chatroom/sendmessage
    [Authorize]
    [HttpPost("sendmessage")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        if (_tokenService.CheckForbid(Request.Headers.Authorization)
            || User.HasClaim("RefreshKey", "RefreshKey"))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _chatroomService.SendMessage(dto.RoomId, int.Parse(User.Identity.Name), dto.Contents);
            await _hub.Clients.All.SendAsync("ReceiveMessage", dto.RoomId);
            return Ok(result.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    //GET api/chatroom
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetRooms()
    {
        if (_tokenService.CheckForbid(Request.Headers.Authorization)
            || User.HasClaim("RefreshKey", "RefreshKey"))
        {
            return Unauthorized();
        }
        try
        {
            var result = await _chatroomService.GetRooms(int.Parse(User.Identity.Name)); 
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}