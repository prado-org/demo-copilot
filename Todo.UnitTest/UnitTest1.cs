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

    // criar um test para a funcao delete
    [TestMethod]
    public async Task DeleteTodo_ReturnsOk()
    {
        // Arrange
        _client.BaseAddress = new Uri(_url);

        // Act
        var response = await _client.DeleteAsync("/TodoApi/1");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    // criar um teste da funcao delete com um id inixistente.
    [TestMethod]
    public async Task DeleteTodo_ReturnsNotFound()
    {
        // Arrange
        _client.BaseAddress = new Uri(_url);

        // Act
        var response = await _client.DeleteAsync("/TodoApi/100");

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    // teste passado um id como string
    [TestMethod]
    public async Task DeleteTodo_ReturnsBadRequest()
    {
        // Arrange
        _client.BaseAddress = new Uri(_url);

        // Act
        var response = await _client.DeleteAsync("/TodoApi/abc");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }
}