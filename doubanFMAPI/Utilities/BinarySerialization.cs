using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace doubanFMAPI.Utilities
{
    public class BinarySerialization<T>
    {
        internal static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"doubanFM\");
        private T obj;
        public BinarySerialization(T obj)
        {
            this.obj = obj;
        }

        /// <summary>
        /// 二进制序列化
        /// </summary>
        /// <param name="filename">将序列化文件保存位置(存在于DataFolder中,仅文件名)</param>
        /// <returns>序列化是否成功</returns>
        public bool Serialization(string filename)
        {
            try
            {
                if (!Directory.Exists(DataFolder))
                    Directory.CreateDirectory(DataFolder);
                using (FileStream stream = File.OpenWrite(Path.Combine(DataFolder, filename)))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, obj);
                }
            }

            catch
            {
                throw;
                //return false;
            }

            return true;
        }

        /// <summary>
        /// 二进制反序列化
        /// </summary>
        /// <param name="filename">序列化文件位置(存在于DataFolder中,仅文件名)</param>
        /// <returns>反序列化对象</returns>
        public T DeSerialization(string filename)
        {
            if (!File.Exists(Path.Combine(DataFolder, filename)))
                return default(T);
            try
            {
                using (FileStream stream = File.OpenRead(Path.Combine(DataFolder, filename)))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (T)formatter.Deserialize(stream);
                }
            }
            catch
            {
                return default(T);
            }
        }
    }
}
