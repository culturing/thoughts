using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using HeyRed.MarkdownSharp;

namespace Thoughts;

class Thought
{
    public string Title { get; set; }
    public string Link { get; set; }
    public bool Bold { get; set; } = false;
    public DateTime PublicationDate { get; set; }
    public static List<string> Months = new List<string> { "", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
    public string FilePath { get; set; }
    public int Page { get; set; }
    public string Style()
    {
        if (string.IsNullOrEmpty(_Style))
        {
            _Style = string.Empty;
            if (Bold)
                _Style += "font-weight: bold;";
        }
        return _Style;
    }

    protected string _Style;
}

class Program
{
    static Markdown md = new Markdown();
    static string ContentTemplate = File.ReadAllText("Templates/content.html");
    static Regex YearMonthRegex = new Regex(@"\d{4}-(01|02|03|04|05|06|07|08|09|10|11|12)");
    static List<Thought> Thoughts { get; set; } = new List<Thought>();
    static Dictionary<string, List<Thought>> ThoughtsByDate = new Dictionary<string, List<Thought>>();

    static void Main(string[] args)
    {
        Directory.Delete("Output", true);
        Directory.CreateDirectory("Output/Thoughts");  

        foreach (string path in Directory.EnumerateFiles("Thoughts"))
        {
            Thought thought = new Thought();

            string[] lines = File.ReadAllLines(path);

            thought.PublicationDate = System.DateTime.Parse(lines[1]);
            thought.Title = lines[0];
            lines[0] = $"## {lines[0]}";
            lines[1] = $"<p style='margin:0;'><em><small><small>{lines[1]}</small></small></em></p>";                

            string content = String.Join("\n", lines);
            string contentHtml = md.Transform(content);      
            string finalHtml = ContentTemplate.Replace("{{content}}", contentHtml);
            string fileName = YearMonthRegex.Replace(Path.GetFileNameWithoutExtension(path), string.Empty).Trim();            
            string finalPath = $"Output/Thoughts/{fileName}.html";
            File.WriteAllText(finalPath, finalHtml);

            thought.Link = $"<a href=\"{finalPath}\">{thought.Title}</a>";
            thought.FilePath = finalPath;

            Thoughts.Add(thought);

        // Sort for chronology
            string key = $"{Thought.Months[thought.PublicationDate.Month]} {thought.PublicationDate.Year}";
            if (!ThoughtsByDate.ContainsKey(key))
                ThoughtsByDate[key] = new List<Thought>();
            ThoughtsByDate[key].Add(thought);
        }

        // Build Index
        string indexHtml = string.Empty;
        foreach(Thought thought in Thoughts.OrderBy(p => Regex.Replace(p.Title, @"[^\w\s]", "")))
        {
            indexHtml += $"<div style='{thought.Style()}'>{thought.Link}</div>\n";
        }

    // Build Chronology
        string chronologyHtml = string.Empty;
        foreach(KeyValuePair<string, List<Thought>> kvp in ThoughtsByDate.OrderByDescending(kvp => DateTime.Parse(kvp.Key)))
        {
            //chronologyHtml += $"<h3>{kvp.Key}</h3>\n";
            foreach(Thought thought in Enumerable.Reverse(kvp.Value))
            {
                chronologyHtml += $"<div style='{thought.Style()}'>{thought.Link}<p style='margin:0;'><em><small><small>{kvp.Key}</small></small></em></p></div>\n";                    
            }
        }

    // Set previous and next links
        List<Thought> OrderedThoughts = Thoughts.OrderBy(thought => thought.PublicationDate).ToList();
        for (int i = 0; i < OrderedThoughts.Count; ++i)
        {
            Thought thought = OrderedThoughts[i];
            string previousPath = string.Empty;
            string nextPath = string.Empty;
            if (i > 0)
            {
                Thought prev = OrderedThoughts[i-1];
                previousPath = Path.GetFileName(prev.FilePath);
            }
            if (i < OrderedThoughts.Count - 1)
            {
                Thought next = OrderedThoughts[i+1];
                nextPath = Path.GetFileName(next.FilePath);
            }
            string contents = File.ReadAllText(thought.FilePath);
            contents = contents.Replace("{{previous}}", previousPath);
            contents = contents.Replace("{{next}}", nextPath);
            File.WriteAllText(thought.FilePath, contents);
        }

        string finalIndexHtml = File.ReadAllText("Templates/index.html")
            .Replace("{{index}}", indexHtml)
            .Replace("{{chronology}}", chronologyHtml);

        File.WriteAllText("index.html", finalIndexHtml);
    }
}