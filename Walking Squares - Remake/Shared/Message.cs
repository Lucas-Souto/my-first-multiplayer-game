using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace Shared
{
    [Serializable]
    public enum MessageType { None, Login, PlayerInput, GameState }
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
            if (Properties.ContainsKey(property))
            {
                if (Properties[property] is JObject jObj) return jObj.ToObject<T>();
                else if (Properties[property] is T propertieValue) return propertieValue;
            }

            return defaultValue;
        }
        public Vector2 GetVector(string property)
        {
            if (Properties.ContainsKey(property))
            {
                if (Properties[property] is string vector && vector.Contains(","))
                {
                    string[] split = vector.Split(',');

                    return new Vector2(Convert.ToInt32(split[0]), Convert.ToInt32(split[1]));
                }
            }

            return Vector2.Zero;
        }
        public void Set(string property, object value)
        {
            if (Properties.ContainsKey(property)) Properties[property] = value;
            else Properties.Add(property, value);
        }
    }
}