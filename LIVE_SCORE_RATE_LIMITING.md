# Live Score Calculator - Estratégia Anti-Bloqueio

## Resumo das Otimizações Implementadas

Este documento descreve as medidas implementadas para **minimizar o risco de bloqueio pelo SofaScore** no Live Score Calculator.

---

## 1. Análise do Problema Original

### Chamadas API por Ciclo (Implementação Inicial):
- **1x** `GetAllLiveMatchesAsync()` - obter lista de jogos ao vivo
- **10x** `GetEventStatisticsAsync()` - statistics para cada jogo prioritário
- **Total: 11 requests a cada 10 minutos**

### Problemas Identificados:
1. ❌ **Burst de requests**: 11 chamadas em sequência sem delays
2. ❌ **Queries repetidas ao DB**: Verificação de mapping para cada evento
3. ❌ **Sem controle granular**: Não é possível desativar só as statistics mantendo ID mapping
4. ❌ **Rate limiting só no SofaScoreRateLimiter**: Delay fixo de 30s não resolve bursts

---

## 2. Melhorias Implementadas

### 2.1 Performance - Caching de Mappings Existentes
**Antes:**
```csharp
foreach (var ssMatch in sofascoreMatches) {
    // Query ao DB para CADA evento
    var existingMapping = await _externalProvidersRepository
        .GetExternalProviderMatchAsync(_liveScoreOptions.SofaScoreProviderId, ssMatch.Id);
}
```

**Depois:**
```csharp
// Carregar todos os mappings UMA VEZ no início
var internalMatchIds = internalMatches.Select(m => m.MatchId).ToList();
var existingMappings = new HashSet<string>();

foreach (var matchId in internalMatchIds) {
    var mapping = await _externalProvidersRepository
        .GetExternalProviderMatchAsync(_liveScoreOptions.SofaScoreProviderId, matchId);
    if (mapping != null)
        existingMappings.Add(mapping.ExternalMatchId);
}

// Verificação em memória (O(1))
if (existingMappings.Contains(ssMatch.Id.ToString()))
    continue;
```

**Benefício:** Reduz queries ao DB de **N eventos** para **M jogos internos** (tipicamente 50+ eventos para 5-10 jogos).

---

### 2.2 Rate Limiting - Delays Entre Statistics Calls

**Nova Configuração:**
```json
"LiveScoreCalculator": {
  "DelayBetweenStatisticsCallsSeconds": 8
}
```

**Implementação:**
```csharp
foreach (var match in priorityMatches) {
    await UpdateMatchWithLiveDataAsync(match, cancellationToken);
    updatedCount++;

    // Delay entre calls (exceto última)
    if (updatedCount < priorityMatches.Count &&
        _liveScoreOptions.DelayBetweenStatisticsCallsSeconds > 0)
    {
        await Task.Delay(
            TimeSpan.FromSeconds(_liveScoreOptions.DelayBetweenStatisticsCallsSeconds),
            cancellationToken);
    }
}
```

**Padrão de Requests (com 5 jogos prioritários):**
```
T=0s:   GetAllLiveMatchesAsync()     [SofaScoreRateLimiter: 30s]
T=30s:  GetEventStatisticsAsync(1)   [SofaScoreRateLimiter: 30s]
T=38s:  [delay 8s]
T=68s:  GetEventStatisticsAsync(2)   [SofaScoreRateLimiter: 30s]
T=76s:  [delay 8s]
T=106s: GetEventStatisticsAsync(3)   ...
...
Total: ~2.5 minutos para 6 requests (1 live + 5 stats)
```

---

### 2.3 Controlo Granular - Flag UpdateStatistics

**Nova Configuração:**
```json
"LiveScoreCalculator": {
  "UpdateStatistics": true  // false = só ID mapping, SEM statistics
}
```

**Cenários de Uso:**

| Cenário | UpdateStatistics | MaxMatchesPerCycle | Requests/Ciclo |
|---------|------------------|-------------------|----------------|
| Normal | `true` | 5 | 6 (1 live + 5 stats) |
| Conservador | `true` | 3 | 4 (1 live + 3 stats) |
| Mapping Only | `false` | N/A | 1 (só live) |

**Quando usar `UpdateStatistics: false`:**
- ⚠️ Suspeita de rate limiting do SofaScore
- 🔧 Debugging/testes sem impacto na API
- 📊 Apenas mapear IDs sem calcular bonuses

---

### 2.4 Configuração Recomendada Por Cenário

#### Cenário A: Operação Normal (Recomendado)
```json
"LiveScoreCalculator": {
  "Enabled": true,
  "IntervalMinutes": 10,
  "MaxMatchesPerCycle": 5,
  "DelayBetweenStatisticsCallsSeconds": 8,
  "UpdateStatistics": true
}
```
- **Requests/hora:** 36 (6 requests × 6 ciclos)
- **Tempo por ciclo:** ~2.5 minutos
- **Risco de bloqueio:** Baixo

#### Cenário B: Modo Conservador (Se houver warnings)
```json
"LiveScoreCalculator": {
  "Enabled": true,
  "IntervalMinutes": 15,
  "MaxMatchesPerCycle": 3,
  "DelayBetweenStatisticsCallsSeconds": 12,
  "UpdateStatistics": true
}
```
- **Requests/hora:** 16 (4 requests × 4 ciclos)
- **Tempo por ciclo:** ~2 minutos
- **Risco de bloqueio:** Muito Baixo

#### Cenário C: Apenas ID Mapping (Emergência)
```json
"LiveScoreCalculator": {
  "Enabled": true,
  "IntervalMinutes": 10,
  "MaxMatchesPerCycle": 0,
  "DelayBetweenStatisticsCallsSeconds": 0,
  "UpdateStatistics": false
}
```
- **Requests/hora:** 6 (1 request × 6 ciclos)
- **Funcionalidade:** Apenas mapeia IDs, sem calcular bonuses
- **Risco de bloqueio:** Mínimo

---

## 3. Camadas de Proteção Anti-Bloqueio

### Camada 1: SofaScoreRateLimiter (Global)
- **Delay mínimo:** 30 segundos entre QUALQUER request
- **Scope:** Todas as chamadas SofaScore (não só live score)
- **Thread-safe:** Sim (SemaphoreSlim)

### Camada 2: DelayBetweenStatisticsCallsSeconds (Local)
- **Delay adicional:** Configurável (default: 8s)
- **Scope:** Apenas statistics calls do LiveScoreCalculator
- **Objetivo:** Espaçar bursts dentro do mesmo ciclo

### Camada 3: MaxMatchesPerCycle (Limite)
- **Limite:** Número máximo de statistics calls por ciclo
- **Priorização:** Por ExcitementScore descendente
- **Ajustável:** Reduzir se houver rate limiting

### Camada 4: UpdateStatistics (Kill Switch)
- **Emergência:** Desativa statistics mantendo ID mapping
- **Recovery:** Permite normalização sem perder funcionalidade

---

## 4. Monitorização e Ajustes

### Sinais de Rate Limiting:
- ⚠️ HTTP 429 (Too Many Requests)
- ⚠️ HTTP 403 (Forbidden) repetidos
- ⚠️ Timeouts frequentes
- ⚠️ Respostas vazias consistentes

### Ações Recomendadas:

**Nível 1 - Avisos Ocasionais:**
```json
"MaxMatchesPerCycle": 3,
"DelayBetweenStatisticsCallsSeconds": 12
```

**Nível 2 - Bloqueio Parcial:**
```json
"IntervalMinutes": 15,
"MaxMatchesPerCycle": 2,
"DelayBetweenStatisticsCallsSeconds": 15
```

**Nível 3 - Bloqueio Total:**
```json
"UpdateStatistics": false  // Apenas mapping
"IntervalMinutes": 20
```

**Recovery:**
Após 24-48h sem incidentes, aumentar gradualmente:
1. `UpdateStatistics: true` com `MaxMatchesPerCycle: 1`
2. Incrementar `MaxMatchesPerCycle` se estável
3. Reduzir `IntervalMinutes` se estável

---

## 5. Exemplo de Execução (Ciclo Normal)

```
[10:00:00] LiveScoreCalculatorJob: Starting cycle
[10:00:00] MatchCalculatorOrchestrator: Found 8 internal live matches
[10:00:00] SofaScoreIntegration: GetAllLiveMatchesAsync() [waiting 30s from last call]
[10:00:30] SofaScoreIntegration: Response OK - 42 SofaScore events
[10:00:30] MatchCalculatorOrchestrator: ID mapping started
[10:00:31] MatchCalculatorOrchestrator: Mapped 3 new matches, 5 already mapped
[10:00:31] MatchCalculatorOrchestrator: Selected 5 priority matches
[10:00:31] SofaScoreIntegration: GetEventStatisticsAsync(match 1) [waiting 30s]
[10:01:01] MatchCalculatorOrchestrator: Updated match 1 with bonus +45
[10:01:09] [delay 8s]
[10:01:39] SofaScoreIntegration: GetEventStatisticsAsync(match 2) [waiting 30s]
[10:02:09] MatchCalculatorOrchestrator: Updated match 2 with bonus +32
[10:02:17] [delay 8s]
[10:02:47] SofaScoreIntegration: GetEventStatisticsAsync(match 3) [waiting 30s]
[10:03:17] MatchCalculatorOrchestrator: Updated match 3 with bonus +58
[10:03:25] [delay 8s]
[10:03:55] SofaScoreIntegration: GetEventStatisticsAsync(match 4) [waiting 30s]
[10:04:25] MatchCalculatorOrchestrator: Updated match 4 with bonus +21
[10:04:33] [delay 8s]
[10:05:03] SofaScoreIntegration: GetEventStatisticsAsync(match 5) [waiting 30s]
[10:05:33] MatchCalculatorOrchestrator: Updated match 5 with bonus +67
[10:05:33] LiveScoreCalculatorJob: Cycle complete - 5 matches updated
[10:10:00] [next cycle starts]
```

**Total:** ~5.5 minutos para processar 5 jogos com máxima segurança.

---

## 6. Comparação: Antes vs Depois

| Métrica | Antes | Depois |
|---------|-------|--------|
| **Requests/ciclo** | 11 (fixo) | 6 (configurável) |
| **Burst protection** | ❌ Não | ✅ Delays de 8s |
| **DB queries (mapping)** | N eventos | M jogos (10x menos) |
| **Configurável** | ❌ Não | ✅ 4 níveis |
| **Kill switch** | ❌ Não | ✅ UpdateStatistics |
| **Tempo/ciclo** | ~6 min | ~2.5-5 min |
| **Risco bloqueio** | Médio | Baixo-Mínimo |

---

## 7. Conclusão

As melhorias implementadas garantem:

✅ **Sustentabilidade:** Configurações conservadoras por padrão
✅ **Flexibilidade:** 4 níveis de proteção ajustáveis
✅ **Recuperação:** Kill switch para emergências
✅ **Performance:** Menos queries, menos requests
✅ **Monitorização:** Logs detalhados para ajustes

**Recomendação:** Começar com configuração Normal (Cenário A) e monitorizar logs. Ajustar apenas se houver sinais de rate limiting.
