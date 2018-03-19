
using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Entities.DocumentModels;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;

namespace FintrakBanking.Repositories.Credit
{
    public class LoanDocumentRepository : ILoanDocumentRepository
    {
        private FinTrakBankingDocumentsContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;

        public LoanDocumentRepository(FinTrakBankingDocumentsContext context, IGeneralSetupRepository general, IAuditTrailRepository audit)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
        }

        #region TBL_MEDIA_LOAN_DOCUMENTS
        public bool AddLoanDocument(LoanDocumentViewModel model, byte[] file)
        {
            try
            {

                var data = new Entities.DocumentModels.TBL_MEDIA_LOAN_DOCUMENTS
                {
                    FILEDATA = file,
                    LOANAPPLICATIONNUMBER = model.loanApplicationNumber,
                    LOANREFERENCENUMBER = model.loanReferenceNumber,
                    DOCUMENTTITLE = model.documentTitle,
                    DOCUMENTTYPEID = model.documentTypeId,
                    LOAN_BOOKING_REQUESTID = model.SourceId,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation,
                    CREATEDBY = (int)model.createdBy,

                };

                context.TBL_MEDIA_LOAN_DOCUMENTS.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Loan Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateLoanDocument(LoanDocumentViewModel model, int documentId)
        {
            var data = this.context.TBL_MEDIA_LOAN_DOCUMENTS.Find(documentId);
            if (data == null)
            {
                return false;
            }

            data.LOANAPPLICATIONNUMBER = model.loanApplicationNumber;
            data.LOANREFERENCENUMBER = model.loanReferenceNumber;
            data.DOCUMENTTITLE = model.documentTitle;
            data.DOCUMENTTYPEID = model.documentTypeId;
            //data
            data.FILENAME = model.fileName;
            data.FILEEXTENSION = model.fileExtension;
            data.SYSTEMDATETIME = DateTime.Now;
            data.PHYSICALFILENUMBER = model.physicalFileNumber;
            data.PHYSICALLOCATION = model.physicalLocation;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Loan Document with title : '{ model.documentTitle }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public IEnumerable<LoanDocumentViewModel> GetAllLoanDocument()
        {
            return this.context.TBL_MEDIA_LOAN_DOCUMENTS.Select(x => new LoanDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                loanReferenceNumber = x.LOANREFERENCENUMBER,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = x.DOCUMENTTYPEID,
                // fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
            });
        }


        public void GetAllLoanDocument(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result)
        {
            result = this.context.TBL_MEDIA_LOAN_DOCUMENTS.Where(x=>x.LOANAPPLICATIONNUMBER==model.loanApplicationNumber)
                .Select(x => new LoanDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                loanReferenceNumber = x.LOANREFERENCENUMBER,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = x.DOCUMENTTYPEID,
                fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
                    dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_LOAN_DOCUMENTS
                }).ToList();
        }
        public LoanDocumentViewModel GetLoanDocument(int documentId)
        {
            var data = (from x in this.context.TBL_MEDIA_LOAN_DOCUMENTS
                        where x.DOCUMENTID == documentId
                        select new LoanDocumentViewModel
                        {
                            documentId = x.DOCUMENTID,
                            loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                            loanReferenceNumber = x.LOANREFERENCENUMBER,
                            documentTitle = x.DOCUMENTTITLE,
                            documentTypeId = x.DOCUMENTTYPEID,
                             fileData = x.FILEDATA,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            systemDateTime = x.SYSTEMDATETIME,
                            physicalFileNumber = x.PHYSICALFILENUMBER,
                            physicalLocation = x.PHYSICALLOCATION,
                            dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_LOAN_DOCUMENTS
                        });

            return data.FirstOrDefault();
        }

        public IEnumerable<LoanDocumentViewModel> GetApplicationLoanDocument(string applicationNumber)
        {
            var data = (from x in this.context.TBL_MEDIA_LOAN_DOCUMENTS
                        where x.LOANAPPLICATIONNUMBER == applicationNumber
                        select new LoanDocumentViewModel
                        {
                            documentId = x.DOCUMENTID,
                            loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                            loanReferenceNumber = x.LOANREFERENCENUMBER,
                            documentTitle = x.DOCUMENTTITLE,
                            documentTypeId = x.DOCUMENTTYPEID,
                             fileData = x.FILEDATA,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            systemDateTime = x.SYSTEMDATETIME,
                            physicalFileNumber = x.PHYSICALFILENUMBER,
                            physicalLocation = x.PHYSICALLOCATION,
                        });

            return data.ToList();
        }
        public void GetApplicationLoanDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {


            var data = (from x in this.context.TBL_MEDIA_LOAN_DOCUMENTS
                        where x.DOCUMENTID == model.documentId 
                        select new LoanDocumentViewModel
                        {
                            documentId = x.DOCUMENTID,
                            loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                            loanReferenceNumber = x.LOANREFERENCENUMBER,
                            documentTitle = x.DOCUMENTTITLE,
                            documentTypeId = x.DOCUMENTTYPEID,
                             fileData = x.FILEDATA,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            systemDateTime = x.SYSTEMDATETIME,
                            physicalFileNumber = x.PHYSICALFILENUMBER,
                            physicalLocation = x.PHYSICALLOCATION,
                            dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_LOAN_DOCUMENTS
                        });

            result = data.FirstOrDefault();
        }

        public void DeleteApplicationLoanDocument(LoanDocumentViewModel model, out int result)
        {

            result = 0;
            var data = (from x in this.context.TBL_MEDIA_LOAN_DOCUMENTS
                        where x.DOCUMENTID == model.documentId
                        select (x)).FirstOrDefault();

            if (data!=null)
            {
                context.TBL_MEDIA_LOAN_DOCUMENTS.Remove(data);
                result = context.SaveChanges();
            }
        }

        public LoanDocumentViewModel GetLoanDocumentByAppNoRefNo(string refNo, string applicationNumber)
        {
            var media = this.context.TBL_MEDIA_LOAN_DOCUMENTS.
                Where(h => h.LOANAPPLICATIONNUMBER == applicationNumber && h.LOANREFERENCENUMBER == refNo).
                Select(x => new LoanDocumentViewModel
                {
                    documentId = x.DOCUMENTID,
                    loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                    loanReferenceNumber = x.LOANREFERENCENUMBER,
                    documentTitle = x.DOCUMENTTITLE,
                    documentTypeId = x.DOCUMENTTYPEID,
                    fileData = x.FILEDATA,
                    fileName = x.FILENAME,
                    fileExtension = x.FILEEXTENSION,
                    systemDateTime = x.SYSTEMDATETIME,
                    physicalFileNumber = x.PHYSICALFILENUMBER,
                    physicalLocation = x.PHYSICALLOCATION,
                    dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_LOAN_DOCUMENTS
                }).FirstOrDefault();
            return media;
        }
        public IEnumerable<LoanDocumentViewModel> GetLoanDocumentByReferenceNumber(string referenceNumber)
        {
            return this.GetAllLoanDocument().Where(x =>
                string.Equals(x.loanReferenceNumber.ToLower(), referenceNumber.ToLower(), StringComparison.Ordinal));
        }

        public bool DeleteLoanDocument(string invoiceNo, string applicationNumber)
        {
            var data = (from a in context.TBL_MEDIA_LOAN_DOCUMENTS
                        where a.LOANREFERENCENUMBER == invoiceNo
&& a.LOANAPPLICATIONNUMBER == applicationNumber
                        select a).FirstOrDefault();
            if (data != null)
            {
                this.context.TBL_MEDIA_LOAN_DOCUMENTS.Remove(data);
                return context.SaveChanges() != 0;
            }
            return false;
        }

        #endregion

        #region COMMITTEE MINUTES

        public bool AddCommitteeDocument(LoanDocumentViewModel model, byte[] file)
        {
            try
            {
                var data = new Entities.DocumentModels.TBL_LOAN_COMMITTEE_MINUTES
                {
                    FILEDATA = file,
                    LOANAPPLICATIONNUMBER = model.loanApplicationNumber,
                    LOANREFERENCENUMBER = model.loanReferenceNumber,
                    DOCUMENTTITLE = model.documentTitle,
                    DOCUMENTTYPEID = model.documentTypeId,
                    LOAN_BOOKING_REQUESTID = model.SourceId,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation,
                    CREATEDBY = (int)model.createdBy,
                };

                context.TBL_LOAN_COMMITTEE_MINUTES.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Committee Minutes '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool DeleteCommitteeDocument(LoanDocumentViewModel model)
        {
            try
            {

                var delete = context.TBL_LOAN_COMMITTEE_MINUTES.FirstOrDefault(o => o.DOCUMENTID == model.customerId);

                context.TBL_LOAN_COMMITTEE_MINUTES.Remove(delete);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Committee Minutes '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public IEnumerable<LoanDocumentViewModel> GetCommitteeDocument(string applicationNumber)
        {
            return this.context.TBL_LOAN_COMMITTEE_MINUTES.Where(x => x.LOANAPPLICATIONNUMBER == applicationNumber).Select(x => new LoanDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                loanReferenceNumber = x.LOANREFERENCENUMBER,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = x.DOCUMENTTYPEID,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,

            });
        }



        public void GetCommitteeDocument(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result)
        {
            result = this.context.TBL_LOAN_COMMITTEE_MINUTES.Where(x => x.LOANAPPLICATIONNUMBER == model.loanApplicationNumber).Select(x => new LoanDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                loanReferenceNumber = x.LOANREFERENCENUMBER,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = x.DOCUMENTTYPEID,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
                dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_LOAN_COMMITTEE_MINUTES

            }).ToList();
        }

        public LoanDocumentViewModel GetCommitteeDocument(int documentId)
        {
            var data = this.context.TBL_LOAN_COMMITTEE_MINUTES.Find(documentId);

            if (data == null) { return null; }

            return new LoanDocumentViewModel
            {
                documentId = data.DOCUMENTID,
                loanApplicationNumber = data.LOANAPPLICATIONNUMBER,
                loanReferenceNumber = data.LOANREFERENCENUMBER,
                documentTitle = data.DOCUMENTTITLE,
                documentTypeId = data.DOCUMENTTYPEID,
                fileData = data.FILEDATA,
                fileName = data.FILENAME,
                fileExtension = data.FILEEXTENSION,
                systemDateTime = data.SYSTEMDATETIME,
                physicalFileNumber = data.PHYSICALFILENUMBER,
                physicalLocation = data.PHYSICALLOCATION,
                dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_LOAN_COMMITTEE_MINUTES
            };
        }
        public void GetCommitteeDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {
            var data = (from x in context.TBL_LOAN_COMMITTEE_MINUTES
                       where x.DOCUMENTID == model.documentId
                       select new LoanDocumentViewModel
            {
                documentId = x.DOCUMENTID,
                loanApplicationNumber = x.LOANAPPLICATIONNUMBER,
                loanReferenceNumber = x.LOANREFERENCENUMBER,
                documentTitle = x.DOCUMENTTITLE,
                documentTypeId = x.DOCUMENTTYPEID,
                fileData = x.FILEDATA,
                fileName = x.FILENAME,
                fileExtension = x.FILEEXTENSION,
                systemDateTime = x.SYSTEMDATETIME,
                physicalFileNumber = x.PHYSICALFILENUMBER,
                physicalLocation = x.PHYSICALLOCATION,
                dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_LOAN_COMMITTEE_MINUTES
            }).FirstOrDefault();

            result = data;
        }

        public void DeleteCommitteeDocument(LoanDocumentViewModel model, out int result)
        {
            result = 0;
            var data = (from x in context.TBL_LOAN_COMMITTEE_MINUTES
                        where x.DOCUMENTID == model.documentId
                        select (x)).FirstOrDefault();
            if (data!=null)
            {
                context.TBL_LOAN_COMMITTEE_MINUTES.Remove(data);
                result = context.SaveChanges();
            }
            
        }
        #endregion COMMITTEE MINUTES

        #region CREDIT BUREAU REPORTS
        public List<LoanDocumentViewModel> GetCreditBureauReportDocument(int customerCreditBureauId)
        {
            var data = (from x in this.context.TBL_CUSTOMER_CREDIT_BUREAU
                        where x.CUSTOMERCREDITBUREAUID == customerCreditBureauId
                        select new LoanDocumentViewModel
                        {
                            documentId = x.DOCUMENTID,
                            customerCreditBureauId = x.CUSTOMERCREDITBUREAUID,
                            documentTitle = x.DOCUMENT_TITLE,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            systemDateTime = x.SYSTEMDATETIME,
                            dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_CUSTOMER_CREDIT_BUREAU
                        });

            return data.ToList();
        }

        public void GetCreditBureauReportDocument(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result)
        {
            var data = (from x in this.context.TBL_CUSTOMER_CREDIT_BUREAU
                        where x.CUSTOMERCREDITBUREAUID == model.customerCreditBureauId
                        select new LoanDocumentViewModel
                        {
                            documentId = x.DOCUMENTID,
                            customerCreditBureauId = x.CUSTOMERCREDITBUREAUID,
                            documentTitle = x.DOCUMENT_TITLE,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            systemDateTime = x.SYSTEMDATETIME,
                            dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_CUSTOMER_CREDIT_BUREAU
                        });

            result = data.ToList();
        }



        public LoanDocumentViewModel GetCreditBureauReportDocumentByDocumentID(int customerCreditBureauId, int documentId)
        {
            return GetCreditBureauReportDocument(customerCreditBureauId).Where(x => x.documentId == documentId).FirstOrDefault();
        }

        public bool AddCreditBureauReportDocument(LoanDocumentViewModel model, byte[] file)
        {
            try
            {
                var data = new Entities.DocumentModels.TBL_CUSTOMER_CREDIT_BUREAU
                {
                    FILEDATA = file,
                    CUSTOMERCREDITBUREAUID = model.customerCreditBureauId,
                    DOCUMENT_TITLE = model.documentTitle,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    DATETIMECREATED = DateTime.Now,
                };

                context.TBL_CUSTOMER_CREDIT_BUREAU.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.CreditBureauReportDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Credit Bureau Report Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateCreditBureauReportDocument(LoanDocumentViewModel model, int documentId)
        {
            var data = this.context.TBL_CUSTOMER_CREDIT_BUREAU.Find(documentId);
            if (data == null)
            {
                return false;
            }

            data.CUSTOMERCREDITBUREAUID = model.customerCreditBureauId;
            data.DOCUMENT_TITLE = model.documentTitle;
            data.FILENAME = model.fileName;
            data.FILEEXTENSION = model.fileExtension;
            data.SYSTEMDATETIME = DateTime.Now;

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreditBureauReportDocumentUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated Credit Bureau Report Document with title : '{ model.documentTitle }' ",
                IPADDRESS = model.userIPAddress,
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }


        #endregion

        #region TBL_LOAN_CONDITION_DOCUMENTS

        public bool AddConditionDocument(LoanDocumentViewModel model, byte[] file)
        {
            try
            {
                var data = new Entities.DocumentModels.TBL_LOAN_CONDITION_DOCUMENTS
                {
                    FILEDATA = file,
                    LOANAPPLICATIONID = (int)model.loanApplicationId,
                    CONDITIONID = (int)model.conditionId,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation,
                    CREATEDBY = (int)model.createdBy,
                };

                context.TBL_LOAN_CONDITION_DOCUMENTS.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Condition Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateConditionDocument(LoanDocumentViewModel model)
        {
            try
            {
                var record = context.TBL_LOAN_CONDITION_DOCUMENTS.FirstOrDefault(o => o.DOCUMENTID == model.documentId);

                if (record == null)
                {
                    return false;
                }
                record.FILEDATA = model.fileData;
                record.LOANAPPLICATIONID = (int)model.loanApplicationId;
                record.CONDITIONID = (int)model.conditionId;
                record.FILENAME = model.fileName;
                record.FILEEXTENSION = model.fileExtension;
                record.SYSTEMDATETIME = DateTime.Now;
                record.PHYSICALFILENUMBER = model.physicalFileNumber;
                record.PHYSICALLOCATION = model.physicalLocation;
                record.CREATEDBY = (int)model.createdBy;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated Loan Condition Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetConditionDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {
            try
            {
                var record = (context.TBL_LOAN_CONDITION_DOCUMENTS.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => new LoanDocumentViewModel
                    {
                        loanApplicationId = x.LOANAPPLICATIONID,
                        conditionId = x.CONDITIONID,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        physicalFileNumber = x.PHYSICALFILENUMBER,
                        physicalLocation = x.PHYSICALLOCATION,
                        createdBy = x.CREATEDBY,
                        fileData=x.FILEDATA,
                        dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_LOAN_CONDITION_DOCUMENTS
                    }));

                result= record.FirstOrDefault();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void DeleteConditionDocument(LoanDocumentViewModel model, out int result)
        {
            result = 0;
            try
            {
                var record = (context.TBL_LOAN_CONDITION_DOCUMENTS.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x=>x)).FirstOrDefault();
                if (record != null)
                {
                    context.TBL_LOAN_CONDITION_DOCUMENTS.Remove(record);
                    result = context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public List<LoanDocumentViewModel> GetConditionDocuments(LoanDocumentViewModel model)
        {
            try
            {
                var record = (context.TBL_LOAN_CONDITION_DOCUMENTS.Where(o => o.LOANAPPLICATIONID == model.loanApplicationId)
                       .Select(x => new LoanDocumentViewModel
                       {
                           loanApplicationId = x.LOANAPPLICATIONID,
                           conditionId = x.CONDITIONID,
                           fileName = x.FILENAME,
                           fileExtension = x.FILEEXTENSION,
                           physicalFileNumber = x.PHYSICALFILENUMBER,
                           physicalLocation = x.PHYSICALLOCATION,
                           createdBy = x.CREATEDBY,
                           dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_LOAN_CONDITION_DOCUMENTS
                       }));

                return record.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetConditionDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result)
        {
            try
            {
                var record = (context.TBL_LOAN_CONDITION_DOCUMENTS.Where(o => o.LOANAPPLICATIONID == model.loanApplicationId)
                       .Select(x => new LoanDocumentViewModel
                       {
                           loanApplicationId = x.LOANAPPLICATIONID,
                           conditionId = x.CONDITIONID,
                           fileName = x.FILENAME,
                           fileExtension = x.FILEEXTENSION,
                           physicalFileNumber = x.PHYSICALFILENUMBER,
                           physicalLocation = x.PHYSICALLOCATION,
                           createdBy = x.CREATEDBY,
                           dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_LOAN_CONDITION_DOCUMENTS
                       }));

                result= record.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region TBL_MEDIA_CHECKLIST_DOCUMENTS

        public bool AddMediaCheckListDocument(LoanDocumentViewModel model, byte[] file)
        {
            try
            {

                var data = new Entities.DocumentModels.TBL_MEDIA_CHECKLIST_DOCUMENTS
                {
                    FILEDATA = file,
                    LOANAPPLICATIONID = (int)model.loanApplicationId,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation,
                    CREATEDBY = (int)model.createdBy,
                    CHECKLISTDEFINITIONID = (int)model.checkListDefinitionId,
                    LOANDETAILSID = (int)model.loanDetailId

                };

                context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Loan Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateMediaCheckListDocument(LoanDocumentViewModel model)
        {
            try
            {
                var record = context.TBL_MEDIA_CHECKLIST_DOCUMENTS.FirstOrDefault(o => o.DOCUMENTID == model.documentId);

                if (record == null)
                {
                    return false;
                }

                record.FILEDATA = model.fileData;
                record.LOANAPPLICATIONID = (int)model.loanApplicationId;
                record.FILENAME = model.fileName;
                record.FILEEXTENSION = model.fileExtension;
                record.SYSTEMDATETIME = DateTime.Now;
                record.PHYSICALFILENUMBER = model.physicalFileNumber;
                record.PHYSICALLOCATION = model.physicalLocation;
                record.CREATEDBY = (int)model.createdBy;
                record.CHECKLISTDEFINITIONID = (int)model.checkListDefinitionId;
                record.LOANDETAILSID = (int)model.loanDetailId;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Update media check list Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetMediaCheckListDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {
            try
            {
                var record = context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => new LoanDocumentViewModel
                    {
                        fileData = x.FILEDATA,
                        loanApplicationId = x.LOANAPPLICATIONID,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        physicalFileNumber = x.PHYSICALFILENUMBER,
                        physicalLocation = x.PHYSICALLOCATION,
                        checkListDefinitionId = x.CHECKLISTDEFINITIONID,
                        loanDetailId = x.LOANDETAILSID,
                        dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_CHECKLIST_DOCUMENTS
                    });

                result= record.FirstOrDefault();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void DeleteMediaCheckListDocument(LoanDocumentViewModel model, out int result)
        {
            result = 0;
            try
            {
                var record = context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => x).FirstOrDefault();
                if (record!=null)
                {
                    context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Remove(record);
                    result = context.SaveChanges();
                }
                
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetMediaCheckListDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result)
        {
            try
            {
                var record = context.TBL_MEDIA_CHECKLIST_DOCUMENTS.Where(o => o.LOANAPPLICATIONID == model.loanApplicationId)
                   .Select(x => new LoanDocumentViewModel
                   {
                       fileData = x.FILEDATA,
                       loanApplicationId = x.LOANAPPLICATIONID,
                       fileName = x.FILENAME,
                       fileExtension = x.FILEEXTENSION,
                       physicalFileNumber = x.PHYSICALFILENUMBER,
                       physicalLocation = x.PHYSICALLOCATION,
                       checkListDefinitionId = x.CHECKLISTDEFINITIONID,
                       loanDetailId = x.LOANDETAILSID,
                       dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_CHECKLIST_DOCUMENTS
                   });

                result= record.ToList();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region TBL_MEDIA_COLLATERAL_DOCUMENTS
        public bool AddMediaCollateralDocument(LoanDocumentViewModel model, byte[] file)
        {
            try
            {

                var data = new Entities.DocumentModels.TBL_MEDIA_COLLATERAL_DOCUMENTS
                {
                    FILEDATA = file,
                    COLLATERALCUSTOMERID = (int)model.collateralCustomerId,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    DOCUMENTCODE = model.documentCode,
                    DOCUMENTID = model.documentId,

                };

                context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Media Collateral Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateMediaCollateralDocument(LoanDocumentViewModel model, byte[] file)
        {
            try
            {
                var record = context.TBL_MEDIA_COLLATERAL_DOCUMENTS.FirstOrDefault(o => o.DOCUMENTID == model.documentId);

                if (record == null)
                {
                    return false;
                }

                record.FILEDATA = file;
                record.COLLATERALCUSTOMERID = (int)model.collateralCustomerId;
                record.FILENAME = model.fileName;
                record.FILEEXTENSION = model.fileExtension;
                record.SYSTEMDATETIME = DateTime.Now;
                record.CREATEDBY = (int)model.createdBy;
                record.DOCUMENTCODE = model.documentCode;
                record.DOCUMENTID = model.documentId;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated Media Collateral Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetMediaCollateralDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {
            try
            {
                var record = context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Where(o => o.DOCUMENTID == model.documentId)
                      .Select(x => new LoanDocumentViewModel
                      {
                          fileData = x.FILEDATA,
                          collateralCustomerId = x.COLLATERALCUSTOMERID,
                          fileName = x.FILENAME,
                          fileExtension = x.FILEEXTENSION,
                          documentCode = x.DOCUMENTCODE,
                          dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_COLLATERAL_DOCUMENTS
                      });
                result= record.FirstOrDefault();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void DeleteMediaCollateralDocument(LoanDocumentViewModel model, out int result)
        {
            result = 0;
            try
            {
                var record = context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Where(o => o.DOCUMENTID == model.documentId)
                      .Select(x => x).FirstOrDefault();
                if (record!=null)
                {
                    context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Remove(record);
                    result = context.SaveChanges();
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void GetMediaCollateralDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result)
        {
            try
            {
                var record = context.TBL_MEDIA_COLLATERAL_DOCUMENTS.Where(o => o.COLLATERALCUSTOMERID == model.collateralCustomerId)
                      .Select(x => new LoanDocumentViewModel
                      {
                          fileData = x.FILEDATA,
                          collateralCustomerId = x.COLLATERALCUSTOMERID,
                          fileName = x.FILENAME,
                          fileExtension = x.FILEEXTENSION,
                          documentCode = x.DOCUMENTCODE,
                          dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_COLLATERAL_DOCUMENTS
                      });
                result= record.ToList();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region TBL_MEDIA_JOB_REQUEST_DOCUMENT
        public bool AddMediaJobRequestDocument(LoanDocumentViewModel model, byte[] file)
        {
            try
            {

                var data = new Entities.DocumentModels.TBL_MEDIA_JOB_REQUEST_DOCUMENT
                {
                    FILEDATA = file,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    DOCUMENTID = model.documentId,
                    DOCUMENTTITLE = model.documentTitle,
                    DOCUMENTTYPEID = model.documentTypeId,
                    JOBREQUESTCODE = model.jobRequestCode,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation

                };

                context.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Media Job Request Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateMediaJobRequestDocument(LoanDocumentViewModel model)
        {
            try
            {

                var record = context.TBL_MEDIA_JOB_REQUEST_DOCUMENT.FirstOrDefault(o => o.DOCUMENTID == model.documentId);

                if (record == null)
                {
                    return false;
                }

                record.FILEDATA = model.fileData;
                record.FILENAME = model.fileName;
                record.FILEEXTENSION = model.fileExtension;
                record.SYSTEMDATETIME = DateTime.Now;
                record.CREATEDBY = (int)model.createdBy;
                record.DOCUMENTID = model.documentId;
                record.DOCUMENTTITLE = model.documentTitle;
                record.DOCUMENTTYPEID = model.documentTypeId;
                record.JOBREQUESTCODE = model.jobRequestCode;
                record.PHYSICALFILENUMBER = model.physicalFileNumber;
                record.PHYSICALLOCATION = model.physicalLocation;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Update Media Job Request Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public LoanDocumentViewModel GetMediaJobRequestDocument(LoanDocumentViewModel model)
        {
            try
            {

                var record = context.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => new LoanDocumentViewModel
                    {
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        documentTitle = x.DOCUMENTTITLE,
                        documentTypeId = x.DOCUMENTTYPEID,
                        jobRequestCode = x.JOBREQUESTCODE,
                        physicalFileNumber = x.PHYSICALFILENUMBER,
                        physicalLocation = x.PHYSICALLOCATION,
                        dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_JOB_REQUEST_DOCUMENT
                    });

                return record.FirstOrDefault();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        public void GetMediaJobRequestDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {
            try
            {

                var record = context.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => new LoanDocumentViewModel
                    {
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        documentTitle = x.DOCUMENTTITLE,
                        documentTypeId = x.DOCUMENTTYPEID,
                        jobRequestCode = x.JOBREQUESTCODE,
                        physicalFileNumber = x.PHYSICALFILENUMBER,
                        physicalLocation = x.PHYSICALLOCATION,
                        dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_JOB_REQUEST_DOCUMENT
                    });

                result = record.FirstOrDefault();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void DeleteMediaJobRequestDocument(LoanDocumentViewModel model, out int result)
        {
            result = 0;
            try
            {

                var record = context.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => x).FirstOrDefault();

                if (record!=null)
                {
                    context.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Remove(record);
                    result = context.SaveChanges();
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetMediaJobRequestDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result)
        {
            List<LoanDocumentViewModel> list = new List<LoanDocumentViewModel>();
            try
            {

                var record = context.TBL_MEDIA_JOB_REQUEST_DOCUMENT.Where(o => o.DOCUMENTID == model.documentId)
                   .Select(x => new LoanDocumentViewModel
                   {
                       fileData = x.FILEDATA,
                       fileName = x.FILENAME,
                       fileExtension = x.FILEEXTENSION,
                       documentTitle = x.DOCUMENTTITLE,
                       documentTypeId = x.DOCUMENTTYPEID,
                       jobRequestCode = x.JOBREQUESTCODE,
                       physicalFileNumber = x.PHYSICALFILENUMBER,
                       physicalLocation = x.PHYSICALLOCATION,
                       dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_JOB_REQUEST_DOCUMENT
                   });

                result = record.ToList();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion

        #region TBL_MEDIA_KYC_DOCUMENTS

        public bool AddMediaKYCDocument(LoanDocumentViewModel model, byte[] file)
        {
            try
            {

                var data = new Entities.DocumentModels.TBL_MEDIA_KYC_DOCUMENTS
                {
                    FILEDATA = file,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    DOCUMENTID = model.documentId,
                    DOCUMENTTITLE = model.documentTitle,
                    DOCUMENTTYPEID = model.documentTypeId,
                    PHYSICALFILENUMBER = model.physicalFileNumber,
                    PHYSICALLOCATION = model.physicalLocation,
                    CUSTOMERCODE = model.customerCode,
                    CUSTOMERID = (int)model.customerId,


                };

                context.TBL_MEDIA_KYC_DOCUMENTS.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Media KYC Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateMediaKYCDocument(LoanDocumentViewModel model)
        {
            try
            {
                var record = context.TBL_MEDIA_KYC_DOCUMENTS.FirstOrDefault(o => o.DOCUMENTID == model.documentId);

                if (record == null)
                {
                    return false;
                }

                record.FILEDATA = model.fileData;
                record.FILENAME = model.fileName;
                record.FILEEXTENSION = model.fileExtension;
                record.SYSTEMDATETIME = DateTime.Now;
                record.CREATEDBY = (int)model.createdBy;
                record.DOCUMENTID = model.documentId;
                record.DOCUMENTTITLE = model.documentTitle;
                record.DOCUMENTTYPEID = model.documentTypeId;
                record.PHYSICALFILENUMBER = model.physicalFileNumber;
                record.PHYSICALLOCATION = model.physicalLocation;
                record.CUSTOMERCODE = model.customerCode;
                record.CUSTOMERID = (int)model.customerId;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Update Media KYC Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetMediaKYCDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {
            try
            {

                var record = context.TBL_MEDIA_KYC_DOCUMENTS.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => new LoanDocumentViewModel
                    {
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        documentTitle = x.DOCUMENTTITLE,
                        documentTypeId = (short)x.DOCUMENTTYPEID,
                        physicalFileNumber = x.PHYSICALFILENUMBER,
                        physicalLocation = x.PHYSICALLOCATION,
                        customerCode = x.CUSTOMERCODE,
                        customerId = x.CUSTOMERID,
                        dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_KYC_DOCUMENTS
                    });

                result= record.FirstOrDefault();


            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void DeleteMediaKYCDocument(LoanDocumentViewModel model, out int result)
        {
            result = 0;
            try
            {

                var record = context.TBL_MEDIA_KYC_DOCUMENTS.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => x).FirstOrDefault();

                if (record!=null)
                {
                    context.TBL_MEDIA_KYC_DOCUMENTS.Remove(record);
                    result = context.SaveChanges();
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void GetMediaKYCDocuments(LoanDocumentViewModel model, out List<LoanDocumentViewModel> result)
        {
            List<LoanDocumentViewModel> list = new List<LoanDocumentViewModel>();
            try
            {

                var record = context.TBL_MEDIA_KYC_DOCUMENTS.Where(o => o.DOCUMENTID == model.documentId)
                     .Select(x => new LoanDocumentViewModel
                     {
                         fileData = x.FILEDATA,
                         fileName = x.FILENAME,
                         fileExtension = x.FILEEXTENSION,
                         documentTitle = x.DOCUMENTTITLE,
                         documentTypeId = (short)x.DOCUMENTTYPEID,
                         physicalFileNumber = x.PHYSICALFILENUMBER,
                         physicalLocation = x.PHYSICALLOCATION,
                         customerCode = x.CUSTOMERCODE,
                         customerId = x.CUSTOMERID,
                         dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_KYC_DOCUMENTS
                     });

                result= record.ToList();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion

        #region TBL_MEDIA_STAFF_PICTURE
        public bool AddMediaStaffPicture(LoanDocumentViewModel model, byte[] file)
        {
            try
            {

                var data = new Entities.DocumentModels.TBL_MEDIA_STAFF_PICTURE
                {
                    FILEDATA = file,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    DOCUMENTID = model.documentId,
                    DOCUMENT_TITLE = model.documentTitle,
                    STAFFCODE = model.staffCode
                };

                context.TBL_MEDIA_STAFF_PICTURE.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Loan Document with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateMediaStaffPicture(LoanDocumentViewModel model)
        {
            try
            {

                var record = context.TBL_MEDIA_STAFF_PICTURE.FirstOrDefault(o => o.DOCUMENTID == model.documentId);

                if (record == null)
                {
                    return false;
                }

                record.FILEDATA = model.fileData;
                record.FILENAME = model.fileName;
                record.FILEEXTENSION = model.fileExtension;
                record.SYSTEMDATETIME = DateTime.Now;
                record.CREATEDBY = (int)model.createdBy;
                record.DOCUMENTID = model.documentId;
                record.DOCUMENT_TITLE = model.documentTitle;
                record.STAFFCODE = model.staffCode;


                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Updated Media Staff Picture with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public LoanDocumentViewModel GetMediaStaffPicture(LoanDocumentViewModel model)
        {
            try
            {

                var record = context.TBL_MEDIA_STAFF_PICTURE.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => new LoanDocumentViewModel
                    {
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        documentTitle = x.DOCUMENT_TITLE,
                        staffCode = x.STAFFCODE,
                    });

                return record.FirstOrDefault();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetMediaStaffPicture(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {
            try
            {

                var record = context.TBL_MEDIA_STAFF_PICTURE.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => new LoanDocumentViewModel
                    {
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        documentTitle = x.DOCUMENT_TITLE,
                        staffCode = x.STAFFCODE,

                        dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_MEDIA_STAFF_PICTURE
                    });

                result = record.FirstOrDefault();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void DeleteMediaStaffPicture(LoanDocumentViewModel model, out int result)
        {
            result = 0;
            try
            {

                var record = context.TBL_MEDIA_STAFF_PICTURE.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => x).FirstOrDefault();

                if (true)
                {
                    context.TBL_MEDIA_STAFF_PICTURE.Remove(record);
                    result = context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion

        #region TBL_MEDIA_STAFF_SIGNATURE
        public bool AddMediaStaffSignature(LoanDocumentViewModel model, byte[] file)
        {
            try
            {

                var data = new Entities.DocumentModels.TBL_MEDIA_STAFF_SIGNATURE
                {
                    FILEDATA = file,
                    FILENAME = model.fileName,
                    FILEEXTENSION = model.fileExtension,
                    SYSTEMDATETIME = DateTime.Now,
                    CREATEDBY = (int)model.createdBy,
                    DOCUMENTID = model.documentId,
                    STAFFCODE = model.staffCode

                };

                context.TBL_MEDIA_STAFF_SIGNATURE.Add(data);

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Added Media Staff Signature with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateMediaStaffSignature(LoanDocumentViewModel model)
        {
            try
            {

                var record = context.TBL_MEDIA_STAFF_SIGNATURE.FirstOrDefault(o => o.DOCUMENTID == model.documentId);

                if (record == null)
                {
                    return false;
                }

                record.FILEDATA = model.fileData;
                record.FILENAME = model.fileName;
                record.FILEEXTENSION = model.fileExtension;
                record.SYSTEMDATETIME = DateTime.Now;
                record.CREATEDBY = (int)model.createdBy;
                record.DOCUMENTID = model.documentId;
                record.STAFFCODE = model.staffCode;

                // Audit Section ---------------------------
                var audit = new TBL_AUDIT
                {
                    AUDITTYPEID = (short)AuditTypeEnum.LoanDocumentAdded,
                    STAFFID = model.createdBy,
                    BRANCHID = (short)model.userBranchId,
                    DETAIL = $"Update Media Staff Signature with title : '{ model.documentTitle }' ",
                    IPADDRESS = model.userIPAddress,
                    URL = model.applicationUrl,
                    APPLICATIONDATE = general.GetApplicationDate(),
                    SYSTEMDATETIME = DateTime.Now
                };
                this.audit.AddAuditTrail(audit);
                // End of Audit Section ---------------------

                return context.SaveChanges() != 0;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public LoanDocumentViewModel GetMediaStaffSignature(LoanDocumentViewModel model)
        {
            try
            {

                var record = context.TBL_MEDIA_STAFF_SIGNATURE.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => new LoanDocumentViewModel
                    {
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        staffCode = x.STAFFCODE
                    });

                return record.FirstOrDefault();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void GetMediaStaffSignature(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {
            try
            {

                var record = context.TBL_MEDIA_STAFF_SIGNATURE.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => new LoanDocumentViewModel
                    {
                        fileData = x.FILEDATA,
                        fileName = x.FILENAME,
                        fileExtension = x.FILEEXTENSION,
                        staffCode = x.STAFFCODE
                    });

                result = record.FirstOrDefault();

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void DeleteMediaStaffSignature(LoanDocumentViewModel model, out int result)
        {
            result = 0;
            try
            {

                var record = context.TBL_MEDIA_STAFF_SIGNATURE.Where(o => o.DOCUMENTID == model.documentId)
                    .Select(x => x).FirstOrDefault();

                if (record!=null)
                {
                    context.TBL_MEDIA_STAFF_SIGNATURE.Remove(record);
                    result = context.SaveChanges();
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion



        public bool uploadDocument(LoanDocumentViewModel model, byte[] file)
        {
            //Check for file size here
            switch (model.dababaseTable)
            {
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_LOAN_DOCUMENTS: AddLoanDocument(model, file); return true;
                case (int)documentUploadDatabaseTableEnum.TBL_LOAN_COMMITTEE_MINUTES: AddCommitteeDocument(model, file); return true;
                case (int)documentUploadDatabaseTableEnum.TBL_CUSTOMER_CREDIT_BUREAU: AddCreditBureauReportDocument(model, file); return true;
                case (int)documentUploadDatabaseTableEnum.TBL_LOAN_CONDITION_DOCUMENTS: AddConditionDocument(model, file); return true;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_CHECKLIST_DOCUMENTS: AddMediaCheckListDocument(model, file); return true;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_COLLATERAL_DOCUMENTS: AddMediaCollateralDocument(model, file); return true;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_JOB_REQUEST_DOCUMENT: AddMediaJobRequestDocument(model, file); return true;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_KYC_DOCUMENTS: AddMediaKYCDocument(model, file); return true;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_STAFF_PICTURE: AddMediaStaffPicture(model, file); return true;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_STAFF_SIGNATURE: AddMediaStaffSignature(model, file); return true;
                default:

                    return false;
            }

        }

        public LoanDocumentViewModel getUploadedDocument(LoanDocumentViewModel model)
        {
            var result = new LoanDocumentViewModel();
            switch (model.dababaseTable)
            {
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_LOAN_DOCUMENTS: GetApplicationLoanDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_LOAN_COMMITTEE_MINUTES: GetCommitteeDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_CUSTOMER_CREDIT_BUREAU: GetCreditBureauReportDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_LOAN_CONDITION_DOCUMENTS: GetConditionDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_CHECKLIST_DOCUMENTS: GetMediaCheckListDocument(model,out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_COLLATERAL_DOCUMENTS: GetMediaCollateralDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_JOB_REQUEST_DOCUMENT: GetMediaJobRequestDocument(model,out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_KYC_DOCUMENTS: GetMediaKYCDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_STAFF_PICTURE: GetMediaStaffPicture(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_STAFF_SIGNATURE: GetMediaStaffSignature(model, out result); return result;
                default:
                    return result;
            }

        }
        public List<LoanDocumentViewModel> getListOfUploadedDocument(LoanDocumentViewModel model)
        {
            var result = new List<LoanDocumentViewModel>();
            switch (model.dababaseTable)
            {
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_LOAN_DOCUMENTS: GetAllLoanDocument( model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_LOAN_COMMITTEE_MINUTES: GetCommitteeDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_CUSTOMER_CREDIT_BUREAU: GetCreditBureauReportDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_LOAN_CONDITION_DOCUMENTS: GetConditionDocuments(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_CHECKLIST_DOCUMENTS: GetMediaCheckListDocuments(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_COLLATERAL_DOCUMENTS: GetMediaCollateralDocuments(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_JOB_REQUEST_DOCUMENT: GetMediaJobRequestDocuments(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_KYC_DOCUMENTS: GetMediaKYCDocuments(model, out result); return result;

                default:
                    return result;
            }

        }


        public int DeleteUploadedDocument(LoanDocumentViewModel model)
        {
            var result = 0;

            switch (model.dababaseTable)
            {
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_LOAN_DOCUMENTS: DeleteApplicationLoanDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_LOAN_COMMITTEE_MINUTES: DeleteCommitteeDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_CUSTOMER_CREDIT_BUREAU: DeleteCreditBureauReportDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_LOAN_CONDITION_DOCUMENTS: DeleteConditionDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_CHECKLIST_DOCUMENTS: DeleteMediaCheckListDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_COLLATERAL_DOCUMENTS: DeleteMediaCollateralDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_JOB_REQUEST_DOCUMENT: DeleteMediaJobRequestDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_KYC_DOCUMENTS: DeleteMediaKYCDocument(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_STAFF_PICTURE: DeleteMediaStaffPicture(model, out result); return result;
                case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_STAFF_SIGNATURE: DeleteMediaStaffSignature(model, out result); return result;
                default:
                    return result;
            }

        }

       
        public void GetCreditBureauReportDocument(LoanDocumentViewModel model, out LoanDocumentViewModel result)
        {
            var data = (from x in this.context.TBL_CUSTOMER_CREDIT_BUREAU
                        where x.DOCUMENTID == model.documentId
                        select new LoanDocumentViewModel
                        {
                            documentId = x.DOCUMENTID,
                            customerCreditBureauId = x.CUSTOMERCREDITBUREAUID,
                            documentTitle = x.DOCUMENT_TITLE,
                            fileName = x.FILENAME,
                            fileExtension = x.FILEEXTENSION,
                            systemDateTime = x.SYSTEMDATETIME,
                            fileData = x.FILEDATA,
                            dababaseTable = (int)documentUploadDatabaseTableEnum.TBL_CUSTOMER_CREDIT_BUREAU
                        });

            result= data.FirstOrDefault();
        }

        public void DeleteCreditBureauReportDocument(LoanDocumentViewModel model, out int result)
        {
            result = 0;
            var data = (from x in this.context.TBL_CUSTOMER_CREDIT_BUREAU
                        where x.CUSTOMERCREDITBUREAUID == model.customerCreditBureauId
                        select (x)).FirstOrDefault();
            if (data!=null)
            {
                context.TBL_CUSTOMER_CREDIT_BUREAU.Remove(data);
                result = context.SaveChanges();
            }
     
        }

       
       

        //public void getUploadedDocument(LoanDocumentViewModel model, byte[] file)
        //{
        //    //Check for file size here
        //    switch (model.dababaseTable)
        //    {
        //        case (int)documentUploadDatabaseTableEnum.TBL_MEDIA_LOAN_DOCUMENTS: GetAllLoanDocument(); break;
        //        case (int)documentUploadDatabaseTableEnum.TBL_LOAN_COMMITTEE_MINUTES: AddCommitteeDocument(model, file); break;
        //        case (int)documentUploadDatabaseTableEnum.TBL_CUSTOMER_CREDIT_BUREAU: AddCreditBureauReportDocument(model, file); break;
        //    }

        //}
    }
}
