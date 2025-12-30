# 🚀 Match to Watch - Plano de Melhorias

## Visão Geral
Este documento contém o plano completo de melhorias para o portal Match to Watch, organizado por prioridade e impacto. O objetivo é tornar o portal mais apelativo, melhorar a experiência do utilizador e maximizar o revenue através de Google Ads.

---

## 📊 Status Geral - Prioridade Alta

| # | Feature | Status | Impacto | Data Conclusão |
|---|---------|--------|---------|----------------|
| 1 | Google Ads Integration | 🟡 Parcial | Alto 💰 | Em progresso |
| 2 | Loading States & Feedback | ✅ Completo | Alto | 30/12/2025 |
| 3 | Clear All Filters Button | ✅ Completo | Médio | 30/12/2025 |
| 4 | Melhorar Search UX | ✅ Completo | Alto | 30/12/2025 |

**Progresso Geral: 75% completo (3 de 4 itens)**

### 🎯 Próximos Passos Imediatos:
1. Completar Google Ads nas páginas Matches e Match Detail
2. Configurar Auto Ads do AdSense
3. Monitorizar performance dos ads

---

## 🥇 PRIORIDADE ALTA - Quick Wins (1-2 dias)

### ✅ 1. Google Ads Integration 💰
**Status:** ✅ Implementado (Index page)
**Impacto:** Alto - Monetização imediata

#### Posicionamentos Implementados:
- ✅ Above the fold (Index) - Banner 728x90
- ✅ Between sections (Index) - Banner fluid

#### Próximos Passos:
- [ ] Adicionar sidebar ad na página Match Detail (300x250)
- [ ] Adicionar ad na página Matches (entre match cards)
- [ ] Configurar Auto Ads do AdSense
- [ ] Monitorizar performance e CTR

**Revenue Estimado:**
- Conservador: €90-270/mês
- Otimista: €900-2.700/mês (com crescimento)

---

### 2. Loading States & Feedback ⏳
**Status:** ✅ Implementado Completo
**Impacto:** Alto - Melhor UX

#### Implementação Realizada:

**Skeleton Screens com Shimmer Animation:**
- ✅ `_skeleton.scss` - Estilos com animação shimmer CSS
- ✅ `_SkeletonMatchCardSimple.cshtml` - Skeleton para match cards
- ✅ `_SkeletonMatchDetail.cshtml` - Skeleton para match detail page
- ✅ Skeleton containers em `Index.cshtml` (Trending + Upcoming matches)
- ✅ Skeleton containers em `Matches.cshtml`

**JavaScript Loading Manager:**
- ✅ `LoadingStateManager` class em `site.js`
- ✅ API: `showLoading()`, `hideLoading()`, `withLoading()`
- ✅ Duração mínima configurável para evitar "flash"
- ✅ Global: `window.loadingManager`

**Abordagem Híbrida:**
- ✅ SSR (Server-Side Render) para primeira carga - rápido, sem skeleton
- ✅ Skeleton screens para interações do utilizador (filtros, pesquisa)

**Documentação:**
- ✅ `LOADING_STATES_GUIDE.md` - Guia completo com exemplos e API

**Ficheiros Criados/Modificados:**
- `src/important-game.web/Styles/_skeleton.scss`
- `src/important-game.web/Pages/Shared/_SkeletonMatchCardSimple.cshtml`
- `src/important-game.web/Pages/Shared/_SkeletonMatchDetail.cshtml`
- `src/important-game.web/wwwroot/js/site.js` (LoadingStateManager)
- `src/important-game.web/Pages/Index.cshtml` (skeleton containers)
- `src/important-game.web/Pages/Matches.cshtml` (skeleton containers)
- `LOADING_STATES_GUIDE.md`

---

### 3. Clear All Filters Button 🔘
**Status:** ✅ Implementado Completo
**Impacto:** Médio - Melhor UX em Matches page

#### Implementação Realizada:

**HTML:**
- ✅ Botão adicionado em `Matches.cshtml` (linha 16-19)
- ✅ Ícone Bootstrap: `bi-x-circle`
- ✅ Visível apenas quando há filtros ativos

**JavaScript:**
- ✅ Função `updateClearButtonVisibility()` - Controla visibilidade
- ✅ Event listener para limpar filtros (linha 128-149)
- ✅ Limpa filtros de liga E campo de pesquisa
- ✅ Integração com `timezoneManager` para notificação
- ✅ Animação de entrada suave

**CSS:**
- ✅ Estilos em `_matches.scss` (linha 111-146)
- ✅ Cor vermelha (#ef4444) para destaque
- ✅ Hover effects e animação
- ✅ Animação `slideInRight` para entrada
- ✅ Responsive design para mobile

**Funcionalidades:**
- ✅ Detecta filtros de liga ativos
- ✅ Detecta texto no campo de pesquisa
- ✅ Remove todos os filtros com um clique
- ✅ Mostra notificação "All filters cleared"
- ✅ Esconde automaticamente quando não há filtros

**Ficheiros Modificados:**
- `src/important-game.web/Pages/Matches.cshtml` (HTML + JavaScript)
- `src/important-game.web/Styles/_matches.scss` (Estilos + Animações)

---

### 4. Melhorar Search UX 🔍
**Status:** ✅ Implementado Completo
**Impacto:** Alto - Feature muito usada

#### Implementação Realizada:

**1. ✅ Posicionamento Correto:**
- `position: relative` no container `.mw-search`
- Dropdown `position: absolute` com `top: 100%`, `left: 0`, `right: 0`
- Alinhamento perfeito com barra de pesquisa
- `min-width: 100%` para largura igual ao search
- Removida linha branca no focus (`outline: none`)

**2. ✅ Mensagem "No Results":**
- Elemento `<li class="search-no-results">` com ícone
- Mensagem: `No matches found for "{query}"`
- Estilo visual diferenciado (cinzento, não clicável)
- Ícone Bootstrap: `bi-search`

**3. ✅ Navegação Completa por Teclado:**
- **ArrowDown**: Seleciona próximo resultado (circular)
- **ArrowUp**: Seleciona resultado anterior (circular)
- **Enter**: Navega para resultado selecionado (ou primeiro se nenhum selecionado)
- **Escape**: Fecha dropdown e remove focus
- Scroll automático para item selecionado
- Estado visual `.selected` com fundo azul (#258cfb)

**4. ✅ Highlight do Texto Pesquisado:**
- Função `highlightMatch()` com regex
- Termos pesquisados em `<strong>` azul (#258cfb)
- Case-insensitive matching
- Visual feedback do que foi encontrado

**5. ✅ Melhorias Visuais:**
- Custom scrollbar estilizada
- Hover effects suaves
- Estados: normal, hover, selected, active
- Transições smooth (0.2s ease)
- Box-shadow moderna
- Border radius 8px

**JavaScript:**
- `selectedIndex` tracking (linha 152)
- `highlightMatch()` - Highlight de termos (linha 155-159)
- `updateSearchResults()` - Renderiza resultados (linha 161-184)
- `selectResult()` - Navegação por teclado (linha 193-205)
- `navigateToMatch()` - Navega para match (linha 186-191)
- Keyboard event handlers (linha 226-268)
- Click outside para fechar (linha 271-276)

**CSS (_matches_section.scss):**
- Linha 5-49: Container e dropdown
- Linha 67-130: Estilos de resultados
- Linha 49-65: Custom scrollbar
- Linha 81-100: Hover e selected states
- Linha 110-129: No results state

**Ficheiros Modificados:**
- `src/important-game.web/Pages/Matches.cshtml` (JavaScript)
- `src/important-game.web/Styles/_matches_section.scss` (CSS)

**Correções Adicionais (30/12/2025):**
- ✅ **Footer Sticky**: Implementado flexbox layout para footer sempre no fundo
  - `site.scss`: `body { display: flex; flex-direction: column; min-height: 100%; }`
  - `_footer.scss`: `margin-top: auto; flex-shrink: 0;`
- ✅ **Search Input**: Removida linha branca no focus (`outline: none`)
- ✅ **Search Results**: Alinhamento perfeito com barra de pesquisa

---

## 🥈 PRIORIDADE MÉDIA - UX Enhancements (3-5 dias)

### 5. Sticky Navigation Mobile 📱
**Status:** ❌ Não implementado
**Impacto:** Alto em mobile

#### Problema:
- Após scroll, utilizador precisa voltar ao topo para navegar
- Dificulta switching entre páginas

#### Solução:
```css
.navbar {
  position: sticky;
  top: 0;
  z-index: 1000;
  background-color: #f8f9fa;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}
```

**Considerações:**
- Adicionar padding-top ao body para compensar
- Testar em diferentes dispositivos
- Garantir que não cobre conteúdo importante

---

### 6. Persist Filter State 💾
**Status:** ❌ Não implementado
**Impacto:** Alto - Evita frustração

#### Problema:
- Filtros resetam ao recarregar página
- Utilizador perde seleções ao voltar atrás

#### Solução 1: URL Parameters (Recomendado)
```javascript
// Ao aplicar filtro
const leagues = getSelectedLeagues();
const url = new URL(window.location);
url.searchParams.set('leagues', leagues.join(','));
history.pushState({}, '', url);

// Ao carregar página
const urlParams = new URLSearchParams(window.location.search);
const leagues = urlParams.get('leagues')?.split(',') || [];
applyFilters(leagues);
```

**Vantagens:**
- URL é shareable
- Funciona com back/forward do browser
- SEO friendly

#### Solução 2: localStorage
```javascript
// Guardar
localStorage.setItem('selectedLeagues', JSON.stringify(leagues));

// Carregar
const saved = JSON.parse(localStorage.getItem('selectedLeagues'));
```

---

### 7. Match Card Visual Improvements 🎨
**Status:** ❌ Não implementado
**Impacto:** Médio - Melhor apelo visual

#### Melhorias:

**1. Sombras Sutis:**
```scss
.match-card-simple {
  box-shadow: 0 2px 8px rgba(0,0,0,0.15);
  transition: transform 0.2s, box-shadow 0.2s;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 16px rgba(0,0,0,0.25);
  }
}
```

**2. Excitement Score Badge:**
```html
<div class="es-badge" data-es="0.75">
  <i class="bi bi-fire"></i> 75
</div>
```

**3. Team Logos Maiores:**
```scss
.match-card-team img {
  width: 80px; // Era 60px
  transition: transform 0.2s;

  &:hover {
    transform: scale(1.1);
  }
}
```

**4. Gradientes Melhorados:**
```scss
.match-card-simple {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}
```

---

### 8. Breadcrumbs Navigation 🍞
**Status:** ❌ Não implementado
**Impacto:** Baixo - Melhor orientação

#### Implementação:
```html
<nav aria-label="breadcrumb">
  <ol class="breadcrumb">
    <li class="breadcrumb-item"><a href="/">Home</a></li>
    <li class="breadcrumb-item"><a href="/matches">Matches</a></li>
    <li class="breadcrumb-item active">Liverpool vs Arsenal</li>
  </ol>
</nav>
```

**CSS:**
```scss
.breadcrumb {
  background: transparent;
  padding: 0.5rem 0;
  font-size: 0.875rem;
  color: #fff;

  a {
    color: #258cfb;
    text-decoration: none;

    &:hover {
      text-decoration: underline;
    }
  }
}
```

---

### 9. Lazy Loading de Imagens 🖼️
**Status:** ❌ Não implementado
**Impacto:** Alto - Performance

#### Problema:
- Todas as imagens carregam imediatamente
- Página lenta em conexões lentas
- Desperdício de bandwidth

#### Solução:
```html
<img loading="lazy"
     src="/images/team/{{teamId}}.png"
     alt="{{teamName}}"
     class="team-logo" />
```

**Fallback para browsers antigos:**
```javascript
if ('loading' in HTMLImageElement.prototype) {
  // Browser suporta loading="lazy"
} else {
  // Usar IntersectionObserver
  const imageObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const img = entry.target;
        img.src = img.dataset.src;
        imageObserver.unobserve(img);
      }
    });
  });
}
```

**Placeholder durante carregamento:**
```css
.team-logo {
  background: linear-gradient(90deg, #f0f0f0 25%, #e0e0e0 50%, #f0f0f0 75%);
  background-size: 200% 100%;
  animation: loading 1.5s infinite;
}

@keyframes loading {
  0% { background-position: 200% 0; }
  100% { background-position: -200% 0; }
}
```

---

## 🥉 PRIORIDADE BAIXA - Nice to Have (1-2 semanas)

### 10. Favoritos/Watchlist ⭐
**Status:** ❌ Não implementado
**Impacto:** Alto - Engagement

#### Funcionalidade:
```html
<!-- Botão no match card -->
<button class="btn-favorite" data-match-id="{{matchId}}">
  <i class="bi bi-star"></i>
  <i class="bi bi-star-fill" style="display:none"></i>
</button>
```

**JavaScript:**
```javascript
class WatchlistManager {
  constructor() {
    this.favorites = JSON.parse(localStorage.getItem('favorites')) || [];
  }

  toggle(matchId) {
    const index = this.favorites.indexOf(matchId);
    if (index > -1) {
      this.favorites.splice(index, 1);
    } else {
      this.favorites.push(matchId);
    }
    this.save();
  }

  save() {
    localStorage.setItem('favorites', JSON.stringify(this.favorites));
  }
}
```

**Página /my-matches:**
```csharp
public class MyMatchesModel : PageModel
{
  public List<MatchDto> FavoriteMatches { get; set; }

  public async Task OnGetAsync(string favorites)
  {
    var ids = favorites.Split(',').Select(int.Parse);
    FavoriteMatches = await _matchService.GetMatchesByIds(ids);
  }
}
```

---

### 11. Share Buttons 📤
**Status:** ❌ Não implementado
**Impacto:** Médio - Viral growth

#### Implementação:
```html
<div class="share-buttons">
  <button onclick="shareWhatsApp()">
    <i class="bi bi-whatsapp"></i> WhatsApp
  </button>
  <button onclick="shareTwitter()">
    <i class="bi bi-twitter"></i> Tweet
  </button>
  <button onclick="copyLink()">
    <i class="bi bi-clipboard"></i> Copy Link
  </button>
</div>
```

**JavaScript:**
```javascript
function shareWhatsApp() {
  const text = `Check out this match: ${matchTitle}`;
  const url = window.location.href;
  window.open(`https://wa.me/?text=${encodeURIComponent(text + ' ' + url)}`);
}

function shareTwitter() {
  const text = `🔥 ${matchTitle} - Excitement Score: ${score}%`;
  const url = window.location.href;
  window.open(`https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(url)}`);
}

function copyLink() {
  navigator.clipboard.writeText(window.location.href);
  showToast('Link copied to clipboard!');
}
```

---

### 12. Live Score Badge 🔴
**Status:** ❌ Não implementado
**Impacto:** Alto - Engagement

#### Para jogos ao vivo:
```html
<div class="live-badge pulsing">
  <span class="live-dot"></span>
  <span>LIVE</span>
  <span class="live-score">2-1</span>
</div>
```

**CSS:**
```scss
.live-badge {
  background-color: #ef4444;
  color: white;
  padding: 4px 12px;
  border-radius: 4px;
  display: flex;
  align-items: center;
  gap: 6px;

  .live-dot {
    width: 8px;
    height: 8px;
    background-color: white;
    border-radius: 50%;
    animation: pulse 1.5s infinite;
  }
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}
```

**Backend (atualizar a cada 30s):**
```javascript
setInterval(async () => {
  const liveMatches = await fetch('/api/live-matches');
  updateLiveBadges(liveMatches);
}, 30000);
```

---

### 13. Match Predictions/Odds 📊
**Status:** ❌ Não implementado
**Impacto:** Alto - Mais informação

#### Se tiver acesso a dados de odds:
```html
<div class="match-odds">
  <div class="odd-item">
    <span class="odd-label">Home Win</span>
    <span class="odd-value">2.10</span>
  </div>
  <div class="odd-item">
    <span class="odd-label">Draw</span>
    <span class="odd-value">3.20</span>
  </div>
  <div class="odd-item">
    <span class="odd-label">Away Win</span>
    <span class="odd-value">3.50</span>
  </div>
</div>

<div class="prediction">
  <div class="prediction-bar">
    <div class="home-chance" style="width: 45%">45%</div>
    <div class="draw-chance" style="width: 25%">25%</div>
    <div class="away-chance" style="width: 30%">30%</div>
  </div>
</div>
```

**API Integration:**
```csharp
public async Task<OddsDto> GetMatchOdds(int matchId)
{
  // Integrar com API de odds (ex: The Odds API)
  var response = await _httpClient.GetAsync($"https://api.the-odds-api.com/v4/sports/soccer/odds?apiKey={_apiKey}&regions=eu&markets=h2h");
  return await response.Content.ReadAsAsync<OddsDto>();
}
```

---

### 14. Calendar View 📅
**Status:** ❌ Não implementado
**Impacto:** Médio - Alternativa útil

#### Vista de calendário mensal:
```html
<div class="calendar-view">
  <div class="calendar-header">
    <button onclick="previousMonth()">←</button>
    <h2>January 2025</h2>
    <button onclick="nextMonth()">→</button>
  </div>

  <div class="calendar-grid">
    <!-- 7 colunas x 5 linhas -->
    <div class="calendar-day" data-date="2025-01-01">
      <span class="day-number">1</span>
      <div class="day-matches">
        <span class="match-dot" data-es="0.75"></span>
        <span class="match-dot" data-es="0.60"></span>
      </div>
    </div>
  </div>
</div>
```

**Export para Google Calendar:**
```javascript
function exportToGoogleCalendar(match) {
  const startDate = formatDateForGCal(match.dateTime);
  const endDate = formatDateForGCal(addHours(match.dateTime, 2));

  const url = `https://calendar.google.com/calendar/render?action=TEMPLATE
    &text=${encodeURIComponent(match.title)}
    &dates=${startDate}/${endDate}
    &details=${encodeURIComponent(match.description)}
    &location=${encodeURIComponent(match.venue)}`;

  window.open(url);
}
```

---

## 📱 Melhorias Específicas Mobile

### Bottom Navigation Bar
**Status:** ❌ Não implementado
**Impacto:** Alto em mobile

```html
<nav class="bottom-nav">
  <a href="/" class="nav-item active">
    <i class="bi bi-house"></i>
    <span>Home</span>
  </a>
  <a href="/matches" class="nav-item">
    <i class="bi bi-calendar"></i>
    <span>Matches</span>
  </a>
  <a href="/stats" class="nav-item">
    <i class="bi bi-graph-up"></i>
    <span>Stats</span>
  </a>
  <a href="/favorites" class="nav-item">
    <i class="bi bi-star"></i>
    <span>Favorites</span>
  </a>
</nav>
```

**CSS:**
```scss
.bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  background: white;
  display: flex;
  justify-content: space-around;
  padding: 0.5rem 0;
  box-shadow: 0 -2px 10px rgba(0,0,0,0.1);
  z-index: 1000;

  .nav-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    color: #666;
    text-decoration: none;
    font-size: 0.75rem;

    i { font-size: 1.5rem; }

    &.active {
      color: #258cfb;
    }
  }
}
```

---

### Swipe Gestures
**Status:** ❌ Não implementado
**Impacto:** Médio - Melhor UX mobile

```javascript
class SwipeHandler {
  constructor(element) {
    this.element = element;
    this.startX = 0;
    this.startY = 0;

    element.addEventListener('touchstart', this.handleStart.bind(this));
    element.addEventListener('touchend', this.handleEnd.bind(this));
  }

  handleStart(e) {
    this.startX = e.touches[0].clientX;
    this.startY = e.touches[0].clientY;
  }

  handleEnd(e) {
    const endX = e.changedTouches[0].clientX;
    const diffX = this.startX - endX;

    if (Math.abs(diffX) > 50) {
      if (diffX > 0) {
        this.onSwipeLeft();
      } else {
        this.onSwipeRight();
      }
    }
  }

  onSwipeLeft() {
    // Próximo jogo
    navigateToNext();
  }

  onSwipeRight() {
    // Jogo anterior
    navigateToPrevious();
  }
}
```

---

### Compact Card Mode
**Status:** ❌ Não implementado
**Impacto:** Médio

```html
<div class="view-toggle">
  <button onclick="setView('normal')" class="active">
    <i class="bi bi-grid-3x3"></i>
  </button>
  <button onclick="setView('compact')">
    <i class="bi bi-list"></i>
  </button>
</div>
```

**CSS:**
```scss
.match-card-simple.compact {
  flex-direction: row;
  padding: 0.5rem;
  height: 60px;

  .match-card-team img {
    width: 40px;
  }

  .match-card-score {
    font-size: 1rem;
  }
}
```

---

## 🎨 Design System Consistency

### Cores Padronizadas
```scss
// Adicionar ao site.scss
$primary: #258cfb;
$success: #00C851;
$warning: #ffbb33;
$danger: #ff4444;
$info: #33b5e5;

$border-radius-sm: 4px;
$border-radius: 8px;
$border-radius-lg: 10px;

$spacing-unit: 1rem;
$spacing-sm: 0.5rem;
$spacing-lg: 2rem;
```

### Typography Hierarchy
```scss
h1 {
  font-size: 2rem;
  font-weight: 700;
  line-height: 1.2;
}

h2 {
  font-size: 1.5rem;
  font-weight: 600;
  line-height: 1.3;
}

h3 {
  font-size: 1.25rem;
  font-weight: 500;
  line-height: 1.4;
}

.text-sm { font-size: 0.875rem; }
.text-xs { font-size: 0.75rem; }
.text-lg { font-size: 1.125rem; }
```

---

## 💡 Novas Funcionalidades MVP

### 1. Match Alerts 🔔
```javascript
class MatchAlerts {
  requestPermission() {
    Notification.requestPermission().then(permission => {
      if (permission === 'granted') {
        this.enabled = true;
      }
    });
  }

  schedule(matchId, matchDate) {
    const alertTime = new Date(matchDate);
    alertTime.setHours(alertTime.getHours() - 1); // 1 hora antes

    const timeUntilAlert = alertTime - new Date();

    setTimeout(() => {
      this.sendNotification(matchId);
    }, timeUntilAlert);

    localStorage.setItem(`alert_${matchId}`, alertTime);
  }

  sendNotification(matchId) {
    new Notification('Match Starting Soon!', {
      body: 'Liverpool vs Arsenal starts in 1 hour',
      icon: '/icon.png',
      badge: '/badge.png'
    });
  }
}
```

---

### 2. Quick Stats 📈
```html
<!-- Tooltip no hover -->
<div class="match-quick-stats">
  <div class="stat-row">
    <span>Last 5:</span>
    <span class="form-indicator">W W D L W</span>
  </div>
  <div class="stat-row">
    <span>Goals:</span>
    <span>15 scored / 8 conceded</span>
  </div>
  <div class="stat-row">
    <span>Possession:</span>
    <span>58% avg</span>
  </div>
</div>
```

---

### 3. Filter Presets 🎯
```html
<div class="filter-presets">
  <button onclick="applyPreset('top')">
    <i class="bi bi-star-fill"></i> Top Matches (ES > 70%)
  </button>
  <button onclick="applyPreset('today')">
    <i class="bi bi-calendar-day"></i> Today
  </button>
  <button onclick="applyPreset('weekend')">
    <i class="bi bi-calendar-week"></i> This Weekend
  </button>
  <button onclick="applyPreset('myleagues')">
    <i class="bi bi-heart"></i> My Leagues
  </button>
</div>
```

---

### 4. Match Comparison ⚖️
```html
<div class="compare-mode">
  <button onclick="enableCompareMode()">
    <i class="bi bi-arrow-left-right"></i> Compare Matches
  </button>
</div>

<!-- Quando 2 matches selecionados -->
<div class="comparison-view">
  <div class="compare-column">
    <h3>Liverpool vs Arsenal</h3>
    <div class="stat">ES: 75%</div>
    <div class="stat">League Position: 1st vs 2nd</div>
  </div>

  <div class="compare-column">
    <h3>Man City vs Chelsea</h3>
    <div class="stat">ES: 68%</div>
    <div class="stat">League Position: 3rd vs 5th</div>
  </div>
</div>
```

---

### 5. Dark/Light Mode Toggle 🌓
```html
<button class="theme-toggle" onclick="toggleTheme()">
  <i class="bi bi-sun-fill" id="light-icon"></i>
  <i class="bi bi-moon-fill" id="dark-icon" style="display:none"></i>
</button>
```

```javascript
function toggleTheme() {
  const currentTheme = localStorage.getItem('theme') || 'dark';
  const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

  document.body.classList.remove(`theme-${currentTheme}`);
  document.body.classList.add(`theme-${newTheme}`);

  localStorage.setItem('theme', newTheme);
  updateIcons(newTheme);
}
```

```scss
// Light theme colors
.theme-light {
  --background-color: #ffffff;
  --text-color: #333333;
  --card-background: #f5f5f5;

  .match-card-simple {
    background-color: var(--card-background);
    color: var(--text-color);
  }
}
```

---

## 📊 Google Ads - Estratégia Completa

### Posicionamentos Implementados ✅
1. ✅ **Above the fold - Index** (728x90)
2. ✅ **Between sections - Index** (Fluid)

### Próximos Posicionamentos 📍

#### Match Detail Page
```html
<!-- Sidebar direito -->
<div class="match-detail-content-right">
  <div class="ad-container ad-sidebar">
    <ins class="adsbygoogle"
         style="display:block"
         data-ad-client="ca-pub-1934500034472565"
         data-ad-slot="XXXXXXXXXX"
         data-ad-format="rectangle"></ins>
  </div>

  <!-- Conteúdo existente -->
</div>
```

#### Matches Page
```html
<!-- A cada 5 match cards -->
@{int cardCount = 0;}
@foreach (var match in matches)
{
  <partial name="_MatchCardSimple" for="@match" />

  cardCount++;
  if (cardCount % 5 == 0)
  {
    <div class="ad-container ad-in-stream">
      <ins class="adsbygoogle" ...></ins>
    </div>
  }
}
```

### Auto Ads (Recomendado)
```html
<!-- Adicionar no <head> -->
<script async src="https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js?client=ca-pub-1934500034472565"
     crossorigin="anonymous"
     data-ad-frequency-hint="30s"></script>
```

### Ad Optimization Tips
1. **A/B Testing:** Testar diferentes posicionamentos
2. **Ad Balance:** Máximo 3 ads por página
3. **Mobile First:** Priorizar responsive ads
4. **Page Speed:** Lazy load ads abaixo do fold
5. **Ad Placement Heatmap:** Usar Google Analytics

---

## 🚀 Roadmap de Implementação

### ✅ Semana 0 (Concluída)
- [x] Google Ads integration (Index page)
- [x] Timezone selector functionality
- [x] Ad styling and layout

### 📅 Semana 1
- [ ] Loading states/skeletons
- [ ] Clear filters button
- [ ] Search improvements (keyboard nav, no results)
- [ ] Compilar SCSS updates

### 📅 Semana 2
- [ ] Sticky navigation mobile
- [ ] Persist filter state (URL params)
- [ ] Card visual improvements (shadows, hover)
- [ ] Lazy loading imagens

### 📅 Semana 3
- [ ] Watchlist/Favorites
- [ ] Share buttons
- [ ] Live badge
- [ ] Breadcrumbs

### 📅 Semana 4
- [ ] Match alerts
- [ ] Quick stats hover
- [ ] Filter presets
- [ ] Dark/Light toggle

---

## 💰 Estimativa de Revenue

### Google Ads - Cenários

#### Conservador
- **Pageviews:** 1.000/dia
- **Ad units:** 3 por página
- **CTR:** 1-2%
- **CPC:** €0.10-0.30
- **Revenue Diário:** €3-9
- **Revenue Mensal:** €90-270
- **Revenue Anual:** €1.080-3.240

#### Otimista (com crescimento)
- **Pageviews:** 10.000/dia
- **Ad units:** 3 por página
- **CTR:** 1.5-2.5%
- **CPC:** €0.15-0.40
- **Revenue Diário:** €30-90
- **Revenue Mensal:** €900-2.700
- **Revenue Anual:** €10.800-32.400

### Fatores que Aumentam Revenue
1. ✅ Conteúdo de qualidade (Excitement Score único)
2. ✅ Traffic orgânico (SEO)
3. ✅ Engagement (tempo na página)
4. ✅ Páginas por sessão
5. 🔄 Ad placement optimization
6. 🔄 Mobile optimization
7. 🔄 Page speed

---

## 📈 Métricas de Sucesso

### KPIs Principais
- **Pageviews:** Target 5.000/dia
- **Bounce Rate:** < 40%
- **Avg Session Duration:** > 2 minutos
- **Pages per Session:** > 2.5
- **Return Visitors:** > 30%

### Google Ads KPIs
- **CTR:** > 1.5%
- **CPC:** €0.20-0.40
- **Viewability:** > 70%
- **Invalid Traffic:** < 5%

### User Engagement
- **Search Usage:** > 20% dos utilizadores
- **Filter Usage:** > 40% dos utilizadores
- **Match Detail Views:** > 60% dos visitantes
- **Favorites Added:** > 10% dos utilizadores

---

## 🔧 Ferramentas Necessárias

### Analytics
- ✅ Google Analytics (instalado)
- [ ] Google Search Console
- [ ] Hotjar (heatmaps)
- [ ] Microsoft Clarity (session recordings)

### Ads
- ✅ Google AdSense (configurado)
- [ ] AdSense Auto Ads
- [ ] Ad Balance optimization
- [ ] Ads.txt verification

### Performance
- [ ] Google PageSpeed Insights
- [ ] Lighthouse CI
- [ ] WebPageTest
- [ ] GTmetrix

### SEO
- [ ] Yoast SEO / Rank Math
- [ ] Schema.org markup
- [ ] Open Graph tags
- [ ] Twitter Cards

---

## 📝 Notas de Implementação

### Prioridades Absolutas
1. **Loading States** - UX crítico
2. **Clear Filters** - Frustração do utilizador
3. **Search Improvements** - Feature muito usada
4. **Google Ads Optimization** - Revenue

### Quick Wins (< 2 horas cada)
- Sticky navigation
- Lazy loading
- Breadcrumbs
- Share buttons

### Requires Planning (> 1 dia)
- Watchlist/Favorites
- Match alerts
- Calendar view
- Dark/Light mode

---

## 🎯 Conclusão

Este plano está organizado por impacto e esforço. Recomenda-se:

1. **Começar pelos Quick Wins** da Semana 1
2. **Monitorizar Google Ads** performance
3. **Iterar baseado em analytics**
4. **Pedir feedback aos utilizadores**

O objetivo é **maximizar engagement e revenue** mantendo excelente UX.

---

**Última atualização:** 30 Dezembro 2025
**Versão:** 1.0
**Próxima revisão:** Após Semana 1
