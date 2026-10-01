# Sistema de Gestión Empresarial con .NET

API de clientes y órdenes del Desafío 3, opción 2. Usa .NET 8, ASP.NET Core Identity/JWT, EF Core SQL Server, Redis, SSRS, xUnit y Moq.

## Estructura

`src/CompanyManagement.Api` contiene API, entidades, DTO, repositorios y servicios; `tests` pruebas; `database` scripts; `reports` RDL y despliegue; `postman` colección; `docs/enunciado` el PDF original; `docs/evidencias` capturas.

## Requisitos e instalación

Verifique: `dotnet --list-sdks`, `sqlcmd -S localhost -E -Q "select 1"`, `docker exec redis redis-cli ping` y abra http://kenn/Reports.

Ejecute, desde la raíz:

```powershell
dotnet restore
sqlcmd -S localhost -E -i database/01_create_database.sql
dotnet ef database update --project src/CompanyManagement.Api --startup-project src/CompanyManagement.Api
sqlcmd -S localhost -E -i database/03_seed.sql
dotnet run --project src/CompanyManagement.Api --urls http://localhost:5000
dotnet test
```

La conexión SQL, Redis (`localhost:6379`), JWT y SSRS están en `appsettings.json`. La clave JWT marcada DEVELOPMENT-ONLY se debe cambiar mediante `dotnet user-secrets set "Jwt:Key" "<secreto-largo>" --project src/CompanyManagement.Api` o variable `Jwt__Key`. Admin de desarrollo: `admin@company.com` / `Admin123!`; cambiar antes de cualquier uso no local.

## Endpoints

|Método|Ruta|Rol|Descripción|
|---|---|---|---|
|POST|/api/auth/register|Público|Registra User|
|POST|/api/auth/login|Público|Devuelve JWT|
|GET/POST|/api/clientes|Autenticado|Lista/crea|
|GET/PUT|/api/clientes/{id}|Autenticado|Consulta/actualiza|
|DELETE|/api/clientes/{id}|Admin|Elimina sin órdenes|
|GET/POST|/api/ordenes|Autenticado|Lista/crea|
|GET|/api/ordenes/{id}|Autenticado|Consulta|
|GET|/api/ordenes/cliente/{clienteId}|Autenticado|Filtra por cliente|
|GET|/api/reportes/clientes-activos, ingresos-clientes, clientes-inactivos|Admin|URL SSRS|

Ejemplo: `curl -X POST http://localhost:5000/api/auth/login -H "Content-Type: application/json" -d '{\"email\":\"admin@company.com\",\"password\":\"Admin123!\"}'`. Importe los dos JSON de `postman`, haga Register, Login y copie `token` al entorno.

## Caché Redis

Claves `clientes:all`, `clientes:{id}` y `ordenes:all`, con TTL de 10 minutos. POST/PUT/DELETE de clientes y POST de orden invalidan las colecciones afectadas. Ante indisponibilidad Redis, el servicio registra warning y consulta SQL.

## SSRS

Ejecute `sqlcmd -S localhost -E -i database/04_ssrs_permissions.sql`. Configure en Portal `/DataSources/CompanyManagementDS`: `Data Source=localhost;Initial Catalog=CompanyManagement`, credenciales Windows/cuenta de servicio. Ejecute `powershell -ExecutionPolicy Bypass -File reports/deploy-reports.ps1`; si REST no acepta el payload de su SSRS, Portal > Cargar cada RDL a `/Desafio3` y enlace el origen. URLs: `http://kenn/ReportServer?/Desafio3/ClientesActivos&rs:Command=Render` y equivalentes para los otros dos.

## Decisiones y supuestos

- Se usa `DeleteBehavior.Restrict`: no se pierde historial de órdenes; DELETE devuelve 409 si hay órdenes.
- Todos los CRUD protegidos requieren JWT; sólo DELETE y reportes requieren Admin.
- Fechas se guardan UTC. La siembra aporta 15 clientes y 40 órdenes, incluyendo inactivos recientes.

## Subir a GitHub

`git remote add origin https://github.com/kennfir3/Desafio_3.git` (si falta), `git add .; git commit -m "feat: desafio 3"; git push -u origin main`. Si pide autenticación: instale/use Git Credential Manager, ejecute `git credential-manager configure`, después `git push -u origin main` e inicie sesión.

## PASOS MANUALES PARA TERMINAR EL PROYECTO AL 100%

1. [ ] Verificar SQL `sqlcmd -S localhost -E -Q "select 1"`, Redis `docker exec redis redis-cli ping`, SSRS http://kenn/Reports y arrancar API con el comando anterior.
2. [ ] Ejecutar permisos SSRS y `reports/deploy-reports.ps1`; si falla, cargar manualmente los tres RDL en `/Desafio3`, enlazar `CompanyManagementDS`, abrir sus tres URLs.
3. [ ] Guardar capturas: `01-postman-auth-crud-reportes.png` (incluye 403 User/200 Admin), `02-dotnet-test.png`, `03-ssrs-clientes-activos.png`, `04-ssrs-ingresos.png`, `05-ssrs-inactivos.png`, `06-redis.png` en `docs/evidencias/`.
4. [ ] En Postman importe colección y entorno; ejecute Register, Login, Clientes, Reportes en ese orden y copie token.
5. [ ] Video 5–8 min: estructura, JWT/roles, CRUD, dos GET para hit/miss, `redis-cli keys '*'`/`ttl`, invalidación, SSRS, pruebas y README.
6. [ ] `git add docs/evidencias README.md; git commit -m "docs: evidencias demo"; git push` (video: añadir enlace en README).
7. [ ] Revisar rúbrica: Core 25, seguridad 20, Redis 15, SSRS 15, pruebas 15, documentación 10; confirmar compilación y que no hay código plagiado, entrega tardía ni errores críticos.

### Preguntas probables en la defensa

1. JWT: token firmado con claims de rol. 2. Identity: hash de contraseñas y usuarios. 3. Repository: desacopla persistencia. 4. Redis: reduce consultas SQL. 5. Invalidación: evita datos obsoletos tras escritura. 6. Restrict: protege órdenes históricas. 7. SSRS: RDL consulta SQL y se publica en ReportServer. 8. InMemory: aisla pruebas sin SQL. 9. DTO: valida y no expone entidades. 10. ProblemDetails: respuestas de error consistentes.
