# Student Information System (SIS)

A web‑based platform for managing student data — from secure user authentication and course enrollment to analytics dashboards. Built with **ASP.NET Web Forms (VB.NET)** and **PostgreSQL** using Supabase.

This project was created for the course "Skills: Programming with Generative AI".

---

## Features

| Module                | Highlights                                                              |
| --------------------- | ----------------------------------------------------------------------- |
| **Authentication**    | BCrypt‑hashed passwords, role‑based redirects                           |
| **Register / Login**  | Student self‑registration + admin login with duplicate‑email guard      |
| **ManageStudents**    | CRUD operations for students                                            |
| **ManageCourses**     | Create or remove courses                                                |
| **ManageEnrollments** | Add / drop students                                                     |
| **AvailableCourses**  | Student‑facing catalog with filter & enroll buttons                     |
| **Reports**           | enrollment counts, charts, CSV/PDF export                               |

---

## Tech Stack

* **ASP.NET Web Forms** (VB.NET)
* **PostgreSQL 15** + **Npgsql** driver
* **Bootstrap 5** UI
* **BCrypt.Net** for password hashing
* **Chart.js** for charts 

---

## Quick Start (Dev)

```bash
# 1. Clone
$ git clone https://github.com/kaebry/Student-Information-System.git
$ cd Student-Information-System

# 2. Restore packages & run
# Visual Studio: F5
# CLI:
$ dotnet restore
$ dotnet run --project StudentInformationSystem
```

---

## Database Schema

```
students      (id PK, first_name, last_name, email, enrollment_date)
users         (id PK, email, password_hash, role, student_id FK)
courses       (course_id PK, course_name, ects, hours, format, instructor)
enrollments   (enrollment_id PK, student_id FK, course_id FK, enrollment_date)
```


