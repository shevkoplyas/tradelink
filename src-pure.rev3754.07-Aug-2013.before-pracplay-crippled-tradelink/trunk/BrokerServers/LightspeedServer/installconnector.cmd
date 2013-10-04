@echo off
set OUTDIR=%1
if not exist %OUTDIR% (
echo no build outdir provided as argument
goto :eof
)
if exist c:\progra~1\lightspeed (
set LSPATH=c:\progra~1\lightspeed\
) else (
if exist c:\progra~2\lightspeed (
set LSPATH=c:\progra~2\lightspeed\
) else (
echo lightspeed is not installed in program files
goto :eof
)
)
echo found lightspeed in %LSPATH%
echo using build in %OUTDIR%
if not exist %LSPATH%\LightspeedServer.dll copy /y "%OUTDIR%\LightspeedServer.dll" %LSPATH%
if not exist %LSPATH%\LightspeedServer.Config.txt copy /y LightspeedServer.Config.txt %LSPATH%
if not exist %LSPATH%\TradeLibFast.dll copy /y "%OUTDIR%\TradeLibFast.dll" %LSPATH%
