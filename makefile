env-local = ASPNETCORE_ENVIRONMENT=Local
env-dev = ASPNETCORE_ENVIRONMENT=Development
env-prev = ASPNETCORE_ENVIRONMENT=Preview
env-prod = ASPNETCORE_ENVIRONMENT=Production

install-tools:
	dotnet tool install -g dotnet-ef
	dotnet tool install -g dotnet-script
install-app:
	npm add -g concurrently
test-local: services-ready
	$(env-local) dotnet test
test-dev: services-ready
	$(env-dev) dotnet test
test-prev:
	make migrations-up env=Preview && $(env-prev) dotnet test
build:
	dotnet restore .
	dotnet publish TheChatbot.csproj -c Release -o out
	dotnet publish Mcp/Mcp.csproj -c Release -o Mcp/out
run-api:
	$(env-local) dotnet watch
run-ngrok:
	ngrok http --url=parrot-fun-nicely.ngrok-free.app --region us 8080
run-local: services-ready
	concurrently -n dotnet,ngrok -c red,blue -k "make run-api" "make run-ngrok"
run-prod:
	$(env-prod) dotnet TheChatbot.dll
services-up:
	docker compose -f Infra/compose.yaml up -d
services-down:
	docker compose -f Infra/compose.yaml down
services-ready:
	make services-up
	dotnet script ./Infra/Scripts/WaitForPostgres.csx
	make migrations-up
name ?=
migrations-create:
	@[ -n "$(name)" ] || (echo "Usage: make migrations-create name=<Name>" && exit 1)
	dotnet ef migrations add $(name) --output-dir Infra/Migrations
env ?= Local
migrations-up:
	ASPNETCORE_ENVIRONMENT=$(env) dotnet ef database update
migrations-down:
	ASPNETCORE_ENVIRONMENT=$(env) dotnet ef database update 0
docker-up:
	docker build -f Infra/Dockerfile -t the-chatbot .
	docker run -d --name the-chatbot -p 8080:8080 the-chatbot
docker-down:
	docker stop the-chatbot || true
	docker rm -v the-chatbot || true
	docker image rm the-chatbot || true
