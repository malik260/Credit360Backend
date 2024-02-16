using FintrakBanking.Common.CustomException;
using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Media;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common;
using FintrakBanking.Interfaces.Credit;
using System.Net.Http;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Office.Interop.Excel;
using ServiceStack;
using FintrakBanking.ViewModels.Setups.Approval;

namespace FintrakBanking.Repositories.Media
{
    public class DocumentUploadRepository : IDocumentUploadRepository
    {
        private FinTrakBankingDocumentsContext docContext;
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private IAdminRepository admin;
        private IWorkflow workflow;
        private ICustomerCreditBureauRepository creditBureau;

        public DocumentUploadRepository(
                FinTrakBankingDocumentsContext _docContext,
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow,
                ICustomerCreditBureauRepository _creditBureau
            )
        {
            this.docContext = _docContext;
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
            this.creditBureau = _creditBureau;
        }

        private string getUrl(string countryCode)
        {
            var url = ConfigurationManager.AppSettings[countryCode];
            return url;

        }

        //public IEnumerable<DocumentUploadViewModel> GetDocumentUploads()
        //{
        //    return docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
        //        .Select(x => new DocumentUploadViewModel
        //        {
        //            documentUploadId = x.DOCUMENTUPLOADID,
        //            fileName = x.FILENAME,
        //            fileExtension = x.FILEEXTENSION,
        //            fileSize = x.FILESIZE,
        //            fileSizeUnit = x.FILESIZEUNIT,
        //            fileData = x.FILEDATA,
        //            companyId = x.COMPANYID,
        //            issueDate = x.ISSUEDATE,
        //            expiryDate = x.EXPIRYDATE,
        //            physicalFilenumber = x.PHYSICALFILENUMBER,
        //            physicalLocation = x.PHYSICALLOCATION,
        //        })
        //        .ToList();
        //}

        public IEnumerable<DocumentUploadViewModel> GetDocumentUploads(int staffId, int operationId, int targetId, bool isOperationSpecific=false)
        {
            var firstQuery = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
                , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) =>
               new {
                   documentUploadId = up.DOCUMENTUPLOADID,
                   fileName = up.FILENAME,
                   fileExtension = up.FILEEXTENSION,
                   fileSize = up.FILESIZE,
                   fileSizeUnit = up.FILESIZEUNIT,
                   //fileData = up.FILEDATA,
                   companyId = up.COMPANYID,
                   issueDate = up.ISSUEDATE,
                   expiryDate = up.EXPIRYDATE,
                   physicalFilenumber = up.PHYSICALFILENUMBER,
                   physicalLocation = up.PHYSICALLOCATION,
                   isOriginalCopy = up.ISORIGINALCOPY,
                   documentTypeId = up.DOCUMENTTYPEID,
                   documentTypeName = up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                   documentCategoryId = up.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                   documentCategoryName = up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                   owner = up.CREATEDBY == staffId,
                   dateTimeCreated = us.DATETIMECREATED,
                   dateTimeUpdated = us.DATETIMEUPDATED,
                   createdBy = us.CREATEDBY.Value,
               }
                )
                .AsEnumerable()
                .Select(up => new DocumentUploadViewModel
                {
                    documentUploadId = up.documentUploadId,
                    fileName = up.fileName,
                    fileExtension = up.fileExtension,
                    fileSize = up.fileSize,
                    fileSizeUnit = up.fileSizeUnit,
                    //fileData = up.fileData,
                    companyId = up.companyId,
                    issueDate = up.issueDate,
                    expiryDate = up.expiryDate,
                    physicalFilenumber = up.physicalFilenumber,
                    physicalLocation = up.physicalLocation,
                    isOriginalCopy = up.isOriginalCopy,
                    documentTypeId = up.documentTypeId,
                    documentTypeName = up.documentTypeName,
                    documentCategoryId = up.documentCategoryId,
                    documentCategoryName = up.documentCategoryName,
                    owner = up.owner,
                    dateTimeCreated = up.dateTimeCreated,
                    dateTimeUpdated = up.dateTimeUpdated,
                    createdBy = up.createdBy,
                    uploadedBy = context.TBL_STAFF.Where(s => s.STAFFID == up.createdBy && s.DELETED != true).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + "(" + s.STAFFCODE + ")").FirstOrDefault(),

                 })
                 .OrderBy(x => x.dateTimeCreated)
                 .ThenBy(x => x.documentCategoryId)
                 .ThenBy(x => x.documentTypeId)?
                 .ToList();



            var customerId = (from ccb in context.TBL_CUSTOMER_CREDIT_BUREAU
                              join app in context.TBL_LOAN_APPLICATION_DETAIL on ccb.CUSTOMERID equals app.CUSTOMERID
                              where app.LOANAPPLICATIONID == targetId
                              select ccb.CUSTOMERID).ToList();

            var customerBureauLog = creditBureau.GetCustomerCreditBureauReportLog(customerId.FirstOrDefault(), null).Select(x => x.customerCreditBureauId).ToList();

            var staff = (from x in context.TBL_STAFF
                            join app in context.TBL_LOAN_APPLICATION_DETAIL on x.STAFFID equals app.CREATEDBY
                            where app.LOANAPPLICATIONID == targetId
                            select x).FirstOrDefault();

            var secondQuery = new List<DocumentUploadViewModel>();

            if (customerBureauLog.Count() > 0) {
                secondQuery = (from d in docContext.TBL_CUSTOMER_CREDIT_BUREAU
                                   where customerBureauLog.Contains(d.CUSTOMERCREDITBUREAUID)
                                   select new DocumentUploadViewModel
                                   {
                                       documentUploadId = d.DOCUMENTID,
                                       documentTypeName = "CREDIT BUREAU",
                                       documentCategoryName = "CREDIT BUREAU",
                                       dateTimeCreated = d.DATETIMECREATED,
                                       //uploadedBy = staff,
                                       uploadedBy = staff.FIRSTNAME + " " + staff.LASTNAME,
                                       documentTitle = d.DOCUMENT_TITLE,
                                       fileName = d.FILENAME,
                                       fileExtension = d.FILEEXTENSION,
                                       owner = staff.STAFFID == staffId,
                                       createdBy = staff.STAFFID,
                                       //fileData = d.FILEDATA,
                                       //fileSize = d.fileSize,
                                   })?.ToList();
            }
            
            //-------------------------------------------------------------------------

            //var customerCreditBureau = (from ccb in context.TBL_CUSTOMER_CREDIT_BUREAU
            //                            join app in context.TBL_LOAN_APPLICATION_DETAIL on ccb.CUSTOMERID equals app.CUSTOMERID
            //                            where app.LOANAPPLICATIONID == targetId
            //                            select ccb.CUSTOMERCREDITBUREAUID).ToList();

            //string staff = (from x in context.TBL_STAFF
            //                join app in context.TBL_LOAN_APPLICATION_DETAIL on x.STAFFID equals app.CREATEDBY
            //                where app.LOANAPPLICATIONID == targetId
            //                select x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault();

            //var secondQuery = (from d in docContext.TBL_CUSTOMER_CREDIT_BUREAU
            //                   where customerCreditBureau.Contains(d.CUSTOMERCREDITBUREAUID)
            //                   select new DocumentUploadViewModel
            //                   {
            //                       documentUploadId = d.DOCUMENTID,
            //                       documentTypeName = "CREDIT BUREAU",
            //                       documentCategoryName = "CREDIT BUREAU",
            //                       dateTimeCreated = d.DATETIMECREATED,
            //                       uploadedBy = staff,
            //                       documentTitle = d.DOCUMENT_TITLE,
            //                       fileName = d.FILENAME,
            //                       fileExtension = d.FILEEXTENSION,
            //                       //fileData = d.FILEDATA,
            //                       //fileSize = d.fileSize,
            //                   })?.ToList();

            var output = firstQuery.Union(secondQuery).ToList();

            if (!isOperationSpecific)
            {
                output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.OfferLetterApproval, targetId));
                output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.LoanReviewApprovalOfferLetter, targetId));
                output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.LoanAvailment, targetId));
                var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).ToList();
                foreach (var f in facilities)
                {
                    var providedConditionCheckLists = context.TBL_LOAN_CONDITION_PRECEDENT.Where(c => c.LOANAPPLICATIONDETAILID == f.LOANAPPLICATIONDETAILID && c.CHECKLISTSTATUSID == (int)CheckListStatusEnum.Provided).ToList();
                    foreach (var c in providedConditionCheckLists)
                    {
                        output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.DefferedChecklistApproval, c.LOANCONDITIONID));
                    }
                    var request = context.TBL_LOAN_BOOKING_REQUEST.Where(x => x.LOANAPPLICATIONDETAILID == f.LOANAPPLICATIONDETAILID);
                    foreach (var r in request)
                    {
                        if (r?.OPERATIONID != null)
                        {
                            output.AddRange(GetDocumentUploadsByOperation(staffId, (short)r.OPERATIONID, r.LOAN_BOOKING_REQUESTID));
                        }
                        else
                        {
                            output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.IndividualDrawdownRequest, r.LOAN_BOOKING_REQUESTID));
                            output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.CorporateDrawdownRequest, r.LOAN_BOOKING_REQUESTID));
                            output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.CreditCardDrawdownRequest, r.LOAN_BOOKING_REQUESTID));
                        }
                        output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.TermLoanBooking, r.LOAN_BOOKING_REQUESTID));
                        output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.RevolvingLoanBooking, r.LOAN_BOOKING_REQUESTID));
                        output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.ContigentLoanBooking, r.LOAN_BOOKING_REQUESTID));

                    }
                }
            }

            return output;
        }

        public IEnumerable<DocumentUploadViewModel> GetDocumentUploadsLms(int staffId, int operationId, int targetId, bool isOperationSpecific = false, bool isLms = false)
        {
            
                var firstQuery = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                    .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
                    , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) =>
                   new
                   {
                       documentUploadId = up.DOCUMENTUPLOADID,
                       fileName = up.FILENAME,
                       fileExtension = up.FILEEXTENSION,
                       fileSize = up.FILESIZE,
                       fileSizeUnit = up.FILESIZEUNIT,
                   //fileData = up.FILEDATA,
                   companyId = up.COMPANYID,
                       issueDate = up.ISSUEDATE,
                       expiryDate = up.EXPIRYDATE,
                       physicalFilenumber = up.PHYSICALFILENUMBER,
                       physicalLocation = up.PHYSICALLOCATION,
                       documentTypeId = up.DOCUMENTTYPEID,
                       documentTypeName = up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                       documentCategoryId = up.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                       documentCategoryName = up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                       owner = up.CREATEDBY == staffId,
                       dateTimeCreated = us.DATETIMECREATED,
                       dateTimeUpdated = us.DATETIMEUPDATED,
                       createdBy = us.CREATEDBY.Value,
                   }
                    )
                    .AsEnumerable()
                    .Select(up => new DocumentUploadViewModel
                    {
                        documentUploadId = up.documentUploadId,
                        fileName = up.fileName,
                        fileExtension = up.fileExtension,
                        fileSize = up.fileSize,
                        fileSizeUnit = up.fileSizeUnit,
                        //fileData = up.fileData,
                        companyId = up.companyId,
                        issueDate = up.issueDate,
                        expiryDate = up.expiryDate,
                        physicalFilenumber = up.physicalFilenumber,
                        physicalLocation = up.physicalLocation,
                        documentTypeId = up.documentTypeId,
                        documentTypeName = up.documentTypeName,
                        documentCategoryId = up.documentCategoryId,
                        documentCategoryName = up.documentCategoryName,
                        owner = up.owner,
                        dateTimeCreated = up.dateTimeCreated,
                        dateTimeUpdated = up.dateTimeUpdated,
                        createdBy = up.createdBy,
                        uploadedBy = context.TBL_STAFF.Where(s => s.STAFFID == up.createdBy && s.DELETED != true).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + "(" + s.STAFFCODE + ")").FirstOrDefault(),

                    })
                    .OrderBy(x => x.dateTimeCreated)
                    .ThenBy(x => x.documentCategoryId)
                    .ThenBy(x => x.documentTypeId)?
                    .ToList();

                if (isLms) return firstQuery;

                var customerCreditBureau = (from ccb in context.TBL_CUSTOMER_CREDIT_BUREAU
                                            join app in context.TBL_LOAN_APPLICATION_DETAIL on ccb.CUSTOMERID equals app.CUSTOMERID
                                            where app.LOANAPPLICATIONID == targetId
                                            select ccb.CUSTOMERCREDITBUREAUID).ToList();

                string staff = (from x in context.TBL_STAFF
                                join app in context.TBL_LOAN_APPLICATION_DETAIL on x.STAFFID equals app.CREATEDBY
                                where app.LOANAPPLICATIONID == targetId
                                select x.FIRSTNAME + " " + x.LASTNAME).FirstOrDefault();

                var secondQuery = (from d in docContext.TBL_CUSTOMER_CREDIT_BUREAU
                                   where customerCreditBureau.Contains(d.CUSTOMERCREDITBUREAUID)
                                   select new DocumentUploadViewModel
                                   {
                                       documentUploadId = d.DOCUMENTID,
                                       documentTypeName = "CREDIT BUREAU",
                                       documentCategoryName = "CREDIT BUREAU",
                                       dateTimeCreated = d.DATETIMECREATED,
                                       uploadedBy = staff,
                                       documentTitle = d.DOCUMENT_TITLE,
                                       fileName = d.FILENAME,
                                       fileExtension = d.FILEEXTENSION,
                                       //fileData = d.FILEDATA,
                                       //fileSize = d.fileSize,
                                   })?.ToList();

                var output = firstQuery.Union(secondQuery).ToList();

                if (!isOperationSpecific)
                {
                    output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.OfferLetterApproval, targetId));
                    output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.LoanReviewApprovalOfferLetter, targetId));
                    output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.LoanAvailment, targetId));
                    var facilities = context.TBL_LOAN_APPLICATION_DETAIL.Where(x => x.LOANAPPLICATIONID == targetId).ToList();
                    foreach (var f in facilities)
                    {
                        var request = context.TBL_LOAN_BOOKING_REQUEST.Where(x => x.LOANAPPLICATIONDETAILID == f.LOANAPPLICATIONDETAILID);
                        if (request != null)
                        {
                            foreach (var r in request)
                            {
                                if (r?.OPERATIONID != null) output.AddRange(GetDocumentUploadsByOperation(staffId, (short)r.OPERATIONID, r.LOAN_BOOKING_REQUESTID));
                                output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.IndividualDrawdownRequest, r.LOANAPPLICATIONDETAILID));
                                output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.CorporateDrawdownRequest, r.LOANAPPLICATIONDETAILID));
                                output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.CreditCardDrawdownRequest, r.LOANAPPLICATIONDETAILID));
                                output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.TermLoanBooking, r.LOAN_BOOKING_REQUESTID));
                                output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.RevolvingLoanBooking, r.LOAN_BOOKING_REQUESTID));
                                output.AddRange(GetDocumentUploadsByOperation(staffId, (short)OperationsEnum.ContigentLoanBooking, r.LOAN_BOOKING_REQUESTID));

                            }
                        }
                    }
                }

                return output;
        }


        public IEnumerable<DocumentUploadViewModel> GetDocumentUploadsLmss(int staffId, int operationId, int targetId)
        {

            var firstQuery = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
                , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) =>
               new {
                   documentUploadId = up.DOCUMENTUPLOADID,
                   fileName = up.FILENAME,
                   fileExtension = up.FILEEXTENSION,
                   fileSize = up.FILESIZE,
                   fileSizeUnit = up.FILESIZEUNIT,
                   //fileData = up.FILEDATA,
                   companyId = up.COMPANYID,
                   issueDate = up.ISSUEDATE,
                   expiryDate = up.EXPIRYDATE,
                   physicalFilenumber = up.PHYSICALFILENUMBER,
                   physicalLocation = up.PHYSICALLOCATION,
                   isOriginalCopy = up.ISORIGINALCOPY,
                   documentTypeId = up.DOCUMENTTYPEID,
                   documentTypeName = up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                   documentCategoryId = up.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                   documentCategoryName = up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                   //uploadedBy = context.TBL_STAFF.Where(s=>s.STAFFID == us.CREATEDBY).Select(s=>s.FIRSTNAME +" "+s.MIDDLENAME +" "+s.LASTNAME).FirstOrDefault(),
                   owner = up.CREATEDBY == staffId,
                   dateTimeCreated = us.DATETIMECREATED,
                   dateTimeUpdated = us.DATETIMEUPDATED,
                   createdBy = us.CREATEDBY.Value,
               }).AsEnumerable()
                .Select(up => new DocumentUploadViewModel
                {
                    documentUploadId = up.documentUploadId,
                    fileName = up.fileName,
                    fileExtension = up.fileExtension,
                    fileSize = up.fileSize,
                    fileSizeUnit = up.fileSizeUnit,
                    //fileData = up.fileData,
                    companyId = up.companyId,
                    issueDate = up.issueDate,
                    expiryDate = up.expiryDate,
                    physicalFilenumber = up.physicalFilenumber,
                    physicalLocation = up.physicalLocation,
                    isOriginalCopy = up.isOriginalCopy,
                    documentTypeId = up.documentTypeId,
                    documentTypeName = up.documentTypeName,
                    documentCategoryId = up.documentCategoryId,
                    documentCategoryName = up.documentCategoryName,
                    owner = up.owner,
                    dateTimeCreated = up.dateTimeCreated,
                    dateTimeUpdated = up.dateTimeUpdated,
                    createdBy = up.createdBy,
                    uploadedBy = context.TBL_STAFF.Where(s => s.STAFFID == up.createdBy && s.DELETED != true).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + "(" + s.STAFFCODE + ")").FirstOrDefault(),

                })
                .OrderBy(x => x.dateTimeCreated)
                .ThenBy(x => x.documentCategoryId)
                .ThenBy(x => x.documentTypeId)?
                .ToList();

            return firstQuery;
        }

        private IEnumerable<DocumentUploadViewModel> GetDocumentUploadsByOperation(int staffId, int operationId, int targetId)
        {
            var firstQuery = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
                , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) =>
               new {
                   documentUploadId = up.DOCUMENTUPLOADID,
                   fileName = up.FILENAME,
                   fileExtension = up.FILEEXTENSION,
                   fileSize = up.FILESIZE,
                   fileSizeUnit = up.FILESIZEUNIT,
                   //fileData = up.FILEDATA,
                   companyId = up.COMPANYID,
                   issueDate = up.ISSUEDATE,
                   expiryDate = up.EXPIRYDATE,
                   physicalFilenumber = up.PHYSICALFILENUMBER,
                   physicalLocation = up.PHYSICALLOCATION,
                   isOriginalCopy = up.ISORIGINALCOPY,
                   documentTypeId = up.DOCUMENTTYPEID,
                   documentTypeName = up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                   documentCategoryId = up.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                   documentCategoryName = up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                   owner = up.CREATEDBY == staffId,
                   dateTimeCreated = us.DATETIMECREATED,
                   dateTimeUpdated = us.DATETIMEUPDATED,
                   createdBy = us.CREATEDBY.Value,
               }
            )
            .AsEnumerable()
            .Select(up => new DocumentUploadViewModel
            {
                documentUploadId = up.documentUploadId,
                fileName = up.fileName,
                fileExtension = up.fileExtension,
                fileSize = up.fileSize,
                fileSizeUnit = up.fileSizeUnit,
                //fileData = up.fileData,
                companyId = up.companyId,
                issueDate = up.issueDate,
                expiryDate = up.expiryDate,
                physicalFilenumber = up.physicalFilenumber,
                physicalLocation = up.physicalLocation,
                isOriginalCopy = up.isOriginalCopy,
                documentTypeId = up.documentTypeId,
                documentTypeName = up.documentTypeName,
                documentCategoryId = up.documentCategoryId,
                documentCategoryName = up.documentCategoryName,
                owner = up.owner,
                dateTimeCreated = up.dateTimeCreated,
                dateTimeUpdated = up.dateTimeUpdated,
                createdBy = up.createdBy,
                uploadedBy = context.TBL_STAFF.Where(s => s.STAFFID == up.createdBy && s.DELETED != true).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + "(" + s.STAFFCODE + ")").FirstOrDefault(),

            })
            .OrderBy(x => x.dateTimeCreated)
            .ThenBy(x => x.documentCategoryId)
            .ThenBy(x => x.documentTypeId)?
            .ToList();

            var output = firstQuery; //.Union(secondQuery);

            return output;
        }

        public IEnumerable<DocumentUploadViewModel> GetDocumentDeleted(int staffId, int operationId, int targetId)
        {
            var firstQuery = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == true && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == true)
                , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) =>
                    new {
                        documentUploadId = up.DOCUMENTUPLOADID,
                        fileName = up.FILENAME,
                        fileExtension = up.FILEEXTENSION,
                        fileSize = up.FILESIZE,
                        fileSizeUnit = up.FILESIZEUNIT,
                        //fileData = up.FILEDATA,
                        companyId = up.COMPANYID,
                        issueDate = up.ISSUEDATE,
                        expiryDate = up.EXPIRYDATE,
                        physicalFilenumber = up.PHYSICALFILENUMBER,
                        physicalLocation = up.PHYSICALLOCATION,
                        documentTypeId = up.DOCUMENTTYPEID,
                        documentTypeName = up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                        documentCategoryId = up.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                        documentCategoryName = up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                        owner = us.CREATEDBY == staffId,
                        dateTimeCreated = us.DATETIMECREATED,
                        dateTimeDeleted = us. DATETIMEDELETED,
                        dateTimeUpdated = us.DATETIMEUPDATED,
                        createdBy = us.CREATEDBY.Value,
                        deletedBy = us.DELETEDBY,

                    }
            ).AsEnumerable()
            .Select(up => new DocumentUploadViewModel
            {
                documentUploadId = up.documentUploadId,
                fileName = up.fileName,
                fileExtension = up.fileExtension,
                fileSize = up.fileSize,
                fileSizeUnit = up.fileSizeUnit,
                //fileData = up.fileData,
                companyId = up.companyId,
                issueDate = up.issueDate,
                expiryDate = up.expiryDate,
                physicalFilenumber = up.physicalFilenumber,
                physicalLocation = up.physicalLocation,
                documentTypeId = up.documentTypeId,
                documentTypeName = up.documentTypeName,
                documentCategoryId = up.documentCategoryId,
                documentCategoryName = up.documentCategoryName,
                owner = up.owner,
                dateTimeCreated = up.dateTimeCreated,
                dateTimeDeleted = up.dateTimeDeleted,
                dateTimeUpdated = up.dateTimeUpdated,
                createdBy = up.createdBy,
                uploadedBy = context.TBL_STAFF.Where(s => s.STAFFID == up.deletedBy && s.DELETED != true).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + "(" + s.STAFFCODE + ")").FirstOrDefault(),

            })
            .OrderBy(x => x.dateTimeCreated)
            .ThenBy(x => x.documentCategoryId)
            .ThenBy(x => x.documentTypeId)
            .ToList();

            var customerId = (from ccb in context.TBL_CUSTOMER_CREDIT_BUREAU
                              join app in context.TBL_LOAN_APPLICATION_DETAIL on ccb.CUSTOMERID equals app.CUSTOMERID
                              where app.LOANAPPLICATIONID == targetId
                              select ccb.CUSTOMERID).ToList();

            var customerBureauLog = creditBureau.GetCustomerCreditBureauReportLogDeleted(customerId.FirstOrDefault(), null).Select(x => x.customerCreditBureauId).ToList();

            var staff = (from x in context.TBL_STAFF
                         join app in context.TBL_LOAN_APPLICATION_DETAIL on x.STAFFID equals app.CREATEDBY
                         where app.LOANAPPLICATIONID == targetId
                         select x).FirstOrDefault();

            var secondQuery = new List<DocumentUploadViewModel>();

            if (customerBureauLog.Count > 0) {
                secondQuery = (from d in docContext.TBL_CUSTOMER_CREDIT_BUREAU
                                   where customerBureauLog.Contains(d.CUSTOMERCREDITBUREAUID)
                                   select new DocumentUploadViewModel
                                   {
                                       documentUploadId = d.DOCUMENTID,
                                       documentTypeName = "CREDIT BUREAU",
                                       documentCategoryName = "CREDIT BUREAU",
                                       dateTimeCreated = d.DATETIMECREATED,
                                       customerCreditBureauId = d.CUSTOMERCREDITBUREAUID,
                                       //uploadedBy = staff,
                                       uploadedBy = staff.FIRSTNAME + " " + staff.LASTNAME,
                                       documentTitle = d.DOCUMENT_TITLE,
                                       fileName = d.FILENAME,
                                       fileExtension = d.FILEEXTENSION,
                                       owner = staff.STAFFID == staffId,
                                       createdBy = staff.STAFFID,
                                       //fileData = d.FILEDATA,
                                       //fileSize = d.fileSize,
                                   }).ToList();

                foreach (var item in secondQuery)
                {
                    item.dateTimeDeleted = context.TBL_CUSTOMER_CREDIT_BUREAU.FirstOrDefault(O => O.CUSTOMERCREDITBUREAUID == item.customerCreditBureauId)?.DATETIMEDELETED;
                }
            }
            
            return firstQuery.Union(secondQuery);
        }

        public IEnumerable<DocumentUploadViewModel> GetDocumentUploads(int staffId)
        {
            //var staffs = context.TBL_STAFF.ToList();
            return docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false)
                .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
                , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) => new { us, up }
            )
                .Select(x => new DocumentUploadViewModel
                {
                    documentUploadId = x.up.DOCUMENTUPLOADID,
                    fileName = x.up.FILENAME,
                    fileExtension = x.up.FILEEXTENSION,
                    fileSize = x.up.FILESIZE,
                    fileSizeUnit = x.up.FILESIZEUNIT,
                    //fileData = x.up.FILEDATA,
                    companyId = x.up.COMPANYID,
                    issueDate = x.up.ISSUEDATE,
                    expiryDate = x.up.EXPIRYDATE,
                    documentCategoryId = x.us.OPERATIONID, // TODO
                    documentTypeId = x.up.DOCUMENTTYPEID,
                    physicalFilenumber = x.up.PHYSICALFILENUMBER,
                    physicalLocation = x.up.PHYSICALLOCATION,
                    isOriginalCopy = x.up.ISORIGINALCOPY,
                    documentTypeName = x.up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                    documentCategoryName = x.up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                    owner = x.us.CREATEDBY == staffId,
                    //uploadedBy = staffs.Where(s => s.STAFFID == x.up.CREATEDBY && s.DELETED != true).Select(s => s.FIRSTNAME + s.LASTNAME).FirstOrDefault(),
                    dateTimeCreated = (DateTime)x.up.DATETIMECREATED

                })
                .OrderBy(x => x.dateTimeCreated)
                .ThenBy(x => x.documentTypeId)
                .ToList();
        }

        public DocumentUploadViewModel GetDocumentUpload(int id)
        {
            //var staffs = context.TBL_STAFF.ToList();
            var entity = docContext.TBL_DOCUMENT_UPLOAD.FirstOrDefault(x => x.DOCUMENTUPLOADID == id && x.DELETED == false);

            return new DocumentUploadViewModel
            {
                documentUploadId = entity.DOCUMENTUPLOADID,
                fileName = entity.FILENAME,
                fileExtension = entity.FILEEXTENSION,
                fileSize = entity.FILESIZE,
                fileSizeUnit = entity.FILESIZEUNIT,
                fileData = entity.FILEDATA,
                companyId = entity.COMPANYID,
                issueDate = entity.ISSUEDATE,
                expiryDate = entity.EXPIRYDATE,
                isOriginalCopy = entity.ISORIGINALCOPY,
                physicalFilenumber = entity.PHYSICALFILENUMBER,
                physicalLocation = entity.PHYSICALLOCATION,
                //uploadedBy = staffs.Where(s => s.STAFFID == entity.CREATEDBY && s.DELETED != true).Select(s => s.FIRSTNAME + s.LASTNAME).FirstOrDefault(),
                dateTimeCreated = (DateTime)entity.DATETIMECREATED

            };
        }

        public IEnumerable<DocumentUploadViewModel> GetDocumentUpload(IEnumerable<DocumentUploadViewModel> model)
        {
            //var staffs = context.TBL_STAFF.ToList();
            var documents = new List<DocumentUploadViewModel>();
            foreach (var o in model)
            {
                var entity = (from x in docContext.TBL_DOCUMENT_UPLOAD
                              join d in docContext.TBL_DOCUMENT_TYPE on x.DOCUMENTTYPEID equals d.DOCUMENTTYPEID
                              where x.DOCUMENTUPLOADID == o.documentUploadId && x.DELETED == false
                              select new DocumentUploadViewModel
                              {
                                  documentUploadId = x.DOCUMENTUPLOADID,
                                  fileName = x.FILENAME,
                                  fileExtension = x.FILEEXTENSION,
                                  fileSize = x.FILESIZE,
                                  fileSizeUnit = x.FILESIZEUNIT,
                                  //fileData = x.FILEDATA,
                                  companyId = x.COMPANYID,
                                  issueDate = x.ISSUEDATE,
                                  expiryDate = x.EXPIRYDATE,
                                  physicalFilenumber = x.PHYSICALFILENUMBER,
                                  isOriginalCopy = x.ISORIGINALCOPY,
                                  physicalLocation = x.PHYSICALLOCATION,
                                  documentCategoryName = docContext.TBL_DOCUMENT_CATEGORY.Where(a=>a.DOCUMENTCATEGORYID==d.DOCUMENTCATEGORYID).Select(a=>a.DOCUMENTCATEGORYNAME).FirstOrDefault(),
                                  documentTypeName = d.DOCUMENTTYPENAME,
                                  //uploadedBy = staffs.Where(s => s.STAFFID == x.CREATEDBY && s.DELETED != true).Select(s => s.FIRSTNAME + s.LASTNAME).FirstOrDefault(),
                                  dateTimeCreated = (DateTime)x.DATETIMECREATED

                              }).FirstOrDefault();

                documents.Add(entity);
            }
            return documents;
        }

        public int AddDocumentUpload(DocumentUploadViewModel model, byte[] buffer)
        {

                var customerCode = String.Empty;
                if (model.customerId > 0)
                {
                    customerCode = GetCustomerCode(model.customerId);
                }
                else
                {
                    customerCode = GetCustomerGroupCode(model.customerGroupId);
                }

                var existing = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false
                        && x.OPERATIONID == model.operationId
                        && x.TARGETID == model.targetId
                        && x.CUSTOMERCODE == customerCode)
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
                    ISORIGINALCOPY = model.isOriginalCopy,
                    DOCUMENTTYPEID = model.documentTypeId,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = general.GetApplicationDate(),
                    SOURCE = model.source
                };
                if (model.edmsDocumentId != null)
                {
                    entity.EDMSDOCID = model.edmsDocumentId;
                }



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
                        CUSTOMERCODE = customerCode,
                        OPERATIONID = model.operationId,
                        APPROVALSTATUSID = model.approvalStatusId,
                        DOCUMENTSTATUSID = model.documentStatusId,
                        ISPRIMARYDOCUMENT = model.isPrimaryDocument,
                        CREATEDBY = model.createdBy,
                        DATETIMECREATED = general.GetApplicationDate(),
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

                        var documentCategory = docContext.TBL_DOCUMENT_TYPE.Where(t => t.DOCUMENTTYPEID == model.documentTypeId).FirstOrDefault().DOCUMENTCATEGORYID;
                        model.documentCategoryId = documentCategory;
                        var docDeffered = docContext.TBL_DEFERRED_DOC_TRACKER.Where(x => x.DELETED == false
                        && x.DOCUMENTCATEGORYID == model.documentCategoryId
                        && x.DOCUMENTTYPEID == model.documentTypeId && x.LOANAPPLICATIONID == model.targetId);
                        if (docDeffered.Any())
                        {
                             var deferredDoc = docContext.TBL_DEFERRED_DOC_TRACKER.Where(x => x.DELETED == false
                             && x.DOCUMENTCATEGORYID == model.documentCategoryId
                             && x.DOCUMENTTYPEID == model.documentTypeId && x.LOANAPPLICATIONID == model.targetId).FirstOrDefault();

                            if (deferredDoc != null)
                            {
                                deferredDoc.DOCUMENTCATEGORYID = model.documentCategoryId;
                                deferredDoc.DOCUMENTTYPEID = model.documentTypeId;
                                deferredDoc.DATETIMESUBMITTED = DateTime.Now;
                                deferredDoc.SUBMITTED = true;
                                deferredDoc.CREATEDBY = model.createdBy;

                            }

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


        public int AddSDDocumentUpload(DocumentUploadViewModel model, byte[] buffer)
        {

            var customerCode = String.Empty;
            if (model.customerId > 0)
            {
                customerCode = GetCustomerCode(model.customerId);
            }
            else
            {
                customerCode = GetCustomerGroupCode(model.customerGroupId);
            }

            var existing = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false
                    && x.OPERATIONID == model.operationId
                    && x.TARGETID == model.targetId
                    && x.CUSTOMERCODE == customerCode)
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
                ISORIGINALCOPY = model.isOriginalCopy,
                DOCUMENTTYPEID = model.documentTypeId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
                SOURCE = model.source
            };
            if (model.edmsDocumentId != null)
            {
                entity.EDMSDOCID = model.edmsDocumentId;
            }



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
                    CUSTOMERCODE = customerCode,
                    OPERATIONID = model.operationId,
                    APPROVALSTATUSID = model.approvalStatusId,
                    DOCUMENTSTATUSID = model.documentStatusId,
                    ISPRIMARYDOCUMENT = model.isPrimaryDocument,
                    CREATEDBY = model.createdBy,
                    DATETIMECREATED = general.GetApplicationDate(),
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

            var sdcCode = GenerateSDCode();
            sdcCode = "CSD" + sdcCode;

            var stampDuty = context.TBL_FACILITY_STAMP_DUTY.Where(s => s.FACILITYSTAMPDUTYID == model.targetId).FirstOrDefault();
            if (stampDuty != null)
            {
                stampDuty.CSDC = sdcCode;
                stampDuty.DATETIMEUPDATED = DateTime.Now;
                stampDuty.CURRENTSTATUS = 3;
            }
            model.csdc = sdcCode;
            context.SaveChanges();


            return 2;

        }



        private string GenerateSDCode()
        {
            var fsd = context.TBL_CODE_TRACKER.OrderByDescending(x => x.CODEID).FirstOrDefault();

            DateTime lastGeneratedDate = fsd.CURRENTDATE;//DateTime.MinValue;
            int lastGeneratedNumber = 0 ;
            if (fsd != null) { lastGeneratedNumber = fsd.CSDC; }

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

            fsd.CSDC = lastGeneratedNumber;
            fsd.CURRENTDATE = lastGeneratedDate;
            context.SaveChanges();

            return serialNumber;


        }

        public bool GoForBulkApproval(FacilityStampDutyViewModel data)
        {
            using (var trans = context.Database.BeginTransaction())
            {
                try
                {
                    

                   
                        var response = CloseStampDuty(data);

                        if (response)
                        {
                            trans.Commit();
                            return true;
                        }
                       
                    
                    else
                    {
                        trans.Rollback();
                    }

                    return false;
                }
                catch (Exception ex)
                {
                   
                    throw new SecureException(ex.Message);
                }
            }
        }

        private bool CloseStampDuty(FacilityStampDutyViewModel data)
        {
            
            try
            {
                var sdcCode = GenerateSDCode();
                sdcCode = "CSD" + sdcCode;

                var stampDuty = context.TBL_FACILITY_STAMP_DUTY.Where(s => s.FACILITYSTAMPDUTYID == data.facilityStampDutyId).FirstOrDefault();
                if (stampDuty != null)
                {
                    stampDuty.CSDC = sdcCode;
                    stampDuty.DATETIMEUPDATED = DateTime.Now;
                    stampDuty.CURRENTSTATUS = 3;
                }
                data.csdc = sdcCode;
                context.SaveChanges();


                var response = context.SaveChanges() >= 0;
               

                context.SaveChanges();
                if (response)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new SecureException(ex.Message);
            }
        }

        private  async Task<DocumentUploadViewModelResut> AddDocumentUploadToSubsidiary(DocumentUploadViewModel model, byte[] buffer, string token, MultipartFormDataContent formContent)
        {
            var response = new DocumentUploadViewModelResut();
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient httpClient = new HttpClient(handler);
           // using (HttpClient httpClient = new HttpClient())
            try{

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", token);
                var url = getUrl(model.countryCode);
                var responseString = await httpClient.PostAsync(url + "/api/v1/document/document-upload", formContent).ConfigureAwait(false);
                var result = await responseString.Content.ReadAsAsync<DocumentUploadViewModelResut>();
                
                if (responseString.IsSuccessStatusCode)
                {
                    
                    response = result;
                }

                return response;
            }
            catch (APIErrorException ex)
            {
                throw new APIErrorException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new APIErrorException($"Error" + ex.Message);
            }
            finally
            {
                handler.Dispose();
                httpClient.Dispose();
            }
        }

        public DocumentUploadViewModelResut AddDocumentUploadToSubsidiaryResult(DocumentUploadViewModel model, byte[] buffer, string token, MultipartFormDataContent formContent)
        {
            DocumentUploadViewModelResut fileUpload = new DocumentUploadViewModelResut();
            Task.Run(async () => fileUpload = await AddDocumentUploadToSubsidiary(model, buffer, token, formContent))
                .GetAwaiter().GetResult();
            return fileUpload;
        }

        private string GetCustomerCode(int customerId)
        {
            var customer = context.TBL_CUSTOMER.FirstOrDefault(x => x.CUSTOMERID == customerId);
            if (customer == null) throw new SecureException("Customer not found!");
            return customer.CUSTOMERCODE;
        }

        private string GetCustomerGroupCode(int customerGroupId)
        {
            var customerGroup = context.TBL_CUSTOMER_GROUP.FirstOrDefault(x => x.CUSTOMERGROUPID == customerGroupId);
            if (customerGroup == null) throw new SecureException("Customer not found!");
            return customerGroup.GROUPCODE;
        }

        public bool UpdateDocumentUpload(DocumentUploadViewModel model, int id, UserInfo user)
        {
            var entity = this.docContext.TBL_DOCUMENT_UPLOAD.Find(id);
            entity.FILENAME = model.fileName;
            entity.FILEEXTENSION = model.fileExtension;
            entity.FILESIZE = model.fileSize;
            entity.FILESIZEUNIT = model.fileSizeUnit;
            entity.FILEDATA = model.fileData;
            entity.COMPANYID = model.companyId;
            entity.ISSUEDATE = model.issueDate;
            entity.EXPIRYDATE = model.expiryDate;
            entity.ISORIGINALCOPY = model.isOriginalCopy;
            entity.PHYSICALFILENUMBER = model.physicalFilenumber;
            entity.PHYSICALLOCATION = model.physicalLocation;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            // Audit Section ---------------------------
            this.audit.AddAuditTrail(new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentUploadUpdated,
                STAFFID = user.createdBy,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"TBL_Document Upload '{model.targetCode}' was updated by {auditStaff}",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.DOCUMENTUPLOADID,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteDocumentUpload(int id, int documentTypeId, UserInfo user)
        {
            var usageCount = 0;
            var creditBureauDoc = docContext.TBL_DOCUMENT_TYPE.FirstOrDefault(O => O.DOCUMENTTYPEID == documentTypeId);

            if (creditBureauDoc.DOCUMENTTYPENAME.ToUpper() == "CREDIT BUREAU")
            {
                var docCreditBureau = docContext.TBL_CUSTOMER_CREDIT_BUREAU.FirstOrDefault(O => O.DOCUMENTID == id);

                if (docCreditBureau != null)
                {
                    var creditBureau = context.TBL_CUSTOMER_CREDIT_BUREAU.FirstOrDefault(O => O.CUSTOMERCREDITBUREAUID == docCreditBureau.CUSTOMERCREDITBUREAUID);

                    if (creditBureau != null)
                    {
                        creditBureau.DELETED = true;
                        creditBureau.DELETEDBY = user.createdBy;
                        creditBureau.DATETIMEDELETED = DateTime.Now;
                    }
                }

                return docContext.SaveChanges() > 0;
            }


            var usage = docContext.TBL_DOCUMENT_USAGE.FirstOrDefault(u => u.DOCUMENTUPLOADID == id);

            if (usage != null)
            {
                usage.DELETED = true;
                usage.DELETEDBY = user.createdBy;
                usage.DATETIMEDELETED = DateTime.Now;
                docContext.SaveChanges();

                usageCount = docContext.TBL_DOCUMENT_USAGE
                  .Where(u => u.DOCUMENTUPLOADID == usage.DOCUMENTUPLOADID && u.DELETED == false)
                  .Count();
            }

            if (usageCount < 1)
            {
                var upload = docContext.TBL_DOCUMENT_UPLOAD.Find(id);
                upload.DELETED = true;
                upload.DELETEDBY = user.createdBy;
                upload.DATETIMEDELETED = DateTime.Now;
            }

            return docContext.SaveChanges() > 0;
        }

        public bool DeleteRecoveryDocumentUpload(int id)
        {
            var usage = docContext.TBL_LOAN_RECOVERY_REPORTING_DOCUMENT.Find(id);

            if (usage != null)
            {
                docContext.TBL_LOAN_RECOVERY_REPORTING_DOCUMENT.Remove(usage);
            }

            return docContext.SaveChanges() > 0;
        }

        public DocumentUploadViewModel GetDocument(int documentId)
        {
            return (from x in docContext.TBL_DOCUMENT_UPLOAD
                    where x.DOCUMENTUPLOADID == documentId
                    select new DocumentUploadViewModel
                    {
                        documentTypeId = x.DOCUMENTTYPEID,
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                    })
                         .FirstOrDefault();
        }

        public DocumentUploadViewModel GetDocumentCreditBereau(int documentId)
        {
            return (from x in docContext.TBL_CUSTOMER_CREDIT_BUREAU
                    where x.DOCUMENTID == documentId
                    select new DocumentUploadViewModel
                    {

                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                    })
                         .FirstOrDefault();
        }

        public IEnumerable<DocumentCategoryViewModel> GetDocumentCategories()
        {
            return docContext.TBL_DOCUMENT_CATEGORY.Where(x => x.DELETED == false)
                            .Select(x => new DocumentCategoryViewModel
                            {
                                documentCategoryId = x.DOCUMENTCATEGORYID,
                                documentCategoryName = x.DOCUMENTCATEGORYNAME,
                            }).OrderBy(l => l.documentCategoryName);
        }

        public IEnumerable<DocumentTypeViewModel> GetDocumentTypes(int id)
        {
            return docContext.TBL_DOCUMENT_TYPE.Where(x => x.DELETED == false && x.DOCUMENTCATEGORYID == id)
                            .Select(x => new DocumentTypeViewModel
                            {
                                documentTypeId = x.DOCUMENTTYPEID,
                                documentTypeName = x.DOCUMENTTYPENAME,
                            }).OrderBy(l => l.documentTypeName);
        }

        public CustomerDocumentSearchViewModel GetCustomerDocuments(DocumentUploadViewModel model, UserInfo user)
        {
            var customers = context.TBL_CUSTOMER
                .Join(context.TBL_CASA, a => a.CUSTOMERID, b => b.CUSTOMERID, (a, b) => new { a, b })
                .Select(x => new
                {
                    CUSTOMERCODE = x.a.CUSTOMERCODE,
                    FIRSTNAME = x.a.FIRSTNAME,
                    MIDDLENAME = x.a.MIDDLENAME,
                    LASTNAME = x.a.LASTNAME,
                    PRODUCTACCOUNTNUMBER = x.b.PRODUCTACCOUNTNUMBER,
                })
                .Where(c => c.CUSTOMERCODE.Trim() == model.customerCode.Trim() || c.PRODUCTACCOUNTNUMBER == model.customerCode || c.FIRSTNAME.ToLower().Contains(model.customerCode.ToLower().Trim()) || c.MIDDLENAME.ToLower().Contains(model.customerCode.ToLower().Trim()) || c.LASTNAME.ToLower().Contains(model.customerCode.ToLower().Trim()))
                //.Where(c => c.CUSTOMERCODE == model.customerCode || c.PRODUCTACCOUNTNUMBER == model.customerCode || c.FIRSTNAME.ToLower().Contains(model.customerCode.ToLower().Trim()) || model.customerCode.ToLower().Contains(c.FIRSTNAME.ToLower().Trim()))
                .ToList();

            if (customers == null) throw new SecureException("Customer not found!");

            CustomerDocumentSearchViewModel result = new CustomerDocumentSearchViewModel();
            result.customerName = model.customerCode;
            //result.customerName = customers.FIRSTNAME + " " + customers.MIDDLENAME + " " + customers.LASTNAME;

            //var usageQuery = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.CUSTOMERCODE == customer.CUSTOMERCODE)
            //                .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
            //                , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) => new { us, up }
            //            )
            //                .Select(x => new DocumentUploadViewModel
            //                {
            //                    documentUploadId = x.up.DOCUMENTUPLOADID,
            //                    fileName = x.up.FILENAME,
            //                    fileExtension = x.up.FILEEXTENSION,
            //                    fileSize = x.up.FILESIZE,
            //                    fileSizeUnit = x.up.FILESIZEUNIT,
            //                    // fileData = x.up.FILEDATA,
            //                    companyId = x.up.COMPANYID,
            //                    issueDate = x.up.ISSUEDATE,
            //                    expiryDate = x.up.EXPIRYDATE,
            //                    physicalFilenumber = x.up.PHYSICALFILENUMBER,
            //                    physicalLocation = x.up.PHYSICALLOCATION,
            //                    documentTypeId = x.up.DOCUMENTTYPEID,
            //                    documentTypeName = x.up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
            //                    documentCategoryId = x.up.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
            //                    documentCategoryName = x.up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
            //                    owner = x.us.CREATEDBY == model.createdBy
            //                });
            IEnumerable<DocumentUploadViewModel> usageQuery = null;
            var usageTemp = new List<DocumentUploadViewModel>();
            foreach (var customer in customers)
            {
                var usage = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.CUSTOMERCODE.Trim() == customer.CUSTOMERCODE.Trim())
               .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
               , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) =>
                   new
                   {
                       documentUploadId = up.DOCUMENTUPLOADID,
                       fileName = up.FILENAME,
                       fileExtension = up.FILEEXTENSION,
                       fileSize = up.FILESIZE,
                       fileSizeUnit = up.FILESIZEUNIT,
                        //fileData = up.FILEDATA,
                        companyId = up.COMPANYID,
                       issueDate = up.ISSUEDATE,
                       expiryDate = up.EXPIRYDATE,
                       physicalFilenumber = up.PHYSICALFILENUMBER,
                       physicalLocation = up.PHYSICALLOCATION,
                       documentTypeId = up.DOCUMENTTYPEID,
                       documentTypeName = up.TBL_DOCUMENT_TYPE.DOCUMENTTYPENAME,
                       documentCategoryId = up.TBL_DOCUMENT_TYPE.DOCUMENTCATEGORYID,
                       documentCategoryName = up.TBL_DOCUMENT_TYPE.TBL_DOCUMENT_CATEGORY.DOCUMENTCATEGORYNAME,
                       owner = us.CREATEDBY == model.createdBy,
                       dateTimeCreated = us.DATETIMECREATED,
                       dateTimeUpdated = us.DATETIMEUPDATED,
                       createdBy = us.CREATEDBY.Value,
                   }
                   ).AsEnumerable()
                   .Select(up => new DocumentUploadViewModel
                   {
                       documentUploadId = up.documentUploadId,
                       fileName = up.fileName,
                       fileExtension = up.fileExtension,
                       fileSize = up.fileSize,
                       fileSizeUnit = up.fileSizeUnit,
                        //fileData = up.fileData,
                        companyId = up.companyId,
                       issueDate = up.issueDate,
                       expiryDate = up.expiryDate,
                       physicalFilenumber = up.physicalFilenumber,
                       physicalLocation = up.physicalLocation,
                       documentTypeId = up.documentTypeId,
                       documentTypeName = up.documentTypeName,
                       documentCategoryId = up.documentCategoryId,
                       documentCategoryName = up.documentCategoryName,
                       owner = up.owner,
                       dateTimeCreated = up.dateTimeCreated,
                       dateTimeUpdated = up.dateTimeUpdated,
                       createdBy = up.createdBy,
                       uploadedBy = context.TBL_STAFF.Where(s => s.STAFFID == up.createdBy && s.DELETED != true).Select(s => s.FIRSTNAME + " " + s.LASTNAME + " " + "(" + s.STAFFCODE + ")").FirstOrDefault(),
                       customerName = customer.FIRSTNAME + " " + customer.MIDDLENAME + " " + customer.LASTNAME,
                   }).ToList();
                usageTemp.AddRange(usage);
            }
            usageQuery = usageTemp;
            int documentCategoryId = model.documentCategoryId;
            int documentTypeId = model.documentTypeId;

            if (documentCategoryId > 0) usageQuery = usageQuery.Where(x => x.documentCategoryId == documentCategoryId);
            if (documentTypeId > 0) usageQuery = usageQuery.Where(x => x.documentCategoryId == documentCategoryId && x.documentTypeId == documentTypeId);

            result.documents = usageQuery.OrderBy(x => x.dateTimeCreated)
                                            .ThenBy(x => x.documentCategoryId)
                                            .ThenBy(x => x.documentTypeId)
                                            .ToList();

            return result;
        }

        //public DocumentUploadViewModel GetUploadedDocument(DocumentUploadViewModel model)
        //{
        //   return (from x in docContext.TBL_DOCUMENT_UPLOAD
        //                where x.DOCUMENTUPLOADID == model.documentUploadId
        //                select new DocumentUploadViewModel
        //                {
        //                    documentTypeId = x.DOCUMENTTYPEID,
        //                    fileData = x.FILEDATA,
        //                    fileName = x.FILENAME,
        //                    fileExtension = x.FILEEXTENSION,
        //                })
        //                .FirstOrDefault();
        //}


        public int AddRecoveryReportingDocumentUpload(RecoveryReportingDocumentViewModel model, byte[] buffer)
        {
            var existing = docContext.TBL_LOAN_RECOVERY_REPORTING_DOCUMENT.Where(x => x.FILENAME == model.fileName
                    && x.TARGETID == model.targetId
                    && x.REFERENCEID == model.referenceId)
            .Select(x => new RecoveryReportingDocumentViewModel
            {
                loanRecoveryReportingDocumentId = x.LOANRECOVERYREPORTDOCUMENTID,
                description = x.DESCRIPTION,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                fileSize = x.FILESIZE,
                referenceId = x.REFERENCEID,
            })
                .FirstOrDefault();

            if (existing != null && model.overwrite == false) return 3;

            var entity = new TBL_LOAN_RECOVERY_REPORTING_DOCUMENT
            {
                FILENAME = model.fileName,
                FILEEXTENSION = model.fileExtension.ToLower(),
                FILESIZE = model.fileSize,
                FILESIZEUNIT = "kilobyte",
                FILEDATA = buffer,
                REFERENCEID = model.referenceId,
                TARGETID = model.targetId,
                OPERATIONID = model.operationId,
                DESCRIPTION = model.description,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            docContext.TBL_LOAN_RECOVERY_REPORTING_DOCUMENT.Add(entity);
            docContext.SaveChanges(); 

            return 2;
        }


        public IEnumerable<RecoveryReportingDocumentViewModel> getAllLoanRecoveryReportingDocuments(string referenceId)
        {
            var records = (from x in docContext.TBL_LOAN_RECOVERY_REPORTING_DOCUMENT
                           where x.REFERENCEID == referenceId
                           select new 
                           {
                               fileName = x.FILENAME,
                               description = x.DESCRIPTION,
                               loanRecoveryReportingDocumentId = x.LOANRECOVERYREPORTDOCUMENTID,
                               referenceId = x.REFERENCEID,
                               fileExtension = x.FILEEXTENSION,
                               fileSize = x.FILESIZE,
                               targetId = x.TARGETID,
                               createdBy = x.CREATEDBY,
                               dateTimeCreated = x.DATETIMECREATED
                           }).AsEnumerable().Select(a => new RecoveryReportingDocumentViewModel
                           {
                               fileName = a.fileName,
                               description = a.description,
                               loanRecoveryReportingDocumentId = a.loanRecoveryReportingDocumentId,
                               referenceId = a.referenceId,
                               fileExtension = a.fileExtension,
                               fileSize = a.fileSize,
                               targetId = a.targetId,
                               createdBy = a.createdBy,
                               dateTimeCreated = a.dateTimeCreated
                           }).ToList();

            var result = records.Select(a => new RecoveryReportingDocumentViewModel
            {
                fileName = a.fileName,
                description = a.description,
                loanRecoveryReportingDocumentId = a.loanRecoveryReportingDocumentId,
                referenceId = a.referenceId,
                fileExtension = a.fileExtension,
                fileSize = a.fileSize,
                targetId = a.targetId,
                dateTimeCreated = a.dateTimeCreated,
                uploadedBy = context.TBL_STAFF.Where(s => s.STAFFID == a.createdBy).Select(s => s.FIRSTNAME + " " + s.MIDDLENAME + " " + s.LASTNAME).FirstOrDefault(),
            }).ToList();

            return result;
        }

        public RecoveryReportingDocumentViewModel GetRecoveryReportDocument(int loanRecoveryReportApprovalId)
        {
            return docContext.TBL_LOAN_RECOVERY_REPORTING_DOCUMENT.Where(x => x.LOANRECOVERYREPORTDOCUMENTID == loanRecoveryReportApprovalId)
                            .Select(x => new RecoveryReportingDocumentViewModel
                            {
                                fileName = x.FILENAME,
                                description = x.DESCRIPTION,
                                loanRecoveryReportingDocumentId = x.LOANRECOVERYREPORTDOCUMENTID,
                                referenceId = x.REFERENCEID,
                                fileExtension = x.FILEEXTENSION,
                                fileSize = x.FILESIZE,
                                fileData = x.FILEDATA,
                            }).FirstOrDefault();
        }

        public bool AddDeferredDocument(DeferredDocumentsViewModel model, UserInfo user)
        {
            try
            {
                var existing = docContext.TBL_DEFERRED_DOC_TRACKER.Where(x => x.DELETED == false
                        && x.DOCUMENTCATEGORYID == model.documentCategoryId
                        && x.DOCUMENTTYPEID == model.documentTypeId);
                if (existing.Any())
                {
                    this.UpdateDeferredDocument(model, model.deferredDodId, user);
                    return true;
                }
                else
                {
                    model.datetimeCreated = DateTime.Now;

                    var deferredDoc = new TBL_DEFERRED_DOC_TRACKER
                    {
                        DOCUMENTCATEGORYID = model.documentCategoryId,
                        DOCUMENTTYPEID = model.documentTypeId,
                        LOANAPPLICATIONID = model.loanApplicationId,
                        DUEDATE = DateTime.Now.AddDays(model.tenor),
                        CREATEDBY = user.createdBy,
                        DATETIMECREATED = model.datetimeCreated,

                    };
                    docContext.TBL_DEFERRED_DOC_TRACKER.Add(deferredDoc);

                }


                if (docContext.SaveChanges() > 0)
                {
                    // Audit Section ---------------------------
                    var audit = new TBL_AUDIT
                    {
                        AUDITTYPEID = (short)AuditTypeEnum.DigitalStampUpload,
                        STAFFID = model.createdBy,
                        BRANCHID = (short)user.BranchId,
                        DETAIL = $"Deferred Document added successfully by staff with STAFFID {model.createdBy}",
                        URL = model.applicationUrl,
                        APPLICATIONDATE = general.GetApplicationDate(),
                        SYSTEMDATETIME = DateTime.Now,
                        DEVICENAME = CommonHelpers.GetDeviceName(),
                        OSNAME = CommonHelpers.FriendlyName()
                    };

                    this.audit.AddAuditTrail(audit);
                    return true;
                }
                return false;
            }
            catch (Exception ex) { throw ex; }
            
        }

        public bool UpdateDeferredDocument(DeferredDocumentsViewModel model, int id, UserInfo user)
        {
            var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            var deferredDoc = docContext.TBL_DEFERRED_DOC_TRACKER.Find(id);
            
            if (deferredDoc != null)
            {
                deferredDoc.DOCUMENTCATEGORYID = model.documentCategoryId;
                deferredDoc.DOCUMENTTYPEID = model.documentTypeId;
                deferredDoc.DATETIMEUPDATED = DateTime.Now;
                deferredDoc.DUEDATE = DateTime.Now.AddDays(model.tenor);
                deferredDoc.LASTUPDATEDBY = user.createdBy;


                

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Deferred Document was updated by {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = model.loanApplicationId,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.audit.AddAuditTrail(audit);
            }
            if (docContext.SaveChanges() > 0) return true;
            return false;
        }

        private bool SubmitDeferredDocument(DocumentUploadViewModel model)
        {
            var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == model.createdBy).Select(x => x.STAFFCODE));
            var deferredDoc = docContext.TBL_DEFERRED_DOC_TRACKER.Where(x => x.DELETED == false
                        && x.DOCUMENTCATEGORYID == model.documentCategoryId
                        && x.DOCUMENTTYPEID == model.documentTypeId && x.LOANAPPLICATIONID == model.targetId).FirstOrDefault();

            if (deferredDoc != null)
            {
                deferredDoc.DOCUMENTCATEGORYID = model.documentCategoryId;
                deferredDoc.DOCUMENTTYPEID = model.documentTypeId;
                deferredDoc.DATETIMEDELETED = DateTime.Now;
                deferredDoc.SUBMITTED = true;
                deferredDoc.CREATEDBY = model.createdBy;

            }
            if (docContext.SaveChanges() > 0) return true;
            return false;
        }

        public IEnumerable<DeferredDocumentsViewModel> GetAllDeferredDocuments()
        {
            var deferredDocs = (from x in docContext.TBL_DEFERRED_DOC_TRACKER
                                where x.DELETED == false 
                                && x.SUBMITTED == false
                                select new DeferredDocumentsViewModel
                                {
                                    deferredDodId = x.DEFERREDDOCID,
                                    documentCategoryId = x.DOCUMENTCATEGORYID,
                                    documentCategoryName = docContext.TBL_DOCUMENT_CATEGORY.Where(c => c.DOCUMENTCATEGORYID == x.DOCUMENTCATEGORYID).FirstOrDefault().DOCUMENTCATEGORYNAME,
                                    documentTypeId = x.DOCUMENTTYPEID,
                                    documentTypeName = docContext.TBL_DOCUMENT_TYPE.Where(t => t.DOCUMENTTYPEID == x.DOCUMENTTYPEID).FirstOrDefault().DOCUMENTTYPENAME,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    dueDate = x.DUEDATE,
                                    datetimeCreated = x.DATETIMECREATED,
                                }).ToList();
            foreach (var doc in deferredDocs)
            {
                doc.applicationReferenceNumber = context.TBL_LOAN_APPLICATION.Where(l => l.LOANAPPLICATIONID == doc.loanApplicationId).FirstOrDefault().APPLICATIONREFERENCENUMBER;
                var appDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == doc.loanApplicationId).ToList();
                doc.facilityTypeId = appDetail[0].PROPOSEDPRODUCTID;
                doc.facilityTypeName = context.TBL_PRODUCT.Where(p => p.PRODUCTID == doc.facilityTypeId).FirstOrDefault().PRODUCTNAME;
            }
            return deferredDocs;
        }

        public IEnumerable<DeferredDocumentsViewModel> GetDeferredDocumentsByLoandApplicationId(int loanApplicationId)
        {
            var deferredDocs = (from x in docContext.TBL_DEFERRED_DOC_TRACKER
                                where x.DELETED == false && x.LOANAPPLICATIONID == loanApplicationId
                                && x.SUBMITTED == false
                                select new DeferredDocumentsViewModel
                                {
                                    deferredDodId = x.DEFERREDDOCID,
                                    documentCategoryId = x.DOCUMENTCATEGORYID,
                                    documentCategoryName = docContext.TBL_DOCUMENT_CATEGORY.Where(c => c.DOCUMENTCATEGORYID == x.DOCUMENTCATEGORYID).FirstOrDefault().DOCUMENTCATEGORYNAME,
                                    documentTypeId = x.DOCUMENTTYPEID,
                                    documentTypeName = docContext.TBL_DOCUMENT_TYPE.Where(t => t.DOCUMENTTYPEID == x.DOCUMENTTYPEID).FirstOrDefault().DOCUMENTTYPENAME,
                                    loanApplicationId = x.LOANAPPLICATIONID,
                                    dueDate = x.DUEDATE,
                                    datetimeCreated = x.DATETIMECREATED,
                                }).ToList();
            foreach(var doc in deferredDocs)
            {
                doc.applicationReferenceNumber = context.TBL_LOAN_APPLICATION.Where(l => l.LOANAPPLICATIONID == doc.loanApplicationId).FirstOrDefault().APPLICATIONREFERENCENUMBER;
                var appDetail = context.TBL_LOAN_APPLICATION_DETAIL.Where(d => d.LOANAPPLICATIONID == doc.loanApplicationId).ToList();
                doc.facilityTypeId = appDetail[0].PROPOSEDPRODUCTID;
                doc.facilityTypeName = context.TBL_PRODUCT.Where(p => p.PRODUCTID == doc.facilityTypeId).FirstOrDefault().PRODUCTNAME;
            }
            return deferredDocs;
        }

        public bool DeleteDeferredDocument(int id, UserInfo user)
        {
            var audit_staff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));
            var deferredDoc = docContext.TBL_DEFERRED_DOC_TRACKER.Find(id);
            if (deferredDoc != null)
            {
                deferredDoc.DELETED = true;
                deferredDoc.DELETEDBY = user.createdBy;
                deferredDoc.DATETIMEDELETED = DateTime.Now;

               

                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.ApprovalLevelDeleted,
                    STAFFID = user.createdBy,
                    BRANCHID = (short)user.BranchId,
                    DETAIL = $"Deferred Document was deleted by {audit_staff}",
                    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                    URL = user.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now,
                    TARGETID = deferredDoc.LOANAPPLICATIONID,
                    DEVICENAME = CommonHelpers.GetDeviceName(),
                    OSNAME = CommonHelpers.FriendlyName()
                };

                this.audit.AddAuditTrail(audit);
            }
            if (docContext.SaveChanges() > 0) return true;
            return false;
        }
    }
}

// kernel.Bind<IDocumentUploadRepository>().To<DocumentUploadRepository>();
// DocumentUploadAdded = ???, DocumentUploadUpdated = ???, DocumentUploadDeleted = ???,
