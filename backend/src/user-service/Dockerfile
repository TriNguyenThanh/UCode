# Development stage với dotnet watch
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app/UserService

# Copy csproj files và restore dependencies
COPY ["Api/UserService.Api.csproj", "Api/"]
COPY ["Application/UserService.Application.csproj", "Application/"]
COPY ["Domain/UserService.Domain.csproj", "Domain/"]
COPY ["Infrastructure/UserService.Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "Api/UserService.Api.csproj"

# Copy toàn bộ source code
COPY . .

# Set working directory to API project
WORKDIR /app/UserService/Api

# Expose ports
EXPOSE 5001

# Install dotnet-ef tool for migrations
RUN dotnet tool install --global dotnet-ef --version 8.0.0
ENV PATH="${PATH}:/root/.dotnet/tools"

# Run with dotnet watch
ENTRYPOINT ["dotnet", "run", "--no-restore", "--urls", "http://0.0.0.0:5001"]
