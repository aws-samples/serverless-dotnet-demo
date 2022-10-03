variable "publish_dir" {
  description = "The location of the published files."
  type        = string
}

variable "zip_file" {
  description = "The location of the ZIP file"
  type        = string
}

variable "lambda_bucket_id" {
  description = "The id of the bucket lambda function code will be stored"
  type        = string
}

variable "function_name" {
  description = "The name of the Lambda function"
  type        = string
}

variable "lambda_handler" {
  description = "The Lambda handler, defined as classlib::namespace.class::method"
  type        = string
}

variable "environment_variables" {
  description = "Environment variables to pass to the Lambda function"
  type = map(string)
}