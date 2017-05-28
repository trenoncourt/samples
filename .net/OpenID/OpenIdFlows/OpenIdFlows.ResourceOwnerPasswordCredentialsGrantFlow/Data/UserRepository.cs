using System.Collections.Generic;
using System.Linq;

namespace OpenIdFlows.ResourceOwnerPasswordCredentialsGrantFlow.Data
{
    public class UserRepository
    {
        private static readonly List<User> Users = new List<User>
        {
            new User
            {
                Id = 1,
                Email = "foo@bar.baz",
                Password = "AQAAAAEAACcQAAAAEIL9OZ1oPlEaBOZzRBw1IgPF1LApuBhKC35d+0wT5hJzOKUClOqqVGIuJ7uzqErm9A==" // Foo
            }
        };

        public User FindByEmail(string email)
        {
            return Users.FirstOrDefault(u => u.Email == email);
        }
    }
}