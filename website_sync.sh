#!/bin/bash

curl -s --data '' 'http://useragentreport.azurewebsites.net/api/v1/management/update-user-agent-database' | python -m json.tool
