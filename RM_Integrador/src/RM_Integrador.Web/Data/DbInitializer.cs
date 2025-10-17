using System.Collections.Generic;
using System.Linq;
using RM_Integrador.Shared.Models;

namespace RM_Integrador.Web.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            Console.WriteLine("🚀 DbInitializer.Initialize() chamado!");
            
            // Certifique-se que o banco existe
            context.Database.EnsureCreated();
            Console.WriteLine("✅ EnsureCreated() executado!");

            // Verifica se já existem dados
            var existingCount = context.DataServers.Count();
            Console.WriteLine($"📊 Contagem atual de DataServers: {existingCount}");
            
            if (!context.DataServers.Any())
            {
                Console.WriteLine("🔄 Tabela DataServers vazia, inserindo dados de exemplo...");
                
                var dataServers = new DataServerInfo[]
                {
                    new DataServerInfo
                    {
                        Name = "EduDataServer",
                        Routine = "edu/getStudentInfo",
                        PrimaryKeys = new List<string> { "id", "code" },
                        Description = "Busca informações do aluno",
                        GetExample = "{ \"id\": 123, \"code\": \"ABC\" }",
                        PostExample = "{ \"name\": \"João\", \"age\": 20 }",
                        RequiresFilter = true,
                        Keywords = new List<string> { "aluno", "estudante", "educacional" }
                    },
                    new DataServerInfo
                    {
                        Name = "HRDataServer",
                        Routine = "hr/getEmployeeData",
                        PrimaryKeys = new List<string> { "employeeId" },
                        Description = "Busca dados do funcionário",
                        GetExample = "{ \"employeeId\": 456 }",
                        PostExample = "{ \"name\": \"Maria\", \"department\": \"TI\" }",
                        RequiresFilter = true,
                        Keywords = new List<string> { "rh", "funcionário", "colaborador" }
                    }
                };

                Console.WriteLine($"📝 Inserindo {dataServers.Length} DataServers...");
                context.DataServers.AddRange(dataServers);
                var savedCount = context.SaveChanges();
                Console.WriteLine($"✅ {savedCount} DataServers inseridos com sucesso!");
            }
            else
            {
                Console.WriteLine("ℹ️ DataServers já existem na base, pulando inicialização.");
            }
        }
    }
}