# Create a set of IAM policies our application will need
resource "aws_iam_policy" "dynamo_db_read" {
  name   = "deploy_dynamo_db_read_policy"
  path   = "/"
  policy = data.aws_iam_policy_document.dynamo_db_read.json
}

resource "aws_iam_policy" "dynamo_db_write" {
  name   = "deploy_dynamo_db_write_policy"
  path   = "/"
  policy = data.aws_iam_policy_document.dynamo_db_write.json
}

resource "aws_iam_policy" "dynamo_db_crud" {
  name   = "deploy_dynamo_db_crud_policy"
  path   = "/"
  policy = data.aws_iam_policy_document.dynamo_db_crud.json
}

resource "aws_iam_policy" "cloud_watch_put_metrics" {
  name   = "deploy_cloud_watch_put_metrics_policy"
  path   = "/"
  policy = data.aws_iam_policy_document.cloud_watch_put_metrics.json
}

resource "aws_iam_policy" "dynamo_db_stream_read_policy" {
  name   = "deploy_dynamo_db_stream_read_policy"
  path   = "/"
  policy = data.aws_iam_policy_document.allow_dynamo_db_streams.json
}

resource "aws_iam_policy" "sns_publish_message" {
  name   = "deploy_sns_publish_message_policy"
  path   = "/"
  policy = data.aws_iam_policy_document.sns_publish_policy.json
}

resource "aws_iam_policy" "event_bridge_put_events" {
  name   = "deploy_event_bridge_put_events_policy"
  path   = "/"
  policy = data.aws_iam_policy_document.event_bridge_put_events.json
}

resource "aws_iam_policy" "ssm_parameter_read" {
  name   = "deploy_ssm_parameter_read_policy"
  path   = "/"
  policy = data.aws_iam_policy_document.ssm_parameter_read.json
}

resource "aws_iam_policy" "allow_x_ray" {
  name   = "deploy_allow_x_ray_policy"
  path   = "/"
  policy = data.aws_iam_policy_document.x_ray_policy.json
}