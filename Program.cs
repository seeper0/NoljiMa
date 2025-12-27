using System.Text;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        var iniPath = GetIniPath();
        var (botToken, chatId) = LoadConfig(iniPath);

        // INI 파일 없음 → 초기 설정 모드
        if (botToken == null || chatId == null)
        {
            Console.WriteLine("설정 파일이 없습니다. 설정을 시작합니다.");
            InitialSetup(iniPath);
            return;
        }

        // 인자 없음 → 사용법 출력
        if (args.Length == 0)
        {
            Console.WriteLine("사용법: NoljiMa \"메시지\"");
            return;
        }

        // 메시지 전송
        string message = args[0];
        bool success = SendTelegramMessage(botToken, chatId, message, out string error);

        if (success)
        {
            Console.WriteLine("전송 성공");
        }
        else
        {
            Console.WriteLine($"전송 실패: {error}");
        }
    }

    static string GetIniPath()
    {
        string exePath = AppContext.BaseDirectory;
        return Path.Combine(exePath, "NoljiMa.ini");
    }

    static (string? BotToken, string? ChatId) LoadConfig(string iniPath)
    {
        try
        {
            if (!File.Exists(iniPath))
                return (null, null);

            string? botToken = null;
            string? chatId = null;
            bool inTelegramSection = false;

            foreach (var line in File.ReadAllLines(iniPath))
            {
                var trimmed = line.Trim();

                // 섹션 확인
                if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
                {
                    inTelegramSection = trimmed.Equals("[Telegram]", StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                // [Telegram] 섹션 내부에서만 파싱
                if (!inTelegramSection)
                    continue;

                // Key=Value 파싱
                var parts = trimmed.Split('=', 2);
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                if (key.Equals("BotToken", StringComparison.OrdinalIgnoreCase))
                    botToken = value;
                else if (key.Equals("ChatId", StringComparison.OrdinalIgnoreCase))
                    chatId = value;
            }

            // 둘 다 있어야 유효
            if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(chatId))
                return (null, null);

            return (botToken, chatId);
        }
        catch
        {
            Console.WriteLine("설정 파일 오류: 다시 설정해주세요");
            return (null, null);
        }
    }

    static void SaveConfig(string iniPath, string botToken, string chatId)
    {
        try
        {
            var content = $"[Telegram]\nBotToken={botToken}\nChatId={chatId}\n";
            File.WriteAllText(iniPath, content, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"설정 파일 저장 실패: {ex.Message}");
        }
    }

    static bool SendTelegramMessage(string botToken, string chatId, string message, out string error)
    {
        error = "";

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
            var payload = new
            {
                chat_id = chatId,
                text = message
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync(url, content).Result;
            var responseBody = response.Content.ReadAsStringAsync().Result;

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // 에러 응답 파싱
                if (responseBody.Contains("Unauthorized") || responseBody.Contains("bot token"))
                {
                    error = "토큰이 유효하지 않습니다";
                }
                else if (responseBody.Contains("chat not found") || responseBody.Contains("Bad Request"))
                {
                    error = "ChatId가 유효하지 않습니다";
                }
                else
                {
                    error = $"API 오류 (HTTP {response.StatusCode})";
                }
                return false;
            }
        }
        catch (HttpRequestException)
        {
            error = "네트워크 오류";
            return false;
        }
        catch (TaskCanceledException)
        {
            error = "네트워크 오류 (시간 초과)";
            return false;
        }
        catch (Exception ex)
        {
            error = $"알 수 없는 오류: {ex.Message}";
            return false;
        }
    }

    static void InitialSetup(string iniPath)
    {
        Console.WriteLine();
        Console.Write("Telegram Bot Token을 입력하세요: ");
        string? botToken = Console.ReadLine()?.Trim();

        Console.Write("Chat ID를 입력하세요: ");
        string? chatId = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(botToken) || string.IsNullOrWhiteSpace(chatId))
        {
            Console.WriteLine("입력이 올바르지 않습니다.");
            return;
        }

        Console.WriteLine("\n테스트 메시지를 전송합니다...");

        bool success = SendTelegramMessage(botToken, chatId, "NoljiMa 설정 완료!", out string error);

        if (success)
        {
            SaveConfig(iniPath, botToken, chatId);
            Console.WriteLine("설정 완료! 설정 파일이 저장되었습니다.");
        }
        else
        {
            Console.WriteLine($"설정 실패: {error}");
            Console.WriteLine("설정 파일이 저장되지 않았습니다.");
        }
    }
}
