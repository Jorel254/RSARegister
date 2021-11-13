using System;

namespace RSARegister.Models
{
    public class ResponseClientKey
    {

        public string Key { get; set; }
        public string Id { get; set; }
        public ResponseClientKey()
        {

        }
        public ResponseClientKey( Guid id, string key)
        {
            this.Id = id.ToString();
            this.Key = key; 
        }
    }
}
