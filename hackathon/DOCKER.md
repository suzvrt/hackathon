# 🐳 Docker - Hackathon

Este documento contém instruções detalhadas para executar o projeto Hackathon usando Docker.

## 📋 Pré-requisitos

- Docker Desktop instalado e executando
- Docker Compose disponível
- Mínimo 4GB de RAM disponível para o SQL Server

## 🚀 Execução Rápida

### 1. Build e Execução Completa
```bash
# Na pasta raiz do projeto
docker-compose up --build
```

### 2. Execução em Background
```bash
docker-compose up -d --build
```

### 3. Visualizar Logs
```bash
# Todos os serviços
docker-compose logs -f

# Apenas a aplicação
docker-compose logs -f hackathon

# Apenas o banco
docker-compose logs -f db
```

## 🏗️ Estrutura dos Serviços

### hackathon (Aplicação Principal)
- **Porta**: 8080
- **Imagem**: Build local com Native AOT
- **Dependências**: SQL Server, Event Hub
- **Health Check**: `/health` endpoint

### db (SQL Server)
- **Porta**: 1433
- **Imagem**: SQL Server 2022 Developer
- **Dados**: Volume persistente
- **Inicialização**: Script SQL automático

### azurite (Azure Storage Emulator)
- **Portas**: 10000, 10001, 10002
- **Propósito**: Emulação de serviços Azure para desenvolvimento

## ⚙️ Configurações

### Variáveis de Ambiente
```yaml
# Aplicação
ASPNETCORE_ENVIRONMENT: Development
ASPNETCORE_URLS: http://+:8080

# Banco de Dados
ConnectionStrings__DefaultConnection: Server=db;Database=hack;User Id=hack;Password=Password23;Encrypt=False;TrustServerCertificate=True;

# Event Hub
EventHub__ConnectionString: [sua_connection_string]
```

### Volumes
- `sqlserver_data`: Dados persistentes do SQL Server
- `./banco`: Scripts de inicialização do banco

## 🔧 Comandos Úteis

### Desenvolvimento
```bash
# Rebuild apenas da aplicação
docker-compose build hackathon

# Restart apenas da aplicação
docker-compose restart hackathon

# Executar comandos dentro do container
docker-compose exec hackathon /bin/bash
```

### Manutenção
```bash
# Parar todos os serviços
docker-compose down

# Parar e remover volumes
docker-compose down -v

# Limpar imagens não utilizadas
docker system prune -a

# Verificar uso de recursos
docker stats
```

### Debug
```bash
# Verificar status dos serviços
docker-compose ps

# Verificar logs de erro
docker-compose logs --tail=100 hackathon

# Verificar conectividade entre serviços
docker-compose exec hackathon ping db
```

## 🐛 Troubleshooting

### Problemas Comuns

#### 1. Porta 8080 já em uso
```bash
# Alterar porta no docker-compose.yml
ports:
  - "8081:8080"  # Mapeia porta 8081 do host para 8080 do container
```

#### 2. SQL Server não inicializa
```bash
# Verificar logs do SQL Server
docker-compose logs db

# Verificar se a porta 1433 está livre
netstat -an | findstr 1433

# Nota: O healthcheck foi corrigido com parâmetro -C para confiar no certificado
# Se persistir, verifique se o container tem recursos suficientes (mínimo 4GB RAM)
```

#### 3. Aplicação não conecta ao banco
```bash
# Verificar se o banco está rodando
docker-compose ps db

# Testar conectividade
docker-compose exec hackathon ping db
```

#### 4. Native AOT falha no build
```bash
# Verificar se o Docker tem recursos suficientes
# Mínimo: 4GB RAM, 2 CPUs

# Limpar cache do Docker
docker system prune -a
```

## 📊 Monitoramento

### Health Checks
- **Aplicação**: `http://localhost:8080/health`
- **Intervalo**: 30 segundos
- **Timeout**: 3 segundos
- **Retry**: 3 tentativas

### Métricas
```bash
# Uso de recursos em tempo real
docker stats

# Uso de disco
docker system df

# Logs de performance
docker-compose logs hackathon | grep "Performance"
```

## 🔒 Segurança

### Usuário Não-Root
- A aplicação roda como usuário `appuser`
- Permissões mínimas necessárias
- Isolamento de rede

### Volumes
- Dados do banco em volume nomeado
- Scripts SQL em volume somente leitura
- Sem exposição de código fonte

## 🚀 Produção

### Build Otimizado
```bash
# Build para produção
docker build -t hackathon:prod --target production .

# Execução em produção
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  hackathon:prod
```

### Variáveis de Produção
```bash
# Substituir por valores reais
export EVENTHUB_CONNECTION_STRING="[produção]"
export DB_CONNECTION_STRING="[produção]"
export ASPNETCORE_ENVIRONMENT="Production"
```

## 📚 Recursos Adicionais

- [Docker .NET Native AOT](https://docs.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [SQL Server Docker](https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker)
- [Docker Compose](https://docs.docker.com/compose/)
- [Azure Event Hubs](https://docs.microsoft.com/en-us/azure/event-hubs/)
