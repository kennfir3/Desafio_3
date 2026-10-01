$ErrorActionPreference='Stop'; $base='http://kenn/Reports/api/v2.0'
function Call($method,$uri,$body=$null){$p=@{Uri=$uri;Method=$method;UseDefaultCredentials=$true;ContentType='application/json'};if($null-ne$body){$p.Body=$body|ConvertTo-Json -Compress};Invoke-RestMethod @p}
try{Call Post "$base/Folders" @{Name='Desafio3';Path='/Desafio3'}|Out-Null}catch{}
try{Call Post "$base/Folders" @{Name='DataSources';Path='/DataSources'}|Out-Null}catch{}
$path='/DataSources/CompanyManagementDS';if(-not (Call Get "$base/DataSources?`$filter=Path%20eq%20%27$path%27").value){Call Post "$base/DataSources" @{Name='CompanyManagementDS';Path=$path;DataSourceType='SQL';ConnectionString='Data Source=localhost;Initial Catalog=CompanyManagement';CredentialRetrieval='integrated';IsEnabled=$true}|Out-Null}
foreach($n in 'ClientesActivos','IngresosClientes','ClientesInactivos'){$content=[Convert]::ToBase64String([IO.File]::ReadAllBytes((Join-Path $PSScriptRoot "$n.rdl")));$r=Call Get "$base/Reports?`$filter=Path%20eq%20%27/Desafio3/$n%27";$b=@{Name=$n;Path="/Desafio3/$n";Content=$content};if($r.value){Call Put "$base/Reports($($r.value[0].Id))" $b|Out-Null}else{Call Post "$base/Reports" $b|Out-Null}}
Write-Host 'Reportes publicados.'
