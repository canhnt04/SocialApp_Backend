-- init-databases.sql
-- Creates all required databases for SocialApp on SQL Server.
-- Executed by the sqlserver-init sidecar container on first run.

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'social_app_auth_db')
    CREATE DATABASE social_app_auth_db;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'social_app_user_db')
    CREATE DATABASE social_app_user_db;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'social_app_chat_db')
    CREATE DATABASE social_app_chat_db;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'social_app_post_db')
    CREATE DATABASE social_app_post_db;
GO

PRINT 'All databases created successfully.';
GO
