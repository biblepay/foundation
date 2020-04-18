using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace Saved.Code
{
    public class SavedObject
    {
        private dynamic _props;
        public dynamic Props { get { return this._props; } set { this._props = value; } }
        public void AddProperty(string propertyName, object propertyValue)
        {
            EntityHelper.AddProperty(this._props, propertyName, propertyValue);
        }

        public SavedObject()
        {
            this.Props = new ExpandoObject();

        }

    }


    public static class EntityHelper
    {
        public static ExpandoObject ConvertToSavedExapandableObject(object obj)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
            PropertyInfo[] properties = obj.GetType().GetProperties(flags);
            //Add Them to a new Dynamic expanding Christian Object
            ExpandoObject ECO = new ExpandoObject();
            foreach (PropertyInfo property in properties)
            {
                AddProperty(ECO, property.Name, property.GetValue(obj));
            }
            return ECO;
        }

        public static void AddProperty(ExpandoObject SavedExpandableObject, string propertyName, object propertyValue)
        {
            var eDict = SavedExpandableObject as IDictionary<string, object>;
            if (eDict.ContainsKey(propertyName))
                eDict[propertyName] = propertyValue;
            else
                eDict.Add(propertyName, propertyValue);
        }
    }


    public class Entity
    {
       
    }
}