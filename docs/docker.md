# Docker
Holder

## Deployment
Holder.

- **Image**: `niemandr/rn-timerr` ([open](https://hub.docker.com/repository/docker/niemandr/rn-timerr))
- **Logo**: `N/A`
- **Volumes**
  - `/app/appsettings.json:/mnt/user/Backups/app-data/rn-timerr/appsettings.json`
  - `/app/nlog.config:/mnt/user/Backups/app-data/rn-timerr/nlog.config`
  - `/logs:/mnt/user/appdata/logs/rn-timerr/`

### appsettings.json
```json
{
  "ConnectionStrings": {
    "RnTimerr": "..."
  },
  "RnTimerr": {
    "Host": "host"
  }
}
```

### nlog.config
```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd" xsi:schemaLocation="NLog NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <targets>
    <target xsi:type="File" name="logfile" fileName="/logs/rn-timerr.log" layout="${longdate} (${level}) ${message}" />
    <target xsi:type="Console" name="logconsole" layout="${longdate} (${level}) ${message}" />
  </targets>
  <rules>
    <logger name="*" minlevel="Trace" writeTo="logfile,logconsole" />
  </rules>
</nlog>
```
