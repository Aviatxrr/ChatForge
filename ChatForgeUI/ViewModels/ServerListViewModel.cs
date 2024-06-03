
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reactive;

using ReactiveUI;

namespace ChatForgeUI.ViewModels;

public class ServerListViewModel : ViewModelBase
{
    private HttpClient _client;
    private LoginViewModel _loginViewModel;

    private ObservableCollection<string> _servers = new();
    public ObservableCollection<string> Servers
    {
        get => _servers;
        set => this.RaiseAndSetIfChanged(ref _servers, value);
    }

    private string _selectedServer;
    public string SelectedServer
    {
        get => _selectedServer;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedServer, value);
            _loginViewModel.IP = value;
        }
    }

    private string _enteredServer;

    public string EnteredServer
    {
        get => _enteredServer;
        set => this.RaiseAndSetIfChanged(ref _enteredServer, value);
    }

    public ReactiveCommand<string, Unit> AddServer;
    public ReactiveCommand<string, Unit> Connect;
    
    public ServerListViewModel(HttpClient client, LoginViewModel loginViewModel)
    {
        _client = client;
        _loginViewModel = loginViewModel;
        AddServer = ReactiveCommand.Create<string>(OnAddClick);
        Connect = ReactiveCommand.Create<string>(OnConnectClick);
        
    }

    public void OnAddClick(string server)
    {
        if (!(Servers.Contains(server) || server is null))
        {
            Servers.Add(server);
        }
        EnteredServer = "";
    }

    public async void OnConnectClick(string server)
    {
        try
        {
            if (server is not null)
            {
                var response = await _client.GetAsync($"http://{server}/api/user");
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                _loginViewModel.IP = server;
                App.ShowDialog(_loginViewModel);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        
    }

    public void DeleteServer(string server)
    {
        if (server is not null && Servers.Contains(server))
        {
            Servers.Remove(server);
        }
        
    }
}