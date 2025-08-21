# Começamos com a imagem oficial do SQL Server 2022 (baseada em Ubuntu 22.04)
FROM mcr.microsoft.com/mssql/server:2022-latest

# NOVA ALTERAÇÃO (BOA PRÁTICA): Garante que as instalações não peçam nenhuma entrada do usuário
ENV DEBIAN_FRONTEND=noninteractive

# Trocamos para o usuário 'root' para poder instalar pacotes
USER root

# Instalamos as dependências e as ferramentas de linha de comando do SQL Server (mssql-tools)
RUN apt-get update && \
    apt-get install -y curl apt-transport-https gnupg && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    # NOVA ALTERAÇÃO (A CORREÇÃO PRINCIPAL): Usando o repositório correto para Ubuntu 22.04
    curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev && \
    # Limpamos o cache para manter a imagem menor
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Voltamos para o usuário padrão do SQL Server
USER mssql