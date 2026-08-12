// AvaFind v1 stack: one Linux App Service (serves API + built frontend)
// and one Azure SQL Basic database. Everything sized for a low-traffic,
// personal-subscription prototype.

param location string
param namePrefix string
param appServiceSku string
param sqlDatabaseSku string
param sqlAdminLogin string
@secure()
param sqlAdminPassword string
param allowedClientIps array

var suffix = uniqueString(resourceGroup().id)
var webAppName = '${namePrefix}-${suffix}'
var sqlServerName = '${namePrefix}-sql-${suffix}'
var sqlDatabaseName = 'avafind'

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${namePrefix}-plan'
  location: location
  kind: 'linux'
  sku: {
    name: appServiceSku
  }
  properties: {
    reserved: true // required for Linux
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'PYTHON|3.12'
      appCommandLine: 'gunicorn --bind=0.0.0.0:8000 --timeout 120 --workers 2 --worker-class uvicorn.workers.UvicornWorker app.main:app'
      ftpsState: 'Disabled'
      alwaysOn: appServiceSku != 'F1'
      // v1 access control: no auth in the app, so the App Service itself is
      // closed to everything except the allowed IP list. Change the
      // allowedClientIps parameter and redeploy to update.
      ipSecurityRestrictionsDefaultAction: 'Deny'
      ipSecurityRestrictions: [for (ip, i) in allowedClientIps: {
        name: 'allow-client-ip-${i + 1}'
        action: 'Allow'
        priority: 100 + i
        ipAddress: '${ip}/32'
        description: 'Allowed viewer/deployer IP ${i + 1}'
      }]
      // Deployment (Kudu/SCM) endpoint gets the same single-IP restriction,
      // so deploys must come from the allowed IP too.
      scmIpSecurityRestrictionsUseMain: true
      appSettings: [
        { name: 'SCM_DO_BUILD_DURING_DEPLOYMENT', value: 'true' }
        { name: 'SQL_SERVER', value: sqlServer.properties.fullyQualifiedDomainName }
        { name: 'SQL_DATABASE', value: sqlDatabaseName }
        { name: 'SQL_USER', value: sqlAdminLogin }
        { name: 'SQL_PASSWORD', value: sqlAdminPassword }
      ]
    }
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled' // required: App Service (no VNet at this tier) and local import connect over public endpoint with firewall rules
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: sqlDatabaseSku
    tier: sqlDatabaseSku == 'Basic' ? 'Basic' : 'Standard'
  }
  properties: {
    maxSizeBytes: 2147483648 // 2 GB — Basic tier max, far more than one extract needs
  }
}

// Let Azure services (the App Service) reach the SQL server.
resource fwAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Let allowed client IPs run the import script locally against Azure SQL.
resource fwClients 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = [for (ip, i) in allowedClientIps: {
  parent: sqlServer
  name: 'AllowClientIp${i + 1}'
  properties: {
    startIpAddress: ip
    endIpAddress: ip
  }
}]

output webAppName string = webApp.name
output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
output sqlServerFqdn string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabaseName
