using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using VersFx.Formats.Text.Epub.Entities;
using VersFx.Formats.Text.Epub.Readers;
using VersFx.Formats.Text.Epub.Schema.Navigation;
using VersFx.Formats.Text.Epub.Schema.Opf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using WPEPUBReader1;
using WPEPUBReader1.EPUB.Utils;
//using VersFx.Formats.Text.Epub.Utils;

namespace VersFx.Formats.Text.Epub
{

    public static class EpubReader
    {
       public static async Task<bool> DoesFileExistAsync(StorageFolder folder, string filename)
        {
            var folders = (await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFoldersAsync()).To‌​List();
            try
            {
                await folder.GetFileAsync(filename);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<EpubBook> OpenBookAsync(string filePath)
        {
            //Changed from .NET function File.Exits to handmade async function
            bool fileExist = await DoesFileExistAsync(Windows.ApplicationModel.Package.Current.InstalledLocation, filePath);
            if (!fileExist)
                throw new FileNotFoundException("Specified epub file not found.", filePath);

            EpubBook book = new EpubBook();
            //File path is now derived from app Local folder
            book.FilePath = filePath;

            //Reworking old Unzip code to comply with Win 8.1 RT framework
            //Opening using LocalFolder as a root dir, file_path may still contain additional sudirectories 
            var zipLocalFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(filePath);
            using (var zipFileStream = await zipLocalFile.OpenStreamForReadAsync())
            {
                using (ZipArchive epubArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
                {
                    book.Schema = await SchemaReader.ReadSchemaAsync(epubArchive);
                    //ItemPage.ShowLoadingProgress(20.0);
                    book.Title = book.Schema.Package.Metadata.Titles.FirstOrDefault() ?? String.Empty;
                    book.AuthorList = book.Schema.Package.Metadata.Creators.Select(creator => creator.Creator).ToList();
                    book.Author = String.Join(", ", book.AuthorList);
                    book.Content = ContentReader.ReadContentFilesToMemory(epubArchive, book);
                    book.CoverImage = await LoadCoverImageAsync(book);
                    book.Chapters = LoadChapters(book, epubArchive);
                    //await ContentReader.ExtractContentFilesToDiskAsync(epubArchive, book);
                }
            }
           
            return book;
        }

        private static async Task<BitmapImage> LoadCoverImageAsync(EpubBook book)
        {
            //TO-DO Currently this function only accepts covers as a image file. If cover is wrapped in.xhtml file - nothing happen.
            List<EpubMetadataMeta> metaItems = book.Schema.Package.Metadata.MetaItems;
            BitmapImage result = new BitmapImage();
            if (metaItems == null || !metaItems.Any())
                return null;
            EpubMetadataMeta coverMetaItem = metaItems.FirstOrDefault(metaItem => String.Compare(metaItem.Name, "cover", StringComparison.OrdinalIgnoreCase) == 0);
            if (coverMetaItem == null)
                return null;
            if (String.IsNullOrEmpty(coverMetaItem.Content))
                throw new Exception("Incorrect EPUB metadata: cover item content is missing");
            EpubManifestItem coverManifestItem = book.Schema.Package.Manifest.FirstOrDefault(manifestItem => String.Compare(manifestItem.Id, coverMetaItem.Content, StringComparison.OrdinalIgnoreCase) == 0);
            if (coverManifestItem == null)
                throw new Exception(String.Format("Incorrect EPUB manifest: item with ID = \"{0}\" is missing", coverMetaItem.Content));
            EpubByteContentFile coverImageContentFile;
            // ---------------------------------------------------------------------------------------------------------------------
            //TO-DO: Currently this function only accepts covers as a image file. If cover is wrapped in.xhtml file - nothing happen.
            // Have to check how people using wrapped cover - do they point <meta content> directly to xhtml or just placing it in the manifest
            // ---------------------------------------------------------------------------------------------------------------------
            if (!book.Content.Images.TryGetValue(coverManifestItem.Href, out coverImageContentFile))
                throw new Exception(String.Format("Incorrect EPUB manifest: item with href = \"{0}\" is missing", coverManifestItem.Href));
            // Old code is not working as SystemDrawings is deprecated
            //using (MemoryStream coverImageStream = new MemoryStream(coverImageContentFile.Content))
            result = await ConvertToBitmapImageAsync(coverImageContentFile.Content);
            return result;
        }
        //Adding this dog-nail function to support previous code
        private static async Task<BitmapImage> ConvertToBitmapImageAsync(byte[] image)
        {
            BitmapImage bitmapimage = null;

            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes((byte[])image);
                    await writer.StoreAsync();
                }
                bitmapimage = new BitmapImage();
                bitmapimage.SetSource(ms);
            }
            return bitmapimage;
        }

        private static List<EpubChapter> LoadChapters(EpubBook book, ZipArchive epubArchive)
        {
            return LoadChapters(book, book.Schema.Navigation.NavMap, epubArchive);
        }

        private static List<EpubChapter> LoadChapters(EpubBook book, List<EpubNavigationPoint> navigationPoints, ZipArchive epubArchive)
        {
            List<EpubChapter> result = new List<EpubChapter>();
            foreach (EpubNavigationPoint navigationPoint in navigationPoints)
            {
                EpubChapter chapter = new EpubChapter();
                chapter.Title = navigationPoint.NavigationLabels.First().Text;
                int contentSourceAnchorCharIndex = navigationPoint.Content.Source.IndexOf('#');
                if (contentSourceAnchorCharIndex == -1)
                    chapter.ContentFileName = navigationPoint.Content.Source;
                else
                {
                    chapter.ContentFileName = navigationPoint.Content.Source.Substring(0, contentSourceAnchorCharIndex);
                    chapter.Anchor = navigationPoint.Content.Source.Substring(contentSourceAnchorCharIndex + 1);
                }
                EpubTextContentFile htmlContentFile;
                if (!book.Content.Html.TryGetValue(chapter.ContentFileName, out htmlContentFile))
                    throw new Exception(String.Format("Incorrect EPUB manifest: item with href = \"{0}\" is missing", chapter.ContentFileName));
                chapter.HtmlContent = htmlContentFile.Content;
                chapter.SubChapters = LoadChapters(book, navigationPoint.ChildNavigationPoints, epubArchive);
                result.Add(chapter);
            }
            return result;
        }
    }
}
