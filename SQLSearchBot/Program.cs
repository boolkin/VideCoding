using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.Extensions.Configuration;

namespace SQLiteSearchBot
{
    class Program
    {
        static IConfiguration? _config;
        static List<DbConfig>? _databases;
        static AppSettings? _appSettings;
        static string? _botToken;

        static async Task Main(string[] args)
        {
            _config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var botSettings = _config.GetSection(BotSettings.SectionName).Get<BotSettings>();
            _botToken = botSettings?.Token;
            _appSettings = _config.GetSection(AppSettings.SectionName).Get<AppSettings>();
            _databases = _config.GetSection("Databases").Get<List<DbConfig>>();

            if (string.IsNullOrWhiteSpace(_botToken) || _databases == null || _databases.Count == 0)
            {
                Console.WriteLine("Ошибка в конфигурации.");
                return;
            }

            DataManager.LoadData(_databases);

            using var cts = new CancellationTokenSource();
            var bot = new TelegramBotClient(_botToken, cancellationToken: cts.Token);
            var me = await bot.GetMe();
            Console.WriteLine($"@{me.Username} is running...");

            var receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
            bot.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Press Enter to terminate");
            Console.ReadLine();
            cts.Cancel();
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message is { } message)
                {
                    await OnMessage(botClient, message);
                }
                else if (update.CallbackQuery is { } query)
                {
                    await OnCallbackQuery(botClient, query);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // Метод для создания клавиатуры с галочками
        private static InlineKeyboardMarkup GetDatabaseKeyboard(long userId)
        {
            var currentDbName = DataManager.GetUserActiveDb(userId);
            
            var buttons = _databases!.Select(db => 
            {
                var label = db.Name == currentDbName ? $"✅ {db.Name}" : db.Name;
                return new[] { InlineKeyboardButton.WithCallbackData(label, $"switch_{db.Name}") };
            }).ToArray();
            
            return new InlineKeyboardMarkup(buttons);
        }

        static (string Key, string Label) ParseColumn(string configValue)
        {
            if (configValue.Contains("|"))
            {
                var parts = configValue.Split('|');
                return (parts[0].Trim(), parts[1].Trim());
            }
            return (configValue, configValue);
        }

        private static async Task OnMessage(ITelegramBotClient botClient, Message message)
        {
            var chatId = message.Chat.Id;
            var userId = message.From?.Id ?? 0;
            var text = message.Text;

            // Проверяем статус доступа
            var accessStatus = DataManager.GetUserAccess(userId);

            // Если пользователь забанен
            if (accessStatus == "false")
            {
                await botClient.SendMessage(chatId, "🚫 <b>Доступ запрещен.</b>\nВы превысили лимит попыток ввода пароля.", parseMode: ParseMode.Html);
                return;
            }

            // Обработка /start
            if (text == "/start")
            {
                // 1. Сбрасываем старые клавиатуры (Reply Keyboard)
                await botClient.SendMessage(chatId, "Настройка клавиатуры...", replyMarkup: new ReplyKeyboardRemove());

                // 2. Логика входа или меню
                if (accessStatus == "true")
                {
                    await botClient.SendMessage(chatId, "👋 Вы уже авторизованы.\nВыберите базу данных (/database) или введите запрос.");
                }
                else
                {
                    await botClient.SendMessage(chatId, "🔒 <b>Доступ защищен паролем.</b>\nПожалуйста, введите пароль:", parseMode: ParseMode.Html);
                }
                return;
            }

            // Обработка /database
            if (text == "/database")
            {
                if (accessStatus == "true")
                {
                    var keyboard = GetDatabaseKeyboard(userId);
                    await botClient.SendMessage(chatId, "Выберите базу данных:", replyMarkup: keyboard);
                }
                else
                {
                    await botClient.SendMessage(chatId, "⛔ Сначала войдите в систему (/start).");
                }
                return;
            }

            // Если текст введен
            if (!string.IsNullOrWhiteSpace(text))
            {
                // Логика ввода пароля
                if (accessStatus == "null")
                {
                    var result = DataManager.CheckPassword(userId, text, _appSettings!.Password);

                    if (result == "success")
                    {
                        await botClient.SendMessage(chatId, "✅ Пароль принят! Добро пожаловать.\nВыберите базу данных командой /database", parseMode: ParseMode.Html);
                    }
                    else if (result == "fail")
                    {
                        var attemptsLeft = 3 - DataManager.GetAttempts(userId);
                        await botClient.SendMessage(chatId, $"❌ Неверный пароль.\nОсталось попыток: {attemptsLeft}");
                    }
                    else if (result == "banned")
                    {
                        await botClient.SendMessage(chatId, "🚫 <b>Доступ запрещен.</b>\nВы ввели неверный пароль 3 раза.", parseMode: ParseMode.Html);
                    }
                    return;
                }

                // Логика поиска (если access == "true")
                var currentDbName = DataManager.GetUserActiveDb(userId);
                if (string.IsNullOrEmpty(currentDbName) || !_databases!.Any(d => d.Name == currentDbName))
                {
                    await botClient.SendMessage(chatId, "⚠️ База данных не выбрана.\nИспользуйте команду /database для выбора.");
                    return;
                }

                var results = DataManager.Search(currentDbName, text);
                
                if (results.Count == 0)
                {
                    await botClient.SendMessage(chatId, "Ничего не найдено.");
                }
                else if (results.Count > (_appSettings?.MaxResultsForList ?? 10))
                {
                    await botClient.SendMessage(chatId, $"Найдено слишком много записей ({results.Count}). Уточните запрос.");
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"🔍 <b>База: {currentDbName}</b>");
                    sb.AppendLine($"🔍 <b>Найдено:</b> {results.Count}\n");
                    
                    var buttons = new List<InlineKeyboardButton>(); 
                    
                    for (int i = 0; i < results.Count; i++)
                    {
                        var rec = results[i];
                        var tableConfig = _databases!.First(d => d.Name == currentDbName)
                                                   .Tables.First(t => t.TableName == rec.TableName);
                        
                        var listParts = new List<string>();
                        foreach (var colCfg in tableConfig.ListColumns)
                        {
                            var (key, label) = ParseColumn(colCfg);
                            var val = rec.Data.FirstOrDefault(kvp => kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;
                            listParts.Add(!string.IsNullOrEmpty(val) ? val : "");
                        }

                        sb.AppendLine($"{i + 1}. {string.Join(" | ", listParts)}");
                        buttons.Add(InlineKeyboardButton.WithCallbackData($"#{i + 1}", $"detail_{currentDbName}_{rec.GlobalIndex}"));
                    }

                    var keyboardRows = new List<InlineKeyboardButton[]>();
                    for (int i = 0; i < buttons.Count; i += 5)
                    {
                        keyboardRows.Add(buttons.Skip(i).Take(5).ToArray());
                    }
                    var keyboard = new InlineKeyboardMarkup(keyboardRows);

                    await botClient.SendMessage(chatId, sb.ToString(), parseMode: ParseMode.Html, replyMarkup: keyboard);
                }
            }
        }

        private static async Task OnCallbackQuery(ITelegramBotClient botClient, CallbackQuery query)
        {
            try 
            {
                await botClient.AnswerCallbackQuery(query.Id);
            }
            catch (ApiRequestException ex) when (ex.Message.Contains("query is too old") || ex.Message.Contains("invalid"))
            {
                return; 
            }

            var chatId = query.Message!.Chat.Id;
            var userId = query.From.Id;
            var data = query.Data;

            // Проверка доступа для кнопок
            if (DataManager.GetUserAccess(userId) != "true")
            {
                await botClient.SendMessage(chatId, "⛔ Сначала авторизуйтесь (/start).");
                return;
            }

            if (data == null) return;

            if (data.StartsWith("switch_"))
            {
                var dbName = data.Substring(7);
                DataManager.SetUserActiveDb(userId, dbName);
                
                // Редактируем сообщение с кнопками, обновляя галочки
                var newKeyboard = GetDatabaseKeyboard(userId);
                try 
                {
                    await botClient.EditMessageReplyMarkup(chatId, query.Message.MessageId, newKeyboard);
                }
                catch { /* Игнорируем, если сообщение старое */ }

                await botClient.SendMessage(chatId, $"✅ База данных переключена на: <b>{dbName}</b>\nВведите текст для поиска.", parseMode: ParseMode.Html);
                return;
            }

            if (data.StartsWith("detail_"))
            {
                var parts = data.Split('_');
                if (parts.Length >= 3)
                {
                    var dbName = parts[1];
                    if (int.TryParse(parts[2], out int index))
                    {
                        var record = DataManager.GetRecordByIndex(dbName, index);

                        if (record != null)
                        {
                            var tableConfig = _databases!.First(d => d.Name == dbName)
                                                       .Tables.First(t => t.TableName == record.TableName);

                            var sb = new StringBuilder();
                            sb.AppendLine($"<b>Запись #{index + 1}</b>\n");

                            foreach (var colCfg in tableConfig.DetailColumns)
                            {
                                var (key, label) = ParseColumn(colCfg);
                                var value = record.Data.FirstOrDefault(kvp => kvp.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Value;
                                
                                if (!string.IsNullOrEmpty(value))
                                {
                                    sb.AppendLine($"<b>{label}:</b>\n{value}\n");
                                }
                            }

                            await SendLongMessage(botClient, chatId, sb.ToString());
                        }
                    }
                }
            }
        }

        private static async Task SendLongMessage(ITelegramBotClient botClient, long chatId, string text)
        {
            const int maxLength = 4000; 
            
            if (text.Length <= maxLength)
            {
                await botClient.SendMessage(chatId, text, parseMode: ParseMode.Html);
            }
            else
            {
                for (int i = 0; i < text.Length; i += maxLength)
                {
                    var chunk = text.Substring(i, Math.Min(maxLength, text.Length - i));
                    await botClient.SendMessage(chatId, chunk, parseMode: ParseMode.Html);
                    
                    if (i + maxLength < text.Length)
                    {
                        await Task.Delay(_appSettings!.MessageDelayMs);
                    }
                }
            }
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}