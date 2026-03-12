param(
    [string]$ExePath,
    [string]$TaskName
)

$xml = @"
  <?xml version="1.0" encoding="UTF-16"?>
  <Task version="1.2" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
    <Triggers>
      <LogonTrigger><Enabled>true</Enabled><UserId>$env:USERDOMAIN\$env:USERNAME</UserId></LogonTrigger>
    </Triggers>
    <Principals>
      <Principal id="Author">
        <UserId>$env:USERDOMAIN\$env:USERNAME</UserId>
        <LogonType>InteractiveToken</LogonType>
        <RunLevel>LeastPrivilege</RunLevel>
      </Principal>
    </Principals>
    <Settings>
      <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
      <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
      <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
      <Enabled>true</Enabled>
    </Settings>
    <Actions Context="Author">
      <Exec><Command>$ExePath</Command></Exec>
    </Actions>
  </Task>
"@

Register-ScheduledTask -TaskName $TaskName -Xml $xml.Trim() -Force
