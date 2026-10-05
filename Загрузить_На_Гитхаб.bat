@echo off
color 0A
echo [ Загрузка ZGameCrosshairZ на Гитхаб... ]
echo.

cd /d "C:\Users\user\Desktop\CrosshairUltra"

git init
git add .
git add "dist\ZGameCrosshairZ-1.0.0.exe" -f
git commit -m "Initial release: ZGameCrosshairZ V1.0"
git branch -M main

:: Удаляем старый origin если был
git remote remove origin 2>nul
git remote add origin https://github.com/1ZAMZAMICH1/Gaming-Scope.-A-scope-for-all-types-of-games.git

echo.
echo [ Отправка файлов на сервер Github... ]
git push -u origin main -f

echo.
echo ГОТОВО! Проверьте вашу страницу на Гитхаб.
pause
