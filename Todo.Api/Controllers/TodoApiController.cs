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
            
            for (int i = 1; i <= 1000; i++)
            {
                _todos.Add(new TodoModel { Id = i, Title = $"Todo {i}", Completed = i % 2 == 0 });
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
            _logger.LogInformation($"Retrieving Todo item with id {id}");
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                _logger.LogWarning($"Todo item with id {id} not found");
                return NotFound();
            }
            return Ok(todo);
        }

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
            _logger.LogInformation($"Todo item with id {id} deleted");
            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] TodoModel updatedTodo)
        {
            if (updatedTodo == null || updatedTodo.Id != id)
            {
                _logger.LogWarning("Invalid Todo item received");
                return BadRequest();
            }
        
            var existingTodo = _todos.FirstOrDefault(t => t.Id == id);
            if (existingTodo == null)
            {
                _logger.LogWarning($"Todo item with id {id} not found");
                return NotFound();
            }
        
            existingTodo.Title = updatedTodo.Title;
            existingTodo.Completed = updatedTodo.Completed;
            _logger.LogInformation($"Todo item with id {id} updated");
            return NoContent();
        }

        [HttpPost]
        public IActionResult Create([FromBody] TodoModel newTodo)
        {
            if (newTodo == null)
            {
                _logger.LogWarning("Invalid Todo item received");
                return BadRequest();
            }
        
            newTodo.Id = _todos.Max(t => t.Id) + 1;
            _todos.Add(newTodo);
            _logger.LogInformation($"Todo item with id {newTodo.Id} created");
            return CreatedAtAction(nameof(Get), new { id = newTodo.Id }, newTodo);
        }


    }
}