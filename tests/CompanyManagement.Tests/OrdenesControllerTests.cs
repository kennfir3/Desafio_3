using CompanyManagement.Api.Controllers;
using CompanyManagement.Api.DTOs;
using CompanyManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManagement.Tests;

public class OrdenesControllerTests
{
    [Fact]
    public async Task Post_valid_order_returns_CreatedAtActionResult()
    {
        // Arrange
        using var db = TestSupport.CreateDb();
        db.Clientes.Add(new Cliente { Id = 1, Nombre = "Ana", Email = "ana@company.com", FechaRegistro = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var controller = new OrdenesController(TestSupport.CreateOrdenes(db));

        // Act
        var result = await controller.Post(new OrdenCreateDto { ClienteId = 1, FechaOrden = DateTime.UtcNow, MontoTotal = 125.50m });

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(OrdenesController.Get), created.ActionName);
        Assert.IsType<OrdenDto>(created.Value);
    }

    [Fact]
    public async Task Post_order_for_missing_client_returns_NotFoundObjectResult()
    {
        // Arrange
        using var db = TestSupport.CreateDb();
        var controller = new OrdenesController(TestSupport.CreateOrdenes(db));

        // Act
        var result = await controller.Post(new OrdenCreateDto { ClienteId = 999, FechaOrden = DateTime.UtcNow, MontoTotal = 25m });

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Get_orders_by_client_returns_filtered_OkObjectResult()
    {
        // Arrange
        using var db = TestSupport.CreateDb();
        db.Ordenes.AddRange(
            new Orden { ClienteId = 1, FechaOrden = DateTime.UtcNow, MontoTotal = 10m },
            new Orden { ClienteId = 2, FechaOrden = DateTime.UtcNow, MontoTotal = 20m });
        await db.SaveChangesAsync();
        var controller = new OrdenesController(TestSupport.CreateOrdenes(db));

        // Act
        var result = await controller.ByClient(1);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var orders = Assert.IsAssignableFrom<IEnumerable<OrdenDto>>(ok.Value);
        Assert.Single(orders);
        Assert.All(orders, order => Assert.Equal(1, order.ClienteId));
    }
}
