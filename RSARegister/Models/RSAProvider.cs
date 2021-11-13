using System.Security.Cryptography;
using System.Text;

namespace RSARegister.Models
{
    public class RSAProvider
    {
        public RSACryptoServiceProvider RSAService { get; set; }
        public RSAProvider()
        {
            this.RSAService = new RSACryptoServiceProvider();
        }
        public string CreatePublicKey()
        {
            string xmlPublicKey = this.RSAService.ToXmlString(false);
            return xmlPublicKey;
        }
        public string CreatePrivateKey()
        {
            string xmlPrivateKey = this.RSAService.ToXmlString(true);
            return xmlPrivateKey;
        }
    }
}
