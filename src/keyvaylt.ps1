
$keyVaultName = "contoso-tst-api-vault"
$objectId = "7dcef129-f912-4e5c-bea5-24615733c914"
$appId = "92701148-5ebb-4a83-8610-e9455e83eb1e"

[String[]] $permissionsToSecrets = ("get","list","set","delete")
[String[]] $permissionsToKeys = ("get","list","update","create","delete")
[String[]] $permissionsToCertificates = ("get")

az ad app show --id $appId
az account set --subscription my-subscription

Set-AzureRmKeyVaultAccessPolicy -VaultName $keyVaultName -ObjectId $objectId -PermissionsToSecrets $permissionsToSecrets -PermissionsToKeys $permissionsToKeys -PermissionsToCertificates $permissionsToCertificates
