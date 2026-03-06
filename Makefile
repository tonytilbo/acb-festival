.PHONY: ui api

ui:
	cd src/ui && npm run dev

api:
	cd src/api && dotnet run --launch-profile http
