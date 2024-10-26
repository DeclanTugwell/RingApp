namespace RingClient
{
    public class AccessToken
    {
        private string _value = "";
        public string Value { 
            get
            {
                var tempValue = _value;
                _value = "";
                return tempValue;
            }
            set
            {
                if (value != _value)
                {
                    _value = value;
                }
            }
        }

        public bool IsTokenAvailable()
        {
            return !string.IsNullOrEmpty(_value);
        }
    }
}
