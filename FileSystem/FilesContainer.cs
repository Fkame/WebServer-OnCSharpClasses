using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace WebServer.FileSystem
{
    public class FilesContainer
    {
        private Dictionary<string, byte[]> container;

        public FilesContainer() 
        {
            this.container = new Dictionary<string, byte[]>();
        }

        public byte[] GetValueByKey(string key)
        {
            byte[] file = null;
            if (!container.TryGetValue(key, out file)) return null;
            return this.GetBytesCopy(file);
        }

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
        /// <returns></returns>
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

        private byte[] GetBytesCopy(byte[] copyFrom) 
        {
            byte[] copy = new byte[copyFrom.Length];
            Array.Copy(copyFrom, copy, copyFrom.Length);
            return copy;
        }

        public bool IsExists(string key)
        {
            byte[] file = null;
            return container.TryGetValue(key, out file);
        }

        public bool ReplaceValue(string key, byte[] newValue)
        {
            if (!container.Remove(key)) return false;
            if (!container.TryAdd(key, newValue)) return false;
            return true;
        }

        public bool ReplaceKey(string oldKey, string newKey)
        {
            byte[] value = null;
            if (!container.TryGetValue(oldKey,out value)) return false;
            if (!container.Remove(oldKey)) return false;
            if (!container.TryAdd(newKey, value)) return false;
            return true;
        }

        public void Clear()
        {
            container.Clear();
        }
    }
}