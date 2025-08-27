FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /App
# Enable dotnet tools
ENV PATH="${PATH}:/root/.dotnet/tools"
# Install make
RUN apt-get update && apt-get install -y make && rm -rf /var/lib/apt/lists/*
# Copy everything
COPY . ./
# Install dependencies
RUN make install-tools
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
COPY --from=build /App/Infra/Scripts/DockerRun.sh /App/DockerRun.sh
RUN chmod +x /App/DockerRun.sh
EXPOSE 8080
ENTRYPOINT ["/App/DockerRun.sh"]
