using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintrakBanking.Common.CustomException;

using FintrakBanking.ViewModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using FintrakBanking.Interfaces.media;
using FintrakBanking.Interfaces.Setups.Credit;
using System.Data.Entity;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Interfaces.Finance;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.CASA;
using FintrakBanking.ViewModels.CASA;
using FintrakBanking.ViewModels.Finance;
using FintrakBanking.ViewModels.ThridPartyIntegration;
using Newtonsoft.Json;
using System.ServiceModel;
using FintrakBanking.Common;
using FintrakBanking.ViewModels.Setups.General;
using System.Configuration;
using FintrakBanking.Entities.StagingModels;
using FinTrakBanking.ThirdPartyIntegration.StagingDatabase.Finacle;
using System.ComponentModel.Design;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using FintrakBanking.ViewModels.Reports;

namespace FintrakBanking.Repositories.Credit
{
    public class CustomerCollateralRepository : ICustomerCollateralRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingStagingContext stageContext;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;
        private IProductRepository product;
        private IMediaRepository media;
        private ICollateralTypeRepository collateralType;
        private IWorkflow workflow;
        private FinTrakBankingDocumentsContext documentContext;
        private IFinanceTransactionRepository repo;
        private IApprovalLevelStaffRepository level;
        private ICasaLienRepository lien;
        private ICasaRepository casa;
        private IIntegrationWithFinacle finacle;
        private ICreditDrawdownRepository drawdownRepo;
        //private IAlertRepository alert;

        public string collateralReleaseStatusName { get; private set; }


        public CustomerCollateralRepository(
            FinTrakBankingContext _context,
            IGeneralSetupRepository _genSetup,
            IAuditTrailRepository _auditTrail, IProductRepository _product,
            IMediaRepository _media,
            ICollateralTypeRepository _collateralType,
            IWorkflow workflow,
            FinTrakBankingDocumentsContext _documentContext,
            IFinanceTransactionRepository _repo,
            IApprovalLevelStaffRepository _level,
            ICasaLienRepository _lien,
            ICasaRepository _casa,
            IIntegrationWithFinacle _finacle,
            ICreditDrawdownRepository _drawdownRepo,
            FinTrakBankingStagingContext _stageContext
            //IAlertRepository _alert
            )
        {
            this.context = _context;
            this.stageContext = _stageContext;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.product = _product;
            this.media = _media;
            this.collateralType = _collateralType;
            this.workflow = workflow;
            this.documentContext = _documentContext;
            this.repo = _repo;
            this.level = _level;
            this.lien = _lien;
            this.casa = _casa;
            this.finacle = _finacle;
            this.drawdownRepo = _drawdownRepo;
            //this.alert = _alert;
        }



        #region New 

        // ADD
        [OperationBehavior(TransactionScopeRequired = true)]
        public int AddCollateral(CollateralViewModel entity) //, 
        {
           
            int collateralId = AddTempCollateralMainForm(entity);
            
            if (collateralId > 0)
            {

                switch (entity.collateralTypeId)
                {
                    case (int)CollateralTypeEnum.FixedDeposit: AddDepositCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.PlantAndMachinery: AddTempEquipmentCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Miscellaneous: AddTempMiscellaneousCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Gaurantee: AddTempGuaranteeCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.CASA: AddTempCasaCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Property: AddTempImmovablePropertyCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.TreasuryBillsAndBonds: AddTempMarketableSecuritiesCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.InsurancePolicy: AddTempPolicyCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.PreciousMetal: AddTempPreciousMetalCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.MarketableSecurities_Shares: AddTempStockCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Vehicle: AddVehicleCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Promissory: AddPromissoryCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.ISPO: AddISPOCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.DomiciliationContract: AddContractDomiciliationCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.DomiciliationSalary: AddSalaryDomiciliationCollateral(collateralId, entity); break;
                    case (int)CollateralTypeEnum.Indemity: AddIndemityCollateral(collateralId, entity); break;

                    default: break;
                }

                //if (entity.hasInsurance) { AddTempItemInsurancePolicy(collateralId, entity); }

                //if (file != null) { SaveCollateralMainDocument(entity, collateralId, file); }

                bool saved;
                try
                {
                    saved = context.SaveChanges() != 0;
                }
                catch (Exception ex)
                {
                    throw new SecureException("Error has occured while creating this collateral");
                }

            }

            return collateralId;
        }

        //public int AddCollateral(CollateralViewModel entity, byte[] file) //, 
        //{
        //    int collateralId = AddTempCollateralMainForm(entity);

        //    if (collateralId > 0)
        //    {
        //        switch (entity.collateralTypeId)
        //        {
        //            case (int)CollateralTypeEnum.TermDeposit: AddTempDepositCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.PlantAndMachinery: AddTempEquipmentCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.Miscellaneous: AddTempMiscellaneousCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.Gaurantee: AddTempGuaranteeCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.CASA: AddTempCasaCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.Property: AddTempImmovablePropertyCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.TreasuryBillsAndBonds: AddTempMarketableSecuritiesCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.InsurancePolicy: AddTempPolicyCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.PreciousMetal: AddTempPreciousMetalCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.MarketableSecurities_Shares: AddTempStockCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.Vehicle: AddVehicleCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.Promissory: AddPromissoryCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.ISPO: AddISPOCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.DomiciliationContract: AddContractDomiciliationCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.DomiciliationSalary: AddSalaryDomiciliationCollateral(collateralId, entity); break;
        //            case (int)CollateralTypeEnum.Indemity: AddIndemityCollateral(collateralId, entity); break;

        //            default: break;
        //        }

        //        if (entity.hasInsurance) { AddTempItemInsurancePolicy(collateralId, entity); }

        //        if (file != null) { SaveCollateralMainDocument(entity, collateralId, file); }

        //        bool saved;
        //        try
        //        {
        //            saved = context.SaveChanges() != 0;
        //        }
        //        catch (Exception ex)
        //        {

        //            throw new SecureException("Error has occured while creating this collateral");
        //        }
        //        if (saved) { return collateralId; }


        //    }

        //    return 0;
        //}

        // UPDATE
        public IQueryable<CollateralViewModel> GetCollateralReleaseAwaitingApproval(int companyId, int staffId)
        {
            var ids1 = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.FinalCollateralRelease).ToList();
            var ids2 = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.TemporalCollateralRelease).ToList();
            var ids = ids1.Union(ids2);
            var levelsettings = context.TBL_APPROVAL_LEVEL_SETTING.Where(x => ids1.Contains(x.APPROVALLEVELID));


            if (levelsettings.Where(x => x.SETTINGCODE == "COLLATERALBUSSINESSMANAGERAPPROVER").FirstOrDefault() != null)//Bussiness Manager
            {

                var record = (from a in context.TBL_COLLATERAL_RELEASE
                              join b in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals b.COLLATERALCUSTOMERID
                              join atrail in context.TBL_APPROVAL_TRAIL on a.COLLATERALRELEASEID equals atrail.TARGETID
                              join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                              //where b.COMPANYID == companyId && a.JOBREQUESTSENT == true
                              where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending)
                                    && (atrail.OPERATIONID == (int)OperationsEnum.FinalCollateralRelease || atrail.OPERATIONID == (int)OperationsEnum.TemporalCollateralRelease)
                                    && ids.Contains((int)atrail.TOAPPROVALLEVELID) && a.JOBREQUESTSENT == true && b.COMPANYID == companyId
                                    && atrail.RESPONSESTAFFID == null && c.SUPERVISOR_STAFFID == staffId
                              select new CollateralViewModel
                              {
                                  collateralId = a.COLLATERALCUSTOMERID,

                                  collateralCustomerId = a.COLLATERALCUSTOMERID,
                                  collateralCode = b.COLLATERALCODE,
                                  customerId = b.CUSTOMERID.Value,
                                  customerName = context.TBL_CUSTOMER.Where(q => q.CUSTOMERID == b.CUSTOMERID).Select(w => w.FIRSTNAME + " " + w.MIDDLENAME + " " + w.LASTNAME).FirstOrDefault(),
                                  collateralReleaseId = a.COLLATERALRELEASEID,
                                  collateralReleaseTypeId = a.COLLATERALRELEASETYPEID,
                                  collateralReleaseTypeName = context.TBL_COLLATERAL_RELEASE_TYPE.Where(q => q.COLLATERALRELEASETYPEID == a.COLLATERALRELEASETYPEID).Select(r => r.COLLATERALRELEASETYPENAME).FirstOrDefault(),
                                  jobRequestSent = a.JOBREQUESTSENT == null ? false : a.JOBREQUESTSENT,
                                  collateralTypeId = b.COLLATERALTYPEID,
                                  hairCut = b.HAIRCUT,
                                  isLocationBased = (bool)b.ISLOCATIONBASED,
                                  valuationCycle = b.VALUATIONCYCLE,
                                  dateTimeCreated = b.DATETIMECREATED,
                                  allowSharing = b.ALLOWSHARING,
                              });

                return record;

            }
            else
            {
                var record = (from a in context.TBL_COLLATERAL_RELEASE
                              join b in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals b.COLLATERALCUSTOMERID
                              join atrail in context.TBL_APPROVAL_TRAIL on a.COLLATERALRELEASEID equals atrail.TARGETID
                              join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                              //where b.COMPANYID == companyId && a.JOBREQUESTSENT == true
                              where (atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending)
                                    && (atrail.OPERATIONID == (int)OperationsEnum.FinalCollateralRelease || atrail.OPERATIONID == (int)OperationsEnum.TemporalCollateralRelease)
                                    && ids.Contains((int)atrail.TOAPPROVALLEVELID) && a.JOBREQUESTSENT == true && b.COMPANYID == companyId
                                    && atrail.RESPONSESTAFFID == null
                              select new CollateralViewModel
                              {
                                  collateralId = a.COLLATERALCUSTOMERID,

                                  collateralCustomerId = a.COLLATERALCUSTOMERID,
                                  collateralCode = b.COLLATERALCODE,
                                  customerId = b.CUSTOMERID.Value,
                                  customerName = context.TBL_CUSTOMER.Where(q => q.CUSTOMERID == b.CUSTOMERID).Select(w => w.FIRSTNAME + " " + w.MIDDLENAME + " " + w.LASTNAME).FirstOrDefault(),
                                  collateralReleaseId = a.COLLATERALRELEASEID,
                                  collateralReleaseTypeId = a.COLLATERALRELEASETYPEID,
                                  collateralReleaseTypeName = context.TBL_COLLATERAL_RELEASE_TYPE.Where(q => q.COLLATERALRELEASETYPEID == a.COLLATERALRELEASETYPEID).Select(r => r.COLLATERALRELEASETYPENAME).FirstOrDefault(),
                                  jobRequestSent = a.JOBREQUESTSENT == null ? false : a.JOBREQUESTSENT,
                                  collateralTypeId = b.COLLATERALTYPEID,
                                  hairCut = b.HAIRCUT,
                                  isLocationBased = (bool)b.ISLOCATIONBASED,
                                  valuationCycle = b.VALUATIONCYCLE,
                                  dateTimeCreated = b.DATETIMECREATED,
                                  allowSharing = b.ALLOWSHARING,
                              });

                return record;
            }




        }

        public IQueryable<CollateralViewModel> GetCollateralReleaseAwaitingJobRequest(int companyId, int branchId)
        {
            var record = (from a in context.TBL_COLLATERAL_RELEASE
                          join b in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals b.COLLATERALCUSTOMERID
                          join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID
                          where b.COMPANYID == companyId && a.JOBREQUESTSENT == false && c.BRANCHID == branchId
                          select new CollateralViewModel
                          {
                              collateralCustomerId = a.COLLATERALCUSTOMERID,
                              collateralCode = b.COLLATERALCODE,
                              customerName = context.TBL_CUSTOMER.Where(q => q.CUSTOMERID == b.CUSTOMERID).Select(w => w.FIRSTNAME + " " + w.MIDDLENAME + " " + w.LASTNAME).FirstOrDefault(),
                              collateralReleaseId = a.COLLATERALRELEASEID,
                              collateralReleaseTypeId = a.COLLATERALRELEASETYPEID,
                              collateralReleaseTypeName = context.TBL_COLLATERAL_RELEASE_TYPE.Where(q => q.COLLATERALRELEASETYPEID == a.COLLATERALRELEASETYPEID).Select(r => r.COLLATERALRELEASETYPENAME).FirstOrDefault(),
                              jobRequestSent = a.JOBREQUESTSENT == null ? false : a.JOBREQUESTSENT,
                          });

            return record;
        }

        private void ValidateJobRequestCheck(TBL_COLLATERAL_RELEASE collateralRelease)
        {
            var record = (from a in context.TBL_JOB_REQUEST
                          join b in context.TBL_COLLATERAL_RELEASE on a.TARGETID equals b.COLLATERALCUSTOMERID
                          where a.TARGETID == collateralRelease.COLLATERALCUSTOMERID
                          select new
                          {
                              a
                          }).ToList();
            if (record.Count > 0)
            {
                var legal = record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID == (short)JobTypeEnum.legal).ToList();
                var middleOfficeVerification = record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification).ToList();
                if (legal.Count > 0)
                {

                }
                else
                {
                    throw new ConditionNotMetException("Kindly Complete The Legal Job Request Before You Proceed");
                }
                if (collateralRelease.COLLATERALRELEASETYPEID == 1) //Final Release
                {
                    if (middleOfficeVerification.Count > 0)
                    {

                    }
                    else
                    {
                        throw new ConditionNotMetException("Kindly Complete The Middle Office Job Request Before You Proceed");
                    }
                }

            }
            else
            {
                throw new ConditionNotMetException("Kindly Complete The Legal & Middle Office Job Request Before You Proceed");

            }
        }


        private void PendingJobRequestCheck(TBL_COLLATERAL_RELEASE collateralRelease)
        {
            var record = (from a in context.TBL_JOB_REQUEST
                          join b in context.TBL_COLLATERAL_RELEASE on a.TARGETID equals b.COLLATERALCUSTOMERID
                          where a.TARGETID == collateralRelease.COLLATERALCUSTOMERID
                          select new
                          {
                              a
                          }).ToList();

            var legal = record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID != (short)JobTypeEnum.legal && (x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending || x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.processing)).Any();
            var middleOfficeVerification = record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.disapproved).Any();

            //foreach (var item in loanApplicationDetails)
            //{
            if (record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID != (short)JobTypeEnum.legal && (x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending || x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.processing)).Any())
                throw new ConditionNotMetException("There are pending Legal job request for this request.");
            if (collateralRelease.COLLATERALRELEASETYPEID == 1) //Final Release
            {
                if (record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.disapproved).Any())
                    throw new ConditionNotMetException("There are unapproved middle office request.");

                if (record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && (x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending || x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.processing)).Any())
                    throw new ConditionNotMetException("There are pending middle office job request for this request..");
            }

            //}
        }

        public bool FinalReleaseCollateralApproval(ApprovalViewModel entity, int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.FinalCollateralRelease).ToList();
            //var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.TemporalCollateralRelease).ToList();
            var levelsettings = context.TBL_APPROVAL_LEVEL_SETTING.Where(x => ids.Contains(x.APPROVALLEVELID));



            var release = context.TBL_COLLATERAL_RELEASE.Where(a => a.COLLATERALRELEASEID == entity.targetId).FirstOrDefault();
            var customerCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(a => a.COLLATERALCUSTOMERID == release.COLLATERALCUSTOMERID).FirstOrDefault();


            var mainCollateral = (from x in context.TBL_COLLATERAL_CUSTOMER
                                  join t in context.TBL_COLLATERAL_TYPE on x.COLLATERALTYPEID equals t.COLLATERALTYPEID
                                  where x.COLLATERALCUSTOMERID == release.COLLATERALCUSTOMERID
                                  select new { x.COLLATERALTYPEID, x.COLLATERALCUSTOMERID, t.REQUIREINSURANCEPOLICY, t.REQUIREVISITATION, x.COLLATERALCODE, x.COLLATERALVALUE }).FirstOrDefault();


            if (mainCollateral.COLLATERALTYPEID > 0)
            {
                var description = "Callateral lien release";
                decimal securityValue = 0;

                if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.CASA)
                {
                    description = "CASA callateral lien release";
                    var collateral = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == mainCollateral.COLLATERALCUSTOMERID);
                    if (collateral != null) securityValue = collateral.SECURITYVALUE;
                }

                if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.FixedDeposit)
                {
                    description = "Deposit callateral lien release";
                    var collateral = context.TBL_COLLATERAL_DEPOSIT.FirstOrDefault(x => x.COLLATERALCUSTOMERID == mainCollateral.COLLATERALCUSTOMERID);
                    if (collateral != null) securityValue = collateral.SECURITYVALUE;
                }

                if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.FixedDeposit ||
                    mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.CASA)
                {
                    var existingLien = context.TBL_CASA_LIEN.FirstOrDefault(x => x.SOURCEREFERENCENUMBER == mainCollateral.COLLATERALCODE && x.LIENTYPEID == (int)LienTypeEnum.CollateralCreation);
                    if (existingLien == null) throw new SecureException("No lien has been placed");
                    string lienReferenceNumber = existingLien.LIENREFERENCENUMBER;

                    lien.ReleaseLien(new CasaLienViewModel
                    {
                        productAccountNumber = mainCollateral.COLLATERALCODE,
                        lienAmount = securityValue,
                        description = description,
                        lienTypeId = (int)LienTypeEnum.CollateralCreation,
                        lienReferenceNumber = lienReferenceNumber,
                        dateTimeCreated = DateTime.Now,
                        createdBy = entity.createdBy,
                        companyId = entity.companyId,
                        branchId = (short)entity.BranchId
                    });
                }
                var tempRelease = context.TBL_COLLATERAL_RELEASE_DOC.Where(a => a.COLLATERALRELEASEID == entity.targetId).ToList();
                TBL_MEDIA_COLLATERAL_DOCUMENTS collateralRecord = new TBL_MEDIA_COLLATERAL_DOCUMENTS();

                if (levelsettings.Where(x => x.SETTINGCODE == "COLLATERALBUSSINESSMANAGERAPPROVER").FirstOrDefault() != null)//Bussiness Manager
                {
                    foreach (var rec in tempRelease)
                    {
                        collateralRecord = documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.Find(rec.DOCUMENTID);
                        collateralRecord.COLLATERALRELEASESTATUSID = (int)CollateralReleaseStatus.ReleasedToCustomer;
                        documentContext.Entry(collateralRecord).State = EntityState.Modified;
                        documentContext.SaveChanges();
                    }

                    customerCollateral.COLLATERALRELEASESTATUSID = (int)CollateralReleaseStatus.ReleasedToCustomer;
                    context.Entry(customerCollateral).State = EntityState.Modified;

                }

                release.APPROVALSTATUSID = (short)entity.approvalStatusId;
                //customerCollateral.COLLATERALRELEASESTATUSID = (int)CollateralReleaseStatus.InVault;
                //context.Entry(release).State = EntityState.Modified;

                // Audit Section ---------------------------
                this.auditTrail.AddAuditTrail(new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseApproval,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.BranchId,
                    DETAIL = $"Collateral Release Approval '{ customerCollateral.COLLATERALCODE }' ",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),

                });
                // End of Audit Section ---------------------
            }


            return context.SaveChanges() > 0;

        }

        public bool TemporaryReleaseCollateralApproval(ApprovalViewModel entity, int staffId)
        {
            //var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.FinalCollateralRelease).ToList();
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.TemporalCollateralRelease).ToList();
            var levelsettings = context.TBL_APPROVAL_LEVEL_SETTING.Where(x => ids.Contains(x.APPROVALLEVELID));



            var release = context.TBL_COLLATERAL_RELEASE.Where(a => a.COLLATERALRELEASEID == entity.targetId).FirstOrDefault();

            var tempRelease = context.TBL_COLLATERAL_RELEASE_DOC.Where(a => a.COLLATERALRELEASEID == entity.targetId).ToList();

            TBL_MEDIA_COLLATERAL_DOCUMENTS collateralRecord = new TBL_MEDIA_COLLATERAL_DOCUMENTS();

            if (levelsettings.Where(x => x.SETTINGCODE == "COLLATERALCREDITCONTROLOFFICERAPPROVER").FirstOrDefault() != null)//Credit Control Officer
            {
                foreach (var rec in tempRelease)
                {
                    collateralRecord = documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.Find(rec.DOCUMENTID);
                    collateralRecord.COLLATERALRELEASESTATUSID = (int)CollateralReleaseStatus.InVault;
                    documentContext.Entry(collateralRecord).State = EntityState.Modified;
                    documentContext.SaveChanges();
                }

            }

            release.APPROVALSTATUSID = (short)entity.approvalStatusId;
            context.Entry(release).State = EntityState.Modified;

            // Audit Section ---------------------------
            this.auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseApproval,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.BranchId,
                DETAIL = $"Collateral Release Approval '{ collateralRecord.COLLATERALCODE }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            });
            // End of Audit Section ---------------------



            return context.SaveChanges() > 0;
        }

        public ApprovalResponse ReleaseCollateralGoForApproval(ApprovalViewModel model)
        {
            ApprovalResponse response = new ApprovalResponse();
            var release = context.TBL_COLLATERAL_RELEASE.Where(a => a.COLLATERALRELEASEID == model.targetId).FirstOrDefault();
            var ids = genSetup.GetStaffApprovalLevelIds(model.createdBy, (int)release.COLLATERALRELEASETYPEID == (int)CollateralReleaseType.FinalRelease ? (int)OperationsEnum.FinalCollateralRelease : (int)OperationsEnum.TemporalCollateralRelease).ToList();


            var customerCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(a => a.COLLATERALCUSTOMERID == release.COLLATERALCUSTOMERID).FirstOrDefault();
            var levelsettings = context.TBL_APPROVAL_LEVEL_SETTING.Where(x => ids.Contains(x.APPROVALLEVELID));
            // int status = 0;

            bool output = false;

            workflow.StaffId = model.createdBy;
            workflow.CompanyId = model.companyId;
            workflow.StatusId = (short)model.approvalStatusId; //(int)ApprovalStatusEnum.Processing;//(short)model.approvalStatusId;
            workflow.TargetId = model.targetId;
            workflow.Comment = model.comment;
            workflow.OperationId = (int)release.COLLATERALRELEASETYPEID == (int)CollateralReleaseType.FinalRelease ? (int)OperationsEnum.FinalCollateralRelease : (int)OperationsEnum.TemporalCollateralRelease;
            workflow.DeferredExecution = true;

            workflow.LogActivity();

            if (workflow.NewState != (int)ApprovalState.Ended)
            {
                if (workflow.StatusId != (int)ApprovalStatusEnum.Referred)
                {
                    if (release.COLLATERALRELEASETYPEID == (int)CollateralReleaseType.FinalRelease) //OperationsEnum.FinalCollateralRelease
                    {

                        if (levelsettings.Where(x => x.SETTINGCODE == "COLLATERALCREDITCONTROLOFFICERAPPROVER").FirstOrDefault() != null)//Credit Control Officer
                        {
                            customerCollateral.COLLATERALRELEASESTATUSID = (int)CollateralReleaseStatus.ReleasedToBM;
                            context.Entry(customerCollateral).State = EntityState.Modified;
                        }
                        output = true;

                    }
                    else if (release.COLLATERALRELEASETYPEID == (int)CollateralReleaseType.TemporaryRelease) //OperationsEnum.TemporalCollateralRelease
                    {
                        var tempRelease = context.TBL_COLLATERAL_RELEASE_DOC.Where(a => a.COLLATERALRELEASEID == release.COLLATERALRELEASEID).ToList();

                        TBL_MEDIA_COLLATERAL_DOCUMENTS collateralRecord = new TBL_MEDIA_COLLATERAL_DOCUMENTS();


                        if (levelsettings.Where(x => x.SETTINGCODE == "COLLATERALLEGAL1APPROVER").FirstOrDefault() != null)//Legal
                        {
                            foreach (var rec in tempRelease)
                            {
                                collateralRecord = documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.Find(rec.DOCUMENTID);
                                collateralRecord.COLLATERALRELEASESTATUSID = (int)CollateralReleaseStatus.InVault;
                                documentContext.Entry(collateralRecord).State = EntityState.Modified;
                                documentContext.SaveChanges();
                            }
                            //customerCollateral.COLLATERALRELEASESTATUSID = (int)CollateralReleaseStatus.ReleasedToCustomer;
                            //context.Entry(customerCollateral).State = EntityState.Modified;

                        }
                        if (levelsettings.Where(x => x.SETTINGCODE == "COLLATERALMANAGEMENTTEAMAPPROVER").FirstOrDefault() != null)//cOLLATERALManagementTeam
                        {
                            foreach (var rec in tempRelease)
                            {
                                collateralRecord = documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.Find(rec.DOCUMENTID);
                                collateralRecord.COLLATERALRELEASESTATUSID = (int)CollateralReleaseStatus.ReleasedToLegal;
                                documentContext.Entry(collateralRecord).State = EntityState.Modified;
                                documentContext.SaveChanges();
                            }

                        }
                        if (levelsettings.Where(x => x.SETTINGCODE == "COLLATERALLEGAL2APPROVER").FirstOrDefault() != null)//Legal
                        {
                            foreach (var rec in tempRelease)
                            {
                                collateralRecord = documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.Find(rec.DOCUMENTID);
                                collateralRecord.COLLATERALRELEASESTATUSID = (int)CollateralReleaseStatus.ReleasedToLegal;
                                documentContext.Entry(collateralRecord).State = EntityState.Modified;
                                documentContext.SaveChanges();
                            }

                        }
                        output = true;

                    }
                    response.status = (int)ApprovalStatusEnum.Processing;
                }
                response.status = (int)ApprovalStatusEnum.Referred;

            }
            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId != (int)ApprovalStatusEnum.Disapproved)
            {
                if (workflow.StatusId != (int)ApprovalStatusEnum.Referred)
                {
                    if (release.COLLATERALRELEASETYPEID == (int)CollateralReleaseType.FinalRelease) //OperationsEnum.FinalCollateralRelease
                    {
                        output = FinalReleaseCollateralApproval(model, model.createdBy);

                    }
                    else if (release.COLLATERALRELEASETYPEID == (int)CollateralReleaseType.TemporaryRelease) //OperationsEnum.TemporalCollateralRelease
                    {
                        output = TemporaryReleaseCollateralApproval(model, model.createdBy);

                    }
                    response.status = (int)ApprovalStatusEnum.Approved;
                }
                response.status = (int)ApprovalStatusEnum.Referred;
            }
            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
            {
                release.APPROVALSTATUSID = workflow.StatusId;
                response.status = (int)ApprovalStatusEnum.Disapproved;

            }


            if (output)
            {

                bool rec = context.SaveChanges() > 0;
                response.approvalLevel = context.TBL_APPROVAL_LEVEL.Where(a => a.APPROVALLEVELID == workflow.NextLevelId).FirstOrDefault().LEVELNAME;

                if (rec)
                {
                    return response;
                }

                return response;

            }
            else
            {
                response.status = 0;
                return response;
            }
        }

        public bool ReleaseCollateralJobRequest(CollateralViewModel entity)
        {
            var release = context.TBL_COLLATERAL_RELEASE.Where(a => a.COLLATERALRELEASEID == entity.collateralReleaseId).FirstOrDefault();

            //ValidateJobRequestCheck(release);

            // PendingJobRequestCheck(release);


            // var collateral = context.TBL_COLLATERAL_CUSTOMER.Find(entity.collateralId);
            release.JOBREQUESTSENT = true;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseAction,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Collateral Release Action '{ context.TBL_COLLATERAL_CUSTOMER.Find(release.COLLATERALCUSTOMERID).COLLATERALCODE }' Job Request Done ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            };
            this.auditTrail.AddAuditTrail(audit);

            var saved = context.SaveChanges() > 0;


            if (saved)
            {
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = release.COLLATERALRELEASEID;
                workflow.Comment = "Request for collateral release";
                workflow.OperationId = (int)release.COLLATERALRELEASETYPEID == 1 ? (int)OperationsEnum.FinalCollateralRelease : (int)OperationsEnum.TemporalCollateralRelease;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                return context.SaveChanges() > 0;

            } // audit here

            return false;
        }

        public bool ReleaseCollateral(CollateralViewModel entity)
        {

            var collateral = context.TBL_COLLATERAL_CUSTOMER.Find(entity.collateralId);

            var validate = context.TBL_COLLATERAL_RELEASE.Where(a => a.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID && (a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)).FirstOrDefault();
            if (validate != null)
            {
                if (entity.releaseType == 2) //Temporary Release
                {
                    foreach (var val in entity.documents)
                    {
                        var record = (from a in context.TBL_COLLATERAL_RELEASE_DOC
                                      join b in context.TBL_COLLATERAL_RELEASE on a.COLLATERALRELEASEID equals b.COLLATERALRELEASEID
                                      where a.DOCUMENTID == val.documentId && b.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID
                                      select new { a }).FirstOrDefault();
                        if (record != null)
                        {
                            throw new ConditionNotMetException("Document Release is Already Undergoing Approval For Release.");
                        }
                    }

                }

                if (entity.releaseType == 1) //Final Release
                {
                    throw new ConditionNotMetException("Collateral is Already Undergoing Approval For Release.");
                }

            }

            if (entity.releaseType == 2) //Temporary Release
            {
                foreach (var val in entity.documents)
                {
                    var record = (from a in context.TBL_COLLATERAL_RELEASE_DOC
                                  join b in context.TBL_COLLATERAL_RELEASE on a.COLLATERALRELEASEID equals b.COLLATERALRELEASEID
                                  where a.DOCUMENTID == val.documentId && b.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID
                                  select new { a }).FirstOrDefault();
                    if (record != null)
                    {
                        throw new ConditionNotMetException("Document Release is Already Undergoing Approval For Release.");
                    }
                }

            }
            bool saved = false;

            TBL_COLLATERAL_RELEASE release = new TBL_COLLATERAL_RELEASE();
            TBL_COLLATERAL_RELEASE_DOC releaseDoc = new TBL_COLLATERAL_RELEASE_DOC();

            release.DESCRIPTION = entity.comment;
            release.COLLATERALRELEASETYPEID = (int)entity.releaseType;
            release.COLLATERALCUSTOMERID = collateral.COLLATERALCUSTOMERID;
            release.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
            release.CREATEDBY = entity.createdBy;
            release.DATETIMECREATED = DateTime.Now.Date;
            release.JOBREQUESTSENT = false;

            context.TBL_COLLATERAL_RELEASE.Add(release);
            if (entity.releaseType == 1) //Final Release
            {
                saved = context.SaveChanges() > 0;

                var document = documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.Where(a => a.COLLATERALCUSTOMERID == release.COLLATERALCUSTOMERID).ToList();
                if (document.Count() > 0)
                {
                    foreach (var a in document)
                    {
                        releaseDoc.DOCUMENTID = a.DOCUMENTID;
                        releaseDoc.COLLATERALRELEASEID = release.COLLATERALRELEASEID;
                        releaseDoc.CREATEDBY = entity.createdBy;
                        releaseDoc.DATETIMECREATED = DateTime.Now.Date;

                        context.TBL_COLLATERAL_RELEASE_DOC.Add(releaseDoc);
                    }
                }
            }

            if (entity.releaseType == 2) //Temporary Release
            {
                if (entity.documents.Count() > 0)
                {
                    saved = context.SaveChanges() > 0;

                    foreach (var a in entity.documents)
                    {
                        releaseDoc.DOCUMENTID = a.documentId;
                        releaseDoc.COLLATERALRELEASEID = release.COLLATERALRELEASEID;
                        releaseDoc.CREATEDBY = entity.createdBy;
                        releaseDoc.DATETIMECREATED = DateTime.Now.Date;

                        context.TBL_COLLATERAL_RELEASE_DOC.Add(releaseDoc);
                    }
                }
            }



            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseAction,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Collateral Release Action '{ context.TBL_COLLATERAL_CUSTOMER.Find(release.COLLATERALCUSTOMERID).COLLATERALCODE }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            };
            this.auditTrail.AddAuditTrail(audit);

            if (saved)
            {
                return context.SaveChanges() > 0;
            }

            return false;
        }
        public int AddReleaseDocument(CollateralViewModel model, byte[] file)
        {
            //var existing = bankingContext.TBL_DOC_COLLATERAL_RELEASE
            //    .Where(x => x.FILENAME == model.fileName
            //        && x.FILEEXTENSION == model.fileExtension
            //        && x.COLLATERALCODE == model.collateralCode
            //        );

            //if (existing.Count() > 0 && model.overwrite == false) return 3;

            //if (existing.Count() > 0 && model.overwrite == true)
            //{
            //    bankingContext.TBL_DOC_COLLATERAL_RELEASE.RemoveRange(existing);
            //}

            var data = new TBL_DOC_COLLATERAL_RELEASE
            {
                FILEDATA = file,
                COLLATERALCODE = model.collateralCode,
                COLLATERALRELEASEID = model.collateralReleaseId,
                COLLATERALCUSTOMERID = model.collateralCustomerId,
                // DOCUMENTTYPEID = model.documentTypeId,
                //LOAN_BOOKING_REQUESTID = model.SourceId,
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension,
                SYSTEMDATETIME = DateTime.Now,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending,
                CREATEDBY = (int)model.createdBy,
            };

            documentContext.TBL_DOC_COLLATERAL_RELEASE.Add(data);
            documentContext.SaveChanges();
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Release Document with Collateral Code : '{ model.collateralCode }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() == 0 ? 1 : 2;
        }

        public IEnumerable<CollateralViewModel> GetCollateralReleaseDocument(int releaseId)
        {
            string createdBy = string.Empty;

            var data = (from x in this.documentContext.TBL_DOC_COLLATERAL_RELEASE
                        where x.COLLATERALRELEASEID == releaseId
                        select new CollateralViewModel
                        {
                            documentId = x.DOCUMENTID,
                            file = x.FILEDATA,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            collateralCode = x.COLLATERALCODE,
                            collateralCustomerId = x.COLLATERALCUSTOMERID,
                            collateralReleaseId = x.COLLATERALRELEASEID,
                            approvalStatusName = (int)x.APPROVALSTATUSID,
                            createdBy = (int)x.CREATEDBY,
                            //systemDateTime = x.SYSTEMDATETIME,
                            //capturedBy = context.TBL_STAFF.Where(a => a.STAFFID == x.CREATEDBY).Select(r => r.FIRSTNAME + " " + r.MIDDLENAME + " " + r.LASTNAME).FirstOrDefault(),

                        }).ToList();

            foreach (var x in data)
            {
                createdBy = context.TBL_STAFF.Where(a => a.STAFFID == x.createdBy).Select(r => r.FIRSTNAME + " " + r.MIDDLENAME + " " + r.LASTNAME).FirstOrDefault();
                //systemDateTime = x.SYSTEMDATETIME,
                x.capturedBy = createdBy;
            }


            return data;
        }

        public CollateralViewModel GetReleaseSupportingDocument(int documentId)
        {
            string createdBy = string.Empty;

            var data = (from x in this.documentContext.TBL_DOC_COLLATERAL_RELEASE
                        where x.DOCUMENTID == documentId
                        select new CollateralViewModel
                        {
                            documentId = x.DOCUMENTID,
                            file = x.FILEDATA,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            collateralCode = x.COLLATERALCODE,
                            collateralCustomerId = x.COLLATERALCUSTOMERID,
                            collateralReleaseId = x.COLLATERALRELEASEID,
                            approvalStatusName = (int)x.APPROVALSTATUSID,
                            createdBy = (int)x.CREATEDBY,

                        }).FirstOrDefault();
            createdBy = context.TBL_STAFF.Where(a => a.STAFFID == data.createdBy).Select(r => r.FIRSTNAME + " " + r.MIDDLENAME + " " + r.LASTNAME).FirstOrDefault();
            data.capturedBy = createdBy;
            return data;
        }


        //public async Task<bool> UpdateCollateral(CollateralViewModel entity, int collateralId)
        //{
        //    UpdateCollateralMainForm(entity, collateralId);

        //    switch (entity.collateralTypeId)
        //    {
        //        // case (int)CollateralTypeEnum.TermDeposit: UpdateDepositCollateral(entity); break;
        //        case (int)CollateralTypeEnum.PlantAndMachinery: UpdateEquipmentCollateral(entity); break;
        //        // case (int)CollateralTypeEnum.Miscellaneous: UpdateMiscellaneousCollateral(entity); break;
        //        case (int)CollateralTypeEnum.Gaurantee: UpdateGuaranteeCollateral(entity); break;
        //        case (int)CollateralTypeEnum.CASA: UpdateCasaCollateral(entity); break;
        //        case (int)CollateralTypeEnum.Property:  UpdateImmovablePropertyCollateral(entity); break;
        //        case (int)CollateralTypeEnum.TreasuryBillsAndBonds: UpdateMarketableSecuritiesCollateral(entity); break;
        //        case (int)CollateralTypeEnum.InsurancePolicy: UpdatePolicyCollateral(entity); break;
        //        case (int)CollateralTypeEnum.PreciousMetal: UpdatePreciousMetalCollateral(entity); break;
        //        case (int)CollateralTypeEnum.MarketableSecurities_Shares: UpdateStockCollateral(entity); break;
        //        case (int)CollateralTypeEnum.Vehicle: UpdateVehicleCollateral(entity); break;
        //        case (int)CollateralTypeEnum.Promissory: UpdatePromissoryCollateral(entity); break;

        //        default: break;
        //    }

        //    if (entity.hasInsurance) { UpdateItemInsurancePolicy(entity); }

        //    bool saved = await context.SaveChangesAsync() != 0;

        //    if (saved) { return true; } // audit here

        //    return false;
        //}

        public bool UpdateCollateral(CollateralViewModel entity, int collateralId)
        {
                entity.collateralId = collateralId;
                UpdateCollateralMainForm(entity, collateralId);

                switch (entity.collateralTypeId)
                {
                    case (int)CollateralTypeEnum.FixedDeposit: UpdateDepositCollateral(entity); break;
                    case (int)CollateralTypeEnum.PlantAndMachinery: UpdateEquipmentCollateral(entity); break;
                    //  case (int)CollateralTypeEnum.Miscellaneous: UpdateMiscellaneousCollateral(entity); break;
                    case (int)CollateralTypeEnum.Gaurantee: UpdateGuaranteeCollateral(entity); break;
                    case (int)CollateralTypeEnum.CASA: UpdateCasaCollateral(entity); break;
                    case (int)CollateralTypeEnum.Property: UpdateImmovablePropertyCollateral(entity); break;
                    case (int)CollateralTypeEnum.TreasuryBillsAndBonds: UpdateMarketableSecuritiesCollateral(entity); break;
                    case (int)CollateralTypeEnum.InsurancePolicy: UpdatePolicyCollateral(entity); break;
                    case (int)CollateralTypeEnum.PreciousMetal: UpdatePreciousMetalCollateral(entity); break;
                    case (int)CollateralTypeEnum.MarketableSecurities_Shares: UpdateStockCollateral(entity); break;
                    case (int)CollateralTypeEnum.Vehicle: UpdateVehicleCollateral(entity); break;
                    case (int)CollateralTypeEnum.Promissory: UpdatePromissoryCollateral(entity); break;
                    case (int)CollateralTypeEnum.ISPO: UpdateISPOCollateral(entity); break;
                    case (int)CollateralTypeEnum.DomiciliationSalary: UpdateSalaryDomiciliationCollateral(entity); break;
                    case (int)CollateralTypeEnum.DomiciliationContract: UpdateContractDomiciliationCollateral(entity); break;
                    case (int)CollateralTypeEnum.Indemity: UpdateIndemityCollateral(entity); break;

                    default: break;
                }

            //if (entity.hasInsurance) { UpdateItemInsurancePolicy(entity); }
           
                bool saved = context.SaveChanges() > 0;

                if (saved) { return true; } // audit here

                return false;
            
        }

        // MAIN collateral


        private int AddCollateralMainFormForGurantee(CollateralViewModel model)
        {
            if (context.TBL_TEMP_COLLATERAL_CUSTOMER.Where(x => x.TEMPCOLLATERALCUSTOMERID == model.collateralId).Any() == true)
            {
                throw new SecureException("The specified Collateral Code is already used in the system!");
            }

            var collateral = context.TBL_TEMP_COLLATERAL_CUSTOMER.Add(new TBL_TEMP_COLLATERAL_CUSTOMER
            {
                COLLATERALTYPEID = model.collateralTypeId,
                COLLATERALSUBTYPEID = model.collateralSubTypeId,
                COLLATERALCODE = model.collateralCode,
                COLLATERALVALUE = (decimal)model.collateralValue,
                COMPANYID = model.companyId,
                ALLOWSHARING = model.allowSharing,
                ISLOCATIONBASED = model.isLocationBased,
                VALUATIONCYCLE = model.valuationCycle,
                HAIRCUT = model.haircut,
                CURRENCYID = model.currencyId,
                CUSTOMERID = model.customerId,
                CAMREFNUMBER = model.camRefNumber,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                ISCURRENT = true,
            });

            if (context.SaveChanges() == 1)
            {
                return collateral.TEMPCOLLATERALCUSTOMERID;
            }

            return 0;
        }

        private void UpdateCollateralMainForm(CollateralViewModel model, int collateralId)
        {
           
                var collateral = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId);
                if (collateral == null) return;
                if (collateral.VALIDTILL != model.validTill)
                {
                    NotifyForCollateralValidity(collateral, model.validTill);
                }
                collateral.COLLATERALTYPEID = model.collateralTypeId;
                collateral.COLLATERALSUBTYPEID = model.collateralSubTypeId;
                collateral.COLLATERALCODE = model.collateralCode.Trim();
                collateral.COLLATERALVALUE = (decimal)model.collateralValue;
                collateral.ALLOWSHARING = model.allowSharing;
                collateral.ISLOCATIONBASED = model.isLocationBased;
                collateral.VALUATIONCYCLE = model.valuationCycle;
                collateral.HAIRCUT = model.haircut;
                collateral.CURRENCYID = model.currencyId;
                collateral.CAMREFNUMBER = model.camRefNumber;
                collateral.LASTUPDATEDBY = model.lastUpdatedBy;
                collateral.DATETIMEUPDATED = genSetup.GetApplicationDate();
                collateral.EXCHANGERATE = model.exchangeRate;
                collateral.COLLATERALSUMMARY = model.collateralSummary;
                collateral.VALIDTILL = model.validTill;
            
        }

        private void DeleteCollateral(int collateralId)
        {
            var collateral = context.TBL_TEMP_COLLATERAL_CUSTOMER.Find(collateralId);
            collateral.DELETED = true; // audit here
            if (collateral != null)
            {
                context.SaveChanges();
            }
        }

        private void DeleteCollateralDocument(int collateralId)
        {
            var collateral = documentContext.TBL_TEMP_MEDIA_COLLATERAL_DOCS.Find(collateralId);
            documentContext.TBL_TEMP_MEDIA_COLLATERAL_DOCS.Remove(collateral); // audit here
            if (collateral != null)
            {
                documentContext.SaveChanges();
            }
        }



        // EQUIPMENT collateral

        private void AddTempEquipmentCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainPlant = (from x in context.TBL_COLLATERAL_PLANT_AND_EQUIP
                                 where x.COLLATERALCUSTOMERID == collateralId
                                 select (x)).FirstOrDefault();

                if (mainPlant != null)
                {
                    mainPlant.REMARK = entity.remark;
                    mainPlant.DESCRIPTION = entity.description;
                    mainPlant.INTENDEDUSE = entity.intendedUse;
                    mainPlant.EQUIPMENTSIZE = entity.equipmentSize;
                    mainPlant.MACHINECONDITION = entity.machineCondition;
                    mainPlant.MACHINENAME = entity.machineName;
                    mainPlant.MACHINENUMBER = entity.machineNumber;
                    mainPlant.MACHINERYLOCATION = entity.machineryLocation;
                    mainPlant.MANUFACTURERNAME = entity.manufacturerName;
                    mainPlant.REPLACEMENTVALUE = entity.replacementValue;
                    mainPlant.VALUEBASETYPEID = (short)entity.valueBaseTypeId;
                    mainPlant.YEAROFMANUFACTURE = entity.yearOfManufacture;
                    mainPlant.YEAROFPURCHASE = entity.yearOfPurchase;
                    comment = "Update plant and equipment collateral through loan application";
                }
                else
                {
                    context.TBL_COLLATERAL_PLANT_AND_EQUIP.Add(new TBL_COLLATERAL_PLANT_AND_EQUIP
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        MACHINENAME = entity.machineName,
                        DESCRIPTION = entity.description,
                        MACHINENUMBER = entity.machineNumber,
                        MANUFACTURERNAME = entity.manufacturerName,
                        YEAROFMANUFACTURE = entity.yearOfManufacture,
                        YEAROFPURCHASE = entity.yearOfPurchase,
                        VALUEBASETYPEID = (short)entity.valueBaseTypeId,
                        MACHINECONDITION = entity.machineCondition,
                        MACHINERYLOCATION = entity.machineryLocation,
                        REPLACEMENTVALUE = entity.replacementValue,
                        EQUIPMENTSIZE = entity.equipmentSize,
                        INTENDEDUSE = entity.intendedUse,
                        REMARK = entity.remark,

                    });
                    comment = "New plant and equipment collateral created through loan application";
                }
            }
            else
            {
                context.TBL_TEMP_COLLATERAL_PLANT_EQUP.Add(new TBL_TEMP_COLLATERAL_PLANT_EQUP
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    MACHINENAME = entity.machineName,
                    DESCRIPTION = entity.description,
                    MACHINENUMBER = entity.machineNumber,
                    MANUFACTURERNAME = entity.manufacturerName,
                    YEAROFMANUFACTURE = entity.yearOfManufacture,
                    YEAROFPURCHASE = entity.yearOfPurchase,
                    VALUEBASETYPEID = (short)entity.valueBaseTypeId,
                    MACHINECONDITION = entity.machineCondition,
                    MACHINERYLOCATION = entity.machineryLocation,
                    REPLACEMENTVALUE = entity.replacementValue,
                    EQUIPMENTSIZE = entity.equipmentSize,
                    INTENDEDUSE = entity.intendedUse,
                    REMARK = entity.remark,

                });
                comment = "New plant and equipment collateral created through Setup";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

        }

        private void UpdateEquipmentCollateral(CollateralViewModel model)
        {
            var collateral = context.TBL_COLLATERAL_PLANT_AND_EQUIP
                .Where(x => x.COLLATERALCUSTOMERID == model.collateralId)
                .FirstOrDefault();
            TBL_COLLATERAL_PLANT_AND_EQUIP equipCollateral = new TBL_COLLATERAL_PLANT_AND_EQUIP();
            if (collateral == null)
            {

                equipCollateral.MACHINENAME = model.machineName;
                equipCollateral.DESCRIPTION = model.description;
                equipCollateral.MACHINENUMBER = model.machineNumber;
                equipCollateral.MANUFACTURERNAME = model.manufacturerName;
                equipCollateral.YEAROFMANUFACTURE = model.yearOfManufacture;
                equipCollateral.YEAROFPURCHASE = model.yearOfPurchase;
                equipCollateral.VALUEBASETYPEID = (short)model.valueBaseTypeId;
                equipCollateral.MACHINECONDITION = model.machineCondition;
                equipCollateral.MACHINERYLOCATION = model.machineryLocation;
                equipCollateral.REPLACEMENTVALUE = model.replacementValue;
                equipCollateral.EQUIPMENTSIZE = model.equipmentSize;
                equipCollateral.INTENDEDUSE = model.intendedUse;
                equipCollateral.REMARK = model.remark;
                equipCollateral.COLLATERALCUSTOMERID = model.collateralId;
                context.TBL_COLLATERAL_PLANT_AND_EQUIP.Add(equipCollateral);
                return;
            }
            collateral.MACHINENAME = model.machineName;
            collateral.DESCRIPTION = model.description;
            collateral.MACHINENUMBER = model.machineNumber;
            collateral.MANUFACTURERNAME = model.manufacturerName;
            collateral.YEAROFMANUFACTURE = model.yearOfManufacture;
            collateral.YEAROFPURCHASE = model.yearOfPurchase;
            collateral.VALUEBASETYPEID = (short)model.valueBaseTypeId;
            collateral.MACHINECONDITION = model.machineCondition;
            collateral.MACHINERYLOCATION = model.machineryLocation;
            collateral.REPLACEMENTVALUE = model.replacementValue;
            collateral.EQUIPMENTSIZE = model.equipmentSize;
            collateral.INTENDEDUSE = model.intendedUse;
            collateral.REMARK = model.remark;

        }



        //private void UpdateDepositCollateral(CollateralViewModel entity)
        //{
        //    var collateral = context.TBL_COLLATERAL_DEPOSIT
        //        .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
        //        .FirstOrDefault();

        //    collateral.DEALREFERENCENUMBER = entity.dealReferenceNumber;
        //    collateral.ACCOUNTNUMBER = "0";
        //    collateral.EXISTINGLIENAMOUNT = 0;
        //    collateral.LIENAMOUNT = entity.lienAmount;
        //    collateral.AVAILABLEBALANCE = entity.availableBalance;
        //    collateral.SECURITYVALUE = (decimal)entity.securityValue;
        //    collateral.MATURITYDATE = entity.maturityDate;
        //    collateral.MATURITYAMOUNT = 0;
        //    collateral.EFFECTIVEDATE = entity.effectiveDate;
        //    collateral.REMARK = entity.remark;
        //    collateral.BANK = entity.bank;
        //}

        // MISCELALEOUS

        private void AddTempMiscellaneousCollateral(int collateralId, CollateralViewModel entity)
        {
            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainMis = (from x in context.TBL_COLLATERAL_MISCELLANEOUS
                               where x.COLLATERALCUSTOMERID == collateralId
                               select (x)).FirstOrDefault();

                if (mainMis != null)
                {
                    mainMis.COLLATERALCUSTOMERID = collateralId;
                    mainMis.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
                    mainMis.NAMEOFSECURITY = entity.securityName;
                    mainMis.SECURITYVALUE = (decimal)entity.securityValue;
                    mainMis.NOTE = entity.note;
                }
                else
                {
                    context.TBL_COLLATERAL_MISCELLANEOUS.Add(new TBL_COLLATERAL_MISCELLANEOUS
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                        NAMEOFSECURITY = entity.securityName,
                        SECURITYVALUE = (decimal)entity.securityValue,
                        NOTE = entity.note
                    });
                }


            }
            else
            {
                var collateral = context.TBL_TEMP_COLLATERAL_MISCELLAN.Add(new TBL_TEMP_COLLATERAL_MISCELLAN
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    NAMEOFSECURITY = entity.securityName,
                    SECURITYVALUE = (decimal)entity.securityValue,
                    NOTE = entity.note,
                });

               var comment = $"New temp miscellaneous collateral type has been created by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
               
                AddMiscellaneousNotes(entity, collateral.TEMPCOLLATERALMISCELLANEOUSID);
            }
        }

        private void AddMiscellaneousNotes(CollateralViewModel entity, int miscellaneousId)
        {
            if (entity.notes != null)
            {
                foreach (var note in entity.notes)
                {
                    context.TBL_COLLATERAL_MISC_NOTES.Add(new TBL_COLLATERAL_MISC_NOTES
                    {
                        MISCELLANEOUSID = miscellaneousId,
                        COLUMNNAME = note.labelName,
                        COLUMNVALUE = note.labelValue,
                        CREATEDBY = entity.createdBy,
                        DATETIMECREATED = DateTime.Now
                    });
                }
                context.SaveChanges();
            }
        }

        //private void UpdateMiscellaneousCollateral(CollateralViewModel entity)
        //{
        //    var collateral = context.TBL_COLLATERAL_MISCELLANEOUS
        //        .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
        //        .FirstOrDefault();

        //    collateral.NAMEOFSECURITY = entity.securityName;
        //    collateral.SECURITYVALUE = (decimal)entity.securityValue;

        //    UpdateMiscellaneousNotes(entity, collateral.COLLATERALMISCELLANEOUSID);
        //}

        private void UpdateMiscellaneousNotes(CollateralViewModel entity, int miscellaneousId)
        {
            var notes = context.TBL_COLLATERAL_MISC_NOTES.Where(x => x.MISCELLANEOUSID == miscellaneousId);
            foreach (var note in notes)
            {
                note.COLUMNVALUE = entity.notes.FirstOrDefault(x => x.labelName == note.COLUMNNAME).labelValue;
            }
        }

        // ITEM INSURANCE

        public void AddTempItemInsurancePolicy(int collateralId, CollateralViewModel entity)
        {
            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainPol = (from x in context.TBL_COLLATERAL_ITEM_POLICY
                               where x.COLLATERALCUSTOMERID == collateralId
                               select (x)).FirstOrDefault();

                if (mainPol != null)
                {
                    mainPol.COLLATERALCUSTOMERID = entity.collateralCustomerId;
                    mainPol.CREATEDBY = entity.createdBy;
                    mainPol.DATETIMECREATED = entity.dateTimeCreated;
                    mainPol.ENDDATE = (DateTime)entity.endDate;
                    mainPol.INSURANCECOMPANYID = entity.insuranceCompanyId;
                    mainPol.INSURANCETYPEID = entity.insuranceTypeId;
                    mainPol.LASTUPDATEDBY = entity.lastUpdatedBy;
                    mainPol.POLICYREFERENCENUMBER = entity.referenceNumber;
                    mainPol.STARTDATE = (DateTime)entity.startDate;
                    mainPol.SUMINSURED = entity.sumInsured;
                    mainPol.PREMIUMAMOUNT = entity.premiumAmount;
                    mainPol.DESCRIPTION = entity.description;
                    mainPol.PREMIUMPERCENT = entity.premiumPercent;
                }
                else
                {
                    context.TBL_COLLATERAL_ITEM_POLICY.Add(new TBL_COLLATERAL_ITEM_POLICY
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        POLICYREFERENCENUMBER = entity.referenceNumber,
                        INSURANCECOMPANYID = entity.insuranceCompanyId,
                        SUMINSURED = entity.sumInsured,
                        STARTDATE = (DateTime)entity.startDate,
                        ENDDATE = (DateTime)entity.expiryDate,
                        INSURANCETYPEID = entity.insuranceTypeId,
                        CREATEDBY = entity.createdBy,
                        DATETIMECREATED = DateTime.Now,
                        DELETED = false,
                        PREMIUMAMOUNT = entity.inSurPremiumAmount,
                        PREMIUMPERCENT = entity.premiumPercent,
                        DESCRIPTION = entity.description,
                        HASEXPIRED = false,
                        //APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing
                        // APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing

                    });

                }


            }
            else
            {
                context.TBL_TEMP_COLLATERAL_ITEM_POLI.Add(new TBL_TEMP_COLLATERAL_ITEM_POLI
                {
                    COLLATERALCUSTOMERID = collateralId,
                    POLICYREFERENCENUMBER = entity.referenceNumber,
                    INSURANCECOMPANYID = entity.insuranceCompanyId,
                    SUMINSURED = entity.sumInsured,
                    STARTDATE = (DateTime)entity.startDate,
                    ENDDATE = (DateTime)entity.expiryDate,
                    INSURANCETYPEID = entity.insuranceTypeId,
                    CREATEDBY = entity.createdBy,
                    DATETIMECREATED = DateTime.Now,
                    DELETED = false,
                    PREMIUMAMOUNT = entity.inSurPremiumAmount,
                    DESCRIPTION = entity.description,
                    //  PREMIUMPERCENT = entity.premiumPercent

                });
            }


        }

        public bool AddNewItemInsurancePolicy(InsurancePolicy entity)
        {
            var policy = context.TBL_TEMP_COLLATERAL_ITEM_POLI.Add(new TBL_TEMP_COLLATERAL_ITEM_POLI
            {
                COLLATERALCUSTOMERID = (int)entity.collateraalId,
                POLICYREFERENCENUMBER = entity.referenceNumber,
                INSURANCECOMPANYID = (int)entity.insuranceCompanyId,
                SUMINSURED = (decimal)entity.sumInsured,
                STARTDATE = (DateTime)entity.startDate,
                ENDDATE = (DateTime)entity.expiryDate,
                INSURANCETYPEID = (int)entity.insuranceTypeId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                ISPOLICYAPPROVAL = true,
                DELETED = false,
                PREMIUMAMOUNT = entity.inSurPremiumAmount,

            });

            if (context.SaveChanges() > 0)
            {
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = policy.TEMPPOLICYID;
                workflow.Comment = "Request for item policy approval";
                workflow.OperationId = (int)OperationsEnum.IsurancePolicyApproval;
                workflow.DeferredExecution = false; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                return true;
            }
            return false;

        }

        public bool checkInsurancePolicy(InsurancePolicy model)
        {
            try
            {
                var result = context.TBL_COLLATERAL_ITEM_POLICY.Where(ip => ip.COLLATERALCUSTOMERID == model.collateraalId
                                                                    && ip.APPROVALSTATUSID != (short)ApprovalStatusEnum.Approved
                                                                    && ip.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved).ToList();

                if (result.Count > 0) return false;
                else return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public bool UpdateInsurancePolicy(int id, CollateralInsurancePolicyViewModel model)
        {
            var entity = this.context.TBL_COLLATERAL_ITEM_POLICY.Find(id);
            if (entity == null) return false;

            entity.INSURANCECOMPANYID = model.insuranceCompanyId;
            entity.COMPANYADDRESS = model.companyAddress;
            entity.SUMINSURED = model.sumInsured;
            entity.STARTDATE = (DateTime)model.startDate;
            entity.ENDDATE = (DateTime)model.expiryDate;
            entity.INSURANCETYPEID = model.insuranceTypeId;
            entity.PREMIUMAMOUNT = model.inSurPremiumAmount;
            entity.PREMIUMPERCENT = model.premiumPercent;
            entity.DESCRIPTION = model.description;
            entity.POLICYSTATEID = model.policyStateId;
            entity.LASTUPDATEDBY = model.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            return context.SaveChanges() != 0;
        }

        public bool AddInsurancePolicy(CollateralInsurancePolicyViewModel entity)
        {
            var result = context.TBL_COLLATERAL_ITEM_POLICY.Where(ip => ip.COLLATERALCUSTOMERID == entity.collateraalId
                                                                    && ip.APPROVALSTATUSID != (short)ApprovalStatusEnum.Approved
                                                                    && ip.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved).ToList();

            if (result.Count > 0) throw new SecureException("An Insurance Request for this Collateral is currently Undergoing Approval");

            var policy = context.TBL_COLLATERAL_ITEM_POLICY.Add(new TBL_COLLATERAL_ITEM_POLICY
            {
                COLLATERALCUSTOMERID = entity.collateraalId,
                POLICYREFERENCENUMBER = entity.referenceNumber,
                INSURANCECOMPANYID = entity.insuranceCompanyId,
                COMPANYADDRESS = entity.companyAddress,
                SUMINSURED = entity.sumInsured,
                STARTDATE = (DateTime)entity.startDate,
                ENDDATE = (DateTime)entity.expiryDate,
                INSURANCETYPEID = entity.insuranceTypeId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,
                PREMIUMAMOUNT = entity.inSurPremiumAmount,
                PREMIUMPERCENT = entity.premiumPercent,
                DESCRIPTION = entity.description,
                HASEXPIRED = false,
                POLICYSTATEID = entity.policyStateId,
                APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing


            });

            return (context.SaveChanges() > 0);

        }

        public bool InsuranceRequestGoForApproval(CollateralViewModel model)
        {
            if (model.approvalStatusId == (short)ApprovalStatusEnum.Referred)
            {
                bool response = false;

                using (var transaction = context.Database.BeginTransaction())
                {
                    workflow.StaffId = model.createdBy;
                    workflow.CompanyId = model.companyId;
                    workflow.StatusId = (short)ApprovalStatusEnum.Processing;
                    workflow.TargetId = model.insuranceRequestId;
                    workflow.Comment = "Update has been applied, Request for Insurance Policy Approval";
                    workflow.OperationId = (int)OperationsEnum.IsurancePolicyApproval;
                    workflow.DeferredExecution = true;
                    workflow.LogActivity();
                    try
                    {

                        response = context.SaveChanges() > 0;
                        transaction.Commit();

                        return response;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
            else
            {
                bool response = false;

                var entity2 = context.TBL_INSURANCE_REQUEST.FirstOrDefault(ir => ir.INSURANCEREQUESTID == model.insuranceRequestId);
                if (entity2 == null) return false;

                var entity = (from ir in context.TBL_INSURANCE_REQUEST
                              join cip in context.TBL_COLLATERAL_ITEM_POLICY on ir.COLLATERALCUSTOMERID equals cip.COLLATERALCUSTOMERID
                              where model.collateralId == cip.COLLATERALCUSTOMERID &&
                                 cip.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                              select cip).ToList();

                if (entity.Any()) throw new SecureException("Collateral Item already undergoing Insurnace Request Approval");





                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = model.insuranceRequestId;
                workflow.Comment = "Request for Insurance Policy approval";
                workflow.OperationId = (int)OperationsEnum.IsurancePolicyApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();


                response = context.SaveChanges() != 0;

                if (response) entity2.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;

                return context.SaveChanges() > 0;
            }


        }

        public string GetReferenceNumber()
        {
            var referenceNumber = CommonHelpers.GenerateRandomDigitCode(10);

            return referenceNumber;
        }

        public string GetLastComment(int targetId, int operationId)
        {
            var result = (from ir in context.TBL_INSURANCE_REQUEST
                          join trail in context.TBL_APPROVAL_TRAIL on ir.INSURANCEREQUESTID equals trail.TARGETID
                          where targetId == trail.TARGETID && operationId == trail.OPERATIONID
                             && ir.INSURANCEREQUESTID == targetId
                          orderby trail.APPROVALTRAILID descending
                          select new CollateralViewModel()
                          {
                              lastApprovalComment = trail.COMMENT,
                          }).FirstOrDefault();

            var comment = result.lastApprovalComment;

            return comment;

        }
        public IEnumerable<CollateralViewModel> GetInsuranceRequests(int staffId)
        {

            var initiator = context.TBL_APPROVAL_TRAIL.Where(o => o.OPERATIONID == (int)OperationsEnum.IsurancePolicyApproval).OrderBy(o => o.APPROVALTRAILID).Select(o => o.REQUESTSTAFFID).FirstOrDefault();

            var result1 = (from ir in context.TBL_INSURANCE_REQUEST
                           join cc in context.TBL_COLLATERAL_CUSTOMER on ir.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                           where ir.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending
                           orderby ir.INSURANCEREQUESTID descending
                           select new CollateralViewModel()
                           {
                               collateralId = cc.COLLATERALCUSTOMERID,
                               collateralTypeId = cc.COLLATERALTYPEID,
                               collateralSubTypeId = cc.COLLATERALSUBTYPEID,
                               customerId = cc.CUSTOMERID.Value,
                               collateralTypeName = context.TBL_COLLATERAL_TYPE.Where(ct => ct.COLLATERALTYPEID == cc.COLLATERALTYPEID).Select(s => s.COLLATERALTYPENAME).FirstOrDefault(),
                               collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(cts => cts.COLLATERALSUBTYPEID == cc.COLLATERALSUBTYPEID).Select(s => s.COLLATERALSUBTYPENAME).FirstOrDefault(),
                               collateralCode = cc.COLLATERALCODE,
                               collateralValue = cc.COLLATERALVALUE,
                               approvalStatusId = ir.APPROVALSTATUSID,
                               statusName = context.TBL_APPROVAL_STATUS.Where(a => a.APPROVALSTATUSID == ir.APPROVALSTATUSID).Select(s => s.APPROVALSTATUSNAME).FirstOrDefault(),
                               collateralReleaseStatusId = cc.COLLATERALRELEASESTATUSID,
                               collateralReleaseStatusName = cc.COLLATERALRELEASESTATUSID == null ? context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.InVault).FirstOrDefault().COLLATERALRELEASESTATUSNAME : context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == cc.COLLATERALRELEASESTATUSID).FirstOrDefault().COLLATERALRELEASESTATUSNAME,
                               collateralUsageStatus = cc.COLLATERALUSAGESTATUSID,
                               requestNumber = ir.REQUESTNUMBER,
                               haircut = cc.HAIRCUT,
                               insuranceRequestId = ir.INSURANCEREQUESTID,
                               requestReason = ir.REQUESTREASON,
                               requestComment = ir.REQUESTCOMMENT,

                           }).ToList();

            var result2 = (from ir in context.TBL_INSURANCE_REQUEST
                           join cc in context.TBL_COLLATERAL_CUSTOMER on ir.COLLATERALCUSTOMERID equals cc.COLLATERALCUSTOMERID
                           join atrail in context.TBL_APPROVAL_TRAIL on ir.INSURANCEREQUESTID equals atrail.TARGETID
                           where atrail.OPERATIONID == (int)OperationsEnum.IsurancePolicyApproval
                            && (ir.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || ir.APPROVALSTATUSID == (short)ApprovalStatusEnum.Disapproved || ir.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred)
                            && atrail.TARGETID == ir.INSURANCEREQUESTID
                           orderby atrail.APPROVALTRAILID descending
                           select new CollateralViewModel()
                           {
                               loopedStaffId = atrail.LOOPEDSTAFFID,
                               collateralId = cc.COLLATERALCUSTOMERID,
                               collateralTypeId = cc.COLLATERALTYPEID,
                               collateralSubTypeId = cc.COLLATERALSUBTYPEID,
                               customerId = cc.CUSTOMERID.Value,
                               collateralTypeName = context.TBL_COLLATERAL_TYPE.Where(ct => ct.COLLATERALTYPEID == cc.COLLATERALTYPEID).Select(s => s.COLLATERALTYPENAME).FirstOrDefault(),
                               collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(cts => cts.COLLATERALSUBTYPEID == cc.COLLATERALSUBTYPEID).Select(s => s.COLLATERALSUBTYPENAME).FirstOrDefault(),
                               collateralCode = cc.COLLATERALCODE,
                               collateralValue = cc.COLLATERALVALUE,
                               approvalStatusId = atrail.APPROVALSTATUSID,
                               statusName = context.TBL_APPROVAL_STATUS.Where(a => a.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(s => s.APPROVALSTATUSNAME).FirstOrDefault(),
                               collateralReleaseStatusId = cc.COLLATERALRELEASESTATUSID,
                               collateralReleaseStatusName = cc.COLLATERALRELEASESTATUSID == null ? context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.InVault).FirstOrDefault().COLLATERALRELEASESTATUSNAME : context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == cc.COLLATERALRELEASESTATUSID).FirstOrDefault().COLLATERALRELEASESTATUSNAME,
                               collateralUsageStatus = cc.COLLATERALUSAGESTATUSID,
                               requestNumber = ir.REQUESTNUMBER,
                               haircut = cc.HAIRCUT,
                               insuranceRequestId = ir.INSURANCEREQUESTID,
                               requestReason = ir.REQUESTREASON,
                               requestComment = ir.REQUESTCOMMENT,

                           }).ToList().GroupBy(group => group.insuranceRequestId).Select(group => group.FirstOrDefault()).Where(first => (first.approvalStatusId == (short)ApprovalStatusEnum.Referred
                                && first.loopedStaffId == initiator) || first.approvalStatusId == (short)ApprovalStatusEnum.Disapproved || first.approvalStatusId == (short)ApprovalStatusEnum.Processing).ToList();


            var result = result1.Union(result2).ToList();

            return result;
        }

        public bool UpdateInsurancePolicyRequest(CollateralInsuranceRequestViewModel model, int id)
        {
            var entity = context.TBL_INSURANCE_REQUEST.Find(id);

            if (entity != null)
            {
                entity.REQUESTCOMMENT = model.requestComment;
                entity.REQUESTREASON = model.requestReason;
                entity.LASTUPDATEDBY = model.createdBy;
                entity.DATETIMEUPDATED = genSetup.GetApplicationDate();
            }

            return context.SaveChanges() > 0;
        }
        public bool AddInsurancePolicyRequest(CollateralInsuranceRequestViewModel model, int? id)
        {
            var data = context.TBL_INSURANCE_REQUEST.FirstOrDefault(d => d.COLLATERALCUSTOMERID == model.collateralCustomerId &&
                                                                        d.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing
                                                                        || d.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred);

            if (data != null) throw new SecureException("Collateral Item is already Undergoing Insurance Request Approval");

            var data2 = context.TBL_INSURANCE_REQUEST.FirstOrDefault(d => d.COLLATERALCUSTOMERID == model.collateralCustomerId &&
                                                                        d.APPROVALSTATUSID == (short)ApprovalStatusEnum.Pending);

            if (data2 != null) throw new SecureException("A Pending Insurance Request already exists for this Collateral");



            var policy = context.TBL_INSURANCE_REQUEST.Add(new TBL_INSURANCE_REQUEST
            {
                COLLATERALCUSTOMERID = model.collateralCustomerId,
                REQUESTNUMBER = model.requestNumber,
                REQUESTREASON = model.requestReason,
                REQUESTCOMMENT = model.requestComment,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Pending,
            });

            var output = context.SaveChanges() > 0;

            if (output == true && id != null)
            {
                var rejectedInsurance = context.TBL_INSURANCE_REQUEST.Find(id);

                if (rejectedInsurance == null || (rejectedInsurance != null && rejectedInsurance.APPROVALSTATUSID != (short)ApprovalStatusEnum.Disapproved))
                {
                    throw new SecureException("An Error Occured, Please Contact the System Administrator");
                }

                rejectedInsurance.APPROVALSTATUSID = (short)ApprovalStatusEnum.RePresent;

                return context.SaveChanges() > 0;

            }

            return output;

        }

        public bool SaveCollateralMainDocument(CollateralViewModel model, int collateralId, byte[] file)
        {
            if (model.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var data = new TBL_MEDIA_COLLATERAL_DOCUMENTS
                {
                    FILEDATA = file,
                    DOCUMENTCODE = model.documentTitle,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    COLLATERALCUSTOMERID = collateralId,
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    ISPRIMARYDOCUMENT = true,
                    TARGETID = model.TargetId,
                    DOCUMENTTYPEID = model.documentTypeId,

                };
            }
            else
            {
                var data = new TBL_TEMP_MEDIA_COLLATERAL_DOCS
                {
                    FILEDATA = file,
                    DOCUMENTCODE = model.documentTitle,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    ISPRIMARYDOCUMENT = true,
                    TARGETID = model.TargetId,
                    DOCUMENTTYPEID = model.documentTypeId,

                };

                documentContext.TBL_TEMP_MEDIA_COLLATERAL_DOCS.Add(data);
                try
                {
                    return documentContext.SaveChanges() != 0;
                }
                catch (Exception ex) { }
            }

            return documentContext.SaveChanges() != 0;
        }

        private void UpdateItemInsurancePolicy(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_ITEM_POLICY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();
            if (collateral != null)
            {
                collateral.POLICYREFERENCENUMBER = entity.referenceNumber;
                collateral.INSURANCECOMPANYID = entity.insuranceCompanyId;
                collateral.SUMINSURED = entity.sumInsured;
                collateral.STARTDATE = (DateTime)entity.startDate;
                collateral.ENDDATE = (DateTime)entity.expiryDate;
                collateral.INSURANCETYPEID = entity.insuranceTypeId;
                collateral.PREMIUMAMOUNT = entity.inSurPremiumAmount;
                collateral.DESCRIPTION = entity.description;
                collateral.PREMIUMPERCENT = entity.premiumPercent;
            }

        }

        // GET MAIN INFO
        public CollateralViewModel GetCustomerCollateralInformation(int colateralcustomerId, int companyId)
        {
            var typeIds = new List<int>();
            var company = context.TBL_COMPANY.Find(companyId);
            bool disAllowCollateral = false;
            bool isForiegnCurrencyFacility = false;

            //if (applicationId != null)
            //{
            //    var productIds = context.TBL_LOAN_APPLICATION_DETAIL
            //        .Where(x => x.LOANAPPLICATIONID == applicationId)
            //        .Select(x => x.PROPOSEDPRODUCTID)
            //        .Distinct();

            //    typeIds = context.TBL_PRODUCT_COLLATERALTYPE.Where(x => productIds.Contains(x.PRODUCTID))
            //       .Select(x => x.COLLATERALTYPEID)
            //       .Distinct().ToList();

            //    isForiegnCurrencyFacility = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.CURRENCYID != company.CURRENCYID).Any();
            //}

            var collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false && x.COLLATERALCUSTOMERID == colateralcustomerId)
                .GroupJoin(
                    context.TBL_LOAN_COLLATERAL_MAPPING,
                    c => c.COLLATERALCUSTOMERID,
                    lc => lc.COLLATERALCUSTOMERID,
                    (c, lc) => new { c, m = lc }
                )
                .SelectMany
                (
                    x => x.m.DefaultIfEmpty(),
                    (c, m) => new CollateralViewModel
                    {
                        collateralId = c.c.COLLATERALCUSTOMERID,
                        collateralTypeId = c.c.COLLATERALTYPEID,
                        collateralSubTypeId = c.c.COLLATERALSUBTYPEID,
                        customerId = c.c.CUSTOMERID.Value,
                        currencyId = c.c.CURRENCYID,
                        baseCurrencyId = company.CURRENCYID,
                        currency = c.c.TBL_CURRENCY.CURRENCYNAME,
                        disAllowCollateral = disAllowCollateral && c.c.CURRENCYID == company.CURRENCYID, // facilityCurrency != baseCurrency && collateralCurrency == baseCurrency
                        collateralTypeName = c.c.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                        collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(r => r.COLLATERALSUBTYPEID == c.c.COLLATERALSUBTYPEID).Select(q => q.COLLATERALSUBTYPENAME).FirstOrDefault(),
                        collateralCode = c.c.COLLATERALCODE,
                        collateralValue = c.c.COLLATERALVALUE,
                        camRefNumber = c.c.CAMREFNUMBER,
                        allowSharing = c.c.ALLOWSHARING,
                        isLocationBased = (bool)c.c.ISLOCATIONBASED,
                        valuationCycle = c.c.VALUATIONCYCLE,
                        haircut = c.c.HAIRCUT,
                        approvalStatusName = c.c.APPROVALSTATUS,
                        //allowApplicationMapping = typeIds.Contains((short)c.c.COLLATERALTYPEID),
                        requireInsurancePolicy = c.c.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                        exchangeRate = c.c.EXCHANGERATE,
                        availableCollateralValue = 0,
                        collateralSummary = c.c.COLLATERALSUMMARY,
                        accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == c.c.CUSTOMERID).ACCOUNTNUMBER,
                    }).FirstOrDefault()
                    ;
            decimal usage = 0;
            var mappings = context.TBL_LOAN_COLLATERAL_MAPPING.Where(m => m.COLLATERALCUSTOMERID == collaterals.collateralId && m.DELETED == false && m.ISRELEASED == false).ToList();
            foreach (var mapping in mappings)
            {
                usage = usage + GetLoanOutstandingBalance(mapping.LOANID, mapping.LOANSYSTEMTYPEID);
            }
            collaterals.availableCollateralValue = (decimal)collaterals.collateralValue - usage;


            //collaterals = ResolveCollateralValues(collaterals);

            //var count = collaterals.Count();
            //var test = collaterals;

            return collaterals;
        }

        public List<CollateralViewModel> GetCustomerPropertyCollaterals(int? customerId, int companyId)
        {
            var company = context.TBL_COMPANY.Find(companyId);
            if (customerId != null)
            {
                var collaterals = (from cc in context.TBL_COLLATERAL_CUSTOMER
                                   join ct in context.TBL_COLLATERAL_TYPE on cc.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                                   join cip in context.TBL_COLLATERAL_IMMOVE_PROPERTY on cc.COLLATERALCUSTOMERID equals cip.COLLATERALCUSTOMERID
                                   where cc.TBL_COMPANY.CURRENCYID == company.CURRENCYID
                                   && cc.DELETED == false && cc.CUSTOMERID == customerId
                                   select new CollateralViewModel()
                                   {
                                       collateralDetail = cip.PROPERTYNAME,
                                       collateralType = ct.COLLATERALTYPENAME,
                                       openMarketValue = cip.OPENMARKETVALUE,
                                       forcedSaleValue = cip.FORCEDSALEVALUE,
                                   }).ToList();
                return collaterals;
            }
            return null;
        }

        public IEnumerable<CollateralViewModel> GetCustomerCollateral(int customerId, int? applicationId, int companyId, bool isLMS)
        {
            var typeIds = new List<int>();
            var company = context.TBL_COMPANY.Find(companyId);
            var baseCurrencyId = company.TBL_CURRENCY.CURRENCYID;
            bool disAllowCollateral = false;
            bool isForiegnCurrencyFacility = false;
            var productIds = new List<short>();
            if (applicationId != null && applicationId != 0)
            {
                if (isLMS)
                {
                    productIds = context.TBL_LMSR_APPLICATION_DETAIL
                    .Where(x => x.LOANAPPLICATIONID == applicationId)
                    .Select(x => x.PRODUCTID)
                    .Distinct().ToList();

                    isForiegnCurrencyFacility = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.CURRENCYID != company.CURRENCYID).Any();
                }
                else
                {
                    productIds = context.TBL_LOAN_APPLICATION_DETAIL
                    .Where(x => x.LOANAPPLICATIONID == applicationId)
                    .Select(x => x.PROPOSEDPRODUCTID)
                    .Distinct().ToList();
                    isForiegnCurrencyFacility = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.CURRENCYID != company.CURRENCYID).Any();
                }

                typeIds = context.TBL_PRODUCT_COLLATERALTYPE.Where(x => productIds.Contains(x.PRODUCTID))
                   .Select(x => x.COLLATERALTYPEID)
                   .Distinct().ToList();

            }

            var collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false && x.CUSTOMERID == customerId)
                .GroupJoin(
                    context.TBL_LOAN_COLLATERAL_MAPPING,
                    c => c.COLLATERALCUSTOMERID,
                    lc => lc.COLLATERALCUSTOMERID,
                    (c, lc) => new { c, m = lc }
                )
                .SelectMany
                (
                    x => x.m.DefaultIfEmpty(),
                    (c, m) => new CollateralViewModel
                    {
                        collateralId = c.c.COLLATERALCUSTOMERID,
                        collateralTypeId = c.c.COLLATERALTYPEID,
                        collateralSubTypeId = c.c.COLLATERALSUBTYPEID,
                        customerId = c.c.CUSTOMERID,
                        customerCode = c.c.CUSTOMERCODE,
                        customerName = c.c.TBL_CUSTOMER.FIRSTNAME + c.c.TBL_CUSTOMER.MIDDLENAME + c.c.TBL_CUSTOMER.LASTNAME,
                        currencyId = c.c.CURRENCYID,
                        currencyCode = c.c.TBL_CURRENCY.CURRENCYCODE,
                        baseCurrencyId = company.CURRENCYID,
                        baseCurrencyCode = (c.c.CURRENCYID == baseCurrencyId) ? "" : company.TBL_CURRENCY.CURRENCYCODE,//so that only fcy will show
                        currency = c.c.TBL_CURRENCY.CURRENCYNAME,
                        disAllowCollateral = disAllowCollateral && c.c.CURRENCYID == company.CURRENCYID, // facilityCurrency != baseCurrency && collateralCurrency == baseCurrency
                        collateralTypeName = c.c.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                        collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(r => r.COLLATERALSUBTYPEID == c.c.COLLATERALSUBTYPEID).Select(q => q.COLLATERALSUBTYPENAME).FirstOrDefault(),
                        collateralCode = c.c.COLLATERALCODE,
                        collateralValue = c.c.COLLATERALVALUE,
                        camRefNumber = c.c.CAMREFNUMBER,
                        allowSharing = c.c.ALLOWSHARING,
                        isLocationBased = c.c.ISLOCATIONBASED ?? false,
                        valuationCycle = c.c.VALUATIONCYCLE,
                        haircut = c.c.HAIRCUT,
                        approvalStatusName = c.c.APPROVALSTATUS,
                        allowApplicationMapping = typeIds.Contains((short)c.c.COLLATERALTYPEID),
                        requireInsurancePolicy = c.c.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                        exchangeRate = c.c.EXCHANGERATE,
                        collateralReleaseStatusId = c.c.COLLATERALRELEASESTATUSID,
                        collateralReleaseStatusName = c.c.COLLATERALRELEASESTATUSID == null ? context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.InVault).FirstOrDefault().COLLATERALRELEASESTATUSNAME : context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == c.c.COLLATERALRELEASESTATUSID).FirstOrDefault().COLLATERALRELEASESTATUSNAME,
                        accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,
                        collateralUsageStatus = c.c.COLLATERALUSAGESTATUSID,
                        loanApplicationId = (applicationId != null && applicationId > 0) ? applicationId : (from a in context.TBL_LOAN_APPLICATION_COLLATERL join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID where b.LOANAPPLICATIONID == (int)c.c.LOANAPPLICATIONID select b.LOANAPPLICATIONID).FirstOrDefault(), 
                        collateralSummary = c.c.COLLATERALSUMMARY,
                        isMapped = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.COLLATERALCUSTOMERID == c.c.COLLATERALCUSTOMERID && o.DELETED == false).Any(),
                        isProposed = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == c.c.COLLATERALCUSTOMERID && o.DELETED == false).Any(),
                        facilityAmount = (from a in context.TBL_LOAN_APPLICATION_COLLATERL join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID where b.LOANAPPLICATIONID == (int)c.c.LOANAPPLICATIONID select b.APPROVEDAMOUNT).FirstOrDefault(),
                        customerAccount = context.TBL_CASA.Where(x => x.CUSTOMERID == c.c.CUSTOMERID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),

                        companyId = companyId,//remark = c.c.
                        validTill = c.c.VALIDTILL,
                    })
                    .ToList()
                    .GroupBy(x => x.collateralId).Select(g => g.First());

            collaterals = ResolveCollateralValues(collaterals.ToList(), company);
            return collaterals.OrderByDescending(x => x.collateralId);
        }

        public IEnumerable<OriginalDocumentSubmissionByFacilityViewModel> GetCustomerFacility(int customerId)
        {

            var application = (from c in context.TBL_LOAN_APPLICATION_DETAIL
                               join a in context.TBL_LOAN_APPLICATION on c.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                               where a.CUSTOMERID == customerId
                               && a.DELETED == false
                               && c.DELETED == false
                               && a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                               select new OriginalDocumentSubmissionByFacilityViewModel
                               {
                                   divisionCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == c.CUSTOMERID select p.BUSINESSUNITINITIALS).FirstOrDefault(),
                                   divisionShortCode = (from p in context.TBL_PROFILE_BUSINESS_UNIT join c in context.TBL_CUSTOMER on p.BUSINESSUNITID equals c.BUSINESSUNTID where c.CUSTOMERID == c.CUSTOMERID select p.BUSINESSUNITSHORTCODE).FirstOrDefault(),
                                   productClassId = a.PRODUCTCLASSID,
                                   productClassName = a.TBL_PRODUCT_CLASS.PRODUCTCLASSNAME,
                                   facility = context.TBL_PRODUCT.Where(p => p.PRODUCTID == c.PROPOSEDPRODUCTID).Select(p => p.PRODUCTNAME).FirstOrDefault(),
                                   systemDateTime = c.DATETIMECREATED,
                                   requireCollateral = a.REQUIRECOLLATERAL,
                                   approvalStatusId = (short)a.APPROVALSTATUSID,
                                   applicationStatusId = a.APPLICATIONSTATUSID,
                                   loanApplicationId = a.LOANAPPLICATIONID,
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                   customerId = a.CUSTOMERID ?? 0,
                                   customerName = a.CUSTOMERID.HasValue ? a.TBL_CUSTOMER.FIRSTNAME + " " + a.TBL_CUSTOMER.MIDDLENAME + " " + a.TBL_CUSTOMER.LASTNAME : "",
                                   loanInformation = a.LOANINFORMATION,
                                   companyId = a.COMPANYID,
                                   branchId = (short)a.BRANCHID,
                                   branchName = a.TBL_BRANCH.BRANCHNAME,
                                   relationshipOfficerId = a.RELATIONSHIPOFFICERID,
                                   relationshipOfficerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                   relationshipManagerId = a.RELATIONSHIPMANAGERID,
                                   relationshipManagerName = a.TBL_STAFF.FIRSTNAME + " " + a.TBL_STAFF.MIDDLENAME + " " + a.TBL_STAFF.LASTNAME,
                                   misCode = a.MISCODE,
                                   teamMisCode = a.TEAMMISCODE,
                                   interestRate = a.INTERESTRATE,
                                   isRelatedParty = a.ISRELATEDPARTY,
                                   isPoliticallyExposed = a.ISPOLITICALLYEXPOSED,
                                   submittedForAppraisal = a.SUBMITTEDFORAPPRAISAL,
                                   customerGroupId = a.CUSTOMERGROUPID ?? 0,
                                   customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                                   loanTypeId = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPEID,
                                   loanTypeName = a.TBL_LOAN_APPLICATION_TYPE.LOANAPPLICATIONTYPENAME,
                                   createdBy = a.OWNEDBY,
                                   applicationDate = a.APPLICATIONDATE,
                                   applicationTenor = a.APPLICATIONTENOR,
                                   applicationAmount = a.APPLICATIONAMOUNT,
                                   dateTimeCreated = a.DATETIMECREATED,
                                   collateralDetail = a.COLLATERALDETAIL,
                                   isEmployerRelated = a.ISEMPLOYERRELATED,
                                   employer = context.TBL_CUSTOMER_EMPLOYER.FirstOrDefault(e => e.EMPLOYERID == a.RELATEDEMPLOYERID).EMPLOYER_NAME,
                                   equityAmount = c.EQUITYAMOUNT,
                                   equityCasaAccountId = c.EQUITYCASAACCOUNTID,
                                   approvedAmount = c.APPROVEDAMOUNT,
                                   approvedInterestRate = c.APPROVEDINTERESTRATE,
                                   approvedProductId = c.APPROVEDPRODUCTID,
                                   approvedTenor = c.APPROVEDTENOR,
                                   currencyId = c.CURRENCYID,
                                   currencyName = c.TBL_CURRENCY.CURRENCYNAME,
                                   loanPurpose = c.LOANPURPOSE,
                                   exchangeRate = c.EXCHANGERATE,
                                   loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                                   subSectorId = c.SUBSECTORID,
                                   proposedAmount = c.PROPOSEDAMOUNT,
                                   proposedInterestRate = c.PROPOSEDINTERESTRATE,
                                   proposedProductId = c.PROPOSEDPRODUCTID,
                                   proposedProductName = c.TBL_PRODUCT.PRODUCTNAME,
                                   statusId = c.STATUSID,
                                   priceIndexId = c.PRODUCTPRICEINDEXID,
                                   priceIndexName = c.TBL_PRODUCT_PRICE_INDEX.PRICEINDEXNAME,
                                   fieldOne = c.FIELD1,
                                   fieldTwo = c.FIELD2,
                                   fieldThree = c.FIELD3,
                                   conditionPrecedent = c.CONDITIONPRECIDENT,
                                   conditionSubsequent = c.CONDITIONSUBSEQUENT,
                                   approvedProductName = c.TBL_PRODUCT.PRODUCTNAME,
                                   approvedRate = c.APPROVEDINTERESTRATE,
                                   //schedule = c.sc
                                   applicationId = a.LOANAPPLICATIONID,
                                   obligorName = c.TBL_CUSTOMER.FIRSTNAME + " " + c.TBL_CUSTOMER.MIDDLENAME + " " + c.TBL_CUSTOMER.LASTNAME,
                                   currencyCode = c.TBL_CURRENCY.CURRENCYCODE,
                                   proposedTenor = c.PROPOSEDTENOR,
                                   proposedRate = c.PROPOSEDINTERESTRATE,
                                   moratorium = c.MORATORIUM
                               }).ToList();



            return application;
        }



        public IEnumerable<CollateralViewModel> GetCustomerCashCollateral(int customerId, int? applicationId, int companyId, bool isLMS = false)
        {
            var typeIds = new List<int>();
            var company = context.TBL_COMPANY.Find(companyId);
            var baseCurrencyId = company.TBL_CURRENCY.CURRENCYID;
            bool disAllowCollateral = false;
            bool isForiegnCurrencyFacility = false;
            var productIds = new List<short>();
            var cashCollateralTypeIds = new List<int> { (int)CollateralTypeEnum.FixedDeposit, (int)CollateralTypeEnum.TreasuryBillsAndBonds, (int)CollateralTypeEnum.CASA };
            IEnumerable<CollateralViewModel> collaterals = null;

            if (applicationId != null && applicationId != 0)
            {
                if (isLMS)
                {
                    productIds = context.TBL_LMSR_APPLICATION_DETAIL
                    .Where(x => x.LOANAPPLICATIONID == applicationId)
                    .Select(x => x.PRODUCTID)
                    .Distinct().ToList();

                    isForiegnCurrencyFacility = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.CURRENCYID != company.CURRENCYID).Any();
                }
                else
                {
                    productIds = context.TBL_LOAN_APPLICATION_DETAIL
                    .Where(x => x.LOANAPPLICATIONID == applicationId)
                    .Select(x => x.APPROVEDPRODUCTID)
                    .Distinct().ToList();
                    isForiegnCurrencyFacility = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.CURRENCYID != company.CURRENCYID).Any();
                }

                typeIds = context.TBL_PRODUCT_COLLATERALTYPE.Where(x => productIds.Contains(x.PRODUCTID))
                  .Select(x => x.COLLATERALTYPEID)
                  .Distinct().ToList();


                collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false && x.CUSTOMERID == customerId && typeIds.Contains(x.COLLATERALTYPEID) && cashCollateralTypeIds.Contains(x.COLLATERALTYPEID))
                .GroupJoin(
                    context.TBL_LOAN_APPLICATION_COLLATERL,
                    c => c.COLLATERALCUSTOMERID,
                    lc => lc.COLLATERALCUSTOMERID,
                    (c, lc) => new { c, m = lc }
                )
                .SelectMany
                (
                    x => x.m.DefaultIfEmpty(),
                    (c, m) => new CollateralViewModel
                    {
                        collateralId = c.c.COLLATERALCUSTOMERID,
                        collateralTypeId = c.c.COLLATERALTYPEID,
                        collateralSubTypeId = c.c.COLLATERALSUBTYPEID,
                        customerId = c.c.CUSTOMERID,
                        customerCode = c.c.CUSTOMERCODE,
                        customerName = c.c.TBL_CUSTOMER.FIRSTNAME + c.c.TBL_CUSTOMER.MIDDLENAME + c.c.TBL_CUSTOMER.LASTNAME,
                        currencyId = c.c.CURRENCYID,
                        currencyCode = c.c.TBL_CURRENCY.CURRENCYCODE,
                        baseCurrencyId = company.CURRENCYID,
                        baseCurrencyCode = (c.c.CURRENCYID == baseCurrencyId) ? "" : company.TBL_CURRENCY.CURRENCYCODE,
                        currency = c.c.TBL_CURRENCY.CURRENCYNAME,
                        disAllowCollateral = disAllowCollateral && c.c.CURRENCYID == company.CURRENCYID,
                        collateralTypeName = c.c.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                        collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(r => r.COLLATERALSUBTYPEID == c.c.COLLATERALSUBTYPEID).Select(q => q.COLLATERALSUBTYPENAME).FirstOrDefault(),
                        collateralCode = c.c.COLLATERALCODE,
                        collateralValue = c.c.COLLATERALVALUE,
                        camRefNumber = c.c.CAMREFNUMBER,
                        allowSharing = c.c.ALLOWSHARING,
                        isLocationBased = c.c.ISLOCATIONBASED ?? false,
                        valuationCycle = c.c.VALUATIONCYCLE,
                        haircut = c.c.HAIRCUT,
                        approvalStatusName = c.c.APPROVALSTATUS,
                        allowApplicationMapping = typeIds.Contains((short)c.c.COLLATERALTYPEID),
                        requireInsurancePolicy = c.c.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                        exchangeRate = c.c.EXCHANGERATE,
                        collateralReleaseStatusId = c.c.COLLATERALRELEASESTATUSID,
                        collateralReleaseStatusName = c.c.COLLATERALRELEASESTATUSID == null ? context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.InVault).FirstOrDefault().COLLATERALRELEASESTATUSNAME : context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == c.c.COLLATERALRELEASESTATUSID).FirstOrDefault().COLLATERALRELEASESTATUSNAME,
                        accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,
                        collateralUsageStatus = c.c.COLLATERALUSAGESTATUSID,
                        loanApplicationId = applicationId,
                        collateralSummary = c.c.COLLATERALSUMMARY,
                        isMapped = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.COLLATERALCUSTOMERID == c.c.COLLATERALCUSTOMERID && o.DELETED == false).Any(),
                        isProposed = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == c.c.COLLATERALCUSTOMERID && o.DELETED == false).Any(),
                        companyId = companyId,
                        validTill = c.c.VALIDTILL,
                        customerAccount = context.TBL_CASA.Where(x => x.CUSTOMERID == c.c.CUSTOMERID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),

                    })
                    .ToList()
                    .GroupBy(x => x.collateralId).Select(g => g.First());

            }
            else
            {
                collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false && x.CUSTOMERID == customerId && cashCollateralTypeIds.Contains(x.COLLATERALTYPEID))
               .GroupJoin(
                   context.TBL_LOAN_APPLICATION_COLLATERL,
                   c => c.COLLATERALCUSTOMERID,
                   lc => lc.COLLATERALCUSTOMERID,
                   (c, lc) => new { c, m = lc }
               )
               .SelectMany
               (
                   x => x.m.DefaultIfEmpty(),
                   (c, m) => new CollateralViewModel
                   {
                       customerAccount = context.TBL_CASA.Where(x => x.CUSTOMERID == c.c.CUSTOMERID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                       collateralId = c.c.COLLATERALCUSTOMERID,
                       collateralTypeId = c.c.COLLATERALTYPEID,
                       collateralSubTypeId = c.c.COLLATERALSUBTYPEID,
                       customerId = c.c.CUSTOMERID,
                       customerCode = c.c.CUSTOMERCODE,
                       customerName = c.c.TBL_CUSTOMER.FIRSTNAME + c.c.TBL_CUSTOMER.MIDDLENAME + c.c.TBL_CUSTOMER.LASTNAME,
                       currencyId = c.c.CURRENCYID,
                       currencyCode = c.c.TBL_CURRENCY.CURRENCYCODE,
                       baseCurrencyId = company.CURRENCYID,
                       baseCurrencyCode = (c.c.CURRENCYID == baseCurrencyId) ? "" : company.TBL_CURRENCY.CURRENCYCODE,
                       currency = c.c.TBL_CURRENCY.CURRENCYNAME,
                       disAllowCollateral = disAllowCollateral && c.c.CURRENCYID == company.CURRENCYID,
                       collateralTypeName = c.c.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                       collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(r => r.COLLATERALSUBTYPEID == c.c.COLLATERALSUBTYPEID).Select(q => q.COLLATERALSUBTYPENAME).FirstOrDefault(),
                       collateralCode = c.c.COLLATERALCODE,
                       collateralValue = c.c.COLLATERALVALUE,
                       camRefNumber = c.c.CAMREFNUMBER,
                       allowSharing = c.c.ALLOWSHARING,
                       isLocationBased = c.c.ISLOCATIONBASED ?? false,
                       valuationCycle = c.c.VALUATIONCYCLE,
                       haircut = c.c.HAIRCUT,
                       approvalStatusName = c.c.APPROVALSTATUS,
                       allowApplicationMapping = typeIds.Contains((short)c.c.COLLATERALTYPEID),
                       requireInsurancePolicy = c.c.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                       exchangeRate = c.c.EXCHANGERATE,
                       collateralReleaseStatusId = c.c.COLLATERALRELEASESTATUSID,
                       collateralReleaseStatusName = c.c.COLLATERALRELEASESTATUSID == null ? context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.InVault).FirstOrDefault().COLLATERALRELEASESTATUSNAME : context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == c.c.COLLATERALRELEASESTATUSID).FirstOrDefault().COLLATERALRELEASESTATUSNAME,
                       accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,
                       collateralUsageStatus = c.c.COLLATERALUSAGESTATUSID,
                       loanApplicationId = applicationId,
                       collateralSummary = c.c.COLLATERALSUMMARY,
                       isMapped = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.COLLATERALCUSTOMERID == c.c.COLLATERALCUSTOMERID && o.DELETED == false).Any(),
                       isProposed = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == c.c.COLLATERALCUSTOMERID && o.DELETED == false).Any(),
                       companyId = companyId,
                       validTill = c.c.VALIDTILL,
                   })
                   .ToList()
                   .GroupBy(x => x.collateralId).Select(g => g.First());
            }


            collaterals = ResolveCollateralValues(collaterals.ToList(), company);
            foreach (var v in collaterals)
            {
                var detailIds = context.TBL_LOAN_APPLICATION_COLLATERL.Where(p => p.COLLATERALCUSTOMERID == v.collateralCustomerId && p.DELETED == false).Select(p => p.LOANAPPLICATIONDETAILID);
                var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => detailIds.Contains(d.LOANAPPLICATIONDETAILID)).ToList();
                v.facilityAmount = facilities.Sum(p => p.APPROVEDAMOUNT * (decimal)p.EXCHANGERATE);
            }
            return collaterals.OrderByDescending(x => x.collateralId);
        }


        public IEnumerable<CollateralCashReleaseViewModel> GetCustomerCashCollateralApplications(int id)
        {

            var collateralsA = (from x in context.TBL_LOAN_APPLICATION_COLLATERL
                                join b in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals b.COLLATERALCUSTOMERID
                                join c in context.TBL_APPLICATIONDETAIL_LIEN on x.LOANAPPLICATIONDETAILID equals c.APPLICATIONDETAILID
                                where
                               x.COLLATERALCUSTOMERID == id
                               && c.COLLATERALCUSTOMERID == id
                               && x.DELETED == false
                               && b.DELETED == false
                               && c.ISRELEASED == false

                                select new CollateralCashReleaseViewModel
                                {
                                    loanApplicationDetailId = (int)x.LOANAPPLICATIONDETAILID,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    loanTypeName = (from y in context.TBL_LOAN_APPLICATION_TYPE join p in context.TBL_LOAN_APPLICATION on y.LOANAPPLICATIONTYPEID equals p.LOANAPPLICATIONTYPEID where x.LOANAPPCOLLATERALID == x.LOANAPPCOLLATERALID select y.LOANAPPLICATIONTYPENAME).FirstOrDefault(),
                                    applicationReferenceNumber = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(x => x.APPLICATIONREFERENCENUMBER).FirstOrDefault(), //d.APPLICATIONREFERENCENUMBER,
                                    loanAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Select(x => x.APPROVEDAMOUNT).FirstOrDefault(), //c.PRINCIPALAMOUNT,
                                    lienAmount = c.AMOUNT,
                                    facility = (from p in context.TBL_PRODUCT join a in context.TBL_LOAN_APPLICATION_DETAIL on p.PRODUCTID equals a.APPROVEDPRODUCTID where a.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID select p.PRODUCTNAME).FirstOrDefault(),
                                    collateralTypeName = b.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                    collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(r => r.COLLATERALSUBTYPEID == b.COLLATERALSUBTYPEID).Select(q => q.COLLATERALSUBTYPENAME).FirstOrDefault(),
                                    collateralCode = b.COLLATERALCODE,
                                    collateralId = x.COLLATERALCUSTOMERID,
                                    collateralTypeId = b.COLLATERALTYPEID,
                                    collateralSubTypeId = b.COLLATERALSUBTYPEID,
                                    customerId = (int)x.CUSTOMERID,
                                    customerCode = b.CUSTOMERCODE,
                                    customerName = b.TBL_CUSTOMER.FIRSTNAME + b.TBL_CUSTOMER.MIDDLENAME + b.TBL_CUSTOMER.LASTNAME,
                                    currencyId = b.CURRENCYID,
                                    currencyCode = b.TBL_CURRENCY.CURRENCYCODE,
                                    currency = b.TBL_CURRENCY.CURRENCYNAME,
                                }).ToList();

            return collateralsA;
        }


        public IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateral(int? loanApplicationId, int currencyId, int companyId)
        {

            var list = new List<CollateralCoverageViewModel>();
            var currencies = context.TBL_CURRENCY.ToList();
            var baseCurrency = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId).CURRENCYID;
            var baseCurrencyCode = currencies.FirstOrDefault(cu => cu.CURRENCYID == baseCurrency).CURRENCYCODE;
            int coveragePercentage = 0;
            decimal collateralValue = 0;
            decimal facilityAmount = 0;
            decimal availableCollateralValue = 0;
            decimal expectedCollateralCoverage = 0;
            decimal coverageAlreadyAchieved = 0;
            decimal remainingCoverageToCover = 0;
            decimal actualCollateralCoverage = 0;
            decimal totalCoverage = 0;
            DateTime valuationDate;
            //  decimal sumOfMultipleCollateralValues = 0;

            var collaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATERL
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               join a in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals a.COLLATERALTYPEID
                               join s in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals s.COLLATERALSUBTYPEID
                              // join v in context.TBL_COLLATERAL_VALUATION on c.COLLATERALCUSTOMERID equals v.COLLATERALCUSTOMERID
                               where x.LOANAPPLICATIONID == loanApplicationId && x.DELETED == false
                               orderby x.LOANAPPCOLLATERALID
                               select new CollateralCoverageViewModel
                               {
                                   loanAppCollateralId = x.LOANAPPCOLLATERALID,
                                   collateralId = x.COLLATERALCUSTOMERID,
                                   collateralCode = c.COLLATERALCODE,
                                   collateralValue = c.COLLATERALVALUE,
                                   loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                   actualCollateralCoverage = x.COLLATERALCOVERAGE,
                                   collateralTypeId = a.COLLATERALTYPEID,
                                   collateralSubTypeId = (short)s.COLLATERALSUBTYPEID,
                                   collateralTypeName = a.COLLATERALTYPENAME,
                                   facilityAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Select(o => o.APPROVEDAMOUNT).Sum(),
                                   facilityCurrencyCodeFcy = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(o => o.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).TBL_CURRENCY.CURRENCYCODE,
                                   approvalStatusId = x.APPROVALSTATUSID,
                                   currencyId = c.CURRENCYID,
                                   customerId = (int)x.CUSTOMERID,
                                   collateralOwnerId = (int)c.CUSTOMERID,
                                   //valuationDate = v.DATETIMECREATED,// context.TBL_COLLATERAL_VALUATION.Where(v=>v.COLLATERALCUSTOMERID == c.COLLATERALCUSTOMERID).Select(v=>v.DATETIMECREATED).FirstOrDefault()
                               })?.ToList();


            if (collaterals == null) return new List<CollateralCoverageViewModel>();


            foreach (var collateral in collaterals)
            {
                var obligor = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == collateral.customerId);
                var obligorName = obligor.FIRSTNAME + " " + obligor.MIDDLENAME + " " + obligor.LASTNAME;
                var collateralOwner = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == collateral.collateralOwnerId);
                var collateralOwnerName = collateralOwner.FIRSTNAME + " " + collateralOwner.MIDDLENAME + " " + collateralOwner.LASTNAME;
                var data = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == collateral.collateralSubTypeId && o.CURRENCYID == collateral.currencyId).Select(o => o).FirstOrDefault();
                if (data == null) continue;
                var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(collateral.loanApplicationDetailId);
                var facilityExchangeRate = repo.GetExchangeRate(DateTime.Now, facility.CURRENCYID, facility.TBL_LOAN_APPLICATION.COMPANYID);
                var collateralExchangeRate = repo.GetExchangeRate(DateTime.Now, (short)collateral.currencyId, facility.TBL_LOAN_APPLICATION.COMPANYID);
                var exchangeRate = repo.GetExchangeRate(DateTime.Now, (short)collateral.currencyId, companyId);
                valuationDate = context.TBL_COLLATERAL_CUSTOMER.Where(v => v.COLLATERALCUSTOMERID == collateral.collateralId).Select(v => v.DATETIMECREATED).FirstOrDefault();
                var vlation = context.TBL_COLLATERAL_VALUATION.Where(v => v.COLLATERALCUSTOMERID == collateral.collateralId).FirstOrDefault();
                if(vlation != null)
                {
                    valuationDate = vlation.DATETIMECREATED;
                }
                coveragePercentage = data.COVERAGE;
                decimal coverage = decimal.Divide(data.COVERAGE, 100);
                collateralValue = collateral.collateralValue * (decimal)collateralExchangeRate.sellingRate;
                facilityAmount = collateral.facilityAmount * (decimal)facilityExchangeRate.sellingRate;
                //collateralValue = collateral.collateralValue;
                //facilityAmount = collateral.facilityAmount;

                var alreadyProposedFacilitiesForThisCollateral = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == collateral.collateralId && o.DELETED == false).Select(o => o).ToList();
                var alreadyProposedCollateralsForThisFacility = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId && o.DELETED == false).Select(o => o).ToList();

                availableCollateralValue = collateralValue;

                //if (alreadyProposedFacilitiesForThisCollateral.Count != 0)
                //{
                //    decimal facilityValue = 0;
                //    foreach (var facility in alreadyProposedFacilitiesForThisCollateral)
                //    {
                //        facilityValue = facilityValue + context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == facility.LOANAPPLICATIONID).Select(o => o.APPROVEDAMOUNT).FirstOrDefault();
                //    }
                //    availableCollateralValue = availableCollateralValue - decimal.Multiply(coverage, facilityValue);
                //}

                availableCollateralValue = availableCollateralValue - alreadyProposedFacilitiesForThisCollateral.Sum(p => p.COLLATERALCOVERAGE);
                expectedCollateralCoverage = decimal.Multiply(coverage, facilityAmount);
                actualCollateralCoverage = collateral.actualCollateralCoverage;
                //remainingCoverageToCover = expectedCollateralCoverage - coverageAlreadyAchieved;

                //if (availableCollateralValue > remainingCoverageToCover)
                //{
                //    actualCollateralCoverage = remainingCoverageToCover;
                //}
                //else
                //{

                //    actualCollateralCoverage = availableCollateralValue;

                //}
                //coverageAlreadyAchieved += actualCollateralCoverage;
                totalCoverage = ((alreadyProposedCollateralsForThisFacility.Sum(c => c.COLLATERALCOVERAGE)) / facilityAmount) * 100;
                var cov = new CollateralCoverageViewModel
                {
                    loanAppCollateralId = collateral.loanAppCollateralId,
                    collateralSummary = collateral.collateralSummary,
                    expectedCoveragePercentage = coveragePercentage,
                    actualCoveragePercentage = ((actualCollateralCoverage / facilityAmount) * 100),
                    loanApplicationId = facility.LOANAPPLICATIONID,
                    loanApplicationDetailId = collateral.loanApplicationDetailId,
                    collateralId = collateral.collateralId,
                    collateralTypeId = collateral.collateralTypeId,
                    collateralTypeName = collateral.collateralTypeName,
                    collateralSubTypeId = collateral.collateralSubTypeId,
                    collateralCode = collateral.collateralCode,
                    collateralValue = collateralValue,
                    currencyId = collateral.currencyId,
                    collateralCurrencyCode = context.TBL_CURRENCY.FirstOrDefault(c => c.CURRENCYID == collateral.currencyId).CURRENCYCODE,
                    facilityAmount = facilityAmount,
                    facilityAmountFcy = collateral.facilityAmount,
                    facilityCurrencyCodeFcy = collateral.facilityCurrencyCodeFcy,
                    expectedCollateralCoverage = expectedCollateralCoverage,
                    availableCollateralValue = availableCollateralValue,
                    availableCollateralValueBaseAmount = availableCollateralValue * (decimal)exchangeRate.sellingRate,
                    baseCurrencyCode = baseCurrencyCode,
                    totalCoverage = totalCoverage,
                    actualCollateralCoverage = actualCollateralCoverage,
                    approvalStatusId = collateral.approvalStatusId,
                    referenceNumber = facility.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                    productName = facility.TBL_PRODUCT.PRODUCTNAME,
                    customerId = collateral.customerId,
                    customerName = obligorName,
                    collateralOwnerName = collateralOwnerName,
                    valuationDate = valuationDate,
                };

                list.Add(cov);

            }
            return list.OrderByDescending(l => l.productName);
            //return list.GroupBy(x=>x.loanApplicationDetailId).Select(x => x.First());

        }

        public IEnumerable<CollateralCoverageViewModel> GetProposedFacilitiesToCollateralByCollateralId(int collateralId)
        {
            var collaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATERL
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               join d in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                               join l in context.TBL_LOAN_APPLICATION on d.LOANAPPLICATIONID equals l.LOANAPPLICATIONID
                               join cu in context.TBL_CUSTOMER on d.CUSTOMERID equals cu.CUSTOMERID
                               join p in context.TBL_PRODUCT on d.APPROVEDPRODUCTID equals p.PRODUCTID
                               join r in context.TBL_LOAN_BOOKING_REQUEST on d.LOANAPPLICATIONDETAILID equals r.LOANAPPLICATIONDETAILID into rr
                               from r in rr.DefaultIfEmpty()
                               join tl in context.TBL_LOAN on r.LOAN_BOOKING_REQUESTID equals tl.LOAN_BOOKING_REQUESTID into tlr
                               join cl in context.TBL_LOAN_CONTINGENT on r.LOAN_BOOKING_REQUESTID equals cl.LOAN_BOOKING_REQUESTID into clr
                               join rl in context.TBL_LOAN_REVOLVING on r.LOAN_BOOKING_REQUESTID equals rl.LOAN_BOOKING_REQUESTID into rlr
                               from tl in tlr.DefaultIfEmpty()
                               from cl in clr.DefaultIfEmpty()
                               from rl in rlr.DefaultIfEmpty()
                               
                                   //let isProperty = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Any(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID)
                               let isTermLoan = (tl != null)
                               let isContingent = (cl != null)
                               let isRevolving = (rl != null)
                               let noDrawDown = (!isTermLoan && !isContingent && !isRevolving)
                               where x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false

                               select new CollateralCoverageViewModel
                               {
                                   loanAppCollateralId = x.LOANAPPCOLLATERALID,
                                   collateralId = x.COLLATERALCUSTOMERID,
                                   collateralCode = c.COLLATERALCODE,
                                   currencyId = c.CURRENCYID,
                                   collateralValue = c.COLLATERALVALUE,
                                   actualCollateralCoverage = x.COLLATERALCOVERAGE,
                                   loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                   applicationReferenceNumber = l.APPLICATIONREFERENCENUMBER,
                                   customerName = cu.FIRSTNAME + " " + cu.MIDDLENAME + " " + cu.LASTNAME,
                                   requestedAmount = r.AMOUNT_REQUESTED,
                                   isBooked = r.ISUSED ?? false,
                                   isDisbursed = (noDrawDown) ? false : (isRevolving) ? rl.ISDISBURSED : (isContingent) ? cl.ISDISBURSED : tl.ISDISBURSED,
                                   disbursedAmount = (noDrawDown) ? 0 : (isRevolving) ? rl.OVERDRAFTLIMIT : (isContingent) ? cl.CONTINGENTAMOUNT : tl.PRINCIPALAMOUNT,
                                   outstandingPrincipal = (noDrawDown) ? 0 : (isRevolving) ? rl.OVERDRAFTLIMIT : (isContingent) ? cl.CONTINGENTAMOUNT : tl.OUTSTANDINGPRINCIPAL,
                                   bookingDate = (isRevolving) ? rl.BOOKINGDATE : (isContingent) ? cl.BOOKINGDATE : tl.BOOKINGDATE,
                                   maturityDate = (isRevolving) ? rl.MATURITYDATE : (isContingent) ? cl.MATURITYDATE : tl.MATURITYDATE,
                                   facilityAmount = d.APPROVEDAMOUNT,
                                   productName = p.PRODUCTNAME,
                                   baseCurrencyCode = d.TBL_CURRENCY.CURRENCYCODE,
                                   customerId = (int)x.CUSTOMERID,
                                   collateralOwnerId = (int)c.CUSTOMERID,
                                   //omv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).OPENMARKETVALUE ?? 0 : c.COLLATERALVALUE,
                                   //fsv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).FORCEDSALEVALUE ?? 0 : c.COLLATERALVALUE,
                                   facilityCurrencyId = d.CURRENCYID,
                               }).ToList();

            return collaterals;
        }

        public IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateralByCustomerId(int customerId, bool getAll = false)
        {

            var list = new List<CollateralCoverageViewModel>();
            var customerFacilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(f => f.DELETED == false && f.CUSTOMERID == customerId).ToList();
            var baseCurrencyCode = context.TBL_COMPANY.FirstOrDefault()?.TBL_CURRENCY.CURRENCYCODE;
            //foreach (var f in customerFacilities)
            //{
            int coveragePercentage = 0;
            decimal collateralValue = 0;
            decimal facilityAmount = 0;
            decimal availableCollateralValue = 0;
            decimal expectedCollateralCoverage = 0;
            decimal actualCollateralCoverage = 0;
            var collaterals = new List<CollateralCoverageViewModel>();

            //  decimal sumOfMultipleCollateralValues = 0;

            if (getAll)
            {
                collaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATERL
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               join a in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals a.COLLATERALTYPEID
                               join s in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals s.COLLATERALSUBTYPEID
                               join f in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals f.LOANAPPLICATIONDETAILID
                               let isProperty = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Any(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID)
                               //where x.LOANAPPLICATIONDETAILID == f.LOANAPPLICATIONDETAILID
                               where c.CUSTOMERID == customerId && x.DELETED == false//where the owner of the collateral is the focus

                               select new CollateralCoverageViewModel
                               {
                                   loanAppCollateralId = x.LOANAPPCOLLATERALID,
                                   collateralId = x.COLLATERALCUSTOMERID,
                                   collateralCode = c.COLLATERALCODE,
                                   currencyId = c.CURRENCYID,
                                   collateralValue = c.COLLATERALVALUE,
                                   actualCollateralCoverage = x.COLLATERALCOVERAGE,
                                   loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                   collateralSubTypeId = s.COLLATERALSUBTYPEID,
                                   approvalStatusId = x.APPROVALSTATUSID,
                                   collateralSummary = c.COLLATERALSUMMARY,
                                   facilityAmount = f.APPROVEDAMOUNT,
                                   customerId = (int)x.CUSTOMERID,
                                   collateralOwnerId = (int)c.CUSTOMERID,
                                   omv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).OPENMARKETVALUE ?? 0 : c.COLLATERALVALUE,
                                   fsv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).FORCEDSALEVALUE ?? 0 : c.COLLATERALVALUE,
                                   //facilityAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Sum(o => o.APPROVEDAMOUNT),
                                   facilityCurrencyId = f.CURRENCYID,
                               }).ToList();
            }
            else
            {
                collaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATERL
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               join a in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals a.COLLATERALTYPEID
                               join s in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals s.COLLATERALSUBTYPEID
                               join f in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals f.LOANAPPLICATIONDETAILID
                               let isProperty = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Any(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID)
                               //where x.LOANAPPLICATIONDETAILID == f.LOANAPPLICATIONDETAILID
                               where f.CUSTOMERID == customerId && x.DELETED == false//where the owner of the facility is the focus

                               select new CollateralCoverageViewModel
                               {
                                   loanAppCollateralId = x.LOANAPPCOLLATERALID,
                                   collateralId = x.COLLATERALCUSTOMERID,
                                   collateralCode = c.COLLATERALCODE,
                                   currencyId = c.CURRENCYID,
                                   collateralValue = c.COLLATERALVALUE,
                                   actualCollateralCoverage = x.COLLATERALCOVERAGE,
                                   loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                   collateralSubTypeId = s.COLLATERALSUBTYPEID,
                                   approvalStatusId = x.APPROVALSTATUSID,
                                   collateralSummary = c.COLLATERALSUMMARY,
                                   customerId = (int)x.CUSTOMERID,
                                   collateralOwnerId = (int)c.CUSTOMERID,
                                   omv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).OPENMARKETVALUE ?? 0 : c.COLLATERALVALUE,
                                   fsv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).FORCEDSALEVALUE ?? 0 : c.COLLATERALVALUE,
                                   facilityAmount = f.APPROVEDAMOUNT,
                                   //facilityAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Sum(o => o.APPROVEDAMOUNT),
                                   facilityCurrencyId = f.CURRENCYID,
                               }).ToList();
            }




            if (collaterals == null) return new List<CollateralCoverageViewModel>();


            foreach (var collateral in collaterals)
            {
                var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(collateral.loanApplicationDetailId);
                var facilityExchangeRate = repo.GetExchangeRate(DateTime.Now, facility.CURRENCYID, facility.TBL_LOAN_APPLICATION.COMPANYID);
                var collateralExchangeRate = repo.GetExchangeRate(DateTime.Now, (short)collateral.currencyId, facility.TBL_LOAN_APPLICATION.COMPANYID);
                var obligor = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == collateral.customerId);
                var obligorName = obligor.FIRSTNAME + " " + obligor.MIDDLENAME + " " + obligor.LASTNAME;
                var collateralOwner = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == collateral.collateralOwnerId);
                var collateralOwnerName = collateralOwner.FIRSTNAME + " " + collateralOwner.MIDDLENAME + " " + collateralOwner.LASTNAME;

                var data = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == collateral.collateralSubTypeId && o.CURRENCYID == collateral.currencyId).Select(o => o).FirstOrDefault();

                if (data == null) continue;

                coveragePercentage = data.COVERAGE;
                decimal coverage = decimal.Divide(data.COVERAGE, 100);
                collateralValue = collateral.collateralValue * (decimal)collateralExchangeRate.sellingRate;
                facilityAmount = collateral.facilityAmount * (decimal)facilityExchangeRate.sellingRate;
                //collateralValue = collateral.collateralValue;
                //facilityAmount = collateral.facilityAmount;

                var alreadyProposedFacilitiesForThisCollateral = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == collateral.collateralId && o.DELETED == false).Select(o => o).ToList();

                availableCollateralValue = collateralValue;

                //if (alreadyProposedFacilitiesForThisCollateral.Count != 0)
                //{
                //    decimal facilityValue = 0;
                //    foreach (var facility in alreadyProposedFacilitiesForThisCollateral)
                //    {
                //        facilityValue = facilityValue + context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONDETAILID == facility.LOANAPPLICATIONDETAILID).Select(o => o.APPROVEDAMOUNT).FirstOrDefault();
                //    }
                //    availableCollateralValue = availableCollateralValue - decimal.Multiply(coverage, facilityValue);
                //}

                expectedCollateralCoverage = decimal.Multiply(coverage, facilityAmount);
                var collateralValueUsed = alreadyProposedFacilitiesForThisCollateral.Sum(p => p.COLLATERALCOVERAGE);
                availableCollateralValue = availableCollateralValue - collateralValueUsed;
                expectedCollateralCoverage = decimal.Multiply(coverage, facilityAmount);
                actualCollateralCoverage = collateral.actualCollateralCoverage;
                //if (availableCollateralValue > expectedCollateralCoverage)
                //{
                //    actualCollateralCoverage = expectedCollateralCoverage;
                //}
                //else
                //{

                //    actualCollateralCoverage = availableCollateralValue;

                //}

                var cov = new CollateralCoverageViewModel
                {
                    loanAppCollateralId = collateral.loanAppCollateralId,
                    collateralSummary = collateral.collateralSummary,
                    loanApplicationDetailId = collateral.loanApplicationDetailId,
                    collateralId = collateral.collateralId,
                    collateralCode = collateral.collateralCode,
                    collateralValue = collateralValue,
                    collateralValueFcy = collateral.collateralValue,
                    currencyId = collateral.currencyId,
                    customerName = obligorName,
                    collateralOwnerName = collateralOwnerName,
                    actualCoveragePercentage = coveragePercentage,
                    facilityAmount = facilityAmount,
                    facilityAmountFcy = collateral.facilityAmount,
                    facilityCurrencyCodeFcy = facility.TBL_CURRENCY.CURRENCYCODE,
                    baseCurrencyCode = baseCurrencyCode,
                    expectedCollateralCoverage = expectedCollateralCoverage,
                    availableCollateralValue = availableCollateralValue,
                    actualCollateralCoverage = actualCollateralCoverage,
                    approvalStatusId = collateral.approvalStatusId,
                    omv = collateral.omv * (decimal)collateralExchangeRate.sellingRate,
                    fsv = collateral.fsv * (decimal)collateralExchangeRate.sellingRate,
                    //productAccNumber = context.
                    referenceNumber = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId).Select(x => x.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER).FirstOrDefault(),
                    productName = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId).Select(x => x.TBL_PRODUCT.PRODUCTNAME).FirstOrDefault(),
                    isUsed = context.TBL_LOAN_APPLICATION_COLLATERL.Any(p => p.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId && p.COLLATERALCUSTOMERID == collateral.collateralId && p.DELETED == false && p.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved)
                };

                list.Add(cov);
            }
            //}
            return list;

        }

        public IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateralByCustomerIdLMS(int customerId, bool getAll = false)
        {

            var list = new List<CollateralCoverageViewModel>();
            var customerFacilities = context.TBL_LMSR_APPLICATION_DETAIL.Where(f => f.DELETED == false && f.CUSTOMERID == customerId).ToList();
            var baseCurrencyCode = context.TBL_COMPANY.FirstOrDefault()?.TBL_CURRENCY.CURRENCYCODE;

            int coveragePercentage = 0;
            decimal collateralValue = 0;
            decimal facilityAmount = 0;
            decimal availableCollateralValue = 0;
            decimal expectedCollateralCoverage = 0;
            decimal actualCollateralCoverage = 0;
            var collaterals = new List<CollateralCoverageViewModel>();

            if (getAll)
            {
                collaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATERL
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               join a in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals a.COLLATERALTYPEID
                               join s in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals s.COLLATERALSUBTYPEID
                               join f in context.TBL_LMSR_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals f.LOANREVIEWAPPLICATIONID
                               let isProperty = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Any(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID)

                               where c.CUSTOMERID == customerId && x.DELETED == false

                               select new CollateralCoverageViewModel
                               {
                                   loanAppCollateralId = x.LOANAPPCOLLATERALID,
                                   collateralId = x.COLLATERALCUSTOMERID,
                                   collateralCode = c.COLLATERALCODE,
                                   collateralCurrencyCode = c.TBL_CURRENCY.CURRENCYCODE,
                                   currencyId = c.CURRENCYID,
                                   companyId = c.COMPANYID,
                                   collateralValue = c.COLLATERALVALUE,
                                   actualCollateralCoverage = x.COLLATERALCOVERAGE,
                                   loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                   collateralSubTypeId = s.COLLATERALSUBTYPEID,
                                   approvalStatusId = x.APPROVALSTATUSID,
                                   collateralSummary = c.COLLATERALSUMMARY,
                                   facilityAmount = f.APPROVEDAMOUNT,
                                   customerId = (int)x.CUSTOMERID,
                                   collateralOwnerId = (int)c.CUSTOMERID,
                                   omv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).OPENMARKETVALUE ?? 0 : c.COLLATERALVALUE,
                                   fsv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).FORCEDSALEVALUE ?? 0 : c.COLLATERALVALUE,
                                   facilityCurrencyId = c.CURRENCYID,
                               }).ToList();
            }
            else
            {
                collaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATERL
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               join a in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals a.COLLATERALTYPEID
                               join s in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals s.COLLATERALSUBTYPEID
                               join f in context.TBL_LMSR_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals f.LOANREVIEWAPPLICATIONID
                               let isProperty = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Any(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID)

                               where f.CUSTOMERID == customerId && x.DELETED == false

                               select new CollateralCoverageViewModel
                               {
                                   loanAppCollateralId = x.LOANAPPCOLLATERALID,
                                   collateralId = x.COLLATERALCUSTOMERID,
                                   collateralCode = c.COLLATERALCODE,
                                   currencyId = c.CURRENCYID,
                                   collateralValue = c.COLLATERALVALUE,
                                   actualCollateralCoverage = x.COLLATERALCOVERAGE,
                                   loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                   collateralSubTypeId = s.COLLATERALSUBTYPEID,
                                   approvalStatusId = x.APPROVALSTATUSID,
                                   collateralSummary = c.COLLATERALSUMMARY,
                                   customerId = (int)x.CUSTOMERID,
                                   collateralOwnerId = (int)c.CUSTOMERID,
                                   omv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).OPENMARKETVALUE ?? 0 : c.COLLATERALVALUE,
                                   fsv = (isProperty) ? context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).FORCEDSALEVALUE ?? 0 : c.COLLATERALVALUE,
                                   facilityAmount = f.APPROVEDAMOUNT,
                                   facilityCurrencyId = c.CURRENCYID,
                               }).ToList();
            }




            if (collaterals == null) return new List<CollateralCoverageViewModel>();


            foreach (var collateral in collaterals)
            {
                var facility = context.TBL_LMSR_APPLICATION_DETAIL.Find(collateral.loanApplicationDetailId);
                var facilityExchangeRate = repo.GetExchangeRate(DateTime.Now, (short)collateral.currencyId, collateral.companyId);
                var collateralExchangeRate = repo.GetExchangeRate(DateTime.Now, (short)collateral.currencyId, collateral.companyId);
                var obligor = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == collateral.customerId);
                var obligorName = obligor.FIRSTNAME + " " + obligor.MIDDLENAME + " " + obligor.LASTNAME;
                var collateralOwner = context.TBL_CUSTOMER.FirstOrDefault(c => c.CUSTOMERID == collateral.collateralOwnerId);
                var collateralOwnerName = collateralOwner.FIRSTNAME + " " + collateralOwner.MIDDLENAME + " " + collateralOwner.LASTNAME;

                var data = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == collateral.collateralSubTypeId && o.CURRENCYID == collateral.currencyId).Select(o => o).FirstOrDefault();

                if (data == null) continue;

                coveragePercentage = data.COVERAGE;
                decimal coverage = decimal.Divide(data.COVERAGE, 100);
                collateralValue = collateral.collateralValue * (decimal)collateralExchangeRate.sellingRate;
                facilityAmount = collateral.facilityAmount * (decimal)facilityExchangeRate.sellingRate;

                var alreadyProposedFacilitiesForThisCollateral = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == collateral.collateralId && o.DELETED == false).Select(o => o).ToList();

                availableCollateralValue = collateralValue;

                expectedCollateralCoverage = decimal.Multiply(coverage, facilityAmount);
                var collateralValueUsed = alreadyProposedFacilitiesForThisCollateral.Sum(p => p.COLLATERALCOVERAGE);
                availableCollateralValue = availableCollateralValue - collateralValueUsed;
                expectedCollateralCoverage = decimal.Multiply(coverage, facilityAmount);
                actualCollateralCoverage = collateral.actualCollateralCoverage;

                var cov = new CollateralCoverageViewModel
                {
                    loanAppCollateralId = collateral.loanAppCollateralId,
                    collateralSummary = collateral.collateralSummary,
                    loanApplicationDetailId = collateral.loanApplicationDetailId,
                    collateralId = collateral.collateralId,
                    collateralCode = collateral.collateralCode,
                    collateralValue = collateralValue,
                    collateralValueFcy = collateral.collateralValue,
                    currencyId = collateral.currencyId,
                    customerName = obligorName,
                    collateralOwnerName = collateralOwnerName,
                    actualCoveragePercentage = coveragePercentage,
                    facilityAmount = facilityAmount,
                    facilityAmountFcy = collateral.facilityAmount,
                    facilityCurrencyCodeFcy = collateral.collateralCurrencyCode,
                    baseCurrencyCode = baseCurrencyCode,
                    expectedCollateralCoverage = expectedCollateralCoverage,
                    availableCollateralValue = availableCollateralValue,
                    actualCollateralCoverage = actualCollateralCoverage,
                    approvalStatusId = collateral.approvalStatusId,
                    omv = collateral.omv * (decimal)collateralExchangeRate.sellingRate,
                    fsv = collateral.fsv * (decimal)collateralExchangeRate.sellingRate,
                    //productAccNumber = context.
                    referenceNumber = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId).Select(x => x.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER).FirstOrDefault(),
                    productName = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId).Select(x => x.TBL_PRODUCT.PRODUCTNAME).FirstOrDefault(),
                    isUsed = context.TBL_LOAN_APPLICATION_COLLATERL.Any(p => p.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId && p.COLLATERALCUSTOMERID == collateral.collateralId && p.DELETED == false && p.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved)
                };

                list.Add(cov);
            }
            //}
            return list;

        }

        public IEnumerable<CollateralCoverageViewModel> GetProposedCustomerCollateralByLoanApplicationDetailId(int loanApplicationDetailId)
        {

            var list = new List<CollateralCoverageViewModel>();
            var currencies = context.TBL_CURRENCY.ToList();
            var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);
            int companyId = facility.TBL_LOAN_APPLICATION.COMPANYID;
            var baseCurrency = context.TBL_COMPANY.FirstOrDefault(x => x.COMPANYID == companyId).CURRENCYID;
            var baseCurrencyCode = currencies.FirstOrDefault(cu => cu.CURRENCYID == baseCurrency).CURRENCYCODE;
            int coveragePercentage = 0;
            decimal collateralValue = 0;
            decimal facilityAmount = 0;
            decimal availableCollateralValue = 0;
            decimal expectedCollateralCoverage = 0;
            decimal coverageAlreadyAchieved = 0;
            decimal remainingCoverageToCover = 0;
            decimal actualCollateralCoverage = 0;
            decimal totalCoverage = 0;

            var collaterals = (from x in context.TBL_LOAN_APPLICATION_COLLATERL
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               join a in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals a.COLLATERALTYPEID
                               join s in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals s.COLLATERALSUBTYPEID
                               //where x.LOANAPPLICATIONDETAILID == loanApplicationDetailId && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing && x.DELETED == false
                               where x.LOANAPPLICATIONDETAILID == loanApplicationDetailId && x.DELETED == false
                               orderby x.LOANAPPCOLLATERALID
                               select new CollateralCoverageViewModel
                               {
                                   collateralId = x.COLLATERALCUSTOMERID,
                                   collateralCode = c.COLLATERALCODE,
                                   collateralValue = c.COLLATERALVALUE,
                                   loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                                   actualCollateralCoverage = x.COLLATERALCOVERAGE,
                                   collateralTypeId = a.COLLATERALTYPEID,
                                   collateralSubTypeId = (short)s.COLLATERALSUBTYPEID,
                                   collateralTypeName = a.COLLATERALTYPENAME,
                                   facilityAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Select(o => o.APPROVEDAMOUNT).Sum(),
                                   facilityCurrencyCodeFcy = context.TBL_LOAN_APPLICATION_DETAIL.FirstOrDefault(o => o.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).TBL_CURRENCY.CURRENCYCODE,
                                   approvalStatusId = x.APPROVALSTATUSID,
                                   currencyId = c.CURRENCYID,
                               }).ToList();


            if (collaterals == null) return new List<CollateralCoverageViewModel>();


            foreach (var collateral in collaterals)
            {


                var data = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == collateral.collateralSubTypeId && o.CURRENCYID == collateral.currencyId).Select(o => o).FirstOrDefault();
                if (data == null) continue;
                //var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(collateral.loanApplicationDetailId);
                var facilityExchangeRate = repo.GetExchangeRate(DateTime.Now, facility.CURRENCYID, facility.TBL_LOAN_APPLICATION.COMPANYID);
                var collateralExchangeRate = repo.GetExchangeRate(DateTime.Now, (short)collateral.currencyId, facility.TBL_LOAN_APPLICATION.COMPANYID);
                var exchangeRate = repo.GetExchangeRate(DateTime.Now, (short)collateral.currencyId, companyId);

                coveragePercentage = data.COVERAGE;
                decimal coverage = decimal.Divide(data.COVERAGE, 100);
                collateralValue = collateral.collateralValue * (decimal)collateralExchangeRate.sellingRate;
                facilityAmount = collateral.facilityAmount * (decimal)facilityExchangeRate.sellingRate;

                var alreadyProposedFacilitiesForThisCollateral = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == collateral.collateralId && o.DELETED == false).Select(o => o).ToList();
                var alreadyProposedCollateralsForThisFacility = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId && o.DELETED == false).Select(o => o).ToList();

                availableCollateralValue = collateralValue;

                availableCollateralValue = availableCollateralValue - alreadyProposedFacilitiesForThisCollateral.Sum(p => p.COLLATERALCOVERAGE);
                expectedCollateralCoverage = decimal.Multiply(coverage, facilityAmount);
                actualCollateralCoverage = collateral.actualCollateralCoverage;
                totalCoverage = ((alreadyProposedCollateralsForThisFacility.Sum(c => c.COLLATERALCOVERAGE)) / facilityAmount) * 100;
                var cov = new CollateralCoverageViewModel
                {
                    collateralSummary = collateral.collateralSummary,
                    expectedCoveragePercentage = coveragePercentage,
                    actualCoveragePercentage = ((actualCollateralCoverage / facilityAmount) * 100),
                    loanApplicationDetailId = collateral.loanApplicationDetailId,
                    collateralId = collateral.collateralId,
                    collateralTypeId = collateral.collateralTypeId,
                    collateralTypeName = collateral.collateralTypeName,
                    collateralSubTypeId = collateral.collateralSubTypeId,
                    collateralCode = collateral.collateralCode,
                    collateralValue = collateralValue,
                    currencyId = collateral.currencyId,
                    collateralCurrencyCode = context.TBL_CURRENCY.FirstOrDefault(c => c.CURRENCYID == collateral.currencyId).CURRENCYCODE,
                    facilityAmount = facilityAmount,
                    facilityAmountFcy = collateral.facilityAmount,
                    facilityCurrencyCodeFcy = collateral.facilityCurrencyCodeFcy,
                    expectedCollateralCoverage = expectedCollateralCoverage,
                    availableCollateralValue = availableCollateralValue,
                    availableCollateralValueBaseAmount = availableCollateralValue * (decimal)exchangeRate.sellingRate,
                    baseCurrencyCode = baseCurrencyCode,
                    totalCoverage = totalCoverage,
                    actualCollateralCoverage = actualCollateralCoverage,
                    approvalStatusId = collateral.approvalStatusId,
                    referenceNumber = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId).Select(x => x.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER).FirstOrDefault(),
                    productName = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == collateral.loanApplicationDetailId).Select(x => x.TBL_PRODUCT.PRODUCTNAME).FirstOrDefault(),
                };

                list.Add(cov);

            }
            return list.OrderByDescending(l => l.productName);
        }


        public IEnumerable<CollateralViewModel> GetCustomerCollateralReport(string searchParam, int companyId)
        {
            var typeIds = new List<int>();
            var company = context.TBL_COMPANY.Find(companyId);
            bool disAllowCollateral = false;
            bool isForiegnCurrencyFacility = false;
            searchParam = searchParam.Trim();
            var collaterals = (
                               // from a in context.TBL_LOAN_COLLATERAL_MAPPING
                               //join l in context.TBL_LOAN on a.LOANID equals l.TERMLOANID
                               //join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                               //join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                               from d in context.TBL_COLLATERAL_CUSTOMER
                               join cus in context.TBL_CUSTOMER on d.CUSTOMERID equals cus.CUSTOMERID into cusd
                               from cus in cusd.DefaultIfEmpty()
                               where (d.CUSTOMERCODE.Contains(searchParam)
                               || cus.FIRSTNAME.ToLower().Contains(searchParam.ToLower())
                               || cus.LASTNAME.ToLower().Contains(searchParam.ToLower())
                               || cus.MIDDLENAME.ToLower().Contains(searchParam.ToLower()))
                               select new CollateralViewModel
                               {
                                   collateralId = d.COLLATERALCUSTOMERID,
                                   collateralTypeId = d.COLLATERALTYPEID,
                                   collateralSubTypeId = d.COLLATERALSUBTYPEID,
                                   customerId = d.CUSTOMERID.Value,
                                   currencyId = d.CURRENCYID,

                                   baseCurrencyId = company.CURRENCYID,
                                   currency = d.TBL_CURRENCY.CURRENCYNAME,            // c.c.TBL_CURRENCY.CURRENCYNAME,
                                   disAllowCollateral = disAllowCollateral && d.CURRENCYID == company.CURRENCYID, // facilityCurrency != baseCurrency && collateralCurrency == baseCurrency
                                   collateralTypeName = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                   collateralSubTypeName = "not implimented",
                                   collateralCode = d.COLLATERALCODE,
                                   collateralValue = d.COLLATERALVALUE,
                                   camRefNumber = d.CAMREFNUMBER,
                                   allowSharing = d.ALLOWSHARING,
                                   isLocationBased = d.ISLOCATIONBASED.HasValue ? (bool)d.ISLOCATIONBASED : d.ISLOCATIONBASED,
                                   valuationCycle = d.VALUATIONCYCLE,
                                   haircut = d.HAIRCUT,
                                   approvalStatusName = d.APPROVALSTATUS,
                                   allowApplicationMapping = typeIds.Contains((short)d.COLLATERALTYPEID),
                                   requireInsurancePolicy = d.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                                   exchangeRate = d.EXCHANGERATE,
                                   collateralSummary = d.COLLATERALSUMMARY,
                                   availableCollateralValue = 0,
                                   // accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,



                               }).ToList().GroupBy(x => x.collateralId).Select(g => g.First()).ToList();


            //var collaterals = (from a in context.TBL_LOAN_COLLATERAL_MAPPING
            //                   join l in context.TBL_LOAN on a.LOANID equals l.TERMLOANID
            //                   join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
            //                   join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
            //                   join d in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals d.COLLATERALCUSTOMERID
            //                   join cus in context.TBL_CUSTOMER on d.CUSTOMERID equals cus.CUSTOMERID
            //                   where (cus.CUSTOMERCODE == searchParam
            //                   || cus.FIRSTNAME.StartsWith(searchParam.ToUpper())
            //                   || cus.MIDDLENAME.StartsWith(searchParam.ToUpper())
            //                   || cus.LASTNAME.StartsWith(searchParam.ToUpper())
            //                   || l.LOANREFERENCENUMBER == searchParam)


            //                   select new CollateralViewModel
            //                   {
            //                       collateralId = a.COLLATERALCUSTOMERID,
            //                       collateralTypeId = d.COLLATERALTYPEID,
            //                       collateralSubTypeId = d.COLLATERALSUBTYPEID,
            //                       customerId = d.CUSTOMERID.Value,
            //                       currencyId = d.CURRENCYID,

            //                       baseCurrencyId = company.CURRENCYID,
            //                       currency = d.TBL_CURRENCY.CURRENCYNAME,            // c.c.TBL_CURRENCY.CURRENCYNAME,
            //                       disAllowCollateral = disAllowCollateral && d.CURRENCYID == company.CURRENCYID, // facilityCurrency != baseCurrency && collateralCurrency == baseCurrency
            //                       collateralTypeName = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
            //                       collateralSubTypeName = "not implimented",
            //                       collateralCode = d.COLLATERALCODE,
            //                       collateralValue = d.COLLATERALVALUE,
            //                       camRefNumber = d.CAMREFNUMBER,
            //                       allowSharing = d.ALLOWSHARING,
            //                       isLocationBased = (bool)d.ISLOCATIONBASED,
            //                       valuationCycle = d.VALUATIONCYCLE,
            //                       haircut = d.HAIRCUT,
            //                       approvalStatusName = d.APPROVALSTATUS,
            //                       allowApplicationMapping = typeIds.Contains((short)d.COLLATERALTYPEID),
            //                       requireInsurancePolicy = d.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
            //                       exchangeRate = d.EXCHANGERATE,
            //                       collateralSummary = d.COLLATERALSUMMARY,
            //                       availableCollateralValue = 0,
            //                       // accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,



            //                   }).ToList().GroupBy(x => x.collateralId).Select(g => g.First());

            //collaterals = ResolveCollateralValues(collaterals.ToList()); ;


            //var collaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => (x.DELETED == false && x.TBL_CUSTOMER.CUSTOMERCODE == searchParam) || (x.DELETED == false && x.TBL_CUSTOMER.FIRSTNAME == searchParam))
            //    .GroupJoin(
            //        context.TBL_LOAN_COLLATERAL_MAPPING,
            //        c => c.COLLATERALCUSTOMERID,
            //        lc => lc.COLLATERALCUSTOMERID,
            //        (c, lc) => new { c, m = lc }
            //    )
            //    .SelectMany
            //    (
            //        x => x.m.DefaultIfEmpty(),
            //        (c, m) => new CollateralViewModel
            //        {
            //            collateralId = c.c.COLLATERALCUSTOMERID,
            //            collateralTypeId = c.c.COLLATERALTYPEID,
            //            collateralSubTypeId = c.c.COLLATERALSUBTYPEID,
            //            customerId = c.c.CUSTOMERID,
            //            currencyId = c.c.CURRENCYID,
            //            baseCurrencyId = company.CURRENCYID,
            //            currency = c.c.TBL_CURRENCY.CURRENCYNAME,
            //            disAllowCollateral = disAllowCollateral && c.c.CURRENCYID == company.CURRENCYID, // facilityCurrency != baseCurrency && collateralCurrency == baseCurrency
            //            collateralTypeName = c.c.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
            //            collateralSubTypeName = "not implimented",
            //            collateralCode = c.c.COLLATERALCODE,
            //            collateralValue = c.c.COLLATERALVALUE,
            //            camRefNumber = c.c.CAMREFNUMBER,
            //            allowSharing = c.c.ALLOWSHARING,
            //            isLocationBased = (bool)c.c.ISLOCATIONBASED,
            //            valuationCycle = c.c.VALUATIONCYCLE,
            //            haircut = c.c.HAIRCUT,
            //            approvalStatus = c.c.APPROVALSTATUS,
            //            allowApplicationMapping = typeIds.Contains((short)c.c.COLLATERALTYPEID),
            //            requireInsurancePolicy = c.c.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
            //            exchangeRate = c.c.EXCHANGERATE,
            //            availableValue = 0,
            //           // accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,
            //        })
            //        .ToList()
            //        .GroupBy(x => x.collateralId).Select(g => g.First());

            //collaterals = ResolveCollateralValues(collaterals.ToList());

            //var count = collaterals.Count();
            //var test = collaterals;

            return collaterals;
        }

        public IEnumerable<CollateralViewModel> GetCustomerFixedDepositCollateral(string searchParam, int companyId)
        {
            var typeIds = new List<int>();
            var company = context.TBL_COMPANY.Find(companyId);
            bool disAllowCollateral = false;
            //bool isForiegnCurrencyFacility = false;
            searchParam = searchParam.Trim();

            var collaterals = (from d in context.TBL_COLLATERAL_CUSTOMER
                               join e in context.TBL_COLLATERAL_DEPOSIT on d.COLLATERALCUSTOMERID equals e.COLLATERALCUSTOMERID
                               join cus in context.TBL_CUSTOMER on d.CUSTOMERID equals cus.CUSTOMERID
                               where (d.CUSTOMERCODE.Contains(searchParam) || cus.FIRSTNAME.ToLower().Contains(searchParam.ToLower())
                               || cus.LASTNAME.ToLower().Contains(searchParam.ToLower()) || cus.MIDDLENAME.ToLower().Contains(searchParam.ToLower()))
                               && d.COLLATERALTYPEID == (int)CollateralTypeEnum.FixedDeposit
                               select new CollateralViewModel
                               {
                                   collateralId = d.COLLATERALCUSTOMERID,
                                   collateralTypeId = d.COLLATERALTYPEID,
                                   collateralSubTypeId = d.COLLATERALSUBTYPEID,
                                   customerId = d.CUSTOMERID.Value,
                                   customerCode = cus.CUSTOMERCODE,
                                   customerName = cus.FIRSTNAME + " " + cus.LASTNAME,
                                   accountNumber = e.ACCOUNTNUMBER,
                                   currencyId = d.CURRENCYID,
                                   baseCurrencyId = company.CURRENCYID,
                                   currency = d.TBL_CURRENCY.CURRENCYNAME,            // c.c.TBL_CURRENCY.CURRENCYNAME,
                                   disAllowCollateral = disAllowCollateral && d.CURRENCYID == company.CURRENCYID, // facilityCurrency != baseCurrency && collateralCurrency == baseCurrency
                                   collateralTypeName = d.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                   collateralSubTypeName = "not implimented",
                                   collateralCode = d.COLLATERALCODE,
                                   collateralValue = d.COLLATERALVALUE,
                                   camRefNumber = d.CAMREFNUMBER,
                                   allowSharing = d.ALLOWSHARING,
                                   isLocationBased = d.ISLOCATIONBASED.HasValue ? (bool)d.ISLOCATIONBASED : d.ISLOCATIONBASED,
                                   valuationCycle = d.VALUATIONCYCLE,
                                   haircut = d.HAIRCUT,
                                   approvalStatusName = d.APPROVALSTATUS,
                                   allowApplicationMapping = typeIds.Contains((short)d.COLLATERALTYPEID),
                                   requireInsurancePolicy = d.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                                   exchangeRate = d.EXCHANGERATE,
                                   collateralSummary = d.COLLATERALSUMMARY,
                                   availableCollateralValue = 0,
                                   // accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,
                               }).ToList().GroupBy(x => x.collateralId).Select(g => g.First()).ToList();

            return collaterals;
        }

        private List<CollateralViewModel> ResolveCollateralValues(List<CollateralViewModel> collaterals, TBL_COMPANY company)
        {
            var baseCurrencyId = company.TBL_CURRENCY.CURRENCYID;
            decimal usage;
            List<CollateralViewModel> list = new List<CollateralViewModel>();
            foreach (var collateral in collaterals)
            {
                usage = 0;
                var proposes = context.TBL_LOAN_APPLICATION_COLLATERL.Where(pc => pc.DELETED == false && pc.COLLATERALCUSTOMERID == collateral.collateralId).ToList();
                usage = proposes.Sum(p => p.COLLATERALCOVERAGE);
                var exchangeRate = repo.GetExchangeRate(DateTime.Now, (short)collateral.currencyId, collateral.companyId);
                collateral.collateralValueLcy = (collateral.currencyId == baseCurrencyId) ? 0 : (decimal)collateral.collateralValue * (decimal)exchangeRate.sellingRate;
                collateral.availableCollateralValue = (collateral.currencyId == baseCurrencyId) ? ((decimal)collateral.collateralValue - usage) : (decimal)collateral.collateralValueLcy - usage;
                list.Add(collateral);
            }
            return list;
        }

        private List<CollateralViewModel> ResolveCollateralOutstandingValues(List<CollateralViewModel> collaterals)
        {
            decimal usage;
            List<CollateralViewModel> list = new List<CollateralViewModel>();
            foreach (var collateral in collaterals)
            {
                usage = 0;
                var mappings = context.TBL_LOAN_COLLATERAL_MAPPING.Where(m => m.COLLATERALCUSTOMERID == collateral.collateralId && m.DELETED == false && m.ISRELEASED == false).ToList();
                foreach (var mapping in mappings) { usage = usage + GetLoanOutstandingBalance(mapping.LOANID, mapping.LOANSYSTEMTYPEID); }
                collateral.availableCollateralValue = (decimal)collateral.collateralValue - usage;
                list.Add(collateral);
            }
            return list;
        }

        private decimal GetLoanOutstandingBalance(int loanId, int loanSystemTypeId)
        {
            decimal balance = 0;
            switch (loanSystemTypeId)
            {
                case (int)LoanSystemTypeEnum.TermDisbursedFacility:
                    balance = context.TBL_LOAN.Where(x => x.TERMLOANID == loanId)?.Sum(x => x.OUTSTANDINGPRINCIPAL) ?? 0;
                    break;
                case (int)LoanSystemTypeEnum.OverdraftFacility:
                    balance = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId)?.Sum(x => x.OVERDRAFTLIMIT) ?? 0;
                    break;
                case (int)LoanSystemTypeEnum.ContingentLiability:
                    balance = context.TBL_LOAN_CONTINGENT.Where(x => x.CONTINGENTLOANID == loanId)?.Sum(x => x.CONTINGENTAMOUNT) ?? 0;
                    break;
            }
            return balance;
        }

        public IEnumerable<CollateralViewModel> GetCollateralByCollateralTypeIdByCustomerId(int companyId, short collateralTypeId, int customerId, int thirdpartyCustomerId)
        {
            return GetCustomerCollateral(companyId).Where(x => x.collateralTypeId == collateralTypeId && (x.customerId == customerId || x.customerId == thirdpartyCustomerId));
        }

        public IEnumerable<CollateralViewModel> GetCustomerCollateral(int companyId)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false
                && x.COMPANYID == companyId
            )
            .Select(x => new CollateralViewModel
            {
                collateralId = x.COLLATERALCUSTOMERID,
                collateralTypeId = x.COLLATERALTYPEID,
                collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                collateralSubTypeId = x.COLLATERALSUBTYPEID,
                customerId = x.CUSTOMERID.Value,
                currencyId = x.CURRENCYID,
                currency = x.TBL_CURRENCY.CURRENCYNAME,
                currencyCode = x.TBL_CURRENCY.CURRENCYCODE,
                collateralCode = x.COLLATERALCODE,
                collateralValue = x.COLLATERALVALUE,
                camRefNumber = x.CAMREFNUMBER,
                allowSharing = x.ALLOWSHARING,
                isLocationBased = (bool)x.ISLOCATIONBASED,
                valuationCycle = x.VALUATIONCYCLE,
                haircut = x.HAIRCUT,
                approvalStatusName = x.APPROVALSTATUS,
                //collateralValue = x.CollateralValue
                exchangeRate = x.EXCHANGERATE,
                collateralSummary = x.COLLATERALSUMMARY

            })
            .OrderByDescending(x => x.collateralId)

            .ToList();



            return collateral;
        }

        public CollateralViewModel GetCustomerCollateralByCustomerCollateralId(int customerCollateralId)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false && x.COLLATERALCUSTOMERID == customerCollateralId)
            .Select(x => new CollateralViewModel
            {
                collateralId = x.COLLATERALCUSTOMERID,
                collateralTypeId = x.COLLATERALTYPEID,
                collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                collateralSubTypeId = x.COLLATERALSUBTYPEID,
                customerId = x.CUSTOMERID.Value,
                currencyId = x.CURRENCYID,
                currency = x.TBL_CURRENCY.CURRENCYNAME,
                currencyCode = x.TBL_CURRENCY.CURRENCYCODE,
                collateralCode = x.COLLATERALCODE,
                collateralValue = x.COLLATERALVALUE,
                customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                camRefNumber = x.CAMREFNUMBER,
                allowSharing = x.ALLOWSHARING,
                isLocationBased = (bool)x.ISLOCATIONBASED,
                valuationCycle = x.VALUATIONCYCLE,
                haircut = x.HAIRCUT,
                approvalStatusName = x.APPROVALSTATUS,
                exchangeRate = x.EXCHANGERATE,
                collateralSummary = x.COLLATERALSUMMARY

            }).FirstOrDefault();

            var company = context.TBL_COMPANY.FirstOrDefault();

            var baseCurrencyId = company.TBL_CURRENCY.CURRENCYID;
            decimal usage;
            usage = 0;
            var proposes = context.TBL_LOAN_APPLICATION_COLLATERL.Where(pc => pc.DELETED == false && pc.COLLATERALCUSTOMERID == collateral.collateralId).ToList();
            usage = proposes.Sum(p => p.COLLATERALCOVERAGE);
            var exchangeRate = repo.GetExchangeRate(DateTime.Now, collateral.currencyId, company.COMPANYID);
            collateral.baseCurrencyCode = context.TBL_CURRENCY.Find(baseCurrencyId).CURRENCYCODE;
            collateral.collateralValueLcy = (collateral.currencyId == baseCurrencyId) ? (decimal)collateral.collateralValue : (decimal)collateral.collateralValue * (decimal)exchangeRate.sellingRate;
            collateral.availableCollateralValue = (collateral.currencyId == baseCurrencyId) ? ((decimal)collateral.collateralValue - usage) : (decimal)collateral.collateralValueLcy - usage;

            return collateral;
        }

        // GET TYPE SPICIFIC & INSURANCE DETAILS

        public CollateralViewModel GetCollateralTypeByCollateralId(int collateralId, int typeId)
        {

            var data = new CollateralViewModel();

            //     data =   GetCustomerCollateralByCollateralId(collateralId);
            try
            {
                switch (typeId)
                {
                    case (int)CollateralTypeEnum.FixedDeposit: data = GetCollateralDeposit(collateralId); break;
                    case (int)CollateralTypeEnum.PlantAndMachinery: data = GetCollateralMachinery(collateralId); break;
                    case (int)CollateralTypeEnum.Miscellaneous: data = GetCollateralMiscellaneous(collateralId); break;
                    case (int)CollateralTypeEnum.Gaurantee: data = GetCollateralGuarantee(collateralId); break;
                    case (int)CollateralTypeEnum.CASA: data = GetCollateralCasa(collateralId); break;
                    case (int)CollateralTypeEnum.Property: data = GetCollateralImmovableProperty(collateralId); break;
                    case (int)CollateralTypeEnum.TreasuryBillsAndBonds: data = GetCollateralMarketableSecurities(collateralId); break;
                    case (int)CollateralTypeEnum.InsurancePolicy: data = GetCollateralPolicy(collateralId); break;
                    case (int)CollateralTypeEnum.PreciousMetal: data = GetCollateralPreciousMetal(collateralId); break;
                    case (int)CollateralTypeEnum.MarketableSecurities_Shares: data = GetCollateralStock(collateralId); break;
                    case (int)CollateralTypeEnum.Vehicle: data = GetCollateralVehicle(collateralId); break;
                    case (int)CollateralTypeEnum.Promissory: data = GetCollateralPromissory(collateralId); break;
                    case (int)CollateralTypeEnum.ISPO: data = GetISPOCollateral(collateralId); break;
                    case (int)CollateralTypeEnum.DomiciliationContract: data = GetContractDomiciliationCollateral(collateralId); break;
                    case (int)CollateralTypeEnum.DomiciliationSalary: data = GetContractDomiciliationSalary(collateralId); break;
                    case (int)CollateralTypeEnum.Indemity: data = GetIndemityCollateral(collateralId); break;
                    default:
                        break;
                }
            }
            catch (Exception ex) { }

            return data;
        }
        private CollateralViewModel GetCollateralMiscellaneous(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_MISCELLANEOUS.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                detailId = specifics.COLLATERALMISCELLANEOUSID,
                securityName = specifics.NAMEOFSECURITY,
                securityValue = specifics.SECURITYVALUE,
            };
            details = GetMiscellaneousNotes(details);
            details = GetCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetMiscellaneousNotes(CollateralViewModel details)
        {
            var notes = context.TBL_COLLATERAL_MISC_NOTES.Where(x => x.MISCELLANEOUSID == details.detailId);
            var list = new List<MiscellaneousNote>();
            foreach (var note in notes)
            {
                list.Add(new MiscellaneousNote
                {
                    labelName = note.COLUMNNAME,
                    labelValue = note.COLUMNVALUE,
                    controlName = note.COLUMNNAME,
                });
            }
            details.notes = list;
            return details;
        }
        private CollateralViewModel GetCollateralMachinery(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_PLANT_AND_EQUIP.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                machineName = specifics.MACHINENAME,
                description = specifics.DESCRIPTION,
                machineNumber = specifics.MACHINENUMBER,
                manufacturerName = specifics.MANUFACTURERNAME,
                yearOfManufacture = specifics.YEAROFMANUFACTURE,
                yearOfPurchase = specifics.YEAROFPURCHASE,
                valueBaseTypeId = specifics.VALUEBASETYPEID,
                valueBaseTypeName = context.TBL_COLLATERAL_VALUEBASE_TYPE.Where(t => t.COLLATERALVALUEBASETYPEID == specifics.VALUEBASETYPEID).Select(q => q.VALUEBASETYPENAME).FirstOrDefault(),

                machineCondition = specifics.MACHINECONDITION,
                machineryLocation = specifics.MACHINERYLOCATION,
                replacementValue = specifics.REPLACEMENTVALUE,
                equipmentSize = specifics.EQUIPMENTSIZE,
                intendedUse = specifics.INTENDEDUSE,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetCollateralDeposit(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_DEPOSIT.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            CasaBalanceViewModel acc = repo.GetCASABalance(specifics.ACCOUNTNUMBER, specifics.TBL_COLLATERAL_CUSTOMER.COMPANYID);
            if (specifics == null)
            {
                return null;
            }
            specifics.AVAILABLEBALANCE = acc.availableBalance;
            specifics.ACCOUNTNAME = acc.accountName;
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                collateralDepositId = specifics.COLLATERALDEPOSITID,
                dealReferenceNumber = specifics.DEALREFERENCENUMBER,
                accountNumber = specifics.ACCOUNTNUMBER,
                existingLienAmount = specifics.EXISTINGLIENAMOUNT,
                lienAmount = specifics.LIENAMOUNT,
                availableBalance = acc.availableBalance,
                securityValue = specifics.SECURITYVALUE,
                maturityDate = specifics.MATURITYDATE,
                effectiveDate = specifics.EFFECTIVEDATE,
                maturityAmount = specifics.MATURITYAMOUNT,
                remark = specifics.REMARK,
                accountName = acc.accountName,
                bank = specifics.BANK,
                baseCurrencyCode = specifics.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,
            };
            details = GetCollateralInsurancePolicy(details);
            var saved = context.SaveChanges() != 0;
            return details;
        }
        private CollateralViewModel GetCollateralInsurancePolicy(CollateralViewModel details)
        {


            /*var insurance = context.TBL_COLLATERAL_ITEM_POLICY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == details.collateralId && x.DELETED == false && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved);
            if (insurance != null)
            {
                details.insurancePolicy = new InsurancePolicy();
                details.insurancePolicy.referenceNumber = insurance.POLICYREFERENCENUMBER;
                details.insurancePolicy.insuranceCompanyId = insurance.INSURANCECOMPANYID;
                details.insurancePolicy.sumInsured = insurance.SUMINSURED;
                details.insurancePolicy.startDate = insurance.STARTDATE;
                details.insurancePolicy.expiryDate = insurance.ENDDATE;
                details.insurancePolicy.insuranceTypeId = insurance.INSURANCETYPEID;
                details.insurancePolicy.policyId = insurance.POLICYID;
                details.insurancePolicy.inSurPremiumAmount = insurance.PREMIUMAMOUNT;
                details.insurancePolicy.description = insurance.DESCRIPTION;
                details.insurancePolicy.premiumPercent = insurance.PREMIUMPERCENT;
                details.insurancePolicy.approvalStatusId = insurance.APPROVALSTATUSID;
                details.insurancePolicy.differInsurancePolicy = insurance.DIFFERPOLICY;
                details.insurancePolicy.companyAddress = insurance.COMPANYADDRESS;

            }*/
            return details;

        }

        public List<InsurancePolicy> GetCollateralInsurancePolicies(int collateralId)
        {
            var insurance = context.TBL_COLLATERAL_ITEM_POLICY.Where(x => x.COLLATERALCUSTOMERID == collateralId
                                                                        && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                                                                        || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                                                        || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                                                                        || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing
                                                                        && x.DELETED == false)
                .Select(i => new InsurancePolicy
                {
                    referenceNumber = i.POLICYREFERENCENUMBER,
                    insuranceCompanyId = i.INSURANCECOMPANYID,
                    insuranceCompany = context.TBL_INSURANCE_COMPANY.Where(o => o.INSURANCECOMPANYID == i.INSURANCECOMPANYID).Select(o => o.COMPANYNAME).FirstOrDefault(),
                    sumInsured = i.SUMINSURED,
                    startDate = i.STARTDATE,
                    expiryDate = i.ENDDATE,
                    insuranceTypeId = i.INSURANCETYPEID,
                    hasExpired = i.HASEXPIRED,
                    policyId = i.POLICYID,
                    inSurPremiumAmount = i.PREMIUMAMOUNT,
                    description = i.DESCRIPTION,
                    premiumPercent = i.PREMIUMPERCENT,
                    differInsurancePolicy = i.DIFFERPOLICY,
                    companyAddress = i.COMPANYADDRESS,
                    insuranceType = context.TBL_INSURANCE_TYPE.Where(ins => ins.INSURANCETYPEID == i.INSURANCETYPEID).Select(ins => ins.INSURANCETYPE).FirstOrDefault()

                }).ToList();

            return insurance;
        }

        public InsurancePolicy GetInsurancePolicy(int collateralId)
        {
            var insurance = context.TBL_COLLATERAL_ITEM_POLICY.Where(x => x.COLLATERALCUSTOMERID == collateralId
                                                                        && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                                                        || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Referred
                                                                        || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing
                                                                        || x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved)
                .Select(i => new InsurancePolicy
                {
                    referenceNumber = i.POLICYREFERENCENUMBER,
                    insuranceCompanyId = i.INSURANCECOMPANYID,
                    insuranceCompany = context.TBL_INSURANCE_COMPANY.Where(o => o.INSURANCECOMPANYID == i.INSURANCECOMPANYID).Select(o => o.COMPANYNAME).FirstOrDefault(),
                    sumInsured = i.SUMINSURED,
                    startDate = i.STARTDATE,
                    expiryDate = i.ENDDATE,
                    insuranceTypeId = i.INSURANCETYPEID,
                    hasExpired = i.HASEXPIRED,
                    policyId = i.POLICYID,
                    inSurPremiumAmount = i.PREMIUMAMOUNT,
                    description = i.DESCRIPTION,
                    premiumPercent = i.PREMIUMPERCENT,
                    insuranceType = context.TBL_INSURANCE_TYPE.Where(ins => ins.INSURANCETYPEID == i.INSURANCETYPEID).Select(ins => ins.INSURANCETYPE).FirstOrDefault(),
                    customerId = (int)i.TBL_COLLATERAL_CUSTOMER.CUSTOMERID,



                }).OrderByDescending(ip => ip.policyId).FirstOrDefault();

            return insurance;
        }

        public List<InsurancePolicy> GetTempCollateralInsurancePoliciesWaitingForApproval(int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.IsurancePolicyApproval).ToList();

            var insurance = (from x in context.TBL_TEMP_COLLATERAL_ITEM_POLI
                             join s in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals s.COLLATERALCUSTOMERID
                             join atrail in context.TBL_APPROVAL_TRAIL on x.TEMPPOLICYID equals atrail.TARGETID
                             join a in context.TBL_COLLATERAL_TYPE on s.COLLATERALTYPEID equals a.COLLATERALTYPEID
                             join c in context.TBL_CUSTOMER on s.CUSTOMERID equals c.CUSTOMERID
                             join b in context.TBL_COLLATERAL_TYPE_SUB on s.COLLATERALSUBTYPEID equals b.COLLATERALSUBTYPEID
                             where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                     && atrail.OPERATIONID == (int)OperationsEnum.IsurancePolicyApproval
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                     && x.ISPOLICYAPPROVAL == true
                                     && atrail.RESPONSESTAFFID == null
                             select new InsurancePolicy
                             {
                                 referenceNumber = x.POLICYREFERENCENUMBER,
                                 insuranceCompanyId = x.INSURANCECOMPANYID,
                                 sumInsured = x.SUMINSURED,
                                 startDate = x.STARTDATE,
                                 expiryDate = x.ENDDATE,
                                 insuranceTypeId = x.INSURANCETYPEID,
                                 collateralCode = s.COLLATERALCODE,
                                 collateralType = a.COLLATERALTYPENAME,
                                 collateralSubType = b.COLLATERALSUBTYPENAME,
                                 collateralValue = s.COLLATERALVALUE,
                                 collateraalId = x.COLLATERALCUSTOMERID,
                                 collateralTypeId = s.COLLATERALTYPEID,
                                 collateralSubTypeId = s.COLLATERALSUBTYPEID,
                                 policyId = x.TEMPPOLICYID,
                                 customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,




                             }).ToList();

            return insurance;
        }

        public List<InsurancePolicy> GetCollateralInsurancePoliciesWaitingForApproval(int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.IsurancePolicyApproval).ToList();

            //var insurance = (from x in context.TBL_COLLATERAL_ITEM_POLICY
            //                 join s in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals s.COLLATERALCUSTOMERID
            //                 join atrail in context.TBL_APPROVAL_TRAIL on x.POLICYID equals atrail.TARGETID
            //                 join a in context.TBL_COLLATERAL_TYPE on s.COLLATERALTYPEID equals a.COLLATERALTYPEID
            //                 join c in context.TBL_CUSTOMER on s.CUSTOMERID equals c.CUSTOMERID
            //                 join b in context.TBL_COLLATERAL_TYPE_SUB on s.COLLATERALSUBTYPEID equals b.COLLATERALSUBTYPEID
            //                 where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
            //                         && atrail.OPERATIONID == (int)OperationsEnum.IsurancePolicyApproval
            //                         && ids.Contains((int)atrail.TOAPPROVALLEVELID)
            //                         && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
            //                         && atrail.RESPONSESTAFFID == null
            var insurance = (from x in context.TBL_INSURANCE_REQUEST
                             join s in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals s.COLLATERALCUSTOMERID
                             join atrail in context.TBL_APPROVAL_TRAIL on x.INSURANCEREQUESTID equals atrail.TARGETID
                             join a in context.TBL_COLLATERAL_TYPE on s.COLLATERALTYPEID equals a.COLLATERALTYPEID
                             join c in context.TBL_CUSTOMER on s.CUSTOMERID equals c.CUSTOMERID
                             join b in context.TBL_COLLATERAL_TYPE_SUB on s.COLLATERALSUBTYPEID equals b.COLLATERALSUBTYPEID
                             where (atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing || atrail.APPROVALSTATUSID == (short)ApprovalStatusEnum.Referred || atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Finishing)
                                     && atrail.OPERATIONID == (int)OperationsEnum.IsurancePolicyApproval
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                     && atrail.LOOPEDSTAFFID == null
                                     && atrail.RESPONSESTAFFID == null
                             select new InsurancePolicy
                             {
                                 insuranceRequestId = x.INSURANCEREQUESTID,
                                 requestNumber = x.REQUESTNUMBER,
                                 collateralCode = s.COLLATERALCODE,
                                 collateralType = a.COLLATERALTYPENAME,
                                 collateralSubType = b.COLLATERALSUBTYPENAME,
                                 collateralValue = s.COLLATERALVALUE,
                                 collateraalId = x.COLLATERALCUSTOMERID,
                                 collateralTypeId = s.COLLATERALTYPEID,
                                 collateralSubTypeId = s.COLLATERALSUBTYPEID,
                                 haircut = s.HAIRCUT,
                                 collateralReleaseStatusName = s.COLLATERALRELEASESTATUSID == null ? context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == (int)CollateralReleaseStatus.InVault).FirstOrDefault().COLLATERALRELEASESTATUSNAME : context.TBL_COLLATERAL_RELEASE_STATUS.Where(q => q.COLLATERALRELEASESTATUSID == s.COLLATERALRELEASESTATUSID).FirstOrDefault().COLLATERALRELEASESTATUSNAME,
                                 collateralUsageStatus = s.COLLATERALUSAGESTATUSID,
                                 //policyId = x.POLICYID,
                                 customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
                                 //description = x.DESCRIPTION,
                                 //premiumAmount = x.PREMIUMAMOUNT,
                                 operationId = (int)OperationsEnum.IsurancePolicyApproval,
                                 requestReason = x.REQUESTREASON,
                                 requestComment = x.REQUESTCOMMENT,
                                 approvalStatusId = atrail.APPROVALSTATUSID,
                                 approvalStatusName = context.TBL_APPROVAL_STATUS.Where(s => s.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(s => s.APPROVALSTATUSNAME).FirstOrDefault(),
                                 customerId = c.CUSTOMERID,
                                 arrivalTime = atrail.SYSTEMARRIVALDATETIME
                             }).ToList();


            return insurance;
        }


        public IEnumerable<InsurancePolicy> Explore(string searchString)
        {
            var operationId = (int)OperationsEnum.IsurancePolicyApproval;

            searchString = searchString.Trim().ToLower();


            var operations = (from x in context.TBL_INSURANCE_REQUEST
                              join s in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals s.COLLATERALCUSTOMERID
                              join atrail in context.TBL_APPROVAL_TRAIL on x.INSURANCEREQUESTID equals atrail.TARGETID
                              join c in context.TBL_CUSTOMER on s.CUSTOMERID equals c.CUSTOMERID

                              where
                                (atrail.OPERATIONID == operationId
                                    && (x.REQUESTNUMBER.ToString().Trim().ToLower().Contains(searchString)
                                    || c.FIRSTNAME.ToLower().Contains(searchString)
                                    || c.LASTNAME.ToLower().Contains(searchString)
                                    || c.MIDDLENAME.ToLower().Contains(searchString)
                                    )
                                )
                              orderby atrail.APPROVALTRAILID descending
                              select new InsurancePolicy
                              {
                                  targetId = atrail.TARGETID,
                                  insuranceRequestId = x.INSURANCEREQUESTID,
                                  requestNumber = x.REQUESTNUMBER,
                                  //collateralCode = s.COLLATERALCODE,  
                                  collateraalId = x.COLLATERALCUSTOMERID,
                                  //collateralSubTypeId = s.COLLATERALSUBTYPEID,
                                  startDate = x.DATETIMECREATED,
                                  currentApprovalLevel = atrail.TOAPPROVALLEVELID != null ? context.TBL_APPROVAL_LEVEL.FirstOrDefault(l => l.APPROVALLEVELID == atrail.TOAPPROVALLEVELID).LEVELNAME : "n/a",
                                  customerName = c.FIRSTNAME + " " + c.LASTNAME + " " + c.MIDDLENAME,
                                  //approvalStatusId = atrail.APPROVALSTATUSID,
                                  approvalStatusName = context.TBL_APPROVAL_STATUS.Where(a => a.APPROVALSTATUSID == atrail.APPROVALSTATUSID).Select(a => a.APPROVALSTATUSNAME).FirstOrDefault(),
                                  customerId = c.CUSTOMERID,
                              }).ToList();

            operations = operations.GroupBy(x => x.targetId).Select(x => x.FirstOrDefault()).ToList();

            return operations;
        }
        // stock collateral

        private void AddTempStockCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainStock = (from x in context.TBL_COLLATERAL_STOCK
                                 where x.COLLATERALCUSTOMERID == collateralId
                                 select (x)).FirstOrDefault();

                if (mainStock != null)
                {
                    mainStock.COLLATERALCUSTOMERID = entity.collateralCustomerId;
                    mainStock.AMOUNT = entity.amount;
                    mainStock.COMPANYNAME = entity.companyName;
                    mainStock.MARKETPRICE = entity.marketPrice;
                    mainStock.SHAREQUANTITY = entity.shareQuantity;
                    mainStock.SHARESSECURITYVALUE = entity.sharesSecurityValue;
                    mainStock.SHAREVALUEAMOUNTTOUSE = entity.shareValueAmountToUse;
                    comment = $"New stock collateral type has been update through loan application by {entity.createdBy} staffid";
                }
                else
                {
                    context.TBL_COLLATERAL_STOCK.Add(new TBL_COLLATERAL_STOCK
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        AMOUNT = entity.amount,
                        COMPANYNAME = entity.companyName,
                        MARKETPRICE = entity.marketPrice,
                        SHAREQUANTITY = entity.shareQuantity,
                        SHARESSECURITYVALUE = entity.sharesSecurityValue,
                        SHAREVALUEAMOUNTTOUSE = entity.shareValueAmountToUse,
                    });
                    comment = $"New stock collateral type has been created through loan application by {entity.createdBy} staffid";
                }
            }
            else
            {
                context.TBL_TEMP_COLLATERAL_STOCK.Add(new TBL_TEMP_COLLATERAL_STOCK
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    COMPANYNAME = entity.companyName,
                    SHAREQUANTITY = entity.shareQuantity,
                    MARKETPRICE = entity.marketPrice,
                    AMOUNT = entity.amount,
                    SHARESSECURITYVALUE = entity.sharesSecurityValue,
                    SHAREVALUEAMOUNTTOUSE = entity.shareValueAmountToUse,
                });

                comment = $"New temp stock collateral type has been created by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = "Request for stock collateral approval";
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

        }

        private void UpdateStockCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_STOCK
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_STOCK()
                {
                    COMPANYNAME = entity.companyName,
                    SHAREQUANTITY = entity.shareQuantity,
                    MARKETPRICE = entity.marketPrice,
                    AMOUNT = entity.amount,
                    SHARESSECURITYVALUE = entity.sharesSecurityValue,
                    SHAREVALUEAMOUNTTOUSE = entity.shareValueAmountToUse,
                    COLLATERALCUSTOMERID = entity.collateralId
                };
                context.TBL_COLLATERAL_STOCK.Add(collateral);
                // var saved = context.SaveChanges() > 0;
                return;
            }

            collateral.COMPANYNAME = entity.companyName;
            collateral.SHAREQUANTITY = entity.shareQuantity;
            collateral.MARKETPRICE = entity.marketPrice;
            collateral.AMOUNT = entity.amount;
            collateral.SHARESSECURITYVALUE = entity.sharesSecurityValue;
            collateral.SHAREVALUEAMOUNTTOUSE = entity.shareValueAmountToUse;
        }

        private CollateralViewModel GetCollateralStock(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_STOCK.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                collateralStockId = specifics.COLLATERALSTOCKID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                companyName = specifics.COMPANYNAME,
                company = context.TBL_STOCK_COMPANY.Where(q => q.STOCKID.ToString() == specifics.COMPANYNAME).Select(y => y.STOCKNAME).FirstOrDefault(),

                shareQuantity = specifics.SHAREQUANTITY,
                marketPrice = specifics.MARKETPRICE,
                amount = specifics.AMOUNT,
                sharesSecurityValue = specifics.SHARESSECURITYVALUE,
                shareValueAmountToUse = specifics.SHAREVALUEAMOUNTTOUSE,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // promissory collateral

        private void AddPromissoryCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainVehicle = (from x in context.TBL_COLLATERAL_PROMISSORY
                                   where x.COLLATERALCUSTOMERID == collateralId
                                   select (x)).FirstOrDefault();

                if (mainVehicle != null)
                {
                    mainVehicle.COLLATERALCUSTOMERID = entity.collateralCustomerId;
                    mainVehicle.PROMISSORYNOTEID = entity.promissoryNoteRefferenceNumber;
                    mainVehicle.EFFECTIVEDATE = entity.promissoryEffectiveDate;
                    mainVehicle.MATURITYDATE = entity.promissoryMaturityDate;
                    comment = $"New promissory collateral type has been update through loan application by {entity.createdBy} staffid";
                }
                else
                {
                    context.TBL_COLLATERAL_PROMISSORY.Add(new TBL_COLLATERAL_PROMISSORY
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        PROMISSORYNOTEID = entity.promissoryNoteRefferenceNumber,
                        EFFECTIVEDATE = entity.promissoryEffectiveDate,
                        MATURITYDATE = entity.promissoryMaturityDate,
                        //PROMISSORYVALUE = tempPromissory.PROMISSORYVALUE,

                    });
                    comment = $"Promissory collateral type has been created through loan application by {entity.createdBy} staffid";

                }
            }

            else
            {
                var promissoryExist = context.TBL_COLLATERAL_PROMISSORY.Where(a => a.PROMISSORYNOTEID == entity.promissoryNoteRefferenceNumber).FirstOrDefault();
                if (promissoryExist != null)
                {
                    throw new ConditionNotMetException("Promisory Note Has Already Been Used Before.");
                }


                context.TBL_TEMP_COLLATERAL_PROMISSORY.Add(new TBL_TEMP_COLLATERAL_PROMISSORY
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    TEMPCOLLATERALPROMISSORYID = entity.collateralPromissoryId,
                    PROMISSORYNOTEID = entity.promissoryNoteRefferenceNumber,
                    //PROMISSORYVALUE = entity.promissoryValue,
                    EFFECTIVEDATE = entity.promissoryEffectiveDate,
                    MATURITYDATE = entity.promissoryMaturityDate,

                });
                comment = $" temp Promissory collateral type has been created by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }


        }

        private void AddISPOCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var collateral = (from x in context.TBL_COLLATERAL_ISPO
                                  where x.COLLATERALCUSTOMERID == collateralId
                                  select (x)).FirstOrDefault();

                if (collateral != null)
                {
                    collateral.COLLATERALCUSTOMERID = entity.collateralCustomerId;
                    collateral.ACCOUNTNAMETODEBIT = entity.accountNameToDebit;
                    collateral.ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit;
                    collateral.FREQUENCYTYPEID = (short)entity.renewalFrequencyTypeId;
                    collateral.SECURITYVALUE = entity.securityValue;
                    collateral.REGULARPAYMENTAMOUNT = (decimal)entity.regularPaymentAmount;
                    collateral.PAYER = entity.payer;
                    collateral.REMARK = entity.remark;
                    collateral.DESCRIPTION = entity.description;
                    comment = $"ISPO collateral type has been update through loan application by {entity.createdBy} staffid";
                }
                else
                {
                    context.TBL_COLLATERAL_ISPO.Add(new TBL_COLLATERAL_ISPO
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        ACCOUNTNAMETODEBIT = entity.accountNameToDebit,
                        ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit,
                        FREQUENCYTYPEID = (short)entity.renewalFrequencyTypeId,
                        SECURITYVALUE = entity.securityValue,
                        REGULARPAYMENTAMOUNT = (decimal)entity.regularPaymentAmount,
                        PAYER = entity.payer,
                        REMARK = entity.remark,
                        DESCRIPTION = entity.description
                    });

                    comment = $"ISPO collateral type has been created through loan application by {entity.createdBy} staffid";

                }
            }
            else
            {
                context.TBL_TEMP_COLLATERAL_ISPO.Add(new TBL_TEMP_COLLATERAL_ISPO
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    ACCOUNTNAMETODEBIT = entity.accountNameToDebit,
                    ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit,
                    FREQUENCYTYPEID = entity.renewalFrequencyTypeId,
                    SECURITYVALUE = entity.securityValue,
                    REGULARPAYMENTAMOUNT = (decimal)entity.regularPaymentAmount,
                    //APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing,
                    PAYER = entity.payer,
                    REMARK = entity.remark,
                    DESCRIPTION = entity.description,
                });
                comment = $"New ISPO collateral type has been created by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

        }

        private void AddContractDomiciliationCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var collateral = (from x in context.TBL_COLLATERAL_DOMICILIATION
                                  where x.COLLATERALCUSTOMERID == collateralId
                                  select (x)).FirstOrDefault();
                if (collateral != null)
                {
                    collateral.COLLATERALCUSTOMERID = collateralId;
                    collateral.CONTRACTDETAILS = entity.contractDetail;
                    collateral.EMPLOYER = entity.contractEmployer;
                    collateral.CONTRACTVALUE = entity.contractValue;
                    collateral.OUTSTANDINGINVOICEAMOUNT = entity.outstandingInvoiceAmount;
                    collateral.ACCOUNTNAMETODEBIT = entity.accountNameToDebit;
                    collateral.PAYER = entity.payer;
                    collateral.ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit;
                    collateral.REGULARPAYMENTAMOUNT = entity.regularPaymentAmount;
                    collateral.FREQUENCYTYPEID = entity.renewalFrequencyTypeId;
                    collateral.INVOICENUMBER = entity.invoiceNumber;
                    collateral.SECURITYVALUE = entity.securityValue;
                    collateral.INVOICEDATE = entity.invoiceDate;
                    //APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing,
                    collateral.REMARK = entity.remark;
                    collateral.DESCRIPTION = entity.description;
                    comment = $"New Salary Domiciliation collateral type has been update through loan application by {entity.createdBy} staffid";

                }
                else
                {
                    context.TBL_COLLATERAL_DOMICILIATION.Add(new TBL_COLLATERAL_DOMICILIATION
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        CONTRACTDETAILS = entity.contractDetail,
                        EMPLOYER = entity.contractEmployer,
                        CONTRACTVALUE = entity.contractValue,
                        OUTSTANDINGINVOICEAMOUNT = entity.outstandingInvoiceAmount,
                        ACCOUNTNAMETODEBIT = entity.accountNameToDebit,
                        PAYER = entity.payer,
                        ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit,
                        REGULARPAYMENTAMOUNT = entity.regularPaymentAmount,
                        FREQUENCYTYPEID = entity.renewalFrequencyTypeId,
                        INVOICENUMBER = entity.invoiceNumber,
                        SECURITYVALUE = entity.securityValue,
                        INVOICEDATE = entity.invoiceDate,
                        REMARK = entity.remark,
                        DESCRIPTION = entity.description,

                    });
                    comment = $"New Salary Domiciliation collateral type has been created through loan application by {entity.createdBy} staffid";

                }
            }
            else
            {
                context.TBL_TEMP_COLLATERAL_DOMCLTN.Add(new TBL_TEMP_COLLATERAL_DOMCLTN
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    CONTRACTDETAILS = entity.contractDetail,
                    EMPLOYER = entity.contractEmployer,
                    CONTRACTVALUE = entity.contractValue,
                    OUTSTANDINGINVOICEAMOUNT = entity.outstandingInvoiceAmount,
                    ACCOUNTNAMETODEBIT = entity.accountNameToDebit,
                    PAYER = entity.payer,
                    ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit,
                    REGULARPAYMENTAMOUNT = entity.regularPaymentAmount,
                    FREQUENCYTYPEID = entity.renewalFrequencyTypeId,
                    INVOICENUMBER = entity.invoiceNumber,
                    SECURITYVALUE = entity.securityValue,
                    INVOICEDATE = entity.invoiceDate,
                    //APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing,
                    REMARK = entity.remark,
                    DESCRIPTION = entity.description,
                });
                comment = $"New temp domiciliation contract collateral type has been created by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }


        }

        private void AddSalaryDomiciliationCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var collateral = (from x in context.TBL_COLLATERAL_DOMICILIATION
                                  where x.COLLATERALCUSTOMERID == collateralId
                                  select (x)).FirstOrDefault();
                if (collateral != null)
                {
                    collateral.CONTRACTDETAILS = entity.contractDetail;
                    collateral.EMPLOYER = entity.contractEmployer;
                    collateral.MONTHLYSALARY = entity.monthlySalary;
                    collateral.ANNUALALLOWANCES = entity.annualAllowances;
                    collateral.ANNUALEMOLUMENT = entity.annualEmolument;
                    collateral.ACCOUNTNUMBER = entity.accountNumber;
                    collateral.ANNUALSALARY = entity.annualSalary;
                    collateral.SECURITYVALUE = entity.securityValue;
                    collateral.REMARK = entity.remark;
                    collateral.DESCRIPTION = entity.description;
                    comment = $"New Salary Domiciliation collateral type has been update through loan application by {entity.createdBy} staffid";

                }
                else
                {
                    context.TBL_COLLATERAL_DOMICILIATION.Add(new TBL_COLLATERAL_DOMICILIATION
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        CONTRACTDETAILS = entity.contractDetail,
                        EMPLOYER = entity.contractEmployer,
                        MONTHLYSALARY = entity.monthlySalary,
                        ANNUALALLOWANCES = entity.annualAllowances,
                        ANNUALEMOLUMENT = entity.annualEmolument,
                        ACCOUNTNUMBER = entity.accountNumber,
                        ANNUALSALARY = entity.annualSalary,
                        SECURITYVALUE = entity.securityValue,
                        REMARK = entity.remark,
                        DESCRIPTION = entity.description

                    });
                    comment = $"New Salary Domiciliation collateral type has been created through loan application by {entity.createdBy} staffid";

                }
            }
            else
            {
                context.TBL_TEMP_COLLATERAL_DOMCLTN.Add(new TBL_TEMP_COLLATERAL_DOMCLTN
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    CONTRACTDETAILS = entity.contractDetail,

                    EMPLOYER = entity.contractEmployer,
                    MONTHLYSALARY = entity.monthlySalary,
                    ANNUALALLOWANCES = entity.annualAllowances,
                    ANNUALEMOLUMENT = entity.annualEmolument,
                    ACCOUNTNUMBER = entity.accountNumber,
                    ANNUALSALARY = entity.annualSalary,
                    SECURITYVALUE = entity.securityValue,
                    //APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing,
                    REMARK = entity.remark,
                    DESCRIPTION = entity.description,
                });
                comment = $"New Temp Salary Domiciliation collateral type has been created by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }


        }

        private void AddIndemityCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var collateral = (from x in context.TBL_COLLATERAL_INDEMNITY
                                  where x.COLLATERALCUSTOMERID == collateralId
                                  select (x)).FirstOrDefault();

                if (collateral != null)
                {
                    collateral.SECURITYVALUE = entity.securityValue;
                    collateral.REMARK = entity.remark;
                    collateral.ADDRESS = entity.address;
                    collateral.BVN = entity.bvn;
                    collateral.EMAILADRRESS = entity.emailAddress;
                    collateral.ENDDATE = entity.endDate;
                    collateral.STARTDATE = entity.startDate;
                    collateral.FIRSTNAME = entity.firstName;
                    collateral.MIDDLENAME = entity.middleName;
                    collateral.LASTNAME = entity.lastName;
                    collateral.PHONENUMBER1 = entity.phoneNumber1;
                    collateral.PHONENUMBER2 = entity.phoneNumber2;
                    collateral.RELATIONSHIPDURATION = entity.relationshipDuration;
                    collateral.RELATIONSHIP = entity.relationship;
                    collateral.TAXNUMBER = entity.taxNumber;
                    collateral.DESCRIPTION = entity.description;
                    comment = $"New Indemnity collateral type has been update through loan application by {entity.createdBy} staffid";
                }
                else
                {
                    context.TBL_COLLATERAL_INDEMNITY.Add(new TBL_COLLATERAL_INDEMNITY
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        SECURITYVALUE = entity.securityValue,
                        ADDRESS = entity.address,
                        BVN = entity.bvn,
                        EMAILADRRESS = entity.emailAddress,
                        ENDDATE = entity.endDate,
                        STARTDATE = entity.startDate,
                        FIRSTNAME = entity.firstName,
                        MIDDLENAME = entity.middleName,
                        LASTNAME = entity.lastName,
                        PHONENUMBER1 = entity.phoneNumber1,
                        PHONENUMBER2 = entity.phoneNumber2,
                        RELATIONSHIPDURATION = entity.relationshipDuration,
                        RELATIONSHIP = entity.relationship,
                        TAXNUMBER = entity.taxNumber,
                        REMARK = entity.remark,
                        DESCRIPTION = entity.description
                    });
                    comment = $"New Indemnity collateral type has been created through loan application by {entity.createdBy} staffid";
                }
            }
            else
            {
                context.TBL_TEMP_COLLATERAL_INDEMNITY.Add(new TBL_TEMP_COLLATERAL_INDEMNITY
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    SECURITYVALUE = entity.securityValue,
                    //APPROVALSTATUSID = (short)ApprovalStatusEnum.Processing,
                    REMARK = entity.remark,
                    ADDRESS = entity.address,
                    BVN = entity.bvn,
                    EMAILADRRESS = entity.emailAddress,
                    ENDDATE = entity.endDate.Value,
                    STARTDATE = entity.cStartDate,
                    FIRSTNAME = entity.firstName,
                    MIDDLENAME = entity.middleName,
                    LASTNAME = entity.lastName,
                    PHONENUMBER1 = entity.phoneNumber1,
                    PHONENUMBER2 = entity.phoneNumber2,
                    RELATIONSHIPDURATION = entity.relationshipDuration,
                    RELATIONSHIP = entity.relationship,
                    TAXNUMBER = entity.taxNumber,
                    DESCRIPTION = entity.description

                });
                comment = $"New Temp Indemnity collateral type has been created by {entity.createdBy} staffid";

                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true;
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

        }

        private void UpdatePromissoryCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_PROMISSORY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_PROMISSORY()
                {
                    PROMISSORYNOTEID = entity.promissoryNoteRefferenceNumber,
                    //PROMISSORYVALUE = entity.promissoryValue,
                    EFFECTIVEDATE = entity.promissoryEffectiveDate,
                    MATURITYDATE = entity.promissoryMaturityDate,
                    COLLATERALCUSTOMERID = entity.collateralId
                };
                context.TBL_COLLATERAL_PROMISSORY.Add(collateral);
                //var saved = context.SaveChanges() > 0;
                return;
            }

            collateral.PROMISSORYNOTEID = entity.promissoryNoteRefferenceNumber;
            //collateral.PROMISSORYVALUE = entity.promissoryValue;
            collateral.EFFECTIVEDATE = entity.promissoryEffectiveDate;
            collateral.MATURITYDATE = entity.promissoryMaturityDate;
        }

        private CollateralViewModel GetCollateralPromissory(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_PROMISSORY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                collateralPromissoryId = specifics.COLLATERALPROMISSORYID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                promissoryNoteRefferenceNumber = specifics.PROMISSORYNOTEID,
                promissoryEffectiveDate = specifics.EFFECTIVEDATE,
                promissoryMaturityDate = specifics.MATURITYDATE,
                //promissoryValue = specifics.PROMISSORYVALUE,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // vehicle collateral

        private void AddVehicleCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainVehicle = (from x in context.TBL_COLLATERAL_VEHICLE
                                   where x.COLLATERALCUSTOMERID == collateralId
                                   select (x)).FirstOrDefault();


                if (mainVehicle != null)
                {
                    //mainVehicle.COLLATERALCUSTOMERID = entity.collateralCustomerId;
                    mainVehicle.CHASISNUMBER = entity.chasisNumber;
                    mainVehicle.INVOICEVALUE = entity.invoiceValue;
                    mainVehicle.ENGINENUMBER = entity.engineNumber;
                    mainVehicle.LASTVALUATIONAMOUNT = entity.lastValuationAmount;
                    mainVehicle.MANUFACTUREDDATE = entity.dateOfManufacture;
                    mainVehicle.MODELNAME = entity.modelName;
                    mainVehicle.NAMEOFOWNER = entity.nameOfOwner;
                    mainVehicle.REGISTRATIONCOMPANY = entity.registrationCompany;
                    mainVehicle.REGISTRATIONNUMBER = entity.registrationNumber;
                    mainVehicle.REMARK = entity.remark;
                    mainVehicle.RESALEVALUE = entity.resaleValue;
                    mainVehicle.SERIALNUMBER = entity.serialNumber;
                    mainVehicle.VEHICLESTATUS = entity.vehicleStatus;
                    mainVehicle.VALUATIONDATE = entity.valuationDate;
                    mainVehicle.VEHICLEMAKE = entity.vehicleMake;
                    mainVehicle.VEHICLETYPE = entity.vehicleType;
                    comment = $"Vehicle collateral type has been update through loan application by {entity.createdBy} staffid";
                }
                else
                {
                    context.TBL_COLLATERAL_VEHICLE.Add(new TBL_COLLATERAL_VEHICLE
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        CHASISNUMBER = entity.chasisNumber,
                        INVOICEVALUE = entity.invoiceValue,
                        ENGINENUMBER = entity.engineNumber,
                        LASTVALUATIONAMOUNT = entity.lastValuationAmount,
                        MANUFACTUREDDATE = entity.dateOfManufacture,
                        MODELNAME = entity.modelName,
                        NAMEOFOWNER = entity.nameOfOwner,
                        REGISTRATIONCOMPANY = entity.registrationCompany,
                        REGISTRATIONNUMBER = entity.registrationNumber,
                        REMARK = entity.remark,
                        RESALEVALUE = entity.resaleValue,
                        SERIALNUMBER = entity.serialNumber,
                        VEHICLESTATUS = entity.vehicleStatus,
                        VALUATIONDATE = entity.valuationDate,
                        VEHICLEMAKE = entity.vehicleMake,
                        VEHICLETYPE = entity.vehicleType,

                    });

                    comment = $"New vehicle collateral type has been created through loan application by {entity.createdBy} staffid";

                }
            }

            else
            {
                context.TBL_TEMP_COLLATERAL_VEHICLE.Add(new TBL_TEMP_COLLATERAL_VEHICLE
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    VEHICLETYPE = entity.vehicleType,
                    VEHICLESTATUS = entity.vehicleStatus,
                    VEHICLEMAKE = entity.vehicleMake,
                    MODELNAME = entity.modelName,
                    MANUFACTUREDDATE = entity.dateOfManufacture,
                    REGISTRATIONNUMBER = entity.registrationNumber,
                    SERIALNUMBER = entity.serialNumber,
                    CHASISNUMBER = entity.chasisNumber,
                    ENGINENUMBER = entity.engineNumber,
                    NAMEOFOWNER = entity.nameOfOwner,
                    REGISTRATIONCOMPANY = entity.registrationCompany,
                    RESALEVALUE = entity.resaleValue,
                    VALUATIONDATE = entity.valuationDate,
                    LASTVALUATIONAMOUNT = entity.lastValuationAmount,
                    INVOICEVALUE = entity.invoiceValue,
                    REMARK = entity.remark,
                });

                comment = $"New vehicle collateral type has been update created by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

        }

        private void UpdateVehicleCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_VEHICLE
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();
            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_VEHICLE()
                {
                    VEHICLETYPE = entity.vehicleType,
                    VEHICLESTATUS = entity.vehicleStatus,
                    VEHICLEMAKE = entity.vehicleMake,
                    MODELNAME = entity.modelName,
                    MANUFACTUREDDATE = entity.dateOfManufacture,
                    REGISTRATIONNUMBER = entity.registrationNumber,
                    SERIALNUMBER = entity.serialNumber,
                    CHASISNUMBER = entity.chasisNumber,
                    ENGINENUMBER = entity.engineNumber,
                    NAMEOFOWNER = entity.nameOfOwner,
                    REGISTRATIONCOMPANY = entity.registrationCompany,
                    RESALEVALUE = entity.resaleValue,
                    VALUATIONDATE = entity.valuationDate,
                    LASTVALUATIONAMOUNT = entity.lastValuationAmount,
                    INVOICEVALUE = entity.invoiceValue,
                    REMARK = entity.remark,
                    COLLATERALCUSTOMERID = entity.collateralId
                };
                context.TBL_COLLATERAL_VEHICLE.Add(collateral);
                //var saved = context.SaveChanges() > 0;
                return;
            }
            collateral.VEHICLETYPE = entity.vehicleType;
            collateral.VEHICLESTATUS = entity.vehicleStatus;
            collateral.VEHICLEMAKE = entity.vehicleMake;
            collateral.MODELNAME = entity.modelName;
            collateral.MANUFACTUREDDATE = entity.dateOfManufacture;
            collateral.REGISTRATIONNUMBER = entity.registrationNumber;
            collateral.SERIALNUMBER = entity.serialNumber;
            collateral.CHASISNUMBER = entity.chasisNumber;
            collateral.ENGINENUMBER = entity.engineNumber;
            collateral.NAMEOFOWNER = entity.nameOfOwner;
            collateral.REGISTRATIONCOMPANY = entity.registrationCompany;
            collateral.RESALEVALUE = entity.resaleValue;
            collateral.VALUATIONDATE = entity.valuationDate;
            collateral.LASTVALUATIONAMOUNT = entity.lastValuationAmount;
            collateral.INVOICEVALUE = entity.invoiceValue;
            collateral.REMARK = entity.remark;
        }

        private void UpdateISPOCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_ISPO
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_ISPO()
                {
                    ACCOUNTNAMETODEBIT = entity.accountNameToDebit,
                    ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit,
                    FREQUENCYTYPEID = (short)entity.renewalFrequencyTypeId,
                    SECURITYVALUE = entity.securityValue,
                    REGULARPAYMENTAMOUNT = (decimal)entity.regularPaymentAmount,
                    PAYER = entity.payer,
                    REMARK = entity.remark,
                    DESCRIPTION = entity.description,
                    COLLATERALCUSTOMERID = entity.collateralId
                };
                context.TBL_COLLATERAL_ISPO.Add(collateral);
                // var saved = context.SaveChanges() > 0;
                return;
            }
            collateral.ACCOUNTNAMETODEBIT = entity.accountNameToDebit;
            collateral.ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit;
            collateral.FREQUENCYTYPEID = (short)entity.renewalFrequencyTypeId;
            collateral.SECURITYVALUE = entity.securityValue;
            collateral.REGULARPAYMENTAMOUNT = (decimal)entity.regularPaymentAmount;
            collateral.PAYER = entity.payer;
            collateral.REMARK = entity.remark;
            collateral.DESCRIPTION = entity.description;
        }

        private void UpdateContractDomiciliationCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_DOMICILIATION
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_DOMICILIATION()
                {
                    CONTRACTDETAILS = entity.contractDetail,
                    COLLATERALCUSTOMERID = entity.collateralId,
                    EMPLOYER = entity.contractEmployer,
                    CONTRACTVALUE = entity.contractValue,
                    OUTSTANDINGINVOICEAMOUNT = entity.outstandingInvoiceAmount,
                    ACCOUNTNAMETODEBIT = entity.accountNameToDebit,
                    PAYER = entity.payer,
                    ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit,
                    REGULARPAYMENTAMOUNT = entity.regularPaymentAmount,
                    FREQUENCYTYPEID = (short)entity.renewalFrequencyTypeId,
                    INVOICENUMBER = entity.invoiceNumber,
                    SECURITYVALUE = entity.securityValue,
                    INVOICEDATE = entity.invoiceDate,
                    REMARK = entity.remark,
                    DESCRIPTION = entity.description
                };
                context.TBL_COLLATERAL_DOMICILIATION.Add(collateral);
                //var saved = context.SaveChanges() > 0;
                return;
            }
            collateral.CONTRACTDETAILS = entity.contractDetail;
            collateral.EMPLOYER = entity.contractEmployer;
            collateral.CONTRACTVALUE = entity.contractValue;
            collateral.OUTSTANDINGINVOICEAMOUNT = entity.outstandingInvoiceAmount;
            collateral.ACCOUNTNAMETODEBIT = entity.accountNameToDebit;
            collateral.PAYER = entity.payer;
            collateral.ACCOUNTNUMBERTODEBIT = entity.accountNumberToDebit;
            collateral.REGULARPAYMENTAMOUNT = entity.regularPaymentAmount;
            collateral.FREQUENCYTYPEID = (short)entity.renewalFrequencyTypeId;
            collateral.INVOICENUMBER = entity.invoiceNumber;
            collateral.SECURITYVALUE = entity.securityValue;
            collateral.INVOICEDATE = entity.invoiceDate;
            collateral.REMARK = entity.remark;
            collateral.DESCRIPTION = entity.description;
        }

        private void UpdateSalaryDomiciliationCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_DOMICILIATION
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_DOMICILIATION()
                {
                    CONTRACTDETAILS = entity.contractDetail,
                    EMPLOYER = entity.contractEmployer,
                    MONTHLYSALARY = entity.monthlySalary,
                    ANNUALALLOWANCES = entity.annualAllowances,
                    ANNUALEMOLUMENT = entity.annualEmolument,
                    ACCOUNTNUMBER = entity.accountNumber,
                    ANNUALSALARY = entity.annualSalary,
                    SECURITYVALUE = entity.securityValue,
                    REMARK = entity.remark,
                    COLLATERALCUSTOMERID = entity.collateralId,
                };
                context.TBL_COLLATERAL_DOMICILIATION.Add(collateral);
                //var saved = context.SaveChanges() > 0;
                return;
            }
            collateral.CONTRACTDETAILS = entity.contractDetail;
            collateral.EMPLOYER = entity.contractEmployer;
            collateral.MONTHLYSALARY = entity.monthlySalary;
            collateral.ANNUALALLOWANCES = entity.annualAllowances;
            collateral.ANNUALEMOLUMENT = entity.annualEmolument;
            collateral.ACCOUNTNUMBER = entity.accountNumber;
            collateral.ANNUALSALARY = entity.annualSalary;
            collateral.SECURITYVALUE = entity.securityValue;
            collateral.REMARK = entity.remark;
        }

        private void UpdateIndemityCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_INDEMNITY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_INDEMNITY()
                {
                    SECURITYVALUE = entity.securityValue,
                    ADDRESS = entity.address,
                    BVN = entity.bvn,
                    EMAILADRRESS = entity.emailAddress,
                    ENDDATE = (DateTime)entity.endDate,
                    STARTDATE = (DateTime)entity.startDate,
                    FIRSTNAME = entity.firstName,
                    MIDDLENAME = entity.middleName,
                    LASTNAME = entity.lastName,
                    PHONENUMBER1 = entity.phoneNumber1,
                    PHONENUMBER2 = entity.phoneNumber2,
                    RELATIONSHIPDURATION = entity.relationshipDuration,
                    RELATIONSHIP = entity.relationship,
                    TAXNUMBER = entity.taxNumber,
                    REMARK = entity.remark,
                    DESCRIPTION = entity.description,
                    COLLATERALCUSTOMERID = entity.collateralId
                };
                context.TBL_COLLATERAL_INDEMNITY.Add(collateral);
                //var saved = context.SaveChanges() > 0;
                return;
            }
            collateral.SECURITYVALUE = entity.securityValue;
            collateral.ADDRESS = entity.address;
            collateral.BVN = entity.bvn;
            collateral.EMAILADRRESS = entity.emailAddress;
            collateral.ENDDATE = (DateTime)entity.endDate;
            collateral.STARTDATE = (DateTime)entity.startDate;
            collateral.FIRSTNAME = entity.firstName;
            collateral.MIDDLENAME = entity.middleName;
            collateral.LASTNAME = entity.lastName;
            collateral.PHONENUMBER1 = entity.phoneNumber1;
            collateral.PHONENUMBER2 = entity.phoneNumber2;
            collateral.RELATIONSHIPDURATION = entity.relationshipDuration;
            collateral.RELATIONSHIP = entity.relationship;
            collateral.TAXNUMBER = entity.taxNumber;
            collateral.REMARK = entity.remark;
            collateral.DESCRIPTION = entity.description;
        }

        private CollateralViewModel GetCollateralVehicle(int collateralId)
        {
            CollateralViewModel details = null;
            var specifics = context.TBL_COLLATERAL_VEHICLE.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            if (specifics != null) { 
                details = new CollateralViewModel
                {
                    collateralId = specifics.COLLATERALCUSTOMERID,
                    collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                    collateralVehicleId = specifics.COLLATERALVEHICLEID,
                    collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                    vehicleType = specifics.VEHICLETYPE,
                    vehicleStatus = specifics.VEHICLESTATUS,
                    vehicleMake = specifics.VEHICLEMAKE,
                    modelName = specifics.MODELNAME,
                    dateOfManufacture = specifics.MANUFACTUREDDATE.Value,
                    registrationNumber = specifics.REGISTRATIONNUMBER,
                    serialNumber = specifics.SERIALNUMBER,
                    chasisNumber = specifics.CHASISNUMBER,
                    engineNumber = specifics.ENGINENUMBER,
                    nameOfOwner = specifics.NAMEOFOWNER,
                    registrationCompany = specifics.REGISTRATIONCOMPANY,
                    resaleValue = specifics.RESALEVALUE,
                    valuationDate = specifics.VALUATIONDATE,
                    lastValuationAmount = specifics.LASTVALUATIONAMOUNT,
                    invoiceValue = specifics.INVOICEVALUE,
                    remark = specifics.REMARK,
                };
                details = GetCollateralInsurancePolicy(details);
            }
            return details;
        }

        private CollateralViewModel GetISPOCollateral(int collateralId)
        {
            CollateralViewModel details = null;
            var collateral = context.TBL_COLLATERAL_ISPO.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            if (collateral != null)
            {
                details = new CollateralViewModel
                {
                    collateralId = collateral.COLLATERALCUSTOMERID,
                    collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                    collateralISPOId = collateral.COLLATERALISPOID,
                    accountNameToDebit = collateral.ACCOUNTNAMETODEBIT,
                    accountNumberToDebit = collateral.ACCOUNTNUMBERTODEBIT,
                    interval = collateral.TBL_FREQUENCY_TYPE.MODE,
                    securityValue = collateral.SECURITYVALUE,
                    regularPaymentAmount = collateral.REGULARPAYMENTAMOUNT,
                    payer = collateral.PAYER,
                    remark = collateral.REMARK,
                    description = collateral.DESCRIPTION

                };
                details = GetCollateralInsurancePolicy(details);
            }
            return details;
        }


        private CollateralViewModel GetContractDomiciliationCollateral(int collateralId)
        {
            CollateralViewModel details = null;
            var collateral = context.TBL_COLLATERAL_DOMICILIATION.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            if (collateral != null)
            {
                details = new CollateralViewModel
                {
                    collateralId = collateral.COLLATERALCUSTOMERID,
                    collateralDomiciliationId = collateral.COLLATERALDOMICILIATIONID,
                    collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                    contractDetail = collateral.CONTRACTDETAILS,
                    contractEmployer = collateral.EMPLOYER,
                    contractValue = collateral.CONTRACTVALUE,
                    outstandingInvoiceAmount = collateral.OUTSTANDINGINVOICEAMOUNT,
                    accountNameToDebit = collateral.ACCOUNTNAMETODEBIT,
                    payer = collateral.PAYER,
                    renewalFrequencyTypeId = collateral.FREQUENCYTYPEID,
                    accountNumberToDebit = collateral.ACCOUNTNUMBERTODEBIT,
                    regularPaymentAmount = collateral.REGULARPAYMENTAMOUNT,
                    interval = collateral.TBL_FREQUENCY_TYPE.MODE,
                    invoiceNumber = collateral.INVOICENUMBER,
                    securityValue = collateral.SECURITYVALUE,
                    invoiceDate = collateral.INVOICEDATE,
                    remark = collateral.REMARK,
                    description = collateral.DESCRIPTION
                };
                details = GetCollateralInsurancePolicy(details);
            }
            return details;
        }


        private CollateralViewModel GetContractDomiciliationSalary(int collateralId)
        {
            CollateralViewModel details = null;
            var collateral = context.TBL_COLLATERAL_DOMICILIATION.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            if (collateral != null)
            {
                details = new CollateralViewModel
                {
                    collateralId = collateral.COLLATERALCUSTOMERID,
                    collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                    collateralDomiciliationId = collateral.COLLATERALDOMICILIATIONID,
                    contractDetail = collateral.CONTRACTDETAILS,
                    contractEmployer = collateral.EMPLOYER,
                    monthlySalary = collateral.MONTHLYSALARY,
                    annualAllowances = collateral.ANNUALALLOWANCES,
                    annualEmolument = collateral.ANNUALEMOLUMENT,
                    accountNumber = collateral.ACCOUNTNUMBER,
                    annualSalary = collateral.ANNUALSALARY,
                    securityValue = collateral.SECURITYVALUE,
                    remark = collateral.REMARK,
                    description = collateral.DESCRIPTION
                };
                details = GetCollateralInsurancePolicy(details);
            }
            return details;
        }

        private CollateralViewModel GetIndemityCollateral(int collateralId)
        {
            CollateralViewModel details = null;
            var collateral = context.TBL_COLLATERAL_INDEMNITY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            if (collateral != null)
            {
                details = new CollateralViewModel
                {
                    collateralId = collateral.COLLATERALCUSTOMERID,
                    collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                    collateralIndemnityId = collateral.COLLATERALINDEMNITYID,
                    securityValue = collateral.SECURITYVALUE,
                    remark = collateral.REMARK,
                    address = collateral.ADDRESS,
                    bvn = collateral.BVN,
                    emailAddress = collateral.EMAILADRRESS,
                    endDate = collateral.ENDDATE,
                    startDate = collateral.STARTDATE,
                    firstName = collateral.FIRSTNAME,
                    middleName = collateral.MIDDLENAME,
                    lastName = collateral.LASTNAME,
                    phoneNumber1 = collateral.PHONENUMBER1,
                    phoneNumber2 = collateral.PHONENUMBER2,
                    relationshipDuration = collateral.RELATIONSHIPDURATION,
                    relationship = collateral.RELATIONSHIP,
                    taxNumber = collateral.TAXNUMBER,
                    description = collateral.DESCRIPTION
                };
                details = GetCollateralInsurancePolicy(details);
            }
            return details;
        }
        // preciousMetal collateral

        private void AddTempPreciousMetalCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainMetal = (from x in context.TBL_COLLATERAL_PRECIOUSMETAL
                                 where x.COLLATERALCUSTOMERID == collateralId
                                 select (x)).FirstOrDefault();

                if (mainMetal != null)
                {
                    mainMetal.COLLATERALCUSTOMERID = entity.collateralCustomerId;
                    mainMetal.REMARK = entity.remark;
                    mainMetal.METALTYPE = entity.metalType;
                    mainMetal.PRECIOUSMETALFORM = entity.preciousMetalFrm;
                    mainMetal.PRECIOUSMETALNAME = entity.preciousMetalName;
                    mainMetal.UNITRATE = entity.metalUnitRate;
                    mainMetal.VALUATIONAMOUNT = entity.metalValuationAmount;
                    mainMetal.WEIGHTINGRAMMES = entity.weightInGrammes;
                    comment = $"New precious metal collateral type has been update through loan application by {entity.createdBy} staffid";
                }
                else
                {
                    context.TBL_COLLATERAL_PRECIOUSMETAL.Add(new TBL_COLLATERAL_PRECIOUSMETAL
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        REMARK = entity.remark,
                        METALTYPE = entity.metalType,
                        PRECIOUSMETALFORM = entity.preciousMetalFrm,
                        PRECIOUSMETALNAME = entity.preciousMetalName,
                        UNITRATE = entity.metalUnitRate,
                        VALUATIONAMOUNT = entity.valuationAmount,
                        WEIGHTINGRAMMES = entity.weightInGrammes,

                    });
                    comment = $" Precious metal collateral type has been created through loan application by {entity.createdBy} staffid";

                }
            }

            else
            {
                context.TBL_TEMP_COLLATERAL_PREC_METAL.Add(new TBL_TEMP_COLLATERAL_PREC_METAL
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    //ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                    PRECIOUSMETALNAME = entity.preciousMetalName,
                    WEIGHTINGRAMMES = entity.weightInGrammes,
                    VALUATIONAMOUNT = entity.metalValuationAmount,
                    UNITRATE = entity.metalUnitRate,
                    PRECIOUSMETALFORM = entity.preciousMetalFrm,
                    METALTYPE = entity.metalType,
                    REMARK = entity.remark,
                });

                comment = $"New temp precious metal collateral type has been created by {entity.createdBy} staffid";

                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

        }

        private void UpdatePreciousMetalCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_PRECIOUSMETAL
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_PRECIOUSMETAL()
                {
                    //ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                    PRECIOUSMETALNAME = entity.preciousMetalName,
                    WEIGHTINGRAMMES = entity.weightInGrammes,
                    VALUATIONAMOUNT = entity.metalValuationAmount,
                    UNITRATE = entity.metalUnitRate,
                    PRECIOUSMETALFORM = entity.preciousMetalFrm,
                    METALTYPE = entity.metalType,
                    REMARK = entity.remark,
                    COLLATERALCUSTOMERID = entity.collateralId
                };
                context.TBL_COLLATERAL_PRECIOUSMETAL.Add(collateral);
                //var saved = context.SaveChanges() > 0;
                return;
            }

            //collateral.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
            collateral.PRECIOUSMETALNAME = entity.preciousMetalName;
            collateral.WEIGHTINGRAMMES = entity.weightInGrammes;
            collateral.VALUATIONAMOUNT = entity.metalValuationAmount;
            collateral.UNITRATE = entity.metalUnitRate;
            collateral.PRECIOUSMETALFORM = entity.preciousMetalFrm;
            collateral.METALTYPE = entity.metalType;
            collateral.REMARK = entity.remark;
        }

        private CollateralViewModel GetCollateralPreciousMetal(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_PRECIOUSMETAL.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                collateralPreciousMetalId = specifics.COLLATERALPRECIOUSMETALID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                //isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                preciousMetalName = specifics.PRECIOUSMETALNAME,
                weightInGrammes = specifics.WEIGHTINGRAMMES,
                metalValuationAmount = specifics.VALUATIONAMOUNT,
                metalUnitRate = specifics.UNITRATE,
                preciousMetalFrm = specifics.PRECIOUSMETALFORM,
                metalType = specifics.METALTYPE,
                remark = specifics.REMARK,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }


        private void UpdateCasaCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_CASA
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();
            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_CASA()
                {
                    ACCOUNTNUMBER = entity.collateralCode,
                    COLLATERALCUSTOMERID = entity.collateralId,
                    // ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                    AVAILABLEBALANCE = entity.availableBalance,
                    // EXISTINGLIENAMOUNT = entity.existingLienAmount,
                    LIENAMOUNT = entity.lienAmount,
                    SECURITYVALUE = (decimal)entity.securityValue,
                    REMARK = entity.remark,
                    ACCOUNTNAME = entity.accountName,
                };
                context.TBL_COLLATERAL_CASA.Add(collateral);
                var saved = context.SaveChanges() != 0;
                return;
            }
            collateral.ACCOUNTNUMBER = entity.collateralCode;
            // collateral.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
            collateral.AVAILABLEBALANCE = entity.availableBalance;
            // collateral.EXISTINGLIENAMOUNT = entity.existingLienAmount;
            collateral.LIENAMOUNT = entity.lienAmount;
            collateral.SECURITYVALUE = (decimal)entity.securityValue;
            collateral.REMARK = entity.remark;
            collateral.ACCOUNTNAME = entity.accountName;
        }

        private CollateralViewModel GetCollateralCasa(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            if (specifics == null)
            {
                return null;
            }
            CasaBalanceViewModel acc = repo.GetCASABalance(specifics.ACCOUNTNUMBER, specifics.TBL_COLLATERAL_CUSTOMER.COMPANYID);
            specifics.AVAILABLEBALANCE = acc.availableBalance;
            specifics.ACCOUNTNAME = acc.accountName;
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                accountNumber = specifics.ACCOUNTNUMBER,
                //  isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                availableBalance = acc.availableBalance,
                //  existingLienAmount = specifics.EXISTINGLIENAMOUNT,
                lienAmount = specifics.LIENAMOUNT,
                securityValue = specifics.SECURITYVALUE,
                remark = specifics.REMARK,
                accountName = acc.accountName,
                baseCurrencyCode = specifics.TBL_COLLATERAL_CUSTOMER.TBL_CURRENCY.CURRENCYCODE,

            };
            details = GetCollateralInsurancePolicy(details);
            var saved = context.SaveChanges() != 0;
            return details;
        }

        // guarantee collateral

        private void AddTempGuaranteeCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;
            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {

                var guarantee = context.TBL_COLLATERAL_GAURANTEE
               .Where(x => x.COLLATERALCUSTOMERID == collateralId)
               .FirstOrDefault();

                if (guarantee != null)
                {
                    guarantee.INSTITUTIONNAME = entity.institutionName;
                    guarantee.GUARANTORADDRESS = entity.guarantorAddress;
                    guarantee.GUARANTEEVALUE = entity.guaranteeValue;
                    guarantee.STARTDATE = entity.cStartDate;
                    guarantee.ENDDATE = entity.endDate;
                    guarantee.REMARK = entity.remark;
                    guarantee.FIRSTNAME = entity.firstName;
                    guarantee.MIDDLENAME = entity.middleName;
                    guarantee.LASTNAME = entity.lastName;
                    guarantee.BVN = entity.bvn;
                    guarantee.RCNUMBER = entity.rcNumber;
                    guarantee.PHONENUMBER1 = entity.phoneNumber1;
                    guarantee.PHONENUMBER2 = entity.phoneNumber2;
                    guarantee.EMAILADDRESS = entity.emailAddress;
                    guarantee.RELATIONSHIP = entity.relationship;
                    guarantee.RELATIONSHIPDURATION = entity.relationshipDuration;
                    guarantee.AGE = entity.age;
                    comment = $"guarantee collateral type has been update through loan application by {entity.createdBy} staffid";

                }
                else
                {
                    context.TBL_COLLATERAL_GAURANTEE.Add(new TBL_COLLATERAL_GAURANTEE
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        INSTITUTIONNAME = entity.institutionName,
                        GUARANTORADDRESS = entity.guarantorAddress,
                        GUARANTEEVALUE = entity.guaranteeValue,
                        STARTDATE = entity.cStartDate,
                        ENDDATE = entity.endDate,
                        REMARK = entity.remark,
                        FIRSTNAME = entity.firstName,
                        MIDDLENAME = entity.middleName,
                        LASTNAME = entity.lastName,
                        BVN = entity.bvn,
                        RCNUMBER = entity.rcNumber,
                        PHONENUMBER1 = entity.phoneNumber1,
                        PHONENUMBER2 = entity.phoneNumber2,
                        EMAILADDRESS = entity.emailAddress,
                        RELATIONSHIP = entity.relationship,
                        RELATIONSHIPDURATION = entity.relationshipDuration,
                        AGE = entity.age,
                });

                    comment = $"New guarantee collateral type has been update through loan application by {entity.createdBy} staffid";
                }

            }

            else
            {
                context.TBL_TEMP_COLLATERAL_GAURANTEE.Add(new TBL_TEMP_COLLATERAL_GAURANTEE
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    // ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                    INSTITUTIONNAME = entity.institutionName,
                    GUARANTORADDRESS = entity.guarantorAddress,
                    // GUARANTORREFERENCENUMBER = entity.guarantorReferenceNumber,
                    GUARANTEEVALUE = entity.guaranteeValue,
                    STARTDATE = entity.cStartDate,
                    ENDDATE = entity.endDate,
                    REMARK = entity.remark,
                    FIRSTNAME = entity.firstName,
                    MIDDLENAME = entity.middleName,
                    LASTNAME = entity.lastName,
                    BVN = entity.bvn,
                    RCNUMBER = entity.rcNumber,
                    PHONENUMBER1 = entity.phoneNumber1,
                    PHONENUMBER2 = entity.phoneNumber2,
                    EMAILADDRESS = entity.emailAddress,
                    RELATIONSHIP = entity.relationship,
                    RELATIONSHIPDURATION = entity.relationshipDuration,
                    TAXNUMBER = entity.taxNumber,
                    AGE = entity.age,
                });

                comment = $"New temp guarantee collateral type has been update through loan application by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

        }
        public List<CollateralViewModel> AddGuaranteeJoinCollateral(CollateralViewModel entity, byte[] buffer)
        {
            int collateralId = 0;
            List<CollateralViewModel> listOfJoinCollateralGuarantee = new List<CollateralViewModel>();

            if (context.TBL_TEMP_COLLATERAL_CUSTOMER.Where(x => x.TEMPCOLLATERALCUSTOMERID == entity.collateralId).Any() != true)
            {

                collateralId = AddCollateralMainFormForGurantee(entity);

                var guarantee = context.TBL_TEMP_COLLATERAL_GAURANTEE.Add(new TBL_TEMP_COLLATERAL_GAURANTEE
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    INSTITUTIONNAME = entity.institutionName,
                    GUARANTORADDRESS = entity.guarantorAddress,
                    GUARANTEEVALUE = entity.guaranteeValue,
                    STARTDATE = entity.cStartDate,
                    ENDDATE = entity.endDate,
                    REMARK = entity.remark,
                    FIRSTNAME = entity.firstName,
                    MIDDLENAME = entity.middleName,
                    LASTNAME = entity.lastName,
                    BVN = entity.bvn,
                    RCNUMBER = entity.rcNumber,
                    PHONENUMBER1 = entity.phoneNumber1,
                    PHONENUMBER2 = entity.phoneNumber2,
                    EMAILADDRESS = entity.emailAddress,
                    RELATIONSHIP = entity.relationship,
                    RELATIONSHIPDURATION = entity.relationshipDuration,

                });

                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = "Request for join Guarantee collateral approval";
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                context.SaveChanges();
                entity.TargetId = guarantee.TEMPCOLLATERALGAURANTEEID;
                if (buffer != null) { SaveCollateralMainDocument(entity, collateralId, buffer); }
                listOfJoinCollateralGuarantee = GetCollateralJoinGuarantiee(collateralId);
            }
            else
            {
                if (entity.collateralId > 0)
                {

                    var guarantee = context.TBL_TEMP_COLLATERAL_GAURANTEE.Add(new TBL_TEMP_COLLATERAL_GAURANTEE
                    {
                        TEMPCOLLATERALCUSTOMERID = entity.collateralId,
                        INSTITUTIONNAME = entity.institutionName,
                        GUARANTORADDRESS = entity.guarantorAddress,
                        GUARANTEEVALUE = entity.guaranteeValue,
                        STARTDATE = entity.cStartDate,
                        ENDDATE = entity.endDate,
                        REMARK = entity.remark,
                        FIRSTNAME = entity.firstName,
                        MIDDLENAME = entity.middleName,
                        LASTNAME = entity.lastName,
                        BVN = entity.bvn,
                        RCNUMBER = entity.rcNumber,
                        PHONENUMBER1 = entity.phoneNumber1,
                        PHONENUMBER2 = entity.phoneNumber2,
                        EMAILADDRESS = entity.emailAddress,
                        RELATIONSHIP = entity.relationship,
                        RELATIONSHIPDURATION = entity.relationshipDuration

                    });

                    workflow.StaffId = entity.createdBy;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = collateralId;
                    workflow.Comment = "Request for join Guarantee collateral approval";
                    workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                    workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();

                    context.SaveChanges();
                    entity.TargetId = guarantee.TEMPCOLLATERALGAURANTEEID;
                    if (buffer != null) { SaveCollateralMainDocument(entity, entity.collateralId, buffer); }
                    listOfJoinCollateralGuarantee = GetCollateralJoinGuarantiee(entity.collateralId);

                }
            }
            return listOfJoinCollateralGuarantee;
        }

        private List<CollateralViewModel> GetCollateralJoinGuarantiee(int collateralId)
        {

            var guarantee = from x in context.TBL_TEMP_COLLATERAL_GAURANTEE
                            where x.TEMPCOLLATERALCUSTOMERID == collateralId

                            select new CollateralViewModel
                            {

                                institutionName = x.INSTITUTIONNAME,
                                guarantorAddress = x.GUARANTORADDRESS,
                                guaranteeValue = x.GUARANTEEVALUE,
                                cStartDate = x.STARTDATE,
                                endDate = x.ENDDATE,
                                remark = x.REMARK,
                                firstName = x.FIRSTNAME,
                                middleName = x.MIDDLENAME,
                                lastName = x.LASTNAME,
                                bvn = x.BVN,
                                rcNumber = x.RCNUMBER,
                                phoneNumber1 = x.PHONENUMBER1,
                                phoneNumber2 = x.PHONENUMBER2,
                                emailAddress = x.EMAILADDRESS,
                                relationship = x.RELATIONSHIP,
                                relationshipDuration = x.RELATIONSHIPDURATION,
                                collateralId = collateralId,
                                TargetId = x.TEMPCOLLATERALGAURANTEEID

                            };
            return guarantee.ToList();
        }

        private void UpdateGuaranteeCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_GAURANTEE
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_GAURANTEE()
                {
                    // ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                    INSTITUTIONNAME = entity.institutionName,
                    GUARANTORADDRESS = entity.guarantorAddress,
                    //   GUARANTORREFERENCENUMBER = entity.guarantorReferenceNumber,
                    GUARANTEEVALUE = entity.guaranteeValue,
                    STARTDATE = entity.cStartDate,
                    ENDDATE = entity.endDate,
                    REMARK = entity.remark,
                    FIRSTNAME = entity.firstName,
                    MIDDLENAME = entity.middleName,
                    LASTNAME = entity.lastName,
                    TAXNUMBER = entity.taxNumber,
                    BVN = entity.bvn,
                    RCNUMBER = entity.rcNumber,
                    PHONENUMBER1 = entity.phoneNumber1,
                    PHONENUMBER2 = entity.phoneNumber2,
                    EMAILADDRESS = entity.emailAddress,
                    RELATIONSHIP = entity.relationship,
                    RELATIONSHIPDURATION = entity.relationshipDuration,
                    COLLATERALCUSTOMERID = entity.collateralId
                };
                context.TBL_COLLATERAL_GAURANTEE.Add(collateral);
                //var saved = context.SaveChanges() > 0;
                return;
            }

            // collateral.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
            collateral.INSTITUTIONNAME = entity.institutionName;
            collateral.GUARANTORADDRESS = entity.guarantorAddress;
            //   collateral.GUARANTORREFERENCENUMBER = entity.guarantorReferenceNumber;
            collateral.GUARANTEEVALUE = entity.guaranteeValue;
            collateral.STARTDATE = entity.cStartDate;
            collateral.ENDDATE = entity.endDate;
            collateral.REMARK = entity.remark;
            collateral.FIRSTNAME = entity.firstName;
            collateral.MIDDLENAME = entity.middleName;
            collateral.LASTNAME = entity.lastName;
            collateral.TAXNUMBER = entity.taxNumber;
            collateral.BVN = entity.bvn;
            collateral.RCNUMBER = entity.rcNumber;
            collateral.PHONENUMBER1 = entity.phoneNumber1;
            collateral.PHONENUMBER2 = entity.phoneNumber2;
            collateral.EMAILADDRESS = entity.emailAddress;
            collateral.RELATIONSHIP = entity.relationship;
            collateral.RELATIONSHIPDURATION = entity.relationshipDuration;
        }

        private CollateralViewModel GetCollateralGuarantee(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_GAURANTEE.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralGauranteeId = specifics.COLLATERALGAURANTEEID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = specifics.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,

                //isOwnedByCustomer = (bool)specifics.ISOWNEDBYCUSTOMER,
                institutionName = specifics.INSTITUTIONNAME,
                guarantorAddress = specifics.GUARANTORADDRESS,
                //    guarantorReferenceNumber = specifics.GUARANTORREFERENCENUMBER,
                guaranteeValue = specifics.GUARANTEEVALUE,
                cStartDate = specifics.STARTDATE,
                endDate = specifics.ENDDATE,
                remark = specifics.REMARK,
                firstName = specifics.FIRSTNAME,
                middleName = specifics.MIDDLENAME,
                lastName = specifics.LASTNAME,
                bvn = specifics.BVN,
                rcNumber = specifics.RCNUMBER,
                phoneNumber1 = specifics.PHONENUMBER1,
                phoneNumber2 = specifics.PHONENUMBER2,
                emailAddress = specifics.EMAILADDRESS,
                relationship = specifics.RELATIONSHIP,
                relationshipDuration = specifics.RELATIONSHIPDURATION,
                taxNumber = specifics.TAXNUMBER
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }



        private void UpdateImmovablePropertyCollateral(CollateralViewModel entity)
        {
            
                var collaterals = context.TBL_COLLATERAL_IMMOVE_PROPERTY.ToList();
                var collateral = collaterals.FirstOrDefault(x => x.COLLATERALCUSTOMERID == entity.collateralId);
                TBL_COLLATERAL_IMMOVE_PROPERTY imCollateral = new TBL_COLLATERAL_IMMOVE_PROPERTY();
                if (collateral == null)
                {
                    imCollateral.PROPERTYNAME = entity.propertyName;
                    imCollateral.CITYID = (int)entity.cityId;
                    imCollateral.COUNTRYID = (short)entity.countryId;
                    imCollateral.CONSTRUCTIONDATE = entity.constructionDate;
                    imCollateral.PROPERTYADDRESS = entity.propertyAddress;
                    imCollateral.DATEOFACQUISITION = entity.dateOfAcquisition;
                    imCollateral.LASTVALUATIONDATE = entity.lastValuationDate;
                    //imCollateral.NEXTVALUATIONDATE = entity.nextValuationDate;
                    imCollateral.VALUERID = entity.valuerId;
                    imCollateral.VALUERREFERENCENUMBER = entity.valuerReferenceNumber;
                    imCollateral.PROPERTYVALUEBASETYPEID = entity.propertyValueBaseTypeId;
                    imCollateral.OPENMARKETVALUE = entity.openMarketValue;

                    // imCollateral.COLLATERALVALUE = (decimal)entity.collateralValue;
                    imCollateral.COLLATERALCUSTOMERID = entity.collateralId;
                    imCollateral.FORCEDSALEVALUE = entity.forcedSaleValue;
                    imCollateral.STAMPTOCOVER = entity.stampToCover.ToString();
                    //imCollateral.VALUATIONSOURCE = entity.valuationSource;
                    //imCollateral.ORIGINALVALUE = entity.originalValue;

                    //imCollateral.AVAILABLEVALUE = entity.availableValue;

                    imCollateral.SECURITYVALUE = entity.securityValue;
                    imCollateral.COLLATERALUSABLEAMOUNT = entity.collateralUsableAmount;
                    imCollateral.REMARK = entity.remark;
                    imCollateral.NEARESTLANDMARK = entity.nearestLandMark;
                    imCollateral.NEARESTBUSSTOP = entity.nearestBusStop;
                    imCollateral.LONGITUDE = entity.longitude;
                    imCollateral.LATITUDE = entity.latitude;
                    imCollateral.PERFECTIONSTATUSID = (byte)entity.perfectionStatusId;
                    imCollateral.PERFECTIONSTATUSREASON = entity.perfectionStatusReason;
                    imCollateral.VALUATIONAMOUNT = entity.valuationAmount;
                    imCollateral.ISRESIDENTIAL = entity.isResidential;
                    imCollateral.ISOWNEROCCUPIED = entity.isOwnerOccupied;
                    imCollateral.ISASSETPLEDGEDBYTHRIDPARTY = entity.isAssetPledgedByThirdParty;
                    imCollateral.THRIDPARTYNAME = entity.thirdPartyName;
                    imCollateral.ISASSETMANAGEDBYTRUSTEE = entity.isAssetManagedByTrustee;
                    imCollateral.TRUSTEENAME = entity.trusteeName;
                    imCollateral.STATEID = entity.stateId;
                    imCollateral.LOCALGOVERNMENTID = entity.localGovernmentId;
                    imCollateral.BANKSHAREOFCOLLATERAL = entity.bankShareOfCollateral;
                    imCollateral.ESTIMATEDVALUE = entity.estimatedValue;
                    imCollateral.COLLATERALCUSTOMERID = entity.collateralId;
                    context.TBL_COLLATERAL_IMMOVE_PROPERTY.Add(imCollateral);
                    if (context.SaveChanges() != 0)
                    {
                        var collateralMain = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == imCollateral.COLLATERALCUSTOMERID);
                        NotifyForCollateralStatusUpdate(collateralMain, entity.perfectionStatusId);
                        NotifyForCollateralRevaluation(collateralMain, entity.lastValuationDate, valuationCycle: entity.valuationCycle);
                        NotifyForCollateralVisitation(collateralMain);
                    }

                    return;
                }
                if ((byte)entity.perfectionStatusId != collateral.PERFECTIONSTATUSID)
                {
                    var collateralMain = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID);
                    NotifyForCollateralStatusUpdate(collateralMain, entity.perfectionStatusId);
                }
                if (entity.lastValuationDate != collateral.LASTVALUATIONDATE)
                {
                    var collateralMain = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID);
                    NotifyForCollateralRevaluation(collateralMain, entity.lastValuationDate, valuationCycle: entity.valuationCycle);
                    NotifyForCollateralVisitation(collateralMain);
                }
                collateral.PROPERTYNAME = entity.propertyName;
                collateral.CITYID = (int)entity.cityId;
                collateral.COUNTRYID = (short)entity.countryId;
                collateral.CONSTRUCTIONDATE = entity.constructionDate;
                collateral.PROPERTYADDRESS = entity.propertyAddress;
                collateral.DATEOFACQUISITION = entity.dateOfAcquisition;
                collateral.LASTVALUATIONDATE = entity.lastValuationDate;
                //collateral.NEXTVALUATIONDATE = entity.nextValuationDate;
                collateral.VALUERID = entity.valuerId;
                collateral.VALUERREFERENCENUMBER = entity.valuerReferenceNumber;
                collateral.PROPERTYVALUEBASETYPEID = entity.propertyValueBaseTypeId;
                collateral.OPENMARKETVALUE = entity.openMarketValue;

                // collateral.COLLATERALVALUE = (decimal)entity.collateralValue;

                collateral.FORCEDSALEVALUE = entity.forcedSaleValue;
                collateral.STAMPTOCOVER = entity.stampToCover.ToString();
                //collateral.VALUATIONSOURCE = entity.valuationSource;
                //collateral.ORIGINALVALUE = entity.originalValue;

                //collateral.AVAILABLEVALUE = entity.availableValue;

                collateral.SECURITYVALUE = entity.securityValue;
                collateral.COLLATERALUSABLEAMOUNT = entity.collateralUsableAmount;
                collateral.REMARK = entity.remark;
                collateral.NEARESTLANDMARK = entity.nearestLandMark;
                collateral.NEARESTBUSSTOP = entity.nearestBusStop;
                collateral.LONGITUDE = entity.longitude;
                collateral.LATITUDE = entity.latitude;
                collateral.PERFECTIONSTATUSID = (byte)entity.perfectionStatusId;
                collateral.PERFECTIONSTATUSREASON = entity.perfectionStatusReason;
                collateral.VALUATIONAMOUNT = entity.valuationAmount;
                collateral.ISRESIDENTIAL = entity.isResidential;
                collateral.ISOWNEROCCUPIED = entity.isOwnerOccupied;
                collateral.ISASSETPLEDGEDBYTHRIDPARTY = entity.isAssetPledgedByThirdParty;
                collateral.THRIDPARTYNAME = entity.thirdPartyName;
                collateral.ISASSETMANAGEDBYTRUSTEE = entity.isAssetManagedByTrustee;
                collateral.TRUSTEENAME = entity.trusteeName;
                collateral.STATEID = entity.stateId;
                collateral.LOCALGOVERNMENTID = entity.localGovernmentId;
                collateral.BANKSHAREOFCOLLATERAL = entity.bankShareOfCollateral;
                collateral.ESTIMATEDVALUE = entity.estimatedValue;
            
        }

        private CollateralViewModel GetCollateralImmovableProperty(int collateralId)
        {
            var details = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(x => x.COLLATERALCUSTOMERID == collateralId).Select(x => new CollateralViewModel
            {
                collateralId = x.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Where(c => c.COLLATERALCUSTOMERID == collateralId).FirstOrDefault().COLLATERALSUBTYPEID,
                revaluationDuration = (from a in context.TBL_COLLATERAL_CUSTOMER join b in context.TBL_COLLATERAL_TYPE_SUB on a.COLLATERALSUBTYPEID equals b.COLLATERALSUBTYPEID where a.COLLATERALCUSTOMERID == collateralId select b.REVALUATIONDURATION).FirstOrDefault(),
                collateralPropertyId = x.COLLATERALPROPERTYID,
                collateralCustomerId = x.COLLATERALCUSTOMERID,
                propertyName = x.PROPERTYNAME,
                cityId = x.CITYID,
                cityName = x.TBL_CITY.CITYNAME,
                countryId = x.COUNTRYID,
                countryName = context.TBL_COUNTRY.Where(q => q.COUNTRYID == x.COUNTRYID).Select(r => r.NAME).FirstOrDefault(),
                constructionDate = x.CONSTRUCTIONDATE,
                propertyAddress = x.PROPERTYADDRESS,
                dateOfAcquisition = x.DATEOFACQUISITION,
                lastValuationDate = x.LASTVALUATIONDATE,
                //nextValuationDate = x.NEXTVALUATIONDATE,
                valuerId = x.VALUERID,
                collateralValuer = context.TBL_ACCREDITEDCONSULTANT.Where(t => t.ACCREDITEDCONSULTANTID == x.VALUERID).Select(t => t.FIRMNAME).FirstOrDefault(),
                valuerReferenceNumber = x.VALUERREFERENCENUMBER,
                propertyValueBaseTypeId = x.PROPERTYVALUEBASETYPEID,
                propertyValueBaseTypeName = context.TBL_COLLATERAL_VALUEBASE_TYPE.Where(t => t.COLLATERALVALUEBASETYPEID == x.PROPERTYVALUEBASETYPEID).Select(q => q.VALUEBASETYPENAME).FirstOrDefault(),
                openMarketValue = (decimal)x.OPENMARKETVALUE,
                //collateralValue = x.COLLATERALVALUE,
                forcedSaleValue = x.FORCEDSALEVALUE,
                stampToCover = x.STAMPTOCOVER,
                estimatedValue = x.ESTIMATEDVALUE,

                //valuationSource = x.VALUATIONSOURCE,
                //originalValue = x.ORIGINALVALUE,
                //availableValue = x.AVAILABLEVALUE,

                securityValue = (decimal)x.SECURITYVALUE,
                collateralUsableAmount = x.COLLATERALUSABLEAMOUNT,
                remark = x.REMARK,
                nearestLandMark = x.NEARESTLANDMARK,
                nearestBusStop = x.NEARESTBUSSTOP,
                longitude = x.LONGITUDE,
                latitude = x.LATITUDE,
                perfectionStatusId = x.PERFECTIONSTATUSID,
                perfectionStatusName = context.TBL_COLLATERAL_PERFECTN_STAT.Where(l => l.PERFECTIONSTATUSID == x.PERFECTIONSTATUSID).Select(j => j.PERFECTIONSTATUSNAME).FirstOrDefault(),

                //perfectionStatusReason = x.PERFECTIONSTATUSREASON,
                valuationAmount = x.VALUATIONAMOUNT,
                isResidential = x.ISRESIDENTIAL,
                isOwnerOccupied = x.ISOWNEROCCUPIED,
                isAssetPledgedByThirdParty = x.ISASSETPLEDGEDBYTHRIDPARTY,
                thirdPartyName = x.THRIDPARTYNAME,
                isAssetManagedByTrustee = x.ISASSETMANAGEDBYTRUSTEE,
                trusteeName = x.TRUSTEENAME,
                stateName = x.TBL_STATE.STATENAME,
                stateId = x.STATEID,
                localGovtName = x.TBL_LOCALGOVERNMENT.NAME,
                localGovernmentId = x.LOCALGOVERNMENTID,
                bankShareOfCollateral = x.BANKSHAREOFCOLLATERAL,
                propertyBaseType = context.TBL_COLLATERAL_VALUEBASE_TYPE.Where(t=>t.COLLATERALVALUEBASETYPEID == x.PROPERTYVALUEBASETYPEID).FirstOrDefault().VALUEBASETYPENAME,
                description = x.PROPERTYNAME,
                valuationDate = x.LASTVALUATIONDATE,
                lastValuationAmount = x.VALUATIONAMOUNT,
                requestReason = x.PERFECTIONSTATUSREASON,
                referenceNumber = x.VALUERREFERENCENUMBER,
                marketPrice = (decimal)x.OPENMARKETVALUE,
                perfectionStatusReason = x.PERFECTIONSTATUSID.ToString(),

                valuerName = x.VALUERNAME,
                valuerAccountNumber = x.VALUERACCOUNTNUMBER,

            }).FirstOrDefault();
            details = GetCollateralInsurancePolicy(details);
            return details;
        }
        // marketableSecurities collateral

        private void AddTempMarketableSecuritiesCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainMarket = (from x in context.TBL_COLLATERAL_MKT_SECURITY
                                  where x.COLLATERALCUSTOMERID == collateralId
                                  select (x)).FirstOrDefault();

                if (mainMarket != null)
                {
                    mainMarket.COLLATERALCUSTOMERID = entity.collateralCustomerId;
                    mainMarket.DEALAMOUNT = entity.dealAmount;
                    mainMarket.BANKPURCHASEDFROM = entity.bank;
                    mainMarket.FUNDNAME = entity.fundName;
                    mainMarket.EFFECTIVEDATE = entity.effectiveDate;
                    mainMarket.INTERESTPAYMENTFREQUENCY = entity.interestPaymentFrequency;
                    mainMarket.ISSUERNAME = entity.issuerName;
                    mainMarket.ISSUERREFERENCENUMBER = entity.issuerReferenceNumber;
                    mainMarket.LIENUSABLEAMOUNT = entity.lienUsableAmount;
                    mainMarket.MATURITYDATE = entity.maturityDate;
                    mainMarket.NUMBEROFUNITS = entity.numberOfUnits;
                    mainMarket.PERCENTAGEINTEREST = entity.percentageInterest;
                    mainMarket.RATING = entity.rating;
                    mainMarket.REMARK = entity.remark;
                    mainMarket.SECURITYTYPE = entity.securityType;
                    mainMarket.SECURITYVALUE = (decimal)entity.securityValue;
                    mainMarket.UNITVALUE = entity.unitValue;
                    comment = $"Market security collateral type has been update through loan application by {entity.createdBy} staffid";
                }
                else
                {
                    context.TBL_COLLATERAL_MKT_SECURITY.Add(new TBL_COLLATERAL_MKT_SECURITY
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        DEALAMOUNT = entity.dealAmount,
                        BANKPURCHASEDFROM = entity.bank,
                        FUNDNAME = entity.fundName,
                        EFFECTIVEDATE = entity.effectiveDate,
                        INTERESTPAYMENTFREQUENCY = entity.interestPaymentFrequency,
                        ISSUERNAME = entity.issuerName,
                        ISSUERREFERENCENUMBER = entity.issuerReferenceNumber,
                        LIENUSABLEAMOUNT = entity.lienUsableAmount,
                        MATURITYDATE = entity.maturityDate,
                        NUMBEROFUNITS = entity.numberOfUnits,
                        PERCENTAGEINTEREST = entity.percentageInterest,
                        RATING = entity.rating,
                        REMARK = entity.remark,
                        SECURITYTYPE = entity.securityType,
                        SECURITYVALUE = (decimal)entity.securityValue,
                        UNITVALUE = entity.unitValue,
                    });
                    comment = $"New market security collateral type has been update through loan application by {entity.createdBy} staffid";
                }
            }

            else
            {
                context.TBL_TEMP_COLLATERAL_MKT_SEC.Add(new TBL_TEMP_COLLATERAL_MKT_SEC
                {

                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    SECURITYTYPE = entity.securityType,
                    DEALREFERENCENUMBER = "aa",
                    EFFECTIVEDATE = entity.effectiveDate,
                    MATURITYDATE = entity.maturityDate,
                    DEALAMOUNT = entity.dealAmount,
                    SECURITYVALUE = (decimal)entity.securityValue,
                    LIENUSABLEAMOUNT = entity.lienUsableAmount,
                    ISSUERNAME = entity.issuerName,
                    ISSUERREFERENCENUMBER = entity.issuerReferenceNumber,
                    UNITVALUE = entity.unitValue,
                    NUMBEROFUNITS = entity.numberOfUnits,
                    RATING = entity.rating,
                    PERCENTAGEINTEREST = entity.percentageInterest,
                    INTERESTPAYMENTFREQUENCY = entity.interestPaymentFrequency,
                    REMARK = entity.remark,
                    FUNDNAME = entity.fundName,
                    BANKPURCHASEDFROM = entity.bank,
                });
                comment = $"New market security collateral type has been created by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }

        }

        private void UpdateMarketableSecuritiesCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_MKT_SECURITY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_MKT_SECURITY()
                {
                    SECURITYTYPE = entity.securityType,
                    //    DEALREFERENCENUMBER = entity.dealReferenceNumber,
                    EFFECTIVEDATE = entity.effectiveDate,
                    MATURITYDATE = entity.maturityDate,
                    DEALAMOUNT = entity.dealAmount,
                    SECURITYVALUE = (decimal)entity.securityValue,
                    LIENUSABLEAMOUNT = entity.lienUsableAmount,
                    ISSUERNAME = entity.issuerName,
                    ISSUERREFERENCENUMBER = entity.issuerReferenceNumber,
                    UNITVALUE = entity.unitValue,
                    NUMBEROFUNITS = entity.numberOfUnits,
                    RATING = entity.rating,
                    PERCENTAGEINTEREST = entity.percentageInterest,
                    INTERESTPAYMENTFREQUENCY = entity.interestPaymentFrequency,
                    REMARK = entity.remark,
                    FUNDNAME = entity.fundName,
                    BANKPURCHASEDFROM = entity.bank,
                    COLLATERALCUSTOMERID = entity.collateralId
                };
                context.TBL_COLLATERAL_MKT_SECURITY.Add(collateral);
                //var saved = context.SaveChanges() > 0;
                return;
            }

            collateral.SECURITYTYPE = entity.securityType;
            //    collateral.DEALREFERENCENUMBER = entity.dealReferenceNumber;
            collateral.EFFECTIVEDATE = entity.effectiveDate;
            collateral.MATURITYDATE = entity.maturityDate;
            collateral.DEALAMOUNT = entity.dealAmount;
            collateral.SECURITYVALUE = (decimal)entity.securityValue;
            collateral.LIENUSABLEAMOUNT = entity.lienUsableAmount;
            collateral.ISSUERNAME = entity.issuerName;
            collateral.ISSUERREFERENCENUMBER = entity.issuerReferenceNumber;
            collateral.UNITVALUE = entity.unitValue;
            collateral.NUMBEROFUNITS = entity.numberOfUnits;
            collateral.RATING = entity.rating;
            collateral.PERCENTAGEINTEREST = entity.percentageInterest;
            collateral.INTERESTPAYMENTFREQUENCY = entity.interestPaymentFrequency;
            collateral.REMARK = entity.remark;
            collateral.FUNDNAME = entity.fundName;
            collateral.BANKPURCHASEDFROM = entity.bank;
        }

        private CollateralViewModel GetCollateralMarketableSecurities(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_MKT_SECURITY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            if (specifics == null) return null;
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                collateralMarketableSecurityId = specifics.COLLATERALMARKETABLESECURITYID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                securityType = specifics.SECURITYTYPE,
                //   dealReferenceNumber = specifics.DEALREFERENCENUMBER,
                effectiveDate = specifics.EFFECTIVEDATE,
                maturityDate = specifics.MATURITYDATE,
                dealAmount = specifics.DEALAMOUNT,
                securityValue = specifics.SECURITYVALUE,
                lienUsableAmount = specifics.LIENUSABLEAMOUNT,
                issuerName = specifics.ISSUERNAME,
                issuerReferenceNumber = specifics.ISSUERREFERENCENUMBER,
                unitValue = specifics.UNITVALUE,
                numberOfUnits = specifics.NUMBEROFUNITS,
                rating = specifics.RATING,
                percentageInterest = specifics.PERCENTAGEINTEREST,
                interestPaymentFrequency = specifics.INTERESTPAYMENTFREQUENCY,
                remark = specifics.REMARK,
                fundName = specifics.FUNDNAME,
                bank = specifics.BANKPURCHASEDFROM,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }


        // policy collateral

        private void AddTempPolicyCollateral(int collateralId, CollateralViewModel entity)
        {
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                //try
                //{
                var mainPolicy = (from x in context.TBL_COLLATERAL_POLICY
                                  where x.COLLATERALCUSTOMERID == collateralId
                                  select (x)).FirstOrDefault();
                //}
                //catch(Exception ex)
                //{
                //    throw ex;
                //}


                if (mainPolicy != null)
                {

                    mainPolicy.COLLATERALCUSTOMERID = entity.collateralCustomerId;
                    mainPolicy.REMARK = entity.remark;
                    mainPolicy.ASSIGNDATE = entity.assignDate;
                    mainPolicy.INSURANCECOMPANYNAME = entity.insuranceCompanyName;
                    mainPolicy.INSURERADDRESS = entity.insurerAddress;
                    mainPolicy.INSURERDETAILS = entity.insurerDetails;
                    mainPolicy.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
                    mainPolicy.INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber;
                    mainPolicy.POLICYAMOUNT = entity.policyAmount;
                    mainPolicy.POLICYRENEWALDATE = entity.policyRenewalDate;
                    mainPolicy.POLICYSTARTDATE = entity.policyStartDate;
                    mainPolicy.PREMIUMAMOUNT = entity.premiumAmount;
                    mainPolicy.RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId;
                    mainPolicy.INSURANCETYPEID = entity.insuranceTypeId;
                    //mainPolicy.INSURANCETYPE = entity.insuranceType;

                    comment = $" Policy collateral type has been update through loan application by {entity.createdBy} staffid";
                }
                else
                {
                    context.TBL_COLLATERAL_POLICY.Add(new TBL_COLLATERAL_POLICY
                    {
                        COLLATERALCUSTOMERID = collateralId,
                        REMARK = entity.remark,
                        ASSIGNDATE = entity.assignDate,
                        INSURANCECOMPANYNAME = entity.insuranceCompanyName,
                        INSURERADDRESS = entity.insurerAddress,
                        INSURERDETAILS = entity.insurerDetails,
                        ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                        INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber,
                        POLICYAMOUNT = entity.policyAmount,
                        POLICYRENEWALDATE = entity.policyRenewalDate,
                        POLICYSTARTDATE = entity.policyStartDate,
                        PREMIUMAMOUNT = entity.premiumAmount,
                        RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId,
                        INSURANCETYPEID = entity.insuranceTypeId,
                        //INSURANCETYPE = entity.insuranceType,

                    });

                    comment = $"New Policy collateral type has been created through loan application by {entity.createdBy} staffid";
                }
            }
            else
            {
                context.TBL_TEMP_COLLATERAL_POLICY.Add(new TBL_TEMP_COLLATERAL_POLICY
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                    INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber,
                    PREMIUMAMOUNT = (decimal)entity.premiumAmount,
                    POLICYAMOUNT = entity.policyAmount,
                    INSURANCECOMPANYNAME = entity.insuranceCompanyName,
                    INSURERADDRESS = entity.insurerAddress,
                    POLICYSTARTDATE = entity.policyStartDate,
                    ASSIGNDATE = entity.assignDate,
                    RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId,
                    INSURERDETAILS = entity.insurerDetails,
                    POLICYRENEWALDATE = entity.policyRenewalDate,
                    REMARK = entity.remark,
                    INSURANCETYPE = entity.policyinsuranceType,
                });

                comment = $"New temp Policy collateral type has been created through loan application by {entity.createdBy} staffid";

                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }



        }

        private void UpdatePolicyCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_POLICY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_POLICY()
                {
                    ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                    // INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber,
                    PREMIUMAMOUNT = (decimal)entity.premiumAmount,
                    POLICYAMOUNT = entity.policyAmount,
                    INSURANCECOMPANYNAME = entity.insuranceCompanyName,
                    INSURERADDRESS = entity.insurerAddress,
                    POLICYSTARTDATE = entity.policyStartDate,
                    ASSIGNDATE = entity.assignDate,
                    RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId,
                    INSURERDETAILS = entity.insurerDetails,
                    POLICYRENEWALDATE = entity.policyRenewalDate,
                    REMARK = entity.remark,
                    INSURANCETYPEID = entity.insuranceTypeId,
                    COLLATERALCUSTOMERID = entity.collateralId,
                    //INSURANCETYPE = entity.insuranceType
                };
                context.TBL_COLLATERAL_POLICY.Add(collateral);
                //var saved = context.SaveChanges() > 0;
                return;
            }

            collateral.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
            // collateral.INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber;
            collateral.PREMIUMAMOUNT = (decimal)entity.premiumAmount;
            collateral.POLICYAMOUNT = entity.policyAmount;
            collateral.INSURANCECOMPANYNAME = entity.insuranceCompanyName;
            collateral.INSURERADDRESS = entity.insurerAddress;
            collateral.POLICYSTARTDATE = entity.policyStartDate;
            collateral.ASSIGNDATE = entity.assignDate;
            collateral.RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId;
            collateral.INSURERDETAILS = entity.insurerDetails;
            collateral.POLICYRENEWALDATE = entity.policyRenewalDate;
            collateral.REMARK = entity.remark;
            collateral.INSURANCETYPEID = entity.insuranceTypeId;
            //collateral.INSURANCETYPE = entity.insuranceType;
        }

        private CollateralViewModel GetCollateralPolicy(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_POLICY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Find(collateralId).COLLATERALSUBTYPEID,
                collateralInsurancePolicyId = specifics.COLLATERALINSURANCEPOLICYID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                insurancePolicyNumber = specifics.INSURANCEPOLICYNUMBER,
                premiumAmount = specifics.PREMIUMAMOUNT,
                policyAmount = specifics.POLICYAMOUNT,
                insuranceCompanyName = specifics.INSURANCECOMPANYNAME,
                insurerAddress = specifics.INSURERADDRESS,
                policyStartDate = specifics.POLICYSTARTDATE,
                assignDate = specifics.ASSIGNDATE,
                renewalFrequencyTypeId = specifics.RENEWALFREQUENCYTYPEID,
                renewalFrequencyTypeName = context.TBL_FREQUENCY_TYPE.Where(w => w.FREQUENCYTYPEID == specifics.RENEWALFREQUENCYTYPEID).Select(u => u.MODE).FirstOrDefault(),
                insurerDetails = specifics.INSURERDETAILS,
                policyRenewalDate = specifics.POLICYRENEWALDATE,
                remark = specifics.REMARK,
                insuranceTypeId = specifics.INSURANCETYPEID,
                //policyinsuranceType = specifics.INSURANCETYPE,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // LOAN COLLATERAL MAPPING


        //Property Visitation Infor

        public int AddPropertyVistation(CollateralDocumentViewModel entity)
        {
            var data = context.TBL_COLLATERAL_VISITATION.Add(new TBL_COLLATERAL_VISITATION
            {
                COLLATERALCUSTOMERID = entity.collateralCustomerId,
                VISITATIONDATE = entity.lastVisitaionDate,
                NEXTVISITATIONDATE = entity.nextVisitationDate,
                REMARK = entity.visitationRemark,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false,

            });

            if (data != null)
            {
                context.TBL_COLLATERAL_VISITATION.Add(data);
                int reponse = context.SaveChanges();
                if (reponse == 1) { return data.COLLATERALVISITATIONID; }
            }

            return 0;
        }

        public List<CollateralDocumentViewModel> GetPropertyVistation(int collateralId)
        {
            List<CollateralDocumentViewModel> response = new List<CollateralDocumentViewModel>();
            var summary = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateralId)?.COLLATERALSUMMARY;
            var specifics = (from x in context.TBL_COLLATERAL_VISITATION
                             where x.COLLATERALCUSTOMERID == collateralId

                             select new CollateralDocumentViewModel
                             {
                                 collateralId = x.COLLATERALCUSTOMERID,
                                 visitationRemark = x.REMARK,
                                 lastVisitaionDate = x.VISITATIONDATE,
                                 nextVisitationDate = x.NEXTVISITATIONDATE,
                                 CollateralVisitationID = x.COLLATERALVISITATIONID,
                                 collateralCustomerId = x.COLLATERALCUSTOMERID,
                                 createdBy = x.CREATEDBY,

                             }).ToList();
            foreach (var file in specifics)
            {
                var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == file.createdBy);
                var staffName = staff.FIRSTNAME + " " + staff.MIDDLENAME + " " + staff.LASTNAME;
                var uploadedBy = staff.TBL_STAFF_ROLE.STAFFROLENAME + ", " + staffName;
                CollateralDocumentViewModel list = new CollateralDocumentViewModel();
                var data = (from image in documentContext.TBL_DOC_COLLATERAL_VISITATION
                            where image.COLLATERALVISITATIONID == file.CollateralVisitationID
                            && image.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved
                            select image).FirstOrDefault();
                if (data != null)
                {
                    list.collateralId = file.collateralCustomerId;
                    list.visitationRemark = file.visitationRemark;
                    list.lastVisitaionDate = file.lastVisitaionDate;
                    list.nextVisitationDate = file.nextVisitationDate;
                    list.CollateralVisitationID = file.CollateralVisitationID;
                    list.collateralCustomerId = file.collateralCustomerId;
                    list.fileData = data.FILEDATA;
                    list.fileExtension = data.FILEEXTENSION;
                    list.fileName = data.FILENAME;
                    list.documentId = data.DOCUMENTID;
                    list.doneBy = uploadedBy;
                    list.collateralSummary = summary;

                    response.Add(list);
                }

            }
            return response.ToList();
        }

        public List<CollateralDocumentViewModel> GetTempPropertyVistation(int collateralId)
        {
            List<CollateralDocumentViewModel> response = new List<CollateralDocumentViewModel>();
            var summary = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateralId)?.COLLATERALSUMMARY;

            var specifics = (from x in context.TBL_COLLATERAL_VISITATION
                             where x.COLLATERALCUSTOMERID == collateralId

                             select new CollateralDocumentViewModel
                             {
                                 collateralId = x.COLLATERALCUSTOMERID,
                                 visitationRemark = x.REMARK,
                                 lastVisitaionDate = x.VISITATIONDATE,
                                 nextVisitationDate = x.NEXTVISITATIONDATE,
                                 CollateralVisitationID = x.COLLATERALVISITATIONID,
                                 collateralCustomerId = x.COLLATERALCUSTOMERID,
                                 createdBy = x.CREATEDBY,

                             }).ToList();
            foreach (var file in specifics)
            {
                var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == file.createdBy);
                var staffName = staff.FIRSTNAME + " " + staff.MIDDLENAME + " " + staff.LASTNAME;
                var uploadedBy = staff.TBL_STAFF_ROLE.STAFFROLENAME + " " + staffName;
                CollateralDocumentViewModel list = new CollateralDocumentViewModel();
                var data = (from image in documentContext.TBL_DOC_COLLATERAL_VISITATION
                            where image.COLLATERALVISITATIONID == file.CollateralVisitationID
                            && image.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                            select image).FirstOrDefault();
                if (data != null)
                {
                    list.collateralId = file.collateralCustomerId;
                    list.visitationRemark = file.visitationRemark;
                    list.lastVisitaionDate = file.lastVisitaionDate;
                    list.nextVisitationDate = file.nextVisitationDate;
                    list.CollateralVisitationID = file.CollateralVisitationID;
                    list.collateralCustomerId = file.collateralCustomerId;
                    list.fileData = data.FILEDATA;
                    list.fileExtension = data.FILEEXTENSION;
                    list.fileName = data.FILENAME;
                    list.documentId = data.DOCUMENTID;
                    list.doneBy = uploadedBy;
                    list.collateralSummary = summary;

                    response.Add(list);
                }

            }
            return response.ToList();
        }

        public IEnumerable<LoanApplicationCollateralViewModel> MapApplicationCollateral(ApplicationCollateralMapping entity)
        {

            var proposed = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == entity.collateralId && o.LOANAPPLICATIONDETAILID == entity.loanApplicationDetailId && o.DELETED == false).FirstOrDefault();
            if (proposed != null)
            {
                proposed.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;

                //var mapping = new TBL_LOAN_COLLATERAL_MAPPING()
                //{
                //    COLLATERALCUSTOMERID = proposed.COLLATERALCUSTOMERID,
                //    CREATEDBY = entity.createdBy,
                //    DATETIMECREATED = genSetup.GetApplicationDate(),
                //    loan
                //};
            }

            context.SaveChanges();

            var mapped = context.TBL_LOAN_APPLICATION_COLLATERL.Where(c => c.LOANAPPLICATIONID == entity.applicationId).Select(c => new LoanApplicationCollateralViewModel
            {
                loanAppCollateralId = c.LOANAPPCOLLATERALID,
                applicationReferenceNumber = c.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                collateralValue = c.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                collateralCustomerId = c.COLLATERALCUSTOMERID,
                collateralReferenceNumber = c.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                collateralType = c.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                loanApplicationId = c.LOANAPPLICATIONID,
                loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                approvalStatusId = c.APPROVALSTATUSID,
                haircut = c.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                customerId = c.TBL_COLLATERAL_CUSTOMER.CUSTOMERID.Value,
            }).OrderByDescending(x => x.loanAppCollateralId);
            return mapped;
        }

        public bool IsCollateralMapped(ApplicationCollateralMapping entity)
        {
            return context.TBL_LOAN_APPLICATION_COLLATERL.Where(c => c.COLLATERALCUSTOMERID == entity.collateralId && c.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).Any();
        }
        public IEnumerable<LoanApplicationCollateralViewModel> UnmapApplicationCollateral(ApplicationCollateralMapping entity)
        {
            //var proposed = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.LOANAPPCOLLATERALID == entity.loanAppCollateralId && o.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved).FirstOrDefault();
            //var mapped = context.TBL_LOAN_COLLATERAL_MAPPING.Where(o => o.LOANAPPCOLLATERALID == entity.loanAppCollateralId).FirstOrDefault();
            //if (proposed != null && mapped != null)
            //{
            //    proposed.DELETED = true;
            //    mapped.ISRELEASED = true;
            //    context.Entry(proposed).State = EntityState.Modified;
            //    context.Entry(mapped).State = EntityState.Modified;
            //    //proposed.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing;
            //    context.SaveChanges();
            //}



            var mapped2 = context.TBL_LOAN_APPLICATION_COLLATERL.Where(c => c.LOANAPPLICATIONID == entity.applicationId).Select(c => new LoanApplicationCollateralViewModel
            {
                loanAppCollateralId = c.LOANAPPCOLLATERALID,
                applicationReferenceNumber = c.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                collateralValue = c.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                collateralCustomerId = c.COLLATERALCUSTOMERID,
                collateralReferenceNumber = c.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                collateralType = c.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                loanApplicationId = c.LOANAPPLICATIONID,
                loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                approvalStatusId = c.APPROVALSTATUSID,
                haircut = c.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                customerId = c.TBL_COLLATERAL_CUSTOMER.CUSTOMERID.Value
            }).OrderByDescending(x => x.loanAppCollateralId);
            return mapped2;
        }

        #endregion New 

        public IEnumerable<ActiveCustomerCollateralViewModel> GetActiveCustomerCollateral(int customerId) // REFACTOR PROJECTION
        {
            // tbl_Customer --> tbl_Collateral_Customer --> tbl_Loan_Application --> tbl_Loan_Collateral_Mapping

            var collaterals = context.TBL_CUSTOMER//.Where(x => x.CustomerId == customerId)
                .Join(context.TBL_COLLATERAL_CUSTOMER, c => c.CUSTOMERID, o => o.CUSTOMERID, (c, o) => new { Customer = c, Collateral = o })
                .Join(context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == customerId), cc => cc.Collateral.CUSTOMERID, a => a.CUSTOMERID, (cc, a) => new { CustomerCollateral = cc, Application = a })
                .Join(context.TBL_LOAN_COLLATERAL_MAPPING, ca => ca.Application.LOANAPPLICATIONID, m => m.LOANID, (ca, m) => new { CollateralApplication = ca, Mapping = m })
                .Select(x => new ActiveCustomerCollateralViewModel
                {
                    customerId = x.CollateralApplication.Application.CUSTOMERID,
                    collateralCustomerId = x.Mapping.COLLATERALCUSTOMERID,
                    //currencyId = x.CollateralApplication.Application.CurrencyId,
                    //productId = x.CollateralApplication.Application.ProductId,
                    loanTypeId = x.CollateralApplication.Application.LOANAPPLICATIONTYPEID,
                    loanCollateralMappingId = x.Mapping.LOANCOLLATERALMAPPINGID,
                    //loanId = x.Mapping.LoanId,
                    loanApplicationId = x.Mapping.LOANID,
                    isReleased = x.Mapping.ISRELEASED,
                    releaseApprovalStatusId = (short)x.Mapping.RELEASEAPPROVALSTATUSID,
                    //productTypeId = x.Mapping.ProductTypeId,
                    customerCode = x.CollateralApplication.CustomerCollateral.Customer.CUSTOMERCODE,
                    firstName = x.CollateralApplication.CustomerCollateral.Customer.FIRSTNAME,
                    middleName = x.CollateralApplication.CustomerCollateral.Customer.MIDDLENAME,
                    lastName = x.CollateralApplication.CustomerCollateral.Customer.LASTNAME,
                    collateralCode = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                    collateralValue = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                    allowSharing = x.Mapping.TBL_COLLATERAL_CUSTOMER.ALLOWSHARING,
                    isLocationBased = (bool)x.Mapping.TBL_COLLATERAL_CUSTOMER.ISLOCATIONBASED,
                    valuationCycle = x.Mapping.TBL_COLLATERAL_CUSTOMER.VALUATIONCYCLE,
                    hairCut = x.Mapping.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                    collateralTypeId = x.Mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALTYPEID,
                    applicationReferenceNumber = x.CollateralApplication.Application.APPLICATIONREFERENCENUMBER,
                    applicationDate = x.CollateralApplication.Application.APPLICATIONDATE,
                    //principalAmount = x.CollateralApplication.Application.PrincipalAmount,
                    interestRate = x.CollateralApplication.Application.INTERESTRATE,
                    //exchangeRate = x.CollateralApplication.Application.ExchangeRate,
                    //tenor = x.CollateralApplication.Application.Tenor,
                    loanInformation = x.CollateralApplication.Application.LOANINFORMATION,
                    exchangeRate = x.Mapping.TBL_COLLATERAL_CUSTOMER.EXCHANGERATE,

                })
                .Where(x => x.isReleased == false)
                .Distinct();

            return collaterals;
        }

        public IEnumerable<ActiveCustomerCollateralViewModel> GetLoanCollateral(int loanId, int productTypeId)
        {
            // var l = context.TBL_LOAN.Find(loanId);

            var collaterals = (from x in context.TBL_LOAN_COLLATERAL_MAPPING
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               join ct in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                               join cs in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals cs.COLLATERALSUBTYPEID
                               where x.LOANID == loanId && x.ISRELEASED != true
                               select new ActiveCustomerCollateralViewModel
                               {
                                   collateralCustomerId = x.COLLATERALCUSTOMERID,
                                   loanSystemTypeId = x.LOANSYSTEMTYPEID,
                                   loanCollateralMappingId = x.LOANCOLLATERALMAPPINGID,
                                   loanApplicationId = x.LOANID,
                                   isReleased = x.ISRELEASED,
                                   collateralCode = c.COLLATERALCODE,
                                   collateralValue = c.COLLATERALVALUE,
                                   allowSharing = c.ALLOWSHARING,
                                   isLocationBased = (bool)c.ISLOCATIONBASED,
                                   valuationCycle = c.VALUATIONCYCLE,
                                   hairCut = c.HAIRCUT,
                                   collateralTypeId = c.COLLATERALTYPEID,
                                   exchangeRate = c.EXCHANGERATE,
                                   releaseApprovalStatusId = (x.RELEASEAPPROVALSTATUSID == null) ? 0 : x.RELEASEAPPROVALSTATUSID,
                               }).ToList();

            var test = collaterals.ToList();

            return collaterals;
        }

        public bool ReleaseCollateral(int collateralMappingId, int staffId, GeneralEntity model)
        {
            var mapping = context.TBL_LOAN_COLLATERAL_MAPPING.Find(collateralMappingId);
            mapping.RELEASEAPPROVALSTATUSID = (short)ApprovalStatusEnum.Processing;
            context.Entry(mapping).State = EntityState.Modified;
            context.SaveChanges();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseAction,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Collateral Release Action '{ mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            workflow.StaffId = model.createdBy;
            workflow.CompanyId = model.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralMappingId;
            workflow.Comment = "Request for collateral release";
            workflow.OperationId = (int)OperationsEnum.CollateralRelease;
            workflow.DeferredExecution = true;
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

            return context.SaveChanges() > 0;
        }

        public bool ApproveCollateralRelease(ApprovalViewModel entity, int staffId, GeneralEntity user)
        {
            workflow.StaffId = user.createdBy;
            workflow.CompanyId = user.companyId;
            workflow.StatusId = (short)entity.approvalStatusId;
            workflow.TargetId = entity.targetId;
            workflow.Comment = entity.comment;
            workflow.OperationId = (int)OperationsEnum.CollateralRelease;
            workflow.DeferredExecution = true;
            workflow.LogActivity();

            var mapping = context.TBL_LOAN_COLLATERAL_MAPPING.Find(entity.targetId);

            if (workflow.NewState == (int)ApprovalState.Ended && workflow.StatusId == (int)ApprovalStatusEnum.Approved)
            {
                var mainCollateral = (from x in context.TBL_COLLATERAL_CUSTOMER
                                      join t in context.TBL_COLLATERAL_TYPE on x.COLLATERALTYPEID equals t.COLLATERALTYPEID
                                      where x.COLLATERALCUSTOMERID == mapping.COLLATERALCUSTOMERID
                                      select new { x.COLLATERALTYPEID, x.COLLATERALCUSTOMERID, t.REQUIREINSURANCEPOLICY, t.REQUIREVISITATION, x.COLLATERALCODE, x.COLLATERALVALUE }).FirstOrDefault();


                if (mainCollateral.COLLATERALTYPEID > 0)
                {
                    var description = "Callateral lien release";
                    decimal securityValue = 0;

                    if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.CASA)
                    {
                        description = "CASA callateral lien release";
                        var collateral = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == mainCollateral.COLLATERALCUSTOMERID);
                        if (collateral != null) securityValue = collateral.SECURITYVALUE;
                    }

                    if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.FixedDeposit)
                    {
                        description = "Deposit callateral lien release";
                        var collateral = context.TBL_COLLATERAL_DEPOSIT.FirstOrDefault(x => x.COLLATERALCUSTOMERID == mainCollateral.COLLATERALCUSTOMERID);
                        if (collateral != null) securityValue = collateral.SECURITYVALUE;
                    }

                    if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.FixedDeposit ||
                        mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.CASA)
                    {
                        var existingLien = context.TBL_CASA_LIEN.FirstOrDefault(x => x.SOURCEREFERENCENUMBER == mainCollateral.COLLATERALCODE && x.LIENTYPEID == (int)LienTypeEnum.CollateralCreation);
                        if (existingLien == null) throw new SecureException("No lien has been placed");
                        string lienReferenceNumber = existingLien.LIENREFERENCENUMBER;

                        lien.ReleaseLien(new CasaLienViewModel
                        {
                            productAccountNumber = mainCollateral.COLLATERALCODE,
                            lienAmount = securityValue,
                            description = description,
                            lienTypeId = (int)LienTypeEnum.CollateralCreation,
                            lienReferenceNumber = lienReferenceNumber,
                            dateTimeCreated = DateTime.Now,
                            createdBy = user.createdBy,
                            companyId = user.companyId,
                            branchId = (short)user.userBranchId
                        });
                    }

                    mapping.RELEASEAPPROVALSTATUSID = (short)entity.approvalStatusId;
                    mapping.ISRELEASED = true;
                    context.Entry(mapping).State = EntityState.Modified;

                    // Audit Section ---------------------------
                    this.auditTrail.AddAuditTrail(new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseApproval,
                        STAFFID = user.createdBy,
                        BRANCHID = (short)user.userBranchId,
                        DETAIL = $"Collateral Release Approval '{ mapping.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE }' ",
                        IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                        URL = user.applicationUrl,
                        APPLICATIONDATE = genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    });
                    // End of Audit Section ---------------------
                }
            }

            return context.SaveChanges() > 0;
        }

        public IEnumerable<ActiveCustomerCollateralViewModel> GetPendingCustomerCollateralRelease(int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.OfferLetterApproval).ToList();

            var pending = (from x in context.TBL_LOAN_COLLATERAL_MAPPING
                           join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                           join cu in context.TBL_CUSTOMER on c.CUSTOMERID equals cu.CUSTOMERID
                           join ct in context.TBL_COLLATERAL_TYPE on c.COLLATERALTYPEID equals ct.COLLATERALTYPEID
                           join cs in context.TBL_COLLATERAL_TYPE_SUB on c.COLLATERALSUBTYPEID equals cs.COLLATERALSUBTYPEID
                           join t in context.TBL_APPROVAL_TRAIL on x.LOANCOLLATERALMAPPINGID equals t.TARGETID
                           where t.OPERATIONID == (int)OperationsEnum.CollateralRelease
                            && ids.Contains((int)t.TOAPPROVALLEVELID)
                            && x.ISRELEASED == false && x.RELEASEAPPROVALSTATUSID == (short)ApprovalStatusEnum.Processing
                           select new ActiveCustomerCollateralViewModel
                           {
                               collateralCustomerId = x.COLLATERALCUSTOMERID,
                               loanSystemTypeId = x.LOANSYSTEMTYPEID,
                               loanCollateralMappingId = x.LOANCOLLATERALMAPPINGID,
                               loanApplicationId = x.LOANID,
                               isReleased = x.ISRELEASED,
                               collateralCode = c.COLLATERALCODE,
                               collateralValue = c.COLLATERALVALUE,
                               allowSharing = c.ALLOWSHARING,
                               isLocationBased = (bool)c.ISLOCATIONBASED,
                               valuationCycle = c.VALUATIONCYCLE,
                               hairCut = c.HAIRCUT,
                               collateralTypeId = c.COLLATERALTYPEID,
                               exchangeRate = c.EXCHANGERATE,
                               releaseApprovalStatusId = (short)x.RELEASEAPPROVALSTATUSID,
                               customerCode = cu.CUSTOMERCODE,
                               firstName = cu.FIRSTNAME,
                               middleName = cu.MIDDLENAME,
                               lastName = cu.LASTNAME,
                           }).ToList();


            return pending.ToList();
        }

        public IQueryable<CollateralSearchViewModel> SearchCollateral(string searchString, int companyId)
        {
            IQueryable<CollateralSearchViewModel> result = null;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(searchString.Trim()))
            {
                result =
                    context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false && x.COMPANYID == companyId)
                    .Select(o => new CollateralSearchViewModel
                    {
                        collateralId = o.COLLATERALCUSTOMERID,
                        customerId = o.CUSTOMERID.Value,
                        collateralTypeId = o.COLLATERALSUBTYPEID,
                        collateralTypeName = o.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                        customerCode = o.TBL_CUSTOMER.CUSTOMERCODE,
                        customerName = o.TBL_CUSTOMER.FIRSTNAME + " " + o.TBL_CUSTOMER.MIDDLENAME + " " + o.TBL_CUSTOMER.LASTNAME,
                        currencyId = o.CURRENCYID,
                        currencyCode = o.TBL_CURRENCY.CURRENCYCODE,
                        collateralCode = o.COLLATERALCODE,
                        allowSharing = o.ALLOWSHARING,
                        isLocationBased = (bool)o.ISLOCATIONBASED,
                        valuationCycle = o.VALUATIONCYCLE,
                        haircut = o.HAIRCUT,
                    })
                    .Where(x =>
                       x.collateralCode.ToLower().Contains(searchString)
                    || x.collateralTypeName.ToLower().Contains(searchString)
                    || x.customerCode.ToLower().Contains(searchString)
                    || x.currencyCode.Contains(searchString)
                    || x.customerName.Contains(searchString)
                    )
                    .Take(12);
            }

            return result;
        }


        public bool AssignCollateral(ActiveCustomerCollateralViewModel entity)
        {
            var assignment = new TBL_LOAN_COLLATERAL_MAPPING
            {
                LOANID = entity.loanId,
                COLLATERALCUSTOMERID = entity.collateralCustomerId,
                LOANSYSTEMTYPEID = entity.loanSystemTypeId,
                RELEASEAPPROVALSTATUSID = 0
            };

            context.TBL_LOAN_COLLATERAL_MAPPING.Add(assignment);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralAssignmentAction,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Collateral Assignment :: LoanApplicationId:'{ assignment.LOANID }' CollateralCustomerId:'{ assignment.COLLATERALCUSTOMERID }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() > 0;
        }




        #region Collateral Customer 

        //public IEnumerable<CollateralCustomerViewModel> GetCollateralCustomer(int customerId, int companyId)
        //{
        //    var collateral = GetCollateralCustomerByCustomerId(customerId, companyId).Where(x => x.deleted == false);

        //    return collateral;
        //}

        //public async Task<bool> AddCollateralCustomer(CollateralCustomerViewModel entity)
        //{
        //    var collateral = new tbl_Collateral_Customer
        //    {
        //        CompanyId = entity.companyId,
        //        CollateralTypeId = entity.collateralTypeId,
        //        CollateralCode = entity.collateralCode,
        //        CurrencyId = entity.currencyId,
        //        AllowSharing = entity.allowSharing,
        //        IsLocationBased = entity.isLocationBased,
        //        ValuationCycle = entity.valuationCycle,
        //        HairCut = entity.hairCut,
        //        CustomerId = entity.customerId,

        //        ApprovalStatus = entity.approvalStatus,
        //        DateActedOn = entity.dateActedOn,
        //        ActedOnBy = entity.actedOnBy,
        //        CamRefNumber = entity.camRefNumber,
        //        DateTimeCreated = genSetup.GetApplicationDate().Date,
        //        CreatedBy = entity.createdBy,
        //        tbl_Collateral_Immovable_Property = AddCollateralProperty((CollateralTypeEnum)entity.collateralTypeId, entity.collateralProperty),
        //        tbl_Collateral_Deposit = AddCollateralDeposit((CollateralTypeEnum)entity.collateralTypeId, entity.collateralDeposit),
        //        tbl_Collateral_Plant_And_Equipment = AddCollateralMachineDetail((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMachineDetail),
        //        tbl_Collateral_Marketable_Security = AddCollateralMarketableSecurity((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMarketableSecurity),
        //        tbl_Collateral_Policy = AddCollateralInsurancePolicy((CollateralTypeEnum)entity.collateralTypeId, entity.collateralInsurancePolicy),
        //        tbl_Collateral_PreciousMetal = AddCollateralPreciousMetal((CollateralTypeEnum)entity.collateralTypeId, entity.collateralPreciousMetal),
        //        tbl_Collateral_Gaurantee = AddCollateralGaurantee((CollateralTypeEnum)entity.collateralTypeId, entity.collateralGaurantee),
        //        tbl_Collateral_Vehicle = AddCollateralVehicle((CollateralTypeEnum)entity.collateralTypeId, entity.collateralVehicle),
        //        tbl_Collateral_Miscellaneous = AddCollateralMiscellaneous((CollateralTypeEnum)entity.collateralTypeId, entity.collateralMiscellaneous),
        //    };

        //    context.tbl_Collateral_Customer.Add(collateral);
        //    return await context.SaveChangesAsync() != 0;
        //}

        public async Task<bool> DeleteCollateralCustomer(int colleralCustomerId, UserInfo user)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Find(colleralCustomerId);
            collateral.DELETED = true;
            collateral.DELETEDBY = user.staffId;
            collateral.DATETIMEDELETED = genSetup.GetApplicationDate();

            return await context.SaveChangesAsync() != 0;
        }

        //private List<CollateralCustomerViewModel> CollateralCustomer(int customerId, int companyId)
        //{
        //    tbl_Collateral_Type_Sub sub = new tbl_Collateral_Type_Sub();
        //    return (from c in context.tbl_Collateral_Customer
        //            join t in context.tbl_Collateral_Type on c.CollateralTypeId equals t.CollateralTypeId
        //            where c.Deleted == false && c.CompanyId == companyId && c.CustomerId == customerId
        //            select new CollateralCustomerViewModel
        //            {
        //                collateralTypeId = c.CollateralTypeId,
        //                collateralType = c.tbl_Collateral_Type.CollateralTypeName,
        //                collateralCustomerId = c.CollateralCustomerId,
        //                collateralCode = c.CollateralCode,
        //                currencyId = c.CurrencyId,
        //                currency = c.tbl_Currency.CurrencyName,
        //                allowSharing = c.AllowSharing,
        //                isLocationBased = c.IsLocationBased,
        //                valuationCycle = c.ValuationCycle,
        //                hairCut = c.HairCut,
        //                customerId = c.CustomerId,
        //                customerName = c.tbl_Customer.LastName + " " + c.tbl_Customer.FirstName,
        //                approvalStatus = c.ApprovalStatus,
        //                dateActedOn = c.DateActedOn,
        //                actedOnBy = c.ActedOnBy,
        //                camRefNumber = c.CamRefNumber,
        //                dateTimeCreated = c.DateTimeCreated,
        //                createdBy = c.CreatedBy,
        //            })
        //            .OrderByDescending(x => x.collateralCustomerId)
        //            .ToList();
        //}

        //public IEnumerable<CollateralCustomerViewModel> GetCollateralCustomerByCustomerId(int customerId, int companyId)
        //{
        //    return CollateralCustomer(customerId, companyId);
        //}

        //public async Task<bool> UpdateCollateralCustomer(int collateralCustomerId, CollateralCustomerViewModel entity)
        //{
        //    var collateral = context.tbl_Collateral_Customer.Find(collateralCustomerId);
        //    collateral.CollateralCode = entity.collateralCode;
        //    collateral.CurrencyId = entity.currencyId;
        //    collateral.AllowSharing = entity.allowSharing;
        //    collateral.IsLocationBased = entity.isLocationBased;
        //    collateral.ValuationCycle = entity.valuationCycle;
        //    collateral.HairCut = entity.hairCut;
        //    collateral.CustomerId = entity.customerId;
        //    collateral.ApprovalStatus = entity.approvalStatus;
        //    collateral.DateActedOn = entity.dateActedOn;
        //    collateral.ActedOnBy = entity.actedOnBy;
        //    collateral.CamRefNumber = entity.camRefNumber;
        //    collateral.DateTimeUpdated = entity.dateTimeCreated;
        //    collateral.LastUpdatedBy = entity.lastUpdatedBy;

        //    //TblCollateralMachineDetail collateralMachineDetail = collateral.TblCollateralMachineDetail.FirstOrDefault();

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.Property)
        //    {
        //        var collateralProperty = context.tbl_Collateral_Immovable_Property.Find(entity.collateralProperty.collateralPropertyId);

        //        collateralProperty.PropertyName = entity.collateralProperty.propertyName;
        //        collateralProperty.CityId = entity.collateralProperty.cityId;
        //        collateralProperty.CountryId = entity.collateralProperty.countryId;
        //        collateralProperty.PropertyAddress = entity.collateralProperty.propertyAddress;
        //        collateralProperty.ConstructionDate = entity.collateralProperty.constructionDate;
        //        collateralProperty.DateOfAcquisition = entity.collateralProperty.dateOfAcquisition;
        //        collateralProperty.LastValuationDate = entity.collateralProperty.lastValuationDate;
        //        collateralProperty.ValuerId = entity.collateralProperty.valuerId;
        //        collateralProperty.ValuerReferenceNumber = entity.collateralProperty.valuerReferenceNumber;
        //        collateralProperty.OpenMarketValue = entity.collateralProperty.openMarketValue;
        //        collateralProperty.CollateralValue = entity.collateralProperty.collateralValue;
        //        collateralProperty.ForcedSaleValue = entity.collateralProperty.forcedSaleValue;
        //        collateralProperty.StampToCover = entity.collateralProperty.stampToCover;
        //        collateralProperty.ValuationSource = entity.collateralProperty.valuationSource;
        //        collateralProperty.OriginalValue = entity.collateralProperty.originalValue;
        //        collateralProperty.AvailableValue = entity.collateralProperty.availableValue;
        //        collateralProperty.SecurityValue = entity.collateralProperty.securityValue;
        //        collateralProperty.CollateralUsableAmount = entity.collateralProperty.collateralUsableAmount;
        //        collateralProperty.PropertyValueBaseTypeId = entity.collateralProperty.propertyValueBaseTypeId;
        //        collateralProperty.Remark = entity.collateralProperty.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.MarketableSecurities)
        //    {
        //        var collateralMarketableSecurity = context.tbl_Collateral_Marketable_Security.Find(entity.collateralMarketableSecurity.collateralMarketableSecurityId);

        //        collateralMarketableSecurity.SecurityType = entity.collateralMarketableSecurity.securityType;
        //        collateralMarketableSecurity.DealReferenceNumber = entity.collateralMarketableSecurity.dealReferenceNumber;
        //        collateralMarketableSecurity.EffectiveDate = entity.collateralMarketableSecurity.effectiveDate;
        //        collateralMarketableSecurity.MaturityDate = entity.collateralMarketableSecurity.maturityDate;
        //        collateralMarketableSecurity.DealAmount = entity.collateralMarketableSecurity.dealAmount;
        //        collateralMarketableSecurity.SecurityValue = entity.collateralMarketableSecurity.securityValue;
        //        collateralMarketableSecurity.LienUsableAmount = entity.collateralMarketableSecurity.lienUsableAmount;
        //        collateralMarketableSecurity.Rating = entity.collateralMarketableSecurity.rating;
        //        collateralMarketableSecurity.PercentageInterest = entity.collateralMarketableSecurity.percentageInterest;
        //        collateralMarketableSecurity.InterestPaymentFrequency = entity.collateralMarketableSecurity.interestPaymentFrequency;
        //        collateralMarketableSecurity.IssuerName = entity.collateralMarketableSecurity.issuerName;
        //        collateralMarketableSecurity.IssuerReferenceNumber = entity.collateralMarketableSecurity.issuerReferenceNumber;
        //        collateralMarketableSecurity.UnitValue = entity.collateralMarketableSecurity.unitValue;
        //        collateralMarketableSecurity.NumberOfUnits = entity.collateralMarketableSecurity.numberOfUnits;
        //        collateralMarketableSecurity.Remark = entity.collateralMarketableSecurity.remark;
        //    }
        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.TermDeposit)
        //    {
        //        var collateralDeposit = context.tbl_Collateral_Deposit.Find(entity.collateralDeposit.collateralDepositId);

        //        collateralDeposit.AccountNumber = entity.collateralDeposit.accountNumber;
        //        collateralDeposit.DealReferenceNumber = entity.collateralDeposit.dealReferenceNumber;
        //        //collateralDeposit.ExistingLienAmount = entity.collateralDeposit.existingLienAmount;
        //        collateralDeposit.LienAmount = entity.collateralDeposit.lienAmount;
        //        collateralDeposit.AvailableBalance = entity.collateralDeposit.availableBalance;
        //        collateralDeposit.SecurityValue = entity.collateralDeposit.securityValue;
        //        collateralDeposit.MaturityDate = entity.collateralDeposit.maturityDate;
        //        collateralDeposit.MaturityAmount = entity.collateralDeposit.maturityAmount;
        //        collateralDeposit.Remark = entity.collateralDeposit.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.CASA)
        //    {
        //        var collateralCasa = context.tbl_Collateral_Casa.Find(entity.collateralCasa.collateralCasaId);

        //        collateralCasa.AccountNumber = entity.collateralCasa.accountNumber;
        //        collateralCasa.IsOwnedByCustomer = entity.collateralCasa.isOwnedByCustomer;
        //        collateralCasa.AvailableBalance = entity.collateralCasa.availableBalance;
        //        collateralCasa.ExistingLienAmount = entity.collateralCasa.existingLienAmount;
        //        collateralCasa.LienAmount = entity.collateralCasa.lienAmount;
        //        collateralCasa.SecurityValue = entity.collateralCasa.securityValue;
        //        collateralCasa.Remark = entity.collateralCasa.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.PlantAndMachinery)
        //    {
        //        var collateralMachineDetail = context.tbl_Collateral_Plant_And_Equipment.Find(entity.collateralMachineDetail.collateralMachineDetailId);

        //        collateralMachineDetail.MachineName = entity.collateralMachineDetail.machineName;
        //        collateralMachineDetail.Description = entity.collateralMachineDetail.description;
        //        collateralMachineDetail.MachineNumber = entity.collateralMachineDetail.machineNumber;
        //        collateralMachineDetail.ManufacturerName = entity.collateralMachineDetail.manufacturerName;
        //        collateralMachineDetail.YearOfManufacture = entity.collateralMachineDetail.yearOfManufacture;
        //        collateralMachineDetail.YearOfPurchase = entity.collateralMachineDetail.yearOfManufacture;
        //        collateralMachineDetail.ValueBaseTypeId = entity.collateralMachineDetail.valueBaseTypeId;
        //        collateralMachineDetail.MachineCondition = entity.collateralMachineDetail.machineCondition;
        //        collateralMachineDetail.MachineryLocation = entity.collateralMachineDetail.machineryLocation;
        //        collateralMachineDetail.EquipmentSize = entity.collateralMachineDetail.equipmentSize;
        //        collateralMachineDetail.ReplacementValue = entity.collateralMachineDetail.replacementValue;
        //        collateralMachineDetail.IntendedUse = entity.collateralMachineDetail.intendedUse;

        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.PreciousMetal)
        //    {
        //        var collateralPreciousMetal = context.tbl_Collateral_PreciousMetal.Find(entity.collateralPreciousMetal.collateralPreciousMetalId);

        //        collateralPreciousMetal.CollateralCustomerId = entity.collateralPreciousMetal.collateralCustomerId;
        //        collateralPreciousMetal.IsOwnedByCustomer = entity.collateralPreciousMetal.isOwnedByCustomer;
        //        collateralPreciousMetal.PreciousMetalName = entity.collateralPreciousMetal.preciousMetalName;
        //        collateralPreciousMetal.WeightInGrammes = entity.collateralPreciousMetal.weightInGrammes;
        //        collateralPreciousMetal.ValuationAmount = entity.collateralPreciousMetal.valuationAmount;
        //        collateralPreciousMetal.UnitRate = entity.collateralPreciousMetal.unitRate;
        //        collateralPreciousMetal.PreciousMetalForm = entity.collateralPreciousMetal.preciousMetalForm;
        //        collateralPreciousMetal.Remark = entity.collateralPreciousMetal.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.InsurancePolicy)
        //    {
        //        tbl_Collateral_Policy collateralInsurancePolicy = collateral.tbl_Collateral_Policy.Where(x => x.CollateralInsurancePolicyId == entity.collateralTypeId)
        //            .FirstOrDefault();

        //        collateralInsurancePolicy.PremiumAmount = entity.collateralInsurancePolicy.premiumAmount;
        //        collateralInsurancePolicy.IsOwnedByCustomer = entity.collateralInsurancePolicy.isOwnedByCustomer;
        //        collateralInsurancePolicy.InsurancePolicyNumber = entity.collateralInsurancePolicy.insurancePolicyNumber;
        //        collateralInsurancePolicy.PolicyAmount = entity.collateralInsurancePolicy.policyAmount;
        //        collateralInsurancePolicy.InsuranceCompanyName = entity.collateralInsurancePolicy.insuranceCompanyName;
        //        collateralInsurancePolicy.PolicyStartDate = entity.collateralInsurancePolicy.policyStartDate;
        //        collateralInsurancePolicy.AssignDate = entity.collateralInsurancePolicy.assignDate;
        //        collateralInsurancePolicy.PolicyRenewalDate = entity.collateralInsurancePolicy.policyRenewalDate;
        //        collateralInsurancePolicy.InsurerAddress = entity.collateralInsurancePolicy.insurerAddress;
        //        collateralInsurancePolicy.InsurerDetails = entity.collateralInsurancePolicy.insurerDetails;
        //        collateralInsurancePolicy.RenewalFrequencyTypeId = entity.collateralInsurancePolicy.renewalFrequencyTypeId;
        //        collateralInsurancePolicy.Remark = entity.collateralInsurancePolicy.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.Gaurantee)
        //    {
        //        tbl_Collateral_Gaurantee collateralGaurantee = collateral.tbl_Collateral_Gaurantee.Where(x => x.CollateralGauranteeId == entity.collateralTypeId)
        //            .FirstOrDefault();

        //        collateralGaurantee.IsOwnedByCustomer = entity.collateralGaurantee.isOwnedByCustomer;
        //        collateralGaurantee.InstitutionName = entity.collateralGaurantee.institutionName;
        //        collateralGaurantee.GuarantorReferenceNumber = entity.collateralGaurantee.guarantorReferenceNumber;
        //        collateralGaurantee.GuaranteeValue = entity.collateralGaurantee.guaranteeValue;
        //        collateralGaurantee.StartDate = entity.collateralGaurantee.startDate;
        //        collateralGaurantee.EndDate = entity.collateralGaurantee.endDate;
        //        collateralGaurantee.GuarantorAddress = entity.collateralGaurantee.guarantorAddress;
        //        collateralGaurantee.Remark = entity.collateralGaurantee.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.Vehicle)
        //    {
        //        tbl_Collateral_Vehicle collateralVehicle = collateral.tbl_Collateral_Vehicle.Where(x => x.CollateralVehicleId == entity.collateralTypeId)
        //            .FirstOrDefault();

        //        collateralVehicle.VehicleType = entity.collateralVehicle.vehicleType;
        //        collateralVehicle.VehicleStatus = entity.collateralVehicle.vehicleStatus;
        //        collateralVehicle.VehicleMake = entity.collateralVehicle.vehicleMake;
        //        collateralVehicle.ModelName = entity.collateralVehicle.modelName;
        //        collateralVehicle.dateOfManufacture = entity.collateralVehicle.dateOfManufacture;
        //        collateralVehicle.SerialNumber = entity.collateralVehicle.serialNumber;
        //        collateralVehicle.NameOfOwner = entity.collateralVehicle.nameOfOwner;
        //        collateralVehicle.RegistrationCompany = entity.collateralVehicle.registrationCompany;
        //        collateralVehicle.LastValuationAmount = entity.collateralVehicle.lastValuationAmount;
        //        collateralVehicle.RegistrationNumber = entity.collateralVehicle.registrationNumber;
        //        collateralVehicle.ChasisNumber = entity.collateralVehicle.chasisNumber;
        //        collateralVehicle.EngineNumber = entity.collateralVehicle.engineNumber;
        //        collateralVehicle.ResaleValue = entity.collateralVehicle.resaleValue;
        //        collateralVehicle.ValuationDate = entity.collateralVehicle.valuationDate;
        //        collateralVehicle.InvoiceValue = entity.collateralVehicle.invoiceValue;
        //        collateralVehicle.Remark = entity.collateralVehicle.remark;
        //    }

        //    if (entity.collateralTypeId == (int)CollateralTypeEnum.Miscellaneous)
        //    {
        //        tbl_Collateral_Miscellaneous collateralMiscellaneous = collateral.tbl_Collateral_Miscellaneous.Where(x => x.CollateralMiscellaneousId == entity.collateralTypeId)
        //            .FirstOrDefault();

        //        collateralMiscellaneous.NameOfSecurity = entity.collateralMiscellaneous.nameOfSecurity;
        //        collateralMiscellaneous.SecurityValue = entity.collateralMiscellaneous.securityValue;
        //        //if (entity.collateralMiscellaneous.collateralMiscellaneousNotes != null)
        //        //{
        //        //    tbl_Collateral_Miscellaneous_Notes collateralMiscellaneousNote = context.tbl_Collateral_Miscellaneous_Notes.Where(x => x.MiscellaneousId == entity.collateralMiscellaneous.collateralMiscellaneousId)
        //        //    .FirstOrDefault();

        //        //    collateralMiscellaneousNote.ColumnName = entity.collateralMiscellaneous.collateralMiscellaneousNotes.;
        //        //    collateralMiscellaneous.NameOfSecurity = entity.collateralMiscellaneous.nameOfSecurity;
        //        //    collateralMiscellaneous.SecurityValue = entity.collateralMiscellaneous.securityValue;
        //        //    collateralMiscellaneous.Note = entity.collateralMiscellaneous.note;
        //        //}
        //    }

        //    //if (entity.collateralCustomerPolicy != null)
        //    //{
        //    //    tbl_Collateral_Item_Policy collateralCustomerPolicy = collateral.tbl_Collateral_Customer_Policy.Where(x => x.PolicyId == entity.collateralCustomerPolicy.policyId)
        //    //        .FirstOrDefault();

        //    //    collateralCustomerPolicy.PolicyReferenceNumber = entity.collateralCustomerPolicy.policyReferenceNumber;
        //    //    collateralCustomerPolicy.InsuranceCompanyName = entity.collateralCustomerPolicy.insuranceCompanyName;
        //    //    collateralCustomerPolicy.StartDate = entity.collateralCustomerPolicy.startDate;
        //    //    collateralCustomerPolicy.EndDate = entity.collateralCustomerPolicy.endDate;
        //    //}

        //    var audit = new tbl_Audit
        //    {
        //        AuditTypeId = (short)AuditTypeEnum.CustomerGroupDeleted,
        //        StaffId = (int)entity.lastUpdatedBy,
        //        BranchId = (short)entity.userBranchId,
        //        Detail = $"Update collateral with code: { entity.collateralCode} of { entity.valuationCycle} valuation cycle",
        //        //Ipaddress = entity.userIPAddress,
        //        Url = entity.applicationUrl,
        //        ApplicationDate = genSetup.GetApplicationDate(),
        //        SystemDateTime = DateTime.Now
        //    };

        //    this.auditTrail.AddAuditTrail(audit);


        //    return await context.SaveChangesAsync() != 0;
        //}

        public bool IsCollateralDocExists(string docName)
        {
            return false;
            //return context.TblCollateralCustomer.Any(c => string.Equals(c.DocumentNo, docName, StringComparison.OrdinalIgnoreCase));
        }
        #endregion Collateral Customer

        #region Property
        private ICollection<TBL_COLLATERAL_IMMOVE_PROPERTY> AddCollateralProperty(CollateralTypeEnum collateralType, CollateralPropertyViewModel entity)
        {
            ICollection<TBL_COLLATERAL_IMMOVE_PROPERTY> collateral;

            if (collateralType != CollateralTypeEnum.Property)
                return null;

            collateral = new List<TBL_COLLATERAL_IMMOVE_PROPERTY>();

            collateral.Add(new TBL_COLLATERAL_IMMOVE_PROPERTY
            {
                //CollateralPropertyId = entity.collateralPropertyId,
                //CollateralCustomerId = entity.collateralCustomerId,
                PROPERTYNAME = entity.propertyName,
                CITYID = entity.cityId,
                COUNTRYID = entity.countryId,
                PROPERTYADDRESS = entity.propertyAddress,
                CONSTRUCTIONDATE = entity.constructionDate,
                PROPERTYVALUEBASETYPEID = entity.propertyValueBaseTypeId,
                DATEOFACQUISITION = entity.dateOfAcquisition,
                LASTVALUATIONDATE = entity.lastValuationDate,
                VALUERID = entity.valuerId,
                VALUERREFERENCENUMBER = entity.valuerReferenceNumber,
                OPENMARKETVALUE = entity.openMarketValue,

                //  COLLATERALVALUE = entity.collateralValue,
                FORCEDSALEVALUE = entity.forcedSaleValue,
                STAMPTOCOVER = entity.stampToCover,
                // VALUATIONSOURCE = entity.valuationSource,
                // ORIGINALVALUE = entity.originalValue,
                // AVAILABLEVALUE = entity.availableValue,

                SECURITYVALUE = entity.securityValue,
                COLLATERALUSABLEAMOUNT = entity.collateralUsableAmount,
                REMARK = entity.remark,
                VALUATIONAMOUNT = entity.valuationAmount
            });

            return collateral;
        }

        private CollateralPropertyViewModel CollateralProperty(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where c.DELETED == false && m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralPropertyViewModel
                    {
                        collateralPropertyId = m.COLLATERALPROPERTYID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        propertyName = m.PROPERTYNAME,
                        cityId = m.CITYID,
                        countryId = m.COUNTRYID,
                        propertyAddress = m.PROPERTYADDRESS,
                        constructionDate = m.CONSTRUCTIONDATE,
                        propertyValueBaseTypeId = m.PROPERTYVALUEBASETYPEID,
                        dateOfAcquisition = m.DATEOFACQUISITION,
                        lastValuationDate = m.LASTVALUATIONDATE,
                        valuerId = m.VALUERID,
                        valuerReferenceNumber = m.VALUERREFERENCENUMBER,
                        estimatedValue = m.ESTIMATEDVALUE,
                        //   collateralValue = m.COLLATERALVALUE,
                        forcedSaleValue = m.FORCEDSALEVALUE,
                        stampToCover = m.STAMPTOCOVER,
                        //  valuationSource = m.VALUATIONSOURCE,
                        //  originalValue = m.ORIGINALVALUE,
                        //   availableValue = m.AVAILABLEVALUE,

                        collateralUsableAmount = m.COLLATERALUSABLEAMOUNT,
                        remark = m.REMARK,
                        //  valuationAmount = m.VALUATIONAMOUNT ,
                    }).FirstOrDefault();
        }

        private CollateralPropertyViewModel GetCollateralPropertyByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralProperty(CollateralCustomerId);
        }
        #endregion Property

        #region Deposit
        private ICollection<TBL_COLLATERAL_DEPOSIT> AddCollateralDeposit(CollateralTypeEnum collateralType, CollateralDepositViewModel entity)
        {
            ICollection<TBL_COLLATERAL_DEPOSIT> collateral;

            if (collateralType != CollateralTypeEnum.FixedDeposit)
                return null;

            collateral = new List<TBL_COLLATERAL_DEPOSIT>();

            collateral.Add(new TBL_COLLATERAL_DEPOSIT
            {
                //CollateralDepositId = entity.collateralDepositId,
                //CollateralCustomerId = entity.collateralCustomerId,
                DEALREFERENCENUMBER = entity.dealReferenceNumber,
                ACCOUNTNUMBER = entity.accountNumber,
                //ExistingLienAmount = entity.existingLienAmount,
                LIENAMOUNT = entity.lienAmount,
                AVAILABLEBALANCE = entity.availableBalance,
                SECURITYVALUE = entity.securityValue,
                MATURITYDATE = entity.maturityDate,
                MATURITYAMOUNT = entity.maturityAmount,
                REMARK = entity.remark,
                ACCOUNTNAME = entity.accountName
            });

            return collateral;
        }

        private void AddDepositCollateral(int collateralId, CollateralViewModel entity)
        {
            context.TBL_COLLATERAL_DEPOSIT.Add(new TBL_COLLATERAL_DEPOSIT
            {
                COLLATERALCUSTOMERID = collateralId,
                BANK = entity.bank,
                DEALREFERENCENUMBER = entity.dealReferenceNumber,
                ACCOUNTNUMBER = entity.accountNumber,
                EXISTINGLIENAMOUNT = entity.existingLienAmount,
                LIENAMOUNT = entity.lienAmount,
                AVAILABLEBALANCE = entity.availableBalance,
                SECURITYVALUE = (decimal)entity.securityValue,
                MATURITYDATE = entity.maturityDate,
                MATURITYAMOUNT = entity.maturityAmount,
                EFFECTIVEDATE = entity.effectiveDate,
                REMARK = entity.remark,
                ACCOUNTNAME = entity.accountName
            });

        }

        private bool AddDepositCollateralArchive(int collateralId)
        {
            var collateral = context.TBL_COLLATERAL_DEPOSIT.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateralId);
            if (collateral == null) throw new SecureException("Deposit Collateral Not Found to be Archived");
            //context.TBL_COLLATERAL_DEPOSIT_ARCHV.Add(new TBL_COLLATERAL_DEPOSIT_ARCHV
            //{
            //    //COLLATERALCUSTOMERID = collateralId,
            //    //BANK = entity.bank,
            //    //DEALREFERENCENUMBER = entity.dealReferenceNumber,
            //    //ACCOUNTNUMBER = entity.accountNumber,
            //    //EXISTINGLIENAMOUNT = entity.existingLienAmount,
            //    //LIENAMOUNT = entity.lienAmount,
            //    //AVAILABLEBALANCE = entity.availableBalance,
            //    //SECURITYVALUE = (decimal)entity.securityValue,
            //    //MATURITYDATE = entity.maturityDate,
            //    //MATURITYAMOUNT = entity.maturityAmount,
            //    //EFFECTIVEDATE = entity.effectiveDate,
            //    //REMARK = entity.remark,
            //    //ACCOUNTNAME = entity.accountName
            //});
            return true;
        }

        private void UpdateDepositCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_DEPOSIT
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();
            if (collateral == null)
            {
                collateral = new TBL_COLLATERAL_DEPOSIT()
                {
                    BANK = entity.bank,
                    COLLATERALCUSTOMERID = entity.collateralId,
                    DEALREFERENCENUMBER = entity.dealReferenceNumber,
                    ACCOUNTNUMBER = entity.accountNumber,
                    EXISTINGLIENAMOUNT = entity.existingLienAmount,
                    LIENAMOUNT = entity.lienAmount,
                    AVAILABLEBALANCE = entity.availableBalance,
                    SECURITYVALUE = (decimal)entity.securityValue,
                    MATURITYDATE = entity.maturityDate,
                    MATURITYAMOUNT = entity.maturityAmount,
                    EFFECTIVEDATE = entity.effectiveDate,
                    REMARK = entity.remark,
                    ACCOUNTNAME = entity.accountName
                };
                context.TBL_COLLATERAL_DEPOSIT.Add(collateral);
                //var saved = context.SaveChanges() != 0;
                return;
            }
            collateral.BANK = entity.bank;
            collateral.DEALREFERENCENUMBER = entity.dealReferenceNumber;
            collateral.ACCOUNTNUMBER = entity.accountNumber;
            collateral.EXISTINGLIENAMOUNT = entity.existingLienAmount;
            collateral.LIENAMOUNT = entity.lienAmount;
            collateral.AVAILABLEBALANCE = entity.availableBalance;
            collateral.SECURITYVALUE = (decimal)entity.securityValue;
            collateral.MATURITYDATE = entity.maturityDate;
            collateral.MATURITYAMOUNT = entity.maturityAmount;
            collateral.EFFECTIVEDATE = entity.effectiveDate;
            collateral.REMARK = entity.remark;
            collateral.ACCOUNTNAME = entity.accountName;

        }

        private CollateralDepositViewModel CollateralDeposit(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_DEPOSIT
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralDepositViewModel
                    {
                        collateralDepositId = m.COLLATERALDEPOSITID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        dealReferenceNumber = m.DEALREFERENCENUMBER,
                        accountNumber = m.ACCOUNTNUMBER,
                        //existingLienAmount = m.ExistingLienAmount,
                        lienAmount = m.LIENAMOUNT,
                        availableBalance = m.AVAILABLEBALANCE,
                        securityValue = m.SECURITYVALUE,
                        maturityDate = m.MATURITYDATE,
                        maturityAmount = m.MATURITYAMOUNT,
                        remark = m.REMARK,
                        accountName = m.ACCOUNTNAME

                    }).FirstOrDefault();
        }

        private CollateralDepositViewModel GetCollateralDepositByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralDeposit(CollateralCustomerId);
        }
        #endregion Deposit

        #region End od CASA
        private ICollection<TBL_COLLATERAL_CASA> AddCollateralCasa(CollateralTypeEnum collateralType, CollateralCasaViewModel entity)
        {
            ICollection<TBL_COLLATERAL_CASA> collateral;

            if (collateralType != CollateralTypeEnum.CASA)
                return null;

            collateral = new List<TBL_COLLATERAL_CASA>();

            collateral.Add(new TBL_COLLATERAL_CASA
            {
                ACCOUNTNUMBER = entity.accountNumber,
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                AVAILABLEBALANCE = entity.availableBalance,
                EXISTINGLIENAMOUNT = entity.existingLienAmount,
                LIENAMOUNT = entity.lienAmount,
                SECURITYVALUE = entity.securityValue,
                REMARK = entity.remark,
                ACCOUNTNAME = entity.accountNumber
            });

            return collateral;
        }

        private CollateralCasaViewModel CollateralCasa(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_CASA
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralCasaViewModel
                    {
                        collateralCasaId = m.COLLATERALCASAID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        accountNumber = m.ACCOUNTNUMBER,
                        isOwnedByCustomer = m.ISOWNEDBYCUSTOMER,
                        availableBalance = m.AVAILABLEBALANCE,
                        existingLienAmount = m.EXISTINGLIENAMOUNT,
                        lienAmount = m.LIENAMOUNT,
                        securityValue = m.SECURITYVALUE,
                        remark = m.REMARK,
                        accountName = m.ACCOUNTNAME

                    }).FirstOrDefault();
        }

        private CollateralCasaViewModel GetCollateralCasaByCollateralCustomerId(int CollateralCustomerId)
        {
            return CollateralCasa(CollateralCustomerId);
        }
        #endregion End of CASA

        #region Plants and Equipment
        private ICollection<TBL_COLLATERAL_PLANT_AND_EQUIP> AddCollateralMachineDetail(CollateralTypeEnum collateralType, CollateralPlantsAndEquipmentViewModel entity)
        {
            ICollection<TBL_COLLATERAL_PLANT_AND_EQUIP> collateral;

            if (collateralType != CollateralTypeEnum.PlantAndMachinery)
                return null;

            collateral = new List<TBL_COLLATERAL_PLANT_AND_EQUIP>();

            collateral.Add(new TBL_COLLATERAL_PLANT_AND_EQUIP
            {
                MACHINENAME = entity.machineName,
                DESCRIPTION = entity.description,
                MACHINENUMBER = entity.machineNumber,
                MANUFACTURERNAME = entity.manufacturerName,
                YEAROFMANUFACTURE = entity.yearOfManufacture,
                YEAROFPURCHASE = entity.yearOfManufacture,
                VALUEBASETYPEID = entity.valueBaseTypeId,
                MACHINECONDITION = entity.machineCondition,
                MACHINERYLOCATION = entity.machineryLocation,
                REPLACEMENTVALUE = entity.replacementValue,
                EQUIPMENTSIZE = entity.equipmentSize,
                INTENDEDUSE = entity.intendedUse
            });

            return collateral;
        }

        private CollateralPlantsAndEquipmentViewModel CollateralMachineDetail(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_PLANT_AND_EQUIP
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralPlantsAndEquipmentViewModel
                    {
                        collateralMachineDetailId = m.COLLATERALMACHINEDETAILID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        machineName = m.MACHINENAME,
                        description = m.DESCRIPTION,
                        machineNumber = m.MACHINENUMBER,
                        manufacturerName = m.MANUFACTURERNAME,
                        yearOfManufacture = m.YEAROFMANUFACTURE,
                        yearOfPurchase = m.YEAROFPURCHASE,
                        valueBaseTypeId = m.VALUEBASETYPEID,
                        machineCondition = m.MACHINECONDITION,
                        machineryLocation = m.MACHINERYLOCATION,
                        replacementValue = m.REPLACEMENTVALUE,
                        equipmentSize = m.EQUIPMENTSIZE,
                        intendedUse = m.INTENDEDUSE
                    }).FirstOrDefault();
        }

        private CollateralPlantsAndEquipmentViewModel GetCollateralMachineDetailByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralMachineDetail(collateralCustomerId);
        }
        #endregion Plants and Equipment

        #region Marketable Security
        private ICollection<TBL_COLLATERAL_MKT_SECURITY> AddCollateralMarketableSecurity(CollateralTypeEnum collateralType, CollateralMarketableSecurityViewModel entity)
        {
            ICollection<TBL_COLLATERAL_MKT_SECURITY> collateral;

            if (collateralType != CollateralTypeEnum.TreasuryBillsAndBonds)
                return null;

            collateral = new List<TBL_COLLATERAL_MKT_SECURITY>();

            collateral.Add(new TBL_COLLATERAL_MKT_SECURITY
            {
                SECURITYTYPE = entity.securityType,
                //    DEALREFERENCENUMBER = entity.dealReferenceNumber,
                EFFECTIVEDATE = entity.effectiveDate,
                MATURITYDATE = entity.maturityDate,
                DEALAMOUNT = entity.dealAmount,
                SECURITYVALUE = entity.securityValue,
                LIENUSABLEAMOUNT = entity.lienUsableAmount,
                ISSUERNAME = entity.issuerName,
                ISSUERREFERENCENUMBER = entity.issuerReferenceNumber,
                UNITVALUE = entity.unitValue,
                NUMBEROFUNITS = entity.numberOfUnits,
                RATING = entity.rating,
                PERCENTAGEINTEREST = entity.percentageInterest,
                INTERESTPAYMENTFREQUENCY = entity.interestPaymentFrequency,
                REMARK = entity.remark

            });

            return collateral;
        }

        private CollateralMarketableSecurityViewModel CollateralMarketableSecurity(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_MKT_SECURITY
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralMarketableSecurityViewModel
                    {
                        collateralMarketableSecurityId = m.COLLATERALMARKETABLESECURITYID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        securityType = m.SECURITYTYPE,
                        //   dealReferenceNumber = m.DEALREFERENCENUMBER,
                        effectiveDate = m.EFFECTIVEDATE,
                        maturityDate = m.MATURITYDATE,
                        dealAmount = m.DEALAMOUNT,
                        securityValue = m.SECURITYVALUE,
                        lienUsableAmount = m.LIENUSABLEAMOUNT,
                        rating = m.RATING,
                        percentageInterest = m.PERCENTAGEINTEREST,
                        interestPaymentFrequency = m.INTERESTPAYMENTFREQUENCY,
                        issuerName = m.ISSUERNAME,
                        issuerReferenceNumber = m.ISSUERREFERENCENUMBER,
                        unitValue = m.UNITVALUE,
                        numberOfUnits = m.NUMBEROFUNITS,
                        remark = m.REMARK



                    }).FirstOrDefault();
        }

        private CollateralMarketableSecurityViewModel GetCollateralMarketableSecurityByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralMarketableSecurity(collateralCustomerId);
        }

        #endregion Marketable Security

        #region Precious Metal
        private ICollection<TBL_COLLATERAL_PRECIOUSMETAL> AddCollateralPreciousMetal(CollateralTypeEnum collateralType, CollateralPreciousMetalViewModel entity)
        {
            ICollection<TBL_COLLATERAL_PRECIOUSMETAL> collateral;

            if (collateralType != CollateralTypeEnum.PreciousMetal)
                return null;

            collateral = new List<TBL_COLLATERAL_PRECIOUSMETAL>();

            collateral.Add(new TBL_COLLATERAL_PRECIOUSMETAL
            {
                //ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                PRECIOUSMETALNAME = entity.preciousMetalName,
                WEIGHTINGRAMMES = entity.weightInGrammes,
                VALUATIONAMOUNT = entity.valuationAmount,
                UNITRATE = entity.unitRate,
                PRECIOUSMETALFORM = entity.preciousMetalForm,
                REMARK = entity.remark

            });

            return collateral;
        }

        private CollateralPreciousMetalViewModel CollateralPreciousMetal(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_PRECIOUSMETAL
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralPreciousMetalViewModel
                    {
                        collateralPreciousMetalId = m.COLLATERALPRECIOUSMETALID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        // isOwnedByCustomer = m.ISOWNEDBYCUSTOMER,
                        preciousMetalName = m.PRECIOUSMETALNAME,
                        weightInGrammes = m.WEIGHTINGRAMMES,
                        valuationAmount = m.VALUATIONAMOUNT,
                        unitRate = m.UNITRATE,
                        preciousMetalForm = m.PRECIOUSMETALFORM,
                        remark = m.REMARK

                    }).FirstOrDefault();
        }

        private CollateralPreciousMetalViewModel GetCollateralPreciousMetalByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralPreciousMetal(collateralCustomerId);
        }
        #endregion Precious Metal

        #region Insurance Policy
        private ICollection<TBL_COLLATERAL_POLICY> AddCollateralInsurancePolicy(CollateralTypeEnum collateralType, CollateralInsurancePolicyViewModel entity)
        {
            ICollection<TBL_COLLATERAL_POLICY> collateral;

            if (collateralType != CollateralTypeEnum.InsurancePolicy)
                return null;

            collateral = new List<TBL_COLLATERAL_POLICY>();

            collateral.Add(new TBL_COLLATERAL_POLICY
            {
                //CollateralInsurancePolicyId = entity.collateralInsurancePolicyId,
                //CollateralCustomerId = entity.collateralCustomerId,
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                //   INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber,
                PREMIUMAMOUNT = entity.premiumAmount,
                POLICYAMOUNT = entity.policyAmount,
                INSURANCECOMPANYNAME = entity.insuranceCompanyName,
                INSURERADDRESS = entity.insurerAddress,
                POLICYSTARTDATE = entity.policyStartDate,
                ASSIGNDATE = entity.assignDate,
                RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId,
                INSURERDETAILS = entity.insurerDetails,
                POLICYRENEWALDATE = entity.policyRenewalDate,
                REMARK = entity.remark

            });

            return collateral;
        }

        private CollateralInsurancePolicyViewModel CollateralInsurancePolicy(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_POLICY
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralInsurancePolicyViewModel
                    {
                        collateralInsurancePolicyId = m.COLLATERALINSURANCEPOLICYID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        isOwnedByCustomer = m.ISOWNEDBYCUSTOMER,
                        //     insurancePolicyNumber = m.INSURANCEPOLICYNUMBER,
                        premiumAmount = m.PREMIUMAMOUNT,
                        policyAmount = m.POLICYAMOUNT,
                        insuranceCompanyName = m.INSURANCECOMPANYNAME,
                        insurerAddress = m.INSURERADDRESS,
                        policyStartDate = m.POLICYSTARTDATE,
                        assignDate = m.ASSIGNDATE,
                        renewalFrequencyTypeId = m.RENEWALFREQUENCYTYPEID,
                        insurerDetails = m.INSURERDETAILS,
                        policyRenewalDate = m.POLICYRENEWALDATE,
                        remark = m.REMARK
                    }).FirstOrDefault();
        }

        private CollateralInsurancePolicyViewModel GetCollateralInsurancePolicyByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralInsurancePolicy(collateralCustomerId);
        }
        #endregion Insurance Policy


        private CollateralGauranteeViewModel CollateralGaurantee(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_GAURANTEE
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralGauranteeViewModel
                    {
                        collateralGauranteeId = m.COLLATERALGAURANTEEID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        institutionName = m.INSTITUTIONNAME,
                        guarantorAddress = m.GUARANTORADDRESS,
                        guaranteeValue = m.GUARANTEEVALUE,
                        startDate = m.STARTDATE,
                        endDate = m.ENDDATE,
                        remark = m.REMARK
                    }).FirstOrDefault();
        }

        #region Vehicle
        private ICollection<TBL_COLLATERAL_VEHICLE> AddCollateralVehicle(CollateralTypeEnum collateralType, CollateralVehicleViewModel entity)
        {
            ICollection<TBL_COLLATERAL_VEHICLE> collateral;

            if (collateralType != CollateralTypeEnum.Vehicle)
                return null;

            collateral = new List<TBL_COLLATERAL_VEHICLE>();

            collateral.Add(new TBL_COLLATERAL_VEHICLE
            {
                VEHICLETYPE = entity.vehicleType,
                VEHICLESTATUS = entity.vehicleStatus,
                VEHICLEMAKE = entity.vehicleMake,
                MODELNAME = entity.modelName,
                MANUFACTUREDDATE = entity.dateOfManufacture,
                REGISTRATIONNUMBER = entity.registrationNumber,
                SERIALNUMBER = entity.serialNumber,
                CHASISNUMBER = entity.chasisNumber,
                ENGINENUMBER = entity.engineNumber,
                NAMEOFOWNER = entity.nameOfOwner,
                REGISTRATIONCOMPANY = entity.registrationCompany,
                RESALEVALUE = entity.resaleValue,
                VALUATIONDATE = entity.valuationDate,
                LASTVALUATIONAMOUNT = entity.lastValuationAmount,
                INVOICEVALUE = entity.invoiceValue,
                REMARK = entity.remark
            });

            return collateral;
        }

        private CollateralVehicleViewModel CollateralVehicle(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_VEHICLE
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralVehicleViewModel
                    {
                        collateralVehicleId = m.COLLATERALVEHICLEID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        vehicleType = m.VEHICLETYPE,
                        vehicleStatus = m.VEHICLESTATUS,
                        vehicleMake = m.VEHICLEMAKE,
                        modelName = m.MODELNAME,
                        dateOfManufacture = m.MANUFACTUREDDATE,
                        registrationNumber = m.REGISTRATIONNUMBER,
                        serialNumber = m.SERIALNUMBER,
                        chasisNumber = m.CHASISNUMBER,
                        engineNumber = m.ENGINENUMBER,
                        nameOfOwner = m.NAMEOFOWNER,
                        registrationCompany = m.REGISTRATIONCOMPANY,
                        resaleValue = m.RESALEVALUE,
                        valuationDate = m.VALUATIONDATE,
                        lastValuationAmount = m.LASTVALUATIONAMOUNT,
                        invoiceValue = m.INVOICEVALUE,
                        remark = m.REMARK
                    }).FirstOrDefault();
        }

        private CollateralVehicleViewModel GetCollateralVehicleByCollateralCustomerId(int collateralCustomerId)
        {
            return CollateralVehicle(collateralCustomerId);
        }
        #endregion Vehicle

        #region Miscellaneous
        private ICollection<TBL_COLLATERAL_MISCELLANEOUS> AddCollateralMiscellaneous(CollateralTypeEnum collateralType, CollateralMiscellaneousViewModel entity)
        {
            ICollection<TBL_COLLATERAL_MISCELLANEOUS> collateral;

            if (collateralType != CollateralTypeEnum.Miscellaneous)
                return null;

            collateral = new List<TBL_COLLATERAL_MISCELLANEOUS>();

            collateral.Add(new TBL_COLLATERAL_MISCELLANEOUS
            {
                //CollateralMiscellaneousId = entity.collateralMiscellaneousId,
                //CollateralCustomerId = entity.collateralCustomerId,
                NAMEOFSECURITY = entity.nameOfSecurity,
                SECURITYVALUE = entity.securityValue,
                TBL_COLLATERAL_MISC_NOTES = AddCollateralMiscNotes(entity.collateralMiscellaneousNotes)
            });

            return collateral;
        }

        private CollateralMiscellaneousViewModel Miscellaneous(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_MISCELLANEOUS
                    join c in context.TBL_COLLATERAL_CUSTOMER on m.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralMiscellaneousViewModel
                    {
                        collateralMiscellaneousId = m.COLLATERALMISCELLANEOUSID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        nameOfSecurity = m.NAMEOFSECURITY,
                        securityValue = (decimal)m.SECURITYVALUE,
                        collateralMiscellaneousNotes = GetCollateralMiscellaneousNotesByMiscellaneousId(m.COLLATERALMISCELLANEOUSID)

                    }).FirstOrDefault();
        }

        private CollateralMiscellaneousViewModel GetCollateralMiscellaneousByCollateralCustomerId(int collateralCustomerId)
        {
            return Miscellaneous(collateralCustomerId);
        }
        #endregion Miscellaneous

        #region Miscellaneous Notes
        private ICollection<TBL_COLLATERAL_MISC_NOTES> AddCollateralMiscNotes(List<CollateralMiscellaneousNotesViewModel> entity)
        {
            ICollection<TBL_COLLATERAL_MISC_NOTES> collateral;
            collateral = new List<TBL_COLLATERAL_MISC_NOTES>();
            foreach (var note in entity)
            {
                collateral.Add(new TBL_COLLATERAL_MISC_NOTES
                {
                    MISCELLANEOUSNOTEID = note.miscellaneousNoteId,
                    MISCELLANEOUSID = note.miscellaneousNoteId,
                    COLUMNNAME = note.columnName,
                    COLUMNVALUE = note.columnValue
                });
            }

            return collateral;
        }

        public Task<bool> DeleteCollateralMiscellaneousNotes(int miscNoteId, UserInfo user)
        {
            return Task.Run(() => false);
        }

        public Task<bool> UpdateCollateralMiscellaneousNotes(int miscNoteId, CollateralMiscellaneousNotesViewModel entity)
        {
            return Task.Run(() => false);
        }

        private List<CollateralMiscellaneousNotesViewModel> MiscellaneousNotes(int miscellaneousId)
        {
            return (from m in context.TBL_COLLATERAL_MISC_NOTES
                    join c in context.TBL_COLLATERAL_MISCELLANEOUS on m.MISCELLANEOUSID equals c.COLLATERALMISCELLANEOUSID
                    where m.MISCELLANEOUSID == miscellaneousId
                    select new CollateralMiscellaneousNotesViewModel
                    {
                        miscellaneousNoteId = m.MISCELLANEOUSNOTEID,
                        miscellaneousId = m.MISCELLANEOUSID,
                        columnName = m.COLUMNNAME,
                        columnValue = m.COLUMNVALUE,
                    }).ToList();
        }

        private List<CollateralMiscellaneousNotesViewModel> GetCollateralMiscellaneousNotesByMiscellaneousId(int collateralMiscellaneousId)
        {
            return MiscellaneousNotes(collateralMiscellaneousId);
        }
        #endregion Miscellaneous Notes

        #region Collateral Customer Policy
        private ICollection<TBL_COLLATERAL_ITEM_POLICY> AddCollateralCustomerPolicy(int collateralTypeId, CollateralCustomerPolicyViewModel entity)
        {
            var type = context.TBL_COLLATERAL_TYPE.Where(x => x.COLLATERALTYPEID == collateralTypeId).FirstOrDefault();
            ICollection<TBL_COLLATERAL_ITEM_POLICY> customerPolicy;

            if (!type.REQUIREINSURANCEPOLICY)
                return null;
            if (entity == null)
                throw new InvalidOperationException("This collateral type requires insurance policy which was not submitted");

            customerPolicy = new List<TBL_COLLATERAL_ITEM_POLICY>();

            customerPolicy.Add(new TBL_COLLATERAL_ITEM_POLICY
            {
                //PolicyId = entity.policyId,
                // CollateralCustomerId = entity.collateralCustomerId,
                POLICYREFERENCENUMBER = entity.policyReferenceNumber,
                INSURANCECOMPANYID = entity.insuranceCompanyId,
                STARTDATE = entity.startDate,
                ENDDATE = entity.endDate,
                PREMIUMAMOUNT = entity.inSurPremiumAmount

            });

            return customerPolicy;
        }

        private CollateralCustomerPolicyViewModel GetCollateralCustomerPolicyByCollateralCustomerId(int collateralCustomerId)
        {
            return (from m in context.TBL_COLLATERAL_ITEM_POLICY
                    where m.COLLATERALCUSTOMERID == collateralCustomerId
                    select new CollateralCustomerPolicyViewModel
                    {
                        policyId = m.POLICYID,
                        collateralCustomerId = m.COLLATERALCUSTOMERID,
                        policyReferenceNumber = m.POLICYREFERENCENUMBER,
                        insuranceCompanyId = m.INSURANCECOMPANYID,
                        startDate = m.STARTDATE,
                        endDate = m.ENDDATE,
                        inSurPremiumAmount = m.PREMIUMAMOUNT,

                    }).FirstOrDefault();
        }
        #endregion End of Collateral Customer Policy

        //#region Collateral Documents
        //private ICollection<tbl_Collateral_Documents> AddCollateralDocument(CollateralTypeEnum collateralType, CollateralDocumentViewModel entity)
        //{
        //    ICollection<tbl_Collateral_Documents> collateral;

        //    //if (collateralType != CollateralTypeEnum.MarketableSecurities)
        //    //    return null;

        //    collateral = new List<tbl_Collateral_Documents>();

        //    collateral.Add(new tbl_Collateral_Documents
        //    {
        //        DocumentId = entity.documentId,
        //        CollateralCustomerId = entity.collateralCustomerId,
        //        DocumentCategory = entity.documentCategory,
        //        DocumentRef = entity.documentRef,
        //        DocumentCode = entity.documentCode,
        //        DocumentType = entity.documentType,
        //        IsMandatory = entity.isMandatory,
        //        Remark = entity.remark,
        //        //CreatedBy = entity
        //        // DateTimeCreated = entity
        //    });

        //    return collateral;
        //}

        //private CollateralDocumentViewModel CollateralDocument(int collateralCustomerId)
        //{
        //    return (from m in context.tbl_Collateral_Documents
        //            join c in context.tbl_Collateral_Customer on m.CollateralCustomerId equals c.CollateralCustomerId
        //            where m.CollateralCustomerId == collateralCustomerId
        //            select new CollateralDocumentViewModel
        //            {
        //                documentId = m.DocumentId,
        //                collateralCustomerId = m.CollateralCustomerId,
        //                documentCategory = m.DocumentCategory,
        //                documentRef = m.DocumentRef,
        //                documentCode = m.DocumentCode,
        //                documentType = m.DocumentType,
        //                isMandatory = m.IsMandatory,
        //                remark = m.Remark
        //            }).FirstOrDefault();
        //}

        //private CollateralDocumentViewModel GetCollateralDocumentByCollateralCustomerId(int collateralCustomerId)
        //{
        //    return CollateralDocument(collateralCustomerId);
        //}

        //#endregion End of Collateral Documents

        public async Task<bool> AddCollateralValuer(CollateralValuersViewModel entity)
        {
            var valuer = new TBL_COLLATERAL_VALUER
            {
                CITYID = (short)entity.cityId,
                NAME = entity.name,
                VALUERLICENCENUMBER = entity.valuerLicenceNumber,
                VALUERTYPEID = entity.valuerTypeId,
                COUNTRYID = entity.countryId,
                EMAILADDRESS = entity.emailAddress,
                PHONENUMBER = entity.phoneNumber,
                ADDRESS = entity.address,
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false
            };
            context.TBL_COLLATERAL_VALUER.Add(valuer);

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Added tbl_Collateral_Valuer with Id: {entity.collateralValuerId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            };

            auditTrail.AddAuditTrail(audit);
            var response = await context.SaveChangesAsync() != 0;
            return response;
        }

        public async Task<bool> UpdateCollateralValuer(CollateralValuersViewModel entity, int id)
        {
            var valuer = context.TBL_COLLATERAL_VALUER.Find(id);

            if (valuer != null)
            {
                valuer.CITYID = (short)entity.cityId;
                valuer.NAME = entity.name;
                valuer.VALUERLICENCENUMBER = entity.valuerLicenceNumber;
                valuer.VALUERTYPEID = entity.valuerTypeId;
                valuer.COUNTRYID = entity.countryId;
                valuer.EMAILADDRESS = entity.emailAddress;
                valuer.PHONENUMBER = entity.phoneNumber;
                valuer.ADDRESS = entity.address;
                valuer.COMPANYID = entity.companyId;
            };

            // Audit Section ----------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralTypeAdded,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Updated tbl_Collateral_Valuer with Id: {entity.collateralValuerId} ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            };

            auditTrail.AddAuditTrail(audit);
            var response = await context.SaveChangesAsync() != 0;
            return response;
        }

        #region Seniority Of Claims
        //This CRUD function should be moved to setup in the collateralType repository and the get function will depend on the its setup Get function 
        public Task<bool> AddCollateralSeniorityOfClaims(CollateralSeniorityOfClaimsViewModel entity)
        {
            return Task.Run(() => false);
        }

        public Task<bool> DeleteCollateralSeniorityOfClaims(int seniorityOfClaimId, UserInfo user)
        {
            return Task.Run(() => false);
        }

        public Task<bool> UpdateCollateralSeniorityOfClaims(int seniorityOfClaimId, CollateralSeniorityOfClaimsViewModel entity)
        {
            return Task.Run(() => false);
        }

        public IEnumerable<CollateralSeniorityOfClaimsViewModel> GetCollateralSeniorityOfClaims()
        {
            return (from m in context.TBL_COLLATERAL_SENIORITY_CLAIM
                    select new CollateralSeniorityOfClaimsViewModel
                    {
                        seniorityOfClaimId = m.COLLATERALSENIORITYOFCLAIMID,
                        seniorityOfClaims = m.SENIORITYOFCLAIMS,
                        description = m.DESCRIPTION,
                        dateTimeCreated = genSetup.GetApplicationDate(),
                    });
        }
        #endregion Seniority Of Claims

        #region Listing Functions
        public IEnumerable<CollateralValueBaseTypeViewModel> GetCollateralValueBaseType(short collateralType)
        {
            return (from m in context.TBL_COLLATERAL_VALUEBASE_TYPE
                    where m.COLLATERALTYPEID == collateralType
                    select new CollateralValueBaseTypeViewModel
                    {
                        collateralValueBaseTypeId = m.COLLATERALVALUEBASETYPEID,
                        collateralTypeId = m.COLLATERALTYPEID,
                        valueBaseTypeName = m.VALUEBASETYPENAME
                    });
        }

        public IEnumerable<CollateralValuersViewModel> GetCollateralValuer(int companyId)
        {
            return (from m in context.TBL_ACCREDITEDCONSULTANT
                    where m.COMPANYID == companyId && m.ACCREDITEDCONSULTANTTYPEID == (int)AccreditedConsultantTypeEnum.Valuer
                    select new CollateralValuersViewModel
                    {
                        collateralValuerId = (short)m.ACCREDITEDCONSULTANTID,
                        cityId = m.CITYID,
                        name = m.FIRMNAME,
                        valuerLicenceNumber = m.PHONENUMBER,
                        valuerTypeId = (short)m.ACCREDITEDCONSULTANTTYPEID,
                        countryId = m.COUNTRYID,
                        accountNumber = m.ACCOUNTNUMBER,
                        //valuerBVN = m.,
                        emailAddress = m.EMAILADDRESS,
                        phoneNumber = m.PHONENUMBER,
                        address = m.ADDRESS,

                    }).OrderBy(x => x.name).ToList();
        }

        public IEnumerable<CollateralPerfectionStatusViewModel> GetCollateralPerfectionStatus()
        {
            return (from m in context.TBL_COLLATERAL_PERFECTN_STAT
                    select new CollateralPerfectionStatusViewModel
                    {
                        perfectionStatusId = m.PERFECTIONSTATUSID,
                        perfectionStatusName = m.PERFECTIONSTATUSNAME
                    });
        }

        public IEnumerable<CollateralValuerTypeViewModel> GetCollateralValuerType()
        {
            return (from m in context.TBL_COLLATERAL_VALUER_TYPE
                    select new CollateralValuerTypeViewModel
                    {
                        valuerTypeId = m.COLLATERALVALUERTYPEID,
                        valuerTypeName = m.VALUERTYPENAME
                    });
        }

        public IEnumerable<CollateralTypeViewModel> GetCollateralType()
        {
            return this.collateralType.GetCollateralTypes();
        }

        #endregion End of Listing Functions



        public IEnumerable<CollateralLoanApplication> GetAllUnmappedCustomerCollateral(int customerId, int loanApplicationId, int companyId)
        {

            var data = (from a in context.TBL_COLLATERAL_CUSTOMER
                        where a.CUSTOMERID == customerId && a.COMPANYID == companyId &&
                         !context.TBL_LOAN_APPLICATION_COLLATERL.Any(c => c.COLLATERALCUSTOMERID == a.COLLATERALCUSTOMERID && c.LOANAPPLICATIONID == loanApplicationId)
                        select new CollateralLoanApplication()
                        {

                            haircut = a.HAIRCUT,
                            collateralId = a.COLLATERALCUSTOMERID,
                            collateralCode = a.COLLATERALCODE,
                            collateralValue = (double)a.COLLATERALVALUE,
                            collateralType = a.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                        });
            return data.ToList();
        }

        public IEnumerable<CollateralLoanApplication> GetAllMappedCustomerCollateral(int customerId, int loanApplicationId, int companyId)
        {
            var data = (from a in context.TBL_COLLATERAL_CUSTOMER
                        join b in context.TBL_LOAN_APPLICATION_COLLATERL on a.COLLATERALCUSTOMERID equals b.COLLATERALCUSTOMERID
                        where b.LOANAPPLICATIONID == loanApplicationId && a.CUSTOMERID == customerId && a.COMPANYID == companyId
                        select new CollateralLoanApplication()
                        {
                            loanApplicationCollateralId = b.LOANAPPCOLLATERALID,
                            haircut = a.HAIRCUT,
                            collateralId = a.COLLATERALCUSTOMERID,
                            collateralCode = a.COLLATERALCODE,
                            collateralValue = (double)a.COLLATERALVALUE,
                            collateralType = a.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,

                        });
            return data.ToList();
        }


        public bool DeleteCollateralApplicationMapped(IEnumerable<CollateralLoanApplication> mappings, int companyId)
        {
            var data = (from a in mappings
                        join b in context.TBL_LOAN_APPLICATION_COLLATERL on a.loanApplicationCollateralId equals b.LOANAPPCOLLATERALID
                        where b.TBL_COLLATERAL_CUSTOMER.COMPANYID == companyId
                        select b);
            context.TBL_LOAN_APPLICATION_COLLATERL.RemoveRange(data);
            return context.SaveChanges() > 0;

        }


        public bool DeleteProposedCollateral(CollateralCoverageViewModel model)
        {
            try
            {
                var data = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.LOANAPPCOLLATERALID == model.loanAppCollateralId).Select(o => o).FirstOrDefault();
                if (data.APPROVALSTATUSID == (int)ApprovalStatusEnum.Approved)
                {
                    throw new Exception("Cannot Delete An Already Approved Collateral Mapping");
                }
                data.DELETED = true;
                data.DELETEDBY = model.createdBy;
                data.DATETIMEDELETED = genSetup.GetApplicationDate();
                context.Entry(data).State = EntityState.Modified;
                //context.TBL_LOAN_APPLICATION_COLLATERL.Remove(data);

                var fsdExists = context.TBL_FACILITY_STAMP_DUTY.Where(f => f.COLLATERALCUSTOMERID == data.COLLATERALCUSTOMERID && f.LOANAPPLICATIONDETAILID == data.LOANAPPLICATIONDETAILID).FirstOrDefault();
                if (fsdExists != null)
                {
                    fsdExists.DELETED = true;
                }

                var stampFees = context.TBL_CHARGE_FEE.Where(s => s.CHARGEFEENAME.ToLower().Contains("(fixed)") && s.DELETED == false).ToList();
                if (stampFees.Count > 0)
                {
                    foreach (var fee in stampFees)
                    {
                        var dat = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(d => d.LOANAPPLICATIONDETAILID == data.LOANAPPLICATIONDETAILID && d.CHARGEFEEID == fee.CHARGEFEEID).ToList();

                        var fees = dat;
                        if (fees != null)
                        {
                            foreach (var f in fees)
                            {
                                context.TBL_LOAN_APPLICATION_DETL_FEE.Remove(f);
                            }

                        }
                    }

                }


                var  isGood = context.SaveChanges();
                if (isGood > 0)
                {
                    return true;
                } 
                return context.SaveChanges() > 0;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool DeleteDuplicatedCollateral(CollateralViewModel model)
        {
            bool status = false;
            var data = context.TBL_COLLATERAL_CUSTOMER.Where(o => o.COLLATERALCUSTOMERID == model.collateralCustomerId).Select(o => o).FirstOrDefault();
            var data2 = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == model.collateralCustomerId && o.DELETED == false).Select(o => o).FirstOrDefault();

            var staff = context.TBL_STAFF.Find(model.createdBy);
            if (data != null && data2 == null)
                //if (data != null && data.CREATEDBY == model.createdBy || staff.STAFFROLEID == 181 && data2 == null)
            {
                data.DELETED = true;
                data.DELETEDBY = model.deletedBy;
                data.DATETIMEDELETED = genSetup.GetApplicationDate();
                if (context.SaveChanges() > 0)
                {
                    status = true;
                }
                else
                {
                    status = false;
                }

            }
            return status;
        }

        #region Collateral Information View
        // .....COMPLETE COLLATERAL INFORMATION VIEW............
        public IEnumerable<AllCollateralViewModel> GetCollateralInformationById(int customercollateralId)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.DELETED == false
                && x.COLLATERALCUSTOMERID == customercollateralId
            )
            .Select(x => new AllCollateralViewModel
            {
                collateralId = x.COLLATERALCUSTOMERID,
                collateralTypeId = x.COLLATERALTYPEID,
                collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                collateralSubTypeId = x.COLLATERALSUBTYPEID,
                collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID == x.COLLATERALSUBTYPEID)
                                                                                   .FirstOrDefault().COLLATERALSUBTYPENAME,
                customerId = x.CUSTOMERID.Value,
                customerName = x.TBL_CUSTOMER.FIRSTNAME + " " + x.TBL_CUSTOMER.MIDDLENAME + " " + x.TBL_CUSTOMER.LASTNAME,
                currencyId = x.CURRENCYID,
                currency = x.TBL_CURRENCY.CURRENCYNAME,
                currencyCode = x.TBL_CURRENCY.CURRENCYCODE,
                collateralCode = x.COLLATERALCODE,
                collateralValue = x.COLLATERALVALUE,
                camRefNumber = x.CAMREFNUMBER,
                allowSharing = x.ALLOWSHARING,
                isLocationBased = (bool)x.ISLOCATIONBASED,
                valuationCycle = x.VALUATIONCYCLE,
                haircut = x.HAIRCUT,
                approvalStatusName = x.APPROVALSTATUS,
                exchangeRate = x.EXCHANGERATE,
                collateralItemPolicy = (from p in context.TBL_COLLATERAL_ITEM_POLICY.Where(s => s.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID)
                                        select new CollateralCustomerPolicyViewModel
                                        {
                                            policyId = p.POLICYID,
                                            policyReferenceNumber = p.POLICYREFERENCENUMBER,
                                            insuranceCompanyId = p.INSURANCECOMPANYID,
                                            startDate = p.STARTDATE,
                                            endDate = p.ENDDATE,

                                        }).ToList(),
            })
            .OrderByDescending(x => x.collateralId)

            .ToList();

            foreach (var record in collateral)
            {
                if (record.collateralTypeId == (int)CollateralTypeEnum.CASA)
                {
                    record.collateralCasa = (from x in context.TBL_COLLATERAL_CASA.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                             select new CollateralCasaViewModel
                                             {
                                                 collateralCasaId = x.COLLATERALCASAID,
                                                 collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                 collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                 collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                    x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                                             .FirstOrDefault().COLLATERALSUBTYPENAME,
                                                 accountNumber = x.ACCOUNTNUMBER,
                                                 isOwnedByCustomer = x.ISOWNEDBYCUSTOMER,
                                                 availableBalance = x.AVAILABLEBALANCE,
                                                 existingLienAmount = x.EXISTINGLIENAMOUNT,
                                                 lienAmount = x.LIENAMOUNT,
                                                 securityValue = x.SECURITYVALUE,
                                                 remark = x.REMARK,
                                                 accountName = x.ACCOUNTNAME

                                             }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.FixedDeposit)
                {
                    record.collateralDeposit = (from x in context.TBL_COLLATERAL_DEPOSIT.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                                select new CollateralDepositViewModel
                                                {
                                                    collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                    collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                    collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                       x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                                                .FirstOrDefault().COLLATERALSUBTYPENAME,
                                                    accountNumber = x.ACCOUNTNUMBER,
                                                    collateralDepositId = x.COLLATERALDEPOSITID,
                                                    dealReferenceNumber = x.DEALREFERENCENUMBER,
                                                    maturityDate = x.MATURITYDATE,
                                                    maturityAmount = x.MATURITYAMOUNT,
                                                    availableBalance = x.AVAILABLEBALANCE,
                                                    existingLienAmount = x.EXISTINGLIENAMOUNT,
                                                    lienAmount = x.LIENAMOUNT,
                                                    securityValue = x.SECURITYVALUE,
                                                    remark = x.REMARK,
                                                    accountName = x.ACCOUNTNAME
                                                }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.Property)
                {
                    record.collateralProperty = (from x in context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                                 select new CollateralPropertyViewModel
                                                 {
                                                     collateralPropertyId = x.COLLATERALPROPERTYID,
                                                     collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                     collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                     collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                        x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                        .FirstOrDefault().COLLATERALSUBTYPENAME,
                                                     propertyName = x.PROPERTYNAME,
                                                     cityId = x.CITYID,
                                                     cityName = x.TBL_CITY.CITYNAME,
                                                     countryId = x.COUNTRYID,
                                                     constructionDate = x.CONSTRUCTIONDATE,
                                                     propertyAddress = x.PROPERTYADDRESS,
                                                     dateOfAcquisition = x.DATEOFACQUISITION,
                                                     lastValuationDate = x.LASTVALUATIONDATE,
                                                     valuerId = x.VALUERID,
                                                     valuerName = context.TBL_COLLATERAL_VALUER.Where(c => c.COLLATERALVALUERID == x.VALUERID).FirstOrDefault().NAME,
                                                     valuerReferenceNumber = x.VALUERREFERENCENUMBER,
                                                     propertyValueBaseTypeId = x.PROPERTYVALUEBASETYPEID,
                                                     openMarketValue = x.OPENMARKETVALUE,
                                                     estimatedValue = x.ESTIMATEDVALUE,
                                                     securityValue = (decimal)x.SECURITYVALUE,
                                                     collateralUsableAmount = x.COLLATERALUSABLEAMOUNT,
                                                     remark = x.REMARK,
                                                     isAssetPledgedByThirdParty = x.ISASSETPLEDGEDBYTHRIDPARTY,
                                                     thirdPartyName = x.THRIDPARTYNAME,
                                                     isAssetManagedByTrustee = x.ISASSETMANAGEDBYTRUSTEE,
                                                     trusteeName = x.TRUSTEENAME,
                                                     stateName = x.TBL_STATE.STATENAME,
                                                     localGovtName = x.TBL_LOCALGOVERNMENT.NAME,
                                                     bankShareOfCollateral = x.BANKSHAREOFCOLLATERAL,
                                                 }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.TreasuryBillsAndBonds)
                {
                    record.collateralMarketableSecurity = (from x in context.TBL_COLLATERAL_MKT_SECURITY.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                                           select new CollateralMarketableSecurityViewModel
                                                           {
                                                               collateralMarketableSecurityId = x.COLLATERALMARKETABLESECURITYID,
                                                               collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                               collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                               collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                                  x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                                  .FirstOrDefault().COLLATERALSUBTYPENAME,
                                                               securityType = x.SECURITYTYPE,
                                                               //    dealReferenceNumber = x.DEALREFERENCENUMBER,
                                                               effectiveDate = x.EFFECTIVEDATE,
                                                               maturityDate = x.MATURITYDATE,
                                                               dealAmount = x.DEALAMOUNT,
                                                               lienUsableAmount = x.LIENUSABLEAMOUNT,
                                                               issuerName = x.ISSUERNAME,
                                                               issuerReferenceNumber = x.ISSUERREFERENCENUMBER,
                                                               unitValue = x.UNITVALUE,
                                                               numberOfUnits = x.NUMBEROFUNITS,
                                                               rating = x.RATING,
                                                               percentageInterest = x.PERCENTAGEINTEREST,
                                                               interestPaymentFrequency = x.INTERESTPAYMENTFREQUENCY,
                                                               securityValue = x.SECURITYVALUE,
                                                               remark = x.REMARK,
                                                           }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.Gaurantee)
                {
                    record.collateralGaurantee = (from x in context.TBL_COLLATERAL_GAURANTEE.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                                  select new CollateralGauranteeViewModel
                                                  {
                                                      collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                      collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                      collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                         x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                                                  .FirstOrDefault().COLLATERALSUBTYPENAME,
                                                      // isOwnedByCustomer = x.ISOWNEDBYCUSTOMER,
                                                      collateralGauranteeId = x.COLLATERALGAURANTEEID,
                                                      institutionName = x.INSTITUTIONNAME,
                                                      guarantorAddress = x.GUARANTORADDRESS,
                                                      //  guarantorReferenceNumber = x.GUARANTORREFERENCENUMBER,
                                                      guaranteeValue = x.GUARANTEEVALUE,
                                                      startDate = x.STARTDATE,
                                                      endDate = x.ENDDATE,
                                                      remark = x.REMARK,
                                                  }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.PlantAndMachinery)
                {
                    record.collateralEquipment = (from x in context.TBL_COLLATERAL_PLANT_AND_EQUIP.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                                  select new CollateralPlantsAndEquipmentViewModel
                                                  {
                                                      collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                      collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                      collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                         x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                                                  .FirstOrDefault().COLLATERALSUBTYPENAME,

                                                      collateralMachineDetailId = x.COLLATERALMACHINEDETAILID,
                                                      machineName = x.MACHINENAME,
                                                      description = x.DESCRIPTION,
                                                      machineNumber = x.MACHINENUMBER,
                                                      manufacturerName = x.MANUFACTURERNAME,
                                                      yearOfManufacture = x.YEAROFMANUFACTURE,
                                                      yearOfPurchase = x.YEAROFPURCHASE,
                                                      valueBaseTypeId = x.VALUEBASETYPEID,
                                                      valueBaseTypeName = x.TBL_COLLATERAL_VALUEBASE_TYPE.VALUEBASETYPENAME,
                                                      machineCondition = x.MACHINECONDITION,
                                                      machineryLocation = x.MACHINERYLOCATION,
                                                      replacementValue = x.REPLACEMENTVALUE,
                                                      equipmentSize = x.EQUIPMENTSIZE,
                                                      intendedUse = x.INTENDEDUSE,
                                                  }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.Vehicle)
                {
                    record.collateralVehicle = (from x in context.TBL_COLLATERAL_VEHICLE.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                                select new CollateralVehicleViewModel
                                                {
                                                    collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                    collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                    collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                       x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                                                .FirstOrDefault().COLLATERALSUBTYPENAME,
                                                    collateralVehicleId = x.COLLATERALVEHICLEID,
                                                    vehicleType = x.VEHICLETYPE,
                                                    vehicleStatus = x.VEHICLESTATUS,
                                                    vehicleMake = x.VEHICLEMAKE,
                                                    modelName = x.MODELNAME,
                                                    dateOfManufacture = x.MANUFACTUREDDATE.Value,
                                                    registrationNumber = x.REGISTRATIONNUMBER,
                                                    serialNumber = x.REGISTRATIONNUMBER,
                                                    chasisNumber = x.CHASISNUMBER,
                                                    engineNumber = x.ENGINENUMBER,
                                                    nameOfOwner = x.NAMEOFOWNER,
                                                    registrationCompany = x.REGISTRATIONCOMPANY,
                                                    resaleValue = x.RESALEVALUE,
                                                    valuationDate = x.VALUATIONDATE,
                                                    lastValuationAmount = x.LASTVALUATIONAMOUNT,
                                                    invoiceValue = x.INVOICEVALUE,
                                                    remark = x.REMARK,
                                                }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.MarketableSecurities_Shares)
                {
                    record.collateralStock = (from x in context.TBL_COLLATERAL_STOCK.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                              select new CollateralStockViewModel
                                              {
                                                  collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                  collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                  collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                     x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                                              .FirstOrDefault().COLLATERALSUBTYPENAME,
                                                  collateralStockId = x.COLLATERALSTOCKID,
                                                  companyName = x.COMPANYNAME,
                                                  shareQuantity = x.SHAREQUANTITY,
                                                  marketPrice = x.MARKETPRICE,
                                                  amount = x.AMOUNT,
                                                  shareSecurityValue = x.SHARESSECURITYVALUE,
                                                  shareValueAmountToUse = x.SHAREVALUEAMOUNTTOUSE,
                                              }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.PreciousMetal)
                {
                    record.collateralPreciousMetal = (from x in context.TBL_COLLATERAL_PRECIOUSMETAL.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                                      select new CollateralPreciousMetalViewModel
                                                      {
                                                          collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                          collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                          collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                             x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                                                      .FirstOrDefault().COLLATERALSUBTYPENAME,
                                                          collateralPreciousMetalId = x.COLLATERALPRECIOUSMETALID,
                                                          //  isOwnedByCustomer = x.ISOWNEDBYCUSTOMER,
                                                          preciousMetalName = x.PRECIOUSMETALNAME,
                                                          weightInGrammes = x.WEIGHTINGRAMMES,
                                                          valuationAmount = x.VALUATIONAMOUNT,
                                                          unitRate = x.UNITRATE,
                                                          preciousMetalForm = x.PRECIOUSMETALFORM,
                                                          remark = x.REMARK,
                                                      }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.InsurancePolicy)
                {
                    record.collateralInsurancePolicy = (from x in context.TBL_COLLATERAL_POLICY.Where(s => s.COLLATERALCUSTOMERID == record.collateralId)
                                                        select new CollateralInsurancePolicyViewModel
                                                        {
                                                            collateralCustomerId = x.COLLATERALCUSTOMERID,
                                                            collateralSubTypeId = x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID,
                                                            collateralSubTypeName = context.TBL_COLLATERAL_TYPE_SUB.Where(t => t.COLLATERALSUBTYPEID ==
                                                                                                                               x.TBL_COLLATERAL_CUSTOMER.COLLATERALSUBTYPEID)
                                                                                                                                                        .FirstOrDefault().COLLATERALSUBTYPENAME,

                                                            collateralInsurancePolicyId = x.COLLATERALINSURANCEPOLICYID,
                                                            isOwnedByCustomer = x.ISOWNEDBYCUSTOMER,
                                                            //           insurancePolicyNumber = x.INSURANCEPOLICYNUMBER,
                                                            premiumAmount = x.PREMIUMAMOUNT,
                                                            policyAmount = x.POLICYAMOUNT,
                                                            insuranceCompanyName = x.INSURANCECOMPANYNAME,
                                                            insurerAddress = x.INSURERADDRESS,
                                                            policyStartDate = x.POLICYSTARTDATE,
                                                            assignDate = x.ASSIGNDATE,
                                                            renewalFrequencyTypeId = x.RENEWALFREQUENCYTYPEID,
                                                            renewalFrequency = x.TBL_FREQUENCY_TYPE.MODE,
                                                            insurerDetails = x.INSURERDETAILS,
                                                            policyRenewalDate = x.POLICYRENEWALDATE,
                                                            remark = x.REMARK,
                                                        }).FirstOrDefault();

                }
            }

            return collateral;
        }

        public decimal GetAccountLeinAmountForFD(string accountNumber)
        {
            return context.TBL_COLLATERAL_DEPOSIT.Where(x => x.ACCOUNTNUMBER == accountNumber).Select(x => x.LIENAMOUNT).FirstOrDefault();
        }

        public decimal GetAccountLeinAmountForCASA(string accountNumber)
        {
            return context.TBL_COLLATERAL_CASA.Where(x => x.ACCOUNTNUMBER == accountNumber).Select(x => x.LIENAMOUNT).FirstOrDefault();
        }

        public CollateralHistory getCollateralHistory(int collateralId)
        {
            //var termLoanCollaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false)// && x.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved)
            //    .Join(context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility),
            //        c => c.COLLATERALCUSTOMERID, lc => lc.COLLATERALCUSTOMERID, (c, lc) => new { c, lc })
            //    .Join(context.TBL_LOAN, clc => clc.lc.LOANID, l => l.TERMLOANID, (clc, l) => new { clc, l }) // TBL_LOAN
            //    .Select(o => new CollateralHistoryList
            //    {
            //        customerName = o.l.TBL_CUSTOMER.FIRSTNAME + " " + o.l.TBL_CUSTOMER.MIDDLENAME + " " + o.l.TBL_CUSTOMER.LASTNAME,
            //        loanRef = o.l.LOANREFERENCENUMBER + o.l.TBL_PRODUCT.PRODUCTNAME,
            //        expirationDate = o.l.MATURITYDATE,
            //        collateralValue = o.clc.c.COLLATERALVALUE,
            //        outstandingPrincipal = o.l.OUTSTANDINGPRINCIPAL,
            //        totalOutstanding = o.l.OUTSTANDINGPRINCIPAL + o.l.OUTSTANDINGINTEREST,
            //        runningPrincipal = o.l.PRINCIPALAMOUNT,
            //        dateProposed = o.clc.lc.DATETIMECREATED,
            //        dateUsed = o.l.DISBURSEDATE,
            //        haircut = o.clc.c.HAIRCUT,
            //        exchangeRate = o.l.EXCHANGERATE,
            //        approvedLoanAmount = o.l.PRINCIPALAMOUNT,
            //    });

            //var odCollaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false)// && x.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved)
            //    .Join(context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility),
            //        c => c.COLLATERALCUSTOMERID, lc => lc.COLLATERALCUSTOMERID, (c, lc) => new { c, lc })
            //    .Join(context.TBL_LOAN_REVOLVING, clc => clc.lc.LOANID, l => l.REVOLVINGLOANID, (clc, l) => new { clc, l }) // TBL_LOAN_REVOLVING
            //    .Select(o => new CollateralHistoryList
            //    {
            //        customerName = o.l.TBL_CUSTOMER.FIRSTNAME + " " + o.l.TBL_CUSTOMER.MIDDLENAME + " " + o.l.TBL_CUSTOMER.LASTNAME,
            //        loanRef = o.l.LOANREFERENCENUMBER + o.l.TBL_PRODUCT.PRODUCTNAME,
            //        expirationDate = o.l.MATURITYDATE,
            //        collateralValue = o.clc.c.COLLATERALVALUE,
            //        outstandingPrincipal = o.l.OVERDRAFTLIMIT,
            //        totalOutstanding = o.l.OVERDRAFTLIMIT,
            //        runningPrincipal = o.l.OVERDRAFTLIMIT,
            //        dateProposed = o.clc.lc.DATETIMECREATED,
            //        dateUsed = o.l.DISBURSEDATE,
            //        haircut = o.clc.c.HAIRCUT,
            //        exchangeRate = o.l.EXCHANGERATE,
            //        approvedLoanAmount = o.l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
            //    });

            //var collaterals = new CollateralHistory();

            //collaterals.usage = termLoanCollaterals.Union(odCollaterals);
            //collaterals.totalAmountUsedByOutstanding = collaterals.usage.Sum(x => x.outstandingPrincipal);
            //collaterals.totalAmountUsedByPrincipal = collaterals.usage.Sum(x => x.approvedLoanAmount);
            //collaterals.collateralValue = collaterals.usage.Any() ? collaterals.usage.Max(x => x.collateralValue) : 0;
            //collaterals.availableValueByPrincipal = collaterals.collateralValue - collaterals.totalAmountUsedByPrincipal;
            //collaterals.availableValueByOutstanding = collaterals.collateralValue - collaterals.totalAmountUsedByOutstanding;

            /*
            var testL = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false).ToList();// && x.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved)
            var test = termLoanCollaterals.Union(odCollaterals).ToList();
            var testTL = termLoanCollaterals.ToList();
            var testOD = odCollaterals.ToList();
            */

            var termLoansTiedToCollateral = (from c in context.TBL_COLLATERAL_CUSTOMER
                                             join lc in context.TBL_LOAN_APPLICATION_COLLATERL on c.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                             join m in context.TBL_LOAN_COLLATERAL_MAPPING on lc.LOANAPPCOLLATERALID equals m.LOANAPPCOLLATERALID into lcm
                                             from m in lcm.DefaultIfEmpty()
                                             join l in context.TBL_LOAN on m.LOANID equals l.TERMLOANID into lcml
                                             from l in lcml.DefaultIfEmpty()
                                             where (m.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility)
                                             && c.COLLATERALCUSTOMERID == collateralId && c.DELETED == false
                                             select new CollateralHistoryList
                                             {
                                                 customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                                 loanRef = l.LOANREFERENCENUMBER + l.TBL_PRODUCT.PRODUCTNAME,
                                                 expirationDate = l.MATURITYDATE,
                                                 collateralValue = c.COLLATERALVALUE,
                                                 amountInUse = lc.COLLATERALCOVERAGE,
                                                 outstandingPrincipal = l.OUTSTANDINGPRINCIPAL,
                                                 totalOutstanding = l.OUTSTANDINGPRINCIPAL + l.OUTSTANDINGINTEREST,
                                                 runningPrincipal = l.PRINCIPALAMOUNT,
                                                 dateProposed = lc.DATETIMECREATED,
                                                 dateUsed = l.DISBURSEDATE,
                                                 haircut = c.HAIRCUT,
                                                 exchangeRate = l.EXCHANGERATE,
                                                 approvedLoanAmount = l.PRINCIPALAMOUNT,
                                             }).ToList();

            var odTiedToCollateral = (from c in context.TBL_COLLATERAL_CUSTOMER
                                      join lc in context.TBL_LOAN_APPLICATION_COLLATERL on c.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                      join m in context.TBL_LOAN_COLLATERAL_MAPPING on lc.LOANAPPCOLLATERALID equals m.LOANAPPCOLLATERALID into lcm
                                      from m in lcm.DefaultIfEmpty()
                                      join l in context.TBL_LOAN_REVOLVING on m.LOANID equals l.REVOLVINGLOANID into lcml
                                      from l in lcml.DefaultIfEmpty()
                                      where (m.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility || m == null)
                                      && c.COLLATERALCUSTOMERID == collateralId && c.DELETED == false
                                      select new CollateralHistoryList
                                      {
                                          customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                          loanRef = l.LOANREFERENCENUMBER + l.TBL_PRODUCT.PRODUCTNAME,
                                          expirationDate = l.MATURITYDATE,
                                          collateralValue = c.COLLATERALVALUE,
                                          amountInUse = lc.COLLATERALCOVERAGE,
                                          outstandingPrincipal = l.OVERDRAFTLIMIT,
                                          totalOutstanding = l.OVERDRAFTLIMIT,
                                          runningPrincipal = l.OVERDRAFTLIMIT,
                                          dateProposed = lc.DATETIMECREATED,
                                          dateUsed = l.DISBURSEDATE,
                                          haircut = c.HAIRCUT,
                                          exchangeRate = l.EXCHANGERATE,
                                          approvedLoanAmount = l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                                      }).ToList();

            var bondTiedtoCollateral = (from c in context.TBL_COLLATERAL_CUSTOMER
                                        join lc in context.TBL_LOAN_APPLICATION_COLLATERL on c.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                        join m in context.TBL_LOAN_COLLATERAL_MAPPING on lc.LOANAPPCOLLATERALID equals m.LOANAPPCOLLATERALID into lcm
                                        from m in lcm.DefaultIfEmpty()
                                        join l in context.TBL_LOAN_CONTINGENT on m.LOANID equals l.CONTINGENTLOANID into lcml
                                        from l in lcml.DefaultIfEmpty()
                                        where (m.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility || m == null)
                                        && c.COLLATERALCUSTOMERID == collateralId && c.DELETED == false
                                        select new CollateralHistoryList
                                        {
                                            customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                            //customerGroupName = l.cu
                                            loanRef = l.LOANREFERENCENUMBER + l.TBL_PRODUCT.PRODUCTNAME,
                                            expirationDate = l.MATURITYDATE,
                                            collateralValue = c.COLLATERALVALUE,
                                            amountInUse = lc.COLLATERALCOVERAGE,
                                            outstandingPrincipal = l.CONTINGENTAMOUNT,
                                            totalOutstanding = l.CONTINGENTAMOUNT,
                                            runningPrincipal = l.CONTINGENTAMOUNT,
                                            dateProposed = lc.DATETIMECREATED,
                                            dateUsed = l.DISBURSEDATE,
                                            haircut = c.HAIRCUT,
                                            exchangeRate = l.EXCHANGERATE,
                                            approvedLoanAmount = l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                                        }).ToList();

            var collaterals = new CollateralHistory();

            collaterals.usage = termLoansTiedToCollateral.Union(odTiedToCollateral).Union(bondTiedtoCollateral);
            collaterals.totalAmountUsedByOutstanding = collaterals.usage.Sum(x => x.outstandingPrincipal);
            collaterals.totalAmountUsedByPrincipal = collaterals.usage.Sum(x => x.approvedLoanAmount);
            collaterals.collateralValue = collaterals.usage.Any() ? collaterals.usage.Max(x => x.collateralValue) : 0;
            collaterals.availableValueByPrincipal = collaterals.collateralValue - collaterals.totalAmountUsedByPrincipal;
            collaterals.availableValueByOutstanding = collaterals.collateralValue - collaterals.totalAmountUsedByOutstanding;

            return collaterals;
        }

        public CollateralHistory getCollateralHistoryUsage(int collateralId)
        {

            var termLoansTiedToCollateral = (from c in context.TBL_COLLATERAL_CUSTOMER
                                             join lc in context.TBL_LOAN_APPLICATION_COLLATERL on c.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                             //join m in context.TBL_LOAN_COLLATERAL_MAPPING on lc.LOANAPPCOLLATERALID equals m.LOANAPPCOLLATERALID into lcm
                                             //from m in lcm.DefaultIfEmpty()
                                             join l in context.TBL_LOAN on lc.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID into lcml
                                             from l in lcml.DefaultIfEmpty()
                                             where 
                                             c.COLLATERALCUSTOMERID == collateralId 
                                             && c.DELETED == false
                                             && l.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility
                                             select new CollateralHistoryList
                                             {
                                                 customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                                 loanRef = l.LOANREFERENCENUMBER + l.TBL_PRODUCT.PRODUCTNAME,
                                                 expirationDate = l.MATURITYDATE == null ? DateTime.Now : l.MATURITYDATE,
                                                 collateralValue = c.COLLATERALVALUE,
                                                 amountInUse = lc.COLLATERALCOVERAGE,
                                                 outstandingPrincipal = l.OUTSTANDINGPRINCIPAL < 1 ? 0 : l.OUTSTANDINGPRINCIPAL,
                                                 //totalOutstanding = l.OUTSTANDINGPRINCIPAL + l.OUTSTANDINGINTEREST,
                                                 runningPrincipal = l.PRINCIPALAMOUNT < 1 ? 0 : l.PRINCIPALAMOUNT,
                                                 dateProposed = lc.DATETIMECREATED == null ? DateTime.Now : lc.DATETIMECREATED,
                                                 dateUsed = l.DISBURSEDATE == null ? DateTime.Now : l.DISBURSEDATE,
                                                 haircut = c.HAIRCUT < 1 ? 0 : c.HAIRCUT,
                                                 exchangeRate = l.EXCHANGERATE < 1 ? 0 : l.EXCHANGERATE,
                                                 approvedLoanAmount = l.PRINCIPALAMOUNT < 1 ? 0 : l.PRINCIPALAMOUNT,
                                                 lastVisitationDate = context.TBL_COLLATERAL_VISITATION.Where(x=>x.COLLATERALCUSTOMERID == c.COLLATERALCUSTOMERID).Select(x=>x.VISITATIONDATE).FirstOrDefault(),
                                             }).ToList();

            var odTiedToCollateral = (from c in context.TBL_COLLATERAL_CUSTOMER
                                      join lc in context.TBL_LOAN_APPLICATION_COLLATERL on c.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                      //join m in context.TBL_LOAN_COLLATERAL_MAPPING on lc.LOANAPPCOLLATERALID equals m.LOANAPPCOLLATERALID into lcm
                                      //from m in lcm.DefaultIfEmpty()
                                      join l in context.TBL_LOAN_REVOLVING on lc.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID into lcml
                                      from l in lcml.DefaultIfEmpty()
                                      where c.COLLATERALCUSTOMERID == collateralId
                                             && c.DELETED == false
                                             && l.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility
                                      select new CollateralHistoryList
                                      {
                                          customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                          loanRef = l.LOANREFERENCENUMBER + l.TBL_PRODUCT.PRODUCTNAME,
                                          expirationDate = l.MATURITYDATE == null ? DateTime.Now : l.MATURITYDATE,
                                          collateralValue = c.COLLATERALVALUE < 1 ? 0 : c.COLLATERALVALUE,
                                          amountInUse = lc.COLLATERALCOVERAGE < 1 ? 0 : lc.COLLATERALCOVERAGE,
                                          outstandingPrincipal = l.OVERDRAFTLIMIT < 1 ? 0 : l.OVERDRAFTLIMIT,
                                          totalOutstanding = l.OVERDRAFTLIMIT < 1 ? 0 : l.OVERDRAFTLIMIT,
                                          runningPrincipal = l.OVERDRAFTLIMIT < 1 ? 0 : l.OVERDRAFTLIMIT,
                                          dateProposed = lc.DATETIMECREATED == null ? DateTime.Now : lc.DATETIMECREATED,
                                          dateUsed = l.DISBURSEDATE == null ? DateTime.Now : l.DISBURSEDATE,
                                          haircut = c.HAIRCUT < 1 ? 0 : c.HAIRCUT,
                                          exchangeRate = l.EXCHANGERATE < 1 ? 0 : l.EXCHANGERATE,
                                          approvedLoanAmount = l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT < 1 ? 0 : l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                                          lastVisitationDate = context.TBL_COLLATERAL_VISITATION.Where(x => x.COLLATERALCUSTOMERID == c.COLLATERALCUSTOMERID).Select(x => x.VISITATIONDATE).FirstOrDefault(),
                                      }).ToList();

            var bondTiedtoCollateral = (from c in context.TBL_COLLATERAL_CUSTOMER
                                        join lc in context.TBL_LOAN_APPLICATION_COLLATERL on c.COLLATERALCUSTOMERID equals lc.COLLATERALCUSTOMERID
                                        //join m in context.TBL_LOAN_COLLATERAL_MAPPING on lc.LOANAPPCOLLATERALID equals m.LOANAPPCOLLATERALID into lcm
                                        //from m in lcm.DefaultIfEmpty()
                                        join l in context.TBL_LOAN_CONTINGENT on lc.LOANAPPLICATIONDETAILID equals l.LOANAPPLICATIONDETAILID into lcml
                                        from l in lcml.DefaultIfEmpty()
                                        where c.COLLATERALCUSTOMERID == collateralId
                                             && c.DELETED == false
                                             && l.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.ContingentLiability
                                        select new CollateralHistoryList
                                        {
                                            customerName = l.TBL_CUSTOMER.FIRSTNAME + " " + l.TBL_CUSTOMER.MIDDLENAME + " " + l.TBL_CUSTOMER.LASTNAME,
                                            loanRef = l.LOANREFERENCENUMBER + l.TBL_PRODUCT.PRODUCTNAME,
                                            expirationDate = l.MATURITYDATE == null ? DateTime.Now : l.MATURITYDATE,
                                            collateralValue = c.COLLATERALVALUE < 1 ? 0 : c.COLLATERALVALUE,
                                            amountInUse = lc.COLLATERALCOVERAGE < 1 ? 0 : lc.COLLATERALCOVERAGE,
                                            outstandingPrincipal = l.CONTINGENTAMOUNT < 1 ? 0 : l.CONTINGENTAMOUNT,
                                            totalOutstanding = l.CONTINGENTAMOUNT < 1 ? 0 : l.CONTINGENTAMOUNT,
                                            runningPrincipal = l.CONTINGENTAMOUNT < 1 ? 0 : l.CONTINGENTAMOUNT,
                                            dateProposed = lc.DATETIMECREATED == null ? DateTime.Now : lc.DATETIMECREATED,
                                            dateUsed = l.DISBURSEDATE == null ? DateTime.Now : l.DISBURSEDATE,
                                            haircut = c.HAIRCUT < 1 ? 0 : c.HAIRCUT,
                                            exchangeRate = l.EXCHANGERATE < 1 ? 0 : l.EXCHANGERATE,
                                            approvedLoanAmount = l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT < 1 ? 0 : l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                                            lastVisitationDate = context.TBL_COLLATERAL_VISITATION.Where(x => x.COLLATERALCUSTOMERID == c.COLLATERALCUSTOMERID).Select(x => x.VISITATIONDATE).FirstOrDefault(),
                                        }).ToList();

            var collaterals = new CollateralHistory();

            collaterals.usage = termLoansTiedToCollateral.Union(odTiedToCollateral).Union(bondTiedtoCollateral);
            collaterals.totalAmountUsedByOutstanding = collaterals.usage.Sum(x => x.outstandingPrincipal);
            collaterals.totalAmountUsedByPrincipal = collaterals.usage.Sum(x => x.approvedLoanAmount);
            collaterals.collateralValue = collaterals.usage.Any() ? collaterals.usage.Max(x => x.collateralValue) : 0;
            collaterals.availableValueByPrincipal = collaterals.collateralValue - collaterals.totalAmountUsedByPrincipal;
            collaterals.availableValueByOutstanding = collaterals.collateralValue - collaterals.totalAmountUsedByOutstanding;

            return collaterals;

        }


        // .....END OF COMPLETE COLLATERAL INFORMATION VIEW......
        #endregion Collateral Information View

        //STCK PRICE
        public IEnumerable<StockCompanyViewModel> getStockPrice()
        {
            var stock = (from c in context.TBL_STOCK_COMPANY
                         join p in context.TBL_STOCK_PRICE on c.STOCKID equals p.STOCKID
                         select new StockCompanyViewModel
                         {
                             stockId = c.STOCKID,
                             stockCode = c.STOCKCODE,
                             stockName = c.STOCKNAME,
                             stockPrice = p.STOCKPRICE

                         }).ToList();
            return stock;
        }

        public bool CheckForExpiredItemPolicies(DateTime currentDate)
        {
            var ExpiredPolicies = from x in context.TBL_COLLATERAL_ITEM_POLICY
                                  where x.ENDDATE > currentDate && x.HASEXPIRED == false
                                  select x;
            if (ExpiredPolicies != null)
            {
                foreach (var x in ExpiredPolicies)
                {
                    x.HASEXPIRED = true;
                    x.DATETIMEDELETED = DateTime.Now;
                }
            }

            if (context.SaveChanges() > 0) { return true; } else { return false; }

        }

        // immovableProperty collateral

        private void AddTempImmovablePropertyCollateral(int collateralId, CollateralViewModel entity)
        {

            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var property = (from x in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                                where x.COLLATERALCUSTOMERID == collateralId
                                select (x)).FirstOrDefault();

                if (property != null)
                {
                    if (property.PERFECTIONSTATUSID != entity.perfectionStatusId)
                    {
                        var collateral = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateralId);
                        NotifyForCollateralStatusUpdate(collateral, entity.perfectionStatusId);
                    }
                    if (property.LASTVALUATIONDATE != entity.lastValuationDate)
                    {
                        var collateral = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateralId);
                        NotifyForCollateralRevaluation(collateral, entity.lastValuationDate, valuationCycle: entity.valuationCycle);
                        NotifyForCollateralVisitation(collateral);
                    }
                    property.CITYID = entity.cityId;
                    property.COLLATERALUSABLEAMOUNT = entity.collateralUsableAmount;
                    property.CONSTRUCTIONDATE = entity.constructionDate;
                    property.COUNTRYID = (short)entity.countryId;
                    property.DATEOFACQUISITION = entity.dateOfAcquisition;
                    property.FORCEDSALEVALUE = entity.forcedSaleValue;
                    property.LASTVALUATIONDATE = entity.lastValuationDate;
                    //property.NEXTVALUATIONDATE = entity.nextValuationDate;
                    property.LATITUDE = entity.latitude;
                    property.LONGITUDE = entity.longitude;
                    property.NEARESTBUSSTOP = entity.nearestBusStop;
                    property.NEARESTLANDMARK = entity.nearestLandMark;
                    property.OPENMARKETVALUE = entity.openMarketValue;
                    property.PERFECTIONSTATUSID = (byte)entity.perfectionStatusId;
                    property.PERFECTIONSTATUSREASON = entity.perfectionStatusReason;
                    property.PROPERTYADDRESS = entity.propertyAddress;
                    property.PROPERTYNAME = entity.propertyName;
                    property.PROPERTYVALUEBASETYPEID = entity.propertyValueBaseTypeId;
                    property.REMARK = entity.remark;
                    property.SECURITYVALUE = entity.securityValue;
                    property.STAMPTOCOVER = entity.stampToCover;
                    property.VALUATIONAMOUNT = entity.valuationAmount;
                    property.VALUERID = entity.valuerId;
                    property.VALUERREFERENCENUMBER = entity.valuerReferenceNumber;
                    property.ISOWNEROCCUPIED = entity.isOwnerOccupied;
                    property.ISRESIDENTIAL = entity.isResidential;
                    property.ISASSETPLEDGEDBYTHRIDPARTY = entity.isAssetPledgedByThirdParty;
                    property.THRIDPARTYNAME = entity.thirdPartyName;
                    property.ISASSETMANAGEDBYTRUSTEE = entity.isAssetManagedByTrustee;
                    property.TRUSTEENAME = entity.trusteeName;
                    property.STATEID = entity.stateId;
                    property.LOCALGOVERNMENTID = entity.localGovernmentId;
                    property.BANKSHAREOFCOLLATERAL = entity.bankShareOfCollateral;
                    property.ESTIMATEDVALUE = entity.estimatedValue;
                    comment = $"Property collateral type has been updated through loan application by {entity.createdBy} staffid";

                    property.VALUERNAME = entity.valuerName;
                    property.VALUERACCOUNTNUMBER = entity.valuerAccountNumber;
                    //if (entity.valuerId == 72) {
                    //}

                    return;
                }
                else
                {
                    var prop = new TBL_COLLATERAL_IMMOVE_PROPERTY
                    {

                        CITYID = entity.cityId,
                        COLLATERALCUSTOMERID = collateralId,
                        COLLATERALUSABLEAMOUNT = entity.collateralUsableAmount,
                        CONSTRUCTIONDATE = entity.constructionDate,
                        COUNTRYID = (short)entity.countryId,
                        DATEOFACQUISITION = entity.dateOfAcquisition,
                        FORCEDSALEVALUE = entity.forcedSaleValue,
                        LASTVALUATIONDATE = entity.lastValuationDate,
                        //NEXTVALUATIONDATE = entity.nextValuationDate,
                        LATITUDE = entity.latitude,
                        LONGITUDE = entity.longitude,
                        NEARESTBUSSTOP = entity.nearestBusStop,
                        NEARESTLANDMARK = entity.nearestLandMark,
                        OPENMARKETVALUE = entity.openMarketValue,
                        PERFECTIONSTATUSID = (byte)entity.perfectionStatusId,
                        PERFECTIONSTATUSREASON = entity.perfectionStatusReason,
                        PROPERTYADDRESS = entity.propertyAddress,
                        PROPERTYNAME = entity.propertyName,
                        PROPERTYVALUEBASETYPEID = entity.propertyValueBaseTypeId,
                        REMARK = entity.remark,
                        SECURITYVALUE = entity.securityValue,
                        STAMPTOCOVER = entity.stampToCover,
                        VALUATIONAMOUNT = entity.valuationAmount,
                        VALUERID = entity.valuerId,
                        VALUERREFERENCENUMBER = entity.valuerReferenceNumber,
                        ISOWNEROCCUPIED = entity.isOwnerOccupied,
                        ISRESIDENTIAL = entity.isResidential,
                        ISASSETPLEDGEDBYTHRIDPARTY = entity.isAssetPledgedByThirdParty,
                        THRIDPARTYNAME = entity.thirdPartyName,
                        ISASSETMANAGEDBYTRUSTEE = entity.isAssetManagedByTrustee,
                        TRUSTEENAME = entity.trusteeName,
                        STATEID = entity.stateId,
                        LOCALGOVERNMENTID = entity.localGovernmentId,
                        BANKSHAREOFCOLLATERAL = entity.bankShareOfCollateral,
                        ESTIMATEDVALUE = entity.estimatedValue,

                        VALUERNAME = entity.valuerName,
                        VALUERACCOUNTNUMBER = entity.valuerAccountNumber,

                    };
                    context.TBL_COLLATERAL_IMMOVE_PROPERTY.Add(prop);
                    comment = $"New property collateral type has been created through loan application by {entity.createdBy} staffid";
                    if (context.SaveChanges() != 0)
                    {
                        var collateral = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateralId);
                        NotifyForCollateralStatusUpdate(collateral, entity.perfectionStatusId);
                        NotifyForCollateralStatusUpdate(collateral, entity.perfectionStatusId);
                        NotifyForCollateralRevaluation(collateral, entity.lastValuationDate, valuationCycle: entity.valuationCycle);
                        NotifyForCollateralVisitation(collateral);
                    }

                }
            }
            else
            {
                context.TBL_TEMP_COLLATERAL_IMMOV_PROP.Add(new TBL_TEMP_COLLATERAL_IMMOV_PROP
                {
                    TEMPCOLLATERALCUSTOMERID = collateralId,
                    PROPERTYNAME = entity.propertyName,
                    CITYID = (int)entity.cityId,
                    COUNTRYID = (short)entity.countryId,
                    CONSTRUCTIONDATE = entity.constructionDate,
                    PROPERTYADDRESS = entity.propertyAddress,
                    DATEOFACQUISITION = entity.dateOfAcquisition,
                    LASTVALUATIONDATE = entity.lastValuationDate,
                    //NEXTVALUATIONDATE = entity.nextValuationDate,
                    VALUERID = entity.valuerId,
                    VALUERREFERENCENUMBER = entity.valuerReferenceNumber,
                    PROPERTYVALUEBASETYPEID = entity.propertyValueBaseTypeId,
                    OPENMARKETVALUE = entity.openMarketValue,
                    FORCEDSALEVALUE = entity.forcedSaleValue,
                    STAMPTOCOVER = entity.stampToCover,
                    SECURITYVALUE = entity.securityValue,
                    COLLATERALUSABLEAMOUNT = entity.collateralUsableAmount,
                    REMARK = entity.remark,
                    NEARESTLANDMARK = entity.nearestLandMark,
                    NEARESTBUSSTOP = entity.nearestBusStop,
                    LONGITUDE = entity.longitude,
                    LATITUDE = entity.latitude,
                    PERFECTIONSTATUSID = (byte)entity.perfectionStatusId,
                    PERFECTIONSTATUSREASON = entity.perfectionStatusReason,
                    VALUATIONAMOUNT = entity.valuationAmount,
                    ISOWNEROCCUPIED = entity.isOwnerOccupied,
                    ISRESIDENTIAL = entity.isResidential,
                    ISASSETPLEDGEDBYTHRIDPARTY = entity.isAssetPledgedByThirdParty,
                    THRIDPARTYNAME = entity.thirdPartyName,
                    ISASSETMANAGEDBYTRUSTEE = entity.isAssetManagedByTrustee,
                    TRUSTEENAME = entity.trusteeName,
                    STATEID = entity.stateId,
                    LOCALGOVERNMENTID = entity.localGovernmentId,
                    BANKSHAREOFCOLLATERAL = entity.bankShareOfCollateral,
                    ESTIMATEDVALUE = entity.estimatedValue,

                    VALUERNAME = entity.valuerName,
                    VALUERACCOUNTNUMBER = entity.valuerAccountNumber,


                });
                comment = $"New temp property collateral type has been cretated by {entity.createdBy} staffid";
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = collateralId;
                workflow.Comment = comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();
            }


        }

        private void AddTempCasaCollateral(int collateralId, CollateralViewModel entity)
        {
            CasaBalanceViewModel casaDetail;
            string errorDesc = "";
            var comment = string.Empty;

            if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainCasa = (from x in context.TBL_COLLATERAL_CASA
                                where x.COLLATERALCUSTOMERID == collateralId
                                select (x)).FirstOrDefault();

                if (mainCasa != null)
                {

                    mainCasa.ACCOUNTNUMBER = entity.accountNumber;
                    mainCasa.AVAILABLEBALANCE = entity.availableBalance;
                    mainCasa.EXISTINGLIENAMOUNT = entity.existingLienAmount;
                    //mainCasa.ISOWNEDBYCUSTOMER = tempCasa.ISOWNEDBYCUSTOMER;
                    mainCasa.LIENAMOUNT = entity.lienAmount;
                    mainCasa.REMARK = entity.remark;
                    mainCasa.SECURITYVALUE = (decimal)entity.securityValue;
                    mainCasa.ACCOUNTNAME = entity.accountName;
                    comment = $"New CASA collateral type has been updated through loan application by {entity.createdBy} staffid";
                }
                else
                {
                    casaDetail = (casa.GetCASABalance(entity.collateralCode, entity.companyId));

                    if (casaDetail.isCasaAccountDetailAvailable == false)
                    {
                        if (casaDetail.errorMessage != null)
                        {
                            if (casaDetail.accountName != null)
                            {
                                var error = JsonConvert.DeserializeObject<List<API_Error>>(casaDetail.accountName);
                                foreach (var a in error)
                                    errorDesc = a.errorDescription;
                                throw new APIErrorException(errorDesc);
                            }
                        }
                    }
                    else
                    {
                        context.TBL_COLLATERAL_CASA.Add(new TBL_COLLATERAL_CASA
                        {
                            ACCOUNTNUMBER = entity.accountNumber,
                            AVAILABLEBALANCE = entity.availableBalance,
                            COLLATERALCUSTOMERID = collateralId,
                            EXISTINGLIENAMOUNT = entity.existingLienAmount,
                            // ISOWNEDBYCUSTOMER = tempCasa.ISOWNEDBYCUSTOMER,
                            LIENAMOUNT = entity.lienAmount,
                            REMARK = entity.remark,
                            SECURITYVALUE = (decimal)entity.securityValue,
                            ACCOUNTNAME = entity.accountName,
                        });
                        comment = $"New CASA collateral type has been created through loan application by {entity.createdBy} staffid";

                    }
                }
            }

            else
            {
                casaDetail = (casa.GetCASABalance(entity.collateralCode, entity.companyId));

                if (casaDetail.isCasaAccountDetailAvailable == false)
                {
                    if (casaDetail.errorMessage != null)
                    {
                        if (casaDetail.accountName != null)
                        {
                            var error = JsonConvert.DeserializeObject<List<API_Error>>(casaDetail.accountName);
                            foreach (var a in error)
                                errorDesc = a.errorDescription;
                            throw new APIErrorException(errorDesc);
                        }
                    }
                }
                else
                {
                    context.TBL_TEMP_COLLATERAL_CASA.Add(new TBL_TEMP_COLLATERAL_CASA
                    {
                        TEMPCOLLATERALCUSTOMERID = collateralId,
                        ACCOUNTNUMBER = entity.collateralCode,
                        AVAILABLEBALANCE = casaDetail.availableBalance,
                        LIENAMOUNT = entity.lienAmount,
                        SECURITYVALUE = (decimal)entity.securityValue,
                        REMARK = entity.remark,
                        ACCOUNTNAME = entity.accountName,
                        EXISTINGLIENAMOUNT = entity.existingLienAmount
                    });
                    comment = $"New Temp CASA collateral type has been created by {entity.createdBy} staffid";
                    workflow.StaffId = entity.createdBy;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = collateralId;
                    workflow.Comment = comment;
                    workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                    workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();

                }

            }


        }

        // FIX DEPOSIT collateral

        private void AddTempDepositCollateral(int collateralId, CollateralViewModel entity)
        {
            TDAccountRecordViewModel finacleBalance;
            TwoFactorAutheticationViewModel twoFADetails = new TwoFactorAutheticationViewModel();
            var comment = string.Empty;
            try
            {
                if (entity.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
                {
                    var mainDeposit = (from x in context.TBL_COLLATERAL_DEPOSIT
                                       where x.COLLATERALCUSTOMERID == collateralId
                                       select (x)).FirstOrDefault();

                    if (mainDeposit != null)
                    {

                        mainDeposit.ACCOUNTNUMBER = entity.accountNumber;
                        mainDeposit.AVAILABLEBALANCE = entity.availableBalance;
                        mainDeposit.BANK = entity.bank;
                        mainDeposit.DEALREFERENCENUMBER = entity.dealReferenceNumber;
                        mainDeposit.EFFECTIVEDATE = entity.effectiveDate;
                        mainDeposit.EXISTINGLIENAMOUNT = entity.existingLienAmount;
                        mainDeposit.LIENAMOUNT = entity.lienAmount;
                        mainDeposit.MATURITYAMOUNT = entity.maturityAmount;
                        mainDeposit.MATURITYDATE = entity.maturityDate;
                        mainDeposit.REMARK = entity.remark;
                        mainDeposit.SECURITYVALUE = (decimal)entity.securityValue;
                        mainDeposit.ACCOUNTNAME = entity.accountName;
                        comment = $"New Fixed Deposit collateral type has been updated through loan application by {entity.createdBy} staffid";
                    }
                    else
                    {
                        var branch = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == entity.customerId).Select(x => x.BRANCHID).FirstOrDefault();

                        CasaLienViewModel model = new CasaLienViewModel
                        {
                            productAccountNumber = entity.collateralCode,
                            lienAmount = (decimal)entity.securityValue,
                            description = "Term deposit collateral creation",
                            lienTypeId = (int)LienTypeEnum.CollateralCreation,
                            sourceReferenceNumber = entity.collateralCode,
                            dateTimeCreated = DateTime.Now,
                            createdBy = entity.createdBy,
                            companyId = entity.companyId,
                            branchId = branch,
                            isTermDeposit = true,

                        };

                        finacleBalance = finacle.ValidateTDAccountNumber(model.productAccountNumber);
                        if (finacleBalance != null)
                        {
                            model.currencyCode = finacleBalance.currencyType;
                        }

                        twoFADetails.passcode = entity.passCode;
                        twoFADetails.username = entity.username;

                        lien.PlaceLien(model, twoFADetails);


                        context.TBL_COLLATERAL_DEPOSIT.Add(new TBL_COLLATERAL_DEPOSIT
                        {
                            ACCOUNTNUMBER = entity.accountNumber,
                            AVAILABLEBALANCE = entity.availableBalance,
                            BANK = entity.bank,
                            COLLATERALCUSTOMERID = collateralId,
                            DEALREFERENCENUMBER = entity.dealReferenceNumber,
                            EFFECTIVEDATE = entity.effectiveDate,
                            EXISTINGLIENAMOUNT = entity.existingLienAmount,
                            LIENAMOUNT = entity.lienAmount,
                            MATURITYAMOUNT = entity.maturityAmount,
                            MATURITYDATE = entity.maturityDate,
                            REMARK = entity.remark,
                            SECURITYVALUE = (decimal)entity.securityValue,
                            ACCOUNTNAME = entity.accountName
                        });
                        comment = $"New Fixed Deposit collateral type has been created through loan application by {entity.createdBy} staffid";

                    }
                }
                else
                {
                    finacleBalance = finacle.ValidateTDAccountNumber(entity.collateralCode);

                    if (finacleBalance.isSuccess == false)
                    {
                        var error = finacleBalance.errorDesc + " Or Closed Account Number";
                        throw new ConditionNotMetException(error);
                    }
                    else
                    {
                        context.TBL_TEMP_COLLATERAL_DEPOSIT.Add(new TBL_TEMP_COLLATERAL_DEPOSIT
                        {
                            TEMPCOLLATERALCUSTOMERID = collateralId,
                            DEALREFERENCENUMBER = entity.dealReferenceNumber,
                            ACCOUNTNUMBER = entity.collateralCode,
                            ACCOUNTNAME = entity.accountName,
                            EXISTINGLIENAMOUNT = 0,
                            LIENAMOUNT = entity.lienAmount,
                            AVAILABLEBALANCE = finacleBalance.balance,
                            SECURITYVALUE = (decimal)entity.securityValue,
                            MATURITYDATE = entity.maturityDate,
                            MATURITYAMOUNT = 0,
                            EFFECTIVEDATE = entity.effectiveDate,
                            REMARK = entity.remark,
                            BANK = entity.bank,


                        });
                        comment = $"New Temp Fixed Deposit collateral type has been created by {entity.createdBy} staffid";
                        workflow.StaffId = entity.createdBy;
                        workflow.CompanyId = entity.companyId;
                        workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                        workflow.TargetId = collateralId;
                        workflow.Comment = comment;
                        workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                        workflow.DeferredExecution = true;
                        workflow.ExternalInitialization = true;
                        workflow.LogActivity();
                    }


                }

            }
            catch (APIErrorException e)
            {
                throw new APIErrorException(e.Message);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public IEnumerable<CollateralViewModel> GetTempCustomerCollateralForApproval(int companyId, int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.CollateralApproval).ToList();

            var collaterals = (from x in context.TBL_TEMP_COLLATERAL_CUSTOMER
                               join c in context.TBL_COLLATERAL_TYPE on x.COLLATERALTYPEID equals c.COLLATERALTYPEID
                               join atrail in context.TBL_APPROVAL_TRAIL on x.TEMPCOLLATERALCUSTOMERID equals atrail.TARGETID
                               join a in context.TBL_CUSTOMER on x.CUSTOMERID equals a.CUSTOMERID
                               let ColSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(c => c.COLLATERALSUBTYPEID == x.COLLATERALSUBTYPEID).Select(c => c.COLLATERALSUBTYPENAME).FirstOrDefault()
                               where atrail.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved 
                                     && atrail.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved
                                     && x.ISCURRENT == true 
                                     && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved
                                     && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved 
                                     && atrail.RESPONSESTAFFID == null
                                     && atrail.OPERATIONID == (int)OperationsEnum.CollateralApproval
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                               orderby x.TEMPCOLLATERALCUSTOMERID descending
                               select new CollateralViewModel
                               {
                                   approvalTrailId = atrail.APPROVALTRAILID,
                                   collateralId = x.TEMPCOLLATERALCUSTOMERID,
                                   collateralTypeId = x.COLLATERALTYPEID,
                                   collateralSubTypeId = x.COLLATERALSUBTYPEID,
                                   customerId = x.CUSTOMERID.Value,
                                   currencyId = x.CURRENCYID,
                                   currency = x.TBL_CURRENCY.CURRENCYNAME,
                                   collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                   collateralSubTypeName = ColSubType,
                                   collateralCode = x.COLLATERALCODE,
                                   collateralValue = x.COLLATERALVALUE,
                                   camRefNumber = x.CAMREFNUMBER,
                                   allowSharing = x.ALLOWSHARING,
                                   isLocationBased = x.ISLOCATIONBASED,
                                   valuationCycle = x.VALUATIONCYCLE,
                                   haircut = x.HAIRCUT,
                                   approvalStatusName = x.APPROVALSTATUSID,
                                   //allowApplicationMapping = typeIds.Contains((short)x.COLLATERALTYPEID),
                                   requireInsurancePolicy = c.REQUIREINSURANCEPOLICY,
                                   dateTimeCreated = x.DATETIMECREATED,
                                   requireVisitation = c.REQUIREVISITATION,
                                   customerName = a.FIRSTNAME + " " + a.LASTNAME + " " + a.MAIDENNAME,

                               }).ToList();
            var data = collaterals.GroupBy(x => x.collateralId).Select(x => x.First()).ToList();

            return data;
        }

        public IEnumerable<CollateralViewModel> GetCustomerCollateralByCollateralId(int companyId, int collaterId)
        {

            var collaterals = (from x in context.TBL_COLLATERAL_CUSTOMER
                               join c in context.TBL_COLLATERAL_TYPE on x.COLLATERALTYPEID equals c.COLLATERALTYPEID
                               join a in context.TBL_CUSTOMER on x.CUSTOMERID equals a.CUSTOMERID
                               let ColSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(c => c.COLLATERALSUBTYPEID == x.COLLATERALSUBTYPEID).Select(c => c.COLLATERALSUBTYPENAME).FirstOrDefault()
                               where x.COLLATERALCUSTOMERID == collaterId
                               orderby x.COLLATERALCUSTOMERID descending
                               select new CollateralViewModel
                               {
                                   collateralId = x.COLLATERALCUSTOMERID,
                                   collateralTypeId = x.COLLATERALTYPEID,
                                   collateralSubTypeId = x.COLLATERALSUBTYPEID,
                                   customerId = x.CUSTOMERID.Value,
                                   currencyId = x.CURRENCYID,
                                   currency = x.TBL_CURRENCY.CURRENCYNAME,
                                   collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                   collateralSubTypeName = ColSubType,
                                   collateralCode = x.COLLATERALCODE,
                                   collateralValue = x.COLLATERALVALUE,
                                   camRefNumber = x.CAMREFNUMBER,
                                   allowSharing = x.ALLOWSHARING,
                                   isLocationBased = (bool)x.ISLOCATIONBASED,
                                   valuationCycle = x.VALUATIONCYCLE,
                                   haircut = x.HAIRCUT,
                                   requireInsurancePolicy = c.REQUIREINSURANCEPOLICY,
                                   dateTimeCreated = x.DATETIMECREATED,
                                   requireVisitation = c.REQUIREVISITATION,
                                   customerName = a.FIRSTNAME + " " + a.LASTNAME + " " + a.MAIDENNAME,
                                   customerCode = a.CUSTOMERCODE == null ? x.CUSTOMERCODE : a.CUSTOMERCODE,
                                   customerAccount = context.TBL_CASA.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => c.PRODUCTACCOUNTNUMBER).FirstOrDefault(),
                               }).ToList();

            return collaterals;

        }
        private int AddTempCollateralMainForm(CollateralViewModel model)
        {
            if (String.IsNullOrWhiteSpace(model.collateralCode) || String.IsNullOrEmpty(model.collateralCode))
            {
                var refNo = CommonHelpers.GenerateRandomDigitCode(7);
                model.collateralCode = refNo;
            }
            var customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == model.customerId).Select(x => x).FirstOrDefault();
            int collateralId = 0;
            DateTime date = DateTime.Now;
            var xchRate = repo.GetExchangeRate(date, model.currencyId, model.companyId);
            if (model.isRegistrationDoneViaLoanApplication == (int)CollateralRegistrationTypeEnum.isRegistrationDoneViaLoanApplication)
            {
                var mainCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCODE.Trim() == model.collateralCode.Trim()).Select(x => x).FirstOrDefault();
                
                if (mainCollateral != null)
                {

                    if (mainCollateral.VALIDTILL != model.validTill)
                    {
                        NotifyForCollateralValidity(mainCollateral, model.validTill);
                    }


                    mainCollateral.COLLATERALCODE = model.collateralCode;
                    //mainCollateral.COLLATERALTYPEID = model.collateralTypeId;
                    //mainCollateral.COLLATERALSUBTYPEID = model.collateralSubTypeId;
                    //mainCollateral.COLLATERALCODE = model.collateralCode;

                    mainCollateral.COLLATERALVALUE = (decimal)model.collateralValue;
                    //mainCollateral.COMPANYID = model.companyId;
                    mainCollateral.ALLOWSHARING = model.allowSharing;
                    mainCollateral.ISLOCATIONBASED = model.isLocationBased;
                    mainCollateral.VALUATIONCYCLE = model.valuationCycle;
                    mainCollateral.HAIRCUT = model.haircut;
                    mainCollateral.CURRENCYID = model.currencyId;
                    mainCollateral.VALIDTILL = model.validTill;
                    mainCollateral.EXCHANGERATE = repo.GetExchangeRate(DateTime.Now, model.currencyId, model.companyId).sellingRate;
                    mainCollateral.CUSTOMERCODE = customer.CUSTOMERCODE;
                    mainCollateral.CAMREFNUMBER = model.camRefNumber;
                    mainCollateral.LASTUPDATEDBY = model.createdBy;
                    mainCollateral.DATETIMEUPDATED = genSetup.GetApplicationDate();
                    mainCollateral.ACTEDONBY = model.createdBy;
                    mainCollateral.RELATEDCOLLATERALCODE = model.relatedCollateralCode;
                    mainCollateral.COLLATERALSUMMARY = model.collateralSummary;
                    context.SaveChanges();
                    collateralId = mainCollateral.COLLATERALCUSTOMERID;
                    return collateralId;
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(model.collateralCode) || String.IsNullOrEmpty(model.collateralCode))
                    {
                        var refNo = CommonHelpers.GenerateRandomDigitCode(7);
                        model.collateralCode = refNo;
                    }

                    var collateral = context.TBL_COLLATERAL_CUSTOMER.Add(new TBL_COLLATERAL_CUSTOMER
                    {
                        COLLATERALTYPEID = model.collateralTypeId,
                        COLLATERALSUBTYPEID = model.collateralSubTypeId,
                        COLLATERALCODE = model.collateralCode,
                        COLLATERALVALUE = (decimal)model.collateralValue,
                        COMPANYID = model.companyId,
                        ALLOWSHARING = model.allowSharing,
                        ISLOCATIONBASED = model.isLocationBased,
                        VALUATIONCYCLE = model.valuationCycle,
                        HAIRCUT = model.haircut,
                        CUSTOMERCODE = customer.CUSTOMERCODE,
                        CURRENCYID = model.currencyId,
                        EXCHANGERATE = repo.GetExchangeRate(date, model.currencyId, model.companyId).sellingRate,
                        CUSTOMERID = model.customerId,
                        CAMREFNUMBER = model.camRefNumber,
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        ACTEDONBY = model.createdBy,
                        RELATEDCOLLATERALCODE = model.relatedCollateralCode,
                        LOANAPPLICATIONID = model.loanApplicationId,
                        COLLATERALSUMMARY = model.collateralSummary,
                        COLLATERALUSAGESTATUSID = (int)CollateralUsageStatusEnum.Propose,
                        VALIDTILL = model.validTill,
                    });


                    //if (model.loanTypeId == 1)
                    //    collateral.CUSTOMERID = model.customerId;
                    //else if (model.loanTypeId == 2)
                    //    collateral.CUSTOMERGROUPID = model.customerGroupId;
                    //else
                    //    collateral.CUSTOMERID = model.customerId;                    

                    if (context.SaveChanges() > 0)
                    {
                        collateralId = collateral.COLLATERALCUSTOMERID;
                        NotifyForCollateralValidity(collateral, model.validTill, true);
                        return collateralId;
                    }

                }

            }
            else
            {
                if (context.TBL_TEMP_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCODE == model.collateralCode && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved && x.ISCURRENT != false).OrderByDescending(x => x.DATETIMECREATED).Any() == true)
                {
                    throw new SecureException("The specified Collateral is edited and is going through approval!");
                }

                if (String.IsNullOrWhiteSpace(model.collateralCode) || String.IsNullOrEmpty(model.collateralCode))
                {
                    var refNo = CommonHelpers.GenerateRandomDigitCode(7);
                    model.collateralCode = refNo;
                }

                if (model.validTill == null)
                {
                    model.validTill = null;
                }

                var collateral = context.TBL_TEMP_COLLATERAL_CUSTOMER.Add(new TBL_TEMP_COLLATERAL_CUSTOMER
                {
                    COLLATERALTYPEID = model.collateralTypeId,
                    COLLATERALSUBTYPEID = model.collateralSubTypeId,
                    COLLATERALCODE = model.collateralCode,
                    COLLATERALVALUE = (decimal)model.collateralValue,
                    COMPANYID = model.companyId,
                    ALLOWSHARING = model.allowSharing,
                    ISLOCATIONBASED = model.isLocationBased,
                    VALUATIONCYCLE = model.valuationCycle,
                    HAIRCUT = model.haircut,
                    CURRENCYID = model.currencyId,
                    EXCHANGERATE = xchRate.sellingRate,
                    CUSTOMERID = model.customerId,
                    CAMREFNUMBER = model.camRefNumber,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    ACTEDONBY = model.createdBy,
                    RELATEDCOLLATERALCODE = model.relatedCollateralCode,
                    //    LOANAPPLICATIONID = model.loanApplicationId,
                    COLLATERALSUMMARY = model.collateralSummary,
                    COLLATERALUSAGESTATUSID = (int)CollateralUsageStatusEnum.Propose,
                    ISCURRENT = true,
                    VALIDTILL = model.validTill,
                });

                //if (model.customerId > 0)
                //    //if (model.loanTypeId == 1)
                //    collateral.CUSTOMERID = model.customerId;
                //else if (model.customerGroupId > 0)
                ////else if (model.loanTypeId == 2)
                //        collateral.CUSTOMERGROUPID = model.customerGroupId;
                //else
                //    collateral.CUSTOMERID = model.customerId;


                if (context.SaveChanges() > 0)
                {
                    collateralId = collateral.TEMPCOLLATERALCUSTOMERID;
                    return collateralId;
                }
            }
            return collateralId;
        }



        public bool AddCollateralInsuranceTrackingForm(int accountOfficer, CollateralInsuranceTrackingViewModel model)
        {
                if (String.IsNullOrWhiteSpace(model.referenceNumber) || String.IsNullOrEmpty(model.referenceNumber))
                {
                var refNo = CommonHelpers.GenerateRandomDigitCode(7);
                model.referenceNumber = refNo;
                }

                var insurancePolicy = context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(x => x.POLICYNUMBER.Trim() == model.referenceNumber.Trim() && x.DELETED == false).Select(x => x).FirstOrDefault();

                if (insurancePolicy != null)
                {
                    throw new ConditionNotMetException("Insurance Reg/Ref Number Already Exists, Kindly enter a unique code or leave the field blank for Auto-Generation");

                }

                var insurancePolicy2 = context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(x => x.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId && x.COLLATERALCUSTOMERID == model.collateralCustomerId && x.DELETED == false).Select(x => x).FirstOrDefault();

                if (insurancePolicy2 != null)
                {
                    throw new ConditionNotMetException("Policy already captured and been assigned to this facility");

                }

            if (model.expiryDate <= model.startDate)
                throw new ConditionNotMetException("Insurance End Date must be greater than the Insurance Start Date");


            var insuranceTracking = context.TBL_COLLATERAL_INSURANCE_TRACKING.Add(new TBL_COLLATERAL_INSURANCE_TRACKING
                {
                    INSURANCECOMPANYID = model.insuranceCompanyId,
                    ISURANCECOMPANYADDRESS = model.companyAddress,
                    POLICYNUMBER = model.referenceNumber,
                    INSURANCESTARTDATE = model.startDate,
                    INSURANCEENDDATE = model.expiryDate,
                    SUMINSURED = model.sumInsured,
                    PREMIUMPAID = model.inSurPremiumAmount,
                    INSURANCESTATUSID = model.insuranceStatus,
                    COLLATERALCUSTOMERID = model.collateralCustomerId,
                    LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                    VALUATIONSTARTDATE = model.valuationStartDate,
                    VALUATIONENDDATE = model.valuationEndDate,
                    OMV = model.openMarketValue,
                    FSV = model.forcedSaleValue,
                    VALUERID = model.valuerId,
                    COLLATERALDETAILS = model.collateralDetails,
                    INSURANCEPOLICYTYPEID = model.insurancePolicyTypeId,
                    OTHERVALUER = model.otherValuer,
                    OTHERINSURANCECOMPANY = model.otherInsuranceCompany,
                    OTHERINSURANCEPOLICYTYPE = model.otherInsurancePolicyType,
                    COLLATERALTYPE = model.collateralTypeId,
                    COLLATERALSUBTYPE = model.collateralSubTypeId,
                    GPSCOORDINATES = model.gpsCoordinates,
                    FIRSTLOSSPAYEE = model.firstLossPayee,
                    INSURABLEVALUE = model.insurableValue,
                    COMMENT = model.comment,
                    CREATEDBY = accountOfficer,
                    DATETIMECREATED = DateTime.Now,
                    COLLATERALDESCRIPTION = model.collateralDetails,
                });

            

            if (context.SaveChanges() > 0)
               {
                return true;
               }
                

            return false;
        }

        public bool UpdateCollateralInsuranceTrackingForm(int accountOfficer, int id, CollateralInsuranceTrackingViewModel model)
        {


            if (id == 0)
            {
                throw new ConditionNotMetException("Tracking Reference ID is Null");

            }
            
                var cit = context.TBL_COLLATERAL_INSURANCE_TRACKING.Find(id);
                if (cit == null) { return false; }

                cit.INSURANCECOMPANYID = model.insuranceCompanyId;
                cit.ISURANCECOMPANYADDRESS = model.companyAddress;
                cit.POLICYNUMBER = model.referenceNumber;
                cit.INSURANCESTARTDATE = model.startDate;
                cit.INSURANCEENDDATE = model.expiryDate;
                cit.SUMINSURED = model.sumInsured;
                cit.PREMIUMPAID = model.inSurPremiumAmount;
                cit.INSURANCESTATUSID = model.insuranceStatus;
                cit.COLLATERALCUSTOMERID = model.collateralCustomerId;
                cit.LOANAPPLICATIONDETAILID = model.loanApplicationDetailId;
                cit.VALUATIONSTARTDATE = model.valuationStartDate;
                cit.VALUATIONENDDATE = model.valuationEndDate;
                cit.OMV = model.openMarketValue;
                cit.FSV = model.forcedSaleValue;
                cit.VALUERID = model.valuerId;
                cit.COLLATERALDETAILS = model.collateralDetails;
                cit.INSURANCEPOLICYTYPEID = model.insurancePolicyTypeId;
                cit.OTHERVALUER = model.otherValuer;
                cit.OTHERINSURANCECOMPANY = model.otherInsuranceCompany;
                cit.OTHERINSURANCEPOLICYTYPE = model.otherInsurancePolicyType;
                cit.COLLATERALTYPE = model.collateralTypeId;
                cit.COLLATERALSUBTYPE = model.collateralSubTypeId;
                cit.GPSCOORDINATES = model.gpsCoordinates;
                cit.FIRSTLOSSPAYEE = model.firstLossPayee;
                cit.INSURABLEVALUE = model.insurableValue;
                cit.COMMENT = model.comment;
                cit.UPDATEDBY = accountOfficer;
                cit.DATETIMEUPDATED = DateTime.Now;
                
                    if (context.SaveChanges() > 0)
                    {
                     return true;
                    }

            return false;
        }

        public bool GetCustomerCollateralInsuranceDetailsConfirmation(int getStaffId, int id)
        {

            if (id == 0)
            {
                throw new ConditionNotMetException("Tracking Reference ID is Null");

            }
            
            var cit = context.TBL_COLLATERAL_INSURANCE_TRACKING.Find(id);
            if (cit == null) { return false; }
            cit.ISINFORMATIONCONFIRMED = true;
            cit.INFORMATIONCONFIRMEDBY = getStaffId;

            var cit2 = context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(x=>x.LOANAPPLICATIONDETAILID == cit.LOANAPPLICATIONDETAILID).ToList();
            if(cit2.Count() > 0)
            {
                foreach(var i in cit2)
                {
                    i.ISINFORMATIONCONFIRMED = true;
                    i.INFORMATIONCONFIRMEDBY = getStaffId;
                }
            }

            if (context.SaveChanges() > 0)
            {
                return true;
            }
                
            return false;
        }


        public bool DeleteCustomerCollateralInsuranceDetails(int getStaffId, int id)
        {

            if (id == 0)
            {
                throw new ConditionNotMetException("Tracking Reference ID is Null");
            }
           
                var cit = context.TBL_COLLATERAL_INSURANCE_TRACKING.Find(id);
                if (cit == null) { return false; }

                cit.DELETED = true;

                if (context.SaveChanges() > 0)
                {
                    return true;
                }

            return false;
        }

        public void NotifyForCollateralVisitation(TBL_COLLATERAL_CUSTOMER collateral, bool saveInternally = false)
        {
            string messageBody;
            string alertSubject;
            string recipients;
            string jobReQuestCode;
            string staffFullName;
            int targetId;
            if (collateral.COLLATERALTYPEID != (int)CollateralTypeEnum.Property) return;
            var property = context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID);
            if (property != null)
            {
                var visitation = context.TBL_COLLATERAL_VISITATION.Where(v => v.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID).OrderByDescending(v => v.COLLATERALVISITATIONID).FirstOrDefault();
                if (visitation == null) return;
                var lastVisit = visitation.VISITATIONDATE;
                var nextVisit = visitation.NEXTVISITATIONDATE.Value;
                var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == visitation.CREATEDBY);
                if (staff == null)
                {
                    staffFullName = "All";
                }
                else
                {
                    staffFullName = staff?.FIRSTNAME + " " + staff?.MIDDLENAME + " " + staff?.LASTNAME;
                }
                var customer = context.TBL_CUSTOMER.FirstOrDefault(s => s.CUSTOMERID == collateral.CUSTOMERID);
                targetId = collateral.COLLATERALCUSTOMERID;
                jobReQuestCode = collateral.COLLATERALCODE;
                alertSubject = "NOTIFICATION COLLATERAL VALUATION REMINDER FROM FINTRAK ALERT";
                recipients = "John.Adeonojobi@ACCESSBANKPLC.com,Fayokemi.Akintunde@ACCESSBANKPLC.com,OLUKAYODE.AJAYI@ACCESSBANKPLC.com,paul.asiemo@accessbankplc.com";
                messageBody = $"Dear {staffFullName} <br /><br />," +
                               $"This is to inform you that the collateral, {collateral?.COLLATERALSUMMARY} belonging to {customer?.LASTNAME + ", " + customer?.FIRSTNAME + " " + customer?.MIDDLENAME}" +
                               $"(Customer ID: {collateral?.CUSTOMERCODE}) with OMV {collateral?.TBL_CURRENCY?.CURRENCYCODE} {String.Format("{0:0,0.00}", property?.OPENMARKETVALUE)} and FSV of {collateral?.TBL_CURRENCY?.CURRENCYCODE} {String.Format("{0:0,0.00}", property?.FORCEDSALEVALUE)} is due for revaluation on {nextVisit} <br /><br />" +
                               $"Kindly inform the customer, and initiate request for revaluation on Fintrak <br /><br />" +
                               $"Regards"
                               ;
                LogEmailAlert(messageBody, alertSubject, recipients, jobReQuestCode, targetId);
            }
            if (saveInternally)
            {
                context.SaveChanges();
            }
        }

        public void NotifyForCollateralRevaluation(TBL_COLLATERAL_CUSTOMER collateral, DateTime lastValuationDate, bool saveInternally = false, int? valuationCycle = null)
        {
            string messageBody;
            string alertSubject;
            string recipients;
            string jobReQuestCode;
            string staffFullName;
            int targetId;
            DateTime valuationDate;

            if (collateral.COLLATERALTYPEID != (int)CollateralTypeEnum.Property) return;
            var property = context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID);

            if (property != null)
            {
                if (valuationCycle != null)
                {
                    valuationDate = lastValuationDate.AddDays(valuationCycle.Value);
                }
                else if (collateral.VALUATIONCYCLE != null)
                {
                    valuationDate = lastValuationDate.AddDays((double)collateral.VALUATIONCYCLE);
                }
                else
                {
                    valuationDate = lastValuationDate;
                }

                //if (collateral.COLLATERALTYPEID == (int)CollateralTypeEnum.PlantAndMachinery)
                //{
                //    var property = context.TBL_COLLATERAL_PLANT_AND_EQUIP.FirstOrDefault(p => p.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID);
                //    if (property != null)
                //    {
                //        valuationDate = property.AddDays((double)collateral.VALUATIONCYCLE);//not complete!!!
                //    }
                //}

                targetId = collateral.COLLATERALCUSTOMERID;
                jobReQuestCode = collateral.COLLATERALCODE;
                var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == collateral.CREATEDBY);
                if (staff == null)
                {
                    staffFullName = "All";
                }
                else
                {
                    staffFullName = staff?.FIRSTNAME + " " + staff?.MIDDLENAME + " " + staff?.LASTNAME;
                }

                var customer = context.TBL_CUSTOMER.FirstOrDefault(s => s.CUSTOMERID == collateral.CUSTOMERID);
                alertSubject = "NOTIFICATION COLLATERAL VALUATION REMINDER FROM FINTRAK";
                recipients = "John.Adeonojobi@ACCESSBANKPLC.com,Fayokemi.Akintunde@ACCESSBANKPLC.com,OLUKAYODE.AJAYI@ACCESSBANKPLC.com,paul.asiemo@accessbankplc.com";
                messageBody = $"Dear {staffFullName} <br /><br />," +
                               $"This is to inform you that the collateral, {collateral?.COLLATERALSUMMARY} belonging to {customer?.LASTNAME + ", " + customer?.FIRSTNAME + " " + customer?.MIDDLENAME}" +
                               $"(Customer ID: {collateral?.CUSTOMERCODE}) with OMV {collateral?.TBL_CURRENCY?.CURRENCYCODE} {String.Format("{0:0,0.00}", property?.OPENMARKETVALUE)} and FSV of {collateral?.TBL_CURRENCY?.CURRENCYCODE} {String.Format("{0:0,0.00}", property?.FORCEDSALEVALUE)} is due for revaluation on {valuationDate.ToShortDateString()} <br /><br />" +
                               $"Kindly inform the customer, and initiate request for revaluation on Fintrak <br /><br />" +
                               $"Regards";

                LogEmailAlert(messageBody, alertSubject, recipients, jobReQuestCode, targetId);

                //alertSubject = "Collateral Valuation Reminder from FINTRAK 360(TEST ALERT)";
                //recipients = "John.Adeonojobi@ACCESSBANKPLC.com,Fayokemi.Akintunde@ACCESSBANKPLC.com,OLUKAYODE.AJAYI@ACCESSBANKPLC.com,ifeanyi.ikemefuna@fintraksoftware.com,chris.sualeze@fintraksoftware.com," +
                //    "tajudeen.onikoyi@fintraksoftware.com,paul.asiemo@accessbankplc.com,felix.afighi@fintraksoftware.com,augustine.nwaka@fintraksoftware.com";
                //messageBody = $"Hello, <br /><br />" +
                //               $"This is to inform you that, <br /><br />" +
                //               $"The collateral, {collateral.COLLATERALSUMMARY} of customer with customerId {collateral.CUSTOMERCODE} of value {collateral.TBL_CURRENCY.CURRENCYCODE} {String.Format("{0:0,0.00}", collateral.COLLATERALVALUE)}" +
                //               $" is due for the next Valuation on {valuationDate.ToShortDateString()}"
                //               ;


            }
            if (saveInternally)
            {
                context.SaveChanges();
            }
        }

        public void NotifyForCollateralStatusUpdate(TBL_COLLATERAL_CUSTOMER collateral, byte? perfectionStatusId, bool saveInternally = false)
        {
            string messageBody;
            string alertSubject;
            string recipients;
            string jobReQuestCode;
            string staffFullName;
            if (collateral.COLLATERALTYPEID != (int)CollateralTypeEnum.Property) return;
            var property = context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(p => p.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID);
            if (property != null)
            {
                int targetId;
                targetId = collateral.COLLATERALCUSTOMERID;
                jobReQuestCode = collateral.COLLATERALCODE;
                var perfectionStatus = "N/A";
                if (perfectionStatusId != null)
                {
                    perfectionStatus = context.TBL_COLLATERAL_PERFECTN_STAT.FirstOrDefault(s => s.PERFECTIONSTATUSID == perfectionStatusId).PERFECTIONSTATUSNAME;
                }
                var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == collateral.CREATEDBY);
                if (staff == null)
                {
                    staffFullName = "All";
                }
                else
                {
                    staffFullName = staff?.FIRSTNAME + " " + staff?.MIDDLENAME + " " + staff?.LASTNAME;
                }
                var customer = context.TBL_CUSTOMER.FirstOrDefault(s => s.CUSTOMERID == collateral.CUSTOMERID);
                alertSubject = "COLLATERAL STATUS UPDATE FROM FINTRAK360 ALERT";
                recipients = "John.Adeonojobi@ACCESSBANKPLC.com,Fayokemi.Akintunde@ACCESSBANKPLC.com,OLUKAYODE.AJAYI@ACCESSBANKPLC.com,paul.asiemo@accessbankplc.com";
                messageBody = $"Dear {staffFullName}, <br /><br />" +
                               $"This is to inform you that, <br /><br />" +
                               $"The collateral, {collateral.COLLATERALSUMMARY} of {customer?.LASTNAME} {customer?.FIRSTNAME} {customer?.MIDDLENAME} with customerId {customer?.CUSTOMERCODE} of value {collateral?.TBL_CURRENCY?.CURRENCYCODE} {String.Format("{0:0,0.00}", collateral?.COLLATERALVALUE)}" +
                               $" has it's perfection status updated as {perfectionStatus}"
                               ;
                LogEmailAlert(messageBody, alertSubject, recipients, jobReQuestCode, targetId);
            }
            if (saveInternally)
            {
                context.SaveChanges();
            }
        }

        public void NotifyForCollateralValidity(TBL_COLLATERAL_CUSTOMER collateral, DateTime? newValidityDate, bool saveInternally = false)
        {
            string messageBody;
            string alertSubject;
            string recipients;
            string jobReQuestCode;
            int targetId;
            string staffFullName;
            var currency = context.TBL_CURRENCY.FirstOrDefault(c => c.CURRENCYID == collateral.CURRENCYID);
            targetId = collateral.COLLATERALCUSTOMERID;
            jobReQuestCode = collateral.COLLATERALCODE;
            var staff = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == collateral.CREATEDBY);
            if (staff == null)
            {
                staffFullName = "All";
            }
            else
            {
                staffFullName = staff?.FIRSTNAME + " " + staff?.MIDDLENAME + " " + staff?.LASTNAME;
            }
            var customer = context.TBL_CUSTOMER.FirstOrDefault(s => s.CUSTOMERID == collateral.CUSTOMERID);
            alertSubject = "COLLATERAL VALIDITY UPDATE FROM FINTRAK360 ALERT";
            recipients = "John.Adeonojobi@ACCESSBANKPLC.com,Fayokemi.Akintunde@ACCESSBANKPLC.com,OLUKAYODE.AJAYI@ACCESSBANKPLC.com,paul.asiemo@accessbankplc.com";
            messageBody = $"Dear {staffFullName}, <br /><br />" +
                           $"This is to inform you that, <br /><br />" +
                           $"The collateral, {collateral.COLLATERALSUMMARY} of {customer?.LASTNAME} {customer?.FIRSTNAME} {customer?.MIDDLENAME} with customerId {customer?.CUSTOMERCODE} of value {currency.CURRENCYCODE} {String.Format("{0:0,0.00}", collateral.COLLATERALVALUE)}" +
                           $" now has a validity period that lasts till {newValidityDate.Value}"
                           ;
            LogEmailAlert(messageBody, alertSubject, recipients, jobReQuestCode, targetId);

            if (saveInternally)
            {
                context.SaveChanges();
            }
        }

        private void LogEmailAlert(string messageBody, string alertSubject, string recipients, string jobReQuestCode, int targetId)
        {
            try
            {
                var title = alertSubject.Trim();
                if (title.Contains("&"))
                {
                    title = title.Replace("&", "AND");
                }
                if (title.Contains("."))
                {
                    title = title.Replace(".", "");
                }

                string recipient = recipients.Trim();
                string messageSubject = title;
                string messageContent = messageBody;
                string templateUrl = messageContent;
                //string templateUrl = "~/EmailTemplates/Monitoring.html";
                //string mailBody = EmailHelpers.PopulateBody(messageContent, templateUrl);
                string mailBody = messageContent;
                MessageLogViewModel messageModel = new MessageLogViewModel
                {
                    MessageSubject = messageSubject,
                    MessageBody = mailBody,
                    MessageStatusId = 1,
                    MessageTypeId = 1,
                    FromAddress = ConfigurationManager.AppSettings["SupportEmailAddr"],
                    ToAddress = $"{recipient}",
                    DateTimeReceived = DateTime.Now,
                    SendOnDateTime = DateTime.Now,
                    ReferenceCode = jobReQuestCode,
                    targetId = targetId,
                };
                SaveMessageDetails(messageModel);
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }
        private void SaveMessageDetails(MessageLogViewModel model)
        {
            var message = new TBL_MESSAGE_LOG()
            {
                //MessageId = model.MessageId,
                MESSAGESUBJECT = model.MessageSubject,
                MESSAGEBODY = model.MessageBody,
                MESSAGESTATUSID = model.MessageStatusId,
                MESSAGETYPEID = model.MessageTypeId,
                FROMADDRESS = model.FromAddress,
                TOADDRESS = model.ToAddress,
                DATETIMERECEIVED = model.DateTimeReceived,
                SENDONDATETIME = model.SendOnDateTime,
                ATTACHMENTCODE = model.ReferenceCode,
                ATTACHMENTTYPEID = (short)AttachementTypeEnum.JobRequest,
                TARGETID = (int)model.targetId
            };

            context.TBL_MESSAGE_LOG.Add(message);

        }

        public int GoForApproval(ApprovalViewModel model)
        {
            TwoFactorAutheticationViewModel twoFADetails = new TwoFactorAutheticationViewModel
            {
                username = model.userName,
                passcode = model.passCode
            };
            int responce = 0;
            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (short)model.approvalStatusId;
                workflow.TargetId = model.targetId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        UpdateCutomerCollateralApprovalStatus(model, (short)workflow.StatusId, twoFADetails);
                    }

                    responce = context.SaveChanges();
                    transaction.Commit();

                    if (responce > 0)
                    {
                        return model.approvalStatusId;
                    }
                    return 0;
                }
                catch (Exception ex)
                {

                    transaction.Rollback();


                    throw ex;
                }
                //return false;
            }
        }

        public int GoForPolicyApproval(ApprovalViewModel model)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = (short)model.approvalStatusId;
                workflow.TargetId = model.targetId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.IsurancePolicyApproval;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        if (model.approvalStatusId != (int)ApprovalStatusEnum.Disapproved)
                        {
                            TBL_TEMP_COLLATERAL_ITEM_POLI data = context.TBL_TEMP_COLLATERAL_ITEM_POLI.Where(x => x.TEMPPOLICYID == model.targetId).FirstOrDefault();
                            UpdateItemPolicyApproval(data);
                        }
                    }

                    int responce = context.SaveChanges();
                    transaction.Commit();

                    if (responce > 0)
                    {
                        return model.approvalStatusId;
                    }
                    return 0;

                }
                catch (Exception ex)
                {

                    transaction.Rollback();

                    throw new SecureException("Error has occured while approving this insurance policy, kindly try again");
                }
            }
        }

        public WorkflowResponse GoForInsurancePolicyApproval(ApprovalViewModel model)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                bool responce;

                workflow.StaffId = model.createdBy;
                workflow.CompanyId = model.companyId;
                workflow.StatusId = model.approvalStatusId == 2 ? (int)ApprovalStatusEnum.Processing : model.approvalStatusId;
                workflow.TargetId = model.targetId;
                workflow.Comment = model.comment;
                workflow.OperationId = (int)OperationsEnum.IsurancePolicyApproval;
                workflow.DeferredExecution = true;
                workflow.LogActivity();
                try
                {
                    if (workflow.NewState == (int)ApprovalState.Ended)
                    {
                        var data = context.TBL_INSURANCE_REQUEST.Where(ir => ir.INSURANCEREQUESTID == model.targetId).FirstOrDefault();
                        var data2 = context.TBL_COLLATERAL_ITEM_POLICY.Where(x => x.COLLATERALCUSTOMERID == data.COLLATERALCUSTOMERID
                                                                                && x.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing)
                                                                                .FirstOrDefault();

                        data.APPROVALSTATUSID = model.approvalStatusId;
                        if (data2 != null)
                        {
                            data2.APPROVALSTATUSID = model.approvalStatusId;
                        }

                    }

                    responce = context.SaveChanges() > 0;
                    transaction.Commit();

                    return workflow.Response;

                }
                catch (Exception ex)
                {

                    transaction.Rollback();

                    throw new SecureException("Error has occured while approving this insurance policy, kindly try again");
                }
            }
        }

        private void UpdateCutomerCollateralApprovalStatus(ApprovalViewModel ApprovalModel, short status, TwoFactorAutheticationViewModel twoFADetails)
        {
            var mainCollateral = (from x in context.TBL_TEMP_COLLATERAL_CUSTOMER
                                  join t in context.TBL_COLLATERAL_TYPE on x.COLLATERALTYPEID equals t.COLLATERALTYPEID
                                  where x.TEMPCOLLATERALCUSTOMERID == ApprovalModel.targetId && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved && x.ISCURRENT != false
                                  select new { x.COLLATERALTYPEID, x.TEMPCOLLATERALCUSTOMERID, t.REQUIREINSURANCEPOLICY, t.REQUIREVISITATION, x.COLLATERALCODE, x.COLLATERALVALUE, x.CUSTOMERID }).FirstOrDefault();


            if (mainCollateral.COLLATERALTYPEID > 0)
            {
                if (status == (int)ApprovalStatusEnum.Disapproved)
                {
                    var collateralDisapproval = context.TBL_TEMP_COLLATERAL_CUSTOMER.Where(o => o.TEMPCOLLATERALCUSTOMERID == ApprovalModel.targetId).Select(o => o).FirstOrDefault();
                    collateralDisapproval.ISCURRENT = false;
                    collateralDisapproval.APPROVALSTATUSID = status;
                }
                else
                {
                    if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.CASA)
                    {
                        var tempCasa = context.TBL_TEMP_COLLATERAL_CASA.Where(x => x.TEMPCOLLATERALCUSTOMERID == mainCollateral.TEMPCOLLATERALCUSTOMERID).FirstOrDefault();
                        var branch = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == mainCollateral.CUSTOMERID).Select(x => x.BRANCHID).FirstOrDefault();
                        CasaLienViewModel model = new CasaLienViewModel
                        {
                            productAccountNumber = mainCollateral.COLLATERALCODE,
                            lienAmount = tempCasa.SECURITYVALUE,
                            description = "CASA collateral creation",
                            lienTypeId = (int)LienTypeEnum.CollateralCreation,
                            sourceReferenceNumber = mainCollateral.COLLATERALCODE,
                            dateTimeCreated = DateTime.Now,
                            createdBy = ApprovalModel.createdBy,
                            companyId = ApprovalModel.companyId,
                            branchId = branch,

                        };

                        //place lien
                        lien.PlaceLien(model, twoFADetails);

                        int collaterId = UpdateCollateralMain(ApprovalModel.targetId);

                        if (collaterId > 0)
                        {
                            UpdateCASAcollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, collaterId);

                            //UpdateCollateralDocument(ApprovalModel.targetId, collaterId);
                            UpdateCollateralVisitation(ApprovalModel.targetId, collaterId);

                            if (mainCollateral.REQUIREINSURANCEPOLICY) { UpdateItemPolicyDetail(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, collaterId); } //insurance documents

                            UpdateTempApprovalStatus(ApprovalModel.targetId, status);

                        }
                    }
                    else if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.FixedDeposit)
                    {
                        var tempDeposit = context.TBL_TEMP_COLLATERAL_DEPOSIT.Where(x => x.TEMPCOLLATERALCUSTOMERID == mainCollateral.TEMPCOLLATERALCUSTOMERID).FirstOrDefault();
                        var branch = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == mainCollateral.CUSTOMERID).Select(x => x.BRANCHID).FirstOrDefault();

                        CasaLienViewModel model = new CasaLienViewModel
                        {
                            productAccountNumber = mainCollateral.COLLATERALCODE,
                            lienAmount = tempDeposit.SECURITYVALUE,
                            description = "Term deposit collateral creation",
                            lienTypeId = (int)LienTypeEnum.CollateralCreation,
                            sourceReferenceNumber = mainCollateral.COLLATERALCODE,
                            dateTimeCreated = DateTime.Now,
                            createdBy = ApprovalModel.createdBy,
                            companyId = ApprovalModel.companyId,
                            branchId = branch,
                            isTermDeposit = true,

                        };

                        var finacleBalance = finacle.ValidateTDAccountNumber(model.productAccountNumber);
                        if (finacleBalance != null)
                        {
                            model.currencyCode = finacleBalance.currencyType;
                        }
                        lien.PlaceLien(model, twoFADetails);

                        int collaterId = UpdateCollateralMain(ApprovalModel.targetId);

                        if (collaterId > 0)
                        {
                            UpdateDepositCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, collaterId);

                            //UpdateCollateralDocument(ApprovalModel.targetId, collaterId);
                            UpdateCollateralVisitation(ApprovalModel.targetId, collaterId);

                            if (mainCollateral.REQUIREINSURANCEPOLICY) { UpdateItemPolicyDetail(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, collaterId); } //insurance documents

                            UpdateTempApprovalStatus(ApprovalModel.targetId, status);

                        }

                    }
                    else
                    {
                        int newCollaterId = UpdateCollateralMain(ApprovalModel.targetId);

                        if (newCollaterId > 0)
                        {
                            switch (mainCollateral.COLLATERALTYPEID)
                            {
                                case (int)CollateralTypeEnum.PlantAndMachinery: UpdatePlantAndEquipmentCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.Miscellaneous: UpdateMiscellaneousCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.Gaurantee: UpdateGuaranteeCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.Property: UpdateApprovedImmovableCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.TreasuryBillsAndBonds: UpdateMarketSecurityCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.InsurancePolicy: UpdatePolicyCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.PreciousMetal: UpdateMetalCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.MarketableSecurities_Shares: UpdateStockCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.Vehicle: UpdateVehicleCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.Promissory: UpdatePromissoryCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.ISPO: UpdateISPOCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.DomiciliationContract: UpdateContractDomiciliationCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.DomiciliationSalary: UpdateSalaryDomiciliationCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.Indemity: UpdateIndemityCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;

                                default: break;
                            }

                            UpdateTempApprovalStatus(ApprovalModel.targetId, status);

                            //UpdateCollateralDocument(ApprovalModel.targetId, newCollaterId);
                            UpdateCollateralVisitation(ApprovalModel.targetId, newCollaterId);

                            if (mainCollateral.REQUIREINSURANCEPOLICY) { UpdateItemPolicyDetail(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); } //insurance documents
                        }
                        else
                        {
                            //abort transaction
                        }
                    }
                }
            }

        }


        private int UpdateCollateralMain(int collateralId)
        {
            var data = new TBL_COLLATERAL_CUSTOMER();


            var mainCollateral = context.TBL_TEMP_COLLATERAL_CUSTOMER.Where(x => x.TEMPCOLLATERALCUSTOMERID == collateralId)
           .Select(x => x).FirstOrDefault();

            if (mainCollateral != null)
            {
                data = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCODE == mainCollateral.COLLATERALCODE).FirstOrDefault();


                if (data != null)
                {
                    if (mainCollateral.VALIDTILL != data.VALIDTILL)
                    {
                        NotifyForCollateralValidity(data, mainCollateral.VALIDTILL);
                    }
                    data.ACTEDONBY = mainCollateral.ACTEDONBY;
                    data.ALLOWSHARING = mainCollateral.ALLOWSHARING;
                    data.CAMREFNUMBER = mainCollateral.CAMREFNUMBER;
                    data.COLLATERALCODE = mainCollateral.COLLATERALCODE;
                    data.COLLATERALSUBTYPEID = mainCollateral.COLLATERALSUBTYPEID;
                    data.COLLATERALTYPEID = mainCollateral.COLLATERALTYPEID;
                    data.COLLATERALVALUE = mainCollateral.COLLATERALVALUE;
                    data.COMPANYID = mainCollateral.COMPANYID;
                    data.CREATEDBY = mainCollateral.CREATEDBY;
                    data.CURRENCYID = mainCollateral.CURRENCYID;
                    data.CUSTOMERID = mainCollateral.CUSTOMERID;
                    data.DATEACTEDON = mainCollateral.DATEACTEDON;
                    data.DATETIMECREATED = mainCollateral.DATETIMECREATED;
                    data.HAIRCUT = mainCollateral.HAIRCUT;
                    data.ISLOCATIONBASED = mainCollateral.ISLOCATIONBASED;
                    data.VALUATIONCYCLE = mainCollateral.VALUATIONCYCLE;
                    data.EXCHANGERATE = mainCollateral.EXCHANGERATE;
                    data.APPROVALSTATUS = (int)ApprovalStatusEnum.Approved;
                    data.COLLATERALSUMMARY = mainCollateral.COLLATERALSUMMARY;
                    data.VALIDTILL = mainCollateral.VALIDTILL;
                    context.SaveChanges();
                    return data.COLLATERALCUSTOMERID;
                }
                else
                {
                    var returnCollateralId = context.TBL_COLLATERAL_CUSTOMER.Add(new TBL_COLLATERAL_CUSTOMER
                    {
                        ACTEDONBY = mainCollateral.ACTEDONBY,
                        ALLOWSHARING = mainCollateral.ALLOWSHARING,
                        CAMREFNUMBER = mainCollateral.CAMREFNUMBER,
                        COLLATERALCODE = mainCollateral.COLLATERALCODE,
                        COLLATERALSUBTYPEID = mainCollateral.COLLATERALSUBTYPEID,
                        COLLATERALTYPEID = mainCollateral.COLLATERALTYPEID,
                        COLLATERALVALUE = mainCollateral.COLLATERALVALUE,
                        COMPANYID = mainCollateral.COMPANYID,
                        CREATEDBY = mainCollateral.CREATEDBY,
                        CURRENCYID = mainCollateral.CURRENCYID,
                        CUSTOMERID = mainCollateral.CUSTOMERID,
                        DATEACTEDON = mainCollateral.DATEACTEDON,
                        DATETIMECREATED = mainCollateral.DATETIMECREATED,
                        HAIRCUT = mainCollateral.HAIRCUT,
                        ISLOCATIONBASED = mainCollateral.ISLOCATIONBASED,
                        VALUATIONCYCLE = mainCollateral.VALUATIONCYCLE,
                        EXCHANGERATE = mainCollateral.EXCHANGERATE,
                        COLLATERALSUMMARY = mainCollateral.COLLATERALSUMMARY,
                        APPROVALSTATUS = (int)ApprovalStatusEnum.Approved
                    });
                    context.SaveChanges();
                    if (data != null)
                    {
                        NotifyForCollateralValidity(data, mainCollateral.VALIDTILL, true);
                    }
                    return returnCollateralId.COLLATERALCUSTOMERID;
                }

            }
            else
            {
                return data.COLLATERALCUSTOMERID;
            }
        }
        private void UpdateTempApprovalStatus(int TempCollateralId, short status)
        {

            var collaterInformation = context.TBL_TEMP_COLLATERAL_CUSTOMER.Where(x => x.TEMPCOLLATERALCUSTOMERID == TempCollateralId).FirstOrDefault();
            if (collaterInformation != null)
            {
                collaterInformation.APPROVALSTATUSID = status;
            }

        }
        private void UpdateItemPolicyDetail(int tempCollateralId, string collateralCode, int newCollaterId)
        {
            //get all collateral details from temp
            var tempPol = context.TBL_TEMP_COLLATERAL_ITEM_POLI.Where(x => x.COLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (tempPol != null)
            {
                //get collateral detial from main table
                var mainPol = (from x in context.TBL_COLLATERAL_ITEM_POLICY
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               where c.COLLATERALCODE == collateralCode
                               select (x)).FirstOrDefault();

                if (mainPol != null)
                {
                    mainPol.COLLATERALCUSTOMERID = newCollaterId;
                    mainPol.CREATEDBY = tempPol.CREATEDBY;
                    mainPol.DATETIMECREATED = tempPol.DATETIMECREATED;
                    mainPol.ENDDATE = tempPol.ENDDATE;
                    mainPol.INSURANCECOMPANYID = tempPol.INSURANCECOMPANYID;
                    mainPol.INSURANCETYPEID = tempPol.INSURANCETYPEID;
                    mainPol.LASTUPDATEDBY = tempPol.LASTUPDATEDBY;
                    mainPol.POLICYREFERENCENUMBER = tempPol.POLICYREFERENCENUMBER;
                    mainPol.STARTDATE = tempPol.STARTDATE;
                    mainPol.SUMINSURED = tempPol.SUMINSURED;
                    mainPol.PREMIUMAMOUNT = tempPol.PREMIUMAMOUNT;
                    mainPol.DESCRIPTION = tempPol.DESCRIPTION;
                    // mainPol.PREMIUMPERCENT = tempPol.PREMIUMPERCENT;
                }
                else
                {
                    context.TBL_COLLATERAL_ITEM_POLICY.Add(new TBL_COLLATERAL_ITEM_POLICY
                    {
                        COLLATERALCUSTOMERID = newCollaterId,
                        CREATEDBY = tempPol.CREATEDBY,
                        DATETIMECREATED = tempPol.DATETIMECREATED,
                        ENDDATE = tempPol.ENDDATE,
                        INSURANCECOMPANYID = tempPol.INSURANCECOMPANYID,
                        INSURANCETYPEID = tempPol.INSURANCETYPEID,
                        LASTUPDATEDBY = tempPol.LASTUPDATEDBY,
                        POLICYREFERENCENUMBER = tempPol.POLICYREFERENCENUMBER,
                        STARTDATE = tempPol.STARTDATE,
                        SUMINSURED = tempPol.SUMINSURED,
                        PREMIUMAMOUNT = tempPol.PREMIUMAMOUNT,
                        DESCRIPTION = tempPol.DESCRIPTION,
                        //  PREMIUMPERCENT = tempPol.PREMIUMPERCENT
                    });
                }
            }
        }

        public void UpdatePropertyCollateralNotifications(TBL_TEMP_COLLATERAL_IMMOV_PROP tempCollateral, TBL_COLLATERAL_IMMOVE_PROPERTY collateral, bool saveInternally = false)
        {
            if (tempCollateral.PERFECTIONSTATUSID != collateral.PERFECTIONSTATUSID)
            {
                var collateralMain = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID);
                NotifyForCollateralStatusUpdate(collateralMain, tempCollateral.PERFECTIONSTATUSID, saveInternally);
            }
            if (tempCollateral.LASTVALUATIONDATE != collateral.LASTVALUATIONDATE)
            {
                var collateralMain = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID);
                NotifyForCollateralRevaluation(collateralMain, tempCollateral.LASTVALUATIONDATE, saveInternally);
                NotifyForCollateralVisitation(collateralMain, saveInternally);
            }
        }

        public void AddPropertyCollateralNotifications(TBL_COLLATERAL_IMMOVE_PROPERTY collateral, bool saveInternally = false)
        {
            var collateralMain = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == collateral.COLLATERALCUSTOMERID);
            NotifyForCollateralStatusUpdate(collateralMain, collateral.PERFECTIONSTATUSID, saveInternally);
            NotifyForCollateralRevaluation(collateralMain, collateral.LASTVALUATIONDATE, saveInternally);
            NotifyForCollateralVisitation(collateralMain, saveInternally);
        }

        private void UpdateApprovedImmovableCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var tempProp = context.TBL_TEMP_COLLATERAL_IMMOV_PROP.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (tempProp != null)
            {
                //get collateral detial from main table
                var mainProp = (from x in context.TBL_COLLATERAL_IMMOVE_PROPERTY
                                join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                where c.COLLATERALCODE == collateralcode
                                select (x)).FirstOrDefault();

                if (mainProp != null)
                {
                    UpdatePropertyCollateralNotifications(tempProp, mainProp);
                    mainProp.CITYID = tempProp.CITYID;
                    mainProp.COLLATERALCUSTOMERID = newCollateralId;
                    mainProp.COLLATERALUSABLEAMOUNT = tempProp.COLLATERALUSABLEAMOUNT;
                    mainProp.CONSTRUCTIONDATE = tempProp.CONSTRUCTIONDATE;
                    mainProp.COUNTRYID = tempProp.COUNTRYID;
                    mainProp.DATEOFACQUISITION = tempProp.DATEOFACQUISITION;
                    mainProp.FORCEDSALEVALUE = tempProp.FORCEDSALEVALUE;
                    mainProp.LASTVALUATIONDATE = tempProp.LASTVALUATIONDATE;
                    mainProp.LATITUDE = tempProp.LATITUDE;
                    mainProp.LONGITUDE = tempProp.LONGITUDE;
                    mainProp.NEARESTBUSSTOP = tempProp.NEARESTBUSSTOP;
                    mainProp.NEARESTLANDMARK = tempProp.NEARESTLANDMARK;
                    mainProp.OPENMARKETVALUE = tempProp.OPENMARKETVALUE;
                    mainProp.PERFECTIONSTATUSID = tempProp.PERFECTIONSTATUSID;
                    mainProp.PERFECTIONSTATUSREASON = tempProp.PERFECTIONSTATUSREASON;
                    mainProp.PROPERTYADDRESS = tempProp.PROPERTYADDRESS;
                    mainProp.PROPERTYNAME = tempProp.PROPERTYNAME;
                    mainProp.PROPERTYVALUEBASETYPEID = tempProp.PROPERTYVALUEBASETYPEID;
                    mainProp.REMARK = tempProp.REMARK;
                    mainProp.SECURITYVALUE = tempProp.SECURITYVALUE;
                    mainProp.STAMPTOCOVER = tempProp.STAMPTOCOVER;
                    mainProp.VALUATIONAMOUNT = tempProp.VALUATIONAMOUNT;
                    mainProp.VALUERID = tempProp.VALUERID;
                    mainProp.VALUERREFERENCENUMBER = tempProp.VALUERREFERENCENUMBER;
                    mainProp.ISOWNEROCCUPIED = tempProp.ISOWNEROCCUPIED;
                    mainProp.ISRESIDENTIAL = tempProp.ISRESIDENTIAL;
                    mainProp.ISASSETPLEDGEDBYTHRIDPARTY = tempProp.ISASSETPLEDGEDBYTHRIDPARTY;
                    mainProp.THRIDPARTYNAME = tempProp.THRIDPARTYNAME;
                    mainProp.ISASSETMANAGEDBYTRUSTEE = tempProp.ISASSETMANAGEDBYTRUSTEE;
                    mainProp.TRUSTEENAME = tempProp.TRUSTEENAME;
                    mainProp.STATEID = tempProp.STATEID;
                    mainProp.LOCALGOVERNMENTID = tempProp.LOCALGOVERNMENTID;
                    mainProp.BANKSHAREOFCOLLATERAL = tempProp.BANKSHAREOFCOLLATERAL;
                    mainProp.ESTIMATEDVALUE = tempProp.ESTIMATEDVALUE;
                }
                else
                {
                    var newCollateral = new TBL_COLLATERAL_IMMOVE_PROPERTY
                    {
                        CITYID = tempProp.CITYID,
                        COLLATERALCUSTOMERID = newCollateralId,
                        COLLATERALUSABLEAMOUNT = tempProp.COLLATERALUSABLEAMOUNT,
                        CONSTRUCTIONDATE = tempProp.CONSTRUCTIONDATE,
                        COUNTRYID = tempProp.COUNTRYID,
                        DATEOFACQUISITION = tempProp.DATEOFACQUISITION,
                        FORCEDSALEVALUE = tempProp.FORCEDSALEVALUE,
                        LASTVALUATIONDATE = tempProp.LASTVALUATIONDATE,
                        LATITUDE = tempProp.LATITUDE,
                        LONGITUDE = tempProp.LONGITUDE,
                        NEARESTBUSSTOP = tempProp.NEARESTBUSSTOP,
                        NEARESTLANDMARK = tempProp.NEARESTLANDMARK,
                        OPENMARKETVALUE = tempProp.OPENMARKETVALUE,
                        PERFECTIONSTATUSID = tempProp.PERFECTIONSTATUSID,
                        PERFECTIONSTATUSREASON = tempProp.PERFECTIONSTATUSREASON,
                        PROPERTYADDRESS = tempProp.PROPERTYADDRESS,
                        PROPERTYNAME = tempProp.PROPERTYNAME,
                        PROPERTYVALUEBASETYPEID = tempProp.PROPERTYVALUEBASETYPEID,
                        REMARK = tempProp.REMARK,
                        SECURITYVALUE = tempProp.SECURITYVALUE,
                        STAMPTOCOVER = tempProp.STAMPTOCOVER,
                        VALUATIONAMOUNT = tempProp.VALUATIONAMOUNT,
                        VALUERID = tempProp.VALUERID,
                        VALUERREFERENCENUMBER = tempProp.VALUERREFERENCENUMBER,
                        ISOWNEROCCUPIED = tempProp.ISOWNEROCCUPIED,
                        ISRESIDENTIAL = tempProp.ISRESIDENTIAL,
                        ISASSETPLEDGEDBYTHRIDPARTY = tempProp.ISASSETPLEDGEDBYTHRIDPARTY,
                        THRIDPARTYNAME = tempProp.THRIDPARTYNAME,
                        ISASSETMANAGEDBYTRUSTEE = tempProp.ISASSETMANAGEDBYTRUSTEE,
                        TRUSTEENAME = tempProp.TRUSTEENAME,
                        STATEID = tempProp.STATEID,
                        LOCALGOVERNMENTID = tempProp.LOCALGOVERNMENTID,
                        BANKSHAREOFCOLLATERAL = tempProp.BANKSHAREOFCOLLATERAL,
                        ESTIMATEDVALUE = tempProp.ESTIMATEDVALUE
                    };
                    context.TBL_COLLATERAL_IMMOVE_PROPERTY.Add(newCollateral);
                    if (context.SaveChanges() != 0)
                    {
                        AddPropertyCollateralNotifications(newCollateral);
                    }
                }
            }
        }
        private void UpdateDepositCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {

            //get all collateral details from temp
            var tempDeposit = context.TBL_TEMP_COLLATERAL_DEPOSIT.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (tempDeposit != null)
            {
                //get collateral detial from main table
                var mainDeposit = (from x in context.TBL_COLLATERAL_DEPOSIT
                                   join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                   where c.COLLATERALCODE == collateralcode
                                   select (x)).FirstOrDefault();

                if (mainDeposit != null)
                {

                    mainDeposit.ACCOUNTNUMBER = tempDeposit.ACCOUNTNUMBER;
                    mainDeposit.AVAILABLEBALANCE = tempDeposit.AVAILABLEBALANCE;
                    mainDeposit.BANK = tempDeposit.BANK;
                    mainDeposit.DEALREFERENCENUMBER = tempDeposit.DEALREFERENCENUMBER;
                    mainDeposit.EFFECTIVEDATE = tempDeposit.EFFECTIVEDATE;
                    mainDeposit.EXISTINGLIENAMOUNT = tempDeposit.EXISTINGLIENAMOUNT;
                    mainDeposit.LIENAMOUNT = tempDeposit.LIENAMOUNT;
                    mainDeposit.MATURITYAMOUNT = tempDeposit.MATURITYAMOUNT;
                    mainDeposit.MATURITYDATE = tempDeposit.MATURITYDATE;
                    mainDeposit.REMARK = tempDeposit.REMARK;
                    mainDeposit.SECURITYVALUE = tempDeposit.SECURITYVALUE;
                    mainDeposit.ACCOUNTNAME = tempDeposit.ACCOUNTNAME;
                }
                else
                {
                    context.TBL_COLLATERAL_DEPOSIT.Add(new TBL_COLLATERAL_DEPOSIT
                    {
                        ACCOUNTNUMBER = tempDeposit.ACCOUNTNUMBER,
                        AVAILABLEBALANCE = tempDeposit.AVAILABLEBALANCE,
                        BANK = tempDeposit.BANK,
                        COLLATERALCUSTOMERID = newCollateralId,
                        DEALREFERENCENUMBER = tempDeposit.DEALREFERENCENUMBER,
                        EFFECTIVEDATE = tempDeposit.EFFECTIVEDATE,
                        EXISTINGLIENAMOUNT = tempDeposit.EXISTINGLIENAMOUNT,
                        LIENAMOUNT = tempDeposit.LIENAMOUNT,
                        MATURITYAMOUNT = tempDeposit.MATURITYAMOUNT,
                        MATURITYDATE = tempDeposit.MATURITYDATE,
                        REMARK = tempDeposit.REMARK,
                        SECURITYVALUE = tempDeposit.SECURITYVALUE,
                        ACCOUNTNAME = tempDeposit.ACCOUNTNAME
                    });
                }

            }

        }
        private void UpdateGuaranteeCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var tempGua = context.TBL_TEMP_COLLATERAL_GAURANTEE.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (tempGua != null)
            {
                //get collateral detial from main table
                var mainGua = (from x in context.TBL_COLLATERAL_GAURANTEE
                               join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                               where c.COLLATERALCODE == collateralcode
                               select (x)).FirstOrDefault();

                if (mainGua != null)
                {
                    mainGua.COLLATERALCUSTOMERID = newCollateralId;
                    mainGua.BVN = tempGua.BVN;
                    mainGua.EMAILADDRESS = tempGua.EMAILADDRESS;
                    mainGua.ENDDATE = tempGua.ENDDATE;
                    mainGua.FIRSTNAME = tempGua.FIRSTNAME;
                    mainGua.GUARANTEEVALUE = tempGua.GUARANTEEVALUE;
                    mainGua.GUARANTORADDRESS = tempGua.GUARANTORADDRESS;
                    mainGua.INSTITUTIONNAME = tempGua.INSTITUTIONNAME;
                    mainGua.LASTNAME = tempGua.LASTNAME;
                    mainGua.MIDDLENAME = tempGua.MIDDLENAME;
                    mainGua.PHONENUMBER1 = tempGua.PHONENUMBER1;
                    mainGua.PHONENUMBER2 = tempGua.PHONENUMBER2;
                    mainGua.RCNUMBER = tempGua.RCNUMBER;
                    mainGua.RELATIONSHIP = tempGua.RELATIONSHIP;
                    mainGua.RELATIONSHIPDURATION = tempGua.RELATIONSHIPDURATION;
                    mainGua.REMARK = tempGua.REMARK;
                    mainGua.STARTDATE = tempGua.STARTDATE;
                    mainGua.TAXNUMBER = tempGua.TAXNUMBER;
                }
                else
                {
                    context.TBL_COLLATERAL_GAURANTEE.Add(new TBL_COLLATERAL_GAURANTEE
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        BVN = tempGua.BVN,
                        EMAILADDRESS = tempGua.EMAILADDRESS,
                        ENDDATE = tempGua.ENDDATE,
                        FIRSTNAME = tempGua.FIRSTNAME,
                        GUARANTEEVALUE = tempGua.GUARANTEEVALUE,
                        GUARANTORADDRESS = tempGua.GUARANTORADDRESS,
                        INSTITUTIONNAME = tempGua.INSTITUTIONNAME,
                        LASTNAME = tempGua.LASTNAME,
                        MIDDLENAME = tempGua.MIDDLENAME,
                        PHONENUMBER1 = tempGua.PHONENUMBER1,
                        PHONENUMBER2 = tempGua.PHONENUMBER2,
                        RCNUMBER = tempGua.RCNUMBER,
                        RELATIONSHIP = tempGua.RELATIONSHIP,
                        RELATIONSHIPDURATION = tempGua.RELATIONSHIPDURATION,
                        REMARK = tempGua.REMARK,
                        STARTDATE = tempGua.STARTDATE,
                        TAXNUMBER = tempGua.TAXNUMBER,

                    });
                }
            }

        }
        private void UpdateMarketSecurityCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var TempMkt = context.TBL_TEMP_COLLATERAL_MKT_SEC.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (TempMkt != null)
            {
                //get collateral detial from main table
                var mainMarket = (from x in context.TBL_COLLATERAL_MKT_SECURITY
                                  join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                  where c.COLLATERALCODE == collateralcode
                                  select (x)).FirstOrDefault();

                if (mainMarket != null)
                {
                    mainMarket.COLLATERALCUSTOMERID = newCollateralId;
                    mainMarket.DEALAMOUNT = TempMkt.DEALAMOUNT;
                    mainMarket.BANKPURCHASEDFROM = TempMkt.BANKPURCHASEDFROM;
                    mainMarket.FUNDNAME = TempMkt.FUNDNAME;
                    mainMarket.EFFECTIVEDATE = TempMkt.EFFECTIVEDATE;
                    mainMarket.INTERESTPAYMENTFREQUENCY = TempMkt.INTERESTPAYMENTFREQUENCY;
                    mainMarket.ISSUERNAME = TempMkt.ISSUERNAME;
                    mainMarket.ISSUERREFERENCENUMBER = TempMkt.ISSUERREFERENCENUMBER;
                    mainMarket.LIENUSABLEAMOUNT = TempMkt.LIENUSABLEAMOUNT;
                    mainMarket.MATURITYDATE = TempMkt.MATURITYDATE;
                    mainMarket.NUMBEROFUNITS = TempMkt.NUMBEROFUNITS;
                    mainMarket.PERCENTAGEINTEREST = TempMkt.PERCENTAGEINTEREST;
                    mainMarket.RATING = TempMkt.RATING;
                    mainMarket.REMARK = TempMkt.REMARK;
                    mainMarket.SECURITYTYPE = TempMkt.SECURITYTYPE;
                    mainMarket.SECURITYVALUE = TempMkt.SECURITYVALUE;
                    mainMarket.UNITVALUE = TempMkt.UNITVALUE;

                }
                else
                {
                    context.TBL_COLLATERAL_MKT_SECURITY.Add(new TBL_COLLATERAL_MKT_SECURITY
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        DEALAMOUNT = TempMkt.DEALAMOUNT,
                        BANKPURCHASEDFROM = TempMkt.BANKPURCHASEDFROM,
                        FUNDNAME = TempMkt.FUNDNAME,
                        EFFECTIVEDATE = TempMkt.EFFECTIVEDATE,
                        INTERESTPAYMENTFREQUENCY = TempMkt.INTERESTPAYMENTFREQUENCY,
                        ISSUERNAME = TempMkt.ISSUERNAME,
                        ISSUERREFERENCENUMBER = TempMkt.ISSUERREFERENCENUMBER,
                        LIENUSABLEAMOUNT = TempMkt.LIENUSABLEAMOUNT,
                        MATURITYDATE = TempMkt.MATURITYDATE,
                        NUMBEROFUNITS = TempMkt.NUMBEROFUNITS,
                        PERCENTAGEINTEREST = TempMkt.PERCENTAGEINTEREST,
                        RATING = TempMkt.RATING,
                        REMARK = TempMkt.REMARK,
                        SECURITYTYPE = TempMkt.SECURITYTYPE,
                        SECURITYVALUE = TempMkt.SECURITYVALUE,
                        UNITVALUE = TempMkt.UNITVALUE,
                    });
                }
            }

        }
        private void UpdatePlantAndEquipmentCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var tempPlant = context.TBL_TEMP_COLLATERAL_PLANT_EQUP.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (tempPlant != null)
            {
                var mainPlant = (from x in context.TBL_COLLATERAL_PLANT_AND_EQUIP
                                 join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                 where c.COLLATERALCODE == collateralcode
                                 select (x)).FirstOrDefault();
                if (mainPlant != null)
                {
                    //get collateral detial from main table


                    mainPlant.COLLATERALCUSTOMERID = newCollateralId;
                    mainPlant.REMARK = tempPlant.REMARK;
                    mainPlant.DESCRIPTION = tempPlant.DESCRIPTION;
                    mainPlant.INTENDEDUSE = tempPlant.INTENDEDUSE;
                    mainPlant.EQUIPMENTSIZE = tempPlant.EQUIPMENTSIZE;
                    mainPlant.MACHINECONDITION = tempPlant.MACHINECONDITION;
                    mainPlant.MACHINENAME = tempPlant.MACHINENAME;
                    mainPlant.MACHINENUMBER = tempPlant.MACHINENUMBER;
                    mainPlant.MACHINERYLOCATION = tempPlant.MACHINERYLOCATION;
                    mainPlant.MANUFACTURERNAME = tempPlant.MANUFACTURERNAME;
                    mainPlant.REPLACEMENTVALUE = tempPlant.REPLACEMENTVALUE;
                    mainPlant.VALUEBASETYPEID = tempPlant.VALUEBASETYPEID;
                    mainPlant.YEAROFMANUFACTURE = tempPlant.YEAROFMANUFACTURE;
                    mainPlant.YEAROFPURCHASE = tempPlant.YEAROFPURCHASE;

                }
                else
                {
                    context.TBL_COLLATERAL_PLANT_AND_EQUIP.Add(new TBL_COLLATERAL_PLANT_AND_EQUIP
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        REMARK = tempPlant.REMARK,
                        DESCRIPTION = tempPlant.DESCRIPTION,
                        INTENDEDUSE = tempPlant.INTENDEDUSE,
                        EQUIPMENTSIZE = tempPlant.EQUIPMENTSIZE,
                        MACHINECONDITION = tempPlant.MACHINECONDITION,
                        MACHINENAME = tempPlant.MACHINENAME,
                        MACHINENUMBER = tempPlant.MACHINENUMBER,
                        MACHINERYLOCATION = tempPlant.MACHINERYLOCATION,
                        MANUFACTURERNAME = tempPlant.MANUFACTURERNAME,
                        REPLACEMENTVALUE = tempPlant.REPLACEMENTVALUE,
                        VALUEBASETYPEID = tempPlant.VALUEBASETYPEID,
                        YEAROFMANUFACTURE = tempPlant.YEAROFMANUFACTURE,
                        YEAROFPURCHASE = tempPlant.YEAROFPURCHASE,

                    });
                }
            }


        }
        private void UpdatePolicyCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var tempPolicy = context.TBL_TEMP_COLLATERAL_POLICY.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (tempPolicy != null)
            {
                //get collateral detial from main table
                var mainPolicy = (from x in context.TBL_COLLATERAL_POLICY
                                  join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                  where c.COLLATERALCODE == collateralcode
                                  select (x)).FirstOrDefault();

                if (mainPolicy != null)
                {

                    mainPolicy.COLLATERALCUSTOMERID = newCollateralId;
                    mainPolicy.REMARK = tempPolicy.REMARK;
                    mainPolicy.ASSIGNDATE = tempPolicy.ASSIGNDATE;
                    mainPolicy.INSURANCECOMPANYNAME = tempPolicy.INSURANCECOMPANYNAME;
                    mainPolicy.INSURERADDRESS = tempPolicy.INSURERADDRESS;
                    mainPolicy.INSURERDETAILS = tempPolicy.INSURERDETAILS;
                    mainPolicy.ISOWNEDBYCUSTOMER = tempPolicy.ISOWNEDBYCUSTOMER;
                    mainPolicy.INSURANCEPOLICYNUMBER = tempPolicy.INSURANCEPOLICYNUMBER;
                    mainPolicy.POLICYAMOUNT = tempPolicy.POLICYAMOUNT;
                    mainPolicy.POLICYRENEWALDATE = tempPolicy.POLICYRENEWALDATE;
                    mainPolicy.POLICYSTARTDATE = tempPolicy.POLICYSTARTDATE;
                    mainPolicy.PREMIUMAMOUNT = tempPolicy.PREMIUMAMOUNT;
                    mainPolicy.RENEWALFREQUENCYTYPEID = tempPolicy.RENEWALFREQUENCYTYPEID;
                    mainPolicy.INSURANCETYPEID = tempPolicy.INSURANCETYPEID;
                    //mainPolicy.INSURANCETYPE = tempPolicy.INSURANCETYPE;
                }
                else
                {
                    context.TBL_COLLATERAL_POLICY.Add(new TBL_COLLATERAL_POLICY
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        REMARK = tempPolicy.REMARK,
                        ASSIGNDATE = tempPolicy.ASSIGNDATE,
                        INSURANCECOMPANYNAME = tempPolicy.INSURANCECOMPANYNAME,
                        INSURERADDRESS = tempPolicy.INSURERADDRESS,
                        INSURERDETAILS = tempPolicy.INSURERDETAILS,
                        ISOWNEDBYCUSTOMER = tempPolicy.ISOWNEDBYCUSTOMER,
                        INSURANCEPOLICYNUMBER = tempPolicy.INSURANCEPOLICYNUMBER,
                        POLICYAMOUNT = tempPolicy.POLICYAMOUNT,
                        POLICYRENEWALDATE = tempPolicy.POLICYRENEWALDATE,
                        POLICYSTARTDATE = tempPolicy.POLICYSTARTDATE,
                        PREMIUMAMOUNT = tempPolicy.PREMIUMAMOUNT,
                        RENEWALFREQUENCYTYPEID = tempPolicy.RENEWALFREQUENCYTYPEID,
                        INSURANCETYPEID = tempPolicy.INSURANCETYPEID,
                        //INSURANCETYPE = tempPolicy.INSURANCETYPE,

                    });
                }

            }
        }
        private void UpdateMetalCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {

            //check if this collateral code is not approved
            if (context.TBL_TEMP_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCODE == collateralcode && x.APPROVALSTATUSID != 2).Any() == true)
            {
                //get all collateral details from temp
                var tempMetal = context.TBL_TEMP_COLLATERAL_PREC_METAL.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
                if (tempMetal != null)
                {
                    //get collateral detial from main table
                    var mainMetal = (from x in context.TBL_COLLATERAL_PRECIOUSMETAL
                                     join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                     where c.COLLATERALCODE == collateralcode
                                     select (x)).FirstOrDefault();

                    if (mainMetal != null)
                    {
                        mainMetal.COLLATERALCUSTOMERID = newCollateralId;
                        mainMetal.REMARK = tempMetal.REMARK;
                        mainMetal.METALTYPE = tempMetal.METALTYPE;
                        mainMetal.PRECIOUSMETALFORM = tempMetal.PRECIOUSMETALFORM;
                        mainMetal.PRECIOUSMETALNAME = tempMetal.PRECIOUSMETALNAME;
                        mainMetal.UNITRATE = tempMetal.UNITRATE;
                        mainMetal.VALUATIONAMOUNT = tempMetal.VALUATIONAMOUNT;
                        mainMetal.WEIGHTINGRAMMES = tempMetal.WEIGHTINGRAMMES;

                    }
                    else
                    {
                        context.TBL_COLLATERAL_PRECIOUSMETAL.Add(new TBL_COLLATERAL_PRECIOUSMETAL
                        {
                            COLLATERALCUSTOMERID = newCollateralId,
                            REMARK = tempMetal.REMARK,
                            METALTYPE = tempMetal.METALTYPE,
                            PRECIOUSMETALFORM = tempMetal.PRECIOUSMETALFORM,
                            PRECIOUSMETALNAME = tempMetal.PRECIOUSMETALNAME,
                            UNITRATE = tempMetal.UNITRATE,
                            VALUATIONAMOUNT = tempMetal.VALUATIONAMOUNT,
                            WEIGHTINGRAMMES = tempMetal.WEIGHTINGRAMMES,
                        });
                    }
                }
            }
            else
            {
                var met = context.TBL_TEMP_COLLATERAL_PREC_METAL.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
                if (met != null)
                {
                    context.TBL_COLLATERAL_PRECIOUSMETAL.Add(new TBL_COLLATERAL_PRECIOUSMETAL
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        REMARK = met.REMARK,
                        METALTYPE = met.METALTYPE,
                        PRECIOUSMETALFORM = met.PRECIOUSMETALFORM,
                        PRECIOUSMETALNAME = met.PRECIOUSMETALNAME,
                        UNITRATE = met.UNITRATE,
                        VALUATIONAMOUNT = met.VALUATIONAMOUNT,
                        WEIGHTINGRAMMES = met.WEIGHTINGRAMMES,

                    });
                }
            }
        }
        private void UpdateStockCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var tempStock = context.TBL_TEMP_COLLATERAL_STOCK.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (tempStock != null)
            {
                //get collateral detial from main table
                var mainStock = (from x in context.TBL_COLLATERAL_STOCK
                                 join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                 where c.COLLATERALCODE == collateralcode
                                 select (x)).FirstOrDefault();

                if (mainStock != null)
                {
                    mainStock.COLLATERALCUSTOMERID = newCollateralId;
                    mainStock.AMOUNT = tempStock.AMOUNT;
                    mainStock.COMPANYNAME = tempStock.COMPANYNAME;
                    mainStock.MARKETPRICE = tempStock.MARKETPRICE;
                    mainStock.SHAREQUANTITY = tempStock.SHAREQUANTITY;
                    mainStock.SHARESSECURITYVALUE = tempStock.SHARESSECURITYVALUE;
                    mainStock.SHAREVALUEAMOUNTTOUSE = tempStock.SHAREVALUEAMOUNTTOUSE;

                }
                else
                {
                    context.TBL_COLLATERAL_STOCK.Add(new TBL_COLLATERAL_STOCK
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        AMOUNT = tempStock.AMOUNT,
                        COMPANYNAME = tempStock.COMPANYNAME,
                        MARKETPRICE = tempStock.MARKETPRICE,
                        SHAREQUANTITY = tempStock.SHAREQUANTITY,
                        SHARESSECURITYVALUE = tempStock.SHARESSECURITYVALUE,
                        SHAREVALUEAMOUNTTOUSE = tempStock.SHAREVALUEAMOUNTTOUSE,
                    });
                }
            }



        }
        private void UpdateVehicleCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var tempVehicle = context.TBL_TEMP_COLLATERAL_VEHICLE.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (tempVehicle != null)
            {
                //get collateral detial from main table
                var mainVehicle = (from x in context.TBL_COLLATERAL_VEHICLE
                                   join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                   where c.COLLATERALCODE == collateralcode
                                   select (x)).FirstOrDefault();

                if (mainVehicle != null)
                {
                    mainVehicle.COLLATERALCUSTOMERID = newCollateralId;
                    mainVehicle.CHASISNUMBER = tempVehicle.CHASISNUMBER;
                    mainVehicle.INVOICEVALUE = tempVehicle.INVOICEVALUE;
                    mainVehicle.ENGINENUMBER = tempVehicle.ENGINENUMBER;
                    mainVehicle.LASTVALUATIONAMOUNT = tempVehicle.LASTVALUATIONAMOUNT;
                    mainVehicle.MANUFACTUREDDATE = tempVehicle.MANUFACTUREDDATE;
                    mainVehicle.MODELNAME = tempVehicle.MODELNAME;
                    mainVehicle.NAMEOFOWNER = tempVehicle.NAMEOFOWNER;
                    mainVehicle.REGISTRATIONCOMPANY = tempVehicle.REGISTRATIONCOMPANY;
                    mainVehicle.REGISTRATIONNUMBER = tempVehicle.REGISTRATIONNUMBER;
                    mainVehicle.REMARK = tempVehicle.REMARK;
                    mainVehicle.RESALEVALUE = tempVehicle.RESALEVALUE;
                    mainVehicle.SERIALNUMBER = tempVehicle.SERIALNUMBER;
                    mainVehicle.VEHICLESTATUS = tempVehicle.VEHICLESTATUS;
                    mainVehicle.VALUATIONDATE = tempVehicle.VALUATIONDATE;
                    mainVehicle.VEHICLEMAKE = tempVehicle.VEHICLEMAKE;
                    mainVehicle.VEHICLETYPE = tempVehicle.VEHICLETYPE;

                }
                else
                {
                    context.TBL_COLLATERAL_VEHICLE.Add(new TBL_COLLATERAL_VEHICLE
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        CHASISNUMBER = tempVehicle.CHASISNUMBER,
                        INVOICEVALUE = tempVehicle.INVOICEVALUE,
                        ENGINENUMBER = tempVehicle.ENGINENUMBER,
                        LASTVALUATIONAMOUNT = tempVehicle.LASTVALUATIONAMOUNT,
                        MANUFACTUREDDATE = tempVehicle.MANUFACTUREDDATE,
                        MODELNAME = tempVehicle.MODELNAME,
                        NAMEOFOWNER = tempVehicle.NAMEOFOWNER,
                        REGISTRATIONCOMPANY = tempVehicle.REGISTRATIONCOMPANY,
                        REGISTRATIONNUMBER = tempVehicle.REGISTRATIONNUMBER,
                        REMARK = tempVehicle.REMARK,
                        RESALEVALUE = tempVehicle.RESALEVALUE,
                        SERIALNUMBER = tempVehicle.SERIALNUMBER,
                        VEHICLESTATUS = tempVehicle.VEHICLESTATUS,
                        VALUATIONDATE = tempVehicle.VALUATIONDATE,
                        VEHICLEMAKE = tempVehicle.VEHICLEMAKE,
                        VEHICLETYPE = tempVehicle.VEHICLETYPE,

                    });
                }
            }

        }

        private void UpdatePromissoryCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            var tempPromissory = context.TBL_TEMP_COLLATERAL_PROMISSORY.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();



            //get all collateral details from temp
            var promissorynoteExist = context.TBL_COLLATERAL_PROMISSORY.Where(x => x.PROMISSORYNOTEID == tempPromissory.PROMISSORYNOTEID).FirstOrDefault();
            if (promissorynoteExist != null)
            {
                throw new ConditionNotMetException("Promissory Note Has Been Used Before");
            }

            if (tempPromissory != null)
            {
                //get collateral detial from main table
                var mainVehicle = (from x in context.TBL_COLLATERAL_PROMISSORY
                                   join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                   where c.COLLATERALCODE == collateralcode
                                   select (x)).FirstOrDefault();

                if (mainVehicle != null)
                {
                    mainVehicle.COLLATERALCUSTOMERID = newCollateralId;
                    mainVehicle.PROMISSORYNOTEID = tempPromissory.PROMISSORYNOTEID;
                    mainVehicle.EFFECTIVEDATE = tempPromissory.EFFECTIVEDATE;
                    mainVehicle.MATURITYDATE = tempPromissory.MATURITYDATE;
                    //mainVehicle.PROMISSORYVALUE = tempPromissory.PROMISSORYVALUE;

                }
                else
                {
                    context.TBL_COLLATERAL_PROMISSORY.Add(new TBL_COLLATERAL_PROMISSORY
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        PROMISSORYNOTEID = tempPromissory.PROMISSORYNOTEID,
                        EFFECTIVEDATE = tempPromissory.EFFECTIVEDATE,
                        MATURITYDATE = tempPromissory.MATURITYDATE,
                        //PROMISSORYVALUE = tempPromissory.PROMISSORYVALUE,

                    });
                }
            }

        }

        private void UpdateCASAcollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var tempCasa = context.TBL_TEMP_COLLATERAL_CASA.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (tempCasa != null)
            {
                //get collateral detial from main table
                var mainCasa = (from x in context.TBL_COLLATERAL_DEPOSIT
                                join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                where c.COLLATERALCODE == collateralcode
                                select (x)).FirstOrDefault();


                if (mainCasa != null)
                {

                    mainCasa.ACCOUNTNUMBER = tempCasa.ACCOUNTNUMBER;
                    mainCasa.AVAILABLEBALANCE = tempCasa.AVAILABLEBALANCE;
                    mainCasa.COLLATERALCUSTOMERID = newCollateralId;
                    mainCasa.EXISTINGLIENAMOUNT = tempCasa.EXISTINGLIENAMOUNT;
                    //mainCasa.ISOWNEDBYCUSTOMER = tempCasa.ISOWNEDBYCUSTOMER;
                    mainCasa.LIENAMOUNT = tempCasa.LIENAMOUNT;
                    mainCasa.REMARK = tempCasa.REMARK;
                    mainCasa.SECURITYVALUE = tempCasa.SECURITYVALUE;
                    mainCasa.ACCOUNTNAME = tempCasa.ACCOUNTNAME;
                }
                else
                {


                    context.TBL_COLLATERAL_CASA.Add(new TBL_COLLATERAL_CASA
                    {
                        ACCOUNTNUMBER = tempCasa.ACCOUNTNUMBER,
                        AVAILABLEBALANCE = tempCasa.AVAILABLEBALANCE,
                        COLLATERALCUSTOMERID = newCollateralId,
                        EXISTINGLIENAMOUNT = tempCasa.EXISTINGLIENAMOUNT,
                        // ISOWNEDBYCUSTOMER = tempCasa.ISOWNEDBYCUSTOMER,
                        LIENAMOUNT = tempCasa.LIENAMOUNT,
                        REMARK = tempCasa.REMARK,
                        SECURITYVALUE = tempCasa.SECURITYVALUE,
                        ACCOUNTNAME = tempCasa.ACCOUNTNAME,
                    });
                }
            }

        }
        private void UpdateCollateralDocument(int tempCollateralId, int newCollateralId)
        {

            var doc = documentContext.TBL_TEMP_MEDIA_COLLATERAL_DOCS.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (doc != null)
            {
                documentContext.TBL_MEDIA_COLLATERAL_DOCUMENTS.Add(new TBL_MEDIA_COLLATERAL_DOCUMENTS
                {
                    CREATEDBY = doc.CREATEDBY,
                    DOCUMENTCODE = doc.DOCUMENTCODE,
                    DOCUMENTID = doc.DOCUMENTID,
                    FILEDATA = doc.FILEDATA,
                    FILEEXTENSION = doc.FILEEXTENSION,
                    FILENAME = doc.FILENAME,
                    ISPRIMARYDOCUMENT = doc.ISPRIMARYDOCUMENT,
                    SYSTEMDATETIME = doc.SYSTEMDATETIME,
                    COLLATERALCUSTOMERID = newCollateralId,
                    TARGETID = doc.TARGETID,
                    DOCUMENTTYPEID = doc.DOCUMENTTYPEID,
                });
                documentContext.SaveChanges();
            }
        }
        private void UpdateCollateralVisitation(int tempCollateralId, int newCollateralId)
        {

            var doc = documentContext.TBL_DOC_COLLATERAL_VISITATION.Where(x => x.COLLATERALCUSTOMERID == tempCollateralId).ToList();
            var data = context.TBL_COLLATERAL_VISITATION.Where(x => x.COLLATERALCUSTOMERID == tempCollateralId).ToList();
            if (data.Count() > 0)
            {
                foreach (var x in data)
                {
                    x.COLLATERALCUSTOMERID = newCollateralId;
                }
            }
            if (doc.Count() > 0)
            {
                foreach (var x in doc)
                {
                    x.APPROVALSTATUSID = (int)ApprovalStatusEnum.Approved;
                    x.COLLATERALCUSTOMERID = newCollateralId;
                }
                documentContext.SaveChanges();
            }
        }
        private void UpdateMiscellaneousCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var tempPol = context.TBL_TEMP_COLLATERAL_MISCELLAN.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            //get collateral detial from main table
            var mainPol = (from x in context.TBL_COLLATERAL_MISCELLANEOUS
                           join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                           where c.COLLATERALCODE == collateralcode
                           select (x)).FirstOrDefault();
            if (mainPol != null)
            {


                mainPol.COLLATERALCUSTOMERID = newCollateralId;
                mainPol.ISOWNEDBYCUSTOMER = tempPol.ISOWNEDBYCUSTOMER;
                mainPol.NAMEOFSECURITY = tempPol.NAMEOFSECURITY;
                mainPol.NOTE = tempPol.NOTE;
                mainPol.SECURITYVALUE = tempPol.SECURITYVALUE;

            }
            else
            {
                context.TBL_COLLATERAL_MISCELLANEOUS.Add(new TBL_COLLATERAL_MISCELLANEOUS
                {
                    COLLATERALCUSTOMERID = newCollateralId,
                    ISOWNEDBYCUSTOMER = tempPol.ISOWNEDBYCUSTOMER,
                    NAMEOFSECURITY = tempPol.NAMEOFSECURITY,
                    NOTE = tempPol.NOTE,
                    SECURITYVALUE = tempPol.SECURITYVALUE,

                });
            }

        }

        private void UpdateItemPolicyApproval(TBL_TEMP_COLLATERAL_ITEM_POLI data)
        {
            if (data != null)
            {
                context.TBL_COLLATERAL_ITEM_POLICY.Add(new TBL_COLLATERAL_ITEM_POLICY
                {
                    COLLATERALCUSTOMERID = data.COLLATERALCUSTOMERID,
                    CREATEDBY = data.CREATEDBY,
                    DATETIMECREATED = data.DATETIMECREATED,
                    ENDDATE = data.ENDDATE,
                    INSURANCECOMPANYID = data.INSURANCECOMPANYID,
                    INSURANCETYPEID = data.INSURANCETYPEID,
                    LASTUPDATEDBY = data.LASTUPDATEDBY,
                    POLICYREFERENCENUMBER = data.POLICYREFERENCENUMBER,
                    STARTDATE = data.STARTDATE,
                    SUMINSURED = data.SUMINSURED,
                    PREMIUMAMOUNT = data.PREMIUMAMOUNT,
                    DESCRIPTION = data.DESCRIPTION,
                    //  PREMIUMPERCENT = data.PREMIUMPERCENT
                });
                data.ISPOLICYAPPROVAL = false;
            }
        }


        public CollateralViewModel GetTempCollateralTypeByCollateralId(int collateralId, int typeId)
        {
            var data = new CollateralViewModel();
            switch (typeId)
            {
                case (int)CollateralTypeEnum.FixedDeposit: data = GetTempCollateralDeposit(collateralId); break;
                case (int)CollateralTypeEnum.PlantAndMachinery: data = GetTempCollateralMachinery(collateralId); break;
                case (int)CollateralTypeEnum.Miscellaneous: data = GetTempCollateralMiscellaneous(collateralId); break;
                case (int)CollateralTypeEnum.Gaurantee: data = GetTempCollateralGuarantee(collateralId); break;
                case (int)CollateralTypeEnum.CASA: data = GetTempCollateralCasa(collateralId); break;
                case (int)CollateralTypeEnum.Property: data = GetTempCollateralImmovableProperty(collateralId); break;
                case (int)CollateralTypeEnum.TreasuryBillsAndBonds: data = GetTempCollateralMarketableSecurities(collateralId); break;
                case (int)CollateralTypeEnum.InsurancePolicy: data = GetTempCollateralPolicy(collateralId); break;
                case (int)CollateralTypeEnum.PreciousMetal: data = GetTempCollateralPreciousMetal(collateralId); break;
                case (int)CollateralTypeEnum.MarketableSecurities_Shares: data = GetTempCollateralStock(collateralId); break;
                case (int)CollateralTypeEnum.Vehicle: data = GetTempCollateralVehicle(collateralId); break;
                case (int)CollateralTypeEnum.Promissory: data = GetTempCollateralPromissory(collateralId); break;
                case (int)CollateralTypeEnum.ISPO: data = GetTempCollateralISPO(collateralId); break;
                case (int)CollateralTypeEnum.DomiciliationContract: data = GetTempCollateralDomiciliationContract(collateralId); break;
                case (int)CollateralTypeEnum.DomiciliationSalary: data = GetTempCollateralDomiciliationSalary(collateralId); break;
                case (int)CollateralTypeEnum.Indemity: data = GetTempCollateralIndemnity(collateralId); break;


                default:
                    break;
            }

            return data;
        }
        private CollateralViewModel GetTempCollateralDeposit(int collateralId)
        {
            var specifics = context.TBL_TEMP_COLLATERAL_DEPOSIT.FirstOrDefault(x => x.TEMPCOLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                collateralDepositId = specifics.TEMPCOLLATERALDEPOSITID,
                dealReferenceNumber = specifics.DEALREFERENCENUMBER,
                accountNumber = specifics.ACCOUNTNUMBER,
                existingLienAmount = specifics.EXISTINGLIENAMOUNT,
                lienAmount = specifics.LIENAMOUNT,
                availableBalance = specifics.AVAILABLEBALANCE,
                securityValue = specifics.SECURITYVALUE,
                maturityDate = specifics.MATURITYDATE,
                maturityAmount = specifics.MATURITYAMOUNT,
                remark = specifics.REMARK,
                bank = specifics.BANK,
                effectiveDate = specifics.EFFECTIVEDATE,
                accountName = specifics.ACCOUNTNAME

            };
            // details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralMachinery(int collateralId)
        {
            var specifics = (from x in context.TBL_TEMP_COLLATERAL_PLANT_EQUP
                             where x.TEMPCOLLATERALCUSTOMERID == collateralId
                             select (x)).FirstOrDefault();

            var MACHINEVALUEBASENAME = (from value in context.TBL_MACHINEVALUE_BASE
                                        where value.MACHINEVALUEBASEID == specifics.VALUEBASETYPEID
                                        select (value.MACHINEVALUEBASENAME)).FirstOrDefault();



            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                machineName = specifics.MACHINENAME,
                description = specifics.DESCRIPTION,
                machineNumber = specifics.MACHINENUMBER,
                manufacturerName = specifics.MANUFACTURERNAME,
                yearOfManufacture = specifics.YEAROFMANUFACTURE,
                yearOfPurchase = specifics.YEAROFPURCHASE,
                valueBaseTypeName = context.TBL_COLLATERAL_VALUEBASE_TYPE.Where(o => o.COLLATERALVALUEBASETYPEID == specifics.VALUEBASETYPEID).Select(o => o.VALUEBASETYPENAME).FirstOrDefault(),// MACHINEVALUEBASENAME,
                                                                                                                                                                                                 // valueBaseTypeName = MACHINEVALUEBASENAME,
                machineCondition = specifics.MACHINECONDITION,
                machineryLocation = specifics.MACHINERYLOCATION,
                replacementValue = specifics.REPLACEMENTVALUE,
                equipmentSize = specifics.EQUIPMENTSIZE,
                intendedUse = specifics.INTENDEDUSE,
                remark = specifics.REMARK
            };
            //details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralMiscellaneous(int collateralId)
        {
            var specifics = context.TBL_TEMP_COLLATERAL_MISCELLAN.FirstOrDefault(x => x.TEMPCOLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                detailId = specifics.TEMPCOLLATERALMISCELLANEOUSID,
                securityName = specifics.NAMEOFSECURITY,
                securityValue = specifics.SECURITYVALUE,
                note = specifics.NOTE,
            };
            details = GetMiscellaneousNotes(details);
            // details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralGuarantee(int collateralId)
        {
            var specifics = context.TBL_TEMP_COLLATERAL_GAURANTEE.FirstOrDefault(x => x.TEMPCOLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                collateralGauranteeId = specifics.TEMPCOLLATERALGAURANTEEID,
                collateralCustomerId = specifics.TEMPCOLLATERALCUSTOMERID,
                //isOwnedByCustomer = (bool)specifics.ISOWNEDBYCUSTOMER,
                institutionName = specifics.INSTITUTIONNAME,
                guarantorAddress = specifics.GUARANTORADDRESS,
                //    guarantorReferenceNumber = specifics.GUARANTORREFERENCENUMBER,
                guaranteeValue = specifics.GUARANTEEVALUE,
                cStartDate = specifics.STARTDATE,
                endDate = specifics.ENDDATE,
                remark = specifics.REMARK,
                firstName = specifics.FIRSTNAME,
                middleName = specifics.MIDDLENAME,
                lastName = specifics.LASTNAME,
                bvn = specifics.BVN,
                rcNumber = specifics.RCNUMBER,
                phoneNumber1 = specifics.PHONENUMBER1,
                phoneNumber2 = specifics.PHONENUMBER2,
                emailAddress = specifics.EMAILADDRESS,
                relationship = specifics.RELATIONSHIP,
                relationshipDuration = specifics.RELATIONSHIPDURATION,
                taxNumber = specifics.TAXNUMBER
            };
            //details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralCasa(int collateralId)
        {
            var specifics = context.TBL_TEMP_COLLATERAL_CASA.FirstOrDefault(x => x.TEMPCOLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                collateralCustomerId = specifics.TEMPCOLLATERALCUSTOMERID,
                accountNumber = specifics.ACCOUNTNUMBER,
                //  isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                availableBalance = specifics.AVAILABLEBALANCE,
                //  existingLienAmount = specifics.EXISTINGLIENAMOUNT,
                lienAmount = specifics.LIENAMOUNT,
                securityValue = specifics.SECURITYVALUE,
                remark = specifics.REMARK,
                accountName = specifics.ACCOUNTNAME
            };
            //details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralImmovableProperty(int collateralId)
        {
            var details = (from x in context.TBL_TEMP_COLLATERAL_IMMOV_PROP
                           where x.TEMPCOLLATERALCUSTOMERID == collateralId
                           select new CollateralViewModel
                           {
                               collateralId = x.TEMPCOLLATERALCUSTOMERID,
                               collateralPropertyId = x.TEMPCOLLATERALPROPERTYID,
                               collateralCustomerId = x.TEMPCOLLATERALCUSTOMERID,
                               propertyName = x.PROPERTYNAME,
                               cityId = x.CITYID,
                               countryId = x.COUNTRYID,
                               constructionDate = x.CONSTRUCTIONDATE,
                               propertyAddress = x.PROPERTYADDRESS,
                               dateOfAcquisition = x.DATEOFACQUISITION,
                               lastValuationDate = x.LASTVALUATIONDATE,
                               valuerId = x.VALUERID,
                               valuerReferenceNumber = x.VALUERREFERENCENUMBER,
                               propertyValueBaseTypeId = x.PROPERTYVALUEBASETYPEID,
                               openMarketValue = x.OPENMARKETVALUE,
                               forcedSaleValue = x.FORCEDSALEVALUE,
                               stampToCover = x.STAMPTOCOVER,
                               securityValue = x.SECURITYVALUE,
                               collateralUsableAmount = x.COLLATERALUSABLEAMOUNT,
                               remark = x.REMARK,
                               nearestLandMark = x.NEARESTLANDMARK,
                               nearestBusStop = x.NEARESTBUSSTOP,
                               longitude = x.LONGITUDE,
                               latitude = x.LATITUDE,
                               perfectionStatusId = x.PERFECTIONSTATUSID,
                               perfectionStatusReason = x.PERFECTIONSTATUSREASON,
                               valuationAmount = x.VALUATIONAMOUNT,
                               cityName = x.TBL_CITY.CITYNAME,
                               isOwnerOccupied = x.ISOWNEROCCUPIED,
                               isResidential = x.ISRESIDENTIAL,
                               isAssetPledgedByThirdParty = x.ISASSETPLEDGEDBYTHRIDPARTY,
                               thirdPartyName = x.THRIDPARTYNAME,
                               isAssetManagedByTrustee = x.ISASSETMANAGEDBYTRUSTEE,
                               trusteeName = x.TRUSTEENAME,
                               stateName = x.TBL_STATE.STATENAME,
                               estimatedValue = x.ESTIMATEDVALUE,
                               localGovtName = x.TBL_LOCALGOVERNMENT.NAME,
                               bankShareOfCollateral = x.BANKSHAREOFCOLLATERAL,
                               countryName = context.TBL_COUNTRY.Where(a => a.COUNTRYID == x.COUNTRYID).Select(a => a.NAME).FirstOrDefault(),
                               collateralValuer = context.TBL_ACCREDITEDCONSULTANT.Where(a => a.ACCREDITEDCONSULTANTID == x.VALUERID).Select(a => a.NAME + ", " + a.FIRMNAME).FirstOrDefault(),
                               propertyBaseType = context.TBL_COLLATERAL_VALUEBASE_TYPE.Where(a => a.COLLATERALVALUEBASETYPEID == x.PROPERTYVALUEBASETYPEID).Select(a => a.VALUEBASETYPENAME).FirstOrDefault(),
                               perfectionStatusName = context.TBL_COLLATERAL_PERFECTN_STAT.Where(a => a.PERFECTIONSTATUSID == x.PERFECTIONSTATUSID).Select(a => a.PERFECTIONSTATUSNAME).FirstOrDefault()

                           }).FirstOrDefault();
            //details = GetPropertyVistation(details
            // details = GetTempCollateralInsurancePolicy(details);

            return details;
        }
        private CollateralViewModel GetTempCollateralMarketableSecurities(int collateralId)
        {
            var specifics = context.TBL_TEMP_COLLATERAL_MKT_SEC.Where(x => x.TEMPCOLLATERALCUSTOMERID == collateralId).FirstOrDefault();
            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                collateralMarketableSecurityId = specifics.TEMPCOLLATERALMARKETSECURITYID,
                collateralCustomerId = specifics.TEMPCOLLATERALCUSTOMERID,
                securityType = specifics.SECURITYTYPE,
                //   dealReferenceNumber = specifics.DEALREFERENCENUMBER,
                effectiveDate = specifics.EFFECTIVEDATE,
                maturityDate = specifics.MATURITYDATE,
                dealAmount = specifics.DEALAMOUNT,
                securityValue = specifics.SECURITYVALUE,
                lienUsableAmount = specifics.LIENUSABLEAMOUNT,
                issuerName = specifics.ISSUERNAME,
                issuerReferenceNumber = specifics.ISSUERREFERENCENUMBER,
                unitValue = specifics.UNITVALUE,
                numberOfUnits = specifics.NUMBEROFUNITS,
                rating = specifics.RATING,
                percentageInterest = specifics.PERCENTAGEINTEREST,
                interestPaymentFrequency = specifics.INTERESTPAYMENTFREQUENCY,
                remark = specifics.REMARK,
                fundName = specifics.FUNDNAME,
                bank = specifics.BANKPURCHASEDFROM,
            };
            // details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralPolicy(int collateralId)
        {
            var specifics = context.TBL_TEMP_COLLATERAL_POLICY.FirstOrDefault(x => x.TEMPCOLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                collateralInsurancePolicyId = specifics.TEMPCOLLATERALINSURPOLICYID,
                collateralCustomerId = specifics.TEMPCOLLATERALCUSTOMERID,
                isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                insurancePolicyNumber = specifics.INSURANCEPOLICYNUMBER,
                premiumAmount = specifics.PREMIUMAMOUNT,
                policyAmount = specifics.POLICYAMOUNT,
                insuranceCompanyName = specifics.INSURANCECOMPANYNAME,
                insurerAddress = specifics.INSURERADDRESS,
                policyStartDate = specifics.POLICYSTARTDATE,
                assignDate = specifics.ASSIGNDATE,
                renewalFrequencyTypeId = specifics.RENEWALFREQUENCYTYPEID,
                insurerDetails = specifics.INSURERDETAILS,
                policyRenewalDate = specifics.POLICYRENEWALDATE,
                remark = specifics.REMARK,
                policyinsuranceType = specifics.INSURANCETYPE,
            };
            // details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralPreciousMetal(int collateralId)
        {
            var specifics = context.TBL_TEMP_COLLATERAL_PREC_METAL.FirstOrDefault(x => x.TEMPCOLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                collateralPreciousMetalId = specifics.TEMPCOLLATERALPRECIOUSMETALID,
                collateralCustomerId = specifics.TEMPCOLLATERALCUSTOMERID,
                //isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                preciousMetalName = specifics.PRECIOUSMETALNAME,
                weightInGrammes = specifics.WEIGHTINGRAMMES,
                metalValuationAmount = specifics.VALUATIONAMOUNT,
                metalUnitRate = specifics.UNITRATE,
                preciousMetalFrm = specifics.PRECIOUSMETALFORM,
                metalType = specifics.METALTYPE,
                remark = specifics.REMARK,

            };
            //details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralStock(int collateralId)
        {
            var specifics = (from x in context.TBL_TEMP_COLLATERAL_STOCK
                             where x.TEMPCOLLATERALCUSTOMERID == collateralId
                             select (x)).FirstOrDefault();

            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                collateralStockId = specifics.TEMPCOLLATERALSTOCKID,
                collateralCustomerId = specifics.TEMPCOLLATERALCUSTOMERID,
                companyName = specifics.COMPANYNAME,
                shareQuantity = specifics.SHAREQUANTITY,
                marketPrice = specifics.MARKETPRICE,
                amount = specifics.AMOUNT,
                sharesSecurityValue = specifics.SHARESSECURITYVALUE,
                shareValueAmountToUse = specifics.SHAREVALUEAMOUNTTOUSE,
            };
            int compId = 0;
            details = GetCollateralInsurancePolicy(details);
            if (details.companyName != null) { compId = Convert.ToInt32(details.companyName); };
            details.companyName = context.TBL_STOCK_COMPANY.FirstOrDefault(x => x.STOCKID == compId).STOCKNAME;

            //details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralPromissory(int collateralId)
        {
            var specifics = context.TBL_TEMP_COLLATERAL_PROMISSORY.FirstOrDefault(x => x.TEMPCOLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                collateralPromissoryId = specifics.TEMPCOLLATERALPROMISSORYID,
                collateralCustomerId = specifics.TEMPCOLLATERALCUSTOMERID,
                promissoryNoteRefferenceNumber = specifics.PROMISSORYNOTEID,
                promissoryValue = specifics.PROMISSORYVALUE,
                promissoryEffectiveDate = specifics.EFFECTIVEDATE,
                promissoryMaturityDate = specifics.MATURITYDATE,
            };
            //  details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralISPO(int collateralId)
        {
            var collateral = context.TBL_TEMP_COLLATERAL_ISPO.Where(x => x.TEMPCOLLATERALCUSTOMERID == collateralId).Select(x => x).FirstOrDefault();
            var details = new CollateralViewModel
            {
                collateralId = collateral.TEMPCOLLATERALCUSTOMERID,
                collateralISPOId = collateral.TEMPCOLLATERALISPOID,
                accountNameToDebit = collateral.ACCOUNTNAMETODEBIT,
                accountNumberToDebit = collateral.ACCOUNTNUMBERTODEBIT,
                renewalFrequencyTypeId = collateral.FREQUENCYTYPEID,
                securityValue = collateral.SECURITYVALUE,
                regularPaymentAmount = collateral.REGULARPAYMENTAMOUNT,
                payer = collateral.PAYER,
                remark = collateral.REMARK,
                description = collateral.DESCRIPTION
            };
            //  details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralDomiciliationContract(int collateralId)
        {
            var collateral = context.TBL_TEMP_COLLATERAL_DOMCLTN.Where(x => x.TEMPCOLLATERALCUSTOMERID == collateralId).Select(x => x).FirstOrDefault();
            var details = new CollateralViewModel
            {
                collateralId = collateral.TEMPCOLLATERALCUSTOMERID,
                collateralDomiciliationId = collateral.TEMPCOLLATERALDOMICILIATIONID,
                contractDetail = collateral.CONTRACTDETAILS,
                contractEmployer = collateral.EMPLOYER,
                contractValue = collateral.CONTRACTVALUE,
                outstandingInvoiceAmount = collateral.OUTSTANDINGINVOICEAMOUNT,
                accountNameToDebit = collateral.ACCOUNTNAMETODEBIT,
                payer = collateral.PAYER,
                accountNumberToDebit = collateral.ACCOUNTNUMBERTODEBIT,
                regularPaymentAmount = collateral.REGULARPAYMENTAMOUNT,
                renewalFrequencyTypeId = collateral.FREQUENCYTYPEID,
                invoiceNumber = collateral.INVOICENUMBER,
                securityValue = collateral.SECURITYVALUE,
                invoiceDate = collateral.INVOICEDATE,
                remark = collateral.REMARK,
                description = collateral.DESCRIPTION
            };
            //  details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralDomiciliationSalary(int collateralId)
        {
            var collateral = context.TBL_TEMP_COLLATERAL_DOMCLTN.Where(x => x.TEMPCOLLATERALCUSTOMERID == collateralId).Select(x => x).FirstOrDefault();
            var details = new CollateralViewModel
            {
                collateralId = collateral.TEMPCOLLATERALCUSTOMERID,
                collateralDomiciliationId = collateral.TEMPCOLLATERALDOMICILIATIONID,
                contractDetail = collateral.CONTRACTDETAILS,
                contractEmployer = collateral.EMPLOYER,
                monthlySalary = collateral.MONTHLYSALARY,
                annualAllowances = collateral.ANNUALALLOWANCES,
                annualEmolument = collateral.ANNUALEMOLUMENT,
                accountNumber = collateral.ACCOUNTNUMBER,
                annualSalary = collateral.ANNUALSALARY,
                securityValue = collateral.SECURITYVALUE,
                remark = collateral.REMARK,
                description = collateral.DESCRIPTION
            };
            //  details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralIndemnity(int collateralId)
        {
            var collateral = context.TBL_TEMP_COLLATERAL_INDEMNITY.Where(x => x.TEMPCOLLATERALCUSTOMERID == collateralId).Select(x => x).FirstOrDefault();
            var details = new CollateralViewModel
            {
                collateralId = collateral.TEMPCOLLATERALCUSTOMERID,
                collateralIndemnityId = collateral.TEMPCOLLATERALINDEMNITYID,
                securityValue = collateral.SECURITYVALUE,
                remark = collateral.REMARK,
                address = collateral.ADDRESS,
                bvn = collateral.BVN,
                emailAddress = collateral.EMAILADRRESS,
                endDate = collateral.ENDDATE,
                startDate = collateral.STARTDATE,
                firstName = collateral.FIRSTNAME,
                middleName = collateral.MIDDLENAME,
                lastName = collateral.LASTNAME,
                phoneNumber1 = collateral.PHONENUMBER1,
                phoneNumber2 = collateral.PHONENUMBER2,
                relationshipDuration = collateral.RELATIONSHIPDURATION,
                relationship = collateral.RELATIONSHIP,
                taxNumber = collateral.TAXNUMBER,
                description = collateral.DESCRIPTION
            };
            //  details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetTempCollateralVehicle(int collateralId)
        {
            var specifics = context.TBL_TEMP_COLLATERAL_VEHICLE.FirstOrDefault(x => x.TEMPCOLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.TEMPCOLLATERALCUSTOMERID,
                collateralVehicleId = specifics.TEMPCOLLATERALVEHICLEID,
                collateralCustomerId = specifics.TEMPCOLLATERALCUSTOMERID,
                vehicleType = specifics.VEHICLETYPE,
                vehicleStatus = specifics.VEHICLESTATUS,
                vehicleMake = specifics.VEHICLEMAKE,
                modelName = specifics.MODELNAME,
                dateOfManufacture = specifics.MANUFACTUREDDATE,
                registrationNumber = specifics.REGISTRATIONNUMBER,
                serialNumber = specifics.SERIALNUMBER,
                chasisNumber = specifics.CHASISNUMBER,
                engineNumber = specifics.ENGINENUMBER,
                nameOfOwner = specifics.NAMEOFOWNER,
                registrationCompany = specifics.REGISTRATIONCOMPANY,
                resaleValue = specifics.RESALEVALUE,
                valuationDate = specifics.VALUATIONDATE,
                lastValuationAmount = specifics.LASTVALUATIONAMOUNT,
                invoiceValue = specifics.INVOICEVALUE,
                remark = specifics.REMARK,
            };
            //  details = GetTempCollateralInsurancePolicy(details);
            return details;
        }
        public List<InsurancePolicy> GetTempCollateralInsurancePolicy(int collateralId)
        {
            var insurance = (context.TBL_TEMP_COLLATERAL_ITEM_POLI.Where(x => x.COLLATERALCUSTOMERID == collateralId)
                .Select(x => new InsurancePolicy
                {

                    referenceNumber = x.POLICYREFERENCENUMBER,
                    insuranceCompanyId = x.INSURANCECOMPANYID,
                    sumInsured = x.SUMINSURED,
                    startDate = x.STARTDATE,
                    expiryDate = x.ENDDATE,
                    insuranceTypeId = x.INSURANCETYPEID,
                    description = x.DESCRIPTION,
                    // premiumPercent = x.PREMIUMPERCENT

                })).ToList();

            return insurance;

        }
        public List<InsurancePolicy> GetCollateralInsurancePolicy(int collateralId)
        {
            
                var insurance = (context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false)
                    .Select(x => new InsurancePolicy
                    {
                        collateralCustomerId = x.COLLATERALCUSTOMERID,
                        collateralInsuranceTrackingId = x.COLLATERALINSURANCETRACKINGID,
                        referenceNumber = x.POLICYNUMBER,
                        insuranceCompanyId = x.INSURANCECOMPANYID,
                        insuranceCompany = x.INSURANCECOMPANYID.Value == 0 ? x.OTHERINSURANCECOMPANY : context.TBL_INSURANCE_COMPANY.Where(o => o.INSURANCECOMPANYID == x.INSURANCECOMPANYID).Select(o => o.COMPANYNAME).FirstOrDefault(),
                        sumInsured = x.SUMINSURED,
                        startDate = x.INSURANCESTARTDATE,
                        expiryDate = x.INSURANCEENDDATE,
                        customerGroupId = (from a in context.TBL_CUSTOMER join b in context.TBL_LOAN_APPLICATION_DETAIL on a.CUSTOMERID equals b.CUSTOMERID join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID where b.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID select c.CUSTOMERGROUPID).FirstOrDefault(),
                        
                        insurancePolicyType = x.INSURANCEPOLICYTYPEID.Value == 0 ? x.OTHERINSURANCEPOLICYTYPE : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == x.INSURANCEPOLICYTYPEID).Select(o => o.DESCRIPTION).FirstOrDefault(),
                        insurancePolicyTypeId = x.INSURANCEPOLICYTYPEID,
                        insuranceStatus = context.TBL_COLLATERAL_INSURANCE_STATUS.Where(o => o.INSURANCESTATUSID == x.INSURANCESTATUSID).Select(o => o.INSURANCESTATUS).FirstOrDefault(),
                        inSurPremiumAmount = x.PREMIUMPAID,
                        insuranceStatusId = x.INSURANCESTATUSID,
                        companyAddress = x.ISURANCECOMPANYADDRESS,
                        valuationStartDate = x.VALUATIONSTARTDATE,
                        valuationEndDate = x.VALUATIONENDDATE,
                        omv = x.OMV,
                        fsv = x.FSV,
                        valuerId = x.VALUERID,
                        valuer = (x.VALUERID.Value == 0) ? x.OTHERVALUER : context.TBL_ACCREDITEDCONSULTANT.Where(b => b.ACCREDITEDCONSULTANTID == x.VALUERID).Select(b => b.FIRMNAME).FirstOrDefault(),
                        collateralDetails = x.COLLATERALDETAILS,
                        isInformationConfirmed = x.ISINFORMATIONCONFIRMED == true ? "TRUE" : "FALSE",
                        gpsCoordinates = x.GPSCOORDINATES,
                        collateralTypeId = x.COLLATERALTYPE,
                        collateralType = context.TBL_COLLATERAL_TYPE.Where(o => o.COLLATERALTYPEID == x.COLLATERALTYPE).Select(o => o.COLLATERALTYPENAME).FirstOrDefault(),
                        collateralSubTypeId = x.COLLATERALSUBTYPE,
                        collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(o => o.COLLATERALSUBTYPEID == x.COLLATERALSUBTYPE).Select(o => o.COLLATERALSUBTYPENAME).FirstOrDefault(),
                        loanAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID).Select(l => l.PROPOSEDAMOUNT).FirstOrDefault(),
                        loanStatus = (from a in context.TBL_LOAN_STATUS join s in context.TBL_LOAN on a.LOANSTATUSID equals s.LOANSTATUSID where s.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID select a.ACCOUNTSTATUS).FirstOrDefault(),
                        loanTypeName = (from y in context.TBL_PRODUCT join s in context.TBL_LOAN_APPLICATION_DETAIL on y.PRODUCTID equals s.PROPOSEDPRODUCTID where s.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID select y.PRODUCTNAME).FirstOrDefault(),
                        securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID select p.COLLATERALRELEASETYPENAME).FirstOrDefault(),
                        taxNumber = (from a in context.TBL_CUSTOMER join b in context.TBL_LOAN_APPLICATION_DETAIL on a.CUSTOMERID equals b.CUSTOMERID where b.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID select a.TAXNUMBER).FirstOrDefault(),
                        rcNumber = (from a in context.TBL_CUSTOMER join b in context.TBL_LOAN_APPLICATION_DETAIL on a.CUSTOMERID equals b.CUSTOMERID join s in context.TBL_CUSTOMER_COMPANYINFOMATION on b.CUSTOMERID equals s.CUSTOMERID where b.LOANAPPLICATIONDETAILID == x.LOANAPPLICATIONDETAILID select s.REGISTRATIONNUMBER).FirstOrDefault(),
                        firstLossPayee = x.FIRSTLOSSPAYEE,
                        insurableValue = x.INSURABLEVALUE,
                        requestComment = x.COMMENT,
                    })).ToList();

            foreach (var i in insurance)
            {
                if(i.collateralCustomerId != null)
                {
                    var collateralCustomer = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == i.collateralCustomerId).FirstOrDefault(); 
                    if(collateralCustomer != null)
                    {
                      i.customerId = (int)collateralCustomer.CUSTOMERID;
                    }
                }
                else
                {
                    var collateralCustomer = context.TBL_LOAN_APPLICATION_COLLATERL.Where(x => x.COLLATERALCUSTOMERID == i.collateralCustomerId).FirstOrDefault();
                    if (collateralCustomer != null)
                    {
                      i.customerId = (int)collateralCustomer.CUSTOMERID;
                    }
                }
                 
            }

                return insurance;
            
        }


        public string GetInsurancePolicyCollateralReport(int trackingId)
        {
            if (trackingId == 0)
            {
                return null;
            }

            var insurance = context.TBL_COLLATERAL_INSURANCE_TRACKING.Find(trackingId);
            var insuranceCompany = context.TBL_INSURANCE_COMPANY.Find(insurance.INSURANCECOMPANYID);
            var loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(insurance.LOANAPPLICATIONDETAILID);
            var loanApplication = context.TBL_LOAN_APPLICATION.Find(loanApplicationDetail.LOANAPPLICATIONID);
            var customerAccount = context.TBL_CASA.Where(x => x.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(x => x.PRODUCTACCOUNTNUMBER).FirstOrDefault();
            var staff = context.TBL_STAFF.Find(loanApplicationDetail.CREATEDBY);
            var rm = context.TBL_STAFF.Find(staff.SUPERVISOR_STAFFID);
            var gh = context.TBL_STAFF.Find(rm.SUPERVISOR_STAFFID);
            var customer = context.TBL_CUSTOMER.Find(loanApplicationDetail.CUSTOMERID);
            var customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(c => c.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(c => c.ADDRESS).FirstOrDefault();
            var teamName = stageContext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == staff.MISCODE).Select(x => x.TEAMNAME).FirstOrDefault();
            var divisionName = stageContext.STG_TEAM.Where(x => x.ACCOUNTOFFICERCODE == staff.MISCODE).Select(x => x.DIVISIONNAME).FirstOrDefault();
            var insurancePolicyType = context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == insurance.INSURANCEPOLICYTYPEID).Select(o => o.DESCRIPTION).FirstOrDefault();
            var loanTypeName = (from y in context.TBL_PRODUCT join s in context.TBL_LOAN_APPLICATION_DETAIL on y.PRODUCTID equals s.PROPOSEDPRODUCTID where s.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID select y.PRODUCTNAME).FirstOrDefault();
            var insuranceStatus = context.TBL_COLLATERAL_INSURANCE_STATUS.Where(o => o.INSURANCESTATUSID == insurance.INSURANCESTATUSID).Select(o => o.INSURANCESTATUS).FirstOrDefault();
            var valuer = insurance.VALUERID.Value == 0 ? insurance.OTHERVALUER : context.TBL_ACCREDITEDCONSULTANT.Where(b => b.ACCREDITEDCONSULTANTID == insurance.VALUERID).Select(b => b.FIRMNAME).FirstOrDefault();

            var loanAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID).Select(l => l.PROPOSEDAMOUNT).FirstOrDefault();
            var loanStatus = (from a in context.TBL_LOAN_STATUS join s in context.TBL_LOAN on a.LOANSTATUSID equals s.LOANSTATUSID where s.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID select a.ACCOUNTSTATUS).FirstOrDefault();
            //var loanTypeName = (from y in context.TBL_LOAN_APPLICATION_TYPE join p in context.TBL_LOAN_APPLICATION on y.LOANAPPLICATIONTYPEID equals p.LOANAPPLICATIONTYPEID join s in context.TBL_LOAN_APPLICATION_DETAIL on p.LOANAPPLICATIONID equals s.LOANAPPLICATIONID where s.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID select y.LOANAPPLICATIONTYPENAME).FirstOrDefault();
            var securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == insurance.COLLATERALCUSTOMERID select p.COLLATERALRELEASETYPENAME).FirstOrDefault();
            var taxNumber = (from a in context.TBL_CUSTOMER join b in context.TBL_LOAN_APPLICATION_DETAIL on a.CUSTOMERID equals b.CUSTOMERID where b.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID select a.TAXNUMBER).FirstOrDefault();
            var rcNumber = (from a in context.TBL_CUSTOMER join b in context.TBL_LOAN_APPLICATION_DETAIL on a.CUSTOMERID equals b.CUSTOMERID join s in context.TBL_CUSTOMER_COMPANYINFOMATION on b.CUSTOMERID equals s.CUSTOMERID where b.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID select s.REGISTRATIONNUMBER).FirstOrDefault();
            var firstLossPayee = insurance.FIRSTLOSSPAYEE;

            var result = String.Empty;
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                    <tr>
                        <td colspan=2 align=right><img src='/assets/images/access.jpg' alt='' width='245' height='52'></td>
                        
                    </tr>
                    <tr>
                        <td><b>INSURANCE POLICY REPORT</b></td>
                        <td>{insurance?.POLICYNUMBER}</td>
                    </tr>";
            result = result + $"</table>";
            result = result + $@"
                <table style='font face: arial; size:12px' border=1 width=900 cellpadding=0 cellspacing=0>
                                      
                    <tr>
                        <th><b>ACCOUNT OFFICER NAME:</b></th>
                        <th>{staff?.FIRSTNAME} {staff?.MIDDLENAME} {staff?.LASTNAME}</th>
                        <th><b>ACCOUNT OFFICER EMAIL:</b></th>
                        <th>{staff?.EMAIL}</th>
                    </tr>
                     <tr>
                        <th><b>TEAM:</b></th>
                        <th>{teamName}</th>
                        <th><b>GROUP HEAD:</b></th>
                        <th>{gh?.FIRSTNAME} {gh?.MIDDLENAME} {gh?.LASTNAME}</th>
                    </tr>
                      
                    <tr>
                        <td>DIVISION:</td>
                        <td>{divisionName}</td>
                        <td>CUSTOMER NAME:</td>
                        <td>{customer?.FIRSTNAME} {customer?.MIDDLENAME} {customer?.LASTNAME}</td>
                    </tr>
                    <tr>
                        <th><b>CUSTOMER EMAIL:</b></th>
                        <th>{customer?.EMAILADDRESS} </th>
                        <th><b>CUSTOMER PHONE NUMBER:</b></th>
                        <th>{customer?.PHONENUMBEROFSIGNATORY}</th>
                    </tr>
                    <tr>
                        <th><b>CUSTOMER ADDRESS:</b></th>
                        <th>{customerAddress} </th>
                        <th><b></b></th>
                        <th></th>
                    </tr>
                    <tr>
                        <td>ACCOUNT NUMBER:</td>
                        <td>{customerAccount}</td>
                        <td>CUSTOMER ID:</td>
                        <td>{customer?.CUSTOMERCODE}</td>
                    </tr>
                    <tr>
                        <td>COLLATERAL DETAILS:</td>
                        <td>{insurance?.COLLATERALDETAILS}</td>
                        <td>SUM INSURED</td>
                        <td>{string.Format("{0:#,##.00}", Convert.ToDecimal(insurance.SUMINSURED))}</td>
                    </tr>
                    <tr>
                        <td>PREMIUM PAID:</td>
                        <td>{string.Format("{0:#,##.00}", Convert.ToDecimal(insurance.PREMIUMPAID))}</td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>INSURANCE START DATE:</td>
                        <td>{insurance?.INSURANCESTARTDATE.Value.ToString("dd-MM-yyyy")}</td>
                        <td>INSURANCE END DATE:</td>
                        <td>{insurance?.INSURANCEENDDATE.Value.ToString("dd-MM-yyyy")}</td>
                    </tr>
                    <tr>
                        <td>INSURANCE POLICY TYPE:</td>
                        <td>{insurancePolicyType}</td>
                        <td>VALUATION START DATE:</td>
                        <td>{insurance?.VALUATIONSTARTDATE.Value.ToString("dd-MM-yyyy")}</td>
                        </tr>
                    <tr>
                        <td>VALUATION END DATE:</td>
                        <td>{insurance?.VALUATIONENDDATE.Value.ToString("dd-MM-yyyy")}</td>
                        <td>VALUATION OPEN MARKET VALUE:</td>
                        <td>{insurance?.OMV}</td>
                    </tr>
                    <tr>
                        <td>VALUATION OPEN MARKET VALUE:</td>
                        <td>{insurance?.FSV}</td>
                        <td>VALUER NAME:</td>
                        <td>{valuer}</td>
                    </tr>
                    <tr>
                        <td>LOAN AMOUNT:</td>
                        <td>{loanApplicationDetail?.APPROVEDAMOUNT}</td>
                        <td>LOAN TYPE:</td>
                        <td>{loanTypeName}</td>
                    </tr>
                    <tr>
                        <td>INSURANCE STATUS:</td>
                        <td>{insuranceStatus}</td>
                        <td>POLICY NUMBER:</td>
                        <td>{insurance?.POLICYNUMBER}</td>
                    </tr>
                    <tr>
                        <td>INSURANCE COMPANY:</td>
                        <td>{insuranceCompany?.COMPANYNAME}</td>
                        <td>LOAN AMOUNT:</td>
                        <td>{loanAmount}</td>
                    </tr>
                    <tr>
                        <td>LOAN STATUS:</td>
                        <td>{loanStatus}</td>
                        <td>SECURITY RELEASE STATUS:</td>
                        <td>{securityReleaseStatus}</td>
                    </tr>
                    <tr>
                        <td>TAX IDENTIFICATION NUMBER:</td>
                        <td>{taxNumber}</td>
                        <td>RC NUMBER:</td>
                        <td>{rcNumber}</td>
                    </tr>
                    <tr>
                        <td>FIRST LOSS PAYEE:</td>
                        <td>{firstLossPayee}</td>
                        <td>INSURABLE VALUE:</td>
                        <td>{string.Format("{0:#,##.00}", Convert.ToDecimal(insurance.INSURABLEVALUE))}</td>
                    </tr>
                    <tr>
                        <td>COMMENT:</td>
                        <td>{insurance.COMMENT}</td>
                        <td></td>
                        <td></td>
                    </tr>
                 ";
            result = result + $"</table>";

            return result;

        }

        public List<InsurancePolicy> GetCollateralInsurancePolicyReport(DateTime? startDate, DateTime? endDate, string searchString, int? businessUnitId)
        {
                List<InsurancePolicy> insurance = null;
                TBL_LOAN_APPLICATION_DETAIL loanApplicationDetail = null;
                TBL_LOAN_APPLICATION loanApplication = null;
                TBL_CUSTOMER customer = null;
                TBL_COLLATERAL_CUSTOMER customerCollateral = null;

                if (searchString == null || searchString == "")
                {
                    insurance = (context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(x => DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value >= DbFunctions.TruncateTime(startDate).Value && DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value <= DbFunctions.TruncateTime(endDate).Value && x.DELETED == false)
                       .Select(x => new InsurancePolicy
                       {
                           confirmedAdmin = context.TBL_STAFF.Where(a=>a.STAFFID == x.INFORMATIONCONFIRMEDBY).Select(a=>a.FIRSTNAME +" "+ a.MIDDLENAME +" "+ a.LASTNAME).FirstOrDefault(),
                           collateralInsuranceTrackingId = x.COLLATERALINSURANCETRACKINGID,
                           loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                           referenceNumber = x.POLICYNUMBER,
                           collateralCustomerId = x.COLLATERALCUSTOMERID,
                           sumInsured = x.SUMINSURED,
                           startDate = x.INSURANCESTARTDATE,
                           expiryDate = x.INSURANCEENDDATE,
                           inSurPremiumAmount = x.PREMIUMPAID,
                           companyAddress = x.ISURANCECOMPANYADDRESS,
                           valuationStartDate = x.VALUATIONSTARTDATE,
                           valuationEndDate = x.VALUATIONENDDATE,
                           insuranceCompanyId = x.INSURANCECOMPANYID,
                           omv = x.OMV,
                           fsv = x.FSV,
                           collateralDetails = x.COLLATERALDETAILS,
                           isInformationConfirmed = x.ISINFORMATIONCONFIRMED == true ? "TRUE" : "FALSE",
                           gpsCoordinates = x.GPSCOORDINATES,
                           firstLossPayee = x.FIRSTLOSSPAYEE,
                           insurableValue = x.INSURABLEVALUE,
                           requestComment = x.COMMENT,
                           premiumAmount = x.PREMIUMPAID,
                           otherInsuranceCompany = x.OTHERINSURANCECOMPANY,
                           valuerId = x.VALUERID,
                           otherValuers = x.OTHERVALUER,
                           collateralTypeId = x.COLLATERALTYPE,
                           collateralSubTypeId = x.COLLATERALSUBTYPE,
                           insurancePolicyTypeId = x.INSURANCEPOLICYTYPEID,
                           otherInsurancePolicyType = x.OTHERINSURANCEPOLICYTYPE,
                           insuranceStatusId = x.INSURANCESTATUSID,
                           createdBy = x.CREATEDBY == null ? 0 : (int)x.CREATEDBY,
                           customerId =context.TBL_COLLATERAL_CUSTOMER.Where(c=>c.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID ).Select(i=>i.CUSTOMERID).FirstOrDefault() ?? 0
                       })).OrderBy(x => x.collateralInsuranceTrackingId).ToList();

                
                if(businessUnitId != null && businessUnitId != 0)
                {
                    var sbuCustomers = context.TBL_CUSTOMER.Where(x => x.BUSINESSUNTID == businessUnitId).Select(i => i.CUSTOMERID).ToList();
                    insurance = insurance.Where(x => sbuCustomers.Contains(x.customerId)).ToList();
                }

                    foreach (var i in insurance)
                    {
                        if (i.loanApplicationDetailId > 0)
                        {
                            loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x=>x.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId)?.FirstOrDefault();
                            if (loanApplicationDetail != null)
                            {
                                loanApplication = context.TBL_LOAN_APPLICATION.Where(x=>x.LOANAPPLICATIONID == loanApplicationDetail.LOANAPPLICATIONID)?.FirstOrDefault();
                                i.loanTypeName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == loanApplicationDetail.PROPOSEDPRODUCTID).Select(x => x.PRODUCTNAME)?.FirstOrDefault();
                                i.customerAccount = context.TBL_CASA.Where(x => x.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(x => x.PRODUCTACCOUNTNUMBER)?.FirstOrDefault();
                                if (loanApplication != null)
                                {
                                    i.applicationreferenceNumber = loanApplication.APPLICATIONREFERENCENUMBER;
                                }
                                i.customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(c => c.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(c => c.ADDRESS)?.FirstOrDefault();
                                i.loanAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId).Select(l => l.PROPOSEDAMOUNT)?.FirstOrDefault();

                                var loan = context.TBL_LOAN.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility)?.FirstOrDefault();
                                if (loan != null)
                                {
                                    i.loanStatus = context.TBL_LOAN_STATUS.Where(x=>x.LOANSTATUSID == loan.LOANSTATUSID).Select(x=>x.ACCOUNTSTATUS)?.FirstOrDefault();
                                }
                                var contingent = context.TBL_LOAN_CONTINGENT.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability)?.FirstOrDefault();
                                if (contingent != null)
                                {
                                    i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == contingent.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                                var revolving = context.TBL_LOAN_REVOLVING.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility)?.FirstOrDefault();
                                if (revolving != null)
                                {
                                    i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == revolving.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                            }
                        }

                        if (i.collateralCustomerId > 0)
                        {
                            customerCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == i.collateralCustomerId)?.FirstOrDefault();
                            if (customerCollateral != null)
                            {
                                if (customerCollateral.CUSTOMERID != null)
                                {
                                    i.collateralCode = customerCollateral.COLLATERALCODE;
                                    i.securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == i.collateralCustomerId select p.COLLATERALRELEASETYPENAME)?.FirstOrDefault();
                                    customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerCollateral.CUSTOMERID)?.FirstOrDefault();
                                    i.taxNumber = customer?.TAXNUMBER;
                                    i.rcNumber = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.REGISTRATIONNUMBER)?.FirstOrDefault();
                                    i.customerName = customer?.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
                                    i.customerCode = customer?.CUSTOMERCODE;
                                    i.insurancePolicyType = (i.insurancePolicyTypeId == 0 || i.insurancePolicyTypeId == null) ? i.otherInsurancePolicyType : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == i.insurancePolicyTypeId).Select(o => o.DESCRIPTION)?.FirstOrDefault();
                                    i.customerPhone = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x=>x.PHONENUMBER).FirstOrDefault();
                                    i.divisionName = context.TBL_PROFILE_BUSINESS_UNIT.Where(x => x.BUSINESSUNITID == customer.BUSINESSUNTID).Select(x => x.BUSINESSUNITNAME + " " + x.BUSINESSUNITSHORTCODE).FirstOrDefault();
                                    i.customerEmail = customer?.EMAILADDRESS;

                            }
                                else
                                {
                                    i.securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == i.collateralCustomerId select p.COLLATERALRELEASETYPENAME)?.FirstOrDefault();
                                    customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == customerCollateral.CUSTOMERCODE)?.FirstOrDefault();
                                    i.taxNumber = customer?.TAXNUMBER;
                                    i.rcNumber = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.REGISTRATIONNUMBER)?.FirstOrDefault();
                                    i.customerName = customer?.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
                                    i.customerCode = customer?.CUSTOMERCODE;
                                    i.insurancePolicyType = (i.insurancePolicyTypeId == 0 || i.insurancePolicyTypeId == null) ? i.otherInsurancePolicyType : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == i.insurancePolicyTypeId).Select(o => o.DESCRIPTION)?.FirstOrDefault();
                                    i.customerPhone = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.PHONENUMBER).FirstOrDefault();
                                    i.divisionName = context.TBL_PROFILE_BUSINESS_UNIT.Where(x => x.BUSINESSUNITID == customer.BUSINESSUNTID).Select(x=>x.BUSINESSUNITNAME +" "+ x.BUSINESSUNITSHORTCODE).FirstOrDefault();
                                    i.customerEmail = customer?.EMAILADDRESS;
                                    i.collateralCode = customerCollateral.COLLATERALCODE;
                                }

                                var createdBy = (i.createdBy < 1) ? customerCollateral?.CREATEDBY : i.createdBy;
                                if (createdBy > 0)
                                {
                                    var staff = context.TBL_STAFF.Find(createdBy);

                                    if (staff != null && staff.SUPERVISOR_STAFFID > 0)
                                    {
                                        i.teamName = staff?.MISCODE;
                                        i.accountOfficerName = staff?.FIRSTNAME + " " + staff?.MIDDLENAME + " " + staff?.LASTNAME;
                                        i.accountOfficerEmail = staff?.EMAIL ?? string.Empty;
                                        var rm = context.TBL_STAFF.Find(staff.SUPERVISOR_STAFFID);
                                        if (rm != null && rm.SUPERVISOR_STAFFID > 0)
                                        {
                                            var zh = context.TBL_STAFF.Find(rm.SUPERVISOR_STAFFID);
                                            if(zh != null && zh.SUPERVISOR_STAFFID > 0)
                                            {
                                                var gh = context.TBL_STAFF.Find(zh.SUPERVISOR_STAFFID);
                                                i.groupHead = gh?.FIRSTNAME + " " + gh?.MIDDLENAME + " " + gh?.LASTNAME;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        i.insuranceCompany = (i.insuranceCompanyId > 0) ? context.TBL_INSURANCE_COMPANY.Where(o => o.INSURANCECOMPANYID == i.insuranceCompanyId).Select(o => o.COMPANYNAME).FirstOrDefault() : i.otherInsuranceCompany;
                        i.valuer = (i.valuerId > 0) ? context.TBL_ACCREDITEDCONSULTANT.Where(b => b.ACCREDITEDCONSULTANTID == i.valuerId).Select(b => b.FIRMNAME).FirstOrDefault() : i.otherValuers;
                        i.collateralType = (i.collateralTypeId > 0) ? context.TBL_COLLATERAL_TYPE.Where(o => o.COLLATERALTYPEID == i.collateralTypeId).Select(o => o.COLLATERALTYPENAME).FirstOrDefault() : "";
                        i.collateralSubType = (i.collateralSubTypeId > 0) ? context.TBL_COLLATERAL_TYPE_SUB.Where(o => o.COLLATERALSUBTYPEID == i.collateralSubTypeId).Select(o => o.COLLATERALSUBTYPENAME).FirstOrDefault() : "";
                    }

                }

                if (searchString.ToLower().Trim() == "all")
                {
                    insurance = (context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(x => DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value >= DbFunctions.TruncateTime(startDate).Value && DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value <= DbFunctions.TruncateTime(endDate).Value && x.DELETED == false)
                       .Select(x => new InsurancePolicy
                       {
                           confirmedAdmin = context.TBL_STAFF.Where(a => a.STAFFID == x.INFORMATIONCONFIRMEDBY).Select(a => a.FIRSTNAME + " " + a.MIDDLENAME + " " + a.LASTNAME).FirstOrDefault(),
                           collateralInsuranceTrackingId = x.COLLATERALINSURANCETRACKINGID,
                           loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                           referenceNumber = x.POLICYNUMBER,
                           collateralCustomerId = x.COLLATERALCUSTOMERID,
                           sumInsured = x.SUMINSURED,
                           startDate = x.INSURANCESTARTDATE,
                           expiryDate = x.INSURANCEENDDATE,
                           inSurPremiumAmount = x.PREMIUMPAID,
                           companyAddress = x.ISURANCECOMPANYADDRESS,
                           valuationStartDate = x.VALUATIONSTARTDATE,
                           valuationEndDate = x.VALUATIONENDDATE,
                           insuranceCompanyId = x.INSURANCECOMPANYID,
                           omv = x.OMV,
                           fsv = x.FSV,
                           collateralDetails = x.COLLATERALDETAILS,
                           isInformationConfirmed = x.ISINFORMATIONCONFIRMED == true ? "TRUE" : "FALSE",
                           gpsCoordinates = x.GPSCOORDINATES,
                           firstLossPayee = x.FIRSTLOSSPAYEE,
                           insurableValue = x.INSURABLEVALUE,
                           requestComment = x.COMMENT,
                           premiumAmount = x.PREMIUMPAID,
                           otherInsuranceCompany = x.OTHERINSURANCECOMPANY,
                           valuerId = x.VALUERID,
                           otherValuers = x.OTHERVALUER,
                           collateralTypeId = x.COLLATERALTYPE,
                           collateralSubTypeId = x.COLLATERALSUBTYPE,
                           insurancePolicyTypeId = x.INSURANCEPOLICYTYPEID,
                           otherInsurancePolicyType = x.OTHERINSURANCEPOLICYTYPE,
                           insuranceStatusId = x.INSURANCESTATUSID,
                           createdBy = x.CREATEDBY == null ? 0 : (int)x.CREATEDBY,
                           customerId = context.TBL_COLLATERAL_CUSTOMER.Where(c => c.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).Select(i => i.CUSTOMERID).FirstOrDefault() ?? 0

                       })).OrderBy(x => x.collateralInsuranceTrackingId).ToList();

                if (businessUnitId != null && businessUnitId != 0)
                {
                    var sbuCustomers = context.TBL_CUSTOMER.Where(x => x.BUSINESSUNTID == businessUnitId).Select(i => i.CUSTOMERID).ToList();
                    insurance = insurance.Where(x => sbuCustomers.Contains(x.customerId)).ToList();
                }

                foreach (var i in insurance)
                {
                    if (i.loanApplicationDetailId > 0)
                    {
                        loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId)?.FirstOrDefault();
                        if (loanApplicationDetail != null)
                        {
                            loanApplication = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == loanApplicationDetail.LOANAPPLICATIONID)?.FirstOrDefault();
                            i.loanTypeName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == loanApplicationDetail.PROPOSEDPRODUCTID).Select(x => x.PRODUCTNAME)?.FirstOrDefault();
                            i.customerAccount = context.TBL_CASA.Where(x => x.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(x => x.PRODUCTACCOUNTNUMBER)?.FirstOrDefault();
                            if (loanApplication != null)
                            {
                                i.applicationreferenceNumber = loanApplication.APPLICATIONREFERENCENUMBER;
                            }
                            i.customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(c => c.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(c => c.ADDRESS)?.FirstOrDefault();
                            i.loanAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId).Select(l => l.PROPOSEDAMOUNT)?.FirstOrDefault();

                            var loan = context.TBL_LOAN.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility)?.FirstOrDefault();
                            if (loan != null)
                            {
                                i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == loan.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                            var contingent = context.TBL_LOAN_CONTINGENT.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability)?.FirstOrDefault();
                            if (contingent != null)
                            {
                                i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == contingent.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                            var revolving = context.TBL_LOAN_REVOLVING.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility)?.FirstOrDefault();
                            if (revolving != null)
                            {
                                i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == revolving.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                        }
                    }

                    if (i.collateralCustomerId > 0)
                    {
                        customerCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == i.collateralCustomerId)?.FirstOrDefault();
                        if (customerCollateral != null)
                        {
                            if (customerCollateral.CUSTOMERID != null)
                            {
                                i.collateralCode = customerCollateral.COLLATERALCODE;
                                i.securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == i.collateralCustomerId select p.COLLATERALRELEASETYPENAME)?.FirstOrDefault();
                                customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerCollateral.CUSTOMERID)?.FirstOrDefault();
                                i.taxNumber = customer?.TAXNUMBER;
                                i.rcNumber = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.REGISTRATIONNUMBER)?.FirstOrDefault();
                                i.customerName = customer?.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
                                i.customerCode = customer?.CUSTOMERCODE;
                                i.insurancePolicyType = (i.insurancePolicyTypeId == 0 || i.insurancePolicyTypeId == null) ? i.otherInsurancePolicyType : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == i.insurancePolicyTypeId).Select(o => o.DESCRIPTION)?.FirstOrDefault();
                                i.customerPhone = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.PHONENUMBER).FirstOrDefault();
                                i.divisionName = context.TBL_PROFILE_BUSINESS_UNIT.Where(x => x.BUSINESSUNITID == customer.BUSINESSUNTID).Select(x => x.BUSINESSUNITNAME + " " + x.BUSINESSUNITSHORTCODE).FirstOrDefault();
                                i.customerEmail = customer?.EMAILADDRESS;

                            }
                            else
                            {
                                i.securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == i.collateralCustomerId select p.COLLATERALRELEASETYPENAME)?.FirstOrDefault();
                                customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == customerCollateral.CUSTOMERCODE)?.FirstOrDefault();
                                i.taxNumber = customer?.TAXNUMBER;
                                i.rcNumber = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.REGISTRATIONNUMBER)?.FirstOrDefault();
                                i.customerName = customer?.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
                                i.customerCode = customer?.CUSTOMERCODE;
                                i.insurancePolicyType = (i.insurancePolicyTypeId == 0 || i.insurancePolicyTypeId == null) ? i.otherInsurancePolicyType : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == i.insurancePolicyTypeId).Select(o => o.DESCRIPTION)?.FirstOrDefault();
                                i.customerPhone = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.PHONENUMBER).FirstOrDefault();
                                i.divisionName = context.TBL_PROFILE_BUSINESS_UNIT.Where(x => x.BUSINESSUNITID == customer.BUSINESSUNTID).Select(x => x.BUSINESSUNITNAME + " " + x.BUSINESSUNITSHORTCODE).FirstOrDefault();
                                i.customerEmail = customer?.EMAILADDRESS;
                                i.collateralCode = customerCollateral.COLLATERALCODE;

                            }

                            var createdBy = (i.createdBy < 1) ? customerCollateral?.CREATEDBY : i.createdBy;
                            if (createdBy > 0)
                            {
                                var staff = context.TBL_STAFF.Find(createdBy);

                                if (staff != null && staff.SUPERVISOR_STAFFID > 0)
                                {
                                    i.teamName = staff?.MISCODE;
                                    i.accountOfficerName = staff?.FIRSTNAME + " " + staff?.MIDDLENAME + " " + staff?.LASTNAME;
                                    i.accountOfficerEmail = staff?.EMAIL;
                                    var rm = context.TBL_STAFF.Find(staff.SUPERVISOR_STAFFID);
                                    if (rm != null && rm.SUPERVISOR_STAFFID > 0)
                                    {
                                        var zh = context.TBL_STAFF.Find(rm.SUPERVISOR_STAFFID);
                                        if (zh != null && zh.SUPERVISOR_STAFFID > 0)
                                        {
                                            var gh = context.TBL_STAFF.Find(zh.SUPERVISOR_STAFFID);
                                            i.groupHead = gh?.FIRSTNAME + " " + gh?.MIDDLENAME + " " + gh?.LASTNAME;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    i.insuranceCompany = (i.insuranceCompanyId > 0) ? context.TBL_INSURANCE_COMPANY.Where(o => o.INSURANCECOMPANYID == i.insuranceCompanyId).Select(o => o.COMPANYNAME).FirstOrDefault() : i.otherInsuranceCompany;
                    i.valuer = (i.valuerId > 0) ? context.TBL_ACCREDITEDCONSULTANT.Where(b => b.ACCREDITEDCONSULTANTID == i.valuerId).Select(b => b.FIRMNAME).FirstOrDefault() : i.otherValuers;
                    i.collateralType = (i.collateralTypeId > 0) ? context.TBL_COLLATERAL_TYPE.Where(o => o.COLLATERALTYPEID == i.collateralTypeId).Select(o => o.COLLATERALTYPENAME).FirstOrDefault() : "";
                    i.collateralSubType = (i.collateralSubTypeId > 0) ? context.TBL_COLLATERAL_TYPE_SUB.Where(o => o.COLLATERALSUBTYPEID == i.collateralSubTypeId).Select(o => o.COLLATERALSUBTYPENAME).FirstOrDefault() : "";
                }

            }

                if (searchString.ToLower().Trim() == "active")
                {
                    insurance = (context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(x => DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value >= DbFunctions.TruncateTime(startDate).Value && DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value <= DbFunctions.TruncateTime(endDate).Value && (DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value >= DbFunctions.TruncateTime(DateTime.Now)) && x.DELETED == false)
                       .Select(x => new InsurancePolicy
                       {
                           confirmedAdmin = context.TBL_STAFF.Where(a => a.STAFFID == x.INFORMATIONCONFIRMEDBY).Select(a => a.FIRSTNAME + " " + a.MIDDLENAME + " " + a.LASTNAME).FirstOrDefault(),
                           collateralInsuranceTrackingId = x.COLLATERALINSURANCETRACKINGID,
                           loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                           referenceNumber = x.POLICYNUMBER,
                           collateralCustomerId = x.COLLATERALCUSTOMERID,
                           sumInsured = x.SUMINSURED,
                           startDate = x.INSURANCESTARTDATE,
                           expiryDate = x.INSURANCEENDDATE,
                           inSurPremiumAmount = x.PREMIUMPAID,
                           companyAddress = x.ISURANCECOMPANYADDRESS,
                           valuationStartDate = x.VALUATIONSTARTDATE,
                           valuationEndDate = x.VALUATIONENDDATE,
                           insuranceCompanyId = x.INSURANCECOMPANYID,
                           omv = x.OMV,
                           fsv = x.FSV,
                           collateralDetails = x.COLLATERALDETAILS,
                           isInformationConfirmed = x.ISINFORMATIONCONFIRMED == true ? "TRUE" : "FALSE",
                           gpsCoordinates = x.GPSCOORDINATES,
                           firstLossPayee = x.FIRSTLOSSPAYEE,
                           insurableValue = x.INSURABLEVALUE,
                           requestComment = x.COMMENT,
                           premiumAmount = x.PREMIUMPAID,
                           otherInsuranceCompany = x.OTHERINSURANCECOMPANY,
                           valuerId = x.VALUERID,
                           otherValuers = x.OTHERVALUER,
                           collateralTypeId = x.COLLATERALTYPE,
                           collateralSubTypeId = x.COLLATERALSUBTYPE,
                           insurancePolicyTypeId = x.INSURANCEPOLICYTYPEID,
                           otherInsurancePolicyType = x.OTHERINSURANCEPOLICYTYPE,
                           insuranceStatusId = x.INSURANCESTATUSID,
                           createdBy = x.CREATEDBY == null ? 0 : (int)x.CREATEDBY,
                           customerId = context.TBL_COLLATERAL_CUSTOMER.Where(c => c.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).Select(i => i.CUSTOMERID).FirstOrDefault() ?? 0

                       })).OrderBy(x => x.collateralInsuranceTrackingId).ToList();

                if (businessUnitId != null && businessUnitId != 0)
                {
                    var sbuCustomers = context.TBL_CUSTOMER.Where(x => x.BUSINESSUNTID == businessUnitId).Select(i => i.CUSTOMERID).ToList();
                    insurance = insurance.Where(x => sbuCustomers.Contains(x.customerId)).ToList();
                }

                foreach (var i in insurance)
                {
                    if (i.loanApplicationDetailId > 0)
                    {
                        loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId)?.FirstOrDefault();
                        if (loanApplicationDetail != null)
                        {
                            loanApplication = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == loanApplicationDetail.LOANAPPLICATIONID)?.FirstOrDefault();
                            i.loanTypeName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == loanApplicationDetail.PROPOSEDPRODUCTID).Select(x => x.PRODUCTNAME)?.FirstOrDefault();
                            i.customerAccount = context.TBL_CASA.Where(x => x.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(x => x.PRODUCTACCOUNTNUMBER)?.FirstOrDefault();
                            if (loanApplication != null)
                            {
                                i.applicationreferenceNumber = loanApplication.APPLICATIONREFERENCENUMBER;
                            }
                            i.customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(c => c.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(c => c.ADDRESS)?.FirstOrDefault();
                            i.loanAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId).Select(l => l.PROPOSEDAMOUNT)?.FirstOrDefault();

                            var loan = context.TBL_LOAN.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility)?.FirstOrDefault();
                            if (loan != null)
                            {
                                i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == loan.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                            var contingent = context.TBL_LOAN_CONTINGENT.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability)?.FirstOrDefault();
                            if (contingent != null)
                            {
                                i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == contingent.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                            var revolving = context.TBL_LOAN_REVOLVING.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility)?.FirstOrDefault();
                            if (revolving != null)
                            {
                                i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == revolving.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                        }
                    }

                    if (i.collateralCustomerId > 0)
                    {
                        customerCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == i.collateralCustomerId)?.FirstOrDefault();
                        if (customerCollateral != null)
                        {
                            if (customerCollateral.CUSTOMERID != null)
                            {
                                i.collateralCode = customerCollateral.COLLATERALCODE;
                                i.securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == i.collateralCustomerId select p.COLLATERALRELEASETYPENAME)?.FirstOrDefault();
                                customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerCollateral.CUSTOMERID)?.FirstOrDefault();
                                i.taxNumber = customer?.TAXNUMBER;
                                i.rcNumber = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.REGISTRATIONNUMBER)?.FirstOrDefault();
                                i.customerName = customer?.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
                                i.customerCode = customer?.CUSTOMERCODE;
                                i.insurancePolicyType = (i.insurancePolicyTypeId == 0 || i.insurancePolicyTypeId == null) ? i.otherInsurancePolicyType : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == i.insurancePolicyTypeId).Select(o => o.DESCRIPTION)?.FirstOrDefault();
                                i.customerPhone = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.PHONENUMBER).FirstOrDefault();
                                i.divisionName = context.TBL_PROFILE_BUSINESS_UNIT.Where(x => x.BUSINESSUNITID == customer.BUSINESSUNTID).Select(x => x.BUSINESSUNITNAME + " " + x.BUSINESSUNITSHORTCODE).FirstOrDefault();
                                i.customerEmail = customer?.EMAILADDRESS;

                            }
                            else
                            {
                                i.securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == i.collateralCustomerId select p.COLLATERALRELEASETYPENAME)?.FirstOrDefault();
                                customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == customerCollateral.CUSTOMERCODE)?.FirstOrDefault();
                                i.taxNumber = customer?.TAXNUMBER;
                                i.rcNumber = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.REGISTRATIONNUMBER)?.FirstOrDefault();
                                i.customerName = customer?.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
                                i.customerCode = customer?.CUSTOMERCODE;
                                i.insurancePolicyType = (i.insurancePolicyTypeId == 0 || i.insurancePolicyTypeId == null) ? i.otherInsurancePolicyType : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == i.insurancePolicyTypeId).Select(o => o.DESCRIPTION)?.FirstOrDefault();
                                i.customerPhone = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.PHONENUMBER).FirstOrDefault();
                                i.divisionName = context.TBL_PROFILE_BUSINESS_UNIT.Where(x => x.BUSINESSUNITID == customer.BUSINESSUNTID).Select(x => x.BUSINESSUNITNAME + " " + x.BUSINESSUNITSHORTCODE).FirstOrDefault();
                                i.customerEmail = customer?.EMAILADDRESS;
                                i.collateralCode = customerCollateral.COLLATERALCODE;

                            }

                            var createdBy = (i.createdBy < 1) ? customerCollateral?.CREATEDBY : i.createdBy;
                            if (createdBy > 0)
                            {
                                var staff = context.TBL_STAFF.Find(createdBy);

                                if (staff != null && staff.SUPERVISOR_STAFFID > 0)
                                {
                                    i.teamName = staff?.MISCODE;
                                    i.accountOfficerName = staff?.FIRSTNAME + " " + staff?.MIDDLENAME + " " + staff?.LASTNAME;
                                    i.accountOfficerEmail = staff?.EMAIL;
                                    var rm = context.TBL_STAFF.Find(staff.SUPERVISOR_STAFFID);
                                    if (rm != null && rm.SUPERVISOR_STAFFID > 0)
                                    {
                                        var zh = context.TBL_STAFF.Find(rm.SUPERVISOR_STAFFID);
                                        if (zh != null && zh.SUPERVISOR_STAFFID > 0)
                                        {
                                            var gh = context.TBL_STAFF.Find(zh.SUPERVISOR_STAFFID);
                                            i.groupHead = gh?.FIRSTNAME + " " + gh?.MIDDLENAME + " " + gh?.LASTNAME;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    i.insuranceCompany = (i.insuranceCompanyId > 0) ? context.TBL_INSURANCE_COMPANY.Where(o => o.INSURANCECOMPANYID == i.insuranceCompanyId).Select(o => o.COMPANYNAME).FirstOrDefault() : i.otherInsuranceCompany;
                    i.valuer = (i.valuerId > 0) ? context.TBL_ACCREDITEDCONSULTANT.Where(b => b.ACCREDITEDCONSULTANTID == i.valuerId).Select(b => b.FIRMNAME).FirstOrDefault() : i.otherValuers;
                    i.collateralType = (i.collateralTypeId > 0) ? context.TBL_COLLATERAL_TYPE.Where(o => o.COLLATERALTYPEID == i.collateralTypeId).Select(o => o.COLLATERALTYPENAME).FirstOrDefault() : "";
                    i.collateralSubType = (i.collateralSubTypeId > 0) ? context.TBL_COLLATERAL_TYPE_SUB.Where(o => o.COLLATERALSUBTYPEID == i.collateralSubTypeId).Select(o => o.COLLATERALSUBTYPENAME).FirstOrDefault() : "";
                }

            }

                if (searchString.ToLower().Trim() == "expired")
                {
                    insurance = (context.TBL_COLLATERAL_INSURANCE_TRACKING.Where(x => DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value >= DbFunctions.TruncateTime(startDate).Value && DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value <= DbFunctions.TruncateTime(endDate).Value && (DbFunctions.TruncateTime(x.INSURANCEENDDATE).Value < DbFunctions.TruncateTime(DateTime.Now)) && x.DELETED == false)
                       .Select(x => new InsurancePolicy
                       {
                           confirmedAdmin = context.TBL_STAFF.Where(a => a.STAFFID == x.INFORMATIONCONFIRMEDBY).Select(a => a.FIRSTNAME + " " + a.MIDDLENAME + " " + a.LASTNAME).FirstOrDefault(),

                           collateralInsuranceTrackingId = x.COLLATERALINSURANCETRACKINGID,
                           loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                           referenceNumber = x.POLICYNUMBER,
                           collateralCustomerId = x.COLLATERALCUSTOMERID,
                           sumInsured = x.SUMINSURED,
                           startDate = x.INSURANCESTARTDATE,
                           expiryDate = x.INSURANCEENDDATE,
                           inSurPremiumAmount = x.PREMIUMPAID,
                           companyAddress = x.ISURANCECOMPANYADDRESS,
                           valuationStartDate = x.VALUATIONSTARTDATE,
                           valuationEndDate = x.VALUATIONENDDATE,
                           insuranceCompanyId = x.INSURANCECOMPANYID,
                           omv = x.OMV,
                           fsv = x.FSV,
                           collateralDetails = x.COLLATERALDETAILS,
                           isInformationConfirmed = x.ISINFORMATIONCONFIRMED == true ? "TRUE" : "FALSE",
                           gpsCoordinates = x.GPSCOORDINATES,
                           firstLossPayee = x.FIRSTLOSSPAYEE,
                           insurableValue = x.INSURABLEVALUE,
                           requestComment = x.COMMENT,
                           premiumAmount = x.PREMIUMPAID,
                           otherInsuranceCompany = x.OTHERINSURANCECOMPANY,
                           valuerId = x.VALUERID,
                           otherValuers = x.OTHERVALUER,
                           collateralTypeId = x.COLLATERALTYPE,
                           collateralSubTypeId = x.COLLATERALSUBTYPE,
                           insurancePolicyTypeId = x.INSURANCEPOLICYTYPEID,
                           otherInsurancePolicyType = x.OTHERINSURANCEPOLICYTYPE,
                           insuranceStatusId = x.INSURANCESTATUSID,
                           createdBy = x.CREATEDBY == null ? 0 : (int)x.CREATEDBY,
                           customerId = context.TBL_COLLATERAL_CUSTOMER.Where(c => c.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).Select(i => i.CUSTOMERID).FirstOrDefault() ?? 0
                       })).OrderBy(x => x.collateralInsuranceTrackingId).ToList();

                if (businessUnitId != null && businessUnitId != 0)
                {
                    var sbuCustomers = context.TBL_CUSTOMER.Where(x => x.BUSINESSUNTID == businessUnitId).Select(i => i.CUSTOMERID).ToList();
                    insurance = insurance.Where(x => sbuCustomers.Contains(x.customerId)).ToList();
                }

                foreach (var i in insurance)
                    {
                        if (i.loanApplicationDetailId > 0)
                        {
                            loanApplicationDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(x=>x.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId)?.FirstOrDefault();
                            if (loanApplicationDetail != null)
                            {
                                loanApplication = context.TBL_LOAN_APPLICATION.Where(x=>x.LOANAPPLICATIONID == loanApplicationDetail.LOANAPPLICATIONID)?.FirstOrDefault();
                                i.loanTypeName = context.TBL_PRODUCT.Where(x => x.PRODUCTID == loanApplicationDetail.PROPOSEDPRODUCTID).Select(x => x.PRODUCTNAME)?.FirstOrDefault();
                                i.customerAccount = context.TBL_CASA.Where(x => x.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(x => x.PRODUCTACCOUNTNUMBER)?.FirstOrDefault();
                                if (loanApplication != null)
                                {
                                    i.applicationreferenceNumber = loanApplication.APPLICATIONREFERENCENUMBER;
                                }
                                i.customerAddress = context.TBL_CUSTOMER_ADDRESS.Where(c => c.CUSTOMERID == loanApplicationDetail.CUSTOMERID).Select(c => c.ADDRESS)?.FirstOrDefault();
                                i.loanAmount = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == i.loanApplicationDetailId).Select(l => l.PROPOSEDAMOUNT)?.FirstOrDefault();

                                var loan = context.TBL_LOAN.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.TermDisbursedFacility)?.FirstOrDefault();
                                if (loan != null)
                                {
                                    i.loanStatus = context.TBL_LOAN_STATUS.Where(x=>x.LOANSTATUSID == loan.LOANSTATUSID).Select(x=>x.ACCOUNTSTATUS)?.FirstOrDefault();
                                }
                                var contingent = context.TBL_LOAN_CONTINGENT.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.ContingentLiability)?.FirstOrDefault();
                                if (contingent != null)
                                {
                                    i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == contingent.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                                var revolving = context.TBL_LOAN_REVOLVING.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetail.LOANAPPLICATIONDETAILID && l.LOANSYSTEMTYPEID == (int)LoanSystemTypeEnum.OverdraftFacility)?.FirstOrDefault();
                                if (revolving != null)
                                {
                                    i.loanStatus = context.TBL_LOAN_STATUS.Where(x => x.LOANSTATUSID == revolving.LOANSTATUSID).Select(x => x.ACCOUNTSTATUS)?.FirstOrDefault();
                            }
                            }
                        }

                        if (i.collateralCustomerId > 0)
                        {
                            customerCollateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == i.collateralCustomerId)?.FirstOrDefault();
                            if (customerCollateral != null)
                            {
                                if (customerCollateral.CUSTOMERID != null)
                                {
                                    i.collateralCode = customerCollateral.COLLATERALCODE;
                                     i.securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == i.collateralCustomerId select p.COLLATERALRELEASETYPENAME)?.FirstOrDefault();
                                    customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERID == customerCollateral.CUSTOMERID)?.FirstOrDefault();
                                    i.taxNumber = customer?.TAXNUMBER;
                                    i.rcNumber = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.REGISTRATIONNUMBER)?.FirstOrDefault();
                                    i.customerName = customer?.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
                                    i.customerCode = customer?.CUSTOMERCODE;
                                    i.insurancePolicyType = (i.insurancePolicyTypeId == 0 || i.insurancePolicyTypeId == null) ? i.otherInsurancePolicyType : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == i.insurancePolicyTypeId).Select(o => o.DESCRIPTION)?.FirstOrDefault();
                                    i.customerPhone = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.PHONENUMBER).FirstOrDefault();
                                    i.divisionName = context.TBL_PROFILE_BUSINESS_UNIT.Where(x => x.BUSINESSUNITID == customer.BUSINESSUNTID).Select(x => x.BUSINESSUNITNAME + " " + x.BUSINESSUNITSHORTCODE).FirstOrDefault();
                                    i.customerEmail = customer?.EMAILADDRESS;

                            }
                                else
                                {
                                    i.securityReleaseStatus = (from y in context.TBL_COLLATERAL_RELEASE join p in context.TBL_COLLATERAL_RELEASE_TYPE on y.COLLATERALRELEASETYPEID equals p.COLLATERALRELEASETYPEID where y.COLLATERALCUSTOMERID == i.collateralCustomerId select p.COLLATERALRELEASETYPENAME)?.FirstOrDefault();
                                    customer = context.TBL_CUSTOMER.Where(x => x.CUSTOMERCODE == customerCollateral.CUSTOMERCODE)?.FirstOrDefault();
                                    i.taxNumber = customer?.TAXNUMBER;
                                    i.rcNumber = context.TBL_CUSTOMER_COMPANYINFOMATION.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.REGISTRATIONNUMBER)?.FirstOrDefault();
                                    i.customerName = customer?.FIRSTNAME + " " + customer?.MIDDLENAME + " " + customer?.LASTNAME;
                                    i.customerCode = customer?.CUSTOMERCODE;
                                    i.insurancePolicyType = (i.insurancePolicyTypeId == 0 || i.insurancePolicyTypeId == null) ? i.otherInsurancePolicyType : context.TBL_INSURANCE_POLICY_TYPE.Where(o => o.POLICYTYPEID == i.insurancePolicyTypeId).Select(o => o.DESCRIPTION)?.FirstOrDefault();
                                    i.customerPhone = customer?.PHONENUMBEROFSIGNATORY;
                                    i.customerPhone = context.TBL_CUSTOMER_PHONECONTACT.Where(x => x.CUSTOMERID == customer.CUSTOMERID).Select(x => x.PHONENUMBER).FirstOrDefault();
                                    i.divisionName = context.TBL_PROFILE_BUSINESS_UNIT.Where(x => x.BUSINESSUNITID == customer.BUSINESSUNTID).Select(x => x.BUSINESSUNITNAME + " " + x.BUSINESSUNITSHORTCODE).FirstOrDefault();
                                    i.customerEmail = customer?.EMAILADDRESS;
                                    i.collateralCode = customerCollateral.COLLATERALCODE;

                            }

                                var createdBy = (i.createdBy < 1) ? customerCollateral?.CREATEDBY : i.createdBy;
                                if (createdBy > 0)
                                {
                                    var staff = context.TBL_STAFF.Find(createdBy);

                                    if (staff != null && staff.SUPERVISOR_STAFFID > 0)
                                    {
                                        i.teamName = staff?.MISCODE;
                                        i.accountOfficerName = staff?.FIRSTNAME + " " + staff?.MIDDLENAME + " " + staff?.LASTNAME;
                                        i.accountOfficerEmail = staff?.EMAIL;
                                        var rm = context.TBL_STAFF.Find(staff.SUPERVISOR_STAFFID);
                                        if (rm != null && rm.SUPERVISOR_STAFFID > 0)
                                        {
                                            var zh = context.TBL_STAFF.Find(rm.SUPERVISOR_STAFFID);
                                            if(zh != null && zh.SUPERVISOR_STAFFID > 0)
                                            {
                                                var gh = context.TBL_STAFF.Find(zh.SUPERVISOR_STAFFID);
                                                i.groupHead = gh?.FIRSTNAME + " " + gh?.MIDDLENAME + " " + gh?.LASTNAME;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        i.insuranceCompany = (i.insuranceCompanyId > 0) ? context.TBL_INSURANCE_COMPANY.Where(o => o.INSURANCECOMPANYID == i.insuranceCompanyId).Select(o => o.COMPANYNAME).FirstOrDefault() : i.otherInsuranceCompany;
                        i.valuer = (i.valuerId > 0) ? context.TBL_ACCREDITEDCONSULTANT.Where(b => b.ACCREDITEDCONSULTANTID == i.valuerId).Select(b => b.FIRMNAME).FirstOrDefault() : i.otherValuers;
                        i.collateralType = (i.collateralTypeId > 0) ? context.TBL_COLLATERAL_TYPE.Where(o => o.COLLATERALTYPEID == i.collateralTypeId).Select(o => o.COLLATERALTYPENAME).FirstOrDefault() : "";
                        i.collateralSubType = (i.collateralSubTypeId > 0) ? context.TBL_COLLATERAL_TYPE_SUB.Where(o => o.COLLATERALSUBTYPEID == i.collateralSubTypeId).Select(o => o.COLLATERALSUBTYPENAME).FirstOrDefault() : "";
                    }

            }
                return insurance.ToList();
        }

        public CasaLienViewModel GetAccountLienDetail(string AccountNumber)
        {
            return (context.TBL_CASA_LIEN.Where(x => x.PRODUCTACCOUNTNUMBER == AccountNumber)
                .Select(x => new CasaLienViewModel
                {
                    lienReferenceNumber = x.LIENREFERENCENUMBER,
                    productAccountNumber = x.PRODUCTACCOUNTNUMBER,
                    lienAmount = x.LIENAMOUNT,
                    dateTimeCreated = x.DATETIMECREATED,

                })).FirstOrDefault();
        }

        public IEnumerable<CollateralViewModel> GetCollateralStampToCoverValues(int customerId)
        {
            var collaterals = (from x in context.TBL_COLLATERAL_CUSTOMER
                               join c in context.TBL_COLLATERAL_TYPE on x.COLLATERALTYPEID equals c.COLLATERALTYPEID
                               join a in context.TBL_CUSTOMER on x.CUSTOMERID equals a.CUSTOMERID
                               let ColSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(c => c.COLLATERALSUBTYPEID == x.COLLATERALSUBTYPEID).Select(c => c.COLLATERALSUBTYPENAME).FirstOrDefault()
                               where x.CUSTOMERID == customerId && x.COLLATERALTYPEID == (int)CollateralTypeEnum.Property
                               orderby x.COLLATERALCUSTOMERID descending
                               select new CollateralViewModel
                               {
                                   collateralId = x.COLLATERALCUSTOMERID,
                                   collateralTypeId = x.COLLATERALTYPEID,
                                   collateralSubTypeId = x.COLLATERALSUBTYPEID,
                                   customerId = x.CUSTOMERID.Value,
                                   currencyId = x.CURRENCYID,
                                   currency = x.TBL_CURRENCY.CURRENCYNAME,
                                   collateralTypeName = x.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                                   collateralSubTypeName = ColSubType,
                                   collateralCode = x.COLLATERALCODE,
                                   collateralValue = x.COLLATERALVALUE,
                                   camRefNumber = x.CAMREFNUMBER,
                                   allowSharing = x.ALLOWSHARING,
                                   isLocationBased = (bool)x.ISLOCATIONBASED,
                                   valuationCycle = x.VALUATIONCYCLE,
                                   haircut = x.HAIRCUT,
                                   requireInsurancePolicy = c.REQUIREINSURANCEPOLICY,
                                   dateTimeCreated = x.DATETIMECREATED,
                                   requireVisitation = c.REQUIREVISITATION,
                                   customerName = a.FIRSTNAME + " " + a.LASTNAME + " " + a.MAIDENNAME,
                                   stampToCover = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(o => o.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).Select(o => o.STAMPTOCOVER).FirstOrDefault()
                               }).ToList();

            return collaterals;
        }


        public TDAccountRecordViewModel GetFixedDepositAccountDetail(string AccpuntNumber)
        {
            var finacleBalance = finacle.ValidateTDAccountNumber(AccpuntNumber);

            return finacleBalance;
        }


        #region
        private void UpdateISPOCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var entity = context.TBL_TEMP_COLLATERAL_ISPO.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (entity != null)
            {
                //get collateral detial from main table
                var collateral = (from x in context.TBL_COLLATERAL_ISPO
                                  join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                  where c.COLLATERALCODE == collateralcode
                                  select (x)).FirstOrDefault();

                if (collateral != null)
                {
                    collateral.COLLATERALCUSTOMERID = newCollateralId;
                    collateral.ACCOUNTNAMETODEBIT = entity.ACCOUNTNAMETODEBIT;
                    collateral.ACCOUNTNUMBERTODEBIT = entity.ACCOUNTNUMBERTODEBIT;
                    collateral.FREQUENCYTYPEID = (short)entity.FREQUENCYTYPEID;
                    collateral.SECURITYVALUE = entity.SECURITYVALUE;
                    collateral.REGULARPAYMENTAMOUNT = entity.REGULARPAYMENTAMOUNT;
                    collateral.PAYER = entity.PAYER;
                    collateral.REMARK = entity.REMARK;
                    collateral.DESCRIPTION = entity.DESCRIPTION;
                }
                else
                {
                    context.TBL_COLLATERAL_ISPO.Add(new TBL_COLLATERAL_ISPO
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        ACCOUNTNAMETODEBIT = entity.ACCOUNTNAMETODEBIT,
                        ACCOUNTNUMBERTODEBIT = entity.ACCOUNTNUMBERTODEBIT,
                        FREQUENCYTYPEID = (short)entity.FREQUENCYTYPEID,
                        SECURITYVALUE = entity.SECURITYVALUE,
                        REGULARPAYMENTAMOUNT = entity.REGULARPAYMENTAMOUNT,
                        PAYER = entity.PAYER,
                        REMARK = entity.REMARK,
                        DESCRIPTION = entity.DESCRIPTION
                    });
                }
            }

        }


        private void UpdateContractDomiciliationCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var entity = context.TBL_TEMP_COLLATERAL_DOMCLTN.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (entity != null)
            {
                //get collateral detial from main table
                var collateral = (from x in context.TBL_COLLATERAL_DOMICILIATION
                                  join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                  where c.COLLATERALCODE == collateralcode
                                  select (x)).FirstOrDefault();

                if (collateral != null)
                {
                    collateral.CONTRACTDETAILS = entity.CONTRACTDETAILS;
                    collateral.EMPLOYER = entity.EMPLOYER;
                    collateral.CONTRACTVALUE = entity.CONTRACTVALUE;
                    collateral.OUTSTANDINGINVOICEAMOUNT = entity.OUTSTANDINGINVOICEAMOUNT;
                    collateral.ACCOUNTNAMETODEBIT = entity.ACCOUNTNAMETODEBIT;
                    collateral.PAYER = entity.PAYER;
                    collateral.ACCOUNTNUMBERTODEBIT = entity.ACCOUNTNUMBERTODEBIT;
                    collateral.REGULARPAYMENTAMOUNT = entity.REGULARPAYMENTAMOUNT;
                    collateral.FREQUENCYTYPEID = (short)entity.FREQUENCYTYPEID;
                    collateral.INVOICENUMBER = entity.INVOICENUMBER;
                    collateral.SECURITYVALUE = entity.SECURITYVALUE;
                    collateral.INVOICEDATE = entity.INVOICEDATE;
                    collateral.REMARK = entity.REMARK;
                    collateral.DESCRIPTION = entity.DESCRIPTION;

                }
                else
                {
                    context.TBL_COLLATERAL_DOMICILIATION.Add(new TBL_COLLATERAL_DOMICILIATION
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        CONTRACTDETAILS = entity.CONTRACTDETAILS,
                        EMPLOYER = entity.EMPLOYER,
                        CONTRACTVALUE = entity.CONTRACTVALUE,
                        OUTSTANDINGINVOICEAMOUNT = entity.OUTSTANDINGINVOICEAMOUNT,
                        ACCOUNTNAMETODEBIT = entity.ACCOUNTNAMETODEBIT,
                        PAYER = entity.PAYER,
                        ACCOUNTNUMBERTODEBIT = entity.ACCOUNTNUMBERTODEBIT,
                        REGULARPAYMENTAMOUNT = entity.REGULARPAYMENTAMOUNT,
                        FREQUENCYTYPEID = (short)entity.FREQUENCYTYPEID,
                        INVOICENUMBER = entity.INVOICENUMBER,
                        SECURITYVALUE = entity.SECURITYVALUE,
                        INVOICEDATE = entity.INVOICEDATE,
                        REMARK = entity.REMARK,
                        DESCRIPTION = entity.DESCRIPTION,

                    });
                }
            }

        }

        private void UpdateSalaryDomiciliationCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var entity = context.TBL_TEMP_COLLATERAL_DOMCLTN.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (entity != null)
            {
                //get collateral detial from main table
                var collateral = (from x in context.TBL_COLLATERAL_DOMICILIATION
                                  join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                  where c.COLLATERALCODE == collateralcode
                                  select (x)).FirstOrDefault();

                if (collateral != null)
                {
                    collateral.CONTRACTDETAILS = entity.CONTRACTDETAILS;
                    collateral.EMPLOYER = entity.EMPLOYER;
                    collateral.MONTHLYSALARY = entity.MONTHLYSALARY;
                    collateral.ANNUALALLOWANCES = entity.ANNUALALLOWANCES;
                    collateral.ANNUALEMOLUMENT = entity.ANNUALEMOLUMENT;
                    collateral.ACCOUNTNUMBER = entity.ACCOUNTNUMBER;
                    collateral.ANNUALSALARY = entity.ANNUALSALARY;
                    collateral.SECURITYVALUE = entity.SECURITYVALUE;
                    collateral.REMARK = entity.REMARK;
                    collateral.DESCRIPTION = entity.DESCRIPTION;

                }
                else
                {
                    context.TBL_COLLATERAL_DOMICILIATION.Add(new TBL_COLLATERAL_DOMICILIATION
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        CONTRACTDETAILS = entity.CONTRACTDETAILS,
                        EMPLOYER = entity.EMPLOYER,
                        MONTHLYSALARY = entity.MONTHLYSALARY,
                        ANNUALALLOWANCES = entity.ANNUALALLOWANCES,
                        ANNUALEMOLUMENT = entity.ANNUALEMOLUMENT,
                        ACCOUNTNUMBER = entity.ACCOUNTNUMBER,
                        ANNUALSALARY = entity.ANNUALSALARY,
                        SECURITYVALUE = entity.SECURITYVALUE,
                        REMARK = entity.REMARK,
                        DESCRIPTION = entity.DESCRIPTION

                    });
                }
            }

        }
        private void UpdateIndemityCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var entity = context.TBL_TEMP_COLLATERAL_INDEMNITY.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (entity != null)
            {
                //get collateral detial from main table
                var collateral = (from x in context.TBL_COLLATERAL_INDEMNITY
                                  join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                  where c.COLLATERALCODE == collateralcode
                                  select (x)).FirstOrDefault();

                if (collateral != null)
                {
                    collateral.SECURITYVALUE = entity.SECURITYVALUE;
                    collateral.REMARK = entity.REMARK;
                    collateral.ADDRESS = entity.ADDRESS;
                    collateral.BVN = entity.BVN;
                    collateral.EMAILADRRESS = entity.EMAILADRRESS;
                    collateral.ENDDATE = (DateTime)entity.ENDDATE;
                    collateral.STARTDATE = (DateTime)entity.STARTDATE;
                    collateral.FIRSTNAME = entity.FIRSTNAME;
                    collateral.MIDDLENAME = entity.MIDDLENAME;
                    collateral.LASTNAME = entity.LASTNAME;
                    collateral.PHONENUMBER1 = entity.PHONENUMBER1;
                    collateral.PHONENUMBER2 = entity.PHONENUMBER2;
                    collateral.RELATIONSHIPDURATION = entity.RELATIONSHIPDURATION;
                    collateral.RELATIONSHIP = entity.RELATIONSHIP;
                    collateral.TAXNUMBER = entity.TAXNUMBER;
                    collateral.REMARK = entity.REMARK;
                    collateral.DESCRIPTION = entity.DESCRIPTION;

                }
                else
                {
                    context.TBL_COLLATERAL_INDEMNITY.Add(new TBL_COLLATERAL_INDEMNITY
                    {
                        SECURITYVALUE = entity.SECURITYVALUE,
                        REMARK = entity.REMARK,
                        ADDRESS = entity.ADDRESS,
                        BVN = entity.BVN,
                        EMAILADRRESS = entity.EMAILADRRESS,
                        ENDDATE = (DateTime)entity.ENDDATE,
                        STARTDATE = (DateTime)entity.STARTDATE,
                        FIRSTNAME = entity.FIRSTNAME,
                        MIDDLENAME = entity.MIDDLENAME,
                        LASTNAME = entity.LASTNAME,
                        PHONENUMBER1 = entity.PHONENUMBER1,
                        PHONENUMBER2 = entity.PHONENUMBER2,
                        RELATIONSHIPDURATION = entity.RELATIONSHIPDURATION,
                        RELATIONSHIP = entity.RELATIONSHIP,
                        TAXNUMBER = entity.TAXNUMBER,
                        DESCRIPTION = entity.DESCRIPTION

                    });
                }
            }

        }

        private void UpdateIndemityDomiciliationCollateral(int tempCollateralId, string collateralcode, int newCollateralId)
        {
            //get all collateral details from temp
            var entity = context.TBL_TEMP_COLLATERAL_INDEMNITY.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();
            if (entity != null)
            {
                //get collateral detial from main table
                var collateral = (from x in context.TBL_COLLATERAL_INDEMNITY
                                  join c in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals c.COLLATERALCUSTOMERID
                                  where c.COLLATERALCODE == collateralcode
                                  select (x)).FirstOrDefault();

                if (collateral != null)
                {
                    collateral.SECURITYVALUE = entity.SECURITYVALUE;
                    collateral.REMARK = entity.REMARK;
                    collateral.ADDRESS = entity.ADDRESS;
                    collateral.BVN = entity.BVN;
                    collateral.EMAILADRRESS = entity.EMAILADRRESS;
                    collateral.ENDDATE = (DateTime)entity.ENDDATE;
                    collateral.STARTDATE = (DateTime)entity.STARTDATE;
                    collateral.FIRSTNAME = entity.FIRSTNAME;
                    collateral.MIDDLENAME = entity.MIDDLENAME;
                    collateral.LASTNAME = entity.LASTNAME;
                    collateral.PHONENUMBER1 = entity.PHONENUMBER1;
                    collateral.PHONENUMBER2 = entity.PHONENUMBER2;
                    collateral.RELATIONSHIPDURATION = entity.RELATIONSHIPDURATION;
                    collateral.RELATIONSHIP = entity.RELATIONSHIP;
                    collateral.TAXNUMBER = entity.TAXNUMBER;
                    collateral.DESCRIPTION = entity.DESCRIPTION;

                }
                else
                {
                    context.TBL_COLLATERAL_INDEMNITY.Add(new TBL_COLLATERAL_INDEMNITY
                    {
                        COLLATERALCUSTOMERID = newCollateralId,
                        SECURITYVALUE = entity.SECURITYVALUE,
                        ADDRESS = entity.ADDRESS,
                        BVN = entity.BVN,
                        EMAILADRRESS = entity.EMAILADRRESS,
                        ENDDATE = (DateTime)entity.ENDDATE,
                        STARTDATE = (DateTime)entity.STARTDATE,
                        FIRSTNAME = entity.FIRSTNAME,
                        MIDDLENAME = entity.MIDDLENAME,
                        LASTNAME = entity.LASTNAME,
                        PHONENUMBER1 = entity.PHONENUMBER1,
                        PHONENUMBER2 = entity.PHONENUMBER2,
                        RELATIONSHIPDURATION = entity.RELATIONSHIPDURATION,
                        RELATIONSHIP = entity.RELATIONSHIP,
                        TAXNUMBER = entity.TAXNUMBER,
                        REMARK = entity.REMARK,
                        DESCRIPTION = entity.DESCRIPTION
                    });
                }
            }

        }
        #endregion

        public bool RejectProposedCollateralForUsage(int collateralCustomerId)
        {
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralCustomerId).Select(x => x).FirstOrDefault();
            if (collateral != null)
                collateral.COLLATERALUSAGESTATUSID = (int)CollateralUsageStatusEnum.Rejected;

            return context.SaveChanges() > 0;

        }
        public bool ProposeCollateralForUsage(CollateralCoverageViewModel model)
        {
            decimal facilitiesValue = 0m;
            decimal availableCollateralValues = 0m;
            var data = new TBL_LOAN_APPLICATION_COLLATERL();
            var sdApplicable = false;
            //var proposedCollateral = context.TBL_LOAN_APPLICATION_COLLATERL.Any(o => o.COLLATERALCUSTOMERID == model.collateralId
            //                                && o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId && o.DELETED == false);

            //if (proposedCollateral == true) throw new Exception("This Collateral has already been proposed for this facility");

            var facility = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId).Select(o => o).FirstOrDefault();

            var obligorId = facility.CUSTOMERID;


            var facilityCurrencyId = facility.CURRENCYID;

            var collateral = context.TBL_COLLATERAL_CUSTOMER.Where(o => o.COLLATERALCUSTOMERID == model.collateralId).Select(o => o).FirstOrDefault();

            var collateralCoverage = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID && o.DELETED == false && o.CURRENCYID == facilityCurrencyId).Select(o => o.COVERAGE).FirstOrDefault();

            decimal coverage = decimal.Divide(collateralCoverage, 100);

            var newCollateral = (!context.TBL_LOAN_APPLICATION_COLLATERL.Any(o => o.COLLATERALCUSTOMERID == model.collateralId && o.DELETED == false));
            var newFacility = (!context.TBL_LOAN_APPLICATION_COLLATERL.Any(o => o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId && o.DELETED == false));
            if (newCollateral == true && newFacility == true)
            {

                data = new TBL_LOAN_APPLICATION_COLLATERL
                {
                    COLLATERALCUSTOMERID = model.collateralId,
                    LOANAPPLICATIONID = model.loanApplicationId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                    LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                    COLLATERALCOVERAGE = model.actualCollateralCoverage,
                    BALANCEAVAILABLE = model.availableCollateralValue - model.actualCollateralCoverage,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    CUSTOMERID = obligorId,
                    DELETED = false,
                    SYSTEMDATETIME = DateTime.Now,

                };
                context.TBL_LOAN_APPLICATION_COLLATERL.Add(data);

                if (context.SaveChanges() > 0)
                {
                    var condition = context.TBL_STAMP_DUTY_CONDITION.Where(f => f.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID).FirstOrDefault();
                    if (condition != null) sdApplicable = ValidateStampDutyApplicable(facility, condition);
                }
                if (sdApplicable == true)
                {
                    facility.STAMPDUTYAPPLICABLE = true;
                    var fsdExists = context.TBL_FACILITY_STAMP_DUTY.Where(f => f.COLLATERALCUSTOMERID == data.COLLATERALCUSTOMERID && f.LOANAPPLICATIONDETAILID == data.LOANAPPLICATIONDETAILID && f.DELETED == true).FirstOrDefault();
                    var fsdDetailExists = context.TBL_FACILITY_STAMP_DUTY.Where(f => f.LOANAPPLICATIONDETAILID == data.LOANAPPLICATIONDETAILID && f.DELETED == true).FirstOrDefault();
                    if (fsdExists != null)
                    {
                        var appl = context.TBL_LOAN_APPLICATION.Find(facility.LOANAPPLICATIONID);
                        if (!appl.ISONLENDING)
                        {
                            fsdExists.DELETED = false;

                            var cond = context.TBL_STAMP_DUTY_CONDITION.Where(f => f.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID).FirstOrDefault();
                            var stampFee = context.TBL_CHARGE_FEE.Where(s => s.CHARGEFEENAME.ToLower().Contains("(fixed)") && s.DELETED == false).ToList();
                            if (stampFee != null)
                            {
                                List<ProductFeesViewModel> fees = new List<ProductFeesViewModel>();
                                foreach (var f in stampFee)
                                {
                                    var feeDetails = context.TBL_CHARGE_FEE_DETAIL.Where(fd => fd.CHARGEFEEID == f.CHARGEFEEID).FirstOrDefault();
                                    var fee = new ProductFeesViewModel()
                                    {
                                        loanChargeFeeId = f.CHARGEFEEID,
                                        rate = cond.DUTIABLEVALUE,
                                        createdBy = model.createdBy,
                                        loanApplicationDetailId = facility.LOANAPPLICATIONDETAILID,

                                    };

                                    fees.Add(fee);

                                }


                                ProductFees(fees, facility.LOANAPPLICATIONDETAILID, model.createdBy);
                            }

                        }

                    }
                    else if (fsdDetailExists != null)
                    {
                        var appl = context.TBL_LOAN_APPLICATION.Find(facility.LOANAPPLICATIONID);
                        if (!appl.ISONLENDING)
                        {
                            fsdDetailExists.DELETED = false;

                            var cond = context.TBL_STAMP_DUTY_CONDITION.Where(f => f.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID).FirstOrDefault();
                            var stampFee = context.TBL_CHARGE_FEE.Where(s => s.CHARGEFEENAME.ToLower().Contains("(fixed)") && s.DELETED == false).ToList();
                            if (stampFee != null)
                            {
                                List<ProductFeesViewModel> fees = new List<ProductFeesViewModel>();
                                foreach (var f in stampFee)
                                {
                                    var feeDetails = context.TBL_CHARGE_FEE_DETAIL.Where(fd => fd.CHARGEFEEID == f.CHARGEFEEID).FirstOrDefault();
                                    var fee = new ProductFeesViewModel()
                                    {
                                        loanChargeFeeId = f.CHARGEFEEID,
                                        rate = cond.DUTIABLEVALUE,
                                        createdBy = model.createdBy,
                                        loanApplicationDetailId = facility.LOANAPPLICATIONDETAILID,

                                    };

                                    fees.Add(fee);

                                }


                                ProductFees(fees, facility.LOANAPPLICATIONDETAILID, model.createdBy);
                            }

                        }
                    }
                    else
                    {
                        var sdoCode = GenerateSDCode();
                        sdoCode = "SDO" + sdoCode;

                        var facilityStampDuty = new TBL_FACILITY_STAMP_DUTY
                        {
                            LOANAPPLICATIONDETAILID = facility.LOANAPPLICATIONDETAILID,
                            COLLATERALCUSTOMERID = collateral.COLLATERALCUSTOMERID,
                            CURRENTSTATUS = 1,
                            OSDC = sdoCode,
                            DATETIMECREATED = DateTime.Now,
                            DATETIMEUPDATED = DateTime.Now,
                            ISSHARED = false,
                            CUSTOMERPERCENTAGE = 100,
                            BANKPERCENTAGE = 0
                        };
                        context.TBL_FACILITY_STAMP_DUTY.Add(facilityStampDuty);

                        var cond = context.TBL_STAMP_DUTY_CONDITION.Where(f => f.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID).FirstOrDefault();
                        var stampFee = context.TBL_CHARGE_FEE.Where(s => s.CHARGEFEENAME.ToLower().Contains("(fixed)") && s.DELETED == false).ToList();
                        if (stampFee != null)
                        {
                            List<ProductFeesViewModel> fees = new List<ProductFeesViewModel>();
                            foreach (var f in stampFee)
                            {
                                var feeDetails = context.TBL_CHARGE_FEE_DETAIL.Where(fd => fd.CHARGEFEEID == f.CHARGEFEEID).FirstOrDefault();
                                var fee = new ProductFeesViewModel()
                                {
                                    loanChargeFeeId = f.CHARGEFEEID,
                                    rate = cond.DUTIABLEVALUE,
                                    createdBy = model.createdBy,
                                    loanApplicationDetailId = facility.LOANAPPLICATIONDETAILID,

                                };

                                fees.Add(fee);

                            }


                            ProductFees(fees, facility.LOANAPPLICATIONDETAILID, model.createdBy);
                        }

                    }




                    context.SaveChanges();
                }
                context.SaveChanges();

                return true;
            }
            else
            {
                //o.LOANAPPLICATIONDETAILID != model.loanApplicationDetailId
                var collateralMappedToFacilities = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == model.collateralId && o.DELETED == false).Select(o => o).ToList();
                // o.COLLATERALCUSTOMERID != model.collateralId
                var facilityMappedToCollaterals = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId && o.DELETED == false).Select(o => o).ToList();
                availableCollateralValues = collateral.COLLATERALVALUE;
                //facilitiesValue = decimal.Multiply((facilitiesValue + facilityValue.APPROVEDAMOUNT), coverage);

                if (facilityMappedToCollaterals.Count != 0)
                {
                    //if (facilityMappedToCollaterals.Sum(f => f.COLLATERALCOVERAGE) >= decimal.Multiply(facilityValue.APPROVEDAMOUNT, coverage))
                    //{
                    //    throw new Exception("Facility is already fully covered");
                    //}

                    //foreach (var x in facilityMappedToCollaterals)
                    //{
                    //    availableCollateralValues = availableCollateralValues + context.TBL_COLLATERAL_CUSTOMER.Where(o => o.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).Select(o => o.COLLATERALVALUE).FirstOrDefault();
                    //}
                }

                if (collateralMappedToFacilities.Count != 0)
                {
                    availableCollateralValues = availableCollateralValues - collateralMappedToFacilities.Sum(c => c.COLLATERALCOVERAGE);
                    //foreach (var x in collateralMappedToFacilities)
                    //{
                    //    facilitiesValue = facilitiesValue + context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => o.APPROVEDAMOUNT).FirstOrDefault();
                    //}
                }

                //availableCollateralValues = availableCollateralValues + collateral.COLLATERALVALUE;

                //availableCollateralValues = decimal.Subtract(availableCollateralValues, facilitiesValue);


                if (availableCollateralValues > 0)
                {
                    data = new TBL_LOAN_APPLICATION_COLLATERL
                    {
                        COLLATERALCUSTOMERID = model.collateralId,
                        LOANAPPLICATIONID = model.loanApplicationId,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                        LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                        COLLATERALCOVERAGE = model.actualCollateralCoverage,
                        BALANCEAVAILABLE = model.availableCollateralValue - model.actualCollateralCoverage,
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = genSetup.GetApplicationDate(),
                        CUSTOMERID = obligorId,
                        DELETED = false,

                    };
                    context.TBL_LOAN_APPLICATION_COLLATERL.Add(data);

                    try
                    {
                        if (context.SaveChanges() > 0)
                        {
                            var condition = context.TBL_STAMP_DUTY_CONDITION.Where(f => f.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID).FirstOrDefault();
                            if (condition != null) sdApplicable = ValidateStampDutyApplicable(facility, condition);
                        }
                        if (sdApplicable)
                        {
                            facility.STAMPDUTYAPPLICABLE = true;
                            var fsdDetailExists = context.TBL_FACILITY_STAMP_DUTY.Where(f => f.LOANAPPLICATIONDETAILID == data.LOANAPPLICATIONDETAILID).FirstOrDefault();
                            var fsdExists = context.TBL_FACILITY_STAMP_DUTY.Where(f => f.COLLATERALCUSTOMERID == data.COLLATERALCUSTOMERID && f.LOANAPPLICATIONDETAILID == data.LOANAPPLICATIONDETAILID).FirstOrDefault();
                            if (fsdExists != null)
                            {
                                var appl = context.TBL_LOAN_APPLICATION.Find(facility.LOANAPPLICATIONID);
                                if (!appl.ISONLENDING)
                                {
                                    fsdExists.DELETED = false;
                                    var cond = context.TBL_STAMP_DUTY_CONDITION.Where(f => f.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID).FirstOrDefault();
                                    var stampFee = context.TBL_CHARGE_FEE.Where(s => s.CHARGEFEENAME.ToLower().Contains("(fixed)") && s.DELETED == false).ToList();
                                    if (stampFee != null)
                                    {
                                        List<ProductFeesViewModel> fees = new List<ProductFeesViewModel>();
                                        foreach (var f in stampFee)
                                        {
                                            var feeDetails = context.TBL_CHARGE_FEE_DETAIL.Where(fd => fd.CHARGEFEEID == f.CHARGEFEEID).FirstOrDefault();
                                            var fee = new ProductFeesViewModel()
                                            {
                                                loanChargeFeeId = f.CHARGEFEEID,
                                                rate = cond.DUTIABLEVALUE,
                                                createdBy = model.createdBy,
                                                loanApplicationDetailId = facility.LOANAPPLICATIONDETAILID,

                                            };

                                            fees.Add(fee);

                                        }


                                        ProductFees(fees, facility.LOANAPPLICATIONDETAILID, model.createdBy);
                                    }
                                }
                            }
                            else if (fsdDetailExists != null)
                            {
                                var appl = context.TBL_LOAN_APPLICATION.Find(facility.LOANAPPLICATIONID);
                                if (!appl.ISONLENDING)
                                {

                                }
                            }
                            else
                            {
                                var sdoCode = GenerateSDCode();
                                sdoCode = "SDO" + sdoCode;

                                var facilityStampDuty = new TBL_FACILITY_STAMP_DUTY
                                {
                                    LOANAPPLICATIONDETAILID = facility.LOANAPPLICATIONDETAILID,
                                    COLLATERALCUSTOMERID = collateral.COLLATERALCUSTOMERID,
                                    CURRENTSTATUS = 1,
                                    OSDC = sdoCode,
                                    DATETIMECREATED = DateTime.Now,
                                    DATETIMEUPDATED = DateTime.Now,
                                    ISSHARED = false,
                                    CUSTOMERPERCENTAGE = 100,
                                    BANKPERCENTAGE = 0
                                };
                                context.TBL_FACILITY_STAMP_DUTY.Add(facilityStampDuty);

                                var cond = context.TBL_STAMP_DUTY_CONDITION.Where(f => f.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID).FirstOrDefault();
                                var stampFee = context.TBL_CHARGE_FEE.Where(s => s.CHARGEFEENAME.ToLower().Contains("(fixed)") && s.DELETED == false).ToList();
                                if (stampFee != null)
                                {
                                    foreach (var fe in stampFee)
                                    {
                                        List<ProductFeesViewModel> fees = new List<ProductFeesViewModel>();

                                        var fee = new ProductFeesViewModel()
                                        {
                                            loanChargeFeeId = stampFee[0].CHARGEFEEID,
                                            rate = cond.DUTIABLEVALUE,
                                            createdBy = model.createdBy,
                                            loanApplicationDetailId = facility.LOANAPPLICATIONDETAILID,

                                        };

                                        fees.Add(fee);
                                        ProductFees(fees, facility.LOANAPPLICATIONDETAILID, model.createdBy);
                                    }



                                }

                            }



                            context.SaveChanges();
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                    context.SaveChanges();
                         return true;
                }
                else
                    throw new Exception("It's fully in use");
            }
        }

        private string GenerateSDCode()
        {
            var fsd = context.TBL_CODE_TRACKER.OrderByDescending(x => x.CODEID).FirstOrDefault();

            DateTime lastGeneratedDate = fsd.CURRENTDATE;
            int lastGeneratedNumber = 0;
            if (fsd != null) lastGeneratedNumber = fsd.OSDC;

       
            DateTime currentDate = DateTime.Now;

            // Check if it's a new year
            if (currentDate.Year > lastGeneratedDate.Year)
            {
                // Reset the number to 1 for the new year
                lastGeneratedNumber = 0;
            }

            // Increment the number
            lastGeneratedNumber++;

            // Format the serial number
            string serialNumber = $"{currentDate.Year}/{currentDate.Month:D2}/{currentDate.Day:D2}/{lastGeneratedNumber:D4}";

            // Update the last generated date
            lastGeneratedDate = currentDate;
            fsd.OSDC = lastGeneratedNumber;
            fsd.CURRENTDATE = lastGeneratedDate;
            context.SaveChanges();

            return serialNumber;


        }

        private void ProductFees(List<ProductFeesViewModel> fees, int loanApplicationDetailId, int createdBy)
        {
            var data = fees.Select(c => new TBL_LOAN_APPLICATION_DETL_FEE()
            {
                CHARGEFEEID = c.loanChargeFeeId,
                RECOMMENDED_FEERATEVALUE = c.rate,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = createdBy,
                HASCONSESSION = false,
                APPROVALSTATUSID = (short)ApprovalStatusEnum.Approved,
                LOANAPPLICATIONDETAILID = loanApplicationDetailId,
                DEFAULT_FEERATEVALUE = c.rate
            });

            context.TBL_LOAN_APPLICATION_DETL_FEE.AddRange(data);

        }

        private bool ValidateStampDutyApplicable(TBL_LOAN_APPLICATION_DETAIL loan, TBL_STAMP_DUTY_CONDITION condition)
        {
            var loanApplication = context.TBL_LOAN_APPLICATION.Find(loan.LOANAPPLICATIONID);
            if (loanApplication.ISONLENDING) return false;
            var tenorDutiable = ValidateTenorCondition(loan, condition);
            var collateralDutiable = ValidateCollateralCondition(loan);
            
            if (collateralDutiable && tenorDutiable) return true;
            return false;

        }

        private bool ValidateCollateralCondition(TBL_LOAN_APPLICATION_DETAIL loan)
        {
            var collateralSubtypeIds = new List<int>();
            var collateralCondition = new List<TBL_STAMP_DUTY_CONDITION>();
            var collateralConditionIds = context.TBL_STAMP_DUTY_CONDITION.Select(c => c.COLLATERALSUBTYPEID).ToList();
            var proposedCollateralIds = context.TBL_LOAN_APPLICATION_COLLATERL.Where(c => c.LOANAPPLICATIONDETAILID == loan.LOANAPPLICATIONDETAILID && c.DELETED == false).Select(c => c.COLLATERALCUSTOMERID).ToList();
            if (proposedCollateralIds.Any())
            {
                foreach (var collateralId in proposedCollateralIds)
                {
                    var subtypeId = context.TBL_COLLATERAL_CUSTOMER.Where(col => col.COLLATERALCUSTOMERID == collateralId && col.DELETED == false).FirstOrDefault().COLLATERALSUBTYPEID;
                    collateralSubtypeIds.Add(subtypeId);
                }
                if (collateralSubtypeIds.Count() > 0)
                {
                    foreach(var subId in collateralSubtypeIds)
                    {
                        var conditn = context.TBL_STAMP_DUTY_CONDITION.Where(c => c.COLLATERALSUBTYPEID ==  subId).FirstOrDefault();
                        collateralCondition.Add(conditn);
                    }
                }
                foreach(var subtyp in collateralSubtypeIds)
                {
                    if (collateralConditionIds.Contains(subtyp)) return true;
                }
                              
            }

            return false;
        }

        private bool ValidateTenorCondition(TBL_LOAN_APPLICATION_DETAIL loan, TBL_STAMP_DUTY_CONDITION condition)
        {
            if (condition.USETENOR == false) return true;
            if( condition.USETENOR == true)
            {
                int tenor = loan.PROPOSEDTENOR;
                if (tenor >= condition.TENOR) return true;
            }
            
            return false;
        }

        private int ConvertTenorToDays(int proposedTenor, int? tenorModeId = 1)
        {
            int tenor = 0;
            switch (tenorModeId) // UPDATED
            {
                case (int)TenorMode.Daily: tenor = proposedTenor; break;
                case (int)TenorMode.Monthly: tenor = proposedTenor * 30; break;
                case (int)TenorMode.Yearly: tenor = proposedTenor * 365; break;
            }
            return tenor;
        }

        public bool ProposeCollateralForUsageLMS(CollateralCoverageViewModel model)
        {

            decimal facilitiesValue = 0m;
            decimal availableCollateralValues = 0m;
            var data = new TBL_LOAN_APPLICATION_COLLATERL();
            var facility = context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId).Select(o => o).FirstOrDefault();
            var obligorId = facility.CUSTOMERID;
            var facilityCurrencyId = facility.CURRENCYID;
            var collateral = context.TBL_COLLATERAL_CUSTOMER.Where(o => o.COLLATERALCUSTOMERID == model.collateralId).Select(o => o).FirstOrDefault();
            var collateralCoverage = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID && o.DELETED == false && o.CURRENCYID == facilityCurrencyId).Select(o => o.COVERAGE).FirstOrDefault();
            decimal coverage = decimal.Divide(collateralCoverage, 100);

            var newCollateral = (!context.TBL_LOAN_APPLICATION_COLLATERL.Any(o => o.COLLATERALCUSTOMERID == model.collateralId && o.DELETED == false));
            var newFacility = (!context.TBL_LOAN_APPLICATION_COLLATERL.Any(o => o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId && o.DELETED == false));
            if (newCollateral == true && newFacility == true)
            {

                data = new TBL_LOAN_APPLICATION_COLLATERL
                {
                    COLLATERALCUSTOMERID = model.collateralId,
                    LOANAPPLICATIONID = model.loanApplicationId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                    LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                    COLLATERALCOVERAGE = model.actualCollateralCoverage,
                    BALANCEAVAILABLE = model.availableCollateralValue - model.actualCollateralCoverage,
                    CREATEDBY = (int)model.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    CUSTOMERID = obligorId,
                    DELETED = false,
                    SYSTEMDATETIME = DateTime.Now,

                };
                context.TBL_LOAN_APPLICATION_COLLATERL.Add(data);

                if (context.SaveChanges() > 0)
                    return true;
            }
            else
            {
                //o.LOANAPPLICATIONDETAILID != model.loanApplicationDetailId
                var collateralMappedToFacilities = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == model.collateralId && o.DELETED == false).Select(o => o).ToList();
                // o.COLLATERALCUSTOMERID != model.collateralId
                var facilityMappedToCollaterals = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId && o.DELETED == false).Select(o => o).ToList();
                availableCollateralValues = collateral.COLLATERALVALUE;
                //facilitiesValue = decimal.Multiply((facilitiesValue + facilityValue.APPROVEDAMOUNT), coverage);

                if (facilityMappedToCollaterals.Count != 0)
                {
                    //if (facilityMappedToCollaterals.Sum(f => f.COLLATERALCOVERAGE) >= decimal.Multiply(facilityValue.APPROVEDAMOUNT, coverage))
                    //{
                    //    throw new Exception("Facility is already fully covered");
                    //}

                    //foreach (var x in facilityMappedToCollaterals)
                    //{
                    //    availableCollateralValues = availableCollateralValues + context.TBL_COLLATERAL_CUSTOMER.Where(o => o.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).Select(o => o.COLLATERALVALUE).FirstOrDefault();
                    //}
                }

                if (collateralMappedToFacilities.Count != 0)
                {
                    availableCollateralValues = availableCollateralValues - collateralMappedToFacilities.Sum(c => c.COLLATERALCOVERAGE);
                    //foreach (var x in collateralMappedToFacilities)
                    //{
                    //    facilitiesValue = facilitiesValue + context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => o.APPROVEDAMOUNT).FirstOrDefault();
                    //}
                }

                //availableCollateralValues = availableCollateralValues + collateral.COLLATERALVALUE;

                //availableCollateralValues = decimal.Subtract(availableCollateralValues, facilitiesValue);


                if (availableCollateralValues > 0)
                {
                    data = new TBL_LOAN_APPLICATION_COLLATERL
                    {
                        COLLATERALCUSTOMERID = model.collateralId,
                        LOANAPPLICATIONID = model.loanApplicationId,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                        LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                        COLLATERALCOVERAGE = model.actualCollateralCoverage,
                        BALANCEAVAILABLE = model.availableCollateralValue - model.actualCollateralCoverage,
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = genSetup.GetApplicationDate(),
                        CUSTOMERID = obligorId,
                        DELETED = false,

                    };
                    context.TBL_LOAN_APPLICATION_COLLATERL.Add(data);

                    try
                    {
                        if (context.SaveChanges() > 0)
                            return true;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
                else
                    throw new Exception("It's fully in use");
            }
            return false;
            /* decimal facilitiesValue = 0m;
            decimal availableCollateralValues = 0m;
            var data = new TBL_LOAN_APPLICATION_COLLATERL();
            var loanDetail = context.TBL_LOAN_APPLICATION_DETAIL.Find(model.loanApplicationDetailId);
            var facility = context.TBL_LMSR_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == loanDetail.LOANAPPLICATIONID).Select(o => o).FirstOrDefault();

            var obligorId = facility.CUSTOMERID;


            var facilityCurrencyId = facility.CURRENCYID;

            var collateral = context.TBL_COLLATERAL_CUSTOMER.Where(o => o.COLLATERALCUSTOMERID == model.collateralId).Select(o => o).FirstOrDefault();

            var collateralCoverage = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == collateral.COLLATERALSUBTYPEID && o.DELETED == false && o.CURRENCYID == facilityCurrencyId).Select(o => o.COVERAGE).FirstOrDefault();

            decimal coverage = decimal.Divide(collateralCoverage, 100);

            var newCollateral = (!context.TBL_LOAN_APPLICATION_COLLATERL.Any(o => o.COLLATERALCUSTOMERID == model.collateralId && o.DELETED == false));
            var newFacility = (!context.TBL_LOAN_APPLICATION_COLLATERL.Any(o => o.LOANAPPLICATIONDETAILID == facility.LOANREVIEWAPPLICATIONID && o.DELETED == false));
            if (newCollateral == true && newFacility == true)
            {

                data = new TBL_LOAN_APPLICATION_COLLATERL
                {
                    COLLATERALCUSTOMERID = model.collateralId,
                    LOANAPPLICATIONID = model.loanApplicationId,
                    APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                    LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                    COLLATERALCOVERAGE = model.actualCollateralCoverage,
                    BALANCEAVAILABLE = model.availableCollateralValue - model.actualCollateralCoverage,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = genSetup.GetApplicationDate(),
                    CUSTOMERID = obligorId,
                    DELETED = false,
                    SYSTEMDATETIME = DateTime.Now,

                };
                context.TBL_LOAN_APPLICATION_COLLATERL.Add(data);

                if (context.SaveChanges() > 0)
                    return true;
            }
            else
            {

                var collateralMappedToFacilities = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.COLLATERALCUSTOMERID == model.collateralId && o.DELETED == false).Select(o => o).ToList();

                var facilityMappedToCollaterals = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId && o.DELETED == false).Select(o => o).ToList();
                availableCollateralValues = collateral.COLLATERALVALUE;


                if (facilityMappedToCollaterals.Count != 0)
                {
                    //if (facilityMappedToCollaterals.Sum(f => f.COLLATERALCOVERAGE) >= decimal.Multiply(facilityValue.APPROVEDAMOUNT, coverage))
                    //{
                    //    throw new Exception("Facility is already fully covered");
                    //}

                    //foreach (var x in facilityMappedToCollaterals)
                    //{
                    //    availableCollateralValues = availableCollateralValues + context.TBL_COLLATERAL_CUSTOMER.Where(o => o.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID).Select(o => o.COLLATERALVALUE).FirstOrDefault();
                    //}
                }

                if (collateralMappedToFacilities.Count != 0)
                {
                    availableCollateralValues = availableCollateralValues - collateralMappedToFacilities.Sum(c => c.COLLATERALCOVERAGE);
                    //foreach (var x in collateralMappedToFacilities)
                    //{
                    //    facilitiesValue = facilitiesValue + context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == x.LOANAPPLICATIONID).Select(o => o.APPROVEDAMOUNT).FirstOrDefault();
                    //}
                }

                //availableCollateralValues = availableCollateralValues + collateral.COLLATERALVALUE;

                //availableCollateralValues = decimal.Subtract(availableCollateralValues, facilitiesValue);


                if (availableCollateralValues > 0)
                {
                    data = new TBL_LOAN_APPLICATION_COLLATERL
                    {
                        COLLATERALCUSTOMERID = model.collateralId,
                        LOANAPPLICATIONID = model.loanApplicationId,
                        APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing,
                        LOANAPPLICATIONDETAILID = model.loanApplicationDetailId,
                        COLLATERALCOVERAGE = model.actualCollateralCoverage,
                        BALANCEAVAILABLE = model.availableCollateralValue - model.actualCollateralCoverage,
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = genSetup.GetApplicationDate(),
                        SYSTEMDATETIME = genSetup.GetApplicationDate(),
                        CUSTOMERID = obligorId,
                        DELETED = false,

                    };
                    context.TBL_LOAN_APPLICATION_COLLATERL.Add(data);

                    try
                    {
                        if (context.SaveChanges() > 0)
                            return true;
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }

                }
                else
                    throw new Exception("It's fully in use");
            }
            return false;*/
        }

        public IEnumerable<CollateralUsageStatus> GetCollateralUsageStatus()
        {
            var m = new List<CollateralUsageStatus> { new CollateralUsageStatus { } };
            return m;
            //return (from x in context.TBL_COLLATERAL_USAGE_STATUS
            //       select new CollateralUsageStatus{
            //    collateralStatusId = x.COLLATERALUSAGESTATUSID,
            //    collateralStatusName =x.USAGESTATUSNAME,

            //}).ToList();
        }

        public IEnumerable<InsurancePolicy> GetInsuranceType()
        {
            return context.TBL_INSURANCE_TYPE.Select(x => new InsurancePolicy { insuranceTypeId = x.INSURANCETYPEID, insuranceType = x.INSURANCETYPE });
        }


        public IEnumerable<InsurancePolicy> GetInsuranceCompany()
        {
            return context.TBL_INSURANCE_COMPANY.Select(x => new InsurancePolicy
            {
                insuranceCompanyId = x.INSURANCECOMPANYID,
                companyName = x.COMPANYNAME,
                address = x.ADDRESS,
                phoneNumber = x.PHONENUMBER,
                email = x.CONTACTEMAIL,
            });
        }
        public bool AddCollateralCoverage(CollateralCoverageViewModel model)
        {
            var data = new TBL_COLLATERAL_COVERAGE
            {
                COLLATERALSUBTYPEID = model.collateralSubTypeId,
                COVERAGE = model.coverage,
                CURRENCYID = model.currencyId,
                DELETED = false,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),

            };

            context.TBL_COLLATERAL_COVERAGE.Add(data);

            return context.SaveChanges() > 0;

        }

        public IEnumerable<CollateralCoverageViewModel> GetCollateralCoverage(int collateralSubTypeId)
        {
            return (context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == collateralSubTypeId && o.DELETED == false).Select(o => new CollateralCoverageViewModel
            {
                collateralSubTypeId = o.COLLATERALSUBTYPEID,
                currencyId = o.CURRENCYID,
                currencyName = context.TBL_CURRENCY.Where(x => x.CURRENCYID == o.CURRENCYID).Select(x => x.CURRENCYNAME).FirstOrDefault(),
                coverage = o.COVERAGE,
                collateralCoverageId = o.COLLATERALCOVERAGEID
            })).ToList();

        }

        public bool DeleteCollateralCoverage(int collateralCoverageId, int createdById)
        {
            var data = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALCOVERAGEID == collateralCoverageId).Select(o => o).FirstOrDefault();

            if (data == null) return false;

            data.DELETED = true;
            data.DELETEDBY = createdById;
            data.DATETIMEDELETED = genSetup.GetApplicationDate();

            return context.SaveChanges() > 0;

        }

        public bool DeleteAddedValuer(int valuerId, int createdById)
        {
            var data = context.TBL_VALUATION_REPORT.Find(valuerId);

            if (data == null) return false;

            data.DELETED = true;
            data.DELETEDBY = createdById;
            data.DATETIMEDELETED = genSetup.GetApplicationDate();

            return context.SaveChanges() > 0;

        }

        public bool DeleteInsuranceRequest(int insuranceRequestId)
        {
            var entity = context.TBL_INSURANCE_REQUEST.FirstOrDefault(ir => ir.INSURANCEREQUESTID == insuranceRequestId);
            if (entity != null) context.TBL_INSURANCE_REQUEST.Remove(entity);

            return context.SaveChanges() > 0;

        }


        public IEnumerable<CollateralCoverageViewModel> CalculateCoverateOfCollateral(CollateralCoverageViewModel model)
        {
            var list = new List<CollateralCoverageViewModel>();

            int coveragePercentage = 0;
            decimal collateralValue = 0;
            decimal facilityAmount = 0;
            decimal availableCollateralValue = 0;
            decimal expectedCollateralCoverage = 0;
            decimal actualCollateralCoverage = 0;
            decimal sumOfMultipleProposedCollateralValues = 0;
            var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(model.loanApplicationDetailId);
            var currCodeFcy = facility.TBL_CURRENCY.CURRENCYCODE;
            var currCodeLcy = context.TBL_CURRENCY.Find(facility.TBL_LOAN_APPLICATION.TBL_COMPANY.CURRENCYID).CURRENCYCODE;
            var facilityExchangeRate = repo.GetExchangeRate(DateTime.Now, facility.CURRENCYID, facility.TBL_LOAN_APPLICATION.COMPANYID);
            var collateralExchangeRate = repo.GetExchangeRate(DateTime.Now, (short)model.currencyId, facility.TBL_LOAN_APPLICATION.COMPANYID);

            var data = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == model.collateralSubTypeId && o.CURRENCYID == model.currencyId).Select(o => o).FirstOrDefault();
            if (data == null)
                throw new Exception("Collateral Coverage has not been set!");


            coveragePercentage = data.COVERAGE;
            decimal coverage = decimal.Divide(data.COVERAGE, 100);
            collateralValue = model.collateralValue * (decimal)collateralExchangeRate.sellingRate;
            facilityAmount = model.facilityAmount * (decimal)facilityExchangeRate.sellingRate;

            var alreadyProposedFacilitiesForThisCollateral = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.DELETED == false && o.COLLATERALCUSTOMERID == model.collateralId).Select(o => o).ToList();

            //  var multipleCollateralOneFacility = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId).Select(o => o.COLLATERALCUSTOMERID).ToList();

            //if (multipleCollateralOneFacility.Count > 1) {
            //    sumOfMultipleCollateralValues = context.TBL_COLLATERAL_CUSTOMER.Where(o => multipleCollateralOneFacility.Contains(o.COLLATERALCUSTOMERID)).Sum(o => o.COLLATERALVALUE);

            //    if(facilityAmount > sumOfMultipleCollateralValues)
            //    {

            //    }
            //}

            availableCollateralValue = collateralValue;

            if (alreadyProposedFacilitiesForThisCollateral.Count != 0)
            {
                // decimal facilityValue = 0;
                //foreach (var facility in alreadyProposedFacilitiesForThisCollateral)
                //{
                //    facilityValue = facilityValue + context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == facility.LOANAPPLICATIONID).Select(o => o.APPROVEDAMOUNT).FirstOrDefault();
                //}
                //availableCollateralValue = availableCollateralValue - decimal.Multiply(coverage, facilityValue);

                decimal collateralCoverages = 0;
                collateralCoverages = alreadyProposedFacilitiesForThisCollateral.Sum(p => p.COLLATERALCOVERAGE);
                availableCollateralValue = availableCollateralValue - collateralCoverages;
            }
            var proposedCollateralsToFacility = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.DELETED == false && o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId).ToList();
            sumOfMultipleProposedCollateralValues = (decimal?)proposedCollateralsToFacility.Sum(c => c.COLLATERALCOVERAGE) ?? 0;
            expectedCollateralCoverage = (decimal.Multiply(coverage, facilityAmount)) - sumOfMultipleProposedCollateralValues;

            if (availableCollateralValue > expectedCollateralCoverage)
            {
                actualCollateralCoverage = expectedCollateralCoverage;
            }
            else
            {

                actualCollateralCoverage = availableCollateralValue;

            }

            var cov = new CollateralCoverageViewModel
            {
                collateralValue = collateralValue,
                facilityAmount = facilityAmount,
                baseCurrencyCode = currCodeLcy,
                facilityCurrencyCodeFcy = currCodeFcy,
                expectedCoveragePercentage = coveragePercentage,
                expectedCollateralCoverage = expectedCollateralCoverage,
                availableCollateralValue = availableCollateralValue,
                actualCollateralCoverage = actualCollateralCoverage,
                referenceNumber = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == model.loanApplicationId).Select(x => x.RELATEDREFERENCENUMBER).FirstOrDefault(),
                productName = model.productName,
                loanApplicationDetailId = model.loanApplicationDetailId,
                loanApplicationId = model.loanApplicationId,
                collateralId = model.collateralId
            };

            list.Add(cov);

            return list;
        }

        public IEnumerable<CollateralCoverageViewModel> CalculateCoverateOfCollateralLMS(CollateralCoverageViewModel model)
        {

            var list = new List<CollateralCoverageViewModel>();

            int coveragePercentage = 0;
            decimal collateralValue = 0;
            decimal facilityAmount = 0;
            decimal availableCollateralValue = 0;
            decimal expectedCollateralCoverage = 0;
            decimal actualCollateralCoverage = 0;
            decimal sumOfMultipleProposedCollateralValues = 0;
            var facility = context.TBL_LOAN_APPLICATION_DETAIL.Find(model.loanApplicationDetailId);
            var currCodeFcy = facility.TBL_CURRENCY.CURRENCYCODE;
            var currCodeLcy = context.TBL_CURRENCY.Find(facility.TBL_LOAN_APPLICATION.TBL_COMPANY.CURRENCYID).CURRENCYCODE;
            var facilityExchangeRate = repo.GetExchangeRate(DateTime.Now, facility.CURRENCYID, facility.TBL_LOAN_APPLICATION.COMPANYID);
            var collateralExchangeRate = repo.GetExchangeRate(DateTime.Now, (short)model.currencyId, facility.TBL_LOAN_APPLICATION.COMPANYID);

            var data = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == model.collateralSubTypeId && o.CURRENCYID == model.currencyId).Select(o => o).FirstOrDefault();
            if (data == null)
                throw new Exception("Collateral Coverage has not been set!");


            coveragePercentage = data.COVERAGE;
            decimal coverage = decimal.Divide(data.COVERAGE, 100);
            collateralValue = model.collateralValue * (decimal)collateralExchangeRate.sellingRate;
            facilityAmount = model.facilityAmount * (decimal)facilityExchangeRate.sellingRate;

            var alreadyProposedFacilitiesForThisCollateral = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.DELETED == false && o.COLLATERALCUSTOMERID == model.collateralId).Select(o => o).ToList();

            //  var multipleCollateralOneFacility = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId).Select(o => o.COLLATERALCUSTOMERID).ToList();

            //if (multipleCollateralOneFacility.Count > 1) {
            //    sumOfMultipleCollateralValues = context.TBL_COLLATERAL_CUSTOMER.Where(o => multipleCollateralOneFacility.Contains(o.COLLATERALCUSTOMERID)).Sum(o => o.COLLATERALVALUE);

            //    if(facilityAmount > sumOfMultipleCollateralValues)
            //    {

            //    }
            //}

            availableCollateralValue = collateralValue;

            if (alreadyProposedFacilitiesForThisCollateral.Count != 0)
            {
                // decimal facilityValue = 0;
                //foreach (var facility in alreadyProposedFacilitiesForThisCollateral)
                //{
                //    facilityValue = facilityValue + context.TBL_LOAN_APPLICATION_DETAIL.Where(o => o.LOANAPPLICATIONID == facility.LOANAPPLICATIONID).Select(o => o.APPROVEDAMOUNT).FirstOrDefault();
                //}
                //availableCollateralValue = availableCollateralValue - decimal.Multiply(coverage, facilityValue);

                decimal collateralCoverages = 0;
                collateralCoverages = alreadyProposedFacilitiesForThisCollateral.Sum(p => p.COLLATERALCOVERAGE);
                availableCollateralValue = availableCollateralValue - collateralCoverages;
            }
            var proposedCollateralsToFacility = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.DELETED == false && o.LOANAPPLICATIONDETAILID == model.loanApplicationDetailId).ToList();
            sumOfMultipleProposedCollateralValues = (decimal?)proposedCollateralsToFacility.Sum(c => c.COLLATERALCOVERAGE) ?? 0;
            expectedCollateralCoverage = (decimal.Multiply(coverage, facilityAmount)) - sumOfMultipleProposedCollateralValues;

            if (availableCollateralValue > expectedCollateralCoverage)
            {
                actualCollateralCoverage = expectedCollateralCoverage;
            }
            else
            {

                actualCollateralCoverage = availableCollateralValue;

            }

            var cov = new CollateralCoverageViewModel
            {
                collateralValue = collateralValue,
                facilityAmount = facilityAmount,
                baseCurrencyCode = currCodeLcy,
                facilityCurrencyCodeFcy = currCodeFcy,
                expectedCoveragePercentage = coveragePercentage,
                expectedCollateralCoverage = expectedCollateralCoverage,
                availableCollateralValue = availableCollateralValue,
                actualCollateralCoverage = actualCollateralCoverage,
                referenceNumber = context.TBL_LOAN_APPLICATION.Where(x => x.LOANAPPLICATIONID == model.loanApplicationId).Select(x => x.RELATEDREFERENCENUMBER).FirstOrDefault(),
                productName = model.productName,
                loanApplicationDetailId = model.loanApplicationDetailId,
                loanApplicationId = model.loanApplicationId,
                collateralId = model.collateralId
            };

            list.Add(cov);

            return list;
            /* var list = new List<CollateralCoverageViewModel>();
             var loanApp = context.TBL_LMSR_APPLICATION.Find(model.loanApplicationId);
             int coveragePercentage = 0;
             decimal collateralValue = 0;
             decimal facilityAmount = 0;
             decimal availableCollateralValue = 0;
             decimal expectedCollateralCoverage = 0;
             decimal actualCollateralCoverage = 0;
             decimal sumOfMultipleProposedCollateralValues = 0;
             var facility = context.TBL_LMSR_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == model.loanApplicationId).FirstOrDefault();
             var currency = context.TBL_CURRENCY.Find(facility.CURRENCYID);
             var currCodeFcy = currency.CURRENCYCODE;
             var currCodeLcy = context.TBL_CURRENCY.Find(currency.CURRENCYID).CURRENCYCODE;
             var facilityExchangeRate = repo.GetExchangeRate(DateTime.Now, currency.CURRENCYID, loanApp.COMPANYID);
             var collateralExchangeRate = repo.GetExchangeRate(DateTime.Now, (short)model.currencyId, loanApp.COMPANYID);

             var data = context.TBL_COLLATERAL_COVERAGE.Where(o => o.COLLATERALSUBTYPEID == model.collateralSubTypeId && o.CURRENCYID == model.currencyId).Select(o => o).FirstOrDefault();
             if (data == null)
                 throw new Exception("Collateral Coverage has not been set!");


             coveragePercentage = data.COVERAGE;
             decimal coverage = decimal.Divide(data.COVERAGE, 100);
             collateralValue = model.collateralValue * (decimal)collateralExchangeRate.sellingRate;
             facilityAmount = model.facilityAmount * (decimal)facilityExchangeRate.sellingRate;

             var alreadyProposedFacilitiesForThisCollateral = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.DELETED == false && o.COLLATERALCUSTOMERID == model.collateralId).Select(o => o).ToList();

             availableCollateralValue = collateralValue;

             if (alreadyProposedFacilitiesForThisCollateral.Count != 0)
             {
                 decimal collateralCoverages = 0;
                 collateralCoverages = alreadyProposedFacilitiesForThisCollateral.Sum(p => p.COLLATERALCOVERAGE);
                 availableCollateralValue = availableCollateralValue - collateralCoverages;
             }
             var proposedCollateralsToFacility = context.TBL_LOAN_APPLICATION_COLLATERL.Where(o => o.DELETED == false && o.LOANAPPLICATIONDETAILID == facility.LOANREVIEWAPPLICATIONID).ToList();
             sumOfMultipleProposedCollateralValues = (decimal?)proposedCollateralsToFacility.Sum(c => c.COLLATERALCOVERAGE) ?? 0;
             expectedCollateralCoverage = (decimal.Multiply(coverage, facilityAmount)) - sumOfMultipleProposedCollateralValues;

             if (availableCollateralValue > expectedCollateralCoverage)
             {
                 actualCollateralCoverage = expectedCollateralCoverage;
             }
             else
             {

                 actualCollateralCoverage = availableCollateralValue;

             }

             var cov = new CollateralCoverageViewModel
             {
                 collateralValue = collateralValue,
                 facilityAmount = facilityAmount,
                 baseCurrencyCode = currCodeLcy,
                 facilityCurrencyCodeFcy = currCodeFcy,
                 expectedCoveragePercentage = coveragePercentage,
                 expectedCollateralCoverage = expectedCollateralCoverage,
                 availableCollateralValue = availableCollateralValue,
                 actualCollateralCoverage = actualCollateralCoverage,
                 referenceNumber = context.TBL_LMSR_APPLICATION.Where(x => x.LOANAPPLICATIONID == model.loanApplicationId).Select(x => x.RELATEDREFERENCENUMBER).FirstOrDefault(),
                 productName = model.productName,
                 loanApplicationDetailId = model.loanApplicationDetailId,
                 loanApplicationId = model.loanApplicationId,
                 collateralId = model.collateralId
             };

             list.Add(cov);

             return list;*/
        }

        public InsuranceCompanyViewModel GetInsuranceCompany(int id)
        {
            var entity = context.TBL_INSURANCE_COMPANY.FirstOrDefault(x => x.INSURANCECOMPANYID == id && x.DELETED == false);

            return new InsuranceCompanyViewModel
            {
                insuranceCompanyId = entity.INSURANCECOMPANYID,
                iompanyId = entity.COMPANYID,
                companyName = entity.COMPANYNAME,
                address = entity.ADDRESS,
                contactEmail = entity.CONTACTEMAIL,
                phoneNumber = entity.PHONENUMBER
            };
        }

        public IEnumerable<InsuranceCompanyViewModel> GetInsuranceCompanies()
        {
            return context.TBL_INSURANCE_COMPANY.Where(x => x.DELETED == false)
                 .Select(x => new InsuranceCompanyViewModel
                 {
                     insuranceCompanyId = x.INSURANCECOMPANYID,
                     iompanyId = x.COMPANYID,
                     companyName = x.COMPANYNAME,
                     address = x.ADDRESS,
                     contactEmail = x.CONTACTEMAIL,
                     phoneNumber = x.PHONENUMBER
                 }).OrderBy(x => x.companyName)
                 .ToList();
        }

        public bool AddInsuranceCompany(InsuranceCompanyViewModel model)
        {
            var entity = new TBL_INSURANCE_COMPANY
            {
                COMPANYNAME = model.companyName,
                ADDRESS = model.address,
                CONTACTEMAIL = model.contactEmail,
                CREATEDBY = model.createdBy,
                PHONENUMBER = model.phoneNumber,
                DATETIMECREATED = genSetup.GetApplicationDate(),
            };

            context.TBL_INSURANCE_COMPANY.Add(entity);
            return context.SaveChanges() != 0;
        }

        public bool DeleteInsuranceCompany(int id, UserInfo user)
        {
            var entity = this.context.TBL_INSURANCE_COMPANY.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = genSetup.GetApplicationDate();
            return context.SaveChanges() != 0;

        }

        public bool UpdateInsuranceCompany(InsuranceCompanyViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_INSURANCE_COMPANY.Find(id);
            entity.COMPANYNAME = model.companyName;
            entity.CONTACTEMAIL = model.contactEmail;
            entity.ADDRESS = model.address;
            entity.PHONENUMBER = model.phoneNumber;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = genSetup.GetApplicationDate();
            return context.SaveChanges() != 0;
        }

        public CollateralInsurancePolicyViewModel GetAddedInsuranceById(int id)
        {
            var insurance = context.TBL_COLLATERAL_ITEM_POLICY.Where(x => x.COLLATERALCUSTOMERID == id
                                                                     && x.APPROVALSTATUSID == (short)ApprovalStatusEnum.Processing).FirstOrDefault();

            var result = new CollateralInsurancePolicyViewModel();

            if (insurance != null)
            {
                result.policyId = insurance.POLICYID;
                result.collateraalId = insurance.COLLATERALCUSTOMERID;
                result.referenceNumber = insurance.POLICYREFERENCENUMBER;
                result.insuranceCompanyId = insurance.INSURANCECOMPANYID;
                result.sumInsured = insurance.SUMINSURED;
                result.startDate = (DateTime)insurance.STARTDATE;
                result.expiryDate = (DateTime)insurance.ENDDATE;
                result.insuranceTypeId = insurance.INSURANCETYPEID;
                result.inSurPremiumAmount = insurance.PREMIUMAMOUNT;
                result.premiumPercent = insurance.PREMIUMPERCENT;
                result.description = insurance.DESCRIPTION;
                //result.hasExpired = insurance.HASEXPIRED;
                result.policyStateId = insurance.POLICYSTATEID;
                result.companyAddress = insurance.COMPANYADDRESS;

            }

            return result;
        }

        public InsuranceTypeViewModel GetInsuranceType(int id)
        {
            var entity = context.TBL_INSURANCE_TYPE.FirstOrDefault(x => x.INSURANCETYPEID == id && x.DELETED == false);

            return new InsuranceTypeViewModel
            {
                insuranceType = entity.INSURANCETYPE,
                insuranceTypeId = entity.INSURANCETYPEID,
            };
        }

        public IEnumerable<InsuranceTypeViewModel> GetInsuranceTypes()
        {
            var data = context.TBL_INSURANCE_TYPE.Where(x => x.DELETED == false)
                 .Select(x => new InsuranceTypeViewModel
                 {
                     insuranceTypeId = x.INSURANCETYPEID,
                     insuranceType = x.INSURANCETYPE,
                 })
                 .ToList();
            return data;
        }

        public IEnumerable<CollateralTypeViewModel> GetCollateralTypes()
        {
            var data = context.TBL_COLLATERAL_TYPE.Where(x => x.DELETED == false)
                 .Select(x => new CollateralTypeViewModel
                 {
                     collateralTypeId = x.COLLATERALTYPEID,
                     collateralTypeName = x.COLLATERALTYPENAME,
                 }).OrderBy(x => x.collateralTypeName)
                 .ToList();
            return data;
        }

        public IEnumerable<CollateralSubTypeViewModel> GetCollateralSubTypes(int collateralTypeId)
        {
            var data = context.TBL_COLLATERAL_TYPE_SUB.Where(x => x.DELETED == false && x.COLLATERALTYPEID == collateralTypeId)
                 .Select(x => new CollateralSubTypeViewModel
                 {
                     collateralTypeId = x.COLLATERALTYPEID,
                     collateralSubTypeId = x.COLLATERALSUBTYPEID,
                     collateralSubTypeName = x.COLLATERALSUBTYPENAME,
                     isGpsCoordinatesCollateralType = x.ISGPSCOORDINATESCOLLATERALTYPE,
                 }).OrderBy(x => x.collateralSubTypeName)
                 .ToList();
            return data;
        }


        public IEnumerable<InsuranceStatusViewModel> GetInsuranceStatus()
        {
            var data = context.TBL_COLLATERAL_INSURANCE_STATUS.Where(x => x.DELETED == false)
                 .Select(x => new InsuranceStatusViewModel
                 {
                     insuranceStatusId = x.INSURANCESTATUSID,
                     insuranceStatus = x.INSURANCESTATUS,
                     deleted = x.DELETED
                 })
                 .ToList();
            return data;
        }

        public IEnumerable<InsuranceTypeViewModel> GetInsuranceTypesViewAll()
        {
            var data = context.TBL_INSURANCE_TYPE.Where(x => x.DELETED == false)
                 .Select(x => new InsuranceTypeViewModel
                 {
                     insuranceTypeId = x.INSURANCETYPEID,
                     insuranceType = x.INSURANCETYPE,
                 })
                 .ToList();
            return data;
        }

        public IEnumerable<InsurancePolicyTypeViewModel> GetInsurancePolicyTypes()
        {
            return context.TBL_INSURANCE_POLICY_TYPE.Where(x => x.DELETED == false)
                 .Select(x => new InsurancePolicyTypeViewModel
                 {
                     policyTypeId = x.POLICYTYPEID,
                     description = x.DESCRIPTION,
                     valuationRequired = x.VALUATIONREQUIRED == true ? true : false,
                 }).OrderBy(x => x.description)
                 .ToList();
        }

        public bool AddInsuranceType(InsuranceTypeViewModel model)
        {
            var entity = new TBL_INSURANCE_TYPE
            {
                INSURANCETYPE = model.insuranceType,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
            };

            context.TBL_INSURANCE_TYPE.Add(entity);
            return context.SaveChanges() != 0;
        }

        public bool AddInsurancePolicyType(InsurancePolicyTypeViewModel model)
        {
            if (model.valuation == "0")
            {
                model.valuationRequired = false;
            }
            else { model.valuationRequired = true; }
            var entity = new TBL_INSURANCE_POLICY_TYPE
            {
                DESCRIPTION = model.description,
                VALUATIONREQUIRED = model.valuationRequired,
                DATETIMECREATED = DateTime.Now,
                CREATEDBY = model.createdBy,
                DELETED = false,
            };

            context.TBL_INSURANCE_POLICY_TYPE.Add(entity);
            return context.SaveChanges() != 0;
        }

        public bool DeleteInsuranceType(int id, UserInfo user)
        {
            var entity = this.context.TBL_INSURANCE_TYPE.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = genSetup.GetApplicationDate();
            return context.SaveChanges() != 0;
        }

        public bool DeleteInsurancePolicyType(int id, UserInfo user)
        {
            var entity = this.context.TBL_INSURANCE_POLICY_TYPE.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = genSetup.GetApplicationDate();
            return context.SaveChanges() != 0;
        }

        public bool UpdateInsuranceType(InsuranceTypeViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_INSURANCE_TYPE.Find(id);
            entity.INSURANCETYPE = model.insuranceType;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = genSetup.GetApplicationDate();
            return context.SaveChanges() != 0;
        }

        public bool UpdateInsurancePolicyType(InsurancePolicyTypeViewModel model, int id, UserInfo user)
        {
            if (model.valuation == "0")
            {
                model.valuationRequired = false;
            }
            else { model.valuationRequired = true; }

            var entity = this.context.TBL_INSURANCE_POLICY_TYPE.Find(id);
            entity.DESCRIPTION = model.description;
            entity.VALUATIONREQUIRED = model.valuationRequired;
            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = genSetup.GetApplicationDate();
            return context.SaveChanges() != 0;
        }

        public bool AddInsurancePolicyFile(InsurancePolicy model)
        {
            var entity = new TBL_COLLATERAL_ITEM_POLICY
            {

                POLICYREFERENCENUMBER = model.referenceNumber,
                //INSURANCETYPE = model.insuranceType,
                INSURANCETYPEID = (int)model.insuranceTypeId,
                SUMINSURED = (decimal)model.sumInsured,
                DATETIMECREATED = model.dateTimeCreated,
                HASEXPIRED = model.hasExpired,
                CREATEDBY = model.createdBy,
                DELETED = false

            };
            context.TBL_COLLATERAL_ITEM_POLICY.Add(entity);

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            this.auditTrail.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseApproval,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"TBL_INSURANCE_TYPE '{entity.ToString()}' created by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName(),

            });

            return context.SaveChanges() != 0;
        }


        public bool DeleteInsurancePolicy(int id, UserInfo user)
        {
            var entity = this.context.TBL_COLLATERAL_ITEM_POLICY.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = genSetup.GetApplicationDate();
            return context.SaveChanges() != 0;
        }

        #region collateralswap
        public IEnumerable<CollateralSwapViewModel> GetAllCollateralSwaps(int staffId)
        {
            var requests = context.TBL_COLLATERAL_SWAP_REQUEST.ToList();
            var staffs = genSetup.GetStaffRlieved(staffId).ToList();
            var businessRoleIds = context.TBL_CREDIT_OFFICER_STAFFROLE.Select(s => s.STAFFROLEID).ToList();
            var swapsInProgress = (from s in context.TBL_COLLATERAL_SWAP_REQUEST
                                   join c in context.TBL_CUSTOMER on s.TBL_LOAN_APPLICATION_COLLATERL.CUSTOMERID equals c.CUSTOMERID
                                   join d in context.TBL_LOAN_APPLICATION_DETAIL on s.TBL_LOAN_APPLICATION_COLLATERL.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                   join t in context.TBL_APPROVAL_TRAIL on s.COLLATERALSWAPID equals t.TARGETID
                                   where
                                    (
                                    s.DELETED == false
                                    && t.OPERATIONID == (int)OperationsEnum.CollateralSwap
                                    && t.APPROVALSTATEID != (int)ApprovalState.Ended
                                    && t.RESPONSESTAFFID == null
                                    //&& s.COLLATERALSWAPSTATUSID == (int)LoanApplicationStatusEnum.collateralSwapInProgress
                                    )
                                   select new CollateralSwapViewModel
                                   {
                                       collateralSwapId = s.COLLATERALSWAPID,
                                       loanCollateralMappingId = s.LOANCOLLATERALMAPPINGID,
                                       loanAppCollateralId = s.LOANAPPCOLLATERALID,
                                       oldCollateralId = s.OLDCOLLATERALID,
                                       newCollateralId = s.NEWCOLLATERALID,
                                       customerId = (s.CUSTOMERID > 0) ? s.CUSTOMERID : s.TBL_LOAN_APPLICATION_COLLATERL.CUSTOMERID,
                                       customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                       loanApplicationId = s.TBL_LOAN_APPLICATION_COLLATERL.LOANAPPLICATIONID,
                                       oldCollateralCode = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == s.OLDCOLLATERALID).COLLATERALCODE,
                                       newCollateralCode = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == s.NEWCOLLATERALID).COLLATERALCODE,
                                       collateralSwapStatusId = s.COLLATERALSWAPSTATUSID,
                                       approvalStatusId = t.APPROVALSTATUSID,
                                       approvalTrailId = t.APPROVALTRAILID,
                                       systemArrivalDateTime = t.SYSTEMARRIVALDATETIME,
                                       loopedStaffId = t.LOOPEDSTAFFID,
                                       toRoleId = t.LOOPEDSTAFFID > 0 ? context.TBL_STAFF.FirstOrDefault(a => a.STAFFID == t.LOOPEDSTAFFID).STAFFROLEID : 0,
                                       applicationReferenceNumber = s.TBL_LOAN_APPLICATION_COLLATERL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                       productName = d.TBL_PRODUCT1.PRODUCTNAME,
                                       swapRef = s.SWAPREF,
                                       coverage = s.TBL_LOAN_APPLICATION_COLLATERL.COLLATERALCOVERAGE,
                                   }).GroupBy(l => l.collateralSwapId).Select(l => l.OrderByDescending(t => t.approvalTrailId).FirstOrDefault())
                                       .Where(l => (l.approvalStatusId == (int)ApprovalStatusEnum.Disapproved)
                                                || (l.approvalStatusId == (int)ApprovalStatusEnum.Referred
                                                && staffs.Contains(l.loopedStaffId ?? 0))).ToList();

            var swapsNotStarted = (from s in context.TBL_COLLATERAL_SWAP_REQUEST
                                   join c in context.TBL_CUSTOMER on s.TBL_LOAN_APPLICATION_COLLATERL.CUSTOMERID equals c.CUSTOMERID
                                   join d in context.TBL_LOAN_APPLICATION_DETAIL on s.TBL_LOAN_APPLICATION_COLLATERL.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                   where s.COLLATERALSWAPSTATUSID == null
                                   && staffs.Contains(s.CREATEDBY)
                                   select new CollateralSwapViewModel
                                   {
                                       collateralSwapId = s.COLLATERALSWAPID,
                                       loanCollateralMappingId = s.LOANCOLLATERALMAPPINGID,
                                       loanAppCollateralId = s.LOANAPPCOLLATERALID,
                                       oldCollateralId = s.OLDCOLLATERALID,
                                       newCollateralId = s.NEWCOLLATERALID,
                                       customerId = (s.CUSTOMERID > 0) ? s.CUSTOMERID : s.TBL_LOAN_APPLICATION_COLLATERL.CUSTOMERID,
                                       customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                       loanApplicationId = s.TBL_LOAN_APPLICATION_COLLATERL.LOANAPPLICATIONID,
                                       oldCollateralCode = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == s.OLDCOLLATERALID).COLLATERALCODE,
                                       newCollateralCode = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == s.NEWCOLLATERALID).COLLATERALCODE,
                                       collateralSwapStatusId = s.COLLATERALSWAPSTATUSID,
                                       approvalStatusId = 1,
                                       approvalTrailId = 0,
                                       systemArrivalDateTime = s.DATETIMECREATED,
                                       loopedStaffId = 0,
                                       toRoleId = context.TBL_STAFF.FirstOrDefault(a => a.STAFFID == s.CREATEDBY).STAFFROLEID,
                                       applicationReferenceNumber = s.TBL_LOAN_APPLICATION_COLLATERL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                       productName = d.TBL_PRODUCT1.PRODUCTNAME,
                                       swapRef = s.SWAPREF,
                                       coverage = s.TBL_LOAN_APPLICATION_COLLATERL.COLLATERALCOVERAGE,

                                   }).ToList();

            var result = swapsNotStarted.Union(swapsInProgress);
            foreach (var s in result)
            {
                if (s.toRoleId > 0)
                {
                    s.isBusiness = businessRoleIds.Contains(s.toRoleId);
                }
            }
            return result;
        }

        public IEnumerable<CollateralSwapViewModel> GetCollateralSwapsForApproval(int staffId)
        {
            var operationId = (int)OperationsEnum.CollateralSwap;
            var levelIds = genSetup.GetStaffApprovalLevelIds(staffId, operationId).ToList();
            var staffs = genSetup.GetStaffRlieved(staffId).ToList();
            var businessRoleIds = context.TBL_CREDIT_OFFICER_STAFFROLE.Select(s => s.STAFFROLEID).ToList();

            var collateralSwapsForApproval = (from s in context.TBL_COLLATERAL_SWAP_REQUEST
                                              join c in context.TBL_CUSTOMER on s.TBL_LOAN_APPLICATION_COLLATERL.CUSTOMERID equals c.CUSTOMERID
                                              join t in context.TBL_APPROVAL_TRAIL on s.COLLATERALSWAPID equals t.TARGETID
                                              join d in context.TBL_LOAN_APPLICATION_DETAIL on s.TBL_LOAN_APPLICATION_COLLATERL.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                              where (s.DELETED == false && t.OPERATIONID == (int)OperationsEnum.CollateralSwap
                                                //&& s.COLLATERALSWAPSTATUSID == (int)LoanApplicationStatusEnum.collateralSwapInProgress
                                                && t.APPROVALSTATEID != (int)ApprovalState.Ended
                                                && t.LOOPEDSTAFFID == null
                                                && t.RESPONSESTAFFID == null
                                                && (levelIds.Contains((int)t.TOAPPROVALLEVELID))
                                                //|| (!levelIds.Contains((int)t.TOAPPROVALLEVELID) && t.LOOPEDSTAFFID == staffId)
                                                //&& levelIds.Contains((int)t.TOAPPROVALLEVELID)
                                                && (t.TOSTAFFID == null || staffs.Contains(t.TOSTAFFID ?? 0)))
                                              select new CollateralSwapViewModel
                                              {
                                                  collateralSwapId = s.COLLATERALSWAPID,
                                                  loanCollateralMappingId = s.LOANCOLLATERALMAPPINGID,
                                                  loanAppCollateralId = s.LOANAPPCOLLATERALID,
                                                  oldCollateralId = s.OLDCOLLATERALID,
                                                  newCollateralId = s.NEWCOLLATERALID,
                                                  operationId = t.OPERATIONID,
                                                  customerId = (s.CUSTOMERID > 0) ? s.CUSTOMERID : s.TBL_LOAN_APPLICATION_COLLATERL.CUSTOMERID,
                                                  loanApplicationId = s.TBL_LOAN_APPLICATION_COLLATERL.LOANAPPLICATIONID,
                                                  collateralSwapStatusId = s.COLLATERALSWAPSTATUSID,
                                                  approvalStatusId = t.APPROVALSTATUSID,
                                                  approvalTrailId = t.APPROVALTRAILID,
                                                  currentApprovalLevelId = t.TOAPPROVALLEVELID,
                                                  currentApprovalLevel = t.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                                  approvalStatus = context.TBL_APPROVAL_STATUS.FirstOrDefault(a => a.APPROVALSTATUSID == t.APPROVALSTATUSID).APPROVALSTATUSNAME.ToUpper(),
                                                  customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                                  applicationReferenceNumber = s.TBL_LOAN_APPLICATION_COLLATERL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                  productName = d.TBL_PRODUCT1.PRODUCTNAME,
                                                  swapRef = s.SWAPREF,
                                                  systemArrivalDateTime = t.SYSTEMARRIVALDATETIME,
                                                  oldCollateralCode = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == s.OLDCOLLATERALID).COLLATERALCODE,
                                                  newCollateralCode = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == s.NEWCOLLATERALID).COLLATERALCODE,
                                                  coverage = s.TBL_LOAN_APPLICATION_COLLATERL.COLLATERALCOVERAGE,
                                                  toRoleId = t.TOSTAFFID > 0 ? context.TBL_STAFF.FirstOrDefault(a => t.TOSTAFFID == a.STAFFID).STAFFROLEID : (t.TBL_APPROVAL_LEVEL1.STAFFROLEID ?? 0),
                                                  dateTimeCreated = s.DATETIMECREATED
                                              }).GroupBy(d => d.collateralSwapId)
                                                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault()).ToList();
            foreach (var s in collateralSwapsForApproval)
            {
                if (s.toRoleId > 0)
                {
                    s.isBusiness = businessRoleIds.Contains(s.toRoleId);
                }
            }
            return collateralSwapsForApproval;
        }


        public CollateralSwapViewModel GetCollateralSwap(int collateralSwapId)
        {
            var swap = (from s in context.TBL_COLLATERAL_SWAP_REQUEST
                        where s.COLLATERALSWAPID == collateralSwapId
                        select new CollateralSwapViewModel
                        {
                            collateralSwapId = s.COLLATERALSWAPID,
                            loanCollateralMappingId = s.LOANCOLLATERALMAPPINGID,
                            loanAppCollateralId = s.LOANAPPCOLLATERALID,
                            oldCollateralId = s.OLDCOLLATERALID,
                            newCollateralId = s.NEWCOLLATERALID,
                            customerId = !(s.CUSTOMERID > 0) ? s.TBL_LOAN_APPLICATION_COLLATERL.CUSTOMERID : s.CUSTOMERID,
                            collateralSwapStatusId = s.COLLATERALSWAPSTATUSID,
                            swapRef = s.SWAPREF
                        }).FirstOrDefault();
            return swap;
        }

        public IEnumerable<CollateralSwapViewModel> SearchCollateralSwap(string searchString)
        {
            searchString = searchString.Trim();

            var collateralSwapsForApproval = (from s in context.TBL_COLLATERAL_SWAP_REQUEST
                                              join c in context.TBL_CUSTOMER on s.TBL_LOAN_APPLICATION_COLLATERL.CUSTOMERID equals c.CUSTOMERID
                                              join d in context.TBL_LOAN_APPLICATION_DETAIL on s.TBL_LOAN_APPLICATION_COLLATERL.LOANAPPLICATIONDETAILID equals d.LOANAPPLICATIONDETAILID
                                              join t in context.TBL_APPROVAL_TRAIL on s.COLLATERALSWAPID equals t.TARGETID into swap
                                              from t in swap.DefaultIfEmpty()
                                              let isInitiation = s.COLLATERALSWAPSTATUSID == null
                                              let isLoop = t.LOOPEDSTAFFID > 0
                                              let loopStaff = context.TBL_STAFF.FirstOrDefault(a => a.STAFFID == t.LOOPEDSTAFFID)
                                              let isLoop2 = t.FROMAPPROVALLEVELID == t.FROMAPPROVALLEVELID
                                              where (s.DELETED == false && t.OPERATIONID == (int)OperationsEnum.CollateralSwap
                                                &&
                                                (
                                                   c.FIRSTNAME.Trim().Contains(searchString)
                                                || c.MIDDLENAME.Trim().Contains(searchString)
                                                || c.LASTNAME.Trim().Contains(searchString)
                                                || d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER.Trim().Contains(searchString)
                                                || s.SWAPREF.Trim().Contains(searchString)
                                                ))
                                              select new CollateralSwapViewModel
                                              {
                                                  collateralSwapId = s.COLLATERALSWAPID,
                                                  loanCollateralMappingId = s.LOANCOLLATERALMAPPINGID,
                                                  loanAppCollateralId = s.LOANAPPCOLLATERALID,
                                                  oldCollateralId = s.OLDCOLLATERALID,
                                                  newCollateralId = s.NEWCOLLATERALID,
                                                  customerId = (s.CUSTOMERID > 0) ? s.CUSTOMERID : s.TBL_LOAN_APPLICATION_COLLATERL.CUSTOMERID,
                                                  loanApplicationId = s.TBL_LOAN_APPLICATION_COLLATERL.LOANAPPLICATIONID,
                                                  collateralSwapStatusId = s.COLLATERALSWAPSTATUSID,
                                                  approvalStatusId = t.APPROVALSTATUSID,
                                                  approvalTrailId = t.APPROVALTRAILID,
                                                  currentApprovalLevelId = t.TOAPPROVALLEVELID,
                                                  currentApprovalLevel = (isInitiation) ? context.TBL_STAFF.FirstOrDefault(a => a.STAFFID == s.CREATEDBY).TBL_STAFF_ROLE.STAFFROLENAME : (isLoop) ? context.TBL_STAFF.FirstOrDefault(a => a.STAFFID == t.LOOPEDSTAFFID).TBL_STAFF_ROLE.STAFFROLENAME : t.TBL_APPROVAL_LEVEL1.LEVELNAME,
                                                  fromLevel = (isLoop2 && !isLoop) ? t.TBL_STAFF.TBL_STAFF_ROLE.STAFFROLENAME : t.TBL_APPROVAL_LEVEL.LEVELNAME,
                                                  responsiblePerson = (isLoop) ? loopStaff.FIRSTNAME + " " + loopStaff.MIDDLENAME + " " + loopStaff.LASTNAME : (t.TOSTAFFID > 0) ? t.TBL_STAFF2.FIRSTNAME + " " + t.TBL_STAFF2.MIDDLENAME + " " + t.TBL_STAFF2.LASTNAME : "N/A",
                                                  approvalStatus = (isInitiation) ? "Pending" : context.TBL_APPROVAL_STATUS.FirstOrDefault(a => a.APPROVALSTATUSID == t.APPROVALSTATUSID).APPROVALSTATUSNAME.ToUpper(),
                                                  customerName = c.FIRSTNAME + " " + c.MIDDLENAME + " " + c.LASTNAME,
                                                  applicationReferenceNumber = s.TBL_LOAN_APPLICATION_COLLATERL.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                                                  productName = d.TBL_PRODUCT1.PRODUCTNAME,
                                                  swapRef = s.SWAPREF,
                                                  systemArrivalDateTime = (isInitiation) ? s.DATETIMECREATED : t.SYSTEMARRIVALDATETIME,
                                                  oldCollateralCode = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == s.OLDCOLLATERALID).COLLATERALCODE,
                                                  newCollateralCode = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(c => c.COLLATERALCUSTOMERID == s.NEWCOLLATERALID).COLLATERALCODE,
                                                  coverage = s.TBL_LOAN_APPLICATION_COLLATERL.COLLATERALCOVERAGE,
                                                  dateTimeCreated = s.DATETIMECREATED,

                                              }).GroupBy(d => d.collateralSwapId)
                                                .Select(g => g.OrderByDescending(b => b.approvalTrailId).FirstOrDefault()).ToList();
            return collateralSwapsForApproval;
        }

        public CollateralSwapViewModel AddCollateralSwap(CollateralSwapViewModel model)
        {
            var reference = CommonHelpers.GenerateRandomDigitCode(10);
            var swapAlreadyExists = context.TBL_COLLATERAL_SWAP_REQUEST.Any(s => s.LOANAPPCOLLATERALID == model.loanAppCollateralId
            && s.COLLATERALSWAPSTATUSID != (int)LoanApplicationStatusEnum.collateralSwapCompleted && s.COLLATERALSWAPSTATUSID != (int)ApprovalStatusEnum.Disapproved);

            if (swapAlreadyExists)
            {
                var existingSwap = context.TBL_COLLATERAL_SWAP_REQUEST.FirstOrDefault(s => s.LOANAPPCOLLATERALID == model.loanAppCollateralId
                && s.COLLATERALSWAPSTATUSID != (int)LoanApplicationStatusEnum.collateralSwapCompleted && s.COLLATERALSWAPSTATUSID != (int)ApprovalStatusEnum.Disapproved);
                var d = existingSwap.TBL_LOAN_APPLICATION_COLLATERL;
                throw new SecureException("Collateral Swap has already been initiated for this proposed Collateral for this Application Ref " + d.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER +
                    " and is currently undergoing approval, kindly search with the Reference to see the current status.");
            }
            var swap = new TBL_COLLATERAL_SWAP_REQUEST()
            {
                SWAPREF = reference,
                LOANCOLLATERALMAPPINGID = model.loanCollateralMappingId,
                LOANAPPCOLLATERALID = model.loanAppCollateralId,
                OLDCOLLATERALID = model.oldCollateralId,
                NEWCOLLATERALID = model.newCollateralId,
                CUSTOMERID = model.customerId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate()
            };

            context.TBL_COLLATERAL_SWAP_REQUEST.Add(swap);
            var saved = context.SaveChanges() != 0;
            if (swap.COLLATERALSWAPID <= 0)
            {
                throw new SecureException("Didn't return new swap Id");
            }
            model.collateralSwapId = swap.COLLATERALSWAPID;
            model.swapRef = reference;
            return model;

        }

        public String ResponseMessage(WorkflowResponse response, string itemHeading)
        {
            if (response.stateId != (int)ApprovalState.Ended)
            {
                if (response.statusId == (int)ApprovalStatusEnum.Referred)
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been REFERRED to " + response.nextLevelName;
                    }
                }
                else
                {
                    if (response.nextPersonId > 0)
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextPersonName;
                    }
                    else
                    {
                        return "The " + itemHeading + " request has been SENT to " + response.nextLevelName;
                    }
                }
            }
            else
            {
                if (response.statusId == (int)ApprovalStatusEnum.Approved)
                {
                    return "The " + itemHeading + " request has been APPROVED successfully";
                }
                else
                {
                    return "The " + itemHeading + " request has been DISAPPROVED successfully";
                }
            }

        }

        public WorkflowResponse CollateralSwapMemorandum(CollateralSwapViewModel model)
        {
            int operationId = (int)OperationsEnum.CollateralSwap; // CHANGE
            var cs = context.TBL_COLLATERAL_SWAP_REQUEST.Find(model.collateralSwapId);

            if (model.forwardAction != (int)ApprovalStatusEnum.Disapproved)
            {
                model.forwardAction = (int)ApprovalStatusEnum.Processing;
            }
            else
            {
                model.forwardAction = (int)ApprovalStatusEnum.Disapproved;
            }

            // WORKFLOW
            workflow.OperationId = operationId;
            workflow.StaffId = model.createdBy;
            workflow.TargetId = model.collateralSwapId;
            workflow.CompanyId = model.companyId;

            //workflow.Vote = model.vote;
            workflow.NextLevelId = null;
            workflow.ToStaffId = model.toStaffId != 0 ? model.toStaffId : null;
            workflow.StatusId = (int)model.forwardAction;
            workflow.Comment = model.comment;
            var c = context.TBL_CUSTOMER.Find(model.customerId);

            workflow.BusinessUnitId = c?.BUSINESSUNTID;
            var placeholders = new AlertPlaceholders();
            placeholders.customerName = "<br />CUSTOMER NAME: " + c?.FIRSTNAME + " " + c?.MIDDLENAME + " " + c?.LASTNAME;
            placeholders.referenceNumber = "<br />COLLATERAL SWAP REFERENCENUMBER: " + cs?.SWAPREF;
            placeholders.facilityType = "<br />COLLATERAL SWAP";
            placeholders.operationName = "<br />OPERATION NAME: COLLATERAL SWAP";
            placeholders.branchName = "<br />BRANCH NAME: " + c?.TBL_BRANCH.BRANCHNAME;
            workflow.Placeholders = placeholders;

            workflow.DeferredExecution = true;
            workflow.LogActivity();

            WorkflowResponse finalResponse = new WorkflowResponse();// workflow.Response;

            // UPDATE APPLICATION
            //cs.APPROVALSTATUSID = (short)workflow.StatusId;
            if (cs != null) { cs.COLLATERALSWAPSTATUSID = (int)LoanApplicationStatusEnum.collateralSwapInProgress; }

            //if (cs.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending) { cs.APPROVALSTATUSID = (int)ApprovalStatusEnum.Processing; }

            if (workflow.NewState == (int)ApprovalState.Ended) // cam status
            {
                if (workflow.StatusId == (int)ApprovalStatusEnum.Approved)
                {
                    cs.COLLATERALSWAPSTATUSID = (int)LoanApplicationStatusEnum.collateralSwapCompleted;
                    //cs.FINALAPPROVAL_LEVELID = workflow.Response.fromLevelId;
                    //cs.APPROVEDDATE = DateTime.Now;
                    workflow.SetResponse = true;
                    ArchiveOldCollateralOfLoan(model.loanAppCollateralId, model.newCollateralId, model.createdBy);
                }
                else //if (workflow.StatusId == (int)ApprovalStatusEnum.Disapproved)
                {
                    cs.COLLATERALSWAPSTATUSID = (int)ApprovalStatusEnum.Disapproved;
                    //SendEmailToCustomerForLoanDisapproval(model.LcIssuanceId, model.companyId);
                }

                //if (contextControl != null) contextControl.SaveChanges();
            }

            //cs.DATEACTEDON = DateTime.Now;
            context.SaveChanges();
            return workflow.Response;
        }

        public int GetNextLevelForCollateralSwap(int collateralSwapId, int createdBy, int companyId)
        {
            var approvalModel = new ForwardViewModel
            {
                createdBy = createdBy,
                companyId = companyId,
                applicationId = collateralSwapId,
                comment = "Get next level for collateral swap",
                operationId = (short)OperationsEnum.CollateralSwap,
                forwardAction = (int)ApprovalStatusEnum.Pending,
            };

            var response = drawdownRepo.LogApprovalForMessage(approvalModel, true);
            return response.nextLevelId.Value;
        }

        private void ArchiveOldCollateralOfLoan(int loanAppCollateralId, int newCollateralCustomerId, int createdBy)
        {
            var collateral = context.TBL_LOAN_APPLICATION_COLLATERL.Where(O => O.LOANAPPCOLLATERALID == loanAppCollateralId).FirstOrDefault();

            if (collateral != null)
            {
                var data = new TBL_LOAN_APPLICATION_COLLATERL_ARCH
                {
                    DATETIMEARCHIVED = DateTime.Now,
                    ARCHIVEDBY = createdBy,
                    COLLATERALCUSTOMERID = collateral.COLLATERALCUSTOMERID,
                    LOANAPPLICATIONID = collateral.LOANAPPCOLLATERALID,
                    APPROVALSTATUSID = collateral.APPROVALSTATUSID,
                    LOANAPPLICATIONDETAILID = collateral.LOANAPPLICATIONDETAILID,
                    COLLATERALCOVERAGE = collateral.COLLATERALCOVERAGE,
                    BALANCEAVAILABLE = collateral.BALANCEAVAILABLE,
                    CREATEDBY = collateral.CREATEDBY,
                    DATETIMECREATED = collateral.DATETIMECREATED,
                    CUSTOMERID = collateral.CUSTOMERID,
                    DELETED = collateral.DELETED,
                    SYSTEMDATETIME = collateral.SYSTEMDATETIME,
                    LOANAPPCOLLATERALID = collateral.LOANAPPCOLLATERALID,
                    LEGAL_FEE_AMOUNT = collateral.LEGAL_FEE_AMOUNT,
                    LEGAL_FEE_DATE = collateral.LEGAL_FEE_DATE,
                    LEGAL_FEE_TAKEN = collateral.LEGAL_FEE_TAKEN,
                    DELETEDBY = collateral.DELETEDBY,
                    DATETIMEDELETED = collateral.DATETIMEDELETED,
                    DATETIMEUPDATED = collateral.DATETIMEUPDATED,
                    LASTUPDATEDBY = collateral.LASTUPDATEDBY,
                };

                collateral.COLLATERALCUSTOMERID = newCollateralCustomerId;
                context.TBL_LOAN_APPLICATION_COLLATERL_ARCH.Add(data);
                context.SaveChanges();
            }
        }

        public bool UpdateCollateralSwap(CollateralSwapViewModel model, int id, UserInfo user)
        {
            var swap = context.TBL_COLLATERAL_SWAP_REQUEST.Find(id);
            if (swap == null) return false;

            swap.LOANAPPCOLLATERALID = model.loanAppCollateralId;
            swap.LOANCOLLATERALMAPPINGID = model.loanCollateralMappingId;
            swap.NEWCOLLATERALID = model.newCollateralId;
            swap.OLDCOLLATERALID = model.oldCollateralId;
            swap.CUSTOMERID = model.customerId;
            swap.DATETIMEUPDATED = genSetup.GetApplicationDate();
            swap.LASTUPDATEDBY = user.createdBy;
            //swap.LASTUPDATEDBY = model.createdBy;

            return context.SaveChanges() != 0;
        }

        public bool DeleteCollateralSwap(int collateralSwapId, UserInfo user)
        {
            var swap = context.TBL_COLLATERAL_SWAP_REQUEST.Find(collateralSwapId);
            if (swap == null) return false;
            swap.DELETED = true;
            swap.DELETEDBY = user.createdBy;

            return context.SaveChanges() != 0;
        }

        public IEnumerable<LoanApplicationDetailViewModel> GetCollateralMappingDetails(int loanAppCollateralId)
        {
            var data = (from m in context.TBL_LOAN_APPLICATION_COLLATERL
                        join a in context.TBL_LOAN_APPLICATION on m.LOANAPPLICATIONID equals a.LOANAPPLICATIONID
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on m.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        where a.DELETED == false && b.DELETED == false && m.DELETED == false && m.LOANAPPCOLLATERALID == loanAppCollateralId
                        select new LoanApplicationDetailViewModel
                        {
                            currencyName = b.TBL_CURRENCY.CURRENCYNAME,
                            exchangeRate = b.EXCHANGERATE,
                            loanApplicationDetailId = b.LOANAPPLICATIONDETAILID,
                            customerName = b.TBL_CUSTOMER.FIRSTNAME + " " + b.TBL_CUSTOMER.MIDDLENAME + " " + b.TBL_CUSTOMER.LASTNAME,
                            approvedProductId = b.APPROVEDPRODUCTID,
                            proposedProductName = b.TBL_PRODUCT.PRODUCTNAME,
                            proposedTenor = b.PROPOSEDTENOR,
                            proposedInterestRate = b.PROPOSEDINTERESTRATE,
                            proposedAmount = b.PROPOSEDAMOUNT,
                            sectorId = (short)b.TBL_SUB_SECTOR.SECTORID,
                            subSectorId = b.SUBSECTORID,
                            approvedProductName = context.TBL_PRODUCT.Where(o => o.PRODUCTID == b.APPROVEDPRODUCTID).Select(o => o.PRODUCTNAME).FirstOrDefault(),

                            //requireCollateral = a.REQUIRECOLLATERAL,
                            repaymentScheduleId = b.REPAYMENTSCHEDULEID,
                            isTakeOverApplication = b.ISTAKEOVERAPPLICATION,
                            repaymentTerm = b.REPAYMENTTERMS,
                            loanApplicationId = b.LOANAPPLICATIONID,
                            applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                            customerId = b.CUSTOMERID,
                            firstName = b.TBL_CUSTOMER.FIRSTNAME,
                            middleName = b.TBL_CUSTOMER.MIDDLENAME,
                            lastName = b.TBL_CUSTOMER.LASTNAME,
                            customerCode = b.TBL_CUSTOMER.CUSTOMERCODE,
                            proposedProductId = b.PROPOSEDPRODUCTID,
                            productClassProcessId = b.TBL_LOAN_APPLICATION.PRODUCT_CLASS_PROCESSID,
                            productClassId = (short?)b.TBL_PRODUCT.PRODUCTCLASSID,
                            customerType = b.TBL_CUSTOMER.TBL_CUSTOMER_TYPE.NAME,
                            branchName = a.TBL_BRANCH.BRANCHNAME,
                            customerGroupId = (int?)a.CUSTOMERGROUPID,//.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            customerGroupName = a.CUSTOMERGROUPID.HasValue ? a.TBL_CUSTOMER_GROUP.GROUPNAME : "",
                            approvalStatusId = a.APPROVALSTATUSID,
                        });
            var result = data.ToList();
            return result;
        }
        #endregion collateralswap


        public IEnumerable <FacilityStampDutyViewModel> GetFacilityStampDuty(int loanApplicationId)
        {

            var result = new List<FacilityStampDutyViewModel>();
            var loanApplicationDetailIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONID == loanApplicationId).Select(l => l.LOANAPPLICATIONDETAILID).ToList();

            foreach (var loanApplicationDetailId in loanApplicationDetailIds)
            {
               var stampDuty = context.TBL_FACILITY_STAMP_DUTY.Where(x => x.LOANAPPLICATIONDETAILID == loanApplicationDetailId && x.DELETED == false)
                    .Select(x => new FacilityStampDutyViewModel
                    {
                        facilityStampDutyId = x.FACILITYSTAMPDUTYID,
                        loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                        collateralCustomerId = x.COLLATERALCUSTOMERID,
                        osdc = x.OSDC,
                        asdc = x.ASDC,
                        csdc = x.CSDC,
                        dateTimeCreated = x.DATETIMEUPDATED,
                        isShared = x.ISSHARED,
                        customerPercentage = x.CUSTOMERPERCENTAGE,
                        bankPercentage = x.BANKPERCENTAGE
                    }).ToList();
                //if (stampDuty != null)
                //{

                //    facilityStampDutyId = stampDuty.FACILITYSTAMPDUTYID;
                //    loanApplicationDetailId = stampDuty.LOANAPPLICATIONDETAILID;
                //    collateralCustomerId = stampDuty.COLLATERALCUSTOMERID;
                //    osdc = stampDuty.OSDC;
                //    dateTimeCreated = stampDuty.DATETIMEUPDATED;
                //    isShared = stampDuty.ISSHARED;
                //    customerPercentage = stampDuty.CUSTOMERPERCENTAGE;
                //    bankPercentage = stampDuty.BANKPERCENTAGE;


                //}
                result = stampDuty;
                return result;
            }
            return result;
            
        }

        public IEnumerable<FacilityStampDutyViewModel> GetFacilityStampDutyId(int loanApplicationId)
        {
            //for loan information only
            var result = new List<FacilityStampDutyViewModel>();
            var loanApplicationDetailIds = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationId).Select(l => l.LOANAPPLICATIONDETAILID).ToList();

            foreach (var loanApplicationDetailId in loanApplicationDetailIds)
            {
                var stampDuty = context.TBL_FACILITY_STAMP_DUTY.Where(x => x.LOANAPPLICATIONDETAILID == loanApplicationDetailId && x.DELETED == false)
                     .Select(x => new FacilityStampDutyViewModel
                     {
                         facilityStampDutyId = x.FACILITYSTAMPDUTYID,
                         loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                         collateralCustomerId = x.COLLATERALCUSTOMERID,
                         osdc = x.OSDC,
                         asdc = x.ASDC,
                         csdc = x.CSDC,
                         dateTimeCreated = x.DATETIMEUPDATED,
                         isShared = x.ISSHARED,
                         customerPercentage = x.CUSTOMERPERCENTAGE,
                         bankPercentage = x.BANKPERCENTAGE
                     }).ToList();
                //if (stampDuty != null)
                //{

                //    facilityStampDutyId = stampDuty.FACILITYSTAMPDUTYID;
                //    loanApplicationDetailId = stampDuty.LOANAPPLICATIONDETAILID;
                //    collateralCustomerId = stampDuty.COLLATERALCUSTOMERID;
                //    osdc = stampDuty.OSDC;
                //    dateTimeCreated = stampDuty.DATETIMEUPDATED;
                //    isShared = stampDuty.ISSHARED;
                //    customerPercentage = stampDuty.CUSTOMERPERCENTAGE;
                //    bankPercentage = stampDuty.BANKPERCENTAGE;


                //}
                result = stampDuty;
                return result;
            }
            return result;

        }


        public IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDuty()
        {
            var fixedCharge = context.TBL_CHARGE_FEE.Where(c => c.CHARGEFEENAME.ToLower().Contains("(fixed)")).FirstOrDefault();
            var cond = context.TBL_STAMP_DUTY_CONDITION.Where(c => c.DUTIABLEVALUE != null).ToList();
            var record = (from x in context.TBL_FACILITY_STAMP_DUTY
                          join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                          join cl in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals cl.COLLATERALCUSTOMERID
                          where x.DELETED == false && x.CURRENTSTATUS == 2

                          select new FacilityStampDutyViewModel
                          {
                              facilityStampDutyId = x.FACILITYSTAMPDUTYID,
                              loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                              collateralCustomerId = x.COLLATERALCUSTOMERID,
                              osdc = x.OSDC,
                              asdc = x.ASDC,
                              csdc = x.CSDC,
                              dateTimeCreated = x.DATETIMECREATED,
                              isShared = x.ISSHARED,
                              customerPercentage = x.CUSTOMERPERCENTAGE,
                              bankPercentage = x.BANKPERCENTAGE,
                              customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.LASTNAME).FirstOrDefault(),
                              loanAmount = a.PROPOSEDAMOUNT,
                              approvedTenor = a.APPROVEDTENOR,
                              collateralSubTypeId = cl.COLLATERALSUBTYPEID,
                              collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(s=>s.COLLATERALSUBTYPEID == cl.COLLATERALSUBTYPEID).FirstOrDefault().COLLATERALSUBTYPENAME,
                              customerId = a.CUSTOMERID,
                              operationId = (int)OperationsEnum.StampDutyClosure,
                              //documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID
                          }).ToList();
            foreach(var rec in record)
            {
                rec.fixedDutyCharge = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(f => f.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId && f.CHARGEFEEID == fixedCharge.CHARGEFEEID).FirstOrDefault()?.RECOMMENDED_FEERATEVALUE == null ? 0 : context.TBL_LOAN_APPLICATION_DETL_FEE.Where(f => f.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId && f.CHARGEFEEID == fixedCharge.CHARGEFEEID).FirstOrDefault()?.RECOMMENDED_FEERATEVALUE;
                var condValue = cond.Where(c => c.COLLATERALSUBTYPEID == rec.collateralSubTypeId).FirstOrDefault();
                var dutyCharge = (rec.loanAmount * (condValue.DUTIABLEVALUE / 100)) + rec.fixedDutyCharge;
                rec.stampDutyAmount = (decimal)dutyCharge;
                rec.documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID;
                rec.dutiableValue = condValue.DUTIABLEVALUE;
                rec.bookingDate = context.TBL_LOAN.Where(b => b.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId).FirstOrDefault()?.BOOKINGDATE;

                if (rec.bookingDate != null)
                {
                    rec.maturityDate = context.TBL_LOAN.Where(b => b.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId).FirstOrDefault()?.MATURITYDATE;
                }
            }
            
            return record;
                          
        }

        public IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDutyFixed()
        {
            var fixedCharge = context.TBL_CHARGE_FEE.Where(c => c.CHARGEFEENAME.ToLower().Contains("(fixed)")).FirstOrDefault();
            var cond = context.TBL_STAMP_DUTY_CONDITION.Where(c => c.DUTIABLEVALUE != null).ToList();
            var record = (from x in context.TBL_LOAN_APPLICATION_DETL_FEE
                          join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                          //join f in context.TBL_FACILITY_STAMP_DUTY on x.LOANAPPLICATIONDETAILID equals f.LOANAPPLICATIONDETAILID
                          join cl in context.TBL_LOAN_APPLICATION_COLLATERL on x.LOANAPPLICATIONDETAILID equals cl.LOANAPPLICATIONDETAILID
                          where x.DELETED == false && x.CHARGEFEEID == fixedCharge.CHARGEFEEID

                          select new FacilityStampDutyViewModel
                          {
                              facilityStampDutyId = x.LOANCHARGEFEEID,
                              loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                              //collateralCustomerId = f.COLLATERALCUSTOMERID,
                              
                              dateTimeCreated = x.DATETIMECREATED,
                              
                              
                              customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.LASTNAME).FirstOrDefault(),
                              loanAmount = a.PROPOSEDAMOUNT,
                              approvedTenor = a.APPROVEDTENOR,
                              collateralId = cl.COLLATERALCUSTOMERID,
                              collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Where(s => s.COLLATERALCUSTOMERID == cl.COLLATERALCUSTOMERID).FirstOrDefault().COLLATERALSUBTYPEID,
                              customerId = a.CUSTOMERID,
                              operationId = (int)OperationsEnum.StampDutyClosure,
                              //documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID
                          }).OrderByDescending(x => x.facilityStampDutyId).ToList()?.Take(200);
            foreach (var rec in record)
            {
                rec.fixedDutyCharge = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(f => f.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId && f.CHARGEFEEID == fixedCharge.CHARGEFEEID).FirstOrDefault()?.RECOMMENDED_FEERATEVALUE;
                var condValue = cond.Where(c => c.COLLATERALSUBTYPEID == rec.collateralSubTypeId).FirstOrDefault();
                var dutyCharge =  rec.fixedDutyCharge;
                if (rec.fixedDutyCharge == null) dutyCharge = 0;
                rec.stampDutyAmount = (decimal)dutyCharge;
                rec.documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID;
                //rec.dutiableValue = condValue.DUTIABLEVALUE;
                rec.bookingDate = context.TBL_LOAN.Where(b => b.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId).FirstOrDefault()?.BOOKINGDATE;
                rec.collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(s => s.COLLATERALSUBTYPEID == rec.collateralSubTypeId).FirstOrDefault()?.COLLATERALSUBTYPENAME;

                if (rec.bookingDate != null)
                {
                    rec.maturityDate = context.TBL_LOAN.Where(b => b.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId).FirstOrDefault()?.MATURITYDATE;
                }
            }

            return record;

        }

        public IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDutyFixedFiltered(DateRange param)
        {
            param.endDate = param.endDate.AddHours(23);
            param.endDate = param.endDate.AddMinutes(59);
            param.endDate = param.endDate.AddSeconds(59);
            var fixedCharge = context.TBL_CHARGE_FEE.Where(c => c.CHARGEFEENAME.ToLower().Contains("(fixed)")).FirstOrDefault();
            var cond = context.TBL_STAMP_DUTY_CONDITION.Where(c => c.DUTIABLEVALUE != null).ToList();
            var record = (from x in context.TBL_LOAN_APPLICATION_DETL_FEE
                          join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                          //join f in context.TBL_FACILITY_STAMP_DUTY on x.LOANAPPLICATIONDETAILID equals f.LOANAPPLICATIONDETAILID
                          join cl in context.TBL_LOAN_APPLICATION_COLLATERL on x.LOANAPPLICATIONDETAILID equals cl.LOANAPPLICATIONDETAILID
                          where x.DELETED == false && x.CHARGEFEEID == fixedCharge.CHARGEFEEID && ((DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                                 && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)))

                          select new FacilityStampDutyViewModel
                          {
                              facilityStampDutyId = x.LOANCHARGEFEEID,
                              loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                              //collateralCustomerId = f.COLLATERALCUSTOMERID,

                              dateTimeCreated = x.DATETIMECREATED,


                              customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.LASTNAME).FirstOrDefault(),
                              loanAmount = a.PROPOSEDAMOUNT,
                              approvedTenor = a.APPROVEDTENOR,
                              collateralId = cl.COLLATERALCUSTOMERID,
                              collateralSubTypeId = context.TBL_COLLATERAL_CUSTOMER.Where(s => s.COLLATERALCUSTOMERID == cl.COLLATERALCUSTOMERID).FirstOrDefault().COLLATERALSUBTYPEID,
                              customerId = a.CUSTOMERID,
                              operationId = (int)OperationsEnum.StampDutyClosure,
                              //documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID
                          }).OrderByDescending(x => x.facilityStampDutyId).ToList().Take(200);
            foreach (var rec in record)
            {
                rec.fixedDutyCharge = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(f => f.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId && f.CHARGEFEEID == fixedCharge.CHARGEFEEID).FirstOrDefault()?.RECOMMENDED_FEERATEVALUE;
                var condValue = cond.Where(c => c.COLLATERALSUBTYPEID == rec.collateralSubTypeId).FirstOrDefault();
                var dutyCharge = rec.fixedDutyCharge;
                if (rec.fixedDutyCharge == null) dutyCharge = 0;
                rec.stampDutyAmount = (decimal)dutyCharge;
                rec.documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID;
                //rec.dutiableValue = condValue.DUTIABLEVALUE;
                rec.bookingDate = context.TBL_LOAN.Where(b => b.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId).FirstOrDefault()?.BOOKINGDATE;
                rec.collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(s => s.COLLATERALSUBTYPEID == rec.collateralSubTypeId).FirstOrDefault()?.COLLATERALSUBTYPENAME;

                if (rec.bookingDate != null)
                {
                    rec.maturityDate = context.TBL_LOAN.Where(b => b.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId).FirstOrDefault()?.MATURITYDATE;
                }
            }

            return record;

        }

        public FacilityStampDutyViewModel GetFacilityStampDutyById(int loanApplicationDetailId)
        {
            var cond = context.TBL_STAMP_DUTY_CONDITION.Where(c => c.DUTIABLEVALUE != null).ToList();
            var loanApplicationId = context.TBL_LOAN_APPLICATION_DETAIL.Where(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetailId).FirstOrDefault().LOANAPPLICATIONID;
            var loanApplication = context.TBL_LOAN_APPLICATION.Find(loanApplicationId);
            var record = (from x in context.TBL_FACILITY_STAMP_DUTY
                          join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                          join cl in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals cl.COLLATERALCUSTOMERID
                          where x.DELETED == false && x.LOANAPPLICATIONDETAILID == loanApplicationDetailId

                          select new FacilityStampDutyViewModel
                          {
                              facilityStampDutyId = x.FACILITYSTAMPDUTYID,
                              loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                              collateralCustomerId = x.COLLATERALCUSTOMERID,
                              osdc = x.OSDC,
                              applicationReferenceNumber = loanApplication.APPLICATIONREFERENCENUMBER,
                              dateTimeCreated = x.DATETIMECREATED,
                              isShared = x.ISSHARED,
                              currentstatus = x.CURRENTSTATUS,
                              customerPercentage = x.CUSTOMERPERCENTAGE,
                              bankPercentage = x.BANKPERCENTAGE,
                              customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.LASTNAME).FirstOrDefault(),
                              loanAmount = a.PROPOSEDAMOUNT,
                              approvedTenor = a.APPROVEDTENOR,
                              collateralSubTypeId = cl.COLLATERALSUBTYPEID,
                              asdc = x.ASDC,
                              csdc = x.CSDC,
                              collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(s => s.COLLATERALSUBTYPEID == cl.COLLATERALSUBTYPEID).FirstOrDefault().COLLATERALSUBTYPENAME,
                              customerId = a.CUSTOMERID,
                          }).FirstOrDefault();
            record.operationId = (int)OperationsEnum.StampDutyClosure;
            record.documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID;
            record.bookingDate = context.TBL_LOAN.Where(b => b.LOANAPPLICATIONDETAILID == record.loanApplicationDetailId).FirstOrDefault()?.BOOKINGDATE;
            var condValue = cond.Where(c => c.COLLATERALSUBTYPEID == record.collateralSubTypeId).FirstOrDefault();
            var dutyCharge = record.loanAmount * (condValue.DUTIABLEVALUE / 100);
            record.stampDutyAmount = dutyCharge;
            record.dutiableValue = condValue.DUTIABLEVALUE;
            if (record.bookingDate != null)
            {
                record.maturityDate = context.TBL_LOAN.Where(b => b.LOANAPPLICATIONDETAILID == record.loanApplicationDetailId).FirstOrDefault()?.MATURITYDATE;
            }
            if (record.currentstatus == 1)
            {
                record.status = "Not yet remitted";
            }
            else if( record.currentstatus == 2)
            {
                record.status = "Not yet remitted";
            }
            else { record.status = "Remitted"; }
            return record;

        }

        public bool AddFacilityStampDutySharing(FacilityStampDutyViewModel model)
        {
            try
            {
                var entity = context.TBL_FACILITY_STAMP_DUTY.Find(model.facilityStampDutyId);
                model.isShared = true;
                if (model.customerPercentage == 100) model.isShared = false;

                if (entity != null)
                {
                    entity.ISSHARED = model.isShared;
                    entity.CUSTOMERPERCENTAGE = model.customerPercentage;
                    entity.BANKPERCENTAGE = model.bankPercentage;
                    entity.DATETIMEUPDATED = DateTime.Now;
                    return context.SaveChanges() != 0;
                };
                context.SaveChanges();

                //var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
                //// Audit Section ---------------------------
                //this.audit.AddAuditTrail(new TBL_AUDIT
                //{
                //    AUDITTYPEID = (short)AuditTypeEnum.CashbackSectionAdded,
                //    STAFFID = model.createdBy,
                //    BRANCHID = (short)model.userBranchId,
                //    DETAIL = $"TBL_FACILITY_STAMP_DUTY '{entity.ToString()}' created by {auditStaff}",
                //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                //    URL = model.applicationUrl,
                //    APPLICATIONDATE = general.GetApplicationDate(),
                //    SYSTEMDATETIME = DateTime.Now,
                //    DEVICENAME = CommonHelpers.GetDeviceName(),
                //    OSNAME = CommonHelpers.FriendlyName(),
                //});
                //// Audit Section end ------------------------

                return context.SaveChanges() != 0;
            }
            catch(Exception e)
            {
                throw e;
            }
            
        }

        public IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDutyReport(DateRange param)
        {
            param.endDate = param.endDate.AddHours(23);
            param.endDate = param.endDate.AddMinutes(59);
            param.endDate = param.endDate.AddSeconds(59);
            var fixedCharge = context.TBL_CHARGE_FEE.Where(c=>c.CHARGEFEENAME.ToLower().Contains("(fixed)")).FirstOrDefault();
            var cond = context.TBL_STAMP_DUTY_CONDITION.Where(c => c.DUTIABLEVALUE != null).ToList();

            var record = (from x in context.TBL_FACILITY_STAMP_DUTY
                          join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                          join cl in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals cl.COLLATERALCUSTOMERID
                          where x.DELETED == false && ((DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                                 && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)))

                          select new FacilityStampDutyViewModel
                          {
                              facilityStampDutyId = x.FACILITYSTAMPDUTYID,
                              loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                              collateralCustomerId = x.COLLATERALCUSTOMERID,
                              osdc = x.OSDC,
                              asdc = x.ASDC,
                              csdc = x.CSDC,
                              dateTimeCreated = x.DATETIMECREATED,
                              isShared = x.ISSHARED,
                              customerPercentage = x.CUSTOMERPERCENTAGE,
                              bankPercentage = x.BANKPERCENTAGE,
                              customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.LASTNAME).FirstOrDefault(),
                              loanAmount = a.PROPOSEDAMOUNT,
                              approvedTenor = a.APPROVEDTENOR,
                              collateralSubTypeId = cl.COLLATERALSUBTYPEID,
                              collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(s => s.COLLATERALSUBTYPEID == cl.COLLATERALSUBTYPEID).FirstOrDefault().COLLATERALSUBTYPENAME,
                              customerId = a.CUSTOMERID,
                              operationId = (int)OperationsEnum.StampDutyClosure,
                              //documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID
                          }).ToList();
            foreach (var rec in record)
            {
                rec.fixedDutyCharge = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(f => f.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId && f.CHARGEFEEID == fixedCharge.CHARGEFEEID).FirstOrDefault()?.RECOMMENDED_FEERATEVALUE == null ? 0 : context.TBL_LOAN_APPLICATION_DETL_FEE.Where(f => f.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId && f.CHARGEFEEID == fixedCharge.CHARGEFEEID).FirstOrDefault()?.RECOMMENDED_FEERATEVALUE; var condValue = cond.Where(c => c.COLLATERALSUBTYPEID == rec.collateralSubTypeId).FirstOrDefault();
                var dutyCharge = (rec.loanAmount * (condValue.DUTIABLEVALUE / 100)) + rec.fixedDutyCharge;
                rec.stampDutyAmount = (decimal)dutyCharge;
                rec.documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID;
                
                rec.dutiableValue = condValue.DUTIABLEVALUE;
            }


            return record;

        }

        public IEnumerable<FacilityStampDutyViewModel> GetAllFacilityStampDutyFiltered(DateRange param)
        {
            param.endDate = param.endDate.AddHours(23);
            param.endDate = param.endDate.AddMinutes(59);
            param.endDate = param.endDate.AddSeconds(59);

            var cond = context.TBL_STAMP_DUTY_CONDITION.Where(c => c.DUTIABLEVALUE != null).ToList();
            var fixedCharge = context.TBL_CHARGE_FEE.Where(c => c.CHARGEFEENAME.ToLower().Contains("(fixed)")).FirstOrDefault();
            var record = (from x in context.TBL_FACILITY_STAMP_DUTY
                          join a in context.TBL_LOAN_APPLICATION_DETAIL on x.LOANAPPLICATIONDETAILID equals a.LOANAPPLICATIONDETAILID
                          join cl in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals cl.COLLATERALCUSTOMERID
                          where x.DELETED == false && x.CURRENTSTATUS == 2 
                          && ((DbFunctions.TruncateTime(x.DATETIMECREATED) >= DbFunctions.TruncateTime(param.startDate)
                                 && DbFunctions.TruncateTime(x.DATETIMECREATED) <= DbFunctions.TruncateTime(param.endDate)))

                          select new FacilityStampDutyViewModel
                          {
                              facilityStampDutyId = x.FACILITYSTAMPDUTYID,
                              loanApplicationDetailId = x.LOANAPPLICATIONDETAILID,
                              collateralCustomerId = x.COLLATERALCUSTOMERID,
                              osdc = x.OSDC,
                              asdc = x.ASDC,
                              csdc = x.CSDC,
                              dateTimeCreated = x.DATETIMECREATED,
                              isShared = x.ISSHARED,
                              customerPercentage = x.CUSTOMERPERCENTAGE,
                              bankPercentage = x.BANKPERCENTAGE,
                              customerName = context.TBL_CUSTOMER.Where(c => c.CUSTOMERID == a.CUSTOMERID).Select(c => c.FIRSTNAME + " " + c.LASTNAME).FirstOrDefault(),
                              loanAmount = a.PROPOSEDAMOUNT,
                              approvedTenor = a.APPROVEDTENOR,
                              collateralSubTypeId = cl.COLLATERALSUBTYPEID,
                              collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(s => s.COLLATERALSUBTYPEID == cl.COLLATERALSUBTYPEID).FirstOrDefault().COLLATERALSUBTYPENAME,
                              customerId = a.CUSTOMERID,
                              operationId = (int)OperationsEnum.StampDutyClosure,
                              //documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID
                          }).ToList();
            foreach (var rec in record)
            {
                rec.fixedDutyCharge = context.TBL_LOAN_APPLICATION_DETL_FEE.Where(f => f.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId && f.CHARGEFEEID == fixedCharge.CHARGEFEEID).FirstOrDefault()?.RECOMMENDED_FEERATEVALUE == null ? 0 : context.TBL_LOAN_APPLICATION_DETL_FEE.Where(f => f.LOANAPPLICATIONDETAILID == rec.loanApplicationDetailId && f.CHARGEFEEID == fixedCharge.CHARGEFEEID).FirstOrDefault()?.RECOMMENDED_FEERATEVALUE;
                var condValue = cond.Where(c => c.COLLATERALSUBTYPEID == rec.collateralSubTypeId).FirstOrDefault();
                var dutyCharge = (rec.loanAmount * (condValue.DUTIABLEVALUE / 100)) + rec.fixedDutyCharge;
                rec.stampDutyAmount = (decimal)dutyCharge;
                rec.documentTypeId = documentContext.TBL_DOCUMENT_TYPE.Where(d => d.DOCUMENTTYPENAME == "STAMP DUTY CERTIFICATE").FirstOrDefault().DOCUMENTTYPEID;
                rec.dutiableValue = condValue.DUTIABLEVALUE;
            }
            decimal totalDutyCharge = (decimal)record.Sum(x => x.stampDutyAmount);
            if (record.Count() > 0) record[0].totalDutyAmount = totalDutyCharge;

            return record;

        }

        public bool AddStampSetup(StampDutyConditionViewModel entity)
        {
            entity.tenor = ConvertTenorToDays(entity.tenor, entity.tenorModeId);
            try
            {
                var data = context.TBL_STAMP_DUTY_CONDITION.Add(new TBL_STAMP_DUTY_CONDITION
                {
                    COLLATERALSUBTYPEID = entity.collateralSubTypeId,
                    TENOR = entity.tenor,
                    USETENOR = entity.useTenor,
                    DUTIABLEVALUE = entity.dutiableValue,
                    ISPERCENTAGE = entity.isPercentage

                });

                context.SaveChanges();

                // Audit Section ---------------------------

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ChargeFeeAdded,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"{"Added Stamp duty condition setup"}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = data.CONDITIONID
                };

                this.auditTrail.AddAuditTrail(audit);
                //end of Audit section -------------------------------
            }
            catch (Exception e)
            {
                throw e;
            }

            return context.SaveChanges() != 0;
        }

        public IEnumerable<StampDutyConditionViewModel> GetStampSetup()
        {
            try
            {
                var data = (from a in context.TBL_STAMP_DUTY_CONDITION
                            where a.DUTIABLEVALUE != null
                            select new StampDutyConditionViewModel
                            {
                                conditionId = a.CONDITIONID,
                                collateralSubTypeId = a.COLLATERALSUBTYPEID,
                                dutiableValue = a.DUTIABLEVALUE,
                                collateralSubType = context.TBL_COLLATERAL_TYPE_SUB.Where(s => s.COLLATERALSUBTYPEID == a.COLLATERALSUBTYPEID).FirstOrDefault().COLLATERALSUBTYPENAME,
                                tenor = a.TENOR,
                                useTenor = a.USETENOR,

                            }).ToList();
                return data;
            }
            catch (Exception e)
            {
                throw e;
            }


        }

        public bool UpdateStampSetup(int conditionId, StampDutyConditionViewModel entity)
        {
            try
            {

                var tat = context.TBL_STAMP_DUTY_CONDITION.FirstOrDefault(x => x.CONDITIONID == conditionId);
                if (tat == null) return false;

                tat.COLLATERALSUBTYPEID = entity.collateralSubTypeId;
                tat.TENOR = entity.tenor;
                tat.USETENOR = entity.useTenor;
                tat.DUTIABLEVALUE = entity.dutiableValue;
                //tat.DATETIMEUPDATED = _genSetup.GetApplicationDate();


                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ChargeFeeUpdated,
                    STAFFID = entity.createdBy,
                    BRANCHID = (short)entity.userBranchId,
                    DETAIL = $"{"Updated stamp duty condition"}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = entity.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -----------------------

            }
            catch (Exception e)
            {
                throw e;
            }
            return context.SaveChanges() != 0;
        }

        public bool DeleteStampSetup(int conditionId, UserInfo user)
        {
            try
            {

                var tat = context.TBL_STAMP_DUTY_CONDITION.FirstOrDefault(x => x.CONDITIONID == conditionId);
                if (tat == null) return false;

                context.TBL_STAMP_DUTY_CONDITION.Remove(tat);

                context.SaveChanges();
                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ChargeFeeDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"{"Deleted Stamp duty condition"}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName(),
                    APPLICATIONDATE = genSetup.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };

                this.auditTrail.AddAuditTrail(audit);

                //end of Audit section -----------------------

            }
            catch (Exception e)
            {
                throw e;
            }
            return context.SaveChanges() > 0;
        }

    }

}

public class API_Error
{
    public string error { get; set; }
    public string errorDescription { get; set; }
}


