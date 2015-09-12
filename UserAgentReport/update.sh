#!/bin/bash

DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )

$DIR/Knapcode.UserAgentReport.exe -populate -query > $DIR/user-agents-latest.json
mv $DIR/user-agents-latest.json $DIR/user-agents.json
