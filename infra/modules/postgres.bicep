@description('The name of the PostgreSQL server to create.')
param name string
@description('Azure region.')
param location string
@description('Tags applied to every resource.')
param tags object
@description('Principal ID of the managed identity that gets Contributor on the PostgreSQL server.')
param principalId string

// Create a PostgreSQL flexible server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2025-08-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    storage: {
      storageSizeGB: 32
    }
  }
  identity: {
    type: 'SystemAssigned,UserAssigned'
    principalId: principalId
  }
}

// Enable the pgvector extension on the PostgreSQL server
resource pgVector 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2025-08-01' = {
  name: 'pgvector.enabled'
  parent: postgresServer
  properties: {
    source: 'azure.extensions'
    value: 'VECTOR'
  }
}

// Create a default database on the PostgreSQL server
resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2025-08-01' = {
  name: 'defaultdb'
  parent: postgresServer
  dependsOn: [
    pgVector
  ]
  properties: {
    charset: 'UTF8'
    collation: 'en_US.UTF8'
  }
}

// firewall rule to allow access from the container apps environment
resource firewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2025-08-01' = {
  name: 'allow-container-apps'
  parent: postgresServer
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '255.255.255.255'
  }
}

// Entra admins 
resource entraAdmin 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2025-08-01' = {
  name: 'entra-admin'
  parent: postgresServer
  properties: {
    principalType: 'Group'
    principalName: 'Entra Admins'
  }
}
