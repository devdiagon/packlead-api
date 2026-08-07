# API Reference

> The full interactive API reference — generated from the OpenAPI spec, including request/response schemas and a "try it out" client — is available at the **`/scalar`** endpoint when running the API in the development environment.
>
> This document is a quick, human-readable index of the available endpoints, their required role, and their payloads. Response shapes are intentionally omitted here — see `/scalar` for those.

All endpoints require a valid Firebase ID token sent as `Authorization: Bearer <token>`, unless noted otherwise. See [authentication.md](./authentication.md) for how tokens and roles work.

Roles:
- **Admin** — full access to orders and dispatchers.
- **Dispatcher** — scoped access to their own assigned orders.

---

## Orders

Base route: `/orders`

| Method | Route | Role | Payload | Notes |
|---|---|---|---|---|
| GET | `/orders` | Admin, Dispatcher | Query params: `state?`, `dispatcherId?` | Dispatchers are automatically scoped to their own orders — `dispatcherId` is forced to the caller's own id regardless of what's passed |
| GET | `/orders/{id}` | Admin, Dispatcher | — | Returns 404 if the order doesn't exist |
| POST | `/orders` | Admin | `CreateOrderRequest` | Creates a new order in `Pending` state |
| PUT | `/orders/{id}` | Admin, Dispatcher | `UpdateOrderRequest` | Admins can update any field. Dispatchers may only update the order's `State` (e.g. `Pending → Shipped → Delivered`), and only for orders assigned to them — otherwise the request is rejected |
| DELETE | `/orders/{id}` | Admin | — | Deletes an order |

### `CreateOrderRequest`

| Field | Type | Notes |
|---|---|---|
| `clientName` | string | required |
| `clientPhoneNumber` | string | required |
| `location` | `{ lat, lng }` | required, valid geographic coordinates |
| `address` | string? | optional |
| `zone` | string | required |
| `deliveryDate` | datetime | required, must be in the future |

### `UpdateOrderRequest`

| Field | Type | Notes |
|---|---|---|
| `clientName` | string | admin only |
| `clientPhoneNumber` | string | admin only |
| `location` | `{ lat, lng }` | admin only |
| `address` | string? | admin only |
| `zone` | string | admin only |
| `deliveryDate` | datetime | admin only |
| `state` | string | `Pending` / `Shipped` / `Delivered` — the only field a dispatcher may change |
| `dispatcherId` | guid? | admin only |

---

## Dispatchers

Base route: `/dispatchers`

| Method | Route | Role | Payload | Notes |
|---|---|---|---|---|
| GET | `/dispatchers` | Admin | — | Lists all dispatchers |
| GET | `/dispatchers/{id}` | Admin | — | |
| GET | `/dispatchers/me` | Dispatcher | — | Returns the caller's own dispatcher profile, resolved from their token claims |
| POST | `/dispatchers` | Admin | `CreateDispatcherRequest` | Creates a Firebase user (or links an existing one) with the `dispatcher` role claim, plus the corresponding dispatcher record; returns a password-reset link so the dispatcher can set their own password |
| PUT | `/dispatchers/{id}` | Admin | `UpdateDispatcherRequest` | |
| DELETE | `/dispatchers/{id}` | Admin | — | |

### `CreateDispatcherRequest`

| Field | Type | Notes |
|---|---|---|
| `name` | string | required |
| `email` | string | required, must be unique |
| `vehicle` | string? | optional |
| `licensePlate` | string? | optional |
| `firebaseUid` | string? | optional — if provided, links an existing Firebase user instead of creating a new one |

### `UpdateDispatcherRequest`

| Field | Type | Notes |
|---|---|---|
| `name` | string | optional |
| `email` | string | optional |
| `vehicle` | string? | optional |
| `licensePlate` | string? | optional |
| `state` | string | `Available` / `Inactive` |

---

## Errors

All endpoints share a consistent error envelope, including validation, authorization, and unexpected failures:

```json
{
  "status": 404,
  "error": "NotFound",
  "message": "Order was not found."
}
```

Common status codes:

| Status | Meaning |
|---|---|
| 400 | Validation error or invalid domain state transition |
| 401 | Missing, invalid, or expired authentication token |
| 403 | Authenticated but not authorized for this action/resource |
| 404 | Resource not found |
| 409 | Conflict (e.g. dispatcher not available, duplicate email) |
| 500 | Unexpected server error |
