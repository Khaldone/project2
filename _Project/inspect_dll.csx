using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

var dllPath = @"c:\Users\aalta\Downloads\Project\Assets\Packages\Imgur.NET.0.1.0\lib\netstandard2.1\Imgur.dll";
var runtimeDir = RuntimeEnvironment.GetRuntimeDirectory();
var paths = Directory.GetFiles(runtimeDir, "*.dll").Append(dllPath).ToArray();
var resolver = new PathAssemblyResolver(paths);
using var mlc = new MetadataLoadContext(resolver);
var asm = mlc.LoadFromAssemblyPath(dllPath);

foreach (var t in asm.GetTypes())
{
    var props = t.GetProperties();
    var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
    if (props.Length == 0 && fields.Length == 0) continue;
    
    Console.WriteLine($"=== {t.FullName} ===");
    foreach (var p in props)
    {
        var required = p.CustomAttributes.Any(a => a.AttributeType.Name == "RequiredMemberAttribute") ? " [Required]" : "";
        Console.WriteLine($"  PROP: {p.Name} : {p.PropertyType.Name}{required}");
    }
    foreach (var f in fields)
    {
        Console.WriteLine($"  FIELD: {f.Name} : {f.FieldType.Name}");
    }
}
