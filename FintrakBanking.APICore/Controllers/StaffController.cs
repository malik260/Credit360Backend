using FintrakBanking.APICore.core;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.ErrorLogger;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Business;
using FintrakBanking.ViewModels.Setups.General;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace FintrakBanking.APICore.Controllers
{
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
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
        public HttpResponseMessage GetStaffInfo(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {

                    var token = new TokenDecryptionHelper();
                    var staffinfo = repo.GetAllStaff().Where(x => x.companyId == token.GetCompanyId).ToList();

                    if (staffinfo == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = staffinfo }));
                }
                catch (System.Exception ex)
                {
                    //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
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
        public HttpResponseMessage GetTempStaffDetailsById(HttpRequestMessage request, int staffId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var staffinfo = repo.GetTempStaffDetail(staffId);

                    if (staffinfo == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = staffinfo }));
                }
                catch (System.Exception ex)
                {
                    errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("staff/approvals/{staffCode}")]
        public HttpResponseMessage GetStaffDetailsById(HttpRequestMessage request, string staffCode)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var staffinfo = repo.GetStaffDetail(staffCode, token.GetCompanyId);

                    if (staffinfo == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = staffinfo }));
                }
                catch (System.Exception ex)
                {
                    errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("staff/approvals")]
        public HttpResponseMessage GetStaffDetails(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var staffinfo = repo.GetStaffDetails(token.GetCompanyId);

                    if (staffinfo.ToList() == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = staffinfo }));
                }
                catch (System.Exception ex)
                {
                    errorLogger.LogError(ex, Request.RequestUri.Host, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("staff/names")]
        public HttpResponseMessage GetStaff(HttpRequestMessage request)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var staffinfo = repo.GetStaffNames();

                    if (staffinfo == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = staffinfo }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("approval-status")]
        public HttpResponseMessage GetApprovalStatus()
        {

            //return GetHttpResponse(request, () =>
            //{
            try
            {
                var token = new TokenDecryptionHelper();
                var staffinfo = repo.GetApprovalStatus();

                if (staffinfo != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,new { success = true, result = staffinfo.ToList() });
                    
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                           new { success = false, message = "No record found" });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK,new { success = false, message = ex.Message });
            }

            //return response;
            // });
        }

        [HttpGet]
        [Route("staff/{staffId}")]
        public HttpResponseMessage GetStaffInfoById(HttpRequestMessage request, int staffId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var staffInfo = repo.GetStaffById(staffId);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = true, result = staffInfo }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { error = true, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpPost]
        [Route("staff")]
        public HttpResponseMessage AddTempStaff(HttpRequestMessage request, [FromBody] StaffInfoViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {

                    if (repo.IsStaffCodeAlreadyExist(model.StaffCode))
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = $"A staff with {model.StaffCode} already exist" }));
                    }
                    if (repo.IsStaffExist(model.StaffCode))
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = $"A staff with {model.StaffCode} already exist waiting for approval" }));
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

                    model.createdBy = staffId; ///This staff Id was gotten from the token


                    var staff = repo.AddTempStaff(model);

                    if (staff)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = staff, message = "Staff has been created successfully, now waiting for approval" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "staff not created" }));
                }
                catch (System.Exception ex)
                {
                    errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpPut]
        [Route("staff/{staffid}")]
        public HttpResponseMessage UpdateStaffInfo(HttpRequestMessage request, int staffid, [FromBody] StaffInfoViewModel model)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {

                    var token = new TokenDecryptionHelper();
                    model.userBranchId = (short)token.GetBranchId;
                    model.userIPAddress = Request.RequestUri.Host;
                    model.applicationUrl = HttpContext.Current.Request.Path;
                    model.createdBy = token.GetStaffId;


                    var staff = repo.UpdateStaff(staffid, model);
                    if (staff)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = staff, message = "Staff has been updated successfully, now waiting for approval" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "staff not created" }));
                }
                catch (System.Exception ex)
                {
                    errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpDelete]
        [Route("staff/{staffId}")]
        public HttpResponseMessage DeleteStaffInfo(HttpRequestMessage request, int staffId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, result = staff, message = "staff has been created successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "staff not created" }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpGet]
        [Route("staffbybranch/{branchId}")]
        public HttpResponseMessage GetStaffInfoByBranchId(HttpRequestMessage request, int branchId)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {

                    var staffInfo = repo.GetAllStaff().SingleOrDefault(c => c.BranchId == branchId);

                    if (staffInfo == null)
                    {
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = false, message = "No record found" }));
                    }
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = staffInfo }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }

        [HttpPost]
        [Route("staff/approval")]
        public HttpResponseMessage GoForApproval(HttpRequestMessage request, [FromBody]ApprovalViewModel entity)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
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
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, message = "Staff record has been approved successfully" }));
                    }
                    else
                        response = request.CreateResponse(HttpStatusCode.OK,
                            Ok(new { success = true, message = "Operation successful, request has been routed to the next approving office" }));
                }
                catch (System.Exception ex)
                {
                    errorLogger.LogError(ex, Request.RequestUri.AbsolutePath, token.GetUsername);
                    response = request.CreateResponse(HttpStatusCode.OK, Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });

        }

        [HttpGet]
        [Route("staff/search/{queryString}")]
        public HttpResponseMessage SearchStaff(HttpRequestMessage request, string queryString)
        {
            HttpResponseMessage response = null;
            return GetHttpResponse(request, () =>
            {
                try
                {
                    var token = new TokenDecryptionHelper();
                    var data = repo.SearchStaff(queryString, token.GetCompanyId);
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = true, result = data.ToList() }));
                }
                catch (System.Exception ex)
                {
                    response = request.CreateResponse(HttpStatusCode.OK,
                        Ok(new { success = false, message = ex.Message }));
                }

                return response;
            });
        }


    }



}