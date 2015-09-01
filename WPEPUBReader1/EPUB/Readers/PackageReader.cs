using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Xml;
using VersFx.Formats.Text.Epub.Schema.Opf;
using VersFx.Formats.Text.Epub.Utils;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class PackageReader
    {
        //Parsing metadata, manifest, spine and guide
        public static async Task<EpubPackage> ReadPackageAsync(ZipArchive epubArchive, string rootFilePath)
        {
            EpubPackage result = new EpubPackage();

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
            {
                // XmlResolver = null,
                Async = true,
                DtdProcessing = DtdProcessing.Ignore
            };

            ZipArchiveEntry rootFileEntry = epubArchive.GetEntry(rootFilePath);
            if (rootFileEntry == null)
                throw new Exception(string.Format("EPUB parsing error: {0} file not found in archive.", rootFilePath));
            //Starting content.opf parsing...
            using (Stream containerStream = rootFileEntry.Open())
            {
                using (XmlReader xmlReader = XmlReader.Create(containerStream, xmlReaderSettings))
                {
                    await xmlReader.ReadToFollowingAsync("package", "http://www.idpf.org/2007/opf");
                    //Trying to get version attribute from <package version=... 
                    //Looks like we only need EPUB version data and we don`t care about unique-identifier
                    //if EPUB version is FUBAR then throwing an exeption
                    xmlReader.MoveToAttribute("version");
                    string epubVersionValue = xmlReader.Value;
                    if (epubVersionValue == "2.0")
                        result.EpubVersion = EpubVersion.EPUB_2;
                    else
                   if (epubVersionValue == "3.0")
                        result.EpubVersion = EpubVersion.EPUB_3;
                    else
                        throw new Exception(String.Format("Unsupported EPUB version: {0}.", epubVersionValue));

                    //Reading metadata
                    EpubMetadata metadata = await ReadMetadataAsync(xmlReader, result.EpubVersion);
                    result.Metadata = metadata;
                    //Reading manifest
                    EpubManifest manifest = await ReadManifestAsync(xmlReader);
                    result.Manifest = manifest;
                    //Reading spine
                    EpubSpine spine = await ReadSpineAsync(xmlReader);
                    result.Spine = spine;
                    //Reading guide. And we actually don`t care if it is no present in our EPUB...
                    bool isGuidePresent = await xmlReader.ReadToFollowingAsync("guide", "http://www.idpf.org/2007/opf");
                    if (isGuidePresent)
                    {
                        EpubGuide guide = await ReadGuideAsync(xmlReader);
                        result.Guide = guide;
                    }
                }
            }

            return result;
        }

        private static async System.Threading.Tasks.Task<EpubMetadata> ReadMetadataAsync(XmlReader reader, EpubVersion epubVersion)
        {
            EpubMetadata result = new EpubMetadata();
            result.Titles = new List<string>();
            result.Creators = new List<EpubMetadataCreator>();
            result.Subjects = new List<string>();
            result.Publishers = new List<string>();
            result.Contributors = new List<EpubMetadataContributor>();
            result.Dates = new List<EpubMetadataDate>();
            result.Types = new List<string>();
            result.Formats = new List<string>();
            result.Identifiers = new List<EpubMetadataIdentifier>();
            result.Sources = new List<string>();
            result.Languages = new List<string>();
            result.Relations = new List<string>();
            result.Coverages = new List<string>();
            result.Rights = new List<string>();
            result.MetaItems = new List<EpubMetadataMeta>();

            //Parsing all metadata insides and saving it in EpubMetadata instance
            //

            //Мне нужно пройтись по всем нодам внутри метадаты последовательно, извлечь ноды указанные в массиве metadataNodesNames...
            //... и сохранить их в структуре EpubMetadata
            //В каждой итерации нам нужно извлечь имя нода, сделать маленькими буквами и,
            // в зависимости от того есть ли он в массиве - выполнить запись в структуру
            //ИЛИ мы можем тупо искать по заданным в массиве именам, с опасностью, что какая-то сука написала капсами и это ебнет весь ридер
            //
            bool isMetadataAvailable = await reader.ReadToFollowingAsync("metadata", "http://www.idpf.org/2007/opf");
            if (!isMetadataAvailable)
                throw new Exception("EPUB parsing error: metadata not found in the package.");

            while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "metadata"))
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.LocalName.ToLowerInvariant())
                        {
                            case "title":
                                result.Titles.Add(reader.ReadElementContentAsString());
                                break;
                            case "creator":
                                EpubMetadataCreator creator = new EpubMetadataCreator();
                                creator.Role = reader.GetAttribute("opf:role");
                                creator.FileAs = reader.GetAttribute("opf:file-as");
                                creator.Creator = reader.ReadElementContentAsString();
                                result.Creators.Add(creator);
                                break;
                            case "subject":
                                result.Subjects.Add(reader.ReadElementContentAsString());
                                break;
                            case "description":
                                result.Description = reader.ReadElementContentAsString();
                                break;
                            case "publisher":
                                result.Publishers.Add(reader.ReadElementContentAsString());
                                break;
                            case "contributor":
                                EpubMetadataContributor contributor = new EpubMetadataContributor();
                                contributor.Role = reader.GetAttribute("opf:role");
                                contributor.FileAs = reader.GetAttribute("opf:file-as");
                                contributor.Contributor = reader.ReadElementContentAsString();
                                result.Contributors.Add(contributor);
                                break;
                            case "date":
                                EpubMetadataDate date = new EpubMetadataDate();
                                date.Event = reader.GetAttribute("opf:event");
                                date.Date = reader.ReadElementContentAsString();
                                result.Dates.Add(date);
                                break;
                            case "type":
                                result.Types.Add(reader.ReadElementContentAsString());
                                break;
                            case "format":
                                result.Formats.Add(reader.ReadElementContentAsString());
                                break;
                            case "identifier":
                                EpubMetadataIdentifier identifier = new EpubMetadataIdentifier();
                                identifier.Id = reader.GetAttribute("id");
                                identifier.Scheme = reader.GetAttribute("opf:scheme");
                                identifier.Identifier = reader.ReadElementContentAsString();
                                result.Identifiers.Add(identifier);
                                break;
                            case "source":
                                result.Sources.Add(reader.ReadElementContentAsString());
                                break;
                            case "language":
                                result.Languages.Add(reader.ReadElementContentAsString());
                                break;
                            case "relation":
                                result.Relations.Add(reader.ReadElementContentAsString());
                                break;
                            case "coverage":
                                result.Coverages.Add(reader.ReadElementContentAsString());
                                break;
                            case "rights":
                                result.Rights.Add(reader.ReadElementContentAsString());
                                break;
                            //looks like there is an optional refining node "meta" and it is present in EPUB3
                            case "meta":
                                if (epubVersion == EpubVersion.EPUB_2)
                                {
                                    EpubMetadataMeta meta = new EpubMetadataMeta();
                                    meta.Name = reader.GetAttribute("name");
                                    meta.Content = reader.GetAttribute("content");
                                    result.MetaItems.Add(meta);
                                }
                                else
                                    if (epubVersion == EpubVersion.EPUB_3)
                                {
                                    EpubMetadataMeta meta = new EpubMetadataMeta();
                                    meta.Id = reader.GetAttribute("id");
                                    meta.Refines = reader.GetAttribute("refines");
                                    meta.Property = reader.GetAttribute("property");
                                    meta.Scheme = reader.GetAttribute("scheme");
                                    meta.Content = reader.ReadElementContentAsString();
                                    result.MetaItems.Add(meta);
                                }
                                break;
                        }
                        break;
                }
            }

            return result;
        }

        private static async System.Threading.Tasks.Task<EpubManifest> ReadManifestAsync(XmlReader reader)
        {
            EpubManifest result = new EpubManifest();
            
            bool isManifestFound = await reader.ReadToFollowingAsync("manifest", "http://www.idpf.org/2007/opf");
            if (!isManifestFound)
                throw new Exception("EPUB parsing error: manifest declarations not found in the package.");

            while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "manifest"))
            {
                if (!String.IsNullOrWhiteSpace(reader.LocalName))
                {
                    EpubManifestItem manifestItem = new EpubManifestItem();
                    switch (reader.LocalName.ToLowerInvariant())
                    {
                        case "item":
                            while (reader.MoveToNextAttribute())
                            {
                                switch (reader.LocalName.ToLowerInvariant())
                                {
                                    case "id":
                                        manifestItem.Id = reader.Value;
                                        break;
                                    case "href":
                                        manifestItem.Href = reader.Value;
                                        break;
                                    case "media-type":
                                        manifestItem.MediaType = reader.Value;
                                        break;
                                    case "required-namespace":
                                        manifestItem.RequiredNamespace = reader.Value;
                                        break;
                                    case "required-modules":
                                        manifestItem.RequiredModules = reader.Value;
                                        break;
                                    case "fallback":
                                        manifestItem.Fallback = reader.Value;
                                        break;
                                    case "fallback-style":
                                        manifestItem.FallbackStyle = reader.Value;
                                        break;
                                }
                            }
                            break;
                    }
                    if (String.IsNullOrWhiteSpace(manifestItem.Id))
                        throw new Exception("Incorrect EPUB manifest: item ID is missing");
                    if (String.IsNullOrWhiteSpace(manifestItem.Href))
                        throw new Exception("Incorrect EPUB manifest: item href is missing");
                    if (String.IsNullOrWhiteSpace(manifestItem.MediaType))
                        throw new Exception("Incorrect EPUB manifest: item media type is missing");
                    result.Add(manifestItem);
                }
            }
            return result;
        }

        private static async Task<EpubSpine> ReadSpineAsync(XmlReader reader)
        {
            EpubSpine result = new EpubSpine();
            bool spineFound = await reader.ReadToFollowingAsync("spine", "http://www.idpf.org/2007/opf");
            if (!spineFound)
                throw new Exception("EPUB parsing error: spine declarations not found in the package.");

            if (String.IsNullOrWhiteSpace(reader.GetAttribute("toc")))
                throw new Exception("Incorrect EPUB spine: TOC attribute is missing or empty");
            result.Toc = reader.GetAttribute("toc");

            while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "spine"))
            {
                if (reader.LocalName.ToLowerInvariant() == "itemref")
                {
                    EpubSpineItemRef spineItemRef = new EpubSpineItemRef();
                    spineItemRef.IsLinear = true;
                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.LocalName.ToLowerInvariant())
                        {
                            case "idref":
                                spineItemRef.IdRef = reader.Value;
                                break;
                            case "linear":
                                if (reader.Value.ToLowerInvariant() == "no")
                                {
                                    spineItemRef.IsLinear = false;
                                }
                                break;
                        }
                    }
                    result.Add(spineItemRef);
                }
            }
            return result;
        }


        private static async Task<EpubGuide> ReadGuideAsync(XmlReader reader)
        {
            EpubGuide result = new EpubGuide();

            while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "guide"))
            {
                if (reader.LocalName.ToLowerInvariant() == "reference")
                {
                    EpubGuideReference guideReference = new EpubGuideReference();
                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.LocalName.ToLowerInvariant())
                        {
                            case "type":
                                guideReference.Type = reader.Value;
                                break;
                            case "title":
                                guideReference.Title = reader.Value;
                                break;
                            case "href":
                                guideReference.Href = reader.Value;
                                break;
                        }
                    }
                    if (String.IsNullOrWhiteSpace(guideReference.Type))
                        throw new Exception("Incorrect EPUB guide: item type is missing");
                    if (String.IsNullOrWhiteSpace(guideReference.Href))
                        throw new Exception("Incorrect EPUB guide: item href is missing");
                    result.Add(guideReference);
                }
            }
            return result;
        }

    }
}
