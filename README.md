# SaasAsaasApp

A robust, production-ready Multi-Tenant SaaS application built with .NET 10, PostgreSQL, and integrated with the Asaas payment gateway.

## 🚀 Overview

SaasAsaasApp is designed to provide a secure and scalable foundation for SaaS businesses. It features strict data isolation between tenants, an automated payment lifecycle, and a modern management dashboard.

## 🛠️ Tech Stack

- **Framework:** .NET 10 (ASP.NET Core Razor Pages)
- **Database:** PostgreSQL 17
- **ORM:** Entity Framework Core 10
- **Authentication:** ASP.NET Core Identity & JWT Bearer
- **Infrastructure:** Docker & Docker Compose
- **Payment Gateway:** Asaas (Brazil)

## ✨ Core Features

- **Multi-Tenancy:** Automated data isolation using global query filters.
- **Identity System:** Secure login/registration with tenant-linked identity.
- **Subscription Engine:** Full integration with Asaas API for PIX, Boleto, and Credit Card payments.
- **Security Middleware:** Real-time session revocation for non-active or overdue subscriptions.
- **Business CRUDs:** Generic repository pattern for fast development of business entities.
- **Soft Delete:** Logical deletion across all entities for historical traceability.
- **Modern Dashboard:** Executive overview with real-time business metrics.

## 📦 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 10 SDK (for local development)

### Setup
1. Clone the repository.
2. Configure your Asaas credentials in `appsettings.json`.
3. Run the environment:
   ```bash
   docker-compose up -d
   ```
4. Access the application at `http://localhost:5000`.

## 📄 License

**Proprietary License - All Rights Reserved.**

Copyright (c) 2026 David Fernandez Garzon. Unauthorized use, modification, or redistribution is strictly prohibited. Contact: davidzodelin@gmail.com
