using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.StagingModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanArchiveRepository : ILoanArchiveRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository generalSetup;
        private IAuditTrailRepository auditTrail;
        private FinTrakBankingStagingContext stagingContext;
        private FinTrakBankingDocumentsContext documentContext;

        public LoanArchiveRepository(
        FinTrakBankingContext _context, IGeneralSetupRepository _genSetup, IAuditTrailRepository _auditTrail,
             FinTrakBankingStagingContext _stagingContext,
             FinTrakBankingDocumentsContext _documentContext

            )
        {

            this.context = _context;
            this.generalSetup = _genSetup;
            this.auditTrail = _auditTrail;
            this.stagingContext = _stagingContext;
            this.documentContext = _documentContext;
        }

        public bool ProcessLoanArchieving() {
            bool status = false;
            //call method to validate final utilized loans
            //ArchiveUtilizedLoanApplicationDetails(int loanApplicationDetailId);
            return status;
        }

        public void ArchiveUtilizedLoanApplication(int loanAppliactionId)
        {
            var app = context.TBL_LOAN_APPLICATION.FirstOrDefault(l => l.LOANAPPLICATIONID == loanAppliactionId);
            var ValidateAppExist = context.TBL_LOAN_APPLICATION_FINAL_ARCHIVE.FirstOrDefault(l => l.LOANAPPLICATIONID == loanAppliactionId);
            if (ValidateAppExist != null && ValidateAppExist.CURRENTSTATUS == (int)LoanApplicationStatusEnum.ArchiveCompleted)
            {
                throw new SecureException("Loan Application already archived!");
            }
            else
            {
                ValidateAppExist.CURRENTSTATUS = (int)LoanApplicationStatusEnum.ArchiveCompleted;
                if (context.SaveChanges() > 0) return;
            }

            TBL_LOAN_APPLICATION_FINAL_ARCHIVE loanApplArchive = new TBL_LOAN_APPLICATION_FINAL_ARCHIVE();
            loanApplArchive.ARCHIVEDATE = generalSetup.GetApplicationDate();
            loanApplArchive.LOANAPPLICATIONID = app.LOANAPPLICATIONID;
            loanApplArchive.APPLICATIONREFERENCENUMBER = app.APPLICATIONREFERENCENUMBER;
            loanApplArchive.LOANPRELIMINARYEVALUATIONID = app.LOANPRELIMINARYEVALUATIONID;
            loanApplArchive.LOANTERMSHEETID = app.LOANTERMSHEETID;
            loanApplArchive.COMPANYID = app.COMPANYID;
            loanApplArchive.CUSTOMERID = app.CUSTOMERID;
            loanApplArchive.BRANCHID = app.BRANCHID;
            loanApplArchive.CUSTOMERGROUPID = app.CUSTOMERGROUPID;
            loanApplArchive.LOANAPPLICATIONTYPEID = app.LOANAPPLICATIONTYPEID;
            loanApplArchive.RELATIONSHIPOFFICERID = app.RELATIONSHIPOFFICERID;
            loanApplArchive.RELATIONSHIPMANAGERID = app.RELATIONSHIPMANAGERID;
            loanApplArchive.CASAACCOUNTID = app.CASAACCOUNTID;
            loanApplArchive.APPLICATIONDATE = app.APPLICATIONDATE;
            loanApplArchive.INTERESTRATE = app.INTERESTRATE;
            loanApplArchive.APPLICATIONTENOR = app.APPLICATIONTENOR;
            loanApplArchive.OPERATIONID = app.OPERATIONID;
            loanApplArchive.PRODUCTCLASSID = app.PRODUCTCLASSID;
            loanApplArchive.PRODUCTID = app.PRODUCTID;
            loanApplArchive.PRODUCT_CLASS_PROCESSID = app.PRODUCT_CLASS_PROCESSID;
            loanApplArchive.APPLICATIONAMOUNT = app.APPLICATIONAMOUNT;
            loanApplArchive.APPROVEDAMOUNT = app.APPROVEDAMOUNT;
            loanApplArchive.TOTALEXPOSUREAMOUNT = app.TOTALEXPOSUREAMOUNT;
            loanApplArchive.APIREQUESTID = app.APIREQUESTID;
            loanApplArchive.LOANINFORMATION = app.LOANINFORMATION;
            loanApplArchive.MISCODE = app.MISCODE;
            loanApplArchive.TEAMMISCODE = app.MISCODE;
            loanApplArchive.ISINVESTMENTGRADE = app.ISINVESTMENTGRADE;
            loanApplArchive.ISRELATEDPARTY = app.ISRELATEDPARTY;
            loanApplArchive.ISPOLITICALLYEXPOSED = app.ISPOLITICALLYEXPOSED;
            loanApplArchive.ISPROJECTRELATED = app.ISPROJECTRELATED;
            loanApplArchive.ISONLENDING = app.ISONLENDING;
            loanApplArchive.ISINTERVENTIONFUNDS = app.ISINTERVENTIONFUNDS;
            loanApplArchive.ISORRBASEDAPPROVAL = app.ISORRBASEDAPPROVAL;
            loanApplArchive.WITHINSTRUCTION = app.WITHINSTRUCTION;
            loanApplArchive.DOMICILIATIONNOTINPLACE = app.DOMICILIATIONNOTINPLACE;
            loanApplArchive.CREATEDBY = app.OWNEDBY;
            loanApplArchive.DATETIMECREATED = app.DATETIMECREATED;
            loanApplArchive.LASTUPDATEDBY = app.LASTUPDATEDBY;
            loanApplArchive.DATETIMEUPDATED = app.DATETIMEUPDATED;
            loanApplArchive.DELETED = app.DELETED;
            loanApplArchive.DELETEDBY = app.DELETEDBY;
            loanApplArchive.DATETIMEDELETED = app.DATETIMEDELETED;
            loanApplArchive.SYSTEMDATETIME = app.SYSTEMDATETIME;
            loanApplArchive.APPROVALSTATUSID = app.APPROVALSTATUSID;
            loanApplArchive.APPLICATIONSTATUSID = app.APPLICATIONSTATUSID;
            loanApplArchive.FINALAPPROVAL_LEVELID = app.FINALAPPROVAL_LEVELID;
            loanApplArchive.TRANCHEAPPROVAL_LEVELID = app.TRANCHEAPPROVAL_LEVELID;
            loanApplArchive.NEXTAPPLICATIONSTATUSID = app.NEXTAPPLICATIONSTATUSID;
            loanApplArchive.DATEACTEDON = app.DATEACTEDON;
            loanApplArchive.ACTEDONBY = app.ACTEDONBY;
            loanApplArchive.RISKRATINGID = app.RISKRATINGID;
            loanApplArchive.SUBMITTEDFORAPPRAISAL = app.SUBMITTEDFORAPPRAISAL;
            loanApplArchive.CUSTOMERINFOVALIDATED = app.CUSTOMERINFOVALIDATED;
            loanApplArchive.APPROVEDDATE = app.APPROVEDDATE;
            loanApplArchive.AVAILMENTDATE = app.AVAILMENTDATE;
            loanApplArchive.DISPUTED = app.DISPUTED;
            loanApplArchive.REQUIRECOLLATERAL = app.REQUIRECOLLATERAL;
            loanApplArchive.COLLATERALDETAIL = app.COLLATERALDETAIL;
            loanApplArchive.CAPREGIONID = app.CAPREGIONID;
            loanApplArchive.REQUIRECOLLATERALTYPEID = app.REQUIRECOLLATERALTYPEID;
            loanApplArchive.RELATEDREFERENCENUMBER = app.RELATEDREFERENCENUMBER;
            loanApplArchive.ISCHECKLISTLOADED = app.ISCHECKLISTLOADED;
            loanApplArchive.ISADHOCAPPLICATION = app.ISADHOCAPPLICATION;
            loanApplArchive.LOANSWITHOTHERS = app.LOANSWITHOTHERS;
            loanApplArchive.OWNERSHIPSTRUCTURE = app.OWNERSHIPSTRUCTURE;
            loanApplArchive.LOANAPPROVEDLIMITID = app.LOANAPPROVEDLIMITID;
            loanApplArchive.FLOWCHANGEID = app.FLOWCHANGEID;
            loanApplArchive.ISMULTIPLEPRODUCTDRAWDOWN = app.ISMULTIPLEPRODUCTDRAWDOWN;
            loanApplArchive.ARCHIVINGOPERATIONID = app.OPERATIONID;
            this.context.TBL_LOAN_APPLICATION_FINAL_ARCHIVE.Add(loanApplArchive);
           

        }

        public bool ArchiveUtilizedLoanApplicationDetails(int loanApplicationDetailId)
        {
            var detailRow = context.TBL_LOAN_APPLICATION_DETAIL.Find(loanApplicationDetailId);
            var ValidateAppExist = context.TBL_LOAN_APPLICATION_DETAIL_FINAL_ARCHIVE.FirstOrDefault(l => l.LOANAPPLICATIONDETAILID == loanApplicationDetailId);
            if (ValidateAppExist != null)
            {
                throw new SecureException("Loan Application detail already archived!");
            }

            var documentusage = documentContext.TBL_DOCUMENT_USAGE.Where(d => d.TARGETID == detailRow.LOANAPPLICATIONID).ToList();
            TBL_LOAN_APPLICATION_DETAIL_FINAL_ARCHIVE addLoanApplDetailsArchive = new TBL_LOAN_APPLICATION_DETAIL_FINAL_ARCHIVE();

            addLoanApplDetailsArchive.ARCHIVEDATE = DateTime.Today;
            addLoanApplDetailsArchive.LOANAPPLICATIONDETAILID = detailRow.LOANAPPLICATIONDETAILID;
            addLoanApplDetailsArchive.LOANAPPLICATIONID = detailRow.LOANAPPLICATIONID;
            addLoanApplDetailsArchive.CUSTOMERID = detailRow.CUSTOMERID;
            addLoanApplDetailsArchive.PROPOSEDPRODUCTID = detailRow.PROPOSEDPRODUCTID;
            addLoanApplDetailsArchive.PROPOSEDTENOR = detailRow.PROPOSEDTENOR;
            addLoanApplDetailsArchive.PROPOSEDINTERESTRATE = detailRow.PROPOSEDINTERESTRATE;
            addLoanApplDetailsArchive.PROPOSEDAMOUNT = detailRow.PROPOSEDAMOUNT;
            addLoanApplDetailsArchive.APPROVEDPRODUCTID = detailRow.APPROVEDPRODUCTID;
            addLoanApplDetailsArchive.APPROVEDTENOR = detailRow.APPROVEDTENOR;
            addLoanApplDetailsArchive.APPROVEDINTERESTRATE = detailRow.APPROVEDINTERESTRATE;
            addLoanApplDetailsArchive.APPROVEDAMOUNT = detailRow.APPROVEDAMOUNT;
            addLoanApplDetailsArchive.CURRENCYID = detailRow.CURRENCYID;
            addLoanApplDetailsArchive.EXCHANGERATE = detailRow.EXCHANGERATE;
            addLoanApplDetailsArchive.SUBSECTORID = detailRow.SUBSECTORID;
            addLoanApplDetailsArchive.STATUSID = detailRow.STATUSID;
            addLoanApplDetailsArchive.LOANPURPOSE = detailRow.LOANPURPOSE;
            addLoanApplDetailsArchive.CREATEDBY = detailRow.CREATEDBY;
            addLoanApplDetailsArchive.DATETIMECREATED = detailRow.DATETIMECREATED;
            addLoanApplDetailsArchive.LASTUPDATEDBY = detailRow.LASTUPDATEDBY;
            addLoanApplDetailsArchive.DATETIMEUPDATED = detailRow.DATETIMEUPDATED;
            addLoanApplDetailsArchive.DELETED = detailRow.DELETED;
            addLoanApplDetailsArchive.DELETEDBY = detailRow.DELETEDBY;
            addLoanApplDetailsArchive.DATETIMEDELETED = detailRow.DATETIMEDELETED;
            addLoanApplDetailsArchive.EQUITYAMOUNT = detailRow.EQUITYAMOUNT;
            addLoanApplDetailsArchive.HASDONECHECKLIST = detailRow.HASDONECHECKLIST;
            addLoanApplDetailsArchive.EQUITYCASAACCOUNTID = detailRow.EQUITYCASAACCOUNTID;
            addLoanApplDetailsArchive.CONSESSIONAPPROVALSTATUSID = detailRow.CONSESSIONAPPROVALSTATUSID;
            addLoanApplDetailsArchive.CONSESSIONREASON = detailRow.CONSESSIONREASON;
            addLoanApplDetailsArchive.ISPOLITICALLYEXPOSED = detailRow.ISPOLITICALLYEXPOSED;
            addLoanApplDetailsArchive.REPAYMENTTERMS = detailRow.REPAYMENTTERMS;
            addLoanApplDetailsArchive.REPAYMENTSCHEDULEID = detailRow.REPAYMENTSCHEDULEID;
            addLoanApplDetailsArchive.EFFECTIVEDATE = detailRow.EFFECTIVEDATE;
            addLoanApplDetailsArchive.ISTAKEOVERAPPLICATION = detailRow.ISTAKEOVERAPPLICATION;
            addLoanApplDetailsArchive.EXPIRYDATE = detailRow.EXPIRYDATE;
            addLoanApplDetailsArchive.CASAACCOUNTID = detailRow.CASAACCOUNTID;
            addLoanApplDetailsArchive.OPERATINGCASAACCOUNTID = detailRow.OPERATINGCASAACCOUNTID;
            addLoanApplDetailsArchive.SECUREDBYCOLLATERAL = detailRow.SECUREDBYCOLLATERAL;
            addLoanApplDetailsArchive.CRMSCOLLATERALTYPEID = detailRow.CRMSCOLLATERALTYPEID;
            addLoanApplDetailsArchive.MORATORIUMDURATION = detailRow.MORATORIUMDURATION;
            addLoanApplDetailsArchive.CRMSFUNDINGSOURCEID = detailRow.CRMSFUNDINGSOURCEID;
            addLoanApplDetailsArchive.CRMSREPAYMENTSOURCEID = detailRow.CRMSREPAYMENTSOURCEID;
            addLoanApplDetailsArchive.CRMSFUNDINGSOURCECATEGORY = detailRow.CRMSFUNDINGSOURCECATEGORY;
            addLoanApplDetailsArchive.CRMS_ECCI_NUMBER = detailRow.CRMS_ECCI_NUMBER;
            addLoanApplDetailsArchive.CRMSCODE = detailRow.CRMSCODE;
            addLoanApplDetailsArchive.CRMSREPAYMENTAGREEMENTID = detailRow.CRMSREPAYMENTAGREEMENTID;
            addLoanApplDetailsArchive.CRMSVALIDATED = detailRow.CRMSVALIDATED;
            addLoanApplDetailsArchive.CRMSDATE = detailRow.CRMSDATE;
            addLoanApplDetailsArchive.TRANSACTIONDYNAMICS = detailRow.TRANSACTIONDYNAMICS;
            addLoanApplDetailsArchive.CONDITIONPRECIDENT = detailRow.CONDITIONPRECIDENT;
            addLoanApplDetailsArchive.CONDITIONSUBSEQUENT = detailRow.CONDITIONSUBSEQUENT;
            addLoanApplDetailsArchive.FIELD1 = detailRow.FIELD1;
            addLoanApplDetailsArchive.PRODUCTPRICEINDEXRATE = detailRow.PRODUCTPRICEINDEXRATE;
            addLoanApplDetailsArchive.PRODUCTPRICEINDEXID = detailRow.PRODUCTPRICEINDEXID;
            addLoanApplDetailsArchive.FIELD2 = detailRow.FIELD2;
            addLoanApplDetailsArchive.FIELD3 = detailRow.FIELD3;
            addLoanApplDetailsArchive.ISSPECIALISED = detailRow.ISSPECIALISED;
            addLoanApplDetailsArchive.TENORFREQUENCYTYPEID = detailRow.TENORFREQUENCYTYPEID;

            context.TBL_LOAN_APPLICATION_DETAIL_FINAL_ARCHIVE.Add(addLoanApplDetailsArchive);
            ArchiveUtilizedLoanApplication(addLoanApplDetailsArchive.LOANAPPLICATIONID);
            foreach (var f in documentusage)
            {
                ArchiveUtilizedLoanDocumentUsage(f.DOCUMENTUSAGEID);
            }
            if (context.SaveChanges() > 0)
            {
                DeleteLoanDocumentUsage(addLoanApplDetailsArchive.LOANAPPLICATIONID, loanApplicationDetailId);
                return true;
            }
            return false;
           
        }

        public void ArchiveUtilizedLoanDocumentUsage(int documentUsageId)
        {
            var detailRow = documentContext.TBL_DOCUMENT_USAGE.Find(documentUsageId);
            var ValidateDocExist = documentContext.TBL_DOCUMENT_USAGE_FINAL_ARCHIVE.FirstOrDefault(l => l.DOCUMENTUSAGEID == documentUsageId);
            if (ValidateDocExist != null)
            {
                throw new SecureException("Loan Document already archived!");
            }

            TBL_DOCUMENT_USAGE_FINAL_ARCHIVE addLoanDocumentArchive = new TBL_DOCUMENT_USAGE_FINAL_ARCHIVE();

            addLoanDocumentArchive.ARCHIVEDATE = DateTime.Today;
            addLoanDocumentArchive.DOCUMENTUSAGEID = detailRow.DOCUMENTUSAGEID;
            addLoanDocumentArchive.DOCUMENTUPLOADID = detailRow.DOCUMENTUPLOADID;
            addLoanDocumentArchive.TARGETID = detailRow.TARGETID;
            addLoanDocumentArchive.TARGETCODE = detailRow.TARGETCODE;
            addLoanDocumentArchive.TARGETREFERENCENUMBER = detailRow.TARGETREFERENCENUMBER;
            addLoanDocumentArchive.DOCUMENTCODE = detailRow.DOCUMENTCODE;
            addLoanDocumentArchive.DOCUMENTTITLE = detailRow.DOCUMENTTITLE;
            addLoanDocumentArchive.CUSTOMERCODE = detailRow.CUSTOMERCODE;
            addLoanDocumentArchive.OPERATIONID = detailRow.OPERATIONID;
            addLoanDocumentArchive.APPROVALSTATUSID = detailRow.APPROVALSTATUSID;
            addLoanDocumentArchive.DOCUMENTSTATUSID = detailRow.DOCUMENTSTATUSID;
            addLoanDocumentArchive.ISPRIMARYDOCUMENT = detailRow.ISPRIMARYDOCUMENT;
            addLoanDocumentArchive.DELETED = detailRow.DELETED;
            addLoanDocumentArchive.DELETEDBY = detailRow.DELETEDBY;
            addLoanDocumentArchive.DATETIMECREATED = detailRow.DATETIMECREATED;
            addLoanDocumentArchive.DATETIMEDELETED = detailRow.DATETIMEDELETED;
            addLoanDocumentArchive.CREATEDBY = detailRow.CREATEDBY;
            addLoanDocumentArchive.LASTUPDATEDBY = detailRow.LASTUPDATEDBY;
            addLoanDocumentArchive.DATETIMEUPDATED = detailRow.DATETIMEUPDATED;
            documentContext.TBL_DOCUMENT_USAGE_FINAL_ARCHIVE.Add(addLoanDocumentArchive);
            documentContext.SaveChanges();
        }


        public bool DeleteLoanDocumentUsage(int targetId, int loanApplicationDetailId)
        {
            var records = documentContext.TBL_DOCUMENT_USAGE_FINAL_ARCHIVE.Where(d => d.TARGETID == targetId).ToList();

            if (records != null)
            {
                documentContext.TBL_DOCUMENT_USAGE_FINAL_ARCHIVE.RemoveRange(records);
                documentContext.SaveChanges();
            }

            DeleteLoanApplicationDetail(loanApplicationDetailId);
            return context.SaveChanges() > 0;
        }

        public void DeleteLoanApplicationDetail(int loanApplicationDetailId)
        {
            var data = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONDETAILID == loanApplicationDetailId).FirstOrDefault();
            context.TBL_LOAN_APPLICATION_DETAIL.Remove(data);

        }
    }
}
