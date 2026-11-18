targetScope = 'subscription'

@allowed(['dev', 'test', 'prod'])
param env string
param location string
param workload string

var rgName = 'rg-${workload}-${env}'

resource rg 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: rgName
  location: location
  tags: {
    environment: env
    workload: workload
  }
}
