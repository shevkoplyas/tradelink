@echo 0 = %0
@echo 1 = %1
@echo 2 = %2

@rem Find directory where NUnit is installed
@for /f "delims=;" %%i in ('dir /b /x "%PROGRAMFILES%\NUnit*"') do set n_dir=%%i
@set NUNIT=%PROGRAMFILES%\%n_dir%

@rem Determine correct version of NUnit to use for this run (either 32-bit or 64-bit version)
@set Platform=
@if /i "%2" == "x86" set Platform=-x86
@echo Platform = %Platform%

@rem NUnit Console Mode
@rem "%NUNIT%\bin\net-2.0\nunit-console%Platform%.exe" "%~f1"

@rem NUnit GUI Mode
"%NUNIT%\bin\net-2.0\nunit%Platform%.exe" "%~f1" /run
