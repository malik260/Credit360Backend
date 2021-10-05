using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using FintrakBanking.APICore.JWTAuth;
using FintrakBanking.APICore.core;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.credit;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Setups.Approval;
using FintrakBanking.ViewModels.credit;
using FintrakBanking.Interfaces.WorkFlow;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/credit")] // TODO: modify!
    public class AtcLodgmentController : ApiControllerBase
    {
        private IAtcLodgmentRepository repo;
        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public AtcLodgmentController(IAtcLodgmentRepository _repo)
        {
            this.repo = _repo;
        }

        #region atcLodgment
        [HttpGet]
        [ClaimsAuthorization]
        [Route("atc-lodgment")]
        public HttpResponseMessage GetAtcLodgments()
        {
            IEnumerable<AtcLodgmentViewModel> response = repo.GetAtcLodgments(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("atc-lodgment-approval")]
        public HttpResponseMessage GetAtcLodgmentApproval()
        {
            IEnumerable<AtcLodgmentViewModel> response = repo.GetAtcLodgmentForApproval(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("atc-type")]
        public HttpResponseMessage GetAtcType()
        {
            IEnumerable<AtcLodgmentViewModel> response = repo.GetAtcType();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("atc-lodgment/{id}")]
        public HttpResponseMessage GetAtcLodgment(int id)
        {
            AtcLodgmentViewModel response = repo.GetAtcLodgment(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("atc-lodgment")]
        public HttpResponseMessage AddAtcLodgment([FromBody] AtcLodgmentViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddAtcLodgment(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("atc-lodgment-save-for-approval")]
        public HttpResponseMessage atcLodgmentForApproval([FromBody] IEnumerable<AtcLodgmentViewModel> model)
        {
            try
            {
                foreach (var atc in model)
                {
                    atc.userBranchId = (short)token.GetBranchId;
                    atc.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                    atc.applicationUrl = HttpContext.Current.Request.Path;
                    atc.createdBy = token.GetStaffId;
                    atc.companyId = token.GetCompanyId;
                }
                var response = repo.AtclodgmentApproval(model);
                if (response != null) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error Occurred, Please Contact the System Administartor" });
            }


            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        //[HttpPost]
        //[ClaimsAuthorization]
        //[Route("atc-lodgment-save-for-approval")]
        //public HttpResponseMessage atcLodgmentForApproval([FromBody] AtcLodgmentViewModel model)
        //{
        //    try
        //    {
        //        model.userBranchId = (short)token.GetBranchId;
        //        model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
        //        model.applicationUrl = HttpContext.Current.Request.Path;
        //        model.createdBy = token.GetStaffId;
        //        model.companyId = token.GetCompanyId;
        //        var response = repo.atclodgmentApproval(model);
        //        if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been Saved for Approval" });
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record/n This record may already be Processing" });
        //    }
        //    catch (SecureException ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //    }
        //}

        [HttpPost]
        [ClaimsAuthorization]
        [Route("atc-lodgement-final-approval")]
        public HttpResponseMessage SubmitLodgementApproval([FromBody] AtcLodgmentViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                WorkflowResponse response = repo.SubmitLodgementApproval(model);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                //return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("atc-lodgment/{id}")]
        public HttpResponseMessage UpdateAtcLodgment([FromBody] AtcLodgmentViewModel model, int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    createdBy = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };
                bool response = repo.UpdateAtcLodgment(model, id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("atc-lodgment/{id}")]
        public HttpResponseMessage DeleteAtcLodgment(int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    createdBy = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };
                bool response = repo.DeleteAtcLodgment(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("atc-type")]
        public HttpResponseMessage AddAtcType([FromBody] AtcTypeViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;
                var response = repo.AddAtcType(model);
                if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }



        [HttpDelete]
        [ClaimsAuthorization]
        [Route("atc-type/{id}")]
        public HttpResponseMessage DeleteAtcType(int id)
        {
            UserInfo user = new UserInfo()
            {
                BranchId = token.GetBranchId,
                companyId = token.GetCompanyId,
                createdBy = token.GetStaffId,
                applicationUrl = HttpContext.Current.Request.Path,
                userIPAddress = HttpContext.Current.Request.UserHostAddress
            };
            bool response = repo.DeleteAtcType(id, user);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
        }

        #endregion

        #region atcRelease


        [HttpGet]
        [ClaimsAuthorization]
        [Route("atc-lodgment-for-release")]
        public HttpResponseMessage GetAtcLodgmentForRelase()
        {
            IEnumerable<AtcLodgmentViewModel> response = repo.GetAtcLodgmentForRelease();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("atc-lodgment-for-releaseList")]
        public HttpResponseMessage GetAtcLodgmentForRelaselist()
        {
            IEnumerable<AtcLodgmentViewModel> response = repo.GetAtcLodgmentForReleaseList(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("atc-release/{id}")]
        public HttpResponseMessage GetAtcRelease(int id)
        {
            //IEnumerable<AtcReleaseViewModel> response = repo.GetAtcRelease(id);
            var response = repo.GetAtcRelease(id);
            if (response == null) return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = 1 });
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("atc-release")]
        public HttpResponseMessage AddAtcRelease([FromBody] IEnumerable<AtcReleaseViewModel> model)
        {
            try
            {
                foreach (var atc in model)
                {
                    atc.applicationUrl = HttpContext.Current.Request.Path;
                    atc.userBranchId = (short)token.GetBranchId;
                    atc.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                    atc.createdBy = token.GetStaffId;
                    atc.companyId = token.GetCompanyId;
                }
                var response = repo.AddAtcRelease(model);
                if (response != null) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "Error Occurred, Please Contact the System Administartor" });
            }

            //try { 
            //model.userBranchId = (short)token.GetBranchId;
            //model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
            //model.applicationUrl = HttpContext.Current.Request.Path;
            //model.createdBy = token.GetStaffId;
            //model.companyId = token.GetCompanyId;
            //var response = repo.AddAtcRelease(model);
            //if (response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "The record has been created successfully" });
            //return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error creating this record" });
            //}
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("atc-lodgment-release-approval")]
        public HttpResponseMessage GetAtcLodgmentsForApproval()
        {
            IEnumerable<AtcLodgmentViewModel> response = repo.GetAtcReleaseForApproval(token.GetStaffId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("atc-lodgment-release/{customerId}")]
        public HttpResponseMessage GetAtcLodgmentsByCustomerId(int customerId)
        {
            IEnumerable<AtcLodgmentViewModel> response = repo.GetAtcLodgmentsByCustomerId(customerId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("atc-release/{id}")]
        public HttpResponseMessage DeleteAtcRelease(int id)
        {
            try
            {
                UserInfo user = new UserInfo()
                {
                    BranchId = token.GetBranchId,
                    companyId = token.GetCompanyId,
                    createdBy = token.GetStaffId,
                    applicationUrl = HttpContext.Current.Request.Path,
                    userIPAddress = HttpContext.Current.Request.UserHostAddress
                };
                bool response = repo.DeleteAtcRelease(id, user);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = response, result = response, count = 1 });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ClaimsAuthorization]
        [Route("atc-referred-approval")]
        public HttpResponseMessage SubmitReferredAtcBackIntoWorkflow(AtcReleaseViewModel model)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                model.applicationUrl = HttpContext.Current.Request.Path;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.SubmitReferredAtcBackIntoWorkflow(model);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = response.responseMessage });
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, result = response, message = "An Error Occured, Please contact the System Administrator" });
                }
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
            
        }


        [HttpPost]
        [ClaimsAuthorization]
        [Route("atc-release-approval")]
        public HttpResponseMessage SubmitApproval([FromBody] IEnumerable<AtcReleaseViewModel>  model)
        {
            try
            {
                foreach(var mod in model)
                {
                    mod.userBranchId = (short)token.GetBranchId;
                    mod.userIPAddress = HttpContext.Current.Request.UserHostAddress;
                    mod.applicationUrl = HttpContext.Current.Request.Path;
                    mod.createdBy = token.GetStaffId;
                    mod.companyId = token.GetCompanyId;
                }
                var modelCount = model.Count();

                var response = repo.SubmitApproval(model);
                if (response != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = modelCount });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error processing this request" });
                //if ( response.Item2 == 0)
                //{
                //    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = $"Application Status: <strong>{response.Item1.statusName}</strong>, Sent to: {response.Item1.nextLevelName} <i>{response.Item1.nextPersonName}</i>" });
                //}
                //return Request.CreateResponse(HttpStatusCode.OK, new { success = true, message = $"{response.Item2}  transaction(s) has been < strong >{ response.Item1.statusName}</ strong > Successfully and  {(modelCount- response.Item2)}  Application Status: < strong >{ response.Item1.statusName}</ strong >, Sent to: { response.Item1.nextLevelName} < i >{ response.Item1.nextPersonName}</ i > " });

            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("save-edited-atc-release/{id}")]
        public HttpResponseMessage SaveEditedATCRelease([FromBody] AtcReleaseViewModel model, int id)
        {
            try
            {
                model.userBranchId = (short)token.GetBranchId;
                model.createdBy = token.GetStaffId;
                model.companyId = token.GetCompanyId;

                var response = repo.SaveEditedATCRelease(model, id);
                if(response) return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, message = "Record has been updated Successfully" });
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "There was an error updating this record" });


            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        #endregion

    }
}
