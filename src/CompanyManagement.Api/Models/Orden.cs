namespace CompanyManagement.Api.Models;
public class Orden { public int Id { get; set; } public int ClienteId { get; set; } public DateTime FechaOrden { get; set; } public decimal MontoTotal { get; set; } public Cliente? Cliente { get; set; } }
