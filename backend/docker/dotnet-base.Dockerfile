# Base image chung cho tất cả .NET services
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install common tools
RUN dotnet nuget locals all --clear && \
    dotnet tool install --global dotnet-ef --version 8.0.11

ENV PATH="${PATH}:/root/.dotnet/tools"

WORKDIR /app
