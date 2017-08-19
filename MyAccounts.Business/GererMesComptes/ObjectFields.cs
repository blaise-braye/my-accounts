using System;
using System.Collections.Generic;

namespace MyAccounts.Business.GererMesComptes
{
    public class ObjectFields
    {
        private readonly Dictionary<string, string> _fields;

        public ObjectFields(Dictionary<string, string> fields)
        {
            _fields = fields;
        }

        public ICollection<string> GetKeys()
        {
            return _fields.Keys;
        }

        public string GetValue(string key, string defaultValue = null)
        {
            if (defaultValue == null)
            {
                EnsureFieldExists(key);
                return _fields[key];
            }

            return _fields.ContainsKey(key) ? _fields[key] : defaultValue;
        }

        public void SetValue(string key, string value)
        {
            EnsureFieldExists(key);
            _fields[key] = value;
        }

        public Dictionary<string, string> ToDictionnary()
        {
            return new Dictionary<string, string>(_fields);
        }

        private void EnsureFieldExists(string key)
        {
            if (!_fields.ContainsKey(key))
            {
                throw new InvalidOperationException("fields key does not exist : " + key);
            }
        }
    }
}