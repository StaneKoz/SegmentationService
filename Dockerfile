FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet tool install --global dotnet-ef  
RUN dotnet publish "SegmentationService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /root/.dotnet/tools /root/.dotnet/tools  
ENV PATH="/root/.dotnet/tools:${PATH}"  
ENTRYPOINT ["dotnet", "SegmentationService.dll"]