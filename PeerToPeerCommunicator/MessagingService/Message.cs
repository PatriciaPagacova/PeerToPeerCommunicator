using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MessagingService
{
    [XmlRoot]
    public class Message
    {
        [XmlElement]
        public string SenderName { get; set; }

        [XmlElement]
        public string MessageText { get; set; }

        [XmlElement]
        public string SendingTime { get; set; }

        [XmlElement]
        public string FileDataInBase64 { get; set; }

        [XmlElement]
        public string FileName { get; set; }

        internal string Serialize()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Message));
            using (var sw = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw))
                {
                    serializer.Serialize(writer, this);
                    return sw.ToString();
                }
            }
        }

        internal static Message Deserialize(string serializeMessage)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(Message));
            using (var sr = new StringReader(serializeMessage))
            {
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    return deserializer.Deserialize(reader) as Message;
                }
            }
        }
    }
}
