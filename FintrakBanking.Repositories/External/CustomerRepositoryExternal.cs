using FintrakBanking.Common;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.External;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.External.Customer;
using FintrakBanking.ViewModels.Customer;
using FintrakBanking.ViewModels.External.Document;
using FinTrakBanking.ThirdPartyIntegration;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities.DocumentModels;

namespace FintrakBanking.Repositories.External
{
    public class CustomerRepositoryExternal : ICustomerRepositoryExternal
    {
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository _genSetup;
        private IIntegrationWithFinacle finacle;
        //private IntegrationWithFinacle integration;

        bool USE_THIRD_PARTY_INTEGRATION;

        public CustomerRepositoryExternal(IAuditTrailRepository auditTrail, IGeneralSetupRepository genSetup, IIntegrationWithFinacle finacle) //, IntegrationWithFinacle _integration) //IntegrationWithFinacle _integration)
        {
            this.auditTrail = auditTrail;
            _genSetup = genSetup;
            this.finacle = finacle;
            //this.integration = _integration;

            using (var context = new FinTrakBankingContext())
            {
                var global = context.TBL_SETUP_GLOBAL.FirstOrDefault();
                if (global != null) this.USE_THIRD_PARTY_INTEGRATION = Convert.ToBoolean(global.USE_THIRD_PARTY_INTEGRATION);
            } 
        }

        public async Task<bool> ValidateCustomerCodeAsync(string customerCode)
        {
            using (var context = new FinTrakBankingContext())
            {
                if (await context.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == customerCode).AnyAsync())
                    return true;
                else
                    return false;
            }
        }

        public bool ValidateCustomerCode(string customerCode)
        {
            using (var context = new FinTrakBankingContext())
            {
                if (context.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == customerCode).Any())
                    return true;
                else
                    return false;
            }
        }

        public async Task<string> UpdateCustomerAsync(UpdateCustomer entity)
        {
            using (FinTrakBankingContext dbContext = new FinTrakBankingContext())
            {
                using (var trans = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // confirm the customer exist
                        var customer = await dbContext.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == entity.customerCode).FirstOrDefaultAsync();

                        if (customer == null)     throw new ConditionNotMetException($"This customer does not exist.");

                        if (entity.maritalStatus > 0)
                        {
                            if (entity.maritalStatus.GetType() != typeof(int) || entity.maritalStatus > 4 || entity.maritalStatus < 1)
                                throw new ConditionNotMetException($"Marital status must be a number between 1 and 4 for Single, Married, Divorced or Widowed respectively.");
                        }

                        //customer.CUSTOMERTYPEID = (int)(CustomerTypeEnum)entity.customerTypeId;
                        if (entity.maritalStatus > 0 && entity.maritalStatus <= 4)
                        {
                            customer.MARITALSTATUS = entity.maritalStatus;
                        }                        
                        customer.DATETIMEUPDATED = DateTime.Now;                         
                        customer.EMAILADDRESS = entity.emailAddress;
                        customer.CUSTOMERNIN = entity.customerNin;
                        if (entity.monthlyIncome > 0)
                        {
                            customer.MONTHLYINCOME = entity.monthlyIncome;
                        }
                        //if (entity.dateOfEmployment > DateTime.Now)
                        //{
                        //    customer.DATEOFEMPLOYMENT = entity.dateOfEmployment;
                        //}                       
                        if (entity.dateOfEmployment < DateTime.Now)
                        {
                            customer.DATEOFEMPLOYMENT = entity.dateOfEmployment;
                        }

                        customer.CUSTOMERBVN = entity.customerBVN;
                        //if (entity.dateOfBirth > DateTime.Now)
                        //{
                        //    customer.DATEOFBIRTH = entity.dateOfBirth;
                        //}

                        if (entity.dateOfBirth < DateTime.Now)
                        {
                            customer.DATEOFBIRTH = entity.dateOfBirth;
                        }

                        if (entity.subSectorId > 0)
                        {
                            var subSectors = await dbContext.TBL_SUB_SECTOR.Select(s => s.SUBSECTORID).ToListAsync();
                            if (subSectors != null)
                            {
                                if (subSectors.Contains((short)entity.subSectorId))
                                {
                                    customer.SUBSECTORID = (short)entity.subSectorId;
                                }
                                else
                                {
                                    throw new ConditionNotMetException($"Please enter a valid sub-sector Id");
                                }
                            }
                        }


                        //if (!string.IsNullOrEmpty(entity.otherBankAccount))
                        //{
                        //    if(string.IsNullOrEmpty(customer.OTHERBANKACCOUNT2))
                        //    {
                        //        customer.OTHERBANKACCOUNT2 = customer.OTHERBANKACCOUNT;
                        //        customer.OTHERBANKSORTCODE2 = customer.OTHERBANKSORTCODE;
                        //        customer.OTHERBANKACCOUNT = entity.otherBankAccount;
                        //        customer.OTHERBANKSORTCODE = entity.otherBankSortCode;

                        //    }
                        //    else if( !string.IsNullOrEmpty(customer.OTHERBANKACCOUNT2) )
                        //    {
                        //        customer.OTHERBANKACCOUNT3 = customer.OTHERBANKACCOUNT;
                        //        customer.OTHERBANKSORTCODE3 = customer.OTHERBANKSORTCODE;
                        //        customer.OTHERBANKACCOUNT = entity.otherBankAccount;
                        //        customer.OTHERBANKSORTCODE = entity.otherBankSortCode;
                        //    }
                        //}

                        if (!string.IsNullOrEmpty(entity.otherBankAccount))
                        {
                            customer.OTHERBANKACCOUNT = entity.otherBankAccount;
                            customer.OTHERBANKSORTCODE = entity.otherBankSortCode;
                        }

                        var updatePhoneContact = await dbContext.TBL_CUSTOMER_PHONECONTACT.Where(p => p.CUSTOMERID == customer.CUSTOMERID).FirstOrDefaultAsync();
                        if (entity.mobilePhoneNo != null || entity.officeLandNo != null || updatePhoneContact != null) 
                        {
                            updatePhoneContact.PHONENUMBER = entity.mobilePhoneNo;
                            updatePhoneContact.PHONE = entity.officeLandNo;
                        }
                        else if (entity.mobilePhoneNo != null || entity.officeLandNo != null || updatePhoneContact == null)
                        {
                            var phoneContact = new TBL_CUSTOMER_PHONECONTACT();
                            phoneContact.PHONENUMBER = entity.mobilePhoneNo;
                            phoneContact.PHONE = entity.officeLandNo;

                            dbContext.TBL_CUSTOMER_PHONECONTACT.Add(phoneContact);
                        }
                        //if (entity.employerNumber != null || entity.employerNumber != "")
                        //{
                        //   customer.EMPLOYERNUMBER = entity.employerNumber;
                        //}
                        if (entity.customerTypeId > 0)
                        {
                            var customerTypes = await dbContext.TBL_CUSTOMER_TYPE.Select(c => c.CUSTOMERTYPEID).ToListAsync();
                            if (!customerTypes.Contains((short)entity.customerTypeId))
                            {
                                throw new SecureException("Customer Type for this customer is not valid, Kindly update from core-banking");
                            }
                            customer.CUSTOMERTYPEID = (short)entity.customerTypeId;
                        }
                        //if (customer.ISBVNVALIDATED != entity.isBvnValidated)
                        //{
                        //    customer.ISBVNVALIDATED = entity.isBvnValidated;
                        //}
                        if (customer.TITLE != null || entity.title != "") {
                            customer.TITLE = entity.title;
                        }
                        if (customer.GENDER != null || entity.gender != "")
                        {
                            customer.GENDER = entity.gender;
                        }

                        if (customer.FIRSTNAME != null || entity.firstName != "")
                        {
                            customer.FIRSTNAME = entity.firstName;
                        }
                        if (customer.MIDDLENAME != null || entity.middleName != "")
                        {
                            customer.MIDDLENAME = entity.middleName;
                        }
                        if (customer.LASTNAME != null || entity.lastName != "")
                        {
                            customer.LASTNAME = entity.lastName;
                        }

                        // save changes
                        var updateSuccess = await dbContext.SaveChangesAsync() > 0;

                        if (updateSuccess)
                        {
                            try
                            {
                                // initiate auit table
                                var audit = new TBL_AUDIT
                                {
                                    AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                                    STAFFID = 1,
                                    BRANCHID = (short)customer.BRANCHID,
                                    DETAIL = $"(External) Updated Customer  {customer.TBL_COMPANY.NAME} with Code: {customer.CUSTOMERCODE} ",
                                    //IPADDRESS = entity.userIPAddress,
                                    //URL = entity.applicationUrl,
                                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                                    SYSTEMDATETIME = DateTime.Now
                                };

                                // add the audit trail
                                auditTrail.AddAuditTrail(audit);

                                var output = await dbContext.SaveChangesAsync() > 0;
                                trans.Commit();
                                trans.Dispose();


                                var result = "Customer Information Updated";

                                if (output == true)
                                {
                                    return result;
                                }
                                else
                                {
                                    return null;
                                }

                            }
                            catch (DbEntityValidationException ex)
                            {
                                trans.Rollback();

                                string errorMessages = string.Join("; ",
                                ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                                throw new DbEntityValidationException(errorMessages);
                            }
                        }

                                
                       
                    }
                    catch (Exception exp)
                    {
                        trans.Rollback();
                        throw new SecureException(exp.Message);
                    }

                    return null;
                }

            }
            
        }

        public string UpdateCustomer(UpdateCustomer entity)
        {
            using (FinTrakBankingContext dbContext = new FinTrakBankingContext())
            {
                using (var trans = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // confirm the customer exist
                        var customer = dbContext.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == entity.customerCode).FirstOrDefault();

                        if (customer == null) throw new ConditionNotMetException($"This customer does not exist.");

                        if (entity.maritalStatus > 0)
                        {
                            if (entity.maritalStatus.GetType() != typeof(int) || entity.maritalStatus > 4 || entity.maritalStatus < 1)
                                throw new ConditionNotMetException($"Marital status must be a number between 1 and 4 for Single, Married, Divorced or Widowed respectively.");
                        }

                        //customer.CUSTOMERTYPEID = (int)(CustomerTypeEnum)entity.customerTypeId;
                        if (entity.maritalStatus > 0 && entity.maritalStatus <= 4)
                        {
                            customer.MARITALSTATUS = entity.maritalStatus;
                        }
                        customer.DATETIMEUPDATED = DateTime.Now;
                        customer.EMAILADDRESS = entity.emailAddress;
                        customer.CUSTOMERNIN = entity.customerNin;
                        if (entity.monthlyIncome > 0)
                        {
                            customer.MONTHLYINCOME = entity.monthlyIncome;
                        }
                        //if (entity.dateOfEmployment > DateTime.Now)
                        //{
                        //    customer.DATEOFEMPLOYMENT = entity.dateOfEmployment;
                        //}                       
                        if (entity.dateOfEmployment < DateTime.Now)
                        {
                            customer.DATEOFEMPLOYMENT = entity.dateOfEmployment;
                        }

                        customer.CUSTOMERBVN = entity.customerBVN;
                        //if (entity.dateOfBirth > DateTime.Now)
                        //{
                        //    customer.DATEOFBIRTH = entity.dateOfBirth;
                        //}

                        if (entity.dateOfBirth < DateTime.Now)
                        {
                            customer.DATEOFBIRTH = entity.dateOfBirth;
                        }

                        if (entity.subSectorId > 0)
                        {
                            var subSectors = dbContext.TBL_SUB_SECTOR.Select(s => s.SUBSECTORID).ToList();
                            if (subSectors != null)
                            {
                                if (subSectors.Contains((short)entity.subSectorId))
                                {
                                    customer.SUBSECTORID = (short)entity.subSectorId;
                                }
                                else
                                {
                                    throw new ConditionNotMetException($"Please enter a valid sub-sector Id");
                                }
                            }
                        }


                        //if (!string.IsNullOrEmpty(entity.otherBankAccount))
                        //{
                        //    if (string.IsNullOrEmpty(customer.OTHERBANKACCOUNT2))
                        //    {
                        //        customer.OTHERBANKACCOUNT2 = customer.OTHERBANKACCOUNT;
                        //        customer.OTHERBANKSORTCODE2 = customer.OTHERBANKSORTCODE;
                        //        customer.OTHERBANKACCOUNT = entity.otherBankAccount;
                        //        customer.OTHERBANKSORTCODE = entity.otherBankSortCode;

                        //    }
                        //    else if (!string.IsNullOrEmpty(customer.OTHERBANKACCOUNT2))
                        //    {
                        //        customer.OTHERBANKACCOUNT3 = customer.OTHERBANKACCOUNT;
                        //        customer.OTHERBANKSORTCODE3 = customer.OTHERBANKSORTCODE;
                        //        customer.OTHERBANKACCOUNT = entity.otherBankAccount;
                        //        customer.OTHERBANKSORTCODE = entity.otherBankSortCode;
                        //    }
                        //}

                        if (!string.IsNullOrEmpty(entity.otherBankAccount))
                        {
                            customer.OTHERBANKACCOUNT = entity.otherBankAccount;
                            customer.OTHERBANKSORTCODE = entity.otherBankSortCode;
                        }

                        var updatePhoneContact = dbContext.TBL_CUSTOMER_PHONECONTACT.Where(p => p.CUSTOMERID == customer.CUSTOMERID).FirstOrDefault();
                        if (entity.mobilePhoneNo != null || entity.officeLandNo != null || updatePhoneContact != null)
                        {
                            updatePhoneContact.PHONENUMBER = entity.mobilePhoneNo;
                            updatePhoneContact.PHONE = entity.officeLandNo;
                        }
                        else if (entity.mobilePhoneNo != null || entity.officeLandNo != null || updatePhoneContact == null)
                        {
                            var phoneContact = new TBL_CUSTOMER_PHONECONTACT();
                            phoneContact.PHONENUMBER = entity.mobilePhoneNo;
                            phoneContact.PHONE = entity.officeLandNo;

                            dbContext.TBL_CUSTOMER_PHONECONTACT.Add(phoneContact);
                        }
                        //if (entity.employerNumber != null || entity.employerNumber != "")
                        //{
                        //    customer.EMPLOYERNUMBER = entity.employerNumber;
                        //}
                        if (entity.customerTypeId > 0)
                        {
                            var customerTypes = dbContext.TBL_CUSTOMER_TYPE.Select(c => c.CUSTOMERTYPEID).ToList();
                            if (!customerTypes.Contains((short)entity.customerTypeId))
                            {
                                throw new SecureException("Customer Type for this customer is not valid, Kindly update from core-banking");
                            }
                            customer.CUSTOMERTYPEID = (short)entity.customerTypeId;
                        }
                        //if (customer.ISBVNVALIDATED != entity.isBvnValidated)
                        //{
                        //    customer.ISBVNVALIDATED = entity.isBvnValidated;
                        //}
                        if (customer.TITLE != null || entity.title != "")
                        {
                            customer.TITLE = entity.title;
                        }
                        if (customer.GENDER != null || entity.gender != "")
                        {
                            customer.GENDER = entity.gender;
                        }

                        if (customer.FIRSTNAME != null || entity.firstName != "")
                        {
                            customer.FIRSTNAME = entity.firstName;
                        }
                        if (customer.MIDDLENAME != null || entity.middleName != "")
                        {
                            customer.MIDDLENAME = entity.middleName;
                        }
                        if (customer.LASTNAME != null || entity.lastName != "")
                        {
                            customer.LASTNAME = entity.lastName;
                        }

                        // save changes
                        var updateSuccess = dbContext.SaveChanges() > 0;

                        if (updateSuccess)
                        {
                            try
                            {
                                // initiate auit table
                                var audit = new TBL_AUDIT
                                {
                                    AUDITTYPEID = (short)AuditTypeEnum.CustomerUpdated,
                                    STAFFID = 1,
                                    BRANCHID = (short)customer.BRANCHID,
                                    DETAIL = $"(External) Updated Customer  {customer.TBL_COMPANY.NAME} with Code: {customer.CUSTOMERCODE} ",
                                    //IPADDRESS = entity.userIPAddress,
                                    //URL = entity.applicationUrl,
                                    APPLICATIONDATE = _genSetup.GetApplicationDate(),
                                    SYSTEMDATETIME = DateTime.Now
                                };

                                // add the audit trail
                                auditTrail.AddAuditTrail(audit);

                                var output = dbContext.SaveChanges() > 0;
                                trans.Commit();
                                trans.Dispose();


                                var result = "Customer Information Updated";

                                if (output == true)
                                {
                                    return result;
                                }
                                else
                                {
                                    return null;
                                }

                            }
                            catch (DbEntityValidationException ex)
                            {
                                trans.Rollback();

                                string errorMessages = string.Join("; ",
                                ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                                throw new DbEntityValidationException(errorMessages);
                            }
                        }



                    }
                    catch (Exception exp)
                    {
                        trans.Rollback();
                        throw new SecureException(exp.Message);
                    }

                    return null;
                }

            }

        }

        public async Task<string> AddCorporateProspectCustomerAsync(ProspectCorporateCustomerForCreation entity)
        {
            using (FinTrakBankingContext dbContext = new FinTrakBankingContext())
            {
                using (var trans = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // confirm the with the branch code
                        var branch = await dbContext.TBL_BRANCH.Where(b => b.BRANCHCODE == entity.branchCode).FirstOrDefaultAsync();

                        if (branch == null)
                            throw new ConditionNotMetException($"The branch code {entity.branchCode} does not exist.");

                        //get the BM mapped to the selected branch
                        var staff = await dbContext.TBL_STAFF.Where(b => b.BRANCHID == branch.BRANCHID && b.STAFFROLEID == 3 && b.DELETED == false).FirstOrDefaultAsync();

                        if (staff == null)
                            throw new ConditionNotMetException($"There is no Business Manager mapped to this selected branch {branch.BRANCHNAME}.");

                        // generate the customer prospect code
                        string code = CommonHelpers.GenerateUniqueIntergers(7).ToString();
                        var prospectCustomerCode = "PROS-" + code;


                        var customer = new TBL_CUSTOMER();
                        customer.ACCOUNTCREATIONCOMPLETE = true;
                        customer.ISPOLITICALLYEXPOSED = false;
                        //customer.PEPBYINTEGRATION = false;
                        customer.CREATIONMAILSENT = false;
                        customer.ISPROSPECT = false;
                        customer.PROSPECTCUSTOMERCODE = prospectCustomerCode;
                        customer.CUSTOMERTYPEID = (int)CustomerTypeEnum.Corporate;
                        customer.CUSTOMERCODE = prospectCustomerCode;
                        customer.CRMSCOMPANYSIZEID = entity.crmsCompanySizeId;
                        customer.CRMSLEGALSTATUSID = entity.crmsLegalStatusId;
                        customer.CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId;
                        customer.RELATIONSHIPOFFICERID = staff.STAFFID;
                        customer.CREATEDBY = (int)staff.STAFFID;
                        customer.DATEOFBIRTH = entity.dateOfIncorporation;
                        customer.DATETIMECREATED = DateTime.Now;
                        customer.FIRSTNAME = entity.companyName;
                        customer.BRANCHID = branch.BRANCHID;
                        customer.COMPANYID = staff.COMPANYID;
                        customer.EMAILADDRESS = entity.emailAddress;
                        customer.TAXNUMBER = entity.taxNumber;

                        // add to table
                        dbContext.TBL_CUSTOMER.Add(customer);
                        try
                        {
                            // save changes
                            await dbContext.SaveChangesAsync();

                            // initiate company's information
                            var company = new TBL_CUSTOMER_COMPANYINFOMATION();
                            company.REGISTRATIONNUMBER = entity.registrationNumber;
                            company.COMPANYNAME = entity.companyName;
                            company.COMPANYWEBSITE = entity.companyWebsite;
                            company.COMPANYEMAIL = entity.companyEmail;
                            company.REGISTEREDOFFICE = entity.registeredOffice;
                            company.ANNUALTURNOVER = entity.annualTurnOver;
                            company.PAIDUPCAPITAL = entity.paidUpCapital;
                            company.AUTHORISEDCAPITAL = entity.authorizedCapital;
                            company.SHAREHOLDER_FUND = entity.shareholderFund;
                            company.CUSTOMERID = customer.CUSTOMERID;


                            // add to table
                            dbContext.TBL_CUSTOMER_COMPANYINFOMATION.Add(company);


                            // initiate audit table
                            var audit = new TBL_AUDIT
                            {
                                AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
                                STAFFID = staff.STAFFID,
                                BRANCHID = (short)branch.BRANCHID,
                                DETAIL = $"(External) Added Customer  {entity.companyName} with Code: {prospectCustomerCode} ",
                                //IPADDRESS = entity.userIPAddress,
                                //URL = entity.applicationUrl,
                                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                                SYSTEMDATETIME = DateTime.Now
                            };

                            // add the audit trail
                             auditTrail.AddAuditTrail(audit);

                            var output = await dbContext.SaveChangesAsync() > 0;
                            trans.Commit();
                            trans.Dispose();


                            var result = prospectCustomerCode;

                            if (output == true)
                            {
                                return result;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            trans.Rollback();

                            string errorMessages = string.Join("; ",
                            ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                            throw new DbEntityValidationException(errorMessages);
                        }
                    }
                    catch (Exception exp)
                    {
                        trans.Rollback();
                        throw new SecureException(exp.Message);
                    }
                }
            }
        }




        public async Task<string> AddCorporateExistingCustomerAsync(ExistingCorporateCustomerForCreation entity)
        {
            using (FinTrakBankingContext dbContext = new FinTrakBankingContext())
            {
                using (var trans = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (await ValidateCustomerCodeAsync(entity.customerCode.Trim()))
                        {
                            throw new ConditionNotMetException($"Customer with customer code {entity.customerCode.Trim()} already exist, kinldy go ahead and apply for a loan.");
                        }

                        // confirm the with the branch code
                        var branch = await dbContext.TBL_BRANCH.Where(b => b.BRANCHCODE == entity.branchCode.Trim()).FirstOrDefaultAsync();

                        if (branch == null)
                            throw new ConditionNotMetException($"The branch code {entity.branchCode.Trim()} does not exist.");

                        //get the BM mapped to the selected branch
                        var staff = await dbContext.TBL_STAFF.Where(b => b.BRANCHID == branch.BRANCHID && b.STAFFROLEID == 3 && b.DELETED == false).FirstOrDefaultAsync();

                        if (staff == null)
                            throw new ConditionNotMetException($"There is no Business Manager mapped to this selected branch {branch.BRANCHNAME}.");


                        bool isPoliticallyExposed = false;
                        bool pepByIntegration = false;

                        if (USE_THIRD_PARTY_INTEGRATION)
                        {
                            var pep = finacle.GetExposePersonStatus(entity.customerCode.Trim());

                            if (pep == true)
                            {
                                isPoliticallyExposed = pep;
                                pepByIntegration = true;
                            }
                            else if (isPoliticallyExposed == false && pep == true)
                            {
                                isPoliticallyExposed = pep;
                                pepByIntegration = true;
                            }
                            else if (isPoliticallyExposed == true && pep == false)
                            {
                                isPoliticallyExposed = true;
                                pepByIntegration = false;
                            }
                            else if (isPoliticallyExposed == false && pep == false)
                            {
                                isPoliticallyExposed = false;
                                pepByIntegration = false;
                            }
                            else
                            {
                                isPoliticallyExposed = false;
                                pepByIntegration = false;
                            }
                        }


                        var customer = new TBL_CUSTOMER();
                        customer.ACCOUNTCREATIONCOMPLETE = true;
                        customer.ISPOLITICALLYEXPOSED = false;
                        //customer.PEPBYINTEGRATION = pepByIntegration;
                        customer.CREATIONMAILSENT = false;
                        customer.ISPROSPECT = false;

                        customer.CUSTOMERTYPEID = (int)CustomerTypeEnum.Corporate;
                        customer.CUSTOMERCODE = entity.customerCode.Trim();
                        customer.CRMSCOMPANYSIZEID = entity.crmsCompanySizeId;
                        customer.CRMSLEGALSTATUSID = entity.crmsLegalStatusId;
                        customer.CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId;
                        customer.RELATIONSHIPOFFICERID = staff.STAFFID;
                        customer.CREATEDBY = (int)staff.STAFFID;
                        customer.DATEOFBIRTH = entity.dateOfIncorporation.Date;
                        customer.DATETIMECREATED = DateTime.Now;
                        customer.FIRSTNAME = entity.companyName;
                        customer.BRANCHID = branch.BRANCHID;
                        customer.COMPANYID = staff.COMPANYID;
                        customer.EMAILADDRESS = entity.emailAddress;
                        customer.TAXNUMBER = entity.taxNumber;
                        customer.CUSTOMERSENSITIVITYLEVELID = (int)CustomerSensitivityLevelENum.Negligible;

                        customer.ISFROMEXTERNALSOURCE = true;
                        //customer.ISBVNVALIDATED = entity.isBvnValidated;
                        //customer.ISPHONEVALIDATED = entity.isPhoneValidated;
                        //customer.ISEMAILVALIDATED = entity.isEmailValidated;


                        // add to table
                        dbContext.TBL_CUSTOMER.Add(customer);
                        try
                        {
                            // save changes
                            await dbContext.SaveChangesAsync();

                            if (USE_THIRD_PARTY_INTEGRATION)
                            {
                                try
                                {
                                    //var i = integration.AddCustomerAccounts(customer.CUSTOMERCODE);
                                }
                                catch (APIErrorException ex)
                                {
                                    throw new APIErrorException(ex.Message);
                                }
                                catch (Exception ex)
                                {
                                    throw new ConditionNotMetException("An error occured while get customer account details");
                                }
                            }

                            // initiate company's information
                            var company = new TBL_CUSTOMER_COMPANYINFOMATION();
                            company.REGISTRATIONNUMBER = entity.registrationNumber;
                            company.COMPANYNAME = entity.companyName;
                            company.COMPANYWEBSITE = entity.companyWebsite;
                            company.COMPANYEMAIL = entity.companyEmail;
                            company.REGISTEREDOFFICE = entity.registeredOffice;
                            company.ANNUALTURNOVER = entity.annualTurnOver;
                            company.PAIDUPCAPITAL = entity.paidUpCapital;
                            company.AUTHORISEDCAPITAL = entity.authorizedCapital;
                            company.SHAREHOLDER_FUND = entity.shareholderFund;
                            company.CUSTOMERID = customer.CUSTOMERID;


                            // add to table
                            dbContext.TBL_CUSTOMER_COMPANYINFOMATION.Add(company);


                            // initiate auit table
                            var audit = new TBL_AUDIT
                            {
                                AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
                                STAFFID = staff.STAFFID,
                                BRANCHID = (short)branch.BRANCHID,
                                DETAIL = $"(External) Added Customer  {entity.companyName} with Code: {entity.customerCode} ",
                                //IPADDRESS = entity.userIPAddress,
                                //URL = entity.applicationUrl,
                                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                                SYSTEMDATETIME = DateTime.Now
                            };

                            // add the audit trail
                             auditTrail.AddAuditTrail(audit);

                            var output = await dbContext.SaveChangesAsync() > 0;
                            trans.Commit();
                            trans.Dispose();


                            var result = entity.customerCode;

                            if (output == true)
                            {
                                return result;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            trans.Rollback();

                            string errorMessages = string.Join("; ",
                            ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                            throw new DbEntityValidationException(errorMessages);
                        }
                    }
                    catch (Exception exp)
                    {
                        trans.Rollback();
                        throw new SecureException(exp.Message);
                    }
                }
            }
        }



        public async Task<List<CustomerUploadedDocumentForReturn>>  GetKYCDocumentUploadByCustomerCode(string customerCode)
        { 
            using (var dbcontext = new FinTrakBankingContext())
            {
                var customer = await dbcontext.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == customerCode).FirstOrDefaultAsync();

                if(customer == null)
                    throw new ConditionNotMetException($"Customer with customer code {customerCode} does not exist.");




                using (var context = new FinTrakBankingDocumentsContext())
                {
                    var Record = await context.TBL_MEDIA_KYC_DOCUMENTS.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => new CustomerUploadedDocumentForReturn
                    {
                        //documentId = x.DOCUMENTID,
                        //customerId = x.CUSTOMERID,
                        customerCode = x.CUSTOMERCODE,
                        documentTitle = x.DOCUMENTTITLE,
                        documentTypeId = (short)x.DOCUMENTTYPEID,
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        systemDateTime = x.SYSTEMDATETIME,
                        physicalFileNumber = x.PHYSICALFILENUMBER,
                        //physicalLocation = x.PHYSICALLOCATION,
                    }).ToListAsync();

                    return Record;
                }
            } 
        }


        public async Task<string> AddIndividualProspectCustomerAsync(ProspectIndividualCustomerForCreation entity)
        {
            using (FinTrakBankingContext dbContext = new FinTrakBankingContext())
            {
                using (var trans = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // confirm the with the branch code
                        var branch = await dbContext.TBL_BRANCH.Where(b => b.BRANCHCODE == entity.branchCode).FirstOrDefaultAsync();

                        if (branch == null)
                            throw new ConditionNotMetException($"The branch code {entity.branchCode} does not exist.");

                        //get the BM mapped to the selected branch
                        var staff = await dbContext.TBL_STAFF.Where(b => b.BRANCHID == branch.BRANCHID && b.STAFFROLEID == 3 && b.DELETED == false).FirstOrDefaultAsync();
                        if (staff == null)
                            throw new ConditionNotMetException($"There is no Business Manager mapped to this selected branch {branch.BRANCHNAME}.");

                        if (entity.contactAddress == null)
                            throw new ConditionNotMetException($"Contact address is compulsory.");

                        var cityData = dbContext.TBL_CITY.Where(x => x.CITYID == entity.contactAddress.cityId).FirstOrDefault();
                        if (cityData == null)
                            throw new ConditionNotMetException($"Please select a valid city.");

                        if (entity.contactPhone == null)
                            throw new ConditionNotMetException($"Contact phone details is compulsory.");

                        if (entity.nextOfKin == null)
                            throw new ConditionNotMetException($"Kindly provide next of kin information.");

                        if (entity.isInsiderRelated == true)
                            if (entity.insiderRelated == null)
                                throw new ConditionNotMetException($"Kindly provide insider related information.");

                        // generate the customer prospect code
                        string code = CommonHelpers.GenerateUniqueIntergers(7).ToString();
                        var prospectCustomerCode = "PROS-" + code;


                        var customer = new TBL_CUSTOMER();
                        customer.ACCOUNTCREATIONCOMPLETE = true;
                        customer.ISPOLITICALLYEXPOSED = false;
                        //customer.PEPBYINTEGRATION = false;
                        customer.CREATIONMAILSENT = false;
                        customer.ISPROSPECT = false;

                        customer.PROSPECTCUSTOMERCODE = prospectCustomerCode;
                        customer.CUSTOMERTYPEID = (int)CustomerTypeEnum.Individual;
                        customer.CUSTOMERCODE = prospectCustomerCode;
                        customer.CRMSLEGALSTATUSID = entity.crmsLegalStatusId;
                        customer.CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId;
                        customer.RELATIONSHIPOFFICERID = staff.STAFFID;
                        customer.CREATEDBY = staff.STAFFID;
                        customer.DATEOFBIRTH = entity.dateOfBirth.Date;
                        customer.DATETIMECREATED = DateTime.Now;
                        customer.FIRSTNAME = entity.firstName;
                        customer.MIDDLENAME = entity.middleName;
                        customer.LASTNAME = entity.lastName;
                        customer.BRANCHID = branch.BRANCHID;
                        customer.COMPANYID = staff.COMPANYID;
                        customer.EMAILADDRESS = entity.emailAddress;
                        customer.TAXNUMBER = entity.taxNumber; 
                        //customer.MARITALSTATUS = entity.maritalStatus;
                        customer.NATIONALITY = entity.nationality;
                        customer.CUSTOMERSENSITIVITYLEVELID = (short)CustomerSensitivityLevelENum.Negligible;

                        //customer.ISBVNVALIDATED = entity.isBvnValidated;
                        //customer.ISPHONEVALIDATED = entity.isPhoneValidated;
                        //customer.ISEMAILVALIDATED = entity.isEmailValidated;

                        try
                        {
                            // add to table
                            dbContext.TBL_CUSTOMER.Add(customer);

                            // save changes
                            await dbContext.SaveChangesAsync();

                            // customer address
                            var address = new TBL_CUSTOMER_ADDRESS();
                            address.ACTIVE = true;
                            address.ELECTRICMETERNUMBER = entity.contactAddress.utilityBillNo;
                            address.CITYID = entity.contactAddress.cityId;
                            address.STATEID = entity.contactAddress.stateId;
                            address.NEARESTLANDMARK = entity.contactAddress.nearestLandmark;
                            address.POBOX = entity.contactAddress.mailingAddress;
                            address.HOMETOWN = cityData.CITYNAME;
                            address.ADDRESS = entity.contactAddress.contactAddress;
                            address.ADDRESSTYPEID = entity.contactAddress.addressTypeId;
                            address.CUSTOMERID = customer.CUSTOMERID;

                            //add to table
                            dbContext.TBL_CUSTOMER_ADDRESS.Add(address);


                            //init contactPhoneNo 
                            var phoneDetails = new TBL_CUSTOMER_PHONECONTACT();
                            phoneDetails.ACTIVE = true;
                            phoneDetails.PHONE = entity.contactPhone.officeLandNo;
                            phoneDetails.PHONENUMBER = entity.contactPhone.mobilePhoneNo;
                            phoneDetails.CUSTOMERID = customer.CUSTOMERID;

                            //add to table
                            dbContext.TBL_CUSTOMER_PHONECONTACT.Add(phoneDetails);


                            if (entity.isInsiderRelated == true)
                            {
                                var insiderRelatedInfo = new TBL_CUSTOMER_RELATED_PARTY();
                                insiderRelatedInfo.COMPANYDIRECTORID = entity.insiderRelated.companyDirectorId;
                                insiderRelatedInfo.RELATIONSHIPTYPE = entity.insiderRelated.relationshipType;
                                insiderRelatedInfo.CUSTOMERID = customer.CUSTOMERID;
                                insiderRelatedInfo.CREATEDBY = staff.STAFFID;
                                insiderRelatedInfo.DATETIMECREATED = DateTime.Now;

                                //add to table
                                dbContext.TBL_CUSTOMER_RELATED_PARTY.Add(insiderRelatedInfo);
                            }

                            //next of kin
                            var nextOfKin = new TBL_CUSTOMER_NEXTOFKIN();
                            nextOfKin.FIRSTNAME = entity.nextOfKin.firstName;
                            nextOfKin.LASTNAME = entity.nextOfKin.lastName;
                            nextOfKin.EMAIL = entity.nextOfKin.emailAddress;
                            nextOfKin.DATEOFBIRTH = entity.nextOfKin.dateOfBirth;
                            nextOfKin.GENDER = entity.nextOfKin.gender;
                            nextOfKin.ADDRESS = entity.nextOfKin.contactAddress;
                            nextOfKin.CITYID = entity.nextOfKin.cityId;
                            nextOfKin.NEAREST_LANDMARK = entity.nextOfKin.nearestLandmark;
                            nextOfKin.RELATIONSHIP = entity.nextOfKin.relationship;
                            nextOfKin.PHONENUMBER = entity.nextOfKin.mobilePhoneNo;
                            nextOfKin.ACTIVE = true;
                            nextOfKin.CUSTOMERID = customer.CUSTOMERID;

                            //add to table
                            dbContext.TBL_CUSTOMER_NEXTOFKIN.Add(nextOfKin);


                            // initiate auit table
                            var audit = new TBL_AUDIT
                            {
                                AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
                                STAFFID = staff.STAFFID,
                                BRANCHID = (short)branch.BRANCHID,
                                DETAIL = $"(External) Added Customer  {entity.customerName} with Code: {prospectCustomerCode} ",
                                //IPADDRESS = entity.userIPAddress,
                                //URL = entity.applicationUrl,
                                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                                SYSTEMDATETIME = DateTime.Now
                            };

                            // add the audit trail
                             auditTrail.AddAuditTrail(audit);

                            var output = await dbContext.SaveChangesAsync() > 0;
                            trans.Commit();
                            trans.Dispose();


                            var result = prospectCustomerCode;

                            if (output == true)
                            {
                                return result;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            trans.Rollback();

                            string errorMessages = string.Join("; ",
                            ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                            throw new DbEntityValidationException(errorMessages);
                        }
                    }
                    catch (Exception exp)
                    {
                        trans.Rollback();
                        throw new SecureException(exp.Message);
                    }
                }
            }
        }

        public async Task<string> AddIndividualExistingCustomerAsync(ExistingIndividualCustomerForCreation entity)
        {
            using (FinTrakBankingContext dbContext = new FinTrakBankingContext())
            {
                using (var trans = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (await ValidateCustomerCodeAsync(entity.customerCode.Trim()))
                        {
                            throw new ConditionNotMetException($"Customer with customer code {entity.customerCode.Trim()} already exist, kinldy go ahead and apply for a loan.");
                        }

                        // confirm the with the branch code
                        var branch = await dbContext.TBL_BRANCH.Where(b => b.BRANCHCODE == entity.branchCode.Trim()).FirstOrDefaultAsync();

                        if (branch == null)
                            throw new ConditionNotMetException($"The branch code {entity.branchCode.Trim()} does not exist.");

                        //get the RM mapped to the selected branch
                        var staff = await dbContext.TBL_STAFF.Where(b => b.BRANCHID == branch.BRANCHID && b.STAFFROLEID == 3 && b.DELETED == false).FirstOrDefaultAsync();

                        if (staff == null)
                            throw new ConditionNotMetException($"There is no Credit Officer mapped to this selected branch {branch.BRANCHNAME}.");

                        if (entity.contactAddress == null)
                            throw new ConditionNotMetException($"Contact address is compulsory.");

                        var cityData = dbContext.TBL_CITY.Where(x => x.CITYID == entity.contactAddress.cityId).FirstOrDefault();
                        if (cityData == null)
                            throw new ConditionNotMetException($"Please select a valid city.");

                        if (entity.contactPhone == null)
                            throw new ConditionNotMetException($"Contact phone details is compulsory.");

                        if (entity.accountDetails == null)
                            throw new ConditionNotMetException($"Account details are compulsory.");

                        if (entity.subSectorId == 0 || entity.subSectorId < 0)
                        {
                            throw new ConditionNotMetException($"kindly enter a valid sub-sector Id.");
                        }
                        //if (entity.customerTypeId == (int)CustomerTypeEnum.PMB || entity.customerTypeId == (int)CustomerTypeEnum.CorporativeSociety || entity.customerTypeId == (int)CustomerTypeEnum.Developer)
                        //{
                        //    string customerTypeName = "";
                        //    if(entity.customerTypeId == (int)CustomerTypeEnum.PMB)
                        //    {
                        //        customerTypeName = "PMB";
                        //    }
                        //    if (entity.customerTypeId == (int)CustomerTypeEnum.CorporativeSociety)
                        //    {
                        //        customerTypeName = "Corporative Society";
                        //    }
                        //    if (entity.customerTypeId == (int)CustomerTypeEnum.Developer)
                        //    {
                        //        customerTypeName = "Developer";
                        //    }
                        //    if (entity.employerNumber == null || entity.employerNumber == "")
                        //    {
                        //        throw new ConditionNotMetException($"kindly enter Employer Number for this {customerTypeName}");
                        //    }
                        //}


                        //if (entity.nextOfKin == null)
                        //    throw new ConditionNotMetException($"Kindly provide next of kin information.");


                        //if (entity.isInsiderRelated == true)
                        //    if (entity.insiderRelated == null)
                        //        throw new ConditionNotMetException($"Kindly provide insider related information.");


                        bool isPoliticallyExposed = false;
                        bool pepByIntegration = false;

                        //if (USE_THIRD_PARTY_INTEGRATION)
                        //{
                        //    var pep = finacle.GetExposePersonStatus(entity.customerCode.Trim());

                        //    if (pep == true)
                        //    {
                        //        isPoliticallyExposed = pep;
                        //        pepByIntegration = true;
                        //    }
                        //    else if (isPoliticallyExposed == false && pep == true)
                        //    {
                        //        isPoliticallyExposed = pep;
                        //        pepByIntegration = true;
                        //    }
                        //    else if (isPoliticallyExposed == true && pep == false)
                        //    {
                        //        isPoliticallyExposed = true;
                        //        pepByIntegration = false;
                        //    }
                        //    else if (isPoliticallyExposed == false && pep == false)
                        //    {
                        //        isPoliticallyExposed = false;
                        //        pepByIntegration = false;
                        //    }
                        //    else
                        //    {
                        //        isPoliticallyExposed = false;
                        //        pepByIntegration = false;
                        //    }
                        //}


                        var customer = new TBL_CUSTOMER();
                        customer.ACCOUNTCREATIONCOMPLETE = true;
                        customer.ISPOLITICALLYEXPOSED = false;
                        //customer.PEPBYINTEGRATION = pepByIntegration;
                        customer.CREATIONMAILSENT = false;
                        customer.ISPROSPECT = false;

                        //customer.CUSTOMERTYPEID = (int)CustomerTypeEnum.Individual;
                        customer.CUSTOMERCODE = entity.customerCode.Trim();
                        customer.CRMSLEGALSTATUSID = entity.crmsLegalStatusId;
                        customer.CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId;
                        customer.RELATIONSHIPOFFICERID = staff.STAFFID;
                        customer.CREATEDBY = (int)staff.STAFFID;
                        customer.DATEOFBIRTH = entity.dateOfBirth.Date;
                        customer.DATETIMECREATED = DateTime.Now;
                        customer.FIRSTNAME = entity.firstName;
                        customer.MIDDLENAME = entity.middleName;
                        customer.LASTNAME = entity.lastName;
                        customer.BRANCHID = branch.BRANCHID;
                        customer.COMPANYID = staff.COMPANYID;
                        customer.EMAILADDRESS = entity.emailAddress;
                        customer.TAXNUMBER = entity.taxNumber;
                        customer.NATIONALITY = entity.nationality;
                        customer.CUSTOMERSENSITIVITYLEVELID = (int)CustomerSensitivityLevelENum.Negligible;

                        customer.ISFROMEXTERNALSOURCE = true;
                        //customer.ISBVNVALIDATED = entity.isBvnValidated;
                        //customer.ISPHONEVALIDATED = entity.isPhoneValidated;
                        //customer.ISEMAILVALIDATED = entity.isEmailValidated;
                        customer.DATEOFEMPLOYMENT = entity.accountDetails.dateOfEmployment;
                        customer.OTHERBANKACCOUNT = entity.accountDetails.otherBankAccountNumber;
                        customer.OTHERBANKSORTCODE = entity.accountDetails.otherBankSortCode;
                        customer.MONTHLYINCOME = entity.accountDetails.monthlyIncome;
                        customer.CUSTOMERBVN = entity.customerBVN;
                        //customer.EMPLOYERNUMBER = entity.employerNumber;
                        //customer.SUBSECTORID = entity.subSectorId;
                        if (entity.subSectorId > 0)
                        {
                            var subSectors = await dbContext.TBL_SUB_SECTOR.Select(s => s.SUBSECTORID).ToListAsync();
                            if (subSectors != null)
                            {
                                if (subSectors.Contains((short)entity.subSectorId))
                                {
                                    customer.SUBSECTORID = (short)entity.subSectorId;
                                }
                                else
                                {
                                    throw new ConditionNotMetException($"kindly enter a valid sub-sector Id");
                                }
                            }
                        }

                        if (entity.customerTypeId != null)
                        {
                            var customerType = await dbContext.TBL_CUSTOMER_TYPE.Select(c => c.CUSTOMERTYPEID).ToListAsync();
                            if (customerType.Contains((short)entity.customerTypeId))
                            {
                                customer.CUSTOMERTYPEID = (short)entity.customerTypeId;
                            }
                            else
                            {
                                throw new SecureException("Kindly enter a valid customer type");
                            }
                        }
                        else if (entity.customerTypeId == null) 
                        {
                            //customer.CUSTOMERTYPEID = (int)CustomerTypeEnum.Individual;
                            throw new SecureException("A valid customer type is required");
                        }

                        //if (entity.profileSourceId > 0)
                        //{
                        //    var applicationSources = await dbContext.TBL_SOURCE_APPLICATION.Select(s => s.APPLICATIONID).ToListAsync();
                        //    if (applicationSources != null)
                        //    {
                        //        if (applicationSources.Contains((short)entity.profileSourceId))
                        //        {
                        //            customer.PROFILESOURCEID = entity.profileSourceId;
                        //        }
                        //        else
                        //        {
                        //            throw new ConditionNotMetException($"This customer profiling source is not valid");
                        //        }
                        //    }
                        //}

                        try
                        {
                            // add to table
                            dbContext.TBL_CUSTOMER.Add(customer);


                            //if (USE_THIRD_PARTY_INTEGRATION)
                            //{
                            //    try
                            //    {
                            //        var i = integration.AddCustomerAccounts(customer.CUSTOMERCODE);
                            //    }
                            //    catch (APIErrorException ex)
                            //    {
                            //        throw new APIErrorException(ex.Message);
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        throw new ConditionNotMetException("An error occured while get customer account details");
                            //    }
                            //}

                            // save changes
                            await dbContext.SaveChangesAsync();


                            // customer address
                            var address = new TBL_CUSTOMER_ADDRESS();
                            address.ACTIVE = true;
                            address.ELECTRICMETERNUMBER = entity.contactAddress.utilityBillNo;
                            address.CITYID = entity.contactAddress.cityId;
                            address.STATEID = entity.contactAddress.stateId;
                            address.NEARESTLANDMARK = entity.contactAddress.nearestLandmark;
                            address.POBOX = entity.contactAddress.mailingAddress;
                            address.HOMETOWN = cityData.CITYNAME;
                            address.ADDRESS = entity.contactAddress.contactAddress;
                            address.ADDRESSTYPEID = entity.contactAddress.addressTypeId;
                            address.CUSTOMERID = customer.CUSTOMERID;

                            //add to table
                            dbContext.TBL_CUSTOMER_ADDRESS.Add(address);


                            //init contactPhoneNo 
                            var phoneDetails = new TBL_CUSTOMER_PHONECONTACT();
                            phoneDetails.ACTIVE = true;
                            phoneDetails.PHONE = entity.contactPhone.officeLandNo;
                            phoneDetails.PHONENUMBER = entity.contactPhone.mobilePhoneNo;
                            phoneDetails.CUSTOMERID = customer.CUSTOMERID;

                            //add to table
                            dbContext.TBL_CUSTOMER_PHONECONTACT.Add(phoneDetails);


                            //add customer account
                            if (entity.profileSourceId == 3)
                            {
                                if (entity.customerTypeId == (int)CustomerTypeEnum.Individual)
                                {
                                    if (dbContext.TBL_CASA.Any(x => x.PRODUCTACCOUNTNUMBER == entity.accountDetails.pmbNhfAccount.Trim()) == false)
                                        throw new SecureException("Kindly profile PMB's customer and account details");
                                }

                                if (dbContext.TBL_CASA.Any(x => x.PRODUCTACCOUNTNUMBER == entity.accountDetails.accountNumber.Trim()))
                                    throw new SecureException("Account number already exist");
                            }
                            else
                            {
                                if (dbContext.TBL_CASA.Any(x => x.PRODUCTACCOUNTNUMBER == entity.accountDetails.accountNumber.Trim()))
                                    throw new SecureException("Account number already exist");
                            }                            
                            
                            var accountStatusId = dbContext.TBL_CASA_ACCOUNTSTATUS.Where(x => x.ACCOUNTSTATUSNAME.ToLower().Trim() == entity.accountDetails.accountStatusName.ToLower().Trim())
                                                    .Select(x => x.ACCOUNTSTATUSID).FirstOrDefault();

                            TBL_CASA addCustomerAcct = new TBL_CASA();
                            addCustomerAcct.CUSTOMERID = customer.CUSTOMERID;
                            addCustomerAcct.AVAILABLEBALANCE = 0;
                            addCustomerAcct.LEDGERBALANCE = 0;
                            addCustomerAcct.PRODUCTACCOUNTNAME = entity.accountDetails.productAccountName;//item.productAccountName;
                            addCustomerAcct.PRODUCTACCOUNTNUMBER = entity.accountDetails.accountNumber;
                            addCustomerAcct.PRODUCTID = (short)DefaultProductEnum.CASA; //(short)(item.productCode != "" ? 8 : 8);
                            addCustomerAcct.COMPANYID = 1;
                            addCustomerAcct.BRANCHID = customer.BRANCHID;
                            addCustomerAcct.CURRENCYID = (short)CurrencyEnum.NGN;
                            addCustomerAcct.ISCURRENTACCOUNT = false;
                            addCustomerAcct.ACCOUNTSTATUSID = accountStatusId;//(short)(item.accountStatusName == "Active" ? 1 : 3);
                            addCustomerAcct.LIENAMOUNT = 0;
                            addCustomerAcct.HASLIEN = false;
                            addCustomerAcct.POSTNOSTATUSID = 1;
                            addCustomerAcct.DELETED = false;
                            //addCustomerAcct.PMBNHFACCOUNT = entity.accountDetails.pmbNhfAccount;
                            dbContext.TBL_CASA.Add(addCustomerAcct);

                            //insider related
                            //if (entity.isInsiderRelated == true)
                            //{
                            //    var insiderRelatedInfo = new TBL_CUSTOMER_RELATED_PARTY();
                            //    insiderRelatedInfo.COMPANYDIRECTORID = entity.insiderRelated.companyDirectorId;
                            //    insiderRelatedInfo.RELATIONSHIPTYPE = entity.insiderRelated.relationshipType;
                            //    insiderRelatedInfo.CUSTOMERID = customer.CUSTOMERID;
                            //    insiderRelatedInfo.CREATEDBY = staff.STAFFID;
                            //    insiderRelatedInfo.DATETIMECREATED = DateTime.Now;

                            //    //add to table
                            //    dbContext.TBL_CUSTOMER_RELATED_PARTY.Add(insiderRelatedInfo);
                            //}


                            //next of kin
                            //var nextOfKin = new TBL_CUSTOMER_NEXTOFKIN();
                            //nextOfKin.FIRSTNAME = entity.nextOfKin.firstName;
                            //nextOfKin.LASTNAME = entity.nextOfKin.lastName;
                            //nextOfKin.EMAIL = entity.nextOfKin.emailAddress;
                            //nextOfKin.DATEOFBIRTH = entity.nextOfKin.dateOfBirth;
                            //nextOfKin.GENDER = entity.nextOfKin.gender;
                            //nextOfKin.ADDRESS = entity.nextOfKin.contactAddress;
                            //nextOfKin.CITYID = entity.nextOfKin.cityId;
                            //nextOfKin.NEAREST_LANDMARK = entity.nextOfKin.nearestLandmark;
                            //nextOfKin.RELATIONSHIP = entity.nextOfKin.relationship;
                            //nextOfKin.PHONENUMBER = entity.nextOfKin.mobilePhoneNo;
                            //nextOfKin.ACTIVE = 1;
                            //nextOfKin.CUSTOMERID = customer.CUSTOMERID;

                            ////add to table
                            //dbContext.TBL_CUSTOMER_NEXTOFKIN.Add(nextOfKin);


                            // initiate auit table
                            var audit = new TBL_AUDIT
                            {
                                AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
                                STAFFID = staff.STAFFID,
                                BRANCHID = (short)branch.BRANCHID,
                                DETAIL = $"(External) Added Customer  {entity.customerName} with Code: {entity.customerCode} ",
                                //IPADDRESS = entity.userIPAddress,
                                //URL = entity.applicationUrl,
                                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                                SYSTEMDATETIME = DateTime.Now
                            };

                            // add the audit trail
                             auditTrail.AddAuditTrail(audit);

                            var output = await dbContext.SaveChangesAsync() > 0;
                            trans.Commit();
                            trans.Dispose();


                            var result = entity.customerCode;

                            if (output == true)
                            {
                                return result;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            trans.Rollback();

                            string errorMessages = string.Join("; ",
                            ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                            throw new DbEntityValidationException(errorMessages);
                        }
                    }
                    catch (Exception exp)
                    {
                        trans.Rollback();
                        throw new SecureException(exp.Message);
                    }
                }
            }
        }
        public string AddIndividualExistingCustomer(ExistingIndividualCustomerForCreation entity)
        {
            using (FinTrakBankingContext dbContext = new FinTrakBankingContext())
            {
                using (var trans = dbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (ValidateCustomerCode(entity.customerCode.Trim()))
                        {
                            throw new ConditionNotMetException($"Customer with customer code {entity.customerCode.Trim()} already exist, kinldy go ahead and apply for a loan.");
                        }

                        // confirm the with the branch code
                        var branch = dbContext.TBL_BRANCH.Where(b => b.BRANCHCODE == entity.branchCode.Trim()).FirstOrDefault();

                        if (branch == null)
                            throw new ConditionNotMetException($"The branch code {entity.branchCode.Trim()} does not exist.");

                        //get the Account Officer mapped to the selected branch
                        var staff = dbContext.TBL_STAFF.Where(b => b.BRANCHID == branch.BRANCHID && b.STAFFROLEID == 6 && b.DELETED == false).FirstOrDefault();

                        if (staff == null)
                            throw new ConditionNotMetException($"There is no Account Officer mapped to this selected branch {branch.BRANCHNAME}.");

                        if (entity.contactAddress == null)
                            throw new ConditionNotMetException($"Contact address is compulsory.");

                        var cityData = dbContext.TBL_CITY.Where(x => x.CITYID == entity.contactAddress.cityId).FirstOrDefault();
                        if (cityData == null)
                            throw new ConditionNotMetException($"Please select a valid city.");

                        if (entity.contactPhone == null)
                            throw new ConditionNotMetException($"Contact phone details is compulsory.");

                        if (entity.accountDetails == null)
                            throw new ConditionNotMetException($"Account details are compulsory.");

                        if (entity.subSectorId == 0 || entity.subSectorId < 0)
                        {
                            throw new ConditionNotMetException($"kindly enter a valid sub-sector Id.");
                        }
                        //if (entity.customerTypeId == (int)CustomerTypeEnum.PMB || entity.customerTypeId == (int)CustomerTypeEnum.CorporativeSociety || entity.customerTypeId == (int)CustomerTypeEnum.Developer)
                        //{
                        //    string customerTypeName = "";
                        //    if(entity.customerTypeId == (int)CustomerTypeEnum.PMB)
                        //    {
                        //        customerTypeName = "PMB";
                        //    }
                        //    if (entity.customerTypeId == (int)CustomerTypeEnum.CorporativeSociety)
                        //    {
                        //        customerTypeName = "Corporative Society";
                        //    }
                        //    if (entity.customerTypeId == (int)CustomerTypeEnum.Developer)
                        //    {
                        //        customerTypeName = "Developer";
                        //    }
                        //    if (entity.employerNumber == null || entity.employerNumber == "")
                        //    {
                        //        throw new ConditionNotMetException($"kindly enter Employer Number for this {customerTypeName}");
                        //    }
                        //}


                        //if (entity.nextOfKin == null)
                        //    throw new ConditionNotMetException($"Kindly provide next of kin information.");


                        //if (entity.isInsiderRelated == true)
                        //    if (entity.insiderRelated == null)
                        //        throw new ConditionNotMetException($"Kindly provide insider related information.");


                        bool isPoliticallyExposed = false;
                        bool pepByIntegration = false;

                        //if (USE_THIRD_PARTY_INTEGRATION)
                        //{
                        //    var pep = finacle.GetExposePersonStatus(entity.customerCode.Trim());

                        //    if (pep == true)
                        //    {
                        //        isPoliticallyExposed = pep;
                        //        pepByIntegration = true;
                        //    }
                        //    else if (isPoliticallyExposed == false && pep == true)
                        //    {
                        //        isPoliticallyExposed = pep;
                        //        pepByIntegration = true;
                        //    }
                        //    else if (isPoliticallyExposed == true && pep == false)
                        //    {
                        //        isPoliticallyExposed = true;
                        //        pepByIntegration = false;
                        //    }
                        //    else if (isPoliticallyExposed == false && pep == false)
                        //    {
                        //        isPoliticallyExposed = false;
                        //        pepByIntegration = false;
                        //    }
                        //    else
                        //    {
                        //        isPoliticallyExposed = false;
                        //        pepByIntegration = false;
                        //    }
                        //}


                        var customer = new TBL_CUSTOMER();
                        customer.ACCOUNTCREATIONCOMPLETE = true;
                        customer.ISPOLITICALLYEXPOSED = false;
                        //customer.PEPBYINTEGRATION = pepByIntegration;
                        customer.CREATIONMAILSENT = false;
                        customer.ISPROSPECT = false;

                        //customer.CUSTOMERTYPEID = (int)CustomerTypeEnum.Individual;
                        customer.CUSTOMERCODE = entity.customerCode.Trim();
                        customer.CRMSLEGALSTATUSID = entity.crmsLegalStatusId;
                        customer.CRMSRELATIONSHIPTYPEID = entity.crmsRelationshipTypeId;
                        customer.RELATIONSHIPOFFICERID = staff.STAFFID;
                        customer.CREATEDBY = (int)staff.STAFFID;
                        customer.DATEOFBIRTH = entity.dateOfBirth.Date;
                        customer.DATETIMECREATED = DateTime.Now;
                        customer.FIRSTNAME = entity.firstName;
                        customer.MIDDLENAME = entity.middleName;
                        customer.LASTNAME = entity.lastName;
                        customer.BRANCHID = branch.BRANCHID;
                        customer.COMPANYID = staff.COMPANYID;
                        customer.EMAILADDRESS = entity.emailAddress;
                        customer.TAXNUMBER = entity.taxNumber;
                        customer.NATIONALITY = entity.nationality;
                        customer.CUSTOMERSENSITIVITYLEVELID = (int)CustomerSensitivityLevelENum.Negligible;

                        customer.ISFROMEXTERNALSOURCE = true;
                        //customer.ISBVNVALIDATED = entity.isBvnValidated;
                        //customer.ISPHONEVALIDATED = entity.isPhoneValidated;
                        //customer.ISEMAILVALIDATED = entity.isEmailValidated;
                        customer.DATEOFEMPLOYMENT = entity.accountDetails.dateOfEmployment;
                        customer.OTHERBANKACCOUNT = entity.accountDetails.otherBankAccountNumber;
                        customer.OTHERBANKSORTCODE = entity.accountDetails.otherBankSortCode;
                        customer.MONTHLYINCOME = entity.accountDetails.monthlyIncome;
                        customer.CUSTOMERBVN = entity.customerBVN;
                        //customer.EMPLOYERNUMBER = entity.employerNumber;
                        //customer.SUBSECTORID = entity.subSectorId;
                        if (entity.subSectorId > 0)
                        {
                            var subSectors = dbContext.TBL_SUB_SECTOR.Select(s => s.SUBSECTORID).ToList();
                            if (subSectors != null)
                            {
                                if (subSectors.Contains((short)entity.subSectorId))
                                {
                                    customer.SUBSECTORID = (short)entity.subSectorId;
                                }
                                else
                                {
                                    throw new ConditionNotMetException($"kindly enter a valid sub-sector Id");
                                }
                            }
                        }

                        if (entity.customerTypeId != null)
                        {
                            var customerType = dbContext.TBL_CUSTOMER_TYPE.Select(c => c.CUSTOMERTYPEID).ToList();
                            if (customerType.Contains((short)entity.customerTypeId))
                            {
                                customer.CUSTOMERTYPEID = (short)entity.customerTypeId;
                            }
                            else
                            {
                                throw new SecureException("Kindly enter a valid customer type");
                            }
                        }
                        else if (entity.customerTypeId == null)
                        {
                            //customer.CUSTOMERTYPEID = (int)CustomerTypeEnum.Individual;
                            throw new SecureException("A valid customer type is required");
                        }

                        //if (entity.profileSourceId > 0)
                        //{
                        //    var applicationSources = dbContext.TBL_SOURCE_APPLICATION.Select(s => s.APPLICATIONID).ToList();
                        //    if (applicationSources != null)
                        //    {
                        //        if (applicationSources.Contains((short)entity.profileSourceId))
                        //        {
                        //            customer.PROFILESOURCEID = entity.profileSourceId;
                        //        }
                        //        else
                        //        {
                        //            throw new ConditionNotMetException($"This customer profiling source is not valid");
                        //        }
                        //    }
                        //}

                        try
                        {
                            // add to table
                            dbContext.TBL_CUSTOMER.Add(customer);


                            //if (USE_THIRD_PARTY_INTEGRATION)
                            //{
                            //    try
                            //    {
                            //        var i = integration.AddCustomerAccounts(customer.CUSTOMERCODE);
                            //    }
                            //    catch (APIErrorException ex)
                            //    {
                            //        throw new APIErrorException(ex.Message);
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        throw new ConditionNotMetException("An error occured while get customer account details");
                            //    }
                            //}

                            // save changes
                            dbContext.SaveChanges();


                            // customer address
                            var address = new TBL_CUSTOMER_ADDRESS();
                            address.ACTIVE = true;
                            address.ELECTRICMETERNUMBER = entity.contactAddress.utilityBillNo;
                            address.CITYID = entity.contactAddress.cityId;
                            address.STATEID = entity.contactAddress.stateId;
                            address.NEARESTLANDMARK = entity.contactAddress.nearestLandmark;
                            address.POBOX = entity.contactAddress.mailingAddress;
                            address.HOMETOWN = cityData.CITYNAME;
                            address.ADDRESS = entity.contactAddress.contactAddress;
                            address.ADDRESSTYPEID = entity.contactAddress.addressTypeId;
                            address.CUSTOMERID = customer.CUSTOMERID;

                            //add to table
                            dbContext.TBL_CUSTOMER_ADDRESS.Add(address);


                            //init contactPhoneNo 
                            var phoneDetails = new TBL_CUSTOMER_PHONECONTACT();
                            phoneDetails.ACTIVE = true;
                            phoneDetails.PHONE = entity.contactPhone.officeLandNo;
                            phoneDetails.PHONENUMBER = entity.contactPhone.mobilePhoneNo;
                            phoneDetails.CUSTOMERID = customer.CUSTOMERID;

                            //add to table
                            dbContext.TBL_CUSTOMER_PHONECONTACT.Add(phoneDetails);


                            //add customer account
                            if (entity.profileSourceId == 3)
                            {
                                if (entity.customerTypeId == (int)CustomerTypeEnum.Individual)
                                {
                                    if (dbContext.TBL_CASA.Any(x => x.PRODUCTACCOUNTNUMBER == entity.accountDetails.pmbNhfAccount.Trim()) == false)
                                        throw new SecureException("Kindly profile PMB's customer and account details");
                                }

                                if (dbContext.TBL_CASA.Any(x => x.PRODUCTACCOUNTNUMBER == entity.accountDetails.accountNumber.Trim()))
                                    throw new SecureException("Account number already exist");
                            }
                            else
                            {
                                if (dbContext.TBL_CASA.Any(x => x.PRODUCTACCOUNTNUMBER == entity.accountDetails.accountNumber.Trim()))
                                    throw new SecureException("Account number already exist");
                            }

                            var accountStatusId = dbContext.TBL_CASA_ACCOUNTSTATUS.Where(x => x.ACCOUNTSTATUSNAME.ToLower().Trim() == entity.accountDetails.accountStatusName.ToLower().Trim())
                                                    .Select(x => x.ACCOUNTSTATUSID).FirstOrDefault();

                            TBL_CASA addCustomerAcct = new TBL_CASA();
                            addCustomerAcct.CUSTOMERID = customer.CUSTOMERID;
                            addCustomerAcct.AVAILABLEBALANCE = 0;
                            addCustomerAcct.LEDGERBALANCE = 0;
                            addCustomerAcct.PRODUCTACCOUNTNAME = entity.accountDetails.productAccountName;//item.productAccountName;
                            addCustomerAcct.PRODUCTACCOUNTNUMBER = entity.accountDetails.accountNumber;
                            addCustomerAcct.PRODUCTID = (short)DefaultProductEnum.CASA; //(short)(item.productCode != "" ? 8 : 8);
                            addCustomerAcct.COMPANYID = 1;
                            addCustomerAcct.BRANCHID = customer.BRANCHID;
                            addCustomerAcct.CURRENCYID = (short)CurrencyEnum.NGN;
                            addCustomerAcct.ISCURRENTACCOUNT = false;
                            addCustomerAcct.ACCOUNTSTATUSID = accountStatusId;//(short)(item.accountStatusName == "Active" ? 1 : 3);
                            addCustomerAcct.LIENAMOUNT = 0;
                            addCustomerAcct.HASLIEN = false;
                            addCustomerAcct.POSTNOSTATUSID = 1;
                            addCustomerAcct.DELETED = false;
                            //addCustomerAcct.PMBNHFACCOUNT = entity.accountDetails.pmbNhfAccount;
                            dbContext.TBL_CASA.Add(addCustomerAcct);

                            //insider related
                            //if (entity.isInsiderRelated == true)
                            //{
                            //    var insiderRelatedInfo = new TBL_CUSTOMER_RELATED_PARTY();
                            //    insiderRelatedInfo.COMPANYDIRECTORID = entity.insiderRelated.companyDirectorId;
                            //    insiderRelatedInfo.RELATIONSHIPTYPE = entity.insiderRelated.relationshipType;
                            //    insiderRelatedInfo.CUSTOMERID = customer.CUSTOMERID;
                            //    insiderRelatedInfo.CREATEDBY = staff.STAFFID;
                            //    insiderRelatedInfo.DATETIMECREATED = DateTime.Now;

                            //    //add to table
                            //    dbContext.TBL_CUSTOMER_RELATED_PARTY.Add(insiderRelatedInfo);
                            //}


                            //next of kin
                            //var nextOfKin = new TBL_CUSTOMER_NEXTOFKIN();
                            //nextOfKin.FIRSTNAME = entity.nextOfKin.firstName;
                            //nextOfKin.LASTNAME = entity.nextOfKin.lastName;
                            //nextOfKin.EMAIL = entity.nextOfKin.emailAddress;
                            //nextOfKin.DATEOFBIRTH = entity.nextOfKin.dateOfBirth;
                            //nextOfKin.GENDER = entity.nextOfKin.gender;
                            //nextOfKin.ADDRESS = entity.nextOfKin.contactAddress;
                            //nextOfKin.CITYID = entity.nextOfKin.cityId;
                            //nextOfKin.NEAREST_LANDMARK = entity.nextOfKin.nearestLandmark;
                            //nextOfKin.RELATIONSHIP = entity.nextOfKin.relationship;
                            //nextOfKin.PHONENUMBER = entity.nextOfKin.mobilePhoneNo;
                            //nextOfKin.ACTIVE = 1;
                            //nextOfKin.CUSTOMERID = customer.CUSTOMERID;

                            ////add to table
                            //dbContext.TBL_CUSTOMER_NEXTOFKIN.Add(nextOfKin);


                            // initiate auit table
                            var audit = new TBL_AUDIT
                            {
                                AUDITTYPEID = (short)AuditTypeEnum.CustomerAdded,
                                STAFFID = staff.STAFFID,
                                BRANCHID = (short)branch.BRANCHID,
                                DETAIL = $"(External) Added Customer  {entity.customerName} with Code: {entity.customerCode} ",
                                //IPADDRESS = entity.userIPAddress,
                                //URL = entity.applicationUrl,
                                APPLICATIONDATE = _genSetup.GetApplicationDate(),
                                SYSTEMDATETIME = DateTime.Now
                            };

                            // add the audit trail
                            auditTrail.AddAuditTrail(audit);

                            var output = dbContext.SaveChanges() > 0;
                            trans.Commit();
                            trans.Dispose();


                            var result = entity.customerCode;

                            if (output == true)
                            {
                                return result;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        catch (DbEntityValidationException ex)
                        {
                            trans.Rollback();

                            string errorMessages = string.Join("; ",
                            ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage));
                            throw new DbEntityValidationException(errorMessages);
                        }
                    }
                    catch (Exception exp)
                    {
                        trans.Rollback();
                        throw new SecureException(exp.Message);
                    }
                }
            }
        }

        public async Task<bool> KYCDocumentUpload(CustomerDocumentUploadViewModel model, byte[] file)
        {
            using (var dbcontext = new FinTrakBankingContext())
            {
                var customer = await dbcontext.TBL_CUSTOMER.Where(a => a.CUSTOMERCODE == model.customerCode).FirstOrDefaultAsync();
                if (customer == null)
                    throw new ConditionNotMetException($"Customer with customer code {model.customerCode} does not exist.");

                string fileExtendtion = model.fileExtension.Remove(0, 1);
                //  string[] chanelArray = new string[] { "docx", "pdf", "PDF", "jpg", "JPG", "jpeg", "JPEG", "png", "PNG", "txt", "xlsx", "xls", "DOC", "DOCX", "doc", "zip", "xml" };
                if (!CommonHelpers.FileTypes.Contains(fileExtendtion))
                {
                    throw new ConditionNotMetException("Kindly Upload Valid File with Accepted Extention " + CommonHelpers.FileTypes);
                }

                try
                {
                    if (model.documentTitle == null || string.IsNullOrEmpty(model.documentTitle))
                        model.documentTitle = "N/A";

                    using (var context = new FinTrakBankingDocumentsContext())
                    {
                        var data = new TBL_MEDIA_KYC_DOCUMENTS();
                        data.FILEDATA = file;
                        data.CUSTOMERID = customer.CUSTOMERID;
                        data.CUSTOMERCODE = customer.CUSTOMERCODE;
                        data.DOCUMENTTITLE = model.documentTitle;
                        data.DOCUMENTTYPEID = model.documentTypeId;
                        data.FILENAME = model.fileName;
                        data.FILEEXTENSION = model.fileExtension;
                        data.SYSTEMDATETIME = DateTime.Now;
                        data.PHYSICALFILENUMBER = string.Empty;
                        data.PHYSICALLOCATION = string.Empty;
                        data.CREATEDBY = customer.CREATEDBY;
                        data.DATECREATED = DateTime.Now;

                        context.TBL_MEDIA_KYC_DOCUMENTS.Add(data);

                        return context.SaveChanges() != 0;
                    }
                }
                catch (Exception ex)
                {
                    throw new SecureException(ex.Message);
                }
            }
        }


    }
}
