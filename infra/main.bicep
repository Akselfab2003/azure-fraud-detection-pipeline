targetScope = 'subscription'

@description('The name of the environment to deploy to.')
param environmentName string
@description('The location to deploy the resources to.')
param location string
@description('The workload name to deploy.')
param workload string

var tags = {
  project: workload
  env: environmentName
  'azd-env-name': environmentName
}

var rgName = 'rg-${workload}-${environmentName}'

resource rg 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: rgName
  location: location
  tags: tags
}
