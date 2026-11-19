// PostgreSQL Flexible Server (Burstable) for transactions/cases, with pgvector allowed.
//
// What Bicep does NOT do (track under issue 4.1 / run as a migration or init step):
//   - CREATE EXTENSION vector;            -> allow-listing below only permits it
//   - Create Postgres roles for app identities (managed identities) -> done in SQL via
//     pgaadauth_create_principal(...) while connected as the Entra admin below.
//
// Auth: Entra only, no passwords anywhere (no secrets in code, per CLAUDE.md).

@description('The name of the PostgreSQL server to create.')
param name string
@description('Azure region.')
param location string
@description('Tags applied to every resource.')
param tags object
@description('Name of the database the services connect to.')
param databaseName string = 'fraud'
@description('PostgreSQL major version.')
param postgresVersion string = '16'
@description('Object ID of the Entra principal that becomes server admin (e.g. the app managed identity principalId, or a group).')
param adminPrincipalId string
@description('Display name of the Entra admin principal (identity name or group name).')
param adminPrincipalName string
@description('Type of the Entra admin principal.')
@allowed([
  'User'
  'Group'
  'ServicePrincipal'
])
param adminPrincipalType string = 'ServicePrincipal'

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2025-08-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: postgresVersion
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    // Entra-only: password auth off, so no admin password to store.
    authConfig: {
      activeDirectoryAuth: 'Enabled'
      passwordAuth: 'Disabled'
      tenantId: tenant().tenantId
    }
  }
}

// Child resources of a flexible server must be deployed one at a time, otherwise
// ARM returns ServerIsBusy. Each resource below dependsOn the previous one.

// Allow-list the vector extension (pgvector). Still needs CREATE EXTENSION in SQL.
resource allowVector 'Microsoft.DBforPostgreSQL/flexibleServers/configurations@2025-08-01' = {
  name: 'azure.extensions'
  parent: postgresServer
  properties: {
    source: 'user-override'
    value: 'VECTOR'
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2025-08-01' = {
  name: databaseName
  parent: postgresServer
  dependsOn: [
    allowVector
  ]
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// 0.0.0.0 - 0.0.0.0 is the special "allow Azure services" rule: any Azure-hosted client
// can reach the endpoint, but still has to authenticate with Entra. Container Apps on the
// Consumption profile have no fixed outbound IP, so a tighter IP rule isn't possible
// without VNet integration. Revisit if the environment moves into a VNet.
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2025-08-01' = {
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  parent: postgresServer
  dependsOn: [
    database
  ]
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// The resource name must be the principal's object ID.
resource entraAdmin 'Microsoft.DBforPostgreSQL/flexibleServers/administrators@2025-08-01' = {
  name: adminPrincipalId
  parent: postgresServer
  dependsOn: [
    allowAzureServices
  ]
  properties: {
    principalType: adminPrincipalType
    principalName: adminPrincipalName
    tenantId: tenant().tenantId
  }
}

output id string = postgresServer.id
output name string = postgresServer.name
output fqdn string = postgresServer.properties.fullyQualifiedDomainName
output databaseName string = database.name
