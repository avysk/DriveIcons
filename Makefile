test:
	dotnet test --collect:"XPlat Code Coverage"^C
build:
	dotnet build
clean:
	dotnet clean
cli:
	dotnet run --project Fi.Pentode.Registry.CLI/Fi.Pentode.Registry.CLI.fsproj
