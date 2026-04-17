@echo off
:: Устанавливаем кодировку UTF-8, чтобы кириллица "Все" распозналась корректно
chcp 65001 >nul

:: Проверка прав администратора
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo Ошибка: Запустите от имени администратора!
    pause
    exit /b
)


netsh http add urlacl url=http://*:8085/ user=Все

echo Готово! Порт открыт и права доступа настроены.
pause