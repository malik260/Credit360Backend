using FintrakBanking.APICore.CFLAuthentication;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Common;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Authentication;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace FintrakBanking.APICore.Controllers
{
    [MyBasicAuthenticationFilter] // Authorization: Basic ZmludHJhayZAIyQ6ZmludHJhayZAIzM0OA==
    [RoutePrefix("api/v1/fintrak")]
    public class CashFlowLendingController : ApiController
    { 
        private ICashFlowLendingRepository repo;
        //private readonly IAuthenticationRepository repo_Auth;
        private  IGeneralSetupRepository genSetup;
        //// private ILoanApplicationRepository loanApplicationRepo;
        //private ILoanApplicationRepository loanApplicationRepo;
        //TokenDecryptionHelper token = new TokenDecryptionHelper();
        private readonly FinTrakBankingContext context;
        //private readonly IAuditTrailRepository auditTrail;

        public CashFlowLendingController(ICashFlowLendingRepository _repo,
            //IAuthenticationRepository _repo_Auth, 
            FinTrakBankingContext _context
            //IGeneralSetupRepository _genSetup,
            //IAuditTrailRepository _auditTrail
            )
        {
            this.repo = _repo;
            //    this.repo_Auth = _repo_Auth;
            this.context = _context;
            //    this.genSetup = _genSetup;
            //    this.auditTrail = _auditTrail;
        }

        //[HttpPost]// [ClaimsAuthorization]
        //[Route("")]
        //public HttpResponseMessage GetTokenAsync([FromBody] TokenVM user)
        //{
        //    //try
        //    //{
        //    byte[] pass = Convert.FromBase64String(user.password);
        //    string password = Encoding.UTF8.GetString(pass);


        //    user.password = StaticHelpers.EncryptSha512(password, StaticHelpers.EncryptionKey);
        //    string ipAddressStr = String.Empty;
        //    if (token.LoginCode == null) ipAddressStr = token.LoginCode.Split('@')[1];

        //    repo_Auth.SessionInfo = repo_Auth.CheckSessionState(user.username.ToLower(), ipAddressStr);
        //    var foundUser = repo_Auth.FindUserByUserNameAndPassword(user.username.ToLower(), user.password);

        //    if (foundUser == null)
        //    {
        //        var found = repo_Auth.GetSingleUserByUserName(user.username.ToLower());

        //        if (found.branchId != null)
        //        {
        //            var audit1 = new TBL_AUDIT
        //            {
        //                AUDITTYPEID = (short)AuditTypeEnum.LoginFailed,
        //                STAFFID = found.staffId,
        //                BRANCHID = (short)found.branchId,
        //                DETAIL = $"{user.username} login failed",
        //                IPADDRESS = CommonHelpers.GetUserIP(),
        //                URL = Request.RequestUri.AbsoluteUri,
        //                APPLICATIONDATE = genSetup.GetApplicationDate(),
        //                SYSTEMDATETIME = DateTime.Now,
        //                TARGETID = -1
        //            };

        //            auditTrail.AddAuditTrail(audit1);
        //        }

        //        context.SaveChanges();
                
        //    }
        //    return Request.CreateResponse(HttpStatusCode.BadRequest, new { success = false, message = "1001 Login Failure." });
        //}

        [HttpPost]
        //[ClaimsAuthorization]
        [Route("customer")]
        public HttpResponseMessage AddCustomer([FromBody] IncomingCustomerViewModels entity)
        {
            try
            {
                //entity.userBranchId = (short)token.GetBranchId;
                //entity.companyId = token.GetCompanyId;
                //entity.createdBy = token.GetStaffId;
                //entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.AddCustomer(entity);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully, now waiting for approval" });
                }
                else
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record, confirm all requested parameters are captured"});
            }
        }


        [HttpPost]
        //[ClaimsAuthorization]
        [Route("cfl-loan-request")]
        public HttpResponseMessage submitRequest([FromBody] CflLoanApplication entity)
        {
            try
            {
                //entity.userBranchId = (short)token.GetBranchId;
                //entity.companyId = token.GetCompanyId;
                //entity.createdBy = token.GetStaffId;
                //entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = repo.submitRequest(entity);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = entity, message = "The record has been created successfully, now waiting for approval" });
                }
                else
                {

                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record, confirm all requested parameters are captured" });
            }
        }

    }
}
