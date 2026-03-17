namespace SQLiteSearchBot
{
    public class BotSettings
    {
        public const string SectionName = "BotSettings";
        public string Token { get; set; } = string.Empty;
    }

    public class AppSettings
    {
        public const string SectionName = "AppSettings";
        public int MaxResultsForList { get; set; } = 10;
        public int MessageDelayMs { get; set; } = 500;
        public string Password { get; set; } = "1234"; // Пароль по умолчанию
    }

    public class DbConfig
    {
        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public List<TableConfig> Tables { get; set; } = new();
    }

    public class TableConfig
    {
        public string TableName { get; set; } = string.Empty;
        public List<string> SearchColumns { get; set; } = new();
        public List<string> ListColumns { get; set; } = new(); 
        public List<string> DetailColumns { get; set; } = new(); 
    }
}