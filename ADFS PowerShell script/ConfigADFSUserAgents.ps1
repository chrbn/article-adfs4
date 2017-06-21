Write-Host "Gettings ADFS supported user agents."
$agents = Get-ADFSProperties | Select-Object -ExpandProperty WIASupportedUserAgents
$newAgent = "Mozilla/5.0"

if($agents.Contains($newAgent) -eq $false) {
    Write-Host -NoNewLine "Adding user agent $newAgent."
    $agents = $agents + "Mozilla/5.0"
    Set-AdfsProperties -WIASupportedUserAgents $agents
    Write-Host -ForegroundColor Green " OK"
}
else {
    Write-Host -ForegroundColor Yellow "User agent $newAgent already present."
}