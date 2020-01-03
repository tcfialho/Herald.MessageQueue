dotnet tool install -g dotnet-reportgenerator-globaltool
dotnet test /maxcpucount:1 /p:CollectCoverage=true /p:CoverletOutputFormat=\"json,opencover\" /p:CoverletOutput="../../coverage/" /p:MergeWith="../coverage.json" /p:Include="[Herald.MessageQueue*.*]*"
reportgenerator "-reports:../coverage/coverage.xml" "-targetdir:../coverage"
start "" "..\coverage\index.htm"