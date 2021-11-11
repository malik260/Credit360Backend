using FintrakBanking.APICore.core;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.Setups.Credit;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FintrakBanking.Common.CustomException;
using FintrakBanking.APICore.JWTAuth;
using System.Collections.Generic;
using FintrakBanking.ViewModels;
using System.Web;

namespace FintrakBanking.APICore.Controllers
{
    [RoutePrefix("api/v1/setups")]
    public class GeneralSetupController : ApiControllerBase
    {
        private IGeneralSetupRepository repo;
        private ICollateralTypeRepository collateralRepo;
        private IStaffRepository staffRepo;

        TokenDecryptionHelper token = new TokenDecryptionHelper();

        public GeneralSetupController(IGeneralSetupRepository _repo, ICollateralTypeRepository _collateralRepo, IStaffRepository _staffRepo)
        {
            this.repo = _repo;
            this.collateralRepo = _collateralRepo;
            this.staffRepo = _staffRepo;
        }

        #region General Setups

      [HttpGet] [ClaimsAuthorization]  
        [Route("calculate-maturity-date/effective-date/{effectiveDate}/tenor-mode/{tenorModeId}/tenor/{tenor}")]
        public HttpResponseMessage GetMaturityDate(DateTime effectiveDate, short tenorModeId, int tenor)
        {
            try
            {
                //var token = new TokenDecryptionHelper(this.HttpContext);
                var data = repo.CalculateMaturityDate(effectiveDate, (TenorModeEnum)tenorModeId, tenor);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = 1 });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = $"Error: {e.Message}" });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("tenor-mode")]
        public HttpResponseMessage GetAllTenorMode()
        {
            try
            {
                var data = repo.GetAllTenorMode();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("currency")]
        public HttpResponseMessage GetAllCurrency()
        {
            try
            {
                var data = repo.GetAllCurrency();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("loanApplicationReferance")]
        public HttpResponseMessage GetLoanApplicationRef()
        {
            try
            {
                var data = repo.GetLoanApplicationRef();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

       

      [HttpGet] [ClaimsAuthorization]  
        [Route("customer-type")]
        public HttpResponseMessage GetAllCustomerType()
        {
            try
            {
                var data = repo.GetAllCustomerType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


      [HttpGet] [ClaimsAuthorization]  
        [Route("deal-classification-type")]
        public HttpResponseMessage GetAllDealClassificationType()
        {
            try
            {
                var data = repo.GetAllDealClassificationType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        [ClaimsAuthorization]
        [Route("application-last-refreshed-date")]
        public HttpResponseMessage GetApplicationEODLastRefreshedDate()
        {
            try
            {
                var data = repo.GetApplicationEODLastRefreshedDate();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }


        [HttpGet] [ClaimsAuthorization]  
        [Route("application-date")]
        public HttpResponseMessage GetApplicaionDate()
        {
            try
            {
                var data = repo.GetApplicationDate();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }
      [HttpGet] [ClaimsAuthorization]  
        [Route("sector")]
        public HttpResponseMessage GetSector()
        {
            try
            {
                var data = repo.GetSector();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("subsector")]
        public HttpResponseMessage GetSubsector()
        {
            try
            {
                var data = repo.GetSubsector();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("day-count")]
        public HttpResponseMessage GetAllDayCount()
        {
            try
            {
                var data = repo.GetAllDayCount();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });//Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("fee-amortisation-type")]
        public HttpResponseMessage GetAllFeeAmortisationType()
        {
            try
            {
                var data = repo.GetAllFeeAmortisationType();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("deal-types")]
        public HttpResponseMessage GetAllDealTypes()
        {
            try
            {
                var data = repo.GetAllDealTypes();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("fs-types")]
        public HttpResponseMessage GetAllFSTypes()
        {
            try
            {
                var data = repo.GetAllFSTypes();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }

        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("frequency-types")]
        public HttpResponseMessage GetAllFrequencyTypes()
        {
            try
            {
                var data = repo.GetAllFrequencyTypes();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("operation-types")]
        public HttpResponseMessage GetAllOperationTypes()
        {
            try
            {
                var data = repo.GetAllOperationTypes();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("operation")]
        public HttpResponseMessage GetAllOperations()
        {
            try
            {
                var data = repo.GetAllOperations();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() }); //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("operation/{operationTypeId}")]
        public HttpResponseMessage GetOperations(short operationTypeId)
        {
            try
            {
                var data = repo.GetOperations(operationTypeId);
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("collateral-types")]
        public HttpResponseMessage GetAllCollateralTypes()
        {
            try
            {
                var data = collateralRepo.GetCollateralTypes();
                if (data == null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });  //Ok(accounts);
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("sectors")]
        public HttpResponseMessage GetAllSectors()
        {
            try
            {
                var data = repo.GetAllSectors();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("global-sectors")]
        public HttpResponseMessage GetAllGlobalSectors()
        {
            try
            {
                var data = repo.GetAllGlobalSectors();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }



        [HttpPost]
        [ClaimsAuthorization]
        [Route("sectors")]
        public HttpResponseMessage AddSector([FromBody] SectorViewModel model)
        {
            try
            {
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                var data = repo.AddSector(model);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, message = "Sector has been created successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "Sector has not been created successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut] [ClaimsAuthorization]
        [Route("sectors/{id}")]
        public HttpResponseMessage UpdateSector([FromBody] SectorViewModel model ,short id)
        {
            try
            {
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.Path;
                var data = repo.UpdateSector( model, id);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, message = "Sector has been updated successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "Sector has not been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [ClaimsAuthorization]
        [Route("global-sectors/{id}")]
        public HttpResponseMessage UpdateGlobalSector([FromBody] GlobalSectorViewModel model, int id)
        {
            try
            {
                model.companyId = token.GetCompanyId;
                model.createdBy = token.GetStaffId;
                model.userBranchId = (short)token.GetBranchId;
                model.userIPAddress = HttpContext.Current.Request.Path;
                var data = repo.UpdateGlobalSector(model, id);

               return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, message = "Global Sector limit has been updated successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [ClaimsAuthorization]
        [Route("sectors/{id}")]
        public HttpResponseMessage DeleteSector(int id)
        {
            try
            {
                UserInfo user = new UserInfo();
                user.companyId = token.GetCompanyId;
                user.staffId = token.GetStaffId;
                user.BranchId = (short)token.GetBranchId;
                user.userIPAddress = HttpContext.Current.Request.Path;
                var data = repo.DeleteSector(id, user);

                if (data)
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, message = "Sector has been deleted successfully" });
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = "Sector has not been deleted successfully" });
            }
            catch (SecureException ex)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [ClaimsAuthorization]
        [Route("region/type/{regionTypeId}")]
        public HttpResponseMessage RegionByType(int regionTypeId)
        {
            var data = repo.GetRegionByType(regionTypeId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data, count = data.Count() });
        }

        [HttpGet] [ClaimsAuthorization]  
        [Route("subsector/{subSectorId}/sectors")]
        public HttpResponseMessage GetAllSectorsBySubSectorId(short subSectorId)
        {
            try
            {
                var data = repo.GetSectorsBySubSectorId(subSectorId);
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

      [HttpGet] [ClaimsAuthorization]  
        [Route("subsectors")]
        public HttpResponseMessage GetAllSubSectors()
        {
            try
            {
                var data = repo.GetAllSubSectors();
                if (!data.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                       new { success = false, message = "No record found" });
                }
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = true, result = data, count = data.Count() });
            }
            catch (SecureException e)
            {
                return Request.CreateResponse(HttpStatusCode.OK,
                   new { success = false, message = $"Error: {e.Message}" });
            }
        }

        


        //[HttpGet]
        //[Route("casa/account-status")]
        //public HttpResponseMessage GetCasaAccountStatus()
        //{
        //     
        //     
        //    {
        //        try
        //        {
        //            var data = repo.GetCasaAccountStatus();
        //            if (data == null)
        //            {
        //                return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = "No record found" });
        //            }
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = data.ToList() });
        //        }
        //        catch (SecureException ex)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.OK, new { success = false, message = ex.Message });
        //        }
        //         
        //    });
        //}




        //[HttpGet("productgroups", Name = "GroupGet")]
        //public IActionResult GetProductGroup()
        //{
        //    try
        //    {
        //        var productGroups = repo.GetAllProductGroup();
        //        return Ok(productGroups);
        //    }
        //    catch (SecureException ex)
        //    {
        //        return BadRequest(new { error = true, message = ex.Message });
        //    }
        //}

        //[HttpPost("productgroup/add")]
        //public IActionResult SaveProductGroup([FromBody]ProductGroupViewModel model)
        //{
        //    try
        //    {
        //        if (repo.SaveProductGroup(model))
        //        {
        //            return Created("", model);
        //        }
        //    }
        //    catch (SecureException ex)
        //    {
        //        return BadRequest();
        //    }

        //    return BadRequest();
        //}

        #endregion General Setups


        [HttpGet]
        [ClaimsAuthorization]
        [Route("profile-business-unit")]
        public HttpResponseMessage GetProfileBusinessUnits()
        {
            IEnumerable<ProfileBusinessUnitViewModel> response = repo.GetProfileBusinessUnits();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true, result = response, count = response.Count() });
        }
    }

}