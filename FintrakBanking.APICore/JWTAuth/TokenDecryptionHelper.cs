using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace FintrakBanking.APICore.JWTAuth
{
    public class TokenDecryptionHelper
    {
        
        //public TokenDecryptionHelper()
        //{
            
        //}


        public int GetStaffId { get { return int.Parse(this.GetInfoFromToken(1).ToString()); } }
        public int GetCompanyId { get { return int.Parse(this.GetInfoFromToken(2).ToString()); } }
        public int GetCountryId { get { return int.Parse(this.GetInfoFromToken(5).ToString()); } }
        public int GetBranchId { get { return int.Parse(this.GetInfoFromToken(3).ToString()); } }
        public string GetUsername { get { return this.GetInfoFromToken(4).ToString(); } }
        public int GetUserId { get { return int.Parse(this.GetInfoFromToken(6).ToString()); } }


        private object GetInfoFromToken(int tokenType)
        {
            var tokenIdentity = new ClaimsIdentity(HttpContext.Current.User.Identity);// this._context.User.Identity);
            var decryptedToken = tokenIdentity.Claims;


            switch (tokenType)
            {
                case 1:
                    return decryptedToken.First(st => st.Type == "staffId").Value.ToString();
                case 2:
                    return decryptedToken.First(st => st.Type == "companyId").Value.ToString();
                case 3:
                    return decryptedToken.First(st => st.Type == "branchId").Value.ToString();
                case 4:                   
                    return decryptedToken.First(st => st.Type == "username").Value.ToString();
                case 5:                  
                    return decryptedToken.First(st => st.Type == "countryId").Value.ToString();
                case 6:                   
                    return decryptedToken.First(st => st.Type == "userId").Value.ToString();
                default:                 
                    return decryptedToken.First(st => st.Type == "staffId").Value.ToString();
            }

            //return null;
        }

    }
}