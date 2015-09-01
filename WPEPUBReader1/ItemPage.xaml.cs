using WPEPUBReader1.Common;
using WPEPUBReader1.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VersFx.Formats.Text.Epub;
using Windows.Storage;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using VersFx.Formats.Text.Epub.Entities;
using VersFx.Formats.Text.Epub.Schema.Opf;
using VersFx.Formats.Text.Epub.Schema.Navigation;
using WPEPUBReader1.WebView;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WPEPUBReader1.EPUB.Utils;


// The Hub Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace WPEPUBReader1
{


    //TO-DO создать класс или дочерний объект типа singleton с глобальной областью видимости  с данными текущей открытой книги
    public static class CurrentBook
    {
        private static EpubBook _currentBook = new EpubBook();
        private static StorageFolder _booksFolder = null;


        public static async Task<StorageFolder> GetBookFolder(string path = "Books")
        {
            try
            {
                _booksFolder = await Windows.ApplicationModel.Package.Current.InstalledLocation.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
            }
            catch (Exception)
            {

                throw new Exception("/Books/ Folder cannot be opened or created");
            }
            return _booksFolder;
        }

        public static EpubBook CurrentEpubBook
        {
            get { return _currentBook; }
        }

        public static async Task<bool> SetCurentBookContent(string fileName)
        {
            _currentBook = await EpubReader.OpenBookAsync("fileName");
            if (_currentBook != null)
            {
                return true;
            }
            return false;
        }
    }

    public sealed partial class ItemPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        public static EpubBook currentEpubBook = new EpubBook();

        StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;

        private StreamUriWinRTResolver myStorageResolver;
        private MemoryStreamUriWinRTResolver myMemoryResolver;

        public ItemPage()
        {
            myStorageResolver = new StreamUriWinRTResolver();
            myMemoryResolver = new MemoryStreamUriWinRTResolver();
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public static void ShowLoadingProgress(double progress)
        {
            var frame = (Frame)Window.Current.Content;
            var page = (ItemPage)frame.Content;
            page.bookLoadingProgressBar.Value = progress;
        }


        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var item = await SampleDataSource.GetItemAsync((string)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        

        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            //Init epub object. 

            //bool fileExist = await EpubReader.DoesFileExistAsync(Windows.ApplicationModel.Package.Current.InstalledLocation, "test.epub");
            //if (!fileExist)
            //    throw new Exception(string.Format("File test.epub not found, bitch"));

            progressbar.Text = "Загрузка книги";
            await progressbar.ShowAsync();
            //bookLoadingProgressBar.Visibility = Visibility.Visible;

            // Opening a book
            currentEpubBook = await EpubReader.OpenBookAsync("test.epub");

            if (currentEpubBook != null)
            {
                loadEbookButton.Content = "Loaded";
                loadEbookButton.IsEnabled = false;
            }
            //// COMMON PROPERTIES
            //// Book's title
            //string title = currentEpubBook.Title;
            //// Book's authors (comma separated list)
            //string author = currentEpubBook.Author;
            //// Book's authors (list of authors names)
            //List<string> authors = currentEpubBook.AuthorList;
            //// Book's cover image (null if there are no cover)
            //BitmapImage coverImage = currentEpubBook.CoverImage;
            //// ShowCoverImage(coverImage); //Only for testing purposes

            

            // CONTENT

            // Book's content (HTML files, stlylesheets, images, fonts, etc.)
            EpubContent bookContent = currentEpubBook.Content;


            // IMAGES

            // All images in the book (file name is the key)
            //Dictionary<string, EpubByteContentFile> images = bookContent.Images;

            //EpubByteContentFile firstImage = images.Values.First();

            //// Content type (e.g. EpubContentType.IMAGE_JPEG, EpubContentType.IMAGE_PNG)
            //EpubContentType contentType = firstImage.ContentType;

            //// MIME type (e.g. "image/jpeg", "image/png")
            //string mimeContentType = firstImage.ContentMimeType;



            // HTML & CSS

            // All XHTML files in the book (file name is the key)
            Dictionary<string, EpubTextContentFile> htmlFiles = bookContent.Html;

            // All CSS files in the book (file name is the key)
            Dictionary<string, EpubTextContentFile> cssFiles = bookContent.Css;

            

            // All CSS content in the book
            //foreach (EpubTextContentFile cssFile in cssFiles.Values)
            //{
            //    string cssContent = cssFile.Content;
            //}
            // OTHER CONTENT

            // All fonts in the book (file name is the key)
            // Dictionary<string, EpubByteContentFile> fonts = bookContent.Fonts;

            // All files in the book (including HTML, CSS, images, fonts, and other types of files)
            //TO-DO looks like this dcitionary not working well at the moment, have to trace
            //Dictionary<string, EpubContentFile> allFiles = bookContent.AllFiles;

            //To-DO:
            //Определить первый файл в книге - через spine или через guide
            //Отслеживать клики по экрану и по краям экрана - чтобы листать вперед и назад.
            //Отслеживать, когда на экране последняя column из файла и нужно подгружать следующую
 
            await progressbar.HideAsync();
            progressbar.Text = "Форматирование";
            await progressbar.ShowAsync();
            // Entire HTML content of the book should be injected in case we are showing chapter by chapter, and not pretending to load the whole set of chapters
            //foreach (KeyValuePair<string, EpubTextContentFile> htmlItem in htmlFiles)
            //{
            //    string injectedItem = WebViewHelpers.injectMonocle(htmlItem.Value.Content,
            //   (int)bookReaderWebViewControl.ActualWidth, (int)bookReaderWebViewControl.ActualHeight);
            //    htmlItem.Value.Content = injectedItem;
            //}

            // --- Организуем навигацию из потока --------------------
            //Uri url = bookReaderWebViewControl.BuildLocalStreamUri("MemoryTag", "section4.xhtml");
            Uri url = bookReaderWebViewControl.BuildLocalStreamUri("MemoryTag", "index.html");
            bookReaderWebViewControl.NavigateToLocalStreamUri(url, myMemoryResolver);

            //Now we could have a look at the chapters list
            chaptersMenuButton.IsEnabled = true;
            await progressbar.HideAsync();
            //bookLoadingProgressBar.Visibility = Visibility.Collapsed;

            // ACCESSING RAW SCHEMA INFORMATION

            //// EPUB OPF data
            //EpubPackage package = epubBook.Schema.Package;

            //// Enumerating book's contributors
            //foreach (EpubMetadataContributor contributor in package.Metadata.Contributors)
            //{
            //    string contributorName = contributor.Contributor;
            //    string contributorRole = contributor.Role;
            //}

            //// EPUB NCX data
            //EpubNavigation navigation = epubBook.Schema.Navigation;

            //// Enumerating NCX metadata
            //foreach (EpubNavigationHeadMeta meta in navigation.Head)
            //{
            //    string metadataItemName = meta.Name;
            //    string metadataItemContent = meta.Content;
            //}

        }

        // Temporary function to experiemnt with creating image control run-time
        private void ShowCoverImage(BitmapImage coverImage)
        {
            Image coverImageControl = new Image() { Width = 320, Height = 480, Stretch = Stretch.Uniform };
            coverImageControl.Source = coverImage;
            ContentRoot.Children.Add(coverImageControl);
        }

        private void CommandBar_Opened(object sender, object e)
        {

        }

        private void chaptersMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // CHAPTERS
            // Enumerating chapters
            chaptersMenuButton.IsEnabled = false;
            ObservableCollection<EpubChapter> fullListOfChapters = new ObservableCollection<EpubChapter>();

            foreach (EpubChapter chapter in currentEpubBook.Chapters)
            {
                // Title of chapter
                // string chapterTitle = chapter.Title;

                // HTML content of current chapter
                //string chapterHtmlContent = chapter.HtmlContent;
                fullListOfChapters.Add(chapter);
                // Nested chapters
                foreach (EpubChapter subChapter in chapter.SubChapters)
                {
                    subChapter.Title = "    " + subChapter.Title;
                    fullListOfChapters.Add(subChapter);
                }
                //List<EpubChapter> subChapters = chapter.SubChapters;
            }
            ChaptersList.ItemsSource = fullListOfChapters;
            // open the popup-window if it isn't open already 
            if (!StandartPopup.IsOpen) { StandartPopup.IsOpen = true; }
        }

        private void ClosePopupClicked(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            if (StandartPopup.IsOpen) { StandartPopup.IsOpen = false; }
            chaptersMenuButton.IsEnabled = true;
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            IndexFileMonocleGenerator _index = new IndexFileMonocleGenerator(currentEpubBook.Title, currentEpubBook.Author, 530);
            EpubTextContentFile _result = _index.CreateIndex(currentEpubBook.Chapters, currentEpubBook.Content.Html);
            try {
                currentEpubBook.Content.Html.Add("index.html", _result);
            } catch (ArgumentException)
            {
                currentEpubBook.Content.Html["index.html"] = _result;
                Debug.WriteLine($"\n---------------------------\nAn element with Key = \"index.html\" UPDATED.\n");
            }

        }
    }
}
