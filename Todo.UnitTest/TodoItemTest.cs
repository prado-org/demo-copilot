using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Todo.Api.Controllers;
using Todo.Api.Model;

namespace Todo.Api.Tests
{
    [TestClass]
    public class TodoApiControllerTests
    {
        private Mock<ILogger<TodoApiController>> _mockLogger;
        private TodoApiController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<TodoApiController>>();
            _controller = new TodoApiController(_mockLogger.Object);
        }

        [TestMethod]
        public void Get_ReturnsAllTodoItems()
        {
            // Act
            var result = _controller.Get();

            // Assert
            var okResult = Assert.IsInstanceOfType(result, typeof(OkObjectResult)) as OkObjectResult;
            var items = Assert.IsInstanceOfType(okResult.Value, typeof(List<TodoModel>)) as List<TodoModel>;
            Assert.AreEqual(1000, items.Count);
        }

        [TestMethod]
        public void GetById_ReturnsTodoItem_WhenItemExists()
        {
            // Arrange
            var id = 1;

            // Act
            var result = _controller.GetById(id);

            // Assert
            var okResult = Assert.IsInstanceOfType(result, typeof(OkObjectResult)) as OkObjectResult;
            var item = Assert.IsInstanceOfType(okResult.Value, typeof(TodoModel)) as TodoModel;
            Assert.AreEqual(id, item.Id);
        }

        [TestMethod]
        public void GetById_ReturnsNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var id = 1001;

            // Act
            var result = _controller.GetById(id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void Create_ReturnsBadRequest_WhenTodoItemIsNull()
        {
            // Act
            var result = _controller.Create(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public void Create_ReturnsBadRequest_WhenTitleIsInvalid()
        {
            // Arrange
            var todoItem = new TodoModel { Title = "Invalid@Title" };

            // Act
            var result = _controller.Create(todoItem);

            // Assert
            var badRequestResult = Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult)) as BadRequestObjectResult;
            Assert.AreEqual("Title can't contain special characters", badRequestResult.Value);
        }

        [TestMethod]
        public void Create_ReturnsCreatedAtAction_WhenTodoItemIsValid()
        {
            // Arrange
            var todoItem = new TodoModel { Title = "Valid Title" };

            // Act
            var result = _controller.Create(todoItem);

            // Assert
            var createdAtActionResult = Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult)) as CreatedAtActionResult;
            var item = Assert.IsInstanceOfType(createdAtActionResult.Value, typeof(TodoModel)) as TodoModel;
            Assert.AreEqual(todoItem.Title, item.Title);
        }

        [TestMethod]
        public void Update_ReturnsBadRequest_WhenTodoItemIsNull()
        {
            // Act
            var result = _controller.Update(1, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        