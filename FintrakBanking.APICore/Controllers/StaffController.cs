using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Setups.General;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setup")]
    public class StaffController : ApiControllerBase
    {
        TokenDecryptionHelper token = null;
        private IStaffRepository repo;
        IErrorLogRepository errorLogger;
        public StaffController(IStaffRepository _repo,
                                IErrorLogRepository _errorLogger)
        {
            this.repo = _repo;
            this.errorLogger = _errorLogger;
        }

        [HttpGet]
        [Route("staff")]
        public HttpResponseMessage GetStaffInfo()
        {

            try
            {

                var token = new TokenDecryptionHelper();
                var staffinfo = repo.GetAllStaff().Where(x => x.companyId == token.GetCompanyId).ToList();

                if (staffinfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }


        }

        [HttpGet]
        [Route("staff/approvals/temp")]
        public HttpResponseMessage GetStaffAwaitingApproval()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var staffinfo = repo.GetStaffAwaitingApprovals(token.GetStaffId, token.GetCompanyId);

                if (staffinfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo.ToList() });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("staff/approvals/temp/{staffId}")]
        public HttpResponseMessage GetTempStaffDetailsById(int staffId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var staffinfo = repo.GetTempStaffDetail(staffId);

                if (staffinfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("staff/approvals/{staffCode}")]
        public HttpResponseMessage GetStaffDetailsById(string staffCode)
        {


            try
            {
                var token = new TokenDecryptionHelper();
                var staffinfo = repo.GetStaffDetail(staffCode, token.GetCompanyId);

                if (staffinfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

        [HttpGet]
        [Route("staff/approvals")]
        public HttpResponseMessage GetStaffDetails()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var staffinfo = repo.GetStaffDetails(token.GetCompanyId);

                if (staffinfo.ToList() == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("staff/names")]
        public HttpResponseMessage GetStaff()
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var staffinfo = repo.GetStaffNames();

                if (staffinfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("approval-status")]
        public HttpResponseMessage GetApprovalStatus()
        {

            try
            {
                var token = new TokenDecryptionHelper();
                var staffinfo = repo.GetApprovalStatus();


                if (staffinfo != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffinfo.ToList() });

                }
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "No record found" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }


        }

        [HttpGet]
        [Route("staff/{staffId}")]
        public HttpResponseMessage GetStaffInfoById(int staffId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var staffInfo = repo.GetStaffById(staffId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffInfo });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { error = true, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("staff")]
        public async Task<HttpResponseMessage> AddTempStaff([FromBody] StaffInfoViewModel model)
        {
            try
            {

                if (repo.IsStaffCodeAlreadyExist(model.StaffCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"A staff with {model.StaffCode} already exist" });
                }
                if (repo.IsTempStaffExist(model.StaffCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = $"A staff with {model.StaffCode} already exist waiting for approval" });
                }



                TokenDecryptionHelper token = new TokenDecryptionHelper();


                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;


                var username = token.GetUsername;
                var staffId = token.GetStaffId;
                var companyId = token.GetCompanyId; //etc

                //We can now use staffId extracted from the token as the created by
                //We ca also get companyId too

                var staff = await repo.AddTempStaff(model);

                if (staff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = staff, message = "Staff has been created successfully, now waiting for approval" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "staff not created" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("staff/{staffid}")]
        public async Task<HttpResponseMessage> UpdateStaffInfo(int staffid, [FromBody] StaffInfoViewModel model)
        {
            string username = string.Empty;
            try
            {
                var token = new TokenDecryptionHelper();
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = Request.RequestUri.Host;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;

                username = token.GetUsername;
                var staff = await repo.UpdateStaff(staffid, model);
                if (staff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = staff, message = "Staff has been updated successfully, now waiting for approval" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "staff not created" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, username);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("staff/{staffId}")]
        public HttpResponseMessage DeleteStaffInfo(int staffId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    staffId = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = Request.RequestUri.Host
                };
                var staff = repo.DeleteStaff(staffId, user);
                if (staff)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = staff, message = "staff has been created successfully" });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "staff not created" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("staffbybranch/{branchId}")]
        public HttpResponseMessage GetStaffInfoByBranchId(int branchId)
        {
            try
            {

                var staffInfo = repo.GetAllStaff().SingleOrDefault(c => c.BranchId == branchId);

                if (staffInfo == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, result = staffInfo });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("staff/approval")]
        public HttpResponseMessage GoForApprovalAsync([FromBody]ApprovalViewModel entity)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                entity.BranchId = token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.staffId = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = Request.RequestUri.Host;

                var data = repo.GoForApproval(entity);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = "Staff record has been approved successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new { success = true, message = "Operation successful, request has been routed to the next approving office" });
            }
            catch (System.Exception ex)
            {
                errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("staff/search/")]
        public HttpResponseMessage SearchStaff(string queryString)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.SearchStaff(queryString, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }

        }

        [HttpGet]
        [Route("staff/{departmentId}/search/")]
        public HttpResponseMessage SearchStaffbyDepartmentId(string queryString, int departmentId)
        {
            try
            {
                var token = new TokenDecryptionHelper();
                var data = repo.SearchStaffbyDepartmentId(queryString, token.GetCompanyId, departmentId);
                return Request.CreateResponse(HttpStatusCode.OK,
                     new { success = true, result = data.ToList() });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }

        }
    }
}