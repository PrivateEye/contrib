@echo off
set COR_ENABLE_PROFILING=1
set COR_PROFILER={D51126CE-1443-42ED-8FD6-B4D32C466292}
if "%PROCESSOR_ARCHITECTURE%" == "x86" (
set COR_PROFILER_PATH=%~dp0\Win32\PrivateEye.Profiler.dll
) else (
set COR_PROFILER_PATH=%~dp0\x64\PrivateEye.Profiler.dll
)
set PRIVATEEYE_PROFILER_CONFIG=%~dp0\runfsi-self.yml
echo %COR_PROFILER_PATH%
"%ProgramFiles(x86)%\Microsoft SDKs\F#\3.1\Framework\v4.0\Fsianycpu.exe" --load:privateeye.fsx
set COR_ENABLE_PROFILING=0
