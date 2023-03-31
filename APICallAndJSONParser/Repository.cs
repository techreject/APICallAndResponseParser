using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace APICallAndJSONParser
{
    public class Repository
    {
        private string _name;
        private string _description;
        // The Uri and int types have built-in functionality to convert to and from 
        // string representation.No extra code is needed to deserialize from JSON string
        // format to those target types. If the JSON packet contains data that doesn't
        // convert to a target type, the serialization action throws an exception.
        private Uri _gitHubHomeUrl;
        private Uri _homepage;
        private int _watchers;
        private DateTime _lastPushUtc;

        // Defines a class to represent the JSON object returned from the GitHub API.
        // You'll use this class to display a list of repository names.
        //
        // The JSON for a repository object contains dozens of properties, but only
        // the name property will be deserialized. The serializer automatically ignores
        // JSON properties for which there is no match in the target class. This feature
        // makes it easier to create types that work with only a subset of fields in a
        // large JSON packet.
        // The C# convention is to capitalize the first letter of property names, but
        // the name property here starts with a lowercase letter because that matches
        // exactly what's in the JSON. Later you'll see how to use C# property names
        // that don't match the JSON property names.
        public Repository() 
        {
            _name = string.Empty;
            _description = string.Empty;
            _watchers = 0;
        }   

    // Declare a Name property of type string:
    //
    // 03/30/23 -  [JsonPropertyName("name")]  - To set the name of individual properties,
    // use the [JsonPropertyName] attribute.The property name set by this attribute:
    //      Applies in both directions, for serialization and deserialization.
    //      Takes precedence over property naming policies.
    //      Doesn't affect parameter name matching for parameterized constructors.
    // 
    // In short, this allows us to name the property after the JSON property to reference it
    // in JSON by its correct name, but following C# naming conventions and reference the C#
    // named variable
    [JsonPropertyName("name")] public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        [property: JsonPropertyName("description")] public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }
        [property: JsonPropertyName("html_url")] public Uri GitHubHomeUrl
        {
            get
            {
                return _gitHubHomeUrl;
            }
            set
            {
                _gitHubHomeUrl = value;
            }
        }
        [property: JsonPropertyName("homepage")] public Uri Homepage
        {
            get
            {
                return _homepage;
            }
            set
            {
                _homepage = value;
            }
        }
        [property: JsonPropertyName("watchers")] public int Watchers
        {
            get
            {
                return _watchers;
            }
            set
            {
                _watchers = value;
            }
        }
        [property: JsonPropertyName("pushed_at")] public DateTime LastPushUtc
        {
            get
            {
                // 03/30/23 - send back UTC Value
                // This format is for Coordinated Universal Time (UTC),
                // so the result of deserialization is a DateTime value
                // whose Kind property is Utc.
                return _lastPushUtc;
            }
            set
            {
                // 03/30/23 - Stores the UTC value
                _lastPushUtc = value;
            }
        }
        // 03/30/23 - Created a function to convert the UTC times to EST
        // This accepts a date/time and returns it, converted, represented
        // in your time zone
        public DateTime ConvertToEST(DateTime time)
        {
            return time.ToLocalTime();
        }
    }
}
