using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using FastMember;

namespace Operations.Classification.WpfUi.Technical.ChangeTracking
{
    public class DataTracker
    {
        private readonly Dictionary<string, PropertyState> _entries = new Dictionary<string, PropertyState>();
        private INotifyPropertyChanged _data;
        private PropertyInfo[] _trackedProperties;
        private ObjectAccessor _propertyAccessor;

        public bool IsTracking => _data != null;

        public bool IsDirty => _entries.Values.Any(e => e.IsDirty);

        public void StartTracking<TData>(TData data)
            where TData : INotifyPropertyChanged
        {
            StopTracking();
            _data = data;
            _trackedProperties = _data
                .GetType()
                .GetProperties()
                .Where(p => p.CanRead && p.CanWrite)
                .ToArray();
            _propertyAccessor = ObjectAccessor.Create(_data);

            ResetSnapshot();
            _data.PropertyChanged += DataPropertyChanged;
        }

        public void StopTracking()
        {
            if (_data != null)
            {
                _data.PropertyChanged -= DataPropertyChanged;
                _propertyAccessor = null;
                _trackedProperties = null;
                _data = null;
            }
        }

        public void ResetOriginalValues()
        {
            foreach (var propertyState in _entries.Values)
            {
                if (propertyState.IsDirty && !propertyState.IsMixed)
                {
                    _propertyAccessor[propertyState.Name] = propertyState.OriginalValue;
                    propertyState.Value = propertyState.OriginalValue;
                }
            }
        }

        public void ResetSnapshot()
        {
            _entries.Clear();

            foreach (var property in _trackedProperties)
            {
                var propertyState = new PropertyState(property.Name, _propertyAccessor[property.Name], property.PropertyType);
                _entries[property.Name] = propertyState;
            }
        }

        public void FillFromDirtyProperties(object targetData, params string[] toSkip)
        {
            var targetProperties = targetData
                .GetType()
                .GetProperties()
                .Where(p => p.CanWrite)
                .Select(p => p.Name)
                .Where(p => !toSkip.Contains(p))
                .ToDictionary(p => p);
            var targetPropertyAccessor = ObjectAccessor.Create(targetData);
            foreach (var propertyState in _entries.Values.Where(e => !e.IsMixed && e.IsDirty))
            {
                if (targetProperties.ContainsKey(propertyState.Name))
                {
                    targetPropertyAccessor[propertyState.Name] = propertyState.Value;
                }
            }
        }

        public void UpdateMixedState(object sourceData, string[] toSkip)
        {
            var sourceProperties = sourceData
                .GetType()
                .GetProperties()
                .Where(p => p.CanRead)
                .Select(p => p.Name)
                .Where(p => !toSkip.Contains(p))
                .ToDictionary(p => p);
            var sourcePropertyAccessor = ObjectAccessor.Create(sourceData);
            foreach (var propertyState in _entries.Values)
            {
                if (sourceProperties.ContainsKey(propertyState.Name))
                {
                    var sourceValue = sourcePropertyAccessor[propertyState.Name];
                    if (!Equals(propertyState.Value, sourceValue))
                    {
                        propertyState.IsMixed = true;
                        propertyState.Value = null;
                        PauseTracking();

                        object defaultValue = null;

                        if (propertyState.Type.IsValueType)
                        {
                            defaultValue = Activator.CreateInstance(propertyState.Type);
                        }

                        _propertyAccessor[propertyState.Name] = defaultValue;
                        ResumeTracking();
                    }
                }
            }
        }

        private void PauseTracking()
        {
            _data.PropertyChanged -= DataPropertyChanged;
        }

        private void ResumeTracking()
        {
            _data.PropertyChanged += DataPropertyChanged;
        }

        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var entry = _entries[e.PropertyName];
            entry.Value = _propertyAccessor[e.PropertyName];
            entry.IsMixed = false;
        }
    }
}