// AvaFind v1 infrastructure — subscription-scope entry point.
// Creates (or targets) a resource group and deploys the stack into it.
//
//   az deployment sub create --location <region> \
//     --template-file infra/main.bicep --parameters @infra/main.parameters.json

targetScope = 'subscription'

@description('Resource group to deploy into.')
param resourceGroupName string = 'rg-avafind'

@description('Create the resource group (false = it already exists).')
param createResourceGroup bool = true

@description('Azure region for all resources.')
param location string = 'eastus2'

@description('Prefix for resource names. The web app and SQL server get a unique suffix appended (their names must be globally unique).')
param namePrefix string = 'avafind'

@description('App Service Plan SKU. Keep this cheap — prototype only.')
@allowed(['F1', 'B1', 'B2'])
param appServiceSku string = 'B1'

@description('Azure SQL database SKU. Basic = cheapest.')
@allowed(['Basic', 'S0'])
param sqlDatabaseSku string = 'Basic'

@description('SQL admin login name.')
param sqlAdminLogin string = 'avafindadmin'

@secure()
@description('SQL admin password.')
param sqlAdminPassword string

@description('Public IPs allowed to reach the app (v1 has no auth; network restriction is the access control). Update and redeploy when your IP changes.')
param allowedClientIps array

resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = if (createResourceGroup) {
  name: resourceGroupName
  location: location
}

module stack 'resources.bicep' = {
  name: 'avafind-stack'
  scope: resourceGroup(resourceGroupName)
  params: {
    location: location
    namePrefix: namePrefix
    appServiceSku: appServiceSku
    sqlDatabaseSku: sqlDatabaseSku
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    allowedClientIps: allowedClientIps
  }
  dependsOn: [rg]
}

output webAppName string = stack.outputs.webAppName
output webAppUrl string = stack.outputs.webAppUrl
output sqlServerFqdn string = stack.outputs.sqlServerFqdn
output sqlDatabaseName string = stack.outputs.sqlDatabaseName
