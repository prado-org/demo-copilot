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

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Retrieving all Todo items");
            return Ok(_todos);
        }

        /// <summary>
        /// Retrieves a Todo item by its ID.
        /// </summary>
        /// <param name="id">The ID of the Todo item to retrieve.</param>
        /// <returns>The retrieved Todo item if found, otherwise returns a NotFound result.</returns>
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            _logger.LogInformation($"Retrieving Todo item with id {id}");
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                _logger.LogWarning($"Todo item with id {id} not found");
                return NotFound();
            }
            return Ok(todo);
        }

        // criar um metodo para deletar um TodoItem
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation($"Deleting Todo item with id {id}");
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                _logger.LogWarning($"Todo item with id {id} not found");
                return NotFound();
            }
            _todos.Remove(todo);
            return Ok();
        }

        





    }
}