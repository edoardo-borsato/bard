# bard

A tool to backup and restore Dynamo DB tables to/from a JSON file

## Usage

### Commands

- `backup`: perform backup from given DynamoDB table to specified JSON file
- `restore`: restore data from given JSON file to specified DynamoDB table

### Arguments

Both commands support the same arguments:

- `-t | --table`: Required. The name of the table
- `-f | --file`: Required. The backup JSON file full path
- `-p | --aws-profile`: Required. The AWS profile to use. This refers to the AWS profile (and its corresponding role) defined in the `C:\Users\<username>\.aws\credentials` file
- `-r | --aws-region`: The code of the AWS region (e.g.: eu-central-1, us-east-1, ...). If the parameter is not provided the default region of the AWS profile will be used. For the complete list of AWS region codes see [here](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/using-regions-availability-zones.html#concepts-regions)

### Examples

- `bard backup -t Test -f "D:\test.json" -p profile`
- `bard backup -t Test -f D:\test.json -p profile -r eu-central-1`
- `bard restore -t Test -f D:\test.json -p profile`
- `bard restore -t Test -f "D:\test.json" -p profile -r us-east-1`
