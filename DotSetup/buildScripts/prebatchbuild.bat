@ECHO OFF

pushd %~dp0
echo %1
echo %2

AutoGenerateAssemblyInfo.exe %1 %2

popd
