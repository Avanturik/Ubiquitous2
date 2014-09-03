using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UB.Model;

namespace UB.Utils
{
    public static class Config
    {
        public static object FieldValue(this List<ConfigField> fields, String fieldName)
        {
            return fields.Where(field => field.Name == fieldName).Select(field => field.Value).FirstOrDefault();
        }

        public static String StringValue(this List<ConfigField> fields, String fieldName)
        {
            return (String)fields.FieldValue(fieldName);
        }

        public static Boolean BooleanValue(this List<ConfigField> fields, String fieldName)
        {
            return (Boolean)fields.FieldValue(fieldName);
        }

        public static String[] StringArrayValue(this List<ConfigField> fields, String fieldName)
        {
            var text = fields.StringValue(fieldName);
            if( String.IsNullOrEmpty( text )) 
                return new String[]{};

            var list = ((String)text).Split(',');
            if (list.Length > 0)
                return list.Select(str => str.Replace(" ", "")).ToArray();
            else
                return list;

        }

    }
}
