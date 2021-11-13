using Amazon.DynamoDBv2.DataModel;
using System;

namespace RSARegister.Models
{   [DynamoDBTable("ClientModel")]
    public class ClientModel
    {
        [DynamoDBHashKey]
        public string ID { get; set; }
        public string IP { get; set; }
        public string Hour { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public ClientModel()
        {

        }
        public ClientModel(string iP, string time,string PublicKey,string PrivateKey)
        {
            ID  = Guid.NewGuid().ToString();
            IP = iP;
            Hour = time;
            this.PublicKey = PublicKey;
            this.PrivateKey = PrivateKey;
        }
    }
}
