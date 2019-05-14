using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Risk
{
    public class RiskAcceptanceCriteriaViewModel : GeneralEntity
    {
        public List<RacComment> comments { get; set; }
        public List<ProductRacCategory> categories { get; set; }


    }

    public class ProductRacCategory
    {
        public List<ProductRacItem> rows { get; set; }
        public string name { get; set; }
        public int id { get; set; }
    }

    public class ProductRacItem
    {
        public int id { get; set; }
        public string criteria { get; set; }
        public string required { get; set; }
        public bool hasException { get; set; }
        public string label { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public string type { get; set; }
        public int? typeId { get; set; }
        public int? optionId { get; set; }
        public List<ProductRacOption> options { get; set; }
        public int status { get; set; }
        public bool fileUpload { get; set; }

    }

    public class ProductRacOption
    {
        public int key { get; set; }
        public string label { get; set; }
    }

    public class RacComment
    {
        public int criteriaId { get; set; }
        public int staffId { get; set; }
        public String comment { get; set; }
    }
}

/* rac = {
        categories: [
            {
                name: 'PRICIPAL RAC',
                rows: [ //criterias
                    {
                        id: 13,//criteriaId
                        criteria: 'Overall Debt Service Ratio',
                        required: '33.33% of basic monthly income',
                        hasException: true,
                        label: '',
                        name: 'overall debt service',
                        value: '',
                        type: 'text',
                        typeId: 1,
                        optionId: null,
                        options: null,
                        status: 2,
                        fileUpload: false,
                    },
                    {
                        id: 34,
                        criteria: 'Overall Debt Service Ratio',
                        required: '33.33% of basic monthly income',
                        label: '',
                        hasException: false,
                        name: 'basic monthly income',
                        value: '',
                        typeId: 2,
                        type: 'text',
                        optionId: null,
                        options: null,
                        status: 3,
                        fileUpload: true,
                    },
                    */