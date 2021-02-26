using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Novell.Directory.Ldap;
using WebArchive.Interfaces;
using WebArchive.Models.Ldap;

namespace WebArchive.Security
{
    public class LdapAuthenticationService : IAuthenticationService
    {
        private const string MemberOfAttribute = "memberOf";
        private const string SAMAccountNameAttribute = "sAMAccountName";
        private const string MailAttribute = "mail";
        private const string DistinguishedNameAttribute = "distinguishedName";
        private const string EmployeeBadgeTypeAttribute = "employeeBadgeType";
        private const string EmployeeIdAttribute = "employeeID";
        private const string DisplayNameAttribute = "displayName";
        private const string ThumbnailAttribute = "thumbnailPhoto";
        private const string FirstnameAttribute = "givenName";
        private const string LastnameAttribute = "sn";

        private readonly LdapConfig Configuration;
        private readonly LdapConnection Connection;

        public LdapAuthenticationService(IOptions<LdapConfig> configAccessor)
        {
            Configuration = configAccessor.Value;
            Connection = new LdapConnection();
            Connection.SecureSocketLayer = Configuration.Ssl;
        }

        public IAppUser Login(string username, string password)
        {
            string[] domainUser = DomainUser(username);
            string searchBase = Configuration.SearchBase;

            if (string.IsNullOrEmpty(domainUser[0]))
                throw new Exception("Your account is missing the domain name.");

            Connection.Connect(Configuration.Url, Configuration.Port);
            Connection.Bind(Configuration.Username, Configuration.Password);

            var searchFilter = String.Format(Configuration.SearchFilter, domainUser[1]);
            var result = Connection.Search(
                searchBase,
                LdapConnection.ScopeSub, 
                searchFilter,
                new[] { 
                    MemberOfAttribute, 
                    SAMAccountNameAttribute, 
                    MailAttribute,
                    DistinguishedNameAttribute,
                    EmployeeBadgeTypeAttribute,
                    EmployeeIdAttribute,
                    DisplayNameAttribute,
                    ThumbnailAttribute,
                    FirstnameAttribute,
                    LastnameAttribute
                }, 
                false
            );

            try
            {
                var user = result.Next();
                if (user != null)
                {
                    Connection.Bind(user.Dn, password);
                    if (Connection.Bound)
                    {
                        byte[] thumbnail = Array.Empty<byte>();
                        var accountName = user.GetAttribute(SAMAccountNameAttribute).StringValue;
                        var member = user.GetAttribute(MemberOfAttribute).StringValueArray;
                        var distinguishedName = user.GetAttribute(DistinguishedNameAttribute).StringValue;
                        if (!distinguishedName.Contains("OU=Generic-Account"))
                        {
                            var email = user.GetAttribute(MailAttribute).StringValue;
                            var badgeType = user.GetAttribute(EmployeeBadgeTypeAttribute).StringValue;
                            var Id = user.GetAttribute(EmployeeIdAttribute).StringValue;
                            var displayName = user.GetAttribute(DisplayNameAttribute).StringValue;
                            var firstName = user.GetAttribute(FirstnameAttribute).StringValue;
                            var lastName = user.GetAttribute(LastnameAttribute).StringValue;
                            try { thumbnail = user.GetAttribute(ThumbnailAttribute).ByteValue; }
                            catch { thumbnail = Array.Empty<byte>(); }

                            return new AppUser
                            {
                                Username = accountName,
                                Fistname = firstName,
                                Lastname = lastName,
                                Email = email,
                                BadgeType = badgeType,
                                BadgeId = Id,
                                DisplayName = displayName,
                                DistinguishedName = distinguishedName,
                                Initials = GetInitials(lastName, firstName),
                                Domain = domainUser[0],
                                Thumbnail = thumbnail,
                                Roles = member
                                    .Select(x => GetGroup(x))
                                    .Where(x => x != null)
                                    .Distinct()
                                    .ToArray()
                            };
                        }
                        else
                        {
                            return new AppUser
                            {
                                Username = accountName,
                                Fistname = "",
                                Lastname = "",
                                Email = "",
                                BadgeType = "generic",
                                BadgeId = "",
                                DisplayName = accountName,
                                DistinguishedName = distinguishedName,
                                Initials = "GA",
                                Domain = domainUser[0],
                                Thumbnail = thumbnail,
                                Roles = member
                                    .Select(x => GetGroup(x))
                                    .Where(x => x != null)
                                    .Distinct()
                                    .ToArray()
                            };
                        }
                    }
                }
            }
            finally
            {
                Connection.Disconnect();
            }

            return null;
        }

        private string GetGroup(string value)
        {
            Match match = Regex.Match(value, "^CN=([^,]*)");
            if (!match.Success)
            {
                return null;
            }

            return match.Groups[1].Value;
        }

        private string[] DomainUser(string user)
        {
            if (user.Contains("\\"))
            {
                return user.Split("\\", StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                string[] array = { "", user };
                return array;
            }
        }

        string GetInitials(string lastName, string firstName)
        {
            return lastName[0].ToString() + firstName[0].ToString();
        }
    }
}
