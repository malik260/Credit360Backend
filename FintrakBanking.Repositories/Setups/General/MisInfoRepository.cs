using FintrakBanking.Entities.Models;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.Setups.General
{
    [Export(typeof(IMisInfoRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MisInfoRepository : IMisInfoRepository
    {
        private FinTrakBankingContext context;

        public MisInfoRepository(FinTrakBankingContext _context)
        {
            this.context = _context;
        }

        #region MisInfo

        public async Task<bool> AddMisInfo(MisInfoViewModel entity)
        {
            var misInfo = new TBL_MIS_INFO
            {
                COMPANYID = entity.companyId,
                CREATEDBY = entity.createdBy,
                DATETIMECREATED = DateTime.Now,
                PARENTMISINFOID = entity.ParentMisinfoId,
                MISCODE = entity.Miscode,
                MISNAME = entity.Misname,
                MISTYPEID = entity.MistypeId
            };
            this.context.TBL_MIS_INFO.Add(misInfo);
            var response = await this.context.SaveChangesAsync();
            return response != 0;
        }

        public async Task<bool> DeleteMisInfo(int misInfoId)
        {
            return await this.context.SaveChangesAsync() != 0;
        }

        public IEnumerable<MisInfoViewModel> GetAllMisInfo()
        {
            var misInfo = from a in context.TBL_MIS_INFO
                          select new MisInfoViewModel()
                          {
                              createdBy = a.CREATEDBY.Value,
                              ParentMisinfoId = a.PARENTMISINFOID,
                              Miscode = a.MISCODE,
                              Misname = a.MISNAME,
                              MisinfoId = a.MISINFOID,
                              MistypeId = a.MISTYPEID,
                              dateTimeCreated = (DateTime)a.DATETIMECREATED,
                              MisType = context.TBL_MIS_TYPE.SingleOrDefault(c => c.MISTYPEID == a.MISTYPEID).MISTYPE,
                          };
            return misInfo;
        }

        public MisInfoViewModel GetMisInfoById(int misInfoId)
        {
            var misInfo = (from a in context.TBL_MIS_INFO
                           where a.MISINFOID == misInfoId
                           select new MisInfoViewModel()
                           {
                               createdBy = a.CREATEDBY.Value,
                               ParentMisinfoId = a.PARENTMISINFOID,
                               Miscode = a.MISCODE,
                               Misname = a.MISNAME,
                               MisinfoId = a.MISINFOID,
                               MistypeId = a.MISTYPEID,
                               dateTimeCreated = (DateTime)a.DATETIMECREATED
                           }).SingleOrDefault();
            return misInfo;
        }

        public async Task<bool> UpdateMisInfo(int misinfoid, MisInfoViewModel entity)
        {
            var misinfo = this.context.TBL_MIS_INFO.Find(misinfoid);
            misinfo.COMPANYID = entity.companyId;
            misinfo.CREATEDBY = entity.createdBy;
            misinfo.DATETIMEUPDATED = DateTime.Now;
            misinfo.PARENTMISINFOID = entity.ParentMisinfoId;
            misinfo.MISCODE = entity.Miscode;
            misinfo.MISNAME = entity.Misname;
            misinfo.MISTYPEID = entity.MistypeId;
            return await this.context.SaveChangesAsync() != 0;
        }


        public IEnumerable<MisInfoViewModel> GetMisInfoByCompanyId(int coyId)
        {
            return from a in context.TBL_MIS_INFO
                   where a.COMPANYID == coyId
                   select new MisInfoViewModel()
                   {
                       //createdBy = a.CreatedBy.Value,
                      // ParentMisinfoId = a.ParentMisinfoId,
                       Miscode = a.MISCODE,
                       Misname = a.MISNAME,
                       MisinfoId = a.MISINFOID                       
                   };
        }

        #endregion MisInfo



        #region MisType

        public async Task<bool> AddMisType(MisTypeViewModel entity)
        {
            var misType = new TBL_MIS_TYPE
            {
                CATEGORY = entity.Category,
                DATETIMECREATED = entity.dateTimeCreated,
                CREATEDBY = entity.createdBy,
                MISTYPE = entity.Mistype,
            };
            this.context.TBL_MIS_TYPE.Add(misType);
            return await this.context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteMisType(int MisInfoId)
        {
            return await this.context.SaveChangesAsync() != 0;
        }

        public IEnumerable<MisTypeViewModel> GetAllMisType()
        {
            var misType = from a in context.TBL_MIS_TYPE
                          select new MisTypeViewModel
                          {
                              Category = a.CATEGORY,
                              createdBy = a.CREATEDBY.Value,
                              dateTimeCreated = (DateTime)a.DATETIMECREATED,
                              Mistype = a.MISTYPE ,
                              MistypeId = a.MISTYPEID
                          };
            return misType;
        }

        public MisTypeViewModel GetMisTypeById(int misTypeId)
        {
            var misType = (from a in context.TBL_MIS_TYPE
                           where a.MISTYPEID == misTypeId
                           select new MisTypeViewModel
                           {
                               Category = a.CATEGORY,
                               createdBy = a.CREATEDBY.Value,
                               dateTimeCreated = (DateTime)a.DATETIMECREATED,
                               Mistype = a.MISTYPE,
                               MistypeId = a.MISTYPEID
                           }).SingleOrDefault();
            return misType;
        }

        public async Task<bool> UpdateMisType(int misTypeid, MisTypeViewModel entity)
        {
            var misType = this.context.TBL_MIS_TYPE.Find(misTypeid);
            misType.CATEGORY = entity.Category;
            misType.CREATEDBY = entity.createdBy;
            misType.DATETIMEUPDATED = entity.dateTimeUpdated;
            misType.MISTYPE = entity.Mistype;
            misType.MISTYPEID = entity.MistypeId;
            misType.LASTUPDATEDBY = entity.lastUpdatedBy;

            return await this.context.SaveChangesAsync() != 0;
        }



        #endregion MisType
    }
}