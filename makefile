env-dev = ASPNETCORE_ENVIRONMENT=Development
env-preview = ASPNETCORE_ENVIRONMENT=Preview

test-dev:
	$(env-dev) dotnet test
test-preview:
	$(env-preview) dotnet test
services-up:
	docker compose -f Infra/compose.yaml up -d
services-down:
	docker compose -f Infra/compose.yaml down
name ?=
migrations-create:
	@[ -n "$(name)" ] || (echo "Usage: make migrations-create name=<Name>" && exit 1)
	dotnet ef migrations add $(name) --output-dir Infra/Migrations
migrations-remove:
	dotnet ef migrations remove

