using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersFx.Formats.Text.Epub.Entities;

namespace WPEPUBReader1.EPUB.Utils
{
    public class IndexFileSceleton {
        public string title;
        public string author;
        public int height;
        public bool showScrubber;
        public string[] scripts;
        public string[] styles;
        public List<EpubChapter> chapters;
        public Dictionary<string, EpubTextContentFile> xhtmlFiles;
    }
    /// <summary>
    /// A helper class for creating proper monocle environment
    /// </summary>  
    // TO-DO: Will be more consistent o create a separate class IndexFileSceleton
    // with all possible options - author, height, scrubber visibility and a list of scripts and styles to include
    public class IndexFileMonocleGenerator
    {
        private string _templateContent;
        private static readonly string _coreJSmodule = "script/monocore.js";
        private static readonly string _coreCSSmodule = "css/monocore.css";
        private string _author;
        private int _height;
        private string _title;

        public string generatedFileContent { get { return _templateContent; } }

        /// <summary>
        /// IndexFileMonocleGenerator constructor initiates the private value with correct HTML code injection 
        /// </summary>
        /// <param name="title">
        /// Page title, in most cases should be equal to book title.
        /// </param>
        /// <param name="height">
        /// Reader container height. Equal to 530px in case of basic resolution screens. Should be calculated basing on device Height
        /// </param>
        public IndexFileMonocleGenerator(string title, string author, int height = 530)
        {
            _author = author;
            _title = title;
            _height = height;
            _templateContent = $"<!DOCTYPE html>" +
                "<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">" +
                $"<head>\n<title> {title}</title>\n" + "<meta charset = \"utf-8\"/>\n" +
                "<meta name=\"viewport\" content=\"width = device-width, initial-scale = 1.0, maximum-scale = 1, user-scalable = no\" />\n" +
                $"<script src=\"{_coreJSmodule}\"></script>" +
                "<script src=\"script/monoctrl.js\"></script>\n" +
                $"<link rel=\"stylesheet\" type=\"text/css\" href=\"{_coreCSSmodule}\"/>\n" +
                "<link rel=\"stylesheet\" type=\"text/css\" href=\"css/monoctrl.css\"/>\n" +
                "<!--Here the magic starts-->\n" +
                "<style> .reader {" +
                    $"position: relative; width: 100%; height: {height}px;}} " +
                    "@-ms-viewport { width: device-width; height: device-height}" + "</style>\n" +
                "<script type=\"text/javascript\">\n" +
                "var bookData = {\n" +
                  "getComponents:" +
                  "getContents:" +
                  "getComponent: function(componentId) {" +
                    "return { url: componentId};},\n" +
                  "getMetaData: function(key) {" +
                    $"return  {{title: \"{title}\", creator:\"{author}\"}}[key];" +
                   "}\n" +
                 "};\n" +
                "Monocle.Events.listen(window, 'load', function(){" +
                "var readerOptions = {}; readerOptions.panels = Monocle.Panels.IMode; " +
                "window.reader = Monocle.Reader('rdr', bookData, readerOptions, function(rdr) {"+
                " var scrubber = new Monocle.Controls.Scrubber(rdr); rdr.addControl(scrubber, 'standard', { container: 'scrubber' });\n"+
                "var stencil = new Monocle.Controls.Stencil(rdr); rdr.addControl(stencil); "+
                "}" +
                ");});" +
                "</script>\n" +
                "</head>\n" +
                "<body>\n" +
                "<div class = \"reader\" id=\"rdr\"></div>\n" +
                "<div id=\"scrubber\"></div>\n" +
                "</body>\n" +
                "</html>";

        }
        public EpubTextContentFile CreateIndex(List<EpubChapter> chapters, Dictionary<string, EpubTextContentFile> xhtmlFiles)
        {
            
            //reading content to string
            var _indexFileContent = _templateContent;

            string componentsTagString = "getComponents:";
            string contentsTagString = "getContents:";
            int indexOfComponentsTag = _indexFileContent.IndexOf(componentsTagString);
            int lengthOfComponentsTag = componentsTagString.Length;

            string componentInjectonPrefix = " function() {\nreturn [";
            string componentInjectonPostfix = "];\n},\n";
            string allComponents = BuildAllXHTMLFilesToString(xhtmlFiles);
            //Injecting after the <head>        
            _indexFileContent = _indexFileContent.Insert(indexOfComponentsTag + lengthOfComponentsTag,
                componentInjectonPrefix + allComponents + componentInjectonPostfix);

            int indexOfContentTag = _indexFileContent.IndexOf(contentsTagString);
            string allContent = BuildChapterListToString(chapters);
            _indexFileContent = _indexFileContent.Insert(indexOfContentTag + contentsTagString.Length,
                componentInjectonPrefix + allContent + componentInjectonPostfix);
            //Adding a new item to the HTML list of book files
            EpubTextContentFile _indexMemoryItem = new EpubTextContentFile()
            {
                Content = _indexFileContent,
                FileName = "index.html",
                ContentType = EpubContentType.XHTML_1_1,
                ContentMimeType = "text/html"
            };
            //currentEpubBook.Content.Html.Add("index.html", _indexMemoryItem);
            Debug.WriteLine(string.Format("-------- Here comes the final result:------- \n{0}\n ------------------------------", _indexFileContent));
            return _indexMemoryItem;

        }
        
        /// <summary>
        /// Helper function - converts list of book chapters to JS code injection - {title: "", src: ""}
        /// </summary>
        /// <param name="chapters">
        /// List of book chapters.
        /// </param>
        private string BuildChapterListToString(List<EpubChapter> chapters)
        {
            string _result = string.Empty;
            foreach (EpubChapter chapter in chapters)
            {
                // Title of chapter
                _result += "{";
                _result += "title: \"" + chapter.Title + "\", src: '" + chapter.ContentFileName + "'";
                // Nested chapters
                if (chapter.SubChapters.Any())
                {
                    _result += ", children: [";
                    foreach (EpubChapter subChapter in chapter.SubChapters)
                    {
                        _result += "\n{title: \"" + subChapter.Title + "\", src: '" + subChapter.ContentFileName + "'},";
                    }
                    _result = _result.Remove(_result.Length - 1);
                    _result += "]\n";
                }
                _result += "},";
            }
            _result = _result.Remove(_result.Length - 1);
            return _result;
        }
        /// <summary>
        /// Helper function - builds a JS string from all book .xhtml file names
        /// </summary>
        /// <param name="xhtmlFiles">
        /// List of files available.
        /// </param>
        private string BuildAllXHTMLFilesToString(Dictionary<string, EpubTextContentFile> xhtmlFiles)
        {
            string _result = string.Empty;
            _result = "'" + string.Join("', '", xhtmlFiles.Keys) + "'";
            return _result;
        }

        //same as (ms-appx://HTML/index.html)
        //var _htmlFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
        //_htmlFolder = await _htmlFolder.GetFolderAsync("HTML");
        //(int)bookReaderWebViewControl.ActualHeight
        //current

        //IndexFileMonocleGenerator _generator = new IndexFileMonocleGenerator(title, author, height);
    }
}
