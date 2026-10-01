using System.ComponentModel.DataAnnotations;
namespace CompanyManagement.Api.DTOs;
public record ClienteDto(int Id,string Nombre,string Email,DateTime FechaRegistro);
public class ClienteCreateDto { [Required,MaxLength(150)] public string Nombre {get;set;}=""; [Required,EmailAddress,MaxLength(256)] public string Email {get;set;}=""; public DateTime? FechaRegistro {get;set;} }
public class ClienteUpdateDto: ClienteCreateDto { }
public record OrdenDto(int Id,int ClienteId,DateTime FechaOrden,decimal MontoTotal);
public class OrdenCreateDto { [Range(1,int.MaxValue)] public int ClienteId {get;set;} [Required] public DateTime? FechaOrden {get;set;} [Range(typeof(decimal),"0.01","9999999999999999.99")] public decimal MontoTotal {get;set;} }
public class RegisterDto { [Required,EmailAddress] public string Email {get;set;}=""; [Required,MinLength(6)] public string Password {get;set;}=""; }
public class LoginDto { [Required,EmailAddress] public string Email {get;set;}=""; [Required] public string Password {get;set;}=""; }
