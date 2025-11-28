function setHostEntries([hashtable] $entries) {
    $hostsFile = "$env:windir\System32\drivers\etc\hosts"
    $newLines = @()

    $c = Get-Content -Path $hostsFile
    foreach ($line in $c) {
        $bits = [regex]::Split($line, "\s+")
        if ($bits.count -eq 2) {
            $match = $NULL
            ForEach ($entry in $entries.GetEnumerator()) {
                if ($bits[1] -eq $entry.Key) {
                    $newLines += ($entry.Value + '     ' + $entry.Key)
                    Write-Host Replacing HOSTS entry for $entry.Key
                    $match = $entry.Key
                    break
                }
            }
            if ($match -eq $NULL) {
                $newLines += $line
            }
            else {
                $entries.Remove($match)
            }
        }
        else {
            $newLines += $line
        }
    }

    foreach ($entry in $entries.GetEnumerator()) {
        Write-Host Adding HOSTS entry for $entry.Key
        $newLines += $entry.Value + '     ' + $entry.Key
    }

    Write-Host Saving $hostsFile
    Clear-Content $hostsFile
    foreach ($line in $newLines) {
        $line | Out-File -encoding ASCII -append $hostsFile
    }
}

$entries = @{
    'whooweswho.mssql.local'         = "127.0.0.1"
    'whooweswho.migration.local'     = "127.0.0.1"
    'whooweswho.encryption.local'    = "127.0.0.1"
    'whooweswho.currency.local'      = "127.0.0.1"
    'whooweswho.authorization.local' = "127.0.0.1"
    'whooweswho.user.local'          = "127.0.0.1"
    'whooweswho.messaging.local'     = "127.0.0.1"
    'whooweswho.event.local'         = "127.0.0.1"
    'whooweswho.payment.local'       = "127.0.0.1"
};
setHostEntries($entries)