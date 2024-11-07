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
        public IActionResult GetById(int id)
        {
            _logger.LogInformation($"Retrieving Todo item with ID: {id}");
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }
            return Ok(todo);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TodoModel todo)
        {
            _logger.LogInformation("Adding a new Todo item");

            if (todo == null || !IsValidTitle(todo.Title))
            {
                return BadRequest();
            }

            _todos.Add(todo);
            return CreatedAtRoute("GetTodo", new { id = todo.Id }, todo);
        }

        /// <summary>
        /// Atualiza um item de tarefa (Todo item) existente.
        /// </summary>
        /// <param name="id">O identificador do item de tarefa a ser atualizado.</param>
        /// <param name="todo">O modelo de dados do item de tarefa atualizado.</param>
        /// <returns>Retorna um <see cref="IActionResult"/>. 
        /// Se o item de tarefa for atualizado com sucesso, retorna um <see cref="NoContentResult"/>. 
        /// Se o item de tarefa não for encontrado, retorna um <see cref="NotFoundResult"/>. 
        /// Se o modelo de dados for inválido, retorna um <see cref="BadRequestResult"/>.</returns>
        /// <response code="204">Se o item de tarefa for atualizado com sucesso.</response>
        /// <response code="404">Se o item de tarefa não for encontrado.</response>
        /// <response code="400">Se o modelo de dados for inválido.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Put(int id, [FromBody] TodoModel todo)
        {
            // Loga a informação de que um item de tarefa está sendo atualizado
            _logger.LogInformation("Updating a Todo item");

            // Verifica se o modelo de dados é nulo, se o ID fornecido não corresponde ao ID do modelo, ou se o título é inválido
            if (todo == null || id != todo.Id || !IsValidTitle(todo.Title))
            {
                // Retorna um resultado de solicitação inválida (400 Bad Request)
                return BadRequest();
            }

            // Procura o item de tarefa existente na lista pelo ID
            var existingTodo = _todos.FirstOrDefault(t => t.Id == id);
            if (existingTodo == null)
            {
                // Se o item de tarefa não for encontrado, retorna um resultado de não encontrado (404 Not Found)
                return NotFound();
            }

            // Atualiza o título e o status de conclusão do item de tarefa existente
            existingTodo.Title = todo.Title;
            existingTodo.Completed = todo.Completed;

            // Retorna um resultado de sucesso sem conteúdo (204 No Content)
            return NoContent();
        }

        /// <summary>
        /// Deletes a Todo item by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the Todo item to delete.</param>
        /// <returns>
        /// Returns a <see cref="IActionResult"/>. 
        /// If the Todo item is not found, returns a <see cref="NotFoundResult"/>. 
        /// If the Todo item is successfully deleted, returns a <see cref="NoContentResult"/>.
        /// </returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("Deleting a Todo item");

            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo == null)
            {
                return NotFound();
            }

            _todos.Remove(todo);
            return NoContent();
        }

        private bool IsValidTitle(string title)
        {
            return !string.IsNullOrEmpty(title) && !System.Text.RegularExpressions.Regex.IsMatch(title, @"[^\w\s]");
        }
    }
}