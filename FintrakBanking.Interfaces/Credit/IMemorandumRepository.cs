using FintrakBanking.ViewModels.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Credit
{
    public interface IMemorandumRepository
    {
        bool Init(int operationId, int targetId);
        string Replace(string content);
        ClassifiedAssetManagementViewModel ClassifiedAssetManagementtReviewTemplate(string applicationReferenceNumber);
    }
}
