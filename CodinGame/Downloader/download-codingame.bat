@echo off
chcp 65001 >nul

echo Installing/updating requirements...
pip install --quiet --disable-pip-version-check codingame browser_cookie3

echo.
echo Running CodinGame downloader...
python download-codingame.py --no-pause %*

echo.
echo Done. Press any key to exit.
pause >nul