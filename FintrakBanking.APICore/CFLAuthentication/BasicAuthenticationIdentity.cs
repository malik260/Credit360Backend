using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace FintrakBanking.APICore.CFLAuthentication
{
    public class BasicAuthenticationIdentity : GenericIdentity
    {
    public BasicAuthenticationIdentity(string name, string password)
      :base(name, "Basic")
        {
            this.Password = password;
        }  
        public string Password { get; set; }
    }
}