using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace WebServer.FileSystem
{
    /// <summary>
    /// Буфер для хранения файлов. Хранит в виде словаря пары короткий_путь - содержимое_файла_в_байтах.
    /// </summary>
    public class FilesContainer
    {
        /// <summary>
        /// Словарь, хранящий пары короткий_путь - содержимое_файла_в_байтах.
        /// </summary>
        private Dictionary<string, byte[]> container;

        public FilesContainer() 
        {
            this.container = new Dictionary<string, byte[]>();
        }

        /// <summary>
        /// По ключу возвращает значение. Если значение не найдено - вернёт null.
        /// </summary>
        /// <param name="key">Ключ для поиска значения в словаре</param>
        /// <returns>Значение, хранимое в словаре под указанным ключем - бассив байтов - содежримое файла в байтах</returns>
        public byte[] GetValueByKey(string key)
        {
            byte[] file = null;
            if (!container.TryGetValue(key, out file)) return null;
            return this.GetBytesCopy(file);
        }

        /// <summary>
        /// По ключу удаляет хранимое значение.
        /// </summary>
        /// <param name="key">Ключ для поиска значения в словаре</param>
        /// <returns>True - если удалось удалить файл, false - если указанный ключ не хранится.</returns>
        public bool Remove(string key)
        {
            if (container.Remove(key)) return true;
            return false;
        }

        public bool Add(string key, byte[] value) 
        {
            if (container.TryAdd(key, this.GetBytesCopy(value))) return true;
            return false;
        }

        /// <summary>
        /// Возвращает массив ключей в виде массива строк (потому что ключи в виде строк и хранятся).
        /// </summary>
        /// <returns>Моссив строк - массив ключей.</returns>
        public string[] GetKeys()
        {
            string[] array = new string[container.Keys.Count];
            int count = 0;
            foreach (string key in container.Keys)
            {
                array[count] = key;
                count++;
            }
            return array;
        }

        /// <summary>
        /// Служебный метод для копирования содержимого значения. Используется, чтобы из-за ссылочной механики нельзя было изменить
        /// хранимые значения извне, например, при запросе значения из словаря и последующей модификации полученного массива.
        /// </summary>
        /// <param name="copyFrom"></param>
        /// <returns></returns>
        private byte[] GetBytesCopy(byte[] copyFrom) 
        {
            byte[] copy = new byte[copyFrom.Length];
            Array.Copy(copyFrom, copy, copyFrom.Length);
            return copy;
        }

        /// <summary>
        /// Определяет, содержится ли какое-либо значение под конкрентым ключом.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>True - если под указанным ключем хранится значение, false - в противном случае.</returns>
        public bool IsExists(string key)
        {
            byte[] file = null;
            return container.TryGetValue(key, out file);
        }

        /// <summary>
        /// Получая на вход ключ и значение, метод заменяет старое значение, хранимое под указанным ключом, на переданное в метод.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public bool ReplaceValue(string key, byte[] newValue)
        {
            if (!container.Remove(key)) return false;
            if (!container.TryAdd(key, newValue)) return false;
            return true;
        }

        /// <summary>
        /// Метод меняет старый ключ на новый, не изменяя хранимого под ключем значения.
        /// </summary>
        /// <param name="oldKey"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public bool ReplaceKey(string oldKey, string newKey)
        {
            byte[] value = null;
            if (!container.TryGetValue(oldKey,out value)) return false;
            if (!container.Remove(oldKey)) return false;
            if (!container.TryAdd(newKey, value)) return false;
            return true;
        }

        /// <summary>
        /// Очищает хранилище.
        /// </summary>
        public void Clear()
        {
            container.Clear();
        }
    }
}