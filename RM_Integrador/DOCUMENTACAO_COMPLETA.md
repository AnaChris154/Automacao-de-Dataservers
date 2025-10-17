# 📋 Documentação Completa - Sistema RM_Integrador

## 📑 Índice
1. [Visão Geral](#visão-geral)
2. [Login e Autenticação](#login-e-autenticação)
3. [Funcionalidades Principais](#funcionalidades-principais)
4. [Área Administrativa](#área-administrativa)
5. [Testes de Conexão](#testes-de-conexão)
6. [Biblioteca de Templates](#biblioteca-de-templates)
7. [Comparação de JSONs](#comparação-de-jsons)
8. [Visualizador JSON](#visualizador-json)
9. [Solução de Problemas](#solução-de-problemas)

---

## 🎯 Visão Geral

O **RM_Integrador** é um sistema web desenvolvido para facilitar a integração e testes com o sistema RM da TOTVS. Ele oferece um conjunto completo de ferramentas para:

- **Testar conexões** com servidores RM
- **Executar requisições** GET e POST de forma simplificada
- **Gerenciar templates** de requisições pré-configuradas
- **Comparar JSONs** entre dados do cliente e templates base
- **Visualizar dados JSON** de forma organizada
- **Administrar usuários** e controlar acesso

---

## 🔐 Login e Autenticação

### 👤 Primeiro Acesso

1. **Acesse o sistema**: URL fornecida pela equipe técnica
2. **Criar conta**: Clique em "Registrar" na tela de login
3. **Preencher dados**:
   - Email válido
   - Senha (mínimo 1 caractere)
   - Confirmar senha

**⚠️ IMPORTANTE**: Para usar o sistema, seu usuário deve estar cadastrado no RM. Se não estiver, você receberá erro de "Não Autorizado" ao tentar fazer requisições.

### 👥 Tipos de Usuário

- **Usuário Comum**: Acesso às funcionalidades básicas
- **Administrador**: Acesso completo + gerenciamento de usuários

### 🔑 Gerenciamento de Senhas

- Senhas simples são permitidas (mínimo 1 caractere)
- Alteração de senha disponível no perfil do usuário
- Reset de senha pelo administrador

---

## 🛠️ Funcionalidades Principais

### 📊 Dashboard Principal

Após o login, você terá acesso ao menu principal com as seguintes opções:

- **🧪 Testes de Requisições**: Realizar testes GET/POST
- **📚 Biblioteca**: Gerenciar templates de requisições
- **🔍 Comparar JSONs**: Comparar dados do cliente com templates base
- **👁️ Visualizar JSON**: Visualizar dados JSON formatados
- **👤 Perfil**: Gerenciar dados pessoais
- **⚙️ Admin** (apenas administradores): Gerenciar sistema

---

## 👑 Área Administrativa

### 🔧 Acesso Administrativo

**Somente usuários com perfil de administrador** têm acesso a esta área.

### 👥 Gerenciamento de Usuários

1. **Visualizar Usuários**
   - Lista completa de usuários cadastrados
   - Status de cada usuário (ativo/inativo)
   - Roles atribuídas

2. **Promover Usuários**
   - Transformar usuário comum em administrador
   - Processo irreversível via interface

3. **Gerenciar Status**
   - Ativar/desativar usuários
   - Controlar acesso ao sistema

### 📋 Logs do Sistema

- Visualização de logs de acesso
- Monitoramento de atividades
- Histórico de erros e eventos

---

## 🧪 Testes de Conexão

### 🎯 Funcionalidade Principal

A tela de **Testes de Requisições** é o coração do sistema, permitindo:

### 🔄 Modes de Execução

1. **Automático (Recomendado)**
   - Sistema escolhe automaticamente a melhor conexão
   - Testa primeiro conexão local, depois remota se necessário
   - Ideal para uso geral

2. **Forçar Local**
   - Força uso do servidor local
   - Útil quando você sabe que o RM está rodando localmente

3. **Forçar Remoto**
   - Força uso do servidor remoto
   - Útil para testes específicos com servidor remoto

### 📡 Tipos de Requisição

#### 🔍 Requisições GET

1. **Selecionar DataServer**
   - Escolha o dataserver na lista suspensa
   - Exemplos: `FopFuncData`, `RhuPessoData`, etc.

2. **Definir Filtros** (opcional)
   - Campo `Filter`: Condições WHERE SQL
   - Exemplo: `CODCOLIGADA = 1 AND ATIVO = 1`

3. **Executar**
   - Clique em "Executar GET"
   - Aguarde o resultado na aba "Resultado"

#### 📤 Requisições POST

1. **Selecionar DataServer**
   - Mesmo processo do GET

2. **Inserir Dados JSON**
   - Cole o JSON na área de texto
   - Formato deve seguir estrutura do RM
   - Exemplo:
   ```json
   {
     "CODCOLIGADA": 1,
     "NOME": "João Silva",
     "EMAIL": "joao@empresa.com"
   }
   ```

3. **Executar**
   - Clique em "Executar POST"
   - Verifique resultado e possíveis erros

### 📊 Interpretação de Resultados

#### ✅ Sucesso
- **Status 200**: Requisição executada com sucesso
- **Dados retornados**: JSON com os dados solicitados
- **Headers**: Informações da resposta

#### ❌ Erros Comuns
- **Status 400**: Erro nos parâmetros enviados
- **Status 401**: Erro de autenticação
- **Status 404**: DataServer não encontrado
- **Status 500**: Erro interno do servidor RM

### 🎨 Interface Visual

- **Indicadores Visuais**: 
  - 🟢 Verde: Sucesso
  - 🟡 Amarelo: Aviso
  - 🔴 Vermelho: Erro

- **Feedback em Tempo Real**:
  - URL sendo utilizada exibida claramente
  - Modo de execução indicado
  - Tempo de resposta mostrado

---

## 📚 Biblioteca de Templates

### 🎯 Objetivo

Armazenar e reutilizar requisições pré-configuradas para agilizar testes.

### 📋 Gerenciamento de Templates

#### ➕ Criar Novo Template

1. **Acesse**: Menu "Biblioteca"
2. **Clique**: "Novo Template"
3. **Preencha**:
   - Nome do template
   - Descrição
   - DataServer
   - Tipo (GET/POST)
   - Dados JSON (se POST)
   - Filtros (se GET)

#### 📝 Editar Template

1. **Localizar**: Template na lista
2. **Clicar**: Botão "Editar"
3. **Modificar**: Campos necessários
4. **Salvar**: Confirmar alterações

#### 🗑️ Excluir Template

1. **Localizar**: Template na lista
2. **Clicar**: Botão "Excluir"
3. **Confirmar**: Ação de exclusão

#### ▶️ Executar Template

1. **Localizar**: Template desejado
2. **Clicar**: Botão "Executar"
3. **Verificar**: Resultado na nova tela

### 📁 Organização

- Templates organizados por tipo (GET/POST)
- Busca por nome ou descrição
- Ordenação por data de criação/modificação

---

## 🔍 Comparação de JSONs

### 🎯 Finalidade

Comparar um JSON enviado pelo cliente com um JSON que você tem como base/template, identificando diferenças entre eles.

### 🔄 Como Usar

#### 1️⃣ Inserir Primeiro JSON

1. **Cole o JSON do Cliente**: Na primeira área de texto
2. **Dar um Nome**: Identifique como "JSON Cliente" ou similar

#### 2️⃣ Inserir Segundo JSON

1. **Cole seu JSON Base**: Na segunda área de texto  
2. **Dar um Nome**: Identifique como "JSON Template" ou similar

#### 3️⃣ Comparar

1. **Clique em "Comparar"**
2. **Analise as Diferenças**: Sistema mostrará lado a lado

### 📊 Entendendo as Diferenças

- **Estrutural**: Mostra se a estrutura dos JSONs é diferente
- **Valores**: Compara os valores dos campos iguais
- **Campos Únicos**: Identifica campos que existem apenas em um dos JSONs

### 🎨 Visualização

- **Lado a Lado**: JSONs exibidos paralelamente
- **Cores Diferenciadas**: 
  - 🟢 Verde: Igual em ambos
  - 🟡 Amarelo: Diferente
  - 🔴 Vermelho: Ausente em um dos lados

---

## 👁️ Visualizador JSON

### 🎯 Objetivo

Visualizar dados JSON de forma organizada e navegável.

### 🔧 Funcionalidades

#### 📋 Visualização Estruturada

1. **Expansão/Contração**: Objetos e arrays
2. **Identação Visual**: Hierarquia clara
3. **Tipos de Dados**: Identificados por cores
4. **Números de Linha**: Para referência

#### 🔍 Navegação

- **Busca de Campos**: Localizar propriedades específicas
- **Filtros**: Mostrar apenas tipos específicos
- **Expandir Tudo**: Ver estrutura completa
- **Contrair Tudo**: Visão resumida

#### 📤 Exportação

- **Copiar**: JSON formatado para clipboard
- **Download**: Arquivo JSON
- **Imprimir**: Versão formatada

### 🎨 Interface

- **Syntax Highlighting**: Destaque de sintaxe
- **Numeração**: Linhas numeradas
- **Dobramento**: Seções dobráveis
- **Zoom**: Controle de tamanho da fonte

---

##  Solução de Problemas

### 🔧 Problemas Comuns

#### 🌐 Erro de Conexão com RM

**Sintomas**:
- Timeout nas requisições
- Erro "Connection refused"
- Status 503/502

**Soluções**:
1. ✅ Verificar se o serviço RM está rodando
2. ✅ Confirmar porta correta (8051)
3. ✅ Testar conectividade de rede
4. ✅ Verificar firewall

#### 🔐 Erro de Autenticação

**Sintomas**:
- Status 401 Unauthorized
- Mensagem de credenciais inválidas

**Soluções**:
1. ✅ Verificar usuário/senha no `appsettings.json`
2. ✅ Confirmar permissões no RM
3. ✅ Testar credenciais manualmente

#### 💾 Erro de Banco de Dados

**Sintomas**:
- Erro ao inicializar aplicação
- Timeout de conexão com SQL

**Soluções**:
1. ✅ Verificar connection string
2. ✅ Confirmar serviço SQL Server ativo
3. ✅ Testar conectividade com banco
4. ✅ Verificar permissões de acesso

#### 📱 Problemas de Interface

**Sintomas**:
- Layout quebrado
- Botões não funcionam
- JavaScript errors

**Soluções**:
1. ✅ Limpar cache do navegador
2. ✅ Atualizar página (F5)
3. ✅ Testar em navegador diferente
4. ✅ Verificar console de erro (F12)

### 📋 Logs e Diagnóstico

#### 📊 Onde Encontrar Logs

1. **Logs da Aplicação**: Console da aplicação
2. **Logs do IIS**: Event Viewer
3. **Logs do SQL**: SQL Server logs
4. **Logs do RM**: Logs do próprio RM

#### 🔍 Informações Úteis para Suporte

- Mensagem de erro completa
- Horário exato do problema
- Ações realizadas antes do erro
- Ambiente (desenvolvimento/produção)
- Versão do sistema operacional
- Navegador utilizado

---

## 🚀 Deploy e Instalação

### 📦 Arquivos Necessários

O sistema é entregue como uma pasta `publish` contendo:

- `RM_Integrador.Web.exe` - Executável principal
- `appsettings.json` - Configurações
- `web.config` - Configuração IIS
- DLLs e dependências
- Pasta `wwwroot` - Arquivos estáticos

### 🔧 Instalação Passo a Passo

#### 1️⃣ Preparar Ambiente

```cmd
# Criar pasta de instalação
mkdir C:\RM_Integrador
cd C:\RM_Integrador

# Copiar arquivos do publish
xcopy "PASTA_PUBLISH\*" "C:\RM_Integrador\" /E /H /C /I
```

#### 2️⃣ Configurar Banco de Dados

1. **Criar Database**:
   ```sql
   CREATE DATABASE RM_Integrador;
   ```

2. **Configurar Connection String**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=SERVIDOR;Database=RM_Integrador;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```

#### 3️⃣ Configurar RM

```json
{
  "RMSettings": {
    "BaseUrl": "http://SERVIDOR_RM:8051/rmsrestdataserver/rest",
    "Username": "USUARIO_RM",
    "Password": "SENHA_RM",
    "CODCOLIGADA": "1"
  }
}
```

#### 4️⃣ Executar Aplicação

```cmd
# Modo direto (desenvolvimento)
cd C:\RM_Integrador
.\RM_Integrador.Web.exe

# Como serviço Windows (produção)
sc create "RM_Integrador" binPath="C:\RM_Integrador\RM_Integrador.Web.exe"
sc start "RM_Integrador"
```

### 🌐 Configuração IIS (Opcional)

#### 1️⃣ Instalar ASP.NET Core Hosting Bundle

Download do site oficial da Microsoft.

#### 2️⃣ Configurar Site IIS

1. **Criar Application Pool**:
   - Nome: `RM_Integrador`
   - .NET CLR Version: `No Managed Code`
   - Managed Pipeline Mode: `Integrated`

2. **Criar Website**:
   - Nome: `RM_Integrador`
   - Physical Path: `C:\RM_Integrador`
   - Port: `80` ou `443` (HTTPS)

3. **Configurar web.config**:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <location path="." inheritInChildApplications="false">
       <system.webServer>
         <handlers>
           <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
         </handlers>
         <aspNetCore processPath=".\RM_Integrador.Web.exe" arguments="" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
       </system.webServer>
     </location>
   </configuration>
   ```

### 🔒 Configurações de Segurança

#### 🛡️ Firewall

```cmd
# Liberar porta da aplicação
netsh advfirewall firewall add rule name="RM_Integrador" dir=in action=allow protocol=TCP localport=5095
```

#### 👥 Permissões de Usuário

1. **Usuário da Aplicação**: Criar usuário específico
2. **Permissões SQL**: Acesso ao banco RM_Integrador
3. **Permissões de Rede**: Acesso ao servidor RM

### 📊 Monitoramento

#### 🔍 Health Checks

- URL: `http://servidor:5095/health`
- Verificação de banco de dados
- Verificação de conectividade RM

#### 📈 Performance Counters

- CPU usage
- Memory usage
- Request count
- Response time

---

## 📞 Suporte e Contato

### 📧 Contatos

- **Desenvolvimento**: [email do desenvolvedor]
- **Suporte Técnico**: [email do suporte]
- **Documentação**: [link para documentação atualizada]

### 🆘 Em Caso de Problemas

1. **Verificar logs** da aplicação
2. **Consultar seção** "Solução de Problemas"
3. **Coletar informações** detalhadas do erro
4. **Contactar suporte** com as informações coletadas

### 📚 Recursos Adicionais

- **Manual do RM**: Documentação oficial TOTVS
- **ASP.NET Core**: Documentação Microsoft
- **SQL Server**: Documentação Microsoft

---

## 📈 Atualizações e Versionamento

### 🔄 Processo de Atualização

1. **Backup**: Sempre fazer backup antes
2. **Parar Serviço**: Interromper aplicação
3. **Substituir Arquivos**: Manter configurações
4. **Reiniciar**: Iniciar nova versão
5. **Testar**: Verificar funcionamento

### 📋 Histórico de Versões

- **v1.0**: Versão inicial
- **v1.1**: Melhorias na interface de testes
- **v1.2**: Adição do sistema de comparação JSON
- **[Atual]**: Versão estável com todas as funcionalidades

---

## ✅ Checklist de Implantação

### 🔧 Pré-Implantação

- [ ] Servidor preparado com .NET 8
- [ ] SQL Server configurado
- [ ] Conectividade com servidor RM testada
- [ ] Permissões de firewall configuradas
- [ ] Backup de configurações existentes

### 🚀 Durante Implantação

- [ ] Arquivos copiados corretamente
- [ ] `appsettings.json` configurado
- [ ] Connection string testada
- [ ] Aplicação inicializada com sucesso
- [ ] Interface web acessível

### ✔️ Pós-Implantação

- [ ] Primeiro usuário administrador criado
- [ ] Testes de conexão RM funcionando
- [ ] Templates básicos criados
- [ ] Usuários finais treinados
- [ ] Documentação entregue à equipe

---

*Esta documentação foi criada para auxiliar na utilização completa do sistema RM_Integrador. Para dúvidas específicas ou problemas não cobertos neste manual, entre em contato com a equipe de suporte.*

**Versão do Documento**: 1.0  
**Data de Criação**: Agosto 2025  
**Última Atualização**: Agosto 2025
