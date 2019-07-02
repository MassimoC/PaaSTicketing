## Visual studio commands
```csharp
docker build -f "C:\GIT\PaaSTicketing\src\api\PaaS.Ticketing.Api\Dockerfile" -t paasticketingapi:dev --target base  --label "com.microsoft.created-by=visual-studio" --label "com.microsoft.visual-studio.project-name=PaaS.Ticketing.Api" "C:\GIT\PaaSTicketing\src\api" 
```
```csharp
docker run -dt -v "C:\Users\massi\vsdbg\vs2017u5:/remote_debugger:rw" -v "C:\GIT\PaaSTicketing\src\api\PaaS.Ticketing.Api:/app" -v "C:\Users\massi\AppData\Roaming\Microsoft\UserSecrets:/root/.microsoft/usersecrets:ro" -v "C:\Users\massi\AppData\Roaming\ASP.NET\Https:/root/.aspnet/https:ro" -v "C:\Users\massi\.nuget\packages\:/root/.nuget/fallbackpackages2" -v "C:\Program Files\dotnet\sdk\NuGetFallbackFolder:/root/.nuget/fallbackpackages" -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e "ASPNETCORE_ENVIRONMENT=Development" -e "NUGET_PACKAGES=/root/.nuget/fallbackpackages2" -e "NUGET_FALLBACK_PACKAGES=/root/.nuget/fallbackpackages;/root/.nuget/fallbackpackages2" -p 50875:80 -p 44327:443 --entrypoint tail paasticketingapi:dev -f /dev/null 
```

docker build . -t ticketingapi:0.1.8
docker run ticketingapi:0.1.8

docker ps
docker ps --all
docker stop 

## stop all containers
docker stop $(docker ps -aq)

## remove all containers
docker rm $(docker ps -aq)

## remove all images
docker rmi $(docker images -q)

## Azure WebApp for containers
az login
az acr create --resource-group data-paas-rg --name thecompany --sku basic 
az acr credential show --name thecompany
docker login thecompany.azurecr.io --username thecompany

docker build . -t ticketingapi:0.4.0
docker images
docker tag df3458953022 thecompany.azurecr.io/ticketingapi:0.4
docker push thecompany.azurecr.io/ticketingapi:0.4
az acr repository list --name thecompany

az webapp config appsettings set --resource-group data-paas-rg --name myticketing --settings WEBSITES_PORT=80

docker run -d -p 19096:80 --name myticketing_1 
	-e WEBSITES_ENABLE_APP_SERVICE_STORAGE=false 
	-e WEBSITES_PORT=80 -e WEBSITE_SITE_NAME=myticketing 
	-e WEBSITE_AUTH_ENABLED=False 
	-e WEBSITE_ROLE_INSTANCE_ID=0 -
	e WEBSITE_HOSTNAME=myticketing.azurewebsites.net 
	-e WEBSITE_INSTANCE_ID=1376bf83c4d3971b801d48eadd52635c5490d97856a2aa1c9584202d2f25aabf thecompany.azurecr.io/ticketingapi:0.4  