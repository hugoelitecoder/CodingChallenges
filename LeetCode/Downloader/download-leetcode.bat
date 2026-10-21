@echo off
chcp 65001 >nul

echo Installing/updating requirements...
pip install --quiet --disable-pip-version-check browser_cookie3 requests

echo.
echo Running LeetCode downloader...
python download-leetcode.py --no-pause %*

echo.
echo Done. Press any key to exit.
pause >nul