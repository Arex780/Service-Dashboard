# NSG WebArchive (Service Dashboard)
Service Dashboard is a C# Web Application (.NET Core MVC) which stores information about other services and web applications (Wiki style) and checks status of provided services (online, offline)

## Features
*  User can add/edit service in database
*  Admin users can be defined
*  Every change is reported in a log file
*  Main Page with as table with all services (with sorting and filtering)
*  Sending emails when website goes online or offline
*  Authentication is based on Windows Active Directory

## Stored information of the project
*  Name
*  Version
*  Category
*  Owner
*  Developers
*  Short and long description
*  Website address
*  Repository address
*  Programming languages used
*  Technologies used
*  Logo
*  Overview image

## Logs
There are 3 types of log files in Service Dashboard:
*  Projects - shows all changes done by user in the project page
*  Emails - shows all email sent about project
*  Status Code - shows all failed status codes of project's website (eg. Bad Gateway)

## Configuration
Two files need to be created from example file (`appsettings.json` and `.env`)
#### Admin users
Admin users can be set in the file `appsettings.json`:

`"AdminList": [ "user" ]`

#### Update Interval
By default the website is checking status of other services every 4 minutes. It can be changed in the file `appsettings.json`:

`"UpdateInterval": 4,`

#### SMTP
Website is sending emails via SMTP. Simple configuration requires to change only `sender` email. More advanced settings are also avaible. It can be configured in the file `appsettings.json`:

```json
"sender": "team.email.example@mail.com",
"host": "smtp.mail.com",
"port": 25,
"ssl": false,
"username": "",
"password": ""
```

#### LDAP
In order to use Windows Active Directory Authentication the faceless username and password need to be provided (url and port configuration is also required). It can be set in the file `appsettings.json`:

```json
"username": "DOMAIN\\faceless_account_name", 
"password": "faceless_account_password",
```

#### Kestrel
Kestrel is used to host web application. By default it uses port 80 for HTTP and port 443 for HTTPS. Port can be changed in the file `appsettings.json`:

`"Url": "http://*:80"`

If HTTPS is used the valid certficate need to be provide as *.pfx file with password. It can be set in the file `appsettings.json`:

```json
"Path": "certificate-path.pfx", 
"Password": "certificate-password"
```

#### Databse password
Database password can be set in the file `.env`:

`DB_PASSWORD=Database_Password`

#### Website DNS
Emails have hyperlinks to website and when DNS is not set then IP address is used (Not providing DNS is not recommend when hosting via Docker because it uses container's IP and not host's IP). DNS can be set in the file `.env`:

`WEBSITE_DNS=`

## Deployment
There are two ways to deploy this application:

#### Via Visual Studio
Could be used for development environment (to test changes in the code). In order to deploy application via Visual Studio the connection string in the file `appsettings.json` needs to be set like this (to ensure local database is used):

`"WebDataContext": "Server=(localdb)\\mssqllocaldb;Database=context;Trusted_Connection=True;"`

#### Via Docker
Could be used for production environment. In order to deploy application via Docker external connection to database (database is in the different container) need to be established. It can be achieved by setting connection string in the file `appsettings.json` like this:

`"WebDataContext": "Server=db;Database=context;User=sa"`

There are 3 volumes: db_data (for persistance of database records), db_backup (for automated database backups) and web_logs (for persistance of logs file)

## Database backup and restore
Backup and restore can be done in two different ways:

#### Automated backup (Docker)
Application is using "backup" container for scheduled database backups. The configuration can be found in `.env` file. `CRON_SCHEDULE` is used to set time of backups. If `BACKUP_CLEANUP` is set to `true` then backup files will be removed if they are older than `BACKUP_AGE` (in days).

#### Manual backup
Manual backup can be useful when moving database records from development to production environment. To do manual backup this SQL query need to be executed (for Windows):

`BACKUP DATABASE <database_name> TO DISK = 'C:\Users\<username>\<backup_name>.bak'`

#### Manual restore (Docker)
First step is to see `<container_id>` of `db` container by `docker ps` command

Second step is to see `LogicalName` of desired backup file in Powershell:

```powershell
docker exec -it <containter_id> /opt/mssql-tools/bin/sqlcmd -S localhost `
   -U SA -P "<database_password>" `
   -Q "RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/backup/<backup_name>.bak'"
```

In most of the cases backup file will consist of `PRIMARY` part and `LOG` part. Each of these parts have `LogicalName`. To restore database backup use this Powershell command:

```powershell
docker exec -it <container_id> /opt/mssql-tools/bin/sqlcmd `
   -S localhost -U SA -P "<database_password>" `
   -Q "RESTORE DATABASE <database_name> FROM DISK = '/var/opt/mssql/backup/<backup_name>.bak' WITH REPLACE, MOVE '<LogicalName of Primary>' TO '/var/opt/mssql/data/<database_name>.mdf', MOVE 'LogicalName of Log' TO '/var/opt/mssql/data/<database_name>_log.ldf'"
```

If you want to use these commands in Bash replace new line char \` with `\`.