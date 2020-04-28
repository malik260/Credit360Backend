using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.ViewModels.Report;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ReportingObjects
{
    public class OutputDocument
    {
        #region OUTPUT DOCUMENT

        FinTrakBankingContext context = new FinTrakBankingContext();
        FinTrakBankingDocumentsContext docContext = new FinTrakBankingDocumentsContext();
        public IEnumerable<OutPutDocumentApprovalViewModel> GetApplicationApproval(int loanApplicationId)
        {

            return context.TBL_LOAN_APPLICATION_DETL_LOG.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == loanApplicationId)
                .Join(context.TBL_STAFF, a => a.CREATEDBY, b => b.STAFFID, (a, b) => new { a, b })
                .Select(x => new OutPutDocumentApprovalViewModel
                {
                    officer = context.TBL_STAFF_ROLE.Where(o => o.STAFFROLEID == x.b.STAFFROLEID).Select(o => o.STAFFROLENAME).FirstOrDefault(),
                    name = x.b.FIRSTNAME + " " + x.b.MIDDLENAME + " " + x.b.LASTNAME,
                    signature = "",// x.a.DECISION,
                    id = x.a.LOAN_APPLICATION_DETAIL_LOGID

                }).OrderByDescending(p => p.id);
        }

        public IEnumerable<OutPutDocumentChecklistViewModel> GetChecklist(int loanApplicationId)
        {

            return (from b in context.TBL_LOAN_APPLICATION
                    where b.LOANAPPLICATIONID == loanApplicationId
                    select new OutPutDocumentChecklistViewModel
                    {

                    }).ToList();
        }

        public IEnumerable<OutPutDocumentCollateralViewModel> GetCollateral(int loanApplicationId)
        {

            var collateral = (from x in context.TBL_LOAN_APPLICATION_COLLATRL2
                              join b in context.TBL_LOAN_APPLICATION on x.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                              where b.LOANAPPLICATIONID == loanApplicationId
                              select new OutPutDocumentCollateralViewModel
                              {
                                  collateralDetail = x.COLLATERALDETAIL,
                                  collateralValue = x.COLLATERALVALUE,
                                  stapedToCoverAmount = x.STAMPEDTOCOVERAMOUNT
                              }).ToList();

            return collateral;
        }

        public IEnumerable<OutPutDocumentConcurrencesViewModel> GetConcurrences(int loanApplicationId)
        {

            return context.TBL_LOAN_APPLICATION_DETL_LOG.Where(x => x.TBL_LOAN_APPLICATION_DETAIL.LOANAPPLICATIONID == loanApplicationId)
            .Join(context.TBL_STAFF, a => a.CREATEDBY, b => b.STAFFID, (a, b) => new { a, b })
            .Select(x => new OutPutDocumentConcurrencesViewModel
            {
                officer = context.TBL_STAFF_ROLE.Where(o => o.STAFFROLEID == x.b.STAFFROLEID).Select(o => o.STAFFROLENAME).FirstOrDefault(),
                name = x.b.FIRSTNAME + " " + x.b.MIDDLENAME + " " + x.b.LASTNAME,
                signature = "",// x.a.DECISION,
                id = x.a.LOAN_APPLICATION_DETAIL_LOGID

            }).OrderByDescending(p => p.id);
        }

        public IEnumerable<OutPutDocumentCustomerFacilitiesViewModel> GetCustomerFacilities(int loanApplicationId)
        {

            var loanDetails = (from a in context.TBL_LOAN_APPLICATION
                               join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                               from d in cg.DefaultIfEmpty()
                               join e in context.TBL_CURRENCY on b.CURRENCYID equals e.CURRENCYID
                               where a.LOANAPPLICATIONID == loanApplicationId
                               && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                               && a.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                               && b.STATUSID == (int)ApprovalStatusEnum.Approved
                               select new OutPutDocumentCustomerFacilitiesViewModel()
                               {
                                   facility = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                   amount = e.CURRENCYNAME + " " + b.APPROVEDAMOUNT,
                                   maturity = b.EXPIRYDATE,
                                   security = "",
                                   performance = "",
                                   operationId = a.OPERATIONID,
                                   customerId = c.CUSTOMERID,
                                   customerCode = c.CUSTOMERCODE,
                                   createdBy = a.OWNEDBY,
                                   applicationReferenceNumber = a.APPLICATIONREFERENCENUMBER,
                                   targetReferenceNumber = a.APPLICATIONREFERENCENUMBER
                               }).ToList();



            return loanDetails;
        }

        public IEnumerable<OutPutDocumentCustomerInformationViewModel> GetCustomerInformation(int loanApplicationId)
        {

            var loanDetails = (from a in context.TBL_LOAN_APPLICATION
                               join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               join c in context.TBL_CUSTOMER on a.CUSTOMERID equals c.CUSTOMERID into cc
                               from c in cc.DefaultIfEmpty()
                               join d in context.TBL_CUSTOMER_GROUP on a.CUSTOMERGROUPID equals d.CUSTOMERGROUPID into cg
                               from d in cg.DefaultIfEmpty()
                               join e in context.TBL_CUSTOMER_ADDRESS on a.CUSTOMERID equals e.CUSTOMERID into dg
                               from e in dg.DefaultIfEmpty()
                               join g in context.TBL_CUSTOMER_PHONECONTACT on a.CUSTOMERID equals g.CUSTOMERID into gg
                               from g in gg.DefaultIfEmpty()
                               join h in context.TBL_CURRENCY on b.CURRENCYID equals h.CURRENCYID into hh
                               from h in hh.DefaultIfEmpty()
                               where a.LOANAPPLICATIONID == loanApplicationId
                               // b.STATUSID == (int)ApprovalStatusEnum.Approved
                               select new OutPutDocumentCustomerInformationViewModel()
                               {
                                   borrower = c.FIRSTNAME + " " + c.LASTNAME,
                                   location = e.ADDRESS ?? " ",
                                   business = "",
                                   accountNumber = b.TBL_CASA.PRODUCTACCOUNTNUMBER,
                                   incorporationDate = "",
                                   principalPromoters = "",
                                   customerRiskRating = "",
                                   classification = "",
                                   accountOpeningDate = "",
                                   businessCommencementDate = "",

                               }).ToList();

            return loanDetails;
        }

        public IEnumerable<OutPutDocumentFeeViewModel> GetFee(int loanApplicationId)
        {

            var fees = (from a in context.TBL_LOAN_APPLICATION_DETL_FEE
                        join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONDETAILID equals b.LOANAPPLICATIONDETAILID
                        join c in context.TBL_CHARGE_FEE on a.CHARGEFEEID equals c.CHARGEFEEID
                        join d in context.TBL_LOAN_APPLICATION on b.LOANAPPLICATIONID equals d.LOANAPPLICATIONID
                        join e in context.TBL_PRODUCT on b.PROPOSEDPRODUCTID equals e.PRODUCTID
                        where d.LOANAPPLICATIONID == loanApplicationId
                        //&& d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationInProgress
                        //&& d.APPLICATIONSTATUSID != (int)LoanApplicationStatusEnum.CancellationCompleted
                        //&& b.STATUSID == (int)ApprovalStatusEnum.Approved
                        select new OutPutDocumentFeeViewModel()
                        {
                            feeName = c.CHARGEFEENAME,
                            rateValue = a.RECOMMENDED_FEERATEVALUE,
                            productName = e.PRODUCTNAME
                        }).ToList();

            return fees;

        }

        public IEnumerable<OutPutDocumentMonthsActivityViewModel> GetMonthsActivity(int loanApplicationId)
        {

            return (from b in context.TBL_LOAN_APPLICATION
                    where b.LOANAPPLICATIONID == loanApplicationId
                    select new OutPutDocumentMonthsActivityViewModel
                    {

                    }).ToList();
        }

        public IEnumerable<OutPutDocumentMonthActivitySignViewModel> MonthActivitySignature(int loanApplicationId)
        {

            FinTrakBankingContext context = new FinTrakBankingContext();

            return (from b in context.TBL_LOAN_APPLICATION
                    where b.LOANAPPLICATIONID == loanApplicationId
                    select new OutPutDocumentMonthActivitySignViewModel
                    {

                    }).ToList();
        }

        public IEnumerable<CurrentRequestViewModel> GetCurrentRequest(int loanApplicationId)
        {

            return (from a in context.TBL_LOAN_APPLICATION
                               join b in context.TBL_LOAN_APPLICATION_DETAIL on a.LOANAPPLICATIONID equals b.LOANAPPLICATIONID
                               where a.LOANAPPLICATIONID == loanApplicationId
                                select new CurrentRequestViewModel()
                               {
                                   productName = context.TBL_PRODUCT.FirstOrDefault(x => x.PRODUCTID == b.APPROVEDPRODUCTID).PRODUCTNAME,
                                    purpose = b.LOANPURPOSE,
                                    tenor = b.APPROVEDTENOR,
                                    repaymentSchedule = b.TBL_REPAYMENT_TERM.REPAYMENTTERMDETAIL ?? "Not applicable",

                               }).ToList();
        }


        public IEnumerable<OutputDocumentSummaryViewModel> GetSummary(int loanApplicationId)
        {

            return (from b in context.TBL_LOAN_APPLICATION
                    where b.LOANAPPLICATIONID == loanApplicationId
                    select new OutputDocumentSummaryViewModel
                    {

                    }).ToList();
        }

        public int AddDocumentUpload(DocumentUploadViewModel model, byte[] buffer)
        {
            var existing = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false
                    && x.OPERATIONID == model.operationId
                    && x.TARGETID == model.targetId
                    && x.CUSTOMERCODE == model.customerCode)
                .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false && x.FILENAME == model.fileName)
                , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) => new { us, up }
            )
            .Select(x => new DocumentUploadViewModel
            {
                documentUploadId = x.up.DOCUMENTUPLOADID,
                documentUsageId = x.us.DOCUMENTUSAGEID,
                fileName = x.up.FILENAME,
                fileExtension = x.up.FILEEXTENSION,
                fileSize = x.up.FILESIZE,
                fileSizeUnit = x.up.FILESIZEUNIT,
                companyId = x.up.COMPANYID,
                issueDate = x.up.ISSUEDATE,
                expiryDate = x.up.EXPIRYDATE,
                createdBy = (int)x.up.CREATEDBY
            })
                .FirstOrDefault();

            if (existing != null && model.overwrite == false) return 3;


            var entity = new TBL_DOCUMENT_UPLOAD
            {
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension.ToLower(),
                FILESIZE = model.fileSize,
                FILESIZEUNIT = model.fileSizeUnit,
                FILEDATA = buffer,
                COMPANYID = model.companyId,
                ISSUEDATE = model.issueDate,
                EXPIRYDATE = model.expiryDate,
                PHYSICALFILENUMBER = model.physicalFilenumber,
                PHYSICALLOCATION = model.physicalLocation,
                DOCUMENTTYPEID = model.documentTypeId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE,
            };

            docContext.TBL_DOCUMENT_UPLOAD.Add(entity);

            if (docContext.SaveChanges() > 0)
            {
                var usage = new TBL_DOCUMENT_USAGE
                {
                    DOCUMENTUPLOADID = entity.DOCUMENTUPLOADID,
                    TARGETID = model.targetId,
                    TARGETCODE = model.targetCode,
                    TARGETREFERENCENUMBER = model.targetReferenceNumber,
                    DOCUMENTCODE = model.documentCode,
                    DOCUMENTTITLE = model.documentTitle,
                    CUSTOMERCODE = model.customerCode,
                    OPERATIONID = model.operationId,
                    APPROVALSTATUSID = model.approvalStatusId,
                    DOCUMENTSTATUSID = model.documentStatusId,
                    ISPRIMARYDOCUMENT = model.isPrimaryDocument,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = context.TBL_FINANCECURRENTDATE.FirstOrDefault().CURRENTDATE
                };

                if (model.overwrite == true)
                {
                    usage.DATETIMEUPDATED = DateTime.Now;
                    usage.LASTUPDATEDBY = model.createdBy;
                }

                docContext.TBL_DOCUMENT_USAGE.Add(usage);

                var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
                // Audit Section ---------------------------
                //this.audit.AddAuditTrail(new TBL_AUDIT
                //{
                //    AUDITTYPEID = (short)AuditTypeEnum.DocumentUploadAdded,
                //    STAFFID = model.createdBy,
                //    BRANCHID = (short)model.userBranchId,
                //    DETAIL = $"TBL_Document Upload '{model.targetCode}' created by {auditStaff}",
                //    IPADDRESS = model.userIPAddress,
                //    URL = model.applicationUrl,
                //    APPLICATIONDATE = general.GetApplicationDate(),
                //    SYSTEMDATETIME = DateTime.Now
                //});

                if (existing != null && model.overwrite == true)
                {
                    var oldUpload = docContext.TBL_DOCUMENT_UPLOAD.Find(existing.documentUploadId);
                    var oldUsage = docContext.TBL_DOCUMENT_USAGE.Find(existing.documentUsageId);

                    oldUpload.DELETED = true;
                    oldUpload.DELETEDBY = model.createdBy;
                    oldUpload.DATETIMEDELETED = DateTime.Now;

                    oldUsage.DELETED = true;
                    oldUsage.DELETEDBY = model.createdBy;
                    oldUsage.DATETIMEDELETED = DateTime.Now;
                }

            }

            if (docContext.SaveChanges() < 1)
            {
                var file = docContext.TBL_DOCUMENT_UPLOAD.Where(o => o.DOCUMENTUPLOADID == entity.DOCUMENTUPLOADID).Select(o => o).FirstOrDefault();
                if (file != null)
                {
                    docContext.TBL_DOCUMENT_UPLOAD.Remove(file);
                    docContext.SaveChanges();
                }
                return 1;
            }

            return 2;
        }
        #endregion

    }
}
