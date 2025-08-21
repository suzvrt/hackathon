# Hackathon - Sistema de Simulação de Empréstimos

## 📋 Descrição

Este projeto é uma API de simulação de empréstimos desenvolvida em .NET 8 com foco em performance e escalabilidade. O sistema permite calcular simulações de financiamento usando diferentes metodologias (SAC e PRICE) e implementa padrões arquiteturais modernos para alta performance.

## 🚀 Características Principais

### Native AOT (Ahead-of-Time Compilation)
- **Compilação Nativa**: O projeto utiliza Native AOT para compilação nativa, eliminando a necessidade do runtime .NET
- **Performance Superior**: Execução mais rápida e menor uso de memória
- **Deploy Otimizado**: Binários nativos para diferentes plataformas (Windows, Linux, macOS)
- **Análise AOT**: Habilitado o analisador AOT para garantir compatibilidade

### Padrão Fire-and-Forget
- **Processamento Assíncrono**: Simulações são processadas em background sem bloquear a resposta da API
- **Channels**: Utiliza `System.Threading.Channels` para comunicação entre threads
- **Background Services**: Serviços em background para persistência de dados
- **Escalabilidade**: Permite processar múltiplas simulações simultaneamente

### Sistema de Telemetria
- **Monitoramento Automático**: Middleware que captura métricas de todos os endpoints
- **Métricas em Tempo Real**: Contagem de requisições, tempo de resposta e taxa de sucesso
- **Persistência Inteligente**: Dados são descarregados a cada 5 minutos para otimizar memória
- **Análise Histórica**: Consultas por data

### Arquitetura Limpa
- **Domain-Driven Design**: Separação clara entre domínio, aplicação e infraestrutura
- **Use Cases**: Lógica de negócio encapsulada em casos de uso específicos
- **Repository Pattern**: Abstração para acesso a dados
- **Dependency Injection**: Injeção de dependências para baixo acoplamento

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

## 🗄️ Banco de Dados

### Tabelas
- **PRODUTO**: Cadastro de produtos financeiros com faixas de valor e prazo
- **SIMULACAO**: Histórico de simulações realizadas
- **TELEMETRIA**: Métricas de performance dos endpoints

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
      "nomeApi": "Simulacao",
      "qtdRequisicoes": 150,
      "tempoMedio": 45,
      "tempoMinimo": 12,
      "tempoMaximo": 120,
      "percentualSucesso": 98.5
    }
  ]
}
```

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- SQL Server (para produção)
- SQLite (para desenvolvimento local)
- Docker (opcional)

### Execução Local
```bash
cd hackathon
dotnet restore
dotnet run
```

### Execução com Native AOT
```bash
dotnet publish -c Release -r win-x64 -p:PublishAot=true
```

### Execução com Docker
```bash
# Build e execução simples
docker build -t hackathon .
docker run -p 8080:8080 hackathon

# Ou usando Docker Compose (recomendado)
docker-compose up --build
```

> **📖 Para instruções Docker detalhadas, consulte [DOCKER.md](hackathon/DOCKER.md)**

## 📈 Benefícios do Native AOT

- **Inicialização Rápida**: Startup em milissegundos
- **Menor Memória**: Redução significativa no uso de RAM
- **Deploy Simples**: Binário único sem dependências
- **Performance**: Execução nativa sem interpretação
- **Segurança**: Menor superfície de ataque

## 🤔 Por que usar Native AOT?

**Performance Superior**: Aplicações compiladas nativamente executam mais rápido, sem overhead do runtime .NET.

**Menor Consumo de Recursos**: Reduz significativamente o uso de memória e CPU, ideal para ambientes com recursos limitados.

**Deploy Simplificado**: Um único arquivo executável sem dependências externas, facilitando distribuição e implantação.

**Inicialização Instantânea**: Startup em milissegundos, perfeito para funções serverless e microserviços que precisam responder rapidamente.

## 🔄 Padrão Fire-and-Forget

- **Resposta Imediata**: API retorna rapidamente sem esperar persistência
- **Processamento Assíncrono**: Simulações são salvas em background
- **Escalabilidade**: Suporte a múltiplas requisições simultâneas
- **Resiliência**: Recuperação automática de falhas

## 📊 Sistema de Telemetria

- **Monitoramento Automático**: Middleware captura métricas de todos os endpoints
- **Métricas em Tempo Real**: Contagem de requisições, tempo de resposta e taxa de sucesso
- **Persistência Inteligente**: Dados são descarregados a cada 5 minutos para otimizar memória
- **Performance**: Não impacta a performance da aplicação principal

## 📝 Exemplos de Uso

### Testando a API
O projeto inclui um arquivo `hackathon.http` com exemplos de todas as requisições disponíveis para testar a API.

### Monitoramento de Performance
```bash
# Ver métricas do dia atual
GET /telemetria

# Ver métricas de uma data específica
GET /telemetria?dataReferencia=2025-01-27
```

## 🤝 Contribuição

Este projeto foi desenvolvido como parte de um hackathon, demonstrando:
- Arquitetura moderna com .NET 8
- Implementação de padrões de alta performance
- Sistema de telemetria avançado
- Uso de tecnologias cloud-ready
- Boas práticas de desenvolvimento

## 📄 Licença

Projeto desenvolvido para fins educacionais e de demonstração.
