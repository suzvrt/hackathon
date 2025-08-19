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

### Arquitetura Limpa
- **Domain-Driven Design**: Separação clara entre domínio, aplicação e infraestrutura
- **Use Cases**: Lógica de negócio encapsulada em casos de uso específicos
- **Repository Pattern**: Abstração para acesso a dados
- **Dependency Injection**: Injeção de dependências para baixo acoplamento

## 🏗️ Estrutura do Projeto

```
hackathon/
├── Api/                    # Camada de apresentação
│   ├── Endpoints/         # Endpoints da API
│   ├── Extensions/        # Configurações e extensões
│   └── Serialization/     # Configurações de serialização JSON
├── Application/           # Camada de aplicação
│   ├── Dto/              # Objetos de transferência de dados
│   ├── Interfaces/       # Contratos e interfaces
│   └── UseCases/         # Casos de uso da aplicação
├── Domain/               # Camada de domínio
│   ├── Entities/         # Entidades do domínio
│   └── ValueObjects/     # Objetos de valor
├── Infrastructure/       # Camada de infraestrutura
│   ├── BackgroundServices/ # Serviços em background
│   ├── Config/           # Configurações
│   ├── Events/           # Publicação de eventos
│   └── Persistence/      # Repositórios e acesso a dados
└── banco/                # Scripts de banco de dados
```

## 🔧 Tecnologias Utilizadas

- **.NET 8**: Framework principal com suporte a Native AOT
- **Dapper**: Micro ORM para acesso a dados SQL Server
- **Dapper.AOT**: Extensão AOT para Dapper
- **Azure Event Hubs**: Mensageria para eventos
- **SQL Server**: Banco de dados relacional
- **Docker**: Containerização da aplicação

## 📊 Funcionalidades

### Simulação de Empréstimos
- **Cálculo SAC**: Sistema de Amortização Constante
- **Cálculo PRICE**: Sistema de Prestações Fixas
- **Produtos Flexíveis**: Diferentes faixas de valor e prazo
- **Taxas Personalizadas**: Taxas de juros específicas por produto

### API REST
- **Endpoint POST**: `/simulacoes` para criação de simulações
- **Validação**: Verificação de compatibilidade de produtos
- **Resposta Rápida**: Retorno imediato com ID da simulação

### Processamento em Background
- **Persistência Assíncrona**: Salvamento de simulações sem impacto na performance
- **Fila Interna**: Sistema de filas para gerenciar simulações
- **Tratamento de Erros**: Logs e recuperação de falhas

## 🗄️ Banco de Dados

### Tabelas
- **PRODUTO**: Cadastro de produtos financeiros com faixas de valor e prazo
- **SIMULACAO**: Histórico de simulações realizadas

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- SQL Server
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

## 🔄 Padrão Fire-and-Forget

- **Resposta Imediata**: API retorna rapidamente sem esperar persistência
- **Processamento Assíncrono**: Simulações são salvas em background
- **Escalabilidade**: Suporte a múltiplas requisições simultâneas
- **Resiliência**: Recuperação automática de falhas

## 📝 Exemplo de Uso

### Request
```json
POST /simulacoes
{
  "valorDesejado": 50000.00,
  "prazo": 36
}
```

### Response
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

## 🤝 Contribuição

Este projeto foi desenvolvido como parte de um hackathon, demonstrando:
- Arquitetura moderna com .NET 8
- Implementação de padrões de alta performance
- Uso de tecnologias cloud-ready
- Boas práticas de desenvolvimento

## 📄 Licença

Projeto desenvolvido para fins educacionais e de demonstração.