// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Security.Principal;

namespace DotSetup
{
    public class UserUtils
    {
        /// <summary>
        /// Determines whether the specified user is an administrator.
        /// </summary>
        /// <param name="username">The user name.</param>
        /// <returns>
        ///   <c>true</c> if the specified user is an administrator; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso href="https://ayende.com/blog/158401/are-you-an-administrator"/>
        public static bool IsAdministratorNoCache(string username)
        {
            PrincipalContext ctx;
            try
            {
                Domain.GetComputerDomain();
                try
                {
                    ctx = new PrincipalContext(ContextType.Domain);
                }
                catch (PrincipalServerDownException)
                {
                    // can't access domain, check local machine instead 
                    ctx = new PrincipalContext(ContextType.Machine);
                }
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                // not in a domain
                ctx = new PrincipalContext(ContextType.Machine);
            }
            var up = UserPrincipal.FindByIdentity(ctx, IdentityType.SamAccountName, username);
            if (up != null)
            {
                using (PrincipalSearchResult<Principal> authGroups = up.GetAuthorizationGroups())
                {
                    return authGroups.Any(principal =>
                                          principal.Sid.IsWellKnown(WellKnownSidType.BuiltinAdministratorsSid) ||
                                          principal.Sid.IsWellKnown(WellKnownSidType.AccountDomainAdminsSid) ||
                                          principal.Sid.IsWellKnown(WellKnownSidType.AccountAdministratorSid) ||
                                          principal.Sid.IsWellKnown(WellKnownSidType.AccountEnterpriseAdminsSid));
                }
            }
            return false;
        }
    }
}
