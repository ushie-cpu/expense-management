using ExpenseWebApp.Core.Interfaces;
using ExpenseWebApp.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseWebApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseCategoryController : ControllerBase
    {
        private readonly ILogger<ExpenseCategoryController> _logger;
        private readonly IExpenseCategoryService _expenseCategoryService;

        public ExpenseCategoryController(ILogger<ExpenseCategoryController> logger, IExpenseCategoryService expenseCategoryService)
        {
            _logger = logger;
            _expenseCategoryService = expenseCategoryService;
        }

        /// <summary>
        /// The Endpoint returns all Expense categories
        /// </summary>
        /// <param name="paging"></param>
        /// <returns></returns>
        [HttpGet("ExpenseCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PagingDto paging)
        {
            var response = await _expenseCategoryService.GetAllExpenseCategoriesAsync(paging);
            return StatusCode(response.StatusCode, response);
        }
    }
}
