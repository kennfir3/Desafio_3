namespace CompanyManagement.Api.Models;
public class Cliente { public int Id { get; set; } public required string Nombre { get; set; } public required string Email { get; set; } public DateTime FechaRegistro { get; set; } public List<Orden> Ordenes { get; set; } = []; }
