$q = [char]39
$doc = Get-Content "$PSScriptRoot\SYSTEM_DOCUMENTATION.txt" -Raw
$csv = Import-Csv "$PSScriptRoot\_users_tmp.csv"

$admins   = $csv | Where-Object { $_.Type -eq 'admin' }
$teachers = $csv | Where-Object { $_.Type -eq 'teacher' } | Sort-Object { [int]($_.User -replace '\D','') }
$students = $csv | Where-Object { $_.Type -eq 'student' } | Sort-Object { [int]($_.User -replace '\D','') }

function Format-Group($heading, $rows, $password) {
    $out = New-Object System.Text.StringBuilder
    [void]$out.AppendLine("")
    [void]$out.AppendLine("   $heading")
    [void]$out.AppendLine("   +------+------------+--------------+--------------------------------+")
    [void]$out.AppendLine("   | ID   | USERNAME   | PASSWORD     | FULL NAME                      |")
    [void]$out.AppendLine("   +------+------------+--------------+--------------------------------+")
    foreach ($r in $rows) {
        $name = $r.Name
        if ($name.Length -gt 30) { $name = $name.Substring(0,30) }
        $line = "   | {0,-4} | {1,-10} | {2,-12} | {3,-30} |" -f $r.Id, $r.User, $password, $name
        [void]$out.AppendLine($line)
    }
    [void]$out.AppendLine("   +------+------------+--------------+--------------------------------+")
    return $out.ToString()
}

$table = Format-Group "C.1 ADMIN ACCOUNTS (1)" $admins 'admin'
$table += Format-Group ("C.2 TEACHER ACCOUNTS ({0})" -f $teachers.Count) $teachers 'password123'
$table += Format-Group ("C.3 STUDENT ACCOUNTS ({0})" -f $students.Count) $students 'password123'

$doc = $doc.Replace('<<USER_TABLE_HERE>>', $table)
Set-Content -Path "$PSScriptRoot\SYSTEM_DOCUMENTATION.txt" -Value $doc -Encoding UTF8
Write-Host "Injected $($csv.Count) accounts into SYSTEM_DOCUMENTATION.txt"
Remove-Item "$PSScriptRoot\_users_tmp.csv","$PSScriptRoot\_extract_users.ps1","$PSScriptRoot\_extract_users_v2.ps1" -ErrorAction SilentlyContinue
Write-Host "Temp files removed."