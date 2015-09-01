using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Xml;
using VersFx.Formats.Text.Epub.Utils;
using Windows.Data.Xml.Dom;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class RootFilePathReader
    {
        public static async System.Threading.Tasks.Task<string> GetRootFilePathAsync(ZipArchive epubArchive)
        {
            //Checking if file exist
            const string EPUB_CONTAINER_FILE_PATH = "META-INF/container.xml";

            ZipArchiveEntry containerFileEntry = epubArchive.GetEntry(EPUB_CONTAINER_FILE_PATH);
            string full_path = string.Empty;

            if (containerFileEntry == null)
                throw new Exception(String.Format("EPUB parsing error: {0} file not found in archive.", EPUB_CONTAINER_FILE_PATH));

            //Loading container.xml to memmory...
            using (Stream containerStream = containerFileEntry.Open())
            {
                // ...and trying to parse it in order to get the full path to the .opf file, like full-path="SomeFolder/SomeFileWithContent.opf"
                full_path = await XmlUtils.GetFilePathAttributeAsync(containerStream);
            }
            //Checking if the problem exist...
            if (full_path == "full-path attribute not found" || full_path == "Yes, rootfile not found...")
            {
                Debug.WriteLine(string.Format("Content.opf path is FUBAR and the problem is: {0}", full_path));
                throw new Exception(string.Format("Content.opf path is FUBAR and the problem is: {0}", full_path));
            }
                
            return full_path;

            //Initial code sucks and is not compatible with Win 8.1 runtime framework
            /*
            xmlNamespaceManager.AddNamespace("cns", "urn:oasis:names:tc:opendocument:xmlns:container");
            XmlNode rootFileNode = containerDocument.DocumentElement.SelectSingleNode("/cns:container/cns:rootfiles/cns:rootfile", xmlNamespaceManager);
            return rootFileNode.Attributes["full-path"].Value;
            */
        }
    }
}
