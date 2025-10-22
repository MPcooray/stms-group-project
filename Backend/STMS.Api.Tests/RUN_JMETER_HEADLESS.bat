@echo off
set JMETER_BIN=C:\Users\Cooray\Downloads\apache-jmeter-5.6.3\apache-jmeter-5.6.3\bin
set PLAN=STMS_Load_Test.jmx
set RESULTS=results.jtl
set REPORT_DIR=reports

REM Optional args: %1 = portNumber (defaults to 5000), %2 = serverName (defaults to localhost)
set PORT=%1
if "%PORT%"=="" set PORT=5000
set HOST=%2
if "%HOST%"=="" set HOST=localhost

echo Using host=%HOST% port=%PORT%

if not exist "%JMETER_BIN%\jmeter.bat" (
  echo Could not find jmeter.bat at %JMETER_BIN%
  echo Please update JMETER_BIN in this script.
  pause
  exit /b 1
)

if not exist %PLAN% (
  echo Could not find %PLAN% in current folder: %cd%
  echo Please run from Backend\STMS.Api.Tests or update PLAN variable.
  pause
  exit /b 1
)

if exist %RESULTS% del /f /q %RESULTS%
if exist %REPORT_DIR% rmdir /s /q %REPORT_DIR%

call "%JMETER_BIN%\jmeter.bat" -n -t %PLAN% -l %RESULTS% -JportNumber=%PORT% -JserverName=%HOST% -e -o %REPORT_DIR%

if %errorlevel% neq 0 (
  echo JMeter run failed with exit code %errorlevel%
  pause
  exit /b %errorlevel%
)

echo Report generated at %cd%\%REPORT_DIR%
start %cd%\%REPORT_DIR%\index.html
