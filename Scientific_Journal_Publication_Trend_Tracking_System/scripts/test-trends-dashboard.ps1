$BaseUrl = "http://localhost:5220"
$Passed = 0
$Failed = 0
$Results = @()

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [object]$Body = $null,
        [int[]]$ExpectedStatus = @(200)
    )

    try {
        $params = @{
            Uri             = $Url
            Method          = $Method
            UseBasicParsing = $true
            ErrorAction     = "Stop"
        }

        if ($Body) {
            $params["ContentType"] = "application/json"
            $params["Body"] = ($Body | ConvertTo-Json -Depth 10)
        }

        $response = Invoke-WebRequest @params
        $status = [int]$response.StatusCode

        if ($ExpectedStatus -contains $status) {
            $script:Passed++
            $script:Results += [PSCustomObject]@{ Test = $Name; Status = "PASS"; Code = $status; Detail = "OK" }
            return $response.Content
        }

        $script:Failed++
        $script:Results += [PSCustomObject]@{ Test = $Name; Status = "FAIL"; Code = $status; Detail = "Unexpected status" }
        return $null
    }
    catch {
        $status = 0
        if ($_.Exception.Response) {
            $status = [int]$_.Exception.Response.StatusCode
        }

        if ($ExpectedStatus -contains $status) {
            $script:Passed++
            $script:Results += [PSCustomObject]@{ Test = $Name; Status = "PASS"; Code = $status; Detail = "Expected error status" }
            return $null
        }

        $detail = $_.Exception.Message
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) {
            $detail = $_.ErrorDetails.Message
        }

        $script:Failed++
        $script:Results += [PSCustomObject]@{ Test = $Name; Status = "FAIL"; Code = $status; Detail = $detail }
        return $null
    }
}

Write-Host "=== Testing Trends & Dashboard API ===" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl`n"

# Dashboard (suppress JSON output)
Test-Endpoint -Name "Dashboard Summary" -Method GET -Url "$BaseUrl/api/dashboard/summary" | Out-Null
Test-Endpoint -Name "Chart Publications By Year" -Method GET -Url "$BaseUrl/api/dashboard/charts/publications-by-year?fromYear=2018&toYear=2024" | Out-Null
Test-Endpoint -Name "Chart Top Keywords" -Method GET -Url "$BaseUrl/api/dashboard/charts/top-keywords?topCount=5" | Out-Null
Test-Endpoint -Name "Chart Publications By Domain" -Method GET -Url "$BaseUrl/api/dashboard/charts/publications-by-domain" | Out-Null
Test-Endpoint -Name "Chart Top Journals" -Method GET -Url "$BaseUrl/api/dashboard/charts/top-journals?topCount=5" | Out-Null

# Trends
Test-Endpoint -Name "Trending Topics" -Method GET -Url "$BaseUrl/api/trends/trending-topics?topCount=5&recentYears=2" | Out-Null
Test-Endpoint -Name "Keyword Trend" -Method GET -Url "$BaseUrl/api/trends/keywords?keyword=machine%20learning&fromYear=2018&toYear=2024" | Out-Null
Test-Endpoint -Name "Research Topics List" -Method GET -Url "$BaseUrl/api/trends/topics" | Out-Null

$topicsJson = Test-Endpoint -Name "Research Topics (for topic trend)" -Method GET -Url "$BaseUrl/api/trends/topics" | Out-String
if ($topicsJson) {
    $topics = @(($topicsJson | ConvertFrom-Json).data)
    if ($topics.Count -gt 0) {
        $topicId = [string]$topics[0].id
        Test-Endpoint -Name "Topic Trend by ID" -Method GET -Url "$BaseUrl/api/trends/topics/$topicId`?fromYear=2018&toYear=2024" | Out-Null
    }
    else {
        $Failed++
        $Results += [PSCustomObject]@{ Test = "Topic Trend by ID"; Status = "FAIL"; Code = "-"; Detail = "No topics returned from seed" }
    }
}

# Validation errors (expect 400)
Test-Endpoint -Name "Keyword Trend - empty keyword" -Method GET -Url "$BaseUrl/api/trends/keywords?keyword=" -ExpectedStatus @(400)
Test-Endpoint -Name "Trending Topics - invalid topCount" -Method GET -Url "$BaseUrl/api/trends/trending-topics?topCount=999" -ExpectedStatus @(400)
Test-Endpoint -Name "Topic Trend - not found" -Method GET -Url "$BaseUrl/api/trends/topics/00000000-0000-0000-0000-000000000001" -ExpectedStatus @(404)

Write-Host "`n=== Results ===" -ForegroundColor Cyan
$Results | Format-Table -AutoSize

Write-Host "Passed: $Passed | Failed: $Failed" -ForegroundColor $(if ($Failed -eq 0) { "Green" } else { "Red" })

if ($Failed -gt 0) { exit 1 }
exit 0
