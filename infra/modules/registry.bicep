@description('The name of the registry to create.')
param name string
@description('Azure region.')
param location string
@description('Tags applied to every resource.')
param tags object
@description('The principal ID of the user-assigned managed identity to assign to the registry.')
param principalId string

resource registry 'Microsoft.ContainerRegistry/registries@2025-11-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${principalId}': {}
    }
  }
  sku: {
    name: 'Basic'
  }
  properties: {
    anonymousPullEnabled: false
    adminUserEnabled: false
    zoneRedundancy: 'Disabled'
    publicNetworkAccess: 'Disabled'
    roleAssignmentMode: 'AbacRepositoryPermissions'
  }
}

output loginServer string = registry.properties.loginServer
