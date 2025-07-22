@echo off
chcp 65001 >nul

echo Installing/updating requirements...
pip install --quiet --disable-pip-version-check browser_cookie3

echo.
echo Running LeetCode downloader...
python download-leetcode.py

echo.
echo Done. Press any key to exit.
pause >nul