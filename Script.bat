set location=%cd%
rmdir /s /q "%location%\.vs"
FOR /d /r . %%d IN (bin) DO @IF EXIST "%%d" (echo "%%d" & rd /s /q "%%d")
FOR /d /r . %%d IN (obj) DO @IF EXIST "%%d" (echo "%%d" & rd /s /q "%%d")