# 1. Crear encabezados iniciales usando texto directo
"=========================================================" > "wp81_proyecto_contexto.txt"
" ARQUITECTURA DEL PROYECTO WINDOWS PHONE 8.1" >> "wp81_proyecto_contexto.txt"
"=========================================================" >> "wp81_proyecto_contexto.txt"

# 2. Listar la jerarquía de archivos filtrando carpetas pesadas
Get-ChildItem -Recurse -Name | Where-Object { 
    \$PSItem -notmatch '^(bin|obj|\.vs|\.git|packages|TestResults|Assets)\\' 
} >> "wp81_proyecto_contexto.txt"

"`n=========================================================" >> "wp81_proyecto_contexto.txt"
" CONTENIDO DE LOS ARCHIVOS (XAML / C# / MANIFEST)" >> "wp81_proyecto_contexto.txt"
"=========================================================`n" >> "wp81_proyecto_contexto.txt"

# 3. Recorrer código fuente e inyectarlo usando tuberías directas hacia el archivo fijo
Get-ChildItem -Recurse -File | Where-Object {
    $PSItem.Extension.ToLower() -match '\.(cs|xaml|xml|config|manifest|appxmanifest)$' -and 
    $PSItem.FullName -notmatch '\\(bin|obj|\.vs|\.git|packages|Assets|Properties|AssemblyInfo)\\'
} | ForEach-Object {
    "--- INICIO ARCHIVO: $($PSItem.Name) ---" >> "wp81_proyecto_contexto.txt"
    Get-Content $PSItem.FullName -Raw -Encoding UTF8 >> "wp81_proyecto_contexto.txt"
    "--- FIN ARCHIVO: $($PSItem.Name) ---`n" >> "wp81_proyecto_contexto.txt"
}

Write-Host "¡Completado con éxito! Revisa tu carpeta SpotifyClassic." -ForegroundColor Green
