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

        public DocumentUploadRepository(
                FinTrakBankingDocumentsContext _docContext,
                FinTrakBankingContext _context,
                IGeneralSetupRepository _general,
                IAuditTrailRepository _audit,
                IAdminRepository _admin,
                IWorkflow _workflow
            )
        {
            this.docContext = _docContext;
            this.context = _context;
            this.general = _general;
            this.audit = _audit;
            this.admin = _admin;
            this.workflow = _workflow;
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

        public IEnumerable<DocumentUploadViewModel> GetDocumentUploads(int staffId, int operationId, int targetId)
        {
            return docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
                , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) => 
                    new {
                        documentUploadId = up.DOCUMENTUPLOADID,
                        fileName = up.FILENAME,
                        fileExtension = up.FILEEXTENSION,
                        fileSize = up.FILESIZE,
                        fileSizeUnit = up.FILESIZEUNIT,
                        fileData = up.FILEDATA,
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
                fileData = up.fileData,
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
            .ThenBy(x => x.documentTypeId)
            .ToList();
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
                    fileData = x.up.FILEDATA,
                    companyId = x.up.COMPANYID,
                    issueDate = x.up.ISSUEDATE,
                    expiryDate = x.up.EXPIRYDATE,
                    documentCategoryId = x.us.OPERATIONID, // TODO
                    documentTypeId = x.up.DOCUMENTTYPEID,
                    physicalFilenumber = x.up.PHYSICALFILENUMBER,
                    physicalLocation = x.up.PHYSICALLOCATION,
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
                                  fileData = x.FILEDATA,
                                  companyId = x.COMPANYID,
                                  issueDate = x.ISSUEDATE,
                                  expiryDate = x.EXPIRYDATE,
                                  physicalFilenumber = x.PHYSICALFILENUMBER,
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
            } else
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
                DOCUMENTTYPEID = model.documentTypeId,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
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

            return 2;
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
                IPADDRESS = user.userIPAddress,
                URL = user.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                TARGETID = entity.DOCUMENTUPLOADID
            });
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteDocumentUpload(int id, UserInfo user)
        {
            var usageCount = 0;

            var usage = docContext.TBL_DOCUMENT_USAGE.FirstOrDefault(u => u.DOCUMENTUSAGEID == id);
            if (usage != null)
            {// throw new SecureException("An error occured! Cannot find target item to delete.");

                usageCount = docContext.TBL_DOCUMENT_USAGE
                   .Where(u => u.DOCUMENTUPLOADID == usage.DOCUMENTUPLOADID)
                   .Count();

                usage.DELETED = true;
                usage.DELETEDBY = user.createdBy;
                usage.DATETIMEDELETED = DateTime.Now;
            }
            if (usageCount < 2)
            {
                var upload = docContext.TBL_DOCUMENT_UPLOAD.Find(id);
                upload.DELETED = true;
                upload.DELETEDBY = user.createdBy;
                upload.DATETIMEDELETED = DateTime.Now;

                docContext.SaveChanges();
            }

            var auditStaff = (context.TBL_STAFF.Where(x => x.STAFFID == user.createdBy).Select(x => x.STAFFCODE));

            // Audit Section ---------------------------
            //this.audit.AddAuditTrail(new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentUploadDeleted,
            //    STAFFID = user.createdBy,
            //    BRANCHID = (short)user.BranchId,
            //    DETAIL = $"TBL_Document Upload '{entity.DOCUMENTTYPEID}' was deleted by {auditStaff}",
            //    IPADDRESS = user.userIPAddress,
            //    URL = user.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    TARGETID = entity.DOCUMENTUPLOADID
            //});
            // Audit Section end ------------------------

            return context.SaveChanges() != 0;
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

        public IEnumerable<DocumentCategoryViewModel> GetDocumentCategories()
        {
            return docContext.TBL_DOCUMENT_CATEGORY.Where(x => x.DELETED == false)
                            .Select(x => new DocumentCategoryViewModel
                            {
                                documentCategoryId = x.DOCUMENTCATEGORYID,
                                documentCategoryName = x.DOCUMENTCATEGORYNAME,
                            })
                            .ToList();
        }

        public IEnumerable<DocumentTypeViewModel> GetDocumentTypes(int id)
        {
            return docContext.TBL_DOCUMENT_TYPE.Where(x => x.DELETED == false && x.DOCUMENTCATEGORYID == id)
                            .Select(x => new DocumentTypeViewModel
                            {
                                documentTypeId = x.DOCUMENTTYPEID,
                                documentTypeName = x.DOCUMENTTYPENAME,
                            })
                            .ToList();
        }

        public CustomerDocumentSearchViewModel GetCustomerDocuments(DocumentUploadViewModel model, UserInfo user)
        {
            var customer = context.TBL_CUSTOMER
                .Join(context.TBL_CASA, a => a.CUSTOMERID, b => b.CUSTOMERID, (a, b) => new { a, b })
                .Select(x => new
                {
                    CUSTOMERCODE = x.a.CUSTOMERCODE,
                    FIRSTNAME = x.a.FIRSTNAME,
                    MIDDLENAME = x.a.MIDDLENAME,
                    LASTNAME = x.a.LASTNAME,
                    PRODUCTACCOUNTNUMBER = x.b.PRODUCTACCOUNTNUMBER,
                })
                .Where(c => c.CUSTOMERCODE == model.customerCode || c.PRODUCTACCOUNTNUMBER == model.customerCode || c.FIRSTNAME.ToLower().Contains(model.customerCode.ToLower().Trim()) || model.customerCode.ToLower().Contains(c.FIRSTNAME.ToLower().Trim()))
                .FirstOrDefault();

            if (customer == null) throw new SecureException("Customer not found!");

            CustomerDocumentSearchViewModel result = new CustomerDocumentSearchViewModel();
            result.customerName = customer.FIRSTNAME + " " + customer.MIDDLENAME + " " + customer.LASTNAME;

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

            var usageQuery = docContext.TBL_DOCUMENT_USAGE.Where(x => x.DELETED == false && x.CUSTOMERCODE == customer.CUSTOMERCODE)
                .Join(docContext.TBL_DOCUMENT_UPLOAD.Where(x => x.DELETED == false)
                , us => us.DOCUMENTUPLOADID, up => up.DOCUMENTUPLOADID, (us, up) =>
                    new
                    {
                        documentUploadId = up.DOCUMENTUPLOADID,
                        fileName = up.FILENAME,
                        fileExtension = up.FILEEXTENSION,
                        fileSize = up.FILESIZE,
                        fileSizeUnit = up.FILESIZEUNIT,
                        fileData = up.FILEDATA,
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
                fileData = up.fileData,
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

            });
           
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
    }
}

// kernel.Bind<IDocumentUploadRepository>().To<DocumentUploadRepository>();
// DocumentUploadAdded = ???, DocumentUploadUpdated = ???, DocumentUploadDeleted = ???,
