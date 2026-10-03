# Company Management API

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=c-sharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022%2B-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server/)
[![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![SSRS](https://img.shields.io/badge/SSRS-Reporting%20Services-0078D4?logo=microsoft&logoColor=white)](https://learn.microsoft.com/sql/reporting-services/)
[![xUnit](https://img.shields.io/badge/Tests-xUnit%20%7C%2020%20Passed-success?logo=xunit&logoColor=white)](https://xunit.net/)

API REST empresarial diseñada para la gestión integral de clientes y órdenes de compra. Implementa autenticación robusta basada en tokens JWT con control de acceso basado en roles (RBAC), persistencia relacional con Entity Framework Core, optimización distribuida de alto rendimiento mediante Redis, y visualización de reportes analíticos paginados en SQL Server Reporting Services (SSRS).

---

## Índice

- [Características Principales](#características-principales)
- [Arquitectura de la Solución](#arquitectura-de-la-solución)
- [Stack Tecnológico](#stack-tecnológico)
- [Requisitos Previos](#requisitos-previos)
- [Puesta en Marcha](#puesta-en-marcha)
- [Especificación de la API](#especificación-de-la-api)
- [Estrategia de Caché Distribuida](#estrategia-de-caché-distribuida)
- [Informes Empresariales (SSRS)](#informes-empresariales-ssrs)
- [Aseguramiento de la Calidad (Testing)](#aseguramiento-de-la-calidad-testing)
- [Estructura del Proyecto](#estructura-del-proyecto)

---

## Características Principales

* **Seguridad y Control de Acceso (RBAC):** Integración con ASP.NET Core Identity. Registro público con asignación automática de rol `User` y protección de endpoints mediante JWT Bearer tokens. Operaciones privilegiadas (reportes analíticos y baja de clientes) restringidas exclusivamente al rol `Admin`.
* **Capa de Dominio y Persistencia:** Mapeo objeto-relacional (ORM) mediante Entity Framework Core 8 con SQL Server, respaldado por migraciones automáticas y scripts de seed inicial. Integridad referencial protegida bajo políticas estrictas de restricción de borrado (`DeleteBehavior.Restrict`).
* **Optimización de Consultas (Redis):** Caché distribuida para consultas frecuentes con TTL de 10 minutos. Mecanismo de invalidación reactiva e inmediata ante eventos de mutación (creación, actualización o borrado) y tolerancia a fallos con degradación controlada a SQL Server.
* **Inteligencia de Negocio y Reportería (SSRS):** Informes paginados RDL 2016 integrados a un origen de datos compartido (`CompanyManagementDS`) y expuestos mediante URLs seguras en la API.
* **Calidad y Mantenibilidad:** Arquitectura desacoplada basada en el patrón Repository, inversión de dependencias y suite de 20 pruebas unitarias automatizadas con xUnit y EF Core InMemory.

---

## Arquitectura de la Solución

El sistema implementa una arquitectura en capas con separación rigurosa de responsabilidades:

```text
┌───────────────────────────────────────────────────────────────┐
│               CompanyManagement.Api (REST / HTTP)             │
│        Controllers  │  Filters  │  Swagger  │  JWT Bearer     │
└───────────────┬───────────────────────────────┬───────────────┘
                │                               │
                ▼                               ▼
┌──────────────────────────────┐ ┌──────────────────────────────┐
│       Services Layer         │ │     RedisCacheService        │
│ Business Logic & Validation  │ │ (Distributed Read Cache)     │
└───────────────┬──────────────┘ └──────────────┬───────────────┘
                │                               │
                ▼                               ▼
┌──────────────────────────────┐ ┌──────────────────────────────┐
│      Repositories Layer      │ │      StackExchange.Redis     │
│   (ICliente / IOrden Repo)   │ │      (Docker: Port 6379)     │
└───────────────┬──────────────┘ └──────────────────────────────┘
                │
                ▼
┌──────────────────────────────┐ ┌──────────────────────────────┐
│    Entity Framework Core     │ │     SSRS Reporting 2022      │
│  (AppDbContext / Identity)   │ │   (RDL Shared Data Source)   │
└───────────────┬──────────────┘ └──────────────┬───────────────┘
                │                               │
                ▼                               ▼
┌───────────────────────────────────────────────────────────────┐
│              Microsoft SQL Server (CompanyManagement)         │
└───────────────────────────────────────────────────────────────┘
```

---

## Stack Tecnológico

| Componente | Tecnología | Versión | Rol en el Sistema |
| :--- | :--- | :--- | :--- |
| **Backend Framework** | ASP.NET Core Web API | 8.0 | Núcleo de servicios RESTful y middleware |
| **ORM & Migraciones** | Entity Framework Core | 8.0 | Persistencia de datos y mapeo relacional |
| **Base de Datos** | Microsoft SQL Server | 2022+ | Almacenamiento relacional de datos e identidad |
| **Seguridad** | ASP.NET Core Identity | 8.0 | Gestión de usuarios, hashes y asignación de roles |
| **Tokens** | Microsoft.IdentityModel (JWT) | 7.x | Autenticación simétrica stateless con claims |
| **Caché Distribuida** | Redis & StackExchange.Redis | 7.x (Docker) | Aceleración de lectura e invalidación reactiva |
| **Reportería** | SQL Server Reporting Services | 2022 (RDL 2016) | Informes paginados financieros y de clientes |
| **Pruebas Unitarias** | xUnit, Moq, EF Core InMemory | 8.0 / 2.5 | Cobertura de lógica de negocio y controladores |
| **Documentación** | Swagger / OpenAPI | 6.5 | Catálogo interactivo de la API REST |

---

## Requisitos Previos

* **Sistema Operativo:** Windows 10/11 o Windows Server.
* **.NET SDK:** Versión `8.0.x` instalada ([global.json](file:///c:/Users/Kenn/Documents/TAREAS%20OFICIALES-%20universidad/DESARROLLO%20DE%20SOFTWARE%20EMPRESARIAL/Desafio%203/global.json)).
* **Motor de Base de Datos:** SQL Server local con autenticación de Windows habilitada.
* **Docker Desktop:** En ejecución para el servicio de Redis.
* **Servidor de Reportes:** SQL Server Reporting Services (SSRS 2022).
* **Herramientas de Consola:** Git, `sqlcmd`, .NET CLI.

---

## Puesta en Marcha

### 1. Clonación del Repositorio y Restauración de Paquetes

```powershell
git clone https://github.com/kennfir3/Desafio_3.git
cd Desafio_3
dotnet restore
```

### 2. Infraestructura de Caché (Redis en Docker)

Inicie el contenedor de Redis en el puerto estándar `6379`:

```powershell
docker run -d --name redis -p 6379:6379 redis
```

*Verificación de conectividad:*
```powershell
docker exec redis redis-cli ping
# Salida esperada: PONG
```

### 3. Aprovisionamiento de Base de Datos

Ejecute la creación de la base de datos, aplique las migraciones de EF Core y cargue los datos de prueba iniciales (seed):

```powershell
# Creación de la base de datos CompanyManagement
sqlcmd -S localhost -E -i database/01_create_database.sql

# Aplicación de migraciones de Entity Framework Core
dotnet ef database update --project src/CompanyManagement.Api --startup-project src/CompanyManagement.Api

# Carga de datos de demostración (clientes y órdenes)
sqlcmd -S localhost -E -i database/03_seed.sql -f 65001
```

> **Nota:** El parámetro `-f 65001` asegura la codificación correcta en UTF-8 para nombres y caracteres acentuados.

### 4. Despliegue de Reportes en SSRS

Otorgue permisos a la cuenta de servicio de SSRS y publique los tres reportes RDL mediante la API de Reporting Services:

```powershell
sqlcmd -S localhost -E -i database/04_ssrs_permissions.sql
powershell -ExecutionPolicy Bypass -File reports/deploy-reports.ps1
```

### 5. Ejecución del Servicio Web

Inicie la API en el entorno de desarrollo:

```powershell
dotnet run --project src/CompanyManagement.Api --urls http://localhost:5000
```

* La interfaz interactiva de Swagger UI queda disponible en: [http://localhost:5000/swagger](http://localhost:5000/swagger)
* Portal de reportes SSRS disponible en: `http://localhost/Reports` (o `http://kenn/Reports`)

---

## Especificación de la API

Todos los endpoints bajo las rutas `/api/clientes`, `/api/ordenes` y `/api/reportes` exigen la cabecera HTTP:
```http
Authorization: Bearer <TOKEN_JWT>
```

### Catálogo de Endpoints

| Método | Endpoint | Nivel de Acceso | Descripción |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Público | Registra un usuario y le asigna el rol `User` por defecto |
| `POST` | `/api/auth/login` | Público | Autentica credenciales y emite el token JWT con claims de rol |
| `GET` | `/api/clientes` | Autenticado | Obtiene la lista completa de clientes (con soporte para caché) |
| `GET` | `/api/clientes/{id}` | Autenticado | Retorna el detalle del cliente especificado |
| `POST` | `/api/clientes` | Autenticado | Registra un cliente e invalida la caché de clientes |
| `PUT` | `/api/clientes/{id}` | Autenticado | Actualiza un cliente existente e invalida su caché |
| `DELETE` | `/api/clientes/{id}` | **Admin** | Elimina un cliente. Se rechaza con `409` si posee órdenes |
| `GET` | `/api/ordenes` | Autenticado | Lista el historial general de órdenes de compra |
| `GET` | `/api/ordenes/{id}` | Autenticado | Obtiene una orden específica por su identificador |
| `GET` | `/api/ordenes/cliente/{clienteId}` | Autenticado | Retorna las órdenes pertenecientes al cliente indicado (1 a N) |
| `POST` | `/api/ordenes` | Autenticado | Registra una orden para un cliente existente e invalida caché |
| `GET` | `/api/reportes/clientes-activos` | **Admin** | Provee la URL de renderizado del reporte de clientes activos |
| `GET` | `/api/reportes/ingresos-clientes` | **Admin** | Provee la URL de renderizado del reporte de ingresos por cliente |
| `GET` | `/api/reportes/clientes-inactivos` | **Admin** | Provee la URL del reporte de clientes inactivos recientes |

### Credenciales de Demostración (Seed Local)

* **Administrador:** `admin@company.com` | `Admin123!`
* **Usuario Estándar:** Puede crearse directamente mediante `POST /api/auth/register` (ej. `usuario.demo@company.com` / `User123!`).

---

## Estrategia de Caché Distribuida

La solución utiliza `StackExchange.Redis` para minimizar la latencia y descargar lecturas repetitivas sobre SQL Server.

### Llaves y Políticas de Expiración

| Clave | Contenido | Política de Expiración | Política de Invalidación |
| :--- | :--- | :--- | :--- |
| `clientes:all` | Listado general de clientes | 10 minutos (Sliding/Absolute) | Se elimina en `POST`, `PUT` y `DELETE` de clientes |
| `clientes:{id}` | Registro individual de cliente | 10 minutos | Se elimina al ejecutar `PUT` o `DELETE` sobre el ID |
| `ordenes:all` | Listado general de órdenes | 10 minutos | Se elimina al registrar una nueva orden (`POST /api/ordenes`) |

### Resiliencia y Monitoreo de Caché

* **Degradación Elegante:** Si el contenedor de Redis pierde conectividad o no responde, `RedisCacheService` captura la excepción, registra una advertencia en los logs de la aplicación y redirige la solicitud de forma transparente a SQL Server sin interrumpir el servicio.
* **Inspección de llaves en tiempo real:**
  ```powershell
  # Listar claves activas
  docker exec redis redis-cli keys "*"

  # Comprobar tiempo de vida (TTL) restante en segundos
  docker exec redis redis-cli ttl clientes:all
  ```

---

## Informes Empresariales (SSRS)

Los reportes han sido diseñados bajo el estándar **RDL 2016** y consumen el origen de datos compartido `/DataSources/CompanyManagementDS`:

| Reporte | Identificador | Criterio de Selección SQL | Indicadores Clave |
| :--- | :--- | :--- | :--- |
| **Clientes Activos** | `ClientesActivos` | Clientes con al menos una orden (`COUNT(o.Id) >= 1`) | `Nombre`, `Email`, `Total órdenes` |
| **Ingresos por Cliente** | `IngresosClientes` | Sumatoria acumulada de compras (`SUM(o.MontoTotal)`) | `Nombre`, `Email`, `Monto total`, Consolidado global |
| **Clientes Inactivos** | `ClientesInactivos` | Registrados en el último mes sin compras asociadas | `Nombre`, `Email`, `Fecha registro` |

*Las URLs de acceso directo retornadas por la API permiten renderizado interactivo en el portal web de SSRS o descarga directa en formato PDF añadiendo el parámetro `&rs:Format=PDF`.*

---

## Aseguramiento de la Calidad (Testing)

El proyecto cuenta con una batería de pruebas automatizadas implementada en el proyecto `CompanyManagement.Tests`.

### Ejecución de Pruebas

```powershell
dotnet test --verbosity normal
```

### Cobertura de la Suite

* **20 de 20 pruebas aprobadas (100% de éxito).**
* **Patrón de diseño:** Arrange-Act-Assert (AAA) riguroso.
* **Aislamiento:** Instancias dedicadas de `Microsoft.EntityFrameworkCore.InMemory` con identificadores únicos por prueba y simulación de dependencias con `Moq`.

| Archivo de Prueba | Componente Evaluado | Escenarios Cubiertos |
| :--- | :--- | :--- |
| `ClientesControllerTests.cs` | Controlador de Clientes | Respuestas HTTP `200`, `404`, `201` y conflicto por email `409` |
| `OrdenesControllerTests.cs` | Controlador de Órdenes | Creación válida, rechazo ante cliente inexistente y filtrado 1 a N |
| `AuthTests.cs` | Servicio de Identidad | Registro con rol `User`, rechazo de duplicados y JWT con claims |
| `AuthorizationTests.cs` | Middleware de Seguridad | Verificación estricta de atributos `[Authorize]` y roles `Admin` |
| `ServiceTests.cs` | Reglas del Dominio | Validación de negocio y prohibición de borrado en cascada |

---

## Estructura del Proyecto

```text
DesafioEmpresarial.sln
├── src/
│   └── CompanyManagement.Api/
│       ├── Controllers/            # Controladores REST (Auth, Clientes, Ordenes, Reportes)
│       ├── Data/                   # AppDbContext, configuración de entidades y Seed
│       ├── DTOs/                   # Data Transfer Objects y validaciones DataAnnotations
│       ├── Models/                 # Entidades del dominio (Cliente, Orden)
│       ├── Repositories/           # Interfaces e implementaciones del patrón Repository
│       ├── Services/               # Lógica de negocio, servicios de autenticación y Redis
│       └── appsettings.json        # Cadenas de conexión (SQL Server, Redis, SSRS, JWT)
├── tests/
│   └── CompanyManagement.Tests/    # Suite de pruebas unitarias xUnit
├── database/                       # Scripts DDL/DML, esquema SQL y permisos de SSRS
├── reports/                        # Definiciones RDL 2016 y script de despliegue automatizado
├── postman/                        # Colección completa v2.1 (14 endpoints) y variables de entorno
└── docs/                           # Enunciado oficial y evidencias técnicas
```

---

## Postman y Pruebas Manuales

En la carpeta [postman/](file:///c:/Users/Kenn/Documents/TAREAS%20OFICIALES-%20universidad/DESARROLLO%20DE%20SOFTWARE%20EMPRESARIAL/Desafio%203/postman) se incluyen los archivos listos para importar:
* **Colección:** `postman/Desafio3.postman_collection.json` (incluye los 14 endpoints estructurados por módulo y scripts automáticos de captura de token).
* **Entorno:** `postman/Desafio3.local.postman_environment.json`.

---

## Licencia y Uso Académico

Proyecto desarrollado con fines académicos para la asignatura de **Desarrollo de Software Empresarial** de la **Universidad Don Bosco (UDB)**. Todos los derechos reservados.
