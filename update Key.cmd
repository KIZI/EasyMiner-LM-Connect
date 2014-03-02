@ECHO OFF

IF EXIST Data\key.sqlite (
	ECHO Updating ...
	Build\Release\LMConnect.Console.exe key update ..\..\Sources\LMConnect.Web\hibernate.cfg.xml
) ELSE (
	ECHO Creating ...
	Build\Release\LMConnect.Console.exe key create ..\..\Sources\LMConnect.Web\hibernate.cfg.xml
	ECHO.

	ECHO Initializing ...
	Build\Release\LMConnect.Console.exe key init ..\..\Sources\LMConnect.Web\hibernate.cfg.xml
)