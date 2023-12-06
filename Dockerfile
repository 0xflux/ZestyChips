FROM ubuntu:latest

# install packages
RUN apt-get update && \
    apt-get install -y dovecot-core dovecot-imapd nano

# copy Dovecot configuration files from project directory into the container
COPY dovecot.conf /etc/dovecot/
COPY 10-mail.conf /etc/dovecot/conf.d/
COPY 10-auth.conf /etc/dovecot/conf.d/
COPY dovecot-users /etc/dovecot/
COPY 10-ssl.conf /etc/dovecot/conf.d/10-ssl.conf

# expose the IMAP port
EXPOSE 143

# start Dovecot when the container launches
CMD ["dovecot", "-F"]