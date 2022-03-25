using ExpenseWebApp.Core.Interfaces;
using ExpenseWebApp.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ExpenseWebApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseFormDetailsController : ControllerBase
    {
        private readonly IExpenseFormDetails _expenseFormDetails;
        private readonly IAttachmentService _attachmentService;

        public ExpenseFormDetailsController(IExpenseFormDetails expenseFormDetails,
            IAttachmentService attachmentService)
        {
            _expenseFormDetails = expenseFormDetails;
            _attachmentService = attachmentService;
        }

        /// <summary>
        /// This Endpoint delete expense form details
        /// </summary>
        /// <param name="expenseDetailId"></param>
        /// <returns></returns>
        [HttpDelete("delete-expenseDetail/expenseDetailId={expenseDetailId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteExpenseDetail(string expenseDetailId)
        {
            var response = await _expenseFormDetails.DeleteExpenseDetailAsync(expenseDetailId);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// This Endpoint saves a file path of an attachment
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost("save-file")]
        public async Task<ActionResult<string>> SaveFilePath([FromForm] FormDto form)
        {
            var result = await _attachmentService.SaveFilePathAsync(form);
            return Ok(result);
        }
    }
}
