Программа написана ИИ для помощи в редактировании и создании файлов конфигурации и списка тегов для программы по сбору сигналов с OPC и выводу их в веб брайзер https://github.com/boolkin/opc2web-client


Поиск документов, аналог программы https://gitverse.ru/Boolkin/TextFinder, но с использование библиотек для работы с документами. Та версия работала с документами как zip архивами


Телеграм бот для боиска внутри БД и выдачи результата в чат пользователя SQLSearchBot. 

dotnet add package Telegram.Bot --version 22.9.0
dotnet add package Microsoft.Data.Sqlite --version 10.0.2
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Configuration.Binder
dotnet run


dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -o ./publish  


