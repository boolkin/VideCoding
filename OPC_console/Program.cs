using System;
using System.Collections.Generic;
using Opc;
using Opc.Da;

class Program
{
    static int totalTags = 0;

    static void Main()
    {
        try
        {
            // 1. Поиск серверов
            OpcCom.ServerEnumerator discovery = new OpcCom.ServerEnumerator();
            Opc.Server[] servers = discovery.GetAvailableServers(Specification.COM_DA_20, "localhost", null);

            Console.WriteLine("Доступные OPC DA серверы:");
            for (int i = 0; i < servers.Length; i++)
            {
                Console.WriteLine(string.Format("{0}: {1}", i, servers[i].Name));
            }

            Console.Write("\nВведите номер сервера: ");
            int index = int.Parse(Console.ReadLine());
            
            Opc.Da.Server selectedServer = (Opc.Da.Server)servers[index];

            // 2. Подключение
            selectedServer.Connect();
            Console.WriteLine(string.Format("Подключено к {0}. Поиск тегов...\n", selectedServer.Name));

            // 3. Рекурсивный поиск всех тегов
            totalTags = 0;
            BrowseAllTags(selectedServer, null);

            Console.WriteLine(string.Format("\nВсего найдено тегов: {0}", totalTags));

            selectedServer.Disconnect();
        }
        catch (Exception ex)
        {
            Console.WriteLine(string.Format("Ошибка: {0}", ex.Message));
        }

        Console.WriteLine("\nНажмите любую клавишу...");
        Console.ReadKey();
    }

    // Рекурсивная функция для обхода дерева тегов
    static void BrowseAllTags(Opc.Da.Server server, ItemIdentifier parentId)
    {
        BrowseFilters filters = new BrowseFilters();
        filters.BrowseFilter = browseFilter.all; // Ищем и папки, и теги
        
        BrowsePosition position;
        BrowseElement[] elements = server.Browse(parentId, filters, out position);

        if (elements != null)
        {
            foreach (BrowseElement el in elements)
            {
                if (el.IsItem) // Если это конечный тег
                {
                    Console.WriteLine(string.Format("Tag: {0}", el.ItemName));
                    totalTags++;
                }
                
                if (el.HasChildren) // Если это папка (ветка) - идем внутрь
                {
                    // Создаем идентификатор для входа в папку
                    ItemIdentifier currentFolder = new ItemIdentifier(el.ItemPath, el.ItemName);
                    BrowseAllTags(server, currentFolder);
                }
            }
        }
    }
}
