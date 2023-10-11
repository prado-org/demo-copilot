using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Todo.Client.Model;

namespace Todo.Client;

class Program
{
    static async Task Main(string[] args)
    {
        using var client = new HttpClient();
        
        var response = await client.GetFromJsonAsync<List<TodoModel>>("http://localhost:5071/TodoApi");
        if (response != null)
        {
            foreach (var todo in response)
            {
                Console.WriteLine($"Id: {todo.Id}, Title: {todo.Title}, Completed: {todo.Completed}");
            }
        }
    }
}