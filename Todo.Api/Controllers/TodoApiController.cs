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

            for (int i = 1; i <= 100; i++)
            {
                _todos.Add(new TodoModel
                {
                    Id = i,
                    Title = $"Todo {i}",
                    Completed = i % 2 == 1 // True for odd numbers, false for even numbers
                });
            }
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
            var todo = FindTodoById(id);
            if (todo == null)
            {
                _logger.LogWarning($"Todo item with id {id} not found");
                return NotFound();
            }
            _logger.LogInformation($"Retrieving Todo item with id {id}");
            return Ok(todo);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TodoModel todo)
        {
            if (todo == null)
            {
                _logger.LogWarning("Invalid Todo item");
                return BadRequest();
            }

            if (!IsValidTitle(todo.Title))
            {
                _logger.LogWarning("Invalid Title");
                return BadRequest();
            }

            todo.Id = _todos.Max(t => t.Id) + 1;
            _todos.Add(todo);
            _logger.LogInformation($"Todo item with id {todo.Id} created");
            return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] TodoModel todo)
        {
            if (todo == null)
            {
                _logger.LogWarning("Invalid Todo item");
                return BadRequest();
            }

            var existingTodo = FindTodoById(id);
            if (existingTodo == null)
            {
                _logger.LogWarning($"Todo item with id {id} not found");
                return NotFound();
            }

            if (!IsValidTitle(todo.Title))
            {
                _logger.LogWarning("Invalid Title");
                return BadRequest();
            }

            existingTodo.Title = todo.Title;
            existingTodo.Completed = todo.Completed;
            _logger.LogInformation($"Todo item with id {id} updated");
            return NoContent();
        }

        private bool IsValidTitle(string title)
        {
            return !string.IsNullOrEmpty(title) && System.Text.RegularExpressions.Regex.IsMatch(title, @"^[a-zA-Z0-9\s]*$");
        }

        private TodoModel FindTodoById(int id)
        {
            return _todos.FirstOrDefault(t => t.Id == id);
        }
    }
}