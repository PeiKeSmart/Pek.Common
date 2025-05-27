using System;

namespace Pek.Configuration.Configuration
{
    public class ConfigurationChangeToken : IChangeToken
    {
        private bool _hasChanged;
        private readonly object _changeTokenLock = new object();

        public bool HasChanged
        {
            get
            {
                lock (_changeTokenLock)
                    return _hasChanged;
            }
        }

        public void OnChange(Action changeCallback)
        {
            // Implementation for registering a callback when the configuration changes
            // This could involve adding the callback to a list and invoking it when HasChanged is set to true
        }

        public void SignalChange()
        {
            lock (_changeTokenLock)
                _hasChanged = true;
        }

        public void Reset()
        {
            lock (_changeTokenLock)
                _hasChanged = false;
        }
    }
}