sc stop DocsifyNetService
timeout /nobreak /t 2 > null
sc delete DocsifyNetService
echo "uninstall success"
pause