using System.Collections.Generic;
using System.Linq;

namespace OpenIdFlows.ClientCredentialsGrantFlow.Data
{
    public class ApplicationRepository
    {
        private static readonly List<Application> Users = new List<Application>
        {
            new Application
            {
                Id = 1,
                ClientId = "ClientId",
                ClientSecret = "ClientSecret"
            }
        };

        public Application FindByClientId(string clientId)
        {
            return Users.FirstOrDefault(u => u.ClientId == clientId);
        }
    }
}