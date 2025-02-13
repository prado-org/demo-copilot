using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        /// Recupera todos os itens de tarefas.
        /// </summary>
        /// <returns>Lista de itens de tarefas.</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Retrieving all Todo items");
            return Ok(_todos);
        }

        /// <summary>
        /// Recupera um item de tarefa pelo ID.
        /// </summary>
        /// <param name="id">ID do item de tarefa a ser recuperado.</param>
        /// <returns>Item de tarefa.</returns>
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

        /// <summary>
        /// Cria um novo item de tarefa.
        /// </summary>
        /// <param name="todo">Dados do novo item de tarefa.</param>
        /// <returns>Item de tarefa criado.</returns>
        [HttpPost]
        public IActionResult Post([FromBody] TodoModel todo)
        {
            _logger.LogInformation($"Creating new Todo item with title {todo.Title}");
            todo.Id = _todos.Max(t => t.Id) + 1;
            _todos.Add(todo);
            return CreatedAtAction(nameof(Get), new { id = todo.Id }, todo);
        }

        /// <summary>
        /// Deleta um item de tarefa pelo ID.
        /// </summary>
        /// <param name="id">ID do item de tarefa a ser deletado.</param>
        /// <returns>Resultado da ação.</returns>
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
            return NoContent();
        }

        /// <summary>
        /// Atualiza um item de tarefa pelo ID.
        /// </summary>
        /// <param name="id">ID do item de tarefa a ser atualizado.</param>
        /// <param name="todo">Dados do item de tarefa a ser atualizado.</param>
        /// <returns>Resultado da ação.</returns>
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

            if (Regex.IsMatch(todo.Title, @"[^a-zA-Z0-9\s]"))
            {
                _logger.LogWarning("Title contains special characters");
                return BadRequest("Title contains special characters");
            }

            existingTodo.Title = todo.Title;
            existingTodo.Completed = todo.Completed;
            return NoContent();
        }
    }
}