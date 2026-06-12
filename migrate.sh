#!/bin/bash

cd ./src
PS3='Select environment: '
options=("Staging" "Production" "Quit")
select opt in "${options[@]}"; do
case $opt in
    "Staging")
        (dotnet ef database update --verbose --connection "Host=localhost;Port=3001;Database=project-staging;Username=postgres" --startup-project API --project Persistence -- --environment Staging)
        ;;
    "Production")
        (dotnet ef database update --verbose --connection "Host=localhost;Port=3003;Database=project-prod;Username=postgres" --startup-project API --project Persistence -- --environment Production)
        ;;
    "Quit")
        break
        ;;
    *) echo "invalid option $REPLY";;
esac
done
