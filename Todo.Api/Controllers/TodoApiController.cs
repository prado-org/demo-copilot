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
                _todos.Add(new TodoModel { Id = i, Title = $"Todo {i}", Completed = i % 2 == 0 });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }
            return Ok(todo);
        }

        /// <summary>
        /// Retrieves a Todo item by its ID.
        /// </summary>
        /// <param name="id">The ID of the Todo item to retrieve.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains the Todo item if found, 
        /// or a NotFound result if the item does not exist.
        /// </returns>
        [HttpPost]
        public IActionResult Post([FromBody] TodoModel todo)
        {
            _logger.LogInformation($"Creating new Todo item with title {todo.Title}");
        
            if (!IsValidTitle(todo.Title))
            {
                _logger.LogWarning("Invalid Todo item");
                return BadRequest();
            }
        
            todo.Id = _todos.Max(t => t.Id) + 1;
            _todos.Add(todo);
            return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
        }
        
        /// <summary>
        /// Atualiza um item Todo existente pelo seu ID.
        /// </summary>
        /// <param name="id">O ID do item Todo a ser atualizado.</param>
        /// <param name="todo">O item Todo atualizado.</param>
        /// <returns>
        /// Um <see cref="IActionResult"/> que indica o resultado da operação:
        /// - <see cref="NoContentResult"/> se a atualização foi bem-sucedida.
        /// - <see cref="NotFoundResult"/> se o item Todo com o ID especificado não foi encontrado.
        /// - <see cref="BadRequestResult"/> se o item Todo fornecido for inválido.
        /// </returns>
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] TodoModel todo)
        {
            // Loga uma mensagem informando que um item Todo está sendo atualizado
            _logger.LogInformation($"Atualizando item Todo com id {id}");
        
            // Procura o item Todo existente pelo ID
            var existingTodo = _todos.FirstOrDefault(t => t.Id == id);
            if (existingTodo == null)
            {
                // Loga uma mensagem de aviso se o item Todo não for encontrado
                _logger.LogWarning($"Item Todo com id {id} não encontrado");
                // Retorna um resultado NotFound se o item não for encontrado
                return NotFound();
            }
        
            // Verifica se o título do item Todo é válido
            if (!IsValidTitle(todo.Title))
            {
                // Loga uma mensagem de aviso se o item Todo for inválido
                _logger.LogWarning("Item Todo inválido");
                // Retorna um resultado BadRequest se o item for inválido
                return BadRequest();
            }
        
            // Atualiza o título e o status de conclusão do item Todo existente
            existingTodo.Title = todo.Title;
            existingTodo.Completed = todo.Completed;
            // Retorna um resultado NoContent indicando que a atualização foi bem-sucedida
            return NoContent();
        }
        
        private bool IsValidTitle(string title)
        {
            return !string.IsNullOrEmpty(title) && System.Text.RegularExpressions.Regex.IsMatch(title, @"^[a-zA-Z0-9\s]*$");
        }
    }
}