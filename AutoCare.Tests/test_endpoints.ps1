$ProgressPreference = 'SilentlyContinue'

Write-Host "=== TEST 1: Public Home Page ==="
$r1 = Invoke-WebRequest -Uri "http://localhost:5287/" -UseBasicParsing
Write-Host "Status Code:" $r1.StatusCode
Write-Host "Has 'nav-link nav-link-autocare active':" ($r1.Content.Contains('nav-link nav-link-autocare active'))
Write-Host "Has 'Admin Hub' in navbar:" ($r1.Content.Contains('Admin Hub'))

Write-Host "`n=== TEST 2: Services Page ==="
$r2 = Invoke-WebRequest -Uri "http://localhost:5287/Home/Services" -UseBasicParsing
Write-Host "Status Code:" $r2.StatusCode
Write-Host "Services link is active:" ($r2.Content -match 'nav-link nav-link-autocare active[^"]*" href="/Home/Services"')

Write-Host "`n=== TEST 3: Register Page HTML & Attributes ==="
$r3 = Invoke-WebRequest -Uri "http://localhost:5287/Account/Register" -UseBasicParsing
Write-Host "Status Code:" $r3.StatusCode
Write-Host "Has 10-digit placeholder:" ($r3.Content.Contains('placeholder="9876543210"'))
Write-Host "Has maxlength 10:" ($r3.Content.Contains('maxlength="10"'))
Write-Host "Has data-val-regex:" ($r3.Content.Contains('data-val-regex='))
Write-Host 'Has data-val-regex-pattern ^[6-9]\d{9}$:' ($r3.Content.Contains('data-val-regex-pattern="^[6-9]\d{9}$"'))

Write-Host "`n=== TEST 4: Customer Dashboard Redirects when Unauthenticated ==="
try {
    $r4 = Invoke-WebRequest -Uri "http://localhost:5287/Customer/Dashboard" -MaximumRedirection 0 -UseBasicParsing
    Write-Host "Status Code:" $r4.StatusCode
} catch {
    Write-Host "Redirected to login as expected:" $_.Exception.Response.StatusCode $_.Exception.Response.Headers.Location
}

Write-Host "`n=== TEST 5: Admin Dashboard Redirects when Unauthenticated ==="
try {
    $r5 = Invoke-WebRequest -Uri "http://localhost:5287/Admin/Dashboard" -MaximumRedirection 0 -UseBasicParsing
    Write-Host "Status Code:" $r5.StatusCode
} catch {
    Write-Host "Redirected to login as expected:" $_.Exception.Response.StatusCode $_.Exception.Response.Headers.Location
}
