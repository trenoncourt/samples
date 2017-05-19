using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Novell.Directory.Ldap;

namespace Ldap.NetCore.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var cn = new LdapConnection())
            {
                // connect
                cn.Connect("ldap.forumsys.com", 389);
                // bind with an username and password
                // this how you can verify the password of an user
                cn.Bind(LdapConnection.Ldap_V3, "cn=read-only-admin,dc=example,dc=com", "password");
                string[] attrs = {   "objectClass"};
                var x = cn.GetSchemaDN("ou=chemists,dc=example,dc=com");
                var xx = cn.Read(x);
                var t = cn.Search("dc=example,dc=com", LdapConnection.SCOPE_SUB, "(objectClass=groupOfUniqueNames)", null, false);
                // call ldap op
                // cn.Delete("<<userdn>>")
                // cn.Add(<<ldapEntryInstance>>)
                var tResponseControls = t.ResponseControls;

                var bite = new List<LdapEntry>();
                while (t.hasMore())
                {
                    bite.Add(t.next()); 
                }
                LdapAttributeSet ldapAttributeSet = bite.Skip(2).First().getAttributeSet();
                Test(ldapAttributeSet);

                IEnumerator allAttributes = ldapAttributeSet.GetEnumerator();
                foreach (object o in ldapAttributeSet)
                {
                    
                }
                var trtr = ldapAttributeSet[0];
                //var next = t.next();
                bool tt = t.hasMore();
            }



            System.Console.ReadKey();
        }

        static void Test(LdapAttributeSet attributeSet)
        {
            IEnumerator objClass = null;
            IEnumerator queryURL = null;
            IEnumerator identity = null;
            IEnumerator excludedMember = null;
            IEnumerator member = null;
            IEnumerator group = null;
            bool isGroup = false, isDynamicGroup = false;

            IEnumerator allAttributes = attributeSet.GetEnumerator();

            while (allAttributes.MoveNext())
            {
                LdapAttribute attribute = (LdapAttribute)allAttributes.Current;
                String attributeName = attribute.Name;
                // Save objectclass values
                if (attributeName.ToUpper().Equals("objectClass".ToUpper()))
                {
                    objClass = attribute.StringValues;
                }

                // Save the memberQueryURL attribute if present
                else if (attributeName.ToUpper().Equals("memberQueryURL".ToUpper()))
                {
                    queryURL = attribute.StringValues;
                }

                // Save the dgIdentity attribute if present
                else if (attributeName.ToUpper().Equals("dgIdentity".ToUpper()))
                {
                    identity = attribute.StringValues;
                }

                // Save the excludedMember attribute if present
                else if (attributeName.ToUpper().Equals("excludedMember".ToUpper()))
                {
                    excludedMember = attribute.StringValues;
                }

                else if (attributeName.ToUpper().Equals("ou".ToUpper()))
                {
                    group = attribute.StringValues;
                }

                /* Save the member attribute.  This may also show up
                 * as uniqueMember
                 */
                else if (attributeName.ToUpper().Equals("member".ToUpper()) ||
                         attributeName.ToUpper().Equals("uniqueMember".ToUpper()))
                {
                    member = attribute.StringValues;
                }

                //else if (attributeName.ToUpper().Equals("group".ToUpper()) ||
                //         attributeName.ToUpper().Equals("groupOfNames".ToUpper()) ||
                //         attributeName.ToUpper().Equals("groupOfUniqueNames".ToUpper()))
                //{
                //    group = attribute.StringValues;
                //}
            }

            /* Verify that this is a group object  (i.e. objectClass contains
			 * the value "group", "groupOfNames", or "groupOfUniqueNames").
			 * Also determine if this is a dynamic group object
			 * (i.e. objectClass contains the value "dynamicGroup" or
			 * "dynamicGroupAux").
			 */
            while (objClass.MoveNext())
            {
                String objectName = (String)objClass.Current;
                if (objectName.ToUpper().Equals("group".ToUpper()) ||
                    objectName.ToUpper().Equals("groupOfNames".ToUpper()) ||
                    objectName.ToUpper().Equals("groupOfUniqueNames".ToUpper()))
                    isGroup = true;
                else if (objectName.ToUpper().Equals("dynamicGroup".ToUpper()) ||
                         objectName.ToUpper().Equals("dynamicGroupAux".ToUpper()))
                    isGroup = isDynamicGroup = true;
            }

            if (!isGroup)
            {
                System.Console.WriteLine("\tThis object is NOT a group object."
                                  + "Exiting.\n");
                Environment.Exit(0);
            }

            /* If this is a dynamic group, display its memberQueryURL, identity
			 * and excluded member list.
			 */
            if (isDynamicGroup)
            {
                if ((queryURL != null) && (queryURL.MoveNext()))
                {
                    System.Console.WriteLine("\tMember Query URL:");
                    while (queryURL.MoveNext())
                        System.Console.WriteLine("\t\t" + queryURL.Current);
                }

                if ((identity != null) && (identity.MoveNext()))
                {
                    System.Console.WriteLine("\tIdentity for search:"
                                      + identity.Current);
                }

                if ((excludedMember != null) &&
                    (excludedMember.MoveNext()))
                {
                    System.Console.WriteLine("\tExcluded member list:");
                    while (excludedMember.MoveNext())
                        System.Console.WriteLine("\t\t"
                                          + excludedMember.Current);
                }
            }

            // Print the goup's member list
            if (member != null && member.MoveNext())
            {
                System.Console.WriteLine("\n\tMember list:");
                System.Console.WriteLine("\t\t" + member.Current);
                while (member.MoveNext())
                    System.Console.WriteLine("\t\t" + member.Current);
            }


            if (group != null && group.MoveNext())
            {
                System.Console.WriteLine("\n\tGroup list:");
                System.Console.WriteLine("\t\t" + group.Current);
                while (group.MoveNext())
                    System.Console.WriteLine("\t\t" + group.Current);
            }

            // disconnect with the server
        }
    }
}