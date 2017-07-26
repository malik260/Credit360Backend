using FintrakBanking.ViewModels.Risk;
using System;
using System.Collections.Generic;
using System.Text;

namespace FintrakBanking.Interfaces.Risk
{
  public   interface IRiskImplementation
    {
        List<TreeNode> GetRiskIndexByRiskTitle(int companyId, int productId, int riskTypeId);
    }
}
