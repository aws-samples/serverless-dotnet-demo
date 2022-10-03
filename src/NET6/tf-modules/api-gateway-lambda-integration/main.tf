resource "aws_apigatewayv2_integration" "lambda_integration" {
  api_id = var.api_id

  integration_uri    = var.function_arn
  integration_type   = "AWS_PROXY"
  integration_method = var.http_method
}

resource "aws_apigatewayv2_route" "get_product_route" {
  api_id = var.api_id

  route_key = "${var.http_method} ${var.route}"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_integration.id}"
}

resource "aws_lambda_permission" "get_lambda_api_gw" {
  statement_id  = "AllowLambdaExecutionFromAPIGateway_${var.function_name}"
  action        = "lambda:InvokeFunction"
  function_name = var.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${var.api_arn}/*/*"
}