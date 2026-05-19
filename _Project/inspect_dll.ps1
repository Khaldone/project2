$dllPath = "c:\Users\aalta\Downloads\Project\Assets\Packages\Imgur.NET.0.1.0\lib\netstandard2.1\Imgur.dll"
$runtimeDir = [System.Runtime.InteropServices.RuntimeEnvironment]::GetRuntimeDirectory()
$paths = [System.IO.Directory]::GetFiles($runtimeDir, "*.dll")
$resolver = New-Object System.Reflection.PathAssemblyResolver($paths)
$mlc = New-Object System.Reflection.MetadataLoadContext($resolver)
$asm = $mlc.LoadFromAssemblyPath($dllPath)
foreach ($t in $asm.GetTypes()) {
    Write-Host "=== $($t.FullName) ==="
    foreach ($p in $t.GetProperties()) {
        $attrs = ""
        foreach ($ca in $p.CustomAttributes) {
            $attrs += " [$($ca.AttributeType.Name)]"
        }
        Write-Host "  PROP: $($p.Name) : $($p.PropertyType.Name)$attrs"
    }
    foreach ($f in $t.GetFields([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance)) {
        Write-Host "  FIELD: $($f.Name) : $($f.FieldType.Name)"
    }
}
$mlc.Dispose()
