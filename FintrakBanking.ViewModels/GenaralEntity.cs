
using System;
using System.Collections.Generic;
using System.Text;
namespace FintrakBanking.ViewModels
{
    public class GeneralEntity
    {
        public int companyId { get; set; }
        public string companyName { get; set; }
        public string company { get; set; }

        public int createdBy { get; set; }
        public int lastUpdatedBy { get; set; }
        public DateTime dateTimeCreated { get; set; }
        public DateTime? dateTimeUpdated { get; set; }
        public bool deleted { get; set; }
        public int? deletedBy { get; set; }
        public DateTime? dateTimeDeleted { get; set; }
        public bool canModified { get; set; }
        public short userBranchId { get; set; }
        public short sourceBranchId { get; set; }
        public string userIPAddress { get; set; }
        public string applicationUrl { get; set; }
        public DateTime? systemCurrentDate { get; set; }
        public DateTime? systemArrivalDateTime { get; set; }
        public int staffId { get; set; }
        public string username { get; set; }
        public string passCode { get; set; }
        public int countryId { get; set; }

    }

}