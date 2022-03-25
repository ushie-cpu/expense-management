using AutoMapper;
using ExpenseWebApp.Core.Interfaces;
using ExpenseWebApp.Data.UnitOfWork.Abstractions;
using ExpenseWebApp.Dtos;
using ExpenseWebApp.Dtos.ExpenseAdvanceDtos;
using ExpenseWebApp.Models;
using ExpenseWebApp.Utilities;
using ExpenseWebApp.Utilities.Pagination;
using ExpenseWebApp.Utilities.ResourceFiles;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseWebApp.Core.Implementation
{
    public class ExpenseAdvanceService : IExpenseAdvance
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFormNumberGeneratorService _formNumberGeneratorService;
        static object _lock = new object();


        public ExpenseAdvanceService(IMapper mapper, IUnitOfWork unitOfWork, IFormNumberGeneratorService formNumberGeneratorService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _formNumberGeneratorService = formNumberGeneratorService;
    }

        /// <inheritdoc />
        public async Task<Response<ExpenseAdvanceFormResponseDTO>> CreateCashAdvanceAsync(CreateExpenseAdvanceDto advanceDto)
        {
            var response = await CreateNewCashAdvance(advanceDto, FormStatus.ToBeSubmitted);
            return response;
        }

        /// <inheritdoc />
        public async Task<Response<PaginatorHelper<IEnumerable<ExpenseAdvanceReturnDto>>>> GetPendingRequestsAsync(PagingDto paging, int companyId)
        {
            var query = _unitOfWork.ExpenseAdvance.GetAllExpenseAdvanceForms().Where(f => f.ExpenseStatus.Description == FormStatus.PendingApproval && f.CompanyId == companyId).AsQueryable();

            var result = await Paginator.PaginateAsync<ExpenseAdvance, ExpenseAdvanceReturnDto>
                                                                (query, paging.PageSize, paging.PageNumber, _mapper);
            return Response<PaginatorHelper<IEnumerable<ExpenseAdvanceReturnDto>>>.Success(ResourceFile.Success, result);
        }

        /// <inheritdoc/>
        public async Task<Response<ExpenseAdvanceFormResponseDTO>> SubmitAdvanceRequestAsync(SubmitExpenseAdvanceDto expenseAdvanceDto)
        {

            if (string.IsNullOrEmpty(expenseAdvanceDto.AdvanceFormNo)) return await CreateNewCashAdvance(expenseAdvanceDto, FormStatus.PendingApproval);

            if (0 >= expenseAdvanceDto.AdvanceAmount) return Response<ExpenseAdvanceFormResponseDTO>.Fail(ResourceFile.InvalidAmount, 400);

            // Get and confirm form
            var advanceForm = await _unitOfWork.ExpenseAdvance.GetExpenseAdvanceByFormNoAsync(expenseAdvanceDto.AdvanceFormNo);

            if(advanceForm == null) return Response<ExpenseAdvanceFormResponseDTO>.Fail(ResourceFile.FormNotFound, 400);

            // Get and confirm status
            var formStatus = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.PendingApproval);

            if (formStatus == null) return Response<ExpenseAdvanceFormResponseDTO>.Fail(ResourceFile.StatusNotFound, 400);

            // Update Record
            advanceForm.ExpenseStatusId = formStatus.ExpenseStatusId;

            _unitOfWork.ExpenseAdvance.Update(advanceForm);
            await _unitOfWork.SaveAsync();

            advanceForm.ExpenseStatus = formStatus;
            var responseDto = _mapper.Map<ExpenseAdvanceFormResponseDTO>(advanceForm);

            return Response<ExpenseAdvanceFormResponseDTO>.Success(ResourceFile.Success, responseDto);
        }

        ///<inheritdoc/>
        public async Task<Response<bool>> UpdateAdvanceRequestForApproverAsync(string formId, UpdateExpenseAdvanceDto updateExpense)
        {
            var expense = await _unitOfWork.ExpenseAdvance.GetExpenseAdvanceByIdAsync(formId);
            var (isValid, message) = UpdateValidation(expense, updateExpense);
            var response = new Response<bool> { Message = message };

            if (isValid != false)
            {
                var status = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(updateExpense.ExpenseStatusDescription);
                expense.ApproverNote = updateExpense.ApproverNote;
                expense.ExpenseStatusId = status.ExpenseStatusId;
                expense.ExpenseStatus = status;
                expense.ApprovedBy = updateExpense.ApprovedBy;

                _unitOfWork.ExpenseAdvance.Update(expense);
                await _unitOfWork.SaveAsync();

                response.Data = true;
                response.Succeeded = true;
                response.StatusCode = 200;
                return response;
            }
            response.StatusCode = 400;
            return response;
        }

        /// <summary>
        /// Get All Approved Expense Advance Forms
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        public async Task<Response<PaginatorHelper<IEnumerable<ExpenseAdvanceFormResponseDTO>>>> GetApprovedCashAdvanceExpenseFormsAsync(PagingDto paging) 
        {
           var advanceExpenseForms = _unitOfWork.ExpenseAdvance.GetAllExpenseAdvanceForms()
                                .Where(q => q.ExpenseStatus.Description == FormStatus.Approved)
                                .OrderByDescending(x => x.AdvanceDate).AsQueryable();
            

            if(advanceExpenseForms == null) 
            {
                return Response<PaginatorHelper<IEnumerable<ExpenseAdvanceFormResponseDTO>>>.Fail(ResourceFile.Unsuccessful);
            }

            var paginatedExpenseForms = await advanceExpenseForms.PaginateAsync<ExpenseAdvance, ExpenseAdvanceFormResponseDTO>(paging.PageSize, paging.PageNumber, _mapper);
            return Response<PaginatorHelper<IEnumerable<ExpenseAdvanceFormResponseDTO>>>.Success(ResourceFile.Success, paginatedExpenseForms);
        }

        /// <summary>
        /// Get All Requestor's Expense Advance Forms
        /// </summary>
        /// <param name="paging"></param>
        /// <param name="userId"></param>
        /// <param name="cacNumber"></param>
        /// <returns></returns>
        public async Task<Response<PaginatorHelper<IEnumerable<ExpenseAdvanceFormResponseDTO>>>> GetRequestorExpenseAdvanceFormsAsync(PagingDto paging, int userId, string cacNumber) 
        {
            var advanceExpenseForms = _unitOfWork.ExpenseAdvance.GetUserExpenseAdvanceForms(userId);
            
            if (advanceExpenseForms == null)
            {
                return Response<PaginatorHelper<IEnumerable<ExpenseAdvanceFormResponseDTO>>>.Fail(ResourceFile.UserFormNotFound);
            }

            else if (advanceExpenseForms.Any())
            {
                var paginatedExpenseAdvanceForms = await advanceExpenseForms.PaginateAsync<ExpenseAdvance, ExpenseAdvanceFormResponseDTO>(paging.PageSize, paging.PageNumber, _mapper);
                return Response<PaginatorHelper<IEnumerable<ExpenseAdvanceFormResponseDTO>>>.Success(ResourceFile.Success, paginatedExpenseAdvanceForms);
            }

            return Response<PaginatorHelper<IEnumerable<ExpenseAdvanceFormResponseDTO>>>.Success(ResourceFile.FormNotFound, null);
        }

        /// <summary>
        /// Edit cash advance form to be submitted for approval
        /// </summary>
        /// <param name="editAdvanceDto"></param>
        /// <returns>Form successfully edited</returns>
        public async Task<Response<ExpenseAdvanceFormResponseDTO>> EditExpenseAdvanceFormAsync(string formDetailId, EditExpenseAdvanceDto editAdvanceDto)
        {
            if (string.IsNullOrEmpty(formDetailId)) return Response<ExpenseAdvanceFormResponseDTO>.Fail(ResourceFile.InvalidData, 400);

            if (editAdvanceDto.AdvanceAmount <= 0) 
                return Response<ExpenseAdvanceFormResponseDTO>.Fail(ResourceFile.InvalidAmount, 400);

            var expenseAdvance = await _unitOfWork.ExpenseAdvance.GetExpenseAdvanceByIdAsync(formDetailId);
            //var formStatus = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescription();

            if(expenseAdvance is null) return Response<ExpenseAdvanceFormResponseDTO>.Fail(ResourceFile.UserFormNotFound, 404);

            if (string.Equals(expenseAdvance.ExpenseStatus.Description,FormStatus.ToBeSubmitted,StringComparison.OrdinalIgnoreCase)
               || string.Equals(expenseAdvance.ExpenseStatus.Description, FormStatus.FurtherInfoRequired, StringComparison.OrdinalIgnoreCase))
            {
                var expenseForm = _mapper.Map(editAdvanceDto, expenseAdvance);
                _unitOfWork.ExpenseAdvance.Update(expenseForm);
                await _unitOfWork.SaveAsync();

                var formToReturn = _mapper.Map<ExpenseAdvanceFormResponseDTO>(expenseForm);
                return Response<ExpenseAdvanceFormResponseDTO>.Success(ResourceFile.FormUpdateSuccess, formToReturn);
            }

            return Response<ExpenseAdvanceFormResponseDTO>.Fail(ResourceFile.FormUpdateFailed, 400);
        }

        /// <summary>
        /// delete cash advance form to be submitted for approval
        /// </summary>
        /// <param name="editAdvanceDto"></param>
        /// <returns>Form successfully edited</returns>
        public async Task<Response<string>> DiscardFormsAsync(string formId)
        {
            var expenseAdvance = await _unitOfWork.ExpenseAdvance.GetExpenseAdvanceByIdAsync(formId);
            if (expenseAdvance == null) return Response<string>.Fail(ResourceFile.FormNotFound);
            
            var formStatus = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(FormStatus.ToBeSubmitted);

            if (expenseAdvance.ExpenseStatusId == formStatus.ExpenseStatusId)
            {
                _unitOfWork.ExpenseAdvance.Delete(expenseAdvance);
                await _unitOfWork.SaveAsync();
                return Response<string>.Success(ResourceFile.Success, null);
            }
            return Response<string>.Fail(ResourceFile.Unsuccessful);
        }

        private static (bool, string) UpdateValidation(ExpenseAdvance expense, UpdateExpenseAdvanceDto updateExpense)
        {
            if (expense == null)
            {
                return (false, ResourceFile.FormNotFound);
            }

            if (expense.ExpenseStatus.Description == FormStatus.Approved)
            {
                return (false, ResourceFile.ProcessedForm);
            }

            if (updateExpense.ExpenseStatusDescription != FormStatus.Approved &&
                updateExpense.ExpenseStatusDescription != FormStatus.Rejected &&
                updateExpense.ExpenseStatusDescription != FormStatus.FurtherInfoRequired)
            {
                return (false, ResourceFile.InvalidStatus);
            }

            if (updateExpense.ExpenseStatusDescription == FormStatus.FurtherInfoRequired && string.IsNullOrWhiteSpace(updateExpense.ApproverNote))
            {
                return (false, ResourceFile.FurtherInfoNoteRequired);
            }

            if (updateExpense.ExpenseStatusDescription == FormStatus.Rejected && string.IsNullOrWhiteSpace(updateExpense.ApproverNote))
            {
                return (false, ResourceFile.RejectionNoteRequired);
            }

            return (true, string.Empty);
        }

        private async Task<Response<ExpenseAdvanceFormResponseDTO>> CreateNewCashAdvance(CreateExpenseAdvanceDto advanceDto, string status)
        {
            var expenseAdvance = _mapper.Map<ExpenseAdvance>(advanceDto);
            expenseAdvance.DateCreated = DateTime.Now;
            expenseAdvance.CompanyId = advanceDto.CompanyId;
            expenseAdvance.AdvanceFormNo = _formNumberGeneratorService.GenerateFormNumber(ResourceFile.CashAdvanceForm, advanceDto.CACNumber);

            if (advanceDto.AdvanceAmount <= 0)
            {
                return Response<ExpenseAdvanceFormResponseDTO>.Fail(ResourceFile.InvalidAmount, 400);
            }

            var formStatus = await _unitOfWork.ExpenseStatus.GetExpenseStatusByDescriptionAsync(status);
            if (formStatus == null) return Response<ExpenseAdvanceFormResponseDTO>.Fail(ResourceFile.Unsuccessful);

            expenseAdvance.ExpenseStatusId = formStatus.ExpenseStatusId;

            await _unitOfWork.ExpenseAdvance.AddAsync(expenseAdvance);
            await _unitOfWork.SaveAsync();

            expenseAdvance.ExpenseStatus = formStatus;
            var responseDto = _mapper.Map<ExpenseAdvanceFormResponseDTO>(expenseAdvance);

            return Response<ExpenseAdvanceFormResponseDTO>.Success(ResourceFile.Success, responseDto);
        }

    }
}