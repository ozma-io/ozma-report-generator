using System.IO;
using DDDN.OdtToHtml;
using WkHtmlToPdfDotNet;

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
                "<!DOCTYPE html><html><head><meta http-equiv='content-type' content='text/html; charset=UTF-8'>" +
                "<style>table, table td { border: 1px solid black; } article {padding: 20px;}  th:empty { display: none;}" +
                articleCss + "</style>"+
                "</head>";
            html = html + "<body>" + articleHtml + "</body></html>";
            //var usedFontFamilies = convertedData.UsedFontFamilies;
            //var pageInfo = convertedData.PageInfo;
            return html;
        }

        public static byte[] HtmlToPdf(string html)
        {
            var converter = new SynchronizedConverter(new PdfTools());
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4Plus,
                },
                Objects =
                {
                    new ObjectSettings()
                    {
                        PagesCount = true,
                        HtmlContent = html,
                        WebSettings = {DefaultEncoding = "utf-8"},
                        //HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                    }
                }
            };
            byte[] pdf = converter.Convert(doc);
            return pdf;
        }
    }
}
