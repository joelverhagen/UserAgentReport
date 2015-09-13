#!/bin/bash

DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOL=$DIR/Knapcode.UserAgentReport.exe
LATEST=$DIR/user-agents-latest

rm -f $LATEST.sqlite3

$TOOL -db $LATEST.sqlite3 -populate
$TOOL -db $LATEST.sqlite3 -query > $LATEST.json

mv $LATEST.sqlite3 $DIR/user-agents.sqlite
mv $LATEST.json $DIR/user-agents.json
