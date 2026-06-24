using System;
using System.Configuration;
using Uniformance.PHD;

public static class PhdConnectionFactory
{
    private static readonly PHDServer _server;

    static PhdConnectionFactory()
    {
        string env_user = ConfigurationManager.AppSettings["PHDServerUsernameEnviromentKey"] ?? "";
        string env_pass = ConfigurationManager.AppSettings["PHDServerPasswordEnviromentKey"] ?? "";

        string user = Environment.GetEnvironmentVariable(env_user) ?? "";
        string pass = Environment.GetEnvironmentVariable(env_pass) ?? "";

        var host = ConfigurationManager.AppSettings["PHDServerHost"];
        var port = int.Parse(ConfigurationManager.AppSettings["PHDServerPort"]);

        _server = new PHDServer(host, SERVERVERSION.RAPI200)
        {
            Port = port,
            WindowsUsername = user,
            WindowsPassword = pass
        };
    }

    public static PHDServer GetServer() => _server;

    public static PHDServer CreateServer()
    {
        string env_user = ConfigurationManager.AppSettings["PHDServerUsernameEnviromentKey"] ?? "";
        string env_pass = ConfigurationManager.AppSettings["PHDServerPasswordEnviromentKey"] ?? "";

        string user = Environment.GetEnvironmentVariable(env_user) ?? "";
        string pass = Environment.GetEnvironmentVariable(env_pass) ?? "";

        var host = ConfigurationManager.AppSettings["PHDServerHost"];
        var port = int.Parse(ConfigurationManager.AppSettings["PHDServerPort"]);

        return new PHDServer(host, SERVERVERSION.RAPI200)
        {
            Port = port,
            WindowsUsername = user,
            WindowsPassword = pass
        }; 
    }
}