using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RM_Integrador.Web.Services;
using System.Text.Json;
using RM_Integrador.Shared.Models; // Para DataServerInfo
using ConsumoDS.Services; // Para outros serviços do ConsumoDS
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace RM_Integrador.Web.Controllers
{
    [AllowAnonymous]
    [Route("TestRequests")]
    public class TestRequestsController : Controller
    {
        private readonly IDataServerService _dataServerService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TestRequestsController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public TestRequestsController(
            IDataServerService dataServerService,
            IConfiguration configuration,
            ILogger<TestRequestsController> logger,
            IHttpClientFactory httpClientFactory) // Alterado aqui
        {
            _dataServerService = dataServerService;
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory; // Alterado aqui
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            _logger.LogInformation("TestRequests Index action chamada");
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na action Index do TestRequests");
                throw;
            }
        }

        // Action específica para carregar conteúdo em abas
        [HttpGet("TabContent")]
        public IActionResult TabContent()
        {
            return View("TabContent");
        }

        // Método de teste para verificar se o roteamento está funcionando
        [HttpGet("TestConnection")]
        public IActionResult TestConnection()
        {
            return Json(new { success = true, message = "Conexão funcionando!" });
        }

        // Endpoint para detectar RM local/remoto
        // Método para detectar RM local
        private async Task<string> DetectLocalRMUrl()
        {
            var commonPorts = new[] { 8051, 8052, 8053 };
            var httpClient = _httpClientFactory.CreateClient("default");
            
            foreach (var port in commonPorts)
            {
                try
                {
                    var testUrl = $"http://localhost:{port}/rmsrestdataserver/rest";
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                    
                    // Testa se o endpoint está acessível
                    var response = await httpClient.GetAsync($"{testUrl}/status", cts.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"RM Local encontrado na porta {port}");
                        return testUrl;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"RM não encontrado na porta {port}: {ex.Message}");
                }
            }
            
            return null;
        }

        [HttpGet("DetectRM")]
        public async Task<IActionResult> DetectRM()
        {
            try
            {
                var localUrl = await DetectLocalRMUrl();
                
                if (!string.IsNullOrEmpty(localUrl))
                {
                    // RM Local detectado
                    var rmInfo = new
                    {
                        isLocal = true,
                        port = ExtractPortFromUrl(localUrl),
                        localUrl = localUrl,
                        remoteUrl = _configuration["RMSettings:BaseUrl"],
                        status = "connected"
                    };
                    return Json(new { success = true, data = rmInfo });
                }
                else
                {
                    // Apenas RM remoto disponível
                    var rmInfo = new
                    {
                        isLocal = false,
                        port = 8051,
                        remoteUrl = _configuration["RMSettings:BaseUrl"],
                        status = "remote_only"
                    };
                    return Json(new { success = true, data = rmInfo });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao detectar RM");
                return Json(new { success = false, message = "Erro ao detectar RM: " + ex.Message });
            }
        }

        private int ExtractPortFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Port;
            }
            catch
            {
                return 8051; // Fallback
            }
        }

        // Endpoint para obter informações do ambiente
        [HttpGet("GetEnvironmentInfo")]
        public async Task<IActionResult> GetEnvironmentInfo()
        {
            try
            {
                var environmentInfo = new
                {
                    environment = "Desenvolvimento",
                    hasLocalRM = false,
                    currentRMUrl = "http://servidor-rm:8051",
                    availablePorts = new[] { 8051, 8052, 8053 }
                };

                return Json(new { success = true, data = environmentInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar informações do ambiente");
                return Json(new { success = false, message = "Erro ao carregar informações: " + ex.Message });
            }
        }

        [HttpGet("SearchDataServer")]
        public async Task<IActionResult> SearchDataServer(string term)
        {
            try
            {
                Console.WriteLine($"=== SearchDataServer called ===");
                Console.WriteLine($"Raw term parameter: '{term}'");
                Console.WriteLine($"Term is null: {term == null}");
                Console.WriteLine($"Term is empty: {string.IsNullOrEmpty(term)}");
                Console.WriteLine($"Term is whitespace: {string.IsNullOrWhiteSpace(term)}");
                
                if (string.IsNullOrWhiteSpace(term))
                {
                    Console.WriteLine("Term está vazio, retornando erro");
                    return Json(new { 
                        success = false, 
                        error = "Nome do DataServer não fornecido" 
                    });
                }
                
                Console.WriteLine($"Buscando DataServer: '{term}'");
                var response = await _dataServerService.GetDataServerInfoAsync(term);
                
                // Debug detalhado
                Console.WriteLine($"Response é null? {response == null}");
                Console.WriteLine($"Tipo da resposta: {response?.GetType().FullName}");
                Console.WriteLine($"Conteúdo completo: {JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })}");

                var dataServer = response as DataServerInfo;
                Console.WriteLine($"Cast para DataServerInfo funcionou? {dataServer != null}");

                if (dataServer == null)
                {
                    return Json(new { 
                        success = false, 
                        error = "DataServer não encontrado na base local" 
                    });
                }

                // Debug dos dados
                Console.WriteLine($"Nome: {dataServer.Name}");
                Console.WriteLine($"Rotina: {dataServer.Routine}");
                Console.WriteLine($"Chaves Primárias: {string.Join(", ", dataServer.PrimaryKeys ?? new List<string>())}");
                Console.WriteLine($"Tem exemplo POST? {!string.IsNullOrEmpty(dataServer.PostExample)}");

                var result = new
                {
                    name = dataServer.Name,
                    routine = dataServer.Routine,
                    primaryKeys = dataServer.PrimaryKeys ?? new List<string>(),
                    postExample = dataServer.PostExample
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro completo: {ex}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { 
                    success = false, 
                    error = $"Erro ao buscar DataServer: {ex.Message}" 
                });
            }
        }

        [HttpGet("GetDataServerInfo")]
        public async Task<IActionResult> GetDataServerInfo(string dataServerName)
        {
            try
            {
                Console.WriteLine($"=== GetDataServerInfo called ===");
                Console.WriteLine($"DataServerName: '{dataServerName}'");
                
                if (string.IsNullOrWhiteSpace(dataServerName))
                {
                    return Json(new { 
                        success = false, 
                        error = "Nome do DataServer não fornecido" 
                    });
                }
                
                var response = await _dataServerService.GetDataServerInfoAsync(dataServerName);
                var dataServer = response as DataServerInfo;

                if (dataServer == null)
                {
                    return Json(new { 
                        success = false, 
                        error = "DataServer não encontrado" 
                    });
                }

                var result = new
                {
                    name = dataServer.Name,
                    routine = dataServer.Routine,
                    description = dataServer.Description,
                    primaryKeys = dataServer.PrimaryKeys ?? new List<string>(),
                    postExample = dataServer.PostExample,
                    getExample = dataServer.GetExample,
                    requiresFilter = dataServer.RequiresFilter
                };

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao buscar informações do DataServer: {ex}");
                return Json(new { 
                    success = false, 
                    error = $"Erro ao buscar DataServer: {ex.Message}" 
                });
            }
        }

        [HttpGet("ListDataServers")]
        public async Task<IActionResult> ListDataServers()
        {
            try
            {
                Console.WriteLine("=== ListDataServers called ===");
                
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT Name, Routine, Description
                    FROM DataServers 
                    ORDER BY Name";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                var dataServers = new List<object>();
                
                while (await reader.ReadAsync())
                {
                    dataServers.Add(new
                    {
                        name = reader.GetString(0),
                        routine = reader.GetString(1),
                        description = reader.IsDBNull(2) ? "" : reader.GetString(2)
                    });
                }

                Console.WriteLine($"Encontrados {dataServers.Count} DataServers");
                return Json(new { success = true, data = dataServers });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao listar DataServers: {ex.Message}");
                return Json(new { 
                    success = false, 
                    error = $"Erro ao listar DataServers: {ex.Message}" 
                });
            }
        }

        [HttpPost]
        [Route("ExecuteGet")]
        public async Task<IActionResult> ExecuteGet([FromBody] GetRequestModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.DataServerName))
                    throw new ArgumentNullException(nameof(model.DataServerName));

                var username = _configuration["RMSettings:Username"];
                var password = _configuration["RMSettings:Password"];

                // Adiciona apenas o CODCOLIGADA nos headers customizados
                var customHeaders = new Dictionary<string, string>();
                var codColigada = _configuration["RMSettings:CODCOLIGADA"];
                if (!string.IsNullOrEmpty(codColigada))
                {
                    customHeaders.Add("CODCOLIGADA", codColigada);
                }

                // Determina a URL baseada no modo de execução
                string baseUrl;
                string executionDetails;
                
                _logger.LogInformation($"=== TESTE GET INICIADO ===");
                _logger.LogInformation($"DataServer: {model.DataServerName}");
                _logger.LogInformation($"Filtro: {model.Filter}");
                _logger.LogInformation($"Modo solicitado: {model.ExecutionMode}");
                _logger.LogInformation($"LocalRMUrl fornecida: {model.LocalRMUrl ?? "null"}");
                
                if (model.ExecutionMode == "local")
                {
                    if (!string.IsNullOrEmpty(model.LocalRMUrl))
                    {
                        // Usa a URL detectada pelo browser do usuário
                        baseUrl = model.LocalRMUrl;
                        executionDetails = $"RM Local (detectado no browser): {baseUrl}";
                        _logger.LogInformation($"✅ {executionDetails}");
                    }
                    else
                    {
                        // Fallback: tenta detectar RM local no servidor (comportamento antigo)
                        baseUrl = await DetectLocalRMUrl();
                        if (string.IsNullOrEmpty(baseUrl))
                        {
                            _logger.LogWarning("❌ RM Local não encontrado nem no browser nem no servidor");
                            return Json(new { 
                                success = false, 
                                message = "RM Local não encontrado. Verifique se o RM está rodando localmente nas portas 8051, 8052 ou 8053.",
                                executionMode = "local",
                                usedUrl = ""
                            });
                        }
                        executionDetails = $"RM Local (detectado no servidor): {baseUrl}";
                        _logger.LogInformation($"⚠️ {executionDetails}");
                    }
                }
                else
                {
                    // Usa a URL do servidor configurada no appsettings
                    baseUrl = _configuration["RMSettings:BaseUrl"];
                    executionDetails = $"RM Remoto (configurado): {baseUrl}";
                    _logger.LogInformation($"🌐 {executionDetails}");
                }

                // Cria uma nova instância do serviço com HttpClient configurado
                var httpClient = _httpClientFactory.CreateClient("default");
                var rmService = new RM_Integrador.Web.Services.ConfiguredDataServerService(
                    httpClient,
                    baseUrl,  // URL determinada acima
                    username,
                    password,
                    customHeaders  // Content-Type será configurado internamente pelo serviço
                );

                _logger.LogInformation($"🔄 Executando GET: {baseUrl}/{model.DataServerName}" + (string.IsNullOrEmpty(model.Filter) ? "" : $"/{model.Filter}"));

                // Executa o GET diretamente (autenticação está nos headers)
                var response = await rmService.ExecuteGetAsync(
                    model.DataServerName,
                    model.Filter
                );

                _logger.LogInformation($"✅ GET executado com sucesso!");
                _logger.LogInformation($"=== TESTE GET CONCLUÍDO ===");

                return Json(new { 
                    success = true, 
                    data = FormatJson(response),
                    usedUrl = baseUrl,
                    executionMode = model.ExecutionMode,
                    executionDetails = executionDetails
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro na requisição GET: {ex.Message}");
                return Json(new { success = false, error = $"Erro na requisição GET: {ex.Message}" });
            }
        }

        // Novo endpoint para preparar o JSON (sem enviar)
        [HttpPost]
        [Route("TestRequests/PreparePost")]
        public async Task<IActionResult> PreparePost([FromBody] JsonElement requestData)
        {
            try
            {
                // Parse do request data
                if (!requestData.TryGetProperty("dataServerName", out JsonElement dataServerNameElement) ||
                    !requestData.TryGetProperty("postData", out JsonElement postDataElement))
                {
                    return Json(new { 
                        success = false, 
                        error = "Dados de entrada inválidos. Esperado: dataServerName e postData" 
                    });
                }

                var dataServerName = dataServerNameElement.GetString();
                _logger.LogInformation($"Preparando JSON para POST: {dataServerName}");

                // 1. Busca o DataServer
                var response = await _dataServerService.GetDataServerInfoAsync(dataServerName);
                var dataServer = response as DataServerInfo;
                
                if (dataServer == null)
                {
                    return Json(new { 
                        success = false, 
                        error = "DataServer não encontrado" 
                    });
                }

                // 2. Obtém o JSON exemplo
                var postExample = dataServer.PostExample;
                if (string.IsNullOrEmpty(postExample))
                {
                    return Json(new { 
                        success = false, 
                        error = "Exemplo de POST não encontrado para este DataServer" 
                    });
                }

                // 3. Monta o JSON final com as chaves primárias fornecidas
                var baseJson = JsonSerializer.Deserialize<JsonElement>(postExample);
                var finalJson = baseJson;

                // 4. Atualiza os valores das chaves primárias no JSON
                foreach (var key in dataServer.PrimaryKeys)
                {
                    if (postDataElement.TryGetProperty(key, out JsonElement value))
                    {
                        finalJson = UpdateJsonValue(finalJson, key, value);
                    }
                }

                var finalJsonString = JsonSerializer.Serialize(finalJson, new JsonSerializerOptions { WriteIndented = true });

                return Json(new { 
                    success = true, 
                    data = new {
                        preparedJson = finalJsonString,
                        dataServerName = dataServerName,
                        primaryKeysUsed = dataServer.PrimaryKeys,
                        exampleJson = postExample
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao preparar JSON: {ex}");
                return Json(new { 
                    success = false, 
                    error = $"Erro ao preparar JSON: {ex.Message}" 
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExecutePost([FromBody] PostRequestModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.DataServerName))
                    throw new ArgumentNullException(nameof(model.DataServerName));

                // Determinar a URL base conforme o modo de execução
                string baseUrl;
                string executionDetails;
                
                _logger.LogInformation($"=== TESTE POST INICIADO ===");
                _logger.LogInformation($"DataServer: {model.DataServerName}");
                _logger.LogInformation($"Modo solicitado: {model.ExecutionMode}");
                _logger.LogInformation($"LocalRMUrl fornecida: {model.LocalRMUrl ?? "null"}");
                
                if (model.ExecutionMode == "local")
                {
                    if (!string.IsNullOrEmpty(model.LocalRMUrl))
                    {
                        // Usa a URL detectada pelo browser do usuário
                        baseUrl = model.LocalRMUrl;
                        executionDetails = $"RM Local (detectado no browser): {baseUrl}";
                        _logger.LogInformation($"✅ {executionDetails}");
                    }
                    else
                    {
                        // Fallback: tenta detectar RM local no servidor (comportamento antigo)
                        var localRmUrl = await DetectLocalRMUrl();
                        if (!string.IsNullOrEmpty(localRmUrl))
                        {
                            baseUrl = localRmUrl;
                            executionDetails = $"RM Local (detectado no servidor): {baseUrl}";
                            _logger.LogInformation($"⚠️ {executionDetails}");
                        }
                        else
                        {
                            _logger.LogWarning("❌ RM Local não encontrado nem no browser nem no servidor");
                            return Json(new { 
                                success = false, 
                                error = "RM local não detectado. Verifique se o RM está executando localmente nas portas 8051, 8052 ou 8053.",
                                executionMode = "local",
                                usedUrl = ""
                            });
                        }
                    }
                }
                else
                {
                    // Usar configuração do servidor
                    baseUrl = _configuration["RMSettings:BaseUrl"];
                    executionDetails = $"RM Remoto (configurado): {baseUrl}";
                    _logger.LogInformation($"🌐 {executionDetails}");
                }

                // 1. Busca o DataServer e faz o cast
                var response = await _dataServerService.GetDataServerInfoAsync(model.DataServerName);
                var dataServer = response as DataServerInfo;
                
                if (dataServer == null)
                {
                    return Json(new { 
                        success = false, 
                        error = "DataServer não encontrado" 
                    });
                }

                // 2. Obtém o JSON exemplo e verifica
                var postExample = dataServer.PostExample;
                if (string.IsNullOrEmpty(postExample))
                {
                    return Json(new { 
                        success = false, 
                        error = "Exemplo de POST não encontrado para este DataServer" 
                    });
                }

                // 3. Processa o JSON exemplo com as chaves primárias
                var baseJson = JsonSerializer.Deserialize<JsonElement>(postExample);
                var finalJson = baseJson;

                // 4. Atualiza os valores das chaves primárias no JSON
                foreach (var key in dataServer.PrimaryKeys)
                {
                    if (model.PostData.TryGetProperty(key, out JsonElement value))
                    {
                        finalJson = UpdateJsonValue(finalJson, key, value);
                    }
                }

                // 5. Configura e autentica no RM
                var username = _configuration["RMSettings:Username"];
                var password = _configuration["RMSettings:Password"];

                var customHeaders = new Dictionary<string, string>();
                var codColigada = _configuration["RMSettings:CODCOLIGADA"];
                if (!string.IsNullOrEmpty(codColigada))
                {
                    customHeaders.Add("CODCOLIGADA", codColigada);
                }

                var httpClient = _httpClientFactory.CreateClient("default");
                var rmService = new RM_Integrador.Web.Services.ConfiguredDataServerService(
                    httpClient,
                    baseUrl,  // URL dinâmica (local ou configurada)
                    username,
                    password,
                    customHeaders
                );

                // 6. Executa o POST no RM (autenticação está nos headers)
                var jsonToSend = JsonSerializer.Serialize(finalJson);
                
                _logger.LogInformation($"🔄 Executando POST: {baseUrl}/{model.DataServerName}");
                _logger.LogInformation($"📤 Dados a enviar: {jsonToSend}");

                var rmResponse = await rmService.ExecutePostAsync(
                    model.DataServerName,
                    jsonToSend
                );

                _logger.LogInformation($"✅ POST executado com sucesso!");
                _logger.LogInformation($"📥 Resposta: {rmResponse}");
                _logger.LogInformation($"=== TESTE POST CONCLUÍDO ===");

                return Json(new { 
                    success = true, 
                    data = FormatJson(rmResponse),
                    usedUrl = baseUrl,
                    executionMode = model.ExecutionMode,
                    executionDetails = executionDetails
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("A requisição POST excedeu o tempo limite");
                return Json(new { 
                    success = false, 
                    error = "A requisição excedeu o tempo limite de 5 minutos" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro completo no POST: {ex}");
                return Json(new { 
                    success = false, 
                    error = $"Erro na requisição POST: {ex.Message}" 
                });
            }
        }

        // Endpoint para enviar JSON já preparado
        [HttpPost]
        [Route("TestRequests/SendPreparedPost")]
        public async Task<IActionResult> SendPreparedPost([FromBody] JsonElement requestData)
        {
            try
            {
                _logger.LogInformation($"Recebido request para SendPreparedPost");
                _logger.LogInformation($"Conteúdo da requisição: {JsonSerializer.Serialize(requestData)}");
                
                // Parse do request data
                if (!requestData.TryGetProperty("dataServerName", out JsonElement dataServerNameElement) ||
                    !requestData.TryGetProperty("jsonData", out JsonElement jsonDataElement))
                {
                    _logger.LogError("Dados de entrada inválidos - propriedades obrigatórias ausentes");
                    return Json(new { 
                        success = false, 
                        error = "Dados de entrada inválidos. Esperado: dataServerName e jsonData" 
                    });
                }

                var dataServerName = dataServerNameElement.GetString();
                var jsonData = jsonDataElement.GetString();

                if (string.IsNullOrEmpty(dataServerName))
                {
                    _logger.LogError("Nome do DataServer não pode ser vazio");
                    return Json(new { success = false, error = "Nome do DataServer não pode ser vazio" });
                }

                if (string.IsNullOrEmpty(jsonData))
                {
                    _logger.LogError("JSON não pode ser vazio");
                    return Json(new { success = false, error = "JSON não pode ser vazio" });
                }

                // Validar se é um JSON válido
                try
                {
                    var _ = JsonSerializer.Deserialize<JsonElement>(jsonData);
                }
                catch (JsonException jex)
                {
                    _logger.LogError($"JSON inválido: {jex.Message}");
                    return Json(new { success = false, error = $"JSON inválido: {jex.Message}" });
                }

                _logger.LogInformation($"Enviando JSON preparado para {dataServerName}");

                // 1. Configura e autentica no RM
                var username = _configuration["RMSettings:Username"];
                var password = _configuration["RMSettings:Password"];
                var baseUrl = _configuration["RMSettings:BaseUrl"];
                
                if (string.IsNullOrEmpty(baseUrl))
                {
                    _logger.LogError("URL do RM não configurada");
                    return Json(new { success = false, error = "URL do RM não configurada no sistema" });
                }
                
                _logger.LogInformation($"Usando URL do RM para POST: {baseUrl}");

                var customHeaders = new Dictionary<string, string>();
                var codColigada = _configuration["RMSettings:CODCOLIGADA"];
                if (!string.IsNullOrEmpty(codColigada))
                {
                    customHeaders.Add("CODCOLIGADA", codColigada);
                    _logger.LogInformation($"Adicionado CODCOLIGADA: {codColigada} nos headers");
                }

                var httpClient = _httpClientFactory.CreateClient("default");
                
                // Configurar timeout mais longo para o HttpClient
                httpClient.Timeout = TimeSpan.FromMinutes(5);
                
                var rmService = new RM_Integrador.Web.Services.ConfiguredDataServerService(
                    httpClient,
                    baseUrl,
                    username,
                    password,
                    customHeaders
                );

                // 2. Executa o POST no RM
                _logger.LogInformation($"Iniciando POST para {dataServerName}");
                _logger.LogInformation($"JSON a enviar: {jsonData}");

                var rmResponse = await rmService.ExecutePostAsync(
                    dataServerName,
                    jsonData
                );

                _logger.LogInformation($"POST concluído com sucesso. Resposta: {rmResponse}");

                return Json(new { 
                    success = true, 
                    data = FormatJson(rmResponse) 
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Erro na comunicação HTTP: {ex.Message}");
                string detailedError = ex.InnerException != null ? 
                    $"{ex.Message} - {ex.InnerException.Message}" : ex.Message;
                
                return Json(new { 
                    success = false, 
                    error = $"Erro de comunicação com o RM: {detailedError}" 
                });
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("A requisição POST foi cancelada por timeout");
                return Json(new { 
                    success = false, 
                    error = "A requisição excedeu o tempo limite de 5 minutos" 
                });
            }
            catch (OperationCanceledException)
            {
                _logger.LogError("A requisição POST foi cancelada");
                return Json(new { 
                    success = false, 
                    error = "A requisição foi cancelada" 
                });
            }
            catch (JsonException jex)
            {
                _logger.LogError($"Erro no processamento do JSON: {jex.Message}");
                return Json(new { 
                    success = false, 
                    error = $"Erro no formato do JSON: {jex.Message}" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro detalhado ao enviar JSON preparado: {ex}");
                return Json(new { 
                    success = false, 
                    error = $"Erro na requisição POST: {ex.Message}" 
                });
            }
        }

        // Endpoint para testar conexão com RM local
        [HttpGet("api/test-rm-connection")]
        public async Task<IActionResult> TestRMConnection()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("default");
                var localRmUrl = "http://localhost:8080"; // URL padrão do RM local
                
                // Testa conexão básica
                var response = await httpClient.GetAsync($"{localRmUrl}/api/status");
                
                return Json(new { 
                    success = response.IsSuccessStatusCode,
                    status = response.StatusCode,
                    message = response.IsSuccessStatusCode ? "RM local disponível" : "RM local indisponível"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao testar conexão com RM: {ex.Message}");
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        // Endpoint para executar requisição híbrida
        [HttpPost("api/hybrid-request")]
        public async Task<IActionResult> ExecuteHybridRequest([FromBody] HybridRequestModel model)
        {
            try
            {
                var result = new
                {
                    success = true,
                    method = model.ExecutionMode,
                    timestamp = DateTime.Now,
                    data = new { message = "Requisição híbrida executada com sucesso" }
                };

                // Log da requisição
                _logger.LogInformation($"Requisição híbrida executada - Modo: {model.ExecutionMode}, Endpoint: {model.Endpoint}");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro na requisição híbrida: {ex.Message}");
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        // Endpoint para obter credenciais do RM
        [HttpGet("api/rm-credentials")]
        public IActionResult GetRMCredentials()
        {
            try
            {
                var credentials = new
                {
                    username = _configuration["RMSettings:Username"],
                    password = _configuration["RMSettings:Password"],
                    codColigada = _configuration["RMSettings:CODCOLIGADA"] ?? "1"
                };

                return Json(credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao obter credenciais: {ex.Message}");
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        private JsonElement UpdateJsonValue(JsonElement root, string key, JsonElement newValue)
        {
            var doc = JsonDocument.Parse(root.GetRawText());
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                foreach (var property in root.EnumerateObject())
                {
                    if (property.Name == key)
                    {
                        writer.WritePropertyName(property.Name);
                        newValue.WriteTo(writer);
                    }
                    else
                    {
                        writer.WritePropertyName(property.Name);
                        property.Value.WriteTo(writer);
                    }
                }
                writer.WriteEndObject();
            }

            stream.Seek(0, SeekOrigin.Begin);
            return JsonDocument.Parse(stream.ToArray()).RootElement;
        }

        private string FormatJson(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) return "{}";

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            catch
            {
                return json;
            }
        }
    }

    public class GetRequestModel
    {
        public string DataServerName { get; set; }
        public string Filter { get; set; }
        public string ExecutionMode { get; set; } // "local" ou "remote"
        public string? LocalRMUrl { get; set; } // URL do RM local detectada no browser
    }

    public class PostRequestModel
    {
        public string DataServerName { get; set; }
        public JsonElement PostData { get; set; } // Alterado para JsonElement
        public string ExecutionMode { get; set; } // "local" ou "remote"
        public string? LocalRMUrl { get; set; } // URL do RM local detectada no browser
    }

    public class SendPreparedPostModel
    {
        public string DataServerName { get; set; }
        public string JsonData { get; set; }
    }

    public class HybridRequestModel
    {
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public string ExecutionMode { get; set; }
        public object Data { get; set; }
    }
}