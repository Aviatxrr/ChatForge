using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI;
namespace ChatForgeUI.ViewModels;

public class ChatDashboardViewModel : ViewModelBase
{

    public event Action NewMessage; 

    private HttpClient _client;
    private SessionContainer _ssc;

    private ObservableCollection<Chatroom> _chatrooms;
    public ObservableCollection<Chatroom> Chatrooms
    {
        get => _chatrooms;
        set => this.RaiseAndSetIfChanged(ref _chatrooms, value);
    }

    private Chatroom _selectedChatroom;
    public Chatroom SelectedChatroom
    {
        get => _selectedChatroom;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedChatroom, value);
            UpdateMessages(value);
        }
    }

    private ObservableCollection<string> _messages;
    public ObservableCollection<string> Messages
    {
        get => _messages;
        set => this.RaiseAndSetIfChanged(ref _messages, value);
    }

    private string _enteredMessage;

    public string EnteredMessage
    {
        get => _enteredMessage;
        set => this.RaiseAndSetIfChanged(ref _enteredMessage, value);
    }

    public ChatDashboardViewModel(HttpClient client, SessionContainer ssc)
    {
        _client = client;
        _ssc = ssc;
        Initialize();
    }

    public async Task<ObservableCollection<Chatroom>> GetRooms()
    {
        
        var response = await _client.GetAsync($"http://{_ssc.IP}/api/Chatroom");
        
        var result = JsonSerializer.Deserialize<Dictionary<int, string>>(
            await response.Content.ReadAsStringAsync()
        );
        
        return new ObservableCollection<Chatroom>(
            result.Select(entry => new Chatroom(entry.Key, entry.Value, _client, _ssc))
        );
    }

    public async Task UpdateMessages(Chatroom room)
    {
        Messages = await room.GetMessages();
        NewMessage?.Invoke();
    }

    public void SendMessage(string contents)
    {
        if (!string.IsNullOrWhiteSpace(contents))
        {
            _selectedChatroom.SendMessage(contents);
        }
        EnteredMessage = "";
    }
    
    public async Task Initialize()
    {
        Chatrooms = await GetRooms();
        ConfigureSignalR();
    }

    private void ConfigureSignalR()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl($"http://{_ssc.IP}/hub")
            .Build();

        connection.On<int>("ReceiveMessage", roomId =>
        {
            Console.WriteLine(roomId);
            if (roomId == _selectedChatroom._roomId)
            {
                UpdateMessages(_selectedChatroom);
            }
        });

        connection.StartAsync();
    }
}