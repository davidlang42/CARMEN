using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarmenUI.Properties
{
    internal abstract class JsonApplicationSettings : ApplicationSettingsBase
    {
        /// <summary>Call StoreJson() for each object which is backed by a Json string.
        /// NOTE: Constructor must call LoadJson() for each object which is backed by a Json string.</summary>
        protected abstract void UpdateJsonStrings();

        protected T LoadJson<T>(string? json) where T : class, new()
        {
            T? obj = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(json))
                    obj = JsonSerializer.Deserialize<T>(json);
            }
            catch { }
            return obj ?? new();
        }

        protected string StoreJson<T>(T obj) => JsonSerializer.Serialize(obj);

        public override void Save()
        {
            UpdateJsonStrings();
            base.Save();
        }
    }
}
