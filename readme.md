# Docker usage for IMAP server

```
# run container
docker run -d --name dovecot-imap -p 143:143 dovecot/dovecot

# if clash, note the id, then run with (then go straight to interact)

docker start f28de1aa1b23c0162b4fe7918af9217fbe598d87eadb8d17c3b8954f8e47c459

# list containers
docker ps

# interact
docker exec -it [container-id] /bin/bash

# quit 
docker stop [container-id]
```