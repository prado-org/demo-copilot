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
                    Completed = i % 2 == 0 // True para IDs pares, False para IDs ímpares
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
            _logger.LogInformation($"Retrieving Todo item with id {id}");
            var todo = _todos.FirstOrDefault(t => t.Id == id);

            //verifique se o campo id é maior do que 10, sim sim salvar o item, caso contrario retornar um exception
            if (id > 10)
            {
                throw new Exception("Id não pode ser maior que 10");
            }

            // verifique o campo title contem caracteres especais usando regex, se sim salvar o item, caso contrario retornar um exception
            if (System.Text.RegularExpressions.Regex.IsMatch(todo.Title, "[^a-zA-Z0-9]"))
            {
                throw new Exception("Title não pode conter caracteres especiais");
            }
            
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
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                _logger.LogWarning($"Todo item with id {id} not found");
                return NotFound();
            }
            _todos.Remove(todo);
            return NoContent();
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] TodoModel todo)
        {
            if (id > 10)
            {
                throw new Exception("Id não pode ser maior que 10");
            }
        
            if (!System.Text.RegularExpressions.Regex.IsMatch(todo.Title, "[^a-zA-Z0-9]"))
            {
                throw new Exception("Title não pode conter caracteres especiais");
            }
        
            var existingTodo = _todos.FirstOrDefault(t => t.Id == id);
            if (existingTodo == null)
            {
                _logger.LogWarning($"Todo item with id {id} not found");
                return NotFound();
            }
        
            existingTodo.Title = todo.Title;
            existingTodo.Completed = todo.Completed;
            return NoContent();
        }




    }
}