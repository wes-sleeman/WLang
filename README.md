# WCompiler  
>	The Authoritative Compiler for the W Programming Language  
---  
##	Setup  
Installing W is easy! Follow the steps to get W working in no-time.  
  1. Go to the [Releases page](https://github.com/wessupermare/WCompiler/releases) and grab the most recent version for your OS.  
  2. Extract/install the file as appropriate.  
  3. Run the command `W` to ensure setup completed successfully.  
  4. Enjoy!  
  
###	Setup with APT  
For easy updates on `apt` compatible systems (Debian, Ubuntu, etc.), consider adding the APT repo.  
Run the following commands in a compatible shell (I use `bash`, use `sudo` as needed.):  
```
echo "deb http://apt.wsleeman.com/ /" > /etc/apt/sources.list.d/wsleeman.list
wget -q -O - http://apt.wsleeman.com/KEY.gpg | apt-key add -
echo "deb http://apt.wsleeman.com/ /" > /etc/apt/sources.list.d/wsleeman.list
apt-get update
apt-get install wlang
```  
---  
##  Getting Started  
Head over to the [wiki](https://github.com/wessupermare/WCompiler/wiki) for tutorials and documentation.  
Check out the [project page](https://wsleeman.com/projects/w).  
