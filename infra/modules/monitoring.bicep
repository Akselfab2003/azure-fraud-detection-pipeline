@description('Name of the Log Analytics workspace.')
param name string
@description('Name of the Application Insights resource.')
param appInsightsName string
@description('Azure region.')
param location string
@description('Tags applied to every resource.')
param tags object

resource law 'Microsoft.OperationalInsights/workspaces@2026-03-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 7
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: law.id
  }
}

output workspaceName string = law.name
output workspaceId string = law.id
output appInsightsConnectionString string = appInsights.properties.ConnectionString
