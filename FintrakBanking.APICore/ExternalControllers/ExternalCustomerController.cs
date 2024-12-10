using FintrakBanking.APICore.core;
using FintrakBanking.APICore.Filters;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.External;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.External.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace FintrakBanking.APICore.ExternalControllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [MiddlewareAuthorizeAttribute]
    [RoutePrefix("api/v1/third-party/customer")]
    public class ExternalCustomerController : ApiController
    {

        //private ICustomerRepository repo;
        private ICustomerRepositoryExternal repo;

        public ExternalCustomerController(ICustomerRepositoryExternal _repo)
        {
            this.repo = _repo;
        }



        //[HttpGet] 
        //[Route("iscustomer-exist/{customerCode}")]
        //public async Task<HttpResponseMessage> IsCustomerCodeExist(string customerCode)
        //{
        //    try
        //    {
        //        if (await repo.ValidateCustomerCodeAsync(customerCode))
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                              new
        //                              {
        //                                  success = true,
        //                                  status = true,
        //                                  message = $"Customer with code {customerCode} exist"
        //                              });
        //        }
        //        else
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                              new
        //                              {
        //                                  success = true,
        //                                  status = false,
        //                                  message = $"Customer with code {customerCode} does not exist"
        //                              });
        //        }
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpGet]
        [Route("iscustomer-exist/{customerCode}")]
        public HttpResponseMessage IsCustomerCodeExist(string customerCode)
        {
            try
            {
                if ( repo.ValidateCustomerCode(customerCode))
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                      new
                                      {
                                          success = true,
                                          status = true,
                                          message = $"Customer with code {customerCode} exist"
                                      });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                                      new
                                      {
                                          success = true,
                                          status = false,
                                          message = $"Customer with code {customerCode} does not exist"
                                      });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpPost]
        //[Route("update-customer")]
        //public async Task<HttpResponseMessage> UpdateCustomerInformation(UpdateCustomer entity)
        //{
        //    try
        //    {

        //        var data = await repo.UpdateCustomerAsync(entity);
        //        if (data != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = true, message = $"The customer information has been updated successfully ." });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = $"There was an error updating this this customer." });

        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        [Route("update-customer")]
        public HttpResponseMessage UpdateCustomerInformation(UpdateCustomer entity)
        {
            try
            {

                var data = repo.UpdateCustomer(entity);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, message = $"The customer information has been updated successfully ." });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error updating this this customer." });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        [Route("corporate-prospect")]
        public async Task<HttpResponseMessage> CreateCorporateProspectCustomer(ProspectCorporateCustomerForCreation customerForCreation)
        {
            try
            {

                var data = await repo.AddCorporateProspectCustomerAsync(customerForCreation);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The customer has been created successfully with prospect code {data}" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this this customer." });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [Route("corporate-existing")]
        public async Task<HttpResponseMessage> CreateCorporateExistingCustomer(ExistingCorporateCustomerForCreation customerForCreation)
        {
            try
            {

                var data = await repo.AddCorporateExistingCustomerAsync(customerForCreation);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The customer has been created successfully with code {data}" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this this customer." });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        //[HttpPost] 
        //[Route("kyc-document-upload")]
        //public async Task<HttpResponseMessage> KYCDocumentUpload()
        //{
        //    try
        //    {
        //        if (!Request.Content.IsMimeMultipartContent())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
        //        }

        //        MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
        //        await Request.Content.ReadAsMultipartAsync(provider);

        //        int uploadType;
        //        if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
        //        }

        //        var entity = new CustomerDocumentUploadViewModel
        //        {
        //            customerId = Convert.ToInt32(provider.FormData["customerId"]),
        //            customerCode = provider.FormData["customerCode"],
        //            documentTitle = provider.FormData["documentTitle"],
        //            documentTypeId = (short)uploadType,
        //            fileName = provider.FormData["fileName"],
        //            fileExtension = provider.FormData["fileExtension"],
        //            physicalFileNumber = provider.FormData["physicalFileNumber"],
        //            physicalLocation = provider.FormData["physicalLocation"],
        //        };

        //        if (!provider.FileStreams.Any())
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
        //        }

        //        entity.userBranchId = (short)token.GetBranchId;
        //        entity.companyId = token.GetCompanyId;
        //        entity.createdBy = token.GetStaffId;
        //        entity.applicationUrl = HttpContext.Current.Request.Path;

        //        var file = provider.Contents.FirstOrDefault();
        //        var buffer = await file.ReadAsByteArrayAsync();
        //        var data = repo.KYCDocumentUpload(entity, buffer);

        //        if (data)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
        //        }

        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
        //    }
        //    catch (ConditionNotMetException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error : {ex.Message}" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
        //    }
        //}


        [HttpGet]
        [Route("uploaded-kyc-documents/{customerCode}")]
        public async Task<HttpResponseMessage> GetKYCDocumentUploadByCustomerCode(string customerCode)
        {
            try
            { 
                var data = await repo.GetKYCDocumentUploadByCustomerCode(customerCode);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [Route("individual-prospect")]
        public async Task<HttpResponseMessage> CreateIndividualProspectCustomer(ProspectIndividualCustomerForCreation customerForCreation)
        {
            try
            {

                var data = await repo.AddIndividualProspectCustomerAsync(customerForCreation);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The customer has been created successfully with prospect code {data}" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this this customer." });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        //[HttpPost]
        //[Route("individual-existing")]
        //public async Task<HttpResponseMessage> CreateIndividualExistingCustomer(ExistingIndividualCustomerForCreation customerForCreation)
        //{
        //    try
        //    {

        //        var data = await repo.AddIndividualExistingCustomerAsync(customerForCreation);
        //        if (data != null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK,
        //                new { success = true, result = data, message = $"The customer has been created successfully with code {data}" });
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK,
        //           new { success = false, message = $"There was an error creating this this customer." });

        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        [Route("individual-existing")]
        public HttpResponseMessage CreateIndividualExistingCustomer(ExistingIndividualCustomerForCreation customerForCreation)
        {
            try
            {

                var data = repo.AddIndividualExistingCustomer(customerForCreation);
                if (data != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        new { success = true, result = data, message = $"The customer has been created successfully with code {data}" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"There was an error creating this this customer." });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("kyc-document-upload")]
        public async Task<HttpResponseMessage> KYCDocumentUpload()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                int uploadType;
                if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }

                var entity = new CustomerDocumentUploadViewModel();
                //customerId = Convert.ToInt32(provider.FormData["customerId"]),
                entity.customerCode = provider.FormData["customerCode"];
                entity.documentTitle = provider.FormData["documentTitle"];
                //int uploadType;
                //Int32.TryParse(provider.FormData["documentTypeId"], out uploadType);
                entity.documentTypeId = (short)uploadType;
                //entity.documentTypeId = (short)Convert.ToInt32(provider.FormData["documentTypeId"]);
                entity.fileName = provider.FormData["fileName"];
                entity.fileExtension = provider.FormData["fileExtension"];
                //physicalFileNumber = provider.FormData["physicalFileNumber"],
                //physicalLocation = provider.FormData["physicalLocation"], 


                //entity.userBranchId = (short)token.GetBranchId;
                //entity.companyId = token.GetCompanyId;
                //entity.createdBy = token.GetStaffId;
                //entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();
                var data = await repo.KYCDocumentUpload(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been uploaded successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error uploading this record" });
            }
            catch (ConditionNotMetException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error : {ex.Message}" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }
    }
}
