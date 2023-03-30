using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace APICallAndJSONParser
{
    internal class Repository
    {
        private string _name = string.Empty;

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

        }

        // Declare a Name property of type string:
        public string name
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
    }
}
