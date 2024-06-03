using System.Diagnostics.Contracts;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Spectre.Console;

namespace ConsoleApp1;

class Program
{
    private static State _appState;
    private static HttpClient _client;
    private static CommandParser _parser;
    private static CommandHandler _handler;
    
    
    public static async Task Main(string[] args)
    {
        // init application
        _appState = State.Home;
        _client = new HttpClient();
        _handler = new CommandHandler(_client, _appState);
        _parser = new CommandParser(_handler);
        Console.Title = "ChatForge";

        Panel panel = new Panel("Hello");
        panel.Header = new PanelHeader("Hi");
        panel.Expand = true;
        AnsiConsole.Write(panel);
        // program main loop
        while (true)
        {
            var command = Console.ReadLine();
            
            // if in a room, assume "send" command, unless preceded by $, which denotes a command
            // this is a bit of a hack until i figure out a more coherent solution for defaulting to 
            // sending a message if the user is looking at a chatroom.
            if (_appState == State.InRoom)
            {
                if (!command.StartsWith("$"))
                {
                    command = $"send {command}";
                }
                else
                {
                    command = command[1..];
                }
            }
            if (command is not null)
            {
                var result = await _parser.Execute(command);
                Console.WriteLine(result.Message);
            }

        }
    }
}

public class CommandParser
{

    private CommandHandler _handler;
    private Dictionary<string, Func<string[], Task<ActionResult>>> _commands;
    
    
    public CommandParser(CommandHandler handler)
    {
        _handler = handler;
        _commands = new Dictionary<string, Func<string[], Task<ActionResult>>>()
        {
            {"connect", _handler.Connect},
            {"disconnect", _handler.Disconnect},
            {"exit", _handler.Exit},
            {"login", _handler.Login},
            {"register", _handler.Register}
        };
    }

    public async Task<ActionResult> Execute(string command)
    {
        var tokenized = tokenize(command);
        if (_commands.ContainsKey(tokenized[0]))
        {
            try
            {
                var result = await _commands[tokenized[0]](tokenized[1..]);
                return result;
            }
            catch (IndexOutOfRangeException ex)
            {
                return new ActionResult()
                {
                    Successful = false,
                    Message = "Not Enough Arguments"
                };
            }
        }
        return new ActionResult()
        {
            Successful = false,
            Message = $"command {tokenized[0]} not found"
        };
    }

    private string[] tokenize(string input)
    {
        return input.Split(" ");
    }
    
}

public class CommandHandler
{

    private readonly HttpClient _client;
    private State _appState;
    private string ip;
    private string _accessToken;
    private string _refreshToken;
    
    public CommandHandler(HttpClient client, State appState)
    {
        _client = client;
        _appState = appState;
    }

    public async Task<ActionResult> Connect(string[] parameters)
    {
        if (_appState != State.Home)
        {
            return new ActionResult()
            {
                Successful = false,
                Message = "Already connected to a server."
            };
        }

        try
        {
            var result = await _client.GetAsync($"http://{parameters[0]}/api/user");
            if (!result.IsSuccessStatusCode) throw new Exception("Failed to connect.");
            _appState = State.Connected;
            ip = $"http://{parameters[0]}";
            return new ActionResult()
            {
                Successful = true,
                Message = $"Connected to server {parameters[0]}"
            };
        }
        catch (Exception ex) when (!(ex is IndexOutOfRangeException))
        {
            return new ActionResult()
            {
                Successful = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResult> Disconnect(string[] _)
    {
        _appState = State.Home;
        ip = "";
        return new ActionResult()
        {
            Successful = true,
            Message = "Disconnected from server."
        };
    }

    public async Task<ActionResult> Login(string[] parameters)
    {
        try
        {
            if (_appState == State.LoggedIn)
                return new ActionResult()
                {
                    Successful = false,
                    Message = "Already logged in"
                };
            if (_appState != State.Connected) 
                return new ActionResult()
                {
                    Successful = false,
                    Message = "Not connected to a server." 
                };
            

            var data = new
            {
                Username = parameters[0],
                Password = parameters[1]
            };
            var result = await _client.PostAsync($"{ip}/api/user/authenticate", JsonContent.Create(data));
            
            if (!result.IsSuccessStatusCode)
            {
                return new ActionResult()
                {
                    Successful = false,
                    Message = await result.Content.ReadAsStringAsync()
                };
            }
            var response = JsonSerializer.Deserialize<Dictionary<string, string>>(await result.Content.ReadAsStringAsync());
            _accessToken = response["access"];
            _refreshToken = response["refresh"];
            _appState = State.LoggedIn;
            return new ActionResult()
            {
                Successful = true,
                Message = "Logged in successfully"
            };
        }
        catch (Exception ex) when (!(ex is IndexOutOfRangeException))
        {
            return new ActionResult()
            {
                Successful = false,
                Message = ex.Message
            };
        }
    }

    public async Task<ActionResult> Register(string[] parameters)
    {
        if (_appState != State.Connected)
        {
            return new ActionResult()
            {
                Successful = false,
                Message = "Not connected to a server"
            };
        }
        var data = new
        {
            Username = parameters[0],
            Password = parameters[1]
        };
        string json = JsonSerializer.Serialize(data);
        var result = await _client.PostAsync($"{ip}/api/user/register", JsonContent.Create(data));
        if (!result.IsSuccessStatusCode)
        {
            return new ActionResult()
            {
                Successful = false,
                Message = await result.Content.ReadAsStringAsync()
            };
        }
        return new ActionResult()
        {
            Successful = true,
            Message = "Successfully created user."
        };
    }

    public async Task<ActionResult> UpdatePassword(string[] password)
    {
        if (_appState != State.LoggedIn)
        {
            return new ActionResult()
            {
                Successful = false,
                Message = "Not logged in."
            };
        }

        return new ActionResult();
    }
    
    public async Task<ActionResult> Exit(string[] _)
    {
        Console.WriteLine("Goodbye!");
        Environment.Exit(0);
        return new ActionResult();
    }
    
}
public enum State
{
    Home,
    Connected,
    LoggedIn,
    InRoom,
}

public class ActionResult
{
    public bool Successful;
    public string? Message;
    public Dictionary<string, string>? Data;
}
