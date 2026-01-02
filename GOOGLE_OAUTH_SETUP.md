# Google OAuth Setup Guide

Este guia explica como configurar a autenticação Google OAuth 2.0 para o Match to Watch.

## Passo 1: Aceder à Google Cloud Console

1. Ir para [Google Cloud Console](https://console.cloud.google.com/)
2. Fazer login com a tua conta Google

## Passo 2: Criar ou Selecionar um Projeto

1. No topo da página, clicar no selector de projetos
2. Opções:
   - **Criar novo projeto**: Clicar em "NEW PROJECT"
     - Nome do projeto: "Match to Watch" (ou outro nome)
     - Clicar em "CREATE"
   - **Usar projeto existente**: Selecionar da lista

## Passo 3: Ativar Google+ API

1. No menu lateral, ir para **"APIs & Services"** → **"Library"**
2. Pesquisar por **"Google+ API"** ou **"Google Identity"**
3. Clicar em **"Google+ API"**
4. Clicar em **"ENABLE"**

## Passo 4: Configurar OAuth Consent Screen

1. No menu lateral, ir para **"APIs & Services"** → **"OAuth consent screen"**
2. Selecionar **"External"** (para permitir qualquer utilizador Google)
3. Clicar em **"CREATE"**
4. Preencher informação obrigatória:
   - **App name**: `Match to Watch`
   - **User support email**: O teu email
   - **Developer contact information**: O teu email
5. Clicar em **"SAVE AND CONTINUE"**
6. **Scopes** (Passo 2):
   - Clicar em **"ADD OR REMOVE SCOPES"**
   - Selecionar:
     - `userinfo.email` (ver o teu endereço de email)
     - `userinfo.profile` (ver as tuas informações pessoais)
   - Clicar em **"UPDATE"**
   - Clicar em **"SAVE AND CONTINUE"**
7. **Test users** (Passo 3): Skip (clicar em **"SAVE AND CONTINUE"**)
8. **Summary**: Review e clicar em **"BACK TO DASHBOARD"**

## Passo 5: Criar OAuth 2.0 Credentials

1. No menu lateral, ir para **"APIs & Services"** → **"Credentials"**
2. Clicar em **"+ CREATE CREDENTIALS"** no topo
3. Selecionar **"OAuth client ID"**
4. Em **"Application type"**, selecionar **"Web application"**
5. Preencher:
   - **Name**: `Match to Watch Web Client`
   - **Authorized JavaScript origins**: (opcional, deixar vazio por agora)
   - **Authorized redirect URIs**:
     - Clicar em **"+ ADD URI"**
     - Adicionar URLs (IMPORTANTE):
       ```
       https://localhost:5001/signin-google
       http://localhost:5000/signin-google
       ```
     - Se tiveres domínio de produção, adiciona também (com e sem www):
       ```
       https://teu-dominio.com/signin-google
       https://www.teu-dominio.com/signin-google
       ```
6. Clicar em **"CREATE"**

## Passo 6: Copiar Client ID e Client Secret

Após criar, aparecerá uma popup com:
- **Your Client ID**: algo como `123456789-abcdefgh.apps.googleusercontent.com`
- **Your Client Secret**: uma string secreta

**⚠️ IMPORTANTE**:
- Copia ambos os valores
- Guarda-os num local seguro
- Podes sempre ver estes valores novamente em "Credentials"

## Passo 7: Configurar a Aplicação

### Opção A: Usar appsettings.json (Desenvolvimento)

1. Abrir `src/important-game.web/appsettings.json`
2. Substituir os valores:
   ```json
   "Authentication": {
     "Google": {
       "ClientId": "COLA_AQUI_O_TEU_CLIENT_ID.apps.googleusercontent.com",
       "ClientSecret": "COLA_AQUI_O_TEU_CLIENT_SECRET"
     }
   }
   ```

**⚠️ CUIDADO**:
- **NÃO** fazer commit deste ficheiro com as credenciais reais
- Adicionar `appsettings.json` ao `.gitignore` ou
- Usar `appsettings.Development.json` (que já deve estar no `.gitignore`)

### Opção B: User Secrets (Recomendado para Desenvolvimento)

```bash
cd src/important-game.web

dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "TEU_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "TEU_CLIENT_SECRET"
```

### Opção C: Environment Variables (Produção)

No servidor de produção, configurar variáveis de ambiente:
```bash
export Authentication__Google__ClientId="TEU_CLIENT_ID"
export Authentication__Google__ClientSecret="TEU_CLIENT_SECRET"
```

## Passo 8: Executar Migração da Base de Dados

Antes de iniciar a aplicação, executar o script SQL para criar as tabelas de utilizadores:

```bash
# Navegar para a pasta do projeto
cd d:\Ricardo\repos\important-game

# Executar migration (SQLite)
sqlite3 data/matchwatch.db < migrations/001_create_user_tables.sql
```

Ou usar uma ferramenta GUI para SQLite (DB Browser for SQLite) e executar o script `migrations/001_create_user_tables.sql`.

## Passo 9: Testar a Autenticação

1. Iniciar a aplicação:
   ```bash
   cd src/important-game.web
   dotnet run
   ```

2. Navegar para `https://localhost:5001`
3. Clicar no botão **"Login"** no menu
4. Deve aparecer a modal de login
5. Clicar em **"Sign in with Google"**
6. Será redirecionado para a página de consent do Google
7. Após autorizar, será redirecionado de volta para a aplicação

## Troubleshooting

### Erro: `redirect_uri_mismatch`
- **Causa**: O redirect URI não está configurado corretamente na Google Cloud Console
- **Solução**: Verificar que `https://localhost:5001/signin-google` está na lista de "Authorized redirect URIs"

### Erro: `invalid_client`
- **Causa**: Client ID ou Client Secret incorretos
- **Solução**: Verificar que copiaste corretamente os valores da Google Cloud Console

### Erro: `access_denied`
- **Causa**: Utilizador cancelou o login ou não tem permissões
- **Solução**: Tentar novamente e autorizar a aplicação

### App ainda em "Testing" mode
- Por defeito, a app fica em modo de teste
- Apenas utilizadores adicionados em "Test users" podem fazer login
- Para produção, ir para "OAuth consent screen" e clicar em **"PUBLISH APP"**

### Erro: `redirect_uri_mismatch` com HTTP em vez de HTTPS (Produção)
- **Causa**: Reverse proxy (Nginx/Cloudflare) termina SSL e encaminha para o container via HTTP
- **Sintoma**: Erro mostra `http://` em vez de `https://` no redirect URI
- **Solução**: O código já está configurado com `UseForwardedHeaders()` no `Program.cs`
- **Verificar**:
  1. Na Google Cloud Console, adicionar AMBOS os URIs:
     - `https://www.matchtowatch.net/signin-google`
     - `https://matchtowatch.net/signin-google`
  2. Certificar que o proxy envia os headers:
     - `X-Forwarded-Proto: https`
     - `X-Forwarded-For: [client-ip]`
  3. Esperar 2-3 minutos para propagação das alterações na Google

## Próximos Passos

Após configurar a autenticação:
1. ✅ Utilizadores podem fazer login/registo com Google
2. ✅ Sistema guarda preferências do utilizador (timezone)
3. ✅ Utilizadores podem marcar matches como favoritos
4. ✅ Utilizadores podem votar em matches

## Referências

- [Google OAuth 2.0 Documentation](https://developers.google.com/identity/protocols/oauth2)
- [ASP.NET Core Google Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins)
