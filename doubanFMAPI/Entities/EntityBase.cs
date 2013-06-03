using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;

namespace doubanFMAPI.Entities
{
    [DataContract]
    public abstract class EntityBase<T>
    {
        /// <summary>
        /// 将获取的json字符串转换为相应的对象
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <returns>对象(泛型)</returns>
        public static T Json2Object(string json)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    return (T)ser.ReadObject(stream);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return default(T);
            }
        }

        public static string Object2Json(T obj)
        {
            try
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream stream = new MemoryStream())
                {
                    ser.WriteObject(stream, obj);
                    byte[] dataBytes = new byte[stream.Length];
                    stream.Position = 0;
                    stream.Read(dataBytes, 0, (int)stream.Length);
                    return Encoding.UTF8.GetString(dataBytes);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return null;
            }
        }
    }
}
