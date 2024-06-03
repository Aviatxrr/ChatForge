using System.Security.Cryptography;
using System.Text;
using ChatForge.DataAccess;
using ChatForge.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ChatForge.Services;

public interface IUserService
{
    Task<ServiceResult> CreateUser(string username, string password);
    Task<ServiceResult> Authenticate(string username, string password);

    Task<ServiceResult> DeleteUser(int userId, string token);
    Task<ServiceResult> ChangePassword(int userId, string password, string token);

    Task<ServiceResult> RefreshToken(int userId);
    Task<ServiceResult> Logout(int userId, string token);
}

public class UserService : IUserService
{
    private Repository<User> _users;
    private TokenService _tokenService;

    public UserService(Repository<User> users, TokenService tokenService)
    {
        _users = users;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult> CreateUser(string username, string password)
    {
        
        // no spaces in username
        if (username.Contains(" "))
        {
            return ServiceResult.Failure("Username must not contain spaces");
        }
        
        // check if user already exists
        if (_users.GetAll().Select(u => u.Username).ToList().Contains(username))
        {
            return ServiceResult.Failure($"User {username} already exists.");
        }
        
        // get password has and salt
        var pwdSalt = generatePwdSalt();
        var saltedPwd = password + pwdSalt;
        var pwdHash = SHA256.HashData(Encoding.UTF8.GetBytes(saltedPwd));
        
        // generate a new user
        User newUser = new User();
        newUser.Username = username;
        newUser.PasswordHash = pwdHash;
        newUser.PasswordSalt = pwdSalt;
        
        // try to add user to DB, need more robust error handling
        try
        {
            _users.Add(newUser);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return ServiceResult.Failure("Error occured");
        }
        
        return ServiceResult.Success($"User {username} has been created");
    }

    public async Task<ServiceResult> Authenticate(string username, string password)
    {
        User authenticateUser;
        try
        {
            authenticateUser = _users.GetAll().First(u => u.Username == username);
        }
        catch (InvalidOperationException ex)
        {
            return ServiceResult.Failure($"Username {username} not found");
        }
        var checkPwd = SHA256.HashData(Encoding.UTF8.GetBytes(password + authenticateUser.PasswordSalt));
        if (checkPwd.SequenceEqual(authenticateUser.PasswordHash))
        {
            var accessToken = _tokenService.GetKey(authenticateUser);
            var refreshToken = _tokenService.GetKey(authenticateUser, 24, true);
            if (authenticateUser.RefreshToken is not null)
            {
                _tokenService.ForbidToken(authenticateUser.RefreshToken);
            }
            authenticateUser.RefreshToken = refreshToken;
            _users.Update(authenticateUser);
            return ServiceResult.Success(data: new Dictionary<string, object>()
            {
                {"access", accessToken}, 
                {"refresh", refreshToken}
            });
        }
        return ServiceResult.Failure("Invalid credentials");
        
    }

    public async Task<ServiceResult> DeleteUser(int userId, string token)
    {
        try
        {
            _users.Delete(_users.GetById(userId));
            _tokenService.ForbidToken(token);
            return ServiceResult.Success($"User {userId} has been deleted");
            
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult> ChangePassword(int userId, string password, string token)
    {
        try
        {
            if (_tokenService.CheckForbid(token))
            {
                _tokenService.ForbidToken(token);
                return ServiceResult.Failure("Invalid Token");
            }
            User user = _users.GetById(userId);

            // get password has and salt
            var pwdSalt = generatePwdSalt();
            var saltedPwd = password + pwdSalt;
            var pwdHash = SHA256.HashData(Encoding.UTF8.GetBytes(saltedPwd));
            user.PasswordHash = pwdHash;
            user.PasswordSalt = pwdSalt;
            if (user.RefreshToken is not null)
            {
                _tokenService.ForbidToken(user.RefreshToken);
            }
            _tokenService.ForbidToken(token);
            _users.Update(user);
            return ServiceResult.Success($"User {userId}'s password has been changed");
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult> RefreshToken(int userId)
    {
        User user = _users.GetById(userId);
        var token = _tokenService.GetKey(user);
        user.RefreshToken = token;
        return ServiceResult.Success(data: new Dictionary<string, object>()
        {
            {"access", token}
        });
    }

    public async Task<ServiceResult> Logout(int userId, string token)
    {
        _tokenService.ForbidToken(token);
        var refreshToken = _users.GetById(userId).RefreshToken;
        if(refreshToken is not null)
        {
            _tokenService.ForbidToken(refreshToken);
        }
        return ServiceResult.Success();
    }
    
    
    private string generatePwdSalt()
    {
        int baseAscii = 'A';
        byte[] letters = new byte[20];
        for (int i = 0; i < 20; i++)
        {
            letters[i] = (byte)(baseAscii + Random.Shared.Next(0, 26));
        }
        return Convert.ToBase64String(letters);
    }
    
    
}