# Docker usage for IMAP server

Using the Dockerfile, use the following to build & interact:

```
docker build -t dove .

docker run -d --name dove-container -p 143:143 dove

docker exec -it dove-container bash

```