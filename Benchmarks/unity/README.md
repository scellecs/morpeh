# Morpeh Benchmarks Setup Instructions

Before running the benchmarks, follow these steps:

## 1. Project Setup
- Create a clean Unity project
- Clone Morpeh into the Assets folder
- Install package: ``com.unity.test-framework.performance``

## 2. Project Settings Configuration
- Disable VSync
- Remove all Quality Settings except one
- Set Scripting Backend to IL2CPP
- Remove the camera from the scene
- Close Unity Editor (not needed anymore)

## 3. Configure Benchmark Settings
- Open ``benchmark_settings.cfg``
- Set ``UNITY_PATH`` to your Unity Editor path (e.g., C:\Program Files\Unity\Hub\Editor\2022.3.49f1\Editor\Unity.exe)
- Set ``PROJECT_PATH`` to your unity project folder path (e.g., M:/morpeh-2024)

## 4. Run Benchmarks
Run the ``run_benchmarks.bat`` script. It will:
- Build and run the benchmark tests
- Wait for tests completion
- Automatically export the results