using FintrakBanking.ViewModels.External.Customer;
using FintrakBanking.ViewModels.External.Document;
using FintrakBanking.ViewModels.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.External
{
    public interface ICustomerRepositoryExternal
    {
        Task<bool> ValidateCustomerCodeAsync(string customerCode);
        bool ValidateCustomerCode(string customerCode);
        Task<string> UpdateCustomerAsync(UpdateCustomer entity);
        string UpdateCustomer(UpdateCustomer entity);
        Task<string> AddCorporateProspectCustomerAsync(ProspectCorporateCustomerForCreation entity);
        Task<string> AddCorporateExistingCustomerAsync(ExistingCorporateCustomerForCreation entity);
        Task<List<CustomerUploadedDocumentForReturn>> GetKYCDocumentUploadByCustomerCode(string customerCode);
        Task<string> AddIndividualProspectCustomerAsync(ProspectIndividualCustomerForCreation entity);
        Task<string> AddIndividualExistingCustomerAsync(ExistingIndividualCustomerForCreation entity);
        string AddIndividualExistingCustomer(ExistingIndividualCustomerForCreation entity);
        Task<bool> KYCDocumentUpload(CustomerDocumentUploadViewModel model, byte[] file);


    }
}
