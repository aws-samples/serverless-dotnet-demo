data "aws_caller_identity" "current" {}
data "aws_region" "current" {}

data "aws_iam_policy_document" "dynamo_db_read" {
  statement {
    actions   = ["dynamodb:GetItem", "dynamodb:Scan", "dynamodb:Query", "dynamodb:BatchGetItem", "dynamodb:DescribeTable"]
    resources = ["arn:aws:dynamodb:*:${data.aws_caller_identity.current.account_id}:table/${var.table_name}", "arn:aws:dynamodb:*:*:table/${var.table_name}/*"]
  }
}

data "aws_iam_policy_document" "dynamo_db_write" {
  statement {
    actions = ["dynamodb:PutItem",
      "dynamodb:UpdateItem",
    "dynamodb:BatchWriteItem"]
    resources = ["arn:aws:dynamodb:*:${data.aws_caller_identity.current.account_id}:table/${var.table_name}", "arn:aws:dynamodb:*:${data.aws_caller_identity.current.account_id}:table/${var.table_name}/*"]
  }
}

data "aws_iam_policy_document" "dynamo_db_crud" {
  statement {
    actions = ["dynamodb:GetItem",
      "dynamodb:DeleteItem",
      "dynamodb:PutItem",
      "dynamodb:Scan",
      "dynamodb:Query",
      "dynamodb:UpdateItem",
      "dynamodb:BatchWriteItem",
      "dynamodb:BatchGetItem",
      "dynamodb:DescribeTable",
    "dynamodb:ConditionCheckItem"]
    resources = ["arn:aws:dynamodb:*:${data.aws_caller_identity.current.account_id}:table/${var.table_name}", "arn:aws:dynamodb:*:${data.aws_caller_identity.current.account_id}:table/${var.table_name}/*"]
  }
}

data "aws_iam_policy_document" "cloud_watch_put_metrics" {
  statement {
    actions   = ["cloudwatch:PutMetricData"]
    resources = ["*"]
  }
}

data "aws_iam_policy_document" "allow_dynamo_db_streams" {
  statement {
    actions = ["dynamodb:GetShardIterator",
      "dynamodb:DescribeStream",
      "dynamodb:GetRecords",
      "dynamodb:ListStreams",
    "dynamodb:ListStreams"]
    resources = [
      "arn:aws:dynamodb:*:${data.aws_caller_identity.current.account_id}:table/${var.table_name}",
      "arn:aws:dynamodb:*:${data.aws_caller_identity.current.account_id}:table/${var.table_name}/*"
    ]
  }
}

data "aws_iam_policy_document" "sns_publish_policy" {
  statement {
    actions   = ["sns:publish"]
    resources = ["arn:aws:sns:*:${data.aws_caller_identity.current.account_id}:${var.topic_name}"]
  }
}

data "aws_iam_policy_document" "event_bridge_put_events" {
  statement {
    actions   = ["events:PutEvents"]
    resources = ["arn:aws:events:*:${data.aws_caller_identity.current.account_id}:event-bus/${var.environment}-application-integration-patterns-samples"]
  }
}

data "aws_iam_policy_document" "ssm_parameter_read" {
  statement {
    actions   = ["ssm:DescribeParameters"]
    resources = ["*"]
  }
  statement {
    actions = ["ssm:GetParameters",
      "ssm:GetParameter",
    "ssm:GetParametersByPath"]
    resources = ["arn:aws:ssm:*:${data.aws_caller_identity.current.account_id}:parameter/*"]
  }
}

data "aws_iam_policy_document" "x_ray_policy" {
  statement {
    actions = ["xray:PutTraceSegments",
      "xray:PutTelemetryRecords",
      "xray:GetSamplingRules",
      "xray:GetSamplingTargets",
    "xray:GetSamplingStatisticSummaries"]
    resources = ["*"]
  }
}
