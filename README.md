![Imagem](./docs/Unifeso.png)

````
https://learn.microsoft.com/en-us/azure/cosmos-db/emulator
````

````
docker run \
--publish 8081:8081 \
--publish 10250-10255:10250-10255 \
--name canal-deploy-cosmos-emulator \
--detach \
mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest
````

````
https://localhost:8081/_explorer/index.html
````
Calculadora

````
https://cosmos.azure.com/capacitycalculator/
````
