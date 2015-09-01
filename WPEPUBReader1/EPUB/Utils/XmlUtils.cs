using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.Data.Xml.Dom;

namespace VersFx.Formats.Text.Epub.Utils
{
    public static class XmlUtils
    {
        public static async Task<bool> ReadToFollowingAsync(this XmlReader reader, string localName, string namespaceURI)
        {
            if (localName == null ||
                localName.Length == 0)
            {
                throw new
                    ArgumentException(
                    "localName is empty or null");
            }
            if (namespaceURI == null)
            {
                throw new
                     ArgumentNullException(
                    "namespaceURI");
            }

            // atomize local name and namespace
            localName =
             reader.NameTable.Add(localName);
            namespaceURI =
            reader.NameTable.Add(namespaceURI);

            // find element with that name
            
            do {
                if (reader.NodeType == XmlNodeType.Element && ((object)localName == (object)reader.LocalName) && ((object)namespaceURI == (object)reader.NamespaceURI))
                {
                    return true;
                } 
            }while (await reader.ReadAsync());
            return false;
        }
        public static async Task<string> GetFilePathAttributeAsync(Stream stream)
        {
            string result = "full-path attribute not found";

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
            {
                // XmlResolver = null,
                Async = true,
                DtdProcessing = DtdProcessing.Ignore
            };

            using (XmlReader xmlReader = XmlReader.Create(stream, xmlReaderSettings))
            {
                //xmlReader.ReadToFollowing("rootfile");
                if (await xmlReader.ReadToFollowingAsync("rootfile", "urn:oasis:names:tc:opendocument:xmlns:container"))
                {
                    xmlReader.MoveToAttribute("full-path");
                    result = xmlReader.Value;
                }
                else
                {
                    result = "Yes, rootfile not found...";
                }
            }
            return result;
        }

        //Don`t need this anymore, parsing wil happen without XmlDocument class, directly within XmlReader using async method calls
        
        /*
        public static XmlDocument LoadDocument(Stream stream)
        {
            //Loading the Zip archive entity into memory and returning it as a XmlDocument for further parsing
            XmlDocument result = new XmlDocument();
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
            {
               // XmlResolver = null,
                DtdProcessing = DtdProcessing.Ignore
            };
            using (XmlReader xmlReader = XmlReader.Create(stream, xmlReaderSettings))
                result.Load(xmlReader);
            return result;
        }*/
    }
}
