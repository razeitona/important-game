# Live Score UI Implementation Guide

## 📊 Overview

Este documento descreve a implementação da UI para mostrar o **LiveExcitementScore** em jogos ao vivo, substituindo o ExcitementScore pré-jogo com dados em tempo real.

## 🎯 Features Implementadas

### 1. Match Cards (Homepage/Listings)

**Localização:** `_MatchCardSimple.cshtml`

Para jogos **LIVE**, o card mostra:

- ✅ **LiveExcitementScore** em destaque (em vez do ExcitementScore)
- ✅ **Indicador de tendência** visual:
  - `↗` (verde) - Score a subir comparado com baseline (+5 ou mais)
  - `↘` (vermelho) - Score a descer comparado com baseline (-5 ou menos)
  - `━` (âmbar) - Score estável (diferença entre -5 e +5)
- ✅ Label alterada para "Live score" (em vez de "Excitement score")
- ✅ Badge "LIVE" com animação pulsante

**Exemplo visual:**
```
╔══════════════════════════════════╗
║ 🔥 Premier League         [LIVE] ║
║                                  ║
║  [Home Team Logo]                ║
║  Home Team Name                  ║
║                                  ║
║      🔥 87 ↗ (+8)               ║
║      Live score                  ║
║                                  ║
║  [Away Team Logo]                ║
║  Away Team Name                  ║
╚══════════════════════════════════╝
```

### 2. Match Detail Page - Hero Section

**Localização:** `Match.cshtml` (linhas 170-195)

Para jogos **LIVE**, a hero section mostra:

- ✅ **LiveExcitementScore** em grande destaque
- ✅ **Indicador de tendência detalhado**:
  - Icon: `↗`, `↘`, ou `━`
  - Diferença numérica: ex: `+8`, `-5`
  - Tooltip: "±X from pre-match baseline"
- ✅ Label: "Live Excitement Score" (em vez de "Excitement Score")
- ✅ Animação pulsante no score

**Exemplo visual:**
```
╔═══════════════════════════════════════════╗
║           🔥 Live Excitement Score        ║
║                                           ║
║                  87                       ║
║               ↗ +8                       ║
║                                           ║
║  This match has high excitement...        ║
╚═══════════════════════════════════════════╝
```

### 3. Match Detail Page - Score Breakdown

**Localização:** `Match.cshtml` (linhas 323-372)

Para jogos **LIVE**, o breakdown mostra **componentes do LiveScore**:

✅ **Live Score Breakdown** (título alterado)
- Score Line (golos, competitividade, underdog)
- Expected Goals (xG)
- Ball Possession
- Big Chances
- Fouls
- Cards (Yellow/Red)

Para jogos **PRE-MATCH**, mantém os componentes originais:
- League Coefficient
- League Standings
- Fixture Importance
- Teams Form
- Teams Goals
- Head to Head
- Rivalry (se aplicável)
- Title Holder (se aplicável)

**Exemplo visual (LIVE):**
```
╔════════════════════════════════════════╗
║ 📊 Live Score Breakdown                ║
╠════════════════════════════════════════╣
║ Expected Goals (xG)         80 ████████║
║ Big Chances                 70 ███████ ║
║ Score Line                  65 ██████  ║
║ Ball Possession             50 █████   ║
║ Cards (Yellow/Red)          30 ███     ║
║ Fouls                       20 ██      ║
╚════════════════════════════════════════╝
```

## 🎨 Design System

### Cores do Indicador de Tendência

```scss
.trend-up {
    color: #10b981;  // Green (Tailwind Green-500)
    animation: pulse-green 2s infinite;
}

.trend-down {
    color: #ef4444;  // Red (Tailwind Red-500)
    animation: pulse-red 2s infinite;
}

.trend-stable {
    color: #f59e0b;  // Amber (Tailwind Amber-500)
    opacity: 0.6;
}
```

### Thresholds

- **Trending Up**: Diferença ≥ +5 pontos
- **Trending Down**: Diferença ≤ -5 pontos
- **Stable**: Diferença entre -5 e +5 pontos

## 📁 Ficheiros Alterados

### Backend (C#)
1. **MatchDto.cs** - Adicionados campos LiveExcitementScore e componentes
2. **MatchDetailDto.cs** - Adicionados campos LiveExcitementScore e componentes
3. **MatchDetailViewModel.cs** - Adicionados campos LiveExcitementScore e componentes
4. **MatchMapper.cs** - Mapeamento dos novos campos
5. **MatchesQueries.cs** - Queries SQL atualizadas para incluir LiveScore

### Frontend (Razor/SCSS)
1. **_MatchCardSimple.cshtml** - Card com LiveScore e indicador de tendência
2. **Match.cshtml** - Hero section e breakdown com LiveScore
3. **_live_score.scss** - Estilos para indicadores de tendência
4. **site.scss** - Import do novo ficheiro SCSS

## 🔄 Lógica de Display

```csharp
// Match Card & Match Detail
var displayScore = Model.IsLive && Model.LiveExcitementScore.HasValue
    ? Model.LiveExcitementScore.Value
    : Model.ExcitmentScore;

var scoreDifference = Model.IsLive && Model.LiveExcitementScore.HasValue
    ? (int)Math.Round((Model.LiveExcitementScore.Value - Model.ExcitmentScore) * 100, 0)
    : 0;

var trendIcon = scoreDifference > 5 ? "↗" : scoreDifference < -5 ? "↘" : "━";
var trendClass = scoreDifference > 5 ? "trend-up" : scoreDifference < -5 ? "trend-down" : "trend-stable";
```

## 🧪 Testing

Para testar a implementação:

1. **Iniciar o LiveScoreCalculatorJob** (deve estar configurado em `appsettings.json`)
2. **Aguardar por um jogo ao vivo** (match dentro da janela de 2 horas)
3. **Verificar que o LiveExcitementScore está a ser calculado** nos logs
4. **Abrir a homepage** e verificar que o card mostra:
   - LiveScore em vez de ExcitementScore
   - Indicador de tendência visível
   - Label "Live score"
5. **Abrir a página de detalhes do jogo** e verificar:
   - Hero section com LiveScore e tendência
   - Breakdown com componentes do LiveScore (xG, Big Chances, etc.)

## 📝 Notas

- O **LiveExcitementScore é nullable** - se for null, mostra o ExcitementScore normal
- A **animação pulsante** no badge LIVE chama atenção para jogos ao vivo
- Os **indicadores de tendência** têm tooltips explicativos
- O **breakdown muda automaticamente** entre componentes pré-jogo e live
- Todos os **valores são arredondados** para inteiros (0-100)

## 🚀 Próximos Passos

Possíveis melhorias futuras:
- [ ] Auto-refresh do LiveScore a cada X segundos (via AJAX/SignalR)
- [ ] Histórico de evolução do LiveScore durante o jogo (gráfico)
- [ ] Notificações quando o score sobe/desce significativamente
- [ ] Comparação visual entre LiveScore e ExcitementScore baseline
