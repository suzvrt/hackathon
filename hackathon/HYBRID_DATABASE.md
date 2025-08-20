# 🗄️ Arquitetura de Banco de Dados Híbrido

Este projeto agora usa uma **abordagem de banco de dados híbrida** onde:
- **SQL Server** é usado para o método `ObterProdutosCompativeisAsync` (conforme solicitado)
- **SQLite** é usado para todas as outras operações de banco de dados (simulações, cálculos de volume, etc.)

## 🏗️ Visão Geral da Arquitetura

```
┌─────────────────────────────────────────────────────────────┐
│                    Camada de Aplicação                      │
├─────────────────────────────────────────────────────────────┤
│                 Camada de Repositório                       │
│  ┌──────────────────┐  ┌───────────────────┐                │
│  │ ProdutoRepository│  │SimulacaoRepository│                │
│  │                  │  │                   │                │
│  │ SQL Server       │  │ SQLite            │                │
│  │ (Produtos)       │  │ (Simulações)      │                │
│  └──────────────────┘  └───────────────────┘                │
├─────────────────────────────────────────────────────────────┤
│              HybridConnectionFactory                        │
│  ┌─────────────────┐  ┌─────────────────┐                   │
│  │   SQL Server    │  │     SQLite      │                   │
│  │   Conexão       │  │   Conexão       │                   │
│  └─────────────────┘  └─────────────────┘                   │
└─────────────────────────────────────────────────────────────┘
```

## 🔧 Detalhes da Implementação

### 1. Fábrica de Conexão
- `HybridConnectionFactory` gerencia ambas as conexões de banco de dados
- Cria automaticamente a conexão apropriada baseada no parâmetro `DatabaseType`
- Tipo de banco padrão é SQLite

### 2. Atualizações dos Repositórios
- **ProdutoRepository**: 
  - `ObterProdutosCompativeisAsync` → SQL Server
  - `ObterVolumeSimuladoPorDiaAsync` → SQLite
- **SimulacaoRepository**: Todos os métodos → SQLite

### 3. Inicialização do Banco de Dados
- Banco SQLite é criado automaticamente na inicialização da aplicação
- Tabelas e índices são criados automaticamente
- Arquivo do banco: `hackathon.db` (na raiz da aplicação)

## 📊 Diferenças de Esquema do Banco de Dados

### SQL Server (tabela PRODUTO)
```sql
-- Estrutura da tabela existente (inalterada)
SELECT CO_PRODUTO AS Codigo,
       NO_PRODUTO AS Nome,
       PC_TAXA_JUROS AS TaxaJuros,
       NU_MINIMO_MESES AS MinimoMeses,
       NU_MAXIMO_MESES AS MaximoMeses,
       VR_MINIMO AS ValorMinimo,
       VR_MAXIMO AS ValorMaximo
FROM dbo.PRODUTO
```

### SQLite (tabela Simulacao)
```sql
-- Nova estrutura da tabela
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

## 🚀 Exemplos de Uso

### Usando SQL Server (Produtos)
```csharp
// Usa automaticamente SQL Server
var produtos = await produtoRepository.ObterProdutosCompativeisAsync(1000m, 12);
```

### Usando SQLite (Simulações)
```csharp
// Usa automaticamente SQLite
var simulacoes = await simulacaoRepository.ObterPaginadoAsync(1, 10);
var volume = await produtoRepository.ObterVolumeSimuladoPorDiaAsync(DateOnly.Today);
```

## ⚙️ Configuração

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=hack;User Id=hack;Password=Password23;Encrypt=True;TrustServerCertificate=True;"
  },
  "Sqlite": {
    "DatabasePath": "hackathon.db",
    "ConnectionString": "Data Source=hackathon.db;Cache=Shared;"
  }
}
```

### Variáveis de Ambiente
```bash
# SQL Server
ConnectionStrings__DefaultConnection="Server=localhost;Database=hack;..."

# SQLite
Sqlite__DatabasePath="hackathon.db"
Sqlite__ConnectionString="Data Source=hackathon.db;Cache=Shared;"
```

## 🔍 Benefícios Principais

1. **Performance**: SQLite para operações locais, SQL Server para dados externos
2. **Flexibilidade**: Fácil alternância entre bancos por método
3. **Manutenibilidade**: Separação clara de responsabilidades
4. **Desenvolvimento**: Banco SQLite local para desenvolvimento/teste

## ⚠️ Considerações Importantes

### 1. Consistência de Dados
- **Sem transações entre bancos** - cada operação é isolada
- **Sincronização de dados** deve ser tratada no nível da aplicação
- **Considere consistência eventual** para cenários distribuídos

### 2. Gerenciamento de Conexões
- Cada método do repositório cria sua própria conexão
- Conexões são descartadas adequadamente usando declarações `using`
- Pool de conexões é gerenciado pelos respectivos provedores

### 3. Estratégia de Migração
- **Dados existentes do SQL Server** permanecem inalterados
- **Novos dados SQLite** começam do zero
- **Ferramentas de migração** podem ser necessárias para produção

## 🧪 Testes

### Testes Unitários
```csharp
// Mock da fábrica de conexão híbrida
var mockFactory = new Mock<IHybridConnectionFactory>();
mockFactory.Setup(f => f.CreateConnection(DatabaseType.Sqlite))
           .Returns(mockSqliteConnection.Object);
mockFactory.Setup(f => f.CreateConnection(DatabaseType.SqlServer))
           .Returns(mockSqlServerConnection.Object);
```

### Testes de Integração
```csharp
// Teste com banco SQLite real
var sqliteSettings = new SqliteSettings { DatabasePath = "test.db" };
var factory = new HybridConnectionFactory(sqlServerSettings, sqliteSettings);
```

## 🚨 Solução de Problemas

### Problemas Comuns

1. **Arquivo SQLite não encontrado**
   - Certifique-se de que a aplicação tem permissões de escrita no diretório
   - Verifique se o caminho do arquivo do banco está correto

2. **Falha na conexão SQL Server**
   - Verifique a string de conexão no appsettings
   - Verifique se o SQL Server está rodando e acessível

3. **Compatibilidade Dapper.AOT**
   - Suporte SQLite no Dapper.AOT pode ter limitações
   - Considere usar Dapper regular para operações SQLite se houver problemas

### Comandos de Debug
```bash
# Verificar banco SQLite
sqlite3 hackathon.db ".tables"
sqlite3 hackathon.db "SELECT COUNT(*) FROM Simulacao;"

# Verificar SQL Server
sqlcmd -S localhost -U hack -P Password23 -d hack -Q "SELECT COUNT(*) FROM PRODUTO;"
```

## 🔮 Melhorias Futuras

1. **Camada de Abstração de Banco**: Implementar uma abstração mais sofisticada
2. **Pool de Conexões**: Otimizar gerenciamento de conexões
3. **Sincronização de Dados**: Implementar sincronização entre bancos
4. **Monitoramento**: Adicionar monitoramento de performance do banco
5. **Ferramentas de Migração**: Criar utilitários de migração de dados

## 📚 Referências

- [Documentação SQLite](https://www.sqlite.org/docs.html)
- [Documentação Dapper](https://dapper-tutorial.net/)
- [Microsoft.Data.Sqlite](https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/)
- [Padrões de Banco de Dados Híbrido](https://docs.microsoft.com/en-us/azure/architecture/patterns/)
