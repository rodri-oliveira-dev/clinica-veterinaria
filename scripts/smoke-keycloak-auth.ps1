[CmdletBinding()]
param(
    [string] $ApiBaseUrl = "http://localhost:5000",
    [string] $KeycloakBaseUrl = "http://localhost:8080",
    [string] $Realm = "petshop-local",
    [string] $ClientId = "petshop-api",
    [string] $Username = "local.petshop.user",
    [string] $Password = "local-dev-password",
    [string] $ExpectedTenantId = "11111111-1111-4111-8111-111111111111"
)

$ErrorActionPreference = "Stop"

function Invoke-SmokeRequest {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Uri,

        [Parameter(Mandatory = $false)]
        [hashtable] $Headers
    )

    try {
        if ($Headers) {
            return Invoke-WebRequest -Uri $Uri -Headers $Headers -UseBasicParsing
        }

        return Invoke-WebRequest -Uri $Uri -UseBasicParsing
    }
    catch [System.Net.WebException] {
        if ($_.Exception.Response -is [System.Net.HttpWebResponse]) {
            $response = [System.Net.HttpWebResponse] $_.Exception.Response
            $statusCode = [int] $response.StatusCode
            $reader = [System.IO.StreamReader]::new($response.GetResponseStream())
            try {
                $content = $reader.ReadToEnd()
            }
            finally {
                $reader.Dispose()
                $response.Dispose()
            }

            return [PSCustomObject]@{
                StatusCode = $statusCode
                Content = $content
            }
        }

        throw
    }
}

$diagnosticsUrl = "$($ApiBaseUrl.TrimEnd('/'))/diagnostics"
$tokenUrl = "$($KeycloakBaseUrl.TrimEnd('/'))/realms/$Realm/protocol/openid-connect/token"

$anonymousResponse = Invoke-SmokeRequest -Uri $diagnosticsUrl
if ($anonymousResponse.StatusCode -ne 401) {
    throw "Expected /diagnostics without token to return 401, got $($anonymousResponse.StatusCode)."
}

$tokenResponse = Invoke-RestMethod `
    -Uri $tokenUrl `
    -Method Post `
    -ContentType "application/x-www-form-urlencoded" `
    -Body @{
        grant_type = "password"
        client_id = $ClientId
        username = $Username
        password = $Password
    }

if ([string]::IsNullOrWhiteSpace($tokenResponse.access_token)) {
    throw "Keycloak did not return an access token."
}

$correlationId = [Guid]::NewGuid().ToString()
$authorizedResponse = Invoke-SmokeRequest `
    -Uri $diagnosticsUrl `
    -Headers @{
        Authorization = "Bearer $($tokenResponse.access_token)"
        "X-Correlation-Id" = $correlationId
    }

if ($authorizedResponse.StatusCode -ne 200) {
    throw "Expected /diagnostics with authorized token to return 200, got $($authorizedResponse.StatusCode)."
}

$body = $authorizedResponse.Content | ConvertFrom-Json
if ($body.service -ne "PetShop.Api") {
    throw "Unexpected diagnostics service '$($body.service)'."
}

if ($body.correlationId -ne $correlationId) {
    throw "Diagnostics response did not preserve X-Correlation-Id."
}

if ($body.tenantId -ne $ExpectedTenantId) {
    throw "Diagnostics response did not expose the authenticated tenant context."
}

if ($authorizedResponse.Content -match "access_token|refresh_token|resource_access|tenant_id|preferred_username") {
    throw "Diagnostics response exposed authentication details."
}

Write-Host "Keycloak authentication smoke test passed."
