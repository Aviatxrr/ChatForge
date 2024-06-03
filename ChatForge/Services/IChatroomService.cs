using System.Text.Json;
using ChatForge.DataAccess;
using ChatForge.Models;

namespace ChatForge.Services;

public interface IChatroomService
{
    Task<ServiceResult> CreateRoom(string roomName, int creatorId, bool isPrivate);
    Task<ServiceResult> DeleteRoom(int roomId, int userId);
    Task<ServiceResult> JoinRoom(int roomId, int userId);
    
    Task<ServiceResult> LeaveRoom(int roomId, int userId);
    Task<ServiceResult> GetMessages(int roomId, int userId, int beginIndex);
    
    Task<ServiceResult> SendMessage(int roomId, int senderId, string contents);
    Task<ServiceResult> GetRooms(int userId);
}

public class ChatroomService : IChatroomService
{
    private readonly Repository<UserChatroom> _userChatrooms;
    private readonly Repository<Chatroom> _chatrooms;
    private readonly Repository<User> _users;
    private readonly Repository<JoinRequest> _joinRequests;
    private readonly Repository<Message> _messages;
    
    public ChatroomService(Repository<UserChatroom> userChatrooms,
        Repository<Chatroom> chatrooms, 
        Repository<User> users, 
        Repository<JoinRequest> joinRequests, 
        Repository<Message> messages)
    {
        _userChatrooms = userChatrooms;
        _chatrooms = chatrooms;
        _users = users;
        _joinRequests = joinRequests;
        _messages = messages;
    }
    public async Task<ServiceResult> CreateRoom(string roomName, int creatorId, bool isPrivate = false)
    {
        try
        {
            Chatroom chatroom = new Chatroom();
            chatroom.Name = roomName;
            chatroom.Users = new List<User>();
            chatroom.Users.Add(_users.GetById(creatorId));
            chatroom.Id = _chatrooms.GetAll().Select(c => c.Id).ToList().Max() + 1;
            chatroom.Private = isPrivate;
            _chatrooms.Add(chatroom);
            var userChatroom = _userChatrooms.GetAll().First(c => c.UserId == creatorId && c.ChatroomId == chatroom.Id);
            userChatroom.ChatroomOwner = true;
            _userChatrooms.Update(userChatroom);
            return ServiceResult.Success($"Chatroom {roomName} created");
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure(ex.Message);
        }
    }
    
    public async Task<ServiceResult> DeleteRoom(int roomId, int userId)
    {
        if (_userChatrooms.GetAll().First(c => c.ChatroomId == roomId
                                               && c.UserId == userId).ChatroomOwner)
        {
            _chatrooms.Delete(_chatrooms.GetById(roomId));
            return ServiceResult.Success($"Chatroom {roomId} deleted");
        }
        return ServiceResult.Failure("User does not own chatroom");
    }
    
    public async Task<ServiceResult> JoinRoom(int roomId, int userId)
    {
        Chatroom room = _chatrooms.GetById(roomId);
        User user = _users.GetById(userId);
        if (!room.Private || checkValidInvite(roomId, userId))
        {
            room.Users.Add(user);
            _chatrooms.Update(room);
            return ServiceResult.Success($"User {userId} joined room {roomId}");
        }
        
        return ServiceResult.Failure("Couldn't join room");
    }
    
    public async Task<ServiceResult> LeaveRoom(int roomId, int userId)
    {
        Chatroom room = _chatrooms.GetById(roomId);
        User user = _users.GetById(userId);
        if (room.Users.Contains(user))
        {
            room.Users.Remove(user);
            _chatrooms.Update(room);
            return ServiceResult.Success($"User {userId} left room {roomId}");
        }
        return ServiceResult.Failure("User is not in room");
    }

    public async Task<ServiceResult> GetMessages(int roomId, int userId, int beginIndex)
    {
        // if user is not in room, return fail
        if (!_chatrooms.GetAll().First(c => c.Id == roomId).Users.Select(u => u.Id).ToList().Contains(userId))
        {
            return ServiceResult.Failure($"User is not in room {roomId}");
        }
        Dictionary<int, List<string>> messages = new Dictionary<int, List<string>>();
        
        foreach (var message in _messages.GetAll().Where(m => m.Id >= beginIndex && m.ChatroomId == roomId).OrderBy(m => m.Id).ToList())
        {
            messages.Add(message.Id, new List<string>(){message.Sender.Username, message.Contents, message.SentAt.ToString()});
        }

        return ServiceResult.Success(data: messages);
    }
    
    public async Task<ServiceResult> SendMessage(int roomId, int senderId, string contents)
    {
        if (!_chatrooms.GetAll().First(c => c.Id == roomId).Users.Select(u => u.Id).ToList().Contains(senderId))
        {
            return ServiceResult.Failure($"User is not in room {roomId}");
        }
        try
        {
            Message newMessage = new Message();
            newMessage.ChatroomId = roomId;
            newMessage.UserId = senderId;
            newMessage.Contents = contents;
            newMessage.SentAt = DateTime.Now;
            _messages.Add(newMessage);
            return ServiceResult.Success("Message sent");
        }
        catch (Exception ex)
        {
            
            return ServiceResult.Failure("Couldn't send message");
        }
    }

    public async Task<ServiceResult> GetRooms(int userId)
    {
        var rooms = _users.GetById(userId).Chatrooms;
        try
        {
            Dictionary<int, string> userRooms = new Dictionary<int, string>();
            foreach (var room in rooms)
            {
                userRooms.Add(room.Id, room.Name);
                Console.WriteLine(rooms);
            }

            return ServiceResult.Success(data: JsonSerializer.Serialize(userRooms));
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure(ex.Message);
        }
    }

    private bool checkValidInvite(int roomId, int userId)
    {
        JoinRequest joinRequest;
        try
        {
            joinRequest = _joinRequests.GetAll().First(j => j.UserId == userId && j.ChatroomId == roomId);
            if (joinRequest.IsInvite)
            {
                return true;
            }

            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}