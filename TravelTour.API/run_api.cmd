@echo off
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=https://localhost:7032;http://localhost:5011
cd /d "c:\Visual Studio\BTL-Travel-Tour-Backend\TravelTour.API"
".\bin\Debug\net10.0\TravelTour.API.exe"
