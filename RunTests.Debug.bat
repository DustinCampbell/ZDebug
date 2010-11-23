echo off
pushd Tests
..\Tools\NUnit\nunit-console.exe .\ZDebug.nunit /nologo
popd