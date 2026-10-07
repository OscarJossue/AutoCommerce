# AutoCommerce

AutoCommerce es una aplicación web desarrollada como reto técnico para demostrar conocimientos y buenas prácticas en Angular, ASP.NET Core .NET y SQL Server.

El sistema permite cargar archivos CSV con información comercial de vehículos, previsualizar los registros antes de enviarlos al servidor, almacenarlos en SQL Server, procesarlos por fecha y enviar automáticamente a cuarentena los registros que no cumplen las reglas de validación.

---

## Tecnologías utilizadas

### Front End
- Angular 21
- TypeScript
- HTML
- CSS
- HttpClient
- Diseño responsive

### Back End
- ASP.NET Core .NET 10
- C#
- REST API
- Microsoft.Data.SqlClient
- CsvHelper

### Base de Datos
- SQL Server 2022 Express
- Stored Procedures
- Transacciones SQL

### Herramientas
- Visual Studio Code
- SQL Server Management Studio
- Git
- Node.js
- npm

---

## Arquitectura

La aplicación utiliza una arquitectura separada por responsabilidades:

```text
┌──────────────────────────────┐
│           ANGULAR            │
│          Front End           │
│                              │
│  - Carga de CSV              │
│  - Previsualizacion          │
│  - Procesamiento por fecha   │
│  - Visualizacion de errores  │
└──────────────┬───────────────┘
               │
               │ HTTP / REST
               ▼
┌──────────────────────────────┐
│       ASP.NET CORE API       │
│           .NET 10            │
│                              │
│  CommerceController          │
│  CommerceRepository          │
│  Models                      │
│  DatabaseConnection          │
└──────────────┬───────────────┘
               │
               │ Microsoft.Data.SqlClient
               ▼
┌──────────────────────────────┐
│          SQL SERVER          │
│                              │
│  commerce                    │
│  commerce_quarantine         │
│                              │
│  sp_create_commerce          │
│  sp_process_commerce         │
└──────────────────────────────┘
```

Angular nunca accede directamente a SQL Server.

El Front End consume la API REST desarrollada en .NET y el Back End es responsable del acceso a la base de datos.

---

## Estructura del proyecto

```text
RetoCommerce
│
├── backend
│   └── Commerce.Api
│       ├── Controllers
│       │   └── CommerceController.cs
│       │
│       ├── Data
│       │   └── DatabaseConnection.cs
│       │
│       ├── Models
│       │   ├── CommerceCsvRow.cs
│       │   ├── CommerceQuarantineRow.cs
│       │   └── ProcessCommerceRequest.cs
│       │
│       ├── Repositories
│       │   └── CommerceRepository.cs
│       │
│       ├── Properties
│       ├── appsettings.json
│       ├── Commerce.Api.csproj
│       └── Program.cs
│
├── database
│   ├── 01_create_database.sql
│   └── 02_stored_procedures.sql
│
├── frontend
│   └── auto-commerce
│
├── sample-data
│   └── commerce_07102026.csv
│
├── README.md
└── .gitignore
```

---

## Funcionalidades principales

La aplicación permite:

- Cargar archivos CSV con el formato `commerce_DDMMYYYY.csv`.
- Validar que el archivo no se encuentre vacío.
- Validar el nombre del archivo.
- Validar la estructura de columnas del CSV.
- Previsualizar los registros antes de enviarlos al Back End.
- Registrar la información en SQL Server.
- Insertar cada registro mediante el Stored Procedure `sp_create_commerce`.
- Procesar información según la columna `pc_processdate`.
- Validar los datos mediante el Stored Procedure `sp_process_commerce`.
- Eliminar de `commerce` los registros inválidos.
- Registrar los registros inválidos en `commerce_quarantine`.
- Registrar el motivo por el cual un registro fue enviado a cuarentena.
- Consultar los registros de cuarentena mediante un endpoint GET.
- Visualizar los registros rechazados desde Angular.
- Mostrar el estado de conexión entre Angular, .NET y SQL Server.
- Mostrar estadísticas de registros cargados, válidos y enviados a cuarentena.

---

## Archivo CSV

El archivo debe utilizar el siguiente formato de nombre:

```text
commerce_DDMMYYYY.csv
```

Ejemplo:

```text
commerce_07102026.csv
```

La estructura utilizada es:

```text
pc_processdate
pc_nomcomred
pc_numdoc
nom_vehiculo
marca
modelo
anio
placa
color
tipo_vehiculo
kilometraje
precio
estado_vehiculo
```

Ejemplo:

```csv
pc_processdate,pc_nomcomred,pc_numdoc,nom_vehiculo,marca,modelo,anio,placa,color,tipo_vehiculo,kilometraje,precio,estado_vehiculo
2026-10-07,AutoMundo Quito,1792456789001,Toyota RAV4 XLE,Toyota,RAV4,2025,PBA1234,Blanco,SUV,0,38990.00,Nuevo
```

Se incluye un archivo de prueba en:

```text
sample-data/commerce_07102026.csv
```

---

## Reglas de validación

### pc_nomcomred

El nombre comercial no puede encontrarse vacío.

Ejemplo inválido:

```text
pc_nomcomred = ""
```

Motivo generado:

```text
El nombre del comercio (nomcomred) se encuentra vacio.
```

### pc_numdoc

El número de documento:

- No puede encontrarse vacío.
- No puede contener letras.
- No puede contener caracteres especiales.

Ejemplo inválido:

```text
ABC123456
```

Motivo generado:

```text
El numero de documento (numdoc) contiene letras o caracteres especiales.
```

Los registros inválidos son eliminados de:

```text
commerce
```

y registrados en:

```text
commerce_quarantine
```

junto con el campo:

```text
motivo
```

---

## Base de Datos

Nombre de la base utilizada:

```text
CommerceDb
```

Instancia utilizada durante el desarrollo:

```text
localhost\SQLEXPRESS01
```

### Tablas

```text
commerce
commerce_quarantine
```

La tabla `commerce` contiene la misma estructura utilizada por el archivo CSV.

La tabla `commerce_quarantine` contiene la información del registro rechazado junto con el campo:

```text
motivo
```

---

## Stored Procedures

### sp_create_commerce

Stored Procedure encargado de registrar los valores recibidos desde el archivo CSV en la tabla:

```text
commerce
```

El Back End utiliza este procedimiento para almacenar cada registro.

### sp_process_commerce

Stored Procedure encargado de procesar los registros correspondientes a una fecha específica utilizando:

```text
pc_processdate
```

Realiza las validaciones de:

```text
pc_nomcomred
pc_numdoc
```

Los registros incorrectos son enviados a:

```text
commerce_quarantine
```

Posteriormente son eliminados de:

```text
commerce
```

El procedimiento utiliza una transacción SQL para mantener la consistencia de la información.

Al finalizar devuelve la cantidad de registros enviados a cuarentena.

---

## API REST

URL utilizada durante el desarrollo:

```text
http://localhost:5294
```

### Estado de conexión

```http
GET /api/commerce/database-status
```

Permite comprobar la conexión entre la API y SQL Server.

---

### Cargar archivo CSV

```http
POST /api/commerce/upload
```

Tipo de contenido:

```text
multipart/form-data
```

Parámetro:

```text
file
```

El archivo debe utilizar:

```text
commerce_DDMMYYYY.csv
```

Ejemplo de respuesta exitosa:

```json
{
  "status": "OK",
  "message": "Archivo CSV cargado correctamente.",
  "archivo": "commerce_07102026.csv",
  "registrosInsertados": 6
}
```

Si el archivo está vacío se retorna:

```text
400 Bad Request
```

Ejemplo:

```json
{
  "status": "ERROR",
  "message": "El archivo CSV se encuentra vacio."
}
```

---

### Procesar información

```http
POST /api/commerce/process
```

Body:

```json
{
  "processDate": "2026-10-07"
}
```

Ejemplo de respuesta:

```json
{
  "status": "OK",
  "message": "Procesamiento realizado correctamente.",
  "fechaProceso": "2026-10-07",
  "registrosCuarentena": 3
}
```

---

### Consultar cuarentena

```http
GET /api/commerce/quarantine
```

Ejemplo:

```json
{
  "status": "OK",
  "total": 3,
  "data": []
}
```

---

## Flujo de procesamiento

```text
commerce_DDMMYYYY.csv
          │
          ▼
       Angular
          │
          ├── Validacion de nombre
          ├── Validacion de archivo
          └── Previsualizacion
          │
          ▼
POST /api/commerce/upload
          │
          ▼
    ASP.NET Core
          │
          ▼
   CsvHelper / Models
          │
          ▼
 CommerceRepository
          │
          ▼
 sp_create_commerce
          │
          ▼
       commerce
          │
          ▼
POST /api/commerce/process
          │
          ▼
 sp_process_commerce
          │
          ├───────────────┐
          │               │
        VALIDO         INVALIDO
          │               │
          ▼               ▼
      commerce    commerce_quarantine
                          │
                          ▼
                       motivo
```

---

## Configuración de SQL Server

Los scripts necesarios se encuentran en:

```text
database/
```

Ejecutar primero:

```text
01_create_database.sql
```

Después ejecutar:

```text
02_stored_procedures.sql
```

La cadena de conexión se encuentra en:

```text
backend/Commerce.Api/appsettings.json
```

Configuración utilizada durante el desarrollo:

```text
Server=localhost\SQLEXPRESS01
Database=CommerceDb
Trusted_Connection=True
TrustServerCertificate=True
Encrypt=True
```

Si se utiliza otra instancia de SQL Server, se debe actualizar la cadena de conexión.

---

## Ejecutar Back End

Entrar a:

```bash
cd backend/Commerce.Api
```

Restaurar dependencias:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

Ejecutar:

```bash
dotnet run
```

La configuración utilizada durante el desarrollo levanta la API en:

```text
http://localhost:5294
```

---

## Ejecutar Front End

Entrar a:

```bash
cd frontend/auto-commerce
```

Instalar dependencias:

```bash
npm install
```

Ejecutar Angular:

```bash
ng serve
```

Abrir:

```text
http://localhost:4200
```

---

## CORS

La API permite peticiones desde el servidor de desarrollo de Angular:

```text
http://localhost:4200
```

La política CORS se encuentra configurada en:

```text
Program.cs
```

---

## Pruebas realizadas

Se verificó correctamente:

```text
Carga de CSV                         OK
Previsualizacion                    OK
Validacion de nombre                OK
Archivo vacio -> BadRequest         OK
Insercion mediante Stored Procedure OK
Procesamiento por pc_processdate    OK
Validacion pc_nomcomred             OK
Validacion pc_numdoc                OK
Movimiento a cuarentena             OK
Registro de motivo                  OK
GET de cuarentena                   OK
Angular -> .NET                     OK
.NET -> SQL Server                  OK
CORS                                OK
```

En el escenario de prueba incluido:

```text
Registros cargados:       6
Registros validos:        3
Registros en cuarentena:  3
```

---

## Build de producción

### Back End

Se verificó mediante:

```bash
dotnet build -c Release
```

Resultado:

```text
Compilacion realizada correctamente.
```

### Front End

Se verificó mediante:

```bash
ng build
```

Resultado:

```text
Application bundle generation complete.
```

---

## Consideraciones de diseño

El proyecto fue desarrollado buscando mantener:

- Separación de responsabilidades.
- Código organizado.
- Acceso a datos centralizado.
- Uso de Stored Procedures.
- Operaciones asíncronas.
- Manejo de errores.
- Validaciones tanto en Front End como Back End.
- Transacciones SQL para proteger la consistencia de datos.
- Comunicación mediante API REST.
- Interfaz responsive.
- Código fácil de mantener y explicar.

---

## Autor

Reto técnico individual.

Proyecto:

```text
AutoCommerce
```