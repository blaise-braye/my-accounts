using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FastMember;

namespace Operations.Classification.WpfUi.Technical.ChangeTracking
{
    public class DataTracker
    {
        private readonly Dictionary<string, PropertyState> _entries = new Dictionary<string, PropertyState>();
        private INotifyPropertyChanged _data;
        private string[] _trackedProperties;
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
                .Where(p => p.CanRead)
                .Select(p => p.Name)
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

        public void ResetSnapshot()
        {
            _entries.Clear();

            foreach (var property in _trackedProperties)
            {
                var propertyState = new PropertyState(property, _propertyAccessor[property]);
                _entries[property] = propertyState;
            }
        }

        public void FillFromDirtyProperties(object targetData)
        {
            var targetProperties = targetData
                .GetType()
                .GetProperties()
                .Where(p => p.CanWrite)
                .Select(p => p.Name)
                .ToDictionary(p => p);
            var targetPropertyAccessor = ObjectAccessor.Create(targetData);
            foreach (var propertyState in _entries.Values.Where(e => e.IsDirty))
            {
                if (targetProperties.ContainsKey(propertyState.Property))
                {
                    targetPropertyAccessor[propertyState.Property] = propertyState.Value;
                }
            }
        }

        private void DataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var entry = _entries[e.PropertyName];
            entry.Value = _propertyAccessor[e.PropertyName];
        }
    }
}