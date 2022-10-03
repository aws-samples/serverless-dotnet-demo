variable "api_name" {
  description = "The name of the HTTP API to create."
  type        = string
}

variable "stage_name" {
    description = "The name of the API stage to create."
    type        = string
}

variable "stage_auto_deploy" {
    description = "Should the API stage auto deploy."
    type        = bool
}