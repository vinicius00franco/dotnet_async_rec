# Flight Service Application

Este repositório contém dois projetos principais:

## FlightServiceClientApp
- **Descrição**: Um cliente que consome a API de serviços de voo.
- **Funcionalidades**:
  - Consulta de voos disponíveis.
  - Simulação de compra de passagens.

## FlightServiceApi
- **Descrição**: Uma API que fornece dados e funcionalidades relacionadas a voos.
- **Funcionalidades**:
  - Listagem de voos disponíveis.
  - Consulta de detalhes de voos.
  - Simulação de compra de passagens.

## Como executar
1. Certifique-se de ter o .NET 9 instalado.
2. Construa os projetos:
   ```bash
   dotnet build FlightServiceClientApp/FlightServiceClientApp.csproj
   dotnet build FlightServiceApi/FlightServiceApi.csproj
   ```
3. Execute a API:
   ```bash
   dotnet run --project FlightServiceApi/FlightServiceApi.csproj
   ```
4. Execute o cliente:
   ```bash
   dotnet run --project FlightServiceClientApp/FlightServiceClientApp.csproj
   ```