using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web;

namespace WPEPUBReader1.WebView
{
    public sealed class StreamUriWinRTResolver : IUriToStreamResolver
    {
        /// <summary>
        /// The entry point for resolving a Uri to a stream.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new Exception();
            }
            string path = uri.AbsolutePath;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Stream Requested: {0}", uri.ToString()));
            }

            // Because of the signature of the this method, it can't use await, so we 
            // call into a seperate helper method that can use the C# await pattern.
            return getContent(path).AsAsyncOperation();
        }

        /// <summary>
        /// Helper that cracks the path and resolves the Uri
        /// Uses the C# await pattern to coordinate async operations
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<IInputStream> getContent(string path)
        {
            // We use a package folder as the source, but the same principle should apply
            // when supplying content from other locations
            StorageFolder current = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("html");
            //string fileExtension = Path.GetExtension(path);

            // Trim the initial '/' if applicable
            if (path.StartsWith("/")) path = path.Remove(0, 1);
            // Split the path into an array of nodes
            string[] nodes = path.Split('/');

            // Walk the nodes of the path checking against the filesystem along the way
            for (int i = 0; i < nodes.Length; i++)
            {
                try
                {
                    // Try and get the node from the file system
                    IStorageItem item = await current.GetItemAsync(nodes[i]);

                    if (item.IsOfType(StorageItemTypes.Folder) && i < nodes.Length - 1)
                    {
                        // If the item is a folder and isn't the leaf node
                        current = item as StorageFolder;
                    }
                    else if (item.IsOfType(StorageItemTypes.File) && i == nodes.Length - 1)
                    {
                        // If the item is a file and is the leaf node
                        
                        //if ((nodes[i] == "monocore.css") || (nodes[i] == "monocore.js"))
                        //{
                            StorageFile f = item as StorageFile;
                            IRandomAccessStream stream = await f.OpenAsync(FileAccessMode.Read);
                            return stream;
                        //}
                        //else //Any other file than locally hosted monocleJS
                        //{
                        //    //TO-DO - check if item exist i the dictionary
                        //    string content = ItemPage.currentEpubBook.Content.Html[nodes[i]].Content;
                        //    using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
                        //    {
                        //        using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                        //        {
                        //            writer.WriteBytes(Encoding.UTF8.GetBytes(content ?? ""));
                        //            await writer.StoreAsync();
                        //            return ms;
                        //        }
                        //    }
                        //}
                    }
                    else
                    {
                        return null;
                        //Leaf is not a file, or the file isn't the leaf node in the path
                        throw new Exception("Invalid path");
                    }
                }
                catch (Exception) { throw new Exception("Invalid path"); }
            }
            return null;
        }
    }

    public sealed class MemoryStreamUriWinRTResolver : IUriToStreamResolver
    {
        private string[] locallyHostedFiles = new string[]{ "monocore.css", "monocore.js", "monoctrl.css", "monoctrl.js"};
        /// <summary>
        /// The entry point for resolving a Uri to a stream.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public IAsyncOperation<IInputStream> UriToStreamAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new Exception();
            }
            string path = uri.AbsolutePath;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Stream Requested: {0}", uri.ToString()));
            }

            // Because of the signature of the this method, it can't use await, so we 
            // call into a separate helper method that can use the C# await pattern.
            return getContent(path).AsAsyncOperation();
        }

        /// <summary>
        /// Helper that cracks the path and resolves the Uri
        /// Uses the C# await pattern to coordinate async operations
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<IInputStream> getContent(string path)
        {
            // We use a package folder as the source, but the same principle should apply
            // when supplying content from other locations
            StorageFolder current = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("html");
            //StorageFolder current = await ApplicationData.Current.LocalFolder.GetFolderAsync("html");
           
            
            // Trim the initial '/' if applicable
            if (path.StartsWith("/")) path = path.Remove(0, 1);
            // Split the path into an array of nodes
            string[] nodes = path.Split('/');

            // If we sure that file is hosted locally then we walk the nodes of the path checking against the filesystem along the way
            if (locallyHosted(nodes))
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    try
                    {
                        // Try and get the node from the file system
                        IStorageItem item = await current.GetItemAsync(nodes[i]);

                        if (item.IsOfType(StorageItemTypes.Folder) && i < nodes.Length - 1)
                        {
                            // If the item is a folder and isn't the leaf node
                            current = item as StorageFolder;
                        }
                        else if (item.IsOfType(StorageItemTypes.File) && i == nodes.Length - 1)
                        {
                            // If the item is a file and is the leaf node                   
                            StorageFile f = item as StorageFile;
                            //string _content = await FileIO.ReadTextAsync(f);
                            IRandomAccessStream stream = await f.OpenAsync(FileAccessMode.Read);
                            if (System.Diagnostics.Debugger.IsAttached)
                            {
                                System.Diagnostics.Debug.WriteLine($"Stream loaded: {nodes[nodes.Length-1]} : {stream.Size}bytes");
                            }
                            return stream;
                        }
                        else
                        {
                            //Leaf is not a file, or the file isn't the leaf node in the path
                            throw new Exception("Leaf is not a file, or the file isn't the leaf node in the path");
                        }
                    }
                    catch (Exception) { throw new Exception("Invalid path"); }
                }
                return null;
            }
            else
            {
                return await createStreamFromMemoryFile(path);
            }
        }

        private async Task<IInputStream> createStreamFromMemoryFile(string fileName)
        {
            //    //TO-DO - check if item exist in the dictionary before any others actions
            string content = string.Empty;
            string fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (fileExtension)
            {
                case ".xhtml":
                case ".html":
                    if (ItemPage.currentEpubBook.Content.Html.ContainsKey(fileName))
                    {
                        content = ItemPage.currentEpubBook.Content.Html[fileName].Content;
                        return await stringContentToInMemoryStream(content, fileName);
                    }
                    else
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debug.WriteLine($"file not found in EPUB: {fileName}");
                        }
                        return null;
                    }
                case ".css":
                    if (ItemPage.currentEpubBook.Content.Css.ContainsKey(fileName))
                    {
                        content = ItemPage.currentEpubBook.Content.Css[fileName].Content;
                        return await stringContentToInMemoryStream(content, fileName);
                    }
                    else
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debug.WriteLine($"file not found in EPUB: {fileName}");
                        }
                        return null;
                    }
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".svg":
                case ".gif":
                    if (ItemPage.currentEpubBook.Content.Images.ContainsKey(fileName))
                    {
                        byte[] byteContent = ItemPage.currentEpubBook.Content.Images[fileName].Content;
                        return await byteContentToInMemoryStream(byteContent, fileName);
                    }
                    else
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                        {
                            System.Diagnostics.Debug.WriteLine($"file not found in EPUB: {fileName}");
                        }
                        return null;
                    }
                case ".js":
                default:
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        System.Diagnostics.Debug.WriteLine($"Trying to resolve not supported file type: {fileExtension}");
                    }
                    return new InMemoryRandomAccessStream();
            }
        }

        private async Task<IInputStream> byteContentToInMemoryStream(byte[] byteContent, string fileName)
        {
            InMemoryRandomAccessStream _ms = new InMemoryRandomAccessStream();
            using (DataWriter _writer = new DataWriter(_ms.GetOutputStreamAt(0)))
            {
                _writer.WriteBytes((byte[])byteContent);
                await _writer.StoreAsync();
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debug.WriteLine($"Stream loaded: {fileName} : {_ms.Size}byte");
                }
                return _ms;
            }
        }

        private async Task<IInputStream> stringContentToInMemoryStream(string content, string fileName)
        {
            InMemoryRandomAccessStream _ms = new InMemoryRandomAccessStream();
            using (DataWriter _writer = new DataWriter(_ms.GetOutputStreamAt(0)))
            {
                _writer.WriteBytes(Encoding.UTF8.GetBytes(content ?? ""));
                await _writer.StoreAsync();
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debug.WriteLine($"Stream loaded: {fileName} : {_ms.Size}byte");
                }
                return _ms;
            }
        }

        private bool locallyHosted(string[] nodes)
        {
            return locallyHostedFiles.Contains(nodes[nodes.Length-1]);
        }

        private async Task<IInputStream> GetContentAsStreamFromEPUB(string itemName)
        {
            byte[] content = ItemPage.currentEpubBook.Content.AllFiles[itemName].Content;
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes((byte[])content);
                    await writer.StoreAsync();
                    return ms;
                }
            }
        }
    }
}
