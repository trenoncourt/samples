namespace Ldap.NetCore.Console
{
    public interface IAuthenticationService
    {
        AppUser Login(string username, string password);
    }
}