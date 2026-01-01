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
            Console.WriteLine("사용법:");
            Console.WriteLine("  NoljiMa \"메시지\"                    # 메시지 전송");
            Console.WriteLine("  NoljiMa --wait \"[패턴]\"             # 응답 대기 (기본 300초)");
            Console.WriteLine("  NoljiMa --wait \"[패턴]\" --timeout 600  # 응답 대기 (타임아웃 지정)");
            return;
        }

        // --wait 파라미터 확인 (응답 대기 모드)
        int waitIndex = Array.IndexOf(args, "--wait");
        if (waitIndex >= 0 && waitIndex + 1 < args.Length)
        {
            string pattern = args[waitIndex + 1];

            // --timeout 파라미터 확인 (기본값 300초)
            int timeout = 300;
            int timeoutIndex = Array.IndexOf(args, "--timeout");
            if (timeoutIndex >= 0 && timeoutIndex + 1 < args.Length)
            {
                if (int.TryParse(args[timeoutIndex + 1], out int parsedTimeout))
                {
                    timeout = parsedTimeout;
                }
            }

            // 대기 모드 실행
            int exitCode = WaitForMessage(botToken, pattern, timeout);
            Environment.Exit(exitCode);
        }

        // 메시지 전송 모드 (기존 로직)
        string message = args[0];
        bool success = SendTelegramMessage(botToken, chatId, message, out string error);

        if (success)
        {
            Console.WriteLine("전송 성공");
            Environment.Exit(0);
        }
        else
        {
            Console.WriteLine($"전송 실패: {error}");
            Environment.Exit(1);
        }
    }

    static string GetIniPath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string noljiMaFolder = Path.Combine(appDataPath, "NoljiMa");

        // 폴더가 없으면 생성
        if (!Directory.Exists(noljiMaFolder))
        {
            Directory.CreateDirectory(noljiMaFolder);
        }

        return Path.Combine(noljiMaFolder, "config.ini");
    }

    static string GetOffsetPath()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string noljiMaFolder = Path.Combine(appDataPath, "NoljiMa");

        // 폴더가 없으면 생성
        if (!Directory.Exists(noljiMaFolder))
        {
            Directory.CreateDirectory(noljiMaFolder);
        }

        return Path.Combine(noljiMaFolder, "offset.txt");
    }

    static long LoadOffset(string offsetPath)
    {
        try
        {
            if (!File.Exists(offsetPath))
                return 0;

            string content = File.ReadAllText(offsetPath).Trim();
            if (long.TryParse(content, out long offset))
                return offset;

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    static bool SaveOffset(string offsetPath, long offset)
    {
        try
        {
            File.WriteAllText(offsetPath, offset.ToString(), Encoding.UTF8);
            return true;
        }
        catch
        {
            return false;
        }
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

    static bool SaveConfig(string iniPath, string botToken, string chatId)
    {
        try
        {
            var content = $"[Telegram]\nBotToken={botToken}\nChatId={chatId}\n";
            File.WriteAllText(iniPath, content, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"설정 파일 저장 실패: {ex.Message}");
            return false;
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

    static List<(long UpdateId, string Text)> GetTelegramUpdates(string botToken, long offset, out string error)
    {
        error = "";
        var updates = new List<(long, string)>();

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(35); // Long Polling 30초 + 여유 5초

            var url = $"https://api.telegram.org/bot{botToken}/getUpdates?offset={offset}&timeout=30";

            var response = httpClient.GetAsync(url).Result;
            var responseBody = response.Content.ReadAsStringAsync().Result;

            if (!response.IsSuccessStatusCode)
            {
                if (responseBody.Contains("Unauthorized") || responseBody.Contains("bot token"))
                {
                    error = "토큰이 유효하지 않습니다";
                }
                else
                {
                    error = $"API 오류 (HTTP {response.StatusCode})";
                }
                return updates;
            }

            // JSON 파싱
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (!root.TryGetProperty("ok", out var ok) || !ok.GetBoolean())
            {
                error = "API 응답 오류";
                return updates;
            }

            if (!root.TryGetProperty("result", out var result))
            {
                return updates; // 빈 배열
            }

            foreach (var update in result.EnumerateArray())
            {
                if (!update.TryGetProperty("update_id", out var updateIdElement))
                    continue;

                long updateId = updateIdElement.GetInt64();

                // message.text 추출
                if (update.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("text", out var text))
                {
                    updates.Add((updateId, text.GetString() ?? ""));
                }
            }

            return updates;
        }
        catch (HttpRequestException)
        {
            error = "네트워크 오류";
            return updates;
        }
        catch (TaskCanceledException)
        {
            error = "네트워크 오류 (시간 초과)";
            return updates;
        }
        catch (Exception ex)
        {
            error = $"알 수 없는 오류: {ex.Message}";
            return updates;
        }
    }

    static int WaitForMessage(string botToken, string pattern, int timeoutSeconds)
    {
        var offsetPath = GetOffsetPath();
        long offset = LoadOffset(offsetPath);

        Console.WriteLine($"응답 대기 중... (패턴: \"{pattern}\", 타임아웃: {timeoutSeconds}초)");

        var startTime = DateTime.Now;
        int retryCount = 0;
        const int maxRetries = 3;

        while ((DateTime.Now - startTime).TotalSeconds < timeoutSeconds)
        {
            var updates = GetTelegramUpdates(botToken, offset, out string error);

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"폴링 실패: {error}");
                retryCount++;

                if (retryCount >= maxRetries)
                {
                    Console.WriteLine($"최대 재시도 횟수({maxRetries})를 초과했습니다.");
                    return 1;
                }

                // 재시도 전 대기 (5초)
                System.Threading.Thread.Sleep(5000);
                continue;
            }

            // 재시도 카운트 리셋
            retryCount = 0;

            // 메시지 확인
            foreach (var (updateId, text) in updates)
            {
                if (text.Contains(pattern))
                {
                    Console.WriteLine($"응답 메시지 수신: {text}");
                    SaveOffset(offsetPath, updateId + 1);
                    return 0;
                }

                // 다음 폴링을 위해 offset 업데이트
                offset = updateId + 1;
            }

            // offset 저장 (중복 메시지 방지)
            if (updates.Count > 0)
            {
                SaveOffset(offsetPath, offset);
            }
        }

        Console.WriteLine("응답 대기 시간 초과");
        return 1;
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

        bool success = SendTelegramMessage(botToken, chatId, "[테스트] NoljiMa 설정 완료!", out string error);

        if (!success)
        {
            Console.WriteLine($"설정 실패: {error}");
            Console.WriteLine("설정 파일이 저장되지 않았습니다.");
            return;
        }

        bool saved = SaveConfig(iniPath, botToken, chatId);
        if (!saved)
        {
            Console.WriteLine("설정 파일 저장에 실패했습니다. 프로그램을 종료합니다.");
            return;
        }

        Console.WriteLine("설정 완료! 설정 파일이 저장되었습니다.");
    }
}
