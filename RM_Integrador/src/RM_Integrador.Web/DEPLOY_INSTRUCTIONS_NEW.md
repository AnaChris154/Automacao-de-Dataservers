# Instruções de Deploy - Sistema RM Integrador

## 📋 Preparação do Ambiente

### 1. Configuração Local (Desenvolvimento)
- ✅ Detecção automática de RM local (portas 8051, 8050, 8052, 8053)
- ✅ Fallback inteligente para servidor quando necessário
- ✅ Configurações flexíveis via `appsettings.Development.json`

### 2. Configuração do Servidor (Produção)

#### A. Editar appsettings.Production.json
Antes de publicar, edite o arquivo `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=NOME_DO_SERVIDOR_BD;Database=RM_Integrador;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "RMSettings": {
    "BaseUrl": "http://IP_DO_SERVIDOR_RM:8051/rmsrestdataserver/rest",
    "Username": "seu_usuario_rm",  
    "Password": "sua_senha_rm",    
    "CODCOLIGADA": "1",
    "Environment": "Production"
  },
  "EnvironmentSettings": {
    "IsProduction": true,
    "PreferLocalRM": false,
    "DetectionTimeout": 2000,
    "LocalRMPorts": [8051, 8050, 8052, 8053]
  }
}
```

**⚠️ IMPORTANTE: Substitua os valores:**
- `NOME_DO_SERVIDOR_BD`: Nome/IP do servidor de banco de dados
- `IP_DO_SERVIDOR_RM`: IP do servidor onde roda o RM
- `seu_usuario_rm` e `sua_senha_rm`: Credenciais válidas do RM

#### B. Configurar Variáveis de Ambiente (Recomendado)
No servidor, configure a variável de ambiente:
```
ASPNETCORE_ENVIRONMENT=Production
```

#### C. Publicar e Deploy
1. **Publicar o projeto:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Copiar arquivos para o servidor:**
   - Toda a pasta `publish` deve ir para o servidor
   - Certifique-se que o `appsettings.Production.json` está com as configurações corretas

3. **No servidor, testar a detecção:**
   - Acesse `http://servidor/Environment` 
   - Verifique se as configurações estão corretas
   - Teste a conectividade com o RM

## 🔧 Funcionalidades do Sistema

### Detecção Inteligente de Ambiente
- **Local**: Detecta automaticamente RM local nas portas configuradas
- **Produção**: Usa configurações fixas do servidor
- **Manual**: Permite override via interface web

### URLs Dinâmicas
- **Desenvolvimento**: `http://localhost:8051/rmsrestdataserver/rest`
- **Produção**: URL configurada no `appsettings.Production.json`
- **Override**: Configurável via `/Environment`

### Fallback Automático
1. URL definida manualmente (prioridade máxima)
2. RM local detectado (desenvolvimento)
3. Configuração do appsettings.json (fallback)

## 🚀 Testando no Servidor

### 1. Teste de Ambiente
- Acesse: `http://servidor/Environment`
- Verifique se mostra "Production" como ambiente
- Confirme se a URL do RM está correta

### 2. Teste de Conectividade
- Na página Environment, use o "Teste de Conectividade"
- Digite a URL do RM do servidor
- Deve retornar sucesso ou erro 401 (que é normal sem autenticação)

### 3. Teste Funcional
- Acesse: `http://servidor/TestRequests`
- Teste uma requisição GET/POST
- Deve usar automaticamente a URL do servidor

## 🔍 Diagnóstico de Problemas

### Problema: RM não encontrado
- Verifique se o IP/porta do RM estão corretos no `appsettings.Production.json`
- Teste conectividade diretamente: `http://IP_RM:8051/rmsrestdataserver/rest`
- Verifique firewall/rede entre servidor web e RM

### Problema: Erro de autenticação
- Confirme usuário/senha do RM no `appsettings.Production.json`
- Teste credenciais diretamente no RM Manager

### Problema: Ambiente não detectado corretamente
- Confirme variável `ASPNETCORE_ENVIRONMENT=Production`
- Verifique se `appsettings.Production.json` está sendo usado

## 📊 Monitoramento

### Logs
- Logs do sistema ficam em: `./logs/`
- Informações de detecção de ambiente são logadas automaticamente

### Interface de Monitoramento
- `/Environment`: Status completo do ambiente
- `/TestRequests`: Status da conexão RM em tempo real

---

## 🎯 Resumo para Deploy Rápido

1. **Editar** `appsettings.Production.json` com as configurações do servidor
2. **Publicar** com `dotnet publish -c Release`
3. **Copiar** para o servidor
4. **Configurar** `ASPNETCORE_ENVIRONMENT=Production`
5. **Testar** via `/Environment`

O sistema funcionará automaticamente com as configurações corretas! 🚀
