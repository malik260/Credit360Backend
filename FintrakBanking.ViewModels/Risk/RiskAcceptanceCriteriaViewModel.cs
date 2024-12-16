using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.ViewModels.Risk
{
    public class RiskAcceptanceCriteriaViewModel : GeneralEntity
    {
        public int customerId { get; set; }
        public bool isOperationbased { get; set; }
        public List<RacComment> comments { get; set; }
        public List<ProductRacCategory> categories { get; set; }
        public int count { get; set; }
        public int productId { get; set; }
        public int productClassId { get; set; }
        public int racCategoryTypeId { get; set; }
        public int? loanApplicationId { get; set; }
        public int? currencyId { get; set; }
        public string currencyType { get; set; }
        public bool isDrawdown { get; set; }
        public int? operationId { get; set; }
        public short? searchBaseId { get; set; }
        public string searchBasePlaceholder { get; set; }
        public bool isAgricRac { get; set; }
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
        public IEnumerable<ProductRacOption> options { get; set; }
        public int status { get; set; }
        public bool fileUpload { get; set; }
        public int definitionId { get; set; }
        public int categoryId { get; set; }
        public bool? viewUpload { get; set; }
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

    public class RacCategoryViewModel : GeneralEntity 
    {
        public int racCategoryId { get; set; }

        public string categoryName { get; set; }
        public int racCategoryTypeId { get; set; }
        public string racCategoryType { get; set; }
        public string categoryTypeName { get; set; }
    }

    public class RacDefinitionViewModel : GeneralEntity
    {
        
        public string productClassName { get; set; }

        public short? currencyId { get; set; }

        public int racDefinitionId { get; set; }

        public int? productId { get; set; }
        public int? productClassId { get; set; }
        public int racCategoryId { get; set; }

        public bool isActive { get; set; }

        public bool isRequired { get; set; }

        public int racItemId { get; set; }

        public int racInputTypeId { get; set; }

        public int? racOptionId { get; set; }

        public int conditionalOperatorId { get; set; }

        public int definedFunctionId { get; set; }

        public bool requireUpload { get; set; }

        public int? operationId { get; set; }

        public int? approvalLevelId { get; set; }

        public int? roleId { get; set; }
        public string productName { get; set; }
        public string CategoryName { get; set; }
        public string racItemName { get; set; }
        public string racInputType { get; set; }
        public string racOptionName { get; set; }
        public string conditionalOperatorName { get; set; }
        public string definedFunctionName { get; set; }
        public string operationName { get; set; }
        public string approvalLevelName { get; set; }
        public decimal? controlAmount { get; set; }
        public decimal? controlAmountMax { get; set; }
        public int? controlOptionId { get; set; }
        public string controlOption { get; set; }
        public string racCategoryType { get; set; }
        public int? racCategoryTypeId { get; set; }
        public bool? showAtDrawDown { get; set; }
        public string currencyType { get; set; }
        public bool? requireComment { get; set; }
        public string selectedValue { get; set; }
        public string value { get; set; }
        public bool? isRacTierControlKey { get; set; }
        public string searchBasePlaceholder { get; set; }
        public string employmentType { get; set; }
        public short? customerTypeId { get; set; }
        public string criteria { get; set; }
        public bool isAgricRac { get; set; }
    }

    public class RacDetailViewModel : GeneralEntity
    {
        public int racDetailId { get; set; }

        public int racDefinitionId { get; set; }

        public int operationId { get; set; }

        public int targetId { get; set; }

        public string actualValue { get; set; }

        public int checklistStatus { get; set; }

        public int checklistStatus2 { get; set; }

        public int checklistStatus3 { get; set; }

    }

    public class RacInputTypeViewModel : GeneralEntity
    {
        public int racInputTypeId { get; set; }

        public string inputTypeName { get; set; }

        public string inputTag { get; set; }

    }

    public class RacItemViewModel : GeneralEntity
    {
        public int racItemId { get; set; }

        public string criteria { get; set; }

        public string description { get; set; }

    }

    public class RacOptionViewModel : GeneralEntity
    {
        public int racOptionId { get; set; }

        public string optionName { get; set; }

    }

    public class RacOptionItemViewModel : GeneralEntity
    {
        public int racOptionItemId { get; set; }

        public string label { get; set; }

        public int key { get; set; }

        public bool isSystemDefined { get; set; }
        public string optionName { get; set; }
        public int racOptionId { get; set; }
    }

    public class ConditionalOperatorViewModel : GeneralEntity
    {
        public int conditionalOperatorId { get; set; }

        public string operatorName { get; set; }

    }
    public class DefinedFunctionViewModel : GeneralEntity
    {
        public int definedFunctionId { get; set; }

        public string functionName { get; set; }

        public string description { get; set; }

        public bool isSystemDefined { get; set; }

    }

    public class RacCategoryTypeViewModel : GeneralEntity
    {
        public string criteria;
        public bool isActive;

        public int racCategoryId { get; set; }
        public string racCategoryType { get; set; }
        public string racCategoryName { get; set; }
        public int racCategoryTypeId { get; set; } 
        public int targetId { get; set; }
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