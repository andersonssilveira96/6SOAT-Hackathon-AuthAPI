resource "aws_lambda_function" "fiapx_lambda_auth" {
  function_name = "fiapx-lambda-auth"
  filename      = "../fiapx.Authentication/auth_lambda.zip"
  handler       = "fiapx.Authentication::fiapx.Authentication.Function_LambdaAuth_Generated::LambdaAuth"
  runtime       = "dotnet8"
  role          = var.arn
  tags = {
    Name = "fiapx-lambda"
  }
  timeout     = 30
  memory_size = 512
}

resource "aws_lambda_function" "fiapx_lambda_signup" {
  function_name = "fiapx-lambda-signup"
  filename      = "../fiapx.Authentication/auth_lambda.zip"
  handler       = "fiapx.Authentication::fiapx.Authentication.Function_LambdaSignUP_Generated::LambdaSignUP"
  runtime       = "dotnet8"
  role          = var.arn
  tags = {
    Name = "fiapx-lambda"
  }
  timeout     = 30
  memory_size = 512
}