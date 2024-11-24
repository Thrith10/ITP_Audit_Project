# Stage 1: Build the application
# Use the .NET 7.0 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy the solution file and restore dependencies
COPY *.sln ./
COPY PKFAuditManagement/*.csproj ./PKFAuditManagement/
COPY PKFAuditManagement.UnitTests/*.csproj ./PKFAuditManagement.UnitTests/
RUN dotnet restore

# Copy the rest of the source code to the container
COPY . .

# Publish the application to the "out" directory in Release mode
RUN dotnet publish -c Release -o out

# Stage 2: Runtime image
# Use the .NET 7.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/out ./

# Expose port 80 to allow traffic to the application
EXPOSE 80

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "PKFAuditManagement.dll"]
