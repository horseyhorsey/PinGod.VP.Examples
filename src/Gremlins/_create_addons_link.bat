cd Gremlins
SET cDir=%cd%
cd ../../../addons/addons
SET addDir=%cd%
cd %cDir%
mklink /D addons "%addDir%"