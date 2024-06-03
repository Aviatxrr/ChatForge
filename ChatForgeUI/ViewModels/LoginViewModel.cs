using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reactive;
using System.Text.Json;
using System.Threading.Tasks;
using ChatForgeUI.Views;

using ReactiveUI;

namespace ChatForgeUI.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly HttpClient _client;
    public string IP;
    
    private string _username;
    public string Username
    {
        get => _username;
        set => this.RaiseAndSetIfChanged(ref _username, value);
    }


    private string _enteredPassword;
    public string EnteredPassword
    {
        get => _enteredPassword;
        set => this.RaiseAndSetIfChanged(ref _enteredPassword, value);
    }

    private string _registerPassword;
    public string RegisterPassword
    {
        get => _registerPassword;
        set => this.RaiseAndSetIfChanged(ref _registerPassword, value);
    }
    
    
    public LoginViewModel(HttpClient client)
    {
        
        _client = client;
    }

    public async Task ConfirmLogin()
    {
        HttpResponseMessage response = new HttpResponseMessage();
        var message = JsonContent.Create(new Dictionary<string, string>()
        {
            { "Username", _username },
            { "Password", _enteredPassword }
        });

        try
        {
            response = await _client.PostAsync($"http://{IP}/api/user/authenticate", message);
            if (response.IsSuccessStatusCode)
            {
                var accessToken =
                    JsonSerializer.Deserialize<Dictionary<string, string>>(
                        response.Content.ReadAsStringAsync().Result)!["access"];
                _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                App.GetService<SessionContainer>().accessToken = accessToken;
                App.GetService<SessionContainer>().IP = IP;
                App.GetService<Dialog>().Hide();
                App.GetService<MainWindowViewModel>().SetContent(App.GetService<ChatDashboardViewModel>());
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message} :: {response.Content.ReadAsStringAsync().Result}");
        }
        finally
        {
            Username = "";
            EnteredPassword = "";
            RegisterPassword = "";
        }
    }

    public async Task Register()
    {
        HttpResponseMessage response = new HttpResponseMessage();
        var message = JsonContent.Create(new Dictionary<string, string>()
        {
            { "Username", _username },
            { "Password", _enteredPassword }
        });

        if (_enteredPassword.Equals(_registerPassword))
        {
            try
            {
                response = await _client.PostAsync($"http://{IP}/api/user/register", message);
                if (response.IsSuccessStatusCode)
                {
                    App.GetService<Dialog>().Hide();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} :: {response.Content.ReadAsStringAsync().Result}");
            }
        }
        Username = "";
        EnteredPassword = "";
        RegisterPassword = "";
    }
}