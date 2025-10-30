# WeSign Digital Signature Platform - Project Overview

## Purpose
WeSign is a comprehensive digital signature platform built on .NET 9.0 that provides document signing, template management, user authentication, and workflow orchestration. It's designed as an enterprise-grade solution for secure document processing and e-signature workflows.

## Core Business Domain
- **Digital Document Signing**: PDF document processing, signature field management, template-based signing
- **User & Group Management**: Multi-tenant user system with group-based access control
- **Document Collections**: Batch document processing and workflow management
- **Certificate Management**: Digital certificate handling and smart card integration
- **Audit & Compliance**: Comprehensive logging and reporting for regulatory compliance

## Key Features
- Multi-application architecture (User Portal, Management Portal, Signer Desktop Client)
- Smart card integration for secure signing
- Template-based document creation
- Real-time SignalR communication
- Background job processing with Hangfire
- External integrations (OCR, PDF services, Active Directory)
- Rate limiting and security middleware
- Comprehensive audit logging

## Technical Characteristics
- Enterprise-grade multi-tenant SaaS platform
- Clean architecture with separated concerns
- Extensive validation and security measures
- Background processing capabilities
- Real-time communication features
- External service integrations