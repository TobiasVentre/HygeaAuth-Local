# AuthMS Decomposition Plan

## Objetivo

Reducir `AuthMS` a identidad y autorizacion, moviendo los endpoints y responsabilidades operativas al microservicio duenio del dominio.

## Regla de ownership

- `AuthMS`: registro, login, refresh token, logout, verificacion de email, password reset, JWT, claims, politicas.
- `DirectoryMS`: perfiles de cliente y tecnico.
- `SchedulingMS`: agenda, disponibilidad, ausencias y reservas.
- `OrderMS`: historial operativo de servicios y consultas derivadas de ordenes.
- Eventos funcionales: se emiten desde el microservicio que realiza el cambio de negocio.

## Inventario actual en AuthMS

### Permanecen en AuthMS

- `POST /api/v1/auth/login`
- `POST /api/v1/auth/logout`
- `POST /api/v1/auth/refreshtoken`
- `POST /api/v1/auth/changepassword`
- `POST /api/v1/auth/passwordresetrequest`
- `POST /api/v1/auth/passwordresetconfirm`
- `POST /api/v1/auth/verifyemail`
- `POST /api/v1/auth/resendverificationemail`
- `POST /api/v1/user` para alta de identidad

### Deben salir de AuthMS

- `GET /api/v1/client/profile`
- `PUT /api/v1/client/profile`
- `GET /api/v1/client/appointments`
- `GET /api/v1/client/medical-history`
- `GET /api/v1/technician/profile`
- `PUT /api/v1/technician/profile`
- `GET /api/v1/technician/schedule`
- `GET /api/v1/technician/clients`
- `GET /api/v1/technician/client/{clientId}/service-history`
- `POST /api/v1/technician/appointments`
- `POST /api/v1/notifications/events`

## Mapa de migracion por endpoint

| Endpoint legacy en AuthMS | Destino | Estado |
| --- | --- | --- |
| `GET /client/profile` | `DirectoryMS` query por `AuthUserId` | Existe base parcial |
| `PUT /client/profile` | `DirectoryMS` update de `ClientProfile` | Existe base parcial |
| `GET /technician/profile` | `DirectoryMS` query por `AuthUserId` | Existe base parcial |
| `PUT /technician/profile` | `DirectoryMS` update de `TechnicianProfile` | Existe base parcial |
| `GET /client/appointments` | `SchedulingMS` reservas por cliente | Existe endpoint de reservas por cliente |
| `GET /technician/schedule` | `SchedulingMS` reservas por tecnico + disponibilidad/ausencias | Existe base parcial |
| `POST /technician/appointments` | `SchedulingMS` `POST /reservations` | Existe base parcial |
| `GET /client/medical-history` | `OrderMS` ordenes por cliente | Existe endpoint de ordenes por cliente |
| `GET /technician/client/{clientId}/service-history` | `OrderMS` query historica tecnico-cliente | Pendiente |
| `GET /technician/clients` | `OrderMS` o read model dedicado | Pendiente |
| `POST /notifications/events` | MS duenio del evento o futuro NotificationMS | Pendiente |

## Plan de ejecucion

1. Mantener compatibilidad temporal marcando endpoints legacy como deprecated.
2. Consumir `DirectoryMS` para perfiles desde frontend y cualquier BFF.
3. Consumir `SchedulingMS` para agenda y reservas.
4. Consumir `OrderMS` para historial operativo.
5. Crear queries faltantes en `OrderMS` para vistas tecnico-cliente.
6. Eliminar endpoints legacy de `AuthMS` cuando no existan consumidores.

## Criterio de salida

`AuthMS` quedara dentro de su bounded context cuando:

- no exponga perfiles operativos;
- no exponga agenda ni reservas;
- no exponga historial de servicios;
- no reciba eventos funcionales de negocio ajenos a autenticacion/seguridad.
