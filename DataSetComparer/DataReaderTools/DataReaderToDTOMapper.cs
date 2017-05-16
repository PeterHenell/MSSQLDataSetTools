using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataSetTools.DataReaderTools
{

    /// <summary>
    /// Maps values 1 to 1 from IDataReader to either struct with fields or class with properties.
    /// </summary>
    public class DataReaderToDTOMapper
    {
        /// <summary>
        /// Expects the IDataReader to have the exact same columns as T.
        /// Should not be used for massive amount of rows as reflection will cause performance issues.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// <remarks>Thank you! http://improve.dk/automatically-mapping-datatable-to-objects/</remarks>
        public static List<T> DataReaderToStruct<T>(IDataReader dr) where T : struct
        {
            var list = new List<T>();

            var fields = typeof(T).GetFields();
            if (fields.Length == 0)
            {
                throw new ArgumentException("T must have public fields, properties are not supported at this moment");
            }
            if (dr.FieldCount != fields.Length)
            {
                throw new ArgumentException("T and dr must have the same fields, 1to1");
            }

            var t = Activator.CreateInstance<T>();
            
            while (dr.Read())
            {
                foreach (FieldInfo fi in fields)
                    fi.SetValueDirect(__makeref(t), dr[fi.Name]);

                list.Add(t);
            }

            return list;
        }

        /// <summary>
        /// Expects the IDataReader to have the exact same columns as T.
        /// Should not be used for massive amount of rows as reflection will cause performance issues.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static List<T> DataReaderToObject<T>(IDataReader dr) where T : new()
        {
            var list = new List<T>();

            var properties = typeof(T).GetProperties();
            if (properties.Length == 0)
            {
                throw new ArgumentException("T must have public properties");
            }
            if (dr.FieldCount != properties.Length)
            {
                throw new ArgumentException("T and dr must have the same fields, 1to1");
            }

            while (dr.Read())
            {
                var t = Activator.CreateInstance<T>();
                foreach (PropertyInfo info in properties)
                {
                    info.SetValue(t, dr[info.Name]);
                }
                list.Add(t);
            }

            return list;
        }
    }
}
