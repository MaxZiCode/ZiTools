@echo off
SET "ProjectName1=ZiTools"
SET "SolutionDir1=F:\GitHub Repos\Rimworld\ZiTools\Source"
SET "ProjectName2=ZiTools BetterMiniMap Addon"
SET "SolutionDir2=F:\GitHub Repos\Rimworld\ZiTools\Source"
@echo on

xcopy /S /Y "%SolutionDir1%\..\%ProjectName1%\Assemblies\*" "F:\Steam\SteamApps\common\RimWorld\Mods\%ProjectName1%\Assemblies\"
xcopy /S /Y "%SolutionDir1%\..\%ProjectName1%\About\*" "F:\Steam\SteamApps\common\RimWorld\Mods\%ProjectName1%\About\"
xcopy /S /Y "%SolutionDir1%\..\%ProjectName1%\Defs\*" "F:\Steam\SteamApps\common\RimWorld\Mods\%ProjectName1%\Defs\"
xcopy /S /Y "%SolutionDir1%\..\%ProjectName1%\Textures\*" "F:\Steam\SteamApps\common\RimWorld\Mods\%ProjectName1%\Textures\"
xcopy /S /Y "%SolutionDir1%\..\%ProjectName1%\Languages\*" "F:\Steam\SteamApps\common\RimWorld\Mods\%ProjectName1%\Languages\"

xcopy /S /Y "%SolutionDir2%\..\%ProjectName2%\Assemblies\*" "F:\Steam\SteamApps\common\RimWorld\Mods\%ProjectName2%\Assemblies\"
xcopy /S /Y "%SolutionDir2%\..\%ProjectName2%\About\*" "F:\Steam\SteamApps\common\RimWorld\Mods\%ProjectName2%\About\"
xcopy /S /Y "%SolutionDir2%\..\%ProjectName2%\Languages\*" "F:\Steam\SteamApps\common\RimWorld\Mods\%ProjectName2%\Languages\"