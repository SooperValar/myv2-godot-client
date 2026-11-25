#!/bin/sh
# HUD art for MyV2, always from TLG's originals: eighth attribute row, wider value columns, Planner and Update tabs.
set -e
python3 /tmp/hud/extend.py && python3 /tmp/hud/columns.py && python3 /tmp/hud/tabs.py
