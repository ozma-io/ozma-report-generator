using System.IO;
using DDDN.OdtToHtml;
using OpenHtmlToPdf;

namespace ReportGenerator
{
    public static class FormatConverter
    {
        public static string OdtToHtml(Stream stream)
        {
            const string contentSubDirname = "content";
            OdtConvertedData? convertedData = null;
            var odtConvertSettings = new OdtConvertSettings
            {
                RootElementTagName = "article",
                RootElementId = "article_id",
                RootElementClassNames = "article_class",
                LinkUrlPrefix = $"/{contentSubDirname}",
                DefaultTabSize = "2rem"
            };
            using (IOdtFile odtFile = new OdtFile(stream))
            {
                convertedData = new OdtConvert().Convert(odtFile, odtConvertSettings);
            }
            var articleHtml = convertedData.Html;
            var articleCss = convertedData.Css;

            var html =
                "<!DOCTYPE html><html><head><meta http-equiv='content-type' content='text/html; charset=UTF-8'><style>" +
                "table, table td { border: 1px solid black; } article {padding: 20px;}  th:empty { display: none;}" +
                articleCss +
                "</style></head>";
            html = html + "<body>" + articleHtml + "</body></html>";
            //var usedFontFamilies = convertedData.UsedFontFamilies;
            //var pageInfo = convertedData.PageInfo;
            return html;
        }

        public static byte[] HtmlToPdf(string html)
        {
            var pdf = Pdf.From(html).Content();
            return pdf;
        }
    }
}
