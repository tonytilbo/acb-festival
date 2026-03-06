variable "subscription_id" {
  description = "Azure subscription ID"
  type        = string
}

variable "location" {
  description = "Azure region"
  type        = string
  default     = "uksouth"
}

variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
  default     = "acb-festival"
}

variable "app_name" {
  description = "Base name used for all resources"
  type        = string
  default     = "acb-festival"
}

variable "custom_domain" {
  description = "Custom domain for the Static Web App"
  type        = string
  default     = "festival.angliancraftbrewers.org.uk"
}

variable "api_image_tag" {
  description = "Container image tag to deploy to Container Apps"
  type        = string
  default     = "latest"
}
