using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Credit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Credit
{
   public class FeeConcessionRepository : IFeeConcessionRepository
    {
        private FinTrakBankingContext context;
        public FeeConcessionRepository(FinTrakBankingContext _context)
        {
            context = _context;
        }
    }
}
