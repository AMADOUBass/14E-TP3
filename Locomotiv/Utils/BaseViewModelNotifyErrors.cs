using System.Collections;
using System.ComponentModel;

namespace Locomotiv.Utils
{
    public abstract partial class BaseViewModel : INotifyDataErrorInfo
    {
        private Dictionary<string, List<string>> _errorsByPropertyName = new();
        public bool HasErrors => _errorsByPropertyName.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public string ErrorMessages
        {
            get
            {
                var allErrors = new List<string>();
                foreach (var errorList in _errorsByPropertyName.Values)
                {
                    allErrors.AddRange(errorList);
                }

                allErrors.RemoveAll(error => string.IsNullOrWhiteSpace(error));

                return string.Join("\n", allErrors);
            }
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            if (
                string.IsNullOrEmpty(propertyName)
                || !_errorsByPropertyName.ContainsKey(propertyName)
            )
            {
                return Enumerable.Empty<string>();
            }

            return _errorsByPropertyName[propertyName];
        }

        protected void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void AddError(string propertyName, string error)
        {
            if (!_errorsByPropertyName.ContainsKey(propertyName))
                _errorsByPropertyName[propertyName] = new List<string>();

            if (!_errorsByPropertyName[propertyName].Contains(error))
            {
                _errorsByPropertyName[propertyName].Add(error);
                OnErrorsChanged(propertyName);
            }
        }

        protected void ClearErrors(string propertyName)
        {
            if (_errorsByPropertyName.ContainsKey(propertyName))
            {
                _errorsByPropertyName.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }
    }
}
