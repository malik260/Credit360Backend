using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using FintrakBanking.Common.CustomException;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/upload")]
    public class GenericDocumentUploadController : ApiController
    {
        TokenDecryptionHelper token = new TokenDecryptionHelper();
        private ILoanDocumentRepository repo;

        public GenericDocumentUploadController(ILoanDocumentRepository repo)
        {
            this.repo = repo;

        }

         [HttpPost] [ClaimsAuthorization]
        [Route("upload-document")]
        public async Task<HttpResponseMessage> UploadDocument()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, "Unsupported media type.");
                }

                MultipartFormDataMemoryStreamProvider provider = new MultipartFormDataMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                int uploadType;
                if (!Int32.TryParse(provider.FormData["documentTypeId"], out uploadType))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Upload Type is invalid.");
                }

                LoanDocumentViewModel entity = populateViewModel(provider, uploadType);

                if (!provider.FileStreams.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No file uploaded.");
                }

                entity.userBranchId = (short)token.GetBranchId;
                entity.companyId = token.GetCompanyId;
                entity.createdBy = token.GetStaffId;
                entity.applicationUrl = HttpContext.Current.Request.Path;

                var file = provider.Contents.FirstOrDefault();
                var buffer = await file.ReadAsByteArrayAsync();

                //file size
                int fileSize = buffer.Length;
                if (fileSize > (1048576))//max file size should come from database
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Exceed File Size. 3MB Maximum size is allowed");
                }

                var data = repo.uploadDocument(entity, buffer);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, message = "The record has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"There was an error creating this record {ex.InnerException}" });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("get-uploaded-document")]
        public async Task<HttpResponseMessage> GetUploadedDocument(LoanDocumentViewModel model)
        {
            try
            {
                var response = repo.getUploadedDocument(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }


         [HttpPost] [ClaimsAuthorization]
        [Route("get-all-document-uploads")]
        public async Task<HttpResponseMessage> GetAllUploadedDocument(LoanDocumentViewModel model)
        {
            try
            {
                var response = repo.getListOfUploadedDocument(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

         [HttpPost] [ClaimsAuthorization]
        [Route("delete-uploaded-document")]
        public async Task<HttpResponseMessage> DeleteUploadedDocument(LoanDocumentViewModel model)
        {
            try
            {
                var response = repo.DeleteUploadedDocument(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, error = ex.InnerException, message = ex.Message });
            }
        }

        private static LoanDocumentViewModel populateViewModel(MultipartFormDataMemoryStreamProvider provider, int uploadType)
        {
            var entity = new LoanDocumentViewModel
            {
                loanApplicationNumber = provider.FormData["loanApplicationNumber"],
                loanReferenceNumber = provider.FormData["loanReferenceNumber"],
                documentTitle = provider.FormData["documentTitle"],
                documentTypeId = (short)uploadType,
                fileName = provider.FormData["fileName"],
                fileExtension = provider.FormData["fileExtension"],
                physicalFileNumber = provider.FormData["physicalFileNumber"],
                physicalLocation = provider.FormData["physicalLocation"],
                SourceId = Convert.ToInt32( provider.FormData["SourceId"]),
                databaseTable = Convert.ToInt32(provider.FormData["databaseTable"]),
                conditionId = Convert.ToInt32(provider.FormData["conditionId"]),
                loanApplicationId = Convert.ToInt32(provider.FormData["loanApplicationId"]),
                checkListDefinitionId = Convert.ToInt32(provider.FormData["checkListDefinitionId"]),
                loanDetailId = Convert.ToInt32(provider.FormData["loanDetailId"]),
                collateralCustomerId = Convert.ToInt32(provider.FormData["collateralCustomerId"]),
                documentCode = provider.FormData["documentCode"],
                jobRequestCode = provider.FormData["jobRequestCode"],
                customerCode = provider.FormData["customerCode"],
                customerId = Convert.ToInt32(provider.FormData["customerId"]),
                staffCode = provider.FormData["staffCode"],
            };
            return entity;
        }
    }
}
