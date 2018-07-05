using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class WorkflowResponseViewModel
    {
        public int statusId { get; set; }
        public string statusName { get; set; }
        public int nextLevelId { get; set; }
        public string nextLevelName { get; set; }
        public int nextPersonId { get; set; }
        public string nextPersonName { get; set; }

        public string nextOperationName { get; set; }
    }
}
