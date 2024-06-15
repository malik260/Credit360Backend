using FintrakBanking.ViewModels;
using FintrakBanking.ViewModels.Notification;
using FintrakBanking.ViewModels.Setups.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FintrakBanking.Interfaces.Setups.General
{
    public interface IAlertRepository
    {
        bool AddAlertBindingMethosUpdate(int id, AlertBindingMethodsViewModel model);
        bool AddAlertPlaceholderUpdate(int id, AlertPlaceHoldersViewModel model);
        bool AddAlertBindingMethod(AlertBindingMethodsViewModel model);
        bool AddAlertPlaceholder(AlertPlaceHoldersViewModel model);
        bool UpdateAlertTitleStatus(AlertTitleViewModel model);
        string GetBusinessTeamEmails(string accountOfficerMIsCode);
        bool validateAlertCheck();
        void GetImminentMaturities();
        //void GetImminentDocumentMaturities();
        void LogEmailAlert(string messageBody, string alertSubject, List<string> recipients, string referenceCode, int targetId, string operationMethod);
        IEnumerable<AlertTitleViewModel> GetAllAlerts();
        IEnumerable<AlertTitleViewModel> GetAllAlertsForDropdown();
        bool AddAlertTitle(AlertTitleViewModel model);
        bool DeleteAlertTitle(int id, UserInfo user);
        AlertTitleViewModel GetAlertById(int id);
        bool UpdateAlertTitle(int id, AlertTitleViewModel model, UserInfo user);
        IEnumerable<AlertSetupViewModel> GetAllAlertSetup();
        AlertSetupViewModel GetAlertSetupById(int id);
        bool AddAlertSetup(AlertSetupViewModel model);
        bool UpdateAlertSetup(int id, AlertSetupViewModel model, UserInfo user);
        bool DeleteAlertSetup(int id, UserInfo user);
        IEnumerable<LevelGroupMappingViewModel> GetAllAlertLevelGroupMapping();
        LevelGroupMappingViewModel GetAlertLevelGroupMappingById(int id);
        bool AddAlertLevelGroupMapping(LevelGroupMappingViewModel model);
        bool UpdateAlertLevelGroupMapping(int id, LevelGroupMappingViewModel model, UserInfo user);
        bool DeleteAlertLevelGroupMapping(int id, UserInfo user);
        IEnumerable<AlertLevelGroupViewModel> GetAllAlertLevelGroup();
        AlertLevelGroupViewModel GetAlertLevelGroupById(int id);
        bool AddAlertLevelGroup(AlertLevelGroupViewModel model);
        bool UpdateAlertLevelGroup(int id, AlertLevelGroupViewModel model, UserInfo user);
        bool DeleteAlertLevelGroup(int id, UserInfo user);
        IEnumerable<AlertLevelViewModel> GetAllAlertLevel();
        bool AddAlertStaffRole(AlertLevelViewModel model);
        bool UpdateAlertLevel(int id, AlertLevelViewModel model, UserInfo user);
        bool DeleteAlertLevel(int id, UserInfo user);
        IEnumerable<AlertMisViewModel> GetAllUserMisCode();
        IEnumerable<AlertTitleViewModel> GetAlerts();
        IEnumerable<AlertFrequencyViewModel> GetAllFrequency();
        IEnumerable<AlertPlaceHoldersViewModel> GetAllAlertPlaceHoldersy();
        IEnumerable<AlertConditionViewModel> GetAllConditions();
        AlertConditionViewModel GetAlertConditionById(int id);
        bool AddAlertCondition(AlertConditionViewModel model);
        bool UpdateAlertCondition(int id, AlertConditionViewModel model, UserInfo user);
        bool DeleteAlertCondition(int id, UserInfo user);
        IEnumerable<TblOperationsViewModel> GetAllOperations();
        IEnumerable<StaffRoleViewModel> GetAllStaffRoles();
        IEnumerable<StaffGroupEmailViewModel> GetAllStaffGroupEmail();
        IEnumerable<AlertLevelViewModel> GetAllAlertGroupEmail();
        bool AddAlertGroupEmail(AlertLevelViewModel model);
        IEnumerable<AlertTitleViewModel> GetAllAlertsBindingMethods();
        IEnumerable<AlertTitleViewModel> GetAllBindingMethods();
    }
}
