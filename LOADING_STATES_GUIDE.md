# Loading States & Skeleton Screens - Guia de Implementação

## 📋 Índice
1. [O que foi implementado](#o-que-foi-implementado)
2. [Como funciona](#como-funciona)
3. [Como usar](#como-usar)
4. [Exemplos de uso](#exemplos-de-uso)
5. [Personalização](#personalização)
6. [Arquivos criados/modificados](#arquivos-criadosmodificados)

---

## 🎯 O que foi implementado

Foi criado um **sistema completo de Loading States com Skeleton Screens** para melhorar a experiência do utilizador durante o carregamento de conteúdo. O sistema inclui:

### ✅ Componentes criados:
1. **Skeleton Screens com animação shimmer** - Placeholders animados que imitam o layout final
2. **Loading Spinner** - Spinner tradicional para casos mais simples
3. **JavaScript Loading Manager** - API para controlar estados de loading facilmente
4. **Partials Razor reutilizáveis** - Componentes skeleton para match cards

### ✅ Páginas atualizadas:
- ✓ Homepage ([Index.cshtml](src/important-game.web/Pages/Index.cshtml)) - Skeleton para trending e upcoming matches
- ✓ Matches ([Matches.cshtml](src/important-game.web/Pages/Matches.cshtml)) - Skeleton para lista completa de matches
- ✓ Sistema preparado para Match Detail

---

## ⚙️ Como funciona

### Arquitetura Híbrida (Server + Client)

```
┌─────────────────────────────────────────────────┐
│  1. Página carrega (SSR - Server Side Render)  │
│     ↓ Dados vêm renderizados do servidor       │
│     ✓ Rápido, sem skeleton na primeira carga    │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│  2. Utilizador interage (filtros, pesquisa)    │
│     ↓ JavaScript mostra skeleton                │
│     ⏳ Feedback visual durante processamento    │
└─────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────┐
│  3. Conteúdo atualizado                         │
│     ✓ Skeleton esconde, conteúdo aparece        │
└─────────────────────────────────────────────────┘
```

### Estrutura HTML

Cada secção tem duas versões do conteúdo:

```html
<div class="matches-section" id="trending-matches-section">
    <!-- SKELETON: Escondido por padrão -->
    <div class="matches-section-content skeleton-content" style="display: none;">
        <partial name="_SkeletonMatchCardSimple" />
        <partial name="_SkeletonMatchCardSimple" />
    </div>

    <!-- CONTEÚDO REAL: Visível por padrão -->
    <div class="matches-section-content actual-content">
        @foreach (var match in Model.TrendingMatches) {
            <partial name="_MatchCardSimple" for="@match" />
        }
    </div>
</div>
```

### Animação Shimmer CSS

A animação shimmer é feita com CSS puro (sem JavaScript):

```scss
@keyframes shimmer {
    0%   { background-position: -468px 0; }
    100% { background-position: 468px 0; }
}

.skeleton {
    background: linear-gradient(
        to right,
        #2a2d35 0%,    // Cor base
        #3a3d45 20%,   // Highlight
        #2a2d35 40%,   // Cor base
        #2a2d35 100%
    );
    animation: shimmer 1.5s linear infinite;
}
```

---

## 🚀 Como usar

### API JavaScript - `window.loadingManager`

O Loading Manager está disponível globalmente através de `window.loadingManager`.

#### 1. **Mostrar/Esconder Loading Manual**

```javascript
// Mostrar skeleton
loadingManager.showLoading('trending-matches-section', 300);

// Esconder skeleton (após operação concluída)
await loadingManager.hideLoading('trending-matches-section');
```

#### 2. **Simular Loading (para testes)**

```javascript
// Mostra skeleton por 2 segundos e depois esconde
await loadingManager.simulateLoading('trending-matches-section', 2000);
```

#### 3. **Envolver função assíncrona com Loading**

```javascript
// A forma mais elegante - envolve qualquer função async
await loadingManager.withLoading(
    'trending-matches-section',
    async () => {
        // Fetch data from API
        const response = await fetch('/api/matches');
        const data = await response.json();

        // Update DOM
        updateMatchCards(data);

        return data;
    },
    300  // Mínimo 300ms de skeleton (evita "flash")
);
```

#### 4. **Loading Spinner Global**

```javascript
// Mostrar spinner overlay
loadingManager.showSpinner('Loading matches...');

// Esconder spinner
loadingManager.hideSpinner();
```

---

## 💡 Exemplos de uso

### Exemplo 1: Aplicar filtro na página Matches

No ficheiro [Matches.cshtml](src/important-game.web/Pages/Matches.cshtml), quando o utilizador clica num filtro de liga:

```javascript
$(".match-league-item").on("click", async function () {
    var selectedLeague = $(this).data("league");

    // Mostrar skeleton durante filtro
    await loadingManager.withLoading(
        'all-matches-section',
        async () => {
            // Lógica de filtro existente
            if (filterLeagues.indexOf(selectedLeague) == -1) {
                filterLeagues.push(selectedLeague);
                $(this).addClass("league-active");
            } else {
                const index = filterLeagues.indexOf(selectedLeague);
                filterLeagues.splice(index, 1);
                $(this).removeClass("league-active");
            }
            updateGameHideStatus();

            // Pequeno delay para garantir smooth UX
            await new Promise(resolve => setTimeout(resolve, 200));
        },
        400  // Mínimo 400ms de skeleton
    );
});
```

### Exemplo 2: Pesquisa de matches

```javascript
document.getElementById('search-match').addEventListener('input', async function () {
    const query = this.value;

    if (query.length < 3) {
        document.getElementById('mw-search-result').innerHTML = '';
        return;
    }

    // Mostrar loading durante pesquisa
    loadingManager.showLoading('all-matches-section');

    try {
        // Perform search
        const results = fuse.search(query);

        // Render results
        renderSearchResults(results);

    } finally {
        // Sempre esconder loading, mesmo se houver erro
        await loadingManager.hideLoading('all-matches-section');
    }
});
```

### Exemplo 3: Carregar mais matches (infinite scroll)

```javascript
async function loadMoreMatches() {
    await loadingManager.withLoading(
        'upcoming-matches-section',
        async () => {
            const response = await fetch('/api/matches?page=2');
            const newMatches = await response.json();

            // Append new matches to DOM
            appendMatchesToDOM(newMatches);
        }
    );
}
```

### Exemplo 4: Refresh automático

```javascript
// Atualizar matches a cada 30 segundos
setInterval(async () => {
    await loadingManager.withLoading(
        'trending-matches-section',
        async () => {
            const response = await fetch('/api/matches/trending');
            const data = await response.json();
            updateTrendingMatches(data);
        },
        500  // Mínimo 500ms para o utilizador perceber a atualização
    );
}, 30000);
```

---

## 🎨 Personalização

### Alterar velocidade da animação shimmer

Em [_skeleton.scss](src/important-game.web/Styles/_skeleton.scss):

```scss
.skeleton {
    animation-duration: 1.5s;  // Alterar para 1s (mais rápido) ou 2s (mais lento)
}
```

### Alterar cores do shimmer

```scss
.skeleton {
    background: linear-gradient(
        to right,
        #2a2d35 0%,      // Alterar cor base
        #3a3d45 20%,     // Alterar highlight
        #2a2d35 40%,
        #2a2d35 100%
    );
}
```

### Criar novo skeleton customizado

1. Criar HTML skeleton em `Pages/Shared/_SkeletonMeuComponente.cshtml`:

```html
<div class="skeleton-meu-componente">
    <div class="skeleton skeleton-image"></div>
    <div class="skeleton skeleton-text"></div>
    <div class="skeleton skeleton-text short"></div>
</div>
```

2. Adicionar estilos em `_skeleton.scss`:

```scss
.skeleton-meu-componente {
    padding: 1rem;

    .skeleton-image {
        width: 100px;
        height: 100px;
        margin-bottom: 10px;
    }
}
```

3. Usar na página:

```html
<div id="minha-secção">
    <div class="skeleton-content" style="display: none;">
        <partial name="_SkeletonMeuComponente" />
    </div>

    <div class="actual-content">
        <!-- Conteúdo real -->
    </div>
</div>
```

---

## 📁 Arquivos criados/modificados

### ✅ Novos arquivos criados:

| Ficheiro | Descrição |
|----------|-----------|
| [Styles/_skeleton.scss](src/important-game.web/Styles/_skeleton.scss) | Estilos skeleton e animações shimmer |
| [Pages/Shared/_SkeletonMatchCardSimple.cshtml](src/important-game.web/Pages/Shared/_SkeletonMatchCardSimple.cshtml) | Skeleton para match card |
| [Pages/Shared/_SkeletonMatchDetail.cshtml](src/important-game.web/Pages/Shared/_SkeletonMatchDetail.cshtml) | Skeleton para match detail |
| [LOADING_STATES_GUIDE.md](LOADING_STATES_GUIDE.md) | Este guia |

### ✏️ Ficheiros modificados:

| Ficheiro | Modificação |
|----------|-------------|
| [Styles/site.scss](src/important-game.web/Styles/site.scss) | Adicionado `@import "_skeleton.scss"` |
| [wwwroot/js/site.js](src/important-game.web/wwwroot/js/site.js) | Adicionado `LoadingStateManager` class |
| [Pages/Index.cshtml](src/important-game.web/Pages/Index.cshtml) | Adicionados skeleton containers |
| [Pages/Matches.cshtml](src/important-game.web/Pages/Matches.cshtml) | Adicionados skeleton containers |
| [wwwroot/css/site.css](src/important-game.web/wwwroot/css/site.css) | CSS compilado com skeleton styles |

---

## 🧪 Testar o sistema

### Teste 1: Simular loading na homepage

Adicionar ao final de [site.js](src/important-game.web/wwwroot/js/site.js):

```javascript
document.addEventListener('DOMContentLoaded', () => {
    // Testar skeleton na homepage
    if (document.getElementById('trending-matches-section')) {
        loadingManager.simulateLoading('trending-matches-section', 2000);
    }
    if (document.getElementById('upcoming-matches-section')) {
        setTimeout(() => {
            loadingManager.simulateLoading('upcoming-matches-section', 2000);
        }, 500);
    }
});
```

### Teste 2: Testar com filtros

Ir à página `/matches` e clicar nos filtros de liga. O skeleton deve aparecer durante a filtragem (se implementado conforme Exemplo 1).

### Teste 3: Developer Tools

Abrir DevTools Console e testar manualmente:

```javascript
// Mostrar skeleton
loadingManager.showLoading('trending-matches-section');

// Esconder após 3 segundos
setTimeout(() => loadingManager.hideLoading('trending-matches-section'), 3000);
```

---

## ⚡ Performance

### Otimizações implementadas:

1. **Skeleton escondido por padrão** - Não afeta First Contentful Paint
2. **CSS puro para animações** - Sem JavaScript overhead
3. **Duração mínima configurável** - Evita "flash" de loading muito rápido
4. **Display: none vs Visibility** - Skeleton não ocupa espaço no DOM quando escondido

### Métricas esperadas:

- **Sem impacto no FCP** (First Contentful Paint) - skeleton não afeta carga inicial
- **Melhoria na UX** - utilizadores vêem feedback visual durante interações
- **< 1ms overhead JavaScript** - LoadingManager é muito leve

---

## 🆘 Troubleshooting

### Skeleton não aparece

**Problema:** `loadingManager.showLoading()` não mostra o skeleton.

**Soluções:**
1. Verificar se o ID da secção está correto
2. Verificar no DevTools se `.skeleton-content` e `.actual-content` existem
3. Verificar se o CSS foi compilado (`dotnet build`)

### Skeleton não tem animação

**Problema:** O skeleton aparece mas não tem o efeito shimmer.

**Soluções:**
1. Verificar se `_skeleton.scss` foi importado em `site.scss`
2. Rebuild do projeto: `dotnet build`
3. Limpar cache do browser (Ctrl+Shift+R)

### Conteúdo real não volta a aparecer

**Problema:** Skeleton esconde mas conteúdo real não aparece.

**Solução:**
```javascript
// Sempre usar await com hideLoading
await loadingManager.hideLoading('section-id');
```

---

## 📚 Próximos passos (opcional)

Para melhorar ainda mais:

1. **API endpoints para matches** - Criar endpoints `/api/matches` para fetch client-side
2. **Infinite scroll** - Carregar mais matches ao fazer scroll
3. **Real-time updates** - WebSockets para atualizar matches em tempo real
4. **Skeleton para Match Detail** - Implementar na página de detalhes do match
5. **Service Worker** - Cache de matches para offline-first

---

## 📞 Suporte

Para dúvidas ou problemas:
- Verificar este guia primeiro
- Verificar exemplos de código acima
- Usar DevTools Console para debug: `console.log(loadingManager)`
