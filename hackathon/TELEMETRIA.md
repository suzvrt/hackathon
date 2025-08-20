# Implementação de Telemetria

## Visão Geral
Este documento descreve o sistema de telemetria implementado na API Native AOT para coletar métricas de performance sem afetar o desempenho da aplicação.

## Arquitetura

### 1. **Buffer em Memória**
- Dados de telemetria são coletados em memória usando `ConcurrentDictionary<string, EndpointMetrics>`
- Nenhuma operação de I/O bloqueante durante o processamento de requisições
- Dados são descarregados para SQLite a cada 5 minutos em segundo plano

### 2. **Características de Performance**
- **Zero Impacto no Processamento de Requisições**: Coleta de telemetria acontece de forma assíncrona
- **Eficiente em Memória**: Métricas são agregadas em tempo real, não armazenadas individualmente
- **Operações em Lote**: Múltiplos registros de telemetria são inseridos em uma única transação de banco
- **Não-Bloqueante**: Usa `Task.Run()` para evitar bloqueio da thread principal de requisição

### 3. **Fluxo de Dados**
```
Requisição HTTP → Middleware → Métricas em Memória → Descarregamento em BG → SQLite
      ↓              ↓              ↓                    ↓              ↓
   Resposta    Cronômetro    ConcurrentDict        Timer        Inserção em Lote
```

## Esquema do Banco de Dados

### Tabela Telemetria
```sql
CREATE TABLE Telemetria (
    Id TEXT PRIMARY KEY,
    DataReferencia TEXT NOT NULL,           -- Data no formato YYYY-MM-DD
    NomeApi TEXT NOT NULL,                  -- Nome do endpoint
    QtdRequisicoes INTEGER NOT NULL,        -- Contagem total de requisições
    TempoMedio INTEGER NOT NULL,            -- Tempo médio de resposta (ms)
    TempoMinimo INTEGER NOT NULL,           -- Tempo mínimo de resposta (ms)
    TempoMaximo INTEGER NOT NULL,           -- Tempo máximo de resposta (ms)
    PercentualSucesso NUMERIC(5,4) NOT NULL, -- Taxa de sucesso (0.0000 a 1.0000)
    CriadoEm TEXT NOT NULL                 -- Timestamp de criação
);
```

### Índices para Performance
- `IX_Telemetria_DataReferencia` - Consultas rápidas por data
- `IX_Telemetria_NomeApi` - Consultas rápidas por endpoint

## Endpoints da API

### GET /Telemetria
Retorna dados de telemetria para uma data específica (padrão: hoje).

**Parâmetros de Consulta:**
- `dataReferencia` (opcional): Data no formato YYYY-MM-DD

**Formato de Resposta:**
```json
{
    "dataReferencia": "2025-01-27",
    "listaEndpoints": [
        {
            "nomeApi": "Simulacao",
            "qtdRequisicoes": 135,
            "tempoMedio": 150,
            "tempoMinimo": 23,
            "tempoMaximo": 860,
            "percentualSucesso": 0.98
        }
    ]
}
```

### GET /Telemetria/Range
Retorna dados de telemetria para um intervalo de datas.

**Parâmetros de Consulta:**
- `inicio`: Data de início no formato YYYY-MM-DD
- `fim`: Data de fim no formato YYYY-MM-DD

## Detalhes da Implementação

### 1. **TelemetriaService**
- Serviço singleton gerenciando métricas em memória
- Descarregamento automático a cada 5 minutos
- Operações thread-safe usando locks e coleções concorrentes

### 2. **TelemetriaMiddleware**
- Envolve automaticamente todas as requisições HTTP
- Mede duração da requisição e status de sucesso
- Gravação de telemetria não-bloqueante

### 3. **TelemetriaRepository**
- Gerencia persistência SQLite
- Usa transações para inserções em lote
- Consultas otimizadas com indexação adequada

## Considerações de Performance

### Uso de Memória
- Métricas são armazenadas em memória por até 5 minutos
- Uso de memória é limitado pelo número de endpoints únicos
- Limpeza automática previne vazamentos de memória

### Impacto no Banco de Dados
- Inserções em lote minimizam operações de I/O
- Processamento em segundo plano previne bloqueio
- Indexação adequada garante consultas rápidas

### Latência de Requisição
- **Latência adicional zero** para coleta de telemetria
- Sobrecarga do middleware é mínima (< 1ms)
- Processamento assíncrono garante operação não-bloqueante

## Monitoramento e Manutenção

### Limpeza Automática
- Dados de telemetria são automaticamente descarregados a cada 5 minutos
- Métricas em memória são limpas após persistência bem-sucedida
- Disposição do serviço garante descarregamento final dos dados

### Tratamento de Erros
- Falhas de telemetria não afetam a aplicação principal
- Falha silenciosa com logging no console para depuração
- Degradação graciosa sob alta carga

## Exemplos de Uso

### Testando Coleta de Telemetria
1. Faça requisições para qualquer endpoint (ex: `/Simulacao`, `/Produtos`)
2. Aguarde até 5 minutos para os dados serem descarregados
3. Consulte `/Telemetria` para ver as métricas coletadas

### Teste de Performance
```bash
# Gerar carga para teste de telemetria
for i in {1..100}; do
  curl -X POST "http://localhost:5000/Simulacao" \
       -H "Content-Type: application/json" \
       -d '{"valorDesejado": "900.00", "prazo": "5"}'
done
```

## Melhorias Futuras

### Melhorias Potenciais
- Intervalos de descarregamento configuráveis
- Agregação de métricas por hora/minuto
- Funcionalidade de exportação para ferramentas de análise
- Alertas sobre limiares de performance
- Integração com sistemas de monitoramento externos

### Considerações de Escalabilidade
- Para cenários de alto tráfego, considere Redis para armazenamento de métricas
- Implementar amostragem de métricas para volumes muito altos de requisições
- Adicionar compressão de métricas para armazenamento de longo prazo
