using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FintrakBanking.Entities.Models;

namespace WinApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            var audit = new TBL_AUDIT
            {
                AUDITTYPEID = 1,
                STAFFID = 1
            };

            var trail = new TBL_APPROVAL_TRAIL
            {
                //APPROVALTRAILID = 1000000,
                FROMAPPROVALLEVELID = 31,
                TOAPPROVALLEVELID = 32,
                TARGETID = 104,
                //COMPANYID = 1,
                //REQUESTSTAFFID = 5221,
                //OPERATIONID = 6,
                //COMMENT = "test from console",
                //ARRIVALDATE =DateTime.Now.Date,
                //APPROVALSTATEID = 2,
                //APPROVALSTATUSID = 2,
                //SYSTEMARRIVALDATETIME = DateTime.Now.Date,
                //VOTE = 1,
                //TOSTAFFID = 1
            };

            var loan = new TBL_LOAN
            {
                PRODUCTID = -10,
                LOANREFERENCENUMBER = "N/A"

            };
            FinTrakBankingContext con = new FinTrakBankingContext();

            //con.Configuration.LazyLoadingEnabled = false;

            con.Database.Log = Console.Write;


            //con.TBL_LOAN.Add(loan);

            //con.TBL_AUDIT.Add(audit);


            var data = con.TBL_APPROVAL_TRAIL.Where(x =>
                            x.COMPANYID == 1
                            && x.OPERATIONID == 1
                            && x.TARGETID == 1
                            && x.RESPONSESTAFFID == null
                             && (x.APPROVALSTATEID != 3 && x.RESPONSEDATE == null)
                            );

            var info = data.ToList();

            //con.TBL_APPROVAL_TRAIL.Add(trail);
            //con.SaveChanges();

            Console.ReadLine();
        }
    }
}
