sc create DocsifyNetService binPath="D:\Git\DocsifyNet\DocsifyNet\bin\Release\net6.0\publish\DocsifyNet.exe" start=auto
sc start DocsifyNetService
echo "success install"
pause