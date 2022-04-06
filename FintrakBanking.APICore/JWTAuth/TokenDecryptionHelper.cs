using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace FintrakBanking.APICore.JWTAuth
{
    public class TokenDecryptionHelper
    {
        public int GetStaffId { get { return int.Parse(this.GetInfoFromToken(1).ToString()); } }
        public int GetCompanyId { get { return int.Parse(this.GetInfoFromToken(2).ToString()); } }
        public int GetCountryId { get { return int.Parse(this.GetInfoFromToken(5).ToString()); } }
        public int GetBranchId { get { return int.Parse(this.GetInfoFromToken(3).ToString()); } }
        public string GetUsername { get { return this.GetInfoFromToken(4).ToString(); } }
        public int GetUserId { get { return int.Parse(this.GetInfoFromToken(6).ToString()); } }
        public string LoginCode { get { return  this.GetInfoFromToken(8).ToString(); } }
        public int GetRoleId { get { return int.Parse(this.GetInfoFromToken(9).ToString()); } }
        public int GetUserGroupId { get { return int.Parse(this.GetInfoFromToken(10).ToString()); } }
        public string GetUserActivities { get { return this.GetInfoFromToken(12).ToString(); } }
        public string GetStaffRoleCode { get { return (this.GetInfoFromToken(11).ToString()); } }


        private object GetInfoFromToken(int tokenType)
        {
            var tokenIdentity = new ClaimsIdentity(HttpContext.Current.User.Identity);
            var decryptedToken = tokenIdentity.Claims;

            //if (tokenIdentity.Name == null) { return String.Empty; }
            if (tokenIdentity.Name == null) { return " "; }

            switch (tokenType)
            {
                case 1: return decryptedToken.FirstOrDefault(st => st.Type == "staffId").Value.ToString();
                case 2: return decryptedToken.FirstOrDefault(st => st.Type == "companyId").Value.ToString();
                case 3: return decryptedToken.FirstOrDefault(st => st.Type == "branchId").Value.ToString();
                case 4: return decryptedToken.FirstOrDefault(st => st.Type == "username").Value.ToString();
                case 5: return decryptedToken.FirstOrDefault(st => st.Type == "countryId").Value.ToString();
                case 6: return decryptedToken.FirstOrDefault(st => st.Type == "userId").Value.ToString();
                case 8: return decryptedToken.FirstOrDefault(st => st.Type == "logincode").Value.ToString();
                case 9: return decryptedToken.FirstOrDefault(st => st.Type == "roleId").Value.ToString();
                case 10: return decryptedToken.FirstOrDefault(st => st.Type == "userGroupId").Value.ToString();
                case 11: return decryptedToken.FirstOrDefault(st => st.Type == "usersRole").Value.ToString();
                case 12: return decryptedToken.FirstOrDefault(st => st.Type == "userActivities").Value.ToString();
                default: return decryptedToken.FirstOrDefault(st => st.Type == "staffId").Value.ToString();
            }
        }
    }
}