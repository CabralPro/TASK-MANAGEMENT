# User Story: Task Management Application

**Title:** Manage Personal Tasks with Authenticated User Access

---

### **As a**

Registered User

### **I want to**

Securely log in to my task management account so that I can create, view, update, and delete my personal tasks with target due dates and completion statuses.

### **So that**

I can organize my daily workload, track task progress efficiently, and ensure my data is kept private from unauthenticated users.

---

### **Description & Scope**

The application must provide a secure, full-stack task management system. It includes a backend RESTful Web API using Clean Architecture principles and Test-Driven Development (TDD), coupled with a responsive Angular SPA.

* **Domain Entity (Task):** `Id` (GUID/Primary Key), `Title` (string), `Description` (string), `Status` (enum/string: e.g., Pending, In Progress, Completed), `DueDate` (DateTime), and `UserId` (Foreign Key).


* **Domain Entity (User):** `Id` (GUID/Primary Key), `Username` (string), `Email` (string), `PasswordHash` (string).



---

### **Acceptance Criteria**

#### **1. Authentication & User Access**

* **AC 1.1:** A user can register a new account with a unique username/email and a secure password.


* **AC 1.2:** A user can log in with valid credentials and receive an authentication token (JWT).


* **AC 1.3:** Unauthenticated users attempting to access protected API endpoints or frontend routes are rejected with an HTTP `401 Unauthorized` status.


* **AC 1.4:** The application includes seeded demo user credentials in the database for presentation purposes.



#### **2. Task Management (CRUD)**

* **AC 2.1 (Create):** Authenticated users can create a new task by providing a Title, Description, Status, and Due Date.


* **AC 2.2 (Read):** Authenticated users can view a list of all their tasks and select a specific task to view its detailed parameters.


* **AC 2.3 (Update):** Authenticated users can edit an existing task's Title, Description, Status, or Due Date.


* **AC 2.4 (Delete):** Authenticated users can permanently remove a task from their list.


* **AC 2.5 (Isolation):** Users can only access and modify tasks linked to their own `UserId`.



#### **3. Business Logic & Validations**

* **AC 3.1:** `Title` is required and cannot exceed 100 characters.


* **AC 3.2:** `Status` must be a valid predefined state (*Pending*, *InProgress*, *Completed*).


* **AC 3.3:** Business logic and validation rules are contained entirely within the Business Logic / Core Layer, maintaining full independence from the Data Access and API layers.



#### **4. Technical & Quality Standards**

* **AC 4.1 Architecture:** Solution follows Clean Architecture (Domain, Application/Business Logic, Infrastructure/Data, and Web API/UI layers).


* **AC 4.2 Automated Testing:** Unit test suites cover the Data Access Layer, Business Logic Layer, and API endpoints using TDD methodologies.


* **AC 4.3 Frontend Integration:** The UI is responsive, user-friendly, clean, and handles loading and error states without browser console errors.


* **AC 4.4 Documentation:** A `README.md` file contains setup instructions, database migration steps, test execution commands, and AI workflow notes.



---

### **Definition of Done (DoD)**

| Requirement Area | Criteria |
| --- | --- |
| **Backend & Architecture** | Clean Architecture solution set up; REST API exposes CRUD endpoints for Tasks and Authentication endpoints for Users.

 |
| **Database** | Database configured with EF Core migrations for `Users` and `Tasks` tables, including initial seed data.

 |
| **Testing** | Unit tests passing across Data Access, Business Logic, and API layers using TDD practices.

 |
| **Frontend** | SPA integrated with API, allowing seamless login and Task CRUD operations.

 |
| **Generative AI** | Prompting strategies, validation, edge cases, and code corrections documented for presentation.

 |