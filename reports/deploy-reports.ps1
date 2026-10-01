$ErrorActionPreference='Stop'; $base='http://kenn/Reports/api/v2.0'; $folder='Desafio3'
try { Invoke-RestMethod "$base/Folders" -UseDefaultCredentials -Method Post -ContentType 'application/json' -Body (@{Name=$folder;Path="/$folder"}|ConvertTo-Json) } catch { Write-Host 'Carpeta existente o no creada:' $_.Exception.Message }
Write-Host 'La API REST de SSRS varía por instalación. Si el POST falla, use el Portal: Nueva > Origen de datos, y Cargar los RDL.'
foreach($n in 'ClientesActivos','IngresosClientes','ClientesInactivos'){ Write-Host "Pendiente subir reports/$n.rdl a /Desafio3 y enlazar CompanyManagementDS" }
