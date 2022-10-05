variable "table_name" {
  description = "The name of the DyanamoDB table"
  type        = string
  default = "MonitoredApi"
}

variable "code_bucket_name" {
  description = "The name of the S3 bucket to store Lambda source code"
  type        = string
  default = "observed-api-source-code-bucket"
}

variable "environment" {
  description = "The current environment"
  type        = string
  default = "dev"
}

variable "prefix" {
  description = "Resource prefix"
  type        = string
}