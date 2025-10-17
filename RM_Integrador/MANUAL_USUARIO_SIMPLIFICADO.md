# Manual do Usuário - Sistema de Automação DataServer BH

**Ana Christine Ferreira Costa – ana.christine@totvs.com.br**

## Índice
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

## Visão Geral

O **RM_Integrador** é um sistema web que facilita a busca de JSONs e testes com o sistema RM da TOTVS a partir de requisições API Rest. Ele oferece ferramentas para:

- **Buscar JSONs exemplos**
- **Executar requisições** GET e POST de forma simplificada
- **Comparar JSONs** entre dados do cliente e templates base
- **Visualizar informações** de dataservers cadastrados, assim como dicas e documentações
- **Cadastrar novos** dataservers
- **Gerenciar usuários** cadastrados
- **Exportar informações** dos dataserver em um arquivo .csv

---

## Login e Autenticação

### Primeiro Acesso

1. **Acesse o sistema**: 
URL1: http://bh-sup-rhtech:5095/Account/login
URL2: http://10.171.32.234:5095/account/login

2. **Preencher dados**:
   - URL de acesso
   - Usuário
   - Senha (mínimo 1 caractere)


**IMPORTANTE**: Para usar o sistema, seu usuário deve estar cadastrado no RM. Se não estiver, você receberá erro de "Não Autorizado" ao tentar fazer requisições e logar no sistema.

### Tipos de Usuário

- **Usuário Comum**: Acesso às funcionalidades básicas
- **Administrador**: Acesso completo + gerenciamento de usuários

## Funcionalidades Principais

### Dashboard Principal

Após o login, você terá acesso ao menu principal com as seguintes opções:

- **Testes de Requisições**: Realizar testes GET/POST
- **Biblioteca**: Gerenciar templates de requisições
- **Comparar JSONs**: Comparar dados do cliente com templates base
- **Visualizar JSON**: Visualizar dados JSON formatados
- **Perfil**: Gerenciar dados pessoais
- **Admin** (apenas administradores): Gerenciar sistema

---

## Área Administrativa

### Acesso Administrativo

**Somente usuários com perfil de administrador** têm acesso a esta área.

### Gerenciamento de Usuários

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

### Logs do Sistema

- Visualização de logs de acesso
- Monitoramento de atividades
- Histórico de erros e eventos

---

## Biblioteca de Templates

### Objetivo

Armazenar e reutilizar requisições pré-configuradas para agilizar testes.

### Gerenciamento de Templates

#### Criar Novo Template

1. **Acesse**: Menu "Biblioteca"
2. **Clique**: "Novo Template"
3. **Preencha**:
   - Nome do template
   - Descrição
   - DataServer
   - Tipo (GET/POST)
   - Dados JSON (se POST)
   - Filtros (se GET)

#### Editar Template

1. **Localizar**: Template na lista
2. **Clicar**: Botão "Editar"
3. **Modificar**: Campos necessários
4. **Salvar**: Confirmar alterações

#### Excluir Template

1. **Localizar**: Template na lista
2. **Clicar**: Botão "Excluir"
3. **Confirmar**: Ação de exclusão

#### Executar Template

1. **Localizar**: Template desejado
2. **Clicar**: Botão "Executar"
3. **Verificar**: Resultado na nova tela

### Organização

- Templates organizados por tipo (GET/POST)
- Busca por nome ou descrição
- Ordenação por data de criação/modificação

---

## Área Usuário

## Testes de Conexão

### Funcionalidade Principal

A tela de **Testes de Requisições** é o coração do sistema, permitindo:

### Modos de Execução

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

### Tipos de Requisição

#### Requisições GET

1. **Selecionar DataServer**
   - Escolha o dataserver na lista suspensa
   - Exemplos: `FopFuncData`, `RhuPessoData`, etc.

2. **Definir Filtros** (opcional)
   - Campo `Filter`: Condições WHERE SQL
   - Exemplo: `CODCOLIGADA = 1 AND ATIVO = 1`

3. **Executar**
   - Clique em "Executar GET"
   - Aguarde o resultado na aba "Resultado"

#### Requisições POST

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

### Interpretação de Resultados

#### Sucesso
- **Status 200**: Requisição executada com sucesso
- **Dados retornados**: JSON com os dados solicitados
- **Headers**: Informações da resposta

#### Erros Comuns
- **Status 400**: Erro nos parâmetros enviados
- **Status 401**: Erro de autenticação
- **Status 404**: DataServer não encontrado
- **Status 500**: Erro interno do servidor RM

### Interface Visual

- **Indicadores Visuais**: 
  - Verde: Sucesso
  - Amarelo: Aviso
  - Vermelho: Erro

- **Feedback em Tempo Real**:
  - URL sendo utilizada exibida claramente
  - Modo de execução indicado
  - Tempo de resposta mostrado

---

## Comparação de JSONs

### Finalidade

Comparar um JSON enviado pelo cliente com um JSON que você tem como base/template, identificando diferenças entre eles.

### Como Usar

#### Inserir Primeiro JSON

1. **Cole o JSON do Cliente**: Na primeira área de texto
2. **Dar um Nome**: Identifique como "JSON Cliente" ou similar

#### Inserir Segundo JSON

1. **Cole seu JSON Base**: Na segunda área de texto  
2. **Dar um Nome**: Identifique como "JSON Template" ou similar

#### Comparar

1. **Clique em "Comparar"**
2. **Analise as Diferenças**: Sistema mostrará lado a lado

### Entendendo as Diferenças

- **Estrutural**: Mostra se a estrutura dos JSONs é diferente
- **Valores**: Compara os valores dos campos iguais
- **Campos Únicos**: Identifica campos que existem apenas em um dos JSONs

### Visualização

- **Lado a Lado**: JSONs exibidos paralelamente
- **Cores Diferenciadas**: 
  - Verde: Igual em ambos
  - Amarelo: Diferente
  - Vermelho: Ausente em um dos lados

---

## Visualizador JSON

### Objetivo

Visualizar dados JSON de forma organizada e navegável.

### Funcionalidades

#### Visualização Estruturada

1. **Expansão/Contração**: Objetos e arrays
2. **Identação Visual**: Hierarquia clara
3. **Tipos de Dados**: Identificados por cores
4. **Números de Linha**: Para referência

#### Navegação

- **Busca de Campos**: Localizar propriedades específicas
- **Filtros**: Mostrar apenas tipos específicos
- **Expandir Tudo**: Ver estrutura completa
- **Contrair Tudo**: Visão resumida

#### Exportação

- **Copiar**: JSON formatado para clipboard
- **Download**: Arquivo JSON
- **Imprimir**: Versão formatada

### Interface

- **Syntax Highlighting**: Destaque de sintaxe
- **Numeração**: Linhas numeradas
- **Dobramento**: Seções dobráveis
- **Zoom**: Controle de tamanho da fonte

---

## Solução de Problemas

### Problemas Comuns

#### "Erro de Conexão" ou "Timeout"

**O que fazer**:
1. Verifique se você está conectado à rede
2. Tente trocar o modo de execução (Local/Remoto/Automático)
3. Entre em contato com a equipe técnica

#### "Não Autorizado" ou "401"

**O que fazer**:
1. Verifique se seu usuário está cadastrado no RM
2. Entre em contato com o administrador do RM
3. Confirme se você tem permissão para acessar o DataServer

#### "Tela não carrega" ou "Botões não funcionam"

**O que fazer**:
1. Atualize a página (F5)
2. Limpe o cache do navegador
3. Tente usar outro navegador (Chrome, Firefox, Edge)

#### "DataServer não encontrado"

**O que fazer**:
1. Verifique se digitou o nome corretamente
2. Consulte a lista de DataServers disponíveis
3. Entre em contato com a equipe técnica

### Quando Entrar em Contato com Suporte

**Sempre forneça estas informações**:
- O que você estava tentando fazer
- Qual erro apareceu (copie a mensagem completa)
- Que DataServer estava usando
- Horário que o erro aconteceu

---

## Suporte e Contato

Para dúvidas ou problemas, entre em contato com a equipe técnica responsável pelo sistema.

---

*Este manual foi criado para auxiliar na utilização do sistema RM_Integrador. Para dúvidas específicas, entre em contato com a equipe de suporte.*

**Versão do Documento**: 1.0  
**Data de Criação**: Agosto 2025  
**Última Atualização**: Agosto 2025
