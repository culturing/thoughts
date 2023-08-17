using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using HeyRed.MarkdownSharp;

namespace Thoughts;

class Program
{
    static Markdown md = new Markdown();
    static string ContentTemplate = File.ReadAllText("Templates/content.html");
    static Regex YearMonthRegex = new Regex(@"\d{4}-(01|02|03|04|05|06|07|08|09|10|11|12)");
    static async Task Main(string[] args)
    {
        Directory.Delete("Output", true);
        Directory.CreateDirectory("Output/Thoughts");  

        foreach (string path in Directory.EnumerateFiles("Thoughts"))
        {
            string content = File.ReadAllText(path);
            string contentHtml = md.Transform(content);      
            string finalHtml = ContentTemplate.Replace("{{content}}", contentHtml);
            string fileName = YearMonthRegex.Replace(Path.GetFileNameWithoutExtension(path), string.Empty).Trim();
            File.WriteAllText($"Output/Thoughts/{fileName}.html", finalHtml);
        }
    }
}