@echo off
setlocal

for /f "tokens=1,2 delims==" %%a in (benchmark_settings.cfg) do (
    if "%%a"=="UNITY_PATH" set UNITY_PATH=%%b
    if "%%a"=="PROJECT_PATH" set PROJECT_PATH=%%b
)

if not exist "%UNITY_PATH%" (
    echo Unity not found at: %UNITY_PATH%
    echo Please update UNITY_PATH in benchmark_settings.txt
    pause
    exit /b 1
)

if not exist "%PROJECT_PATH%" (
    echo Project not found at: %PROJECT_PATH%
    echo Please update PROJECT_PATH in benchmark_settings.txt
    pause
    exit /b 1
)

echo Running benchmarks...
"%UNITY_PATH%" -runTests -batchMode -projectPath "%PROJECT_PATH%" -testPlatform StandaloneWindows64 -buildTarget StandaloneWindows64 -mtRendering -scriptingbackend=il2cpp
if errorlevel 1 (
    echo Benchmark tests failed
    pause
    exit /b 1
)

echo Tests completed successfully. Exporting results...
"%UNITY_PATH%" -batchmode -projectPath "%PROJECT_PATH%" -executeMethod Scellecs.Morpeh.Benchmarks.Collections.Editor.BenchmarkExporter.RunExport -quit
if errorlevel 1 (
    echo Export failed
    pause
    exit /b 1
)

echo All done!
pause