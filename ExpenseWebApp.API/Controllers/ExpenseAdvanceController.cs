using ExpenseWebApp.Core.Interfaces;
using ExpenseWebApp.Dtos;
using ExpenseWebApp.Dtos.ExpenseAdvanceDtos;
using ExpenseWebApp.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ExpenseWebApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseAdvanceController : ControllerBase
    {
        private readonly IExpenseAdvance _expenseAdvance;

        public ExpenseAdvanceController(IExpenseAdvance expenseAdvance)
        {
            _expenseAdvance = expenseAdvance;
        }

        /// <summary>
        /// The endpoint submits an Advance Request Form
        /// </summary>
        /// <param name="submitExpenseAdvanceDto">model to submit</param>
        /// <returns></returns>
        [HttpPost("SubmitAdvanceRequest")]
        public async Task<IActionResult> SubmitAdvanceRequest(SubmitExpenseAdvanceDto submitExpenseAdvanceDto)
        {
            var result = await _expenseAdvance.SubmitAdvanceRequestAsync(submitExpenseAdvanceDto);
            return StatusCode(result.StatusCode, result);
        }


        /// <summary>
        /// This Endpoint create a new cash Advance form
        /// </summary>
        /// <param name="expenseAdvanceDto">model containing form details to be created</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateCashAdvance(CreateExpenseAdvanceDto expenseAdvanceDto)
        {
            var result = await _expenseAdvance.CreateCashAdvanceAsync(expenseAdvanceDto);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Returns all pending ExpenseAdvance requests in a paginated format.
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="companyId"></param>
        /// <returns>A list of all pending ExpenseAdvance requests in a paginated format.</returns>
        [HttpGet]
        [Route("pendingrequests/{companyId}")]
        public async Task<IActionResult> GetPendingExpenseAdvanceRequestsAsync([FromQuery] PagingDto searchQuery, int companyId)
        {
            var result = await _expenseAdvance.GetPendingRequestsAsync(searchQuery, companyId);
            return StatusCode(result.StatusCode, result);
        }

        
        /// <summary>
        /// The Endpoint returns all Approved Expense form
        /// </summary>
        /// <param name="paging"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllApprovedExpenseAdvanceForms([FromQuery] PagingDto paging, [FromQuery] string status)
        {
            var response = await _expenseAdvance.GetApprovedCashAdvanceExpenseFormsAsync(paging);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// The Endpoint to read Expense Advance form requestor
        /// </summary>
        /// <param name="paging"></param>
        /// <param name="userId">The Id of the user</param>
        /// <param name="cacNumber"></param>
        /// <returns></returns>
        [HttpGet("Requestor/cacNumber={cacNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReadRequestorExpenseAdvanceForms([FromQuery] PagingDto paging, int userId, string cacNumber)
        {
            var response = await _expenseAdvance.GetRequestorExpenseAdvanceFormsAsync(paging, userId, cacNumber);
            return StatusCode(response.StatusCode, response);
        }


        /// <summary>
        /// Edit ExpenseAdvance form details.
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="updateExpense"></param>
        /// <returns>A response message that shows details if the request was successful or failed</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPut]
        [Route("edit/{formId}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditExpenseAdvanceForm([FromBody]EditExpenseAdvanceDto updateExpense, string formId)
        {
            var result = await _expenseAdvance.EditExpenseAdvanceFormAsync(formId, updateExpense);
            return StatusCode(result.StatusCode, result);
        }
    }
}