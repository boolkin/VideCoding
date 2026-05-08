@echo off
:: Устанавливаем кодировку UTF-8 для корректной работы с кириллицей
chcp 65001 >nul

:: Получаем дату через wmic (универсальный способ)
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /value') do set datetime=%%I
set YEAR=%datetime:~0,4%
set MONTH=%datetime:~4,2%

:: Ваш путь с русскими буквами (пример: Отчеты)
set TARGET_PATH="\\127.0.0.1\Daily reports\%YEAR%\_%MONTH%.%YEAR%\"

:: Переходим в папку со скриптом
cd /d "%~dp0"

:: Запуск программы
DocxToSqlite.exe --quiet --db reports.db --table DailyReports --path %TARGET_PATH%

echo Обработка пути %TARGET_PATH% завершена.
