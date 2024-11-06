using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using Todo.Api.Controllers;

namespace Todo.Api.Tests.Controllers
{
    [TestClass]
    public class WeatherForecastControllerTest
    {
        private Mock<ILogger<WeatherForecastController>> _loggerMock;
        private WeatherForecastController _controller;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<WeatherForecastController>>();
            _controller = new WeatherForecastController(_loggerMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "A string de conexão com o banco de dados não foi encontrada.")]
        public void WeatherForecastById_ConnectionStringNotFound_ThrowsException()
        {
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", null);
            _controller.GetType().GetMethod("WeatherForecastById", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_controller, new object[] { 1 });
        }

        [TestMethod]
        public void WeatherForecastById_ValidId_ReturnsWeatherForecast()
        {
            var connectionString = "your_connection_string";
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", connectionString);

            var mockReader = new Mock<IDataReader>();
            mockReader.SetupSequence(r => r.Read())
                .Returns(true)
                .Returns(false);
            mockReader.Setup(r => r.GetDateTime(0)).Returns(DateTime.Now);
            mockReader.Setup(r => r.GetString(1)).Returns("Sunny");
            mockReader.Setup(r => r.GetInt32(2)).Returns(25);

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(c => c.ExecuteReader()).Returns(mockReader.Object);

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            SqlConnectionFactory.SetConnection(mockConnection.Object);

            var result = _controller.GetType().GetMethod("WeatherForecastById", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_controller, new object[] { 1 }) as WeatherForecast;

            Assert.IsNotNull(result);
            Assert.AreEqual("Sunny", result.Summary);
            Assert.AreEqual(25, result.TemperatureC);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "An error occurred while retrieving the weather forecast.")]
        public void WeatherForecastById_SqlException_ThrowsException()
        {
            var connectionString = "your_connection_string";
            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", connectionString);

            var mockCommand = new Mock<IDbCommand>();
            mockCommand.Setup(c => c.ExecuteReader()).Throws(new SqlException());

            var mockConnection = new Mock<IDbConnection>();
            mockConnection.Setup(c => c.CreateCommand()).Returns(mockCommand.Object);

            SqlConnectionFactory.SetConnection(mockConnection.Object);

            _controller.GetType().GetMethod("WeatherForecastById", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(_controller, new object[] { 1 });
        }
    }

    public static class SqlConnectionFactory
    {
        private static IDbConnection _connection;

        public static void SetConnection(IDbConnection connection)
        {
            _connection = connection;
        }

        public static IDbConnection CreateConnection()
        {
            return _connection;
        }
    }
}