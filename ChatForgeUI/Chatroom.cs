using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using ReactiveUI;

namespace ChatForgeUI;

public class Chatroom : ReactiveObject
{
    public int _roomId;
    private HttpClient _client;
    private SessionContainer _ssc;
    private string _roomName;
    public string RoomName
    {
        get => _roomName;
    }

    public Chatroom(int roomId, string roomName, HttpClient client, SessionContainer ssc)
    {
        _roomId = roomId;
        _client = client;
        _ssc = ssc;
        _roomName = roomName;
    }
    
    public async Task<ObservableCollection<string>> GetMessages()
    {
        try
        {

            //get all messages in a room, based on currently selected room
            
            var message = JsonContent.Create(new Dictionary<string, int>()
            {
                { "RoomId", _roomId },
                { "BeginIndex", 1 }
            });
            
            //get the result, save it into a dict of same format as backend
            var response = await _client.PostAsync($"http://{_ssc.IP}/api/chatroom/getmessages", message);
            var result = JsonSerializer.Deserialize<Dictionary<int, List<string>>>(
                await response.Content.ReadAsStringAsync()
            );
            //massage the data a bit, concat the three message parts.
            var formatted = new Dictionary<int, string>();
            foreach (var entry in result.Keys)
            {
                formatted[entry] = $"{result[entry][0]} on {result[entry][2]}:\n \n" +
                                   $"{result[entry][1]}\n \n";
            }
            //return the values
            return new ObservableCollection<string>(formatted.Values);
        }
        catch (Exception ex)
        {
            return new ObservableCollection<string>()
            {
                $"{ex.Message}\n \n" +
                $"{ex.InnerException}" +
                $"{ex.InnerException}"
            };
        }
    }
    public async Task SendMessage(string contents)
    {
        var message = JsonContent.Create(new Dictionary<string, string>()
        {
            { "RoomId", _roomId.ToString() },
            { "Contents", contents}
        });
        await _client.PostAsync($"http://{_ssc.IP}/api/chatroom/sendmessage", message);
    }
}