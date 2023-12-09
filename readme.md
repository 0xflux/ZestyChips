# ZestyChips

Reverse engineering a .net c# stealer seen in the wild. You must **read this page in full** before you use or attempt to review this code, full sections include:

 - LEGAL DISCLAIMER
 - Usage

This project, including all associated source code and documentation, is developed and shared solely for educational, research, and defensive purposes in the field of cybersecurity. It is intended to be used exclusively by cybersecurity professionals, researchers, and educators to enhance understanding, develop defensive strategies, and improve security postures. **It is the user's responsibility to comply with all relevant local, state, national, and international laws and regulations related to cybersecurity and the use of such tools and information.**


![image](https://github.com/0xflux/ZestyChips/assets/49762827/171fa741-ba4d-4b49-9486-bb30a2046f9c)


# LEGAL DISCLAIMER

THIS IS NOT DESIGNED TO WORK 'AS IS' FOR ETHICAL REASONS TO PREVENT 'SCRIPT KIDDY' USAGE. If you wish to use this script, please get in touch via Twitter: https://twitter.com/0xfluxsec. **This requires an activation code to run**. This is written by a security professional, for security professionals.

This project, including all associated source code and documentation, is developed and shared solely for educational, research, and defensive purposes in the field of cybersecurity. It is intended to be used exclusively by cybersecurity professionals, researchers, and educators to enhance understanding, develop defensive strategies, and improve security postures.

**Under no circumstances shall this project be used for criminal, unethical, or any other unauthorized activities.** The re-engineering and analysis of the malware sample provided in this project are meant to serve as a resource for learning and should not be employed for offensive operations or actions that infringe upon any individual's or organization's rights or privacy.

The author of this project disclaim any responsibility for misuse or illegal application of the material provided herein. By accessing, studying, or using this project, you acknowledge and agree to use the information contained within strictly for lawful purposes and in a manner that is consistent with ethical guidelines and applicable laws and regulations.

USE AT YOUR OWN RISK. If you decide to use this software CONDUCT A THOROUGH INDEPENDENT CODE REVIEW to ensure it meets your standards. No unofficial third party dependencies are included to minimise attack surface of a supply chain risk. I cannot be held responsible for any problems that arise as a result of executing this, the burden is on the user of the software to validate its safety & integrity. All care has been taken to write safe code.

**It is the user's responsibility to comply with all relevant local, state, national, and international laws and regulations related to cybersecurity and the use of such tools and information.** If you are unsure about the legal implications of using or studying the material provided in this project, please consult with a legal professional before proceeding. Remember, responsible and ethical behavior is paramount in cybersecurity research and practice. The knowledge and tools shared in this project are provided in good faith to contribute positively to the cybersecurity community, and I trust they will be used with the utmost integrity.


# Usage

This is purpose built to use no third party libraries, other than official .net libraries built by Microsoft.

 - Requires >= .NET8.0

 - Requires the official SQLite package: https://www.nuget.org/packages/System.Data.SQLite/

 - Requires official ProtectData https://www.nuget.org/packages/System.Security.Cryptography.ProtectedData

 - Requires the c2 counterpart, written by me (0xflux) with no external dependencies: https://github.com/0xflux/SIMAP/ (see below instructions for installation with Docker, can run standalone if you like just by building the Go project)

 - Requires activation code, contact https://twitter.com/0xfluxsec for code.

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

5. Run (and enter) the container:

docker run -p 143:143 -it simap

6. Further Docker controls

Thereafter use `docker stop (container name)` and `docker start (container name)` to use the same container as above. To find containers use `docker ps -a`

## Step 2 - Set environment variables

To authenticate into the IMAP server, you must log in with a username and password, set the following environment variables in whichever machine you are running ZestyChips from. It **MUST** match whatever 
you chose in the above step, under sub-step 3, otehrwise you cannot authenticate.

E.g. on windows:

```
[System.Environment]::SetEnvironmentVariable("simap_poc_username", "exampleUser", [System.EnvironmentVariableTarget]::User)
[System.Environment]::SetEnvironmentVariable("simap_poc_password", "examplePassword", [System.EnvironmentVariableTarget]::User)
```
