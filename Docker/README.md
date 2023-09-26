# Setup

This file includes a full list of services and how to configure them to run in either a 
development or production environment.

1. copy the ``.env.example`` file and rename it to be ``.env``
2. build in dev/prod:
  - ```docker-compose --profile dev build --no-cache```
  - ```docker-compose --profile prod build --no-cache```
3. run in dev/prod:
  - ```docker-compose --profile dev run -d```
  - ```docker-compose --profile prod run -d```

## Services
### TAPAS
### PostGreSQL
### Elasticsearch

1. Configure your password in the ``.env`` file.
2. Make sure to set the same password in the correct ``appsettings.{Environment}.json`` file.
3. After Kibana is started enter a console within ``elasticsearch`` and execute:
  ``bin/elasticsearch-create-enrollment-token -s kibana``.
4. Copy the result and use it as the enrollment token in Kibana.

### Kibana
1. After connecting go to ``Elasticsearch:3``.
2. When prompted for it view the logs of ``Kibana`` there you will find the code.
3. Run ``TAPAS``.
4. Log in using ``elastic`` and your password.
5. Once logged in navigate to ``Left Panel->Management->Data->Index Management`` you should see
  see an index.