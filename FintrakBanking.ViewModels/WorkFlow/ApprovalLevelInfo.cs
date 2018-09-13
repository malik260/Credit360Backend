using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.WorkFlow
{
    public class ApprovalLevelInfo
    {
        public int groupId { get; set; }
        public int groupPosition { get; set; }
        public int levelPosition { get; set; }
        public int levelId { get; set; }
        public string levelName { get; set; }
        public int? staffRoleId { get; set; }
        public int? levelTypeId { get; set; }
    }
}
