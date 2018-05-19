namespace FintrakBanking.Entities.StagingModels
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class FinTrakBankingStagingContext : DbContext
    {
        public FinTrakBankingStagingContext()
            : base("name=FinTrakBankingStagingContext")
        {
        }

        public virtual DbSet<STG_BRANCH> STG_BRANCH { get; set; }
        public virtual DbSet<STG_LOAN_MART> STG_LOAN_MART { get; set; }
        public virtual DbSet<STG_STAFFMIS> STG_STAFFMIS { get; set; }
        public virtual DbSet<STG_CASA_BALANCE> STG_CASA_BALANCE { get; set; }
        public virtual DbSet<STG_CASA_DAILY_BALANCE> STG_CASA_DAILY_BALANCE { get; set; }
        public virtual DbSet<STG_CUSTOMER> STG_CUSTOMER { get; set; }
        public virtual DbSet<STG_CUSTOMER_CLIENT> STG_CUSTOMER_CLIENT { get; set; }
        public virtual DbSet<STG_CUSTOMER_DIRECTOR> STG_CUSTOMER_DIRECTOR { get; set; }
        public virtual DbSet<STG_CUSTOMER_SHAREHOLDER> STG_CUSTOMER_SHAREHOLDER { get; set; }
        public virtual DbSet<STG_CUSTOMER_SUPPLIER> STG_CUSTOMER_SUPPLIER { get; set; }
        public virtual DbSet<STG_FINSCAN> STG_FINSCAN { get; set; }
        public virtual DbSet<STG_LOAN_WATCHLIST> STG_LOAN_WATCHLIST { get; set; }
        public virtual DbSet<STG_PRICE_INDEX_RATE> STG_PRICE_INDEX_RATE { get; set; }
        public virtual DbSet<TBL_CUSTOMER_SIGNATORY> TBL_CUSTOMER_SIGNATORY { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.BRANCHCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.BRANCHNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.COMPANYCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.ADDRESSLINE1)
                .IsUnicode(false);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.ADDRESSLINE2)
                .IsUnicode(false);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.COMMENTS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.STATECODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.CITYCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.STATENAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_BRANCH>()
                .Property(e => e.CITYNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.PRODUCTCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.CASAACCOUNT)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.BRANCHCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.LOANREFERENCENUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.INTERESTRATE)
                .HasPrecision(15, 0);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.PRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.OUTSTANDINGPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.CLASSIFICATION)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.SUBCLASSIFICATION)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.CASABALANCE)
                .HasPrecision(19, 4);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.STAFFCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.SECTOR)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.SUBSECTOR)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.CURRENCY)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_MART>()
                .Property(e => e.EXCHANGERATE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.USERNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.STAFFCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.MISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.MISNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.MISTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.FIRSTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.LASTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.MIDDLENAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.PHONE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.EMAIL)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.ADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.GENDER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.BRANCHCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.DEPARTMENTCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.SUPERVISORSTAFFCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.RANKCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.STATUS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_STAFFMIS>()
                .Property(e => e.TEAMSTRUCTURE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CASA_BALANCE>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_CASA_BALANCE>()
                .Property(e => e.ACCOUNTNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CASA_BALANCE>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CASA_BALANCE>()
                .Property(e => e.ACCOUNTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CASA_BALANCE>()
                .Property(e => e.ACCOUNTBALANCE)
                .HasPrecision(19, 2);

            modelBuilder.Entity<STG_CASA_BALANCE>()
                .Property(e => e.TOTALINFLOW)
                .HasPrecision(19, 2);

            modelBuilder.Entity<STG_CASA_BALANCE>()
                .Property(e => e.TOTALOUTFLOW)
                .HasPrecision(19, 2);

            modelBuilder.Entity<STG_CASA_DAILY_BALANCE>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_CASA_DAILY_BALANCE>()
                .Property(e => e.ACCOUNTNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CASA_DAILY_BALANCE>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CASA_DAILY_BALANCE>()
                .Property(e => e.ACCOUNTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CASA_DAILY_BALANCE>()
                .Property(e => e.ACCOUNTBALANCE)
                .HasPrecision(19, 2);

            modelBuilder.Entity<STG_CASA_DAILY_BALANCE>()
                .Property(e => e.TOTALINFLOW)
                .HasPrecision(19, 2);

            modelBuilder.Entity<STG_CASA_DAILY_BALANCE>()
                .Property(e => e.TOTALOUTFLOW)
                .HasPrecision(19, 2);

            modelBuilder.Entity<STG_CASA_DAILY_BALANCE>()
                .Property(e => e.CURRENCY)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.BRANCHCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.TITLE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.CONTACTADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.LASTCONTACTADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.FIRSTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.MIDDLENAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.LASTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.GENDER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.NATIONALITY)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.MARITALSTATUS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.EMAILADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.MAIDENNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.OCCUPATION)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.CUSTOMERTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.RELATIONSHIPOFFICERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.MISCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.STAFFCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.FSCAPTIONGROUPCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.TAXIDNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.BUSINESSTAXIDNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.ELECTRICMETERNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.BANKVERIFICATIONNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.OFFICEADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.NEARESTLANDMARK)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.PAIDUPCAPITAL)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.AUTHORIZEDCAPITAL)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.EMPLOYERDETAILS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.RCNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.SUBSECTORCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER>()
                .Property(e => e.SECTORCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_CLIENT>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_CUSTOMER_CLIENT>()
                .Property(e => e.FIRSTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_CLIENT>()
                .Property(e => e.LASTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_CLIENT>()
                .Property(e => e.ADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_CLIENT>()
                .Property(e => e.PHONENUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_CLIENT>()
                .Property(e => e.EMAIL)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_CLIENT>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_CLIENT>()
                .Property(e => e.CLIENTTYPE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_DIRECTOR>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_CUSTOMER_DIRECTOR>()
                .Property(e => e.FIRSTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_DIRECTOR>()
                .Property(e => e.LASTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_DIRECTOR>()
                .Property(e => e.ADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_DIRECTOR>()
                .Property(e => e.EMAIL)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_DIRECTOR>()
                .Property(e => e.PHONENUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_DIRECTOR>()
                .Property(e => e.BVN)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_DIRECTOR>()
                .Property(e => e.SHAREHOLDERPERCENTAGE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_DIRECTOR>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SHAREHOLDER>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_CUSTOMER_SHAREHOLDER>()
                .Property(e => e.FIRSTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SHAREHOLDER>()
                .Property(e => e.LASTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SHAREHOLDER>()
                .Property(e => e.BVN)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SHAREHOLDER>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SHAREHOLDER>()
                .Property(e => e.TYPE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SUPPLIER>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_CUSTOMER_SUPPLIER>()
                .Property(e => e.FIRSTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SUPPLIER>()
                .Property(e => e.LASTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SUPPLIER>()
                .Property(e => e.ADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SUPPLIER>()
                .Property(e => e.PHONENUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SUPPLIER>()
                .Property(e => e.EMAIL)
                .IsUnicode(false);

            modelBuilder.Entity<STG_CUSTOMER_SUPPLIER>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_FINSCAN>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_FINSCAN>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_FINSCAN>()
                .Property(e => e.FIRSTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_FINSCAN>()
                .Property(e => e.LASTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<STG_FINSCAN>()
                .Property(e => e.BVN)
                .IsUnicode(false);

            modelBuilder.Entity<STG_FINSCAN>()
                .Property(e => e.RCNUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.PRODUCTCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.CASAACCOUNT)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.BRANCHCODE)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.LOANREFERENCENUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.INTERESTRATE)
                .HasPrecision(15, 0);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.PRINCIPALAMOUNT)
                .HasPrecision(19, 4);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.OUTSTANDINGPRINCIPAL)
                .HasPrecision(19, 4);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.CLASSIFICATION)
                .IsUnicode(false);

            modelBuilder.Entity<STG_LOAN_WATCHLIST>()
                .Property(e => e.SUBCLASSIFICATION)
                .IsUnicode(false);

            modelBuilder.Entity<STG_PRICE_INDEX_RATE>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_PRICE_INDEX_RATE>()
                .Property(e => e.BID_RATE)
                .HasPrecision(15, 0);

            modelBuilder.Entity<STG_PRICE_INDEX_RATE>()
                .Property(e => e.PRICEINDEX)
                .IsUnicode(false);

            modelBuilder.Entity<STG_PRICE_INDEX_RATE>()
                .Property(e => e.OFFER_RATE)
                .HasPrecision(38, 0);

            modelBuilder.Entity<STG_PRICE_INDEX_RATE>()
                .Property(e => e.CURRENCY)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_SIGNATORY>()
                .Property(e => e.ID)
                .HasPrecision(38, 0);

            modelBuilder.Entity<TBL_CUSTOMER_SIGNATORY>()
                .Property(e => e.FIRSTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_SIGNATORY>()
                .Property(e => e.LASTNAME)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_SIGNATORY>()
                .Property(e => e.ADDRESS)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_SIGNATORY>()
                .Property(e => e.PHONENUMBER)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_SIGNATORY>()
                .Property(e => e.EMAIL)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_SIGNATORY>()
                .Property(e => e.BVN)
                .IsUnicode(false);

            modelBuilder.Entity<TBL_CUSTOMER_SIGNATORY>()
                .Property(e => e.CUSTOMERCODE)
                .IsUnicode(false);
        }
    }
}
