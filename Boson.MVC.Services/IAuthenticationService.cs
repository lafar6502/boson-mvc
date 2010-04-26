using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace BosonMVC.Services
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Authenticate user by login/password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="passwd"></param>
        /// <returns></returns>
        IPrincipal PasswordAuthenticate(string user, string passwd);
        /// <summary>
        /// Return principal of an already authenticated user (authenticated by forms or windows auth)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="authType"></param>
        /// <returns></returns>
        IPrincipal GetAuthenticatedUser(string userId, string authType);
    }
}
