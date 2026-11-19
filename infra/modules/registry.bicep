@description('The name of the registry to create.')
param name string
@description('Azure region.')
param location string
@description('Tags applied to every resource.')
param tags object
@description('Principal ID of the managed identity that gets AcrPull on the registry.')
param principalId string

resource registry 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    anonymousPullEnabled: false
    adminUserEnabled: false
    zoneRedundancy: 'Disabled'
    publicNetworkAccess: 'Enabled'
  }
}

resource identityAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, principalId, 'acrpull')
  scope: registry
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '7f951dda-4ed3-4680-a7ca-43fe172d538d'
    ) // AcrPull
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

output loginServer string = registry.properties.loginServer
