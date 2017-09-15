using FintrakBanking.Common.Enum;
using FintrakBanking.Entities.Models; 
using FintrakBanking.Interfaces.Admin;
using FintrakBanking.Interfaces.Credit;
using FintrakBanking.Interfaces.Customer;
using FintrakBanking.Interfaces.Setups.Approval;
using FintrakBanking.Interfaces.Setups.General;
using FintrakBanking.Interfaces.WorkFlow;
using FintrakBanking.ViewModels.WorkFlow;
using FintrakBanking.ViewModels.Setups.Approval;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;

namespace FintrakBanking.Repositories.WorkFlow
{
    public class WorkFlowRepository : IWorkFlowRepository
    {
        private tbl_Approval_Trail trail = null;
        private IApprovalLevelStaffRepository levelStaffRepo;
        private IApprovalLevelRepository approvelRepo;
        private IApprovalGroupMappingRepository groupMappingRepo;
        private IApprovalGroupRepository groupRepo;
        private IGeneralSetupRepository genSetup;
        private IAuditTrailRepository auditTrail;      
        private FinTrakBankingContext context;
        private ICustomerRepository customerRepo;

        public WorkFlowRepository(IApprovalLevelRepository _approvel, IApprovalGroupRepository _groupRepo,
        FinTrakBankingContext _context, IAuditTrailRepository _auditTrail,
            IApprovalLevelStaffRepository _level, ICustomerRepository _customerRepo,
            IGeneralSetupRepository _genSetup, IApprovalGroupMappingRepository _groupMappingRepo)
        {
            approvelRepo = _approvel;
            levelStaffRepo = _level;
            genSetup = _genSetup;
            auditTrail = _auditTrail;
            groupRepo = _groupRepo;
              customerRepo = _customerRepo;
            context = _context;
            groupMappingRepo = _groupMappingRepo;
        }

        private ApprovalLevelStaffViewModel GetStaffLevel(int staffId, int companyId, int operationId)
        {
            return levelStaffRepo.GetAllApprovalLevelStaffByStaffId(staffId, companyId, operationId);
        }

        private IEnumerable<ApprovalLevelViewModel> GetAllLevels(int operationId, int companyId)
        {
            return approvelRepo.GetAllApprovalLevel(companyId).Where(c => c.operationId == operationId).OrderBy(c => c.position);
        }

        /// <summary>
        /// When a request is initiated use the function to log it for approval
        /// </summary>
        /// <param name="Approval entity"> </param>
        /// <returns></returns>
        public async Task<Tuple<bool, ApprovalViewModel>> LogForApproval(ApprovalViewModel entity)
        {
            var nextLevel = new ApprovalLevelViewModel();
            int? fromApprovalLevelId = null;
            var currentStaffLevel = GetStaffLevel(entity.staffId, entity.companyId, entity.operationId);
            if (currentStaffLevel != null)
            {
                fromApprovalLevelId = currentStaffLevel.approvalLevelId;
                nextLevel = GetNextApprovalLevel(entity.operationId, currentStaffLevel.approvalLevelId, entity.companyId); //get next level
            }

            trail = new tbl_Approval_Trail
            {
                ArrivalDate = genSetup.GetApplicationDate(),
                ToApprovalLevelId = GetStatingApprovalLevel(entity.operationId, entity.companyId),
                TargetId = entity.targetId,
                ApprovalStatusId = entity.approvalStatusId,
                CompanyId = entity.companyId,
                RequestStaffId = entity.staffId,
                Comment = entity.comment,
                ApprovalStateId = (int)ApprovalState.Initiation,
                OperationId = entity.operationId,
                //FromApprovalLevelId = nextLevel.approvalLevelId,//= null ? GetStatingApprovalLevel(entity.operationId, entity.companyId),
                SystemArrivalDateTime = DateTime.Now,
                

            };

            if (fromApprovalLevelId.HasValue)
            {
                trail.FromApprovalLevelId = fromApprovalLevelId.Value;
            }
             

            await approvelRepo.AddApprovalTrail(trail);

            decimal amount = GetAmount(entity);

            if (await IsWithinMyLimit(entity, currentStaffLevel))
            {
                return ApproveOperation(entity);
            }

            return Tuple.Create(false, entity);
        }

        public bool CheckRouteForOperation(int operationId, int companyId)
        {

          var data =  approvelRepo.GetAllApprovalLevel(companyId).Where(c => c.operationId == operationId).ToList();
            if (!data.Any()) {
                return false;
            }  return true;         
        }

        /// <summary>
        /// Call this function for approval routing.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>this returns a tuple of bool and ApprovalViewModel</returns>
        public async Task<Tuple<bool, ApprovalViewModel>> GoForApproval(ApprovalViewModel entity)
        {
            var currentStaffLevel = GetStaffLevel(entity.staffId, entity.companyId, entity.operationId); // get current users approval level
            entity.myLevelId = currentStaffLevel.approvalLevelId;
            bool terminate = false;
            // get all approval level for the operation
            // check for approval limit and approve if it is within limit
            decimal amount = GetAmount(entity );
          //  decimal tenor = GetAmount(entity);
        
            if (amount > 0 ) //    || tenor > 0)
            {
                decimal totalLimit = currentStaffLevel.minimumAmount + currentStaffLevel.maximumAmount;
                ApprovalGroupViewModel[] allGroups = groupRepo.GetAllApprovalGroup(entity.companyId).ToArray();

                if (allGroups.FirstOrDefault(c => c.groupId == currentStaffLevel.groupId).isCommittee)
                {
                    if (IsCommitteeGroup(currentStaffLevel))
                    {
                        if (totalLimit >= amount)
                        {
                            ApprovalLevelViewModel[] allLevels = GetAllLevels(entity.operationId, entity.companyId).ToArray();
                            var level = allLevels.FirstOrDefault(c => c.approvalLevelId == currentStaffLevel.approvalLevelId);
                            var approvalTrail = GetApprovalTrail(currentStaffLevel.approvalLevelId, entity)
                                .Where(c => c.RequestStaffId != entity.staffId);
                            if (level.numberOfApprovals >= approvalTrail.Count())
                            {
                                if (UpdateCurrentTransitionState(entity))
                                {
                                    trail = new tbl_Approval_Trail
                                    {
                                        ArrivalDate = genSetup.GetApplicationDate(),
                                        FromApprovalLevelId = currentStaffLevel.approvalLevelId,
                                        ToApprovalLevelId = currentStaffLevel.approvalLevelId,
                                        TargetId = entity.targetId,
                                        ApprovalStatusId = entity.approvalStatusId,
                                        CompanyId = entity.companyId,
                                        Comment = entity.comment,
                                        SystemArrivalDateTime = DateTime.Now,
                                        SystemResponseDateTime = DateTime.Now,
                                        ApprovalStateId = (short)ApprovalState.Processing,
                                        RequestStaffId = entity.staffId,
                                        OperationId = entity.operationId,
                                    };
                                    await approvelRepo.AddApprovalTrail(trail);
                                }
                            }
                        }
                    }
                    else
                    if (await IsWithinMyLimit(entity, currentStaffLevel))
                    {
                        return ApproveOperation(entity);
                    }
                }

            }

            var nextLevel = GetNextApprovalLevel(currentStaffLevel.operationId, currentStaffLevel.approvalLevelId, entity.companyId); //get next level
            if (nextLevel != null)
            {
                if (nextLevel.routeViaStaffOrganogram)  // find the next approving officer. use organogram or continue from approval levels
                {
                    //use organogram 
                    if (UseOrganogram(entity.staffId, entity.approvalStatusId, entity.targetId, nextLevel, entity.companyId,entity.comment ))
                    {
                        return Tuple.Create(false, entity);
                    }
                }
                else
                {//use continue from approval levels 
                    if (await NextApprovingLine(entity, nextLevel,terminate))
                    {
                        if (!terminate)
                        {
                            return Tuple.Create(false, entity);
                        }
                        else
                        {
                          return  ApproveOperation(entity);
                        }                      
                    }
                }
            }
            else
            {
                //final approval
                if (UpdateCurrentTransitionState(entity))
                {
                    return Tuple.Create(true, entity);
                }

            }
            //var audit = new tbl_Audit
            //{
            //    AuditTypeId = (short)AuditTypeEnum.LoanApplication,
            //    StaffId = entity.createdBy,
            //    BranchId = (short)entity.BranchId,
            //    Detail = $"Attended to a { Operations.LoanApplication.Equals(entity.operationId).GetType().Name} request and has routed the requst for further processing.",
            //    IPAddress = entity.userIPAddress,
            //    Url = entity.applicationUrl,
            //    ApplicationDate = genSetup.GetApplicaionDate(),
            //    SystemDateTime = DateTime.Now,
            //    TargetId = entity.targetId

            //};
            //this.auditTrail.AddAuditTrail(audit);
            return Tuple.Create(false, entity);
        }

        private decimal GetAmount(ApprovalViewModel entity)
        {
            decimal amount = 0;
            if (entity.operationId == (int)OperationsEnum.LoanApplication)
            {
                var loan = context.tbl_Loan_Application.Where(c => c.CompanyId == entity.companyId && c.LoanApplicationId == entity.targetId);
                if (loan.Any())
                {
                    amount = loan.SingleOrDefault().PrincipalAmount;
                }
            }
            if (entity.operationId == (int)OperationsEnum.LoanBooking)
            { 
                var loan = context.tbl_Loan.Where(c => c.CompanyId == entity.companyId && c.LoanId == entity.targetId);
                if (loan.Any())
                {
                    amount = loan.SingleOrDefault().PrincipalAmount;
                }
            }
            return amount;
        }

        private decimal GetTenor(ApprovalViewModel entity)
        {
            decimal tenor = 0;
            if (entity.operationId == (int)OperationsEnum.LoanApplication)
            {
                var loan = context.tbl_Loan_Application.Where(c => c.CompanyId == entity.companyId && c.LoanApplicationId == entity.targetId);
                if (loan.Any())
                {
                    tenor = loan.SingleOrDefault().Tenor;
                }
            }
            if (entity.operationId == (int)OperationsEnum.LoanBooking)
            {
                var loan = context.tbl_Loan.Where(c => c.CompanyId == entity.companyId && c.LoanId == entity.targetId);
                if (loan.Any())
                {
                    tenor = (loan.SingleOrDefault().MaturityDate - loan.SingleOrDefault().EffectiveDate).Days;
                }
            }
            return tenor;
        }

        private IQueryable<tbl_Approval_Trail> GetApprovalTrail(int approvalLevelId, ApprovalViewModel entity)
        {
            ApprovalLevelViewModel[] allLevels = GetAllLevels(entity.operationId, entity.companyId).ToArray();
            var level = allLevels.FirstOrDefault(c => c.approvalLevelId == approvalLevelId);
            var userCount = approvelRepo.GetApprovalTrail(entity.operationId, entity.targetId,
                entity.approvalStatusId, level.numberOfApprovals);
            return userCount;
        }

        private bool IsCommitteeGroup(ApprovalLevelStaffViewModel staffLevel)
        {

            ApprovalGroupViewModel[] allGroups = groupRepo.GetAllApprovalGroup(staffLevel.companyId).ToArray();
            var group = allGroups.Where(c => c.groupId == staffLevel.groupId).SingleOrDefault();
            return group.isCommittee;
        }

        private int GetStatingApprovalLevel(int operationId, int companyId)
        {
            ApprovalLevelViewModel[] allLevels = GetAllLevels(operationId, companyId).ToArray();

            tbl_Approval_Group_Mapping[] allGroups = (from a in context.tbl_Approval_Group join
                                                  b in context.tbl_Approval_Group_Mapping on a.GroupId equals b.GroupId
                                                   where a.CompanyId == companyId
                                                   orderby b.Position
                                                   select b).ToArray();//.ToList().ToArray();


            var group = allGroups.Where(c => c.OperationId == operationId).OrderBy(c => c.Position).FirstOrDefault();
            var levels = allLevels.Where(c => c.groupOperationMappingId == group.GroupOperationMappingId).OrderBy(c => c.position).FirstOrDefault();
            return levels.approvalLevelId;
        }

        private ApprovalLevelViewModel GetNextApprovalLevel(int opereationId, int approvalLevelId, int companyId)
        {
            int nextGroupOperationMappingId = 0;
            int nextLevelPosition = 0;
            int groupOperationMappingId = 0;
            ApprovalLevelViewModel[] allLevels = GetAllLevels(opereationId, companyId).ToArray();
            ApprovalLevelViewModel nextLevel = null;
            ApprovalGroupMappingViewModel[] allGroups = groupMappingRepo.GetAllApprovalGroupMapping().ToArray();

            var currentLevel = GetAllLevels(opereationId, companyId).FirstOrDefault(c => c.approvalLevelId == approvalLevelId);

            groupOperationMappingId = currentLevel.groupOperationMappingId;

            var approvalLevels = allLevels.Where(c => c.groupOperationMappingId == groupOperationMappingId).OrderBy(c => c.position).ToArray();

            if (approvalLevels.Length > currentLevel.position)
            {
                nextLevelPosition = currentLevel.position + 1;
                nextLevel = approvalLevels.FirstOrDefault(c => c.position == nextLevelPosition);
            }
            if (approvalLevels.Length == currentLevel.position)
            {
                var newGroup = allGroups.Where(c => c.operationId == opereationId && c.groupOperationMappingId != groupOperationMappingId).OrderBy(c => c.position);
                if (newGroup.Any())
                {
                    nextGroupOperationMappingId = newGroup.FirstOrDefault().groupOperationMappingId;
                    nextLevel = allLevels.Where(c => c.groupOperationMappingId == nextGroupOperationMappingId).OrderBy(c => c.position).FirstOrDefault();
                }
            }
            return nextLevel;
        }

        private bool UseOrganogram(int staffId, short approvalStatusId, int targetId, ApprovalLevelViewModel entity, int companyId, string comment)
        {

            int currentLevel = 0;
            int nextLevelId = 0;

            currentLevel = GetStaffLevel(staffId, companyId, entity.operationId).approvalLevelId;
            
            // get currents staffs line manager from the organogram table
            var organogram = approvelRepo.GetStaffOrganogram(companyId).FirstOrDefault(c => c.StaffId == staffId);
            
            // get the staff id of line manager
            var lineManagerStaffId = context.tbl_Staff.Where(v => v.StaffCode == organogram.StaffCode).SingleOrDefault().StaffId;
            
            //get line managers approval level
            var lineManagerLevel = GetStaffLevel(lineManagerStaffId, companyId, entity.operationId);

            nextLevelId = lineManagerLevel.approvalLevelId;

            trail = new tbl_Approval_Trail
            {
                ArrivalDate = genSetup.GetApplicationDate(),
                FromApprovalLevelId = currentLevel,
                ToApprovalLevelId = nextLevelId,
                TargetId = targetId,
                SystemArrivalDateTime = DateTime.Now,
                ApprovalStateId = (short)ApprovalState.Processing,
                ApprovalStatusId = approvalStatusId,
                CompanyId = companyId,
                RequestStaffId = staffId,
                OperationId = entity.operationId,
                 Comment = comment
            };

            approvelRepo.AddApprovalTrail(trail);
            return true;
        }

        private bool UpdateCurrentTransitionState(ApprovalViewModel approval)
        {

            trail = new tbl_Approval_Trail
            {
                ArrivalDate = genSetup.GetApplicationDate(),
                ToApprovalLevelId = approval.myLevelId,
                SystemResponseDateTime = DateTime.Now,
                TargetId = approval.targetId,
                ApprovalStatusId = approval.approvalStatusId,
                CompanyId = approval.companyId,
                Comment = approval.comment,
                ApprovalStateId = (short)ApprovalState.Processing,
                RequestStaffId = approval.staffId,
                OperationId = approval.operationId
            };
            return approvelRepo.UpdateApprovalTrail(trail);
        }

        private async Task<bool> NextApprovingLine(ApprovalViewModel approval, ApprovalLevelViewModel entity,  bool terminate )
        {
            bool result = false;
            int currentLevel = 0;
            int nextLevelId = 0;
            terminate = false;
            var action = context.tbl_Operations.Where(c => c.OperationId == approval.operationId).FirstOrDefault();
            // terminate proccess when this condition is met
            if (approval.approvalStatusId == (int)ApprovalStatusEnum.Disapproved && action.TerminateIfDisapproved)
            {
                terminate = true;
                return UpdateCurrentTransitionState(approval);
            }

            var levelStaff = levelStaffRepo.GetAllApprovalLevelStaff(approval.companyId).FirstOrDefault(c => c.approvalLevelId == entity.approvalLevelId);
            if (levelStaff != null)
            {
             

                if (UpdateCurrentTransitionState(approval))
                {
                    currentLevel = approval.myLevelId;
                    nextLevelId = levelStaff.approvalLevelId;

                    trail = new tbl_Approval_Trail
                    {
                        ArrivalDate = genSetup.GetApplicationDate(),
                        FromApprovalLevelId = currentLevel,
                        ToApprovalLevelId = nextLevelId,SystemArrivalDateTime = DateTime.Now ,
                        TargetId = approval.targetId,
                        ApprovalStateId = (short)ApprovalState.Processing,
                        ApprovalStatusId = (int)ApprovalStatusEnum.Pending,
                        CompanyId = approval.companyId,
                        RequestStaffId = approval.staffId,
                        OperationId = approval.operationId
                         ,Comment = approval.comment 
                    };

                    result = await approvelRepo.AddApprovalTrail(trail);

                }
            }
            else
            {
                approval.approvalStatusId = (short)ApprovalState.Ended;
                UpdateCurrentTransitionState(approval);
            }
            return result;
        }
    
        private async Task<bool> IsWithinMyLimit(ApprovalViewModel approval, ApprovalLevelStaffViewModel entity)
        {
            bool terminate = false;
            bool result = false;
            if (approval.amount > 0)
            {
                if (!approval.isPoliticalyExposed)
                {
                    if ((entity.minimumAmount + entity.maximumAmount) >= approval.amount)
                    {
                        result = UpdateCurrentTransitionState(approval);
                    }
                }
                else
                {
                    var levelentity = GetAllLevels(approval.operationId , approval.companyId).Where(c=> c.approvalLevelId == approval.myLevelId ).FirstOrDefault();
                    result = await NextApprovingLine(approval, levelentity, terminate);
                    
                }
               
            }
            return result;
        }
        
        private Tuple<bool, ApprovalViewModel> ApproveOperation(ApprovalViewModel entity)
        {
            if (entity.operationId == int.Parse(OperationsEnum.LoanApplication.ToString()))
            {
                return Tuple.Create(true, entity);
            }

            if (entity.operationId == int.Parse(OperationsEnum.LoanBooking.ToString()))
            {
                return Tuple.Create(true, entity);
            }
            if (entity.operationId == int.Parse(OperationsEnum.ProductCreation.ToString()))
            {
                return Tuple.Create(true, entity);
            }
            if (entity.operationId == int.Parse(OperationsEnum.StaffCreation.ToString()))
            {
                return Tuple.Create(true, entity);
            }
            if (entity.operationId == int.Parse(OperationsEnum.UserCreation.ToString()))
            {
                return Tuple.Create(true, entity);
            }
            if (entity.operationId == int.Parse(OperationsEnum.ChartOfAccountCreation.ToString()))
            {
                return Tuple.Create(true, entity);
            }
            if (entity.operationId == int.Parse(OperationsEnum.LoanPreliminaryEvaluation.ToString()))
            {
                return Tuple.Create(true, entity);
            }
            return Tuple.Create(false, entity);
        }

    }

}


 
