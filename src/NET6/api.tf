resource "aws_dynamodb_table" "synchornous_api_table" {
  name             = "${var.prefix}-${var.table_name}"
  billing_mode     = "PAY_PER_REQUEST"
  hash_key         = "PK"
  stream_enabled   = true
  stream_view_type = "NEW_AND_OLD_IMAGES"

  attribute {
    name = "PK"
    type = "S"
  }
}

# Create S3 bucket to store our application source code.
resource "aws_s3_bucket" "lambda_bucket" {
  bucket = var.code_bucket_name

  acl           = "private"
  force_destroy = true
}

# Initialize module containing IAM policies.
module "iam_policies" {
  source      = "./tf-modules/iam-policies"
  table_name  = aws_dynamodb_table.synchornous_api_table.name
  topic_name  = "*"
  environment = var.environment
  prefix = var.prefix
}

# Create Product Lambda
module "put_product_lambda" {
  source           = "./tf-modules/lambda-function"
  lambda_bucket_id = aws_s3_bucket.lambda_bucket.id
  publish_dir      = "${path.module}/PutProduct/bin/Release/net6.0/linux-x64/publish"
  zip_file         = "PutProduct.zip"
  function_name    = "${var.prefix}-MonitoredPutProduct"
  lambda_handler   = "PutProduct::PutProduct.Function::FunctionHandler"
  environment_variables = {
    "PRODUCT_TABLE_NAME"           = aws_dynamodb_table.synchornous_api_table.name
  }
}

module "put_product_lambda_api" {
  source        = "./tf-modules/api-gateway-lambda-integration"
  api_id        = module.api_gateway.api_id
  api_arn       = module.api_gateway.api_arn
  function_arn  = module.put_product_lambda.function_arn
  function_name = module.put_product_lambda.function_name
  http_method   = "POST"
  route         = "/"
}

resource "aws_iam_role_policy_attachment" "put_product_lambda_dynamo_db_write" {
  role       = module.put_product_lambda.function_role_name
  policy_arn = module.iam_policies.dynamo_db_write
}

resource "aws_iam_role_policy_attachment" "put_product_lambda_cw_metrics" {
  role       = module.put_product_lambda.function_role_name
  policy_arn = module.iam_policies.cloud_watch_put_metrics
}

resource "aws_iam_role_policy_attachment" "put_product_x_ray_attach" {
  role       = module.put_product_lambda.function_role_name
  policy_arn = module.iam_policies.allow_x_ray_policy
}

resource "aws_iam_role_policy_attachment" "put_product_lambda_sns_publish" {
  role       = module.put_product_lambda.function_role_name
  policy_arn = module.iam_policies.sns_publish_message
}

# Get Product Lambda
module "get_product_lambda" {
  source           = "./tf-modules/lambda-function"
  lambda_bucket_id = aws_s3_bucket.lambda_bucket.id
  publish_dir      = "${path.module}/GetProduct/bin/Release/net6.0/linux-x64/publish"
  zip_file         = "GetProduct.zip"
  function_name    = "${var.prefix}-MonitoredGetProduct"
  lambda_handler   = "GetProduct::GetProduct.Function::FunctionHandler"
  environment_variables = {
    "PRODUCT_TABLE_NAME"           = aws_dynamodb_table.synchornous_api_table.name
  }
}

module "get_product_lambda_api" {
  source        = "./tf-modules/api-gateway-lambda-integration"
  api_id        = module.api_gateway.api_id
  api_arn       = module.api_gateway.api_arn
  function_arn  = module.get_product_lambda.function_arn
  function_name = module.get_product_lambda.function_name
  http_method   = "GET"
  route         = "/{id}"
}

resource "aws_iam_role_policy_attachment" "put_product_lambda_dynamo_db_read" {
  role       = module.get_product_lambda.function_role_name
  policy_arn = module.iam_policies.dynamo_db_read
}

resource "aws_iam_role_policy_attachment" "get_product_lambda_cw_metrics" {
  role       = module.get_product_lambda.function_role_name
  policy_arn = module.iam_policies.cloud_watch_put_metrics
}

resource "aws_iam_role_policy_attachment" "get_product_x_ray_attach" {
  role       = module.get_product_lambda.function_role_name
  policy_arn = module.iam_policies.allow_x_ray_policy
}

# Get Products Lambda
module "get_products_lambda" {
  source           = "./tf-modules/lambda-function"
  lambda_bucket_id = aws_s3_bucket.lambda_bucket.id
  publish_dir      = "${path.module}/GetProducts/bin/Release/net6.0/linux-x64/publish"
  zip_file         = "GetProducts.zip"
  function_name    = "${var.prefix}-MonitoredGetProducts"
  lambda_handler   = "GetProducts::GetProducts.Function::TracingFunctionHandler"
  environment_variables = {
    "PRODUCT_TABLE_NAME"           = aws_dynamodb_table.synchornous_api_table.name
    "POWERTOOLS_SERVICE_NAME"      = "product-api"
    "POWERTOOLS_METRICS_NAMESPACE" = "product-api"
  }
}

module "get_products_lambda_api" {
  source        = "./tf-modules/api-gateway-lambda-integration"
  api_id        = module.api_gateway.api_id
  api_arn       = module.api_gateway.api_arn
  function_arn  = module.get_products_lambda.function_arn
  function_name = module.get_products_lambda.function_name
  http_method   = "GET"
  route         = "/"
}

resource "aws_iam_role_policy_attachment" "get_products_lambda_dynamo_db_read" {
  role       = module.get_products_lambda.function_role_name
  policy_arn = module.iam_policies.dynamo_db_crud
}

resource "aws_iam_role_policy_attachment" "get_products_lambda_cw_metrics" {
  role       = module.get_products_lambda.function_role_name
  policy_arn = module.iam_policies.cloud_watch_put_metrics
}

resource "aws_iam_role_policy_attachment" "get_products_x_ray_attach" {
  role       = module.get_products_lambda.function_role_name
  policy_arn = module.iam_policies.allow_x_ray_policy
}

# Delete Product Lambda
module "delete_product_lambda" {
  source           = "./tf-modules/lambda-function"
  lambda_bucket_id = aws_s3_bucket.lambda_bucket.id
  publish_dir      = "${path.module}/DeleteProduct/bin/Release/net6.0/linux-x64/publish"
  zip_file         = "DeleteProduct.zip"
  function_name    = "${var.prefix}-MonitoredDeleteProduct"
  lambda_handler   = "DeleteProduct::DeleteProduct.Function::FunctionHandler"
  environment_variables = {
    "PRODUCT_TABLE_NAME"           = aws_dynamodb_table.synchornous_api_table.name
  }
}

module "delete_product_lambda_api" {
  source        = "./tf-modules/api-gateway-lambda-integration"
  api_id        = module.api_gateway.api_id
  api_arn       = module.api_gateway.api_arn
  function_arn  = module.delete_product_lambda.function_arn
  function_name = module.delete_product_lambda.function_name
  http_method   = "DELETE"
  route         = "/{id}"
}

resource "aws_iam_role_policy_attachment" "delete_product_lambda_dynamo_db_read" {
  role       = module.delete_product_lambda.function_role_name
  policy_arn = module.iam_policies.dynamo_db_crud
}

resource "aws_iam_role_policy_attachment" "delete_product_lambda_cw_metrics" {
  role       = module.delete_product_lambda.function_role_name
  policy_arn = module.iam_policies.cloud_watch_put_metrics
}

resource "aws_iam_role_policy_attachment" "delete_product_x_ray_attach" {
  role       = module.delete_product_lambda.function_role_name
  policy_arn = module.iam_policies.allow_x_ray_policy
}

module "api_gateway" {
  source            = "./tf-modules/api-gateway"
  api_name          = "${var.prefix}-monitored-api"
  stage_name        = "dev"
  stage_auto_deploy = true
}

output "api_endpoint" {
  value = module.api_gateway.api_endpoint
}

