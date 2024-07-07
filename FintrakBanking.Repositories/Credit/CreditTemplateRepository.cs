using System;
using System.Collections.Generic;
using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.ViewModels.Setups.Credit;
using FintrakBanking.Common.Enum;
using System.Linq;
using FintrakBanking.ViewModels.Credit;
using FintrakBanking.Common.CustomException;
using FintrakBanking.Common;
using System.Drawing;
using System.IO;

namespace FintrakBanking.Repositories.Credit
{
    public class CreditTemplateRepository : ICreditTemplateRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        private IAuditTrailRepository audit;
        private ILoanRepository loan;
        private MemorandumRepository memo;

        public CreditTemplateRepository(FinTrakBankingContext context, IGeneralSetupRepository general, IAuditTrailRepository audit, ILoanRepository loan, MemorandumRepository memo)
        {
            this.context = context;
            this.general = general;
            this.audit = audit;
            this.loan = loan;
           this.memo = memo;
        }

        #region DOCUMENT TEMPLATE OLD

        public bool AddCreditTemplate(CreditTemplateViewModel model)
        {
            if (String.IsNullOrEmpty(model.templateDocument)) { throw new SecureException("Document is blank. Cannot create a blank document!"); }

            var data = new TBL_CREDIT_TEMPLATE
            {
                COMPANYID = model.companyId,
                TEMPLATETITLE = model.templateTitle,
                TEMPLATEDOCUMENT = model.templateDocument,
                APPROVALLEVELID = model.approvalLevelId,
                //ProductClassId = model.productClassId,
                CREATEDBY = (int)model.createdBy,
                DATETIMECREATED = general.GetApplicationDate()
            };

            context.TBL_CREDIT_TEMPLATE.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreditTemplateAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added CreditTemplate '{ model.templateTitle }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateCreditTemplate(CreditTemplateViewModel model, int creditTemplateId)
        {
            var data = this.context.TBL_CREDIT_TEMPLATE.Find(creditTemplateId);
            if (data == null)
            {
                return false;
            }

            data.COMPANYID = model.companyId;
            data.TEMPLATETITLE = model.templateTitle;
            data.TEMPLATEDOCUMENT = model.templateDocument;
            data.APPROVALLEVELID = model.approvalLevelId;
            //data.ProductClassId = model.productClassId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.CreditTemplateUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated CreditTemplate '{ model.templateTitle }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool DeleteCreditTemplate(int creditTemplateId)
        {
            var data = this.context.TBL_CREDIT_TEMPLATE.Find(creditTemplateId);
            if (data != null)
            {
                data.DELETED = true;
            }
            return context.SaveChanges() != 0;
        }

        public IEnumerable<CreditTemplateViewModel> GetAllCreditTemplate()
        {
            return this.context.TBL_CREDIT_TEMPLATE.Where(x => x.DELETED == false).Select(x => new CreditTemplateViewModel
            {
                creditTemplateId = x.CREDITTEMPLATEID,
                companyId = x.COMPANYID,
                templateTitle = x.TEMPLATETITLE,
                templateDocument = x.TEMPLATEDOCUMENT,
                approvalLevelId = x.APPROVALLEVELID,
                //productClassId = x.ProductClassId,
            });
        }

        public CreditTemplateViewModel GetCreditTemplate(int creditTemplateId)
        {
            var data = this.context.TBL_CREDIT_TEMPLATE.Where(x => x.DELETED == false && x.CREDITTEMPLATEID == creditTemplateId).FirstOrDefault();

            if (data == null)
            {
                return null;
            }

            return new CreditTemplateViewModel
            {
                creditTemplateId = data.CREDITTEMPLATEID,
                companyId = data.COMPANYID,
                templateTitle = data.TEMPLATETITLE,
                templateDocument = data.TEMPLATEDOCUMENT,
                approvalLevelId = data.APPROVALLEVELID,
                //productClassId = data.ProductClassId,
            };
        }

        public IEnumerable<CreditTemplateViewModel> GetAllCreditTemplateByLevelProduct(int levelId, int productId, int companyId)
        {
            return this.GetAllCreditTemplate().Where(x =>
                x.approvalLevelId == levelId
                && x.productClassId == productId
                && x.companyId == companyId
            );
        }

        public IEnumerable<CreditTemplateViewModel> GetCreditTemplateByLevelId(int levelId, int companyId)
        {
            return this.GetAllCreditTemplate().Where(x =>
                x.approvalLevelId == levelId
            );
        }

        public IEnumerable<CreditTemplateViewModel> GetAllCreditTemplateByProductClass(int productId, int staffId)
        {
            var staffApprovalLevelIds = context.TBL_APPROVAL_LEVEL_STAFF.Where(x => x.STAFFID == staffId).Select(x => x.APPROVALLEVELID);
            return this.GetAllCreditTemplate().Where(x =>
                staffApprovalLevelIds.Contains(x.approvalLevelId)
                && x.productClassId == productId
            );
        }

        #endregion DOCUMENT TEMPLATE OLD

        #region DOCUMENT TEMPLATE IMPL

        public List<LoadedDocumentSectionViewModel> GetLoadedDocumentSections(int staffId, int operationId, int targetId)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            List<int> sectionIds = new List<int>();

           /* var memoOperationId = 0;
            var sectionsOperation = context.TBL_DOC_TEMPLATE_DETAIL.Where(x => x.DELETED == false && x.TARGETID == targetId).FirstOrDefault();
            if(sectionsOperation != null)
            {
                memoOperationId = sectionsOperation.OPERATIONID;
            }
            else
            {
                memoOperationId = operationId;
            }*/

            if (staff != null)
            {
                sectionIds = context.TBL_DOC_TEMPLATE_SECTION_ROLE
                    .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                    .Select(x => x.TEMPLATESECTIONID)
                    .ToList();
            }

            var sections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId).OrderBy(x => x.POSITION)
                .Join(context.TBL_STAFF, d => d.CREATEDBY, s => s.STAFFID, (d, s) => new { d, s })
                .Join(context.TBL_STAFF_ROLE, ds => ds.s.STAFFROLEID, r => r.STAFFROLEID, (ds, r) => new { t = ds.d, r })
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.t.POSITION,
                    sectionId = x.t.DOCUMENTDETAILID,
                    title = x.t.TITLE,
                    description = x.t.DESCRIPTION,
                    canEdit = x.t.CANEDIT, // system
                    editable = sectionIds.Contains(x.t.TEMPLATESECTIONID),
                    templateSectionId = x.t.TEMPLATESECTIONID,
                    staffRoleName = x.r.STAFFROLENAME
                }).OrderBy(d => d.position)
                .ToList();

            return sections;
        }

        public bool SaveApprovedDocumentation(int staffId, int operationId, int targetId)
        {
            // int staffId, is REDUNDANT!
            var printedDoc = "";
            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList();

            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();
            
            memo.Init(operationId, targetId); //content = memo.Replace(content);
            foreach (var raw in rawSections)
            {
                raw.templateDocument = memo.Replace(raw.templateDocument);
                replacedSections.Add(raw);
                printedDoc = raw.title;
            }

            var docsToSave = new List<TBL_DOC_TEMPLATE_SAVED>();
            foreach (var section in replacedSections)
            {
                TBL_DOC_TEMPLATE_SAVED docToSave = new TBL_DOC_TEMPLATE_SAVED();
                var sect = context.TBL_DOC_TEMPLATE_DETAIL.Find(section.sectionId);
                docToSave.OPERATIONID = sect.OPERATIONID;
                docToSave.DOCUMENTDETAILID = sect.DOCUMENTDETAILID;
                docToSave.TARGETID = sect.TARGETID;
                docToSave.TEMPLATESECTIONID = sect.TEMPLATESECTIONID;
                docToSave.TITLE = sect.TITLE;
                docToSave.DESCRIPTION = sect.DESCRIPTION;
                docToSave.TEMPLATEDOCUMENT = section.templateDocument;
                docToSave.POSITION = sect.POSITION;
                docToSave.CANEDIT = sect.CANEDIT;
                docToSave.CREATEDBY = sect.CREATEDBY;
                docToSave.DATETIMECREATED = DateTime.Now;
                docsToSave.Add(docToSave);
            }

            context.TBL_DOC_TEMPLATE_SAVED.AddRange(docsToSave);

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
            //    STAFFID = staffId,
            //    BRANCHID = 1, //(short)model.userBranchId,
            //    DETAIL = $"Printed Document Template '{ printedDoc }' ",
            //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            //    URL = "localhost",//model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    DEVICENAME = CommonHelpers.GetDeviceName(),
            //    OSNAME = CommonHelpers.FriendlyName()
            //};
            //this.audit.AddAuditTrail(audit);
            return context.SaveChanges() > 0;
        }

        public List<LoadedDocumentSectionViewModel> GetSavedDocumentation(int operationId, int targetId)
        {
            // int staffId, is REDUNDANT!
            //var printedDoc = "";

            //var docSections = context.TBL_DOC_TEMPLATE_DETAIL
            //    .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
            //    .OrderBy(x => x.POSITION).FirstOrDefault();

            //var sections = context.TBL_DOC_TEMPLATE_SAVED
            //    .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
            //    .OrderBy(x => x.POSITION)
            //    .Select(x => new LoadedDocumentSectionViewModel
            //    {
            //        position = x.POSITION,
            //        sectionId = x.DOCUMENTDETAILID,
            //        title = x.TITLE,
            //        description = x.DESCRIPTION,
            //        canEdit = x.CANEDIT, // system
            //        // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
            //        templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
            //    })
            //    .ToList();

            // return sections;
            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();

            //memo.Init(operationId, targetId, docSections.SHOWMCCSTAMP, docSections.SHOWBCCSTAMP); //content = memo.Replace(content);
            //foreach (var raw in sections)
            //{
            //    raw.templateDocument = memo.Replace(raw.templateDocument);
            //    replacedSections.Add(raw);
            //    printedDoc = raw.title;
            //}

            //var audit = new TBL_AUDIT
            //{
            //    AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
            //    STAFFID = staffId,
            //    BRANCHID = 1, //(short)model.userBranchId,
            //    DETAIL = $"Printed Document Template '{ printedDoc }' ",
            //    IPADDRESS = CommonHelpers.GetLocalIpAddress(),
            //    URL = "localhost",//model.applicationUrl,
            //    APPLICATIONDATE = general.GetApplicationDate(),
            //    SYSTEMDATETIME = DateTime.Now,
            //    DEVICENAME = CommonHelpers.GetDeviceName(),
            //    OSNAME = CommonHelpers.FriendlyName()
            //};
            //this.audit.AddAuditTrail(audit);
            //context.SaveChanges();

            return replacedSections;
        }

        public List<LoadedDocumentSectionViewModel> GetLoadedDocumentBulkLiquidation(int staffId, int operationId, int targetId, UserInfo user)
        {
            // int staffId, is REDUNDANT!
            var printedDoc = "";
            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateSectionId = x.TEMPLATESECTIONID,
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList();

            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();

            //memo.Init(operationId, targetId); //content = memo.Replace(content);
            foreach (var raw in rawSections)
            {
                var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                raw.templateDocument = memo.Replace(raw.templateDocument);
                if (templateId == 1)
                {
                    raw.templateDocument = memo.UpdateEsg(raw.templateDocument);
                    raw.templateDocument = memo.UpdateGreenRating(raw.templateDocument);
                }
                replacedSections.Add(raw);
                printedDoc = raw.title;
            }

            var staff = context.TBL_STAFF.Find(staffId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
                STAFFID = staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Printed Document Template '{ printedDoc }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = "localhost",//model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            context.SaveChanges();

            return replacedSections;
        }

        public List<LoadedDocumentSectionViewModel> GetLoadedDocumentation(int staffId, int operationId, int targetId, UserInfo user, bool isThirdPartyFacility)
        {
            // int staffId, is REDUNDANT!
           /* var memoOperationId = 0;
            var templateOperation = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.TARGETID == targetId).FirstOrDefault();
            if(templateOperation != null)
            {
                memoOperationId = operationId;
            }
            else
            {
                memoOperationId = templateOperation.OPERATIONID;
            }*/

            var printedDoc = "";
           /* var docSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION).FirstOrDefault();

            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId )
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateSectionId = x.TEMPLATESECTIONID,
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList(); */


            var rawSectionsQuery = context.TBL_DOC_TEMPLATE_DETAIL
            .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
            .OrderBy(x => x.POSITION);

            var docSections = rawSectionsQuery.FirstOrDefault();

            var rawSections = rawSectionsQuery
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT,
                    templateSectionId = x.TEMPLATESECTIONID,
                    templateDocument = x.TEMPLATEDOCUMENT
                })
            .ToList();

            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();
            if (isThirdPartyFacility)
            {
                memo.InitForThirdpartyLoans(operationId, targetId);
                foreach (var raw in rawSections)
                {
                    var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                    raw.templateDocument = memo.Replace(raw.templateDocument);
                    //if (templateId == 1)
                    //{
                    //    raw.templateDocument = memo.UpdateEsg(raw.templateDocument);
                    //    raw.templateDocument = memo.UpdateGreenRating(raw.templateDocument);
                    //}
                    replacedSections.Add(raw);
                    printedDoc = raw.title;
                }
            }
            else
            {
                memo.Init(operationId, targetId, docSections.SHOWMCCSTAMP, docSections.SHOWBCCSTAMP); //content = memo.Replace(content);
                foreach (var raw in rawSections)
                {
                    var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                    raw.templateDocument = memo.Replace(raw.templateDocument);
                    if (templateId == 1)
                    {
                        raw.templateDocument = memo.UpdateEsg(raw.templateDocument);
                        raw.templateDocument = memo.UpdateGreenRating(raw.templateDocument);
                    }
                    replacedSections.Add(raw);
                    printedDoc = raw.title;
                }
            }

            var staff = context.TBL_STAFF.Find(staffId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
                STAFFID = staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Printed Document Template '{ printedDoc }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = "localhost",//model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            context.SaveChanges();
           
            return replacedSections;
        }

        public List<LoadedDocumentSectionViewModel> GetLoadedDocumentationStamped(int staffId, int operationId, int targetId, UserInfo user, int approvalLevelId, bool isThirdPartyFacility)
        {

            var showMccStamp = false;
            var showBccStamp = false;
            
            var approvalLevel = context.TBL_APPROVAL_LEVEL.Find(approvalLevelId);
            var staffRoleId = context.TBL_STAFF_ROLE.Where(s => s.STAFFROLEID == approvalLevel.STAFFROLEID && (approvalLevel.DELETED == false && approvalLevel.ISACTIVE == true)).FirstOrDefault().STAFFROLEID;
            var digitalStamp = context.TBL_DIGITAL_STAMP.Where(d => d.STAFFROLEID == approvalLevel.STAFFROLEID && d.DELETED == false).FirstOrDefault();

            var printedDoc = "";
            var docSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION).ToList();
            showMccStamp = docSections[0].SHOWMCCSTAMP;
            showBccStamp = docSections[0].SHOWBCCSTAMP;

            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId )
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateSectionId = x.TEMPLATESECTIONID,
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList();

            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();
            if(digitalStamp != null)
            {
                if (digitalStamp.STAMPNAME.ToLower().Contains("mcc")) showMccStamp = true;
                if (digitalStamp.STAMPNAME.ToLower().Contains("bcc")) showBccStamp = true;
                if (isThirdPartyFacility)
                {
                    memo.InitForThirdpartyLoans(operationId, targetId);
                    foreach (var raw in rawSections)
                    {
                        var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                        raw.templateDocument = memo.Replace(raw.templateDocument);
                        //if (templateId == 1)
                        //{
                        //    raw.templateDocument = memo.UpdateEsg(raw.templateDocument);
                        //    raw.templateDocument = memo.UpdateGreenRating(raw.templateDocument);
                        //}
                        replacedSections.Add(raw);
                        printedDoc = raw.title;
                    }
                }
                else
                {
                    memo.Init(operationId, targetId, showMccStamp, showBccStamp); //content = memo.Replace(content);
                    foreach (var raw in rawSections)
                    {
                        var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                        raw.templateDocument = memo.Replace(raw.templateDocument);
                        if (templateId == 1)
                        {
                            raw.templateDocument = memo.UpdateEsg(raw.templateDocument);
                            raw.templateDocument = memo.UpdateGreenRating(raw.templateDocument);
                        }
                        
                        
                        replacedSections.Add(raw);
                        printedDoc = raw.title;
                    }
                    foreach (var section in docSections)
                    {
                        section.SHOWMCCSTAMP = showMccStamp;
                        section.SHOWBCCSTAMP = showBccStamp;
                    }
                }
            }
            

            var staff = context.TBL_STAFF.Find(staffId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
                STAFFID = staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Printed Document Template '{ printedDoc }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = "localhost",//model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            context.SaveChanges();
           
            return replacedSections;
        }

        public List<LoadedDocumentSectionViewModel> RemoveLoadedDocumentationStamped(int staffId, int operationId, int targetId, UserInfo user, int approvalLevelId, bool isThirdPartyFacility)
        {

            var showMccStamp = false;
            var showBccStamp = false;

            var approvalLevel = context.TBL_APPROVAL_LEVEL.Find(approvalLevelId);
            var staffRoleId = context.TBL_STAFF_ROLE.Where(s => s.STAFFROLEID == approvalLevel.STAFFROLEID && (approvalLevel.DELETED == false && approvalLevel.ISACTIVE == true)).FirstOrDefault().STAFFROLEID;
            var digitalStamp = context.TBL_DIGITAL_STAMP.Where(d => d.STAFFROLEID == approvalLevel.STAFFROLEID && d.DELETED == false).FirstOrDefault();

            var printedDoc = "";
            var docSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION).ToList();
            showMccStamp = docSections[0].SHOWMCCSTAMP;
            showBccStamp = docSections[0].SHOWBCCSTAMP;

            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateSectionId = x.TEMPLATESECTIONID,
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList();

            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();
            if (digitalStamp != null)
            {
                if (digitalStamp.STAMPNAME.ToLower().Contains("mcc"))
                { 
                    showMccStamp = false;
                    foreach (var section in docSections)
                    {
                        section.SHOWMCCSTAMP = showMccStamp;

                    }
                }
                if (digitalStamp.STAMPNAME.ToLower().Contains("bcc"))
                {
                    showBccStamp = false;
                    foreach (var section in docSections)
                    {
                       section.SHOWBCCSTAMP = showBccStamp;
                    }
                }

                if (isThirdPartyFacility)
                {
                    memo.InitForThirdpartyLoans(operationId, targetId);
                    foreach (var raw in rawSections)
                    {
                        var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                        raw.templateDocument = memo.Replace(raw.templateDocument);
                        //if (templateId == 1)
                        //{
                        //    raw.templateDocument = memo.UpdateEsg(raw.templateDocument);
                        //    raw.templateDocument = memo.UpdateGreenRating(raw.templateDocument);
                        //}
                        replacedSections.Add(raw);
                        printedDoc = raw.title;
                    }
                }
                else
                {
                    memo.Init(operationId, targetId, showMccStamp, showBccStamp); //content = memo.Replace(content);
                    foreach (var raw in rawSections)
                    {
                        var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                        raw.templateDocument = memo.Replace(raw.templateDocument);
                        if (templateId == 1)
                        {
                            raw.templateDocument = memo.UpdateEsg(raw.templateDocument);
                            raw.templateDocument = memo.UpdateGreenRating(raw.templateDocument);
                        }
                       

                        replacedSections.Add(raw);
                        printedDoc = raw.title;
                    }
                }
            }


            var staff = context.TBL_STAFF.Find(staffId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
                STAFFID = staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Printed Document Template '{printedDoc}' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = "localhost",//model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            context.SaveChanges();

            return replacedSections;
        }

        public List<LoadedDocumentSectionViewModel> GetLoadedDocumentationGeneric(int staffId, int operationId, int targetId, int targetIdForWorkFlow, UserInfo user, int customerId)
        {
            // int staffId, is REDUNDANT!
            var printedDoc = "";
            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateSectionId = x.TEMPLATESECTIONID,
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList();

            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();
            memo.InitGenericMemo(operationId, targetId, targetIdForWorkFlow, customerId); //content = memo.Replace(content);
            foreach (var raw in rawSections)
            {
                var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                raw.templateDocument = memo.Replace(raw.templateDocument);
                replacedSections.Add(raw);
                printedDoc = raw.title;
            }

            var staff = context.TBL_STAFF.Find(staffId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
                STAFFID = staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Printed Document Template '{ printedDoc }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = "localhost",//model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            context.SaveChanges();

            return replacedSections;
        }


        public List<LoadedDocumentSectionViewModel> GetLoadedExceptionDocumentation(int staffId, int operationId, int targetId, UserInfo user)
        {
            var printedDoc = "";
            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, 
                    templateSectionId = x.TEMPLATESECTIONID,
                    templateDocument = x.TEMPLATEDOCUMENT,
                })
                .ToList();

            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();
            
                memo.InitForExceptionalLoans(operationId, targetId); 
                foreach (var raw in rawSections)
                {
                    var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                    raw.templateDocument = memo.Replace(raw.templateDocument);
                    if (templateId == 1)
                    {
                        raw.templateDocument = memo.UpdateEsg(raw.templateDocument);
                        raw.templateDocument = memo.UpdateGreenRating(raw.templateDocument);
                    }
                    replacedSections.Add(raw);
                    printedDoc = raw.title;
                }
            

            var staff = context.TBL_STAFF.Find(staffId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
                STAFFID = staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Printed Document Template '{ printedDoc }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = "localhost",//model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            context.SaveChanges();

            return replacedSections;
        }

        public List<LoadedDocumentSectionViewModel> getRecoveryAnalysisDocumentation(int staffId, int operationId, int targetId, string referenceId, UserInfo user, int templateId)
        {
            // int staffId, is REDUNDANT!
            var printedDoc = "";
            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateSectionId = x.TEMPLATESECTIONID,
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList();

               List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();
                memo.InitRecoveryDate(templateId, referenceId); //content = memo.Replace(content);
                foreach (var raw in rawSections)
                {
                    var templateIds = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                    raw.templateDocument = memo.Replace(raw.templateDocument);
                    replacedSections.Add(raw);
                    printedDoc = raw.title;
                }
            

            var staff = context.TBL_STAFF.Find(staffId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
                STAFFID = staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Printed Document Template '{ printedDoc }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = "localhost",//model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            context.SaveChanges();

            return replacedSections;
        }

        public List<LoadedDocumentSectionViewModel> GetLoadedDocumentationBulkLiquidation(int staffId, int operationId, int targetId, UserInfo user)
        {
            // int staffId, is REDUNDANT!
            var printedDoc = "";
            var rawSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.TARGETID == targetId)
                .OrderBy(x => x.POSITION)
                .Select(x => new LoadedDocumentSectionViewModel
                {
                    position = x.POSITION,
                    sectionId = x.DOCUMENTDETAILID,
                    title = x.TITLE,
                    description = x.DESCRIPTION,
                    canEdit = x.CANEDIT, // system
                    // editable = sectionIds.Contains(x.TEMPLATESECTIONID),
                    templateSectionId = x.TEMPLATESECTIONID,
                    templateDocument = x.TEMPLATEDOCUMENT, // placeholder find replace
                })
                .ToList();

            List<LoadedDocumentSectionViewModel> replacedSections = new List<LoadedDocumentSectionViewModel>();

            foreach (var raw in rawSections)
            {
                var templateId = context.TBL_DOC_TEMPLATE_SECTION.Find(raw.templateSectionId)?.TEMPLATEID;
                raw.templateDocument = memo.Replace(raw.templateDocument);
                if (templateId == 1)
                {
                    raw.templateDocument = memo.UpdateEsg(raw.templateDocument);
                    raw.templateDocument = memo.UpdateGreenRating(raw.templateDocument);
                }
                replacedSections.Add(raw);
                printedDoc = raw.title;
            }

            var staff = context.TBL_STAFF.Find(staffId);

            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplatePrinted,
                STAFFID = staffId,
                BRANCHID = (short)user.BranchId,
                DETAIL = $"Printed Document Template '{ printedDoc }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = "localhost",//model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            context.SaveChanges();

            return replacedSections;
        }

        public bool LoadDocumentTemplate(DocumentTemplateViewModel entity)
        {
            var templateSections = context.TBL_DOC_TEMPLATE_SECTION
                .Where(x => x.TEMPLATEID == entity.templateId && x.ISDISABLED == false && x.DELETED == false)
                .ToList();

            var ownerId = context.TBL_STAFF.FirstOrDefault(s => s.STAFFID == entity.staffId).STAFFROLEID;

            var isOwner = context.TBL_DOC_TEMPLATE
                .Any(x => x.DELETED == false && x.OPERATIONID == entity.operationId && x.COMPANYID == entity.companyId && x.STAFFROLEID == ownerId);

            if (!isOwner)
            {
                return true;
            }

            var loadedSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.TARGETID == entity.targetId && x.OPERATIONID == entity.operationId)
                .ToList();

            foreach (var temp in templateSections)
            {
                if (loadedSections.Any(x => x.TEMPLATESECTIONID == temp.TEMPLATESECTIONID)) continue;

                context.TBL_DOC_TEMPLATE_DETAIL.Add(new TBL_DOC_TEMPLATE_DETAIL
                {
                    OPERATIONID = entity.operationId,
                    TARGETID = entity.targetId,
                    TEMPLATESECTIONID = temp.TEMPLATESECTIONID,
                    TITLE = temp.TITLE,
                    DESCRIPTION = temp.DESCRIPTION,
                    TEMPLATEDOCUMENT = temp.TEMPLATEDOCUMENT,
                    POSITION = temp.POSITION,
                    CANEDIT = temp.CANEDIT,
                    CREATEDBY = entity.staffId,
                    DATETIMECREATED = DateTime.Now,
                });
            }

            var oldSections = new List<TBL_DOC_TEMPLATE_DETAIL>();
            foreach (var sect in loadedSections)
            {
                if (templateSections.Any(x => x.TEMPLATESECTIONID == sect.TEMPLATESECTIONID)) continue;
                oldSections.Add(sect);
            }

            context.TBL_DOC_TEMPLATE_DETAIL.RemoveRange(oldSections);
            return context.SaveChanges() > 0;
        }


        public bool LoadDocumentTemplateLMS(DocumentTemplateViewModel entity)
        {
            var templateSections = context.TBL_DOC_TEMPLATE_SECTION
                .Where(x => x.TEMPLATEID == entity.templateId && x.ISDISABLED == false && x.DELETED == false)
                .ToList();

            var loadedSections = context.TBL_DOC_TEMPLATE_DETAIL
                .Where(x => x.TARGETID == entity.targetId && x.OPERATIONID == entity.lmsOperationId)
                .ToList();

            foreach (var temp in templateSections)
            {
                if (loadedSections.Any(x => x.TEMPLATESECTIONID == temp.TEMPLATESECTIONID)) continue;

                context.TBL_DOC_TEMPLATE_DETAIL.Add(new TBL_DOC_TEMPLATE_DETAIL
                {
                    OPERATIONID = entity.lmsOperationId,
                    TARGETID = entity.targetId,
                    TEMPLATESECTIONID = temp.TEMPLATESECTIONID,
                    TITLE = temp.TITLE,
                    DESCRIPTION = temp.DESCRIPTION,
                    TEMPLATEDOCUMENT = temp.TEMPLATEDOCUMENT,
                    POSITION = temp.POSITION,
                    CANEDIT = temp.CANEDIT,
                    CREATEDBY = entity.staffId,
                    DATETIMECREATED = DateTime.Now,
                });
            }

            var oldSections = new List<TBL_DOC_TEMPLATE_DETAIL>();
            foreach (var sect in loadedSections)
            {
                if (templateSections.Any(x => x.TEMPLATESECTIONID == sect.TEMPLATESECTIONID)) continue;
                oldSections.Add(sect);
            }

            context.TBL_DOC_TEMPLATE_DETAIL.RemoveRange(oldSections);
            return context.SaveChanges() > 0;
        }

        public bool SaveLoadedDocumentSection(LoadedDocumentSectionViewModel entity) // dont call if not editable
        {
            var detail = context.TBL_DOC_TEMPLATE_DETAIL.Find(entity.sectionId);
            var section = context.TBL_DOC_TEMPLATE_SECTION.Find(detail.TEMPLATESECTIONID);
            if (detail == null) return true;
            if (section.CANEDIT == false) return true;
            //if (section.CANEDIT == false) return true;

            if (entity.templateDocument != null)
            {
                detail.TEMPLATEDOCUMENT = entity.templateDocument;
                detail.LASTUPDATEDBY = entity.staffId;
                detail.DATETIMEUPDATED = DateTime.Now;

                return context.SaveChanges() > 0;

            }

            return true;
        }

        public bool AppendDigitalStamp(LoadedDocumentSectionViewModel entity) // dont call if not editable
        {
            var approvalLevel = context.TBL_APPROVAL_LEVEL.Find(entity.approvalLevelId);
            var stamp = context.TBL_DIGITAL_STAMP.Where(d => d.STAFFROLEID == approvalLevel.STAFFROLEID).FirstOrDefault();
           // var templateId = context.TBL_DOC_TEMPLATE_DETAIL.Where(s => s.TARGETID == entity.targetId && s.TITLE == stamp.DIGITALSTAMP).FirstOrDefault().TEMPLATESECTIONID;
            var detail = context.TBL_DOC_TEMPLATE_DETAIL.Find(entity.sectionId);
            if(detail == null)
            {
               // detail = context.TBL_DOC_TEMPLATE_DETAIL.Find(templateId);
            }
            var section = context.TBL_DOC_TEMPLATE_SECTION.Find(detail.TEMPLATESECTIONID);
            
            if (detail == null) return true;
            if (section.CANEDIT == false) return true;
            //if (section.CANEDIT == false) return true;
           

            if (stamp == null) return false;
            //var stamp = ByteToImage(image);
            //System.Drawing.Image digitalStamp = BinaryToImage(image);

            if (stamp != null && detail != null)
            {
                detail.TEMPLATEDOCUMENT = stamp.DIGITALSTAMP;
                detail.LASTUPDATEDBY = entity.staffId;
                detail.DATETIMEUPDATED = DateTime.Now;

                return context.SaveChanges() > 0;

            }

            return true;
        }

        public bool RemoveDigitalStamp(LoadedDocumentSectionViewModel entity) // dont call if not editable
        {
            var approvalLevel = context.TBL_APPROVAL_LEVEL.Find(entity.approvalLevelId);
            var stamp = context.TBL_DIGITAL_STAMP.Where(d => d.STAFFROLEID == approvalLevel.STAFFROLEID).FirstOrDefault();
            // var templateId = context.TBL_DOC_TEMPLATE_DETAIL.Where(s => s.TARGETID == entity.targetId && s.TITLE == stamp.DIGITALSTAMP).FirstOrDefault().TEMPLATESECTIONID;
            var detail = context.TBL_DOC_TEMPLATE_DETAIL.Find(entity.sectionId);
            if (detail == null)
            {
                // detail = context.TBL_DOC_TEMPLATE_DETAIL.Find(templateId);
            }
            var section = context.TBL_DOC_TEMPLATE_SECTION.Find(detail.TEMPLATESECTIONID);

            if (detail == null) return true;
            if (section.CANEDIT == false) return true;
            //if (section.CANEDIT == false) return true;


            if (stamp == null) return false;
            //var stamp = ByteToImage(image);
            //System.Drawing.Image digitalStamp = BinaryToImage(image);

            if (stamp != null && detail != null)
            {
                entity.templateDocument = "<p></P>";
                detail.TEMPLATEDOCUMENT = entity.templateDocument;
                detail.LASTUPDATEDBY = entity.staffId;
                detail.DATETIMEUPDATED = DateTime.Now;

                return context.SaveChanges() > 0;

            }

            return true;
        }


        public LoadedDocumentSectionViewModel GetDocumentSectionBulkLiquidation(int staffId, int operationId, int targetId, int sectionId)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            List<int> sectionIds = new List<int>();

            if (staff != null)
            {
                sectionIds = context.TBL_DOC_TEMPLATE_SECTION_ROLE
                    .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                    .Select(x => x.TEMPLATESECTIONID)
                    .ToList();
            }


            var doc = context.TBL_DOC_TEMPLATE_DETAIL.FirstOrDefault(x => x.OPERATIONID == operationId && x.DOCUMENTDETAILID == sectionId);
            var section = context.TBL_DOC_TEMPLATE_SECTION.FirstOrDefault(s => s.TEMPLATESECTIONID == doc.TEMPLATESECTIONID);
            if (doc == null) return new LoadedDocumentSectionViewModel();

            //memo.Init(operationId, targetId); //content = memo.Replace(content);
            return new LoadedDocumentSectionViewModel
            {
                sectionId = doc.DOCUMENTDETAILID,
                title = doc.TITLE,
                description = doc.DESCRIPTION,
                templateDocument = memo.Replace(doc.TEMPLATEDOCUMENT),
                canEdit = section.CANEDIT,
                editable = section.CANEDIT && sectionIds.Contains(doc.TEMPLATESECTIONID),
            };
        }

        public InsurancePolicy GetInsurancePolicyConfirmationStatus(int staffId, int appDetailId)
        {
            var insurancePolicy = (from a in context.TBL_COLLATERAL_INSURANCE_TRACKING
                        where a.DELETED == false && a.COLLATERALINSURANCETRACKINGID == appDetailId
                        select new InsurancePolicy
                        {
                            isPolicyInformationConfirmed = a.ISINFORMATIONCONFIRMED,
                            isInformationConfirmed = a.ISINFORMATIONCONFIRMED == true? "TRUE" : "FALSE",
                        }).FirstOrDefault();
            return insurancePolicy;
        }

        public InsurancePolicyRecordViewModel GetInsurancePolicyConfirmationStatusByAppDetailId(int staffId, int appDetailId)
        {
            InsurancePolicyRecordViewModel insurancePolicy = null;
            insurancePolicy = (from a in context.TBL_COLLATERAL_INSURANCE_TRACKING
                                   where a.DELETED == false && a.LOANAPPLICATIONDETAILID == appDetailId
                                   select new InsurancePolicyRecordViewModel
                                   {
                                       isPolicyInformationConfirmed = a.ISINFORMATIONCONFIRMED,
                                       isInformationConfirmed = a.ISINFORMATIONCONFIRMED == true ? "TRUE" : "FALSE",
                                   }).FirstOrDefault();
            return insurancePolicy;
        }

        public LoadedDocumentSectionViewModel GetDocumentSection(int staffId, int operationId, int targetId, int sectionId, int customerId, int targetIdForWorkFlow, bool isGeneric = false)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            List<int> sectionIds = new List<int>();

            if (staff != null)
            {
                sectionIds = context.TBL_DOC_TEMPLATE_SECTION_ROLE
                    .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                    .Select(x => x.TEMPLATESECTIONID)
                    .ToList();
            }


            var doc = context.TBL_DOC_TEMPLATE_DETAIL.FirstOrDefault(x => x.OPERATIONID == operationId && x.DOCUMENTDETAILID == sectionId);
            var section = context.TBL_DOC_TEMPLATE_SECTION.FirstOrDefault(s => s.TEMPLATESECTIONID == doc.TEMPLATESECTIONID);
            if (doc == null) return new LoadedDocumentSectionViewModel();

            if (isGeneric)
            {
                memo.InitGenericMemo(operationId, targetId, targetIdForWorkFlow, customerId); //content = memo.Replace(content);
            }
            else
            {
                memo.Init(operationId, targetId, false); //content = memo.Replace(content);
            }
            return new LoadedDocumentSectionViewModel
            {
                sectionId = doc.DOCUMENTDETAILID,
                title = doc.TITLE,
                description = doc.DESCRIPTION,
                templateDocument = memo.Replace(doc.TEMPLATEDOCUMENT),
                canEdit = section.CANEDIT,
                editable = section.CANEDIT && sectionIds.Contains(doc.TEMPLATESECTIONID),
            };
        }


        public LoadedDocumentSectionViewModel GetExceptionDocumentSection(int staffId, int operationId, int targetId, int sectionId)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            List<int> sectionIds = new List<int>();

            if (staff != null)
            {
                sectionIds = context.TBL_DOC_TEMPLATE_SECTION_ROLE
                    .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                    .Select(x => x.TEMPLATESECTIONID)
                    .ToList();
            }


            var doc = context.TBL_DOC_TEMPLATE_DETAIL.FirstOrDefault(x => x.OPERATIONID == operationId && x.DOCUMENTDETAILID == sectionId);
            var section = context.TBL_DOC_TEMPLATE_SECTION.FirstOrDefault(s => s.TEMPLATESECTIONID == doc.TEMPLATESECTIONID);
            if (doc == null) return new LoadedDocumentSectionViewModel();

            memo.InitForExceptionalLoans(operationId, targetId); 
            return new LoadedDocumentSectionViewModel
            {
                sectionId = doc.DOCUMENTDETAILID,
                title = doc.TITLE,
                description = doc.DESCRIPTION,
                templateDocument = memo.Replace(doc.TEMPLATEDOCUMENT),
                canEdit = section.CANEDIT,
                editable = section.CANEDIT && sectionIds.Contains(doc.TEMPLATESECTIONID),
            };
        }


        public LoadedDocumentSectionViewModel GetRecoveryAnalysisDocumentSection(int staffId, int operationId, int targetId, string referenceId, int sectionId)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            List<int> sectionIds = new List<int>();

            if (staff != null)
            {
                sectionIds = context.TBL_DOC_TEMPLATE_SECTION_ROLE
                    .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                    .Select(x => x.TEMPLATESECTIONID)
                    .ToList();
            }


            var doc = context.TBL_DOC_TEMPLATE_DETAIL.FirstOrDefault(x => x.OPERATIONID == operationId && x.DOCUMENTDETAILID == sectionId);
            var section = context.TBL_DOC_TEMPLATE_SECTION.FirstOrDefault(s => s.TEMPLATESECTIONID == doc.TEMPLATESECTIONID);
            if (doc == null) return new LoadedDocumentSectionViewModel();

            memo.InitRecoveryDate(targetId, referenceId); //recovery analysis
            return new LoadedDocumentSectionViewModel
            {
                sectionId = doc.DOCUMENTDETAILID,
                title = doc.TITLE,
                description = doc.DESCRIPTION,
                templateDocument = memo.Replace(doc.TEMPLATEDOCUMENT),
                canEdit = section.CANEDIT,
                editable = section.CANEDIT && sectionIds.Contains(doc.TEMPLATESECTIONID),
            };
        }

        public LoadedDocumentSectionViewModel GetThirdPartyLoanDocumentSection(int staffId, int operationId, int targetId, int sectionId)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            List<int> sectionIds = new List<int>();

            if (staff != null)
            {
                sectionIds = context.TBL_DOC_TEMPLATE_SECTION_ROLE
                    .Where(x => x.DELETED == false && x.STAFFROLEID == staff.STAFFROLEID)
                    .Select(x => x.TEMPLATESECTIONID)
                    .ToList();
            }

           /* var memoOperationId = 0;
            var memoOperation = context.TBL_DOC_TEMPLATE_DETAIL.FirstOrDefault(x => x.TARGETID == targetId && x.DOCUMENTDETAILID == sectionId);
            if(memoOperation != null)
            {
                memoOperationId = memoOperation.OPERATIONID;
            }
            else
            {
                memoOperationId = operationId;
            }*/
            var doc = context.TBL_DOC_TEMPLATE_DETAIL.FirstOrDefault(x => x.OPERATIONID == operationId && x.DOCUMENTDETAILID == sectionId);
            var section = context.TBL_DOC_TEMPLATE_SECTION.FirstOrDefault(s => s.TEMPLATESECTIONID == doc.TEMPLATESECTIONID);
            if (doc == null) return new LoadedDocumentSectionViewModel();

            memo.InitForThirdpartyLoans(operationId, targetId); //content = memo.Replace(content);
            return new LoadedDocumentSectionViewModel
            {
                sectionId = doc.DOCUMENTDETAILID,
                title = doc.TITLE,
                description = doc.DESCRIPTION,
                templateDocument = memo.Replace(doc.TEMPLATEDOCUMENT),
                canEdit = section.CANEDIT,
                editable = section.CANEDIT && sectionIds.Contains(doc.TEMPLATESECTIONID),
            };
        }

        public List<DocumentTemplateViewModel> GetDocumentTemplates(int staffId, int operationId, int companyId)
        {
            var staff = context.TBL_STAFF.Find(staffId);
            int ownerId = staff == null ? 0 : staff.STAFFROLEID;

            //List<int> roles = new List<int>();
            //roles.Add(521); //cp
            //roles.Add(6);//AO
            //roles.Add(45);//RMO
            //roles.Add(43);//PMO

            return this.context.TBL_DOC_TEMPLATE
                //.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.COMPANYID == companyId && x.STAFFROLEID == ownerId)
                //.Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.COMPANYID == companyId && roles.Contains(ownerId))
                .Where(x => x.DELETED == false && x.OPERATIONID == operationId && x.COMPANYID == companyId)
                .Select(x => new DocumentTemplateViewModel
                {
                    templateId = x.TEMPLATEID,
                    templateName = x.TEMPLATENAME,
                    canLoadDocument = x.STAFFROLEID == ownerId
                })
                .ToList();
        }

        public dynamic GetIsLLLViolated(int operationId, int targetId)
        {
            memo.Init(operationId, targetId);
            //return new ()
            //{
            var isLLLViolated = memo.IsLLLViolated();
            var legalLendingLimit = memo.getTotalLLLImpact();
            //}
            Tuple<bool, decimal> result = new Tuple<bool, decimal>(isLLLViolated, legalLendingLimit);
            return result;
        }

        #endregion DOCUMENT TEMPLATE IMPL

        #region DOCUMENT TEMPLATE SETUP
        public IEnumerable<DocumentTemplateViewModel> GetAllDocumentTemplateSetup()
        {
            return this.context.TBL_DOC_TEMPLATE.Where(x => x.DELETED == false).Select(x => new DocumentTemplateViewModel
            {
                templateId = x.TEMPLATEID,
                companyId = x.COMPANYID,
                templateName = x.TEMPLATENAME,
                staffRoleId = x.STAFFROLEID,
                operationId = x.OPERATIONID,

                //productClassId = x.ProductClassId,
            });
        }
        public IEnumerable<DocumentTemplateSectionViewModel> GetAllDocumentTemplateSectionSetup(int templateId)
        {
            return this.context.TBL_DOC_TEMPLATE_SECTION.Where(x => x.DELETED == false && x.TEMPLATEID == templateId).Select(x => new DocumentTemplateSectionViewModel
            {
                templateSectionId = x.TEMPLATESECTIONID,
                templateId = x.TEMPLATEID,
                title = x.TITLE,
                templateDocument = x.TEMPLATEDOCUMENT,
                position = x.POSITION,
                isDisabled = x.ISDISABLED,
                canEdit = x.CANEDIT,
                description = x.DESCRIPTION,
            }).OrderBy(t => t.position);
        }
        public IEnumerable<DocumentTemplateSectionRoleViewModel> GetAllDocumentTemplateSectionRoleSetup(int templateSectionId)
        {
            return this.context.TBL_DOC_TEMPLATE_SECTION_ROLE.Where(x => x.DELETED == false && x.TEMPLATESECTIONID == templateSectionId).Select(x => new DocumentTemplateSectionRoleViewModel
            {
                sectionRoleId = x.SECTIONROLEID,
                templateSectionId = x.TEMPLATESECTIONID,
                staffRoleId = x.STAFFROLEID,
            });
        }
        public bool AddDocumentTemplate(DocumentTemplateViewModel model)
        {
            //if (String.IsNullOrEmpty(model.templateDocument)) { throw new SecureException("Document is blank. Cannot create a blank document!"); }

            var data = new TBL_DOC_TEMPLATE
            {
                COMPANYID = model.companyId,
                TEMPLATENAME = model.templateName,
                STAFFROLEID = model.staffRoleId,
                OPERATIONID = model.operationId,
                //ProductClassId = model.productClassId,
                CREATEDBY = (int)model.createdBy,
                DATETIMECREATED = general.GetApplicationDate()
            };

            context.TBL_DOC_TEMPLATE.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplateAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added DocumentTemplate '{ model.templateName }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL =model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateDocumentTemplate(DocumentTemplateViewModel model, int documentTemplateId)
        {
            var data = this.context.TBL_DOC_TEMPLATE.Find(documentTemplateId);
            if (data == null)
            {
                return false;
            }
            data.COMPANYID = model.companyId;
            data.TEMPLATENAME = model.templateName;
            data.STAFFROLEID = model.staffRoleId;
            data.OPERATIONID = model.operationId;
            //data.ProductClassId = model.productClassId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplateUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated DocumentTemplate '{ model.templateName }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }
        public bool DeleteDocumentTemplate(int documentTemplateId)
        {
            var data = this.context.TBL_DOC_TEMPLATE.Find(documentTemplateId);
            if (data != null)
            {
                data.DELETED = true;
            }
            return context.SaveChanges() != 0;
        }




        public bool AddDocumentTemplateSection(DocumentTemplateSectionViewModel model)
        {
            //if (String.IsNullOrEmpty(model.templateDocument)) { throw new SecureException("Document is blank. Cannot create a blank document!"); }
            if (string.IsNullOrEmpty(model.templateDocument))
            {
                model.templateDocument = "<p> </p>";
            }
            var data = new TBL_DOC_TEMPLATE_SECTION
            {
                TEMPLATEID = model.templateId,
                TITLE = model.title,
                TEMPLATEDOCUMENT = model.templateDocument,
                POSITION = model.position,
                CANEDIT = model.canEdit,
                DESCRIPTION = model.description,
                //ProductClassId = model.productClassId,
                CREATEDBY = (int)model.createdBy,
                DATETIMECREATED = general.GetApplicationDate()
            };

            context.TBL_DOC_TEMPLATE_SECTION.Add(data);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplateSectionAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added DocumentTemplateSection '{ model.title }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }

        public bool UpdateDocumentTemplateSection(DocumentTemplateSectionViewModel model, int documentTemplateSectionId)
        {
            var data = this.context.TBL_DOC_TEMPLATE_SECTION.Find(documentTemplateSectionId);
            if (data == null)
            {
                return false;
            }
            if (string.IsNullOrEmpty(model.templateDocument))
            {
                model.templateDocument = "<p> </p>";
            }
            data.TEMPLATEID = model.templateId;
            data.TITLE = model.title;
            data.TEMPLATEDOCUMENT = model.templateDocument;
            data.POSITION = model.position;
            data.CANEDIT = model.canEdit;
            data.DESCRIPTION = model.description;
            //data.ProductClassId = model.productClassId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplateSectionUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated DocumentTemplateSection '{ model.title }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }
        public bool DeleteDocumentTemplateSection(int documentTemplateSectionId, short userBranchId, int companyId, int lastUpdatedBy, string applicationUrl, string userIPAddress)
        {

            var data = this.context.TBL_DOC_TEMPLATE_SECTION.Find(documentTemplateSectionId);
            if (data != null)
            {
                data.DELETED = true;
            }
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplateSectionDeleted,
                STAFFID = lastUpdatedBy,
                BRANCHID = userBranchId,
                DETAIL = $"Deleted DocumentTemplateSection '{ data.TITLE }' ",
                IPADDRESS =CommonHelpers.GetLocalIpAddress(),
                URL = applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }
        public bool AddDocumentTemplateSectionRole(DocumentTemplateSectionRoleViewModel model)
        {
            //if (String.IsNullOrEmpty(model.templateDocument)) { throw new SecureException("Document is blank. Cannot create a blank document!"); }

            var data = new TBL_DOC_TEMPLATE_SECTION_ROLE
            {

                TEMPLATESECTIONID = model.templateSectionId,
                DELETED = false,
                STAFFROLEID = model.staffRoleId,
                CREATEDBY = (int)model.createdBy,
                DATETIMECREATED = general.GetApplicationDate()
            };

            context.TBL_DOC_TEMPLATE_SECTION_ROLE.Add(data);
            var section = this.context.TBL_DOC_TEMPLATE_SECTION.Find(data.TEMPLATESECTIONID);
            var staff = this.context.TBL_STAFF_ROLE.Find(data.STAFFROLEID);
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplateSectionRoleAdded,
                STAFFID = model.createdBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Added DocumentTemplateSectionRole for Section ' { section.TITLE } with Staff Role {staff.STAFFROLENAME } '",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }
        public bool UpdateDocumentTemplateSectionRole(DocumentTemplateSectionRoleViewModel model)
        {
            var data = this.context.TBL_DOC_TEMPLATE_SECTION_ROLE.Find(model.sectionRoleId);
            if (data == null)
            {
                return false;
            }
            data.STAFFROLEID = model.staffRoleId;
            data.LASTUPDATEDBY = model.lastUpdatedBy;
            data.DATETIMEUPDATED = general.GetApplicationDate();
            var section = this.context.TBL_DOC_TEMPLATE_SECTION.Find(data.TEMPLATESECTIONID);
            var staff = this.context.TBL_STAFF_ROLE.Find(data.STAFFROLEID);

            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplateSectionUpdated,
                STAFFID = model.lastUpdatedBy,
                BRANCHID = (short)model.userBranchId,
                DETAIL = $"Updated DocumentTemplateSectionRole '{ model.sectionRoleId } Of Section { section.TITLE } with Staff Role {staff.STAFFROLENAME } ' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = model.applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }
        public bool DeleteDocumentTemplateSectionRole(int sectionRoleId, short userBranchId, int companyId, int lastUpdatedBy, string applicationUrl, string userIPAddress)
        {

            var data = this.context.TBL_DOC_TEMPLATE_SECTION_ROLE.Find(sectionRoleId);
            if (data != null)
            {
                data.DELETED = true;
            }
            // Audit Section ---------------------------
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = (short)AuditTypeEnum.DocumentTemplateSectionDeleted,
                STAFFID = lastUpdatedBy,
                BRANCHID = userBranchId,
                DETAIL = $"Deleted DocumentTemplateSectionRole '{ data.SECTIONROLEID }' ",
                IPADDRESS = CommonHelpers.GetLocalIpAddress(),
                URL = applicationUrl,
                APPLICATIONDATE = general.GetApplicationDate(),
                SYSTEMDATETIME = DateTime.Now,
                DEVICENAME = CommonHelpers.GetDeviceName(),
                OSNAME = CommonHelpers.FriendlyName()
            };
            this.audit.AddAuditTrail(audit);
            // End of Audit Section ---------------------

            return context.SaveChanges() != 0;
        }


        #endregion

    }
}
