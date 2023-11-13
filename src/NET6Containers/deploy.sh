#Arguments:
#$1 - ECR URI
#$2 - delete stack formation to ensure that all test have same conditon and favour similar ammount of cold start events

STACK_NAME=dotnet6-container
ECR_URI=NotSet
DELETE_STACK=yes

COLOR='\033[0;33m'
NO_COLOR='\033[0m' # No Color

if [ "x$1" != x ];
then
  ECR_URI=$1
fi

if [ "x$2" != x ];  
then
  DELETE_STACK=$2
fi

echo "${COLOR}"
echo --------------------------------------------
echo ECR URI: $ECR_URI
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
sam deploy --stack-name $STACK_NAME --resolve-s3 --s3-prefix $STACK_NAME --image-repository $ECR_URI --no-confirm-changeset --no-fail-on-empty-changeset --capabilities CAPABILITY_IAM