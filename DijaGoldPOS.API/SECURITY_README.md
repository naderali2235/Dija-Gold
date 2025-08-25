# Security Hardening Guide

This document outlines the security measures implemented for the DijaGold POS API and provides guidance for secure deployment.

## Environment Variables Configuration

The application now supports configuration through environment variables for enhanced security. Sensitive information should never be stored in configuration files that might be committed to version control.

### Required Environment Variables

Copy `env-example.txt` to `.env` and configure the following variables:

#### Database Configuration
- `DB_SERVER`: Database server hostname/IP
- `DB_NAME`: Database name
- `DB_USER`: Database username
- `DB_PASSWORD`: Database password

#### JWT Configuration
- `JWT_SECRET_KEY`: Strong secret key for JWT token signing (minimum 32 characters)
- `JWT_ISSUER`: JWT token issuer
- `JWT_AUDIENCE`: JWT token audience

#### Company Information
- `COMPANY_NAME`: Company name for receipts
- `COMPANY_ADDRESS`: Company address
- `COMPANY_PHONE`: Company phone number
- `TAX_REGISTRATION_NUMBER`: Tax registration number
- `COMMERCIAL_REGISTRATION_NUMBER`: Commercial registration number

#### Network Configuration
- `CORS_ALLOWED_ORIGINS`: Comma-separated list of allowed CORS origins
- `ALLOWED_HOSTS`: Comma-separated list of allowed hostnames

## Security Best Practices

### 1. JWT Token Security
- Use strong, randomly generated secret keys
- Rotate keys regularly (recommended: every 30-90 days)
- Set appropriate token expiration times
- Store secret keys securely (environment variables, Azure Key Vault, AWS Secrets Manager)

### 2. Database Security
- Use strong passwords for database accounts
- Implement connection string encryption
- Enable SSL/TLS for database connections
- Use least-privilege database accounts

### 3. Network Security
- Configure CORS properly for production environments
- Use HTTPS in production
- Implement rate limiting
- Set up proper firewall rules

### 4. Application Security
- Keep dependencies updated
- Use security headers middleware
- Implement proper input validation
- Enable comprehensive logging

## Production Deployment Checklist

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Configure all required environment variables
- [ ] Enable HTTPS
- [ ] Set up proper logging and monitoring
- [ ] Configure health checks
- [ ] Set up backup and recovery procedures
- [ ] Implement proper error handling (avoid exposing sensitive information)
- [ ] Configure rate limiting
- [ ] Set up security monitoring and alerting

## Monitoring and Alerting

The application includes comprehensive health checks that monitor:
- Database connectivity
- Database migrations status
- System resource usage
- Business logic integrity

Set up monitoring for:
- Failed health checks
- Authentication failures
- Database connection issues
- High memory/CPU usage

## Configuration Files

- `appsettings.json`: Development configuration (keep secure values out)
- `appsettings.Production.json`: Production configuration using environment variables
- `env-example.txt`: Template for environment variables
- `.env`: Local environment variables (never commit to version control)

## Security Headers

Consider implementing the following security headers:
- X-Frame-Options: DENY
- X-Content-Type-Options: nosniff
- X-XSS-Protection: 1; mode=block
- Strict-Transport-Security: max-age=31536000; includeSubDomains

## Regular Security Audits

Perform regular security audits including:
- Dependency vulnerability scanning
- Code security review
- Penetration testing
- Configuration review
- Access control audit
