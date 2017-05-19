using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ldap.Net.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryEntry entry = new DirectoryEntry();

            SearchResult result;
            
            DirectorySearcher search = new DirectorySearcher(new DirectoryEntry("www.zflexldap.com", "cn=ro_admin,ou=sysadmins,dc=zflexsoftware,dc=com", "zflexpass"));

            search.Filter = "(&(objectClass=user))";

            search.PropertiesToLoad.Add("cn");

            result = search.FindOne();
        }
    }
}
