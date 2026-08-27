# TaskManagement.WebUI

Angular 22 SPA for personal task management (JWT auth + task CRUD).

For full monorepo setup, demo credentials, and API docs, see the [root README](../../README.md).

## Companion API

.NET projects are siblings under `src/` — default URL `http://localhost:5000` (configured in `src/environments/environment.ts`).

The SPA expects the API to be running. Prefer `npm start` from the repo root (API + Angular), or start the API first with `npm run start:api`.

## Features

- Register and sign in (JWT in `localStorage`)
- Protected `/tasks` route with create / edit / delete
- Task fields: title, description, status (`Pending` | `InProgress` | `Completed`), due date
- Standalone components, signals, Material UI

## Setup

From repo root:

```bash
npm run install:all
npm run start:web
```

Or:

```bash
cd src/TaskManagement.WebUI
npm install
npm start
```

App: **http://localhost:4200**.

## Tests

```bash
npm --prefix src/TaskManagement.WebUI test -- --watch=false
npm --prefix src/TaskManagement.WebUI run test:coverage
```

Coverage report: `coverage/task-management/index.html`.

## Structure

```
src/app/
  core/              # API client, auth, tasks services, guards, interceptor
  features/auth/     # Login, register
  features/tasks/
    tasks-page/          # Task list + filter
    task-form-dialog/    # Create/edit dialog
  shared/layout/     # App shell
  shared/ui/         # Confirm dialog
```
