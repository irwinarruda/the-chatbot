env-dev = ASPNETCORE_ENVIRONMENT=Development
env-prod = ASPNETCORE_ENVIRONMENT=Production

test-dev: services-ready
	$(env-dev) dotnet test
test-prod: services-ready
	$(env-prod) dotnet test
services-up:
	docker compose -f Infra/compose.yaml up -d
services-down:
	docker compose -f Infra/compose.yaml down
services-ready: services-up
	dotnet script ./Infra/Scripts/WaitForPostgres.csx
name ?=
migrations-create:
	@[ -n "$(name)" ] || (echo "Usage: make migrations-create name=<Name>" && exit 1)
	dotnet ef migrations add $(name) --output-dir Infra/Migrations
migrations-remove:
	dotnet ef migrations remove
