env-dev = ASPNETCORE_ENVIRONMENT=Development
env-preview = ASPNETCORE_ENVIRONMENT=Preview

test-dev:
	$(env-dev) dotnet test
test-preview:
	$(env-preview) dotnet test
test-dev-watch:
	$(env-dev) dotnet watch test
test-preview-watch:
	$(env-preview) dotnet watch test
