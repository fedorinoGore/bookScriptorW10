using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersFx.Formats.Text.Epub.Entities;

namespace WPEPUBReader1.WebView
{
    public static class WebViewHelpers
    {
        public static string injectMonocle(string htmlSource, int viewWidth, int viewHeight)
        {
            string headTagString = "<head>";
            int indexOfHeadTag = htmlSource.IndexOf(headTagString);
            int lengthOfHeadTag = headTagString.Length;

            string bodyTagString = "<body>";           
            int lengthOfBodyTag = bodyTagString.Length;
            string bodyEndTagString = "</body>";

            //string headInsertionScriptString = "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, maximum-scale=1, user-scalable = no\" /><script src=\"script/monocore.js\"></script><link rel=\"stylesheet\" type =\"text/css\" href =\"css/monocore.css\" />"+
            //    "<style> #reader {width:" + viewWidth+"px; height:"+viewHeight+"px; border: 0px solid #000;}</style>\n"+ 
            //    "@-ms-viewport {width: device-width;}";
            string headInsertionScriptString = "<meta name=\"viewport\" content=\"width="+viewWidth+", height="+ viewHeight + "initial-scale=1.0, maximum-scale=1, user-scalable=no\" /><script src=\"script/monocore.js\"></script><link rel=\"stylesheet\" type =\"text/css\" href =\"css/monocore.css\" />" +
               "\n<style> #reader\n{width: "+ (viewWidth-10) + "px; height: "+ (viewHeight) + "px; border: 0px solid #000;}\n"+
               "body {overflow-y: hidden;}\n"+"</style>\n"+ 
               "@-ms-viewport {width:"+ viewWidth + "px;\n height:"+ viewHeight + "px;}";


            string bodyInsertionScriptString =
                String.Format("<div id=\"reader\">");
           
            string finalInsertionScriptString =
                String.Format("</div> <script>Monocle.Reader('reader');</script>");


            //Injecting after the <head>        
            htmlSource = htmlSource.Insert(indexOfHeadTag + lengthOfHeadTag + 1, headInsertionScriptString);
            //Injecting after the <body>
            int indexOfBodyTag = htmlSource.IndexOf(bodyTagString);
            htmlSource = htmlSource.Insert(indexOfBodyTag + lengthOfBodyTag + 1, bodyInsertionScriptString);
            //Injecting before the </body>
            int indexOfBodyEndTag = htmlSource.IndexOf(bodyEndTagString);
            htmlSource = htmlSource.Insert(indexOfBodyEndTag - 1, finalInsertionScriptString);

            return htmlSource;
        }
    }
}
