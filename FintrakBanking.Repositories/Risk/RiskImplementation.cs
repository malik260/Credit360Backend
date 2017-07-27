using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Risk;
using FintrakBanking.ViewModels.Risk;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;


namespace FintrakBanking.Repositories.Risk
{
    [Export(typeof(IRiskImplementation))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class RiskImplementation : IRiskImplementation
    {

        List<TreeNode> nodeList = new List<TreeNode>();
        private FinTrakBankingContext context;

        public RiskImplementation(FinTrakBankingContext _context)
        {
            context = _context;
        }
        public List<TreeNode> GetRiskIndexByRiskTitle(int companyId, int productId, int riskTypeId)
        {
            tbl_Risk_Assessment_Index[] allIndexes = (from c in context.tbl_Risk_Assessment_Index  where c.tbl_Risk_Assessment_Title.RiskTypeId == riskTypeId && c.tbl_Risk_Assessment_Title.ProductId== productId && c.CompanyId == companyId  select c).ToArray();
            var node = new TreeNode();
            tbl_Risk_Assessment_Index[] risk = (from c in context.tbl_Risk_Assessment_Index where c.CompanyId == companyId && c.tbl_Risk_Assessment_Title.RiskTypeId == riskTypeId && c.tbl_Risk_Assessment_Title.ProductId == productId && c.ParentId == 0 select c).ToArray();
            var Parents = risk.Where(c => c.ParentId == 0);
            if (Parents.Any())
            {
                TreeNode node2 = new TreeNode();
                // countParent = Parents.Count();
                foreach (var parent in Parents)
                {
                    node.indexTypeId = parent.IndexTypeId;
                    node.riskId = parent.RiskId;
                    node.description = parent.Description;
                    node.parentId = (int)parent.ParentId;
                    node.name = parent.Name;
                    node.riskAssessmentTitleId = parent.RiskAssessmentTitleId;
                    //node.titleName = context.TblRiskAssessmentTitle.FirstOrDefault(c => c.RiskTypeId == parent.RiskAssessmentTitleId).RiskTitle; 
                    node.weight = parent.Weight;

                    node.riskAssessmentTitleId = parent.RiskAssessmentTitleId;
                   
                    nodeList.Add(node);
                    AllNodes(allIndexes, parent);
                }
            }

            return nodeList;
        }

        bool visited = false;

        private void AllNodes(tbl_Risk_Assessment_Index[] allIndexes, tbl_Risk_Assessment_Index parent)
        {
            var children = allIndexes.Where(c => c.ParentId == parent.RiskId);
            foreach (var child in children)
            {
                TreeNode node2 = new TreeNode();
                node2.riskId = child.RiskId;

                node2.indexTypeId = child.IndexTypeId;
                node2.description = child.Description;
                node2.parentId = (int)child.ParentId;
                node2.name = child.Name;
                node2.riskAssessmentTitleId = child.RiskAssessmentTitleId;
               // node2.titleName = context.TblRiskAssessmentTitle.FirstOrDefault(c => c.RiskTypeId == child.RiskAssessmentTitleId).RiskTitle;
                node2.weight = child.Weight;
                nodeList.Add(node2);

                var newChildren = allIndexes.Where(c => c.ParentId == child.RiskId);

                foreach (var newChild in newChildren)
                {

                    TreeNode node3 = new TreeNode();
                    var check = nodeList.Where(c => c.riskId == newChild.RiskId);
                    if (check.Any())
                    {
                        visited = true;
                    }
                    if (!visited)
                    {
                        if (!nodeList.Any(c => c.riskId == newChild.RiskId))
                        {
                            node3.indexTypeId = newChild.IndexTypeId;
                            node3.riskId = newChild.RiskId;
                            node3.description = newChild.Description;
                            node3.parentId = (int)newChild.ParentId;
                            node3.name = newChild.Name;
                            node3.riskAssessmentTitleId = newChild.RiskAssessmentTitleId;
                         //  node3.titleName = context.TblRiskAssessmentTitle.FirstOrDefault(c => c.RiskTypeId == newChild.RiskAssessmentTitleId).RiskTitle;
                            node3.weight = newChild.Weight;
                            nodeList.Add(node3);

                            var newGrandChildren = allIndexes.Where(c => c.ParentId == child.RiskId);

                            foreach (var newGrandChild in newGrandChildren)
                            {
                                TreeNode node4 = new TreeNode();                             
                                if (!nodeList.Any(c => c.riskId == newGrandChild.RiskId))
                                {
                                    node4.indexTypeId = newGrandChild.IndexTypeId;
                                    node4.riskId = newGrandChild.RiskId;
                                    node4.description = newGrandChild.Description;
                                    node4.parentId = (int)newGrandChild.ParentId;
                                    node4.name = newGrandChild.Name;
                                    node4.riskAssessmentTitleId = newGrandChild.RiskAssessmentTitleId;
                                 //    node4.titleName = context.TblRiskAssessmentTitle.FirstOrDefault(c => c.RiskTypeId == newGrandChild.RiskAssessmentTitleId).RiskTitle; 
                                    nodeList.Add(node4);
                                }
                            }
                        }
                    }
                }
                AllNodes(allIndexes, child);
            }
        }

    }
}

