namespace FintrakBanking.Entities.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("core.tbl_AccreditedConsultant")]
    public partial class tbl_AccreditedConsultant
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_AccreditedConsultant()
        {
            tbl_AccreditedConsultant_State = new HashSet<tbl_AccreditedConsultant_State>();
        }

        [Key]
        public int AccreditedConsultantId { get; set; }

        [StringLength(50)]
        public string RegistrationNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string FirmName { get; set; }

        public int? AccreditedConsultantTypeId { get; set; }

        public int? CompanyId { get; set; }

        public short? CityId { get; set; }

        [StringLength(50)]
        public string AccountNumber { get; set; }

        [StringLength(50)]
        public string SolicitorBVN { get; set; }

        public short? CountryId { get; set; }

        [StringLength(50)]
        public string EmailAddress { get; set; }

        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(500)]
        public string CoreCompetence { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime DateTimeCreated { get; set; }

        public DateTime? DateTimeUpdated { get; set; }

        public bool Deleted { get; set; }

        public int? DeletedBy { get; set; }

        public DateTime? DateTimeDeleted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_AccreditedConsultant_State> tbl_AccreditedConsultant_State { get; set; }

        public virtual tbl_AccreditedConsultant_Type tbl_AccreditedConsultant_Type { get; set; }
    }
}
