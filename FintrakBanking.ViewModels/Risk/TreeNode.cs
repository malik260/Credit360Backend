using System;
using System.Collections.Generic;

namespace FintrakBanking.ViewModels.Risk
{
    public class TreeNode
    {
      public   TreeNode()
        {
            TNode = new List<TreeNode>();
        }

        public int riskId { get; set; }
        public string name { get; set; }
        public string titleName { get; set; }
        public string description { get; set; }
        public Decimal weight { get; set; }
        public int parentId { get; set; }
        public int titleId { get; set; }
        public List<TreeNode> TNode { get; set; }
    }
}
