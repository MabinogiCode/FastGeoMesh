Param(
    [string]$TestsDir = "tests\FastGeoMesh.Tests",
    [switch]$ApplyRenames
)

$report = @()
$files = Get-ChildItem -Path $TestsDir -Recurse -Filter *.cs
$total = $files.Count
$violationsCount = 0

foreach ($file in $files) {
    $content = Get-Content -Raw -Encoding UTF8 $file.FullName
    $lines = $content -split "\r?\n"

    $braceDepth = 0
    $topLevelTypeCount = 0
    $hasNestedType = $false
    $classStack = @()
    $hasMethodWithUnderscore = $false
    $hasStaticMethodInInstanceClass = $false

    for ($i = 0; $i -lt $lines.Length; $i++) {
        $line = $lines[$i]
        $trim = $line.Trim()

        # update brace depth based on braces in the line
        $open = ($line.ToCharArray() | Where-Object { $_ -eq '{' }).Count
        $close = ($line.ToCharArray() | Where-Object { $_ -eq '}' }).Count

        # detect namespace declaration
        if ($trim -match '^namespace\s+') {
            # nothing special
        }

        # detect type declarations
        if ($trim -match '^(public|internal|private|protected|sealed|static|partial|abstract|record|\s)*\s*(class|struct|record|enum)\s+([A-Za-z_][A-Za-z0-9_]*)') {
            $typeName = $Matches[3]
            # if braceDepth <= 1 consider top-level (inside namespace or file root)
            if ($braceDepth -le 1) {
                $topLevelTypeCount++
            } else {
                $hasNestedType = $true
            }

            # determine whether this class is static
            $isStatic = $trim -match '\bstatic\b'
            $classStack += @{ Name = $typeName; Static = $isStatic }
        }

        # detect method-like lines (very approximate)
        if ($trim -match '[A-Za-z0-9_<>\[\],\s]+\s+([A-Za-z0-9_]+)\s*\(') {
            $methodName = $Matches[1]
            if ($methodName -match '_') {
                $hasMethodWithUnderscore = $true
            }

            # if inside a class, check for static method and class static status
            if ($classStack.Count -gt 0) {
                $currentClass = $classStack[-1]
                if ($trim -match '\bstatic\b' -and -not $currentClass.Static) {
                    $hasStaticMethodInInstanceClass = $true
                }
            }
        }

        # update brace depth AFTER processing line so declarations at same line are considered at current depth
        $braceDepth += $open - $close

        # if closing a class scope, pop classStack when braceDepth decreased below a class start
        # Approximate: if classStack not empty and braceDepth < 1, clear stack
        if ($braceDepth -lt 1 -and $classStack.Count -gt 0) {
            $classStack = @()
        }
    }

    $fileViolations = @()
    if ($topLevelTypeCount -gt 1) { $fileViolations += "Multiple top-level types ($topLevelTypeCount)" }
    if ($hasNestedType) { $fileViolations += "Nested types detected" }
    if ($hasMethodWithUnderscore) { $fileViolations += "Method names contain underscores" }
    if ($hasStaticMethodInInstanceClass) { $fileViolations += "Static method inside non-static class" }

    if ($fileViolations.Count -gt 0) { $violationsCount++ }

    # filename underscores
    $fileName = $file.Name
    $renamed = $null
    if ($fileName -match '_') {
        $newName = $fileName -replace '_',''
        $newPath = Join-Path $file.DirectoryName $newName
        if ($ApplyRenames) {
            if (-not (Test-Path $newPath)) {
                Write-Host "Renaming $($file.FullName) -> $newPath"
                Rename-Item -Path $file.FullName -NewName $newName
                $renamed = $newName
            } else {
                $fileViolations += "Cannot rename: target exists ($newName)"
            }
        } else {
            $fileViolations += "Filename contains underscore ($fileName)"
        }
    }

    $report += [PSCustomObject]@{
        File = $file.FullName
        TopLevelTypes = $topLevelTypeCount
        HasNestedType = $hasNestedType
        MethodWithUnderscore = $hasMethodWithUnderscore
        StaticMethodInInstanceClass = $hasStaticMethodInInstanceClass
        FilenameRename = $renamed
        Violations = ($fileViolations -join '; ')
    }
}

# compute score
$filesWithViolations = $report | Where-Object { $_.Violations -ne '' }
$violationCount = $filesWithViolations.Count
$score = if ($total -eq 0) { 100 } else { [int](100 - (100 * $violationCount / $total)) }

$summary = @()
$summary += "Total files scanned: $total"
$summary += "Files with violations: $violationCount"
$summary += "Clean code score: $score / 100"
$summary += "Violations detail (first 50):"

$filesWithViolations | Select-Object -First 50 | ForEach-Object {
    $summary += "- $($_.File): $($_.Violations)"
}

$summaryText = $summary -join "`n"
$reportPath = Join-Path "tools" "clean_report.txt"
$summaryText | Out-File -FilePath $reportPath -Encoding UTF8

Write-Host $summaryText
Write-Host "Detailed CSV report written to $reportPath"

if ($ApplyRenames) { Write-Host "Renames applied." }
else { Write-Host "Run with -ApplyRenames to apply safe filename renames (removes '_' from filenames)." }
