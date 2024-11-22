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

        [HttpPost]
        public IActionResult Post([FromBody] TodoModel todo)
        {
            _logger.LogInformation($"Creating new Todo item with title {todo.Title}");
            todo.Id = _todos.Max(t => t.Id) + 1;

            // o campo title não pode conter mais de 50 caracteres e não pode coter caracteres especiais
            if (todo.Title.Length > 50 || !todo.Title.All(char.IsLetterOrDigit))
            {
                _logger.LogWarning("Title must have a maximum of 50 characters and cannot contain special characters");
                return BadRequest();
            }

            _todos.Add(todo);
            return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] TodoModel todo)
        {
            _logger.LogInformation($"Updating Todo item with id {id}");
            var existingTodo = _todos.FirstOrDefault(t => t.Id == id);
            if (existingTodo == null)
            {
                _logger.LogWarning($"Todo item with id {id} not found");
                return NotFound();
            }

            // o campo title não pode conter mais de 50 caracteres e não pode coter caracteres especiais
            if (existingTodo.Title.Length > 50 || !existingTodo.Title.All(char.IsLetterOrDigit))
            {
                _logger.LogWarning("Title must have a maximum of 50 characters and cannot contain special characters");
                return BadRequest();
            }

            existingTodo.Title = todo.Title;
            existingTodo.Completed = todo.Completed;
            return NoContent();
        }
    }
}