namespace ChatForgeUI;

public class SessionContainer
{
    public string accessToken;
    public string refreshToken;
    public string IP;

    public SessionContainer()
    {
        accessToken = "";
        refreshToken = "";
        IP = "";
    }
}