# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS builder
WORKDIR /src
COPY ["CryptoPulseApi.csproj", ""]
RUN dotnet restore "CryptoPulseApi.csproj"
COPY . .
RUN dotnet build "CryptoPulseApi.csproj" -c Release -o /app/build
RUN dotnet publish "CryptoPulseApi.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=builder /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
CMD ["dotnet", "CryptoPulseApi.dll"]
