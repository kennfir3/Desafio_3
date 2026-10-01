# Company Management API

API REST para gestionar clientes y órdenes con autenticación, autorización por roles, caché Redis y reportes paginados en SQL Server Reporting Services (SSRS).

> Proyecto académico terminado para el Desafío 3, opción 2. La solución y las instrucciones de ejecución están en este repositorio.

## Funcionalidades

- Registro e inicio de sesión con ASP.NET Core Identity y tokens JWT.
- Roles `User` y `Admin`; acceso restringido a los reportes para administradores.
- CRUD de clientes y gestión de órdenes, con validaciones y respuestas HTTP apropiadas.
- Persistencia en SQL Server mediante Entity Framework Core y migraciones.
- Caché Redis de 10 minutos para consultas principales, con invalidación al modificar datos.
- Tres reportes SSRS: clientes activos, ingresos por cliente y clientes inactivos recientes.
- Pruebas unitarias con xUnit y EF Core InMemory.

## Tecnologías

|Área|Tecnología|
|---|---|
|API|ASP.NET Core 8 Web API|
|Datos|Entity Framework Core 8, SQL Server|
|Identidad|ASP.NET Core Identity, JWT Bearer|
|Caché|Redis, StackExchange.Redis|
|Informes|SSRS 2022, RDL 2016|
|Pruebas|xUnit, EF Core InMemory, Moq|
|Documentación de API|Swagger / OpenAPI|

## Arquitectura del repositorio

```text
DesafioEmpresarial.sln
├── src/CompanyManagement.Api/   API, controladores, DTO, entidades, datos y servicios
├── tests/CompanyManagement.Tests/ Pruebas unitarias
├── database/                    Scripts de base de datos, esquema, seed y permisos SSRS
├── reports/                     Informes RDL y script de publicación
├── postman/                     Colección y entorno de Postman
└── docs/
    ├── enunciado/               PDF original del desafío
    └── evidencias/              PDF de ejemplo y capturas del proyecto
```

La API separa controladores, servicios de aplicación, repositorios, DTO y acceso a datos. `AppDbContext` integra las entidades del negocio con las tablas de Identity.

## Requisitos

- Windows y .NET SDK `8.0.408` (configurado en `global.json`).
- SQL Server local con autenticación de Windows.
- Redis accesible en `localhost:6379`.
- SSRS 2022 disponible en `http://kenn/Reports` para publicar y consultar informes.
- `sqlcmd`, Docker y Git.

Comprobaciones rápidas:

```powershell
dotnet --list-sdks
sqlcmd -S localhost -E -Q "SELECT 1"
docker exec redis redis-cli ping
```

## Configuración y ejecución

Clona el repositorio y entra en su carpeta:

```powershell
git clone https://github.com/kennfir3/Desafio_3.git
cd Desafio_3
dotnet restore
```

Crea la base, aplica la migración y carga datos de demostración:

```powershell
sqlcmd -S localhost -E -i database/01_create_database.sql
dotnet ef database update --project src/CompanyManagement.Api --startup-project src/CompanyManagement.Api
sqlcmd -S localhost -E -i database/03_seed.sql -f 65001
```

El seed contiene 15 clientes y 40 órdenes, con clientes sin órdenes para distinguir los reportes. El código también genera datos equivalentes al iniciar en entorno `Development` si la tabla de clientes está vacía.

Inicia la API y abre Swagger:

```powershell
dotnet run --project src/CompanyManagement.Api --urls http://localhost:5000
```

Swagger queda disponible en `http://localhost:5000/swagger`. La cadena de SQL Server, Redis y SSRS se configuran en `src/CompanyManagement.Api/appsettings.json`.

### Configuración de seguridad

El valor de JWT del archivo de configuración está marcado como exclusivo de desarrollo. Antes de utilizar otro entorno, configura un secreto propio, por ejemplo:

```powershell
dotnet user-secrets init --project src/CompanyManagement.Api
dotnet user-secrets set "Jwt:Key" "REEMPLAZAR_POR_UN_SECRETO_LARGO_Y_ALEATORIO" --project src/CompanyManagement.Api
```

También puedes usar la variable de entorno `Jwt__Key`. El usuario `admin@company.com` con contraseña `Admin123!` es una cuenta de demostración local; cambia esos valores antes de desplegar fuera del entorno de desarrollo.

## API

Los endpoints de clientes y órdenes requieren `Authorization: Bearer <token>`. Los reportes requieren además el rol `Admin`. El registro es público y asigna el rol `User` automáticamente.

|Método|Ruta|Acceso|Descripción|
|---|---|---|---|
|`POST`|`/api/auth/register`|Público|Registra un usuario con rol `User`|
|`POST`|`/api/auth/login`|Público|Autentica y devuelve token y expiración|
|`GET`|`/api/clientes`|Autenticado|Lista clientes|
|`GET`|`/api/clientes/{id}`|Autenticado|Obtiene un cliente|
|`POST`|`/api/clientes`|Autenticado|Crea un cliente|
|`PUT`|`/api/clientes/{id}`|Autenticado|Actualiza un cliente|
|`DELETE`|`/api/clientes/{id}`|Admin|Elimina un cliente que no tenga órdenes|
|`GET`|`/api/ordenes`|Autenticado|Lista órdenes|
|`GET`|`/api/ordenes/{id}`|Autenticado|Obtiene una orden|
|`POST`|`/api/ordenes`|Autenticado|Crea una orden para un cliente existente|
|`GET`|`/api/ordenes/cliente/{clienteId}`|Autenticado|Lista las órdenes de un cliente|
|`GET`|`/api/reportes/clientes-activos`|Admin|Devuelve la URL del informe de clientes activos|
|`GET`|`/api/reportes/ingresos-clientes`|Admin|Devuelve la URL del informe de ingresos|
|`GET`|`/api/reportes/clientes-inactivos`|Admin|Devuelve la URL del informe de clientes inactivos|

### Ejemplo de autenticación

```powershell
$body = @{ email = 'admin@company.com'; password = 'Admin123!' } | ConvertTo-Json
$login = Invoke-RestMethod 'http://localhost:5000/api/auth/login' -Method Post -ContentType 'application/json' -Body $body
$headers = @{ Authorization = "Bearer $($login.token)" }
Invoke-RestMethod 'http://localhost:5000/api/clientes' -Headers $headers
```

Para probar con Postman, importa `postman/Desafio3.postman_collection.json` y `postman/Desafio3.local.postman_environment.json`. Registra o inicia sesión, guarda el token en la variable `token` y ejecuta las solicitudes protegidas.

## Caché Redis

La API usa estas claves con expiración de 10 minutos:

|Clave|Contenido|
|---|---|
|`clientes:all`|Listado de clientes|
|`clientes:{id}`|Cliente por identificador|
|`ordenes:all`|Listado de órdenes|

Crear, actualizar o eliminar clientes invalida el listado y, cuando aplica, la clave individual. Crear órdenes invalida el listado de órdenes y la caché del cliente relacionado. Los registros de aplicación identifican aciertos y fallos de caché. Si Redis no responde, la API registra el problema y continúa consultando SQL Server.

Para inspeccionar las claves:

```powershell
docker exec redis redis-cli keys "*"
docker exec redis redis-cli ttl clientes:all
```

## Informes SSRS

Los tres informes utilizan el origen compartido `/DataSources/CompanyManagementDS` y definiciones RDL 2016:

|Informe|Contenido|
|---|---|
|`ClientesActivos`|Clientes con al menos una orden y su total de órdenes|
|`IngresosClientes`|Suma de órdenes por cliente|
|`ClientesInactivos`|Clientes registrados durante el último mes que aún no tienen órdenes|

La publicación configurada para este entorno es:

```powershell
sqlcmd -S localhost -E -i database/04_ssrs_permissions.sql
powershell -ExecutionPolicy Bypass -File reports/deploy-reports.ps1
```

El primer script otorga permisos de lectura a la cuenta de servicio de SSRS. El segundo crea o actualiza la carpeta y el origen de datos, y publica los tres informes mediante la API REST.

Informes publicados:

- [Clientes activos](http://kenn/Reports?/Desafio3/ClientesActivos)
- [Ingresos por cliente](http://kenn/Reports?/Desafio3/IngresosClientes)
- [Clientes inactivos](http://kenn/Reports?/Desafio3/ClientesInactivos)

La API de la aplicación devuelve las URL de renderizado para usuarios Admin. Los PDF de muestra están en `docs/evidencias/`.

## Pruebas

Ejecuta toda la suite desde la raíz:

```powershell
dotnet test
```

Todas las pruebas siguen Arrange–Act–Assert. Cada escenario que usa persistencia crea una base EF Core InMemory aislada; las pruebas de Identity usan Moq sobre `UserManager` y un origen de configuración en memoria para la generación de JWT.

|Archivo|Cobertura|
|---|---|
|`ServiceTests.cs`|Servicios de clientes y órdenes: consultas, creación, duplicados, filtrado y eliminación restringida|
|`ClientesControllerTests.cs`|GET existente/no existente y POST válido/duplicado, verificando resultados HTTP `200`, `404`, `201` y `409`|
|`OrdenesControllerTests.cs`|POST válido, POST con cliente inexistente y GET filtrado por cliente|
|`AuthTests.cs`|Registro con asignación del rol `User`, email duplicado, credenciales inválidas y JWT con claim de rol|
|`AuthorizationTests.cs`|Atributos `[Authorize]` de clientes, órdenes, reportes y DELETE restringido a `Admin`|

Resultado verificado: **20 pruebas aprobadas**. Para listar los casos sin ejecutarlos:

```powershell
dotnet test --list-tests
```

## Decisiones de implementación

- La relación entre cliente y órdenes usa `DeleteBehavior.Restrict`. No se eliminan clientes con órdenes asociadas; la API responde `409 Conflict`.
- La eliminación de clientes y el acceso a reportes requieren `Admin`. El resto de operaciones protegidas admite cualquier usuario autenticado.
- Las fechas se guardan en UTC.
- Las credenciales de demostración y la clave JWT del archivo de ejemplo sólo se deben usar en desarrollo local.

## Evidencias

`docs/evidencias/` contiene PDFs renderizados desde SSRS. Las capturas para la entrega académica se pueden guardar en esa misma carpeta.

## Licencia

Proyecto académico. No se especificó una licencia de distribución.
