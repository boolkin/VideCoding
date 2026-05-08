import sqlite3
import json
import os
import logging

# Настройка логирования
logging.basicConfig(format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', level=logging.INFO)
logger = logging.getLogger(__name__)

class DatabaseManager:
    def __init__(self, config_path):
        with open(config_path, 'r', encoding='utf-8') as f:
            self.config = json.load(f)
        
        # Хранилище данных в памяти: { "db_key": [ {row_data}, ... ] }
        self.memory_data = {}
        
        # Инициализация БД пользователей
        self._init_user_db()

    def _init_user_db(self):
        """Создает таблицу для хранения настроек пользователей, если её нет."""
        db_file = self.config.get('user_db_file', 'users_settings.db')
        conn = sqlite3.connect(db_file)
        cursor = conn.cursor()
        cursor.execute('''
            CREATE TABLE IF NOT EXISTS user_settings (
                user_id INTEGER PRIMARY KEY,
                current_db_key TEXT
            )
        ''')
        conn.commit()
        conn.close()

    def load_all_databases(self):
        """Загружает все базы данных из конфига в оперативную память."""
        logger.info("Начинаю загрузку баз данных в память...")
        for db_cfg in self.config.get('databases', []):
            key = db_cfg['key']
            file_path = db_cfg['file_path']
            table_name = db_cfg['table_name']
            
            if not os.path.exists(file_path):
                logger.warning(f"Файл базы данных {file_path} не найден. Пропускаю.")
                continue

            try:
                conn = sqlite3.connect(file_path)
                conn.row_factory = sqlite3.Row # Доступ к колонкам по имени
                cursor = conn.cursor()
                
                # Выбираем все данные из таблицы
                cursor.execute(f"SELECT * FROM {table_name}")
                rows = cursor.fetchall()
                
                # Преобразуем в список словарей
                self.memory_data[key] = [dict(row) for row in rows]
                conn.close()
                logger.info(f"База '{db_cfg['display_name']}' загружена. Записей: {len(self.memory_data[key])}")
            except Exception as e:
                logger.error(f"Ошибка при загрузке {file_path}: {e}")

    def get_user_db_key(self, user_id):
        """Получает ключ текущей базы пользователя. По умолчанию первая из списка."""
        db_file = self.config.get('user_db_file', 'users_settings.db')
        default_key = self.config['databases'][0]['key'] if self.config['databases'] else None
        
        conn = sqlite3.connect(db_file)
        cursor = conn.cursor()
        cursor.execute("SELECT current_db_key FROM user_settings WHERE user_id = ?", (user_id,))
        result = cursor.fetchone()
        conn.close()
        
        if result:
            return result[0]
        return default_key

    def set_user_db_key(self, user_id, db_key):
        """Сохраняет выбор базы пользователя."""
        db_file = self.config.get('user_db_file', 'users_settings.db')
        conn = sqlite3.connect(db_file)
        cursor = conn.cursor()
        cursor.execute("INSERT OR REPLACE INTO user_settings (user_id, current_db_key) VALUES (?, ?)",
                       (user_id, db_key))
        conn.commit()
        conn.close()

    def get_db_config(self, db_key):
        """Возвращает конфиг конкретной базы."""
        for db in self.config['databases']:
            if db['key'] == db_key:
                return db
        return None