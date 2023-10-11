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
            _todos = new List<TodoModel>();
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Retrieving all Todo items");
            var todos = Enumerable.Range(1, 100).Select(i => new TodoModel
            {
                Id = i,
                Title = $"Todo {i}",
                Completed = true
            }).ToList();
            return Ok(todos);
        }
    }
}