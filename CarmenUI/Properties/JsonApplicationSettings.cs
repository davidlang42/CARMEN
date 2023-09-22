using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarmenUI.Properties
{
    internal abstract class JsonApplicationSettings : ApplicationSettingsBase
    {
        private readonly Dictionary<string, (Func<object> getter, Action<object> setter)> jsonProperties = new();

        private SettingsProvider GetFirstProvider()
        {
            var e = Providers.GetEnumerator();
            if (!e.MoveNext())
                throw new ApplicationException("JsonApplicationSettings must have at least 1 generated setting, so that ApplicationSettingsBase constructs a provider");
            return (SettingsProvider)e.Current;
        }

        protected void RegisterJsonProperty<T>(string name, Func<T> getter, Action<T> setter) where T : class, new()
        {
            jsonProperties.Add(name, (() => getter(), o => setter((T)o)));
            Properties.Add(new SettingsProperty(name)
            {
                IsReadOnly = false,
                SerializeAs = SettingsSerializeAs.String,
                PropertyType = typeof(string),
                Provider = GetFirstProvider(),
                Attributes =
                {
                    { typeof(UserScopedSettingAttribute) , new UserScopedSettingAttribute() }
                }
            });
            LoadJson<T>(name);
        }

        private void LoadJson<T>(string name) where T : class, new()
        {
            if (!jsonProperties.TryGetValue(name, out var details))
                throw new ApplicationException($"Json property '{name}' not found.");
            var json = (string)this[name];
            T? obj = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(json))
                    obj = JsonSerializer.Deserialize<T>(json);
            }
            catch { }
            details.setter(obj ?? new());
        }

        private void StoreJson(string name)
        {
            if (!jsonProperties.TryGetValue(name, out var details))
                throw new ApplicationException($"Json property '{name}' not found.");
            var obj = details.getter();
            this[name] = JsonSerializer.Serialize(obj);
        }

        public override void Save()
        {
            foreach (var property in jsonProperties.Keys)
                StoreJson(property);
            base.Save();
        }
    }
}
