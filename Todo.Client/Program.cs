using System;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Todo.Client.Model;

namespace Todo.Client;

class Program
{
    static async Task Main(string[] args)
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Starting Todo Client");

        Console.WriteLine("Hello Todo Client");
        using var client = new HttpClient();
        
        var response = await client.GetFromJsonAsync<List<TodoModel>>("https://localhost:7086/TodoApi");
        if (response != null)
        {
            foreach (var todo in response)
            {
                Console.WriteLine($"Id: {todo.Id}, Title: {todo.Title}, Completed: {todo.Completed}");
            }
        }
        else
        {
            Console.WriteLine("No response from server");
        }

        Console.WriteLine("Press any key to exit");
        Console.Read();
    }

    public double CalculateAverage(int[] numbers)
    {
        int sum = 0;
        for (int i = 0; i < numbers.Length; i++)
        {
            sum += numbers[i];
        }
        return sum / numbers.Length;
    }
}