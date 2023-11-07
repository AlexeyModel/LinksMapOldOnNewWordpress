// See https://aka.ms/new-console-template for more information
using System.Xml.Linq;

if (args.Count() != 0 && args.Count() % 2 == 0)
{
    var result = new List<string>();

    var newValues = new Dictionary<string, string>();
    var oldValues = new Dictionary<string, string>();

    var parms = new Dictionary<string,string>();

    for(var i = 0; i < args.Count(); i++)
    {
        parms.Add(args[i], args[i++ + 1]);
    }

    var str = "";
    using (var reader = new StreamReader(parms["-po"]))
    {
        string input;
        while ((input = reader.ReadLine()) != null)
        {
            str = str + input.Replace("&#31;", " ");
        }
    }
    File.WriteAllText(parms["-po"], str);

    str = "";
    using (var reader = new StreamReader(parms["-pn"]))
    {
        string input;
        while ((input = reader.ReadLine()) != null)
        {
            str = str + input.Replace("&#31;", " ");
        }
    }
    File.WriteAllText(parms["-pn"], str);

    var oldDoc = XDocument.Load(parms["-po"]).Document;
    var newDoc = XDocument.Load(parms["-pn"]).Document;

    var newItems = newDoc?
                    .Descendants("rss")
                    .Descendants("channel")
                    .Descendants("item");
   
    XNamespace wpns = "http://wordpress.org/export/1.2/";

    foreach (var vItem in newItems)
    {
        string? link; string? oldId;

        link = vItem.Element("link")?.Value;

        foreach(var postmeta in vItem.Elements(wpns + "postmeta"))
        {
            if(postmeta.Element(wpns + "meta_key")?.Value == "old_wp_post_id")
            {
                oldId = postmeta.Element(wpns + "meta_value")?.Value;
                
                if (!string.IsNullOrEmpty(link) && !string.IsNullOrEmpty(oldId))
                {
                    newValues.Add(oldId, link);
                }

                break;
            }
        }
    }

    var oldItems = oldDoc?
                    .Descendants("rss")
                    .Descendants("channel")
                    .Descendants("item");

    foreach (var vItem in oldItems)
    {
        string? link; string? oldId;

        link = vItem.Element("link")?.Value;
        oldId = vItem.Element(wpns + "post_id")?.Value;

        if (!string.IsNullOrEmpty(link) && !string.IsNullOrEmpty(oldId))
        {
            oldValues.Add(oldId, link);
        }
    }

    var oldDomain = parms["-do"];
    var newDomain = parms["-dn"];

    foreach (var vItem in oldValues)
    {
        var oldUri = vItem.Value.Replace(oldDomain, string.Empty);
        var newUri = newValues[vItem.Key].Replace(newDomain, string.Empty);
        result.Add($"{oldUri} {newUri};");
    }

    File.WriteAllLines("oldnew.map", result);
}
else
{
    Console.WriteLine("Example:");
    Console.WriteLine("-do old_domain -dn new_domain -po old_path_export_xml1 -pn new_path_export_xml1");
    Console.WriteLine("-------------------------------------------------------------------------------");
    Console.WriteLine("old_domain  - with protocol http/https");
    Console.WriteLine("new_domain  - with protocol http/https");
}