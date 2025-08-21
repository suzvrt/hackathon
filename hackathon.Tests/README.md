# 🧪 Testes - Hackathon

Este diretório contém os testes unitários e de integração para o projeto Hackathon.

## 📁 Estrutura dos Testes

```
hackathon.Tests/
├── Integration/                    # Testes de Integração
│   ├── Endpoints/                 # Testes dos endpoints da API
│   │   ├── SimulacoesEndpointIntegrationTests.cs
│   │   ├── ProdutosEndpointIntegrationTests.cs
│   │   └── TelemetriaEndpointIntegrationTests.cs
│   ├── Repositories/              # Testes dos repositórios
│   │   ├── SimulacaoRepositoryIntegrationTests.cs
│   │   └── ProdutoRepositoryIntegrationTests.cs
│   ├── Services/                  # Testes dos serviços
│   │   └── TelemetriaServiceIntegrationTests.cs
│   ├── UseCases/                  # Testes dos casos de uso
│   │   └── SimularEmprestimoUseCaseIntegrationTests.cs
│   ├── Middleware/                # Testes do middleware
│   │   └── TelemetryMiddlewareIntegrationTests.cs
│   ├── TestBase.cs                # Classe base para testes de integração
│   ├── TestDataHelper.cs          # Helper para dados de teste
│   └── TestConfiguration.cs       # Configuração dos testes
├── Services/                      # Testes unitários dos serviços
│   └── TelemetriaServiceTests.cs
├── UseCases/                      # Testes unitários dos casos de uso
│   ├── SimularEmprestimoUseCaseTests.cs
│   ├── ObterSimulacoesUseCaseTests.cs
│   └── ObterVolumeDiarioUseCaseTests.cs
├── GlobalUsings.cs                # Usings globais para testes
└── hackathon.Tests.csproj         # Projeto de testes
```

## 🚀 Como Executar os Testes

### Pré-requisitos
- .NET 8 SDK
- Visual Studio 2022 ou VS Code
- SQLite (para testes de integração)

### Executar Todos os Testes
```bash
# Na pasta raiz do projeto
dotnet test

# Ou na pasta de testes
cd hackathon.Tests
dotnet test
```

### Executar Apenas Testes Unitários
```bash
dotnet test --filter "Category!=Integration"
```

### Executar Apenas Testes de Integração
```bash
dotnet test --filter "Category=Integration"
```

### Executar Testes Específicos
```bash
# Testes de um namespace específico
dotnet test --filter "FullyQualifiedName~Endpoints"

# Testes de uma classe específica
dotnet test --filter "FullyQualifiedName~SimulacoesEndpointIntegrationTests"

# Testes de um método específico
dotnet test --filter "FullyQualifiedName~POST_Simulacoes_ComValoresValidos_DeveRetornarSimulacao"
```

### Executar com Cobertura de Código
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 🔧 Configuração dos Testes

### Testes de Integração
Os testes de integração usam:
- **SQLite em memória** para simular o banco de dados
- **WebApplicationFactory** para testar a API completa
- **Dados de teste** pré-configurados
- **Configuração isolada** para cada teste

### Testes Unitários
Os testes unitários usam:
- **Moq** para mock de dependências
- **xUnit** como framework de testes
- **Isolamento completo** das dependências

## 📊 Dados de Teste

### Banco de Teste
O `TestDataHelper` cria automaticamente:
- **3 produtos** com diferentes faixas de valor e prazo
- **3 simulações** de exemplo
- **2 registros de telemetria** para validação

### Estrutura dos Dados
```sql
-- Produtos de teste
Produto 1: R$ 1.000 - R$ 50.000, 6-60 meses, 1.5% ao mês
Produto 2: R$ 5.000 - R$ 100.000, 12-120 meses, 1.75% ao mês
Produto 3: R$ 10.000 - R$ 200.000, 24-240 meses, 2.0% ao mês
```

## 🧪 Tipos de Testes

### 1. Testes de Endpoints
- **SimulacoesEndpointIntegrationTests**: Testa criação e listagem de simulações
- **ProdutosEndpointIntegrationTests**: Testa consulta de volume diário com parâmetro sistema
- **TelemetriaEndpointIntegrationTests**: Testa consulta de métricas

### 2. Testes de Repositórios
- **SimulacaoRepositoryIntegrationTests**: Testa persistência e consulta de simulações
- **ProdutoRepositoryIntegrationTests**: Testa busca de produtos e cálculo de volume

### 3. Testes de Serviços
- **TelemetriaServiceIntegrationTests**: Testa coleta e persistência de métricas

### 4. Testes de Casos de Uso
- **SimularEmprestimoUseCaseIntegrationTests**: Testa fluxo completo de simulação

### 5. Testes de Middleware
- **TelemetryMiddlewareIntegrationTests**: Testa captura automática de telemetria

## 🔍 Padrões de Nomenclatura

### Convenções de Teste
- **Arrange**: Preparação dos dados e dependências
- **Act**: Execução da ação sendo testada
- **Assert**: Validação dos resultados esperados

### Nomenclatura dos Métodos
```
[Verbo]_[Sujeito]_[Condição]_[ResultadoEsperado]
Exemplo: POST_Simulacoes_ComValoresValidos_DeveRetornarSimulacao
```

## 🐛 Troubleshooting

### Problemas Comuns

#### 1. Testes Falhando por Timeout
```bash
# Aumentar timeout dos testes
dotnet test --logger "console;verbosity=detailed"
```

#### 2. Problemas de Conectividade com Banco
```bash
# Verificar se SQLite está funcionando
dotnet test --filter "Category=Integration" --logger "console;verbosity=detailed"
```

#### 3. Testes de Integração Lentos
```bash
# Executar apenas testes unitários para desenvolvimento rápido
dotnet test --filter "Category!=Integration"
```

### Logs de Debug
```bash
# Executar com logs detalhados
dotnet test --logger "console;verbosity=detailed"

# Executar com logs de console
dotnet test --logger "console"
```

## 📈 Cobertura de Código

### Gerar Relatório de Cobertura
```bash
# Instalar coverlet
dotnet tool install --global coverlet.collector

# Executar testes com cobertura
coverlet hackathon.Tests/bin/Debug/net8.0/hackathon.Tests.dll --target "dotnet" --targetargs "test --no-build" --format opencover
```

### Visualizar Cobertura
```bash
# Gerar relatório HTML
reportgenerator -reports:coverage.opencover.xml -targetdir:coverage-report
```

## 🚀 CI/CD

### GitHub Actions
Os testes são executados automaticamente em:
- **Pull Requests**: Validação de qualidade
- **Push para main**: Garantia de estabilidade
- **Releases**: Validação final antes do deploy

### Pipeline de Testes
```yaml
- name: Run Tests
  run: dotnet test --configuration Release --verbosity normal
```

## 📚 Recursos Adicionais

- [Documentação xUnit](https://xunit.net/)
- [Documentação Moq](https://github.com/moq/moq4)
- [Testes de Integração ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [SQLite em Memória](https://www.sqlite.org/inmemorydb.html)
