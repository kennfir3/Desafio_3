using System.Reflection;
using CompanyManagement.Api.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace CompanyManagement.Tests;

public class AuthorizationTests
{
    [Fact]
    public void Controllers_and_delete_action_declare_expected_authorization()
    {
        // Arrange
        var clientes = typeof(ClientesController);
        var ordenes = typeof(OrdenesController);
        var reportes = typeof(ReportesController);
        var delete = clientes.GetMethod(nameof(ClientesController.Delete), BindingFlags.Instance | BindingFlags.Public)!;

        // Act
        var clientesAuthorize = clientes.GetCustomAttribute<AuthorizeAttribute>();
        var ordenesAuthorize = ordenes.GetCustomAttribute<AuthorizeAttribute>();
        var reportesAuthorize = reportes.GetCustomAttribute<AuthorizeAttribute>();
        var deleteAuthorize = delete.GetCustomAttribute<AuthorizeAttribute>();

        // Assert
        Assert.NotNull(clientesAuthorize);
        Assert.NotNull(ordenesAuthorize);
        Assert.Equal("Admin", reportesAuthorize?.Roles);
        Assert.Equal("Admin", deleteAuthorize?.Roles);
    }
}
