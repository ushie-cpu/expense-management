using ExpenseWebApp.Core.Interfaces;
using ExpenseWebApp.Dtos;
using ExpenseWebApp.Dtos.ExpenseFormDetailsDtos;
using ExpenseWebApp.Dtos.ExpenseFormDto;
using ExpenseWebApp.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpenseWebApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseFormController : ControllerBase
    {
        private readonly IExpenseFormService _expenseFormService;
        private readonly ILogger<ExpenseFormController> _logger;

        public ExpenseFormController(IExpenseFormService expenseFormService,
            ILogger<ExpenseFormController> logger)
        {
            _expenseFormService = expenseFormService;
            _logger = logger;
        }

        /// <summary>
        /// The Endpoint is used to edit an existing expense form
        /// </summary>
        /// <param name="formDetailId">The Id of the form to be edited</param>
        /// <param name="editExpenseFormDto">The model</param>
        /// <returns></returns>
        [HttpPut("{formDetailId}/edit-form-detail")]
        public async Task<ActionResult<Response<bool>>> EditExpenseForm(string formDetailId, [FromBody] ExpenseFormDetailDto editExpenseFormDto)
        {
            var result = await _expenseFormService.EditExpenseFormDetailAsync(formDetailId, editExpenseFormDto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// This Endpoint is to save an Expense form with details
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="expenses"></param>
        /// <returns></returns>
        [HttpPost("{formId}/save-form")]
        public async Task<ActionResult<Response<IEnumerable<ExpenseFormDetailResponseDto>>>> SaveExpenseForm(string formId, [FromBody] List<ExpenseFormDetailDto> expenses)
        {
            var result = await _expenseFormService.SaveExpenseFormAsync(formId, expenses);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// this Expense form is to submit an expense form
        /// </summary>
        /// <param name="formId"> the form Id</param>
        /// <param name="cacNumber">Cac number associated with the company</param>
        /// <param name="expenses"> List of Expenses to be submitted</param>
        /// <returns></returns>
        [HttpPost("{formId}/submit-form")]
        public async Task<ActionResult<Response<bool>>> SubmitExpenseForm([FromRoute]string formId,  [FromQuery]string cacNumber, [FromBody]List<ExpenseFormDetailDto> expenses)
        {
            var result = await _expenseFormService.SubmitExpenseFormAsync(formId, cacNumber, expenses);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// This Endpoint retrieve an expense form by Id
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="cacNumber"></param>
        /// <returns></returns>
        [HttpGet("{formId}/cacNumber={cacNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExpenseFormById(string formId, string cacNumber)
        {
            var response = await _expenseFormService.GetExpenseFormByIdAsync(formId, cacNumber);
            return StatusCode(response.StatusCode, response);

        }

        /// <summary>
        /// The Endpoint retrieves all expense form submitted for approval 
        /// </summary>
        /// <param name="paging"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet("Submitted/companyId={companyId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReadFormsSubmittedForApproval([FromQuery] PagingDto paging, int companyId)
        {
            var response = await _expenseFormService.GetAllSubmittedExpenseFormsAsync(paging, companyId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// This Endpoint retrieves the approved forms paid by a specific employee
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        [HttpGet("PaidByEmployees")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApprovedFormsPaidByEmployeesAsync([FromQuery] PagingDto paging)
        {
            var response = await _expenseFormService.GetApprovedExpenseFormsPaidByEmployeeAsync(paging);
            return StatusCode(response.StatusCode, response);
        }


        /// <summary>
        /// Fetches all direct expense forms that belongs to the employee
        /// </summary>
        /// <param name="paging"></param>
        /// <param name="userId"></param>
        /// <param name="cacNumber"></param>
        /// <returns></returns>
        [HttpGet("Employee/cacNumber={cacNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeExpenseForms([FromQuery] PagingDto paging, int userId, string cacNumber)
        {
            var response = await _expenseFormService.GetEmployeeExpenseFormsAsync(paging, userId, cacNumber);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// This Endpoint is use to discard an Expense form
        /// </summary>
        /// <param name="formId"></param>
        /// <returns></returns>
        [HttpDelete("DiscardExpenseDetails")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DiscardExpenseFormsAsync(string formId)
        {
            var response = await _expenseFormService.DiscardExpenseFormsAsync(formId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// This Endpoint is used to reimburse an approved expense
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="cacNumber"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPatch("{cacNumber}/reimburse-expense")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReimburseExpense(string formId, string cacNumber, string token)
        {
            _logger.LogInformation($"Geting information for the expense with formId {formId} for disbursement");
            var response = await _expenseFormService.ReimburseExpenseAsync(formId, cacNumber, token);
            _logger.LogInformation($"Gotten information for the expense with expense formId {formId}");
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// This Enpoint is used to approve form by ID
        /// </summary>
        /// <param name="model"></param>
        /// <param name="formId"></param>
        /// <returns></returns>
        [HttpPatch("ApproveForm/{formId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveForm([FromBody] ExpenseFormApprovalDto model, string formId)
        {
            _logger.LogInformation($"Geting information for the expense with formId {formId}");
            var response = await _expenseFormService.ApproveFormAsync(model, formId);
            _logger.LogInformation($"Gotten information for the expense with expense formId {formId}");
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// This Endpoint cancels an expense form
        /// </summary>
        /// <param name="formId">Id of the form to be cancelled</param>
        /// <returns></returns>
        [HttpDelete("cancel-expense-form")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelExpenseForm(string formId)
        {
            _logger.LogInformation($"Retrieving the expense form with ID {formId}");
            var response = await _expenseFormService.CancelExpenseFormAsync(formId);
            _logger.LogInformation($"Gotten information for the expense with expense formId {formId}");
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// This Enpoint retrieve all approved expense forms
        /// </summary>
        /// <param name="pagingDto"></param>
        /// <param name="companyId">The company ID</param>
        /// <returns></returns>
        [HttpGet("{companyId}/approved-expense-forms")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Response<bool>>> GetAllApprovedExpenseForms([FromQuery] PagingDto pagingDto, int companyId)
        {
            var result = await _expenseFormService.GetAllApprovedFormsAsync(pagingDto, companyId);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// This Endpoint creates a new Expense form
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cacNumber"></param>
        /// <returns></returns>
        [HttpPost("create-new-expenseForm/cacNumber={cacNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<Response<ExpenseFormCreateResponseDto>>> CreateExpenseForm([FromBody]ExpenseFormCreateRequestDto model, string cacNumber)
        
        {
            var result = await _expenseFormService.CreateExpenseFormAsync(model, cacNumber);
            return StatusCode(result.StatusCode, result);
        }         
    }
}