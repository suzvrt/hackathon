-- Garante que estamos executando os comandos no contexto do banco de dados principal (master)
-- para poder criar um novo banco de dados.
USE master;
GO

-- 1. Cria o banco de dados 'hack' se ele ainda não existir.
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'hack')
BEGIN
    CREATE DATABASE hack;
END
GO

-- 2. Muda o contexto para o banco de dados 'hack' que acabamos de criar.
-- Todos os comandos seguintes (CREATE TABLE, etc.) serão executados dentro deste banco.
USE hack;
GO

-- 3. Cria um LOGIN a nível de servidor. O LOGIN é o que permite a autenticação no SQL Server.
IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'hack')
BEGIN
    CREATE LOGIN hack WITH PASSWORD = 'Password23';
END
GO

-- 4. Cria um USER dentro do banco de dados 'hack' e o associa ao LOGIN 'hack'.
-- O USER é o que tem permissões dentro de um banco de dados específico.
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'hack')
BEGIN
    CREATE USER hack FOR LOGIN hack;
END
GO

-- 5. Concede ao usuário 'hack' as permissões de dono do banco de dados ('db_owner').
-- Isso dá a ele permissão total para criar tabelas, inserir dados, etc.
ALTER ROLE db_owner ADD MEMBER hack;
GO

-- ======================================================================
-- A PARTIR DAQUI, SEU SCRIPT ORIGINAL CONTINUA NORMALMENTE
-- ======================================================================

-- Cria a tabela de produtos, se ela não existir
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PRODUTO' and xtype='U')
BEGIN
    CREATE TABLE dbo.PRODUTO (
        CO_PRODUTO int NOT NULL primary key,
        NO_PRODUTO varchar(200) NOT NULL,
        PC_TAXA_JUROS numeric(10, 9) NOT NULL,
        NU_MINIMO_MESES smallint NOT NULL,
        NU_MAXIMO_MESES smallint NULL,
        VR_MINIMO numeric(18, 2) NOT NULL,
        VR_MAXIMO numeric(18, 2) NULL
    );
END
GO

-- Insere os dados (opcionalmente, pode-se verificar se já existem para evitar duplicatas)
-- Para simplificar, vamos assumir que a tabela está vazia.
INSERT INTO dbo.PRODUTO (CO_PRODUTO, NO_PRODUTO, PC_TAXA_JUROS,
NU_MINIMO_MESES, NU_MAXIMO_MESES, VR_MINIMO, VR_MAXIMO)
VALUES (1, 'Produto 1', 0.017900000, 0, 24, 200.00, 10000.00)

INSERT INTO dbo.PRODUTO (CO_PRODUTO, NO_PRODUTO, PC_TAXA_JUROS,
NU_MINIMO_MESES, NU_MAXIMO_MESES, VR_MINIMO, VR_MAXIMO)
VALUES (2, 'Produto 2', 0.017500000, 25, 48, 10001.00, 100000.00)

INSERT INTO dbo.PRODUTO (CO_PRODUTO, NO_PRODUTO, PC_TAXA_JUROS,
NU_MINIMO_MESES, NU_MAXIMO_MESES, VR_MINIMO, VR_MAXIMO)
VALUES (3, 'Produto 3', 0.018200000, 49, 96, 100000.01, 1000000.00)

INSERT INTO dbo.PRODUTO (CO_PRODUTO, NO_PRODUTO, PC_TAXA_JUROS,
NU_MINIMO_MESES, NU_MAXIMO_MESES, VR_MINIMO, VR_MAXIMO)
VALUES (4, 'Produto 4', 0.015100000, 96, null, 1000000.01, null)
GO