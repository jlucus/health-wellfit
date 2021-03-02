using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.ComponentModel;
using System.Reflection;

namespace WellFitPlus.Mobile
{
    public abstract class Settings
    {
        #region Properties
        protected static ISettings AppSettings {
            get {
                // NOTE:    we are using a 3rd party plugin for cross-platform settings
                return CrossSettings.Current;
            }
        }
        #endregion

        #region Methods
        public void Save() {
            var properties = this.GetType().GetProperties();

            foreach (var property in properties) {
                try
                {
                    switch (Type.GetTypeCode(property.GetType()))
                    {
                        case TypeCode.Int32:
                            AppSettings.AddOrUpdateValue(property.Name, Convert.ToInt32(property.GetValue(this)), null);
                            break;
                        case TypeCode.Int64:
                            AppSettings.AddOrUpdateValue(property.Name, Convert.ToInt64(property.GetValue(this)), null);
                            break;
                        case TypeCode.Single:
                            AppSettings.AddOrUpdateValue(property.Name, Convert.ToSingle(property.GetValue(this)), null);
                            break;
                        case TypeCode.DateTime:
                            AppSettings.AddOrUpdateValue(property.Name, Convert.ToDateTime(property.GetValue(this)), null);
                            break;
                        case TypeCode.Boolean:
                            AppSettings.AddOrUpdateValue(property.Name, Convert.ToBoolean(property.GetValue(this)), null);
                            break;
                        case TypeCode.String:
                            AppSettings.AddOrUpdateValue(property.Name, property.GetValue(this)?.ToString(), null);
                            break;
                        case TypeCode.Object:
                            if (property.GetType() == typeof(Guid) || property.GetType() == typeof(Guid?))
                            {
                                AppSettings.AddOrUpdateValue(property.Name, Guid.Parse(property.GetValue(this)?.ToString()), null);
                                return;
                            }
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void Clear() {
            var properties = this.GetType().GetProperties();

            foreach (var property in properties) {
                AppSettings.Remove(property.Name);
            }

            App.Log(string.Format("Cleared Settings: {0}", this));
        }

        protected T DefaultValue<T>(string name) {
            var property = this.GetType().GetProperty(name);
            var attribute = property.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute;

            if (attribute.Value == null) {
                return default(T);
            }

            return (T)attribute.Value;
        }
        #endregion
    }
}
