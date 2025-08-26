FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App
# Install make (required for invoking the Makefile inside the container)
RUN apt-get update && apt-get install -y make && rm -rf /var/lib/apt/lists/*
# Copy everything
COPY . ./
# Restore, build and publish a release
RUN make build

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /App
# Install make to allow using the existing Makefile as entrypoint
RUN apt-get update && apt-get install -y make && rm -rf /var/lib/apt/lists/*
COPY --from=build /App/out .
COPY --from=build /App/Mcp/out /Mcp
COPY --from=build /App/makefile .
ENTRYPOINT ["make", "run-prod"]
