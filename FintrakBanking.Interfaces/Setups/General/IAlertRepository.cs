using FintrakBanking.ViewModels;
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
        IEnumerable<AlertViewModel> GetAllAlerts();
        bool AddAlertTitle(AlertViewModel model);
        bool DeleteAlertTitle(int id, UserInfo user);
        AlertViewModel GetAlertById(int id);
        bool UpdateAlertTitle(int id, AlertViewModel model, UserInfo user);
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
        AlertLevelViewModel GetAlertLevelById(int id);
        bool AddAlertLevel(AlertLevelViewModel model);
        bool UpdateAlertLevel(int id, AlertLevelViewModel model, UserInfo user);
        bool DeleteAlertLevel(int id, UserInfo user);
        IEnumerable<AlertMisViewModel> GetAllUserMisCode();
    }
}
