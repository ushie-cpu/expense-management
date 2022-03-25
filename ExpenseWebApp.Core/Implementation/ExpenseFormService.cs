using AutoMapper;
using ExpenseWebApp.Core.Interfaces;
using ExpenseWebApp.Data.UnitOfWork.Abstractions;
using ExpenseWebApp.Dtos;
using ExpenseWebApp.Dtos.ExpenseFormDetailsDtos;
using ExpenseWebApp.Dtos.ExpenseFormDto;
using ExpenseWebApp.Dtos.NotificationDtos;
using ExpenseWebApp.Models;
using ExpenseWebApp.Utilities;
using ExpenseWebApp.Utilities.Helpers;
using ExpenseWebApp.Utilities.Pagination;
using ExpenseWebApp.Utilities.ResourceFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


namespace ExpenseWebApp.Core.Implementation
{
    public class ExpenseFormService : IExpenseFormService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUpdateFormStatus _updateFormStatus;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ExpenseFormService> _logger;
        private readonly ICompanyService _companyService;
        private readonly IFormNumberGeneratorService _formNumberGeneratorService;
        static object _lock = new object();

        public ExpenseFormService(IUnitOfWork unitOfWork,
            IMapper mapper,
            IUpdateFormStatus updateFormStatus,
            INotificationService notificationService,
            ICompanyService companyService,
            IFormNumberGeneratorService formNumberGeneratorService,
            ILogger<ExpenseFormService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _updateFormStatus = updateFormStatus;
            _notificationService = notificationService;
            _logger = logger;
            _companyService = companyService;
            _formNumberGeneratorService = formNumberGeneratorService;
        }


        /// <summary>
        /// Gets a single expense form by id
        /// </summary>
        /// <param name="formId"></param>
        /// <returns>An expense form with its expense form details</returns>
        public async Task<Response<ExpenseFormResponseDto>> GetExpenseFormByIdAsync(string formId, string cacNumber)
        {

            var form = await _unitOfWork.ExpenseForm.GetExpenseFormByIdAsync(formId);
            var userCompany = await _companyService.GetCompanyUsersAsync(cacNumber, null);
            var user = userCompany.UserInfo.Where(user => user.Id == form.UserId).FirstOrDefault();

            if (form == null || user == null)
            {
                return Response<ExpenseFormResponseDto>.Fail("Record Not Found");
            }

            var result = _mapper.Map<ExpenseFormResponseDto>(form);

            result.EmployeeName = user.FullName;

            return Response<ExpenseFormResponseDto>.Success("Expense form with this id", result);

        }

        /// <summary>
        /// Fetches all approved forms
        /// </summary>
        /// <param name="paging"></param>
        /// <param name="companyId"></param>
        /// <returns>A paginated result of IEnumerable<ExpenseFormResponseDto></returns>
        public async Task<Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>> GetAllApprovedFormsAsync(PagingDto paging, int companyId)
        {
            var expenseForms = _unitOfWork.ExpenseForm.GetExpenseFormsByCompanyId(companyId)
                                .Where(q => q.ExpenseStatus.Description == FormStatus.Approved)
                                .OrderByDescending(x => x.DateCreated).AsQueryable();

            var paginatedExpenseForms = await expenseForms.PaginateAsync<ExpenseForm, ExpenseFormResponseDto>(paging.PageSize, paging.PageNumber, _mapper);
            return Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>.Success(ResourceFile.Success, paginatedExpenseForms);
        }

        /// <summary>
        /// This Method updates expense form detail
        /// </summary>
        /// <param name="formDetailId">form detail id</param>
        /// <param name="expenseFormDto"></param>
        /// <returns>a boolean</returns>
        public async Task<Response<bool>> EditExpenseFormDetailAsync(string formDetailId, ExpenseFormDetailDto expenseFormDto)
        {
            var expenseDetail = await _unitOfWork.ExpenseFormDetails.GetExpenseFormDetailsByIdAsync(formDetailId);

            if(expenseDetail == null) return Response<bool>.Fail(ResourceFile.FormNotFound, StatusCodes.Status404NotFound);

            if (expenseDetail.ExpenseForm.ExpenseStatus.Description.ToLower() == FormStatus.NewRequest.ToLower() ||
                expenseDetail.ExpenseForm.ExpenseStatus.Description.ToLower() == FormStatus.ToBeSubmitted.ToLower() ||
                expenseDetail.ExpenseForm.ExpenseStatus.Description.ToLower() == FormStatus.FurtherInfoRequired.ToLower())
            {
                var expenseForm = _mapper.Map(expenseFormDto, expenseDetail);
                _unitOfWork.ExpenseFormDetails.Update(expenseForm);
                await _unitOfWork.SaveAsync();

                return Response<bool>.Success(ResourceFile.FormUpdateSuccess, true);
            }
            return Response<bool>.Fail("Cannot perform this operation");
        }

        /// <summary>
        /// This method save an expense form to the database and
        /// also update the status to 'ToBeSubmitted'
        /// </summary>
        /// <param name="formId">expense form id</param>
        /// <returns>a boolean</returns>
        public async Task<Response<IEnumerable<ExpenseFormDetailResponseDto>>> SaveExpenseFormAsync(string formId, List<ExpenseFormDetailDto> expenses)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormByIdAsync(formId);

            if (expenseForm != null)
            {
                var expenseFormDetails = _mapper.Map<List<ExpenseFormDetails>>(expenses);

                expenseFormDetails.ForEach(x =>
                {
                    var getCategory = _unitOfWork.ExpenseCategory.GetExpenseCategoryByName(x.ExpenseCategory.ExpenseCategoryName);
                    x.ExpenseFormId = formId;
                    x.ExpenseCategoryId = getCategory?.ExpenseCategoryId;
                    x.ExpenseCategory = null;
                });
                _unitOfWork.ExpenseFormDetails.AddRange(expenseFormDetails);
                await _unitOfWork.SaveAsync();
                var expenseDetailForms = await _unitOfWork.ExpenseFormDetails.GetAllExpenseFormDetailsByFormIdAsync(formId);
                var result = _mapper.Map<IEnumerable<ExpenseFormDetailResponseDto>>(expenseDetailForms);
                return Response<IEnumerable<ExpenseFormDetailResponseDto>>.Success(ResourceFile.Success, result);
            }
            return Response<IEnumerable<ExpenseFormDetailResponseDto>>.Fail(ResourceFile.FormNotFound, StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// This method submit an expense form to the database with
        /// the status PendingApproval
        /// </summary>
        /// <param name="formId">expense form id</param>
        /// <returns>a boolean</returns>
        public async Task<Response<bool>> SubmitExpenseFormAsync(string formId, string cacNumber, List<ExpenseFormDetailDto> expenses)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormByIdAsync(formId);

            var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.PendingApproval);

            List<ExpenseFormDetails> form = new List<ExpenseFormDetails>();

            if (expenseForm != null)
            {
                if (expenses.Any())
                {
                    var expenseForms = _mapper.Map(expenses, form);

                    expenseForms.ForEach(x =>
                    {
                        var getCategory = _unitOfWork.ExpenseCategory.GetExpenseCategoryByName(x.ExpenseCategory.ExpenseCategoryName);
                        x.ExpenseFormId = formId;
                        x.ExpenseCategoryId = getCategory?.ExpenseCategoryId;
                        x.ExpenseCategory = null;
                    });

                    _unitOfWork.ExpenseFormDetails.AddRange(expenseForms);
                    await _unitOfWork.SaveAsync();
                }
                expenseForm.ExpenseStatus = status;
                _unitOfWork.ExpenseForm.UpdateFormStatus(expenseForm);
                var notificationDto = CreateNotificationDto(expenseForm);
                await _notificationService.SendNotificationToApproverAsync(notificationDto, cacNumber);
                return Response<bool>.Success(ResourceFile.Success, true);
            }
            return Response<bool>.Fail(ResourceFile.FormNotFound, StatusCodes.Status404NotFound);
        }

        /// <summary>
        /// A method that gets all forms belongingto an employee
        /// </summary>
        /// <param name="paging"></param>
        /// <param name="userId"></param>
        /// <returns>A paginated list of all employee's forms</returns>
        public async Task<Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>> GetEmployeeExpenseFormsAsync(PagingDto paging, int userId, string cacNumber)
        {
            var expenseForms = _unitOfWork.ExpenseForm.GetUserExpenseForms(userId);
            var userCompany = await _companyService.GetCompanyUsersAsync(cacNumber, null);
            var user = userCompany.UserInfo.Where(user => user.Id == userId).FirstOrDefault();

            if (user == null)
            {
                return Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>.Fail(ResourceFile.InvalidData);
            }


            if (expenseForms.Any())
            {
                var paginatedExpenseForms = await expenseForms.PaginateAsync<ExpenseForm, ExpenseFormResponseDto>(paging.PageSize, paging.PageNumber, _mapper);
                
                paginatedExpenseForms.PageItems.ToList().ForEach(x => x.EmployeeName = user.FullName);
                return Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>.Success(ResourceFile.Success, paginatedExpenseForms);
            }
            return Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>.Success(ResourceFile.FormNotFound, null);
        }

        /// <summary>
        /// Filters a sequence of expense forms based on status payment and approval status.
        /// </summary>
        /// <param name="paging">Page number and page size</param>
        /// <returns>Return paginated expense forms paid by employee</returns>
        public async Task<Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>> GetApprovedExpenseFormsPaidByEmployeeAsync(PagingDto paging)
        {
            var expenseForms = _unitOfWork.ExpenseForm.GetAllExpenseFormsByStatus(FormStatus.Approved)
                .Where(x => x.PaidBy.ToLower().Trim() == PaidBy.Employee.ToLower().Trim())
                .OrderByDescending(x => x.DateCreated);

            if (expenseForms.Any())
            {
                var paginatedExpenseForms = await expenseForms.PaginateAsync<ExpenseForm, ExpenseFormResponseDto>(paging.PageSize, paging.PageNumber, _mapper);
                return Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>.Success(ResourceFile.Success, paginatedExpenseForms);
            }
            return Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>.Success(ResourceFile.FormNotFound, null);
        }

        /// <summary>
        /// Filters all expense form that corresponds to the inputed company Id and whose status is Pending Approval.
        /// </summary>
        /// <param name="paging">Page number and page size</param>
        /// <returns>Paginated list of all submitted forms</returns>
        public async Task<Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>> GetAllSubmittedExpenseFormsAsync(PagingDto paging, int companyId)
        {
            _logger.LogInformation("Attempting to fetch user company id");
            var expenseForms = _unitOfWork.ExpenseForm.GetExpenseFormsByCompanyId(companyId)
                    .Where(x => x.ExpenseStatus.Description.ToLower() == FormStatus.PendingApproval.ToLower())
                    .OrderByDescending(x => x.DateCreated);

            var paginatedExpenseForms = await expenseForms.PaginateAsync<ExpenseForm, ExpenseFormResponseDto>(paging.PageSize, paging.PageNumber, _mapper);
            return Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>.Success(ResourceFile.Success, paginatedExpenseForms);
        }
       
        public async Task<Response<string>> DiscardExpenseFormsAsync(string formId)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormAsync(formId);

            if (expenseForm != null && expenseForm.ExpenseFormDetails.Count > 0)
            {
                foreach (var form in expenseForm.ExpenseFormDetails)
                {
                    _unitOfWork.ExpenseFormDetails.Delete(form);
                }
                await UpdateDatabase(expenseForm);

                foreach (var item in expenseForm.ExpenseFormDetails)
                {
                    ExpenseFormDetailsService.DeleteAttachment(item.Attachments);
                }
                
                return Response<string>.Success(ResourceFile.Success, null);
            }
            return Response<string>.Fail(ResourceFile.FormNotFound);
        }


        /// <summary>
        /// Performs either of Approve, Reject, RequireFurtherInfo operations on a form
        /// </summary>
        /// <param name="formApprovalDto"></param>
        /// <param name="formId"></param>
        /// <returns>Returns an instance of ExpenseFormResponseDto if a form with the provided formId exists
        /// in record, else returns a failed response.</returns>
        public async Task<Response<ExpenseFormResponseDto>> ApproveFormAsync(ExpenseFormApprovalDto formApprovalDto, string formId)
        {

            if (formApprovalDto.IsApproved)
            {
                var approvedForm = await _updateFormStatus.SetApprovedStatusAsync(formId, formApprovalDto.ApprovedBy);

                if (!approvedForm.Succeeded) return Response<ExpenseFormResponseDto>.Fail(approvedForm.Message);

                return await ApproveFormHelper(approvedForm.Data, formApprovalDto.CacNumber, formApprovalDto.Token);
            }

            if (formApprovalDto.IsDetailsRequired)
            {
                var updatedForm = await _updateFormStatus.SetFurtherInfoRequiredStatusAsync(formId, formApprovalDto.Note);

                if (!updatedForm.Succeeded) return Response<ExpenseFormResponseDto>.Fail(updatedForm.Message);

                return await ApproveFormHelper(updatedForm.Data, formApprovalDto.CacNumber, formApprovalDto.Token);

            }

            var rejectedForm = await _updateFormStatus.SetRejectedStatusAsync(formId, formApprovalDto.Note);

            if (!rejectedForm.Succeeded) return Response<ExpenseFormResponseDto>.Fail(rejectedForm.Message);

            return await ApproveFormHelper(rejectedForm.Data, formApprovalDto.CacNumber, formApprovalDto.Token);

        }


        /// <summary>
        /// Reimburses the Expense and sets the status of the expense form to Disbursed
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="cacNumber"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Response<ExpenseFormResponseDto>> ReimburseExpenseAsync(string formId, string cacNumber, string token)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormByIdAsync(formId);
            if (expenseForm != null && expenseForm.ExpenseStatus.Description == FormStatus.Approved)
            {
                var updateFormStatus = await _updateFormStatus.SetDisbursedStatusAsync(expenseForm.ExpenseFormId);

                if (updateFormStatus.Succeeded)
                {
                    var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.Disbursed);
                    expenseForm.ExpenseStatus = status;
                    var result = _mapper.Map<ExpenseFormResponseDto>(expenseForm);

                    var notificationDto = CreateNotificationDto(expenseForm);

                    await _notificationService.SendNotificationToFormCreatorAsync(notificationDto, cacNumber, token);
                    await _notificationService.SendNotificationToDisburserAsync(notificationDto, cacNumber, token);
                    return Response<ExpenseFormResponseDto>.Success(ResourceFile.Success, result);
                }
                else
                {
                    return Response<ExpenseFormResponseDto>.Fail(ResourceFile.FormUpdateFailed);
                }
            }

            return Response<ExpenseFormResponseDto>.Fail(ResourceFile.Unsuccessful);
        }

        /// <summary>
        /// Delete the Expense Form from the DB
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        public async Task<Response<ExpenseFormResponseDto>> CancelExpenseFormAsync(string formId)
        {
            var expenseForm = await _unitOfWork.ExpenseForm.GetExpenseFormByIdAsync(formId);

            if (expenseForm != null)
            {
                var updateFormStatus = await _updateFormStatus.SetCancelledStatusAsync(expenseForm.ExpenseFormId);

                if (updateFormStatus.Succeeded)
                {
                    var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.Cancelled);
                    expenseForm.ExpenseStatus = status;
                    var result = _mapper.Map<ExpenseFormResponseDto>(expenseForm);
                    return Response<ExpenseFormResponseDto>.Success(ResourceFile.Success, result);
                }
                return Response<ExpenseFormResponseDto>.Fail(ResourceFile.FormUpdateFailed);
            }
            return Response<ExpenseFormResponseDto>.Fail(ResourceFile.FormNotFound);
        }

        /// <summary>
        /// Creates a new instance of ExpenseForm
        /// </summary>
        /// <param name="expenseFormCreateDto"></param>
        /// <param name="cacNumber"></param>
        /// <returns></returns>
        public async Task<Response<ExpenseFormCreateResponseDto>> CreateExpenseFormAsync(ExpenseFormCreateRequestDto expenseFormCreateDto, string cacNumber)
        {
            _logger.LogInformation("Attempting to fetch user company id");
            var companyResponse = await _companyService.GetCompanyAsync(cacNumber, expenseFormCreateDto.Token);
            var userCompany = await _companyService.GetCompanyUsersAsync(cacNumber, null);
            var user = userCompany.UserInfo.Where(user => user.Id == expenseFormCreateDto.UserId).FirstOrDefault();

            var expenseForm = _mapper.Map<ExpenseForm>(expenseFormCreateDto);
            expenseForm.DateCreated = DateTime.Now;
            expenseForm.PaidBy = PaidBy.Employee;

            if (expenseForm != null && companyResponse != null)
            {
                _logger.LogInformation("user company details obtained successfully");
                expenseForm.CompanyId = companyResponse.CompanyId;

                lock (_lock)
                {
                    expenseForm.ExpenseFormNo = _formNumberGeneratorService.GenerateFormNumber(ResourceFile.ExpenseForm, cacNumber);
                }


                var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.NewRequest);

                if (status != null)
                {
                    expenseForm.ExpenseStatusId = status.ExpenseStatusId;

                    await _unitOfWork.ExpenseForm.AddAsync(expenseForm);
                    await _unitOfWork.SaveAsync();

                    var result = _mapper.Map<ExpenseFormCreateResponseDto>(expenseForm);
                    result.EmployeeName = user.FullName;
                    result.ExpenseStatus = status.Description;
                    return Response<ExpenseFormCreateResponseDto>.Success(ResourceFile.Success, result);
                }

                _logger.LogInformation("Form status not found");
                return Response<ExpenseFormCreateResponseDto>.Fail(ResourceFile.Unsuccessful);
            }

            _logger.LogInformation("Operation failed");

            return Response<ExpenseFormCreateResponseDto>.Fail(ResourceFile.Unsuccessful);

        }

       
        /// <summary>
        /// Creates an instance of NotificationCreateDto
        /// </summary>
        /// <param name="expenseForm"></param>
        /// <returns></returns>
        private NotificationCreateDto CreateNotificationDto(ExpenseForm expenseForm)
        {
            return new NotificationCreateDto()
            {
                FormNo = expenseForm.ExpenseFormNo,
                FormId = expenseForm.ExpenseFormId,
                CompanyId = expenseForm.CompanyId,
                FormStatus = expenseForm.ExpenseStatus.Description,
                IsRead = false,
                UserId = expenseForm.UserId
            };
        }       

        private async Task<bool> UpdateDatabase(ExpenseForm expenseForm)
        {
            try
            {
                _unitOfWork.ExpenseForm.Update(expenseForm);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<Response<ExpenseFormResponseDto>> ApproveFormHelper(ExpenseForm expenseForm, string cacNumber, string token)
        {
            var result = _mapper.Map<ExpenseFormResponseDto>(expenseForm); //TODO: Send Email to Requestor
            var notificationDto = CreateNotificationDto(expenseForm);
            await _notificationService.SendNotificationToFormCreatorAsync(notificationDto, cacNumber, token);
            return Response<ExpenseFormResponseDto>.Success(ResourceFile.Success, result);
        }
    }
}