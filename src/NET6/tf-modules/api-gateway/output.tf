output "api_id" {
  value       =  aws_apigatewayv2_api.lambda.id
} 

output "api_arn" {
  value       =  aws_apigatewayv2_api.lambda.execution_arn
}