variable "api_id" {
  description = "The ID of the HTTP API to attach to."
  type        = string
}

variable "api_arn" {
  description = "The ARN of the HTTP API to attach to."
  type        = string
}

variable "function_arn" {
  description = "The ARN of the Lambda function."
  type        = string
}

variable "function_name" {
  description = "The name of the Lambda function."
  type        = string
}

variable "http_method" {
  description = "The HTTP method to use (GET, PUT, POST, DELETE)."
  type        = string
}

variable "route" {
  description = "The API route."
  type        = string
}