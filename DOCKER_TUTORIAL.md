# Tutorial Docker - Deployment para VPS

## Índice

1. [Construir Imagens Localmente](#1-construir-imagens-localmente)
2. [Guardar Imagens em Ficheiros TAR](#2-guardar-imagens-em-ficheiros-tar)
3. [Enviar para VPS](#3-enviar-para-vps)
4. [Carregar Imagens no VPS](#4-carregar-imagens-no-vps)
5. [Configurar e Iniciar Containers](#5-configurar-e-iniciar-containers)
6. [Gestão de Logs](#6-gestão-de-logs)
7. [Base de Dados](#7-base-de-dados)
8. [Comandos Úteis](#8-comandos-úteis)

---

## 1. Construir Imagens Localmente

### 1.1 Preparar o ambiente

No Windows, abra PowerShell ou Command Prompt e navegue até a pasta do projeto:

```powershell
cd d:\Ricardo\repos\important-game
```

### 1.2 Construir as imagens Docker

O ficheiro `docker-compose.yml` define dois serviços: `web` e `app`. Para construir ambas as imagens:

```powershell
docker compose build
```

Este comando:
- Lê o ficheiro `docker-compose.yml`
- Constrói a imagem `important-game-web` usando `src/important-game.web/Dockerfile`
- Constrói a imagem `important-game-app` usando `src/important-game.app/Dockerfile`
- Compila o código .NET dentro dos containers
- Cria imagens prontas para executar

### 1.3 Verificar imagens criadas

```powershell
docker images
```

Deve ver algo como:
```
REPOSITORY           TAG       IMAGE ID       CREATED         SIZE
important-game-web   latest    abc123def456   2 minutes ago   250MB
important-game-app   latest    def789ghi012   2 minutes ago   230MB
```

---

## 2. Guardar Imagens em Ficheiros TAR

### 2.1 Exportar imagens para ficheiros TAR

As imagens Docker podem ser guardadas como ficheiros `.tar` para transporte:

```powershell
# Guardar imagem web
docker save -o important-game-web.tar important-game-web

# Guardar imagem app
docker save -o important-game-app.tar important-game-app
```

**Alternativa (um único comando):**
```powershell
docker save -o important-game-web.tar important-game-web && docker save -o important-game-app.tar important-game-app
```

### 2.2 Verificar ficheiros criados

```powershell
ls -lh *.tar
```

Deve ver dois ficheiros:
```
-rw-r--r-- 1 User 197121 117M Dec 30 17:42 important-game-app.tar
-rw-r--r-- 1 User 197121 130M Dec 30 17:42 important-game-web.tar
```

### 2.3 Por que usar TAR?

- ✅ Não precisa de copiar código fonte para o VPS
- ✅ Não precisa de instalar .NET SDK no VPS
- ✅ Imagens já testadas localmente
- ✅ Deployment mais rápido
- ✅ Consistência entre ambientes (o que funciona localmente funciona no VPS)

---

## 3. Enviar para VPS

### 3.1 Preparar credenciais

Certifique-se que tem:
- IP do VPS (exemplo: `123.45.67.89`)
- Utilizador SSH (exemplo: `debian`)
- Acesso SSH configurado

### 3.2 Copiar ficheiros TAR para VPS

No Windows (PowerShell), substitua `YOUR_VPS_IP` pelo IP real:

```powershell
# Copiar imagem web
scp important-game-web.tar debian@YOUR_VPS_IP:~/

# Copiar imagem app
scp important-game-app.tar debian@YOUR_VPS_IP:~/

# Copiar docker-compose.yml
scp docker-compose.yml debian@YOUR_VPS_IP:~/
```

**Exemplo com IP real:**
```powershell
scp important-game-web.tar debian@123.45.67.89:~/
scp important-game-app.tar debian@123.45.67.89:~/
scp docker-compose.yml debian@123.45.67.89:~/
```

### 3.3 Copiar base de dados (opcional)

Se quiser usar a base de dados existente:

```powershell
scp data/matchwatch.db debian@YOUR_VPS_IP:~/
```

### 3.4 Progresso do envio

O SCP mostrará o progresso:
```
important-game-web.tar    100%  130MB   5.2MB/s   00:25
```

---

## 4. Carregar Imagens no VPS

### 4.1 Ligar ao VPS via SSH

```powershell
ssh debian@YOUR_VPS_IP
```

### 4.2 Verificar Docker instalado

```bash
# Verificar versão do Docker
docker --version

# Verificar versão do Docker Compose
docker compose version
```

**Se não estiver instalado:**
```bash
sudo apt update
sudo apt install docker.io docker-compose-plugin
sudo systemctl enable docker
sudo systemctl start docker

# Adicionar utilizador ao grupo docker
sudo usermod -aG docker debian

# Fazer logout e login novamente para aplicar
exit
ssh debian@YOUR_VPS_IP
```

### 4.3 Carregar imagens TAR

```bash
# Carregar imagem web
docker load -i ~/important-game-web.tar

# Carregar imagem app
docker load -i ~/important-game-app.tar
```

Deve ver output como:
```
Loaded image: important-game-web:latest
Loaded image: important-game-app:latest
```

### 4.4 Verificar imagens carregadas

```bash
docker images
```

Deve ver as duas imagens listadas:
```
REPOSITORY           TAG       IMAGE ID       CREATED         SIZE
important-game-web   latest    abc123def456   10 minutes ago  250MB
important-game-app   latest    def789ghi012   10 minutes ago  230MB
```

### 4.5 Limpar ficheiros TAR (opcional)

Após carregar, pode apagar os ficheiros TAR para poupar espaço:

```bash
rm ~/important-game-web.tar ~/important-game-app.tar
```

---

## 5. Configurar e Iniciar Containers

### 5.1 Criar estrutura de pastas

```bash
# Criar pasta para a aplicação
mkdir -p ~/important-game/data

# Mover docker-compose.yml
mv ~/docker-compose.yml ~/important-game/

# Navegar para a pasta
cd ~/important-game
```

### 5.2 Mover base de dados (se copiou)

```bash
# Se copiou a base de dados
mv ~/matchwatch.db ~/important-game/data/
```

### 5.3 Configurar Firewall

```bash
# Permitir tráfego HTTP (porta 80)
sudo ufw allow 80/tcp

# Permitir tráfego HTTPS (porta 443)
sudo ufw allow 443/tcp

# Ativar firewall
sudo ufw enable

# Verificar status
sudo ufw status
```

Deve ver:
```
Status: active

To                         Action      From
--                         ------      ----
80/tcp                     ALLOW       Anywhere
443/tcp                    ALLOW       Anywhere
```

### 5.4 Iniciar containers

```bash
# Iniciar em modo detached (background)
docker compose up -d
```

Output esperado:
```
[+] Running 3/3
 ✔ Network important-game_important-game-network  Created
 ✔ Container important-game-app                    Started
 ✔ Container important-game-web                    Started
```

### 5.5 Verificar status dos containers

```bash
docker compose ps
```

Deve mostrar ambos containers como "Up":
```
NAME                 IMAGE                COMMAND                  STATUS              PORTS
important-game-app   important-game-app   "dotnet important-ga…"   Up 10 seconds
important-game-web   important-game-web   "dotnet important-ga…"   Up 10 seconds       0.0.0.0:80->80/tcp, 0.0.0.0:443->443/tcp
```

---

## 6. Gestão de Logs

### 6.1 Ver logs em tempo real

```bash
# Todos os containers
docker compose logs -f

# Apenas web
docker compose logs -f web

# Apenas app
docker compose logs -f app
```

Press `Ctrl+C` para sair.

### 6.2 Ver últimas N linhas

```bash
# Últimas 50 linhas
docker compose logs --tail=50

# Últimas 100 linhas do web
docker compose logs --tail=100 web
```

### 6.3 Ver logs desde um timestamp

```bash
# Logs desde há 1 hora
docker compose logs --since 1h

# Logs desde uma data específica
docker compose logs --since 2025-01-01T00:00:00
```

### 6.4 Logs individuais de containers

```bash
# Logs do container web
docker logs important-game-web

# Logs do container app
docker logs important-game-app

# Seguir logs em tempo real
docker logs -f important-game-web
```

### 6.5 Logs de aplicação bem-sucedida

Quando tudo está a funcionar, deve ver:
```
important-game-web | info: Microsoft.Hosting.Lifetime[14]
important-game-web |       Now listening on: http://[::]:80
important-game-web | info: Microsoft.Hosting.Lifetime[0]
important-game-web |       Application started. Press Ctrl+C to shut down.
important-game-web | info: Microsoft.Hosting.Lifetime[0]
important-game-web |       Hosting environment: Production
```

---

## 7. Base de Dados

### 7.1 Localização da Base de Dados

Com a configuração atual (`./data:/app/data`), a base de dados está em:

- **No VPS:** `~/important-game/data/matchwatch.db`
- **Dentro do container:** `/app/data/matchwatch.db`

### 7.2 Verificar se a base de dados existe

```bash
ls -lh ~/important-game/data/
```

Deve ver:
```
-rw-r--r-- 1 debian debian 12M Dec 30 18:00 matchwatch.db
```

### 7.3 Backup da Base de Dados

```bash
# Criar backup simples
cp ~/important-game/data/matchwatch.db ~/important-game/data/matchwatch.db.backup

# Backup com timestamp
cp ~/important-game/data/matchwatch.db ~/important-game/data/matchwatch.db.$(date +%Y%m%d_%H%M%S)

# Backup comprimido
tar -czf ~/matchwatch-backup-$(date +%Y%m%d).tar.gz -C ~/important-game/data matchwatch.db
```

### 7.4 Restaurar Base de Dados

```bash
# Parar containers
docker compose down

# Substituir base de dados
cp ~/matchwatch.db.backup ~/important-game/data/matchwatch.db

# Iniciar containers novamente
docker compose up -d
```

### 7.5 Copiar Base de Dados do VPS para Windows

No Windows:
```powershell
scp debian@YOUR_VPS_IP:~/important-game/data/matchwatch.db ./data/
```

### 7.6 Aceder à Base de Dados (SQLite)

```bash
# Instalar sqlite3 se necessário
sudo apt install sqlite3

# Aceder à base de dados
sqlite3 ~/important-game/data/matchwatch.db

# Dentro do sqlite3:
.tables                    # Listar tabelas
SELECT * FROM Match LIMIT 5;  # Ver dados
.quit                      # Sair
```

### 7.7 Volume vs Bind Mount

**A configuração atual usa Bind Mount:**

```yaml
volumes:
  - ./data:/app/data
```

**Vantagens:**
- ✅ Base de dados acessível diretamente no VPS
- ✅ Fácil fazer backup (copiar pasta `data`)
- ✅ Pode editar/substituir ficheiros facilmente
- ✅ Transparente - vê os ficheiros no sistema

**Se usasse Named Volume:**
```yaml
volumes:
  - shared-data:/app/data
```
- ❌ Ficheiros guardados em `/var/lib/docker/volumes/...`
- ❌ Mais difícil aceder diretamente
- ✅ Gerido pelo Docker (pode ser mais eficiente)

---

## 8. Comandos Úteis

### 8.1 Gestão de Containers

```bash
# Iniciar containers
docker compose up -d

# Parar containers
docker compose down

# Reiniciar containers
docker compose restart

# Reiniciar apenas um serviço
docker compose restart web

# Parar sem remover containers
docker compose stop

# Iniciar containers parados
docker compose start

# Ver status
docker compose ps

# Ver processos dentro dos containers
docker compose top
```

### 8.2 Reconstruir e Atualizar

```bash
# Reconstruir imagens (se código mudou)
docker compose build

# Reconstruir sem cache
docker compose build --no-cache

# Parar, reconstruir e iniciar
docker compose down
docker compose build
docker compose up -d

# Forçar recriação de containers
docker compose up -d --force-recreate
```

### 8.3 Executar Comandos nos Containers

```bash
# Abrir shell no container web
docker exec -it important-game-web /bin/bash

# Executar comando único
docker exec important-game-web ls -la /app/data

# Ver variáveis de ambiente
docker exec important-game-web env
```

### 8.4 Inspecionar Containers

```bash
# Ver detalhes do container
docker inspect important-game-web

# Ver IP do container
docker inspect important-game-web | grep IPAddress

# Ver volumes montados
docker inspect important-game-web | grep -A 10 Mounts

# Ver uso de recursos
docker stats

# Ver uso de recursos de um container
docker stats important-game-web
```

### 8.5 Limpeza

```bash
# Remover containers parados
docker container prune

# Remover imagens não usadas
docker image prune

# Remover volumes não usados
docker volume prune

# Limpeza completa (cuidado!)
docker system prune -a

# Ver espaço usado
docker system df
```

### 8.6 Verificar Conectividade

```bash
# Testar se a aplicação responde
curl http://localhost

# Testar porta específica
curl http://localhost:80

# Ver portas abertas
sudo netstat -tulpn | grep :80

# Ou com ss
ss -tulpn | grep :80
```

### 8.7 Rede Docker

```bash
# Listar redes
docker network ls

# Inspecionar rede
docker network inspect important-game_important-game-network

# Ver containers numa rede
docker network inspect important-game_important-game-network | grep -A 5 Containers
```

### 8.8 Healthcheck

```bash
# Ver estado de health
docker inspect important-game-web | grep -A 10 Health

# Executar healthcheck manualmente
docker exec important-game-web curl -f http://localhost:80/
```

---

## 9. Troubleshooting

### 9.1 Container não inicia

```bash
# Ver logs de erro
docker compose logs web

# Ver eventos Docker
docker events

# Ver se há conflito de portas
sudo netstat -tulpn | grep :80
```

### 9.2 Permissões da Base de Dados

```bash
# Dar permissões corretas
sudo chown -R debian:debian ~/important-game/data
chmod -R 755 ~/important-game/data
chmod 644 ~/important-game/data/matchwatch.db
```

### 9.3 Porta 80 já em uso

```bash
# Ver o que está a usar porta 80
sudo lsof -i :80

# Parar serviço que esteja a usar (exemplo: nginx)
sudo systemctl stop nginx
```

### 9.4 Container sai imediatamente

```bash
# Ver logs completos
docker logs important-game-web

# Iniciar em modo interativo para debug
docker run -it --rm important-game-web /bin/bash
```

### 9.5 Não consegue aceder via navegador

1. Verificar firewall:
```bash
sudo ufw status
```

2. Verificar se container está a correr:
```bash
docker compose ps
```

3. Testar localmente no VPS:
```bash
curl http://localhost
```

4. Verificar IP público do VPS:
```bash
curl ifconfig.me
```

---

## 10. Workflow Completo

### 10.1 Primeira Deployment

**No Windows:**
```powershell
# 1. Construir imagens
docker compose build

# 2. Guardar em TAR
docker save -o important-game-web.tar important-game-web
docker save -o important-game-app.tar important-game-app

# 3. Enviar para VPS
scp important-game-web.tar debian@YOUR_VPS_IP:~/
scp important-game-app.tar debian@YOUR_VPS_IP:~/
scp docker-compose.yml debian@YOUR_VPS_IP:~/
scp data/matchwatch.db debian@YOUR_VPS_IP:~/
```

**No VPS:**
```bash
# 4. Carregar imagens
docker load -i ~/important-game-web.tar
docker load -i ~/important-game-app.tar

# 5. Organizar ficheiros
mkdir -p ~/important-game/data
mv ~/docker-compose.yml ~/important-game/
mv ~/matchwatch.db ~/important-game/data/
cd ~/important-game

# 6. Configurar firewall
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# 7. Iniciar
docker compose up -d

# 8. Verificar
docker compose ps
docker compose logs -f
```

### 10.2 Atualização

**No Windows:**
```powershell
# 1. Reconstruir imagens
docker compose build

# 2. Guardar novas versões
docker save -o important-game-web.tar important-game-web
docker save -o important-game-app.tar important-game-app

# 3. Enviar para VPS
scp important-game-web.tar debian@YOUR_VPS_IP:~/
scp important-game-app.tar debian@YOUR_VPS_IP:~/
```

**No VPS:**
```bash
# 4. Parar containers
cd ~/important-game
docker compose down

# 5. Carregar novas imagens
docker load -i ~/important-game-web.tar
docker load -i ~/important-game-app.tar

# 6. Iniciar com novas imagens
docker compose up -d

# 7. Verificar
docker compose logs -f
```

---

## 11. Checklist de Deployment

- [ ] Docker e Docker Compose instalados no VPS
- [ ] Imagens construídas localmente
- [ ] Imagens exportadas para TAR
- [ ] Ficheiros TAR copiados para VPS
- [ ] docker-compose.yml copiado para VPS
- [ ] Base de dados copiada (se necessário)
- [ ] Imagens carregadas no VPS
- [ ] Estrutura de pastas criada
- [ ] Firewall configurado
- [ ] Containers iniciados
- [ ] Logs verificados
- [ ] Aplicação acessível via browser
- [ ] Backup da base de dados configurado

---

## 12. Links Úteis

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [ASP.NET Core in Docker](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [SQLite Documentation](https://www.sqlite.org/docs.html)

---

**Última Atualização:** 30 de Dezembro de 2025
