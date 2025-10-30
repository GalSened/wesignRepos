Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

Import-Module Dism

function Check-WindowsFeature {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=$true)] [string]$FeatureName 
    )  
  if((Get-WindowsOptionalFeature -FeatureName $FeatureName -Online).State -eq "Enabled") {
        Write-Host "$FeatureName is already installed"
        
    } else {
        DISM /online /Enable-Feature /Featurename:$FeatureName /All 
    }
  }

Check-WindowsFeature NetFx4ServerFeatures
Check-WindowsFeature NetFx4