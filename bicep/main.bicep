param webAppName string = uniqueString(resourceGroup().id) // Generate unique String for web app name

param sku string = 'B3' // The SKU of App Service Plan

param location string = resourceGroup().location // Location for all resources

var appServicePlanName = toLower('AppServicePlan-${webAppName}')

var webSiteName = toLower('wapp-${webAppName}')

@secure()
param dockerUsername string
@secure()
param dockerPassword string

resource appServicePlan 'Microsoft.Web/serverfarms@2020-06-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: true
    
  }
  sku: {
    name: sku
  }
  kind: 'linux'
  
}
resource appService 'Microsoft.Web/sites@2020-06-01' = {
  name: webSiteName
  location: resourceGroup().location
  properties: {
    siteConfig: {
      appSettings: [
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'true'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: 'https://index.docker.io'
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: dockerUsername
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: dockerPassword
        }
      ]
      linuxFxVersion: 'DOCKER|coderpatros/geoserver-azure-web-app'
    }
    serverFarmId: appServicePlan.id
  }
}


resource geostorage 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: 'geostorage'
  location: resourceGroup().location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
}
