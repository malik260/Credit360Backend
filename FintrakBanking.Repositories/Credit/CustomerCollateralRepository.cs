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

namespace FintrakBanking.Repositories.Credit
{
    public class CustomerCollateralRepository : ICustomerCollateralRepository
    {
        private FinTrakBankingContext context;
        private FinTrakBankingContext delContext;
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

        public CustomerCollateralRepository(
            FinTrakBankingContext _context,
            FinTrakBankingContext _delContext,
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
            IIntegrationWithFinacle _finacle
            )
        {
            this.context = _context;
            this.genSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.product = _product;
            this.media = _media;
            this.collateralType = _collateralType;
            this.workflow = workflow;
            this.documentContext = _documentContext;
            this.delContext = _delContext;
            this.repo = _repo;
            this.level = _level;
            this.lien = _lien;
            this.casa = _casa;
            this.finacle = _finacle;
        }



        #region New 

        // ADD
        [OperationBehavior(TransactionScopeRequired = true)]
        public int AddCollateral(CollateralViewModel entity, byte[] file) //, 
        {
                int collateralId = AddTempCollateralMainForm(entity);

                if (collateralId > 0)
                {
                    switch (entity.collateralTypeId)
                    {
                        case (int)CollateralTypeEnum.TermDeposit: AddTempDepositCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.PlantAndMachinery: AddTempEquipmentCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.Miscellaneous: AddTempMiscellaneousCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.Gaurantee: AddTempGuaranteeCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.CASA: AddTempCasaCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.Property: AddTempImmovablePropertyCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.MarketableSecurities: AddTempMarketableSecuritiesCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.InsurancePolicy: AddTempPolicyCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.PreciousMetal: AddTempPreciousMetalCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.Stock: AddTempStockCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.Vehicle: AddVehicleCollateral(collateralId, entity); break;
                        case (int)CollateralTypeEnum.Promissory: AddPromissoryCollateral(collateralId, entity); break;

                        default: break;
                    }

                    if (entity.hasInsurance) { AddTempItemInsurancePolicy(collateralId, entity); }

                    if (file != null) { SaveCollateralMainDocument(entity, collateralId, file); }

                    bool saved;
                    try
                    {
                        saved = context.SaveChanges() != 0;
                    }
                    catch (Exception ex)
                    {

                        throw new SecureException("Error has occured while creating this collateral");
                    }
                    if (saved) { return collateralId; }

                
            }

            return 0;
        }

        // UPDATE
        public IQueryable<CollateralViewModel> GetCollateralReleaseAwaitingApproval(int companyId, int staffId)
        {
            var ids1 = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.FinalCollateralRelease).ToList();
            var ids2 = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.TemporalCollateralRelease).ToList();
            var ids = ids1.Union(ids2);
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
                              customerId = b.CUSTOMERID,
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

        public IQueryable<CollateralViewModel> GetCollateralReleaseAwaitingJobRequest(int companyId, int branchId)
        {
            var record = (from a in context.TBL_COLLATERAL_RELEASE
                          join b in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals b.COLLATERALCUSTOMERID
                          join c in context.TBL_STAFF on a.CREATEDBY equals c.STAFFID 
                          where b.COMPANYID == companyId && a.JOBREQUESTSENT== false && c.BRANCHID == branchId
                          select new CollateralViewModel
                          {
                              collateralCustomerId = a.COLLATERALCUSTOMERID,
                              collateralCode = b.COLLATERALCODE,
                              customerName = context.TBL_CUSTOMER.Where(q=>q.CUSTOMERID==b.CUSTOMERID).Select(w=>w.FIRSTNAME + " " + w.MIDDLENAME + " " + w.LASTNAME ).FirstOrDefault(),
                              collateralReleaseId = a.COLLATERALRELEASEID,
                              collateralReleaseTypeId = a.COLLATERALRELEASETYPEID,
                              collateralReleaseTypeName = context.TBL_COLLATERAL_RELEASE_TYPE.Where(q => q.COLLATERALRELEASETYPEID == a.COLLATERALRELEASETYPEID).Select(r=>r.COLLATERALRELEASETYPENAME).FirstOrDefault(),
                              jobRequestSent = a.JOBREQUESTSENT == null ? false : a.JOBREQUESTSENT,
                          });

            return record;
        }

        private void ValidateJobRequestCheck(TBL_COLLATERAL_RELEASE collateralRelease)
        {
            var record = (from a in context.TBL_JOB_REQUEST
                          join b in context.TBL_COLLATERAL_RELEASE on a.TARGETID equals b.COLLATERALCUSTOMERID
                          where a.TARGETID == collateralRelease.COLLATERALCUSTOMERID
                          select new  { a
                          } ).ToList();
            if(record.Count != 0)
            {
                var legal = record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID == (short)JobTypeEnum.legal).ToList();
                var middleOfficeVerification = record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification).ToList();
                if (legal.Count != 0)
                {

                }
                else
                {
                    throw new ConditionNotMetException("Kindly Complete The Legal Job Request Before You Proceed");
                }

                if (middleOfficeVerification.Count != 0)
                {

                }
                 else{
                    throw new ConditionNotMetException("Kindly Complete The Middle Office Job Request Before You Proceed");
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

                if (record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.disapproved).Any())
                    throw new ConditionNotMetException("There are unapproved middle office request.");

                if (record.Where(x => x.a.TARGETID == collateralRelease.COLLATERALCUSTOMERID && x.a.JOBTYPEID == (short)JobTypeEnum.middleOfficeVerification && (x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.pending || x.a.REQUESTSTATUSID == (short)JobRequestStatusEnum.processing)).Any())
                    throw new ConditionNotMetException("There are pending middle office job request for this request..");


            //}
        }

        public bool ReleaseCollateralJobRequest(CollateralViewModel entity)
        {
            var release = context.TBL_COLLATERAL_RELEASE.Where(a=>a.COLLATERALRELEASEID == entity.collateralReleaseId).FirstOrDefault();

            ValidateJobRequestCheck(release);

            PendingJobRequestCheck(release);


           // var collateral = context.TBL_COLLATERAL_CUSTOMER.Find(entity.collateralId);
            release.JOBREQUESTSENT = true;

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseAction,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Collateral Release Action '{ context.TBL_COLLATERAL_CUSTOMER.Find(release.COLLATERALCUSTOMERID).COLLATERALCODE }' Job Request Done ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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

            var validate = context.TBL_COLLATERAL_RELEASE.Where(a => a.COLLATERALRELEASEID == collateral.COLLATERALCUSTOMERID && (a.APPROVALSTATUSID == (int)ApprovalStatusEnum.Pending || a.APPROVALSTATUSID== (int)ApprovalStatusEnum.Processing)).FirstOrDefault();
            if (validate != null)
            {
                throw new ConditionNotMetException("Collateral is Already Undergoing Approval For Release.");
            }

            TBL_COLLATERAL_RELEASE release = new TBL_COLLATERAL_RELEASE();
            release.DESCRIPTION = entity.comment;
            release.COLLATERALRELEASETYPEID = (int)entity.releaseType;
            release.COLLATERALCUSTOMERID = collateral.COLLATERALCUSTOMERID;
            release.APPROVALSTATUSID = (int)ApprovalStatusEnum.Pending;
            release.CREATEDBY = entity.createdBy;
            release.DATETIMECREATED = DateTime.Now.Date;
            release.JOBREQUESTSENT = false;

            context.TBL_COLLATERAL_RELEASE.Add(release);
            var saved = context.SaveChanges() > 0;
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CollateralReleaseAction,
                STAFFID = entity.createdBy,
                BRANCHID = (short)entity.userBranchId,
                DETAIL = $"Collateral Release Action '{ context.TBL_COLLATERAL_CUSTOMER.Find(release.COLLATERALCUSTOMERID).COLLATERALCODE }' ",
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);

            if (saved)
            {


                return context.SaveChanges() > 0;

            } // audit here

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

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added Release Document with Collateral Code : '{ model.collateralCode }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.auditTrail.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() == 0 ? 1 : 2;
        }

        public IEnumerable<CollateralViewModel> GetCollateralReleaseDocument(int releaseId)
        {
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
                            approvalStatus = (int)x.APPROVALSTATUSID,
                            //systemDateTime = x.SYSTEMDATETIME,
                        }).ToList();

            return data;
        }

        public CollateralViewModel GetReleaseSupportingDocument(int documentId)
        {
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
                            approvalStatus = (int)x.APPROVALSTATUSID,

                        });

            return data.FirstOrDefault();
        }


        public async Task<bool> UpdateCollateral(CollateralViewModel entity, int collateralId)
        {
            UpdateCollateralMainForm(entity, collateralId);

            switch (entity.collateralTypeId)
            {
                // case (int)CollateralTypeEnum.TermDeposit: UpdateDepositCollateral(entity); break;
                case (int)CollateralTypeEnum.PlantAndMachinery: UpdateEquipmentCollateral(entity); break;
                // case (int)CollateralTypeEnum.Miscellaneous: UpdateMiscellaneousCollateral(entity); break;
                case (int)CollateralTypeEnum.Gaurantee: UpdateGuaranteeCollateral(entity); break;
                case (int)CollateralTypeEnum.CASA: UpdateCasaCollateral(entity); break;
                case (int)CollateralTypeEnum.Property: UpdateImmovablePropertyCollateral(entity); break;
                case (int)CollateralTypeEnum.MarketableSecurities: UpdateMarketableSecuritiesCollateral(entity); break;
                case (int)CollateralTypeEnum.InsurancePolicy: UpdatePolicyCollateral(entity); break;
                case (int)CollateralTypeEnum.PreciousMetal: UpdatePreciousMetalCollateral(entity); break;
                case (int)CollateralTypeEnum.Stock: UpdateStockCollateral(entity); break;
                case (int)CollateralTypeEnum.Vehicle: UpdateVehicleCollateral(entity); break;
                case (int)CollateralTypeEnum.Promissory: UpdatePromissoryCollateral(entity); break;

                default: break;
            }

            if (entity.hasInsurance) { UpdateItemInsurancePolicy(entity); }

            bool saved = await context.SaveChangesAsync() != 0;

            if (saved) { return true; } // audit here

            return false;
        }

        public async Task<bool> GetCollateral(CollateralViewModel entity, int collateralId)
        {
            UpdateCollateralMainForm(entity, collateralId);

            switch (entity.collateralTypeId)
            {
                //  case (int)CollateralTypeEnum.TermDeposit: UpdateDepositCollateral(entity); break;
                case (int)CollateralTypeEnum.PlantAndMachinery: UpdateEquipmentCollateral(entity); break;
                //  case (int)CollateralTypeEnum.Miscellaneous: UpdateMiscellaneousCollateral(entity); break;
                case (int)CollateralTypeEnum.Gaurantee: UpdateGuaranteeCollateral(entity); break;
                case (int)CollateralTypeEnum.CASA: UpdateCasaCollateral(entity); break;
                case (int)CollateralTypeEnum.Property: UpdateImmovablePropertyCollateral(entity); break;
                case (int)CollateralTypeEnum.MarketableSecurities: UpdateMarketableSecuritiesCollateral(entity); break;
                case (int)CollateralTypeEnum.InsurancePolicy: UpdatePolicyCollateral(entity); break;
                case (int)CollateralTypeEnum.PreciousMetal: UpdatePreciousMetalCollateral(entity); break;
                case (int)CollateralTypeEnum.Stock: UpdateStockCollateral(entity); break;
                case (int)CollateralTypeEnum.Vehicle: UpdateVehicleCollateral(entity); break;
                case (int)CollateralTypeEnum.Promissory: UpdatePromissoryCollateral(entity); break;

                default: break;
            }

            if (entity.hasInsurance) { UpdateItemInsurancePolicy(entity); }

            bool saved = await context.SaveChangesAsync() != 0;

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
            collateral.COLLATERALTYPEID = model.collateralTypeId;
            collateral.COLLATERALSUBTYPEID = model.collateralSubTypeId;
            collateral.COLLATERALCODE = model.collateralCode;
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
        }

        private void DeleteCollateral(int collateralId)
        {
            var collateral = delContext.TBL_TEMP_COLLATERAL_CUSTOMER.Find(collateralId);
            collateral.DELETED = true; // audit here
            if (collateral != null)
            {
                delContext.SaveChanges();
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

            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralId;
            workflow.Comment = "Request for plant and equipment collateral approval";
            workflow.OperationId = (int)OperationsEnum.CollateralApproval;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();
        }

        private void UpdateEquipmentCollateral(CollateralViewModel model)
        {
            var collateral = context.TBL_COLLATERAL_PLANT_AND_EQUIP
                .Where(x => x.COLLATERALCUSTOMERID == model.collateralId)
                .FirstOrDefault();

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
            var collateral = context.TBL_TEMP_COLLATERAL_MISCELLAN.Add(new TBL_TEMP_COLLATERAL_MISCELLAN
            {
                TEMPCOLLATERALCUSTOMERID = collateralId,
                NAMEOFSECURITY = entity.securityName,
                SECURITYVALUE = (decimal)entity.securityValue,
                NOTE = entity.note,
            });

            //if (context.SaveChanges() > 0) // EF will take care of this
            AddMiscellaneousNotes(entity, collateral.TEMPCOLLATERALMISCELLANEOUSID);
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
                //context.SaveChanges();
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
            context.TBL_TEMP_COLLATERAL_ITEM_POLI.Add(new TBL_TEMP_COLLATERAL_ITEM_POLI
            {
                COLLATERALCUSTOMERID = collateralId,
                POLICYREFERENCENUMBER = entity.referenceNumber,
                INSURANCECOMPANYNAME = entity.insuranceCompany,
                SUMINSURED = entity.sumInsured,
                STARTDATE = (DateTime)entity.startDate,
                ENDDATE = (DateTime)entity.expiryDate,
                INSURANCETYPE = entity.insuranceType,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                DELETED = false

            });
        }

        public bool AddNewItemInsurancePolicy(InsurancePolicies entity)
        {
            var policy = context.TBL_TEMP_COLLATERAL_ITEM_POLI.Add(new TBL_TEMP_COLLATERAL_ITEM_POLI
            {
                COLLATERALCUSTOMERID = entity.collateraalId,
                POLICYREFERENCENUMBER = entity.referenceNumber,
                INSURANCECOMPANYNAME = entity.insuranceCompany,
                SUMINSURED = entity.sumInsured,
                STARTDATE = (DateTime)entity.startDate,
                ENDDATE = (DateTime)entity.expiryDate,
                INSURANCETYPE = entity.insuranceType,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                ISPOLICYAPPROVAL = true,
                DELETED = false

            });

            if (context.SaveChanges() > 0)
            {
                workflow.StaffId = entity.createdBy;
                workflow.CompanyId = entity.companyId;
                workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                workflow.TargetId = policy.TEMPPOLICYID;
                workflow.Comment = "Request for item policy approval";
                workflow.OperationId = (int)OperationsEnum.ItemPolicyApproval;
                workflow.DeferredExecution = false; // false by default will call the internal SaveChanges()
                workflow.ExternalInitialization = true;
                workflow.LogActivity();

                return true;
            }
            return false;

        }
        public bool SaveCollateralMainDocument(CollateralViewModel model, int collateralId, byte[] file)
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
            catch(Exception ex) { }

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
                collateral.INSURANCECOMPANYNAME = entity.insuranceCompany;
                collateral.SUMINSURED = entity.sumInsured;
                collateral.STARTDATE = (DateTime)entity.startDate;
                collateral.ENDDATE = (DateTime)entity.expiryDate;
                collateral.INSURANCETYPE = entity.insuranceType;
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
                        customerId = c.c.CUSTOMERID,
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
                        approvalStatus = c.c.APPROVALSTATUS,
                        //allowApplicationMapping = typeIds.Contains((short)c.c.COLLATERALTYPEID),
                        requireInsurancePolicy = c.c.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                        exchangeRate = c.c.EXCHANGERATE,
                        availableValue = 0,
                        accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == c.c.CUSTOMERID).ACCOUNTNUMBER,
                    }).FirstOrDefault()                    
                    ;
            decimal usage = 0;
            var mappings = context.TBL_LOAN_COLLATERAL_MAPPING.Where(m => m.COLLATERALCUSTOMERID == collaterals.collateralId && m.DELETED == false && m.ISRELEASED == false).ToList();
            foreach (var mapping in mappings)
            {
                usage = usage + GetLoanOutstandingBalance(mapping.LOANID, mapping.LOANSYSTEMTYPEID);
            }
            collaterals.availableValue = (decimal)collaterals.collateralValue - usage;


            //collaterals = ResolveCollateralValues(collaterals);

            //var count = collaterals.Count();
            //var test = collaterals;

            return collaterals;
        }

        public IEnumerable<CollateralViewModel> GetCustomerCollateral(int customerId, int? applicationId, int companyId)
        {
            var typeIds = new List<int>();
            var company = context.TBL_COMPANY.Find(companyId);
            bool disAllowCollateral = false;
            bool isForiegnCurrencyFacility = false;

            if (applicationId != null)
            {
                var productIds = context.TBL_LOAN_APPLICATION_DETAIL
                    .Where(x => x.LOANAPPLICATIONID == applicationId)
                    .Select(x => x.PROPOSEDPRODUCTID)
                    .Distinct();

                typeIds = context.TBL_PRODUCT_COLLATERALTYPE.Where(x => productIds.Contains(x.PRODUCTID))
                   .Select(x => x.COLLATERALTYPEID)
                   .Distinct().ToList();

                isForiegnCurrencyFacility = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.CURRENCYID != company.CURRENCYID).Any();
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
                        currencyId = c.c.CURRENCYID,
                        baseCurrencyId = company.CURRENCYID,
                        currency = c.c.TBL_CURRENCY.CURRENCYNAME,
                        disAllowCollateral = disAllowCollateral && c.c.CURRENCYID == company.CURRENCYID, // facilityCurrency != baseCurrency && collateralCurrency == baseCurrency
                        collateralTypeName = c.c.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                        collateralSubTypeName =  context.TBL_COLLATERAL_TYPE_SUB.Where(r=>r.COLLATERALSUBTYPEID == c.c.COLLATERALSUBTYPEID).Select(q=>q.COLLATERALSUBTYPENAME).FirstOrDefault(),
                        collateralCode = c.c.COLLATERALCODE,
                        collateralValue = c.c.COLLATERALVALUE,
                        camRefNumber = c.c.CAMREFNUMBER,
                        allowSharing = c.c.ALLOWSHARING,
                        isLocationBased = (bool)c.c.ISLOCATIONBASED,
                        valuationCycle = c.c.VALUATIONCYCLE,
                        haircut = c.c.HAIRCUT,
                        approvalStatus = c.c.APPROVALSTATUS,
                        allowApplicationMapping = typeIds.Contains((short)c.c.COLLATERALTYPEID),
                        requireInsurancePolicy = c.c.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                        exchangeRate = c.c.EXCHANGERATE,
                        availableValue = 0,
                        accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,
        })
                    .ToList()
                    .GroupBy(x => x.collateralId).Select(g => g.First())
                    ;

            collaterals = ResolveCollateralValues(collaterals.ToList());

            //var count = collaterals.Count();
            //var test = collaterals;

            return collaterals;
        }

        public IEnumerable<CollateralViewModel> GetCustomerCollateralReport(string searchParam,  int companyId)
        {
            var typeIds = new List<int>();
            var company = context.TBL_COMPANY.Find(companyId);
            bool disAllowCollateral = false;
            bool isForiegnCurrencyFacility = false;


            var collaterals = (from a in context.TBL_LOAN_COLLATERAL_MAPPING
                       join l in context.TBL_LOAN on a.LOANID equals l.TERMLOANID
                       join b in context.TBL_LOAN_APPLICATION_DETAIL on l.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                       join c in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals c.LOANAPPLICATIONID
                       join d in context.TBL_COLLATERAL_CUSTOMER on a.COLLATERALCUSTOMERID equals d.COLLATERALCUSTOMERID
                       join cus in context.TBL_CUSTOMER on d.CUSTOMERID equals cus.CUSTOMERID
                       where ((cus.CUSTOMERCODE == searchParam) || (cus.FIRSTNAME == searchParam) ||  (l.LOANREFERENCENUMBER == searchParam) )

                             
            select new CollateralViewModel
                       {
                           collateralId = a.COLLATERALCUSTOMERID,
                           collateralTypeId = d.COLLATERALTYPEID,
                           collateralSubTypeId = d.COLLATERALSUBTYPEID,
                           customerId = d.CUSTOMERID,
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
                           isLocationBased = (bool)d.ISLOCATIONBASED,
                           valuationCycle = d.VALUATIONCYCLE,
                           haircut = d.HAIRCUT,
                           approvalStatus = d.APPROVALSTATUS,
                           allowApplicationMapping = typeIds.Contains((short)d.COLLATERALTYPEID),
                           requireInsurancePolicy = d.TBL_COLLATERAL_TYPE.REQUIREINSURANCEPOLICY,
                           exchangeRate = d.EXCHANGERATE,
                           availableValue = 0,
                           // accountNumber = context.TBL_COLLATERAL_CASA.FirstOrDefault(x => x.COLLATERALCUSTOMERID == customerId).ACCOUNTNUMBER,



                       }).ToList().GroupBy(x => x.collateralId).Select(g => g.First());

            collaterals = ResolveCollateralValues(collaterals.ToList()); ;


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
        private List<CollateralViewModel> ResolveCollateralValues(List<CollateralViewModel> collaterals)
        {
            decimal usage;
            List<CollateralViewModel> list = new List<CollateralViewModel>();
            foreach (var collateral in collaterals)
            {
                usage = 0;
                var mappings = context.TBL_LOAN_COLLATERAL_MAPPING.Where(m => m.COLLATERALCUSTOMERID == collateral.collateralId && m.DELETED == false && m.ISRELEASED == false).ToList();
                foreach (var mapping in mappings) usage = usage + GetLoanOutstandingBalance(mapping.LOANID,mapping.LOANSYSTEMTYPEID);
                collateral.availableValue = (decimal)collateral.collateralValue - usage;
                list.Add(collateral);
            }
            return list;
        }

        private decimal GetLoanOutstandingBalance(int loanId, int loanSystemTypeId)
        {
            decimal balance = 0;
            switch (loanSystemTypeId)
            {
                case (int)LoanSystemTypeEnum.TermDisbursedFacility :
                    balance = context.TBL_LOAN.Where(x => x.TERMLOANID == loanId).Sum(x => x.OUTSTANDINGPRINCIPAL);
                    break;
                case (int)LoanSystemTypeEnum.OverdraftFacility :
                    balance = context.TBL_LOAN_REVOLVING.Where(x => x.REVOLVINGLOANID == loanId).Sum(x => x.OVERDRAFTLIMIT);
                    break;
                case  (int)LoanSystemTypeEnum.ContingentLiability :
                    balance = context.TBL_LOAN_CONTINGENT.Where(x => x.CONTINGENTLOANID == loanId).Sum(x => x.CONTINGENTAMOUNT);
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
                customerId = x.CUSTOMERID,
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
                approvalStatus = x.APPROVALSTATUS,
                //collateralValue = x.CollateralValue
                exchangeRate = x.EXCHANGERATE

            })
            .OrderByDescending(x => x.collateralId)

            .ToList();



            return collateral;
        }



        // GET TYPE SPICIFIC & INSURANCE DETAILS

        public CollateralViewModel GetCollateralTypeByCollateralId(int collateralId, int typeId)
        {
            var data = new CollateralViewModel();
            switch (typeId)
            {
                case (int)CollateralTypeEnum.TermDeposit: data = GetCollateralDeposit(collateralId); break;
                case (int)CollateralTypeEnum.PlantAndMachinery: data = GetCollateralMachinery(collateralId); break;
                case (int)CollateralTypeEnum.Miscellaneous: data = GetCollateralMiscellaneous(collateralId); break;
                case (int)CollateralTypeEnum.Gaurantee: data = GetCollateralGuarantee(collateralId); break;
                case (int)CollateralTypeEnum.CASA: data = GetCollateralCasa(collateralId); break;
                case (int)CollateralTypeEnum.Property: data = GetCollateralImmovableProperty(collateralId); break;
                case (int)CollateralTypeEnum.MarketableSecurities: data = GetCollateralMarketableSecurities(collateralId); break;
                case (int)CollateralTypeEnum.InsurancePolicy: data = GetCollateralPolicy(collateralId); break;
                case (int)CollateralTypeEnum.PreciousMetal: data = GetCollateralPreciousMetal(collateralId); break;
                case (int)CollateralTypeEnum.Stock: data = GetCollateralStock(collateralId); break;
                case (int)CollateralTypeEnum.Vehicle: data = GetCollateralVehicle(collateralId); break;
                case (int)CollateralTypeEnum.Promissory: data = GetCollateralPromissory(collateralId); break;

                default:
                    break;
            }

            return data;
        }
        private CollateralViewModel GetCollateralMiscellaneous(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_MISCELLANEOUS.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
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
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralDepositId = specifics.COLLATERALDEPOSITID,
                dealReferenceNumber = specifics.DEALREFERENCENUMBER,
                accountNumber = specifics.ACCOUNTNUMBER,
                existingLienAmount = specifics.EXISTINGLIENAMOUNT,
                lienAmount = specifics.LIENAMOUNT,
                availableBalance = specifics.AVAILABLEBALANCE,
                securityValue = specifics.SECURITYVALUE,
                maturityDate = specifics.MATURITYDATE,
                maturityAmount = specifics.MATURITYAMOUNT,
                remark = specifics.REMARK,
                accountName = specifics.ACCOUNTNAME,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }
        private CollateralViewModel GetCollateralInsurancePolicy(CollateralViewModel details)
        {
            var insurance = context.TBL_COLLATERAL_ITEM_POLICY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == details.collateralId);
            if (insurance != null)
            {
                details.referenceNumber = insurance.POLICYREFERENCENUMBER;
                details.insuranceCompany = insurance.INSURANCECOMPANYNAME;
                details.sumInsured = insurance.SUMINSURED;
                details.startDate = insurance.STARTDATE;
                details.expiryDate = insurance.ENDDATE;
                details.insuranceType = insurance.INSURANCETYPE;
                details.policyId = insurance.POLICYID;


            }
            return details;

        }

        public List<InsurancePolicies> GetCollateralInsurancePolicies(int collateralId)
        {
            var insurance = context.TBL_COLLATERAL_ITEM_POLICY.Where(x => x.COLLATERALCUSTOMERID == collateralId)
                .Select(i => new InsurancePolicies
                {
                    referenceNumber = i.POLICYREFERENCENUMBER,
                    insuranceCompany = i.INSURANCECOMPANYNAME,
                    sumInsured = i.SUMINSURED,
                    startDate = i.STARTDATE,
                    expiryDate = i.ENDDATE,
                    insuranceType = i.INSURANCETYPE,
                    hasExpired = i.HASEXPIRED
                }).ToList();
            return insurance;
        }
        public List<InsurancePolicies> GetTempCollateralInsurancePoliciesWaitingForApproval(int staffId)
        {
            var ids = genSetup.GetStaffApprovalLevelIds(staffId, (int)OperationsEnum.ItemPolicyApproval).ToList();

            var insurance = (from x in context.TBL_TEMP_COLLATERAL_ITEM_POLI
                             join s in context.TBL_COLLATERAL_CUSTOMER on x.COLLATERALCUSTOMERID equals s.COLLATERALCUSTOMERID
                             join atrail in context.TBL_APPROVAL_TRAIL on x.TEMPPOLICYID equals atrail.TARGETID
                             join a in context.TBL_COLLATERAL_TYPE on s.COLLATERALTYPEID equals a.COLLATERALTYPEID
                             join c in context.TBL_CUSTOMER on s.CUSTOMERID equals c.CUSTOMERID
                             join b in context.TBL_COLLATERAL_TYPE_SUB on s.COLLATERALSUBTYPEID equals b.COLLATERALSUBTYPEID
                             where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing
                                     && atrail.OPERATIONID == (int)OperationsEnum.ItemPolicyApproval
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                                     && x.ISPOLICYAPPROVAL == true
                                     && atrail.RESPONSESTAFFID == null
                             select new InsurancePolicies
                             {
                                 referenceNumber = x.POLICYREFERENCENUMBER,
                                 insuranceCompany = x.INSURANCECOMPANYNAME,
                                 sumInsured = x.SUMINSURED,
                                 startDate = x.STARTDATE,
                                 expiryDate = x.ENDDATE,
                                 insuranceType = x.INSURANCETYPE,
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
        // stock collateral

        private void AddTempStockCollateral(int collateralId, CollateralViewModel entity)
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

        private void UpdateStockCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_STOCK
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

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
                collateralStockId = specifics.COLLATERALSTOCKID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                companyName = specifics.COMPANYNAME,
                company = context.TBL_STOCK_COMPANY.Where(q=>q.STOCKID.ToString() == specifics.COMPANYNAME).Select(y=>y.STOCKNAME).FirstOrDefault(),

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
            var promissoryExist = context.TBL_COLLATERAL_PROMISSORY.Where(a => a.PROMISSORYNOTEID == entity.promissoryNoteRefferenceNumber).FirstOrDefault();
            if (promissoryExist !=null)
            {
                throw new ConditionNotMetException("Promisory Note Has Already Been Used Before.");
            }


            //if (entity.valuationDate > DateTime.Now || entity.dateOfManufacture > DateTime.Now)
            //    throw new SecureException("Wrong date selected. Transaction aborted");

            context.TBL_TEMP_COLLATERAL_PROMISSORY.Add(new TBL_TEMP_COLLATERAL_PROMISSORY
            {
                TEMPCOLLATERALCUSTOMERID = collateralId,
                TEMPCOLLATERALPROMISSORYID = entity.collateralPromissoryId,
                PROMISSORYNOTEID = entity.promissoryNoteRefferenceNumber,
                //PROMISSORYVALUE = entity.promissoryValue,
                EFFECTIVEDATE = entity.promissoryEffectiveDate,
                MATURITYDATE = entity.promissoryMaturityDate,
                
            });
            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralId;
            workflow.Comment = "Request for promissory collateral approval";
            workflow.OperationId = (int)OperationsEnum.CollateralApproval;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();
        }

        private void UpdatePromissoryCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_PROMISSORY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

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

            //if (entity.valuationDate > DateTime.Now || entity.dateOfManufacture > DateTime.Now)
            //    throw new SecureException("Wrong date selected. Transaction aborted");

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
            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralId;
            workflow.Comment = "Request for vehicle collateral approval";
            workflow.OperationId = (int)OperationsEnum.CollateralApproval;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();
        }

        private void UpdateVehicleCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_VEHICLE
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

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

        private CollateralViewModel GetCollateralVehicle(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_VEHICLE.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralVehicleId = specifics.COLLATERALVEHICLEID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
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
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // preciousMetal collateral

        private void AddTempPreciousMetalCollateral(int collateralId, CollateralViewModel entity)
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

            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralId;
            workflow.Comment = "Request for precious metal collateral approval";
            workflow.OperationId = (int)OperationsEnum.CollateralApproval;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();
        }

        private void UpdatePreciousMetalCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_PRECIOUSMETAL
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

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
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                accountNumber = specifics.ACCOUNTNUMBER,
                //  isOwnedByCustomer = specifics.ISOWNEDBYCUSTOMER,
                availableBalance = specifics.AVAILABLEBALANCE,
                //  existingLienAmount = specifics.EXISTINGLIENAMOUNT,
                lienAmount = specifics.LIENAMOUNT,
                securityValue = specifics.SECURITYVALUE,
                remark = specifics.REMARK,
                accountName = specifics.ACCOUNTNAME,

            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }

        // guarantee collateral

        private void AddTempGuaranteeCollateral(int collateralId, CollateralViewModel entity)
        {
            //if (entity.cStartDate > DateTime.Now || entity.endDate  < DateTime.Now )
            //    throw new SecureException("Wrong date selected. Transaction aborted");

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
            });

            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralId;
            workflow.Comment = "Request for guarantee collateral approval";
            workflow.OperationId = (int)OperationsEnum.CollateralApproval;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();
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
            var collateral = context.TBL_COLLATERAL_IMMOVE_PROPERTY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.PROPERTYNAME = entity.propertyName;
            collateral.CITYID = (int)entity.cityId;
            collateral.COUNTRYID = entity.countryId;
            collateral.CONSTRUCTIONDATE = entity.constructionDate;
            collateral.PROPERTYADDRESS = entity.propertyAddress;
            collateral.DATEOFACQUISITION = entity.dateOfAcquisition;
            collateral.LASTVALUATIONDATE = entity.lastValuationDate;
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
            
        }

        private CollateralViewModel GetCollateralImmovableProperty(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_IMMOVE_PROPERTY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
                collateralPropertyId = specifics.COLLATERALPROPERTYID,
                collateralCustomerId = specifics.COLLATERALCUSTOMERID,
                propertyName = specifics.PROPERTYNAME,
                cityId = specifics.CITYID,
                cityName = specifics.TBL_CITY.CITYNAME,
                countryId = specifics.COUNTRYID,
                countryName = context.TBL_COUNTRY.Where(q=>q.COUNTRYID == specifics.COUNTRYID).Select(r=>r.NAME).FirstOrDefault(),
                constructionDate = specifics.CONSTRUCTIONDATE,
                propertyAddress = specifics.PROPERTYADDRESS,
                dateOfAcquisition = specifics.DATEOFACQUISITION,
                lastValuationDate = specifics.LASTVALUATIONDATE,
                valuerId = specifics.VALUERID,
                collateralValuer = context.TBL_COLLATERAL_VALUER.Where(t=>t.COLLATERALVALUERID == specifics.VALUERID).Select(q=>q.NAME).FirstOrDefault(),
                valuerReferenceNumber = specifics.VALUERREFERENCENUMBER,
                propertyValueBaseTypeId = specifics.PROPERTYVALUEBASETYPEID,
                propertyValueBaseTypeName = context.TBL_COLLATERAL_VALUEBASE_TYPE.Where(t => t.COLLATERALVALUEBASETYPEID == specifics.PROPERTYVALUEBASETYPEID).Select(q => q.VALUEBASETYPENAME).FirstOrDefault(),
                openMarketValue = (decimal)specifics.OPENMARKETVALUE,
                //collateralValue = specifics.COLLATERALVALUE,
                forcedSaleValue = specifics.FORCEDSALEVALUE,
                stampToCover = specifics.STAMPTOCOVER,

                //valuationSource = specifics.VALUATIONSOURCE,
                //originalValue = specifics.ORIGINALVALUE,
                //availableValue = specifics.AVAILABLEVALUE,

                securityValue = (decimal)specifics.SECURITYVALUE,
                collateralUsableAmount = specifics.COLLATERALUSABLEAMOUNT,
                remark = specifics.REMARK,
                nearestLandMark = specifics.NEARESTLANDMARK,
                nearestBusStop = specifics.NEARESTBUSSTOP,
                longitude = specifics.LONGITUDE,
                latitude = specifics.LATITUDE,
                perfectionStatusId = specifics.PERFECTIONSTATUSID,
                perfectionStatusName = context.TBL_COLLATERAL_PERFECTN_STAT.Where(l=>l.PERFECTIONSTATUSID== specifics.PERFECTIONSTATUSID).Select(j=>j.PERFECTIONSTATUSNAME).FirstOrDefault(),

                perfectionStatusReason = specifics.PERFECTIONSTATUSREASON,
                valuationAmount = specifics.VALUATIONAMOUNT,
                isResidential = specifics.ISRESIDENTIAL,
                isOwnerOccupied = specifics.ISOWNEROCCUPIED,
            };
            details = GetCollateralInsurancePolicy(details);
            return details;
        }
        // marketableSecurities collateral

        private void AddTempMarketableSecuritiesCollateral(int collateralId, CollateralViewModel entity)
        {
            //if (entity.effectiveDate > DateTime.Now || entity.maturityDate < DateTime.Now)
            //    throw new SecureException("Wrong date selected. Transaction aborted");

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

            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralId;
            workflow.Comment = "Request for marketable security collateral approval";
            workflow.OperationId = (int)OperationsEnum.CollateralApproval;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();
        }

        private void UpdateMarketableSecuritiesCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_MKT_SECURITY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

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
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
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
            //if (entity.policyStartDate > DateTime.Now || entity.assignDate > DateTime.Now || entity.policyRenewalDate < DateTime.Now)
            //    throw new SecureException("Wrong date selected. Transaction aborted");

            context.TBL_TEMP_COLLATERAL_POLICY.Add(new TBL_TEMP_COLLATERAL_POLICY
            {
                TEMPCOLLATERALCUSTOMERID = collateralId,
                ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer,
                INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber,
                PREMIUMAMOUNT = entity.premiumAmount,
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
            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralId;
            workflow.Comment = "Request for policy collateral approval";
            workflow.OperationId = (int)OperationsEnum.CollateralApproval;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();
        }

        private void UpdatePolicyCollateral(CollateralViewModel entity)
        {
            var collateral = context.TBL_COLLATERAL_POLICY
                .Where(x => x.COLLATERALCUSTOMERID == entity.collateralId)
                .FirstOrDefault();

            collateral.ISOWNEDBYCUSTOMER = entity.isOwnedByCustomer;
            // collateral.INSURANCEPOLICYNUMBER = entity.insurancePolicyNumber;
            collateral.PREMIUMAMOUNT = entity.premiumAmount;
            collateral.POLICYAMOUNT = entity.policyAmount;
            collateral.INSURANCECOMPANYNAME = entity.insuranceCompanyName;
            collateral.INSURERADDRESS = entity.insurerAddress;
            collateral.POLICYSTARTDATE = entity.policyStartDate;
            collateral.ASSIGNDATE = entity.assignDate;
            collateral.RENEWALFREQUENCYTYPEID = entity.renewalFrequencyTypeId;
            collateral.INSURERDETAILS = entity.insurerDetails;
            collateral.POLICYRENEWALDATE = entity.policyRenewalDate;
            collateral.REMARK = entity.remark;
            collateral.INSURANCETYPE = entity.insuranceType;
        }

        private CollateralViewModel GetCollateralPolicy(int collateralId)
        {
            var specifics = context.TBL_COLLATERAL_POLICY.FirstOrDefault(x => x.COLLATERALCUSTOMERID == collateralId);
            var details = new CollateralViewModel
            {
                collateralId = specifics.COLLATERALCUSTOMERID,
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
                renewalFrequencyTypeName= context.TBL_FREQUENCY_TYPE.Where(w=>w.FREQUENCYTYPEID == specifics.RENEWALFREQUENCYTYPEID).Select(u=>u.MODE).FirstOrDefault(),
                insurerDetails = specifics.INSURERDETAILS,
                policyRenewalDate = specifics.POLICYRENEWALDATE,
                remark = specifics.REMARK,
                policyinsuranceType = specifics.INSURANCETYPE,
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

            var specifics = (from x in context.TBL_COLLATERAL_VISITATION
                             where x.COLLATERALCUSTOMERID == collateralId
                             
                             select new CollateralDocumentViewModel
                             {
                                 collateralId = x.COLLATERALCUSTOMERID,
                                 visitationRemark = x.REMARK,
                                 lastVisitaionDate = x.VISITATIONDATE,
                                 CollateralVisitationID = x.COLLATERALVISITATIONID,
                                 collateralCustomerId = x.COLLATERALCUSTOMERID,

                             }).ToList();
            foreach (var file in specifics)
            {
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
                    list.CollateralVisitationID = file.CollateralVisitationID;
                    list.collateralCustomerId = file.collateralCustomerId;
                    list.fileData = data.FILEDATA;
                    list.fileExtension = data.FILEEXTENSION;
                    list.fileName = data.FILENAME;
                    list.documentId = data.DOCUMENTID;

                    response.Add(list);
                }

            }
            return response.ToList();
        }

        public List<CollateralDocumentViewModel> GetTempPropertyVistation(int collateralId)
        {
            List<CollateralDocumentViewModel> response = new List<CollateralDocumentViewModel>();

            var specifics = (from x in context.TBL_COLLATERAL_VISITATION
                             where x.COLLATERALCUSTOMERID == collateralId
                             
                             select new CollateralDocumentViewModel
                             {
                                 collateralId = x.COLLATERALCUSTOMERID,
                                 visitationRemark = x.REMARK,
                                 lastVisitaionDate = x.VISITATIONDATE,
                                 CollateralVisitationID = x.COLLATERALVISITATIONID,
                                 collateralCustomerId = x.COLLATERALCUSTOMERID,

                             }).ToList();
            foreach (var file in specifics)
            {
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
                    list.CollateralVisitationID = file.CollateralVisitationID;
                    list.collateralCustomerId = file.collateralCustomerId;
                    list.fileData = data.FILEDATA;
                    list.fileExtension = data.FILEEXTENSION;
                    list.fileName = data.FILENAME;
                    list.documentId = data.DOCUMENTID;

                    response.Add(list);
                }

            }
            return response.ToList();
        }

        public IEnumerable<LoanApplicationCollateralViewModel> MapApplicationCollateral(ApplicationCollateralMapping entity)
        {
            int collateralId = entity.collateralId == null ? 0 : (int)entity.collateralId;

            if (entity.collateralCode != null && entity.collateralId == null)
            {
                var collateral = context.TBL_COLLATERAL_CUSTOMER.FirstOrDefault(x => x.COLLATERALCODE == entity.collateralCode);
                if (collateral != null) { collateralId = collateral.COLLATERALCUSTOMERID; }
            }

            context.TBL_LOAN_APPLICATION_COLLATERL.Add(new TBL_LOAN_APPLICATION_COLLATERL
            {
                COLLATERALCUSTOMERID = collateralId,
                LOANAPPLICATIONID = entity.applicationId,
                //LOANAPPLICATIONDETAILID = entity.applicationDetailId,
                CREATEDBY = entity.staffId,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            });
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
                //loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                haircut = c.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                customerId = c.TBL_COLLATERAL_CUSTOMER.CUSTOMERID
            }).OrderByDescending(x => x.loanAppCollateralId);
            return mapped;
        }

        public IEnumerable<LoanApplicationCollateralViewModel> UnmapApplicationCollateral(ApplicationCollateralMapping entity)
        {
            var item = this.context.TBL_LOAN_APPLICATION_COLLATERL.FirstOrDefault(x => x.LOANAPPLICATIONID == entity.applicationId && x.COLLATERALCUSTOMERID == entity.collateralId);

            if (item != null)
            {
                context.TBL_LOAN_APPLICATION_COLLATERL.Remove(item);
                context.SaveChanges();
            }

            var mapped = context.TBL_LOAN_APPLICATION_COLLATERL.Where(c => c.LOANAPPLICATIONID == entity.applicationId).Select(c => new LoanApplicationCollateralViewModel
            {
                loanAppCollateralId = c.LOANAPPCOLLATERALID,
                applicationReferenceNumber = c.TBL_LOAN_APPLICATION.APPLICATIONREFERENCENUMBER,
                collateralValue = c.TBL_COLLATERAL_CUSTOMER.COLLATERALVALUE,
                collateralCustomerId = c.COLLATERALCUSTOMERID,
                collateralReferenceNumber = c.TBL_COLLATERAL_CUSTOMER.COLLATERALCODE,
                collateralType = c.TBL_COLLATERAL_CUSTOMER.TBL_COLLATERAL_TYPE.COLLATERALTYPENAME,
                loanApplicationId = c.LOANAPPLICATIONID,
                //loanApplicationDetailId = c.LOANAPPLICATIONDETAILID,
                haircut = c.TBL_COLLATERAL_CUSTOMER.HAIRCUT,
                customerId = c.TBL_COLLATERAL_CUSTOMER.CUSTOMERID
            }).OrderByDescending(x => x.loanAppCollateralId);
            return mapped;
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
                    exchangeRate = x.Mapping.TBL_COLLATERAL_CUSTOMER.EXCHANGERATE
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
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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

                    if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.TermDeposit)
                    {
                        description = "Deposit callateral lien release";
                        var collateral = context.TBL_COLLATERAL_DEPOSIT.FirstOrDefault(x => x.COLLATERALCUSTOMERID == mainCollateral.COLLATERALCUSTOMERID);
                        if (collateral != null) securityValue = collateral.SECURITYVALUE;
                    }

                    if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.TermDeposit ||
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
                       IPADDRESS = user.userIPAddress,
                       URL = user.applicationUrl,
                       APPLICATIONDATE = genSetup.GetApplicationDate(),
                       SYSTEMDATETIME = DateTime.Now
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
                        customerId = o.CUSTOMERID,
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
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
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

            if (collateralType != CollateralTypeEnum.TermDeposit)
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
                       accountName =m.ACCOUNTNAME

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

            if (collateralType != CollateralTypeEnum.MarketableSecurities)
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
                INSURANCECOMPANYNAME = entity.insuranceCompanyName,
                STARTDATE = entity.startDate,
                ENDDATE = entity.endDate

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
                        insuranceCompanyName = m.INSURANCECOMPANYNAME,
                        startDate = m.STARTDATE,
                        endDate = m.ENDDATE
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
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
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
                IPADDRESS = entity.userIPAddress,
                URL = entity.applicationUrl,
                APPLICATIONDATE = genSetup.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
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
                    where m.COMPANYID == companyId && m.ACCREDITEDCONSULTANTTYPEID == 2
                    select new CollateralValuersViewModel
                    {
                        collateralValuerId = (short)m.ACCREDITEDCONSULTANTID,
                        cityId = m.CITYID,
                        name = m.FIRMNAME,
                        valuerLicenceNumber = m.PHONENUMBER,
                        valuerTypeId = (short)m.ACCREDITEDCONSULTANTTYPEID,
                        countryId = m.COUNTRYID,
                        //accountNumber = m.nu,
                        //valuerBVN = m.,
                        emailAddress = m.EMAILADDRESS,
                        phoneNumber = m.PHONENUMBER,
                        address = m.ADDRESS,

                    });
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
                customerId = x.CUSTOMERID,
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
                approvalStatus = x.APPROVALSTATUS,
                exchangeRate = x.EXCHANGERATE,
                collateralItemPolicy = (from p in context.TBL_COLLATERAL_ITEM_POLICY.Where(s => s.COLLATERALCUSTOMERID == x.COLLATERALCUSTOMERID)
                                        select new CollateralCustomerPolicyViewModel
                                        {
                                            policyId = p.POLICYID,
                                            policyReferenceNumber = p.POLICYREFERENCENUMBER,
                                            insuranceCompanyName = p.INSURANCECOMPANYNAME,
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
                else if (record.collateralTypeId == (int)CollateralTypeEnum.TermDeposit)
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

                                                     securityValue = (decimal)x.SECURITYVALUE,
                                                     collateralUsableAmount = x.COLLATERALUSABLEAMOUNT,
                                                     remark = x.REMARK
                                                 }).FirstOrDefault();

                }
                else if (record.collateralTypeId == (int)CollateralTypeEnum.MarketableSecurities)
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
                                                    dateOfManufacture = x.MANUFACTUREDDATE,
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
                else if (record.collateralTypeId == (int)CollateralTypeEnum.Stock)
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
            var termLoanCollaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false)// && x.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved)
                .Join(context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.TermDisbursedFacility),
                    c => c.COLLATERALCUSTOMERID, lc => lc.COLLATERALCUSTOMERID, (c, lc) => new { c, lc })
                .Join(context.TBL_LOAN, clc => clc.lc.LOANID, l => l.TERMLOANID, (clc, l) => new { clc, l }) // TBL_LOAN
                .Select(o => new CollateralHistoryList
                {
                    customerName = o.l.TBL_CUSTOMER.FIRSTNAME + " " + o.l.TBL_CUSTOMER.MIDDLENAME + " " + o.l.TBL_CUSTOMER.LASTNAME,
                    loanRef = o.l.LOANREFERENCENUMBER,
                    expirationDate = o.l.MATURITYDATE,
                    collateralValue = o.clc.c.COLLATERALVALUE,
                    outstandingPrincipal = o.l.OUTSTANDINGPRINCIPAL,
                    runningPrincipal = o.l.PRINCIPALAMOUNT,
                    dateUsed = o.clc.lc.DATETIMECREATED,
                    haircut = o.clc.c.HAIRCUT,
                    exchangeRate = o.l.EXCHANGERATE,
                    approvedLoanAmount = o.l.PRINCIPALAMOUNT,
                });

            var odCollaterals = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false)// && x.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved)
                .Join(context.TBL_LOAN_COLLATERAL_MAPPING.Where(x => x.LOANSYSTEMTYPEID == (short)LoanSystemTypeEnum.OverdraftFacility),
                    c => c.COLLATERALCUSTOMERID, lc => lc.COLLATERALCUSTOMERID, (c, lc) => new { c, lc })
                .Join(context.TBL_LOAN_REVOLVING, clc => clc.lc.LOANID, l => l.REVOLVINGLOANID, (clc, l) => new { clc, l }) // TBL_LOAN_REVOLVING
                .Select(o => new CollateralHistoryList
                {
                    customerName = o.l.TBL_CUSTOMER.FIRSTNAME + " " + o.l.TBL_CUSTOMER.MIDDLENAME + " " + o.l.TBL_CUSTOMER.LASTNAME,
                    loanRef = o.l.LOANREFERENCENUMBER,
                    expirationDate = o.l.MATURITYDATE,
                    collateralValue = o.clc.c.COLLATERALVALUE,
                    outstandingPrincipal = o.l.OVERDRAFTLIMIT,
                    runningPrincipal = o.l.OVERDRAFTLIMIT,
                    dateUsed = o.clc.lc.DATETIMECREATED,
                    haircut = o.clc.c.HAIRCUT,
                    exchangeRate = o.l.EXCHANGERATE,
                    approvedLoanAmount = o.l.TBL_LOAN_APPLICATION_DETAIL.APPROVEDAMOUNT,
                });

            var collaterals = new CollateralHistory();

            collaterals.usage = termLoanCollaterals.Union(odCollaterals);
            collaterals.totalAmountUsedByOutstanding = collaterals.usage.Sum(x => x.outstandingPrincipal);
            collaterals.totalAmountUsedByPrincipal = collaterals.usage.Sum(x => x.approvedLoanAmount);
            collaterals.collateralValue = collaterals.usage.Any() ? collaterals.usage.Max(x => x.collateralValue) : 0;
            collaterals.availableValueByPrincipal = collaterals.collateralValue - collaterals.totalAmountUsedByPrincipal;
            collaterals.availableValueByOutstanding = collaterals.collateralValue - collaterals.totalAmountUsedByOutstanding;

            /*
            var testL = context.TBL_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCUSTOMERID == collateralId && x.DELETED == false).ToList();// && x.APPROVALSTATUS == (int)ApprovalStatusEnum.Approved)
            var test = termLoanCollaterals.Union(odCollaterals).ToList();
            var testTL = termLoanCollaterals.ToList();
            var testOD = odCollaterals.ToList();
            */

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
            //if (entity.constructionDate > DateTime.Now || entity.lastValuationDate > DateTime.Now || entity.lastValuationDate > DateTime.Now || entity.dateOfAcquisition>DateTime.Now)
            //    throw new SecureException("Wrong date selected. Transaction aborted"); 

            context.TBL_TEMP_COLLATERAL_IMMOV_PROP.Add(new TBL_TEMP_COLLATERAL_IMMOV_PROP
            {
                TEMPCOLLATERALCUSTOMERID = collateralId,
                PROPERTYNAME = entity.propertyName,
                CITYID = (int)entity.cityId,
                COUNTRYID = entity.countryId,
                CONSTRUCTIONDATE = entity.constructionDate,
                PROPERTYADDRESS = entity.propertyAddress,
                DATEOFACQUISITION = entity.dateOfAcquisition,
                LASTVALUATIONDATE = entity.lastValuationDate,
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
                ISRESIDENTIAL=entity.isResidential

            });

            workflow.StaffId = entity.createdBy;
            workflow.CompanyId = entity.companyId;
            workflow.StatusId = (int)ApprovalStatusEnum.Processing;
            workflow.TargetId = collateralId;
            workflow.Comment = "Request for property collateral approval";
            workflow.OperationId = (int)OperationsEnum.CollateralApproval;
            workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
            workflow.ExternalInitialization = true;
            workflow.LogActivity();

        }

        private void AddTempCasaCollateral(int collateralId, CollateralViewModel entity)
        {
            CasaBalanceViewModel casaDetail;
            string errorDesc = "";

            try
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
                        ACCOUNTNAME = entity.accountName
                    });

                    workflow.StaffId = entity.createdBy;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = collateralId;
                    workflow.Comment = "Request for property collateral approval";
                    workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                    workflow.DeferredExecution = true; // false by default will call the internal SaveChanges()
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        // FIX DEPOSIT collateral

        private void AddTempDepositCollateral(int collateralId, CollateralViewModel entity)
        {
            TDAccountRecordViewModel finacleBalance;

            try
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

                    workflow.StaffId = entity.createdBy;
                    workflow.CompanyId = entity.companyId;
                    workflow.StatusId = (int)ApprovalStatusEnum.Processing;
                    workflow.TargetId = collateralId;
                    workflow.Comment = "Request for FD collateral approval";
                    workflow.OperationId = (int)OperationsEnum.CollateralApproval;
                    workflow.DeferredExecution = true;
                    workflow.ExternalInitialization = true;
                    workflow.LogActivity();
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
                               where atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Processing && x.ISCURRENT == true//|| atrail.APPROVALSTATUSID == (int)ApprovalStatusEnum.Disapproved
                                                                                                                         //  && x.ISCURRENT == true
                                     && atrail.RESPONSESTAFFID == null
                                     && atrail.OPERATIONID == (int)OperationsEnum.CollateralApproval
                                     && ids.Contains((int)atrail.TOAPPROVALLEVELID)
                               orderby x.TEMPCOLLATERALCUSTOMERID descending
                               select new CollateralViewModel
                               {
                                   collateralId = x.TEMPCOLLATERALCUSTOMERID,
                                   collateralTypeId = x.COLLATERALTYPEID,
                                   collateralSubTypeId = x.COLLATERALSUBTYPEID,
                                   customerId = x.CUSTOMERID,
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
                                   approvalStatus = x.APPROVALSTATUSID,
                                   //allowApplicationMapping = typeIds.Contains((short)x.COLLATERALTYPEID),
                                   requireInsurancePolicy = c.REQUIREINSURANCEPOLICY,
                                   dateTimeCreated = x.DATETIMECREATED,
                                   requireVisitation = c.REQUIREVISITATION,
                                   customerName = a.FIRSTNAME + " " + a.LASTNAME + " " + a.MAIDENNAME

                               }).ToList();

            return collaterals;
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
                                   customerId = x.CUSTOMERID,
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

                               }).ToList();

            return collaterals;
        }
        private int AddTempCollateralMainForm(CollateralViewModel model)
        {
            if (context.TBL_TEMP_COLLATERAL_CUSTOMER.Where(x => x.COLLATERALCODE == model.collateralCode && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Disapproved && x.ISCURRENT!=false).OrderByDescending(x=>x.DATETIMECREATED).Any() == true)
            {
                throw new SecureException("The specified Collateral is edited and is going through approval!");
            }
            DateTime date = DateTime.Now;
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
                EXCHANGERATE = repo.GetExchangeRate(date, model.currencyId, model.companyId).sellingRate,
                CUSTOMERID = model.customerId,
                CAMREFNUMBER = model.camRefNumber,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = genSetup.GetApplicationDate(),
                ACTEDONBY = model.staffId,
                ISCURRENT = true,


            });

            if (context.SaveChanges() == 1)
            {
                return collateral.TEMPCOLLATERALCUSTOMERID;
            }

            return 0;
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
                workflow.OperationId = (int)OperationsEnum.ItemPolicyApproval;
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



        private void UpdateCutomerCollateralApprovalStatus(ApprovalViewModel ApprovalModel, short status, TwoFactorAutheticationViewModel twoFADetails)
        {
            var mainCollateral = (from x in context.TBL_TEMP_COLLATERAL_CUSTOMER
                                  join t in context.TBL_COLLATERAL_TYPE on x.COLLATERALTYPEID equals t.COLLATERALTYPEID
                                  where x.TEMPCOLLATERALCUSTOMERID == ApprovalModel.targetId && x.APPROVALSTATUSID != (int)ApprovalStatusEnum.Approved && x.APPROVALSTATUSID!= (int)ApprovalStatusEnum.Disapproved && x.ISCURRENT!=false
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

                            UpdateCollateralDocument(ApprovalModel.targetId, collaterId);
                            UpdateCollateralVisitation(ApprovalModel.targetId, collaterId);

                            if (mainCollateral.REQUIREINSURANCEPOLICY) { UpdateItemPolicyDetail(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, collaterId); } //insurance documents

                            UpdateTempApprovalStatus(ApprovalModel.targetId, status);

                        }
                    }
                    else if (mainCollateral.COLLATERALTYPEID == (int)CollateralTypeEnum.TermDeposit)
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

                            UpdateCollateralDocument(ApprovalModel.targetId, collaterId);
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
                                case (int)CollateralTypeEnum.MarketableSecurities: UpdateMarketSecurityCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.InsurancePolicy: UpdatePolicyCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.PreciousMetal: UpdateMetalCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.Stock: UpdateStockCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.Vehicle: UpdateVehicleCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;
                                case (int)CollateralTypeEnum.Promissory: UpdatePromissoryCollateral(ApprovalModel.targetId, mainCollateral.COLLATERALCODE, newCollaterId); break;

                                default: break;
                            }

                            UpdateTempApprovalStatus(ApprovalModel.targetId, status);

                            UpdateCollateralDocument(ApprovalModel.targetId, newCollaterId);
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
                        APPROVALSTATUS = (int)ApprovalStatusEnum.Approved


                    });
                    context.SaveChanges();
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
                    mainPol.INSURANCECOMPANYNAME = tempPol.INSURANCECOMPANYNAME;
                    mainPol.INSURANCETYPE = tempPol.INSURANCETYPE;
                    mainPol.LASTUPDATEDBY = tempPol.LASTUPDATEDBY;
                    mainPol.POLICYREFERENCENUMBER = tempPol.POLICYREFERENCENUMBER;
                    mainPol.STARTDATE = tempPol.STARTDATE;
                    mainPol.SUMINSURED = tempPol.SUMINSURED;
                }
                else
                {
                    context.TBL_COLLATERAL_ITEM_POLICY.Add(new TBL_COLLATERAL_ITEM_POLICY
                    {
                        COLLATERALCUSTOMERID = newCollaterId,
                        CREATEDBY = tempPol.CREATEDBY,
                        DATETIMECREATED = tempPol.DATETIMECREATED,
                        ENDDATE = tempPol.ENDDATE,
                        INSURANCECOMPANYNAME = tempPol.INSURANCECOMPANYNAME,
                        INSURANCETYPE = tempPol.INSURANCETYPE,
                        LASTUPDATEDBY = tempPol.LASTUPDATEDBY,
                        POLICYREFERENCENUMBER = tempPol.POLICYREFERENCENUMBER,
                        STARTDATE = tempPol.STARTDATE,
                        SUMINSURED = tempPol.SUMINSURED,
                    });
                }
            }

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

                }
                else
                {
                    context.TBL_COLLATERAL_IMMOVE_PROPERTY.Add(new TBL_COLLATERAL_IMMOVE_PROPERTY
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
                        ISRESIDENTIAL = tempProp.ISRESIDENTIAL
                    });
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
                    mainPolicy.INSURANCETYPE = tempPolicy.INSURANCETYPE;
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
                        INSURANCETYPE = tempPolicy.INSURANCETYPE,

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
            var tempPromissory= context.TBL_TEMP_COLLATERAL_PROMISSORY.Where(x => x.TEMPCOLLATERALCUSTOMERID == tempCollateralId).FirstOrDefault();



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
                    INSURANCECOMPANYNAME = data.INSURANCECOMPANYNAME,
                    INSURANCETYPE = data.INSURANCETYPE,
                    LASTUPDATEDBY = data.LASTUPDATEDBY,
                    POLICYREFERENCENUMBER = data.POLICYREFERENCENUMBER,
                    STARTDATE = data.STARTDATE,
                    SUMINSURED = data.SUMINSURED,
                });
                data.ISPOLICYAPPROVAL = false;
            }
        }


        public CollateralViewModel GetTempCollateralTypeByCollateralId(int collateralId, int typeId)
        {
            var data = new CollateralViewModel();
            switch (typeId)
            {
                case (int)CollateralTypeEnum.TermDeposit: data = GetTempCollateralDeposit(collateralId); break;
                case (int)CollateralTypeEnum.PlantAndMachinery: data = GetTempCollateralMachinery(collateralId); break;
                case (int)CollateralTypeEnum.Miscellaneous: data = GetTempCollateralMiscellaneous(collateralId); break;
                case (int)CollateralTypeEnum.Gaurantee: data = GetTempCollateralGuarantee(collateralId); break;
                case (int)CollateralTypeEnum.CASA: data = GetTempCollateralCasa(collateralId); break;
                case (int)CollateralTypeEnum.Property: data = GetTempCollateralImmovableProperty(collateralId); break;
                case (int)CollateralTypeEnum.MarketableSecurities: data = GetTempCollateralMarketableSecurities(collateralId); break;
                case (int)CollateralTypeEnum.InsurancePolicy: data = GetTempCollateralPolicy(collateralId); break;
                case (int)CollateralTypeEnum.PreciousMetal: data = GetTempCollateralPreciousMetal(collateralId); break;
                case (int)CollateralTypeEnum.Stock: data = GetTempCollateralStock(collateralId); break;
                case (int)CollateralTypeEnum.Vehicle: data = GetTempCollateralVehicle(collateralId); break;
                case (int)CollateralTypeEnum.Promissory: data = GetTempCollateralPromissory(collateralId); break;

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
                valueBaseTypeName = context.TBL_COLLATERAL_VALUEBASE_TYPE.Where(o=>o.COLLATERALVALUEBASETYPEID==specifics.VALUEBASETYPEID).Select(o=>o.VALUEBASETYPENAME).FirstOrDefault(),// MACHINEVALUEBASENAME,
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
                               isOwnerOccupied =x.ISOWNEROCCUPIED,
                               isResidential =x.ISRESIDENTIAL,
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
            var test = context.TBL_TEMP_COLLATERAL_PROMISSORY.Where(a=>a.TEMPCOLLATERALCUSTOMERID==collateralId).FirstOrDefault();
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
        public List<InsurancePolicies> GetTempCollateralInsurancePolicy(int collateralId)
        {
            var insurance = (context.TBL_TEMP_COLLATERAL_ITEM_POLI.Where(x => x.COLLATERALCUSTOMERID == collateralId)
                .Select(x => new InsurancePolicies
                {

                    referenceNumber = x.POLICYREFERENCENUMBER,
                    insuranceCompany = x.INSURANCECOMPANYNAME,
                    sumInsured = x.SUMINSURED,
                    startDate = x.STARTDATE,
                    expiryDate = x.ENDDATE,
                    insuranceType = x.INSURANCETYPE,
                })).ToList();

            return insurance;

        }
        public List<InsurancePolicies> GetCollateralInsurancePolicy(int collateralId)
        {
            var insurance = (context.TBL_COLLATERAL_ITEM_POLICY.Where(x => x.COLLATERALCUSTOMERID == collateralId)
                .Select(x => new InsurancePolicies
                {

                    referenceNumber = x.POLICYREFERENCENUMBER,
                    insuranceCompany = x.INSURANCECOMPANYNAME,
                    sumInsured = x.SUMINSURED,
                    startDate = x.STARTDATE,
                    expiryDate = x.ENDDATE,
                    insuranceType = x.INSURANCETYPE,
                })).ToList();

            return insurance;

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
                                   customerId = x.CUSTOMERID,
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
                                   stampToCover = context.TBL_COLLATERAL_IMMOVE_PROPERTY.Where(o=>o.COLLATERALCUSTOMERID==x.COLLATERALCUSTOMERID).Select(o=>o.STAMPTOCOVER).FirstOrDefault()
                               }).ToList();

            return collaterals;
        }

        public TDAccountRecordViewModel GetFixedDepositAccountDetail(string AccpuntNumber)
        {
          var  finacleBalance = finacle.ValidateTDAccountNumber(AccpuntNumber);

            return finacleBalance;
        }

       
    }

}

public class API_Error
{
    public string error { get; set; }
    public string errorDescription { get; set; }
}
