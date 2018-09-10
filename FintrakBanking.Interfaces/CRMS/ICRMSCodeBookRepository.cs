using FintrakBanking.ViewModels.CRMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.CRMS
{
    public interface ICRMSCodeBookRepository
    {
        IQueryable<CRMSCodeBookViewModel> LoanType();
        IQueryable<CRMSCodeBookViewModel> BusinessLine();
        IQueryable<CRMSCodeBookViewModel> SubSector();
    }
}
