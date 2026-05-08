import asyncio
import logging
from telegram import Update, InlineKeyboardButton, InlineKeyboardMarkup
from telegram.constants import ParseMode
from telegram.ext import Application, CommandHandler, MessageHandler, CallbackQueryHandler, filters

# Импорт нашего модуля
from database import DatabaseManager

# Настройка логирования
logging.basicConfig(format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', level=logging.INFO)
logger = logging.getLogger(__name__)

# Глобальный экземпляр менеджера данных
db_manager = None

async def start(update: Update, context):
    user_id = update.effective_user.id
    db_key = db_manager.get_user_db_key(user_id)
    db_cfg = db_manager.get_db_config(db_key)
    
    db_name = db_cfg['display_name'] if db_cfg else "Не выбрана"
    
    msg = (
        f"👋 Привет! Я бот поиска по базе данных.\n"
        f"🔍 Текущая база: *{db_name}*\n\n"
        f"Просто отправьте мне текст, и я найду совпадения.\n"
        f"Для переключения базы используйте /database"
    )
    await update.message.reply_text(msg, parse_mode=ParseMode.MARKDOWN)

async def switch_db(update: Update, context):
    """Показывает меню выбора базы данных."""
    keyboard = []
    for db in db_manager.config['databases']:
        # Кнопка с названием базы, callback_data содержит ключ базы
        keyboard.append([InlineKeyboardButton(db['display_name'], callback_data=f"setdb_{db['key']}")])
    
    reply_markup = InlineKeyboardMarkup(keyboard)
    await update.message.reply_text("Выберите базу данных для поиска:", reply_markup=reply_markup)

async def button_callback(update: Update, context):
    """Обработка нажатий на кнопки."""
    query = update.callback_query
    await query.answer()
    
    data = query.data
    
    # Если это переключение базы
    if data.startswith("setdb_"):
        db_key = data.split("_")[1]
        user_id = update.effective_user.id
        db_manager.set_user_db_key(user_id, db_key)
        db_cfg = db_manager.get_db_config(db_key)
        await query.edit_message_text(f"✅ База данных изменена на: *{db_cfg['display_name']}*", parse_mode=ParseMode.MARKDOWN)
        return

    # Если это запрос полной информации (формат: get_dbkey_index)
    if data.startswith("get_"):
        parts = data.split("_")
        db_key = parts[1]
        try:
            row_index = int(parts[2])
        except (IndexError, ValueError):
            return

        # Получаем данные из памяти
        rows = db_manager.memory_data.get(db_key, [])
        if row_index >= len(rows):
            await query.edit_message_text("Ошибка: запись не найдена.")
            return

        row = rows[row_index]
        db_cfg = db_manager.get_db_config(db_key)
        
        # Формируем полный текст
        text_parts = []
        titles = db_cfg.get('display_titles', {})
        
        for col in db_cfg['columns']:
            title = titles.get(col, col)
            value = row.get(col, "")
            text_parts.append(f"🔹 *{title}:*\n{value}")
            
        full_text = "\n\n".join(text_parts)
        
        # Отправляем с задержкой и разбивкой, если нужно
        await send_long_message(query.message.chat_id, full_text, context.bot)

async def search(update: Update, context):
    """Обработка поискового запроса."""
    user_id = update.effective_user.id
    text = update.message.text.strip().lower()
    
    if not text:
        return

    db_key = db_manager.get_user_db_key(user_id)
    if not db_key or db_key not in db_manager.memory_data:
        await update.message.reply_text("База данных не настроена или пуста.")
        return

    db_cfg = db_manager.get_db_config(db_key)
    search_cols = db_cfg['search_columns']
    preview_cols = db_cfg['preview_columns']
    titles = db_cfg.get('display_titles', {})
    
    # Данные из памяти
    all_rows = db_manager.memory_data[db_key]
    
    # Разбиваем запрос пользователя на слова
    keywords = text.split()
    
    # Логика последовательного сужающего поиска
    # Сначала ищем все строки, где есть первое слово
    # Потом среди найденных ищем второе слово и т.д.
    
    candidates = list(enumerate(all_rows)) # Сохраняем оригинальные индексы (id)
    
    for kw in keywords:
        if not candidates:
            break
            
        next_candidates = []
        for idx, row in candidates:
            # Ищем слово в указанных столбцах
            for col in search_cols:
                col_val = str(row.get(col, "")).lower()
                if kw in col_val:
                    next_candidates.append((idx, row))
                    break # Если нашли в одном столбце, нет смысла искать в других для этой строки
        candidates = next_candidates

    # Проверка лимита вывода
    limit = db_manager.config.get('max_results_to_show', 10)
    
    if len(candidates) == 0:
        await update.message.reply_text("Ничего не найдено.")
        return
        
    if len(candidates) > limit:
        await update.message.reply_text(f"Найдено слишком много результатов ({len(candidates)}). Уточните запрос.")
        return

    # Формируем сообщение со списком
    msg_lines = []
    keyboard_rows = [] # Список списков кнопок (строки кнопок)
    current_btn_row = []
    buttons_per_row = 5

    # Создаем список с 1-based нумерацией для пользователя
    for i, (idx, row) in enumerate(candidates):
        num = i + 1
        
        # Формируем превью строку (например, "Code - Name")
        preview_parts = []
        for col in preview_cols:
            val = str(row.get(col, "")).strip()
            preview_parts.append(val)
        preview_text = " - ".join(preview_parts)
        
        msg_lines.append(f"{num}. {preview_text}")
        
        # Кнопка. callback_data = get_dbkey_rowindex
        btn = InlineKeyboardButton(f"#{num}", callback_data=f"get_{db_key}_{idx}")
        current_btn_row.append(btn)
        
        if len(current_btn_row) >= buttons_per_row:
            keyboard_rows.append(current_btn_row)
            current_btn_row = []
            
    if current_btn_row:
        keyboard_rows.append(current_btn_row)

    reply_markup = InlineKeyboardMarkup(keyboard_rows)
    
    result_text = "🔍 *Результаты поиска:*\n\n" + "\n".join(msg_lines)
    await update.message.reply_text(result_text, reply_markup=reply_markup, parse_mode=ParseMode.MARKDOWN)

async def send_long_message(chat_id, text, bot):
    """Разбивает длинное сообщение и отправляет частями с задержкой."""
    max_len = 4096
    parts = []
    
    # Простая разбивка по символам, можно улучшить разбивкой по абзацам
    while len(text) > max_len:
        part = text[:max_len]
        text = text[max_len:]
        parts.append(part)
    parts.append(text)
    
    for part in parts:
        try:
            await bot.send_message(chat_id=chat_id, text=part, parse_mode=ParseMode.MARKDOWN)
            await asyncio.sleep(db_manager.config.get('message_delay', 0.1))
        except Exception as e:
            logger.error(f"Ошибка отправки части сообщения: {e}")

def main():
    global db_manager
    
    # Инициализация менеджера БД
    db_manager = DatabaseManager('config.json')
    
    # Загрузка данных в память
    db_manager.load_all_databases()
    
    # Создание приложения
    token = db_manager.config['bot_token']
    application = Application.builder().token(token).build()

    # Обработчики
    application.add_handler(CommandHandler("start", start))
    application.add_handler(CommandHandler("database", switch_db))
    application.add_handler(MessageHandler(filters.TEXT & ~filters.COMMAND, search))
    application.add_handler(CallbackQueryHandler(button_callback))

    # Запуск
    logger.info("Бот запущен...")
    application.run_polling()

if __name__ == "__main__":
    main()