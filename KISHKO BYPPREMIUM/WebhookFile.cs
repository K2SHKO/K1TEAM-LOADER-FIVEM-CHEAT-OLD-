using System.IO;

namespace KISHKO_BYPPREMIUM
{
    internal class WebhookFile
    {
        private string v;
        private MemoryStream memoryStream;

        public WebhookFile(string v, MemoryStream memoryStream)
        {
            this.v = v;
            this.memoryStream = memoryStream;
        }
    }
}