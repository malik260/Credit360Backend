using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
    public class AuthourisedSignatoriesRepository
    {
        public AuthourisedSignatoriesViewModel GetSignatoryName(int id)
        {
            var entity = context.TBL_AUTHORISED_SIGNATORY.FirstOrDefault(x => x.SIGNATORYID == id && x.DELETED == false);

            return new AuthourisedSignatoriesViewModel
            {
                signatoryId = entity.SIGNATORYID,
                signatoryName = entity.SIGNATORYNAME,
            };
        }

        public IEnumerable<AuthourisedSignatoriesViewModel> GetDocumentCategorys()
        {
            return context.TBL_AUTHORISED_SIGNATORY.Where(x => x.DELETED == false)
                .Select(x => new AuthourisedSignatoriesViewModel
                {
                    signatoryId = x.SIGNATORYID,
                    signatoryName = x.SIGNATORYNAME,
                })
                .ToList();
        }

        public bool AddSignatory(AuthourisedSignatoriesViewModel model)
        {
            var entity = new TBL_AUTHORISED_SIGNATORY
            {
                SIGNATORYNAME = model.signatoryName
                CREATEDBY = model.createdBy,
                DATETIMECREATED = general.GetApplicationDate(),
            };

            context.TBL_AUTHORISED_SIGNATORY.Add(entity);
            return context.SaveChanges() != 0;
        }

        public bool UpdateSignatory(AuthourisedSignatoriesViewModel model, int id, UserInfo user)
        {
            var entity = this.context.TBL_AUTHORISED_SIGNATORY.Find(id);
            entity.DOCUMENTCATEGORYNAME = model.signatoryName;

            entity.LASTUPDATEDBY = user.createdBy;
            entity.DATETIMEUPDATED = DateTime.Now;
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
