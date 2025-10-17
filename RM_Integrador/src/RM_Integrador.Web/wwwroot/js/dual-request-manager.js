/**
 * Sistema de Requisições Dual - RM Local + Servidor
 * Permite fazer requisições diretamente no RM local do usuário com fallback para servidor
 */

class DualRequestManager {
    constructor() {
        this.preferLocal = true;
        this.localRMUrl = null;
        this.serverRMUrl = '/TestRequests/ExecutePost'; // Endpoint atual do servidor
        this.isDetecting = false;
        this.detectionResults = {
            isLocalAvailable: false,
            localUrl: null,
            localVersion: null,
            detectionTime: null
        };
        
        // Configurações padrão
        this.config = {
            commonPorts: [8051, 8050, 8052, 8053],
            timeout: 5000,
            retryCount: 2,
            detectionTimeout: 10000
        };

        // Não inicializar detecção automática - será feita manualmente quando necessário
    }

    /**
     * Detecta automaticamente se há um RM local disponível
     */
    async detectLocalRM() {
        if (this.isDetecting) return this.detectionResults;
        
        this.isDetecting = true;
        this.updateStatus('Detectando RM local...', 'info');
        
        try {
            for (const port of this.config.commonPorts) {
                const baseUrl = `http://localhost:${port}`;
                
                if (await this.testRMConnection(baseUrl)) {
                    this.localRMUrl = `${baseUrl}/rmsrestdataserver/rest`;
                    this.detectionResults = {
                        isLocalAvailable: true,
                        localUrl: this.localRMUrl,
                        localVersion: await this.getRMVersion(baseUrl),
                        detectionTime: new Date()
                    };
                    
                    this.updateStatus(`RM local detectado em ${baseUrl}`, 'success');
                    this.isDetecting = false;
                    return this.detectionResults;
                }
            }
            
            // Não encontrou RM local
            this.detectionResults = {
                isLocalAvailable: false,
                localUrl: null,
                localVersion: null,
                detectionTime: new Date()
            };
            
            this.updateStatus('RM local não detectado, usando servidor', 'warning');
            
        } catch (error) {
            console.warn('Erro na detecção de RM local:', error);
            this.updateStatus('Erro na detecção, usando servidor', 'warning');
        } finally {
            this.isDetecting = false;
        }
        
        return this.detectionResults;
    }

    /**
     * Testa se uma URL do RM está respondendo
     */
    async testRMConnection(baseUrl) {
        try {
            const controller = new AbortController();
            const timeout = setTimeout(() => controller.abort(), this.config.timeout);
            
            const response = await fetch(`${baseUrl}/rmsrestdataserver/rest`, {
                method: 'GET',
                signal: controller.signal,
                mode: 'cors'
            });
            
            clearTimeout(timeout);
            return response.ok || response.status === 401; // 401 é esperado sem auth
            
        } catch (error) {
            // Tentar com no-cors para contornar CORS
            try {
                const controller = new AbortController();
                const timeout = setTimeout(() => controller.abort(), this.config.timeout);
                
                await fetch(`${baseUrl}/rmsrestdataserver/rest`, {
                    method: 'HEAD',
                    signal: controller.signal,
                    mode: 'no-cors'
                });
                
                clearTimeout(timeout);
                return true; // Se não deu erro, assume que está disponível
                
            } catch (noCorsError) {
                return false;
            }
        }
    }

    /**
     * Tenta obter a versão do RM local
     */
    async getRMVersion(baseUrl) {
        try {
            const response = await fetch(`${baseUrl}/rmsrestdataserver/rest`, {
                method: 'GET',
                timeout: this.config.timeout
            });
            
            // Extrair versão do cabeçalho ou resposta se possível
            const serverHeader = response.headers.get('Server');
            return serverHeader || 'Desconhecida';
            
        } catch (error) {
            return 'Desconhecida';
        }
    }

    /**
     * Executa uma requisição usando o sistema dual
     */
    async executeRequest(dataServerName, postData, executionMode = 'auto') {
        const startTime = Date.now();
        let source = 'unknown';
        let error = null;
        
        try {
            // Determinar qual método usar
            if (executionMode === 'server') {
                source = 'server';
                return await this.executeServerRequest(dataServerName, postData);
            }
            
            if (executionMode === 'local') {
                source = 'local';
                try {
                    // Usar o método com fallback para proxy
                    return await this.executeLocalRequestWithFallback(dataServerName, postData);
                } catch (localError) {
                    console.error('Requisição local falhou mesmo com proxy:', localError);
                    throw localError;
                }
            }
            
            // Modo automático: tenta local primeiro, depois servidor
            if (this.detectionResults.isLocalAvailable) {
                try {
                    source = 'local';
                    // Usar o método com fallback para proxy
                    const result = await this.executeLocalRequestWithFallback(dataServerName, postData);
                    this.logRequest(dataServerName, postData, result, source, Date.now() - startTime, null);
                    return result;
                } catch (localError) {
                    console.warn('Requisição local falhou, tentando servidor...', localError);
                    error = localError;
                }
            }
            
            // Fallback para servidor
            source = 'server';
            const result = await this.executeServerRequest(dataServerName, postData);
            this.logRequest(dataServerName, postData, result, source, Date.now() - startTime, error);
            return result;
            
        } catch (finalError) {
            this.logRequest(dataServerName, postData, null, source, Date.now() - startTime, finalError);
            throw finalError;
        }
    }

    /**
     * Executa requisição diretamente no RM local
     */
    async executeLocalRequest(dataServerName, postData) {
        if (!this.detectionResults.isLocalAvailable) {
            throw new Error('RM local não está disponível');
        }

        // Obter credenciais do usuário
        const credentials = await this.getLocalCredentials();
        
        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 300000); // 5 minutos
        
        try {
            // Preparar headers com autenticação completa
            const headers = {
                'Content-Type': 'application/json',
                'Authorization': `Basic ${btoa(`${credentials.username}:${credentials.password}`)}`,
                'Accept': 'application/json'
            };
            
            // Adicionar CODCOLIGADA se disponível
            if (credentials.codColigada) {
                headers['CODCOLIGADA'] = credentials.codColigada;
            }
            
            console.log(`Executando requisição local para ${dataServerName}...`);
            console.log(`URL: ${this.localRMUrl}/${dataServerName}`);
            console.log(`Usuário: ${credentials.username}`);
            console.log(`CODCOLIGADA: ${credentials.codColigada || 'não informado'}`);
            
            const response = await fetch(`${this.localRMUrl}/${dataServerName}`, {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(postData),
                signal: controller.signal
            });
            
            clearTimeout(timeout);
            
            // Log da resposta para debug
            const responseText = await response.text();
            console.log(`Resposta do RM local (${response.status}):`, responseText);
            
            if (!response.ok) {
                let errorMessage = `Erro na requisição local: ${response.status} ${response.statusText}`;
                
                try {
                    const errorData = JSON.parse(responseText);
                    if (errorData.message) {
                        errorMessage += ` - ${errorData.message}`;
                    }
                    if (errorData.detailedMessage) {
                        errorMessage += ` (${errorData.detailedMessage})`;
                    }
                } catch (parseError) {
                    errorMessage += ` - ${responseText}`;
                }
                
                throw new Error(errorMessage);
            }
            
            return {
                success: true,
                data: responseText,
                source: 'local',
                url: `${this.localRMUrl}/${dataServerName}`
            };
            
        } catch (error) {
            clearTimeout(timeout);
            
            if (error.name === 'AbortError') {
                throw new Error('Requisição local cancelada por timeout');
            }
            
            throw new Error(`Falha na requisição local: ${error.message}`);
        }
    }

    /**
     * Executa requisição via servidor (método atual)
     */
    async executeServerRequest(dataServerName, postData) {
        const response = await fetch('/TestRequests/ExecutePost', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                dataServerName: dataServerName,
                postData: postData
            })
        });

        const result = await response.json();
        
        if (!result.success) {
            throw new Error(result.error || 'Erro na requisição do servidor');
        }
        
        return {
            success: true,
            data: result.data,
            source: 'server',
            url: 'servidor'
        };
    }

    /**
     * Obtém credenciais para RM local
     */
    async getLocalCredentials() {
        // Tentar obter credenciais do servidor primeiro
        try {
            const response = await fetch('/api/rm-credentials', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });
            
            if (response.ok) {
                const credentials = await response.json();
                return {
                    username: credentials.username,
                    password: credentials.password,
                    codColigada: credentials.codColigada
                };
            }
        } catch (error) {
            console.warn('Erro ao obter credenciais do servidor:', error);
        }
        
        // Fallback para credenciais padrão (deve ser configurado pelo usuário)
        const savedCredentials = localStorage.getItem('rm-local-credentials');
        if (savedCredentials) {
            return JSON.parse(savedCredentials);
        }
        
        // Padrão temporário - deve ser substituído por interface de configuração
        return {
            username: 'mestre',
            password: 'totvs',
            codColigada: '1'
        };
    }

    /**
     * Registra a requisição para auditoria
     */
    logRequest(dataServerName, postData, result, source, duration, error) {
        const logEntry = {
            timestamp: new Date().toISOString(),
            dataServerName: dataServerName,
            postData: postData,
            result: result,
            source: source,
            duration: duration,
            error: error ? error.message : null,
            success: result !== null
        };
        
        console.log('Request Log:', logEntry);
        
        // TODO: Enviar para servidor para persistência
        this.saveToLocalStorage(logEntry);
    }

    /**
     * Salva log no localStorage temporariamente
     */
    saveToLocalStorage(logEntry) {
        try {
            const logs = JSON.parse(localStorage.getItem('rm-request-logs') || '[]');
            logs.push(logEntry);
            
            // Manter apenas os últimos 100 logs
            if (logs.length > 100) {
                logs.splice(0, logs.length - 100);
            }
            
            localStorage.setItem('rm-request-logs', JSON.stringify(logs));
        } catch (error) {
            console.warn('Erro ao salvar log local:', error);
        }
    }

    /**
     * Atualiza o status na interface
     */
    updateStatus(message, type = 'info') {
        const statusElement = document.getElementById('rm-connection-status');
        if (statusElement) {
            statusElement.className = `alert alert-${type}`;
            statusElement.innerHTML = `<i class="fas fa-${this.getStatusIcon(type)}"></i> ${message}`;
        }
        
        console.log(`[RM Status] ${message}`);
    }

    /**
     * Retorna ícone apropriado para o status
     */
    getStatusIcon(type) {
        const icons = {
            info: 'info-circle',
            success: 'check-circle',
            warning: 'exclamation-triangle',
            danger: 'exclamation-circle'
        };
        return icons[type] || 'info-circle';
    }

    /**
     * Força re-detecção do RM local
     */
    async redetectLocalRM() {
        try {
            const response = await fetch('/api/environment/redetect', { method: 'POST' });
            const result = await response.json();
            
            if (result.success) {
                this.environmentInfo = result.data;
                if (result.data.isLocalRMAvailable) {
                    this.localRMUrl = result.data.recommendedRMUrl;
                    this.detectionResults = {
                        isLocalAvailable: true,
                        localUrl: result.data.recommendedRMUrl,
                        localVersion: 'Re-detectado',
                        detectionTime: new Date(result.data.detectionTime)
                    };
                } else {
                    this.detectionResults.isLocalAvailable = false;
                    this.localRMUrl = null;
                }
                return this.detectionResults;
            } else {
                throw new Error(result.error);
            }
        } catch (error) {
            console.warn('Erro na re-detecção via API, tentando método legado:', error);
            // Fallback para método antigo
            this.detectionResults.isLocalAvailable = false;
            this.localRMUrl = null;
            return await this.detectLocalRM();
        }
    }

    /**
     * Obtém informações completas do ambiente
     */
    async getEnvironmentInfo() {
        try {
            const response = await fetch('/api/environment/info');
            console.log('Response do environment/info:', response.status);
            
            if (response.ok) {
                const result = await response.json();
                console.log('Dados do ambiente obtidos:', result);
                
                if (result.success) {
                    this.environmentInfo = result.data;
                    
                    // Atualizar as informações com base na detecção local também
                    if (this.detectionResults.isLocalAvailable) {
                        this.environmentInfo.isLocalRMAvailable = true;
                        this.environmentInfo.currentRMUrl = this.detectionResults.localUrl;
                    }
                    
                    return this.environmentInfo;
                } else {
                    console.warn('API retornou erro:', result.error);
                }
            } else {
                console.warn('Erro ao obter ambiente:', response.status, response.statusText);
            }
        } catch (error) {
            console.warn('Erro ao obter informações de ambiente:', error);
        }
        
        // Se chegamos aqui, ou a API falhou ou não há ambiente configurado
        // Usar informações da detecção local como fallback
        const fallbackInfo = {
            environment: 'Local Development',
            isProduction: false,
            isLocalRMAvailable: this.detectionResults.isLocalAvailable,
            availableRMPorts: this.config.commonPorts,
            currentRMUrl: this.detectionResults.localUrl || 'Não disponível'
        };
        
        this.environmentInfo = fallbackInfo;
        return fallbackInfo;
    }

    /**
     * Obtém status atual da detecção
     */
    getDetectionStatus() {
        return {
            ...this.detectionResults,
            isDetecting: this.isDetecting,
            config: this.config
        };
    }

    /**
     * Configura preferências do usuário
     */
    setPreferences(preferences) {
        this.preferLocal = preferences.preferLocal !== false;
        if (preferences.config) {
            this.config = { ...this.config, ...preferences.config };
        }
    }

    /**
     * Configura credenciais locais do usuário
     */
    setLocalCredentials(username, password, codColigada = '1') {
        const credentials = {
            username: username,
            password: password,
            codColigada: codColigada
        };
        
        localStorage.setItem('rm-local-credentials', JSON.stringify(credentials));
        this.updateStatus('Credenciais locais atualizadas', 'success');
    }

    /**
     * Limpa credenciais locais armazenadas
     */
    clearLocalCredentials() {
        localStorage.removeItem('rm-local-credentials');
        this.updateStatus('Credenciais locais removidas', 'info');
    }

    /**
     * Testa credenciais com o RM local
     */
    async testLocalCredentials(username, password, codColigada = '1') {
        if (!this.detectionResults.isLocalAvailable) {
            throw new Error('RM local não detectado');
        }
        
        try {
            const headers = {
                'Content-Type': 'application/json',
                'Authorization': `Basic ${btoa(`${username}:${password}`)}`,
                'Accept': 'application/json'
            };
            
            if (codColigada) {
                headers['CODCOLIGADA'] = codColigada;
            }
            
            const response = await fetch(`${this.localRMUrl}/FopFuncData`, {
                method: 'GET',
                headers: headers
            });
            
            if (response.ok || response.status === 200) {
                return { success: true, message: 'Credenciais válidas' };
            } else {
                const errorText = await response.text();
                return { 
                    success: false, 
                    message: `Erro ${response.status}: ${errorText}` 
                };
            }
            
        } catch (error) {
            return { 
                success: false, 
                message: `Erro na conexão: ${error.message}` 
            };
        }
    }

    /**
     * Executa requisição via proxy para contornar problemas de CORS
     */
    async executeViaProxy(dataServerName, jsonData, method = 'POST') {
        try {
            console.log(`Executando requisição via proxy para ${dataServerName}...`);
            
            const response = await fetch('/api/proxy/forward-rm-request', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    method: method,
                    dataServerName: dataServerName,
                    jsonData: jsonData,
                    filter: null // Apenas para GET
                })
            });

            if (!response.ok) {
                const errorText = await response.text();
                console.error(`Erro no proxy (${response.status}):`, errorText);
                
                let errorMessage = `Erro na requisição proxy: ${response.status}`;
                try {
                    const errorData = JSON.parse(errorText);
                    if (errorData.error) {
                        errorMessage += ` - ${errorData.error}`;
                    }
                } catch (e) {
                    errorMessage += ` - ${errorText.substring(0, 100)}`;
                }
                
                throw new Error(errorMessage);
            }

            const result = await response.json();
            
            if (!result.success) {
                throw new Error(result.error || 'Erro na requisição via proxy');
            }
            
            return {
                success: true,
                data: result.data,
                source: 'proxy',
                url: dataServerName
            };
        } catch (error) {
            console.error('Erro no proxy:', error);
            throw error;
        }
    }

    /**
     * Versão melhorada do método executeLocalRequest que tenta o proxy em caso de falha CORS
     */
    async executeLocalRequestWithFallback(dataServerName, postData) {
        try {
            // Primeiro tenta diretamente
            return await this.executeLocalRequest(dataServerName, postData);
        } catch (error) {
            console.warn(`Falha na requisição local direta: ${error.message}`);
            
            // Se parece um erro de CORS, tenta via proxy
            if (error.message.includes('CORS') || 
                error.message.includes('NetworkError') || 
                error.message.includes('Failed to fetch') ||
                error.message.includes('cross-origin')) {
                
                console.log('Possível erro de CORS, tentando via proxy...');
                return await this.executeViaProxy(dataServerName, JSON.stringify(postData));
            }
            
            // Outros erros são repassados
            throw error;
        }
    }
}

// Instância global
window.dualRequestManager = new DualRequestManager();
