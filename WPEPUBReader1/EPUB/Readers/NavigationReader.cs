using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using VersFx.Formats.Text.Epub.Schema.Navigation;
using VersFx.Formats.Text.Epub.Schema.Opf;
using VersFx.Formats.Text.Epub.Utils;
using Windows.Data.Xml.Dom;

namespace VersFx.Formats.Text.Epub.Readers
{
    internal static class NavigationReader
    {
        public static async Task<EpubNavigation> ReadNavigationAsync(ZipArchive epubArchive, string contentDirectoryPath, EpubPackage package)
        {
            EpubNavigation result = new EpubNavigation();
            string tocId = package.Spine.Toc;
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings
            {
                // XmlResolver = null,
                Async = true,
                DtdProcessing = DtdProcessing.Ignore
            };

            if (String.IsNullOrEmpty(tocId))
                throw new Exception("EPUB parsing error: TOC ID is empty.");

            //Cheking if toc id referenced in spine exist in manifest 
            EpubManifestItem tocManifestItem = package.Manifest.FirstOrDefault(item => String.Compare(item.Id, tocId, StringComparison.OrdinalIgnoreCase) == 0);
            if (tocManifestItem == null)
                throw new Exception(String.Format("EPUB parsing error: TOC item {0} not found in EPUB manifest.", tocId));
            //Opening .toc file in archive using href-reference from manifest 
            string tocFileEntryPath = ZipPathUtils.Combine(contentDirectoryPath, tocManifestItem.Href);
            ZipArchiveEntry tocFileEntry = epubArchive.GetEntry(tocFileEntryPath);
            if (tocFileEntry == null)
                throw new Exception(String.Format("EPUB parsing error: TOC file {0} not found in archive.", tocFileEntryPath));
            if (tocFileEntry.Length > Int32.MaxValue)
                throw new Exception(String.Format("EPUB parsing error: TOC file {0} is bigger than 2 Gb.", tocFileEntryPath));
            // ------------------ Actual Parsing starts here: -------------------------
            using (Stream containerStream = tocFileEntry.Open())
            {
                using (XmlReader xmlReader = XmlReader.Create(containerStream, xmlReaderSettings))
                {
                    result.Head = await ReadNavigationHeadAsync(xmlReader);
                    result.DocTitle = await ReadNavigationDocTitleAsync(xmlReader);
                    result.DocAuthors = await ReadNavigationAuthorsAsync(xmlReader);
                    result.NavMap = await ReadNavigationMapAsync(xmlReader);
                    result.NavLists = new List<EpubNavigationList>(); //Empty, because not implemented
                    result.PageList = new EpubNavigationPageList(); //Empty, because not implemented

                }
            }
            
            return result;
            //-------------------------------------------Boring old style Silverlight code...-----------------------------------------------------------------
            //------------------------------------------------------------------------------------------------------------------------------------------------
            //XmlDocument containerDocument;
            //containerDocument = XmlDocument.Load(containerStream);
            //XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(containerDocument.NameTable);
            //xmlNamespaceManager.AddNamespace("ncx", "http://www.daisy.org/z3986/2005/ncx/");
            ////Parsing head section
            //XmlNode headNode = containerDocument.DocumentElement.SelectSingleNode("ncx:head", xmlNamespaceManager);
            //if (headNode == null)
            //    throw new Exception("EPUB parsing error: TOC file does not contain head element");
            //EpubNavigationHead navigationHead = ReadNavigationHead(headNode);
            //result.Head = navigationHead;
            ////Parsing title
            //XmlNode docTitleNode = containerDocument.DocumentElement.SelectSingleNode("ncx:docTitle", xmlNamespaceManager);
            //if (docTitleNode == null)
            //    throw new Exception("EPUB parsing error: TOC file does not contain docTitle element");
            //EpubNavigationDocTitle navigationDocTitle = ReadNavigationDocTitle(docTitleNode);
            //result.DocTitle = navigationDocTitle;
            ////Parsing authors section...
            //result.DocAuthors = new List<EpubNavigationDocAuthor>();
            //foreach (XmlNode docAuthorNode in containerDocument.DocumentElement.SelectNodes("ncx:docAuthor", xmlNamespaceManager))
            //{
            //    EpubNavigationDocAuthor navigationDocAuthor = ReadNavigationDocAuthor(docAuthorNode);
            //    result.DocAuthors.Add(navigationDocAuthor);
            //}
            //Parsing navMap section
            //XmlNode navMapNode = containerDocument.DocumentElement.SelectSingleNode("ncx:navMap", xmlNamespaceManager);
            //if (navMapNode == null)
            //    throw new Exception("EPUB parsing error: TOC file does not contain navMap element");
            //EpubNavigationMap navMap = ReadNavigationMap(navMapNode);
            //result.NavMap = navMap;

            //-----------------------------------TO-DO: Implement -----------------------------------------------------------
            //TO-DO:  Implement  pageList parsing. Needed to tide-up  position inside epub to actual pages of the printed book
            //--------------------------------------------------------------------------------------------------------------
            //Parsing pageList node
            //XmlNode pageListNode = containerDocument.DocumentElement.SelectSingleNode("ncx:pageList", xmlNamespaceManager);
            //if (pageListNode != null)
            //{
            //    EpubNavigationPageList pageList = ReadNavigationPageList(pageListNode);
            //    result.PageList = pageList;
            //}
            ////TO-DO:  Implement  navList parsing. It is a secondary navigation system for supplied book info - schemes, fugures, diagrams, illustrations etc
            ////Parsing navList nodes 
            //result.NavLists = new List<EpubNavigationList>();
            //foreach (XmlNode navigationListNode in containerDocument.DocumentElement.SelectNodes("ncx:navList", xmlNamespaceManager))
            //{
            //    EpubNavigationList navigationList = ReadNavigationList(navigationListNode);
            //    result.NavLists.Add(navigationList);
            //}
            //--------------------------------------------------------------------------------------------------------------

        }

        //Reading navigation map starting from <navMap> node
        private static async Task<EpubNavigationMap> ReadNavigationMapAsync(XmlReader reader)
        {
            EpubNavigationMap result = new EpubNavigationMap();
            bool mapFound = await reader.ReadToFollowingAsync("navMap", "http://www.daisy.org/z3986/2005/ncx/");
            if (!mapFound)
                throw new Exception("EPUB parsing error: navMap section not found in the .toc file.");
            //reading till the </navMap> tag appearance
            while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "navMap"))
            {
                //We are looking for a top-level <navPoint> entries, considering that it could be any level of nesting:
                if ((reader.LocalName == "navPoint") && (reader.NodeType != XmlNodeType.EndElement))
                {
                    //We need to create a subreader space to limit the scope for each single navPoint
                    XmlReader subReader = reader.ReadSubtree();
                    EpubNavigationPoint navigationPoint = await ReadNavigationPointAsync(subReader);
                    //we reached the end of the top-level <navPoint> entry and it is time to add it to collection and to get rid of the sub-reader
                    result.Add(navigationPoint);
                    subReader.Dispose();

                }
            }
            return result;
        }

        private static async Task<EpubNavigationPoint> ReadNavigationPointAsync(XmlReader reader)
        {
            EpubNavigationPoint result = new EpubNavigationPoint();
            //we have to skip first entry as it could be empty after new sub-reader created
            if (reader.NodeType == XmlNodeType.None) await reader.ReadAsync();
            //Now the pointer should point to the <navPoint> itself
            while (reader.MoveToNextAttribute()) //Doing this we just passing through <navPoint> tag
            {
                switch (reader.LocalName.ToLowerInvariant()) //We have to collect all possible attributes from the <navPoint>
                {
                    case "id":
                        result.Id = reader.Value;
                        break;
                    case "class":
                        result.Class = reader.Value;
                        break;
                    case "playorder":
                        result.PlayOrder = reader.Value;
                        break;
                }
            }
            if (String.IsNullOrWhiteSpace(result.Id))
                throw new Exception("Incorrect EPUB navigation point: point ID is missing");

            result.NavigationLabels = new List<EpubNavigationLabel>();
            result.ChildNavigationPoints = new List<EpubNavigationPoint>();
            // We need to make sure that we will return pointer back to <navPoint> entry after reading all attributes
            reader.MoveToElement();
            //Now we are looking for subnodes - navLabel, content and sub-navPoints
            while (await reader.ReadAsync())
            {
                if (reader.NodeType != XmlNodeType.EndElement)
                    switch (reader.LocalName.ToLowerInvariant())
                    {
                        case "navlabel":
                            EpubNavigationLabel navigationLabel = await ReadNavigationLabelAsync(reader);// Adding label to collection
                            result.NavigationLabels.Add(navigationLabel);
                            break;
                        case "content":
                            EpubNavigationContent content = await ReadNavigationContentAsync(reader); //Adding content to collection
                            result.Content = content;
                            break;
                        case "navpoint": //Yeep. Looks like we found a <navPoint> sub-node
                            XmlReader subTree = reader.ReadSubtree(); //Cooking a separate sub-reader scope for this node to separate it from the others siblings
                            EpubNavigationPoint childNavigationPoint = await ReadNavigationPointAsync(subTree);// I hate recursion...
                            //Adding a child to a collection
                            result.ChildNavigationPoints.Add(childNavigationPoint);
                            subTree.Dispose();
                            break;
                    }
            };

            return result;
        }

        private static async Task<EpubNavigationLabel> ReadNavigationLabelAsync(XmlReader reader)
        {
            EpubNavigationLabel result = new EpubNavigationLabel();
            var navigationLabelText = string.Empty;
            //We have to read to <text> subnode of the navLabel node
            do
            {
                if ((reader.LocalName.ToLowerInvariant() == "text") && (reader.NodeType == XmlNodeType.Element))
                {
                    navigationLabelText = reader.ReadElementContentAsString();
                }

            } while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName.ToLowerInvariant() == "navlabel"));

            if (string.IsNullOrEmpty(navigationLabelText))
                throw new Exception("Incorrect EPUB navigation label: label text element is missing");
            result.Text = navigationLabelText;
            return result;
        }

        private static async Task<EpubNavigationContent> ReadNavigationContentAsync(XmlReader reader)
        {
            EpubNavigationContent result = new EpubNavigationContent();
            bool contentFound = await reader.ReadToFollowingAsync("content", "http://www.daisy.org/z3986/2005/ncx/");
            while (reader.MoveToNextAttribute())
            {
                switch (reader.LocalName.ToLowerInvariant())
                {
                    case "id":
                        result.Id = reader.Value;
                        break;
                    case "src":
                        result.Source = reader.Value;
                        break;
                }
            }

            if (String.IsNullOrWhiteSpace(result.Source))
                throw new Exception("Incorrect EPUB navigation content: content source is missing");
            reader.MoveToElement();
            return result;
        }

        private static async Task<List<EpubNavigationDocAuthor>> ReadNavigationAuthorsAsync(XmlReader reader)
        {
            List<EpubNavigationDocAuthor> result = new List<EpubNavigationDocAuthor>();
            bool authorFound = await reader.ReadToFollowingAsync("docAuthor", "http://www.daisy.org/z3986/2005/ncx/");

            ////we don't really care if there is no authors mentioned in toc file... But we could save a warning to a log file if any
            //TO-DO: This code is very week as I don`t have any reliable tools to extract all of docAuthor nodes and parse them.
            //So I`m relying on basic EPUB structure that demands that file should have at least one navMap node and all docAuthors should come before it
            //I think I should rewrite this code later using LINQ to XML

            while (await reader.ReadAsync() && !(reader.IsStartElement() && reader.LocalName == "navMap"))
            {
                EpubNavigationDocAuthor author = new EpubNavigationDocAuthor();
                if (reader.NodeType == XmlNodeType.Text)
                {
                    author.Add(reader.Value);
                    result.Add(author);
                }

            }

            return result;

        }

        private static async Task<EpubNavigationDocTitle> ReadNavigationDocTitleAsync(XmlReader reader)
        {
            EpubNavigationDocTitle result = new EpubNavigationDocTitle();
            bool titleFound = await reader.ReadToFollowingAsync("docTitle", "http://www.daisy.org/z3986/2005/ncx/");

            if (!titleFound)
                throw new Exception("EPUB parsing error: title section not found in the .toc file.");
            while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "docTitle"))
            {
                if (reader.LocalName.ToLowerInvariant() == "text")
                {
                    result.Add(reader.ReadElementContentAsString());
                }
            }
            return result;
        }

        private static async Task<EpubNavigationHead> ReadNavigationHeadAsync(XmlReader reader)
        {
            EpubNavigationHead result = new EpubNavigationHead();
            //"ncx:head" is our starting point
            bool headFound = await reader.ReadToFollowingAsync("head", "http://www.daisy.org/z3986/2005/ncx/");
            if (!headFound)
                throw new Exception("EPUB parsing error: head section not found in the .toc file.");

            while (await reader.ReadAsync() && !(reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "head"))
            {
                if (reader.LocalName.ToLowerInvariant() == "meta")
                {
                    EpubNavigationHeadMeta meta = new EpubNavigationHeadMeta();
                    while (reader.MoveToNextAttribute())
                    {
                        switch (reader.LocalName.ToLowerInvariant())
                        {
                            case "name":
                                meta.Name = reader.Value;
                                break;
                            case "content":
                                meta.Content = reader.Value;
                                break;
                            case "scheme":
                                meta.Scheme = reader.Value;
                                break;
                        }
                    }
                    if (String.IsNullOrWhiteSpace(meta.Name))
                        throw new Exception("Incorrect EPUB navigation meta: meta name is missing");
                    if (meta.Content == null)
                        throw new Exception("Incorrect EPUB navigation meta: meta content is missing");
                    result.Add(meta);
                }
            }
            return result;
        }

      

        //------------------------------------------------------------------------------------------------
        //                                  These function are not implemented yet:
        //------------------------------------------------------------------------------------------------
        //private static EpubNavigationPageList ReadNavigationPageList(XmlNode navigationPageListNode)
        //{
        //    EpubNavigationPageList result = new EpubNavigationPageList();
        //    foreach (XmlNode pageTargetNode in navigationPageListNode.ChildNodes)
        //        if (String.Compare(pageTargetNode.LocalName, "pageTarget", StringComparison.OrdinalIgnoreCase) == 0)
        //        {
        //            EpubNavigationPageTarget pageTarget = ReadNavigationPageTarget(pageTargetNode);
        //            result.Add(pageTarget);
        //        }
        //    return result;
        //}

        //private static EpubNavigationPageTarget ReadNavigationPageTarget(XmlNode navigationPageTargetNode)
        //{
        //    EpubNavigationPageTarget result = new EpubNavigationPageTarget();
        //    foreach (XmlAttribute navigationPageTargetNodeAttribute in navigationPageTargetNode.Attributes)
        //    {
        //        string attributeValue = navigationPageTargetNodeAttribute.Value;
        //        switch (navigationPageTargetNodeAttribute.Name.ToLowerInvariant())
        //        {
        //            case "id":
        //                result.Id = attributeValue;
        //                break;
        //            case "value":
        //                result.Value = attributeValue;
        //                break;
        //            case "type":
        //                EpubNavigationPageTargetType type;
        //                if (!Enum.TryParse<EpubNavigationPageTargetType>(attributeValue, out type))
        //                    throw new Exception(String.Format("Incorrect EPUB navigation page target: {0} is incorrect value for page target type", attributeValue));
        //                result.Type = type;
        //                break;
        //            case "class":
        //                result.Class = attributeValue;
        //                break;
        //            case "playOrder":
        //                result.PlayOrder = attributeValue;
        //                break;
        //        }
        //    }
        //    if (result.Type == default(EpubNavigationPageTargetType))
        //        throw new Exception("Incorrect EPUB navigation page target: page target type is missing");
        //    foreach (XmlNode navigationPageTargetChildNode in navigationPageTargetNode.ChildNodes)
        //        switch (navigationPageTargetChildNode.LocalName.ToLowerInvariant())
        //        {
        //            case "navlabel":
        //                EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationPageTargetChildNode);
        //                result.NavigationLabels.Add(navigationLabel);
        //                break;
        //            case "content":
        //                EpubNavigationContent content = ReadNavigationContent(navigationPageTargetChildNode);
        //                result.Content = content;
        //                break;
        //        }
        //    if (!result.NavigationLabels.Any())
        //        throw new Exception("Incorrect EPUB navigation page target: at least one navLabel element is required");
        //    return result;
        //}

        //private static EpubNavigationList ReadNavigationList(XmlNode navigationListNode)
        //{
        //    EpubNavigationList result = new EpubNavigationList();
        //    foreach (XmlAttribute navigationListNodeAttribute in navigationListNode.Attributes)
        //    {
        //        string attributeValue = navigationListNodeAttribute.Value;
        //        switch (navigationListNodeAttribute.Name.ToLowerInvariant())
        //        {
        //            case "id":
        //                result.Id = attributeValue;
        //                break;
        //            case "class":
        //                result.Class = attributeValue;
        //                break;
        //        }
        //    }
        //    foreach (XmlNode navigationListChildNode in navigationListNode.ChildNodes)
        //        switch (navigationListChildNode.LocalName.ToLowerInvariant())
        //        {
        //            case "navlabel":
        //                EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationListChildNode);
        //                result.NavigationLabels.Add(navigationLabel);
        //                break;
        //            case "navTarget":
        //                EpubNavigationTarget navigationTarget = ReadNavigationTarget(navigationListChildNode);
        //                result.NavigationTargets.Add(navigationTarget);
        //                break;
        //        }
        //    if (!result.NavigationLabels.Any())
        //        throw new Exception("Incorrect EPUB navigation page target: at least one navLabel element is required");
        //    return result;
        //}

        //private static EpubNavigationTarget ReadNavigationTarget(XmlNode navigationTargetNode)
        //{
        //    EpubNavigationTarget result = new EpubNavigationTarget();
        //    foreach (XmlAttribute navigationPageTargetNodeAttribute in navigationTargetNode.Attributes)
        //    {
        //        string attributeValue = navigationPageTargetNodeAttribute.Value;
        //        switch (navigationPageTargetNodeAttribute.Name.ToLowerInvariant())
        //        {
        //            case "id":
        //                result.Id = attributeValue;
        //                break;
        //            case "value":
        //                result.Value = attributeValue;
        //                break;
        //            case "class":
        //                result.Class = attributeValue;
        //                break;
        //            case "playOrder":
        //                result.PlayOrder = attributeValue;
        //                break;
        //        }
        //    }
        //    if (String.IsNullOrWhiteSpace(result.Id))
        //        throw new Exception("Incorrect EPUB navigation target: navigation target ID is missing");
        //    foreach (XmlNode navigationTargetChildNode in navigationTargetNode.ChildNodes)
        //        switch (navigationTargetChildNode.LocalName.ToLowerInvariant())
        //        {
        //            case "navlabel":
        //                EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationTargetChildNode);
        //                result.NavigationLabels.Add(navigationLabel);
        //                break;
        //            case "content":
        //                EpubNavigationContent content = ReadNavigationContent(navigationTargetChildNode);
        //                result.Content = content;
        //                break;
        //        }
        //    if (!result.NavigationLabels.Any())
        //        throw new Exception("Incorrect EPUB navigation target: at least one navLabel element is required");
        //    return result;
        //}
    }
}

// ------------------------------------- Already implemented ------------------------------------------------------------
// ----------------------------------------------------------------------------------------------------------------------
  //private static EpubNavigationHead ReadNavigationHead(XmlNode headNode)
        //{
        //    EpubNavigationHead result = new EpubNavigationHead();
        //    foreach (XmlNode metaNode in headNode.ChildNodes)
        //        if (String.Compare(metaNode.LocalName, "meta", StringComparison.OrdinalIgnoreCase) == 0)
        //        {
        //            EpubNavigationHeadMeta meta = new EpubNavigationHeadMeta();
        //            foreach (XmlAttribute metaNodeAttribute in metaNode.Attributes)
        //            {
        //                string attributeValue = metaNodeAttribute.Value;
        //                switch (metaNodeAttribute.Name.ToLowerInvariant())
        //                {
        //                    case "name":
        //                        meta.Name = attributeValue;
        //                        break;
        //                    case "content":
        //                        meta.Content = attributeValue;
        //                        break;
        //                    case "scheme":
        //                        meta.Scheme = attributeValue;
        //                        break;
        //                }
        //            }
        //            if (String.IsNullOrWhiteSpace(meta.Name))
        //                throw new Exception("Incorrect EPUB navigation meta: meta name is missing");
        //            if (meta.Content == null)
        //                throw new Exception("Incorrect EPUB navigation meta: meta content is missing");
        //            result.Add(meta);
        //        }
        //    return result;
        //}

        //private static EpubNavigationDocTitle ReadNavigationDocTitle(XmlNode docTitleNode)
        //{
        //    EpubNavigationDocTitle result = new EpubNavigationDocTitle();
        //    foreach (XmlNode textNode in docTitleNode.ChildNodes)
        //        if (String.Compare(textNode.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0)
        //            result.Add(textNode.InnerText);
        //    return result;
        //}

        //private static EpubNavigationDocAuthor ReadNavigationDocAuthor(XmlNode docAuthorNode)
        //{
        //    EpubNavigationDocAuthor result = new EpubNavigationDocAuthor();
        //    foreach (XmlNode textNode in docAuthorNode.ChildNodes)
        //        if (String.Compare(textNode.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0)
        //            result.Add(textNode.InnerText);
        //    return result;
        //}

        //private static EpubNavigationMap ReadNavigationMap(XmlNode navigationMapNode)
        //{
        //    EpubNavigationMap result = new EpubNavigationMap();
        //    foreach (XmlNode navigationPointNode in navigationMapNode.ChildNodes)
        //        if (String.Compare(navigationPointNode.LocalName, "navPoint", StringComparison.OrdinalIgnoreCase) == 0)
        //        {
        //            EpubNavigationPoint navigationPoint = ReadNavigationPoint(navigationPointNode);
        //            result.Add(navigationPoint);
        //        }
        //    return result;
        //}

        //private static EpubNavigationPoint ReadNavigationPoint(XmlNode navigationPointNode)
        //{
        //    EpubNavigationPoint result = new EpubNavigationPoint();
        //    foreach (XmlAttribute navigationPointNodeAttribute in navigationPointNode.Attributes)
        //    {
        //        string attributeValue = navigationPointNodeAttribute.Value;
        //        switch (navigationPointNodeAttribute.Name.ToLowerInvariant())
        //        {
        //            case "id":
        //                result.Id = attributeValue;
        //                break;
        //            case "class":
        //                result.Class = attributeValue;
        //                break;
        //            case "playOrder":
        //                result.PlayOrder = attributeValue;
        //                break;
        //        }
        //    }
        //    if (String.IsNullOrWhiteSpace(result.Id))
        //        throw new Exception("Incorrect EPUB navigation point: point ID is missing");
        //    result.NavigationLabels = new List<EpubNavigationLabel>();
        //    result.ChildNavigationPoints = new List<EpubNavigationPoint>();
        //    foreach (XmlNode navigationPointChildNode in navigationPointNode.ChildNodes)
        //        switch (navigationPointChildNode.LocalName.ToLowerInvariant())
        //        {
        //            case "navlabel":
        //                EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationPointChildNode);
        //                result.NavigationLabels.Add(navigationLabel);
        //                break;
        //            case "content":
        //                EpubNavigationContent content = ReadNavigationContent(navigationPointChildNode);
        //                result.Content = content;
        //                break;
        //            case "navpoint":
        //                EpubNavigationPoint childNavigationPoint = ReadNavigationPoint(navigationPointChildNode);
        //                result.ChildNavigationPoints.Add(childNavigationPoint);
        //                break;
        //        }
        //    if (!result.NavigationLabels.Any())
        //        throw new Exception(String.Format("EPUB parsing error: navigation point {0} should contain at least one navigation label", result.Id));
        //    if (result.Content == null)
        //        throw new Exception(String.Format("EPUB parsing error: navigation point {0} should contain content", result.Id));
        //    return result;
        //}

        //private static EpubNavigationLabel ReadNavigationLabel(XmlNode navigationLabelNode)
        //{
        //    EpubNavigationLabel result = new EpubNavigationLabel();
        //    XmlNode navigationLabelTextNode = navigationLabelNode.ChildNodes.OfType<XmlNode>().
        //        Where(node => String.Compare(node.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
        //    if (navigationLabelTextNode == null)
        //        throw new Exception("Incorrect EPUB navigation label: label text element is missing");
        //    result.Text = navigationLabelTextNode.InnerText;
        //    return result;
        //}

        //private static EpubNavigationContent ReadNavigationContent(XmlNode navigationContentNode)
        //{
        //    EpubNavigationContent result = new EpubNavigationContent();
        //    foreach (XmlAttribute navigationContentNodeAttribute in navigationContentNode.Attributes)
        //    {
        //        string attributeValue = navigationContentNodeAttribute.Value;
        //        switch (navigationContentNodeAttribute.Name.ToLowerInvariant())
        //        {
        //            case "id":
        //                result.Id = attributeValue;
        //                break;
        //            case "src":
        //                result.Source = attributeValue;
        //                break;
        //        }
        //    }
        //    if (String.IsNullOrWhiteSpace(result.Source))
        //        throw new Exception("Incorrect EPUB navigation content: content source is missing");
        //    return result;
        //}
//private static EpubNavigationHead ReadNavigationHead(XmlNode headNode)
//{
//    EpubNavigationHead result = new EpubNavigationHead();
//    foreach (XmlNode metaNode in headNode.ChildNodes)
//        if (String.Compare(metaNode.LocalName, "meta", StringComparison.OrdinalIgnoreCase) == 0)
//        {
//            EpubNavigationHeadMeta meta = new EpubNavigationHeadMeta();
//            foreach (XmlAttribute metaNodeAttribute in metaNode.Attributes)
//            {
//                string attributeValue = metaNodeAttribute.Value;
//                switch (metaNodeAttribute.Name.ToLowerInvariant())
//                {
//                    case "name":
//                        meta.Name = attributeValue;
//                        break;
//                    case "content":
//                        meta.Content = attributeValue;
//                        break;
//                    case "scheme":
//                        meta.Scheme = attributeValue;
//                        break;
//                }
//            }
//            if (String.IsNullOrWhiteSpace(meta.Name))
//                throw new Exception("Incorrect EPUB navigation meta: meta name is missing");
//            if (meta.Content == null)
//                throw new Exception("Incorrect EPUB navigation meta: meta content is missing");
//            result.Add(meta);
//        }
//    return result;
//}

//private static EpubNavigationDocTitle ReadNavigationDocTitle(XmlNode docTitleNode)
//{
//    EpubNavigationDocTitle result = new EpubNavigationDocTitle();
//    foreach (XmlNode textNode in docTitleNode.ChildNodes)
//        if (String.Compare(textNode.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0)
//            result.Add(textNode.InnerText);
//    return result;
//}

//private static EpubNavigationDocAuthor ReadNavigationDocAuthor(XmlNode docAuthorNode)
//{
//    EpubNavigationDocAuthor result = new EpubNavigationDocAuthor();
//    foreach (XmlNode textNode in docAuthorNode.ChildNodes)
//        if (String.Compare(textNode.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0)
//            result.Add(textNode.InnerText);
//    return result;
//}

//private static EpubNavigationMap ReadNavigationMap(XmlNode navigationMapNode)
//{
//    EpubNavigationMap result = new EpubNavigationMap();
//    foreach (XmlNode navigationPointNode in navigationMapNode.ChildNodes)
//        if (String.Compare(navigationPointNode.LocalName, "navPoint", StringComparison.OrdinalIgnoreCase) == 0)
//        {
//            EpubNavigationPoint navigationPoint = ReadNavigationPoint(navigationPointNode);
//            result.Add(navigationPoint);
//        }
//    return result;
//}

//private static EpubNavigationPoint ReadNavigationPoint(XmlNode navigationPointNode)
//{
//    EpubNavigationPoint result = new EpubNavigationPoint();
//    foreach (XmlAttribute navigationPointNodeAttribute in navigationPointNode.Attributes)
//    {
//        string attributeValue = navigationPointNodeAttribute.Value;
//        switch (navigationPointNodeAttribute.Name.ToLowerInvariant())
//        {
//            case "id":
//                result.Id = attributeValue;
//                break;
//            case "class":
//                result.Class = attributeValue;
//                break;
//            case "playOrder":
//                result.PlayOrder = attributeValue;
//                break;
//        }
//    }
//    if (String.IsNullOrWhiteSpace(result.Id))
//        throw new Exception("Incorrect EPUB navigation point: point ID is missing");
//    result.NavigationLabels = new List<EpubNavigationLabel>();
//    result.ChildNavigationPoints = new List<EpubNavigationPoint>();
//    foreach (XmlNode navigationPointChildNode in navigationPointNode.ChildNodes)
//        switch (navigationPointChildNode.LocalName.ToLowerInvariant())
//        {
//            case "navlabel":
//                EpubNavigationLabel navigationLabel = ReadNavigationLabel(navigationPointChildNode);
//                result.NavigationLabels.Add(navigationLabel);
//                break;
//            case "content":
//                EpubNavigationContent content = ReadNavigationContent(navigationPointChildNode);
//                result.Content = content;
//                break;
//            case "navpoint":
//                EpubNavigationPoint childNavigationPoint = ReadNavigationPoint(navigationPointChildNode);
//                result.ChildNavigationPoints.Add(childNavigationPoint);
//                break;
//        }
//    if (!result.NavigationLabels.Any())
//        throw new Exception(String.Format("EPUB parsing error: navigation point {0} should contain at least one navigation label", result.Id));
//    if (result.Content == null)
//        throw new Exception(String.Format("EPUB parsing error: navigation point {0} should contain content", result.Id));
//    return result;
//}

//private static EpubNavigationLabel ReadNavigationLabel(XmlNode navigationLabelNode)
//{
//    EpubNavigationLabel result = new EpubNavigationLabel();
//    XmlNode navigationLabelTextNode = navigationLabelNode.ChildNodes.OfType<XmlNode>().
//        Where(node => String.Compare(node.LocalName, "text", StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
//    if (navigationLabelTextNode == null)
//        throw new Exception("Incorrect EPUB navigation label: label text element is missing");
//    result.Text = navigationLabelTextNode.InnerText;
//    return result;
//}

//private static EpubNavigationContent ReadNavigationContent(XmlNode navigationContentNode)
//{
//    EpubNavigationContent result = new EpubNavigationContent();
//    foreach (XmlAttribute navigationContentNodeAttribute in navigationContentNode.Attributes)
//    {
//        string attributeValue = navigationContentNodeAttribute.Value;
//        switch (navigationContentNodeAttribute.Name.ToLowerInvariant())
//        {
//            case "id":
//                result.Id = attributeValue;
//                break;
//            case "src":
//                result.Source = attributeValue;
//                break;
//        }
//    }
//    if (String.IsNullOrWhiteSpace(result.Source))
//        throw new Exception("Incorrect EPUB navigation content: content source is missing");
//    return result;
//}