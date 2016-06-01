using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace UtmCliUtility.XML{

    [XmlRoot("A")]
    public class A
    {
        private Url[] _urlArray;

        [XmlElement("url")]
        public Url[] UrlArray
        {
            get
            {
                if (_urlArray == null) return new Url[0];
                return _urlArray;
            }
            set { _urlArray = value; }
        }

        [XmlElement("sign", IsNullable = true)]
        public string Sign { get; set; }

        [XmlElement("ver")]
        public int Ver { get; set; }

        [XmlElement("error")]
        public string Error { get; set; }

        public A()
        {
            UrlArray = new Url[0];
        }

        public bool ShouldSerializeSign()
        {
            return Sign != null;
        }

        public static A Create(byte[] xmlContentInBytes)
        {
            if (xmlContentInBytes == null) throw new ArgumentNullException("xmlContentInBytes");
            var ser = new XmlSerializer(typeof(A));
            return (A)ser.Deserialize(new MemoryStream(xmlContentInBytes));
        }

        public static A ChangeHost(A a, string host, int port)
        {
            a.UrlArray = a.UrlArray.Where(u => !string.IsNullOrEmpty(u.Value)).Select(u => new Url()
            {
                ReplyId = u.ReplyId,
                Value = new UriBuilder(new Uri(u.Value))
                {
                    Host = host,
                    Port = port
                }.ToString()
            }).ToArray();
            return a;
        }
    }

    public class Url
    {
        [XmlAttribute("replyId")]
        public string ReplyId { get; set; }

        [ XmlText()]
        public string Value { get; set; }
    }
}
