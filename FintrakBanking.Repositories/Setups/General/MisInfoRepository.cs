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
            var misInfo = new tbl_MIS_Info
            {
                CompanyId = entity.companyId,
                CreatedBy = entity.createdBy,
                DateTimeCreated = DateTime.Now,
                ParentMISInfoId = entity.ParentMisinfoId,
                MISCode = entity.Miscode,
                MISName = entity.Misname,
                MISTypeId = entity.MistypeId
            };
            this.context.tbl_MIS_Info.Add(misInfo);
            var response = await this.context.SaveChangesAsync();
            return response != 0;
        }

        public async Task<bool> DeleteMisInfo(int misInfoId)
        {
            return await this.context.SaveChangesAsync() != 0;
        }

        public IEnumerable<MisInfoViewModel> GetAllMisInfo()
        {
            var misInfo = from a in context.tbl_MIS_Info
                          select new MisInfoViewModel()
                          {
                              createdBy = a.CreatedBy.Value,
                              ParentMisinfoId = a.ParentMISInfoId,
                              Miscode = a.MISCode,
                              Misname = a.MISName,
                              MisinfoId = a.MISInfoId,
                              MistypeId = a.MISTypeId,
                              dateTimeCreated = (DateTime)a.DateTimeCreated,
                              MisType = context.tbl_MIS_Type.SingleOrDefault(c => c.MISTypeId == a.MISTypeId).MISType,
                          };
            return misInfo;
        }

        public MisInfoViewModel GetMisInfoById(int misInfoId)
        {
            var misInfo = (from a in context.tbl_MIS_Info
                           where a.MISInfoId == misInfoId
                           select new MisInfoViewModel()
                           {
                               createdBy = a.CreatedBy.Value,
                               ParentMisinfoId = a.ParentMISInfoId,
                               Miscode = a.MISCode,
                               Misname = a.MISName,
                               MisinfoId = a.MISInfoId,
                               MistypeId = a.MISTypeId,
                               dateTimeCreated = (DateTime)a.DateTimeCreated
                           }).SingleOrDefault();
            return misInfo;
        }

        public async Task<bool> UpdateMisInfo(int misinfoid, MisInfoViewModel entity)
        {
            var misinfo = this.context.tbl_MIS_Info.Find(misinfoid);
            misinfo.CompanyId = entity.companyId;
            misinfo.CreatedBy = entity.createdBy;
            misinfo.DateTimeUpdated = DateTime.Now;
            misinfo.ParentMISInfoId = entity.ParentMisinfoId;
            misinfo.MISCode = entity.Miscode;
            misinfo.MISName = entity.Misname;
            misinfo.MISTypeId = entity.MistypeId;
            return await this.context.SaveChangesAsync() != 0;
        }


        public IEnumerable<MisInfoViewModel> GetMisInfoByCompanyId(int coyId)
        {
            return from a in context.tbl_MIS_Info
                   where a.CompanyId == coyId
                   select new MisInfoViewModel()
                   {
                       //createdBy = a.CreatedBy.Value,
                      // ParentMisinfoId = a.ParentMisinfoId,
                       Miscode = a.MISCode,
                       Misname = a.MISName,
                       MisinfoId = a.MISInfoId                       
                   };
        }

        #endregion MisInfo



        #region MisType

        public async Task<bool> AddMisType(MisTypeViewModel entity)
        {
            var misType = new tbl_MIS_Type
            {
                Category = entity.Category,
                DateTimeCreated = entity.dateTimeCreated,
                CreatedBy = entity.createdBy,
                MISType = entity.Mistype,
            };
            this.context.tbl_MIS_Type.Add(misType);
            return await this.context.SaveChangesAsync() != 0;
        }

        public async Task<bool> DeleteMisType(int MisInfoId)
        {
            return await this.context.SaveChangesAsync() != 0;
        }

        public IEnumerable<MisTypeViewModel> GetAllMisType()
        {
            var misType = from a in context.tbl_MIS_Type
                          select new MisTypeViewModel
                          {
                              Category = a.Category,
                              createdBy = a.CreatedBy.Value,
                              dateTimeCreated = (DateTime)a.DateTimeCreated,
                              Mistype = a.MISType ,
                              MistypeId = a.MISTypeId
                          };
            return misType;
        }

        public MisTypeViewModel GetMisTypeById(int misTypeId)
        {
            var misType = (from a in context.tbl_MIS_Type
                           where a.MISTypeId == misTypeId
                           select new MisTypeViewModel
                           {
                               Category = a.Category,
                               createdBy = a.CreatedBy.Value,
                               dateTimeCreated = (DateTime)a.DateTimeCreated,
                               Mistype = a.MISType,
                               MistypeId = a.MISTypeId
                           }).SingleOrDefault();
            return misType;
        }

        public async Task<bool> UpdateMisType(int misTypeid, MisTypeViewModel entity)
        {
            var misType = this.context.tbl_MIS_Type.Find(misTypeid);
            misType.Category = entity.Category;
            misType.CreatedBy = entity.createdBy;
            misType.DateTimeUpdated = entity.dateTimeUpdated;
            misType.MISType = entity.Mistype;
            misType.MISTypeId = entity.MistypeId;
            misType.LastUpdatedBy = entity.lastUpdatedBy;

            return await this.context.SaveChangesAsync() != 0;
        }



        #endregion MisType
    }
}