using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Todo.Test.Model;

namespace Todo.UnitTest;

[TestClass]
public class UnitTest1
{
    private HttpClient _client;
    private static string _url = "http://localhost:5071/";

    [TestInitialize]
    public void Initialize()
    {
        _client = new HttpClient();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client.Dispose();
    }

    [TestMethod]
    public async Task GetTodo_ReturnsOk()
    {
        // Arrange
        _client.BaseAddress = new Uri(_url);

        // Act
        var response = await _client.GetAsync("/TodoApi");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetTodoById_ReturnsOk()
    {
        // Arrange
        var id = 1; // replace with the actual id you want to test
        _client.BaseAddress = new Uri(_url);

        // Act
        var response = await _client.GetAsync($"/TodoApi/{id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task GetTodoById_ReturnsNotFound()
    {
        // Arrange
        var id = 100; // replace with the actual id you want to test
        _client.BaseAddress = new Uri(_url);

        // Act
        var response = await _client.GetAsync($"/TodoApi/{id}");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }


}