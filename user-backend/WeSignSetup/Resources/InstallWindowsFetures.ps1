Set-ExecutionPolicy Bypass -Scope Process

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
Check-WindowsFeature NetFx4Extended-ASPNET45
Check-WindowsFeature MicrosoftWindowsPowerShell
Check-WindowsFeature KeyDistributionService-PSH-Cmdlets
Check-WindowsFeature TlsSessionTicketKey-PSH-Cmdlets
Check-WindowsFeature Tpm-PSH-Cmdlets
Check-WindowsFeature MicrosoftWindowsPowerShellV2
Check-WindowsFeature Server-Psh-Cmdlets
Check-WindowsFeature MicrosoftWindowsPowerShellISE
Check-WindowsFeature ActiveDirectory-PowerShell
Check-WindowsFeature DirectoryServices-AdministrativeCenter
Check-WindowsFeature IIS-WebServerRole
Check-WindowsFeature IIS-WebServer
Check-WindowsFeature IIS-CommonHttpFeatures
Check-WindowsFeature IIS-Security
Check-WindowsFeature IIS-RequestFiltering
Check-WindowsFeature IIS-StaticContent
Check-WindowsFeature IIS-DefaultDocument
Check-WindowsFeature IIS-DirectoryBrowsing
Check-WindowsFeature IIS-HttpErrors
Check-WindowsFeature IIS-ApplicationDevelopment
Check-WindowsFeature IIS-WebSockets
Check-WindowsFeature IIS-ApplicationInit
Check-WindowsFeature IIS-NetFxExtensibility
Check-WindowsFeature IIS-NetFxExtensibility45
Check-WindowsFeature IIS-ISAPIExtensions
Check-WindowsFeature IIS-ISAPIFilter
Check-WindowsFeature IIS-ASPNET
Check-WindowsFeature IIS-ASPNET45
Check-WindowsFeature IIS-ASP
Check-WindowsFeature IIS-CGI
Check-WindowsFeature IIS-HealthAndDiagnostics
Check-WindowsFeature IIS-HttpLogging
Check-WindowsFeature IIS-LoggingLibraries
Check-WindowsFeature IIS-RequestMonitor
Check-WindowsFeature IIS-HttpTracing
Check-WindowsFeature IIS-CustomLogging
Check-WindowsFeature IIS-ODBCLogging
Check-WindowsFeature IIS-WindowsAuthentication
Check-WindowsFeature IIS-Performance
Check-WindowsFeature IIS-HttpCompressionStatic
Check-WindowsFeature IIS-HttpCompressionDynamic
Check-WindowsFeature IIS-WebServerManagementTools
Check-WindowsFeature IIS-ManagementConsole
Check-WindowsFeature IIS-LegacySnapIn
Check-WindowsFeature IIS-ManagementService
Check-WindowsFeature IIS-IIS6ManagementCompatibility
Check-WindowsFeature IIS-Metabase
Check-WindowsFeature IIS-FTPServer
Check-WindowsFeature IIS-FTPSvc
Check-WindowsFeature WAS-WindowsActivationService
Check-WindowsFeature MicrosoftWindowsPowerShellRoot
Check-WindowsFeature WAS-ProcessModel
Check-WindowsFeature WAS-NetFxEnvironment
Check-WindowsFeature WAS-ConfigurationAPI
Check-WindowsFeature IIS-HostableWebCore
Check-WindowsFeature WCF-Services45
Check-WindowsFeature WCF-HTTP-Activation45
Check-WindowsFeature WCF-TCP-PortSharing45
Check-WindowsFeature ADCertificateServicesRole
Check-WindowsFeature CertificateServices
Check-WindowsFeature Smtpsvc-Admin-Update-Name
Check-WindowsFeature UpdateServices-RSAT
Check-WindowsFeature UpdateServices-API
Check-WindowsFeature UpdateServices-UI
Check-WindowsFeature ServerCore-WOW64
Check-WindowsFeature Printing-Client
Check-WindowsFeature Printing-Client-Gui
Check-WindowsFeature ServerCore-EA-IME-WOW64
Check-WindowsFeature NetFx3ServerFeatures
Check-WindowsFeature NetFx3
Check-WindowsFeature Server-Shell
Check-WindowsFeature Internet-Explorer-Optional-amd64
Check-WindowsFeature Server-Gui-Mgmt
Check-WindowsFeature RSAT
Check-WindowsFeature ADCertificateServicesManagementTools
Check-WindowsFeature CertificateServicesManagementTools
Check-WindowsFeature RSAT-AD-Tools-Feature
Check-WindowsFeature RSAT-ADDS-Tools-Feature
Check-WindowsFeature DirectoryServices-DomainController-Tools
Check-WindowsFeature DirectoryServices-ADAM-Tools
Check-WindowsFeature WindowsServerBackupSnapin
Check-WindowsFeature Windows-Defender-Gui
Check-WindowsFeature Microsoft-Hyper-V-Management-Clients
Check-WindowsFeature Microsoft-Hyper-V-Management-PowerShell
Check-WindowsFeature MediaPlayback
Check-WindowsFeature WindowsMediaPlayer
Check-WindowsFeature Microsoft-Hyper-V-Common-Drivers-Package
Check-WindowsFeature Microsoft-Hyper-V-Guest-Integration-Drivers-Package
Check-WindowsFeature Microsoft-Windows-NetFx-VCRedist-Package
Check-WindowsFeature Microsoft-Windows-Printing-PrintToPDFServices-Package
Check-WindowsFeature Microsoft-Windows-Printing-XPSServices-Package
Check-WindowsFeature Microsoft-Windows-Client-EmbeddedExp-Package
Check-WindowsFeature Printing-PrintToPDFServices-Features
Check-WindowsFeature Printing-XPSServices-Features
Check-WindowsFeature TelnetClient
Check-WindowsFeature SMB1Protocol
Check-WindowsFeature ServerManager-Core-RSAT
Check-WindowsFeature ServerManager-Core-RSAT-Role-Tools
Check-WindowsFeature ServerManager-Core-RSAT-Feature-Tools
Check-WindowsFeature SmbDirect
Check-WindowsFeature Windows-Defender-Features
Check-WindowsFeature Windows-Defender
Check-WindowsFeature RSAT-RDS-Tools-Feature
Check-WindowsFeature ServerCore-EA-IME
Check-WindowsFeature Server-Drivers-Printers
Check-WindowsFeature Server-Drivers-General
Check-WindowsFeature SearchEngine-Client-Package
Check-WindowsFeature FileAndStorage-Services
Check-WindowsFeature Storage-Services
Check-WindowsFeature File-Services
Check-WindowsFeature CoreFileServer
Check-WindowsFeature ServerCore-Drivers-General
Check-WindowsFeature ServerCore-Drivers-General-WOW64
