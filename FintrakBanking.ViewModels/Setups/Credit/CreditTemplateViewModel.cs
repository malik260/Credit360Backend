using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.Credit
{
    public class CreditTemplateViewModel : GeneralEntity
    {
        public int creditTemplateId { get; set; }
        public string templateTitle { get; set; }
        public string templateDocument { get; set; }
        public int approvalLevelId { get; set; }
        public short productClassId { get; set; }
    }

    public class LoadedDocumentSectionViewModel : GeneralEntity
    {
        public int sectionId { get; set; }
        public int operationId { get; set; }
        public int targetId { get; set; }
        public int templateSectionId { get; set; }
        public string title { get; set; }
        public string templateDocument { get; set; }
        public int position { get; set; }
        public bool canEdit { get; set; }
        public bool editable { get; set; }
        public string description { get; set; }
        public string staffRoleName { get; set; }
        public int staffRoleId { get; set; }
        public int approvalLevelId { get; set; }
    }

    public class DocumentTemplateViewModel : GeneralEntity
    {
        public int templateId { get; set; }
        public string templateName { get; set; }
        public int staffRoleId { get; set; }
        public int operationId { get; set; }
        public int lmsOperationId { get; set; }
        public int targetId { get; set; }
        public bool canLoadDocument { get; set; }
    }
    public class DocumentTemplateSectionViewModel : GeneralEntity
    {
        public int templateSectionId { get; set; }
        public int templateId { get; set; }
        public string title { get; set; }
        public string templateDocument { get; set; }
        public int position { get; set; }
        public bool isDisabled { get; set; }
        public bool canEdit { get; set; }
        public string description { get; set; }

    }
    public class DocumentTemplateSectionRoleViewModel : GeneralEntity
    {
        public int sectionRoleId { get; set; }
        public int templateSectionId { get; set; }
        public int staffRoleId { get; set; }
    }
}
