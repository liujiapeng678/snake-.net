using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.IO;
using SnakeRunningSystem.Utils;
using System.Linq;
using Microsoft.CodeAnalysis.Emit;



namespace SnakeRunningSystem.Utils
{
    
    public class Consumer
    {
        private readonly HttpClient _httpClient;
        public Consumer(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task RunBotCode(Bot bot)
        {
            try
            {
                string uid = Guid.NewGuid().ToString().Substring(0, 8);

                // 编译和执行用户代码
                var botInterface = CompileAndCreateBot(bot.botCode, uid);
                int direction = botInterface.NextMove(bot.input);

                // 发送HTTP请求
                var response = await _httpClient.PostAsJsonAsync(
                    "http://localhost:5081/api/game/receive",
                    new
                    {
                        userId = bot.userId,
                        botId = bot.botId,
                        direction = direction,
                    }
                );
                Console.WriteLine(direction);
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Bot execution was interrupted due to timeout");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing bot code: {ex.Message}");
            }
        }

        private string AddUid(string code, string uid)
        {
            // 在类名后面加上uid
            string searchPattern = " : IBotInterface";
            int index = code.IndexOf(searchPattern);
            if (index != -1)
            {
                return code.Substring(0, index) + uid + code.Substring(index);
            }
            return code;
        }

        private IBotInterface CompileAndCreateBot(string botCode, string uid)
        {
            // 添加必要的using语句和修改类名
            string modifiedCode = $@"
using System;
using SnakeRunningSystem.Utils;

namespace SnakeRunningSystem.Utils
{{
    public class BotTest{uid} : IBotInterface
    {{
        public int NextMove(string input)
        {{
            // 用户代码逻辑在这里
            return 0; // 默认返回0，实际应该解析用户代码
        }}
    }}
}}";

            // 如果用户提供了自定义代码，尝试使用它
            if (!string.IsNullOrEmpty(botCode))
            {
                modifiedCode = AddUid(botCode, uid);
            }

            // 编译代码
            var syntaxTree = CSharpSyntaxTree.ParseText(modifiedCode);

            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IBotInterface).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location)
            };

            var compilation = CSharpCompilation.Create(
                assemblyName: $"BotAssembly{uid}",
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var errors = string.Join("\n", result.Diagnostics
                        .Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
                        .Select(d => d.ToString()));
                    throw new InvalidOperationException($"Compilation failed: {errors}");
                }

                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());
                var type = assembly.GetTypes().FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IBotInterface)));

                if (type == null)
                {
                    throw new InvalidOperationException("No class implementing IBotInterface found in compiled code");
                }

                return (IBotInterface)Activator.CreateInstance(type);
            }
        }
    }
}
