#Arguments:
#$1 - delete stack formation to ensure that all test have same conditon and favour similar ammount of cold start events

STACK_NAME=dotnet8-minimal-api
DELETE_STACK=yes

COLOR='\033[0;33m'
NO_COLOR='\033[0m' # No Color

if [ "x$1" != x ];  
then
  DELETE_STACK=$1
fi

echo "${COLOR}"
echo --------------------------------------------
echo DELETE_STACK: $DELETE_STACK
echo --------------------------------------------
echo "${NO_COLOR}"

if [ $DELETE_STACK == "yes" ];  
then
    echo "${COLOR}"
    echo --------------------------------------------
    echo DELETING STACK $STACK_NAME
    echo --------------------------------------------
    echo "${NO_COLOR}"
    aws cloudformation delete-stack --stack-name $STACK_NAME
    echo "${COLOR}"
    echo ---------------------------------------------
    echo Waiting stack to be deleted
    echo --------------------------------------------
    echo "${NO_COLOR}"
    aws cloudformation wait stack-delete-complete --stack-name $STACK_NAME
    echo "${COLOR}"
    echo ---------------------------------------------
    echo Stack deleted
    echo --------------------------------------------
    echo "${NO_COLOR}"
fi

sam build
sam deploy --stack-name dotnet8-minimal-api --resolve-s3 --s3-prefix dotnet8-minimal-api --no-confirm-changeset --no-fail-on-empty-changeset --capabilities CAPABILITY_IAM