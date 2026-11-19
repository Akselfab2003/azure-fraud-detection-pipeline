targetScope = 'subscription'

@description('The name of the environment to deploy to.')
param environmentName string
@description('The location to deploy the resources to.')
param location string
@description('The workload name to deploy.')
param workload string

var rgName = 'rg-${workload}-${environmentName}'
var acrName = 'acr${workload}${environmentName}${uniqueString(subscription().id, environmentName, location)}'
var tags = { project: workload, env: environmentName, 'azd-env-name': environmentName }

resource rg 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: rgName
  location: location
  tags: tags
}

module monitoring 'modules/monitoring.bicep' = {
  name: 'monitoring'
  scope: rg
  params: {
    name: 'log-${workload}-${environmentName}'
    appInsightsName: 'appi-${workload}-${environmentName}'
    location: location
    tags: tags
  }
}

module containerAppsEnv 'modules/containerapps-env.bicep' = {
  name: 'containerAppsEnv'
  scope: rg
  params: {
    name: 'cae-${workload}-${environmentName}'
    location: location
    tags: tags
    logAnalyticsName: monitoring.outputs.workspaceName
  }
}

module identity 'modules/identity.bicep' = {
  name: 'identity'
  scope: rg
  params: {
    name: 'identity-${workload}-${environmentName}'
    location: location
    tags: tags
  }
}

module registry 'modules/registry.bicep' = {
  name: 'registry'
  scope: rg
  params: {
    name: acrName
    location: location
    tags: tags
    principalId: identity.outputs.principalId
  }
}

module postgres 'modules/postgres.bicep' = {
  name: 'postgres'
  scope: rg
  params: {
    name: 'psql-${workload}-${environmentName}-${uniqueString(subscription().id, environmentName, location)}'
    location: location
    tags: tags
    adminPrincipalId: identity.outputs.principalId
    adminPrincipalName: identity.outputs.name
  }
}

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = registry.outputs.loginServer
output AZURE_RESOURCE_GROUP string = rg.name
output AZURE_CONTAINER_APPS_ENVIRONMENT_ID string = containerAppsEnv.outputs.id
output AZURE_USER_ASSIGNED_IDENTITY_ID string = identity.outputs.id
output APPLICATIONINSIGHTS_CONNECTION_STRING string = monitoring.outputs.appInsightsConnectionString
