using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WebXmlLoader
{
    public class XmlLoader
    {
        public async Task<byte[]> LoadByteFromUrlAsync(string url)
        {
            using (var webClient = new WebClient())
                return await webClient.DownloadDataTaskAsync(url);
            
        }

        public T LoadXmlFromByte<T>(byte[] xmlData)
        {
           
            var xmlSer = new XmlSerializer(typeof(T));
            using (var memStream = new MemoryStream(xmlData))
            {
                memStream.Position = 0;

                return (T)xmlSer.Deserialize(memStream);
            }
        }
        public T LoadFromUrl<T>(string url)
        {
            byte[] xmlData;
            using (var webClient = new WebClient())
                xmlData =  webClient.DownloadData(url);

            var xmlSer = new XmlSerializer(typeof(T));
            using (var memStream = new MemoryStream(xmlData))
            {
                memStream.Position = 0;

                return (T)xmlSer.Deserialize(memStream);
            }
        }

        public async Task<T> LoadFromUrlAsync<T>(string url)
        {
            byte[] xmlData;
            using (var webClient = new WebClient())
                 xmlData = await webClient.DownloadDataTaskAsync(url);
            
            var xmlSer = new XmlSerializer(typeof(T));
            using (var memStream = new MemoryStream(xmlData))
            {
                memStream.Position = 0;

                return (T) xmlSer.Deserialize(memStream);
            }
        }
    }
}