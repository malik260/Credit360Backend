using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;

using FintrakBanking.ViewModels;
using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.APICore.core;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class AlertController : ApiControllerBase
    {
        IAlertRepository _repo;
        IExternalAlertRepository _alertRepo;
        TokenDecryptionHelper _token = new TokenDecryptionHelper();

        public AlertController(IAlertRepository repo, IExternalAlertRepository alertRepo)
        {
            this._repo = repo;
            this._alertRepo = alertRepo;
        }

       
        #region title Setup
        [HttpGet]
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
        [Route("alert-binding-methods")]
        public HttpResponseMessage GetAllBindingMethods()
        {
            try
            {
                var alertViewModels = _repo.GetAllBindingMethods();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-title-dropdown")]
        public HttpResponseMessage GetAlertTitleForDropdown()
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
        [Route("alert-all-binding-methods")]
        public HttpResponseMessage GetAllAlertsBindingMethods()
        {
            try
            {
                var alertViewModels = _repo.GetAllAlertsBindingMethods();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("load-staff-role")]
        public HttpResponseMessage GetAlertStaffRoles()
        {
            try
            {
                var staffRoleInfo = _repo.GetAllStaffRoles();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffRoleInfo, count = staffRoleInfo.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //formerly load-staff-role
        [HttpGet]
        [ClaimsAuthorization]
        [Route("load-staff-group-email")]
        public HttpResponseMessage GetAllStaffGroupEmail()
        {
            try
            {
                var staffRoleInfo = _repo.GetAllStaffGroupEmail();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = staffRoleInfo, count = staffRoleInfo.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("load-alert-title")]
        public HttpResponseMessage GetAlerts()
        {
            try
            {
                var alertViewModels = _repo.GetAlerts();
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
        public HttpResponseMessage AddAlertTitle([FromBody] AlertTitleViewModel entity)
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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("update-alert-title-status")]
        public HttpResponseMessage UpdateAlertTitleStatus([FromBody] AlertTitleViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;
                //entity.userActivities = _token.GetUserActivities.ToLower();

                //if(!_token.GetUserActivities.ToLower().Contains(entity.userActivities.ToLower()))
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK,
                //   new { success = false, message = $"Unauthorize api access" });
                //}
                var data = _repo.UpdateAlertTitleStatus(entity);
                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The record status has been updated successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record status" });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this record status {e.Message}" });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("alert-title/{id}")]
        public HttpResponseMessage UpdateAlertTitle([FromUri] int id, [FromBody] AlertTitleViewModel entity)
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
        [Route("alert-group-email-list")]
        public HttpResponseMessage GetAllAlertGroupEmail()
        {
            try
            {
                var alertViewModels = _repo.GetAllAlertGroupEmail();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
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

                var data = _repo.AddAlertStaffRole(entity);
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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-group-email")]
        public HttpResponseMessage AddAlertGroupEmail([FromBody] AlertLevelViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddAlertGroupEmail(entity);
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

        #region others
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

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-frequency")]
        public HttpResponseMessage GetAllFrequency()
        {
            try
            {
                var frequency = _repo.GetAllFrequency();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = frequency, count = frequency.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-operations")]
        public HttpResponseMessage GetAllOpeartions()
        {
            try
            {
                var alertViewModels = _repo.GetAllOperations();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region condition Setup
        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-condition")]
        public HttpResponseMessage GetAllAlertConditions()
        {
            try
            {
                var alertViewModels = _repo.GetAllConditions();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, count = alertViewModels.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-condition/{id}")]
        public HttpResponseMessage GetAlertConditionById([FromUri] int id)
        {
            try
            {
                var alertViewModels = _repo.GetAlertConditionById(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-condition")]
        public HttpResponseMessage AddAlertCondition([FromBody] AlertConditionViewModel entity)
        {
            try
            {
                entity.companyId = _token.GetCompanyId;
                entity.userBranchId = (short)_token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.createdBy = _token.GetStaffId;

                var data = _repo.AddAlertCondition(entity);
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
        [Route("alert-condition/{id}")]
        public HttpResponseMessage UpdateAlertCondition([FromUri] int id, [FromBody] AlertConditionViewModel entity)
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

                var data = _repo.UpdateAlertCondition(id, entity, user);
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
        [Route("alert-condition/{id}")]
        public HttpResponseMessage DeleteAlertCondition(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = _token.GetBranchId,
                companyId = _token.GetCompanyId,
                createdBy = _token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = _repo.DeleteAlertCondition(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }
        #endregion


        [HttpGet]
        [ClaimsAuthorization]
        [Route("auto-assignment-loan-recovery")]
        public HttpResponseMessage GetAutoAssignmentOfLoanRecovery()
        {
           
                var alertViewModels = _alertRepo.MonthlyAutoAssignRecoveryAnalysisByCustomer();
                if (alertViewModels)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = alertViewModels, message = "Completed Successfully" });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error" });
                }
                
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("alert-place-holders")]
        public HttpResponseMessage GetAllAlertPlaceHolders()
        {
            try
            {
                var placeHolders = _repo.GetAllAlertPlaceHoldersy();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = placeHolders, count = placeHolders.Count() });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-placeholder")]
        public HttpResponseMessage AddAlertPlaceholder([FromBody] AlertPlaceHoldersViewModel entity)
        {
            try
            {

                entity.userBranchId = (short)_token.GetBranchId;
                entity.createdBy = _token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = _repo.AddAlertPlaceholder(entity);
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
        [Route("alert-placeholder-update/{placeHolderId}")]
        public HttpResponseMessage AddAlertPlaceholderUpdate(int placeHolderId, [FromBody] AlertPlaceHoldersViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)_token.GetBranchId;
                entity.createdBy = _token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                var data = _repo.AddAlertPlaceholderUpdate(placeHolderId,entity);
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

        [HttpPost]
        [ClaimsAuthorization]
        [Route("alert-binding-method")]
        public HttpResponseMessage AddAlertBindingMethod([FromBody] AlertBindingMethodsViewModel entity)
        {
            try
            {

                entity.userBranchId = (short)_token.GetBranchId;
                entity.createdBy = _token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var data = _repo.AddAlertBindingMethod(entity);
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
        [Route("alert-binding-method-update/{bindingMethodId}")]
        public HttpResponseMessage AddAlertBindingMethodUpdate(int bindingMethodId, [FromBody] AlertBindingMethodsViewModel entity)
        {
            try
            {
                entity.userBranchId = (short)_token.GetBranchId;
                entity.createdBy = _token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                var data = _repo.AddAlertBindingMethosUpdate(bindingMethodId, entity);
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

    }

}
