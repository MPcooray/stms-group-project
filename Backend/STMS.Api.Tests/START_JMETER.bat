@echo off
echo ========================================
echo Starting JMeter for STMS Load Testing
echo ========================================
echo.

cd C:\Users\Cooray\Downloads\apache-jmeter-5.6.3\apache-jmeter-5.6.3\bin

echo Starting JMeter GUI...
echo.
echo Once JMeter opens:
echo 1. File ^> Open ^> Select STMS_Load_Test.jmx
echo 2. Click the Play button to run tests
echo 3. View results in listeners
echo.

start jmeter.bat

echo JMeter is starting...
echo.
pause
