# Hackathon 2025 - Sistema de Simulação de Empréstimos

## 👨‍💻 Criador

**Enzo Suzart Pinto** - **C159438-4** - Desenvolvedor Back-End na Box Gestão Arquivística.

---

## 📚 Sumário

- [📋 Descrição](#-descrição)
- [🤔 Por que usar Native AOT?](#-por-que-usar-native-aot)
- [🚀 Como Executar](#-como-executar)
- [✔️ Características Principais](#-características-principais)
  - [Native AOT (Ahead-of-Time Compilation)](#native-aot-ahead-of-time-compilation)
  - [Padrão Fire-and-Forget](#padrão-fire-and-forget)
  - [Sistema de Telemetria](#sistema-de-telemetria)
  - [Arquitetura Limpa](#arquitetura-limpa)
  - [Resiliência e Tratamento de Erros](#resiliência-e-tratamento-de-erros)
- [🏗️ Estrutura do Projeto](#️-estrutura-do-projeto)
- [🔧 Tecnologias Utilizadas](#-tecnologias-utilizadas)
- [🧰 O que poderia ser implementado? (E porque não implementei)](#-o-que-poderia-ser-implementado-e-porque-não-implementei)
- [⚙️ Configuração e Variáveis de Ambiente](#️-configuração-e-variáveis-de-ambiente)
- [📊 Funcionalidades](#-funcionalidades)
- [🗄️ Banco de Dados](#️-banco-de-dados)
- [🌐 Endpoints da API](#-endpoints-da-api)
- [🧪 Testes](#-testes)

---

## 📋 Descrição

Este projeto é uma API de simulação de empréstimos desenvolvida em .NET 8 utilizando Native AOT com foco em performance e escalabilidade. O sistema permite calcular simulações de financiamento usando diferentes metodologias (SAC e PRICE) e implementa padrões arquiteturais modernos para alta performance.

## 🤔 Por que usar Native AOT?

**Performance Superior**: Aplicações compiladas nativamente executam mais rápido, sem overhead do runtime .NET.

**Menor Consumo de Recursos**: Reduz significativamente o uso de memória e CPU, ideal para ambientes com recursos limitados.

**Deploy Simplificado**: Um único arquivo executável sem dependências externas, facilitando distribuição e implantação.

**Inicialização Instantânea**: Startup em milissegundos, perfeito para funções serverless e microserviços que precisam responder rapidamente.

> **📖 Para mais informações sobre Native AOT, consulte a [documentação oficial](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/native-aot?view=aspnetcore-8.0).**

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- Visual Studio 2022 Build Tools com:
  - Workload: Desktop development with C++
  - Windows 10/11 SDK
- SQL Server
- SQLite
- Docker (opcional)

### Execução Local
```bash
cd hackathon
dotnet restore
dotnet run
```

### Execução com Native AOT (Windows)
```bash
dotnet publish -c Release -r win-x64 -p:PublishAot=true
```

### Execução com Docker (prioritário)
```bash
# Build e execução simples
docker build -t hackathon .
docker run -p 8080:8080 hackathon

# Ou usando Docker Compose (nessa opção, o SQL Server também será criado localmente)
docker-compose up --build
```

> **📖 Para instruções Docker detalhadas, consulte [DOCKER.md](hackathon/DOCKER.md)**

## 🚀 Características Principais

### Native AOT (Ahead-of-Time Compilation)
- **Compilação Nativa**: O projeto utiliza Native AOT para compilação nativa, eliminando a necessidade do runtime .NET
- **Performance Superior**: Execução mais rápida e menor uso de memória
- **Deploy Otimizado**: Binários nativos para diferentes plataformas (Windows, Linux, macOS)
- **Análise AOT**: Habilitado o analisador AOT para garantir compatibilidade
- **Segurança**: Menor superfície de ataque

### Padrão Fire-and-Forget
- **Processamento Assíncrono**: Simulações são processadas em background sem bloquear a resposta da API
- **Channels**: Utiliza `System.Threading.Channels` para comunicação entre threads
- **Background Services**: Serviços em background para persistência de dados
- **Escalabilidade**: Permite processar múltiplas simulações simultaneamente

### Sistema de Telemetria
- **Monitoramento Automático**: Middleware que captura métricas de todos os endpoints
- **Métricas em Tempo Real**: Contagem de requisições, tempo de resposta e taxa de sucesso
- **Análise Histórica**: Consulta dos dados por data
- **Agregação via Channel**: Eventos de telemetria são enfileirados e processados por um único worker (evita concorrência).
- **Flush Inteligente**: Persistência periódica (30 seg) e sob demanda (antes de consultas).
- **UPSERT Idempotente**: Métricas agregadas por dia/endpoint no SQLite, com retry para lidar com locks.

### Arquitetura Limpa
- **Domain-Driven Design**: Separação clara entre domínio, aplicação e infraestrutura
- **Use Cases**: Lógica de negócio encapsulada em casos de uso específicos
- **Repository Pattern**: Abstração para acesso a dados
- **Dependency Injection**: Injeção de dependências para baixo acoplamento

### Resiliência e Tratamento de Erros
- **Fire-and-Forget Seguro**: Uso de extensão `SafeFireAndForget` para logar falhas sem impactar a resposta.
- **Background Services**: Persistência de simulações e telemetria via `System.Threading.Channels` + `BackgroundService`.
- **Retry com Backoff**: Implementado para operações críticas no SQLite.
- **ProblemDetails**: Erros críticos retornam respostas padronizadas (RFC 7807).

## 🏗️ Estrutura do Projeto

```
hackathon/
├── Api/                    # Camada de apresentação
│   ├── Endpoints/          # Endpoints da API
│   ├── Extensions/         # Configurações e extensões
│   ├── Middleware/         # Middleware de telemetria
│   └── Serialization/      # Configurações de serialização JSON
├── Application/            # Camada de aplicação
│   ├── Dtos/               # Objetos de transferência de dados
│   ├── Interfaces/         # Contratos e interfaces
│   └── UseCases/           # Casos de uso da aplicação
├── Domain/                 # Camada de domínio
│   ├── Entities/           # Entidades do domínio
│   └── ValueObjects/       # Objetos de valor
├── Infrastructure/         # Camada de infraestrutura
│   ├── BackgroundServices/ # Serviços em background
│   ├── Config/             # Configurações
│   ├── Events/             # Publicação de eventos
│   ├── Persistence/        # Repositórios e acesso a dados
│   └── Services/           # Serviços de infraestrutura
└── banco/                  # Scripts de banco de dados
```

## 🔧 Tecnologias Utilizadas

- **.NET 8**: Framework principal com suporte a Native AOT
- **Dapper**: Micro ORM para acesso a dados SQL Server
- **Dapper.AOT**: Extensão AOT para Dapper
- **SQLite**: Banco de dados local para desenvolvimento e telemetria
- **Azure Event Hubs**: Mensageria para eventos
- **SQL Server**: Banco de dados relacional principal
- **Docker**: Containerização da aplicação
- **System.Threading.Channels**: Para comunicação assíncrona entre threads
- **Microsoft.Extensions.Logging**: Sistema de logging estruturado

## 🧰 O que poderia ser implementado? (E porque não implementei)

**Swagger UI**: Por utilizar reflexão, o Swagger não é compatível com Native AOT.

**Autenticação JWT**: Embora seja uma camada de segurança quase obrigatória, a adição poderia interferir nos testes a serem feitos pela banca avaliadora.

**Rate Limiting**: Para uma API altamente escalável, definir um rate limit é muito útil para evitar ataques como DDoS, entretanto, para testes de carga acabaria sendo influenciado.

## ⚙️ Configuração e Variáveis de Ambiente

### Arquivo appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "[connection_string_sql_server]"
  },
  "Sqlite": {
    "DatabasePath": "hackathon.db",
    "ConnectionString": "Data Source=hackathon.db;Cache=Shared;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Hackathon": "Information"
    }
  },
  "EventHub": {
    "ConnectionString": "[connection_string_eventhub]"
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://+:8080"
      }
    }
  }
}
```

### Variáveis de Ambiente Principais

| Variável | Descrição | Padrão |
|----------|-----------|---------|
| `ASPNETCORE_ENVIRONMENT` | Ambiente de execução | `Development` |
| `ASPNETCORE_URLS` | URLs do servidor | `http://localhost:5000` |
| `ConnectionStrings__DefaultConnection` | String de conexão SQL Server | - |
| `Sqlite__DatabasePath` | Caminho do arquivo SQLite | `hackathon.db` |
| `EventHub__ConnectionString` | String de conexão Azure Event Hub | - |
| `Logging__LogLevel__Default` | Nível de log padrão | `Warning` |

## 📊 Funcionalidades

### Simulação de Empréstimos
- **Cálculo SAC**: Sistema de Amortização Constante
- **Cálculo PRICE**: Sistema de Prestações Fixas
- **Produtos Flexíveis**: Diferentes faixas de valor e prazo
- **Taxas Personalizadas**: Taxas de juros específicas por produto

### API REST Completa
- **POST `/simulacoes`**: Criação de simulações de empréstimo
- **GET `/simulacoes`**: Listagem paginada de simulações realizadas
- **GET `/produtos`**: Relatório de volume diário por produto
- **GET `/telemetria`**: Métricas de telemetria por data

### Sistema de Telemetria
- **Monitoramento Automático**: Captura automática de métricas de todos os endpoints
- **Métricas de Performance**: Tempo de resposta, contagem de requisições, taxa de sucesso
- **Persistência Otimizada**: Dados são descarregados periodicamente para otimizar memória
- **Consultas Flexíveis**: Análise por data específica ou intervalo de datas

### Processamento em Background
- **Persistência Assíncrona**: Salvamento de simulações sem impacto na performance
- **Fila Interna**: Sistema de filas para gerenciar simulações
- **Tratamento de Erros**: Logs e recuperação de falhas

### Arquitetura de Banco de Dados Híbrido
- **SQL Server**: Utilizado para produtos financeiros (dados externos)
- **SQLite**: Utilizado para simulações e telemetria (dados locais)
- **Conexão Transparente**: Abstração automática do tipo de banco por operação
- **Performance Otimizada**: Cada banco utilizado para seu caso de uso ideal

## 🗄️ Banco de Dados

### Arquitetura Híbrida Detalhada

O sistema utiliza uma arquitetura de banco de dados híbrida otimizada para diferentes tipos de dados:

#### SQL Server (Dados Externos)
- **Propósito**: Produtos financeiros e dados de referência
- **Localização**: Servidor dedicado ou Azure SQL Database
- **Casos de Uso**: Operações críticas e dados compartilhados

##### Tabela PRODUTO
```sql
CREATE TABLE dbo.PRODUTO (
    CO_PRODUTO int NOT NULL PRIMARY KEY,
    NO_PRODUTO varchar(200) NOT NULL,
    PC_TAXA_JUROS numeric(10, 9) NOT NULL,
    NU_MINIMO_MESES smallint NOT NULL,
    NU_MAXIMO_MESES smallint NULL,
    VR_MINIMO numeric(18, 2) NOT NULL,
    VR_MAXIMO numeric(18, 2) NULL
);
```

#### SQLite (Dados Locais)
- **Propósito**: Simulações, telemetria e dados de processamento
- **Localização**: Arquivo local `hackathon.db`
- **Casos de Uso**: Dados temporários e métricas

##### Tabela SIMULACAO
```sql
CREATE TABLE Simulacao (
    Id TEXT PRIMARY KEY,
    ValorDesejado REAL NOT NULL,
    CodigoProduto INTEGER NOT NULL,
    DescricaoProduto TEXT NOT NULL,
    TaxaJuros REAL NOT NULL,
    CriadoEm TEXT NOT NULL,
    SimulacaoSac TEXT NOT NULL,
    SimulacaoPrice TEXT NOT NULL
);
```

##### Tabela TELEMETRIA
```sql
CREATE TABLE Telemetria (
    Id TEXT PRIMARY KEY,
    DataReferencia TEXT NOT NULL,
    NomeApi TEXT NOT NULL,
    QtdRequisicoes INTEGER NOT NULL,
    TempoMedio INTEGER NOT NULL,
    TempoMinimo INTEGER NOT NULL,
    TempoMaximo INTEGER NOT NULL,
    PercentualSucesso NUMERIC(5,4) NOT NULL,
    CriadoEm TEXT NOT NULL
);
```

### Estratégia de Dados

#### Produtos Financeiros
- **Armazenamento**: SQL Server (fonte externa de verdade)
- **Atualização**: Manual ou através de processos ETL
- **Cache**: Implementado em memória para performance

#### Simulações
- **Armazenamento**: SQLite (dados temporários/transacionais)
- **Persistência**: Assíncrona via background services
- **Retenção**: Configurável (padrão: indefinida)

#### Telemetria
- **Armazenamento**: SQLite (dados analíticos locais)
- **Agregação**: A cada 30 segundos em memória
- **Flush**: A cada 5 minutos para o banco
- **Retenção**: Configurável por requisitos de análise

## 🌐 Endpoints da API

### Simulações
#### POST `/simulacoes`
Cria uma nova simulação de empréstimo.

**Request:**
```json
{
  "valorDesejado": 50000.00,
  "prazo": 36
}
```

**Response:**
```json
{
  "idSimulacao": 12345,
  "codigoProduto": 2,
  "descricaoProduto": "Produto 2",
  "taxaJuros": 0.0175,
  "sac": {
    "tipo": "SAC",
    "parcelas": [...]
  },
  "price": {
    "tipo": "PRICE",
    "parcelas": [...]
  }
}
```

#### GET `/simulacoes`
Lista simulações realizadas com paginação.

**Query Parameters:**
- `pagina` (opcional): Número da página (padrão: 1)
- `qtdRegistrosPagina` (opcional): Registros por página (padrão: 200)
- `sistema` (opcional): Sistema de amortização (padrão: "PRICE")

**Response:**
```json
{
  "pagina": 1,
  "qtdRegistros": 150,
  "qtdRegistrosPagina": 200,
  "registros": [
    {
      "idSimulacao": 12345,
      "valorDesejado": 50000.00,
      "prazo": 36,
      "valorTotalParcelas": 65000.00
    }
  ]
}
```

### Produtos e Volume
#### GET `/produtos`
Obtém relatório de volume diário por produto.

**Query Parameters:**
- `dataReferencia` (obrigatório): Data de referência no formato YYYY-MM-DD
- `sistema` (opcional): Sistema de amortização (padrão: "PRICE")

**Response:**
```json
{
  "dataReferencia": "2025-01-27",
  "simulacoes": [
    {
      "codigoProduto": 1,
      "descricaoProduto": "Produto 1",
      "taxaMediaJuro": 0.0150,
      "valorMedioPrestacao": 1500.00,
      "valorTotalDesejado": 50000.00,
      "valorTotalCredito": 54000.00
    }
  ]
}
```

### Telemetria
#### GET `/telemetria`
Obtém métricas de telemetria para uma data específica.

**Query Parameters:**
- `dataReferencia` (opcional): Data de referência (padrão: data atual)

**Response:**
```json
{
  "dataReferencia": "2025-01-27",
  "listaEndpoints": [
    {
      "nomeApi": "POST /simulacao",
      "qtdRequisicoes": 150,
      "tempoMedio": 45,
      "tempoMinimo": 12,
      "tempoMaximo": 120,
      "percentualSucesso": 98.5
    }
  ]
}
```

## 🧪 Testes

O projeto inclui uma suíte de testes automatizados para garantir qualidade e confiabilidade.

### Estrutura de Testes

```
hackathon.Tests/
├── Services/               # Testes de serviços
│   └── TelemetriaServiceTests.cs
├── UseCases/              # Testes de casos de uso
│   ├── ObterSimulacoesUseCaseTests.cs
│   ├── ObterVolumeDiarioUseCaseTests.cs
│   └── SimularEmprestimoUseCaseTests.cs
└── GlobalUsings.cs        # Configurações globais de teste
```

### Execução de Testes

#### Executar Todos os Testes
```bash
cd hackathon.Tests
dotnet test
```

#### Executar Testes Específicos
```bash
# Testes de um caso de uso específico
dotnet test --filter "ObterSimulacoesUseCase"

# Testes de uma classe específica
dotnet test --filter "SimularEmprestimoUseCaseTests"
```

### Tipos de Teste Implementados

#### Testes Unitários
- **Use Cases**: Lógica de negócio isolada
- **Services**: Funcionalidades de infraestrutura
- **Mocks**: Utilização de Moq para dependências externas

#### Cobertura de Teste
- **Casos de Uso**: 100% cobertura dos fluxos principais
- **Serviços**: Testes de integração com dependências
- **Endpoints**: Testes de contrato da API

### Cenários de Teste Principais

#### SimularEmprestimoUseCaseTests
- ✅ Simulação válida com produto compatível
- ✅ Erro quando produto não encontrado
- ✅ Erro quando valor/prédio fora da faixa
- ✅ Validação de parâmetros de entrada

#### ObterSimulacoesUseCaseTests
- ✅ Listagem paginada com sucesso
- ✅ Filtros por sistema de amortização
- ✅ Tratamento de página vazia

#### ObterVolumeDiarioUseCaseTests
- ✅ Cálculo correto de volume por produto
- ✅ Agregação por data específica
- ✅ Tratamento de dados vazios

#### TelemetriaServiceTests
- ✅ Registro de métricas de performance
- ✅ Agregação automática de dados
- ✅ Persistência em lote

### Boas Práticas de Teste

- **Isolamento**: Cada teste é independente
- **Mocks**: Utilização de interfaces para isolamento
- **Dados de Teste**: Fixtures com dados realistas
- **Assertividade**: Verificações específicas e claras
- **Performance**: Testes executados rapidamente

### Integração Contínua

Os testes são executados automaticamente em:
- **Build Local**: `dotnet build`
- **Pull Requests**: Via GitHub Actions
- **Deploy**: Antes de cada release


### Boas Práticas Implementadas
- **Clean Architecture**: Separação clara entre camadas (Domain, Application, Infrastructure, API).
- **Use Cases**: Lógica de negócio isolada.
- **Repository Pattern**: Abstração para persistência.
- **Dependency Injection**: Baixo acoplamento.
- **Native AOT Ready**: Uso de `System.Text.Json` com Source Generators e eliminação de reflection.

### Ferramentas de Diagnóstico

#### Health Check Endpoint
```bash
# Verificar saúde da aplicação
curl http://localhost:8080/health

# Resposta esperada
{"status":"Healthy","checks":{"database":"Healthy","eventhub":"Healthy"}}
```

#### Métricas de Performance
```bash
# Ver métricas atuais
curl http://localhost:8080/telemetria

# Métricas detalhadas
curl http://localhost:8080/telemetria | jq '.'
```

### Suporte e Recursos Adicionais

#### Documentação Específica
- 📖 **[BANCO_HIBRIDO.md](hackathon/BANCO_HIBRIDO.md)**: Detalhes da arquitetura de banco híbrido
- 📖 **[TELEMETRIA.md](hackathon/TELEMETRIA.md)**: Sistema de telemetria detalhado
- 📖 **[DOCKER.md](hackathon/DOCKER.md)**: Configurações Docker avançadas

#### Recursos Externos
- 🔗 [Documentação .NET 8](https://learn.microsoft.com/pt-br/dotnet/core/whats-new/dotnet-8)
- 🔗 [Native AOT Guide](https://learn.microsoft.com/pt-br/dotnet/core/deploying/native-aot)
- 🔗 [SQLite Documentation](https://www.sqlite.org/docs.html)
- 🔗 [Dapper Documentation](https://dapper-tutorial.net/)