#!/bin/bash

curl -sSL https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.sh | DNX_BRANCH=dev sh && source ~/.dnx/dnvm/dnvm.sh
dnvm install 1.0.0-beta8-15599 -u
dnu restore SharpStar -f https://www.myget.org/F/aspnetvnext/ -f https://www.myget.org/F/star2/api/v2
dnu restore StarLib -f https://www.myget.org/F/aspnetvnext/
dnu publish SharpStar -o output