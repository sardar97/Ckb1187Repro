using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

Console.WriteLine($"OS      : {RuntimeInformation.OSDescription}");
Console.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"USENLS  : {Environment.GetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_USENLS") ?? "(unset)"}");

Console.WriteLine("\n== 1. What CultureInfo does with each name (this is what the SDK compares) ==");
foreach (var name in new[] { "ckb", "ckb-IQ", "ku", "ku-Arab-IQ" })
{
    try
    {
        var c = CultureInfo.GetCultureInfo(name);
        var chain = new List<string>();
        for (var p = c; !string.IsNullOrEmpty(p.Name); p = p.Parent) chain.Add(p.Name);
        Console.WriteLine($"{name,-11} -> Name='{c.Name}' ({c.EnglishName}); probe order: {string.Join(" > ", chain)}");
    }
    catch (CultureNotFoundException) { Console.WriteLine($"{name,-11} -> CultureNotFoundException"); }
}

Console.WriteLine("\n== 2. Satellite folders the build put next to the app ==");
foreach (var d in Directory.GetDirectories(AppContext.BaseDirectory).Select(Path.GetFileName).Order())
    if (d!.StartsWith("ckb", StringComparison.OrdinalIgnoreCase) || d.StartsWith("ku", StringComparison.OrdinalIgnoreCase))
        Console.WriteLine($"  {d}/");

Console.WriteLine("\n== 3. Does Central Kurdish actually load at runtime? ==");
var asm = Assembly.Load("MudBlazor.Translations");
var baseName = asm.GetManifestResourceNames().First(n => n.EndsWith(".resources"))[..^".resources".Length];
var rm = new ResourceManager(baseName, asm);
var englishCount = rm.GetResourceSet(CultureInfo.InvariantCulture, true, false)!.Cast<DictionaryEntry>().Count();
Console.WriteLine($"neutral (English) strings: {englishCount}");
foreach (var name in new[] { "ckb", "ckb-IQ", "de" })
{
    // tryParents: false => only succeeds if the satellite for exactly this culture was found on disk
    var set = rm.GetResourceSet(CultureInfo.GetCultureInfo(name), true, false);
    Console.WriteLine(set is null
        ? $"{name,-7}: SATELLITE NOT FOUND (runtime looked for folder '{CultureInfo.GetCultureInfo(name).Name}/')"
        : $"{name,-7}: satellite loaded, {set.Cast<DictionaryEntry>().Count()} translated strings");
}
