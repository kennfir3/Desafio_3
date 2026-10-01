using CompanyManagement.Api.Controllers;
using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManagement.Tests;

public class ClientesControllerTests
{
    [Fact]
    public async Task Get_existing_client_returns_OkObjectResult()
    {
        // Arrange
        using var db = TestSupport.CreateDb();
        db.Clientes.Add(new Cliente { Id = 1, Nombre = "Ana López", Email = "ana@company.com", FechaRegistro = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var controller = new ClientesController(TestSupport.CreateClientes(db));

        // Act
        var result = await controller.Get(1);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ClienteDto>(ok.Value);
    }

    [Fact]
    public async Task Get_missing_client_returns_NotFoundResult()
    {
        // Arrange
        using var db = TestSupport.CreateDb();
        var controller = new ClientesController(TestSupport.CreateClientes(db));

        // Act
        var result = await controller.Get(404);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Post_valid_client_returns_CreatedAtActionResult()
    {
        // Arrange
        using var db = TestSupport.CreateDb();
        var controller = new ClientesController(TestSupport.CreateClientes(db));

        // Act
        var result = await controller.Post(new ClienteCreateDto { Nombre = "María Díaz", Email = "maria@company.com" });

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(ClientesController.Get), created.ActionName);
        Assert.IsType<ClienteDto>(created.Value);
    }

    [Fact]
    public async Task Post_duplicate_email_returns_ConflictObjectResult()
    {
        // Arrange
        using var db = TestSupport.CreateDb();
        db.Clientes.Add(new Cliente { Nombre = "Ana", Email = "duplicate@company.com", FechaRegistro = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var controller = new ClientesController(TestSupport.CreateClientes(db));

        // Act
        var result = await controller.Post(new ClienteCreateDto { Nombre = "Otra", Email = "duplicate@company.com" });

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }
}
