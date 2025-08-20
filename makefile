env-local = ASPNETCORE_ENVIRONMENT=Local
env-dev = ASPNETCORE_ENVIRONMENT=Development
env-prev = ASPNETCORE_ENVIRONMENT=Preview

install-tools:
	dotnet tool install -g dotnet-ef && dotnet tool install -g dotnet-script
install-app:
	npm add -g concurrently
test-local: services-ready
	$(env-local) dotnet test
test-dev: services-ready
	$(env-dev) dotnet test
test-prev:
	$(env-prev) make migrations-up && $(env-prev) dotnet test
build-mcp:
	dotnet publish Mcp/Mcp.csproj -c Release -o Mcp/bin/Release/publish
run-api:
	$(env-local) dotnet watch
run-ngrok:
	ngrok http --url=parrot-fun-nicely.ngrok-free.app --region us 8080
run-local: services-ready
	concurrently -n dotnet,ngrok -c red,blue -k "make run-api" "make run-ngrok"
run-local-mcp: services-ready build-mcp
	concurrently -n dotnet,ngrok -c red,blue -k "make run-api" "make run-ngrok"
services-up:
	docker compose -f Infra/compose.yaml up -d
services-down:
	docker compose -f Infra/compose.yaml down
services-ready:
	make services-up && dotnet script ./Infra/Scripts/WaitForPostgres.csx && make migrations-up
name ?=
migrations-create:
	@[ -n "$(name)" ] || (echo "Usage: make migrations-create name=<Name>" && exit 1)
	dotnet ef migrations add $(name) --output-dir Infra/Migrations
migrations-up:
	dotnet ef database update
migrations-down:
	dotnet ef database update 0
