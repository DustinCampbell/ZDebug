echo off
pushd Tests
..\Tools\NUnit\nunit-console-x86.exe .\ZDebug.nunit /nologo
popd