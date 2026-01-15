# Live Score Progressive Weights System

## 🎯 Objetivo

Implementar um sistema de **pesos progressivos** que ajusta dinamicamente a influência do LiveScore vs ExcitementScore baseline ao longo dos 90 minutos de jogo.

## 📊 Rationale

Durante um jogo de futebol, a importância dos dados ao vivo aumenta progressivamente:

- **Início (0-30 min)**: Ainda há poucos dados, as previsões pré-jogo são mais confiáveis
- **Meio (30-60 min)**: Padrões começam a emergir, ambos são importantes
- **Final (60-90+ min)**: O que está a acontecer ao vivo é determinante, outcome está a tornar-se claro

## 🔢 Sistema de Pesos

### Fórmula
```
FinalLiveScore = (BaselineScore × BaseWeight) + (LiveComponents × LiveWeight)
```

Onde `BaseWeight + LiveWeight = 1.0` (100%)

### Distribuição por Período

| Tempo de Jogo | Baseline Weight | Live Weight | Descrição |
|---------------|-----------------|-------------|-----------|
| **0-30 min**  | 80% → 50%      | 20% → 50%  | **Early Game** - Interpolação linear |
| **30-60 min** | 50% → 20%      | 50% → 80%  | **Mid Game** - Interpolação linear |
| **60-120 min**| 20%            | 80%        | **Late Game** - Peso fixo |

### Detalhes da Interpolação

#### Early Game (0-30 min)
```csharp
progress = elapsedMinutes / 30.0          // 0.0 to 1.0
baseWeight = 0.80 - (progress × 0.30)     // 0.80 -> 0.50
liveWeight = 1.0 - baseWeight             // 0.20 -> 0.50
```

**Exemplos:**
- 0 min: 80% baseline, 20% live
- 15 min: 65% baseline, 35% live
- 30 min: 50% baseline, 50% live

#### Mid Game (30-60 min)
```csharp
progress = (elapsedMinutes - 30) / 30.0   // 0.0 to 1.0
baseWeight = 0.50 - (progress × 0.30)     // 0.50 -> 0.20
liveWeight = 1.0 - baseWeight             // 0.50 -> 0.80
```

**Exemplos:**
- 30 min: 50% baseline, 50% live
- 45 min: 35% baseline, 65% live
- 60 min: 20% baseline, 80% live

#### Late Game (60-120 min)
```csharp
baseWeight = 0.20  // Fixed
liveWeight = 0.80  // Fixed
```

**Exemplos:**
- 60 min: 20% baseline, 80% live
- 75 min: 20% baseline, 80% live
- 90 min: 20% baseline, 80% live
- 90+5 min: 20% baseline, 80% live

## ⏱️ Cálculo do Tempo Decorrido

O sistema usa múltiplas fontes para determinar o tempo de jogo com precisão:

### 1. Prioridade Alta: `StatusTime.Initial`
```csharp
if (eventData.StatusTime?.Initial.HasValue == true)
{
    int elapsedMinutes = eventData.StatusTime.Initial.Value;

    // Add injury time if in second half
    if (eventData.Time?.InjuryTime2.HasValue == true)
    {
        elapsedMinutes += eventData.Time.InjuryTime2.Value;
    }

    return elapsedMinutes;
}
```

**Vantagem**: Mais preciso, inclui injury time automaticamente

### 2. Fallback: `CurrentPeriodStartTimestamp`
```csharp
if (eventData.CurrentPeriodStartTimestamp.HasValue)
{
    long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    long periodStart = eventData.CurrentPeriodStartTimestamp.Value;
    int elapsedMinutes = (int)((currentTimestamp - periodStart) / 60);

    // If in second half, add 45 minutes
    if (eventData.Status?.Description?.Contains("2nd half") == true)
    {
        elapsedMinutes += 45;
    }

    return Math.Clamp(elapsedMinutes, 0, 120);
}
```

**Vantagem**: Sempre disponível, usa tempo real

### 3. Ultimate Fallback: Assume 45 min
```csharp
_logger.LogWarning("Could not determine elapsed time, assuming 45 minutes");
return 45;
```

**Uso**: Apenas quando dados não disponíveis (muito raro)

## 📈 Gráfico de Evolução

```
Baseline Weight (%)
    100│
     90│
     80│●
     70│ ╲
     60│  ╲
     50│   ●────────●
     40│             ╲
     30│              ╲
     20│               ●═══════════●
     10│
      0│
        └─────────────────────────────────────
        0   15   30   45   60   75   90   105  (min)

        Early │    Mid    │      Late
```

## 🔍 Exemplos Práticos

### Exemplo 1: Jogo Equilibrado (0-0 aos 20 min)

**Dados:**
- ExcitementScore (baseline): 0.70 (70)
- LiveScore (components): 0.40 (40) - poucos eventos
- Tempo decorrido: 20 min

**Cálculo:**
```
progress = 20 / 30 = 0.667
baseWeight = 0.80 - (0.667 × 0.30) = 0.60 (60%)
liveWeight = 0.40 (40%)

FinalLiveScore = (0.70 × 0.60) + (0.40 × 0.40)
               = 0.42 + 0.16
               = 0.58 (58)
```

**Resultado**: Score desce ligeiramente de 70 para 58 (jogo menos excitante que previsto)

### Exemplo 2: Jogo Emocionante (2-2 aos 75 min)

**Dados:**
- ExcitementScore (baseline): 0.65 (65)
- LiveScore (components): 0.90 (90) - muitos golos, chances
- Tempo decorrido: 75 min

**Cálculo:**
```
Late game (>60 min):
baseWeight = 0.20 (20%)
liveWeight = 0.80 (80%)

FinalLiveScore = (0.65 × 0.20) + (0.90 × 0.80)
               = 0.13 + 0.72
               = 0.85 (85)
```

**Resultado**: Score sobe significativamente de 65 para 85 (jogo muito mais excitante que previsto)

### Exemplo 3: Jogo Parado (0-0 aos 85 min, poucas chances)

**Dados:**
- ExcitementScore (baseline): 0.75 (75)
- LiveScore (components): 0.35 (35) - jogo parado, poucas chances
- Tempo decorrido: 85 min

**Cálculo:**
```
Late game (>60 min):
baseWeight = 0.20 (20%)
liveWeight = 0.80 (80%)

FinalLiveScore = (0.75 × 0.20) + (0.35 × 0.80)
               = 0.15 + 0.28
               = 0.43 (43)
```

**Resultado**: Score desce drasticamente de 75 para 43 (jogo decepcionante)

## 🛠️ Implementação

### Ficheiro
[LiveScoreCalculator.cs](src/important-game.infrastructure/Contexts/ScoreCalculator/LiveScoreCalculator.cs)

### Métodos Principais

#### 1. `CalculateLiveScore`
```csharp
public (double newLiveScore, LiveScoreComponents components) CalculateLiveScore(
    double baselineScore,
    double? currentLiveScore,
    SSEvent eventData,
    SSEventStatistics statistics,
    int homeTeamPosition,
    int awayTeamPosition)
{
    // Calculate elapsed time
    int elapsedMinutes = CalculateElapsedMinutes(eventData);

    // Get progressive weights
    var (baseWeight, liveWeight) = CalculateProgressiveWeights(elapsedMinutes);

    // Apply formula
    double calculatedScore = (baseline × baseWeight) + (components.TotalLiveBonus × liveWeight);

    return (finalScore, components);
}
```

#### 2. `CalculateElapsedMinutes`
```csharp
private int CalculateElapsedMinutes(SSEvent eventData)
{
    // 1. Try StatusTime.Initial (most accurate)
    // 2. Fallback to CurrentPeriodStartTimestamp
    // 3. Ultimate fallback: 45 min
}
```

#### 3. `CalculateProgressiveWeights`
```csharp
private (double baseWeight, double liveWeight) CalculateProgressiveWeights(int elapsedMinutes)
{
    // Early (0-30): 80%->50% baseline
    // Mid (30-60): 50%->20% baseline
    // Late (60+): 20% baseline (fixed)
}
```

## 📊 Logging

O sistema faz log detalhado para debugging:

```
LiveScore calculation: Baseline=0.70, CurrentLive=0.65,
Score=2-1, ElapsedMin=75, BaseWeight=0.20, LiveWeight=0.80,
Components=0.85, Final=0.81
```

```
Progressive weights: ElapsedMin=75, BaseWeight=20%, LiveWeight=80%
```

## ✅ Benefícios

1. **Início Conservador**: Não reage exageradamente a poucos eventos iniciais
2. **Transição Suave**: Interpolação linear evita saltos bruscos
3. **Final Responsivo**: Reflete fielmente o que está a acontecer ao vivo
4. **Previsível**: Fórmula simples e clara
5. **Testável**: Fácil verificar se comportamento está correto

## 🧪 Testing

Para testar o sistema:

1. **Early Game Test** (15 min):
   - Verificar que baseline domina (70% weight)
   - Score não deve variar muito

2. **Mid Game Test** (45 min):
   - Verificar pesos equilibrados (35% baseline, 65% live)
   - Score começa a refletir live data

3. **Late Game Test** (80 min):
   - Verificar que live domina (80% weight)
   - Score segue eventos ao vivo

4. **Edge Cases**:
   - Injury time (90+5 min)
   - Half time (45 min)
   - Extra time (105+ min)

## 📝 Notas

- Os pesos são **sempre recalculados** a cada update (não são cacheados)
- O sistema funciona mesmo sem dados de tempo (fallback a 45 min)
- Injury time é adicionado ao tempo decorrido quando disponível
- O máximo é 120 minutos (90 + extra time + injury)
