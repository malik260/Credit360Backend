using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.ViewModels.Customer;
using System.ServiceModel;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common;

namespace FintrakBanking.Repositories.Credit
{
    [Export(typeof(ILoanPreliminaryEvaluationRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class LoanPreliminaryEvaluationRepository : ILoanPreliminaryEvaluationRepository
    {
        private FinTrakBankingContext context;
        private IAuditTrailRepository auditTrail;
        private IGeneralSetupRepository genSetup;
        private IWorkflow workFlow;
        private IApprovalLevelStaffRepository level;


        public LoanPreliminaryEvaluationRepository(IAuditTrailRepository _auditTrail,
                                    IGeneralSetupRepository _genSetup, IWorkflow _workFlow,
        FinTrakBankingContext _context, IApprovalLevelStaffRepository _level)
        {
            context = _context;
            auditTrail = _auditTrail;
            genSetup = _genSetup;
            workFlow = _workFlow;
            level = _level;
        }

        private async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<LoanPreliminaryEvaluationViewModel> AddPreliminaryEvaluation(LoanPreliminaryEvaluationViewModel model)
        {
            if (model == null)
            {
                throw new SecureException("The data submitted in the form is invalid. Please try again");
            }

            bool output = false;

            var penRecord = new TBL_LOAN_PRELIMINARY_EVALUATN()
            {
                PRELIMINARYEVALUATIONCODE = GeneratePENCode(),
                BANKROLE = model.bankRole,
                BRANCHID = model.userBranchId,
                BUSINESSPROFILE = model.businessProfile,
                CLIENTDESCRIPTION = model.clientDescription,
                COLLATERALARRANGEMENT = model.collateralArrangement,
                RELATEDCOMPANIES = model.relatedCompanies,
                COMMERCIALVIABILITYASSESSMENT = model.commercialViabilityAssessment,
                COMPANYID = model.companyId,
                CUSTOMERID = model.customerId,
                CUSTOMERGROUPID = model.customerGroupId,
                ENVIRONMENTALIMPACT = model.environmentalImpact,
                EXISTINGEXPOSURE = model.existingExposure,
                REGISTRATIONNUMBER = model.registrationNumber,
                TAXIDENTIFICATIONNUMBER = model.taxIdentificationNumber,
                IMPLEMENTATIONARRANGEMENTS = model.implementationArrangements,
                MARKETDEMAND = model.marketDemand,
                OWNERSHIPSTRUCTURE = model.ownershipStructure,
                PORTFOLIOSTRATEGICALIGNMENT = model.portfolioStrategicAlignment,
                PROJECTDESCRIPTION = model.projectDescription,
                PROJECTFINANCINGPLAN = model.projectFinancingPlan,
                LOANAPPLICATIONTYPEID = model.loanApplicationtypeId,
                PROPOSEDTERMSANDCONDITIONS = model.proposedTermsAndConditions,
                RISKSANDCONCERNS = model.risksAndConcerns,
                PRUDENT_EXPOSUR_LIMIT_IMPLCATN = model.prudentialExposureLimitImplications,
                RELATIONSHIPMANAGERID = model.relationshipManagerId,
                RELATIONSHIPOFFICERID = model.relationshipOfficerId,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                ISCURRENT = model.isCurrent,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                CREATEDBY = model.createdBy,
                LOANAMOUNT = model.loanAmount,
                //LOANTYPEID = model.loanTypeId,
                // SubSectorId = model.subSectorId,
                PRODUCTCLASSID = model.productClassId,
                CAPREGIONID = model.capRegionId
            };

            var customerRecord = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == model.customerId);

            var groupRecord = context.TBL_CUSTOMER_GROUP.FirstOrDefault(c => c.CUSTOMERGROUPID == model.customerGroupId);
            var detail = string.Empty;
            if (customerRecord != null)
            {
                detail = $"Created Prelimenary Evaluation with code ({model.preliminaryEvaluationCode}) for customer with code {customerRecord.CUSTOMERCODE}";
            }
            else
            {
                detail = $"Created Prelimenary Evaluation with code ({model.preliminaryEvaluationCode})";
            }
                

            var auditRecord = new TBL_AUDIT()
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanPreliminaryEvaluationAdded,
                BRANCHID = model.userBranchId,
                STAFFID = model.createdBy,
                DETAIL = detail,
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                SYSTEMDATETIME = DateTime.Now,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
            };

            if (model.customerId != null)
            {
                auditRecord.DETAIL =
                    $"Created Prelimenary Evaluation Note with code ({model.preliminaryEvaluationCode}) for customer {customerRecord?.FIRSTNAME} {customerRecord?.LASTNAME}";
            }
            else
            {
                auditRecord.DETAIL = $"Created Prelimenary Evaluation with code ({model.preliminaryEvaluationCode}) for customer group {groupRecord?.GROUPNAME}";
            }

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    context.TBL_LOAN_PRELIMINARY_EVALUATN.Add(penRecord);
                    context.TBL_AUDIT.Add(auditRecord);

                    output = await SaveAllAsync();

                    if (model.sendForEvaluation)
                    {
                        workFlow.StaffId = penRecord.CREATEDBY;
                        workFlow.CompanyId = penRecord.COMPANYID;
                        workFlow.StatusId = (int)ApprovalStatusEnum.Pending;
                        workFlow.TargetId = penRecord.LOANPRELIMINARYEVALUATIONID;
                        workFlow.Comment = "Request Preliminary Evaluation";
                        workFlow.OperationId = (int)OperationsEnum.LoanPreliminaryEvaluation;
                        workFlow.DeferredExecution = true;
                        workFlow.ExternalInitialization = true;

                        workFlow.LogActivity();
                        context.SaveChanges();
                    }

                    trans.Commit();

                    if (output)
                    {
                        return new LoanPreliminaryEvaluationViewModel
                        {
                            preliminaryEvaluationCode = penRecord.PRELIMINARYEVALUATIONCODE,
                            sendForEvaluation = model.sendForEvaluation
                        };
                    }
                    return null;
                }

                catch (Exception ex)
                {
                    trans.Rollback();

                    throw new SecureException(ex.Message);
                }
            }

        }


        public async Task<LoanPreliminaryEvaluationViewModel> AddMultiplePreliminaryEvaluation(List<LoanPreliminaryEvaluationViewModel> model)
        {
            if (model == null)
            {
                throw new SecureException("The data submitted in the form is invalid. Please try again");
            }

            bool output = false;

            List<string> groupPenCodes = new List<string>();

            TBL_LOAN_PRELIMINARY_EVALUATN penRecord = new TBL_LOAN_PRELIMINARY_EVALUATN();

            foreach (var item in model)
            {
                penRecord = new TBL_LOAN_PRELIMINARY_EVALUATN()
                {
                    PRELIMINARYEVALUATIONCODE = GeneratePENCode(),
                    BANKROLE = item.bankRole,
                    BRANCHID = item.userBranchId,
                    BUSINESSPROFILE = item.businessProfile,
                    CLIENTDESCRIPTION = item.clientDescription,
                    COLLATERALARRANGEMENT = item.collateralArrangement,
                    COMMERCIALVIABILITYASSESSMENT = item.commercialViabilityAssessment,
                    COMPANYID = item.companyId,
                    CUSTOMERID = item.customerId,
                    ENVIRONMENTALIMPACT = item.environmentalImpact,
                    EXISTINGEXPOSURE = item.existingExposure,
                    REGISTRATIONNUMBER = item.registrationNumber,
                    TAXIDENTIFICATIONNUMBER = item.taxIdentificationNumber,
                    IMPLEMENTATIONARRANGEMENTS = item.implementationArrangements,
                    MARKETDEMAND = item.marketDemand,
                    OWNERSHIPSTRUCTURE = item.ownershipStructure,
                    PORTFOLIOSTRATEGICALIGNMENT = item.portfolioStrategicAlignment,
                    PROJECTDESCRIPTION = item.projectDescription,
                    PROJECTFINANCINGPLAN = item.projectFinancingPlan,
                    PROPOSEDTERMSANDCONDITIONS = item.proposedTermsAndConditions,
                    RISKSANDCONCERNS = item.risksAndConcerns,
                    PRUDENT_EXPOSUR_LIMIT_IMPLCATN = item.prudentialExposureLimitImplications,
                    RELATIONSHIPMANAGERID = item.relationshipManagerId,
                    RELATIONSHIPOFFICERID = item.relationshipOfficerId,
                    APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
                    ISCURRENT = item.isCurrent,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    CREATEDBY = item.createdBy,
                    LOANAMOUNT = item.loanAmount,
                    //LOANTYPEID = item.loanTypeId,
                    SUBSECTORID = item.subSectorId,
                    PRODUCTCLASSID = item.productClassId,
                    CAPREGIONID = item.capRegionId

                };

                context.TBL_LOAN_PRELIMINARY_EVALUATN.Add(penRecord);

                groupPenCodes.Add(penRecord.PRELIMINARYEVALUATIONCODE);

                var customerRecord = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == item.customerId);

                var auditRecord = new TBL_AUDIT()
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanPreliminaryEvaluationAdded,
                    BRANCHID = item.userBranchId,
                    STAFFID = item.createdBy,
                    DETAIL = $"Created Prelimenary Evaluation with code ({penRecord.PRELIMINARYEVALUATIONCODE}) for customer {customerRecord.FIRSTNAME} {customerRecord.LASTNAME}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = item.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    
                };

                using (var trans = context.Database.BeginTransaction())
                {
                    try
                    {
                        context.TBL_AUDIT.Add(auditRecord);

                        output = await SaveAllAsync();

                        if (item.sendForEvaluation)
                        {
                            workFlow.StaffId = penRecord.CREATEDBY;
                            workFlow.CompanyId = penRecord.COMPANYID;
                            workFlow.StatusId = (int)ApprovalStatusEnum.Pending;
                            workFlow.TargetId = penRecord.LOANPRELIMINARYEVALUATIONID;
                            workFlow.Comment = "Request Preliminary Evaluation";
                            workFlow.OperationId = (int)OperationsEnum.LoanPreliminaryEvaluation;
                            workFlow.DeferredExecution = true;
                            workFlow.ExternalInitialization = true;

                            workFlow.LogActivity();

                            context.SaveChanges();

                            trans.Commit();
                        }
                        else trans.Commit();
                    }

                    catch (Exception ex)
                    {
                        trans.Rollback();

                        throw new SecureException(ex.Message);
                    }
                }
            }

            if (output)
            {
                var codes = string.Empty;

                foreach (var code in groupPenCodes)
                {
                    codes += code;
                }
                return new LoanPreliminaryEvaluationViewModel
                {
                    preliminaryEvaluationCode = codes
                };
            }
            return null;
        }

        private string GeneratePENCode()
        {
            var data = this.context.TBL_LOAN_PRELIMINARY_EVALUATN.Count();
            int counter = data + 1;
            var penCode = string.Empty;

            //penCode = $"PEN -- {counter.ToString().PadLeft(10, '0')}";
            penCode = $"PEN -- { CommonHelpers.GenerateZeroString(5) + counter.ToString().Right(5)}";
            
            return penCode;
        }

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetLoanPreliminaryEvaluationsAwaitingApprovalByLoanTypeId(int staffId, int companyId, int loanTypeId)
        {
            int[] regionIds = GetStaffRegions(staffId);

            if (regionIds.Length > 0)
                return GetCustomerPreliminaryEvaluationsByAtaffRegionAwaitingApproval(staffId, companyId, regionIds);


            switch (loanTypeId)
            {
                case (int)LoanTypeEnum.Single:
                    return GetSingleCustomerPreliminaryEvaluationsAwaitingApproval(staffId, companyId);

                case (int)LoanTypeEnum.CustomerGroup:
                    return GetGroupCustomerPreliminaryEvaluationsAwaitingApproval(staffId, companyId);

                default:
                    return new List<LoanPreliminaryEvaluationViewModel>();
            }
        }

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetSingleCustomerPreliminaryEvaluationsAwaitingApproval(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanPreliminaryEvaluation).ToList();

            var data = (from pen in context.TBL_LOAN_PRELIMINARY_EVALUATN
                        join coy in context.TBL_COMPANY on pen.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on pen.BRANCHID equals br.BRANCHID
                        join atrail in context.TBL_APPROVAL_TRAIL on pen.LOANPRELIMINARYEVALUATIONID equals atrail.TARGETID
                        where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                            && pen.ISCURRENT == true 
                            //&& pen.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.Single 
                            && atrail.RESPONSESTAFFID == null
                            && atrail.OPERATIONID == (int)OperationsEnum.LoanPreliminaryEvaluation 
                            && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                            orderby pen.LOANPRELIMINARYEVALUATIONID descending
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            companyId = pen.COMPANYID,
                            companyName = pen.TBL_COMPANY.NAME,
                            loanPreliminaryEvaluationId = pen.LOANPRELIMINARYEVALUATIONID,
                            preliminaryEvaluationCode = pen.PRELIMINARYEVALUATIONCODE,
                            bankRole = pen.BANKROLE,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            businessProfile = pen.BUSINESSPROFILE,
                            clientDescription = pen.CLIENTDESCRIPTION,
                            collateralArrangement = pen.COLLATERALARRANGEMENT,
                            commercialViabilityAssessment = pen.COMMERCIALVIABILITYASSESSMENT,
                            customerId = pen.CUSTOMERID,
                            customerName = pen.TBL_CUSTOMER.FIRSTNAME + " " + pen.TBL_CUSTOMER.LASTNAME,
                            customerGroupId = pen.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID,
                            customerGroupName = pen.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = pen.TBL_CUSTOMER_GROUP.GROUPCODE,
                            customerCode = pen.TBL_CUSTOMER.CUSTOMERCODE,
                            environmentalImpact = pen.ENVIRONMENTALIMPACT,
                            existingExposure = pen.EXISTINGEXPOSURE,
                            implementationArrangements = pen.IMPLEMENTATIONARRANGEMENTS,
                            marketDemand = pen.MARKETDEMAND,
                            ownershipStructure = pen.OWNERSHIPSTRUCTURE,
                            portfolioStrategicAlignment = pen.PORTFOLIOSTRATEGICALIGNMENT,
                            projectDescription = pen.PROJECTDESCRIPTION,
                            projectFinancingPlan = pen.PROJECTFINANCINGPLAN,
                            proposedTermsAndConditions = pen.PROPOSEDTERMSANDCONDITIONS,
                            risksAndConcerns = pen.RISKSANDCONCERNS,
                            prudentialExposureLimitImplications = pen.PRUDENT_EXPOSUR_LIMIT_IMPLCATN,
                            relationshipManagerId = pen.RELATIONSHIPMANAGERID,
                            relationshipOfficerId = pen.RELATIONSHIPOFFICERID,
                            taxIdentificationNumber = pen.TAXIDENTIFICATIONNUMBER,
                            registrationNumber = pen.REGISTRATIONNUMBER,
                            operationId = (int)OperationsEnum.LoanPreliminaryEvaluation,
                            dateTimeCreated = pen.DATETIMECREATED,
                            customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == pen.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                            {
                                bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                customerBvnid = b.CUSTOMERBVNID,
                                firstname = b.FIRSTNAME,
                                isValidBvn = b.ISVALIDBVN,
                                isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                surname = b.SURNAME
                            }).ToList(),
                            customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == pen.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                            .Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == pen.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                            .Select(s => new CustomerCompanyShareholdersViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == pen.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                            .Select(cs => new CustomerClientOrSupplierViewModels()
                            {
                                client_SupplierId = cs.CLIENT_SUPPLIERID,
                                clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                firstName = cs.FIRSTNAME,
                                middleName = cs.MIDDLENAME,
                                lastName = cs.LASTNAME,
                                client_SupplierAddress = cs.ADDRESS,
                                client_SupplierPhoneNumber = cs.PHONENUMBER,
                                client_SupplierEmail = cs.EMAILADDRESS,
                                client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            }).ToList(),
                            customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == pen.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                             .Select(cs => new CustomerSupplierViewModels()
                             {
                                 client_SupplierId = cs.CLIENT_SUPPLIERID,
                                 clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                 firstName = cs.FIRSTNAME,
                                 middleName = cs.MIDDLENAME,
                                 lastName = cs.LASTNAME,
                                 client_SupplierAddress = cs.ADDRESS,
                                 client_SupplierPhoneNumber = cs.PHONENUMBER,
                                 client_SupplierEmail = cs.EMAILADDRESS,
                                 client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                 client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                             }).ToList(),
                            loanAmount = pen.LOANAMOUNT,
                            loanTypeId = pen.LOANAPPLICATIONTYPEID,
                            loanTypeName = pen.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productClassId = (short)pen.PRODUCTCLASSID,
                            productClassName = pen.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            capRegionId=pen.CAPREGIONID
                            //subSectorId = pen.SubSectorId,
                            //subSectorName = pen.tbl_Sub_Sector.Name,
                            //sectorId = context.tbl_Sub_Sector.FirstOrDefault(x => x.SubSectorId == pen.SubSectorId).SectorId ?? 0,
                        });
            return data;
        }

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetGroupCustomerPreliminaryEvaluationsAwaitingApproval(int staffId, int companyId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanPreliminaryEvaluation).ToList();

            var data = (from pen in context.TBL_LOAN_PRELIMINARY_EVALUATN
                        join coy in context.TBL_COMPANY on pen.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on pen.BRANCHID equals br.BRANCHID
                        join atrail in context.TBL_APPROVAL_TRAIL on pen.LOANPRELIMINARYEVALUATIONID equals atrail.TARGETID
                        where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                        && pen.ISCURRENT == true 
                        //&& pen.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup 
                        && atrail.RESPONSESTAFFID == null
                        && atrail.OPERATIONID == (int)OperationsEnum.LoanPreliminaryEvaluation 
                        && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                        orderby pen.LOANPRELIMINARYEVALUATIONID descending
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            companyId = pen.COMPANYID,
                            companyName = pen.TBL_COMPANY.NAME,
                            loanPreliminaryEvaluationId = pen.LOANPRELIMINARYEVALUATIONID,
                            preliminaryEvaluationCode = pen.PRELIMINARYEVALUATIONCODE,
                            bankRole = pen.BANKROLE,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            businessProfile = pen.BUSINESSPROFILE,
                            clientDescription = pen.CLIENTDESCRIPTION,
                            collateralArrangement = pen.COLLATERALARRANGEMENT,
                            commercialViabilityAssessment = pen.COMMERCIALVIABILITYASSESSMENT,
                            customerId = pen.CUSTOMERID,
                            customerName = pen.TBL_CUSTOMER.FIRSTNAME + " " + pen.TBL_CUSTOMER.LASTNAME,
                            customerGroupId = pen.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID,
                            customerGroupName = pen.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = pen.TBL_CUSTOMER_GROUP.GROUPCODE,
                            customerCode = pen.TBL_CUSTOMER.CUSTOMERCODE,
                            environmentalImpact = pen.ENVIRONMENTALIMPACT,
                            existingExposure = pen.EXISTINGEXPOSURE,
                            implementationArrangements = pen.IMPLEMENTATIONARRANGEMENTS,
                            marketDemand = pen.MARKETDEMAND,
                            ownershipStructure = pen.OWNERSHIPSTRUCTURE,
                            portfolioStrategicAlignment = pen.PORTFOLIOSTRATEGICALIGNMENT,
                            projectDescription = pen.PROJECTDESCRIPTION,
                            projectFinancingPlan = pen.PROJECTFINANCINGPLAN,
                            proposedTermsAndConditions = pen.PROPOSEDTERMSANDCONDITIONS,
                            risksAndConcerns = pen.RISKSANDCONCERNS,
                            prudentialExposureLimitImplications = pen.PRUDENT_EXPOSUR_LIMIT_IMPLCATN,
                            relationshipManagerId = pen.RELATIONSHIPMANAGERID,
                            relationshipOfficerId = pen.RELATIONSHIPOFFICERID,
                            taxIdentificationNumber = pen.TAXIDENTIFICATIONNUMBER,
                            registrationNumber = pen.REGISTRATIONNUMBER,
                            operationId = atrail.OPERATIONID,
                            dateTimeCreated = pen.DATETIMECREATED,
                            customerGroupMappings = context.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERGROUPID == pen.CUSTOMERGROUPID).Select(s => new CustomerGroupMappingViewModel()
                            {
                                customerId = s.CUSTOMERID,
                                customerName = s.TBL_CUSTOMER.FIRSTNAME + " " + s.TBL_CUSTOMER.LASTNAME,
                                customerCode = s.TBL_CUSTOMER.CUSTOMERCODE,
                                //customerAccountNumber = context.tbl_CASA.FirstOrDefault(x => x.CustomerId == p.CustomerId).ProductAccountNumber,
                                //customerTypeId = context.tbl_Customer.FirstOrDefault(x => x.CustomerId == p.CustomerId).CustomerTypeId,
                                customerType = s.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                relationshipTypeId = s.RELATIONSHIPTYPEID,
                                relationshipTypeName = s.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                                productAccountNumber = context.TBL_CASA.FirstOrDefault(x => x.CUSTOMERID == s.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                                taxIdentificationNumber = s.TBL_CUSTOMER.TAXNUMBER,
                                registrationNumber = s.TBL_CUSTOMER.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == s.CUSTOMERID).REGISTRATIONNUMBER,
                                customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == s.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                                {
                                    bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                    customerBvnid = b.CUSTOMERBVNID,
                                    firstname = b.FIRSTNAME,
                                    isValidBvn = b.ISVALIDBVN,
                                    isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                    surname = b.SURNAME
                                }).ToList(),
                                customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                   .Where(cd => cd.CUSTOMERID == s.CUSTOMERID && cd.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                               .Select(x => new CustomerCompanyDirectorsViewModels()
                               {
                                   bankVerificationNumber = x.CUSTOMERBVN,
                                   companyDirectorTypeId = x.COMPANYDIRECTORTYPEID,
                                   companyDirectorTypeName = x.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                   customerId = x.CUSTOMERID,
                                   firstname = x.FIRSTNAME,
                                   surname = x.SURNAME
                               }).ToList(),
                                customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                   .Where(cc => cc.CUSTOMERID == s.CUSTOMERID && cc.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                               .Select(cs => new CustomerCompanyShareholdersViewModels()
                               {
                                   bankVerificationNumber = cs.CUSTOMERBVN,
                                   companyDirectorTypeId = cs.COMPANYDIRECTORTYPEID,
                                   companyDirectorTypeName = cs.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                   customerId = cs.CUSTOMERID,
                                   firstname = cs.FIRSTNAME,
                                   surname = cs.SURNAME
                               }).ToList(),
                                customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER
                                   .Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                               .Select(cs => new CustomerClientOrSupplierViewModels()
                               {
                                   client_SupplierId = cs.CLIENT_SUPPLIERID,
                                   clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                   firstName = cs.FIRSTNAME,
                                   middleName = cs.MIDDLENAME,
                                   lastName = cs.LASTNAME,
                                   client_SupplierAddress = cs.ADDRESS,
                                   client_SupplierPhoneNumber = cs.PHONENUMBER,
                                   client_SupplierEmail = cs.EMAILADDRESS,
                                   client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                   client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                               }).ToList(),
                                customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER
                                   .Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                               .Select(cs => new CustomerSupplierViewModels()
                               {
                                   client_SupplierId = cs.CLIENT_SUPPLIERID,
                                   clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                   firstName = cs.FIRSTNAME,
                                   middleName = cs.MIDDLENAME,
                                   lastName = cs.LASTNAME,
                                   client_SupplierAddress = cs.ADDRESS,
                                   client_SupplierPhoneNumber = cs.PHONENUMBER,
                                   client_SupplierEmail = cs.EMAILADDRESS,
                                   client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                   client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                               }).ToList(),
                            }).ToList(),
                            approvalStatusId = pen.APPROVALSTATUSID,
                            sentForLoanApplication = pen.SENTFORLOANAPPLICATION,
                            sendForEvaluation = pen.SENTFOREVALUATION,
                            loanAmount = pen.LOANAMOUNT,
                            loanTypeId = pen.LOANAPPLICATIONTYPEID,
                            loanTypeName = pen.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productClassId = (short)pen.PRODUCTCLASSID,
                            productClassName = pen.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            capRegionId = pen.CAPREGIONID
                        });
            return data;
        }

        public bool GoForApproval(ApprovalViewModel entity)
        {
            entity.operationId = (int)OperationsEnum.LoanPreliminaryEvaluation;

            entity.externalInitialization = false;

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    workFlow.StaffId = entity.createdBy;
                    workFlow.CompanyId = entity.companyId;
                    workFlow.StatusId = (short)entity.approvalStatusId == (short)ApprovalStatusEnum.Approved ? (short)ApprovalStatusEnum.Processing : (short)entity.approvalStatusId;
                    workFlow.TargetId = entity.targetId;
                    workFlow.Comment = entity.comment;
                    workFlow.OperationId = (int)OperationsEnum.LoanPreliminaryEvaluation;
                    workFlow.DeferredExecution = true;
                    workFlow.LogActivity();
                    context.SaveChanges();

                    if (workFlow.NewState == (int)ApprovalState.Ended)
                    {
                        if (entity.approvalStatusId == (short)ApprovalStatusEnum.Disapproved)
                            return false;

                        var response = ApprovePreliminaryEvaluation(entity.targetId, (short)workFlow.StatusId, entity);

                        if (response)
                        {
                            trans.Commit();
                        }
                        return true;
                    }
                    else
                    {
                        trans.Commit();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }
        }

        private bool ApprovePreliminaryEvaluation(int loanPenId, short approvalStatusId, UserInfo user)
        {
            var penRecord = context.TBL_LOAN_PRELIMINARY_EVALUATN.Find(loanPenId);

            penRecord.ISCURRENT = false;
            penRecord.APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved;
            penRecord.DATEAPPROVED = DateTime.Now;
            penRecord.DATETIMEUPDATED = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanPreliminaryEvaluationAdded,
                STAFFID = user.staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Approved Prelimenary Evaluation with code ({penRecord.PRELIMINARYEVALUATIONCODE})",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            return this.context.SaveChanges() > 0;

        }

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetLoanPreliminaryEvaluationsByLoanTypeId(int loanTypeId)
        {
           
            switch (loanTypeId)
            {
                case (int)LoanTypeEnum.Single:
                    return GetAllSingleCustomerLoanPreliminaryEvaluations();

                case (int)LoanTypeEnum.CustomerGroup:
                    return GetAllGroupCustomerLoanPreliminaryEvaluations();

                default:
                    return new List<LoanPreliminaryEvaluationViewModel>();
            }
        }

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetLoanApplicationPreliminaryEvaluations(int applicationId)
        {
            var appl = context.TBL_LOAN_APPLICATION.Find(applicationId);
            var applicationPenId = appl.LOANPRELIMINARYEVALUATIONID;

            if (applicationPenId == null) return new List<LoanPreliminaryEvaluationViewModel>();

            // return GetAllSingleCustomerLoanPreliminaryEvaluations().Where(x => x.loanPreliminaryEvaluationId == applicationPenId).ToList();
            
            var data = (from pen in context.TBL_LOAN_PRELIMINARY_EVALUATN
                        join coy in context.TBL_COMPANY on pen.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on pen.BRANCHID equals br.BRANCHID
                        //join atrail in context.TBL_APPROVAL_TRAIL on pen.LOANPRELIMINARYEVALUATIONID equals atrail.TARGETID
                        where 
                        //(atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved)
                            //&& pen.ISCURRENT == true
                             pen.LOANPRELIMINARYEVALUATIONID == applicationPenId
                            //&& atrail.RESPONSESTAFFID == null
                            //&& atrail.OPERATIONID == (int)OperationsEnum.LoanPreliminaryEvaluation
                        orderby pen.LOANPRELIMINARYEVALUATIONID descending
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            companyId = pen.COMPANYID,
                            companyName = pen.TBL_COMPANY.NAME,
                            loanPreliminaryEvaluationId = pen.LOANPRELIMINARYEVALUATIONID,
                            preliminaryEvaluationCode = pen.PRELIMINARYEVALUATIONCODE,
                            bankRole = pen.BANKROLE,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            businessProfile = pen.BUSINESSPROFILE,
                            clientDescription = pen.CLIENTDESCRIPTION,
                            collateralArrangement = pen.COLLATERALARRANGEMENT,
                            commercialViabilityAssessment = pen.COMMERCIALVIABILITYASSESSMENT,
                            customerId = pen.CUSTOMERID,
                            customerName = pen.TBL_CUSTOMER.FIRSTNAME + " " + pen.TBL_CUSTOMER.LASTNAME,
                            customerGroupId = pen.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID,
                            customerGroupName = pen.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = pen.TBL_CUSTOMER_GROUP.GROUPCODE,
                            customerCode = pen.TBL_CUSTOMER.CUSTOMERCODE,
                            environmentalImpact = pen.ENVIRONMENTALIMPACT,
                            existingExposure = pen.EXISTINGEXPOSURE,
                            implementationArrangements = pen.IMPLEMENTATIONARRANGEMENTS,
                            marketDemand = pen.MARKETDEMAND,
                            ownershipStructure = pen.OWNERSHIPSTRUCTURE,
                            portfolioStrategicAlignment = pen.PORTFOLIOSTRATEGICALIGNMENT,
                            projectDescription = pen.PROJECTDESCRIPTION,
                            projectFinancingPlan = pen.PROJECTFINANCINGPLAN,
                            proposedTermsAndConditions = pen.PROPOSEDTERMSANDCONDITIONS,
                            risksAndConcerns = pen.RISKSANDCONCERNS,
                            prudentialExposureLimitImplications = pen.PRUDENT_EXPOSUR_LIMIT_IMPLCATN,
                            relationshipManagerId = pen.RELATIONSHIPMANAGERID,
                            relationshipOfficerId = pen.RELATIONSHIPOFFICERID,
                            taxIdentificationNumber = pen.TAXIDENTIFICATIONNUMBER,
                            registrationNumber = pen.REGISTRATIONNUMBER,
                            approvalStatusId =pen.APPROVALSTATUSID,
                            operationId = (int)OperationsEnum.LoanPreliminaryEvaluation,
                            dateTimeCreated = pen.DATETIMECREATED,
                            customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == pen.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                            {
                                bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                customerBvnid = b.CUSTOMERBVNID,
                                firstname = b.FIRSTNAME,
                                isValidBvn = b.ISVALIDBVN,
                                isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                surname = b.SURNAME
                            }).ToList(),
                            customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == pen.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                            .Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == pen.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                            .Select(s => new CustomerCompanyShareholdersViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == pen.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                            .Select(cs => new CustomerClientOrSupplierViewModels()
                            {
                                client_SupplierId = cs.CLIENT_SUPPLIERID,
                                clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                firstName = cs.FIRSTNAME,
                                middleName = cs.MIDDLENAME,
                                lastName = cs.LASTNAME,
                                client_SupplierAddress = cs.ADDRESS,
                                client_SupplierPhoneNumber = cs.PHONENUMBER,
                                client_SupplierEmail = cs.EMAILADDRESS,
                                client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            }).ToList(),
                            customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == pen.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                             .Select(cs => new CustomerSupplierViewModels()
                             {
                                 client_SupplierId = cs.CLIENT_SUPPLIERID,
                                 clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                 firstName = cs.FIRSTNAME,
                                 middleName = cs.MIDDLENAME,
                                 lastName = cs.LASTNAME,
                                 client_SupplierAddress = cs.ADDRESS,
                                 client_SupplierPhoneNumber = cs.PHONENUMBER,
                                 client_SupplierEmail = cs.EMAILADDRESS,
                                 client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                 client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                             }).ToList(),
                            loanAmount = pen.LOANAMOUNT,
                            loanTypeId = pen.LOANAPPLICATIONTYPEID,
                            loanTypeName = pen.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productClassId = (short)pen.PRODUCTCLASSID,
                            productClassName = pen.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            capRegionId = pen.CAPREGIONID
                        });
            return data.ToList();
        }

        public IEnumerable<LookupViewModel> GetCustomerLoanPreliminaryEvaluations(int customerId, int loanTypeId, int customerGroupId = 0)
        {
            if ((int)LoanTypeEnum.Single == loanTypeId)
            {
                var data = (from p in context.TBL_LOAN_PRELIMINARY_EVALUATN
                            where p.CUSTOMERID == customerId && p.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                            select new LookupViewModel()
                            {
                                lookupId = (short)p.LOANPRELIMINARYEVALUATIONID,
                                lookupName = p.PRELIMINARYEVALUATIONCODE,
                            }).ToList();
                return data;
            }
            else 
            {
                var data = (from p in context.TBL_LOAN_PRELIMINARY_EVALUATN
                            join g in context. TBL_CUSTOMER_GROUP_MAPPING on p.CUSTOMERGROUPID equals g.CUSTOMERGROUPID
                            where p.CUSTOMERGROUPID == customerGroupId && p.APPROVALSTATUSID == (short)ApprovalStatusEnum.Approved
                            select new LookupViewModel()
                            {
                                lookupId = (short)p.LOANPRELIMINARYEVALUATIONID,
                                lookupName = p.PRELIMINARYEVALUATIONCODE,
                            }).ToList();
                return data;
            }
            
        }

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetLoanPreliminaryEvaluationMappedToApplication()
        {
            var data = (from p in context.TBL_LOAN_PRELIMINARY_EVALUATN
                        join coy in context.TBL_COMPANY on p.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on p.BRANCHID equals br.BRANCHID
                        where p.SENTFORLOANAPPLICATION == true
                        orderby p.LOANPRELIMINARYEVALUATIONID descending
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            companyId = p.COMPANYID,
                            companyName = p.TBL_COMPANY.NAME,
                            loanPreliminaryEvaluationId = p.LOANPRELIMINARYEVALUATIONID,
                            preliminaryEvaluationCode = p.PRELIMINARYEVALUATIONCODE,
                            bankRole = p.BANKROLE,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            businessProfile = p.BUSINESSPROFILE,
                            clientDescription = p.CLIENTDESCRIPTION,
                            collateralArrangement = p.COLLATERALARRANGEMENT,
                            commercialViabilityAssessment = p.COMMERCIALVIABILITYASSESSMENT,
                            customerId = p.CUSTOMERID.Value,
                            customerName = p.TBL_CUSTOMER.FIRSTNAME + " " + p.TBL_CUSTOMER.LASTNAME,
                            customerCode = p.TBL_CUSTOMER.CUSTOMERCODE,
                            customerGroupId = p.CUSTOMERGROUPID.Value,
                            customerGroupName = p.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = p.TBL_CUSTOMER_GROUP.GROUPCODE,
                            customerAccountNumber = context.TBL_CASA.FirstOrDefault(x => x.CUSTOMERID == p.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                            //customerTypeId = context.tbl_Customer.FirstOrDefault(x => x.CustomerId == p.CustomerId).CustomerTypeId,
                            environmentalImpact = p.ENVIRONMENTALIMPACT,
                            existingExposure = p.EXISTINGEXPOSURE,
                            implementationArrangements = p.IMPLEMENTATIONARRANGEMENTS,
                            marketDemand = p.MARKETDEMAND,
                            ownershipStructure = p.OWNERSHIPSTRUCTURE,
                            portfolioStrategicAlignment = p.PORTFOLIOSTRATEGICALIGNMENT,
                            projectDescription = p.PROJECTDESCRIPTION,
                            projectFinancingPlan = p.PROJECTFINANCINGPLAN,
                            proposedTermsAndConditions = p.PROPOSEDTERMSANDCONDITIONS,
                            risksAndConcerns = p.RISKSANDCONCERNS,
                            prudentialExposureLimitImplications = p.PRUDENT_EXPOSUR_LIMIT_IMPLCATN,
                            relationshipManagerId = p.RELATIONSHIPMANAGERID,
                            relationshipOfficerId = p.RELATIONSHIPOFFICERID,
                            taxIdentificationNumber = p.TAXIDENTIFICATIONNUMBER,
                            registrationNumber = p.REGISTRATIONNUMBER,
                            operationId = (short)OperationsEnum.LoanPreliminaryEvaluation,
                            customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == p.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                            {
                                bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                customerBvnid = b.CUSTOMERBVNID,
                                firstname = b.FIRSTNAME,
                                isValidBvn = b.ISVALIDBVN,
                                isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                surname = b.SURNAME
                            }).ToList(),
                            customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == p.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                            .Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == p.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                            .Select(s => new CustomerCompanyShareholdersViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == p.CUSTOMERID &&
                           cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                            .Select(cs => new CustomerClientOrSupplierViewModels()
                            {
                                client_SupplierId = cs.CLIENT_SUPPLIERID,
                                clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                firstName = cs.FIRSTNAME,
                                middleName = cs.MIDDLENAME,
                                lastName = cs.LASTNAME,
                                client_SupplierAddress = cs.ADDRESS,
                                client_SupplierPhoneNumber = cs.PHONENUMBER,
                                client_SupplierEmail = cs.EMAILADDRESS,
                                client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            }).ToList(),
                            customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == p.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                             .Select(cs => new CustomerSupplierViewModels()
                             {
                                 client_SupplierId = cs.CLIENT_SUPPLIERID,
                                 clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                 firstName = cs.FIRSTNAME,
                                 middleName = cs.MIDDLENAME,
                                 lastName = cs.LASTNAME,
                                 client_SupplierAddress = cs.ADDRESS,
                                 client_SupplierPhoneNumber = cs.PHONENUMBER,
                                 client_SupplierEmail = cs.EMAILADDRESS,
                                 client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                 client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                             }).ToList(),
                            approvalStatusId = p.APPROVALSTATUSID,
                            dateTimeCreated = p.DATETIMECREATED,
                            sentForLoanApplication = p.SENTFORLOANAPPLICATION,
                            sendForEvaluation = p.SENTFOREVALUATION,
                            loanAmount = p.LOANAMOUNT,
                            loanTypeId = p.LOANAPPLICATIONTYPEID,
                            loanTypeName = p.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productClassId = (short)p.PRODUCTCLASSID,
                            productClassName = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            capRegionId = p.CAPREGIONID
                            //subSectorId = p.SubSectorId,
                            //subSectorName = p.tbl_Sub_Sector.Name,
                            //sectorId = context.tbl_Sub_Sector.FirstOrDefault(x => x.SubSectorId == p.SubSectorId).SectorId ?? 0,
                        });



            return data;
        }

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetAllSingleCustomerLoanPreliminaryEvaluations()
        {
            var data = (from p in context.TBL_LOAN_PRELIMINARY_EVALUATN
                        join coy in context.TBL_COMPANY on p.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on p.BRANCHID equals br.BRANCHID
                        where //p.ISCURRENT == false //&& p.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.Single 
                        p.SENTFORLOANAPPLICATION == false
                        orderby p.LOANPRELIMINARYEVALUATIONID descending
                        //|| p.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending || p.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            companyId = p.COMPANYID,
                            companyName = p.TBL_COMPANY.NAME,
                            loanPreliminaryEvaluationId = p.LOANPRELIMINARYEVALUATIONID,
                            preliminaryEvaluationCode = p.PRELIMINARYEVALUATIONCODE,
                            bankRole = p.BANKROLE,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            businessProfile = p.BUSINESSPROFILE,
                            clientDescription = p.CLIENTDESCRIPTION,
                            collateralArrangement = p.COLLATERALARRANGEMENT,
                            commercialViabilityAssessment = p.COMMERCIALVIABILITYASSESSMENT,
                            customerId = p.CUSTOMERID.Value,
                            customerName = p.TBL_CUSTOMER.FIRSTNAME + " " + p.TBL_CUSTOMER.LASTNAME,
                            customerCode = p.TBL_CUSTOMER.CUSTOMERCODE,
                            customerGroupId = p.CUSTOMERGROUPID.Value,
                            customerGroupName = p.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = p.TBL_CUSTOMER_GROUP.GROUPCODE,
                            customerAccountNumber = context.TBL_CASA.FirstOrDefault(x => x.CUSTOMERID == p.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                            //customerTypeId = context.tbl_Customer.FirstOrDefault(x => x.CustomerId == p.CustomerId).CustomerTypeId,
                            environmentalImpact = p.ENVIRONMENTALIMPACT,
                            existingExposure = p.EXISTINGEXPOSURE,
                            implementationArrangements = p.IMPLEMENTATIONARRANGEMENTS,
                            marketDemand = p.MARKETDEMAND,
                            ownershipStructure = p.OWNERSHIPSTRUCTURE,
                            portfolioStrategicAlignment = p.PORTFOLIOSTRATEGICALIGNMENT,
                            projectDescription = p.PROJECTDESCRIPTION,
                            projectFinancingPlan = p.PROJECTFINANCINGPLAN,
                            proposedTermsAndConditions = p.PROPOSEDTERMSANDCONDITIONS,
                            risksAndConcerns = p.RISKSANDCONCERNS,
                            prudentialExposureLimitImplications = p.PRUDENT_EXPOSUR_LIMIT_IMPLCATN,
                            relationshipManagerId = p.RELATIONSHIPMANAGERID,
                            relationshipOfficerId = p.RELATIONSHIPOFFICERID,
                            taxIdentificationNumber = p.TAXIDENTIFICATIONNUMBER,
                            registrationNumber = p.REGISTRATIONNUMBER,
                            operationId = (short)OperationsEnum.LoanPreliminaryEvaluation,
                            customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == p.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                            {
                                bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                customerBvnid = b.CUSTOMERBVNID,
                                firstname = b.FIRSTNAME,
                                isValidBvn = b.ISVALIDBVN,
                                isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                surname = b.SURNAME
                            }).ToList(),
                            customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == p.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                            .Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == p.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                            .Select(s => new CustomerCompanyShareholdersViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == p.CUSTOMERID &&
                           cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                            .Select(cs => new CustomerClientOrSupplierViewModels()
                            {
                                client_SupplierId = cs.CLIENT_SUPPLIERID,
                                clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                firstName = cs.FIRSTNAME,
                                middleName = cs.MIDDLENAME,
                                lastName = cs.LASTNAME,
                                client_SupplierAddress = cs.ADDRESS,
                                client_SupplierPhoneNumber = cs.PHONENUMBER,
                                client_SupplierEmail = cs.EMAILADDRESS,
                                client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            }).ToList(),
                            customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == p.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                             .Select(cs => new CustomerSupplierViewModels()
                             {
                                 client_SupplierId = cs.CLIENT_SUPPLIERID,
                                 clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                 firstName = cs.FIRSTNAME,
                                 middleName = cs.MIDDLENAME,
                                 lastName = cs.LASTNAME,
                                 client_SupplierAddress = cs.ADDRESS,
                                 client_SupplierPhoneNumber = cs.PHONENUMBER,
                                 client_SupplierEmail = cs.EMAILADDRESS,
                                 client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                 client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                             }).ToList(),
                            approvalStatusId = p.APPROVALSTATUSID,
                            dateTimeCreated = p.DATETIMECREATED,
                            sentForLoanApplication = p.SENTFORLOANAPPLICATION,
                            sendForEvaluation = p.SENTFOREVALUATION,
                            loanAmount = p.LOANAMOUNT,
                            loanTypeId = p.LOANAPPLICATIONTYPEID,
                            loanTypeName = p.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productClassId = (short)p.PRODUCTCLASSID,
                            productClassName = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            capRegionId = p.CAPREGIONID
                            //subSectorId = p.SubSectorId,
                            //subSectorName = p.tbl_Sub_Sector.Name,
                            //sectorId = context.tbl_Sub_Sector.FirstOrDefault(x => x.SubSectorId == p.SubSectorId).SectorId ?? 0,
                        });



            return data;
        }

        public IEnumerable<LoanPreliminaryEvaluationViewModel> GetAllGroupCustomerLoanPreliminaryEvaluations()
        {
            var data = (from p in context.TBL_LOAN_PRELIMINARY_EVALUATN
                        join coy in context.TBL_COMPANY on p.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on p.BRANCHID equals br.BRANCHID
                        where p.ISCURRENT == false 
                        && p.LOANAPPLICATIONTYPEID == (short)LoanTypeEnum.CustomerGroup 
                        && p.SENTFORLOANAPPLICATION == false
                        orderby p.LOANPRELIMINARYEVALUATIONID descending
                        //|| p.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending || p.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            companyId = p.COMPANYID,
                            companyName = p.TBL_COMPANY.NAME,
                            loanPreliminaryEvaluationId = p.LOANPRELIMINARYEVALUATIONID,
                            preliminaryEvaluationCode = p.PRELIMINARYEVALUATIONCODE,
                            bankRole = p.BANKROLE,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            businessProfile = p.BUSINESSPROFILE,
                            clientDescription = p.CLIENTDESCRIPTION,
                            collateralArrangement = p.COLLATERALARRANGEMENT,
                            commercialViabilityAssessment = p.COMMERCIALVIABILITYASSESSMENT,
                            customerGroupId = p.CUSTOMERGROUPID.Value,
                            customerGroupName = p.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = p.TBL_CUSTOMER_GROUP.GROUPCODE,
                            environmentalImpact = p.ENVIRONMENTALIMPACT,
                            existingExposure = p.EXISTINGEXPOSURE,
                            implementationArrangements = p.IMPLEMENTATIONARRANGEMENTS,
                            marketDemand = p.MARKETDEMAND,
                            ownershipStructure = p.OWNERSHIPSTRUCTURE,
                            portfolioStrategicAlignment = p.PORTFOLIOSTRATEGICALIGNMENT,
                            operationId = (short)OperationsEnum.LoanPreliminaryEvaluation,
                            projectDescription = p.PROJECTDESCRIPTION,
                            projectFinancingPlan = p.PROJECTFINANCINGPLAN,
                            proposedTermsAndConditions = p.PROPOSEDTERMSANDCONDITIONS,
                            risksAndConcerns = p.RISKSANDCONCERNS,
                            prudentialExposureLimitImplications = p.PRUDENT_EXPOSUR_LIMIT_IMPLCATN,
                            relationshipManagerId = p.RELATIONSHIPMANAGERID,
                            relationshipOfficerId = p.RELATIONSHIPOFFICERID,
                            customerGroupMappings = context.TBL_CUSTOMER_GROUP_MAPPING.Where(x => x.CUSTOMERGROUPID == p.CUSTOMERGROUPID).Select(s => new CustomerGroupMappingViewModel()
                            {
                                customerId = s.CUSTOMERID,
                                customerName = s.TBL_CUSTOMER.FIRSTNAME + " " + s.TBL_CUSTOMER.LASTNAME,
                                customerCode = s.TBL_CUSTOMER.CUSTOMERCODE,
                                //customerAccountNumber = context.tbl_CASA.FirstOrDefault(x => x.CustomerId == p.CustomerId).ProductAccountNumber,
                                //customerTypeId = context.tbl_Customer.FirstOrDefault(x => x.CustomerId == p.CustomerId).CustomerTypeId,
                                customerType = s.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                                relationshipTypeId = s.RELATIONSHIPTYPEID,
                                relationshipTypeName = s.TBL_CUSTOMER_GROUP_RELATN_TYPE.RELATIONSHIPTYPENAME,
                                productAccountNumber = context.TBL_CASA.FirstOrDefault(x => x.CUSTOMERID == s.CUSTOMERID).PRODUCTACCOUNTNUMBER,
                                taxIdentificationNumber = s.TBL_CUSTOMER.TAXNUMBER,
                                registrationNumber = s.TBL_CUSTOMER.TBL_CUSTOMER_COMPANYINFOMATION.FirstOrDefault(x => x.CUSTOMERID == s.CUSTOMERID).REGISTRATIONNUMBER,
                                customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == s.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                                {
                                    bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                    customerBvnid = b.CUSTOMERBVNID,
                                    firstname = b.FIRSTNAME,
                                    isValidBvn = b.ISVALIDBVN,
                                    isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                    surname = b.SURNAME
                                }).ToList(),
                                customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                    .Where(cd => cd.CUSTOMERID == s.CUSTOMERID && cd.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                                .Select(x => new CustomerCompanyDirectorsViewModels()
                                {
                                    bankVerificationNumber = x.CUSTOMERBVN,
                                    companyDirectorTypeId = x.COMPANYDIRECTORTYPEID,
                                    companyDirectorTypeName = x.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                    customerId = x.CUSTOMERID,
                                    firstname = x.FIRSTNAME,
                                    surname = x.SURNAME
                                }).ToList(),
                                customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                                    .Where(cc => cc.CUSTOMERID == s.CUSTOMERID && cc.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                                .Select(cs => new CustomerCompanyShareholdersViewModels()
                                {
                                    bankVerificationNumber = cs.CUSTOMERBVN,
                                    companyDirectorTypeId = cs.COMPANYDIRECTORTYPEID,
                                    companyDirectorTypeName = cs.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                    customerId = cs.CUSTOMERID,
                                    firstname = cs.FIRSTNAME,
                                    surname = cs.SURNAME
                                }).ToList(),
                                customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER
                                    .Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                                .Select(cs => new CustomerClientOrSupplierViewModels()
                                {
                                    client_SupplierId = cs.CLIENT_SUPPLIERID,
                                    clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                    firstName = cs.FIRSTNAME,
                                    middleName = cs.MIDDLENAME,
                                    lastName = cs.LASTNAME,
                                    client_SupplierAddress = cs.ADDRESS,
                                    client_SupplierPhoneNumber = cs.PHONENUMBER,
                                    client_SupplierEmail = cs.EMAILADDRESS,
                                    client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                    client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                                }).ToList(),
                                customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER
                                    .Where(cs => cs.CUSTOMERID == s.CUSTOMERID && cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                                .Select(cs => new CustomerSupplierViewModels()
                                {
                                    client_SupplierId = cs.CLIENT_SUPPLIERID,
                                    clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                    firstName = cs.FIRSTNAME,
                                    middleName = cs.MIDDLENAME,
                                    lastName = cs.LASTNAME,
                                    client_SupplierAddress = cs.ADDRESS,
                                    client_SupplierPhoneNumber = cs.PHONENUMBER,
                                    client_SupplierEmail = cs.EMAILADDRESS,
                                    client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                    client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                                }).ToList(),
                            }).ToList(),
                            approvalStatusId = p.APPROVALSTATUSID,
                            dateTimeCreated = p.DATETIMECREATED,
                            sentForLoanApplication = p.SENTFORLOANAPPLICATION,
                            sendForEvaluation = p.SENTFOREVALUATION,
                            loanAmount = p.LOANAMOUNT,
                            loanTypeId = p.LOANAPPLICATIONTYPEID,
                            loanTypeName = p.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productClassId = (short)p.PRODUCTCLASSID,
                            productClassName = p.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            capRegionId = p.CAPREGIONID
                            //subSectorId = p.SubSectorId,
                            //subSectorName = p.tbl_Sub_Sector.Name,
                            //sectorId = context.tbl_Sub_Sector.FirstOrDefault(x => x.SubSectorId == p.SubSectorId).SectorId ?? 0,
                        });

            return data;
        }

        public async Task<bool> UpdatePreliminaryEvaluation(int loanPenId, LoanPreliminaryEvaluationViewModel model)
        {
            if (model == null)
            {
                return false;
            }

            bool output = false;

            var penRecord = context.TBL_LOAN_PRELIMINARY_EVALUATN.Find(loanPenId);

            if (penRecord != null)
            {
                penRecord.PRELIMINARYEVALUATIONCODE = model.preliminaryEvaluationCode;
                penRecord.BANKROLE = model.bankRole;
                penRecord.BRANCHID = model.userBranchId;
                penRecord.BUSINESSPROFILE = model.businessProfile;
                penRecord.CLIENTDESCRIPTION = model.clientDescription;
                penRecord.COLLATERALARRANGEMENT = model.collateralArrangement;
                penRecord.COMMERCIALVIABILITYASSESSMENT = model.commercialViabilityAssessment;
                penRecord.COMPANYID = model.companyId;
                penRecord.CUSTOMERID = model.customerId;
                penRecord.CUSTOMERGROUPID = model.customerGroupId;
                penRecord.ENVIRONMENTALIMPACT = model.environmentalImpact;
                penRecord.EXISTINGEXPOSURE = model.existingExposure;
                penRecord.REGISTRATIONNUMBER = model.registrationNumber;
                penRecord.TAXIDENTIFICATIONNUMBER = model.taxIdentificationNumber;
                penRecord.IMPLEMENTATIONARRANGEMENTS = model.implementationArrangements;
                penRecord.MARKETDEMAND = model.marketDemand;
                penRecord.OWNERSHIPSTRUCTURE = model.ownershipStructure;
                penRecord.PORTFOLIOSTRATEGICALIGNMENT = model.portfolioStrategicAlignment;
                penRecord.PROJECTDESCRIPTION = model.projectDescription;
                penRecord.PROJECTFINANCINGPLAN = model.projectFinancingPlan;
                penRecord.PROPOSEDTERMSANDCONDITIONS = model.proposedTermsAndConditions;
                penRecord.RISKSANDCONCERNS = model.risksAndConcerns;
                penRecord.PRUDENT_EXPOSUR_LIMIT_IMPLCATN = model.prudentialExposureLimitImplications;
                penRecord.RELATIONSHIPMANAGERID = model.relationshipManagerId;
                penRecord.RELATIONSHIPOFFICERID = model.relationshipOfficerId;
                penRecord.APPROVALSTATUSID = model.sendForEvaluation ? (short)ApprovalStatusEnum.Processing
                    : (short)ApprovalStatusEnum.Pending;
                penRecord.ISCURRENT = model.isCurrent;
                penRecord.SENTFOREVALUATION = model.sendForEvaluation;
                penRecord.SENTFORLOANAPPLICATION = model.sentForLoanApplication;
                penRecord.DATETIMEUPDATED = DateTime.Now;
                penRecord.CREATEDBY = model.createdBy;
                penRecord.LOANAMOUNT = model.loanAmount;
                //penRecord.LOANTYPEID = model.loanTypeId;
                //penRecord.SubSectorId = model.subSectorId;
                penRecord.PRODUCTCLASSID = model.productClassId;
                penRecord.CAPREGIONID = model.capRegionId;
            }
            else
            {
                return false;
            }

            var auditRecord = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanPreliminaryEvaluationUpdated,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Prelimenary Evaluation with code ({penRecord.PRELIMINARYEVALUATIONCODE}) updated",
                IPADDRESS = model.userIPAddress,
                URL = CommonHelpers.GetLocalIpAddress(),// groupModel.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = loanPenId
            };

            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    context.TBL_AUDIT.Add(auditRecord);
                    // Audit Section ---------------------------

                    output = await SaveAllAsync();

                    if (model.sendForEvaluation)
                    {
                        workFlow.StaffId = penRecord.CREATEDBY;
                        workFlow.OperationId = (int)OperationsEnum.LoanPreliminaryEvaluation;
                        workFlow.TargetId = loanPenId;
                        workFlow.CompanyId = penRecord.COMPANYID;
                        workFlow.Comment = "Request Preliminary Evaluation";
                        workFlow.ExternalInitialization = true;
                        workFlow.StatusId = (int)ApprovalStatusEnum.Pending;

                         workFlow.LogActivity();

                        context.SaveChanges();
                    }

                    trans.Commit();

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    throw new SecureException(ex.Message);
                }
            }

            return output;

        }
        private int[] GetStaffRegions(int staffId)
        {
            return context.TBL_BRANCH_REGION_STAFF.Where(o => o.STAFFID == staffId).Select(o => o.REGIONID).ToArray();
        }

        private IEnumerable<LoanPreliminaryEvaluationViewModel> GetCustomerPreliminaryEvaluationsByAtaffRegionAwaitingApproval(int staffId, int companyId, int[] regionIds)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.LoanPreliminaryEvaluation).ToList();

            var data = (from pen in context.TBL_LOAN_PRELIMINARY_EVALUATN
                        join coy in context.TBL_COMPANY on pen.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on pen.BRANCHID equals br.BRANCHID
                        join atrail in context.TBL_APPROVAL_TRAIL on pen.LOANPRELIMINARYEVALUATIONID equals atrail.TARGETID
                        where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                            && pen.ISCURRENT == true
                           // && atrail.TBL_APPROVAL_LEVEL.LEVELTYPEID != (short)ApprovalLevelType.Routing
                            && atrail.RESPONSESTAFFID == null
                            && atrail.OPERATIONID == (int)OperationsEnum.LoanPreliminaryEvaluation
                            && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                            && regionIds.ToList().Contains((int)pen.CAPREGIONID)

                        orderby pen.LOANPRELIMINARYEVALUATIONID descending
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            levelTypeId = atrail.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // 2 for routing TOAPPROVALLEVEL
                            companyId = pen.COMPANYID,
                            companyName = pen.TBL_COMPANY.NAME,
                            loanPreliminaryEvaluationId = pen.LOANPRELIMINARYEVALUATIONID,
                            preliminaryEvaluationCode = pen.PRELIMINARYEVALUATIONCODE,
                            bankRole = pen.BANKROLE,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            businessProfile = pen.BUSINESSPROFILE,
                            clientDescription = pen.CLIENTDESCRIPTION,
                            collateralArrangement = pen.COLLATERALARRANGEMENT,
                            commercialViabilityAssessment = pen.COMMERCIALVIABILITYASSESSMENT,
                            customerId = pen.CUSTOMERID,
                            customerName = pen.TBL_CUSTOMER.FIRSTNAME + " " + pen.TBL_CUSTOMER.LASTNAME,
                            customerGroupId = pen.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID,
                            customerGroupName = pen.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = pen.TBL_CUSTOMER_GROUP.GROUPCODE,
                            customerCode = pen.TBL_CUSTOMER.CUSTOMERCODE,
                            environmentalImpact = pen.ENVIRONMENTALIMPACT,
                            existingExposure = pen.EXISTINGEXPOSURE,
                            implementationArrangements = pen.IMPLEMENTATIONARRANGEMENTS,
                            marketDemand = pen.MARKETDEMAND,
                            ownershipStructure = pen.OWNERSHIPSTRUCTURE,
                            portfolioStrategicAlignment = pen.PORTFOLIOSTRATEGICALIGNMENT,
                            projectDescription = pen.PROJECTDESCRIPTION,
                            projectFinancingPlan = pen.PROJECTFINANCINGPLAN,
                            proposedTermsAndConditions = pen.PROPOSEDTERMSANDCONDITIONS,
                            risksAndConcerns = pen.RISKSANDCONCERNS,
                            prudentialExposureLimitImplications = pen.PRUDENT_EXPOSUR_LIMIT_IMPLCATN,
                            relationshipManagerId = pen.RELATIONSHIPMANAGERID,
                            relationshipOfficerId = pen.RELATIONSHIPOFFICERID,
                            taxIdentificationNumber = pen.TAXIDENTIFICATIONNUMBER,
                            registrationNumber = pen.REGISTRATIONNUMBER,
                            operationId = (int)OperationsEnum.LoanPreliminaryEvaluation,
                            dateTimeCreated = pen.DATETIMECREATED,
                            capRegionId = pen.CAPREGIONID,
                            isHouRouting = true,
                            customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == pen.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                            {
                                bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                customerBvnid = b.CUSTOMERBVNID,
                                firstname = b.FIRSTNAME,
                                isValidBvn = b.ISVALIDBVN,
                                isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                surname = b.SURNAME
                            }).ToList(),
                            customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == pen.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                            .Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == pen.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                            .Select(s => new CustomerCompanyShareholdersViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == pen.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                            .Select(cs => new CustomerClientOrSupplierViewModels()
                            {
                                client_SupplierId = cs.CLIENT_SUPPLIERID,
                                clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                firstName = cs.FIRSTNAME,
                                middleName = cs.MIDDLENAME,
                                lastName = cs.LASTNAME,
                                client_SupplierAddress = cs.ADDRESS,
                                client_SupplierPhoneNumber = cs.PHONENUMBER,
                                client_SupplierEmail = cs.EMAILADDRESS,
                                client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            }).ToList(),
                            customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == pen.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                             .Select(cs => new CustomerSupplierViewModels()
                             {
                                 client_SupplierId = cs.CLIENT_SUPPLIERID,
                                 clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                 firstName = cs.FIRSTNAME,
                                 middleName = cs.MIDDLENAME,
                                 lastName = cs.LASTNAME,
                                 client_SupplierAddress = cs.ADDRESS,
                                 client_SupplierPhoneNumber = cs.PHONENUMBER,
                                 client_SupplierEmail = cs.EMAILADDRESS,
                                 client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                 client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                             }).ToList(),
                            loanAmount = pen.LOANAMOUNT,
                            loanTypeId = pen.LOANAPPLICATIONTYPEID,
                            loanTypeName = pen.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productClassId = (short)pen.PRODUCTCLASSID,
                            productClassName = pen.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            //subSectorId = pen.SubSectorId,
                            //subSectorName = pen.tbl_Sub_Sector.Name,
                            //sectorId = context.tbl_Sub_Sector.FirstOrDefault(x => x.SubSectorId == pen.SubSectorId).SectorId ?? 0,
                        }).ToList();

            var data2 = (from pen in context.TBL_LOAN_PRELIMINARY_EVALUATN
                        join coy in context.TBL_COMPANY on pen.COMPANYID equals coy.COMPANYID
                        join br in context.TBL_BRANCH on pen.BRANCHID equals br.BRANCHID
                        join atrail in context.TBL_APPROVAL_TRAIL on pen.LOANPRELIMINARYEVALUATIONID equals atrail.TARGETID
                        where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                            && pen.ISCURRENT == true
                            && atrail.TBL_APPROVAL_LEVEL.LEVELTYPEID == (short)ApprovalLevelType.Routing 
                            && atrail.RESPONSESTAFFID == null
                            && atrail.OPERATIONID == (int)OperationsEnum.LoanPreliminaryEvaluation
                            && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                            && regionIds.ToList().Contains((int)pen.CAPREGIONID)

                        orderby pen.LOANPRELIMINARYEVALUATIONID descending
                        select new LoanPreliminaryEvaluationViewModel()
                        {
                            levelTypeId = atrail.TBL_APPROVAL_LEVEL1.LEVELTYPEID, // 2 for routing TOAPPROVALLEVEL
                            companyId = pen.COMPANYID,
                            companyName = pen.TBL_COMPANY.NAME,
                            loanPreliminaryEvaluationId = pen.LOANPRELIMINARYEVALUATIONID,
                            preliminaryEvaluationCode = pen.PRELIMINARYEVALUATIONCODE,
                            bankRole = pen.BANKROLE,
                            branchId = br.BRANCHID,
                            branchName = br.BRANCHNAME,
                            businessProfile = pen.BUSINESSPROFILE,
                            clientDescription = pen.CLIENTDESCRIPTION,
                            collateralArrangement = pen.COLLATERALARRANGEMENT,
                            commercialViabilityAssessment = pen.COMMERCIALVIABILITYASSESSMENT,
                            customerId = pen.CUSTOMERID,
                            customerName = pen.TBL_CUSTOMER.FIRSTNAME + " " + pen.TBL_CUSTOMER.LASTNAME,
                            customerGroupId = pen.TBL_CUSTOMER_GROUP.CUSTOMERGROUPID,
                            customerGroupName = pen.TBL_CUSTOMER_GROUP.GROUPNAME,
                            customerGroupCode = pen.TBL_CUSTOMER_GROUP.GROUPCODE,
                            customerCode = pen.TBL_CUSTOMER.CUSTOMERCODE,
                            environmentalImpact = pen.ENVIRONMENTALIMPACT,
                            existingExposure = pen.EXISTINGEXPOSURE,
                            implementationArrangements = pen.IMPLEMENTATIONARRANGEMENTS,
                            marketDemand = pen.MARKETDEMAND,
                            ownershipStructure = pen.OWNERSHIPSTRUCTURE,
                            portfolioStrategicAlignment = pen.PORTFOLIOSTRATEGICALIGNMENT,
                            projectDescription = pen.PROJECTDESCRIPTION,
                            projectFinancingPlan = pen.PROJECTFINANCINGPLAN,
                            proposedTermsAndConditions = pen.PROPOSEDTERMSANDCONDITIONS,
                            risksAndConcerns = pen.RISKSANDCONCERNS,
                            prudentialExposureLimitImplications = pen.PRUDENT_EXPOSUR_LIMIT_IMPLCATN,
                            relationshipManagerId = pen.RELATIONSHIPMANAGERID,
                            relationshipOfficerId = pen.RELATIONSHIPOFFICERID,
                            taxIdentificationNumber = pen.TAXIDENTIFICATIONNUMBER,
                            registrationNumber = pen.REGISTRATIONNUMBER,
                            operationId = (int)OperationsEnum.LoanPreliminaryEvaluation,
                            dateTimeCreated = pen.DATETIMECREATED,
                            capRegionId = pen.CAPREGIONID,
                            isHouRouting = false,
                            customerBvnInformation = context.TBL_CUSTOMER_BVN.Where(b => b.CUSTOMERID == pen.CUSTOMERID).Select(b => new CustomerBvnViewModels()
                            {
                                bankVerificationNumber = b.BANKVERIFICATIONNUMBER,
                                customerBvnid = b.CUSTOMERBVNID,
                                firstname = b.FIRSTNAME,
                                isValidBvn = b.ISVALIDBVN,
                                isPoliticallyExposed = b.ISPOLITICALLYEXPOSED,
                                surname = b.SURNAME
                            }).ToList(),
                            customerCompanyDirectors = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == pen.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.BoardMember)
                            .Select(s => new CustomerCompanyDirectorsViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerCompanyShareholders = context.TBL_CUSTOMER_COMPANY_DIRECTOR
                            .Where(s => s.CUSTOMERID == pen.CUSTOMERID && s.COMPANYDIRECTORTYPEID == (short)CompanyDirectorTypeEnum.Shareholder)
                            .Select(s => new CustomerCompanyShareholdersViewModels()
                            {
                                bankVerificationNumber = s.CUSTOMERBVN,
                                companyDirectorTypeId = s.COMPANYDIRECTORTYPEID,
                                companyDirectorTypeName = s.TBL_CUSTOMER_COMPANY_DIREC_TYP.COMPANYDIRECTORYTYPENAME,
                                customerId = s.CUSTOMERID,
                                firstname = s.FIRSTNAME,
                                surname = s.SURNAME
                            }).ToList(),
                            customerClients = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == pen.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Client)
                            .Select(cs => new CustomerClientOrSupplierViewModels()
                            {
                                client_SupplierId = cs.CLIENT_SUPPLIERID,
                                clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                firstName = cs.FIRSTNAME,
                                middleName = cs.MIDDLENAME,
                                lastName = cs.LASTNAME,
                                client_SupplierAddress = cs.ADDRESS,
                                client_SupplierPhoneNumber = cs.PHONENUMBER,
                                client_SupplierEmail = cs.EMAILADDRESS,
                                client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                            }).ToList(),
                            customerSuppliers = context.TBL_CUSTOMER_CLIENT_SUPPLIER.Where(cs => cs.CUSTOMERID == pen.CUSTOMERID &&
                            cs.CLIENT_SUPPLIERTYPEID == (short)CompanyClientOrSupplierTypeEnum.Supplier)
                             .Select(cs => new CustomerSupplierViewModels()
                             {
                                 client_SupplierId = cs.CLIENT_SUPPLIERID,
                                 clientOrSupplierName = cs.FIRSTNAME + " " + cs.LASTNAME,
                                 firstName = cs.FIRSTNAME,
                                 middleName = cs.MIDDLENAME,
                                 lastName = cs.LASTNAME,
                                 client_SupplierAddress = cs.ADDRESS,
                                 client_SupplierPhoneNumber = cs.PHONENUMBER,
                                 client_SupplierEmail = cs.EMAILADDRESS,
                                 client_SupplierTypeId = cs.CLIENT_SUPPLIERTYPEID,
                                 client_SupplierTypeName = cs.TBL_CUSTOMER_CLIENT_SUPPLR_TYP.CLIENT_SUPPLIERTYPENAME
                             }).ToList(),
                            loanAmount = pen.LOANAMOUNT,
                            loanTypeId = pen.LOANAPPLICATIONTYPEID,
                            loanTypeName = pen.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                            productClassId = (short)pen.PRODUCTCLASSID,
                            productClassName = pen.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                            //subSectorId = pen.SubSectorId,
                            //subSectorName = pen.tbl_Sub_Sector.Name,
                            //sectorId = context.tbl_Sub_Sector.FirstOrDefault(x => x.SubSectorId == pen.SubSectorId).SectorId ?? 0,
                        }).ToList();
            return data; //.Union(data);
        }


        public bool SendPreliminaryEvaluationForLoanApplication(int loanPenId, LoanPreliminaryEvaluationViewModel model)
        {
            var penRecord = context.TBL_LOAN_PRELIMINARY_EVALUATN.Find(loanPenId);

            bool output = false;

            if (penRecord != null)
            {
                penRecord.SENTFORLOANAPPLICATION = model.sentForLoanApplication;
                penRecord.DATETIMEUPDATED = DateTime.Now;
            }
            else
            {
                return false;
            }

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanPreliminaryEvaluationUpdated,
                STAFFID = model.createdBy,
                BRANCHID = model.branchId,
                DETAIL = $"Prelimenary Evaluation with code ({penRecord.PRELIMINARYEVALUATIONCODE}) has been sent for loan application",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = loanPenId
            };

            this.auditTrail.AddAuditTrail(audit);
            // Audit Section ---------------------------

            output = context.SaveChanges() > 0;

            return output;
        }
    }
}
