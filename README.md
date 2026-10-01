# Company Management API

API REST para gestionar clientes y órdenes con autenticación, autorización por roles, caché Redis y reportes paginados en SQL Server Reporting Services (SSRS).

> Proyecto académico del Desafío 3, opción 2.

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

Si Redis no existe o no está iniciado, créalo con Docker:

```powershell
docker run -d --name redis -p 6379:6379 redis
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

El seed contiene 15 clientes y 40 órdenes, con clientes sin órdenes para distinguir los reportes. El código también genera datos equivalentes al iniciar en entorno `Development` si la tabla de clientes está vacía. **No omitas `-f 65001`: sin ese parámetro `sqlcmd` puede corromper los caracteres acentuados.**

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

Portal SSRS (para navegar):

- [Clientes activos](http://kenn/Reports/report/Desafio3/ClientesActivos)
- [Ingresos por cliente](http://kenn/Reports/report/Desafio3/IngresosClientes)
- [Clientes inactivos](http://kenn/Reports/report/Desafio3/ClientesInactivos)

Renderizado directo (las URLs que devuelve la API a Admin):

- `http://kenn/ReportServer?/Desafio3/ClientesActivos&rs:Command=Render`
- `http://kenn/ReportServer?/Desafio3/IngresosClientes&rs:Command=Render`
- `http://kenn/ReportServer?/Desafio3/ClientesInactivos&rs:Command=Render`

Para descargar o abrir una versión PDF, añade `&rs:Format=PDF` al final de cualquiera de esas URL. Las tres rutas de la API se comprobaron y devolvieron exactamente estas URLs; los informes abrieron en SSRS.

El host `kenn` es el nombre del equipo de desarrollo. En otro equipo, cambia `SSRS:BaseUrl` en `appsettings.json` y los hosts en `reports/deploy-reports.ps1`.

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

Guarda las capturas de la demostración en `docs/evidencias/` con estos nombres:

|Archivo|Contenido sugerido|
|---|---|
|`01_postman_register_login.png`|Registro e inicio de sesión; token obtenido|
|`02_postman_clientes_crud.png`|Operaciones CRUD de clientes con token|
|`03_postman_403_user.png`|Usuario `User` recibe `403` al consultar un reporte|
|`04_postman_200_admin.png`|Usuario `Admin` obtiene `200` del endpoint de reporte|
|`05_dotnet_test.png`|Salida de pruebas aprobadas|
|`06_reporte_clientes_activos.png`|Informe de clientes activos en SSRS|
|`07_reporte_ingresos_clientes.png`|Informe de ingresos por cliente en SSRS|
|`08_reporte_clientes_inactivos.png`|Informe de clientes inactivos en SSRS|
|`09_redis_keys_ttl.png`|Claves Redis y expiración con `redis-cli`|

Los PDF de ejemplo renderizados están guardados en esta carpeta.

## Video demo

Enlace al video: `<PEGAR_AQUI_EL_ENLACE>`

Guion sugerido para una demostración de 5 a 8 minutos:

1. **0:00–0:45:** presentar la estructura del repositorio y tecnologías principales.
2. **0:45–1:45:** registrar/iniciar sesión, mostrar JWT y explicar los roles `User` y `Admin`.
3. **1:45–2:45:** demostrar CRUD de clientes y creación/consulta de órdenes.
4. **2:45–3:45:** repetir un GET para observar cache hit; mostrar claves y TTL en Redis y luego invalidación tras una escritura.
5. **3:45–4:45:** mostrar el `403` de `User`, el `200` de `Admin` y abrir los tres informes SSRS.
6. **4:45–5:45:** ejecutar `dotnet test` y comentar la cobertura por archivo.
7. **5:45–6:15:** cerrar con configuración, decisiones de persistencia y README.

## Preguntas probables en la defensa

1. **¿Cómo se autentica una petición?** Se envía un JWT firmado en `Authorization: Bearer`; JwtBearer valida emisor, audiencia, firma y vigencia.
2. **¿Cuál es el esquema de autenticación por defecto?** `Program.cs` establece `JwtBearerDefaults.AuthenticationScheme` como esquema de autenticación, desafío y prohibición.
3. **¿Qué rol recibe un usuario nuevo?** `AuthService.Register` crea el usuario y llama a `AddToRoleAsync` con `User`.
4. **¿Para qué se usa Repository?** `IClienteRepository` e `IOrdenRepository` aíslan las consultas de EF Core de los servicios.
5. **¿Cómo se invalida la caché?** Los servicios eliminan las claves afectadas al crear/actualizar/eliminar clientes y al crear órdenes.
6. **¿Qué ocurre si Redis cae?** `RedisCacheService` registra una advertencia y sus operaciones de caché fallan de forma controlada; la consulta continúa hacia SQL Server.
7. **¿Por qué `DeleteBehavior.Restrict`?** Evita borrar órdenes históricas indirectamente; el servicio rechaza eliminar clientes que todavía tienen órdenes.
8. **¿Quién puede ver los reportes?** `ReportesController` está decorado con `[Authorize(Roles = "Admin")]`; un usuario autenticado sin ese rol recibe `403`.
9. **¿Cómo se aíslan las pruebas de base de datos?** Cada prueba de persistencia crea un nombre único para EF Core InMemory.
10. **¿Qué configuración de datos usan los RDL?** Cada informe referencia el origen compartido `/DataSources/CompanyManagementDS`, que apunta a `CompanyManagement`.

## Licencia

Proyecto académico. No se especificó una licencia de distribución.
