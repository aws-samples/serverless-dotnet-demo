data "archive_file" "lambda_archive" {
  type = "zip"

  source_dir  = var.publish_dir
  output_path = "${path.module}/../../${var.zip_file}"
}

resource "aws_s3_object" "lambda_bundle" {
  bucket = var.lambda_bucket_id

  key    = var.zip_file
  source = data.archive_file.lambda_archive.output_path

  etag = filemd5(data.archive_file.lambda_archive.output_path)
}

resource "aws_lambda_function" "function" {
  function_name = var.function_name
  s3_bucket = var.lambda_bucket_id
  s3_key    = aws_s3_object.lambda_bundle.key
  runtime = "dotnet6"
  handler = var.lambda_handler
  source_code_hash = data.archive_file.lambda_archive.output_base64sha256
  role = aws_iam_role.lambda_function_role.arn
  timeout = 30
  tracing_config {
    mode="Active"
  }
  dynamic "environment" {
    for_each = length(var.environment_variables) > 0 ? [1] : []
    content {
      variables = merge(var.environment_variables, {
        POWERTOOLS_METRICS_NAMESPACE = "PowerToolsSample"
        POWERTOOLS_SERVICE_NAME = "monitored-service"
        POWERTOOLS_LOG_LEVEL = "Debug"
        POWERTOOLS_LOGGER_LOG_EVENT = true
        POWERTOOLS_LOGGER_CASE = "SnakeCase"
    })
    }
  }
}

resource "aws_cloudwatch_log_group" "aggregator" {
  name = "/aws/lambda/${aws_lambda_function.function.function_name}"

  retention_in_days = 30
}

resource "aws_iam_role" "lambda_function_role" {
  name = "DeployFunctionIamRole_${var.function_name}"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Sid    = ""
      Principal = {
        Service = "lambda.amazonaws.com"
      }
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_policy_attach" {
  role       = aws_iam_role.lambda_function_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}