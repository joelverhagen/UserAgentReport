#!/bin/bash

curl -s 'http://useragentreport.azurewebsites.net/api/v1/top-user-agents?limit=1' | python -m json.tool
