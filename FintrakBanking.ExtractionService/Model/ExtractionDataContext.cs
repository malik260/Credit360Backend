namespace FintrakBanking.ExtractionService
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using  System.Data.Entity;


    namespace Model
    {
         
        public partial class ExtractionDataContext : DbContext
        {
            public ExtractionDataContext() : base("name=ExtractionDataContext"){}

           
            public virtual  DbSet<STG_FREECODE1> STG_FREECODE1 { get; set; }
            //public virtual DbSet<STG_FREECODE1> STG_FREECODE { get; set; }
            //public virtual DbSet<STG_FREECODE1> STG_FREECODE1 { get; set; }
            //public virtual DbSet<STG_FREECODE1> STG_FREECODE1 { get; set; }
            //public virtual DbSet<STG_FREECODE1> STG_FREECODE1 { get; set; }
            //public virtual DbSet<STG_FREECODE1> STG_FREECODE1 { get; set; }
            //public virtual DbSet<STG_FREECODE1> STG_FREECODE1 { get; set; }

        }
    }

}
