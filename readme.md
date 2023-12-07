# Usage

## Step 1 - run SIMAP (Simple IMAP)

SIMAP, an application developed by me in Go, with no external dependancies, will run a simple IMAP server that we can communicate with.

1. Clone SIMAP:

git clone https://github.com/0xflux/SIMAP.git

2. Move to the cloned directory

cd SIMAP

3. Change the default environment variable usernamme:password

Open the dockerfile and edit `simap_poc_username=defaultUsername` and `simap_poc_password=defaultPassword` to contain a username:password combo of your choice.

4. Build the Docker image:

docker build -t simap .

5. Run the Docker container:

docker run -p 143:143 simap

## Step 2 - Set environment variables

To authenticate into the IMAP server, you must log in with a username and password, set the following environment variables in whichever machine you are running ZestyChips from. It **MUST** match whatever 
you chose in the above step, under sub-step 3, otehrwise you cannot authenticate.

E.g. on windows:

```
[System.Environment]::SetEnvironmentVariable("simap_poc_username", "exampleUser", [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable("simap_poc_password", "examplePassword", [System.EnvironmentVariableTarget]::User)
```
