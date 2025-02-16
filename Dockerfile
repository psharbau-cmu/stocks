FROM microsoft/aspnetcore-build:2.0 AS builder
WORKDIR /source
COPY NNRunner/*.csproj .
RUN dotnet restore
COPY NNRunner/ .
RUN dotnet publish --output /app/ --configuration Release
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=builder app/ .
EXPOSE 80
ENTRYPOINT ["dotnet", "NNRunner.dll"]
