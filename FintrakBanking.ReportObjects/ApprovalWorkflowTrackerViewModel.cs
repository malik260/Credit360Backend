using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ReportObjects.ViewModels
{
    public class GroupWorkflowTracker
    {
        public string groupName { get; set; }
        public string levelName { get; set; }
        public string order { get; set; }
    }
    public class LevelWorkflowTracker
    {
        public string levelName { get; set; }
        public string levelLimitAmount { get; set; }
        public string order { get; set; }
        public bool MyProperty { get; set; }
    }

    public class WorkFlowViewModel
    {
        public string operationName { get; set; }
        public string groupName { get; set; }
        public string vetoPower { get; set; }
        public string levelName { get; set; }
        public string username { get; set; }
        public string scope { get; set; }
        public string grpPosition { get; set; }
        public string levelPosition { get; set; }
        public string canApprove { get; set; }
        public string canEdit { get; set; }
        public string canUploadFile { get; set; }
        public string canSendJobRequest { get; set; }
        public string staffLevelId { get; set; }
        public DateTime reportDateTime { get { return DateTime.Now; } }
    }

}
