using System;
using System.Collections.Generic;

namespace Shared
{
    [Serializable]
    public enum MessageType { None, Login, Disconect, PlayerInput, GameData }
    [Serializable]
    public class Message
    {
        public MessageType Type;
        public Dictionary<string, object> Properties;

        public Message() : this(MessageType.None, new Dictionary<string, object>()) { }
        public Message(MessageType type, Dictionary<string, object> properties)
        {
            Type = type;
            Properties = properties;
        }

        public T Get<T>(string property, T defaultValue)
        {
            if (Properties.ContainsKey(property) && Properties[property] is T propertyValue) return propertyValue;

            return defaultValue;
        }
        public void Set(string property, object value)
        {
            if (Properties.ContainsKey(property)) Properties[property] = value;
            else Properties.Add(property, value);
        }
    }
}