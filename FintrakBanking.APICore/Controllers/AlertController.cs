using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;

using FintrakBanking.ViewModels;
using FintrakBanking.APICore.JWTAuth;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class AlertController : ApiController
    {
        private readonly IAlertRepository _repo;
        private readonly TokenDecryptionHelper _token = new TokenDecryptionHelper();

        public AlertController(IAlertRepository repo)
        {
            this._repo = repo;
        }

        #region title Setup
        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-title")]
        public HttpResponseMessage GetAlertTitle()
        {
            try
            {
                var alertViewModels = _repo.GetAllAlerts();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-title/{id}")]
        public HttpResponseMessage GetAlertTitleById([FromUri] int id)
        {
            try
            {
                var alertViewModels = _repo.GetAlertById(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-title")]
        public HttpResponseMessage AddAlertTitle([FromBody] AlertViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddAlertTitle(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("alert-title/{id}")]
        public HttpResponseMessage UpdateAlertTitle([FromUri] int id, [FromBody] AlertViewModel entity)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = _token.GetBranchId,
                    companyId = _token.GetCompanyId,
                    createdBy = _token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };

                var data = _repo.UpdateAlertTitle(id, entity, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("alert-title/{id}")]
        public HttpResponseMessage DeleteAlertTitle(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = _token.GetBranchId,
                companyId = _token.GetCompanyId,
                createdBy = _token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = _repo.DeleteAlertTitle(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region Setup
        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-setup")]
        public HttpResponseMessage GetAllAlertSetup()
        {
            try
            {
                var alertViewModels = _repo.GetAllAlertSetup();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-setup/{id}")]
        public HttpResponseMessage GetAlertSetupById([FromUri] int id)
        {
            try
            {
                var alertViewModels = _repo.GetAlertSetupById(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-setup")]
        public HttpResponseMessage AddAlertSetup([FromBody] AlertSetupViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddAlertSetup(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("alert-setup/{id}")]
        public HttpResponseMessage UpdateAlertSetup([FromUri] int id, [FromBody] AlertSetupViewModel entity)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = _token.GetBranchId,
                    companyId = _token.GetCompanyId,
                    createdBy = _token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };

                var data = _repo.UpdateAlertSetup(id, entity, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("alert-setup/{id}")]
        public HttpResponseMessage DeleteAlertSetup(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = _token.GetBranchId,
                companyId = _token.GetCompanyId,
                createdBy = _token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = _repo.DeleteAlertSetup(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region levelgroupmapping Setup
        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-levelgroupmapping")]
        public HttpResponseMessage GetAllAlertLevelGroupMapping()
        {
            try
            {
                var alertViewModels = _repo.GetAllAlertLevelGroupMapping();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-levelgroupmapping/{id}")]
        public HttpResponseMessage GetAlertLevelGroupMappingById([FromUri] int id)
        {
            try
            {
                var alertViewModels = _repo.GetAlertLevelGroupMappingById(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-levelgroupmapping")]
        public HttpResponseMessage AddAlertLevelGroupMapping([FromBody] LevelGroupMappingViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddAlertLevelGroupMapping(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("alert-levelgroupmapping/{id}")]
        public HttpResponseMessage UpdateAlertLevelGroupMapping([FromUri] int id, [FromBody] LevelGroupMappingViewModel entity)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = _token.GetBranchId,
                    companyId = _token.GetCompanyId,
                    createdBy = _token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };

                var data = _repo.UpdateAlertLevelGroupMapping(id, entity, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("alert-levelgroupmapping/{id}")]
        public HttpResponseMessage DeleteAlertLevelGroupMapping(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = _token.GetBranchId,
                companyId = _token.GetCompanyId,
                createdBy = _token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = _repo.DeleteAlertLevelGroupMapping(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region levelgroup Setup
        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-levelgroup")]
        public HttpResponseMessage GetAllAlertLevelGroup()
        {
            try
            {
                var alertViewModels = _repo.GetAllAlertLevelGroup();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-levelgroup/{id}")]
        public HttpResponseMessage GetAlertLevelGroupById([FromUri] int id)
        {
            try
            {
                var alertViewModels = _repo.GetAlertLevelGroupById(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-levelgroup")]
        public HttpResponseMessage AddAlertLevelGroup([FromBody] AlertLevelGroupViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddAlertLevelGroup(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("alert-levelgroup/{id}")]
        public HttpResponseMessage UpdateAlertLevelGroup([FromUri] int id, [FromBody] AlertLevelGroupViewModel entity)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = _token.GetBranchId,
                    companyId = _token.GetCompanyId,
                    createdBy = _token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };

                var data = _repo.UpdateAlertLevelGroup(id, entity, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("alert-levelgroup/{id}")]
        public HttpResponseMessage DeleteAlertLevelGroup(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = _token.GetBranchId,
                companyId = _token.GetCompanyId,
                createdBy = _token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = _repo.DeleteAlertLevelGroup(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        #region level Setup
        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-level")]
        public HttpResponseMessage GetAllAlertLevel()
        {
            try
            {
                var alertViewModels = _repo.GetAllAlertLevel();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-level/{id}")]
        public HttpResponseMessage GetAlertLevelById([FromUri] int id)
        {
            try
            {
                var alertViewModels = _repo.GetAlertLevelById(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-level")]
        public HttpResponseMessage AddAlertLevel([FromBody] AlertLevelViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddAlertLevel(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this record {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("alert-level/{id}")]
        public HttpResponseMessage UpdateAlertLevel([FromUri] int id, [FromBody] AlertLevelViewModel entity)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = _token.GetBranchId,
                    companyId = _token.GetCompanyId,
                    createdBy = _token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };

                var data = _repo.UpdateAlertLevel(id, entity, user);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updateding this record {e.Message}" });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("alert-level/{id}")]
        public HttpResponseMessage DeleteAlertLevel(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = _token.GetBranchId,
                companyId = _token.GetCompanyId,
                createdBy = _token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = _repo.DeleteAlertLevel(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-level-mis")]
        public HttpResponseMessage GetAllUserMisCode()
        {
            try
            {
                var misViewModels = _repo.GetAllUserMisCode();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = misViewModels, count = misViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

    }

}
