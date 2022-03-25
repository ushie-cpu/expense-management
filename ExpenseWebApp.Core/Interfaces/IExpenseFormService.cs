using ExpenseWebApp.Dtos;
using ExpenseWebApp.Dtos.ExpenseFormDetailsDtos;
using ExpenseWebApp.Dtos.ExpenseFormDto;
using ExpenseWebApp.Utilities;
using ExpenseWebApp.Utilities.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseWebApp.Core.Interfaces
{
    public interface IExpenseFormService
    {
        Task<Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>> GetAllApprovedFormsAsync(PagingDto paging, int companyId);
        Task<Response<bool>> EditExpenseFormDetailAsync(string id, ExpenseFormDetailDto editExpenseFormDto);
        Task<Response<IEnumerable<ExpenseFormDetailResponseDto>>> SaveExpenseFormAsync(string formId, List<ExpenseFormDetailDto> expenses);
        Task<Response<bool>> SubmitExpenseFormAsync(string formId, string cacNumber, List<ExpenseFormDetailDto> expenses);
        Task<Response<ExpenseFormResponseDto>> GetExpenseFormByIdAsync(string formId, string cacNumber);
        Task<Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>> GetEmployeeExpenseFormsAsync(PagingDto paging, int userId, string cacNumber);
        Task<Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>> GetApprovedExpenseFormsPaidByEmployeeAsync(PagingDto paging);
        Task<Response<PaginatorHelper<IEnumerable<ExpenseFormResponseDto>>>> GetAllSubmittedExpenseFormsAsync(PagingDto paging, int companyId);
        Task<Response<string>> DiscardExpenseFormsAsync(string formId);
        Task<Response<ExpenseFormResponseDto>> ReimburseExpenseAsync(string formId, string cacNumber, string token);
        Task<Response<ExpenseFormResponseDto>> CancelExpenseFormAsync(string formId);
        Task<Response<ExpenseFormCreateResponseDto>> CreateExpenseFormAsync(ExpenseFormCreateRequestDto expenseFormCreateDto, string cacNumber);
        Task<Response<ExpenseFormResponseDto>> ApproveFormAsync(ExpenseFormApprovalDto formApprovalDto, string formId);
     
    }
}