using System;
using System.Threading.Tasks;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.APICore.JWTAuth;
using System.Web.Http;
using System.Net.Http;
using System.Web;
using FintrakBanking.APICore.core;
using System.Net;
using FintrakBanking.Interfaces.Setups.Finance;
using System.Web.Http.Cors;
using FintrakBanking.Interfaces.ErrorLogger;
using System.Linq;
using FintrakBanking.Interfaces.Setups.Credit;
using System.Data.SqlClient;
using System.IO;
using FintrakBanking.ViewModels.WorkFlow;
using System.Collections.Generic;
using System.Globalization;
using FintrakBanking.Common;

namespace FintrakBanking.APICore.Controllers
{

    [RoutePrefix("api/v1/credit")]
    public class CustomerCollateralController : ApiControllerBase
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        private ICustomerCollateralRepository repo;
        private ICollateralDocumentRepository document;
        private ICollateralTypeRepository type;

        public CustomerCollateralController(
            ICustomerCollateralRepository repo,
            ICollateralTypeRepository type,
            ICollateralDocumentRepository document
            )
        {
            this.repo = repo;
            this.type = type;
            this.document = document;
        }

        #region New

        [HttpPost, Route("customer-collateral")]
        public async Task<HttpResponseMessage> AddCollateral([FromBody] CollateralViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = await repo.AddCollateral(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPut, Route("customer-collateral/{collateralId}")]
        public async Task<HttpResponseMessage> UpdateCollateral([FromBody] CollateralViewModel entity, int collateralId)
        {
            try
            {
                entity.lastUpdatedBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = await repo.UpdateCollateral(entity, collateralId);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Collateral Updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpGet, Route("customer-collateral/customer/{customerId}/application/{applicationId}")]
        public HttpResponseMessage GetCustomerCollateral(int customerId, int? applicationId)
        {
            try
            {
                var response = repo.GetCustomerCollateral(customerId, applicationId, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }



        //[HttpGet, Route("customer-collateral/customer/{customerId}")]
        //public HttpResponseMessage GetCustomerCollateralFiltered(int customerId)
        //{
        //    try
        //    {
        //        var response = repo.GetCustomerCollateral(customerId, token.GetCompanyId);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
        //    }
        //}

        [HttpGet, Route("customer-collateral/customer/{customerId}/collateral-type/{collateralTypeId}/thirdparty/{thirdpartyCustomerId}")]
        public HttpResponseMessage GetCollateralByCollateralTypeIdByCustomerId(int customerId, short collateralTypeId, short thirdpartyCustomerId = 0)
        {
            try
            {
                var response = repo.GetCollateralByCollateralTypeIdByCustomerId(token.GetCompanyId, collateralTypeId, customerId, thirdpartyCustomerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/type/collateral/{collateralId}/type/{typeId}")]
        public HttpResponseMessage GetCollateralTypeByCollateralId(int collateralId, int typeId)
        {
            try
            {
                var response = repo.GetCollateralTypeByCollateralId(collateralId, typeId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("collateral-document/{collateralId}")]
        public HttpResponseMessage GetCollateralDocumentByCollateral(int collateralId)
        {
            try
            {
                var data = document.GetCustomerCollateralDocument(collateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("loan-visitation-file/{documentId}")]
        public HttpResponseMessage GetVisitationFile(int documentId)
        {
            try
            {
                var data = document.GetCollateralVisitationDocument(documentId); //CollateralVisitationDocumentViewModel

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [Route("loan-visitation/{collateralVisitationId}")]
        public HttpResponseMessage GetVisitationDocument(int collateralVisitationId)
        {
            try
            {
                var data = repo.GetPropertyVistation(collateralVisitationId);

                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("visitation-document")]
        public async Task<HttpResponseMessage> AddVisitationDocument( )
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                int collateralCustomerId;
                if (!Int32.TryParse(provider.FormData["collateralCustomerId"], out collateralCustomerId))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }

              var visitationDate =  provider.FormData["lastVisitaionDate"];

                var actualDate = visitationDate.Substring(0, 15);
                var dateVisited = DateTime.ParseExact(actualDate, "ddd MMM dd yyyy", CultureInfo.InvariantCulture);

                var entity = new CollateralDocumentViewModel
                {
                    lastVisitaionDate = dateVisited, 
                    visitationRemark = provider.FormData["visitationRemark"],
                    collateralCustomerId = Convert.ToInt32(provider.FormData["collateralCustomerId"]),
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                };

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.collateralCustomerId = collateralCustomerId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = document.AddCollateralVisitation(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpPost, Route("collateral-visitation")]
        public HttpResponseMessage AddCollateralVisitation([FromBody] CollateralDocumentViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response =  repo.AddPropertyVistation(entity);
                if (response>0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPost, Route("collateral-document")]
        public async Task<HttpResponseMessage> AddCollateralDocument()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                int collateralId;
                if (!Int32.TryParse(provider.FormData["collateralId"], out collateralId))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }

                var entity = new CollateralDocumentViewModel
                {
                    documentTitle = provider.FormData["documentTitle"], // document code
                    fileName = provider.FormData["fileName"],
                    fileExtension = provider.FormData["fileExtension"],
                };

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.collateralId = collateralId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = document.AddCollateralDocument(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

        [HttpGet, Route("customer-collateral/loan/{loanId}")]
        public HttpResponseMessage GetLoanCollateral(int loanId)
        {
            try
            {
                var response = repo.GetLoanCollateral(loanId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/active/{customerId}")]
        public HttpResponseMessage GetActiveCustomerCollateral(int customerId)
        {
            try
            {
                var response = repo.GetActiveCustomerCollateral(customerId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/release/{mappingId}")]
        public HttpResponseMessage ReleaseCollateral(int mappingId)
        {
            try
            {
                GeneralEntity userInfo = new GeneralEntity()
                {
                    createdBy = token.GetStaffId,
                    companyId = token.GetCompanyId,
                    userBranchId = (short)token.GetBranchId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress,
                };

                var response = repo.ReleaseCollateral(mappingId, token.GetStaffId, userInfo);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("collateral-release/pending-approval")]
        public HttpResponseMessage GetPendingCustomerCollateralRelease()
        {
            try
            {
                var response = repo.GetPendingCustomerCollateralRelease();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("customer-collateral/release-approval")]
        public HttpResponseMessage ApproveCollateralRelease([FromBody] ApprovalViewModel entity)
        {
            try
            {
                GeneralEntity userInfo = new GeneralEntity()
                {
                    createdBy = token.GetStaffId,
                    companyId = token.GetCompanyId,
                    userBranchId = (short)token.GetBranchId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress,
                };
                var response = repo.ApproveCollateralRelease(entity, token.GetStaffId, userInfo);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("customer-collateral/assignment")]
        public HttpResponseMessage AssignCollateral([FromBody] ActiveCustomerCollateralViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.companyId = token.GetCompanyId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;

                var response = false; // repo.AssignCollateral(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("customer-collateral/search/")]
        public HttpResponseMessage SearchStaff(string queryString)
        {
            try
            {
                var data = repo.SearchCollateral(queryString, token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK,  new { success = true, result = data.ToList() });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }

        }

        #endregion New

        #region Collateral 

        //[HttpPost]
        //[Route("customer-collateral")]
        //public async Task<HttpResponseMessage> AddCollateral([FromBody] CollateralCustomerViewModel entity)
        //{
        //    try
        //    {
        //        TokenDecryptionHelper token = new TokenDecryptionHelper();

        //        entity.createdBy = token.GetStaffId;
        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        //entity.userIPAddress = HttpContext.Current.Request.UserHostAddress;
        //        entity.companyId = token.GetCompanyId;

        //        var response = await repo.AddCollateralCustomer(entity);
        //        if (response)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
        //    }
        //    catch (Exception ex)
        //    {
        //        //this.errorLogger.LogError(ex, HttpContext.Current.Request.Path, token.GetUsername);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpPut]
        //[Route("customer-collateral/{collateralCustomerId}")]
        //public async Task<HttpResponseMessage> UpdateCustomCollateral(int collateralCustomerId, [FromBody] CollateralCustomerViewModel entity)
        //{

        //    try
        //    {
        //        entity.lastUpdatedBy = token.GetStaffId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;
        //        entity.userBranchId = (short)token.GetBranchId;

        //        var response = await repo.UpdateCollateralCustomer(collateralCustomerId, entity);
        //        if (!response)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}


        #endregion

        #region Collatera Types

        [HttpGet]
        [Route("collateral-type")]
        public HttpResponseMessage GetCollateralType()
        {
            try
            {
                var response = type.GetCollateralTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-type/loan-application/{id}")]
        public HttpResponseMessage GetCollateralTypeByLoanApplicationId(int? id)
        {
            try
            {
                var response = type.CollateralTypesByLoanApplication(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-sub-type")]
        public HttpResponseMessage GetCollateralSubTypes()
        {
            try
            {
                var response = type.GetCollateralSubTypes();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        [Route("collateral-sub-type/{id}")]
        public HttpResponseMessage GetCollateralSubTypes(int id)
        {
            try
            {
                var response = type.CollateralSubType(id);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-sub-type/collateral-type/{collateralTypeId}")]
        public HttpResponseMessage GetCollateralSubTypeByCollateralTypeId(short collateralTypeId)
        {
            try
            {
                var response = type.GetCollateralSubTypeByCollateralTypeId(collateralTypeId);
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [Route("collateral-sub-type")]
        public async Task<HttpResponseMessage> AddCollateralSubType([FromBody] CollateralSubTypeViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = await type.AddCollateralSubTypes(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPut]
        [Route("collateral-type/{collateralTypeId}")]
        public async Task<HttpResponseMessage> UpdateCollateralType(short collateralTypeId, [FromBody] CollateralTypeViewModel entity)
        {
            try
            {
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;

                var response = await type.UpdateCollateralTypes(collateralTypeId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Record created sussessfully", result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("collateral-sub-type/{collateralSubTypeId}")]
        public async Task<HttpResponseMessage> UpdateCollateralSubType(short collateralSubTypeId, [FromBody] CollateralSubTypeViewModel entity)
        {
            try
            {
                entity.lastUpdatedBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.userBranchId = (short)token.GetBranchId;

                var response = await type.UpdateCollateralSubTypes(collateralSubTypeId, entity);
                if (!response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = "Update successful", result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion  End of Collateral Types

        #region Seniority Of Claims
        [HttpGet]
        [Route("collateral-seniority-of-claims")]
        public HttpResponseMessage GetCollateralSeciorityOfClaims()
        {
            try
            {
                TokenDecryptionHelper token = new TokenDecryptionHelper();

                var response = repo.GetCollateralSeniorityOfClaims();
                if (response == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
        #endregion Seniority Of Claims

        [HttpGet]
        [Route("collateral-valuer")]
        public HttpResponseMessage GetCollateralValuers()
        {
            try
            {
                var response = repo.GetCollateralValuer(token.GetCompanyId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-perfection-status")]
        public HttpResponseMessage GetCollateralPerfectionStatus()
        {
            try
            {
                var response = repo.GetCollateralPerfectionStatus();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (System.Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-valuer-type")]
        public HttpResponseMessage GetCollateralValuerType()
        {
            try
            {
                var response = repo.GetCollateralValuerType();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-value-base-type/{collateralType}")]
        public HttpResponseMessage GetCollateralValueBaseType(short collateralType)
        {
            try
            {
                var response = repo.GetCollateralValueBaseType(collateralType);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost, Route("collateral-valuer")]
        public async Task<HttpResponseMessage> AddCollateralValuer([FromBody] CollateralValuersViewModel entity)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = await repo.AddCollateralValuer(entity);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        [HttpPut, Route("collateral-valuer/{id}")]
        public async Task<HttpResponseMessage> UpdateCollateralValuer([FromBody] CollateralValuersViewModel entity, int id)
        {
            try
            {
                entity.createdBy = token.GetStaffId;
                entity.userBranchId = (short)token.GetBranchId;
                entity.applicationUrl = HttpContext.Current.Request.Path;
                entity.companyId = token.GetCompanyId;

                var response = await repo.UpdateCollateralValuer(entity, id);
                if (response)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "An unknown error has occured" });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message, error = ex.InnerException });
            }
        }

        //[HttpGet, Route("unmapped-collateral-application/customer/{customerId}/loanapplication/{loanapplicationid}")]
        //public HttpResponseMessage GetAllUnmappedCustomerCollateral(int customerId, int loanApplicationId)
        //{
        //    try
        //    {
        //        var response = repo.GetAllUnmappedCustomerCollateral(customerId, loanApplicationId , token.GetCompanyId );
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
        //    }
        //}

        //[HttpGet, Route("mapped-collateral-application/customer/{customerId}/loanapplication/{loanapplicationid}")]
        //public HttpResponseMessage GetAllMappedCustomerCollateral(int customerId, int loanApplicationId)
        //{
        //    try
        //    {
        //        var response = repo.GetAllMappedCustomerCollateral(customerId, loanApplicationId, token.GetCompanyId);
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
        //    }
        //}

        [HttpPost, Route("application-collateral/map")]
        public HttpResponseMessage MapApplicationCollateral([FromBody] ApplicationCollateralMapping entity)
        {
            try
            {

                entity.staffId = token.GetStaffId;

                var response = repo.MapApplicationCollateral(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost, Route("application-collateral/unmap")]
        public HttpResponseMessage UnmapApplicationCollateral([FromBody] ApplicationCollateralMapping entity)
        {
            try
            {
                var response = repo.UnmapApplicationCollateral(entity);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet, Route("collateral-information-view/{customercollateralId}")]
        public HttpResponseMessage GetCollateralInformationById(int customercollateralId)
        {
            try
            {
                var response = repo.GetCollateralInformationById(customercollateralId);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("get-fixeddeposit-lien-amount")]
        public HttpResponseMessage GetLienAmountForFD([FromBody]string accountNumber)
        {
            try
            {
                var response = repo.GetAccountLeinAmountForFD(accountNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("get-casa-lien-amount")]
        public HttpResponseMessage GetLienAmountForCASA([FromBody]string accountNumber)
        {
            try
            {
                var response = repo.GetAccountLeinAmountForCASA(accountNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("collateral-history/{collateralID}")]
        public HttpResponseMessage GetCollateralHistory(short collateralID)
        {
            try
            {
                var response = repo.getCollateralHistory(collateralID);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("stock-price")]
        public HttpResponseMessage GetStockPrice()
        {
            try
            {
                var response = repo.getStockPrice();
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
    }
}

