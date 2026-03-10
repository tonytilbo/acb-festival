output "static_web_app_url" {
  description = "Default URL of the Static Web App"
  value       = "https://${azurerm_static_web_app.frontend.default_host_name}"
}

output "static_web_app_api_key" {
  description = "Deployment API key for the Static Web App (used in GitHub Actions)"
  value       = azurerm_static_web_app.frontend.api_key
  sensitive   = true
}

output "container_registry_login_server" {
  description = "ACR login server"
  value       = azurerm_container_registry.main.login_server
}

output "container_app_url" {
  description = "URL of the Container App API"
  value       = "https://${azurerm_container_app.api.ingress[0].fqdn}"
}

output "storage_account_name" {
  description = "Storage account name"
  value       = azurerm_storage_account.main.name
}

output "managed_identity_client_id" {
  description = "Client ID of the managed identity used by the Container App"
  value       = azurerm_user_assigned_identity.api.client_id
}

output "custom_domain_cname_target" {
  description = "Point a CNAME record for 'festival' at this value to enable the custom domain"
  value       = azurerm_static_web_app.frontend.default_host_name
}
