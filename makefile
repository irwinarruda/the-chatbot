env-dev = ASPNETCORE_ENVIRONMENT=Development
env-prod = ASPNETCORE_ENVIRONMENT=Production

install-tools:
	dotnet tool install -g dotnet-ef && dotnet tool install -g dotnet-script
test-dev: services-ready
	$(env-dev) dotnet test
test-prod:
	$(env-prod) dotnet test
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
