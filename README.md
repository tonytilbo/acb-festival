# ACB Beer Festival

A web app for the Anglian Craft Brewers beer festival. Browse beers, rate them 1–10, and track what you've tried.

**Stack:** Vue 3 + TypeScript frontend · .NET 10 Minimal API backend · Azure Table Storage

---

## Running locally

### Prerequisites

- [Node.js 22+](https://nodejs.org)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (local Azure Storage emulator) — `npm install -g azurite`

### Start everything

```bash
# Terminal 1 — local storage emulator
azurite --silent

# Terminal 2 — API
make api

# Terminal 3 — frontend
make ui
```

The app will be available at `http://localhost:5173`.

The API falls back to `UseDevelopmentStorage=true` (Azurite) when no `AZURE_STORAGE_ACCOUNT_NAME` environment variable is set.

---

## Deploying to Azure

### Architecture

```
User → Azure Static Web Apps (festival.angliancraftbrewers.org.uk)
                    ↓ /api/*
           Azure Container Apps → .NET API → Azure Table Storage
```

### Prerequisites

- [Terraform 1.6+](https://developer.hashicorp.com/terraform/install)
- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) — `az login` before running Terraform
- [Docker](https://www.docker.com) — for building the API image
- An Azure subscription

### 1. Provision infrastructure

```bash
cp infra/terraform.tfvars.example infra/terraform.tfvars
```

Edit `infra/terraform.tfvars` and fill in your subscription ID:

```hcl
subscription_id = "00000000-0000-0000-0000-000000000000"
```

Then apply:

```bash
cd infra
terraform init
terraform apply
```

Note the outputs — you'll need them in the next steps:

```bash
terraform output static_web_app_api_key        # sensitive — use -raw flag
terraform output container_registry_login_server
terraform output custom_domain_validation_token
terraform output container_app_url
```

### 2. Configure DNS

Add a CNAME record at your DNS provider:

| Type | Name | Value |
|------|------|-------|
| CNAME | `festival` | value from `terraform output custom_domain_validation_token` |

### 3. Create a GitHub Actions service principal

This gives GitHub Actions permission to push images to ACR and update the Container App:

```bash
az ad sp create-for-rbac \
  --name "acb-festival-github" \
  --role contributor \
  --scopes /subscriptions/<YOUR_SUBSCRIPTION_ID>/resourceGroups/acb-festival \
  --sdk-auth
```

Copy the full JSON output — you'll need it in the next step.

### 4. Add GitHub Actions secrets

In your GitHub repository go to **Settings → Secrets and variables → Actions** and add:

| Secret | Value |
|--------|-------|
| `AZURE_STATIC_WEB_APP_API_TOKEN` | `terraform output -raw static_web_app_api_key` |
| `ACR_LOGIN_SERVER` | `terraform output -raw container_registry_login_server` |
| `AZURE_CREDENTIALS` | Full JSON from the `az ad sp create-for-rbac` command above |

### 5. Push to main

Once secrets are in place, push to `main`. GitHub Actions will:

- **deploy-ui.yml** — build the Vue app and deploy to Static Web Apps
- **deploy-api.yml** — build the Docker image, push to ACR, and update the Container App

### 6. Link the API to Static Web Apps

After the first successful deploy, link the Container App as the backend so `/api/*` routes are proxied automatically:

```bash
az staticwebapp backends link \
  --name acb-festival-ui \
  --resource-group acb-festival \
  --backend-resource-id $(az containerapp show \
      --name acb-festival-api \
      --resource-group acb-festival \
      --query id -o tsv) \
  --backend-region uksouth
```

---

## Project structure

```
.
├── infra/                        # Terraform infrastructure
│   ├── main.tf
│   ├── variables.tf
│   ├── outputs.tf
│   └── terraform.tfvars.example
├── src/
│   ├── api/                      # .NET 10 Minimal API
│   │   ├── Program.cs
│   │   ├── Dockerfile
│   │   └── Api.csproj
│   └── ui/                       # Vue 3 frontend
│       ├── src/
│       ├── staticwebapp.config.json
│       └── package.json
├── .github/
│   └── workflows/
│       ├── deploy-api.yml
│       └── deploy-ui.yml
└── Makefile
```
