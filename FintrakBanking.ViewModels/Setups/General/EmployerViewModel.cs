using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Setups.General
{
   public  class EmployerViewModel : GeneralEntity
    {
        public int employerId { get; set; }
        public string employerName { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public string emailAddress { get; set; }
        public int cityId { get; set; }
        public string  cityName { get; set; }
        public string employerSubTypeName { get; set; }
        public short employerSubTypeId { get; set; }
        public new int createdBy { get; set; }
        public int stateId { get; set; }
        public string employerTypeName { get; set; }
        public short employerTypeId { get; set; }
        public bool active { get; set; }
        public DateTime? establishmentDate { get; set; }
        public int operationId { get; set; }
        public short approvalStatusId { get; set; }

        public int forwardAction { get; set; }
        public string approvalStatus { get; set; }
        public string comment { get; set; }
        public short vote { get; set; }
        public string currentApprovalLevel { get; set; }
        public int approvalTrailId { get; set; }
        public int? currentApprovalLevelId { get; set; }
        //public int countryId { get; set; }

    }
}
