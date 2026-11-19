targetScope = 'subscription'

@description('The name of the environment to deploy to.')
param environmentName string
@description('The location to deploy the resources to.')
param location string
@description('The workload name to deploy.')
param workload string

var rgName = 'rg-${workload}-${environmentName}'
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
    name: 'law-${workload}-${environmentName}'
    appInsightsName: 'appinsights-${workload}-${environmentName}'
    location: location
    tags: tags
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
    name: 'acr${workload}${uniqueString(subscription().id, environmentName, location)}'
    location: location
    tags: tags
    identityid: identity.outputs.id
  }
}
