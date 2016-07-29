using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Phantom
{

    public static class TextHandler
    {
        static MarkdownSharp.Markdown formatter;
        static Html2Markdown.Converter converter;
        private static object lockObj = new object();
        public static Tuple<string, FileType> QuickSaveData { get; set; }
        static string stylesheet;

        static TextHandler()
        {
            formatter = new MarkdownSharp.Markdown();
            converter = new Html2Markdown.Converter();
        }

        public static string Convert(string text, FileType to, FileType from)
        {
            if (from == FileType.Markdown && to == FileType.HTML)
            {
                var t = formatter.Transform(text);
                return $"{Resources.StyleSheet.GITHUB_STYLE}<body class='markdown-body'>{t}</body>";
            }

            if (from == FileType.HTML && to == FileType.Markdown)
                return converter.Convert(text);

            return text;
        }

    }

    public enum FileType
    {
        None = 0,
        Markdown = 1,
        HTML = 2,
        Text = 3
    }
}
