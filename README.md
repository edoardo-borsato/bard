# bard

A tool to perform some operations in Dynamo DB tables.

## Framework

.NET 8

## Prerequisites

[AWS CLI](https://aws.amazon.com/cli/)

## Usage

### Commands

- `backup`: perform backup from given DynamoDB table to specified JSON file
- `restore`: restore data from given JSON file to specified DynamoDB table
- `batch-delete`: delete all records from the specified DynamoDB table matching partition key and sort key details

### Arguments

`backup` supports the following arguments:

- `-t | --table`: Required. The name of the table
- `-f | --file`: Required. The backup JSON file full path
- `-p | --aws-profile`: Required. The AWS profile to use. This refers to the AWS profile (and its corresponding role) defined in the `C:\Users\<username>\.aws\credentials` file
- `-r | --aws-region`: The code of the AWS region (e.g.: eu-central-1, us-east-1, ...). If the parameter is not provided the default region of the AWS profile will be used. For the complete list of AWS region codes see [here](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/using-regions-availability-zones.html#concepts-regions)

`restore` supports the following arguments:

- `-t | --table`: Required. The name of the table
- `-f | --file`: Required. The backup JSON file full path
- `-p | --aws-profile`: Required. The AWS profile to use. This refers to the AWS profile (and its corresponding role) defined in the `C:\Users\<username>\.aws\credentials` file
- `-r | --aws-region`: The code of the AWS region (e.g.: eu-central-1, us-east-1, ...). If the parameter is not provided the default region of the AWS profile will be used. For the complete list of AWS region codes see [here](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/using-regions-availability-zones.html#concepts-regions)

`batch-delete` supports the following arguments:

- `-t | --table`: Required. The name of the table
- `--partition-key-name`: Required. The partition key name
- `--partition-key-type`: Required. The partition key type. Supported types are `B` for binary, `S` for string and `N` for number
- `--partition-key-value`: Required. The partition key value
- `--sort-key-name`: The sort key name. If the parameter is not provided all the records matching the partition key will be deleted
- `--sort-key-type`: The sort key type. Supported types are `B` for binary, `S` for string and `N` for number
- `--sort-key-value`: The sort key value
- `-p | --aws-profile`: Required. The AWS profile to use. This refers to the AWS profile (and its corresponding role) defined in the `C:\Users\<username>\.aws\credentials` file
- `-r | --aws-region`: The code of the AWS region (e.g.: eu-central-1, us-east-1, ...). If the parameter is not provided the default region of the AWS profile will be used. For the complete list of AWS region codes see [here](https://docs.aws.amazon.com/AWSEC2/latest/UserGuide/using-regions-availability-zones.html#concepts-regions)

### Examples

- `bard backup -t Test -f "D:\test.json" -p profile`
- `bard backup -t Test -f D:\test.json -p profile -r eu-central-1`
- `bard restore -t Test -f D:\test.json -p profile`
- `bard restore -t Test -f "D:\test.json" -p profile -r us-east-1`
- `batch-delete -t TEST -p dev -r eu-central-1 --partition-key-name id --partition-key-type S --partition-key-value a`
- `batch-delete -t TEST -p dev -r eu-central-1 --partition-key-name id --partition-key-type S --partition-key-value b --sort-key-name number --sort-key-type N --sort-key-value 2`
