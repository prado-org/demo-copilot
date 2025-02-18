using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Model;

namespace Todo.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TodoApiController : ControllerBase
    {
        private readonly ILogger<TodoApiController> _logger;
        private readonly List<TodoModel> _todos;

        public TodoApiController(ILogger<TodoApiController> logger)
        {
            _logger = logger;
            _todos = new List<TodoModel>
            {
                new TodoModel { Id = 1, Title = "Todo 1", Completed = false },
                new TodoModel { Id = 2, Title = "Todo 2", Completed = false },
                new TodoModel { Id = 3, Title = "Todo 3", Completed = false },
                new TodoModel { Id = 4, Title = "Todo 4", Completed = false },
                new TodoModel { Id = 5, Title = "Todo 5", Completed = false },
            };
        }

        /// <summary>
        /// Retrieves all Todo items.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of Todo items.</returns>
        /// <response code="200">Returns the list of Todo items.</response>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Retrieving all Todo items");
            return Ok(_todos);
        }
    }
}