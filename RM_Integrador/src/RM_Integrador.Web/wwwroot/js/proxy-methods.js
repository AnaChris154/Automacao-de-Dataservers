/**
 * Métodos de proxy para o DualRequestManager
 * Este arquivo contém funções utilitárias para o sistema dual de requisições
 */

// Exportamos as funções para serem usadas no dual-request-manager.js
const ProxyMethods = {
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
    },

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
};

// Tornamos o objeto disponível globalmente
window.ProxyMethods = ProxyMethods;
