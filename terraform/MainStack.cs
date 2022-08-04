using System;
using System.Collections.Generic;
using Constructs;
using HashiCorp.Cdktf;
using HashiCorp.Cdktf.Providers.Azurerm;

namespace MyCompany.MyApp
{
    class MainStack : TerraformStack
    {
        private const string loc = "uksouth";

        public MainStack(Construct scope, string id) : base(scope, id)
        {
            new AzurermProvider(this, "AzureRm", new AzurermProviderConfig(){
                Features=new AzurermProviderFeatures(){
                    
                }
            });

            var rg = new ResourceGroup(this,"geoserver-dev-rg",new ResourceGroupConfig(){
                Location = loc,
                Name = "geoserver-dev-rg"
            });

            var asp = new AppServicePlan(this, "geoserver-dev-tf-appserviceplan", new AppServicePlanConfig()
            {
                Name = "geoserver-dev-tf-appserviceplan",
                Location = loc,
                ResourceGroupName = rg.Name,
                Sku = new AppServicePlanSku()
                {
                    Tier = "Basic",
                    Size = "B3"
                },
                Reserved = true,
                Kind = "linux"
            });

            

            

            var geoserverDataStorage = new StorageAccount(this,"geoserver-dev-storage",new StorageAccountConfig(){
                Name="geoserverdeva010182",
                AccountKind="StorageV2",
                AccountReplicationType="LRS",
                AccountTier="Standard",
                Location=loc,
                ResourceGroupName=rg.Name,
                

            });

            var geoserverDataStorageShare = new StorageShare(this,"geoserver-dev-share",new StorageShareConfig(){
                Name = geoserverDataStorage.Name+"-share",

                StorageAccountName = geoserverDataStorage.Name,
                Quota = 1
            });

            new LinuxWebApp(this,"geoserver-dev-tf-appservice-linux",new LinuxWebAppConfig(){
                Name = "geoserver-dev-tf-appservice",
                ResourceGroupName = rg.Name,
                ServicePlanId = asp.Id,
                Location = loc,
                AppSettings = new Dictionary<string,string>(){
                    {"WEBSITES_PORT","8080"},
                    {"WEBSITES_ENABLE_APP_SERVICE_STORAGE","true"},
                    {"GEOSERVER_DATA_DIR","/var/geoserver/datadir"},
                    {"SAMPLE_DATA","true"}
                },
                
                SiteConfig = new LinuxWebAppSiteConfig(){
                    ApplicationStack = new LinuxWebAppSiteConfigApplicationStack(){
                        DockerImage = "kartoza/geoserver",
                        DockerImageTag = "2.21.0",

                    }
                    
                },
                StorageAccount=new LinuxWebAppStorageAccount[]{new LinuxWebAppStorageAccount(){
                    AccessKey= geoserverDataStorage.PrimaryAccessKey,
                    AccountName=geoserverDataStorage.Name,
                    MountPath="/var/geoserver/datadir",
                    ShareName = geoserverDataStorageShare.Name,
                    Type = "AzureFiles",
                    Name = geoserverDataStorageShare.Name
                    
                }}
            });
        }
    }
}