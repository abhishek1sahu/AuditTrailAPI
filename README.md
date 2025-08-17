# AuditTrailAPI

A **.NET Core Web API** to track and manage **audit logs** for database entities.  
The project follows **Clean Architecture principles** with layers for Controllers, DTOs, Repository, and Service.

---

## 📂 Project Structure

AuditTrailAPI/
│-- Controllers/
│ └── AuditController.cs # API endpoints for audit operations
│
│-- Data/
│ └── AuditDbContext.cs # EF Core DbContext configuration
│
│-- DTO/
│ ├── AuditEntry.cs # Represents audit log entity
│ ├── AuditFieldChange.cs # Captures field-level changes
│ ├── AuditQueryRequest.cs # Request object for searching audits
│ ├── AuditRequest.cs # Request object for creating audits
│ ├── AuditResponse.cs # Response object for audit details
│ ├── FieldChangeDto.cs # DTO for changed fields
│ └── PagedResult.cs # Generic class for paginated results
│
│-- Migrations/
│ ├── 20250816092017_InitialCreate.cs # Initial EF migration
│ └── AuditDbContextModelSnapshot.cs # Migration snapshot
│
│-- Model/ # (Future domain models can go here)
│
│-- Repository/
│ ├── IRepository/
│ │ └── IAuditRepository.cs # Interface for repository layer
│ └── AuditRepository.cs # Repository implementation
│
│-- Service/
│ ├── IService/
│ │ └── IAuditService.cs # Interface for audit service
│ └── AuditService.cs (expected) # Business logic for auditing
│
│-- Program.cs # Entry point
│-- Startup.cs / minimal setup # (depending on template)
│-- README.md



## 🚀 Features
- Record **create, update, delete** actions with before/after values.
- Capture **field-level changes** for better tracking.
- Store **user ID, timestamp, and entity details**.
- Supports **search and pagination** of audit logs.
- Built with **Entity Framework Core** and repository pattern.

---

## 🔑 API Endpoints

### 1. Create Audit Entry
**POST** `/api/audit`

```json
{
"entityName": "Customer", "entityId": "123", "action": "Updated", "userId": "u001",
 "objectBefore": "{ \"name\": \"John\", \"age\": 30 }",
"objectAfter": "{ \"name\": \"Johnny\", \"age\": 31 }"
 "additionalMetadata": {
    "ipAddress": "192.168.1.10"
  } }


Clone repository:

git clone https://github.com/abhishek1sahu/AuditTrailAPI.git
cd AuditTrailAPI


Restore dependencies:

dotnet restore


Run EF Core migration:

dotnet ef database update


Run project:

dotnet run


Open Swagger UI in browser:

https://localhost:5127/index.html
