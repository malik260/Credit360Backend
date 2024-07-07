using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class ExposureRepository : IExposureRepository
    {
        private FinTrakBankingContext context;

        public ExposureRepository(
            FinTrakBankingContext context)
        {
            this.context = context;
        }


        public List<CurrentCustomerExposure> GetCurrentCustomerExposure(List<CustomerExposure> customer, int loanTypeId, int companyId)
        {
            IEnumerable<CurrentCustomerExposure> exposure = null;
            var customerId = customer.FirstOrDefault()?.customerId;
            var allGroupMappings = GetCustomerGroupMapping();
            List<CurrentCustomerExposure> exposures = new List<CurrentCustomerExposure>();

            if (loanTypeId == (int)LoanTypeEnum.CustomerGroup && customer.Count() == 1)
            {
                var customerGroupMappings = new List<CustomerGroupMappingViewModel>();
                var mappings = allGroupMappings.Where(m => m.customerGroupId == customerId).ToList();

                if (mappings.Count() > 0)
                {
                    customer = mappings.Select(m => new CustomerExposure { customerId = m.customerId }).ToList();
                }
            }
            else
            {
                var customerIsAGroupMember = allGroupMappings.Any(m => m.customerId == customerId);
                if (customerIsAGroupMember)
                {
                    var mappings = new List<CustomerGroupMappingViewModel>();
                    var customerGroups = allGroupMappings.Where(m => m.customerId == customerId).ToList();
                    var allGroupIds = customerGroups.Select(m => m.customerGroupId).Distinct().ToList();
                    foreach (var groupId in allGroupIds)
                    {
                        var mapping = allGroupMappings.Where(m => m.customerGroupId == groupId).ToList();
                        mappings.AddRange(mapping);
                    }
                    if (mappings.Count() > 0)
                    {
                        customer = mappings.Select(m => new CustomerExposure { customerId = m.customerId }).ToList();
                    }
                }
            }

            foreach (var item in customer)
            {
                var customerCode = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == item.customerId)?.CUSTOMERCODE.Trim();

                exposure = (from a in context.TBL_GLOBAL_EXPOSURE
                            where a.CUSTOMERID.Contains(customerCode)
                            select new CurrentCustomerExposure
                            {
                                customerName = a.CUSTOMERNAME,
                                customerCode = a.CUSTOMERID.Trim(),
                                facilityType = a.ADJFACILITYTYPE,
                                approvedAmount = a.LOANAMOUNYTCY ?? 0,
                                approvedAmountLcy = a.LOANAMOUNYLCY ?? 0,
                                currency = a.CURRENCYNAME,
                                currencyType = a.CURRENCYTYPE,
                                exposureTypeCodeString = a.EXPOSURETYPECODE,
                                adjFacilityTypeString = a.ADJFACILITYTYPE,
                                adjFacilityTypeCode = a.ADJFACILITYTYPEid.Trim(),
                                productIdString = a.PRODUCTID,
                                productCode = a.PRODUCTCODE,
                                productName = a.PRODUCTNAME,
                                currencyCode = a.ALPHACODE,
                                
                                outstandings = a.PRINCIPALOUTSTANDINGBALTCY ?? 0,
                                outstandingsLcy = a.PRINCIPALOUTSTANDINGBALLCY ?? 0,
                                pastDueObligationsPrincipal = a.TOTALUNPAIDOBLIGATION ?? 0,
                                reviewDate = DateTime.Now,
                                bookingDate = a.BOOKINGDATE,
                                maturityDate = a.MATURITYDATE,
                                tenorString = a.TENOR,
                                
                                loanStatus = a.CBNCLASSIFICATION,
                                referenceNumber = a.REFERENCENUMBER,
                            }).ToList();

                if (exposure.Count() > 0)
                {
                    foreach (var e in exposure)
                    {
                        e.exposureTypeId = int.Parse(e.exposureTypeCodeString);
                        e.tenor = int.Parse(String.IsNullOrEmpty(e.tenorString) ? "0" : e.tenorString);
                        e.bookingDate = e.bookingDate?.Date;
                        e.maturityDate = e.maturityDate?.Date;
                        //e.productId = int.Parse(e.productIdString);
                        e.exposureTypeCode = int.Parse(String.IsNullOrEmpty(e.exposureTypeCodeString) ? "0" : e.exposureTypeCodeString);
                        e.adjFacilityTypeId = int.Parse(String.IsNullOrEmpty(e.adjFacilityTypeCode) ? "0" : e.adjFacilityTypeCode);
                    }
                    exposures.AddRange(exposure);
                }


              
            }

            if (exposures.Count() == 0)
            {
                return exposures;
            }

           

            return exposures;
        }

        public IEnumerable<CustomerGroupMappingViewModel> GetCustomerGroupMapping()
        {
            var customerGroupMapping = (from a in context.TBL_CUSTOMER_GROUP_MAPPING
                                        where a.DELETED == false
                                        select new CustomerGroupMappingViewModel
                                        {
                                            customerGroupMappingId = a.CUSTOMERGROUPMAPPINGID,
                                            customerGroupId = a.CUSTOMERGROUPID,
                                            relationshipTypeId = a.RELATIONSHIPTYPEID,
                                            //createdBy = a.CreatedBy,
                                            customerId = a.CUSTOMERID,
                                            //dateTimeCreated = a.DateTimeCreated
                                        }).ToList();

            return customerGroupMapping;
        }
    }
}
