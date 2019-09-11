using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
    public class AuthourisedSignatoryRepository : IAuthourisedSignatoryRepository
    {
        private FinTrakBankingContext context;
        private IGeneralSetupRepository general;
        public AuthourisedSignatoryRepository(
             FinTrakBankingContext context,
           IGeneralSetupRepository general
            )
        {
            this.context = context;
            this.general = general;

        }
        public AuthorisedSignatoryViewModel GetSignatory(int id)
        {
            var entity = context.TBL_AUTHORISED_SIGNATORY.FirstOrDefault(x => x.SIGNATORYID == id && x.DELETED == false);

            return new AuthorisedSignatoryViewModel
            {
                signatoryId = entity.SIGNATORYID,
                signatoryName = entity.SIGNATORYNAME,
                signatoryInitials = entity.SIGNATORYINITIALS,
                signatoryTitle = entity.SIGNATORYTITLE,
            };
        }

        public IEnumerable<AuthorisedSignatoryViewModel> GetSignatories()
        {
            return context.TBL_AUTHORISED_SIGNATORY.Where(x => x.DELETED == false)
                .Select(x => new AuthorisedSignatoryViewModel
                {
                    signatoryId = x.SIGNATORYID,
                    signatoryName = x.SIGNATORYNAME,
                    signatoryInitials = x.SIGNATORYINITIALS,
                    signatoryTitle = x.SIGNATORYTITLE,
                })
                .ToList();
        }

        public bool AddSignatory(AuthorisedSignatoryViewModel model)
        {
            var entity = new TBL_AUTHORISED_SIGNATORY
            {
                SIGNATORYNAME = model.signatoryName,
                SIGNATORYINITIALS = model.signatoryInitials,
                SIGNATORYTITLE = model.signatoryTitle,
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_AUTHORISED_SIGNATORY.Add(entity);
            return context.SaveChanges() != 0;
        }

        public bool UpdateSignatory(AuthorisedSignatoryViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_AUTHORISED_SIGNATORY.Find(id);
            entity.SIGNATORYNAME = model.signatoryName;
            entity.SIGNATORYTITLE = model.signatoryTitle;
            entity.SIGNATORYINITIALS = model.signatoryInitials;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = general.GetApplicationDate();
            return context.SaveChanges() != 0;
        }

        public bool DeleteSignatory(int id, UserInfo user)
        {
            var entity = this.context.TBL_AUTHORISED_SIGNATORY.Find(id);
            entity.DELETED = true;
            entity.DELETEDBY = user.createdBy;
            entity.DATETIMEDELETED = general.GetApplicationDate();
            return context.SaveChanges() != 0;
        }


    }
}
