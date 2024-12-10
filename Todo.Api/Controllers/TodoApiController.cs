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
                _todos.Add(new TodoModel
                {
                    Id = i,
                    Title = $"Todo {i}",
                    Completed = i % 2 == 0 // Alterna entre true e false
                });
            }
        }

        /// <summary>
        /// Retrieves all Todo items.
        /// </summary>
        /// <returns>An <see cref="IActionResult"/> containing the list of Todo items.</returns>
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Retrieving all Todo items");
            return Ok(_todos);
        }

        /// <summary>
        /// Retrieves a Todo item by its ID.
        /// </summary>
        /// <param name="id">The ID of the Todo item.</param>
        /// <returns>An <see cref="IActionResult"/> containing the Todo item.</returns>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation($"Retrieving Todo item with ID: {id}");
            var todoItem = _todos.FirstOrDefault(t => t.Id == id);
            if (todoItem == null)
            {
                return NotFound();
            }
            return Ok(todoItem);
        }

        /// <summary>
    /// Creates a new Todo item.
    /// </summary>
    /// <param name="todoItem">The Todo item to create.</param>
    /// <returns>An <see cref="IActionResult"/> containing the created Todo item.</returns>
    [HttpPost]
    public IActionResult Create([FromBody] TodoModel todoItem)
    {
        if (todoItem == null)
        {
            return BadRequest();
        }

        if (!IsValidTitle(todoItem.Title))
        {
            return BadRequest("Title can't contain special characters");
        }

        todoItem.Id = _todos.Max(t => t.Id) + 1; // Assign a new ID
        _todos.Add(todoItem);

        _logger.LogInformation($"Created new Todo item with ID: {todoItem.Id}");
        return CreatedAtAction(nameof(GetById), new { id = todoItem.Id }, todoItem);
    }

    /// <summary>
    /// Updates an existing Todo item.
    /// </summary>
    /// <param name="id">The ID of the Todo item to update.</param>
    /// <param name="todoItem">The updated Todo item.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] TodoModel todoItem)
    {
        if (todoItem == null)
        {
            return BadRequest();
        }

        var existingTodoItem = _todos.FirstOrDefault(t => t.Id == id);
        if (existingTodoItem == null)
        {
            return NotFound();
        }

        if (!IsValidTitle(todoItem.Title))
        {
            return BadRequest("Title can't contain special characters");
        }

        existingTodoItem.Title = todoItem.Title;
        existingTodoItem.Completed = todoItem.Completed;

        _logger.LogInformation($"Updated Todo item with ID: {id}");
        return Ok(existingTodoItem);
    }

    private bool IsValidTitle(string title)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(title, @"^[a-zA-Z0-9\s]*$");
    }






    }
}